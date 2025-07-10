using Enterprise.Client.UPE.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutEFTPaymentForm))]
	internal class CalloutEFTPaymentFormBasherTest : CalloutPaymentDetailsFormTestCase
	{
		protected override UPECargoPaymentMethod PaymentMethod
		{
			get
			{
				return UPECargoPaymentMethod.EFT;
			}
		}
	}
}
