using System;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class TransactionReversalValidation_BankTransferFromRowTest : TransactionReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(BankTransferFromRow); }
		}
	}
}
