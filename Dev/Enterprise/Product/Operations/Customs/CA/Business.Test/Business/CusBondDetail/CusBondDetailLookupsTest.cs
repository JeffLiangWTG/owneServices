using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusBondDetailLookupsTest : TestCaseWithFactory
	{
		public void TestBondDetailDataLookups()
		{
			var bondData = Factory.New<CusBondDetail>();
			bondData.Parent = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("BondTypeList", Factory.GetCachedValue<BondTypeList>(), bondData.Lookups.BondTypeList);
		}
	}
}
