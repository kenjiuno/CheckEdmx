using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace CheckEdmx {
    class Program {
        static void Main(string[] args) {
            // MSSQL.ssdl.xml ConceptualSchemaDefinition.csdl MSSQL.msl.xml
            // C:\Git\Npgsql2\src\Npgsql\NpgsqlSchema.ssdl ConceptualSchemaDefinition.csdl C:\Git\Npgsql2\src\Npgsql\NpgsqlSchema.msl
            if (args.Length == 3 && File.Exists(args[0]) && File.Exists(args[1]) && File.Exists(args[2])) {
                new Program().Run(args[0], args[1], args[2]);
                return;
            }
            Console.Error.WriteLine("CheckEdmx NpgsqlSchema.ssdl ConceptualSchemaDefinition.csdl NpgsqlSchema.msl ");
            Environment.ExitCode = 1;
        }

        public Program() {
            trace.Listeners.Add(new ConsoleTraceListener(false));
        }

        TextWriter L = new StringWriter();

        SortedDictionary<String, XElement> EntityTypes = new SortedDictionary<string, XElement>();
        SortedDictionary<String, XElement> EntitySets = new SortedDictionary<string, XElement>();
        SortedDictionary<String, XElement> Associations = new SortedDictionary<string, XElement>();

        SortedDictionary<String, XElement> ssdlEntityTypes = new SortedDictionary<string, XElement>();
        SortedDictionary<String, XElement> ssdlAssociations = new SortedDictionary<string, XElement>();

        String xCSDL;
        String xMSL;
        String xSSDL;

        XElement Schema;
        XElement mslMapping;
        XElement ssdlSchema;

        TraceSource trace = new TraceSource("CheckEdmx", SourceLevels.All);

        XDocument ssdl;
        XDocument csdl;
        XDocument msl;

        void Run(string fpssdl, string fpcsdl, string fpmsl) {
            trace.TraceEvent(TraceEventType.Information, 100, "Loading " + fpssdl);
            ssdl = XDocument.Load(fpssdl);
            trace.TraceEvent(TraceEventType.Information, 100, "Loaded");

            trace.TraceEvent(TraceEventType.Information, 100, "Loading " + fpcsdl);
            csdl = XDocument.Load(fpcsdl);
            trace.TraceEvent(TraceEventType.Information, 100, "Loaded");

            trace.TraceEvent(TraceEventType.Information, 100, "Loading " + fpmsl);
            msl = XDocument.Load(fpmsl);
            trace.TraceEvent(TraceEventType.Information, 100, "Loaded");

            trace.TraceEvent(TraceEventType.Information, 101, "Checking Schema version");
            if (false) { }
            else if (ssdl.Element("{" + NS.SSDLv1 + "}" + "Schema") != null && msl.Element("{" + NS.MSLv1 + "}" + "Mapping") != null && csdl.Element("{" + NS.CSDLv1 + "}" + "Schema") != null) {
                xCSDL = "{" + NS.CSDLv1 + "}";
                xMSL = "{" + NS.MSLv1 + "}";
                xSSDL = "{" + NS.SSDLv1 + "}";
                trace.TraceEvent(TraceEventType.Information, 101, "Checked. v1");
            }
            else if (ssdl.Element("{" + NS.SSDLv2 + "}" + "Schema") != null && msl.Element("{" + NS.MSLv2 + "}" + "Mapping") != null && csdl.Element("{" + NS.CSDLv2 + "}" + "Schema") != null) {
                xCSDL = "{" + NS.CSDLv2 + "}";
                xMSL = "{" + NS.MSLv2 + "}";
                xSSDL = "{" + NS.SSDLv2 + "}";
                trace.TraceEvent(TraceEventType.Information, 101, "Checked. v2");
            }
            else if (ssdl.Element("{" + NS.SSDLv3 + "}" + "Schema") != null && msl.Element("{" + NS.MSLv3 + "}" + "Mapping") != null && csdl.Element("{" + NS.CSDLv3 + "}" + "Schema") != null) {
                xCSDL = "{" + NS.CSDLv3 + "}";
                xMSL = "{" + NS.MSLv3 + "}";
                xSSDL = "{" + NS.SSDLv3 + "}";
                trace.TraceEvent(TraceEventType.Information, 101, "Checked. v3");
            }
            else {
                trace.TraceEvent(TraceEventType.Warning, 101, "Checked. unknown");
            }

            EntityTypes["Boolean"] = null;
            EntityTypes["Int16"] = null;
            EntityTypes["Int32"] = null;
            EntityTypes["Int64"] = null;
            EntityTypes["UInt16"] = null;
            EntityTypes["UInt32"] = null;
            EntityTypes["UInt64"] = null;
            EntityTypes["Guid"] = null;
            EntityTypes["String"] = null;

            ssdlSchema = ssdl.Element(xSSDL + "Schema");
            foreach (var ssdlEntityType in ssdlSchema.Elements(xSSDL + "EntityType")) {
                ssdlEntityTypes.Add(RUt.GetName(ssdlSchema.Attribute("Alias"), ssdlEntityType.Attribute("Name")), ssdlEntityType);
                ssdlEntityTypes.Add(RUt.GetName(ssdlSchema.Attribute("Namespace"), ssdlEntityType.Attribute("Name")), ssdlEntityType);
            }
            foreach (var ssdlAssociation in ssdlSchema.Elements(xSSDL + "Association")) {
                ssdlAssociations.Add(RUt.GetName(ssdlSchema.Attribute("Alias"), ssdlAssociation.Attribute("Name")), ssdlAssociation);
                ssdlAssociations.Add(RUt.GetName(ssdlSchema.Attribute("Namespace"), ssdlAssociation.Attribute("Name")), ssdlAssociation);
            }

            mslMapping = msl.Element(xMSL + "Mapping");

            Schema = csdl.Element(xCSDL + "Schema");
            foreach (var EntityType in Schema.Elements(xCSDL + "EntityType")) {
                EntityTypes.Add(RUt.GetName(Schema.Attribute("Alias"), EntityType.Attribute("Name")), EntityType);
                EntityTypes.Add(RUt.GetName(Schema.Attribute("Namespace"), EntityType.Attribute("Name")), EntityType);
            }
            foreach (var EntityType in Schema.Elements(xCSDL + "ComplexType")) {
                EntityTypes.Add(RUt.GetName(Schema.Attribute("Alias"), EntityType.Attribute("Name")), EntityType);
                EntityTypes.Add(RUt.GetName(Schema.Attribute("Namespace"), EntityType.Attribute("Name")), EntityType);
            }

            foreach (var Association in Schema.Elements(xCSDL + "Association")) {
                Associations.Add(RUt.GetName(Schema.Attribute("Alias"), Association.Attribute("Name")), Association);
                Associations.Add(RUt.GetName(Schema.Attribute("Namespace"), Association.Attribute("Name")), Association);
            }

            L = Console.Out;

            foreach (var EntityContainer in Schema.Elements(xCSDL + "EntityContainer")) {
                trace.TraceEvent(TraceEventType.Information, 102, "Checking {0}[Name='{1}']", "csdl.EntityContainer", EntityContainer.Attribute("Name").Value);

                trace.TraceEvent(TraceEventType.Information, 102, " Find {0}[Name='{1}']", "msl.EntityContainerMapping", EntityContainer.Attribute("Name").Value);
                var mslEntityContainerMapping = mslMapping.Elements(xMSL + "EntityContainerMapping")
                    .Where(p => p.Attribute("CdmEntityContainer").Value == EntityContainer.Attribute("Name").Value)
                    .FirstOrDefault();
                if (mslEntityContainerMapping == null) {
                    trace.TraceEvent(TraceEventType.Error, 102, " Not Found!");
                    trace.TraceEvent(TraceEventType.Information, 102, "May be one of: " + String.Join("/", mslMapping.Elements(xMSL + "EntityContainerMapping").Attributes("CdmEntityContainer").Select(p => p.Value).ToArray()));
                    throw new DirectoryNotFoundException(); //chk
                }
                else {
                    trace.TraceEvent(TraceEventType.Information, 102, " Found.");
                }

                trace.TraceEvent(TraceEventType.Information, 103, " Find {0}[Name='{1}']", "ssdl.EntityContainer", mslEntityContainerMapping.Attribute("StorageEntityContainer").Value);
                var ssdlEntityContainer = ssdlSchema.Elements(xSSDL + "EntityContainer")
                    .Where(p => p.Attribute("Name").Value == mslEntityContainerMapping.Attribute("StorageEntityContainer").Value)
                    .FirstOrDefault();
                if (ssdlEntityContainer == null) {
                    trace.TraceEvent(TraceEventType.Error, 103, " Not Found!");
                    trace.TraceEvent(TraceEventType.Information, 103, " May be one of: " + String.Join("/", ssdlSchema.Elements(xSSDL + "EntityContainer").Attributes("Name").Select(p => p.Value).ToArray()));
                    throw new DirectoryNotFoundException();//chk
                }
                else {
                    trace.TraceEvent(TraceEventType.Information, 103, " Found.");
                }

                foreach (var EntitySet in EntityContainer.Elements(xCSDL + "EntitySet")) {
                    EntitySets[EntitySet.Attribute("Name").Value] = EntitySet;

                    trace.TraceEvent(TraceEventType.Information, 104, " Checking {0}[Name='{1}']", "csdl.EntitySet", EntitySet.Attribute("Name").Value);

                    var EntityType = EntityTypes[EntitySet.Attribute("EntityType").Value];
                    WalkEntityType(EntityType);

                    trace.TraceEvent(TraceEventType.Information, 104, " Find {0}[Name='{1}']", "msl.EntitySetMapping", EntitySet.Attribute("Name").Value);
                    var mslEntitySetMapping = mslEntityContainerMapping.Elements(xMSL + "EntitySetMapping")
                        .Where(p => p.Attribute("Name").Value == EntitySet.Attribute("Name").Value)
                        .FirstOrDefault();
                    if (mslEntitySetMapping == null) {
                        trace.TraceEvent(TraceEventType.Error, 104, " Not Found!");
                        trace.TraceEvent(TraceEventType.Information, 104, " May be one of: " + String.Join("/", mslEntityContainerMapping.Elements(xMSL + "EntitySetMapping").Attributes("Name").Select(p => p.Value).ToArray()));
                        throw new DirectoryNotFoundException();//chk
                    }
                    else {
                        trace.TraceEvent(TraceEventType.Information, 104, " Found.");
                    }

                    ReadMslEntitySetMapping(mslEntitySetMapping, ssdlEntityContainer, EntitySet, EntityType);
                }

                foreach (var AssociationSet in EntityContainer.Elements(xCSDL + "AssociationSet")) {
                    trace.TraceEvent(TraceEventType.Information, 105, " Checking {0}[Name='{1}']", "csdl.AssociationSet", AssociationSet.Attribute("Name").Value);

                    trace.TraceEvent(TraceEventType.Information, 105, "  Find {0}[Name='{1}']", "csdl.Association", AssociationSet.Attribute("Association").Value);
                    var Association = Associations
                        .Where(p => p.Key == AssociationSet.Attribute("Association").Value)
                        .Select(p => p.Value)
                        .FirstOrDefault();
                    if (Association == null) {
                        trace.TraceEvent(TraceEventType.Error, 105, "  Not Found!");
                        trace.TraceEvent(TraceEventType.Information, 105, "  May be one of: " + String.Join("/", Associations.Select(p => p.Key).ToArray()));
                        throw new DirectoryNotFoundException();//chk
                    }
                    else {
                        trace.TraceEvent(TraceEventType.Information, 105, "  Found.");
                    }

                    foreach (var asEnd in AssociationSet.Elements(xCSDL + "End")) {
                        trace.TraceEvent(TraceEventType.Information, 105, "  Checking {0}[Role='{1}']", "csdl.AssociationSet.End", asEnd.Attribute("Role").Value);

                        trace.TraceEvent(TraceEventType.Information, 105, "   Find {0}[Name='{1}']", "csdl.EntitySet", asEnd.Attribute("EntitySet").Value);
                        var asEndty = EntitySets
                            .Where(p => p.Key == asEnd.Attribute("EntitySet").Value)
                            .Select(p => p.Value)
                            .FirstOrDefault();
                        if (asEndty == null) {
                            trace.TraceEvent(TraceEventType.Error, 105, "   Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 105, "   May be one of: " + String.Join("/", EntitySets.Select(p => p.Key).ToArray()));
                            throw new DirectoryNotFoundException();//chk
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 105, "   Found.");
                        }

                        trace.TraceEvent(TraceEventType.Information, 105, "   Find {0}[Name='{1}']", "csdl.Association.End", asEnd.Attribute("Role").Value);
                        var aEnd = Association.Elements(xCSDL + "End")
                            .Where(p => p.Attribute("Role").Value == asEnd.Attribute("Role").Value)
                            .FirstOrDefault();
                        if (aEnd == null) {
                            trace.TraceEvent(TraceEventType.Error, 105, "   Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 105, "   May be one of: " + String.Join("/", Association.Elements(xCSDL + "End").Attributes("Role").Select(p => p.Value).ToArray()));
                            throw new DirectoryNotFoundException();//chk
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 105, "   Found.");
                        }

                        trace.TraceEvent(TraceEventType.Information, 105, "   Find {0}[Name='{1}']", "csdl.EntityType", aEnd.Attribute("Type").Value);
                        var aEndty = EntityTypes
                            .Where(p => p.Key == aEnd.Attribute("Type").Value)
                            .Select(p => p.Value)
                            .FirstOrDefault();
                        if (aEndty == null) {
                            trace.TraceEvent(TraceEventType.Error, 105, "   Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 105, "   May be one of: " + String.Join("/", EntityTypes.Select(p => p.Key).ToArray()));
                            throw new DirectoryNotFoundException();//!
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 105, "   Found.");
                        }

                    }
                }

                foreach (var mslAssociationSetMapping in mslEntityContainerMapping.Elements(xMSL + "AssociationSetMapping")) {
                    trace.TraceEvent(TraceEventType.Information, 106, "  Checking {0}[Name='{1}']", "msl.AssociationSetMapping", mslAssociationSetMapping.Attribute("Name").Value);

                    trace.TraceEvent(TraceEventType.Information, 106, "   Find {0}[Name='{1}']", "csdl.AssociationSet", mslAssociationSetMapping.Attribute("Name").Value);
                    var csdlAssociationSet = EntityContainer.Elements(xCSDL + "AssociationSet")
                        .Where(p => p.Attribute("Name").Value == mslAssociationSetMapping.Attribute("Name").Value)
                        .FirstOrDefault();
                    if (csdlAssociationSet == null) {
                        trace.TraceEvent(TraceEventType.Error, 106, "   Not Found!");
                        trace.TraceEvent(TraceEventType.Information, 106, "   May be one of: " + String.Join("/", EntityContainer.Elements(xCSDL + "AssociationSet").Attributes("Name").Select(p => p.Value).ToArray()));
                        throw new DirectoryNotFoundException();//chk
                    }
                    else {
                        trace.TraceEvent(TraceEventType.Information, 106, "   Found.");
                    }
                }

                foreach (var ssdlAssociationSet in ssdlEntityContainer.Elements(xSSDL + "AssociationSet")) {
                    trace.TraceEvent(TraceEventType.Information, 107, "  Checking {0}[Name='{1}']", "ssdl.AssociationSet", ssdlAssociationSet.Attribute("Name").Value);

                    trace.TraceEvent(TraceEventType.Information, 107, "   Find {0}[Name='{1}']", "ssdl.Association", ssdlAssociationSet.Attribute("Association").Value);
                    var ssdlAssociation = ssdlAssociations
                        .Where(p => p.Key == ssdlAssociationSet.Attribute("Association").Value)
                        .Select(p => p.Value)
                        .FirstOrDefault();
                    if (ssdlAssociation == null) {
                        trace.TraceEvent(TraceEventType.Error, 107, "   Not Found!");
                        trace.TraceEvent(TraceEventType.Information, 107, "   May be one of: " + String.Join("/", ssdlAssociations.Select(p => p.Key).ToArray()));
                        throw new DirectoryNotFoundException();//chk
                    }
                    else {
                        trace.TraceEvent(TraceEventType.Information, 107, "   Found.");
                    }

                    foreach (var asEnd in ssdlAssociationSet.Elements(xSSDL + "End")) {
                        trace.TraceEvent(TraceEventType.Information, 107, "   Checking {0}[Role='{1}']", "ssdl.AssociationSet.End", asEnd.Attribute("Role").Value);

                        trace.TraceEvent(TraceEventType.Information, 107, "    Find {0}[Name='{1}']", "ssdl.EntitySet", asEnd.Attribute("EntitySet").Value);
                        var ssdlEntitySet = ssdlEntityContainer.Elements(xSSDL + "EntitySet")
                            .Where(p => p.Attribute("Name").Value == asEnd.Attribute("EntitySet").Value)
                            .FirstOrDefault();
                        if (ssdlEntitySet == null) {
                            trace.TraceEvent(TraceEventType.Error, 107, "    Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 107, "    May be one of: " + String.Join("/", ssdlEntityContainer.Elements(xSSDL + "EntitySet").Attributes("Name").Select(p => p.Value).ToArray()));
                            throw new DirectoryNotFoundException();//chk
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 107, "    Found.");
                        }

                        trace.TraceEvent(TraceEventType.Information, 107, "    Find {0}[Name='{1}']", "ssdl.Association.End", asEnd.Attribute("Role").Value);
                        var aEnd = ssdlAssociation.Elements(xSSDL + "End")
                            .Where(p => p.Attribute("Role").Value == asEnd.Attribute("Role").Value)
                            .FirstOrDefault();
                        if (aEnd == null) {
                            trace.TraceEvent(TraceEventType.Error, 107, "    Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 107, "    May be one of: " + String.Join("/", ssdlAssociation.Elements(xSSDL + "End").Attributes("Role").Select(p => p.Value).ToArray()));
                            throw new DirectoryNotFoundException();//chk
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 107, "    Found.");
                        }

                        trace.TraceEvent(TraceEventType.Information, 107, "    Find {0}[Name='{1}']", "ssdl.EntityType", asEnd.Attribute("Role").Value);
                        var ssdlEntityType = ssdlEntityTypes
                            .Where(p => p.Key == aEnd.Attribute("Type").Value)
                            .Select(p => p.Value)
                            .FirstOrDefault();
                        if (ssdlEntityType == null) {
                            trace.TraceEvent(TraceEventType.Error, 107, "    Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 107, "    May be one of: " + String.Join("/", ssdlEntityTypes.Select(p => p.Key).ToArray()));
                            throw new DirectoryNotFoundException();//chk
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 107, "    Found.");
                        }

                        String ty1 = ssdlEntitySet.Attribute("EntityType").Value;
                        String ty2 = RUt.GetName(ssdlSchema.Attribute("Namespace"), ssdlEntityType.Attribute("Name"));

                        trace.TraceEvent(TraceEventType.Information, 107, "    Check if ssdl.AssociationSet.End[EntitySet='{2}'] -> ssdl.EntitySet[EntityType='{0}'] == ssdl.Association.End[Type='{1}']"
                            , ssdlEntitySet.Attribute("EntityType").Value
                            , ssdlEntityType.Attribute("Name").Value
                            , asEnd.Attribute("EntitySet").Value
                            );
                        if (ty1 != ty2) {
                            trace.TraceEvent(TraceEventType.Error, 107, "     Failure, mismatch detected.");
                            throw new InvalidDataException(ty1 + " != " + ty2);//chk
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 107, "     Pass.");
                        }

                        // http://msdn.microsoft.com/ja-jp/library/vstudio/bb387115.aspx

                        {
                            var a = ssdlAssociation
                                .Element(xSSDL + "ReferentialConstraint")
                                .Element(xSSDL + "Principal");
                            {
                                trace.TraceEvent(TraceEventType.Information, 107, "    Checking {0}[Role='{1}']", "ssdl.Association.End.ReferentialConstraint.Principal", a.Attribute("Role").Value);
                                if (a.Attribute("Role").Value == aEnd.Attribute("Role").Value) {
                                    var aPropertyRef = a.Element(xSSDL + "PropertyRef");
                                    {
                                        trace.TraceEvent(TraceEventType.Information, 107, "     Checking {0}[Name='{1}']", "ssdl.Association.End.ReferentialConstraint.Principal.PropertyRef", aPropertyRef.Attribute("Name").Value);

                                        trace.TraceEvent(TraceEventType.Information, 107, "      Find {0}[Name='{1}']", "ssdl.EntityType", aPropertyRef.Attribute("Name").Value);
                                        var ssdlProperty = ssdlEntityType.Elements(xSSDL + "Property")
                                            .Where(p => p.Attribute("Name").Value == aPropertyRef.Attribute("Name").Value)
                                            .FirstOrDefault();
                                        if (ssdlProperty == null) {
                                            trace.TraceEvent(TraceEventType.Error, 107, "      Not Found!");
                                            trace.TraceEvent(TraceEventType.Information, 107, "      May be one of: " + String.Join("/", ssdlEntityType.Elements(xSSDL + "Property").Attributes("Name").Select(p => p.Value).ToArray()));
                                            throw new DirectoryNotFoundException();//chk
                                        }
                                        else {
                                            trace.TraceEvent(TraceEventType.Information, 107, "      Found.");
                                        }
                                    }
                                }
                            }
                        }
                        {
                            var a = ssdlAssociation
                                .Element(xSSDL + "ReferentialConstraint")
                                .Element(xSSDL + "Dependent");
                            {
                                trace.TraceEvent(TraceEventType.Information, 107, "    Checking {0}[Role='{1}']", "ssdl.Association.End.ReferentialConstraint.Dependent", a.Attribute("Role").Value);
                                if (a.Attribute("Role").Value == aEnd.Attribute("Role").Value) {
                                    var aPropertyRef = a.Element(xSSDL + "PropertyRef");
                                    {
                                        trace.TraceEvent(TraceEventType.Information, 107, "     Checking {0}[Name='{1}']", "ssdl.Association.End.ReferentialConstraint.Dependent.PropertyRef", aPropertyRef.Attribute("Name").Value);

                                        trace.TraceEvent(TraceEventType.Information, 107, "      Find {0}[Name='{1}']", "ssdl.EntityType", aPropertyRef.Attribute("Name").Value);
                                        var ssdlProperty = ssdlEntityType.Elements(xSSDL + "Property")
                                            .Where(p => p.Attribute("Name").Value == aPropertyRef.Attribute("Name").Value)
                                            .FirstOrDefault();
                                        if (ssdlProperty == null) {
                                            trace.TraceEvent(TraceEventType.Error, 107, "      Not Found!");
                                            trace.TraceEvent(TraceEventType.Information, 107, "      May be one of: " + String.Join("/", ssdlEntityType.Elements(xSSDL + "Property").Attributes("Name").Select(p => p.Value).ToArray()));
                                            throw new DirectoryNotFoundException();//chk
                                        }
                                        else {
                                            trace.TraceEvent(TraceEventType.Information, 107, "      Found.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ReadMslEntitySetMapping(
            XElement mslEntitySetMapping,
            XElement ssdlEntityContainer,
            XElement EntitySet,
            XElement EntityType
        ) {
            trace.TraceEvent(TraceEventType.Information, 104, " Checking {0}[Name='{1}']", "msl.EntitySetMapping", EntitySet.Attribute("Name").Value);

            if (mslEntitySetMapping.Attribute("StoreEntitySet") != null) {
                ReadMslStoreEntitySet(mslEntitySetMapping, ssdlEntityContainer, EntitySet, EntityType, "  ");
            }
            else {
                foreach (var mslEntityTypeMapping in mslEntitySetMapping.Elements(xMSL + "EntityTypeMapping")) {
                    trace.TraceEvent(TraceEventType.Information, 104, "  Checking {0}[TypeName='{1}']", "msl.EntityTypeMapping", mslEntityTypeMapping.Attribute("TypeName").Value);

                    foreach (var mslMappingFragment in mslEntityTypeMapping.Elements(xMSL + "MappingFragment")) {
                        trace.TraceEvent(TraceEventType.Information, 104, "   Checking {0}[StoreEntitySet='{1}']", "msl.MappingFragment", mslMappingFragment.Attribute("StoreEntitySet").Value);

                        ReadMslStoreEntitySet(mslMappingFragment, ssdlEntityContainer, EntitySet, EntityType, "    ");
                    }
                }
            }
        }

        private void ReadMslStoreEntitySet(
            XElement mslEntitySetMapping,
            XElement ssdlEntityContainer,
            XElement EntitySet,
            XElement EntityType,
            String ws
        ) {
            trace.TraceEvent(TraceEventType.Information, 104, ws + "Find {0}[Name='{1}']", "ssdl.EntitySet", mslEntitySetMapping.Attribute("StoreEntitySet").Value);
            var ssdlEntitySet = ssdlEntityContainer.Elements(xSSDL + "EntitySet")
                .Where(p => p.Attribute("Name").Value == mslEntitySetMapping.Attribute("StoreEntitySet").Value)
                .FirstOrDefault();
            if (mslEntitySetMapping == null) {
                trace.TraceEvent(TraceEventType.Error, 104, ws + "Not Found!");
                trace.TraceEvent(TraceEventType.Information, 104, ws + "May be one of: " + String.Join("/", ssdlEntityContainer.Elements(xSSDL + "EntitySet").Attributes("Name").Select(p => p.Value).ToArray()));
                throw new DirectoryNotFoundException();
            }
            else {
                trace.TraceEvent(TraceEventType.Information, 104, ws + "Found.");
            }

            trace.TraceEvent(TraceEventType.Information, 104, ws + "Find {0}[EntityType='{1}']", "ssdl.EntityType", ssdlEntitySet.Attribute("EntityType").Value);
            var ssdlEntityType = ssdlEntityTypes
                .Where(p => p.Key == ssdlEntitySet.Attribute("EntityType").Value)
                .Select(p => p.Value)
                .FirstOrDefault();
            if (ssdlEntityType == null) {
                trace.TraceEvent(TraceEventType.Error, 104, ws + "Not Found!");
                trace.TraceEvent(TraceEventType.Information, 104, ws + "May be one of: " + String.Join("/", ssdlEntityTypes.Select(p => p.Key).ToArray()));
                throw new DirectoryNotFoundException();
            }
            else {
                trace.TraceEvent(TraceEventType.Information, 104, ws + "Found.");
            }

            foreach (var mslScalarProperty in mslEntitySetMapping.Elements(xMSL + "ScalarProperty")) {
                trace.TraceEvent(TraceEventType.Information, 104, ws + "Checking {0}[Name='{1}']", "msl.ScalarProperty", mslScalarProperty.Attribute("Name").Value);

                trace.TraceEvent(TraceEventType.Information, 104, ws + " Find {0}[Name='{1}']", "msl.Property", mslScalarProperty.Attribute("Name").Value);
                // Todo: Select better method!
                var mslScalarPropertyFor = CollectProperty(EntityType)
                    .Where(p => p.Attribute("Name").Value == mslScalarProperty.Attribute("Name").Value)
                    .FirstOrDefault();
                if (mslScalarPropertyFor == null) {
                    trace.TraceEvent(TraceEventType.Error, 104, ws + " Not Found!");
                    trace.TraceEvent(TraceEventType.Information, 104, ws + " May be one of: " + String.Join("/", CollectProperty(EntityType).Attributes("Name").Select(p => p.Value).ToArray()));
                    throw new DirectoryNotFoundException();
                }
                else {
                    trace.TraceEvent(TraceEventType.Information, 104, ws + " Found.");
                }

                trace.TraceEvent(TraceEventType.Information, 104, ws + " Find {0}[Name='{1}']", "msl.EntitySetMapping", EntitySet.Attribute("Name").Value);
                var ssdlProperty = ssdlEntityType.Elements(xSSDL + "Property")
                    .Where(p => p.Attribute("Name").Value == mslScalarProperty.Attribute("ColumnName").Value)
                    .FirstOrDefault();
                if (ssdlProperty == null) {
                    trace.TraceEvent(TraceEventType.Error, 104, ws + " Not Found!");
                    trace.TraceEvent(TraceEventType.Information, 104, ws + " May be one of: " + String.Join("/", ssdlEntityType.Attributes("Name").Select(p => p.Value).ToArray()));
                    throw new DirectoryNotFoundException();
                }
                else {
                    trace.TraceEvent(TraceEventType.Information, 104, ws + " Found.");
                }
            }
        }

        private List<XElement> CollectProperty(XElement EntityType) {
            List<XElement> eles = new List<XElement>();
            CollectMyProperty(eles, EntityType);
            CollectBaseTypeProperty(eles, EntityType);
            CollectSubTypeProperty(eles, EntityType);
            return eles;
        }

        void CollectMyProperty(List<XElement> eles, XElement EntityType) {
            foreach (var Property in EntityType.Elements(xCSDL + "Property")) {
                eles.Add(Property);
            }
        }

        void CollectBaseTypeProperty(List<XElement> eles, XElement EntityType) {
            if (EntityType.Attribute("BaseType") != null) {
                var BaseType = EntityTypes[EntityType.Attribute("BaseType").Value];
                CollectMyProperty(eles, BaseType);
                CollectBaseTypeProperty(eles, BaseType);
            }
        }

        void CollectSubTypeProperty(List<XElement> eles, XElement EntityType) {
            foreach (XElement SubType in EntityTypes.Values.Where(p => p != null && p.Attribute("BaseType") != null && p.Attribute("BaseType").Value == RUt.GetName(Schema.Attribute("Alias"), EntityType.Attribute("Name")))) {
                CollectMyProperty(eles, SubType);
                CollectSubTypeProperty(eles, SubType);
            }
        }

        private void WalkEntityType(XElement EntityType) {
            if (EntityType.Attribute("BaseType") != null) {
                var BaseType = EntityTypes[EntityType.Attribute("BaseType").Value];
                WalkEntityType(BaseType);
            }

            trace.TraceEvent(TraceEventType.Information, 104, "  Checking {0}[Name='{1}']", "csdl.EntityType", EntityType.Attribute("Name").Value);

            {
                var Key = EntityType.Element(xCSDL + "Key");
                if (Key != null) {
                    foreach (var PropertyRef in Key.Elements(xCSDL + "PropertyRef")) {
                        trace.TraceEvent(TraceEventType.Information, 104, "   Checking {0}[Name='{1}']", "csdl.EntityType.Key.PropertyRef", PropertyRef.Attribute("Name").Value);

                        trace.TraceEvent(TraceEventType.Information, 104, "    Find {0}[Name='{1}']", "csdl.Property", PropertyRef.Attribute("Name").Value);
                        var csdlProperty = EntityType.Elements(xCSDL + "Property")
                            .Where(p => p.Attribute("Name").Value == PropertyRef.Attribute("Name").Value)
                            .FirstOrDefault();
                        if (csdlProperty == null) {
                            trace.TraceEvent(TraceEventType.Error, 104, "    Not Found!");
                            trace.TraceEvent(TraceEventType.Information, 104, "    May be one of: " + String.Join("/", EntityType.Elements(xCSDL + "Property").Attributes("Name").Select(p => p.Value).ToArray()));
                            throw new DirectoryNotFoundException();
                        }
                        else {
                            trace.TraceEvent(TraceEventType.Information, 104, "    Found.");
                        }
                    }
                }
            }

            foreach (var Property in EntityType.Elements(xCSDL + "Property")) {
                trace.TraceEvent(TraceEventType.Information, 104, "   Checking {0}[Name='{1}']", "csdl.EntityType.Property", Property.Attribute("Name").Value);
                var Propertyty = EntityTypes[Property.Attribute("Type").Value];
            }

            foreach (var NavigationProperty in EntityType.Elements(xCSDL + "NavigationProperty")) {
                trace.TraceEvent(TraceEventType.Information, 104, "   Checking {0}[Name='{1}']", "csdl.EntityType.NavigationProperty", NavigationProperty.Attribute("Name").Value);

                // Todo :Check them
                var Association = Associations[NavigationProperty.Attribute("Relationship").Value];
                var FromRole = Association.Elements(xCSDL + "End").First(p => p.Attribute("Role").Value == NavigationProperty.Attribute("FromRole").Value);
                var FromRolety = EntityTypes[FromRole.Attribute("Type").Value];
                var ToRole = Association.Elements(xCSDL + "End").First(p => p.Attribute("Role").Value == NavigationProperty.Attribute("ToRole").Value);
                var ToRolety = EntityTypes[ToRole.Attribute("Type").Value];
            }
        }
    }

    public class RUt {
        public static string GetName(XAttribute alias, XAttribute ty) {
            String s = alias.Value + "." + ty.Value;
            return s.Trim('.');
        }
    }

    public class NS {
        public static string EDMXv1 { get { return "http://schemas.microsoft.com/ado/2007/06/edmx"; } }
        public static string EDMXv2 { get { return "http://schemas.microsoft.com/ado/2008/10/edmx"; } }
        public static string EDMXv3 { get { return "http://schemas.microsoft.com/ado/2009/11/edmx"; } }

        public static string SSDLv1 { get { return "http://schemas.microsoft.com/ado/2006/04/edm/ssdl"; } }
        public static string SSDLv2 { get { return "http://schemas.microsoft.com/ado/2009/02/edm/ssdl"; } }
        public static string SSDLv3 { get { return "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"; } }

        public static string MSLv1 { get { return "urn:schemas-microsoft-com:windows:storage:mapping:CS"; } }
        public static string MSLv2 { get { return "http://schemas.microsoft.com/ado/2008/09/mapping/cs"; } }
        public static string MSLv3 { get { return "http://schemas.microsoft.com/ado/2009/11/mapping/cs"; } }

        public static string CSDLv1 { get { return "http://schemas.microsoft.com/ado/2006/04/edm"; } }
        public static string CSDLv2 { get { return "http://schemas.microsoft.com/ado/2008/09/edm"; } }
        public static string CSDLv3 { get { return "http://schemas.microsoft.com/ado/2009/11/edm"; } }
    }
}
