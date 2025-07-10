using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class ForwardingConsolCustomsStatusProviderCcsukTest : TestCaseWithFactory
	{
		public void TestCustomsCargoStatus()
		{
			var consol = Factory.New<ForwardingConsol>();

			var customsInformation = new ForwardingConsolCustomsInformation(consol);

			AssertEquals("Not valid for CCSUK", "", customsInformation.CustomsCargoStatus);
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "123";
			AssertEquals("Valid but no CCSUK Mawb", "", customsInformation.CustomsCargoStatus);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.PresenceOnNetworkStatus = "XXX";

			customsInformation = new ForwardingConsolCustomsInformation(consol);
			AssertContains("XXX", customsInformation.CustomsCargoStatus);

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_JK = consol.PK;
			mawb2.PresenceOnNetworkStatus = "YYY";

			customsInformation = new ForwardingConsolCustomsInformation(consol);
			AssertContains("XXX", customsInformation.CustomsCargoStatus);
			AssertContains("YYY", customsInformation.CustomsCargoStatus);

			mawb2.LatestCustomsActionText = "ZZZ";

			customsInformation = new ForwardingConsolCustomsInformation(consol);
			AssertContains("ZZZ", customsInformation.CustomsCargoStatus);
		}
	}
}
