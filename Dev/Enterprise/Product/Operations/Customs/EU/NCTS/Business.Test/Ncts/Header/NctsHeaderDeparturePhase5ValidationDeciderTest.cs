using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderDeparturePhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleB1823Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleB1823Active);
		}

		public void TestIsRuleC0001Active()
		{
			AssertEquals(expected: true, validationDecider.IsRuleC0001Active);
		}

		public void TestIsRuleC0001_1()
		{
			AssertEquals(false, validationDecider.IsRuleC0001_1Active);
		}

		public void TestIsRuleC0001_4Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleC0001_4Active);
		}

		public void TestIsRuleC0001_6Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleC0001_6Active);
		}

		public void TestIsRuleG0001_1Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleG0001_1Active);
		}

		public void TestIsRuleNR0068Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleNR0068Active);
		}

		public void TestIsRuleNR0069Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleNR0069Active);
		}

		public void TestIsRuleNR0071Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleNR0071Active);
		}

		public void TestIsRuleNR0074Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleNR0074Active);
		}

		public void TestIsRuleTR0079Active()
		{
			AssertEquals(expected: true, validationDecider.IsRuleTR0079Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsHeaderDeparturePhase5ValidationDecider();
		}

		INctsHeaderDeparturePhase5ValidationDecider validationDecider;
	}
}
