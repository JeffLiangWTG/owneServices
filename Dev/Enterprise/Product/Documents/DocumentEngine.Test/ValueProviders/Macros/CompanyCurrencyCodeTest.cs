using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCurrencyCode))]
	sealed class CompanyCurrencyCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCurrencyCode >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCurrencyCode >", Passes.FirstPass));
			Assert("should match < Company Currency Code>", ValueProviderToTest.IsResponsibleForReplacing("< Company Currency Code>", Passes.FirstPass));
			Assert("should match < Company Currency Code >", ValueProviderToTest.IsResponsibleForReplacing("< Company Currency Code >", Passes.FirstPass));
			Assert("should match < CompanyCurrencyCode>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCurrencyCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ValueProviderToTest.GetReplacement("<CompanyCurrencyCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCurrencyCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
		}
	}
}
