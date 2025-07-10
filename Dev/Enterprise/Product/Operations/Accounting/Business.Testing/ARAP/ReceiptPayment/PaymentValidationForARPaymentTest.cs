using System;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class PaymentValidationForARPaymentTest : PaymentValidationTest
	{
		protected override Type GetValidationParentType()
		{
			return typeof(ARPayment);
		}
	}
}
