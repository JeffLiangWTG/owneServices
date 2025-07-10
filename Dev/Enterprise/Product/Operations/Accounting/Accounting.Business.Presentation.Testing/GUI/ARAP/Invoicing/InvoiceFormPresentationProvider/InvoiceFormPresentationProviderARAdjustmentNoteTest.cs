using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Presentation.Testing.GUI
{
	public class InvoiceFormPresentationProviderARAdjustmentNoteTest : InvoiceFormPresentationProviderTest
	{
		protected override ZString TransactionLedger => LedgerTypes.AccountsReceivable;

		protected override Type HeaderType => typeof(ARAdjustmentNote);

		protected override InvoicingBase GetOriginalTransaction() => Factory.NewWithValidTestData(HeaderType) as InvoicingBase;
	}
}
