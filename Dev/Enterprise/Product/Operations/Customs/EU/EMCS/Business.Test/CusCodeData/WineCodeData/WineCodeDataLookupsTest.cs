using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class WineCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var codeData = Factory.NewWithValidTestData<WineCodeData>();
			AssertEquals(typeof(EMCSOperationCodeList), codeData.Lookups.CY_CodeList.GetType());
		}
	}
}
