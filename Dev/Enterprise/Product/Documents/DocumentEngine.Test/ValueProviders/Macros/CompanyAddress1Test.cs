using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyAddress1))]
	sealed class CompanyAddress1Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyAddress1 >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyAddress1 >", Passes.FirstPass));
			Assert("should match < Company Address 1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Address 1 >", Passes.FirstPass));
			Assert("should match < Company Address1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Address1 >", Passes.FirstPass));
			Assert("should match < CompanyAddress 1>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyAddress 1>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Address1, ValueProviderToTest.GetReplacement("<CompanyAddress1>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyAddress1();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_Address1 = "Address1";
		}
	}
}
