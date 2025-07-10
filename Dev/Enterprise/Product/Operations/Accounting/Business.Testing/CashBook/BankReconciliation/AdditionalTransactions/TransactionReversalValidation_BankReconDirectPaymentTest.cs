using System;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public class TransactionReversalValidation_BankReconDirectPaymentTest : DirectPaymentTransactionReversalValidationTest
	{
		protected override Type HeaderType
		{
			get { return typeof(BankReconDirectPayment); }
		}
	}
}
