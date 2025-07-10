using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	public class TransactionReversalValidation_DirectReceiptTest : TransactionReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(DirectReceipt); }
		}
	}
}
