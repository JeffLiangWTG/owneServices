using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(ValidationRuleConfiguration))]
	sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
	{
		public override void TestIsRuleB1822Active()
		{
			AssertEquals(true, configuration.IsRuleB1822Active);
		}

		public override void TestIsRuleB1896Active()
		{
			AssertEquals(true, configuration.IsRuleB1896Active);
		}

		public override void TestIsRuleC0030Active()
		{
			AssertEquals(true, configuration.IsRuleC0030Active);
		}

		public override void TestIsRuleC0587_1Active()
		{
			AssertEquals(true, configuration.IsRuleC0587_1Active);
		}

		public override void TestIsRuleR0520Active()
		{
			AssertEquals(true, configuration.IsRuleR0520Active);
		}
	}
}
