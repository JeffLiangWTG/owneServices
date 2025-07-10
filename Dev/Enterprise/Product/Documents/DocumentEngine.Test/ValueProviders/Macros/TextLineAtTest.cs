using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TextLineAt))]
	sealed class TextLineAtTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<TextLineAt(tbl.col,1)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<TextLineAt(tbl.col, 2)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< TextLineAt(tbl.col, 2)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< TextLineAt( tbl.col, 2 ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< TextLineAt( tbl.col , 12 ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<TextLineAt(field,1)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<TextLineAt(field, 1)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<TextLineAt(1)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<TextLineAt(,1)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = new ConfigArea(1, 2, Report, "");
			AssertEquals(" Line4                                                                             ", ValueProviderToTest.GetReplacement("<TextLineAt(Header.MultiLineText, 4)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<TextLineAt(Header.MultiLineText, 5)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TextLineAt();
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = new ConfigArea(1, 2, Report, "");
		}
	}
}
