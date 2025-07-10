#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentBatchForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}

		public void PaymentBatchGrid_CurrentCellChanged_ForTestOnly(object sender, EventArgs e)
		{
			PaymentBatchGrid_CurrentCellChanged(sender, e);
		}

		public ZButton ApplyEXXButton_ForTestOnly
		{
			get { return ApplyEXXButton; }
		}

		public Business.ARAP.ReceiptPayment.APPaymentBatchPoster PaymentBatchPoster_ForTestOnly => PaymentBatchPoster;

		public ZOrganisationsForm ShowOrganisationFormToEdit_ForTestOnly()
		{
			return ShowOrganisationFormToEdit();
		}

		public PaymentBatchPrintManager PrintManager_ForTestOnly => PrintManager;

		public PaymentProcessingGUIHelper PaymentProcessingGUIHelperField_ForTestOnly => paymentProcessingGUIHelper;

		public Core.Forms.ZPostOrCancelButton SaveAndCloseButton_ForTestOnly
		{
			get { return SaveAndCloseButton; }
		}

		public Core.Forms.ZPostOrCancelButton SaveButton_ForTestOnly => SaveButton;
		public Core.Forms.ZPostOrCancelButton SaveAsDraftButton_ForTestOnly => SaveAsDraftButton;

		public ZGrid PaymentBatchGrid_ForTestOnly
		{
			get { return PaymentBatchGrid; }
		}

		public PaymentApprovalBase FirstApproval_ForTestOnly => FirstApproval;

		public PaymentApprovalBase SelectedPayment_ForTestOnly => SelectedPayment;

		public ZButton ExchangeDiffButton_ForTestOnly
		{
			get { return ExchangeDiffButton; }
		}

		public IZForm ShowNewMiscTransactionForm_ForTestOnly(ZString transactionType)
		{
			return ShowNewMiscTransactionForm(transactionType);
		}

		public ZBool PaymentDetailsRefreshed_ForTestOnly
		{
			get { return PaymentDetailsRefreshed; }
			set { PaymentDetailsRefreshed = value; }
		}

		public IEnumerable<Business.Base.Transaction.TransactionHeader> PaymentCollectionForPrinting_ForTestOnly => PaymentCollectionForPrinting;

		public void DeletePaymentTransaction_ForTestOnly(object sender, EventArgs e)
		{
			DeletePaymentTransaction(sender, e);
		}

		public IZForm PaymentBatchGrid_DoubleClick_ForTestOnly()
		{
			return ShowAPPaymentProcessingForm();
		}

		public IZForm PaymentBatchGrid_ViewMenuItemClick_ForTestOnly()
		{
			return ShowAPPaymentProcessingForm(true);
		}

		public void ProviderLogoPictureBox_Click_ForTestOnly(object sender, EventArgs e) => ProviderLogoPictureBox_Click(sender, e);

		public ZGrid MatchTransactionsGrid_ForTestOnly
		{
			get { return MatchTransactionsGrid; }
		}

		public ZGrid QuoteSummaryGrid_ForTestOnly
		{
			get { return quoteSummaryGrid; }
		}

		public void DeleteTransaction_ForTestOnly(object sender, EventArgs e)
		{
			DeleteTransaction(sender, e);
		}

		public void AcceptQuotesButton_ForTestOnly(object sender, EventArgs e) => AcceptQuotesButton_Click(sender, e);

		public Business.Base.Matching.MatchingBase SelectedMatchingBase_ForTestOnly => SelectedMatchingBase;

		public ZButton ProcessEPaymentsButton_ForTestOnly => ProcessEPaymentsButton;

		public ZButton RefreshButton_ForTestOnly => RefreshButton;

		public ZButton CheckExRateButton_ForTestOnly => CheckExRateButton;

		public ZCalcEdit CurrencySummaryTotalEPaymentTextBox_ForTestOnly => CurrencySummaryTotalEPaymentTextBox;

		public ZCalcEdit CurrencySummaryTotalEPaymentFeesTextBox_ForTestOnly => CurrencySummaryTotalEPaymentFeesTextBox;

		public ZTabPage EPaymentTabPage_ForTestOnly => zTabPage2;
	}
}

#endif
