using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CountryExtraTaxDescription))]
	sealed class CountryExtraTaxDescriptionTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < Country Extra Tax Description >", ValueProviderToTest.IsResponsibleForReplacing("< Country Extra Tax Description >", Passes.FirstPass));
			Assert("should match < Country ExtraTax Description>", ValueProviderToTest.IsResponsibleForReplacing("< Country ExtraTax Description>", Passes.FirstPass));
			Assert("should match < CountryExtraTaxDescription>", ValueProviderToTest.IsResponsibleForReplacing("< CountryExtraTaxDescription>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Extra Tax", ValueProviderToTest.GetReplacement("<CountryExtraTaxDescription>", Report));
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Input VAT Claimed", ValueProviderToTest.GetReplacement("<CountryExtraTaxDescription>", Report));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CountryExtraTaxDescription();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}
	}
}
