using System;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	class TransactionReversalValidation_BankTransferChargeTest : DirectPaymentTransactionReversalValidationTest
	{
		protected override Type HeaderType
		{
			get { return typeof(BankTransferCharge); }
		}
	}
}
