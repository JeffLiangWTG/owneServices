using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(ConsolToManyMawbsPluginHelper))]
	class ConsolToManyMawbsPluginHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEverything()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "11122222222";
			var mawb1Fk = Factory.New<CusMAWB>();
			var mawb2MasterNum = Factory.New<CusMAWB>();
			var helper = new ConsolToManyMawbsPluginHelper(consol);
			AssertEquals("No mawbs match", 0, helper.Mawbs.Count);
			mawb1Fk.CM_JK = consol.PK;
			AssertEquals("One linke dby FK matches", 1, helper.ResetReloadAndCount());
			mawb2MasterNum.CM_MAWB = "11122222222";
			AssertEquals("Another matches by mawb number", 2, helper.ResetReloadAndCount());
			mawb2MasterNum.CM_ArrivalDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("Setting a cuirrent date doesn't exclude", 2, helper.ResetReloadAndCount());
			mawb2MasterNum.CM_ArrivalDate = ZDateTime.Now.AddDays(-367);
			AssertEquals("An old arrival date does exclude", 1, helper.ResetReloadAndCount());

			helper.MakeNewMawbAndPurgeCache();
			AssertEquals(2, helper.Mawbs.Count);
			AssertEquals(consol.PK, helper.Mawbs[0].CM_JK);
			AssertEquals(consol.PK, helper.Mawbs[1].CM_JK);

			mawb1Fk.CM_JK = ZGuid.Empty;
			mawb2MasterNum.CM_JK = ZGuid.Empty;
			helper.RegisterChildMawbsToConsol();
			AssertEquals(consol.PK, helper.Mawbs[0].CM_JK);
			AssertEquals(consol.PK, helper.Mawbs[1].CM_JK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "11122222222";
			var mawb1Fk = Factory.New<CusMAWB>();
			var mawb2MasterNum = Factory.New<CusMAWB>();
			return new ConsolToManyMawbsPluginHelper(consol);
		}
	}

	[TestedType(typeof(ShipmentToManyHawbsPluginHelper))]
	class ShipmentToManyHawbsPluginHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEverything()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "11122222222";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HAWB0001";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;  // the link between mawb and consol is immaterial to the shipment helper, so long as they are linked by FK or NK
			var helper = new ShipmentToManyHawbsPluginHelper(shipment, consol);
			AssertEquals(0, helper.Hawbs.Count);
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals("A hawb exists by it has no relation to shipment", 0, helper.ResetReloadAndCount());
			hawb.CS_JS = shipment.PK;
			AssertEquals("A hawb exists linked by FK to shipment", 1, helper.ResetReloadAndCount());
			hawb.CS_JS = ZGuid.Empty;
			hawb.CS_HAWB = shipment.JS_HouseBill;
			AssertEquals("A hawb exists linked by NK to shipment", 1, helper.ResetReloadAndCount());
			mawb.CM_JK = ZGuid.Empty;
			AssertEquals("A hawb exists linked to shipment by NK, but the mawb is not linked ot the consol", 0, helper.ResetReloadAndCount());

			mawb.CM_JK = consol.PK;
			helper.ResetReloadAndCount();
			AssertNotEquals(shipment.PK, hawb.CS_JS);
			helper.RegisterHawbsToShipment();
			AssertEquals(shipment.PK, hawb.CS_JS);

			mawb.CM_JK = consol.PK;
			helper.MakeNewHawbAndPurgeCache(mawb);
			AssertEquals("New hawb made", 2, helper.ResetReloadAndCount());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "11122222222";
			var shipment = consol.Shipments.AddNew();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			return new ShipmentToManyHawbsPluginHelper(shipment, consol);
		}
	}
}
