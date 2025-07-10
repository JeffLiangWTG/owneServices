using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionReversalValidationARCreditNoteTest : InvoicingBaseReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(ARCreditNote); }
		}

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(typeof(ARInvoice)) as InvoicingBase;

		protected override ZString TransactionLedger => LedgerTypes.AccountsReceivable;
	}
}
