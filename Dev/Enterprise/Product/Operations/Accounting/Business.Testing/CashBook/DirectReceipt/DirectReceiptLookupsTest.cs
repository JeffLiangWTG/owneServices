using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public class DirectReceiptLookupsTest : DirectTransactionHeaderBaseLookupsTest
	{
		public override void TestReceiptPaymentMethodsList()
		{
			DirectReceipt.DirectReceipt receipt = Factory.New<DirectReceipt.DirectReceipt>();
			AssertNotNull("ReceiptPaymentMethodsList", receipt.Lookups.ReceiptPaymentMethodsList);
			AssertEquals("LookupEditType", OLookUpEditType.ReceiptMethod, receipt.Lookups.ReceiptPaymentMethodsList.LookupEditType);
		}
	}
}