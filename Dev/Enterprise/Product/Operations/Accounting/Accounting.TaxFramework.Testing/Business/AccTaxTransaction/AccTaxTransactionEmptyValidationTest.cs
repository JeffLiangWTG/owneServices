using CargoWise.EntityFramework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class AccTaxTransactionEmptyValidationTest : AccTaxTransactionValidationTest
	{
		protected override AccTaxTransaction CreateTaxTransaction()
		{
			var taxTransaction = base.CreateTaxTransaction();
			taxTransaction.ATT_IsCancelled = true;
			return taxTransaction;
		}

		protected override void AssertError(ZPropertyInfo propertyInfo, string message)
		{
			AssertNoErrors(message, propertyInfo);
		}
	}
}
