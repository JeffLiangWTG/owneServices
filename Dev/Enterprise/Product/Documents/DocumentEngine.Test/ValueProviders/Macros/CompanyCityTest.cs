using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCity))]
	sealed class CompanyCityTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCity >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCity >", Passes.FirstPass));
			Assert("should match < Company City>", ValueProviderToTest.IsResponsibleForReplacing("< Company City>", Passes.FirstPass));
			Assert("should match < Company City >", ValueProviderToTest.IsResponsibleForReplacing("< Company City >", Passes.FirstPass));
			Assert("should match < CompanyCity>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCity>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_City, ValueProviderToTest.GetReplacement("<CompanyCity>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCity();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_City = "Sydney";
		}
	}
}
