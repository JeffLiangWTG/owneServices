using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCustomsCurrencyCode))]
	sealed class CompanyCustomsCurrencyCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCustomsCurrencyCode >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCustomsCurrencyCode >", Passes.FirstPass));
			Assert("should match < Company Customs Currency Code>", ValueProviderToTest.IsResponsibleForReplacing("< Company Customs Currency Code>", Passes.FirstPass));
			Assert("should match < Company Customs Currency Code >", ValueProviderToTest.IsResponsibleForReplacing("< Company Customs Currency Code >", Passes.FirstPass));
			Assert("should match < CompanyCustomsCurrencyCode>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCustomsCurrencyCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, ValueProviderToTest.GetReplacement("<CompanyCustomsCurrencyCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCustomsCurrencyCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency = "AUD";
		}
	}
}
