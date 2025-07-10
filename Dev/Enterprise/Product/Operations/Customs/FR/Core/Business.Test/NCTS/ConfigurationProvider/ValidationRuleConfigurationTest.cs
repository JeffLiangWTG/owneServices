using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(ValidationRuleConfiguration))]
	public class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
	{
		public override void TestIsCountryCodeRequiredToBeSameAsCurrentCompany()
		{
			AssertEquals(nameof(configuration.IsCountryCodeRequiredToBeSameAsCurrentCompany), true, configuration.IsCountryCodeRequiredToBeSameAsCurrentCompany);
		}

		public override void TestIsRuleC0839Active()
		{
			AssertEquals(nameof(configuration.IsRuleC0839Active), false, configuration.IsRuleC0839Active);
		}

		public override void TestIsRuleE1406Active()
		{
			AssertEquals(nameof(configuration.IsRuleE1406Active), false, configuration.IsRuleE1406Active);
		}

		public override void TestIsRuleC0065Active()
		{
			AssertEquals(nameof(configuration.IsRuleC0065Active), false, configuration.IsRuleC0065Active);
		}
	}
}
