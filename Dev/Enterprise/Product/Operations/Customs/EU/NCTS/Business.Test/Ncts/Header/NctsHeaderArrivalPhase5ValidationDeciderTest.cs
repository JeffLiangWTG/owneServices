using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderArrivalPhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleNR0015Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0015Active);

		public void TestIsRuleTR0035Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0035Active);

		public void TestIsRuleTR0047Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0047Active);

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsHeaderArrivalPhase5ValidationDecider();
		}

		INctsHeaderArrivalPhase5ValidationDecider validationDecider;
	}
}
