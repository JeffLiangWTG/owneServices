using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyAddress2))]
	sealed class CompanyAddress2Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyAddress2 >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyAddress2 >", Passes.FirstPass));
			Assert("should match < Company Address 2 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Address 2 >", Passes.FirstPass));
			Assert("should match < Company Address2 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Address2 >", Passes.FirstPass));
			Assert("should match < CompanyAddress 2>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyAddress 2>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Address2, ValueProviderToTest.GetReplacement("<CompanyAddress2>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyAddress2();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_Address2 = "Address2";
		}
	}
}
