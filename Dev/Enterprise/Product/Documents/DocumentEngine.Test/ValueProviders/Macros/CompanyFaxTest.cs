using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyFax))]
	sealed class CompanyFaxTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <copmany FAX>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany FAX>", Passes.FirstPass));
			Assert("should match < company    fax       >", ValueProviderToTest.IsResponsibleForReplacing("< company    fax       >", Passes.FirstPass));
			Assert("should match <CompanyFax>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyFax>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Fax, ValueProviderToTest.GetReplacement("<Company Fax>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyFax();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_Fax = "+61 2 9025 1199";
		}
	}
}
