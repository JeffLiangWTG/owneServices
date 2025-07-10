using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment.Testing
{
	public class TransactionReversalValidation_OpeningPaymentTest : TransactionReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(OpeningPayment); }
		}
	}
}
