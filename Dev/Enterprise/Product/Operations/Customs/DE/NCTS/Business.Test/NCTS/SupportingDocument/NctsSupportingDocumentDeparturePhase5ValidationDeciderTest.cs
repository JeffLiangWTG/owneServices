using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentDeparturePhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleE1301Active() => AssertEquals(false, validationDecider.IsRuleE1301Active);

		public void TestIsRuleG0321Active() => AssertEquals(false, validationDecider.IsRuleG0321Active);

		public void TestIsRuleNR0006Active() => AssertEquals(true, validationDecider.IsRuleNR0006Active);

		public void TestIsRuleRP30Active() => AssertEquals(false, validationDecider.IsRuleRP30Active);

		protected override void SetUp()
		{
			validationDecider = new();
		}
		NctsSupportingDocumentDeparturePhase5ValidationDecider validationDecider;
	}
}
