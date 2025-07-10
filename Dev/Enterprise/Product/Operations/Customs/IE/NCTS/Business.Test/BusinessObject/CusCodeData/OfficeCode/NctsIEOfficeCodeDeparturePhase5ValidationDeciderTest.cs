using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NctsIEOfficeCodeDeparturePhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleG0034Active()
		{
			AssertEquals(true, validationDecider.IsRuleG0034Active);
		}

		public void TestIsRuleB1831Active()
		{
			AssertEquals(true, validationDecider.IsRuleB1831Active);
		}

		public void TestIsRuleB1836Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1836Active);
		}

		public void TestIsRuleB1904Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1904Active);
		}

		public void TestIsRuleC0030Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0030Active);
		}

		public void TestIsRuleC0030_1Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0030_1Active);
		}

		public void TestIsRuleC0598Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0598Active);
		}

		public void TestIsRuleR0005Active()
		{
			AssertEquals(false, validationDecider.IsRuleR0005Active);
		}

		public void TestIsRuleR0006Active()
		{
			AssertEquals(true, validationDecider.IsRuleR0006Active);
		}

		public void TestIsRuleR0103Active()
		{
			AssertEquals(true, validationDecider.IsRuleR0103Active);
		}

		public void TestIsRuleR0900Active()
		{
			AssertEquals(true, validationDecider.IsRuleR0900Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsIEOfficeCodeDeparturePhase5ValidationDecider();
		}

		NctsIEOfficeCodeDeparturePhase5ValidationDecider validationDecider;
	}
}
