using System.IO;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(HtmlEx))]
	sealed class HtmlExTest : ValueProviderTest
	{
		public void TestHtmlMacroThatExceedsTwoLinesWithShrinkToFitMacro()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<HtmlEx(""<Z0_VarCharMax>"", true)>]
{A}-[#EndOfReport]", "UnitTest");

			using (var excelInterface = new ExcelInterface(template.SO_Template))
			{
				var excelFile = excelInterface.Xls;
				var row = 4;
				var column = 2;

				excelFile.ActiveSheet = 1;

				var cellFormat = excelFile.GetCellFormat(row, column);
				var format = excelFile.GetFormat(cellFormat);

				format.WrapText = true;

				excelFile.SetColWidth(2, 6000);
				excelFile.SetCellFormat(row, column, excelFile.AddFormat(format));

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = "Hello Big <b>Bold</b> World. Hello Big <b>Bold</b> World. Hello Big <b>Bold</b> World.";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();
				Assert(workSheet.GetRowHeight(0) >= 712);
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<HtmlEx>");
			AssertNotResponsibleForReplacing("<HtmlEx()>");

			AssertIsResponsibleForReplacing("<HtmlEx(\"\", true)>");
			AssertIsResponsibleForReplacing("< HtmlEx ( \"\",  true) >");

			AssertIsResponsibleForReplacing("<HtmlEx(\"\\<pre\\>\\Public class\\</pre\\>\", false)>");
			AssertIsResponsibleForReplacing("<HtmlEx(\"<HY_Question>\", true)>");
		}

		public void TestReplacement()
		{
			var macro = "<HtmlEx(\"<pre><font color=\"#0f0\">Interface ITest</font></pre>\", false)>";

			AssertEquals(typeof(TRichString), ValueProviderToTest.GetReplacement(macro, Report).GetType());
			AssertEquals("Interface ITest", ValueProviderToTest.GetReplacement(macro, Report).ToString());

			macro = "<HtmlEx(\"<pre><font color=\"#0f0\">&lt;xml&gt;Content&lt;/xml&gt;</font></pre>\", false)>";

			AssertEquals(typeof(TRichString), ValueProviderToTest.GetReplacement(macro, Report).GetType());
			AssertEquals("\\<xml\\>Content\\</xml\\>", ValueProviderToTest.GetReplacement(macro, Report).ToString());
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new HtmlEx();
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
