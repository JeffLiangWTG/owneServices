using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	public class DirectPaymentTransactionReversalValidationTest : TransactionReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(DirectPayment); }
		}
	}
}
