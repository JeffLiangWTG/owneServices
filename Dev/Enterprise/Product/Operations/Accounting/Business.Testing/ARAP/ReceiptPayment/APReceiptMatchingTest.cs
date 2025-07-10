using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(APReceipt))]
	public class APReceiptMatchingTest : ReceiptPaymentBaseMatchingTest
	{
		protected override ReceiptPaymentBase GetNewReceiptPayment()
		{
			return Factory.New<APReceipt>();
		}
	}
}
