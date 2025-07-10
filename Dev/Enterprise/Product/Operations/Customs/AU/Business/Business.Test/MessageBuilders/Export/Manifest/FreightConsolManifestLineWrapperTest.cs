using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FreightConsolManifestLineWrapperTest : TestCaseWithFactory
	{
		public void TestAirWayBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "08148273748";
			var lineWrapper = new FreightConsolManifestLineWrapper(new FreightConsolManifestHeaderWrapper(consol));
			AssertEquals("08148273748", lineWrapper.AirWaybillNumber);
		}
	}
}
