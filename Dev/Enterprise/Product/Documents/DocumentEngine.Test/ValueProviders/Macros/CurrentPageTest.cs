using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrentPage))]
	sealed class CurrentPageTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<Curent Page>", Passes.FirstPass));
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("< current      page       >", Passes.FirstPass));
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<CurrentPage>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("< current      page       >", Passes.SecondPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<CurrentPage>", Passes.SecondPass));

			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<Curent Page>", Passes.SecondPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<CurrentPage()>", Passes.SecondPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<CurrentPage(1)>", Passes.SecondPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<Current  Page(2)>", Passes.SecondPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("< current      page       ()>", Passes.SecondPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("< current      page       (1)>", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals(0, ValueProviderToTest.GetReplacement("<Current PAGE>", Report));
			AssertEquals(0, ValueProviderToTest.GetReplacement("<Current Page()>", Report));
			AssertEquals(0, ValueProviderToTest.GetReplacement("<Current Page(-5)>", Report));
			AssertEquals(0, ValueProviderToTest.GetReplacement("<Current Page(invalidnumber)>", Report));
			AssertEquals(4, ValueProviderToTest.GetReplacement("<Current Page(5)>", Report));
			AssertEquals(554, ValueProviderToTest.GetReplacement("<Current Page(555)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CurrentPage();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var area = new DocumentHeaderArea(1, 2, Report, "");
			area.RenderedToPageNumber = 3;
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = area;
		}
	}
}
