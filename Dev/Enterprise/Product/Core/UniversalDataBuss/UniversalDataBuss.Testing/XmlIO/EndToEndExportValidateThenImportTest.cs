using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.TestDataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XsdGeneration;

namespace Enterprise.UniversalDataBuss.XmlIO.Testing
{
	class EndToEndExportValidateThenImportTest : TestCaseWithFactory
	{
		public void TestPopulatedUberShipmentValidatesAgainstTheGeneratedSchemas_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				AssertEquals("Precondition: Got the right namespace for this test", UniversalXmlInfo.Namespace_2012_11, SchemaVersionManager.Current.Namespace);
				AssertPopulatedUberShipmentValidatesAgainstTheGeneratedSchemas("2012_11");
			}
		}

		public void TestPopulatedUberShipmentValidatesAgainstTheGeneratedSchemas_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				AssertEquals("Precondition: Got the right namespace for this test", UniversalXmlInfo.Namespace_2011_11, SchemaVersionManager.Current.Namespace);
				AssertPopulatedUberShipmentValidatesAgainstTheGeneratedSchemas("2011_11");
			}
		}

		static void AssertPopulatedUberShipmentValidatesAgainstTheGeneratedSchemas(string namespaceVersion)
		{
			var commonSchema = XElement.Parse(new UniversalXsdGenerator().GetXsdOutput(typeof(UberShipment).Assembly));
			var shipmentSchema = XElement.Parse(new UniversalXsdGenerator().GetXsdOutput(typeof(UberShipment)));
			var shipmentDataObject = GetNewUberShipmentWithContent(namespaceVersion);

			using (var dataStream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(shipmentDataObject, dataStream);
				var shipment = XElement.Load(dataStream);

				using (var shipmentReader = shipment.CreateReader())
				using (var shipmentSchemaReader = shipmentSchema.CreateReader())
				using (var commonSchemaReader = commonSchema.CreateReader())
				{
					var schemaSet = new XmlSchemaSet();
					schemaSet.Add(SchemaVersionManager.Current.Namespace, shipmentSchemaReader);
					schemaSet.Add(SchemaVersionManager.Current.Namespace, commonSchemaReader);

					var errors = new List<string>();
					var shipmentDocument = XDocument.Load(shipmentReader);
					shipmentDocument.Validate(schemaSet, (sender, eventArgs) => { errors.Add(eventArgs.Severity.ToString() + ":- " + eventArgs.Message); });
					AssertMultilineASCIIEquals("There should be no errors... Errors shown below.", "", string.Join("\r\n", errors.ToArray()));
				}
			}
		}

		static UberShipment GetNewUberShipmentWithContent(string namespaceVersion)
		{
			var shipment = new UberShipment();

			shipment.DataContext = GetNewDataContext(namespaceVersion);

			shipment.CandidateKeyField = 1001;
			shipment.ReferenceData = "MyReference";
			shipment.ZZZMandatoryField = 2;

			shipment.UberBigField = @"Some really big block of text.
2 > 1 & 3 < 4
I mean it should have at least 3 or 4 lines.
Enough to know better.";
			shipment.UberBoolean = true;
			shipment.UberByteField = 14;
			shipment.UberDate = new ZDate(2017, 11, 27);
			shipment.UberDateTime = new ZDateTime(2010, 2, 12);
			shipment.UberDecimal = 12.23;
			shipment.UberInteger = 44;
			shipment.UberLongInt = 99;
			shipment.UberMediumField = new UberCodeDescriptionPairAlwaysAttributes() { Code = "HELLO HELLO HELLO", Description = @"He <can't> &stop saying ""hello""!" };
			shipment.UberShortIntField = 9;
			shipment.UberSmallField = "SHORTFIELDTRUNCATED";
			shipment.UberDuration = new TimeSpan(1, 2, 3);

			shipment.Origin = new UberUNLOCO() { Code = "AUSYD", Name = "Syd-er-nee" };
			shipment.Destination = new UberUNLOCO() { Code = "NZAKL", Name = "Doorklund" };

			shipment.PackageCount = 12;
			shipment.PackageUnit = new UberPackingUnit() { Code = "PK" };

			shipment.Weight = 44;
			shipment.WeightUnit = new UberUnitOfWeight() { Code = "KG", Description = "Kilograms" };

			shipment.Volume = 33;
			shipment.VolumeUnit = new UberUnitOfVolume() { Code = "M3", Description = "Cubic Meters" };

			shipment.InnerRelation = new UberInnerRelatedObject() { ExtraBoolean = false, ExtraDecimal = 1.224, ExtraField = "hOOters" };

			var dates = shipment.DateCollection = new UberList<UberDate>();
			dates.Add(new UberDate() { Type = UberDateType.Pickup, IsEstimate = false, Value = new ZDateTime(2010, 2, 1) });
			dates.Add(new UberDate() { Type = UberDateType.Departure, Value = new ZDateTime(2010, 2, 4) });
			dates.Add(new UberDate() { Type = UberDateType.Pack, IsEstimate = true, Value = ZDateTime.Empty });

			var organisations = shipment.OrganizationCollection = new List<UberOrganization>();
			organisations.Add(new UberOrganization()
			{
				Code = "FRED",
				CompanyName = "FRED'S TILES",
				Address1 = "104 Bourke Rd",
				City = "Alexandria",
				State = "NSW",
				Postcode = "2015",
				Country = new UberCountry() { Code = "AU", Name = "Australia" },
				Phone = "9310 1310"
			});

			return shipment;
		}

		static IDataContextDataObject GetNewDataContext(string namespaceVersion)
		{
			switch (namespaceVersion)
			{
				case "2011_11":
					return new DataObjects.TestDataObjects._2011_11.UberDataContext()
					{
						DataSourceCollection = new List<DataObjects.TestDataObjects._2011_11.UberDataSource>(new[]
						{
							new DataObjects.TestDataObjects._2011_11.UberDataSource()
							{
								Type = "HireOnSight",
								Key = "S000010000",
							}
						})
						,
						ActionPurpose = new UberCodeDescriptionPair() { Code = "SDS", Description = "Something" },
						EventType = new UberCodeDescriptionPair() { Code = "FFF", Description = "Free Floating FFFF" },
						DataTargetCollection = null,
						DocumentaryOverride = new UberDocumentaryOverride()
						{
							DocumentName = "TEST DOCUMENT",
							IsSystemDefined = true,
							Purpose = new UberCodeDescriptionPair() { Code = "TES", Description = "Testing" },
							DataVersion = 2,
							SubmissionVersion = 1
						}
					};
				case "2012_11":
					return new DataObjects.TestDataObjects._2012_11.UberDataContext()
					{
						DataSource = new DataObjects.TestDataObjects._2012_11.UberDataSource()
						{
							Type = UberReferenceType.HireOnSight,
							Key = "S000010000",
							DataProvider = new DataObjects.TestDataObjects._2012_11.UberDataProvider()
							{
								Code = "HYEHYESYD",
								Type = "EnterpriseID"
							}
						}
						,
						Workflow = new DataObjects.TestDataObjects._2012_11.UberWorkflow()
						{
							ActionPurpose = new UberCodeDescriptionPair() { Code = "SDS", Description = "Something" },
							EventType = new UberCodeDescriptionPair() { Code = "FFF", Description = "Free Floating FFFF" }
						}
						,
						DataTargetCollection = null,
						DocumentaryOverride = new UberDocumentaryOverride()
						{
							DocumentName = "TEST DOCUMENT",
							IsSystemDefined = true,
							Purpose = new UberCodeDescriptionPair() { Code = "TES", Description = "Testing" },
							DataVersion = 2,
							SubmissionVersion = 1
						}
					};
			}

			throw new Exception("A namespace we don't handle??? DOH!! If we add a new namespace version further up, have to add it here too.");
		}
	}
}
