using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	public abstract class OverpaymentValidationTest : TransactionHeaderValidationTest
	{
		public void TestCheckBindableInvoiceAmount()
		{
			AROverpayment aROvp = Factory.NewWithValidTestData<AROverpayment>();
			Assert("Precondition: BindableInvoiceAmount on Overpayment should not have errors", !aROvp.BindableInvoiceAmountInfo.HasErrors());
			aROvp.BindableInvoiceAmount = -1M;
			Assert("BindableInvoiceAmount on Overpayment should have error", aROvp.BindableInvoiceAmountInfo.HasErrors());
		}
	}
}
