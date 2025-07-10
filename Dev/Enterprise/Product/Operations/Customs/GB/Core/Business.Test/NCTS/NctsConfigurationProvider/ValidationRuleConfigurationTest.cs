using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(ValidationRuleConfiguration))]
	sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
	{
		public override void TestIsCountryCodeRequiredToBeSameAsCurrentCompany()
		{
			AssertEquals(nameof(configuration.IsCountryCodeRequiredToBeSameAsCurrentCompany), true, configuration.IsCountryCodeRequiredToBeSameAsCurrentCompany);
		}
	}
}
