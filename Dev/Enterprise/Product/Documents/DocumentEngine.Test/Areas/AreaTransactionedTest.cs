using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class AreaTransactionedTest : TransactionedTestCase
	{
		public void TestForceSplittingBeSetWhenDoSplittingArea()
		{
			var areaToTest = new GroupByArea(4, 12, TestReport, "#GroupBY:Invalid") { forceSplitting = true };
			var splitArea = areaToTest.DoSplitArea(2).Area;
			Assert(splitArea.forceSplitting);
		}

		public void TestExpandWithNullAnalyser()
		{
			using (embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				using (var documentPack = new DocumentPack())
				using (var testReport = new Report(documentPack, excelTemplate))
				{
					var createdArea = AreaFactory.InstantiateArea(1, 10, testReport, "#Config:someOption");
					AssertExceptionThrown<InvalidOperationException>(() => createdArea.Expand(0, 1, 1));
				}
			}
		}

		public void TestShouldApplyFormatPatternOnACellWithNoArea()
		{
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "DocumentCurrency=<CompanyCurrencyCode>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 1] = "ApplyDocumentCurrency";
			TestReport.Analyser.Areas.Clear();
			TestReport.Analyser.Analyse();

			var pageHeaderArea = new PageHeaderArea(1, 10, TestReport, "#Config");
			TestReport.Analyser.Areas.Clear();
			Assert(!pageHeaderArea.ShouldApplyFormatPatternOnCells);
		}

		public void TestShouldApplyFormatPatternOnACell()
		{
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "DocumentCurrency=<CompanyCurrencyCode>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 1] = "ApplyDocumentCurrency";
			TestReport.Analyser.Areas.Clear();
			TestReport.Analyser.Analyse();

			var pageHeaderArea = new PageHeaderArea(1, 10, TestReport, "#Config");
			Assert(!pageHeaderArea.ShouldApplyFormatPatternOnCells);
			var groupByArea = new GroupByArea(1, 10, TestReport, "#GroupBy:Test.Col");
			Assert(groupByArea.ShouldApplyFormatPatternOnCells);
			var sectionBodyArea = new SectionBodyArea(1, 10, TestReport, "#Config");
			Assert(sectionBodyArea.ShouldApplyFormatPatternOnCells);

			TestReport.Renderer.CurrentPass = Passes.SecondPass;
			Assert(!pageHeaderArea.ShouldApplyFormatPatternOnCells);
			Assert(!groupByArea.ShouldApplyFormatPatternOnCells);
			Assert(!sectionBodyArea.ShouldApplyFormatPatternOnCells);

			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			Assert(!pageHeaderArea.ShouldApplyFormatPatternOnCells);
			Assert(groupByArea.ShouldApplyFormatPatternOnCells);
			Assert(sectionBodyArea.ShouldApplyFormatPatternOnCells);

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "DocumentCurrency=ASS";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 1] = "ApplyDocumentCurrency";
			TestReport.Analyser.Areas.Clear();
			TestReport.Analyser.Analyse();
			AssertEquals("The format pattern should be empty", "", TestReport.Analyser.Config.DocumentCurrencyPattern);
			Assert(!pageHeaderArea.ShouldApplyFormatPatternOnCells);
			Assert(!groupByArea.ShouldApplyFormatPatternOnCells);
			Assert(!sectionBodyArea.ShouldApplyFormatPatternOnCells);
		}

		public void TestGetRangesWillReturnCorrectRangeAfterSplit()
		{
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			var areaToTest = new GroupByArea(4, 12, TestReport, "#GroupBY:Invalid");
			areaToTest.SetWorksheetForFormulaProvider(TestReport.WorkSheetCurrentlyBeingProcessed);
			areaToTest.FormulaProvider.AddColumn(2, "test", 3);

			var areas = new List<Area>();
			areas.Add(areaToTest);
			AssertEquals("=C9", areaToTest.FormulaProvider.GetFormula("test", areas, TFileFormats.Xls).Text);

			_ = areaToTest.SplitArea(2);
			AssertEquals("=C10", areaToTest.FormulaProvider.GetFormula("test", areas, TFileFormats.Xls).Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateLinesTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
			testReport?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		Report testReport;
		Report TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
					testReport = new Report(new DocumentPack(), excelTemplate);
					testReport.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
					testReport.PrepareForRender();
					testReport.Renderer.CurrentPass = Passes.SecondPass;
				}
				return testReport;
			}
		}
	}
}
