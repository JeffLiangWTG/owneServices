using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Delimit))]
	sealed class DelimitTest : ValueProviderTest
	{
		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals("ValueProviderToTest.ComponentType", VisualiserComponentTypes.TextEdit, ValueProviderToTest.ComponentType);
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<Delimit(\"CORNHOLIO\",\"butthead\",\"beavis\")>");
			AssertIsResponsibleForReplacing("< Delimit ( \"beavis\" , \"butthead\" , \"beavis\" ) >");
			AssertIsResponsibleForReplacing("< Delimit ( \"beavis\", \"CORNHOLIO\", \"beavis\") >");
			AssertIsResponsibleForReplacing("<Delimit(\"<Have>\", \" a \", \"<Cow>\") >");

			AssertNotResponsibleForReplacing("< Delimit ( \"beavis\", \"butthead\")>");
			AssertNotResponsibleForReplacing("< Delimit ( condition, butthead, CORNHOLIO)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("Eat My Shorts", "<Delimit(\"Eat\", \" My \", \"Shorts\")>");
			AssertIsReplacedWith("Eat", "<Delimit(\"Eat\", \" My \", \"\")>");
			AssertIsReplacedWith("Shorts", "<Delimit(\"\", \" My \", \"Shorts\")>");
			AssertIsReplacedWith("", "<Delimit(\"\", \" My \", \"\")>");
			AssertIsReplacedWith("Have a Cow", "<Delimit(\"Have\", \" a \", \"Cow\")>");

			AssertEquals("Have\r a Cow", ValueProviderToTest.GetReplacement("<Delimit(\"Have\r\", \" a \", \"Cow\") >", Report));
			AssertEquals("Have\n a Cow", ValueProviderToTest.GetReplacement("<Delimit(\"Have\n\", \" a \", \"Cow\") >", Report));
			AssertEquals("Have\r\n a Cow", ValueProviderToTest.GetReplacement("<Delimit(\"Have\r\n\", \" a \", \"Cow\") >", Report));
			AssertEquals("Have\n\r a Cow", ValueProviderToTest.GetReplacement("<Delimit(\"Have\n\r\", \" a \", \"Cow\") >", Report));

			AssertEquals("Have a Cow\r", ValueProviderToTest.GetReplacement("<Delimit(\"Have\", \" a \", \"Cow\r\") >", Report));
			AssertEquals("Have a Cow\n", ValueProviderToTest.GetReplacement("<Delimit(\"Have\", \" a \", \"Cow\n\") >", Report));
			AssertEquals("Have a Cow\r\n", ValueProviderToTest.GetReplacement("<Delimit(\"Have\", \" a \", \"Cow\r\n\") >", Report));
			AssertEquals("Have a Cow\n\r", ValueProviderToTest.GetReplacement("<Delimit(\"Have\", \" a \", \"Cow\n\r\") >", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Delimit();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var area = new DocumentHeaderArea(1, 2, Report, "");
			area.RenderedToPageNumber = 1;
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = area;
			Report.Renderer.Pages.AddNew();
			Report.Renderer.Pages.AddNew();
		}

		protected override Passes PassToReplaceExample => Passes.SecondPass;
	}
}
