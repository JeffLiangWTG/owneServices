using System;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class ReceiptValidationAPReceiptTest : ReceiptValidationTest
	{
		protected override Type GetValidationParentType()
		{
			return typeof(APReceipt);
		}
	}
}
