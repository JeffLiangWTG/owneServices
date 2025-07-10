using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ExchangeRate))]
	sealed class ExchangeRateTest : ValueProviderWithLoadControlFactoryTest<ExchangeRate>
	{
		public void TestReplacementWithInputStringCouldNotParseToDecimal()
		{
			Report.ErrorManager.ClearErrors();

			var returnedValue = ValueProviderToTest.GetReplacement("<ExchangeRate()>", Report);

			AssertEquals("Should be empty", "", returnedValue);
			AssertEquals(false, Report.ErrorManager.HasErrors);

			returnedValue = ValueProviderToTest.GetReplacement("<ExchangeRate(test)>", Report);
			AssertEquals("Should be empty", "", returnedValue);
			AssertEquals(true, Report.ErrorManager.HasErrors);
			AssertEquals("Error in ExchangeRate Macro: Couldn't parse test to Decimal.", ((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
		}

		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <ExchangeRate(12, USD)>", !ValueProviderToTest.IsResponsibleForReplacing("<ExchangeRate(12, USD)>", Passes.FirstPass));
			Assert("should not match <   ExchangeRate  \t       blah   >", !ValueProviderToTest.IsResponsibleForReplacing("<   ExchangeRate  \t       blah   >", Passes.FirstPass));
			Assert("should match <   Exchange  Rate  \t       (1.2231123)   >", ValueProviderToTest.IsResponsibleForReplacing("<   Exchange   Rate  \t       (1.2231123)   >", Passes.SecondPass));
			Assert("should match <   Exchange  Rate  \t       ()   >", ValueProviderToTest.IsResponsibleForReplacing("<   Exchange   Rate  \t       ()   >", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			PrepareRenderer();
			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			var testGroupByAreaArea = new GroupByArea(3, 4, Report, "#GroupBy:Fld");
			testGroupByAreaArea.DataParent = testSectionBodyArea;
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;

			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			FormattedCellValue returnedValue = ValueProviderToTest.GetReplacement("<ExchangeRate(1.2)>", Report) as FormattedCellValue;
			AssertNotNull(returnedValue);
			AssertEquals(1.2M, returnedValue.Value);
			AssertEquals("0.000000", returnedValue.Format);

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			returnedValue = ValueProviderToTest.GetReplacement("<ExchangeRate(1.2)>", Report) as FormattedCellValue;
			AssertNotNull(returnedValue);
			AssertEquals(1.2M, returnedValue.Value);
			AssertEquals("0.000000", returnedValue.Format);

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			AssertEquals("", ValueProviderToTest.GetReplacement("<ExchangeRate()>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			var testGroupByAreaArea = new GroupByArea(3, 4, Report, "#GroupBy:Fld");
			testGroupByAreaArea.DataParent = testSectionBodyArea;
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ExRate", 1.2));
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var actualResult = Report.MacroTranslator.GetValue(example, PassToReplaceExample);
			AssertType(typeof(FormattedCellValue), actualResult);
			AssertEquals("\"" + example + "\" produced an unexpected result", ((FormattedCellValue)expectedResult).Value, ((FormattedCellValue)actualResult).Value);
			AssertEquals("\"" + example + "\" produced an unexpected result", ((FormattedCellValue)expectedResult).Format, ((FormattedCellValue)actualResult).Format);
		}
	}
}
