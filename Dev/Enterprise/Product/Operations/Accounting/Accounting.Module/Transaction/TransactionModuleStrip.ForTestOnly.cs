#if DEBUG

using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class TransactionModuleStrip
	{
		public MultilingualString ResetStatusToQueuedText_ForTestOnly => ResetStatusToQueuedText;
		public MultilingualString SetStatusToAwaitText_ForTestOnly => SetStatusToAwaitText;

		public MultilingualString AuthorizeAndSendText_ForTestOnly => AuthorizeAndSendText;

		public void HandleMatch_ForTestOnly(object sender, EventArgs e)
		{
			HandleMatch(sender, e);
		}

		public MultilingualString PrintTransactionMenuText_ForTestOnly => PrintTransactionMenuText;

		public MultilingualString PrintMatchDocMenuText_ForTestOnly => PrintMatchDocMenuText;

		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}

		public MultilingualString MarkAsNotPrintedMenuText_ForTestOnly => MarkAsNotPrintedMenuText;

		public static string NothingSelectedMessage_ForTestOnly => NothingSelectedMessage;

		public static string NothingToPostAmongSelectedTransactionsMessage_ForTestOnly => NothingToPostAmongSelectedTransactionsMessage;

		public SecurityCheckpoint AuditSecurityCheckpoint_ForTestOnly => AuditSecurityCheckpoint;

		public SecurityCheckpoint UndoAuditSecurityCheckpoint_ForTestOnly => UndoAuditSecurityCheckpoint;

		public SecurityCheckpoint RecordCashierSecurityCheckpoint_ForTestOnly => RecordCashierSecurityCheckpoint;

		public SecurityCheckpoint ClearCashierSecurityCheckpoint_ForTestOnly => ClearCashierSecurityCheckpoint;

		public MultilingualString OverrideInvoiceReferencesMenuText_ForTestOnly => OverrideInvoiceReferencesMenuText;

		public MultilingualString NewInvoiceMenuText_ForTestOnly => NewInvoiceMenuText;

		public MultilingualString NewCreditNoteMenuText_ForTestOnly => NewCreditNoteMenuText;

		public MultilingualString NewAdjustmentNoteMenuText_ForTestOnly => NewAdjustmentNoteMenuText;

		public MultilingualString NewJournalMenuText_ForTestOnly => NewJournalMenuText;

		public MultilingualString NewTransferMenuText_ForTestOnly => NewTransferMenuText;

		public MultilingualString NewContraMenuText_ForTestOnly => NewContraMenuText;

		public MultilingualString NewReceiptMenuText_ForTestOnly => NewReceiptMenuText;

		public MultilingualString OverrideTransactionDescriptionMenuText_ForTestOnly => OverrideTransactionDescriptionMenuText;

		public void HandleResetStatusToQueued_ForTestOnly(object sender, EventArgs e)
		{
			HandleResetStatusToQueued(sender, e);
		}

		public MultilingualString OverrideCashFlowCategoryMenuText_ForTestOnly => OverrideCashFlowCategoryMenuText;

		public SecurityCheckpoint ModifyCashFlowCategoryForPostedTransactionsSecurity_ForTestOnly => ModifyCashFlowCategoryForPostedTransactionsSecurity;

		public SecurityCheckpoint ModifyTransactionDescriptionsSecurity_ForTestOnly => ModifyTransactionDescriptionsSecurity;

		public MultilingualString OverrideAddressContactMenuText_ForTestOnly => OverrideAddressContactMenuText;

		public SecurityCheckpoint ModifyAddressContactForPostedTransactionsSecurity_ForTestOnly => ModifyAddressContactForPostedTransactionsSecurity;

		public MultilingualString OverrideAgreedPaymentMethodMenuText_ForTestOnly => OverrideAgreedPaymentMethodMenuText;

		public SecurityCheckpoint ModifyAgreedPaymentMethodForPostedTransactionsSecurity_ForTestOnly => ModifyAgreedPaymentMethodForPostedTransactionsSecurity;

		public SecurityCheckpoint ModifyDueDateForPostedTransactionsSecurity_ForTestOnly => ModifyDueDateForPostedTransactionsSecurity;

		public ResourceStringData OverrideMatchStatusMenuText_ForTestOnly => OverrideMatchStatusMenuText;

		public SecurityCheckpoint ModifyMatchStatusAndReasonSecurity_ForTestOnly => ModifyMatchStatusAndReasonSecurity;

		public MultilingualString SumSelectedTransactionsMenuItemName_ForTestOnly => SumSelectedTransactionsMenuItemName;

		public ControllerID ExpectedOpenedMatchFormID_ForTestOnly => ExpectedOpenedMatchFormID;

		public SecurityCheckpoint MatchCheckpoint_ForTestOnly => MatchCheckpoint;

		public ZController ControllerFromTransactionType_ForTestOnly(string transactionType)
		{
			return ControllerFromTransactionType(transactionType);
		}

		public MultilingualString PrintAccountingVoucherText_ForTestOnly => PrintAccountingVoucherText;

		public IZForm ShowTemplateCopyForm_ForTestOnly(BusinessObject selectedBusinessObject)
		{
			return ShowTemplateCopyForm(selectedBusinessObject);
		}

		public void HandlePrintMatchingReport_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintMatchingReport(sender, e);
		}

		public void HandlePrintAccountingJournal_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintAccountingJournal(sender, e);
		}

		public ZQuery ExportQuery_ForTestOnly => ExportQuery;

		public SecurityCheckpoint ImportRemittanceFileSecurityCheckpoint_ForTestOnly => ImportRemittanceFileSecurityCheckpoint;

		public ResourceStringData ViewMatchedTransactionsMenuItemText_ForTestOnly => ViewMatchedTransactionsMenuItemText;

		public void HandleSumSelectedTransactions_ForTestOnly(object sender, EventArgs e)
		{
			HandleSumSelectedTransactions(sender, e);
		}

		public ZForm LastShownInvoicePrintingForm_ForTestOnly
		{
			get { return LastShownInvoicePrintingForm; }
			set { LastShownInvoicePrintingForm = value; }
		}

		public void HandleViewMatchedTransactions_ForTestOnly(object sender, EventArgs e)
		{
			HandleViewMatchedTransactions(sender, e);
		}
	}
}

#endif
