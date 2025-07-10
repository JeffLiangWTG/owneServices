using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyPhone))]
	sealed class CompanyPhoneTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <copmany fone>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany fone>", Passes.FirstPass));
			Assert("should match < company    phone       >", ValueProviderToTest.IsResponsibleForReplacing("< company    phone       >", Passes.FirstPass));
			Assert("should match <CompanyPhone>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyPhone>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Phone, ValueProviderToTest.GetReplacement("<Company Phone>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyPhone();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_Phone = "+61 2 9025 1100";
		}
	}
}
