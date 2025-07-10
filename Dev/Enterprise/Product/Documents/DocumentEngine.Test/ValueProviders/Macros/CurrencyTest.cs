using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Currency))]
	sealed class CurrencyTest : ValueProviderWithLoadControlFactoryTest<Currency>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Currency to words(12, USD)>", !ValueProviderToTest.IsResponsibleForReplacing("<Currency to words(12, USD)>", Passes.FirstPass));
			Assert("should not match <   currency  \t       blah   >", !ValueProviderToTest.IsResponsibleForReplacing("<   currency  \t       blah   >", Passes.FirstPass));
			Assert("should match <   currency  \t       (1.2, USD)   >", ValueProviderToTest.IsResponsibleForReplacing("<   currency  \t       (1.2, USD)   >", Passes.SecondPass));
			Assert("should match <   currency  \t       (, USD)   >", ValueProviderToTest.IsResponsibleForReplacing("<   currency  \t       (, USD)   >", Passes.SecondPass));
			Assert("should match <   currency  \t       (, )   >", ValueProviderToTest.IsResponsibleForReplacing("<   currency  \t       (, )   >", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			PrepareRenderer();
			SectionBodyArea testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			GroupByArea testGroupByAreaArea = new GroupByArea(3, 4, Report, "#GroupBy:Fld");
			testGroupByAreaArea.DataParent = testSectionBodyArea;
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;

			FormattedCellValue returnedValue = ValueProviderToTest.GetReplacement("<Currency(1.2, USD)>", Report) as FormattedCellValue;
			AssertNotNull(returnedValue);
			AssertEquals(1.2M, returnedValue.Value);
			AssertEquals("#,##0.00", returnedValue.Format);

			returnedValue = ValueProviderToTest.GetReplacement("<Currency(1001.2, USD)>", Report) as FormattedCellValue;
			AssertNotNull(returnedValue);
			AssertEquals(1001.2M, returnedValue.Value);
			AssertEquals("#,##0.00", returnedValue.Format);

			returnedValue = ValueProviderToTest.GetReplacement("<Currency(1.2, OMR)>", Report) as FormattedCellValue;
			AssertNotNull(returnedValue);
			AssertEquals(1.2M, returnedValue.Value);
			AssertEquals("#,##0.000", returnedValue.Format);

			returnedValue = ValueProviderToTest.GetReplacement("<Currency(1.2, JPY)>", Report) as FormattedCellValue;
			AssertNotNull(returnedValue);
			AssertEquals(1.2M, returnedValue.Value);
			AssertEquals("#,##0", returnedValue.Format);

			FormattedCellValue emptyValue = (FormattedCellValue)ValueProviderToTest.GetReplacement("<Currency(, JPY)>", Report);
			AssertEquals("Should return and empty string if no value is passed", "", emptyValue.Value.ToString());

			AssertExceptionThrown(typeof(System.FormatException), CheckForBadCurrencyException);

			string exampleOfColumnNotFound = "Column 'P41' is not found in Table 'ReportData'";
			FormattedCellValue result = (FormattedCellValue)ValueProviderToTest.GetReplacement("<Currency(" + exampleOfColumnNotFound + ", JPY)>", Report);
			AssertEquals("Should handle column not found", exampleOfColumnNotFound, result.Value.ToString());

			FormattedCellValue localCurrencyResult = (FormattedCellValue)ValueProviderToTest.GetReplacement("<Currency(0," + GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency + ")>", Report);
			Assert("There should be a format value for local currency", localCurrencyResult.Format.Length > 0);
			FormattedCellValue defaultFormat = (FormattedCellValue)ValueProviderToTest.GetReplacement("<Currency(, )>", Report);
			AssertEquals("When no currency specified should default to local currency", defaultFormat.Format, localCurrencyResult.Format);
			AssertEquals("When no value and no currency provided value is still empty stirng", defaultFormat.Value.ToString(), "");

			// Check what happens when bad data is passed in 
			FormattedCellValue output = (FormattedCellValue)ValueProviderToTest.GetReplacement("<Currency(123.456,00,USD)>", Report);
			AssertEquals("123.456,00", output.Value);
		}

		public void TestReplacementsWithFormula()
		{
			PrepareRenderer();
			SectionBodyArea sectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			GroupByArea groupByArea = new GroupByArea(3, 4, Report, "#GroupBy:Fld");
			groupByArea.DataParent = sectionBodyArea;
			sectionBodyArea.ExpandForDataRows(4);
			sectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			sectionBodyArea.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
			Report.Renderer.CurrentAreaToProcess = groupByArea;

			FormattedCellValue formattedCellValue = ValueProviderToTest.GetReplacement("<Currency(1.2, USD)>", Report) as FormattedCellValue;
			AssertNotNull(formattedCellValue);
			AssertEquals(1.2M, formattedCellValue.Value);
			AssertEquals("#,##0.00", formattedCellValue.Format);

			TFormula formula = ValueProviderToTest.GetReplacement("<Currency(=B25, USD)>", Report) as TFormula;
			AssertNotNull(formula);
			AssertEquals("=ROUND(B25,2)", formula.Text);

			TFormula formulaSum = ValueProviderToTest.GetReplacement("<Currency(=B25+A1+C3, OMR)>", Report) as TFormula;
			AssertNotNull(formulaSum);
			AssertEquals("=ROUND(B25+A1+C3,3)", formulaSum.Text);
		}

		void CheckForBadCurrencyException()
		{
			object errorValue = ValueProviderToTest.GetReplacement("<Currency(1.2, XXX)>", Report);
			Assert(errorValue is string);
			AssertEquals("1.2, XXX", errorValue.ToString());
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", 1m));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("CurrencyCode", "JPY"));
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
