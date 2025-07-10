using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyIsReciprocal))]
	sealed class CompanyIsReciprocalTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyIsReciprocal >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyIsReciprocal >", Passes.FirstPass));
			Assert("should match < Company Is Reciprocal>", ValueProviderToTest.IsResponsibleForReplacing("< Company Is Reciprocal>", Passes.FirstPass));
			Assert("should match < Company Is Reciprocal >", ValueProviderToTest.IsResponsibleForReplacing("< Company Is Reciprocal >", Passes.FirstPass));
			Assert("should match < CompanyIsReciprocal>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyIsReciprocal>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_IsReciprocal, ValueProviderToTest.GetReplacement("<CompanyIsReciprocal>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyIsReciprocal();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
		}
	}
}
