using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(ValidationRuleConfiguration))]
	sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
	{
		public override void TestIsRuleC0065Active()
		{
			AssertEquals(expected: false, configuration.IsRuleC0065Active);
		}

		public override void TestIsRuleC0505Active()
		{
			AssertEquals(expected: false, configuration.IsRuleC0505Active);
		}

		public override void TestIsRuleC0587_1Active()
		{
			AssertEquals(expected: true, configuration.IsRuleC0587_1Active);
		}

		public override void TestIsRuleC0587_2Active()
		{
			AssertEquals(expected: true, configuration.IsRuleC0587_2Active);
		}

		public override void TestIsRuleC0839Active()
		{
			AssertEquals(expected: false, configuration.IsRuleC0839Active);
		}

		public override void TestIsRuleE1102Active()
		{
			AssertEquals(expected: false, configuration.IsRuleE1102Active);
		}

		public override void TestIsRuleNR0002Active()
		{
			AssertEquals(expected: true, configuration.IsRuleNR0002Active);
		}

		public override void TestIsRuleR0350Active()
		{
			AssertEquals(expected: false, configuration.IsRuleR0350Active);
		}

		public override void TestIsRuleR0850Active()
		{
			AssertEquals(expected: false, configuration.IsRuleR0850Active);
		}

		public override void TestIsRuleTR0046Active()
		{
			AssertEquals(expected: false, configuration.IsRuleTR0046Active);
		}

		public override void TestIsRuleTR0055Active()
		{
			AssertEquals(expected: true, configuration.IsRuleTR0055Active);
		}

		public override void TestIsRuleTR0056Active()
		{
			AssertEquals(expected: true, configuration.IsRuleTR0056Active);
		}
	}
}
