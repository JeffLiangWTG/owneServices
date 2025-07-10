using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Presentation.Testing.GUI
{
	public class InvoiceFormPresentationProviderAPCreditNoteTest : InvoiceFormPresentationProviderTest
	{
		protected override ZString TransactionLedger => LedgerTypes.AccountsPayable;

		protected override Type HeaderType => typeof(APCreditNote);

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(typeof(APInvoice)) as InvoicingBase;
	}
}
