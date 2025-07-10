using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CountryExtraTaxDescriptionNotRecoverable))]
	sealed class CountryExtraTaxDescriptionNotRecoverableTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < Country Extra Tax Description Not Recoverable>", ValueProviderToTest.IsResponsibleForReplacing("< Country Extra Tax Description Not Recoverable >", Passes.FirstPass));
			Assert("should match < Country ExtraTax Description NotRecoverable>", ValueProviderToTest.IsResponsibleForReplacing("< Country ExtraTax Description NotRecoverable>", Passes.FirstPass));
			Assert("should match < CountryExtraTaxDescriptionNotRecoverable>", ValueProviderToTest.IsResponsibleForReplacing("< CountryExtraTaxDescriptionNotRecoverable>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Input Extra Tax Not Recoverable", ValueProviderToTest.GetReplacement("<CountryExtraTaxDescriptionNotRecoverable>", Report));
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Input VAT Claimed Not Recoverable", ValueProviderToTest.GetReplacement("<CountryExtraTaxDescriptionNotRecoverable>", Report));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CountryExtraTaxDescriptionNotRecoverable();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}
	}
}
