using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ARPayment))]
	public class ARPaymentMatchingTest : ReceiptPaymentBaseMatchingTest
	{
		protected override ReceiptPaymentBase GetNewReceiptPayment()
		{
			return Factory.New<ARPayment>();
		}
	}
}
