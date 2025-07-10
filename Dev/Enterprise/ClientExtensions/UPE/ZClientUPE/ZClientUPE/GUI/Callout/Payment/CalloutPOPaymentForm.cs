using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutPOPaymentForm : CalloutPaymentDetailsForm
	{
		public CalloutPOPaymentForm(CalloutPOPayment paymentDetails)
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
