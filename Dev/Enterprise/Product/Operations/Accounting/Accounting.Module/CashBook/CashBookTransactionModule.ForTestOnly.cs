#if DEBUG

using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public partial class CashBookTransactionModule
	{
		public MenuItem[] GetNewStandardMenuItems_ForTestOnly()
		{
			return GetNewStandardMenuItems();
		}

		public MultilingualString NewOpeningReceiptText_ForTestOnly
		{
			get { return NewOpeningReceiptText; }
			set { NewOpeningReceiptText = value; }
		}

		public MultilingualString NewOpeningPaymentText_ForTestOnly
		{
			get { return NewOpeningPaymentText; }
			set { NewOpeningPaymentText = value; }
		}

		public MultilingualString NewBankCurrencyAdjustmentText_ForTestOnly
		{
			get { return NewBankCurrencyAdjustmentText; }
			set { NewBankCurrencyAdjustmentText = value; }
		}

		public MultilingualString NewBankTransferText_ForTestOnly
		{
			get { return NewBankTransferText; }
			set { NewBankTransferText = value; }
		}

		public MultilingualString NewDirectReceiptText_ForTestOnly
		{
			get { return NewDirectReceiptText; }
			set { NewDirectReceiptText = value; }
		}

		public MultilingualString NewDirectPaymentText_ForTestOnly
		{
			get { return NewDirectPaymentText; }
			set { NewDirectPaymentText = value; }
		}

		public MultilingualString PrintMenuItemText_ForTestOnly
		{
			get { return PrintMenuItemText; }
			set { PrintMenuItemText = value; }
		}

		public MultilingualString PrintAccountingVoucherText_ForTestOnly
		{
			get { return PrintAccountingVoucherText; }
			set { PrintAccountingVoucherText = value; }
		}

		public MultilingualString ExportPositivePayMenuItemText_ForTestOnly
		{
			get { return ExportPositivePayMenuItemText; }
			set { ExportPositivePayMenuItemText = value; }
		}

		public MultilingualString CustomizePositivePayExportMenuItemText_ForTestOnly
		{
			get { return CustomizePositivePayExportMenuItemText; }
			set { CustomizePositivePayExportMenuItemText = value; }
		}

		public MultilingualString ExportCheckPaymentsMenuItemText_ForTestOnly
		{
			get { return ExportCheckPaymentsMenuItemText; }
			set { ExportCheckPaymentsMenuItemText = value; }
		}

		public MultilingualString CustomizeCheckPaymentsExportMenuItemText_ForTestOnly
		{
			get { return CustomizeCheckPaymentsExportMenuItemText; }
			set { CustomizeCheckPaymentsExportMenuItemText = value; }
		}

		public void HandleReallocateCheckNumber_ForTestOnly(object sender, EventArgs e)
		{
			HandleReallocateCheckNumber(sender, e);
		}

		public void HandleReprint_ForTestOnly(object sender, EventArgs e)
		{
			HandleReprint(sender, e);
		}

		public void HandleExportPositivePay_ForTestOnly(object sender, EventArgs e)
		{
			HandleExportPositivePay(sender, e);
		}

		public void HandleCustomizePositivePayExport_ForTestOnly(object sender, EventArgs e)
		{
			HandleCustomizePositivePayExport(sender, e);
		}

		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}

		public void HandlePrintAccountingJournal_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintAccountingJournal(sender, e);
		}

		public SecurityCheckpoint AuditSecurityCheckpoint_ForTestOnly => AuditSecurityCheckpoint;
		public SecurityCheckpoint UndoAuditSecurityCheckpoint_ForTestOnly => UndoAuditSecurityCheckpoint;
		public SecurityCheckpoint RecordCashierSecurityCheckpoint_ForTestOnly => RecordCashierSecurityCheckpoint;
		public SecurityCheckpoint ClearCashierSecurityCheckpoint_ForTestOnly => ClearCashierSecurityCheckpoint;

		public static string CannotPrintExchangeDifference_ForTestOnly => CannotPrintExchangeDifference;
		public static string CannotPrintOpeningReceipt_ForTestOnly => CannotPrintOpeningReceipt;
		public static string CannotPrintOpeningPayment_ForTestOnly => CannotPrintOpeningPayment;
		public static string CannotCopyTransaction_ForTestOnly => CannotCopyTransaction;

		public static TransactionHeader[] ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher_ForTestOnly(TransactionHeader[] gridSelections)
		{
			return ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher(gridSelections);
		}
	}
}

#endif
