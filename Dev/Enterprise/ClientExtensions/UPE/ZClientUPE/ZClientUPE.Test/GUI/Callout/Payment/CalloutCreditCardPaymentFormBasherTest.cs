using Enterprise.Client.UPE.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutCreditCardPaymentForm))]
	internal class CalloutCreditCardPaymentFormBasherTest : CalloutPaymentDetailsFormTestCase
	{
		protected override UPECargoPaymentMethod PaymentMethod
		{
			get
			{
				return UPECargoPaymentMethod.CreditCard;
			}
		}
	}
}
