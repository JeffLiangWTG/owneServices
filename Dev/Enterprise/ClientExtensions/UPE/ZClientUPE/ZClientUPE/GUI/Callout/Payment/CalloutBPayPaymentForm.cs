using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutBPayPaymentForm : CalloutPaymentDetailsForm
	{
		public CalloutBPayPaymentForm(CalloutBPayPayment paymentDetails)
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
