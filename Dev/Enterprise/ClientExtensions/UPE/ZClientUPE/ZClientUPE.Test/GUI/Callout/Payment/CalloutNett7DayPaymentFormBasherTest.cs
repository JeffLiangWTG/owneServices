using Enterprise.Client.UPE.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(CalloutNett7DayPaymentForm))]
	internal class CalloutNett7DayPaymentFormBasherTest : CalloutPaymentDetailsFormTestCase
	{
		protected override UPECargoPaymentMethod PaymentMethod
		{
			get
			{
				return UPECargoPaymentMethod.Nett7Day;
			}
		}
	}
}
