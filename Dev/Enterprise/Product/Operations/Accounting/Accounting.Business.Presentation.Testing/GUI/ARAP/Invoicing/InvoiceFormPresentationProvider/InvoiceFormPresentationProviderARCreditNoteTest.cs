using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Presentation.Testing.GUI
{
	public class InvoiceFormPresentationProviderARCreditNoteTest : InvoiceFormPresentationProviderTest
	{
		protected override ZString TransactionLedger => LedgerTypes.AccountsReceivable;

		protected override Type HeaderType => typeof(ARCreditNote);

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(typeof(ARInvoice)) as InvoicingBase;
	}
}
