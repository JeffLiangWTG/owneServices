using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyPostCode))]
	sealed class CompanyPostCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <copmany Post code>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany Post code>", Passes.FirstPass));
			Assert("should match < Company      Post         Code  >", ValueProviderToTest.IsResponsibleForReplacing("< Company      Post       Code  >", Passes.FirstPass));
			Assert("should match <CompanyPostCode>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyPostCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_PostCode, ValueProviderToTest.GetReplacement("<CompanyPostCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyPostCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_PostCode = "2015";
		}
	}
}
