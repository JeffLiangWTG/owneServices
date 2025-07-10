using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Html))]
	sealed class HtmlTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<Html>");
			AssertNotResponsibleForReplacing("<Html()>");

			AssertIsResponsibleForReplacing("<Html(\"\", true)>");
			AssertIsResponsibleForReplacing("< Html ( \"\",  true) >");

			AssertIsResponsibleForReplacing("<Html(\"\\<pre\\>\\Public class\\</pre\\>\", false)>");
			AssertIsResponsibleForReplacing("<Html(\"<HY_Question>\", true)>");
		}

		public void TestReplacement()
		{
			AssertReplacement(Report);
		}

		public void TestReplacementWhenExcelInterfaceIsNull()
		{
			using (var report = new Report(null, null, new DataProviderList(new DocumentWrapperForTesting("Main")), "", new UserControlProviderList(), DocumentDirection.ANY, false, true, false))
			{
				AssertReplacement(report);
			}
		}

		void AssertReplacement(DocumentEngine.Report report)
		{
			string macro = "<Html(\"<pre><font color=\"#0f0\">Interface ITest</font></pre>\", false)>";

			AssertEquals(typeof(TRichString), ValueProviderToTest.GetReplacement(macro, report).GetType());
			AssertEquals("Interface ITest", ValueProviderToTest.GetReplacement(macro, report).ToString());

			macro = "<Html(\"<pre><font color=\"#0f0\">&lt;xml&gt;Content&lt;/xml&gt;</font></pre>\", false)>";

			AssertEquals(typeof(TRichString), ValueProviderToTest.GetReplacement(macro, report).GetType());
			AssertEquals("\\<xml\\>Content\\</xml\\>", ValueProviderToTest.GetReplacement(macro, report).ToString());
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.StaticText, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Html();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("DbField", "WTG"));
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var actualResult = Report.MacroTranslator.GetValue(example, PassToReplaceExample);
			AssertType(typeof(TRichString), actualResult);
			AssertEquals(expectedResult, ((TRichString)actualResult).Value);
		}

		protected override Passes PassToReplaceExample => Passes.SecondPass;
	}
}
