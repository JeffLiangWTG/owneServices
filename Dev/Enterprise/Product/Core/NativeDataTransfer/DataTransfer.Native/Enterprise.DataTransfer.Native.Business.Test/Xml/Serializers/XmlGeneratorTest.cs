using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Operations;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xml.Serializers
{
	public class XmlGeneratorTest : TransactionedTestCase
	{
		public void TestGenerateEntitySetWithData()
		{
			// Prepare data in database
			var pk = TestUtil.PrepareDummyBizoData();
			var childPK1 = TestUtil.PrepareDummyDependentBizoData(pk);
			var childPK2 = TestUtil.PrepareDummyDependentSuffixData(pk);
			var pivotPK = TestUtil.PrepareDummyPivotData(pk, childPK2);

			var entityRepository = new RetrieveOperation(new AncillaryImportServices());

			var dummyDefinition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var dummy = (Entity)entityRepository.FindByInternalPK(pk, dummyDefinition);
			dummy.Action = EntityAction.MERGE;

			var generator = new EntitySetXmlSerializer();
			var output = generator.Serialize(dummy);

			AssertEquals("Dummy", output.Name.LocalName);
			var schemaElement = output;

			AssertEquals(1, schemaElement.Elements(ns + "DummyBizo").Count());
			var dummyElement = schemaElement.Elements(ns + "DummyBizo").First();

			AssertEquals("Key Element should only be in Schema not Data Element", 0, dummyElement.Elements(ns + "Key").Count());

			AssertEquals(3, dummyElement.Elements().Count(e => e.Name.ToString().Contains("Collection")));

			Assert("Excluded properties are not serialized", !dummyElement.Elements(ns + "excludedColumn").Any());
			Assert("Excluded properties are not serialized", !dummyElement.Elements(ns + "FK_Code").Any());
			Assert("Excluded from export properties are not serialized", !dummyElement.Elements(ns + "AnotherNumber").Any());
		}

		// Integration test/Functional test
		public void TestGenerateEntitySetShouldBeValidateWithXsd()
		{
			const string entitySetName = "Dummy";
			var dummyEntity = TestUtil.PrepareDummyBizoEntity(new AncillaryImportServices());
			var definition = TestUtil.GetEntitySetDefinition(entitySetName);

			dummyEntity.InternalPK = Guid.NewGuid();
			dummyEntity["PK"] = dummyEntity.InternalPK;
			dummyEntity["Code"] = "ABC";
			dummyEntity["Description"] = "Description";
			dummyEntity["Number"] = 1234;
			dummyEntity["Decimal"] = 1.234;
			dummyEntity["AnotherDecimal"] = 1.234;
			dummyEntity["AnotherNumber"] = 1234;
			dummyEntity["Bool"] = "Y";
			dummyEntity["Short"] = 123;
			dummyEntity["Byte"] = 8;
			dummyEntity["Money"] = 1.23;
			dummyEntity["NVarChar"] = "ABC";
			dummyEntity["VarCharMax"] = "ABC";
			dummyEntity["NVarCharMax"] = "ABC";
			dummyEntity["BitFalse"] = true;
			dummyEntity["BitTrue"] = false;

			using (var tempDirectory = new TempDirectory())
			{
				var importService = new NativeXmlImportService();
				importService.GenerateAndSaveAllXSDs(tempDirectory);
				importService.WriteUniversalCommonSchemaForTesting(tempDirectory);

				var nativeRootSchemaPath = Path.Combine(tempDirectory, "Native.xsd");
				var nativeRootSchema = XDocument.Load(nativeRootSchemaPath);

				var commonSchemaPath = Path.Combine(tempDirectory, UniversalXmlInfo.CommonSchemaName);
				var commonSchema = XDocument.Load(commonSchemaPath);

				var xsdGenerator = new NativeXsdGenerator();
				var dummyBoSchema = xsdGenerator.Generate(definition);

				var generator = new EntitySetXmlSerializer();
				var dummyBoXml = generator.Serialize(dummyEntity);

				using (var dummyBoXmlReader = dummyBoXml.CreateReader())
				{
					var doc = XDocument.Load(dummyBoXmlReader);

					using (var dummyBoSchemaReader = dummyBoSchema.CreateReader())
					using (var nativeRootSchemaReader = nativeRootSchema.CreateReader())
					using (var commonSchemaReader = commonSchema.CreateReader())
					{
						var schemaSet = new XmlSchemaSet();
						schemaSet.Add(SchemaVersionManager.Instance.Namespace, dummyBoSchemaReader);
						schemaSet.Add(SchemaVersionManager.Instance.Namespace, nativeRootSchemaReader);
						schemaSet.Add(SchemaVersionManager.Instance.UniversalNamespace, commonSchemaReader);

						AssertNoExceptionThrown(() => doc.Validate(schemaSet, (o, e) => { throw e.Exception; }));
					}
				}
			}
		}

		public void TestGenerateEntityWithInvalidXMLChars()
		{
			const string entitySetName = "Dummy";
			var dummyEntity = TestUtil.PrepareDummyBizoEntity(new AncillaryImportServices());
			var definition = TestUtil.GetEntitySetDefinition(entitySetName);

			dummyEntity.InternalPK = Guid.NewGuid();
			dummyEntity["PK"] = dummyEntity.InternalPK;
			dummyEntity["Code"] = "ABC";
			dummyEntity["Description"] = "Description\u001E";
			dummyEntity["NVarChar"] = "A\u0020B\uD800C";
			dummyEntity["VarCharMax"] = "\uD83D\uDCA9";

			var generator = new EntitySetXmlSerializer();
			var dummyBoXml = generator.Serialize(dummyEntity);

			AssertContains($@"<Dummy version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <DummyBizo>
    <PK>{dummyEntity.InternalPK}</PK>
    <Code>ABC</Code>
    <Description>Description</Description>
    <NVarChar>A BC</NVarChar>
    <VarCharMax>💩</VarCharMax>
  </DummyBizo>
</Dummy>"
  , dummyBoXml.ToString());

			CombineAssertions("Entity should not have invalid XML characters", () =>
			{
				AssertEquals("Description", dummyEntity["Description"]);
				AssertEquals("A BC", dummyEntity["NVarChar"]);
				AssertEquals("💩", dummyEntity["VarCharMax"]);
			});
		}

		public void TestXMLGenerationWithNoDuplicateElements()
		{
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.TestFiles.Organization_CHIBEALAX.xml"));
			var xml = reader.ReadToEnd();
			reader.Close();
			var stream = new MemoryStream(Encoding.Default.GetBytes(xml));

			importHandler.Import(stream);
			var query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.OH_Code, "CHIBEALAX");
			var org = factory.LoadTop1<OrgHeader>(query);

			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
			var serializer = new NativeXmlSerializer { Converter = converter };
			var exportService = new NativeXmlExportService { Serializer = serializer };
			stream = new MemoryStream();
			exportService.Export(new[] { org }, stream);
			stream.Position = 0;
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			var orgSupplierBuyerLinkElemList = xmlDoc.GetElementsByTagName("OrgSupBuyLinkTrnMode");

			foreach (var elem in orgSupplierBuyerLinkElemList)
			{
				var childNodes = ((XmlNode)elem).ChildNodes;
				var nodeNameList = new List<string>();
				for (var i = 0; i < childNodes.Count; i++)
				{
					nodeNameList.Add(childNodes[i].Name);
				}
				AssertEquals(false, nodeNameList.GroupBy(n => n).Any(c => c.Count() > 1));
			}
		}

		public void TestXMLGenerationWithNoDuplicateChildren()
		{
			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			manufacturer.MainAddress.OA_Code = "101 Main Street";
			manufacturer.MainAddress.Address1 = "101 Main Street";
			var address = manufacturer.Addresses.AddNew();
			address.OA_Code = "184 Test Street";
			address.Address1 = "184 Test Street";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "003008787159", Core.Constants.CountryCodes.UnitedStates);
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "56668558", Core.Constants.CountryCodes.UnitedStates);
			var craig = factory.New<OrgHeader>();
			craig.OH_Code = "CRAIMPCHI";
			factory.Save();

			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.TestFiles.Product_PRODUCTA_XYL.xml"));
			var xml = reader.ReadToEnd();
			reader.Close();
			var stream = new MemoryStream(Encoding.Default.GetBytes(xml));

			importHandler.Import(stream);
			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PRODUCTA_XYL");
			var product = factory.LoadTop1<Enterprise.Integration.Customs.US.IOrgSupplierPart>(query) as BusinessObject;

			AssertNotNull(product);

			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
			var serializer = new NativeXmlSerializer { Converter = converter };
			var exportService = new NativeXmlExportService { Serializer = serializer };
			stream = new MemoryStream();
			exportService.Export(new[] { product }, stream);
			stream.Position = 0;
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			var manufacturerElements = xmlDoc.GetElementsByTagName("Manufacturer");

			foreach (XmlElement element in manufacturerElements)
			{
				var orgCusCodeCollection = element.GetElementsByTagName("OrgCusCodeCollection")[0];
				AssertEquals("Only two OrgCusCode should be exported", 2, orgCusCodeCollection.ChildNodes.Count);
				var mIDCode = orgCusCodeCollection.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.GetElementsByTagName("CodeType")[0].InnerText == "MID");
				var fRMCode = orgCusCodeCollection.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.GetElementsByTagName("CodeType")[0].InnerText == "FRM");

				AssertEquals("No more OrgAdress node should be exported for OrgCusCode", 0, mIDCode.GetElementsByTagName("Manufacturer").Count);
				AssertEquals("No more OrgAdress node should be exported for OrgCusCode", 0, fRMCode.GetElementsByTagName("Manufacturer").Count);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			ns = NativeXmlInfo.Namespace_2011_11;

			var session = new AncillaryImportServices();
			importHandler = new ImportHandler(session);
			factory = new BusinessObjectFactory();
		}

		XNamespace ns;
		ImportHandler importHandler;
		BusinessObjectFactory factory;
		#endregion
	}
}
