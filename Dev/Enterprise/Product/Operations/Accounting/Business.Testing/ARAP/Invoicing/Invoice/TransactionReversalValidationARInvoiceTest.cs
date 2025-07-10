using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionReversalValidationARInvoiceTest : InvoicingBaseReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(ARInvoice); }
		}

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(typeof(ARCreditNote)) as InvoicingBase;

		protected override ZString TransactionLedger => LedgerTypes.AccountsReceivable;
	}
}
