using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyState))]
	sealed class CompanyStateTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyState >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyState >", Passes.FirstPass));
			Assert("should match < Company State>", ValueProviderToTest.IsResponsibleForReplacing("< Company State>", Passes.FirstPass));
			Assert("should match < Company State >", ValueProviderToTest.IsResponsibleForReplacing("< Company State >", Passes.FirstPass));
			Assert("should match < CompanyState>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyState>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_State, ValueProviderToTest.GetReplacement("<CompanyState>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyState();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_State = "NSW";
		}
	}
}
