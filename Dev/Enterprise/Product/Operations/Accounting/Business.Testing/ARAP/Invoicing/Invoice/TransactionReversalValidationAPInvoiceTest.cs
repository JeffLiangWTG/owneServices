using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionReversalValidationAPInvoiceTest : InvoicingBaseReversalValidationTestCase
	{
		protected override Type HeaderType
		{
			get { return typeof(APInvoice); }
		}

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(typeof(APCreditNote)) as InvoicingBase;

		protected override ZString TransactionLedger => LedgerTypes.AccountsPayable;

		protected override bool ShouldFillTransactionNumber => true;
	}
}
