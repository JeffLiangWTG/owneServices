using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusDV1DetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYesNoList()
		{
			var dv1Detail = Factory.New<CusDV1Detail>();
			var lookups = new CusDV1DetailLookups(dv1Detail);
			AssertEquals("N, Y", lookups.YesNoList.CodesAsString);
		}
	}
}
