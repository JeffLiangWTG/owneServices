using System;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class PaymentMatchingValidationARPaymentTest : PaymentMatchingValidationTest
	{
		protected override Type GetValidationParentType()
		{
			return typeof(ARPayment);
		}
	}
}
