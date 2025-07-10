using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ExportGuidedDecisionMakingBasicValidationConfigurationTest : BusinessObjectValidationTestCase
	{
		public void TestIsPreferenceRequired()
		{
			AssertEquals(false, new ExportGuidedDecisionMakingBasicValidationConfiguration().IsPreferenceRequired);
		}

		public void TestIsQuotaOrderNumberRequired()
		{
			AssertEquals(false, new ExportGuidedDecisionMakingBasicValidationConfiguration().IsQuotaOrderNumberRequired);
		}

		public void TestIsCountryOfOriginRequired()
		{
			AssertEquals(false, new ExportGuidedDecisionMakingBasicValidationConfiguration().IsCountryOfOriginRequired);
		}

		public void TestIsCountryOfDestinationRequired()
		{
			AssertEquals(true, new ExportGuidedDecisionMakingBasicValidationConfiguration().IsCountryOfDestinationRequired);
		}
	}
}
