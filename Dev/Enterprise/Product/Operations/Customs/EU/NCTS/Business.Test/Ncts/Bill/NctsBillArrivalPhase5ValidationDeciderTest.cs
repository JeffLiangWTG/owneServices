using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillArrivalPhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleB1964Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1964Active);
		}

		public void TestIsRuleNR0062Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleNR0062Active);
		}

		public void TestIsRuleC0909Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0909Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsBillArrivalPhase5ValidationDecider();
		}

		NctsBillArrivalPhase5ValidationDecider validationDecider;
	}
}
