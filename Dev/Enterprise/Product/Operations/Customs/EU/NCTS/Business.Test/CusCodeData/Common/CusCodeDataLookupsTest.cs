using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CusCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var cusCodeData = Factory.New<Seal>();
			AssertEquals("COR, ITM, SEL", new CusCodeDataLookups(cusCodeData).CY_CodeList.CodesAsString);
		}
	}
}
