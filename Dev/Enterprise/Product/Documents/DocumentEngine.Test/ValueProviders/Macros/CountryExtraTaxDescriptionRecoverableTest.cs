using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CountryExtraTaxDescriptionRecoverable))]
	sealed class CountryExtraTaxDescriptionRecoverableTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < Country Extra Tax Description Recoverable>", ValueProviderToTest.IsResponsibleForReplacing("< Country Extra Tax Description Recoverable >", Passes.FirstPass));
			Assert("should match < CountryExtraTaxDescriptionRecoverable >", ValueProviderToTest.IsResponsibleForReplacing("< CountryExtraTaxDescriptionRecoverable >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Extra Tax Payable / Recoverable", ValueProviderToTest.GetReplacement("<CountryExtraTaxDescriptionRecoverable>", Report));
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Input VAT Claimed Payable / Recoverable", ValueProviderToTest.GetReplacement("<CountryExtraTaxDescriptionRecoverable>", Report));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CountryExtraTaxDescriptionRecoverable();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}
	}
}
