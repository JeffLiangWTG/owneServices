#if DEBUG

using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentApprovalForm
	{
		public NewMatchGroupForm FMatchingForm_ForTestOnly
		{
			get { return fMatchingForm; }
			set { fMatchingForm = value; }
		}

		public void FMatchingForm_Closed_ForTestOnly(object sender, EventArgs e)
		{
			fMatchingForm_Closed(sender, e);
		}

		public bool SessionBalancesToZero_ForTestOnly => SessionBalancesToZero;

		public ZButton PaymentDetailButton_ForTestOnly
		{
			get { return PaymentDetailButton; }
			set { PaymentDetailButton = value; }
		}

		public ContinueWithSave ShowPreSaveDialogs_ForTestOnly()
		{
			return ShowPreSaveDialogs();
		}

		public ReceiptPayment.PaymentPrintManager PrintManager_ForTestOnly => PrintManager;

		public void Save_ForTestOnly(ITransactionParticipant[] factories1)
		{
			Save(factories1);
		}

		public ZButton PostWithoutMatchingButton_ForTestOnly
		{
			get { return PostWithoutMatchingButton; }
			set { PostWithoutMatchingButton = value; }
		}

		public ZArchitecture.ZTextBox ChequeNoTextBox_ForTestOnly
		{
			get { return ChequeNoTextBox; }
			set { ChequeNoTextBox = value; }
		}

		public PaymentApprovalBase Payment_ForTestOnly => Payment;

		public ZBool IsPostWithoutMatching_ForTestOnly
		{
			get { return IsPostWithoutMatching; }
			set { IsPostWithoutMatching = value; }
		}

		public ZButton SaveAsDraftButton_ForTestOnly => SaveAsDraftButton;
		public ZButton SubmitForApprovalButton_ForTestOnly => SubmitForApprovalButton;

		public ZButton ApproveForPostingButton_ForTestOnly => ApproveForPostingButton;

		public ZButton CheckEPayRateButton_ForTestOnly => CheckEPayRateButton;

		public ZButton LearnMoreButton_ForTestOnly => LearnMoreButton;

		public ZButton RefreshEPaymentButton_ForTestOnly => RefreshEPaymentButton;

		public ZButton AcceptQuoteButton_ForTestOnly => AcceptQuoteButton;

		public ZButton ProcessEPaymentButton_ForTestOnly => ProcessEPaymentButton;

		public ZTabPage EPaymentTabPage_ForTestOnly => EPaymentTabPage;

		public void ProviderLogoPictureBox_Click_ForTestOnly(object sender, EventArgs e) => ProviderLogoPictureBox_Click(sender, e);

		public void SetSuspendReiterantEventsDueToDangerousApplicationDoEvents_ForTestOnly(FunctionalitySuspender suspender)
		{
			suspendReiterantEventsDueToDangerousApplicationDoEvents = suspender;
		}
	}
}

#endif
