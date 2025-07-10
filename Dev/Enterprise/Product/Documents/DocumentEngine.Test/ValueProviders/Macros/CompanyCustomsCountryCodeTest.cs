using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCustomsCountryCode))]
	sealed class CompanyCustomsCountryCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCustomsCountryCode >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCustomsCountryCode >", Passes.FirstPass));
			Assert("should match < Company Customs Country Code>", ValueProviderToTest.IsResponsibleForReplacing("< Company Customs Country Code>", Passes.FirstPass));
			Assert("should match < Company Customs Country Code >", ValueProviderToTest.IsResponsibleForReplacing("< Company Customs Country Code >", Passes.FirstPass));
			Assert("should match < CompanyCustomsCountryCode>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCustomsCountryCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), ValueProviderToTest.GetReplacement("<CompanyCustomsCountryCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCustomsCountryCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
		}
	}
}
