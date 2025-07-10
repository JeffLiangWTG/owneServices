using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirManifestDataContextManagerTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportUniversalEventMatchesToCorrectData()
		{
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "08112302135";
			mawb1.CM_MasterHouseBill = ZString.Empty;
			mawb1.CM_ArrivalDate = ZDateTime.Today;
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB896587";
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "08112302135";
			mawb2.CM_MasterHouseBill = "MB232423";
			mawb2.CM_ArrivalDate = ZDateTime.Today;
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB234423";
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
      </Company>
    </DataContext>

    <EventTime>2013-03-20T09:49:21.9</EventTime>
    <EventType>CAD</EventType>

    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>08112302135</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>HB234423</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
");
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Service Task Log",
				"Linked Event to Air Cargo House (HAWB: HB234423).",
				serviceTaskLog.ToString());
			AssertEquals("Message Log Note",
				"Linked Event to Air Cargo House (HAWB: HB234423).",
				message.GetLogNoteText());

			var logs = mawb1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("No [CAD] event should be linked to mawb1", 0, logs.Length);
			logs = hawb1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("No [CAD] event should be linked to hawb1", 0, logs.Length);
			logs = mawb2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("No [CAD] event should be linked to mawb2", 0, logs.Length);
			logs = hawb2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("One [CAD] event should be linked to hawb2", 1, logs.Length);
		}

		public void TestImportUniversalEventForMAWB()
		{
			var oldMawb = Factory.New<CusMAWB>();
			oldMawb.CM_MAWB = "08112302135";
			oldMawb.CM_MasterHouseBill = "MHB";
			oldMawb.CM_ArrivalDate = ZDateTime.Today.AddMonths(-13);

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "08112302135";
			mawb.CM_MasterHouseBill = "MHB";
			mawb.CM_ArrivalDate = ZDateTime.Today.AddMonths(-1);

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(new EmbeddedResourceRetriever().GetString(GetTestFilePathFor("CADUniversalEventForMAWB.xml")));
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Service Task Log",
				"Linked Event to AirCargo Report (MAWB: 081-12302135 MHB: MHB).",
				serviceTaskLog.ToString());
			AssertEquals("Message Log Note",
				"Linked Event to AirCargo Report (MAWB: 081-12302135 MHB: MHB).",
				message.GetLogNoteText());

			var logs = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("[CAD] event count", 1, logs.Length);

			var contextItems = logs[0].SourceInfoItems;
			var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select(item => item.Key + " - " + item.Data));
			AssertMultilineASCIIEquals("Context Items on Event", @"
MAWB Number - 08112302135
Master House Bill - MHB
Data Source Company - EDI
				".Trim(), actualContextItems);
		}

		public void TestImportUniversalEventForMAWBWithoutMasterHouse()
		{
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "08112302135";
			mawb1.CM_MasterHouseBill = "MHB1";
			mawb1.CM_ArrivalDate = ZDateTime.Today.AddMonths(-1);

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "08112302135";
			mawb2.CM_MasterHouseBill = "MHB2";
			mawb2.CM_ArrivalDate = ZDateTime.Today;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(new EmbeddedResourceRetriever().GetString(GetTestFilePathFor("CADUniversalEventForMAWBWithoutMasterHouse.xml")));
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertContainsExactLinesInAnyOrder("Service Task Log",
				"Linked Event to AirCargo Report (MAWB: 081-12302135 MHB: MHB1)." + System.Environment.NewLine +
				"Linked Event to AirCargo Report (MAWB: 081-12302135 MHB: MHB2).",
				serviceTaskLog.ToString());
			AssertContainsExactLinesInAnyOrder("Message Log Note",
				"Linked Event to AirCargo Report (MAWB: 081-12302135 MHB: MHB1)." + System.Environment.NewLine +
				"Linked Event to AirCargo Report (MAWB: 081-12302135 MHB: MHB2).",
				message.GetLogNoteText());

			var logs = mawb1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("[CAD] event count", 1, logs.Length);

			var contextItems = logs[0].SourceInfoItems;
			var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select(item => item.Key + " - " + item.Data));
			AssertMultilineASCIIEquals("Context Items on Event", @"
MAWB Number - 08112302135
Data Source Company - EDI
				".Trim(), actualContextItems);
		}

		public void TestImportUniversalEventForHAWB()
		{
			var oldMawb = Factory.New<CusMAWB>();
			oldMawb.CM_MAWB = "08112302135";
			oldMawb.CM_MasterHouseBill = "MHB";
			oldMawb.CM_ArrivalDate = ZDateTime.Today.AddMonths(-13);
			oldMawb.ChildBills.AddNew().CS_HAWB = "HAWB1";

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "08112302135";
			mawb.CM_MasterHouseBill = "MHB";
			mawb.CM_ArrivalDate = ZDateTime.Today.AddMonths(-1);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HAWB1";

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(new EmbeddedResourceRetriever().GetString(GetTestFilePathFor("CADUniversalEventForHAWB.xml")));
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Service Task Log",
				"Linked Event to Air Cargo House (HAWB: HAWB1).",
				serviceTaskLog.ToString());
			AssertEquals("Message Log Note",
				"Linked Event to Air Cargo House (HAWB: HAWB1).",
				message.GetLogNoteText());

			var logs = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode));
			AssertEquals("[CAD] event count", 1, logs.Length);

			var contextItems = logs[0].SourceInfoItems;
			var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select(item => item.Key + " - " + item.Data));
			AssertMultilineASCIIEquals("Context Items on Event", @"
MAWB Number - 08112302135
Master House Bill - MHB
HAWB Number - HAWB1
Data Source Company - EDI
				".Trim(), actualContextItems);
		}

		string GetTestFilePathFor(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Universal.AirCargo.Testing.TestFiles." + fileName;
	}
}
