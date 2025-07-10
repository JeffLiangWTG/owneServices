using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoConsolWithScanPlugInTest : AirCargoConsolPlugInTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			mAWB.ChildBills.AddNew();
			return new AirCargoConsolWithScanPlugIn(consol);
		}

		protected override AirCargoConsolPlugIn GetAirCargoConsolPlugin(ForwardingConsol consol) => new AirCargoConsolWithScanPlugIn(consol);
	}
}
