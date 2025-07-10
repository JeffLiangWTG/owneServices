using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutOtherPaymentForm : CalloutPaymentDetailsForm
	{
		public CalloutOtherPaymentForm(CalloutChequePayment paymentDetails)
			: base(paymentDetails)
		{
		}

		public CalloutOtherPaymentForm(CalloutOtherPayment paymentDetails)
			: base(paymentDetails)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
