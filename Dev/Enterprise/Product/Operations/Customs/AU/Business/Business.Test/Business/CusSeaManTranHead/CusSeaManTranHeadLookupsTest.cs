using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManTranHeadLookupsTest : Customs.Business.Testing.CusSeaManTranHeadLookupsTest
	{
		public void TestImpendingArrivalStatusList()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			AssertEquals(typeof(CMRBaseStatuses), header.Lookups.ImpendingArrivalStatusList.GetType());
		}

		public void TestAllArrivalPortsWithAllValue()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();
			CusSeaManArrivalPort arrivalPort = manifest.Arrivals.AddNew();
			arrivalPort.BA_RL_NKArrivalPort = "AUSYD";

			Assert(manifest.Lookups.AllArrivalPortsWithAllValue.ContainsCode("AUSYD"));
			Assert(manifest.Lookups.AllArrivalPortsWithAllValue.ContainsCode("ALL"));
		}
	}
}
