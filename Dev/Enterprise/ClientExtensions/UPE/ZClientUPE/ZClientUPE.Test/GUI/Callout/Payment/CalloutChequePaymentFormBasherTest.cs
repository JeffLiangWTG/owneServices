using Enterprise.Client.UPE.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutOtherPaymentForm))]
	internal class CalloutChequePaymentFormBasherTest : CalloutPaymentDetailsFormTestCase
	{
		protected override UPECargoPaymentMethod PaymentMethod
		{
			get
			{
				return UPECargoPaymentMethod.Cheque;
			}
		}
	}
}
