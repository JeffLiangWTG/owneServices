using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsArrivalCargoDescPhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleNR0004Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0004Active);
		}

		public void TestIsRuleNR0029Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0029Active);
		}

		public void TestIsRuleNR0055Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0055Active);
		}

		public void TestIsRuleNR0058Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0058Active);
		}

		public void TestIsRuleNR0059Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0059Active);
		}

		public void TestIsRuleNR0060Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0060Active);
		}

		public void TestIsRuleE1109_1Active() => AssertEquals(false, validationDecider.IsRuleE1109_1Active);

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsArrivalCargoDescPhase5ValidationDecider();
		}

		INctsArrivalCargoDescPhase5ValidationDecider validationDecider;
	}
}
