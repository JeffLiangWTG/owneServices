using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class DummyBOImportingTest : TestCaseWithFactory
	{
		public void TestImportingFooStringInStringElement()
		{
			AssertImportInStringElement("FOO");
		}

		public void TestImportingEmptyStringInStringElement()
		{
			AssertImportInStringElement("");
		}

		public void TestImportingXMLInStringElement()
		{
			AssertImportInStringElement("<Badness></Badness>");
		}

		void AssertImportInStringElement(string value)
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();

			Factory.Save();

			string xmlToImport = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Body>
    <Dummy>
      <DummyBizo Action=""MERGE"">
        <PK>{dummyBO.PK}</PK>
        <NVarChar>{value}</NVarChar>
        <Description>{value}</Description>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlToImport)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 0 inserts, 1 updates, 0 deletes
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());

				dummyBO.Reload();
				AssertEquals(nameof(dummyBO.Z0_Description), value, dummyBO.Z0_Description);
				AssertEquals(nameof(dummyBO.Z0_NVarChar), value, dummyBO.Z0_NVarChar);
			}
		}

		public void TestImportingBlankFields()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			var dummyBOXML = string.Empty;
			using (var dataStream = xmlSerializer.SerializeToStream(dummyBO))
			using (var reader = new StreamReader(dataStream))
			{
				dummyBOXML = reader.ReadToEnd();
			}

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(dummyBOXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 0 inserts, 1 updates, 0 deletes
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TestDecimalTruncating()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_AnotherDecimal = 5.12345;

			var dummyBOXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy version=""2.0"">
      <DummyBizo Action=""MERGE"">
        <PK>95c9cfc4-5c9e-4da4-9728-b936109a763b</PK>
        <Guid></Guid>
        <NVarChar></NVarChar>
        <Description>Default</Description>
        <Number>0</Number>
        <Date></Date>
        <Code>NCODE</Code>
        <AnotherDate></AnotherDate>
        <Decimal>123.456</Decimal>
        <AnotherDecimal>5.12345678901234567890123456789012345678901234567890123456789012345678901234567890</AnotherDecimal>
        <AnotherNumber>0</AnotherNumber>
        <BitFalse>false</BitFalse>
        <BitTrue>true</BitTrue>
        <Bool>false</Bool>
        <Byte>0</Byte>
        <Money>0.0000</Money>
        <Short>0</Short>
        <SmallDateTime></SmallDateTime>
        <VarCharMax></VarCharMax>
        <NVarCharMax></NVarCharMax>
        <VarBinaryMax></VarBinaryMax>
        <Xml></Xml>
        <BitFiltered>false</BitFiltered>
        <DateTimeOffset></DateTimeOffset>
        <Geography></Geography>
        <DateOnly></DateOnly>
        <IsSystem>true</IsSystem>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dummyBOXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
<DummyBizo>.<Decimal> - Too many decimal places provided, truncated from 3 to 0 places.
<DummyBizo>.<AnotherDecimal> - Too many decimal places provided, truncated from 28 to 3 places.
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 1 inserts, 0 updates, 0 deletes
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TestDecimalMaxSize()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_AnotherDecimal = 5.12345;

			var dummyBOXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy version=""2.0"">
      <DummyBizo Action=""MERGE"">
        <PK>95c9cfc4-5c9e-4da4-9728-b936109a763b</PK>
        <Description>Default</Description>
        <Code>NCODE</Code>
        <Decimal>123456789012345678</Decimal>
        <AnotherDecimal>123456789012345.123</AnotherDecimal>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dummyBOXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 1 inserts, 0 updates, 0 deletes
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TestDecimalTooBig()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_AnotherDecimal = 5.12345;

			var dummyBOXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy version=""2.0"">
      <DummyBizo Action=""MERGE"">
        <PK>95c9cfc4-5c9e-4da4-9728-b936109a763b</PK>
        <Description>Default</Description>
        <Code>NCODE</Code>
        <Decimal>1234567890123456789</Decimal>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dummyBOXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Dummy failed to Import:
[DummyBizo.Decimal] : Value was too large for a decimal type. Couldn't store <1234567890123456789> in decimal column. Limit is 18 digits before the decimal point but 19 were provided.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TestAnotherDecimalTooBig()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_AnotherDecimal = 5.12345;

			var dummyBOXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy version=""2.0"">
      <DummyBizo Action=""MERGE"">
        <PK>95c9cfc4-5c9e-4da4-9728-b936109a763b</PK>
        <Description>Default</Description>
        <Code>NCODE</Code>
        <AnotherDecimal>1234567890123456.12</AnotherDecimal>
      </DummyBizo>
    </Dummy>
  </Body>
</Native>";

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dummyBOXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Dummy failed to Import:
[DummyBizo.AnotherDecimal] : Value was too large for a decimal type. Couldn't store <1234567890123456.12> in decimal column. Limit is 15 digits before the decimal point but 16 were provided.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.	
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TesttExportingImportingXmlInFields_Simple()
		{
			ExportingImportingXmlInFields("<tag>value</tag>", "&lt;tag&gt;value&lt;/tag&gt;");
		}

		public void TesttExportingImportingXmlInFields_Escaped()
		{
			ExportingImportingXmlInFields("<tag>&lt;value&gt;</tag>", "&lt;tag&gt;&amp;lt;value&amp;gt;&lt;/tag&gt;");
		}

		public void TesttExportingImportingXmlInFields_DoubleEscaped()
		{
			ExportingImportingXmlInFields("<tag>&amp;lt;value&amp;gt;</tag>", "&lt;tag&gt;&amp;amp;lt;value&amp;amp;gt;&lt;/tag&gt;");
		}

		void ExportingImportingXmlInFields(string xmlImport, string xmlExport)
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummyBO = Factory.New<DummyBusinessObject>();

			dummyBO.Z0_Xml = dummyBO.Z0_Description = xmlImport;

			Factory.Save();

			using (var baseStream = NativeDataTransferTestHelper.ExportToStream(dummyBO))
			{
				StreamReader reader = new StreamReader(baseStream, Encoding.UTF8);
				string text = reader.ReadToEnd();

				Assert(text.Contains("<Description>" + xmlExport + "</Description>"));
				Assert(text.Contains("<Xml>" + xmlExport + "</Xml>"));

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
				{
					var manager = new ImportServiceManagerForTesting();
					manager.ImportService.Import(stream);

					var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Dummy
--- Import Process Finished -----------------------------------------------------------
DummyBizo - 0 inserts, 1 updates, 0 deletes
			".Trim();

					AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
				}

				var newBO = Factory.Load<DummyBusinessObject>(dummyBO.PK);
				AssertEquals(dummyBO.Z0_Description, newBO.Z0_Description);
				AssertEquals(dummyBO.Z0_Xml, newBO.Z0_Xml);
			}
		}
	}
}
