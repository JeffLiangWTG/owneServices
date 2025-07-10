using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCountryCode))]
	sealed class CompanyCountryCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCountryCode >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCountryCode >", Passes.FirstPass));
			Assert("should match < Company Country Code>", ValueProviderToTest.IsResponsibleForReplacing("< Company Country Code>", Passes.FirstPass));
			Assert("should match < Company Country Code >", ValueProviderToTest.IsResponsibleForReplacing("< Company Country Code >", Passes.FirstPass));
			Assert("should match < CompanyCountryCode>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCountryCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ValueProviderToTest.GetReplacement("<CompanyCountryCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCountryCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
		}
	}
}
