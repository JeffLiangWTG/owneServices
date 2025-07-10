using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.OpeningReceipt.Testing
{
	public class TransactionReversalValidation_OpeningReceiptTest : TransactionReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(OpeningReceipt); }
		}
	}
}
