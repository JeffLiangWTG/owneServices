using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutAccountPaymentForm : CalloutPaymentDetailsForm
	{
		public CalloutAccountPaymentForm(CalloutAccountPayment paymentDetails)
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
