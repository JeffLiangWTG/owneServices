using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MethodOfPaymentHelperTest : TestCaseWithFactory
	{
		public void TestRequireDeferralPaymentParty()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not DeferredMethodsOfPayment", false, MethodOfPaymentHelper.RequireDeferralPaymentParty(MethodOfPaymentTypes.A));
				AssertEquals("DeferredMethodsOfPayment", true, MethodOfPaymentHelper.RequireDeferralPaymentParty(MethodOfPaymentTypes.E));
			});
		}

		public void TestDeferredMethodsOfPayment()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] { MethodOfPaymentTypes.E, MethodOfPaymentTypes.F, MethodOfPaymentTypes.G, MethodOfPaymentTypes.Z }, MethodOfPaymentHelper.DeferredMethodsOfPayment);
		}
	}
}
