using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyCode))]
	sealed class CompanyCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyCode >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCode >", Passes.FirstPass));
			Assert("should match < Company Code>", ValueProviderToTest.IsResponsibleForReplacing("< Company Code>", Passes.FirstPass));
			Assert("should match < Company Code >", ValueProviderToTest.IsResponsibleForReplacing("< Company Code >", Passes.FirstPass));
			Assert("should match < CompanyCode>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, ValueProviderToTest.GetReplacement("<CompanyCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_Code = "WTG";
		}
	}
}
