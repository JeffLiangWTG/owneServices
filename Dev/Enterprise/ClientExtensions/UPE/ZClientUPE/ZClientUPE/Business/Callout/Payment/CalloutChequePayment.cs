namespace Enterprise.Client.UPE.Business
{
	public class CalloutChequePayment : CalloutOtherPayment
	{
		public CalloutChequePayment(Callout callout)
			: base(callout)
		{
		}

		protected override string PaymentMethodAsText
		{
			get { return "Cheque"; }
		}
	}
}
