using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(RemovedCommoditiesLogCollection))]
	public class RemovedCommoditiesLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RemovedCommoditiesLogCollection>
	{
		public void TestLoadCollection()
		{
			var provider = (BusinessObject)Factory.New<IForwardingShipment>() as IComplianceItemRiskStatusProvider;
			var eventLog1 = Factory.New<StmComplianceEvent>();
			eventLog1.SCE_ParentID = provider.ParentID;
			eventLog1.SCE_ParentTableCode = provider.ParentTableCode;
			eventLog1.SCE_EventType = ComplianceEventList.EventType.ComplianceCommodityInteraction;
			eventLog1.SCE_EventSubType = ComplianceEventList.Codes.CommodityLineDeleted;
			var eventLog2 = Factory.New<StmComplianceEvent>();
			eventLog2.SCE_ParentID = ZGuid.NewZGuid();
			eventLog2.SCE_ParentTableCode = provider.ParentTableCode;
			eventLog2.SCE_EventType = ComplianceEventList.EventType.ComplianceCommodityInteraction;
			eventLog2.SCE_EventSubType = ComplianceEventList.Codes.CommodityLineDeleted;
			var eventLog3 = Factory.New<StmComplianceEvent>();
			eventLog3.SCE_ParentID = provider.ParentID;
			eventLog3.SCE_ParentTableCode = "XXX";
			eventLog3.SCE_EventType = ComplianceEventList.EventType.ComplianceCommodityInteraction;
			eventLog3.SCE_EventSubType = ComplianceEventList.Codes.CommodityLineDeleted;
			var eventLog4 = Factory.New<StmComplianceEvent>();
			eventLog4.SCE_ParentID = provider.ParentID;
			eventLog4.SCE_ParentTableCode = provider.ParentTableCode;
			eventLog4.SCE_EventType = "ABC";
			eventLog4.SCE_EventSubType = "CBA";

			var collection = new RemovedCommoditiesLogCollection(provider);
			collection.LoadCollection();
			AssertEquals(1, collection.Count);
		}

		public void TestAllowNewCore()
		{
			AssertEquals("Suppress user interface to create log", false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemoveCore()
		{
			AssertEquals("Suppress user interface to delete log", false, GetCollectionToTest().AllowRemove);
		}

		protected override RemovedCommoditiesLogCollection GetCollectionToTest()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			return new RemovedCommoditiesLogCollection((IComplianceItemRiskStatusProvider)shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RemovedCommoditiesLog(Factory.New<StmComplianceEvent>());
		}
	}
}
