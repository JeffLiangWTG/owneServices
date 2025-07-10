using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ImportGuidedDecisionMakingBasicValidationConfigurationTest : BusinessObjectValidationTestCase
	{
		public void TestIsPreferenceRequired()
		{
			AssertEquals(true, new ImportGuidedDecisionMakingBasicValidationConfiguration().IsPreferenceRequired);
		}

		public void TestIsQuotaOrderNumberRequired()
		{
			AssertEquals(true, new ImportGuidedDecisionMakingBasicValidationConfiguration().IsQuotaOrderNumberRequired);
		}

		public void TestIsCountryOfOriginRequired()
		{
			AssertEquals(true, new ImportGuidedDecisionMakingBasicValidationConfiguration().IsCountryOfOriginRequired);
		}

		public void TestIsCountryOfDestinationRequired()
		{
			AssertEquals(false, new ImportGuidedDecisionMakingBasicValidationConfiguration().IsCountryOfDestinationRequired);
		}
	}
}
