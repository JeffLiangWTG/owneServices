using System;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class PaymentValidationForAPPaymentTest : PaymentValidationTest
	{
		protected override Type GetValidationParentType()
		{
			return typeof(APPayment);
		}
	}
}
