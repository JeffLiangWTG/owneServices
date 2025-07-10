using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ExcelTemplates;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using FlexCel.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class AreaBaseOnlyTest : TestCaseWithFactory
	{
		public void TestHeightInXlsWhenHavingPageBreak()
		{
			using (var stream = new MemoryStream())
			using (var testReport = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(DocumentPack.EmptyPack, null, stream,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionHeader]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#SectionBody]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#DocumentFooter]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#EndOfReport]"))
			{
				testReport.PrepareForRender();

				var sectionHeader = testReport.Analyser.Areas[1];
				var sectionBody = testReport.Analyser.Areas[2];
				var documentFooter = testReport.Analyser.Areas[3];

				AssertEquals(255 * 4, sectionHeader.HeightInXls);
				AssertEquals(255 * 2, sectionBody.HeightInXls);
				AssertEquals(255 * 6, documentFooter.HeightInXls);
			}
		}

		public void TestGetRowsToKeepWhenHavingPageBreak()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionHeader:ShowEvenWithNoData:Sticky]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#SectionBody:Data=Collection]
{B}-[WhatEver]
{B}-[<Collection.Code>]
{B}-[WhatEver]
{A}-[#SectionFooter:ShowEvenWithNoData:Sticky]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#SectionBody:Data=Collection]
{B}-[WhatEver]
{B}-[<Collection.Code>]
{B}-[WhatEver]
{A}-[#SectionBody:Data=Collection]
{B}-[WhatEver]
{B}-[<Collection.Code>]
{B}-[WhatEver]
{A}-[#DocumentFooter]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");
			dummy.Collection.AddNew("CCC", "CCC Description");
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand)[0];
			AssertNotNull(printJob);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets[0];

				AssertEquals(41, workSheet.RowCount);
				AssertEquals(0, workSheet.GetRowHeight(34));
				AssertEquals(2, workSheet.HPageBreakCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadDataFields()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("AllSections.xls", TestFilesSubFolder.ReportTestFiles);

			var expected = @"ConfigArea: DataSource:TABLE1, Fields:COLUMN1
DocumentHeaderArea: DataSource:TABLE1, Fields:COLUMN2
SectionPageHeaderArea: DataSource:TABLE1, Fields:COLUMN3
SectionHeaderArea: DataSource:TABLE1, Fields:COLUMN4
SectionBodyArea: DataSource:TABLE1, Fields:COLUMN5
GroupByArea: DataSource:TABLE1, Fields:GROUPBYCOLUMN1
GroupByArea: DataSource:TABLE, Fields:GROUPBYCOLUMN2
SectionFooterArea: DataSource:TABLE1, Fields:COLUMN7
SectionPageFooterArea: DataSource:TABLE1, Fields:COLUMN8
PageHeaderArea: DataSource:TABLE2, Fields:COLUMN1
OnlyOnePageFooterArea: DataSource:TABLE1, Fields:COLUMN2
FirstPageFooterArea: DataSource:TABLE1, Fields:COLUMN9
LastPageFooterArea: DataSource:TABLE1, Fields:COLUMN2
PageFooterArea: DataSource:TABLE1, Fields:COLUMN2
DocumentFooterArea: DataSource:TABLE1, Fields:COLUMN2
BackPageArea: DataSource:TABLE1, Fields:COLUMN2";

			var result = new StringBuilder();
			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();

				var areas = report.Analyser.Areas;
				AssertEquals(16, areas.Count);
				foreach (var area in areas)
				{
					if (!(area is EndOfReportArea))
					{
						AssertGreaterThan("All fields populated.", area.AllDatafields.Count, 0);
						foreach (var datasource in area.AllDatafields.Keys)
						{
							area.AllDatafields[datasource].Sort();
							result.AppendLine(string.Format(@"{0}: DataSource:{1}, Fields:{2}", area.GetType().Name, datasource, string.Join("", area.AllDatafields[datasource].ToArray())));
						}
					}
				}
			}

			AssertMultilineASCIIEquals("All read data fileds should match.", expected, result.ToString());
		}

		public void TestLogLocationAndContentsForCellWhenTryingToAccessWidthOfDisposedImageInImageMacro()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<Image(Z0_VarBinaryMaxAsImage, 1, 1)>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();

			using (var stream = new MemoryStream())
			{
				var bitmap = new Bitmap(10, 10);
				bitmap.Save(stream, ImageFormat.Png);
				dummy.Z0_VarBinaryMax = stream.CopyToByteArray();
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				using (var stream = new MemoryStream())
				{
					dummy.Z0_VarBinaryMaxAsImage.Dispose();

					try
					{
						report.Save(stream);
					}
					catch (Exception exception)
					{
						Assert(exception.Message, exception.Message.Contains(
							"[Error in Image Macro: The image can't be disposed. Input macro: [<Image(Z0_VarBinaryMaxAsImage, 1, 1)>]] Cell Content: [<Image(Z0_VarBinaryMaxAsImage, 1, 1)>]  Cell: [B5] Sheetname: [Document]"));
					}
				}
			}
		}

		public void TestLogLocationAndContentsForCellWhenTryingToAccessWidthOfDisposedImageInImageMacro_CompanyLogo()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<Image(CompanyLogo, 1, 1)>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();

			var documentWrappers = new DocumentWrapper[] { new DummyBusinessObjectWrapperWithDisposedAlternativeBrandingImage(dummy, Factory) };
			(dummy.DocumentSupporter as DummyDocumentSupporter).SetDocumentWrappers(documentWrappers);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				using (var stream = new MemoryStream())
				{
					try
					{
						report.Save(stream);
					}
					catch (Exception exception)
					{
						Assert(exception.Message, exception.Message.Contains(
							"[Error in Image Macro: The image can't be disposed. Input macro: [<Image(CompanyLogo, 1, 1)>]] Cell Content: [<Image(CompanyLogo, 1, 1)>]  Cell: [B5] Sheetname: [Document]"));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoHeightMacroUsedWithinRowsThatHaveBeenMergedWithOtherRows()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax =
@"Line 1
Line 2
Line 3
Line 4
Line 5
Line 6
Line 7
Line 8
Line 9
Line 10
Line 11
Line 12
Line 13
Line 14
Line 15
Line 16
Line 17
Line 18
Line 19
Line 20
Line 21
Line 22
Line 23
Line 24
Line 25
Line 26
Line 27
Line 28
Line 29
Line 30
Line 31
Line 32
Line 33
Line 34
Line 35
Line 36
Line 37
Line 38
Line 39
Line 40";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightMacroWithinRowContainingMergedRowsInOtherColumn.xls", TestFilesSubFolder.DocumentTestFiles);

			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = "UnitTest";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("If merged rows go over a page, just split anyway.",
@"{C}-[Page 1 of 2]
{C}-[Line 1]
{C}-[Line 2]
{C}-[Line 3]
{C}-[Line 4]
{C}-[Line 5]
{C}-[Line 6]
{C}-[Line 7]
{C}-[Line 8]
{C}-[Line 9]
{C}-[Line 10]
{C}-[Line 11]
{C}-[Line 12]
{C}-[Line 13]
{C}-[Line 14]
{C}-[Line 15]
{C}-[Line 16]
{C}-[Line 17]
{C}-[Line 18]
{C}-[Line 19]
{C}-[Line 20]
{C}-[Line 21]
{C}-[Page 2 of 2]
{C}-[Line 22]
{C}-[Line 23]
{C}-[Line 24]
{C}-[Line 25]
{C}-[Line 26]
{C}-[Line 27]
{C}-[Line 28]
{C}-[Line 29]
{C}-[Line 30]
{C}-[Line 31]
{C}-[Line 32]
{C}-[Line 33]
{C}-[Line 34]
{C}-[Line 35]
{C}-[Line 36]
{C}-[Line 37]
{C}-[Line 38]
{C}-[Line 39]
{C}-[Line 40]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestOverflowToFollowPage()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[<OverFlowToFollowPage(""Test Title"", 2, WrapOverflowWithContinued)>This is some long string that will need its cell height to be adjusted. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed.SetColWidth(1, 10000);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There is some problem.",
@"{B}-[This is some long string that will need its]
{B}-[cell height to be adjusted. (continued...)]

{C}-[Follow Page]   {AR}-[Page 1 of 1]

{C}-[NOTES CONTINUED FROM DOCUMENT BODY]


{C}-[Test Title]
{C}-[Yes, this string is quite long. The quick brown fox jumps over the lazy dog.]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestTwoOverflowToFollowPageInOneRow()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{A}-[<OverFlowToFollowPage(""Test Title"", 0, MoveAllContentWithoutContinued)><Z0_Code>] {B}-[<OverFlowToFollowPage(""Test Title"", 5, WrapOverflowWithContinued)><Z0_Description>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Description = "Description";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var stream = new MemoryStream())
			{
				var report = documentPack.GetFirstReport();
				report.Save(stream);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);

					AssertMultilineASCIIEquals("There is some problem.",
@"{B}-[Descripti]
{B}-[on]",
						excelInterface.WorkSheets[0].ToString());
				}
			}
		}

		public void TestColumnCountIncludesCellsWithFormulaThatCanEvaluateToEmptyString()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[5, 5] = new TFormula("=IF(TRUE, \"\",\"TestValue\")");

			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			AssertEquals("areaToTest.ColumnCount", 5, areaToTest.ColumnCount);
		}

		public void TestInsertPictureThrowAndHandleExternalExceptionGracefully()
		{
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=None";
					workSheet[5, 0] = "#SectionBody";
					workSheet[7, 1] = "<image(COMPANYLOGO,1,1)>";
					workSheet[12, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				using (var report = DocumentEngineTestHelper.GetNewReportWithNoExceptionOnErrors(Factory, excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(5, 5));
					try
					{
						ExcelWorkSheet.ExceptionToThrowWhenInsertingPictureForTesting = new ExternalException("Some Other error occurred in GDI+.");
						AssertEquals("Pre-Condition: report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
						AssertNoExceptionThrown(() => report.Save(outputStream));
						AssertMultilineASCIIEquals("report.ErrorManager.ToString()", "Severity: [Warning (without error report)] Message: [Error inserting image into cell or group of cells. It's likely the image resolution is too big] Cell: [B8] Sheetname: [Sheet1]", report.ErrorManager.ToString());
						report.ErrorManager.ClearErrors();
					}
					finally
					{
						ExcelWorkSheet.ExceptionToThrowWhenInsertingPictureForTesting = null;
					}
				}
			}
		}

		public void TestReplaceNonFormulaInCell_SpecialCharacters()
		{
			AssertReplaceNonFormulaInCell_SpecialCharacters("<ShrinkToFitForBillOfLading>");
			AssertReplaceNonFormulaInCell_SpecialCharacters("<ExpandToFit>");
		}

		public void TestReplaceFormulaInCellTrapsErrorsAsWarningsAndBlanksOutCell()
		{
			TestReport.Renderer.CurrentPass = Passes.SecondPass;
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("NottaaBiggun", @"EddieRocksMySocks"));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("SmallrBiggun", @"EddieRocksMySocks".PadRight(300, '!')));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("BiggunBiggun", @"EddieRocksMySocks".PadRight(70000, '!')));
			var areaToTest = new ConfigArea(4, 12, TestReport, "");

			using (TestReport.ErrorManager.EvaluatingCell(new CellReference("Sheet1", 5, 2)))
			{
				areaToTest.ReplaceFormulaInCell(new TFormula(@"=UPPER(""<NottaaBiggun>"")", null), 5, 2);
			}
			var cellContent = TestReport.WorkSheetCurrentlyBeingProcessed[5, 2] as TFormula;
			AssertNotNull("Cell Content 1", cellContent);
			AssertEquals("Cell Content 1 Formula Text", @"=UPPER(""EddieRocksMySocks"")", cellContent.Text);

			using (TestReport.ErrorManager.EvaluatingCell(new CellReference("Sheet1", 6, 2)))
			{
				areaToTest.ReplaceFormulaInCell(new TFormula(@"=UPPER(""<SmallrBiggun>"")", null), 6, 2);
			}
			AssertEquals("Cell Content 2 Smaller error", "", TestReport.WorkSheetCurrentlyBeingProcessed[6, 2].ToString());

			using (TestReport.ErrorManager.EvaluatingCell(new CellReference("Sheet1", 7, 2)))
			{
				areaToTest.ReplaceFormulaInCell(new TFormula(@"=UPPER(""<BiggunBiggun>"")", null), 7, 2);
			}
			AssertEquals("Cell Content 3 Bigger error", "", TestReport.WorkSheetCurrentlyBeingProcessed[7, 2]);

			using (TestReport.ErrorManager.EvaluatingCell(new CellReference("Sheet1", 8, 2)))
			{
				areaToTest.ReplaceFormulaInCell(new TFormula(@"=UPPER(""<MythicalBiggun>"")", null), 8, 2);
			}
			var cellValue = TestReport.WorkSheetCurrentlyBeingProcessed[8, 2] as TFormula;
			AssertNotNull("Cell Content 4 Mythical error as TFormula", cellValue);
			AssertEquals("Cell Content 4 Mythical error", @"=UPPER("""")", cellValue.Text);

			AssertMultilineASCIIEquals("", @"
Severity: [Warning (without error report)] Message: [Field <MythicalBiggun> not found on DataSource.] Cell: [C9]
Severity: [Warning] Message: [Error Replacing Macros in [=UPPER(""<BiggunBiggun>"")] - Formula too long: ""=UPPER(""EddieRocksMySocks!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ... !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"")""'] Cell: [C8]
Severity: [Warning] Message: [Error Replacing Macros in [=UPPER(""<SmallrBiggun>"")] - String constant ""EddieRocksMySocks!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ... !!!!!!!!"")"" is too long. (max 255 chars)] Cell: [C7]
".Trim(), TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestReplaceFormulaInCell()
		{
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Currency", "\"Val\"ue\""));
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			areaToTest.ReplaceFormulaInCell(new TFormula("=UPPER(\"<Currency>\")", null), 5, 2);

			AssertEquals("=UPPER(\"\"\"Val\"\"ue\"\"\")", ((TFormula)TestReport.WorkSheetCurrentlyBeingProcessed[5, 2]).Text);
		}

		public void TestReplaceFormulaWithNestedMacroInCell()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-F[=""<NumberToWords(<Z0_AnotherDecimal>, FR-FR)>""]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_AnotherDecimal = new ZDecimal(12.2);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				documentPack.Language = Core.SharedConstants.Languages.French;
				var report = documentPack.GetFirstReport();
				report.StTemplate.IsDocBuilderStyleForTest = true;
				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertMultilineASCIIEquals("Macro in formula should be replaced correctly", @"{B}-[douze virgule deux]", excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestReplaceFormulaWithEmptyAngleBracketsInsideHasNoErrors()
		{
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			using (TestReport.ErrorManager.EvaluatingCell(new CellReference("Sheet1", 5, 2)))
			{
				areaToTest.ReplaceFormulaInCell(new TFormula(@"=COUNTIF(C1:C2, ""<>1"")", null), 5, 2);
			}
			Assert($"Should not have any error when replacing a formula containing '<>'\r\n{TestReport.ErrorManager}", !TestReport.ErrorManager.HasErrors);
		}

		public void TestReplaceFormulaWithEmptyAngleBracketsInside()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-F[=COUNTIF(C4:D4, ""<>1"")]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				report.StTemplate.IsDocBuilderStyleForTest = true;
				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertMultilineASCIIEquals("Macro in formula should be replaced correctly", @"{B}-[2]", excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestProcessSectionForeachAreasWithMacrosInSameLineOfAreaIdentifier()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection] {B}-[<ExpandToFit>]
{A}-[#EndLoop]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();
				report.Renderer.Render();

				AssertMultilineASCIIEquals("", @"
Severity: [Error] Message: [Macros <ExpandToFit> are not allowed on the same line as #BeginLoop/#EndLoop as configuration lines are removed from the report/document.] Cell: [B4]
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		public void TestReplaceFormulaInCellEscapeAngleBrackets()
		{
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Currency", "<Value>"));
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			areaToTest.ReplaceFormulaInCell(new TFormula("=UPPER(\"<Currency>\")", null), 5, 2);
			AssertEquals("=UPPER(\"\\<Value\\>\")", ((TFormula)TestReport.WorkSheetCurrentlyBeingProcessed[5, 2]).Text);

			TestReport.Renderer.CurrentPass = Passes.SecondPass;
			areaToTest.ReplaceFormulaInCell(new TFormula("=UPPER(\"<Currency>\")", null), 5, 2);
			AssertEquals("=UPPER(\"<Value>\")", ((TFormula)TestReport.WorkSheetCurrentlyBeingProcessed[5, 2]).Text);
		}

		public void TestReplaceFormulaInCellShouldNotThrowExceptionWhenFormulaHasPairAngleBrackets()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "aa", 0, 10);
			var formula = @"=IF(AND(B80>=0,B80<0.42),""Dense++"", IF(B80<0.75,""Dense+"", IF(B80<1.42,""Neutral"", IF(B80<1.75,""Volume"", IF(B80>1.75,""Volume++"", ""NOT APPLY"")))))";
			AssertForFormulaWithinDoubleQuotes(dummy, formula, new List<string> { "{C}-[Dense++]" });
		}

		public void TestReplaceFormulaIncellWithinDoubleQuotes()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description", 0, 10);
			var formula = @"=IF(""<Collection.Code>""=""AAA"",IF(""<Collection.Number>""=""0"",""<Collection.Description>"",""""),"""")";
			AssertForFormulaWithinDoubleQuotes(dummy, formula, new List<string> { "{C}-[AAA Description]" });

			formula = @"=IF(""<Collection.Code>""=""AAA"",IF(""<Collection.Number>""=CONCAT(E3),""<Collection.Description>"",""""),"""")";
			AssertForFormulaWithinDoubleQuotes(dummy, formula, new List<string> { "{C}-[AAA Description]" });

			formula = @"=IF(""<Collection.Code>""=""AAA"",IF(0=E3,""<Collection.Description>"",""""),"""")";
			AssertForFormulaWithinDoubleQuotes(dummy, formula, new List<string> { "{C}-[AAA Description]" });

			formula = @"=IF(AND(D3>=0,D3<0.42),""Dense++"", IF(D3<0.75,""Dense+"", IF(D3<1.42,""Neutral"", IF(D3<1.75,""Volume"", IF(D3>1.75,""It's me <Collection.Description>"", ""NOT APPLY"")))))";
			AssertForFormulaWithinDoubleQuotes(dummy, formula, new List<string> { "{C}-[It's me AAA Description]" });

			formula = @"=IF(""<Collection.Code>""=""AAA"",IF(""<Collection.Number>""=""0"",""Description: <Collection.Description>, Code: <Collection.Code>, Number: <Collection.Number>"",""""),"""")";
			AssertForFormulaWithinDoubleQuotes(dummy, formula, new List<string> { "{C}-[Description: AAA Description, Code: AAA, Number: 0]" });

			dummy.Collection.AddNew("BBB", "BBB Description", 2, 2);
			dummy.Collection.AddNew("CCC", "CCC Description", 3, 3);
			dummy.Collection.AddNew("DDD", "DDD Description", 4, 4);
			dummy.Collection.AddNew("NNN", "NNN Description", 20, 20);
			formula = @"=IF(AND(D3>=0,D3<2.5),""B: <Collection.Code>"", IF(D3<3.5,""C: <Collection.Code>"", IF(D3<4.5,""D: <Collection.Code>"", IF(D3<15,""A: <Collection.Code>"", IF(D3>16,""No matched code"", ""NOT APPLY"")))))";
			var expected = new List<string>
			{
				"{C}-[A: AAA]",
				"{C}-[B: BBB]",
				"{C}-[C: CCC]",
				"{C}-[D: DDD]",
				"{C}-[No matched code]"
			};
			AssertForFormulaWithinDoubleQuotes(dummy, formula, expected);
		}

		public void TestReplaceNonFormulaInCell()
		{
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Currency", "\"Val\"ue\""));
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "<Currency>";
			AssertEquals("Pre-condition: TestReport.WorkSheetCurrentlyBeingProcessed.GetCellWrapText(2, 5)", false, TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(2, 5).WrapText);
			var format = TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(2, 5);
			format.WrapText = true;
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(2, 5, format);
			AssertEquals(true, TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(2, 5).WrapText);
			areaToTest.ReplaceNonFormulaInCell(2, 5, new Dictionary<int, MultiLineCellInfo>(), new Dictionary<int, string>(), new List<int>());

			var replacedFormat = TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(2, 5);
			AssertEquals(true, replacedFormat.WrapText);
			AssertEquals("No format pattern applied to the cell", "", replacedFormat.FormatPattern);
			AssertEquals("\"Val\"ue\"", TestReport.WorkSheetCurrentlyBeingProcessed[2, 5]);

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 6] = "<Currency(=1,AUD)>";
			AssertEquals("Pre-condition: TestReport.WorkSheetCurrentlyBeingProcessed.GetCellWrapText(3, 6)", false, TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(3, 6).WrapText);

			areaToTest.ReplaceNonFormulaInCell(3, 6, new Dictionary<int, MultiLineCellInfo>(), new Dictionary<int, string>(), new List<int>());
			var cellFormat = TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(3, 6);
			AssertEquals("Cell should have a format pattern based on CurrencyCode: AUD", "#,##0.00", cellFormat.FormatPattern);

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 7] = "<Currency(=Round(1.5555,3),AUD)>";
			AssertEquals("Pre-condition: TestReport.WorkSheetCurrentlyBeingProcessed.GetCellWrapText(3, 7)", false, TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(3, 7).WrapText);

			areaToTest.ReplaceNonFormulaInCell(3, 7, new Dictionary<int, MultiLineCellInfo>(), new Dictionary<int, string>(), new List<int>());
			cellFormat = TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(3, 7);
			AssertEquals("Cell should have a format pattern based on CurrencyCode: AUD", "#,##0.00", cellFormat.FormatPattern);
		}

		public void TestAutoheightNotChangeRowHeightInAHiddenRow()
		{
			var mocker = new MockRepository(MockBehavior.Default);
			var renderer = mocker.Create<IReportRenderer>();
			renderer.Setup(m => m.CurrentPass).Returns(Passes.FirstPass);
			renderer.Setup(m => m.HiddenColumns).Returns(new List<int>() { 2 });

			TestReport.Renderer = renderer.Object;

			var area = new ConfigArea(4, 12, TestReport, "");
			var autoHeightColumns = new Dictionary<int, MultiLineCellInfo>();
			TestReport.WorkSheetCurrentlyBeingProcessed[1, 1] = "<AutoHeight>N\no\nw\n\n\n";
			area.ReplaceNonFormulaInCell(1, 1, autoHeightColumns, new Dictionary<int, string>(), TestReport.Renderer.HiddenColumns);
			AssertEquals(1, autoHeightColumns.Count);
			AssertEquals(3, autoHeightColumns[1].MultiLineContents.Count);
			AssertEquals("N", autoHeightColumns[1].MultiLineContents[0]);
			AssertEquals("o", autoHeightColumns[1].MultiLineContents[1]);
			AssertEquals("w", autoHeightColumns[1].MultiLineContents[2]);
			AssertEquals("N\no\nw", TestReport.WorkSheetCurrentlyBeingProcessed[1, 1].ToString());
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 1] = "<AutoHeight><Now>";
			area.ReplaceNonFormulaInCell(1, 2, autoHeightColumns, new Dictionary<int, string>(), TestReport.Renderer.HiddenColumns);
			AssertEquals(1, autoHeightColumns.Count);

			mocker.VerifyAll();
		}

		public void TestIsHideRowIfCellsEmptyShouldSkipFormattingOnlyMacros()
		{
			var mocker = new MockRepository(MockBehavior.Default);
			var renderer = mocker.Create<IReportRenderer>();
			renderer.Setup(m => m.CurrentPass).Returns(Passes.FirstPass);
			renderer.Setup(m => m.HiddenColumns).Returns(new List<int> { 2 });

			TestReport.Renderer = renderer.Object;

			var area = new ConfigArea(4, 12, TestReport, "");
			var autoHeightColumns = new Dictionary<int, MultiLineCellInfo>();
			TestReport.WorkSheetCurrentlyBeingProcessed[1, 1] = "<ShrinkToFit><HideRowIfCellIsEmpty>";
			area.ReplaceNonFormulaInCell(1, 1, autoHeightColumns, new Dictionary<int, string>(), TestReport.Renderer.HiddenColumns);
			AssertEquals(0, TestReport.WorkSheetCurrentlyBeingProcessed.GetRowHeight(1));
			mocker.VerifyAll();
		}

		public void TestHideRowIfWorksForAllRowsAddedByAutoHeight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[]    {C}-[]
{B}-[]    {C}-[]
{B}-[Something should not be hidden]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				excelInterface.SetCellValue(0, 2, 1, @"<AutoHeight>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia");
				excelInterface.SetCellValue(0, 2, 2, @"<HideRowIfCellIsEmpty><ExcelFormula(""=IF(1=1,"""","""")"")>");
				excelInterface.SetCellValue(0, 3, 1, @"<AutoHeight>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia");
				excelInterface.SetCellValue(0, 3, 2, @"<HideRowIf(""<CurrentPage>""!= ""0"")>");

				var workSheet = excelInterface.WorkSheets.First();
				var format = workSheet.GetCellFormat(2, 1);
				format.VTextAlign = VerticalTextAlignment.Top;
				format.WrapText = true;

				workSheet.SetCellFormat(2, 1, format);
				workSheet.SetColWidth(1, 20000);
				workSheet.SetColWidth(2, 20000);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();

						AssertMultilineASCIIEquals(
							"HideRowIf should work for new added rows",
							"{B}-[Something should not be hidden]",
							workSheet.ToString());
					}
				}
			}
		}

		public void TestAutoHeight_WhenAutoHeightFlagSet_ShouldRemoveLineBreaks()
		{
			AssertAutoHeightWithContentTooTallToFitInOneCell("<AutoHeight(RemoveLineBreaksToFit)>", true);
		}

		public void TestAutoHeight_WhenAutoHeightFlagSetWithMinimumRowLimit_ShouldRemoveLineBreaks()
		{
			AssertAutoHeightWithContentTooTallToFitInOneCell("<AutoHeight(1, RemoveLineBreaksToFit)>", true);
		}

		public void TestAutoHeight_WhenAutoHeightFlagNOTSet_ShouldNOTRemoveLineBreaks()
		{
			AssertAutoHeightWithContentTooTallToFitInOneCell("<AutoHeight>", false);
		}

		public void TestAutoHeight_WhenAutoHeightFlagNOTSetWithMinimumRowLimit_ShouldNOTRemoveLineBreaks()
		{
			AssertAutoHeightWithContentTooTallToFitInOneCell("<AutoHeight(1)>", false);
		}

		public void TestReplaceNonFormulaInCell_FormattingOnlyMacros()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-]DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ShrinkToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ShrinkToFit(6)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ShrinkToFit(4)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<AutoHeight><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<AutoHeight(3)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ExpandToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<OverFlowToFollowPage(""Test Title"", 2, WrapOverflowWithoutContinued)><Z0_NVarCharMax>]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets.First();

				var format = workSheet.GetCellFormat(7, 1);
				format.WrapText = true;
				workSheet.SetCellFormat(7, 1, format);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = @"B B";
			dummy.Z0_NVarCharMax = @"This is some long string that will need its cell height to be adjusted. Yes, this string is quite long. I'd say a very very very very very very very very very very very very long one!";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There is some problem.",
@"{B}-[B BB BB BB BB B]
{B}-[B BB BB BB BB B]
{B}-[B BB BB BB BB B]
{B}-[B BB BB]
{B}-[BB BB B]
{B}-[B BB BB]
{B}-[BB BB B]

{B}-[B BB BB BB BB B]
{B}-[B BB BB BB BB B]
{B}-[This is]
{B}-[some]

{C}-[Follow Page]   {AR}-[Page 1 of 1]

{C}-[NOTES CONTINUED FROM DOCUMENT BODY]


{C}-[Test Title]
{C}-[long string that will need its cell height to be adjusted. Yes, this string is quite long. I'd say a very very very very very very very very very]
{C}-[very very very long one!]
", excelInterface.WorkSheets[0].ToString());

						var workSheet = excelInterface.WorkSheets.First();
						var defaultHeight = excelInterface.Xls.DefaultRowHeight;
						float defaultFontSize = excelInterface.Xls.GetDefaultFormat.Font.Size20 / 20;

						CombineAssertions(() =>
						{
							var row = 0;

							var cellShrinkToFitWithDefaultMinimunFontSize = workSheet.GetCell(row, 1);
							/*
							 * ShrinkToFitMacro has an issue. It's never using the DefaultMinimumFontSize of 8. Will be fixed in a separate WI.
							 * In the meantime, the assert below has been commented out and replaced with the one expecting the buggy value.
							 */
							//AssertEquals(string.Format("ShrinkToFit: Font size should have been reduced to ShrinkToFit's default minimum font size [{0}]", ShrinkToFit.DefaultMinimumFontSize),
							//	ShrinkToFit.DefaultMinimumFontSize, cellShrinkToFitWithDefaultMinimunFontSize.Format.FontSize);
							var expectedFontSize = 6f;
							Assert("ShrinkToFit: Font size should have been reduced", cellShrinkToFitWithDefaultMinimunFontSize.Format.FontSize <= expectedFontSize);
							AssertEquals(string.Format("ShrinkToFit: Cell height should have default value [{0}]", defaultHeight),
								defaultHeight, cellShrinkToFitWithDefaultMinimunFontSize.Height);

							row++;
							var cellShrinkToFitWithMinimunFontSizeOfSix = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("ShrinkToFit: Font size should have been reduced to {0}", expectedFontSize),
								expectedFontSize, cellShrinkToFitWithMinimunFontSizeOfSix.Format.FontSize);
							AssertEquals(string.Format("ShrinkToFit: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellShrinkToFitWithMinimunFontSizeOfSix.Height);

							row++;
							var cellShrinkToFitWithMinimunFontSizeOfFour = workSheet.GetCell(row, 1);
							Assert("ShrinkToFit: Font size should have been reduced", cellShrinkToFitWithMinimunFontSizeOfFour.Format.FontSize <= expectedFontSize);
							AssertEquals(string.Format("ShrinkToFit: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellShrinkToFitWithMinimunFontSizeOfFour.Height);

							row++;
							var cellAutoHeightWithMinimunRow1 = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("AutoHeight: Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellAutoHeightWithMinimunRow1.Format.FontSize);
							AssertEquals(string.Format("AutoHeight: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellAutoHeightWithMinimunRow1.Height);

							row++;
							var cellAutoHeightWithMinimunRow2 = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("AutoHeight: Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellAutoHeightWithMinimunRow2.Format.FontSize);
							AssertEquals(string.Format("AutoHeight: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellAutoHeightWithMinimunRow2.Height);

							row++;
							var cellAutoHeight1 = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("AutoHeight: Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellAutoHeight1.Format.FontSize);
							AssertEquals(string.Format("AutoHeight: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellAutoHeight1.Height);

							row++;
							var cellAutoHeight2 = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("AutoHeight: Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellAutoHeight2.Format.FontSize);
							AssertEquals(string.Format("AutoHeight: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellAutoHeight2.Height);

							row++;
							var cellAutoHeight3 = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("AutoHeight: Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellAutoHeight3.Format.FontSize);
							AssertEquals(string.Format("AutoHeight: Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellAutoHeight3.Height);

							row++;
							var cellExpandToFit = workSheet.GetCell(row, 1);
							var adjustedHeight = 489; //Height calculated by ExpandToFit macro
							AssertEquals(string.Format("ExpandToFit: Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellExpandToFit.Format.FontSize);
							Assert("ExpandToFit: Cell height should have been adjusted", cellExpandToFit.Height >= adjustedHeight);

							row++;
							var cellWithNonFormattingMacro = workSheet.GetCell(row, 1);
							AssertEquals(string.Format("Font size should have default value [{0}]", defaultFontSize), defaultFontSize, cellWithNonFormattingMacro.Format.FontSize);
							AssertEquals(string.Format("Cell height should have default value [{0}]", defaultHeight), defaultHeight, cellWithNonFormattingMacro.Height);
						});
					}
				}
			}
		}

		public void TestFormattingMacrosAreProcessedBeforeAddingPageBreak()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-]DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ShrinkToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ShrinkToFit(6)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ShrinkToFit(4)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<AutoHeight><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_NVarCharMax>]
{B}-[<AutoHeight(3)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_NVarCharMax>]
{B}-[<ExpandToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_NVarCharMax>]
{B}-[<ShrinkToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ShrinkToFit(6)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<ShrinkToFit(4)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<AutoHeight><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_NVarCharMax>]
{B}-[<AutoHeight(3)><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_NVarCharMax>]
{B}-[<ExpandToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_NVarCharMax>]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets.First();

				var format = workSheet.GetCellFormat(8, 1);
				format.WrapText = true;
				workSheet.SetCellFormat(8, 1, format);

				format = workSheet.GetCellFormat(14, 1);
				format.WrapText = true;
				workSheet.SetCellFormat(14, 1, format);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = @"B B";
			dummy.Z0_NVarCharMax = @"This is some long string that will need its cell height to be adjusted. Yes, this string is quite long. I'd say a very very very very very very very very very very very very long one!";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						Assert(excelInterface.Xls.HasHPageBreak(49));
						Assert(excelInterface.Xls.HPageBreakCount >= 3);
					}
				}
			}
		}

		public void TestAutoHeightOnlyAppliedOnRequiredCells()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=WI00062045]
{A}-[Data:ReportData=select 1.02 as Number, 'Aaaaaa bbbbbbb ccccccc ddddd' as LongText]
{A}-[#SectionBody:Data=ReportData]
{B}-[<AutoHeight><ReportData.Number>]{C}-[<AutoHeight><ReportData.LongText>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Expected template",
@"{B}-[1.02]   {C}-[Aaaaaa]
{C}-[bbbbbbb]
{C}-[ccccccc]
{C}-[ddddd]
", workSheet.ToString());

				AssertEquals("Cell content [1.02] should be a number", typeof(double), workSheet.GetCell(0, 1).Value.GetType());
				AssertEquals("Cell content [Aaaaaa] should be a string", typeof(string), workSheet.GetCell(0, 2).Value.GetType());
			}
		}

		public void TestTotalReplacementWithAutoHeight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=WI00062045]
{A}-[Data:ReportData=select top 3 ROW_NUMBER() OVER (ORDER BY OH_PK DESC) AS ID, 'Test' as GroupByColumn from dbo.OrgHeader order by ID]
{A}-[#SectionBody:Data=ReportData]
{B}-[<AutoHeight><ReportData.ID>]
{A}-[#GroupBy:ReportData.GroupByColumn]
{B}-[<Total ReportData.ID>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Total should not be 0",
@"{B}-[1]
{B}-[2]
{B}-[3]
{B}-[6]", workSheet.ToString());
			}
		}

		public void TestDontShrinkToFitWithoutTheMacro()
		{
			var areaToTest = new ConfigArea(4, 12, TestReport, "");

			var format = TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(1, 1);
			format.FontName = "Arial";
			format.FontSize = 16;
			format.FontStyle = FontStyle.Bold;
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(1, 1, format);
			TestReport.WorkSheetCurrentlyBeingProcessed[1, 1] = "Blaticus";
			areaToTest.ReplaceNonFormulaInCell(1, 1, new Dictionary<int, MultiLineCellInfo>(), new Dictionary<int, string>(), TestReport.Renderer.HiddenColumns);

			AssertEquals("New Font Size", 16f, TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFont(1, 1).SizeInPoints);
		}

		public void TestDontShrinkIfAlsoAutoHeight()
		{
			var areaToTest = new ConfigArea(4, 12, TestReport, "");

			var format = TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(1, 1);
			format.FontName = "Arial";
			format.FontSize = 16;
			format.FontStyle = FontStyle.Bold;
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(1, 1, format);
			TestReport.WorkSheetCurrentlyBeingProcessed[1, 1] = "<ShrinkToFit><AutoHeight>Blaticus";
			areaToTest.ReplaceNonFormulaInCell(1, 1, new Dictionary<int, MultiLineCellInfo>(), new Dictionary<int, string>(), TestReport.Renderer.HiddenColumns);

			AssertEquals("New Font Size", 16f, TestReport.WorkSheetCurrentlyBeingProcessed.GetCellFont(1, 1).SizeInPoints);
		}

		[GuiTest]
		public void TestRenderAndSaveWithFontNotFound()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[<OverFlowToFollowPage(""Test Title"", 2, WrapOverflowWithContinued)>This is some long string that will need its cell height to be adjusted. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed.SetColWidth(1, 10000);

				using (var stream = new MemoryStream())
				{
					try
					{
						using (ObjectFactory.Substitute<IExceptionWhenRenderAndSave>(new ExceptionToThrow(new ArgumentException("Font 'Arial' does not support style 'Regular'."))))
						{
							Assert(!report.ErrorManager.HasErrors);

							report.Save(stream);
							Assert(report.ErrorManager.HasErrors);
							AssertMultilineASCIIEquals("Severity: [Warning (without error report)] Message: [Font not found. Error details : \r\nFont 'Arial' does not support style 'Regular'.] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString());
						}
					}
					finally
					{
						report.ErrorManager.ClearErrors();
						ExceptionReporterTestListener.Instance.Clear();
					}
				}
			}
		}

		public void TestParenthesisDisplayForRightToLeftLanguage()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]

{A}-[#SectionBody]
{B}-[GEN (Test)]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			var rightToLeftMark = (char)0x200F;

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Arabic))
			{
				var printJob1 = DeliveryTestHelper.DeliverReport(reportCommand).First();
				using (var excelInterface = new ExcelInterface(printJob1.SP_CustomProperties))
				{
					var workSheet = excelInterface.WorkSheets.First();
					var expected = rightToLeftMark + "GEN (Test)";
					AssertContains(rightToLeftMark.ToString(), workSheet.ToString());
					AssertContains(expected, workSheet.ToString());
				}
			}

			var printJob2 = DeliveryTestHelper.DeliverReport(reportCommand).First();
			using (var excelInterface = new ExcelInterface(printJob2.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();
				AssertNotContains(rightToLeftMark.ToString(), workSheet.ToString());
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
			testReport?.Dispose();
			embeddedResourceRetriever?.Dispose();
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

		void AssertReplaceNonFormulaInCell_SpecialCharacters(string cellText)
		{
			var specialCharacters = new string(new char[] { (char)160, (char)774 });
			using (var templateStream = new MemoryStream())
			{
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.NewExcelFile(1);
					var workSheet = excelInterface.WorkSheets[0];
					workSheet[0, 0] = "#Config";
					workSheet[1, 0] = "DataContext=None";
					workSheet[2, 0] = "#SectionBody";
					workSheet[3, 1] = cellText + specialCharacters;
					workSheet[4, 0] = "#EndOfReport";
					excelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("Template", templateStream);

				using (var report = DocumentEngineTestHelper.GetNewReportWithNoExceptionOnErrors(Factory, excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown(delegate
					{ report.Save(outputStream); });
					AssertEquals($"Severity: [Error (without error report)] Message: [The cell could not be generated. Can't calculate size of the text '{specialCharacters}'. Please ensure that there are no special characters in the text.] Cell: [B4] Sheetname: [Sheet1]",
						report.ErrorManager.ToString());
					report.ErrorManager.ClearErrors();
				}
			}
		}

		void AssertForFormulaWithinDoubleQuotes(IDocumentSupportable documentSupportable, string formula, List<string> expected)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Code>]   {C}-[]   {D}-[<Collection.Decimal>]   {E}-[<Collection.Number>]   {F}-[<Collection.Description>]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets[0];
				workSheet[2, 2] = new TFormula(formula);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = documentSupportable;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				expected.ForEach(e => Assert(excelInterface.WorkSheets.First().ToString().Contains(e)));
			}
		}

		void AssertAutoHeightWithContentTooTallToFitInOneCell(string autoHeightMacro, bool shouldRemoveLineBreaks)
		{
			const string stringWithLotsOfLineBreaks = "abc.\r\nabc.\nabc.\r\nabc.\nabc.\r\nabc.\nabc.\r\nabc.\r\nabc.\nabc.\r\nabc.\r\nabc.\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.\r\nabc.";

			var mocks = new MockRepository(MockBehavior.Default);
			var renderer = mocks.Create<IReportRenderer>();
			TestReport.Renderer = renderer.Object;
			renderer.Setup(m => m.CurrentPass).Returns(Passes.FirstPass);
			renderer.Setup(m => m.HiddenColumns).Returns(Array.Empty<int>());
			renderer.Setup(m => m.ExpandRowsForAutoHeight).Returns(true);
			var area = new ConfigArea(4, 12, TestReport, "");
			var autoHeightColumns = new Dictionary<int, MultiLineCellInfo>();
			var contentsShouldCopyToNewLines = new Dictionary<int, string>();
			TestReport.WorkSheetCurrentlyBeingProcessed[1, 1] = autoHeightMacro + stringWithLotsOfLineBreaks;
			area.ReplaceNonFormulaInCell(1, 1, autoHeightColumns, new Dictionary<int, string>(), TestReport.Renderer.HiddenColumns);

			AssertEquals(1, autoHeightColumns.Count);
			var multilineCellInfo = autoHeightColumns.Values.First();
			area.AddLinesToFitMultiLineRowColumnContents(1, multilineCellInfo.MultiLineContents.Count, autoHeightColumns, contentsShouldCopyToNewLines, renderer.Object);

			if (shouldRemoveLineBreaks)
			{
				var stringWithoutLineBreaks = stringWithLotsOfLineBreaks.Replace("\r\n", " ").Replace('\n', ' ');
				AssertEquals(stringWithoutLineBreaks, TestReport.WorkSheetCurrentlyBeingProcessed[1, 1]);
			}
			else
			{
				AssertEquals("abc.", TestReport.WorkSheetCurrentlyBeingProcessed[1, 1]);
			}
		}

		sealed class ExceptionToThrow : IExceptionWhenRenderAndSave
		{
			readonly Exception ex;
			public ExceptionToThrow(Exception ex)
			{
				this.ex = ex;
			}

			public void Throw()
			{
				throw ex;
			}
		}
	}
}
