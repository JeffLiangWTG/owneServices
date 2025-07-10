using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	public class TransactionReversalValidation_DepositBatchTest : TransactionReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(DepositBatch); }
		}
	}
}
