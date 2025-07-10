using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using FlexCel.Core;
using NUnit.Framework;
using static Enterprise.DocumentEngine.Testing.DocumentPackTest;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class CellContentReplacerTest : TestCaseWithFactory
	{
		public void TestReplaceMultipleMacrosAndNestedMacrosMixedWithStraightText()
		{
			AddMacroValueReplacement("ShoeColour", "R");
			AddMacroValueReplacement("ShoeSize", "L");
			Worksheet[1, 1] = @"  <AutoHeight>Shoe Style: <If(""<ShoeSize>"" == ""L"", ""(<If(""<ShoeColour>"" == ""R"", ""Large Red"", ""Small Red"")>)"", ""Small Blue"")> Code: <ShoeColour><ShoeSize>  Shoe Style: <If(""<ShoeSize>"" == ""L"", ""(<If(""<ShoeColour>"" == ""R"", ""Large Red"", ""Small Red"")>)"", ""Small Blue"")> Code: <ShoeColour><ShoeSize>  ";

			var replacer = new CellContentReplacer(TestReport, 1, 1);
			replacer.ReplaceMacros();

			AssertEquals("Style should be Large Red, also leading and trailing spaces should be untouched.", "  Shoe Style: (Large Red) Code: RL  Shoe Style: (Large Red) Code: RL  ", replacer.Content);
		}

		public void TestCellOverflow()
		{
			Action<string, bool> assertIsCellOverflow = (content, expected) =>
			{
				var replacer = new CellContentReplacer(TestReport, content);
				AssertEquals(content, expected, replacer.IsCellOverflow);
			};

			CombineAssertions(() =>
			{
				assertIsCellOverflow("", false);
				assertIsCellOverflow("Hello World", false);
				assertIsCellOverflow("<ShrinkToFit>Hello World", false);
				assertIsCellOverflow("<AutoHeight><OverFlowToFollowPage(\"Andrew\", 25, MoveAllContentWithContinued)>Hello World", false);
				assertIsCellOverflow("<ShrinkToFit><OverFlowToFollowPage(\"Andrew\", 25, MoveAllContentWithContinued)>Hello World", false);
				assertIsCellOverflow("<ExpandToFit><OverFlowToFollowPage(\"Andrew\", 25, MoveAllContentWithContinued)>Hello World", false);

				assertIsCellOverflow("<OverFlowToFollowPage(\"Andrew\", 25, MoveAllContentWithContinued)>Hello World", true);
			});
		}

		public void TestIsExpandToFit()
		{
			Action<string, bool> assertIsExpandToFit = (content, expected) =>
			{
				var replacer = new CellContentReplacer(TestReport, content);
				AssertEquals(content, expected, replacer.IsCellExpandToFit);
			};

			CombineAssertions(() =>
			{
				assertIsExpandToFit("", false);
				assertIsExpandToFit("Hello World", false);
				assertIsExpandToFit("<ShrinkToFit>Hello World", false);
				assertIsExpandToFit("<ExpandToFit(Middle)>Hello World", false);
				assertIsExpandToFit("<AutoHeight><ExpandToFit>Hello World", false);
				assertIsExpandToFit("<ShrinkToFit><ExpandToFit>Hello World", false);

				assertIsExpandToFit("<ExpandToFit>Hello World", true);
				assertIsExpandToFit("<ExpandToFit(First)>Hello World", true);
				assertIsExpandToFit("<ExpandToFit(Last)>Hello World", true);
			});
		}

		public void TestRowToExpand()
		{
			Action<string, RowToExpand> assertRowToExpand = (content, expected) =>
			{
				var replacer = new CellContentReplacer(TestReport, content);
				AssertEquals(content, expected, replacer.RowToExpand);
			};

			CombineAssertions(() =>
			{
				assertRowToExpand("", RowToExpand.First);
				assertRowToExpand("Hello World", RowToExpand.First);
				assertRowToExpand("<ShrinkToFit>Hello World", RowToExpand.First);
				assertRowToExpand("<ExpandToFit(Middle)>Hello World", RowToExpand.First);
				assertRowToExpand("<AutoHeight><ExpandToFit(Last)>Hello World", RowToExpand.First);
				assertRowToExpand("<ShrinkToFit><ExpandToFit>Hello World", RowToExpand.First);

				assertRowToExpand("<ExpandToFit>Hello World", RowToExpand.First);
				assertRowToExpand("<ExpandToFit(First)>Hello World", RowToExpand.First);
				assertRowToExpand("<ExpandToFit(Last)>Hello World", RowToExpand.Last);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplaceMacrosUsesOverringDataSetForDocuments()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			dummyBizO.Z0_Code = "Code1";

			var mockDocSupportBizO = new MockDocSupportBizO();
			var menuItem = Factory.New<DocumentCommand>();
			var pack = new TestableDocumentPack(menuItem, mockDocSupportBizO, null);

			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(dummyBizO), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				pack.Add(report);
				report.PrepareForRender();
				var components = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet).Components;

				var dataTable = report.OverridingDataSet.MainTable;
				dataTable.Rows[0]["<Z0_Code>"] = "Code2";
				pack.SaveVisualizerContentNote();

				var worksheet = report.WorkSheetCurrentlyBeingProcessed;
				worksheet[1, 1] = "<Z0_Code>";

				var cellContentReplacer = new CellContentReplacer(report, 1, 1);
				cellContentReplacer.ReplaceMacros();
				AssertEquals("Content got replaced", "Code2", cellContentReplacer.Content);
			}
		}

		public void TestReplaceMacrosDoesNotLoadOverridingDataSetForReports()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
					report.PrepareForRender();

					var worksheet = report.WorkSheetCurrentlyBeingProcessed;
					report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("FingersIAmHoldingUp", "1"));

					worksheet[1, 1] = @"<FingersIAmHoldingUp>";

					var cellContentReplacer = new CellContentReplacer(report, 1, 1);

					var initialCount = BusinessObjectFactory._NextInstance;
					cellContentReplacer.ReplaceMacros();
					var currentCount = BusinessObjectFactory._NextInstance;
					AssertEquals("Content got replaced", "1", cellContentReplacer.Content);
					AssertEquals("No new factories should have been created.", initialCount, currentCount);
				}
			}
		}

		public void TestMultipleMacrosEvaluateFromTheOutermostMacrosIn()
		{
			AddMacroValueReplacement("FingersIAmHoldingUp", "1");
			AddMacroValueReplacement("FieldForFingersIAmHoldingUp", "<FingersIAmHoldingUp>");

			Worksheet[1, 1] = @"<EvaluateInnerContent(""<FieldForFingersIAmHoldingUp> DAY"")>";
			var cellReplacer1 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer1.ReplaceMacros();
			AssertEquals("cellReplacer.Content", "1 DAY", cellReplacer1.Content);

			Worksheet[1, 1] = @"<AutoHeight><EvaluateInnerContent(""<FieldForFingersIAmHoldingUp> DAY"")>";
			var cellReplacer2 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer2.ReplaceMacros();
			AssertEquals("cellReplacer.Content", "1 DAY", cellReplacer2.Content);
		}

		public void TestStringValuesKeepSurroundingWhiteSpaceWhereTheresLeadingOrTrailingBlanks()
		{
			AddMacroValueReplacement("ShoeStyle", "LARGE");
			Worksheet[1, 1] = "<ShoeStyle> ";
			var cellReplacer1 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer1.ReplaceMacros();
			AssertEquals("cellReplacer.Content", "LARGE ", cellReplacer1.Content);

			Worksheet[1, 1] = " <ShoeStyle>";
			var cellReplacer2 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer2.ReplaceMacros();
			AssertEquals("cellReplacer.Content", " LARGE", cellReplacer2.Content);
		}

		public void TestNonStringValuesGetReplacedInTheirNativeTypeEvenIfTheresLeadingOrTrailingBlanks()
		{
			AddMacroValueReplacement("ShoeSize", 1.23m);

			Worksheet[1, 1] = "<ShoeSize> ";
			var cellReplacer1 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer1.ReplaceMacros();
			AssertEquals("cellReplacer.Content", 1.23m, cellReplacer1.Content);

			Worksheet[1, 1] = " <ShoeSize>";
			var cellReplacer2 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer2.ReplaceMacros();
			AssertEquals("cellReplacer.Content", 1.23m, cellReplacer2.Content);
		}

		public void TestNonStringValuesGetReplacedInTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacros()
		{
			AddMacroValueReplacement("TestDecimal", 1.23m);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestDecimal>", 1.23m, typeof(decimal));

			AddMacroValueReplacement("TestZDecimal", new ZDecimal(1.230m));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestZDecimal>", 1.230m, typeof(ZDecimal));

			AddMacroValueReplacement("TestZInt", new ZInt(2));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestZInt>", 2, typeof(ZInt));

			AddMacroValueReplacement("TestZLong", new ZLong(3));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestZLong>", 3L, typeof(ZLong));

			AddMacroValueReplacement("TestZBool", new ZBool(true));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestZBool>", new ZBool(true), typeof(ZBool));

			AddMacroValueReplacement("TestZDateTime", new ZDateTime(2017, 1, 1));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestZDateTime>", new ZDateTime(2017, 1, 1), typeof(ZDateTime));

			AddMacroValueReplacement("TestZDate", new ZDate(2017, 1, 1));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestZDate>", new ZDate(2017, 1, 1), typeof(ZDate));

			var tFormula = new TFormula("=SUM(A32:A33)");
			AddMacroValueReplacement("TestTFormula", tFormula);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases("<TestTFormula>", tFormula, typeof(TFormula));
		}

		[TestDate(2008, 12, 3, 2, 46, 30)]
		public void TestDateTimeFormat()
		{
			Worksheet[1, 1] = "<Now>";
			CellContentReplacer cellReplacer1 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer1.ReplaceMacros();
			AssertEquals("cellReplacer.Content", new ZDateTime(2008, 12, 3, 2, 46, 30), cellReplacer1.Content);
			AssertEquals("cellReplacer.Content Type", typeof(ZDateTime), cellReplacer1.Content.GetType());

			Worksheet[1, 1] = "Today is <Now>";
			CellContentReplacer cellReplacer2 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer2.ReplaceMacros();
			AssertEquals("cellReplacer.Content", "Today is 03-Dec-08 02:46", cellReplacer2.Content);
			AssertEquals("cellReplacer.Content Type", typeof(string), cellReplacer2.Content.GetType());
		}

		public void TestNonStringValuesGetReplacedInTheirNativeType()
		{
			AddMacroValueReplacement("ShoeSize", 1.23m);
			Worksheet[15, 12] = "<ShoeSize>";
			var cellReplacer = new CellContentReplacer(TestReport, 15, 12);
			cellReplacer.ReplaceMacros();
			AssertEquals("cellReplacer.Content", 1.23m, cellReplacer.Content);
		}

		public void TestAllErrorsGetReported()
		{
			AddMacroValueReplacement("FieldOK1", "Value is irrelevant - It's the fields that aren't there that matter.");
			AddMacroValueReplacement("FieldOK2", "Value still irrelevant...");
			Worksheet[3, 2] = "<FieldOK1>:<FieldNotOK1><FieldNotOK2><FieldOK2>";
			var cellReplacer = new CellContentReplacer(TestReport, 3, 2);
			AssertEquals("Pre-Condition: TestReport.ErrorManager.HasErrors is false", false, TestReport.ErrorManager.HasErrors);
			cellReplacer.ReplaceMacros();
			AssertEquals("Report.Errors", @"
Severity: [Warning (without error report)] Message: [Field <FieldNotOK1> not found on DataSource.]
Severity: [Warning (without error report)] Message: [Field <FieldNotOK2> not found on DataSource.]
".Trim(), TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestHasHideRowIfCellIsEmpty()
		{
			AddMacroValueReplacement("ShoeStyle", "Big Fat and Hairy");
			Worksheet[1, 1] = "<ShoeStyle>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.HasHideRowIfCellIsEmpty", false, cellReplacer.IsHideRowIfCellIsEmpty);
			Worksheet[1, 1] = "<HideRowIfCellIsEmpty><ShoeStyle>";
			cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.HasHideRowIfCellIsEmpty", true, cellReplacer.IsHideRowIfCellIsEmpty);
		}

		public void TestAutoHeightWithStringValuesKeepSurroundingWhiteSpaceWhereThereisTrailingBlanks()
		{
			AddMacroValueReplacement("ReportData.Product", "MyProduct");
			Worksheet[1, 1] = "<AutoHeight><ReportData.Product> ";
			var cellReplacer1 = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer1.ReplaceMacros();
			AssertEquals("cellReplacer.Content", "MyProduct ", cellReplacer1.Content);
		}

		public void TestReplaceTextWithDollarSign()
		{
			AddMacroValueReplacement("ReportData.Amount", "$9876543210123456789");
			Worksheet[1, 1] = "<AutoHeight><ReportData.Amount> ";
			var replacer = new CellContentReplacer(TestReport, 1, 1);
			AssertNoExceptionThrown(() => replacer.ReplaceMacros());
			AssertEquals("Amount should be $9876543210123456789 ", "$9876543210123456789 ", replacer.Content);
		}

		public void TestReplaceTwoMacrosWithDollarSigns()
		{
			AddMacroValueReplacement("ReportData.Amount", "$9876543210123456789");
			Worksheet[1, 1] = "<AutoHeight><ReportData.Amount> <ReportData.Amount> ";
			var replacer = new CellContentReplacer(TestReport, 1, 1);
			AssertNoExceptionThrown(() => replacer.ReplaceMacros());
			AssertEquals("Amount should be $9876543210123456789 $9876543210123456789 ", "$9876543210123456789 $9876543210123456789 ", replacer.Content);
		}

		public void TestReplaceTextWithTwoDollarSigns()
		{
			AddMacroValueReplacement("ReportData.Amount", "$$9876543210123456789");
			Worksheet[1, 1] = "<AutoHeight><ReportData.Amount> ";
			var replacer = new CellContentReplacer(TestReport, 1, 1);
			AssertNoExceptionThrown(() => replacer.ReplaceMacros());
			AssertEquals("Amount should be $$9876543210123456789 ", "$$9876543210123456789 ", replacer.Content);
		}

		public void TestIsCellAutoHeight()
		{
			AddMacroValueReplacement("ShoeStyle", "Big Fat and Hairy");
			Worksheet[1, 1] = "<ShoeStyle>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellAutoHeight", false, cellReplacer.IsCellAutoHeight);
			Worksheet[1, 1] = "<AutoHeight><ShoeStyle>";
			cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellAutoHeight", true, cellReplacer.IsCellAutoHeight);

			Worksheet[1, 1] = "<Html(\"Html Test\", false)>";
			cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellAutoHeight", true, cellReplacer.IsCellAutoHeight);
		}

		public void TestIsCellShrinkToFit()
		{
			AddMacroValueReplacement("ShoeStyle", "Big Fat and Hairy");
			Worksheet[1, 1] = "<ShoeStyle>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellShrinkToFit", false, cellReplacer.IsCellShrinkToFit);
			AssertEquals("cellReplacer.IsCellShrinkToFitForBillOfLading", false, cellReplacer.IsCellShrinkToFitForBillOfLading);
			Worksheet[1, 1] = "<ShrinkToFit><ShoeStyle>";
			cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellShrinkToFit", true, cellReplacer.IsCellShrinkToFit);
			AssertEquals("cellReplacer.IsCellShrinkToFitForBillOfLading", false, cellReplacer.IsCellShrinkToFitForBillOfLading);
		}

		public void TestIsCellShrinkToFitForBillOfLading()
		{
			AddMacroValueReplacement("ShoeStyle", "Big Fat and Hairy");
			Worksheet[1, 1] = "<ShoeStyle>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellShrinkToFitForBillOfLading", false, cellReplacer.IsCellShrinkToFitForBillOfLading);
			AssertEquals("cellReplacer.IsCellShrinkToFit", false, cellReplacer.IsCellShrinkToFit);
			Worksheet[1, 1] = "<ShrinkToFitForBillOfLading><ShoeStyle>";
			cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("cellReplacer.IsCellShrinkToFitForBillOfLading", true, cellReplacer.IsCellShrinkToFitForBillOfLading);
			AssertEquals("cellReplacer.IsCellShrinkToFit", false, cellReplacer.IsCellShrinkToFit);
		}

		public void TestIsReplacedAndStillContainsAtLeastOneMacro()
		{
			AddMacroValueReplacement("ShoeStyle", "Big Fat and Hairy");
			Worksheet[15, 12] = "<ShoeStyle>";
			var cellReplacer = new CellContentReplacer(TestReport, 15, 12);
			AssertEquals("cellReplacer.StillContainsAtLeastOneMacro", true, cellReplacer.StillContainsAtLeastOneMacro);
			AssertEquals("cellReplacer.Content", "<ShoeStyle>", cellReplacer.Content);
			AssertEquals("cellReplacer.ContentAsString", "<ShoeStyle>", cellReplacer.ContentAsString);
			AssertEquals("cellReplacer.HasHideRowIfCellIsEmpty", false, cellReplacer.IsHideRowIfCellIsEmpty);
			AssertEquals("cellReplacer.IsCellAutoHeight", false, cellReplacer.IsCellAutoHeight);
			AssertEquals("cellReplacer.IsCellShrinkToFit", false, cellReplacer.IsCellShrinkToFit);
			AssertEquals("cellReplacer.IsReplaced", false, cellReplacer.IsReplaced);

			cellReplacer.ReplaceMacros();
			AssertEquals("cellReplacer.StillContainsAtLeastOneMacro", false, cellReplacer.StillContainsAtLeastOneMacro);
			AssertEquals("cellReplacer.Content", "Big Fat and Hairy", cellReplacer.Content);
			AssertEquals("cellReplacer.ContentAsString", "Big Fat and Hairy", cellReplacer.ContentAsString);
			AssertEquals("cellReplacer.HasHideRowIfCellIsEmpty", false, cellReplacer.IsHideRowIfCellIsEmpty);
			AssertEquals("cellReplacer.IsCellAutoHeight", false, cellReplacer.IsCellAutoHeight);
			AssertEquals("cellReplacer.IsCellShrinkToFit", false, cellReplacer.IsCellShrinkToFit);
			AssertEquals("cellReplacer.IsReplaced", true, cellReplacer.IsReplaced);
		}

		public void TestTrimTrailingLineFeeds()
		{
			AddMacroValueReplacement("ShoeStyle", "\nBig Fat\n and Hairy\n\r\n\r\n");
			Worksheet[1, 1] = "<ShoeStyle>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer.ReplaceMacros();
			AssertEquals("Precondition: cellReplacer.Content", "\nBig Fat\n and Hairy\n\r\n\r\n", cellReplacer.Content);
			cellReplacer.TrimTrailingLineFeeds();
			AssertEquals("cellReplacer.Content", "\nBig Fat\n and Hairy", cellReplacer.Content);
		}

		public void TestDoNotTrimTrailingLineFeedsForTRichString()
		{
			AddMacroValueReplacement("Whatever", new TRichString("Do trim\r\n\r\n"));
			Worksheet[1, 1] = "<Whatever>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			cellReplacer.ReplaceMacros();
			AssertEquals("Precondition: cellReplacer.Content", "Do trim\r\n\r\n", cellReplacer.ContentAsString);
			cellReplacer.TrimTrailingLineFeeds();
			AssertEquals("cellReplacer.Content", "Do trim", cellReplacer.ContentAsString);
			AssertEquals("Type of cellReplacer.Content", typeof(TRichString), cellReplacer.Content.GetType());
		}

		public void TestUnEscapeEscapedMacroSyntax()
		{
			Worksheet[1, 1] = "\\<ShoeStyle\\>";
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			AssertEquals("Precondition: cellReplacer.Content", "\\<ShoeStyle\\>", cellReplacer.Content);
			cellReplacer.UnEscapeEscapedMacroSyntax();
			AssertEquals("cellReplacer.Content", "<ShoeStyle>", cellReplacer.Content);
		}

		public void TestGet2PartRichStringValue()
		{
			var excelFile = TestReport.XlInterface.Xls;
			var richString = new TRichString();
			richString.SetFromHtml("&lt;ShoeStyle&gt;<B>&lt;ShoeStyle&gt;</B>", excelFile.GetDefaultFormat, excelFile);
			AssertEquals("Precondition: richString.ToString()", "<ShoeStyle><ShoeStyle>", richString.ToString());
			Worksheet[1, 2] = richString;
			AddMacroValueReplacement("ShoeStyle", "Big Fat and Hairy");
			var cellReplacer = new CellContentReplacer(TestReport, 1, 2);
			cellReplacer.ReplaceMacros();
			Worksheet[1, 2] = cellReplacer.Content;
			var formatter = new BoldResponsiveCellFormatter();
			var actualValueAsHTML = formatter.Format(Worksheet, 1, 2);
			AssertEquals("1:2>- Raw Value: " + Worksheet[1, 2].ToString(), "Big Fat and Hairy<B>Big Fat and Hairy<\\B>", actualValueAsHTML);
		}

		public void TestRightToLeftMarkIsAddedToNonEmptyCellsForRightToLeftLanguages()
		{
			//Non-right-to-left languages: cells should not be affected at all
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
			{
				Worksheet[1, 1] = "ABC";
				var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
				cellReplacer.ForceFormatForRightToLeftLanguage();
				AssertEquals("ABC", cellReplacer.Content);

				Worksheet[1, 1] = string.Empty;
				cellReplacer = new CellContentReplacer(TestReport, 1, 1);
				cellReplacer.ForceFormatForRightToLeftLanguage();
				AssertEquals(string.Empty, cellReplacer.Content);
			}

			//Right-to-left languages: non-empty cells should contain the right-to-left-mark
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Arabic))
			{
				Worksheet[1, 1] = "ABC";
				var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
				cellReplacer.ForceFormatForRightToLeftLanguage();
				var rightToLeftMark = (char)0x200F;
				AssertEquals(rightToLeftMark + "ABC", cellReplacer.Content);

				Worksheet[1, 1] = string.Empty;
				cellReplacer = new CellContentReplacer(TestReport, 1, 1);
				cellReplacer.ForceFormatForRightToLeftLanguage();
				AssertEquals(string.Empty, cellReplacer.Content);
			}
		}

		public void TestIsAutoHeight()
		{
			Worksheet[1, 1] = "<AutoHeight>";
			var replacer = new CellContentReplacer(TestReport, 1, 1);

			Assert(replacer.IsCellAutoHeight);
			Assert(!replacer.RemoveLineBreaksToFitAutoHeightOverflow);
		}

		public void TestRemoveLineBreaksToFitAutoHeightOverflow()
		{
			Worksheet[1, 1] = "<AutoHeight(RemoveLineBreaksToFit)>";
			var replacer = new CellContentReplacer(TestReport, 1, 1);

			Assert(replacer.IsCellAutoHeight);
			Assert(replacer.RemoveLineBreaksToFitAutoHeightOverflow);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShrinkToFitWorkingWithTFormulaValueProvider()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var child = dummy.Collection.AddNew();
			child.Z0_Number = 1000000000;

			var excelTemplate = new ExcelTemplateForUnitTesting("FormattingMacrosAndDataTypeIncludingTFormula.xlsx", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(DocumentPack.EmptyPack, excelTemplate, BODocDataProvider.Get(dummy), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals("<ShrinkToFit><AccumulativeTotal Collection.Z0_Number>", report.WorkSheetCurrentlyBeingProcessed[4, 2]);
				var originalFontSizeForShrinkToFit = report.WorkSheetCurrentlyBeingProcessed.GetCellFontSize(4, 2);

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						//ShrinkToFit with TFormula
						var fontSizeNormalForShrinkToFit = excelInterface.WorkSheets[0].GetCellFontSize(0, 1);
						var fontSizeTFormulaForShrinkToFit = excelInterface.WorkSheets[0].GetCellFontSize(0, 2);
						Assert(fontSizeNormalForShrinkToFit < originalFontSizeForShrinkToFit);
						AssertEquals(fontSizeNormalForShrinkToFit, fontSizeTFormulaForShrinkToFit);
					}
				}
			}
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
			worksheet?.Dispose();
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

		ExcelWorkSheet worksheet;
		ExcelWorkSheet Worksheet => worksheet ?? (worksheet = TestReport.WorkSheetCurrentlyBeingProcessed);

		void AddMacroValueReplacement(string macro, object replacement)
		{
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider(macro, replacement));
		}

		void AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string macro, object expectedValue, Type expectedType)
		{
			Worksheet[1, 1] = macro;
			var cellReplacer = new CellContentReplacer(TestReport, 1, 1);
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			cellReplacer.ReplaceMacros();
			TestReport.Renderer.CurrentPass = Passes.SecondPass;
			cellReplacer.ReplaceMacros();
			AssertEquals("cellReplacer.Content", expectedValue, cellReplacer.Content);
			AssertEquals("cellReplacer.Content Type", expectedType, cellReplacer.Content.GetType());
		}

		void AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacroInDifferentCases(string macro, object expectedValue, Type expectedType)
		{
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("<AutoHeight>{0} ", macro), expectedValue, expectedType);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("<ShrinkToFit>  {0} ", macro), expectedValue, expectedType);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("  <ShrinkToFitForBillOfLading>{0} ", macro), expectedValue, expectedType);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format(" <HideRowIfCellIsEmpty> {0} ", macro), expectedValue, expectedType);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("<HideRowIf(1==2)> {0} ", macro), expectedValue, expectedType);
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("<ExpandToFit>{0} ", macro), expectedValue, expectedType);

			if (expectedType != typeof(ZDateTime) && expectedType != typeof(ZDate))
			{
				AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("<OverFlowToFollowPage(\"Test\")>{0} ", macro), expectedValue, expectedType);
			}

			if (expectedType == typeof(ZDateTime))
			{
				expectedValue = ((ZDateTime)expectedValue).ToLongTimeString();
			}

			if (expectedType == typeof(TFormula))
			{
				expectedValue = TestReport.MacroTranslator.GetFormulaResult(((TFormula)expectedValue).Text);
			}

			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("{0} {0}", macro), string.Format("{0} {0}", expectedValue.ToString()), typeof(string));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("{0}US", macro), string.Format("{0}US", expectedValue.ToString()), typeof(string));
			AssertTestNonStringValuesGetReplacedByTheirNativeTypeEvenIfTheresLeadingFormattingOnlyMacro(string.Format("{0} US", macro), string.Format("{0} US", expectedValue.ToString()), typeof(string));
		}
	}
}
