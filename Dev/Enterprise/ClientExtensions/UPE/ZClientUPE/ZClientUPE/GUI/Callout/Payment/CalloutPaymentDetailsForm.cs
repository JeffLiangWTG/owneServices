using System;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutPaymentDetailsForm : ZChildForm
	{
		public static CalloutPaymentDetailsForm New(Callout callout, UPECargoPaymentMethod paymentMethod)
		{
			CalloutPaymentDetailsForm result;

			switch (paymentMethod)
			{
				case UPECargoPaymentMethod.Account:
					result = new CalloutAccountPaymentForm(new CalloutAccountPayment(callout));
					break;

				case UPECargoPaymentMethod.Cheque:
					result = new CalloutOtherPaymentForm(new CalloutChequePayment(callout));
					break;

				case UPECargoPaymentMethod.CreditCard:
					result = new CalloutCreditCardPaymentForm(new CalloutCreditCardPayment(callout));
					break;

				case UPECargoPaymentMethod.EFT:
					result = new CalloutEFTPaymentForm(new CalloutEFTPayment(callout));
					break;

				case UPECargoPaymentMethod.BPay:
					result = new CalloutBPayPaymentForm(new CalloutBPayPayment(callout));
					break;

				case UPECargoPaymentMethod.Nett7Day:
					result = new CalloutNett7DayPaymentForm(new CalloutNett7DayPayment(callout));
					break;

				case UPECargoPaymentMethod.Other:
					result = new CalloutOtherPaymentForm(new CalloutOtherPayment(callout));
					break;

				case UPECargoPaymentMethod.PurchaseOrder:
					result = new CalloutPOPaymentForm(new CalloutPOPayment(callout));
					break;

				default:
					result = null;
					break;
			}

			return result;
		}

		protected CalloutPaymentDetailsForm(CalloutPaymentDetails paymentDetails)
			: base(paymentDetails)
		{
		}

		[Obsolete("Design mode only", true)]
		public CalloutPaymentDetailsForm()
		{
		}

		public override string FormHeading
		{
			get
			{
				return "Enter Payment Details";
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			((CalloutPaymentDetails)BusinessEntity).CreateCalloutPaymentNote();
			SaveDetails();
		}

		protected virtual void SaveDetails()
		{
		}
	}
}
