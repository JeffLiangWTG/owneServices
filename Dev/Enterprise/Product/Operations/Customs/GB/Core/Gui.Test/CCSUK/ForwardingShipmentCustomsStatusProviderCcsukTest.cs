using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class ForwardingShipmentCustomsStatusProviderCcsukTest : TestCaseWithFactory
	{
		public void TestCustomsCargoStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var customsStatusProvider = Enterprise.Customs.Forwarding.Business.ForwardingShipmentCustomsStatusProvider.New(shipment);

			AssertEquals("Not valid for CCSUK", "", customsStatusProvider.CustomsCargoStatus());

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = "GBLHR";
			shipment.JS_HouseBill = "DANIEL01";
			var consol = shipment.Consols.AddNew();
			consol.JK_MasterBillNum = "11122222222";

			AssertEquals("Valid but no HAWBs", "No HAWB(s) found", customsStatusProvider.CustomsCargoStatus());

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			hawb.PresenceOnNetworkStatus = "XXX";

			AssertContains("XXX", customsStatusProvider.CustomsCargoStatus());

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_JK = consol.PK;
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_JS = shipment.PK;
			hawb2.PresenceOnNetworkStatus = "YYY";

			AssertContains("XXX", customsStatusProvider.CustomsCargoStatus());
			AssertContains("YYY", customsStatusProvider.CustomsCargoStatus());
		}
	}
}
