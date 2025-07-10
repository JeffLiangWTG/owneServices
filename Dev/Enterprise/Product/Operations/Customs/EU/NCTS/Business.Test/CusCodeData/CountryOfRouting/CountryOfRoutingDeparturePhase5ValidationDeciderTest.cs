using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CountryOfRoutingDeparturePhase5ValidationDeciderTest : TestCase
	{
		public void TestIsIsRuleB1836Active()
		{
			AssertEquals(true, validationDecider.IsRuleB1836Active);
		}

		public void TestIsRuleC0030Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0030Active);
		}

		public void TestIsRuleC0586Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0586Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new CountryOfRoutingDeparturePhase5ValidationDecider();
		}

		CountryOfRoutingDeparturePhase5ValidationDecider validationDecider;
	}
}
