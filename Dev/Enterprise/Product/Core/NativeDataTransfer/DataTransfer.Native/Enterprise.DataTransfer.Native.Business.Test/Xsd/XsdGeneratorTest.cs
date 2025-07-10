using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	public class XsdGeneratorTest : TestCaseWithFactory
	{
		//TODO: Will be uncommented under WI00034844 and made to pass. 
		//    public void TestUses_2012_11_NamespaceWhenRegistryItemIsTurnedOn()
		//    {
		//      using (SchemaVersionManager.SetNamespaceForTesting(NativeXmlInfo.Namespace_2012_11))
		//      {
		//        var expectedElement = @"
		//<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Native/2012/11"" version=""2.0"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Native/2012/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
		//  <xs:include schemaLocation=""Native.xsd"" />
		//  <xs:element name=""Dummy"" type=""DummyData"" />
		//  <xs:complexType name=""DummyData"">
		//    <xs:all>
		//      <xs:element name=""DummyBizo"" type=""NativeDummy"" />
		//    </xs:all>
		//    <xs:attribute name=""version"" type=""xs:token"" />
		//  </xs:complexType>
		//  <xs:complexType name=""NativeDummy"">
		//    <xs:all>"
		//          .Trim();

		//        var xsd = xsdGenerator.Generate(TestUtil.GetEntitySetDefinition("Dummy"));
		//        AssertStartsWith("NativeDummy.xsd", expectedElement, xsd.ToString());
		//      }
		//    }
		//TODO: Will be uncommented under WI00034844 and made to pass. 

		public void TestAllGeneratedXsdAreValid_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(NativeXmlInfo.Namespace_2012_11))
			{
				GenerateAndCheckAllNativeSchemas();
			}
		}

		public void TestAllGeneratedXsdAreValid_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(NativeXmlInfo.Namespace_2011_11))
			{
				GenerateAndCheckAllNativeSchemas();
			}
		}

		void GenerateAndCheckAllNativeSchemas()
		{
			using (var allSchemasDirectory = new TempDirectory())
			{
				var xsdWritingService = new NativeXmlImportService();
				xsdWritingService.GenerateAndSaveAllXSDs(allSchemasDirectory);
				xsdWritingService.WriteUniversalCommonSchemaForTesting(allSchemasDirectory);

				var entitySetNames = new string[] { "Order", "Organization", "Declaration", "CurrencyExchangeRate", "UNLOCO", "Airline", "CommodityCode", "Container", "Country", "DangerousGood", "ServiceLevel", "Shipment" };
				var definitions = entitySetNames.Select(TestUtil.GetEntitySetDefinition);

				foreach (var definition in definitions)
				{
					AssertGeneratedXsdForGivenDefinitionIsValid(definition, allSchemasDirectory);
				}
			}
		}

		public void TestAllEntityDefinitionBooleansAreValid()
		{
			var entitySetNames = new string[] { "Order", "Organization", "Declaration", "CurrencyExchangeRate", "UNLOCO", "Airline", "CommodityCode", "Container", "Country", "DangerousGood", "ServiceLevel", "Shipment" };
			var definitions = entitySetNames.Select(TestUtil.GetEntitySetDefinition);

			var expectedBoolColumnDefs = XsdBooleanDataTypeHelper.GetBooleanExcludedProperties();
			var boolColumnDefs = new List<string>();

			foreach (var definition in definitions)
			{
				foreach (var entity in definition.Entities)
				{
					entity.PropertyDefinitions.Where(p => p.ColumnDef.DataType.Contains(DbDataType.Char) && (p.ColumnDef.DefaultValue?.ToString() == BoolDefaultValue.False || p.ColumnDef.DefaultValue?.ToString() == BoolDefaultValue.True)).ForEach(p => boolColumnDefs.Add(p.ColumnDef.Name));
				}
			}
			Assert("Please make it explicit if new column definition should be treated as a boolean in XsdBooleanDataTypeValidator", expectedBoolColumnDefs.EqualIgnoringOrder(boolColumnDefs.Distinct().ToArray()));
		}

		void AssertGeneratedXsdForGivenDefinitionIsValid(EntitySetDefinition definition, string allSchemasDirectory)
		{
			var commonSchemaPath = Path.Combine(allSchemasDirectory, UniversalXmlInfo.CommonSchemaName);
			var commonSchema = XDocument.Load(commonSchemaPath);
			using (var commonSchemaReader = commonSchema.CreateReader())
			{
				var additionalSchemas = new XmlSchemaSet();
				additionalSchemas.Add(SchemaVersionManager.Instance.UniversalNamespace, commonSchemaReader);

				var xsdForDefinition = xsdGenerator.Generate(definition);
				using (var xsdReaderForDefinition = xsdForDefinition.CreateReader())
				{
					var xsdDocument = XDocument.Load(xsdReaderForDefinition);
					AssertNoExceptionThrown(() => xsdDocument.Validate(additionalSchemas, (o, e) => { throw e.Exception; }));
				}
			}
		}

		public void TestRequiredAttributeIsNoLongerPresent()
		{
			var xsd = xsdGenerator.Generate(TestUtil.GetEntitySetDefinition("Dummy"));

			AssertNotContains("Required Attribute should not be present", @":Required=""false""", xsd.ToString());
			AssertNotContains("Required Attribute should not be present", @":Required=""true""", xsd.ToString());
		}

		public void TestGenerateCandidateKeyComments()
		{
			var expected1 = @"
  <xs:complexType name=""NativeZonePivot"">
    <xs:all>
      <!-- Candidate Key: RefUNLOCO.PK + RefZoneHeader.PK -->";

			var expected2 = @"
      <xs:element name=""RefUNLOCO"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <!-- Candidate Key: Code -->";

			var expected3 = @"
      <xs:element name=""RefZoneHeader"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <!-- Candidate Key: Code + Description + ZoneType -->";

			var info = TestUtil.GetEntitySetDefinition("ZonePivot");
			var xsdResult = xsdGenerator.Generate(info).ToString();

			CombineAssertions(() =>
			{
				AssertContains(expected1, xsdResult);
				AssertContains(expected2, xsdResult);
				AssertContains(expected3, xsdResult);
			});
		}

		public void TestGenerateXsd()
		{
			var info = TestUtil.GetEntitySetDefinition("Dummy");
			var schemaElement = xsdGenerator.Generate(info);

			AssertEquals(xmlSchemaNamespace + "schema", schemaElement.Name);

			var targetNamespaceAttribute = schemaElement.Attribute("targetNamespace");
			AssertNotNull(targetNamespaceAttribute);
			AssertEquals(NativeXmlInfo.Namespace_2011_11, targetNamespaceAttribute.Value);

			var versionAttribute = schemaElement.Attribute("version");
			AssertNotNull(versionAttribute);
			AssertEquals(NativeXmlInfo.Version_2011_11, versionAttribute.Value);

			var elementFormDefault = schemaElement.Attribute("elementFormDefault");
			AssertNotNull(elementFormDefault);
			AssertEquals("qualified", elementFormDefault.Value);

			var xmlnsAttribute = schemaElement.Attribute("xmlns");
			AssertNotNull(xmlnsAttribute);
			AssertEquals(NativeXmlInfo.Namespace_2011_11, xmlnsAttribute.Value);

			var schemaNamespaceAttribute = schemaElement.GetNamespaceOfPrefix("xs");
			AssertNotNull(schemaNamespaceAttribute);
			AssertEquals("http://www.w3.org/2001/XMLSchema", schemaNamespaceAttribute.NamespaceName);

			var includeSchema = schemaElement.Element(xmlSchemaNamespace + "include");
			AssertNotNull(includeSchema);
			AssertEquals("Native.xsd", includeSchema.Attribute("schemaLocation").Value);

			AssertEquals(1, schemaElement.Elements(xmlSchemaNamespace + "element").Count());

			AssertEquals("namedType_Description", ((XElement)schemaElement.LastNode).Attribute("name").Value);
		}

		public void TestGenerateRelativeElement()
		{
			var dummy = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var schema = xsdGenerator.GenerateRelativeElement(dummy);
			var minOccurAttribute = schema.Attribute("minOccurs");
			AssertNotNull(minOccurAttribute);
			AssertEquals("0", minOccurAttribute.Value);
		}

		public void TestGenerateMainElement()
		{
			var dummy = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var schema = xsdGenerator.GenerateRootEntityXsd(dummy.EntitySetDefinition);
			var minOccurAttribute = schema.Attribute("minOccurs");
			AssertNull(minOccurAttribute);
		}

		public void TestXSDUsesTypedTopLevelEntity()
		{
			var expectedElement = @"
<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""Native.xsd"" />
  <xs:element name=""Dummy"" type=""DummyData"" />
  <xs:complexType name=""DummyData"">
    <xs:all>
      <xs:element name=""DummyBizo"" type=""NativeDummy"" />
    </xs:all>
    <xs:attribute name=""version"" type=""xs:token"" />
  </xs:complexType>
  <xs:complexType name=""NativeDummy"">
    <xs:all>"
				.Trim();

			var xsd = xsdGenerator.Generate(TestUtil.GetEntitySetDefinition("Dummy"));
			AssertStartsWith("NativeDummy.xsd", string.Format(expectedElement, NativeXmlInfo.Version_2011_11), xsd.ToString());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			xsdGenerator = new NativeXsdGenerator();
		}

		NativeXsdGenerator xsdGenerator;
		readonly XNamespace xmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

		#endregion
	}
}
