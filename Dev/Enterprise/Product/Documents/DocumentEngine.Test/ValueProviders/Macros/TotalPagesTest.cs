using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TotalPages))]
	sealed class TotalPagesTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("should not match <TotalPages>", !ValueProviderToTest.IsResponsibleForReplacing("<TotalPages>", Passes.FirstPass));
			Assert("should match <TotalPages>", ValueProviderToTest.IsResponsibleForReplacing("<TotalPages>", Passes.SecondPass));
			Assert("should not match <PageCount>", !ValueProviderToTest.IsResponsibleForReplacing("<PageCount>", Passes.SecondPass));
			Assert("should not match <   Total  \t       Pages   >", !ValueProviderToTest.IsResponsibleForReplacing("<   Total  \t       Pages   >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals(0, ValueProviderToTest.GetReplacement("<     TotalPages      >", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TotalPages();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.Pages.AddNew();
			Report.Renderer.Pages.AddNew();
		}
	}
}
