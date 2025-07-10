using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutCreditCardPaymentForm : CalloutPaymentDetailsForm
	{
		public CalloutCreditCardPaymentForm(CalloutCreditCardPayment paymentDetails)
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
