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
	class PutawayGroupTest : TestCaseWithFactory
	{
		public void TestPutawayGroupImport()
		{
			string putawayGroup = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <PutawayGroup>
      <WhsPutawayGroup Action=""MERGE"">
        <Code>ABC</Code>
        <Description>ABC Test</Description>
      </WhsPutawayGroup>
    </PutawayGroup>
  </Body>
</Native>";

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(putawayGroup)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: PutawayGroup
--- Import Process Finished -----------------------------------------------------------
WhsPutawayGroup - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestPutawayGroupExport()
		{
			var putawayGroup = Factory.New<IWhsPutawayGroup>();
			putawayGroup.WPG_Code = "ABC";
			putawayGroup.WPG_Description = "ABC Test";

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string putawayGroupXml = "";
			using (var dataStream = xmlSerializer.SerializeToStream((BusinessObject)putawayGroup))
			using (var reader = new StreamReader(dataStream))
			{
				putawayGroupXml = reader.ReadToEnd();
			}

			AssertNotNullOrEmpty("PutawayGroup xml was generated.", putawayGroupXml);
			AssertContains("PutawayGroup details are correct.", "<WhsPutawayGroup Action=\"MERGE\">", putawayGroupXml);
			AssertContains("PutawayGroup details are correct.", "<Code>ABC</Code>", putawayGroupXml);
			AssertContains("PutawayGroup details are correct.", "<Description>ABC Test</Description>", putawayGroupXml);
		}
	}
}
