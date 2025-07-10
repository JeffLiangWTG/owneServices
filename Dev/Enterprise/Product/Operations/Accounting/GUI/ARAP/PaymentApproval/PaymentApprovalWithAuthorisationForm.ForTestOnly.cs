#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentApprovalWithAuthorisationForm
	{
		public Business.ARAP.PaymentApproval.PaymentApprovalWithAuthorisation Payment_ForTestOnly => Payment;

		public ZToolStripButton SaveAsDraftButton_ForTestOnly => SaveAsDraftButton;

		public ZPostingButtonsUserControl PostingButtonsUserControl_ForTestOnly => PostingButtonsUserControl;

		public ZButton LearnMoreButton_ForTestOnly => LearnMoreButton;

		public ContinueWithSave ShowPreSaveDialogs_ForTestOnly()
		{
			return ShowPreSaveDialogs();
		}

		public ZButton CheckExRateButton_ForTestOnly
		{
			get { return CheckExRateButton; }
			set { CheckExRateButton = value; }
		}

		public ZButton PaymentDetailButton_ForTestOnly
		{
			get { return PaymentDetailButton; }
			set { PaymentDetailButton = value; }
		}

		public ZButton FirstAuthorisationButton_ForTestOnly
		{
			get { return FirstAuthorisationButton; }
			set { FirstAuthorisationButton = value; }
		}

		public ZButton SecondAuthorisationButton_ForTestOnly
		{
			get { return SecondAuthorisationButton; }
			set { SecondAuthorisationButton = value; }
		}

		public ZButton ThirdAuthorisationButton_ForTestOnly
		{
			get { return ThirdAuthorisationButton; }
			set { ThirdAuthorisationButton = value; }
		}

		public void PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(object sender, string message)
		{
			PaymentApprovalBizO_RequiredAuthorisationChanged(sender, message);
		}

		public void FirstAuthorisationButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			FirstAuthorisationButton_Click(sender, e);
		}

		public void SecondAuthorisationButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			SecondAuthorisationButton_Click(sender, e);
		}

		public void ThirdAuthorisationButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			ThirdAuthorisationButton_Click(sender, e);
		}

		public void CheckExRateButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			CheckExRateButton_Click(sender, e);
		}

		public void RefreshButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			RefreshButton_Click(sender, e);
		}

		public void RejectButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			RejectButton_Click(sender, e);
		}

		public void ProviderLogoPictureBox_Click_ForTestOnly(object sender, EventArgs e) => ProviderLogoPictureBox_Click(sender, e);

		public ZButton RejectButton_ForTestOnly
		{
			get { return RejectButton; }
			set { RejectButton = value; }
		}

		public ZButton AcceptQuoteButton_ForTestOnly => AcceptQuoteButton;

		public ZButton ProcessEPaymentButton_ForTestOnly => ProcessEPaymentButton;
		public ZButton RefreshButton_ForTestOnly => refreshButton;

		public ZTabPage EPaymentTabPage_ForTestOnly => EPaymentTab;

		public ZDropEdit RejectReasonCodeDropEdit_ForTestOnly
		{
			get { return RejectReasonCodeDropEdit; }
			set { RejectReasonCodeDropEdit = value; }
		}

		public ZTextBox RejectReasonTextBox_ForTestOnly
		{
			get { return RejectReasonTextBox; }
			set { RejectReasonTextBox = value; }
		}

		public ReceiptPayment.PaymentPrintManager PrintManager_ForTestOnly => PrintManager;

		public ZTextBox ChequeNoTextBox_ForTestOnly
		{
			get { return ChequeNoTextBox; }
			set { ChequeNoTextBox = value; }
		}

		public void UpdateAuthorizationButtonReadonly_ForTestOnly()
		{
			UpdateAuthorizationButtonReadonly();
		}

		public MenuItem ActionsMenuItem_ForTestOnly => ActionsMenuItem;

		public MenuItem CancelEPaymentMenuItem_ForTestOnly => CancelEPaymentMenuItem;
	}
}

#endif
