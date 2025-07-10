using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CapitaliseFirstLetterOfWords))]
	sealed class CapitaliseFirstLetterOfWordsTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CapitaliseFirstLetterOfWords(tbl.col)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<CapitaliseFirstLetterOfWords(tbl . col)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< CapitaliseFirstLetterOfWords(tbl . col)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< CapitaliseFirstLetterOfWords(tbl . col) >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< CapitaliseFirstLetterOfWords(tbl . col ) >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< CapitaliseFirstLetterOfWords( tbl . col ) >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< CapitaliseFirstLetterOfWords( tbl . col ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CapitaliseFirstLetterOfWords(tblcol)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<CapitaliseFirstLetterOfWords()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = new ConfigArea(1, 2, Report, "");
			AssertEquals("Some  Words I N Caps, Some \r\nWords In Caps\r\n With Break Line!", ValueProviderToTest.GetReplacement("<CapitaliseFirstLetterOfWords(Header.SomeWordsInCaps)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CapitaliseFirstLetterOfWords();
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		public void TestGetReplacementCore_WhenCurrentAreaToProcessIsNull()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertNoExceptionThrown(() => ValueProviderToTest.GetReplacement("<CapitaliseFirstLetterOfWords(Header.SomeWordsInCaps)>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = new ConfigArea(1, 2, Report, "");
		}
	}
}
