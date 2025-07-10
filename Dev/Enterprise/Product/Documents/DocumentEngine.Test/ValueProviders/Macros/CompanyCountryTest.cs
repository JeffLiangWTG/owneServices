using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCountry))]
	sealed class CompanyCountryTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCountry >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCountry >", Passes.FirstPass));
			Assert("should match < Company Country>", ValueProviderToTest.IsResponsibleForReplacing("< Company Country>", Passes.FirstPass));
			Assert("should match < Company Country >", ValueProviderToTest.IsResponsibleForReplacing("< Company Country >", Passes.FirstPass));
			Assert("should match < CompanyCountry>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCountry>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.Country.RN_Desc, ValueProviderToTest.GetReplacement("<CompanyCountry>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCountry();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.Country.RN_Desc = "Australia";
		}
	}
}
