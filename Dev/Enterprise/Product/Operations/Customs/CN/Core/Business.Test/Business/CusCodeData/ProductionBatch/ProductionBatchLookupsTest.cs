using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ProductionBatchLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var testList = Factory.New<ProductionBatch>().Lookups.CY_CodeList;
			AssertEquals(1, testList.Count);
			Assert(testList.ContainsCode(Constants.CusCodeDataCode.BatchNumber));
		}
	}
}
