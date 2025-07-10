using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public class DirectPaymentLookupsTest : DirectTransactionHeaderBaseLookupsTest
	{
		public override void TestReceiptPaymentMethodsList()
		{
			DirectPayment.DirectPayment payment = Factory.New<DirectPayment.DirectPayment>();
			AssertNotNull("ReceiptPaymentMethodsList", payment.Lookups.ReceiptPaymentMethodsList);
			AssertEquals("LookupEditType", OLookUpEditType.PaymentMethod, payment.Lookups.ReceiptPaymentMethodsList.LookupEditType);
		}
	}
}