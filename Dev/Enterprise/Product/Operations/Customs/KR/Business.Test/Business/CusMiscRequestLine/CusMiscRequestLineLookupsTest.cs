using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusMiscRequestLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceNumberTypeList()
		{
			var header = Factory.New<CusMiscRequestLine>();
			var lookups = new CusMiscRequestLineLookups(header);
			AssertEquals("IMP, EXP, CMN", lookups.ReferenceNumberTypeList.CodesAsString);
		}
	}
}
