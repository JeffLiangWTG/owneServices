using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(RemovedCommoditiesLog))]
	public class RemovedCommoditiesLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitFromStmComplianceEvent_WithDeletedCommoditiesSnapshot()
		{
			var snapshot = new RemovedCommoditiesSnapshot();
			snapshot.Commodities.Add(new Commodity { Code = "C1", Conditions = "C2", HsCodeDescription = "D1", RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked, Source = "S1", CommoditySource = "C3", Notes = "N1", IsAssessmentInitiated = true });

			var eventLog = Factory.New<StmComplianceEvent>();
			eventLog.SCE_EventType = ComplianceEventList.EventType.ComplianceCommodityInteraction;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.CommodityLineDeleted;
			eventLog.SCE_EventTimeOffset = new ZDateTimeOffset(new ZDateTime(2024, 8, 7, 10, 51, 09));
			eventLog.SCE_SystemCreateTimeUtc = new ZDateTime(2024, 8, 7, 10, 52, 01);
			eventLog.SCE_SystemCreateUser = "UX1";
			eventLog.SCE_Snapshot = JsonConvert.SerializeObject(snapshot);
			var log = new RemovedCommoditiesLog(eventLog);

			AssertEquals(eventLog.SCE_EventTimeOffset.ToDateTime(), log.EventDateTime);
			AssertEquals(eventLog.SCE_SystemCreateTimeUtc.ToLocalBranchTime(), log.PostedDateTimeLocal);
			AssertEquals("UX1", log.User);
			AssertEquals(1, log.CommodityRiskLogCollection.Count);

			var commodityLine = (ComplianceCommodityRiskLog)log.CommodityRiskLogCollection.Single();
			AssertEquals("C1", commodityLine.Code);
			AssertEquals("C2", commodityLine.Conditions);
			AssertEquals("D1", commodityLine.HsCodeDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodityLine.RiskStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Blocked, commodityLine.RiskStatusDescription);
			AssertEquals("S1", commodityLine.Source);
			AssertEquals("C3", commodityLine.CommoditySource);
			AssertEquals("N1", commodityLine.Notes);
		}

		public void TestEmptyLog()
		{
			var log = RemovedCommoditiesLog.EmptyLog;
			Assert(log.EventDateTime.IsEmpty);
			Assert(log.PostedDateTimeLocal.IsEmpty);
			Assert(log.User.IsEmpty);
			AssertEquals(0, log.CommodityRiskLogCollection.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RemovedCommoditiesLog(Factory.New<StmComplianceEvent>());
		}
	}
}
