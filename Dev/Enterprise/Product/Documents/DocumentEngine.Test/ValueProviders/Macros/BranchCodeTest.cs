using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BranchCode))]
	sealed class BranchCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < BranchCode >", ValueProviderToTest.IsResponsibleForReplacing("< BranchCode >", Passes.FirstPass));
			Assert("should match < Branch Code>", ValueProviderToTest.IsResponsibleForReplacing("< Branch Code>", Passes.FirstPass));
			Assert("should match < Branch Code >", ValueProviderToTest.IsResponsibleForReplacing("< Branch Code >", Passes.FirstPass));
			Assert("should match < BranchCode>", ValueProviderToTest.IsResponsibleForReplacing("< BranchCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, ValueProviderToTest.GetReplacement("<BranchCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new BranchCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbBranch.CurrentBranch.GB_Code = "WTG";
		}
	}
}
