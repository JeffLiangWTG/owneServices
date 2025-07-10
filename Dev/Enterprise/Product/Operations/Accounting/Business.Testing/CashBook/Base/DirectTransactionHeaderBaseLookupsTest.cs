using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public abstract class DirectTransactionHeaderBaseLookupsTest : BusinessObjectLookupsTestCase
	{
		public abstract void TestReceiptPaymentMethodsList();
	}
}