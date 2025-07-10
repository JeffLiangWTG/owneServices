#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class APTransactionModuleStrip
	{
		internal IZForm LastShownBulkAPInvoicePostingForm_ForTestOnly { get; private set; }

		internal ZForm LastShownAPPaymentBatchPostingForm_ForTestOnly { get; private set; }

		public void DisplayImportedTransactionForm_ForTestOnly(Business.ARAP.Invoicing.InvoicingBase importedTransaction)
		{
			DisplayImportedTransactionForm(importedTransaction);
		}

		public void HandleReallocateCheckNumber_ForTestOnly(object sender, EventArgs e)
		{
			HandleReallocateCheckNumber(sender, e);
		}

		public void HandleReprint_ForTestOnly(object sender, EventArgs e)
		{
			HandleReprint(sender, e);
		}

		public MultilingualString PrintSelfBillingInvoiceMenuText_ForTestOnly => PrintSelfBillingInvoiceMenuText;

		public void HandlePrintSelfBillingInvoice_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintSelfBillingInvoice(sender, e);
		}

		public void HandlePrintAutofatturaInvoice_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintAutofatturaInvoice(sender, e);
		}

		public MenuItem[] GetOverrideDetailsMenuItems_ForTestOnly()
		{
			return GetOverrideDetailsMenuItems();
		}

		public TransactionHeaderCollection FilterSelectedTransactions_ForTestOnly(BusinessObject[] gridTransactions)
		{
			return FilterSelectedTransactions(gridTransactions);
		}

		public void HandleExportRemittanceInfo_ForTestOnly(object sender, EventArgs e)
		{
			HandleExportRemittanceInfo(sender, e);
		}

		public MenuItem[] GetEInvoicingActionMenuItems_ForTestOnly()
		{
			return EInvoicingGUIActionHelper.GetActionMenuItems(isAPTransaction: true);
		}
	}
}

#endif
