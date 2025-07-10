using System;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class ReceiptValidationARReceiptTest : ReceiptValidationTest
	{
		protected override Type GetValidationParentType()
		{
			return typeof(ARReceipt);
		}
	}
}
