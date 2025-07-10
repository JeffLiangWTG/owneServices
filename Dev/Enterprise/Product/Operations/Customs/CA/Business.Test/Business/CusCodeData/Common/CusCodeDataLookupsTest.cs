using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var number = Factory.New<SITTCertificationNumber>();
			AssertEquals(typeof(CusCodeDataTypeList), new CusCodeDataLookups(number).CY_CodeList.GetType());
		}
	}
}
