using System;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class PaymentMatchingValidationAPPaymentTest : PaymentMatchingValidationTest
	{
		protected override Type GetValidationParentType()
		{
			return typeof(APPayment);
		}
	}
}
