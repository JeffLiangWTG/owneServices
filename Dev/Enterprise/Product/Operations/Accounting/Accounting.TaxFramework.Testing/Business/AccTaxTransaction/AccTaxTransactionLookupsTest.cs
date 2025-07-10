using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	internal class AccTaxTransactionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void Test_TaxBasisListCount()
		{
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();

			AssertNotNull("TaxBasisList should not be null", taxTransaction.Lookups.TaxBasisList);
			AssertEquals("TaxBasisList.Count", 3, taxTransaction.Lookups.TaxBasisList.Count);
		}
	}
}
