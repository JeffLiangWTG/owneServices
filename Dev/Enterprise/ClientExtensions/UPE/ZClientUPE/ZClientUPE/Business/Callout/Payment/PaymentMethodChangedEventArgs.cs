using System.ComponentModel;

namespace Enterprise.Client.UPE.Business
{
	public delegate void PaymentMethodChangedEventHandler(object sender, PaymentMethodChangedEventArgs e);

	public class PaymentMethodChangedEventArgs : CancelEventArgs
	{
		public PaymentMethodChangedEventArgs(UPECargoPaymentMethod paymentMethod)
		{
			this.PaymentMethod = paymentMethod;
		}

		public readonly UPECargoPaymentMethod PaymentMethod;
	}
}
