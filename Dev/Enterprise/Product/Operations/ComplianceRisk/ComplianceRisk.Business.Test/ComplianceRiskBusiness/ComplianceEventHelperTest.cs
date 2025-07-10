using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class CommodityEventHelperTest : TestCaseWithFactory
	{
		public void TestAddCommodityAdditionLogForBillingWhenAssessmentInitialized()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_OverallRisk = "CLR";
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "CLR";

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456";
			AssertEquals("Precondition: ", 0, complianceRiskStatus.GetEventLogs().Count());

			var commodityHelper = new CommodityEventHelper(complianceRiskStatus);
			commodityHelper.AddCommodityAdditionLogForBillingWhenAssessmentInitialized(true);

			var eventLog = complianceRiskStatus.GetEventLogs().Single();
			CombineAssertions(() =>
			{
				AssertEquals("CVO", eventLog.SCE_EventType);
				AssertEquals("CPC", eventLog.SCE_EventSubType);
				AssertEquals(2, eventLog.SCE_ItemsCount);
				AssertNotEquals(ZArchitecture.Environment.User.ServiceUserCode, eventLog.SCE_SystemCreateUser);
				AssertNotEquals(ZArchitecture.Environment.User.ServiceUserCode, eventLog.SCE_SystemLastEditUser);
			});

			commodityHelper.AddCommodityAdditionLogForBillingWhenAssessmentInitialized(true);
			eventLog = complianceRiskStatus.GetEventLogs().Single();
			CombineAssertions("No duplicates added", () =>
			{
				AssertEquals("CVO", eventLog.SCE_EventType);
				AssertEquals("CPC", eventLog.SCE_EventSubType);
				AssertEquals(2, eventLog.SCE_ItemsCount);
				AssertNotEquals(ZArchitecture.Environment.User.ServiceUserCode, eventLog.SCE_SystemCreateUser);
				AssertNotEquals(ZArchitecture.Environment.User.ServiceUserCode, eventLog.SCE_SystemLastEditUser);
			});
		}

		public void TestAddCommodityAdditionAndDeletionLogIfNeeded()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_OverallRisk = "CLR";
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "CLR";
			Factory.Save();

			var commodityHelper = new CommodityEventHelper(complianceRiskStatus);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456";
			commodity2.CCD_SystemCreateTimeUtc = new ZDateTime(2024, 9, 6, 1, 2, 3);

			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "789";
			Factory.Save();

			commodity1.Delete();
			CommodityEventHelper.AddCommodityAdditionAndDeletionLogIfNeeded(complianceRiskStatus);
			AssertEquals("No Addition/Deletion log when Assessment not initiated.", 0, complianceRiskStatus.GetEventLogs().Count(s => s.SCE_EventType == "CVO" || s.SCE_EventType == "CCI"));

			ComplianceRiskStatusWorkflowInitializer.InitializeAssessmentWorkflow(complianceRiskStatus);
			commodity2.Delete();

			CommodityEventHelper.AddCommodityAdditionAndDeletionLogIfNeeded(complianceRiskStatus);
			var additionLog = complianceRiskStatus.GetEventLogs().Single(s => s.SCE_EventType == "CVO");
			AssertEquals("CPC", additionLog.SCE_EventSubType);
			AssertEquals(2, additionLog.SCE_ItemsCount);

			var deletionLog = complianceRiskStatus.GetEventLogs().Single(s => s.SCE_EventType == "CCI");
			AssertEquals("CLD", deletionLog.SCE_EventSubType);
			AssertEquals(1, deletionLog.SCE_ItemsCount);
			AssertEquals("{\"Commodities\":[{\"Code\":\"456\",\"Conditions\":\"\",\"HsCodeDescription\":\"\",\"RiskStatus\":\"NCH\",\"NomenclatureCondition\":\"\",\"SpecificCondition\":\"\",\"Source\":\"\",\"CommoditySource\":\"Compliance\",\"Notes\":\"\",\"GoodsDescription\":\"\",\"OriginOfGoods\":\"\",\"IsAssessmentInitiated\":true,\"DateAddedUtc\":\"2024-09-06T01:02:03\"}]}", deletionLog.SCE_Snapshot);
		}
	}
}
