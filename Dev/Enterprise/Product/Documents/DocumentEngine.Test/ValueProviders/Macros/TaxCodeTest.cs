using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TaxCode))]
	sealed class TaxCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < TaxCode >", ValueProviderToTest.IsResponsibleForReplacing("< TaxCode >", Passes.FirstPass));
			Assert("should match < Tax Code>", ValueProviderToTest.IsResponsibleForReplacing("< Tax Code>", Passes.FirstPass));
			Assert("should match < Tax Code >", ValueProviderToTest.IsResponsibleForReplacing("< Tax Code >", Passes.FirstPass));
			Assert("should match < TaxCode>", ValueProviderToTest.IsResponsibleForReplacing("< TaxCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription, ValueProviderToTest.GetReplacement("<TaxCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TaxCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}
	}
}
