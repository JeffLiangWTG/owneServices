using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.Warehouse.Integration;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class SalesChannelTest : TestCaseWithFactory
	{
		public void TestSalesChannelImport()
		{
			string salesChannel = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <SalesChannel>
      <WhsSalesChannel Action=""MERGE"">
        <Code>ABC</Code>
        <Description>ABC Test</Description>
      </WhsSalesChannel>
    </SalesChannel>
  </Body>
</Native>";

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(salesChannel)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: SalesChannel
--- Import Process Finished -----------------------------------------------------------
WhsSalesChannel - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestSalesChannelExport()
		{
			var salesChannel = Factory.New<IWhsSalesChannel>();
			salesChannel.WSH_Code = "ABC";
			salesChannel.WSH_Description = "ABC Test";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string salesChannelXml = "";
			using (var dataStream = xmlSerializer.SerializeToStream((BusinessObject)salesChannel))
			using (var reader = new StreamReader(dataStream))
			{
				salesChannelXml = reader.ReadToEnd();
			}

			AssertNotNullOrEmpty("SalesChannel xml was generated.", salesChannelXml);
			AssertContains("SalesChannel details are correct.", "<WhsSalesChannel Action=\"MERGE\">", salesChannelXml);
			AssertContains("SalesChannel details are correct.", "<Code>ABC</Code>", salesChannelXml);
			AssertContains("SalesChannel details are correct.", "<Description>ABC Test</Description>", salesChannelXml);
		}
	}
}
