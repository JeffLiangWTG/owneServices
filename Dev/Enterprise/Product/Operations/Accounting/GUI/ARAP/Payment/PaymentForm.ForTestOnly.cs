#if DEBUG

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentForm
	{
		public Core.Forms.ZPostOrCancelButton PaymentDetailButton_ForTestOnly
		{
			get { return PaymentDetailButton; }
			set { PaymentDetailButton = value; }
		}

		public ContinueWithSave ShowPreSaveDialogs_ForTestOnly()
		{
			return ShowPreSaveDialogs();
		}

		public Core.Forms.ZPostOrCancelButton CloseButton_ForTestOnly
		{
			get { return CloseButton; }
			set { CloseButton = value; }
		}

		public ReceiptPayment.PaymentPrintManager PrintManager_ForTestOnly => PrintManager;

		public ZBool CalledFromMatchingFormClosing_ForTestOnly => CalledFromMatchingFormClosing;

		public void SetCalledFromMatchingFormClosingTrue_ForTestOnly()
		{
			SetCalledFromMatchingFormClosingTrue();
		}

		public ZGuidFindBox ChequeBookGuidFindBox_ForTestOnly
		{
			get { return ChequeBookGuidFindBox; }
			set { ChequeBookGuidFindBox = value; }
		}

		public ZButton CheckEPayRateButton_ForTestOnly => CheckEPayRateButton;

		public ZButton AcceptQuoteButton_ForTestOnly => AcceptQuoteButton;

		public ZButton ProcessEPaymentButton_ForTestOnly => ProcessEPaymentButton;
		public ZButton RefreshButton_ForTestOnly => RefreshButton;

		public ZTabPage EPaymentTabPage_ForTestOnly => EPaymentTabPage;

		public Business.ARAP.ReceiptPayment.Payment Payment_ForTestOnly => Payment;

		public void RefreshButton_ForTestonly(object sender, EventArgs e) => RefreshButton_Click(sender, e);
	}
}

#endif
