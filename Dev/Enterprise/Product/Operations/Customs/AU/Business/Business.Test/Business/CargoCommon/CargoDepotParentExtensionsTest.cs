using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoDepotParentExtensionsTest : TestCaseWithFactory
	{
		public void TestOutturnCargo()
		{
			var hawb = Factory.New<CusHAWB>();
			var outturn = Factory.New<CusOutturn>();
			outturn.C5_PackagesOutturned = 1;
			AssertEquals(0, hawb.CargoReceivedAtDepotLogs.Count);
			hawb.OutturnCargo(outturn);
			AssertEquals(1, hawb.CargoReceivedAtDepotLogs.Count);
			outturn.C5_PackagesOutturned = 0;
			outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			hawb.OutturnCargo(outturn);
			AssertEquals(0, hawb.CargoReceivedAtDepotLogs.Count);
		}
	}
}
