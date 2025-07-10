using Enterprise.Client.UPE.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutPOPaymentForm))]
	internal class CalloutPOPaymentFormBasherTest : CalloutPaymentDetailsFormTestCase
	{
		protected override UPECargoPaymentMethod PaymentMethod
		{
			get
			{
				return UPECargoPaymentMethod.PurchaseOrder;
			}
		}
	}
}
