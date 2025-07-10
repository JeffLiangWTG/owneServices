using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class ReceiptPaymentDefaultsTest : TestCaseWithFactory
	{
		public abstract void TestGetDefaultBankAccount();
	}
}
