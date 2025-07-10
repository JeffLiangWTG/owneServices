using System;
using Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public class TransactionReversalValidation_BankReconDirectReceiptTest : TransactionReversalValidation_DirectReceiptTest
	{
		protected override Type HeaderType
		{
			get { return typeof(BankReconDirectReceipt); }
		}
	}
}
