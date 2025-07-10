using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business.CustomValues;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportOutputTest : BaseOutputTest
	{
		public void TestBackPageOnlyPrintsOnceWhenDocumentHasPageFooterAndLastPageFooter()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#PageFooter]
{B}-[PageFooter]
{A}-[#LastPageFooter]
{B}-[LastPageFooter]
{A}-[#BackPage]
{B}-[BackPage]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There should only be one back page.",
@"{B}-[Page 1 of 1]


{B}-[LastPageFooter]
{B}-[BackPage]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadBadFormatExcelFileDoesReportErrorInsteadOfThrowException()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateForUnitTesting("BadExcelFormatTest.xls", TestFilesSubFolder.ReportTestFiles);

			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				Pack.Add(report);
				AssertEquals("Pre Condition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				AssertNoExceptionThrown(delegate { report.PrepareForRender(); });
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("Report.Errors", @"Severity: [Fatal Error (without error report)] Message: [You can only load templates saved as 'Excel 97-2003 Workbook' (.xls) or 'Excel 2007 Workbook' (.xlsx) format.] Cell: [N/A] Sheetname: [(unknown)]",
							report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentCurrencyIsAppliedCorrectlyAfterHiddenColumn()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateForUnitTesting("ApplyCurrencyAfterHiddenColumn.xls", TestFilesSubFolder.ReportTestFiles);

			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				Pack.Add(report);
				report.PrepareForRender();
				report.XlInterface.Xls.ActiveSheet = 1;

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];

						ExcelFile excel = workSheet.ParentExcelInterface.Xls;
						excel.ActiveSheet = 1;

						CombineAssertions(delegate
						{
							AssertEquals("", excel.GetStringFromCell(1, 3).Value);
							AssertEquals("100,000.00", excel.GetStringFromCell(1, 4).Value);
							AssertEquals("900,000.00", excel.GetStringFromCell(1, 5).Value);
							AssertEquals("1920", excel.GetStringFromCell(1, 6).Value);
							AssertEquals("6,521.00", excel.GetStringFromCell(1, 7).Value);
							AssertEquals("98,745.00", excel.GetStringFromCell(1, 8).Value);
							AssertEquals("300080", excel.GetStringFromCell(1, 9).Value);
							AssertEquals("500,689.00", excel.GetStringFromCell(1, 10).Value);
						});
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentCurrencyIsAppliedCorrectlyAfterRearrangeColumns()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ApplyCurrencyAfterHiddenColumn.xls", TestFilesSubFolder.ReportTestFiles);

			using (var report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				Pack.Add(report);
				report.PrepareForRender();

				var columnHeadings = report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Template"].ColumnHeadings;
				columnHeadings["Showing with Format 3"].CurrentPosition = 2;
				columnHeadings["Showing with No Format"].CurrentPosition = 3;
				columnHeadings["Showing with Format"].CurrentPosition = 4;
				columnHeadings["Showing with Format 2"].CurrentPosition = 5;
				columnHeadings["Showing with No Format 2"].CurrentPosition = 6;

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets[0];

						var excel = workSheet.ParentExcelInterface.Xls;
						excel.ActiveSheet = 1;

						CombineAssertions(delegate
						{
							AssertEquals("", excel.GetStringFromCell(1, 3).Value);
							AssertEquals("100,000.00", excel.GetStringFromCell(1, 4).Value);
							AssertEquals("900,000.00", excel.GetStringFromCell(1, 5).Value);
							AssertEquals("500,689.00", excel.GetStringFromCell(1, 6).Value);
							AssertEquals("1920", excel.GetStringFromCell(1, 7).Value);
							AssertEquals("6,521.00", excel.GetStringFromCell(1, 8).Value);
							AssertEquals("98,745.00", excel.GetStringFromCell(1, 9).Value);
							AssertEquals("300080", excel.GetStringFromCell(1, 10).Value);
						});
					}
				}
			}
		}

		public void TestTotalWorksWhenItConvertsFormulaeToARealValue()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=None";
					workSheet[4, 0] = "Data:Lines=select top 500 convert(varchar(3), ROW_NUMBER() over (order by RL_PK)) + convert(varchar(3), ROW_NUMBER() over (order by RL_PK)) + '.' + convert(varchar(3), ROW_NUMBER() over (order by RL_PK)) as MyValue from dbo.RefUnLoco";
					workSheet[5, 0] = "#SectionBody:Data=Lines";
					workSheet[6, 0] = "#GroupBy:Lines.MyValue";
					workSheet[7, 1] = "<Lines.MyValue>";
					workSheet[9, 0] = "#SectionFooter";
					workSheet[10, 1] = ">>>>>> GRAND TOTAL <<<<<<";
					workSheet[11, 1] = "<Total Lines.MyValue>";
					workSheet[12, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", expectedTotalWorksWhenItConvertsFormulaeToARealValue.Trim(), workSheet.ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionalColumnsWithGroupByOnValueFromHiddenColumn()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("OptionalColumnsWithGroupByOnValueFromHiddenColumn.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", ExpectedOptionalColumnsWithGroupByOnValueFromHiddenColumn.Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestCompanyLicenceInfoEvaluatesInternalsFirst()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=None";
					workSheet[4, 0] = "Data:Company=select GC_PK from dbo.GlbCompany where GC_Code = 'DEM'";
					workSheet[5, 0] = "#PageHeader";
					workSheet[6, 1] = "<CompanyLicenceInfo(<CompanyPK>).LicenceCompanyCode>";
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[EDI]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestSectionBodyDataCaseInSensitive()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=None";
					workSheet[4, 0] = "Data:Collection=select 1 as Z0_Number, 'One' as Z0_Code, 'One' as Z0_Description union select 2 as Z0_Number, 'Two', 'Two' union select 3 as Z0_Number, 'Three', 'Three'";
					workSheet[5, 0] = "#SectionBody:Data=Collection";
					workSheet[6, 1] = "<collection.Z0_Code>";
					workSheet[6, 2] = "<Collection.Z0_Description>";
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[One]   {C}-[One]
{B}-[Two]   {C}-[Two]
{B}-[Three]   {C}-[Three]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestCurrencyMacroWorksCorrectlyWithAutoHeight()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet.SetColWidth(1, 5600);
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=None";
					workSheet[4, 0] = "Data:Collection=select 1.10 as Z0_Number, 'One' as Z0_Code, 'One Man When To Mawn' as Z0_Description union select 2.53 as Z0_Number, 'Two', 'Two Man When To Mawn' union select 3.85 as Z0_Number, 'Three', 'Three Man When To Mawn' union select 4.85 as Z0_Number, 'Four', 'Four Man' union select 5.85 as Z0_Number, 'Five', 'Five Man'";
					workSheet[5, 0] = "#SectionBody:Data=Collection";
					workSheet[6, 1] = "<AutoHeight><Collection.Z0_Description>";
					workSheet[6, 2] = "<Collection.Z0_Code>";
					workSheet[6, 3] = "<Currency(<Collection.Z0_Number>,<CompanyCurrencyCode>)>";
					workSheet[6, 4] = "<Collection.Z0_Number>,<CompanyCurrencyCode>";
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						//xlInterface.SaveToFile(@"c:\temp\test.xls");
						//xlInterface.LoadExcelFile(@"c:\temp\test.xls");
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[One Man When To]   {C}-[One]   {D}-[1.1]   {E}-[1.10,AUD]
{B}-[Mawn]
{B}-[Two Man When To]   {C}-[Two]   {D}-[2.53]   {E}-[2.53,AUD]
{B}-[Mawn]
{B}-[Three Man When To]   {C}-[Three]   {D}-[3.85]   {E}-[3.85,AUD]
{B}-[Mawn]
{B}-[Four Man]   {C}-[Four]   {D}-[4.85]   {E}-[4.85,AUD]
{B}-[Five Man]   {C}-[Five]   {D}-[5.85]   {E}-[5.85,AUD]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSimpleReportWithBusinessObjectDatasource()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown(delegate { report.Save(outputStream); });
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSimpleReportWithBusinessObjectDatasource1()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown(delegate { report.Save(outputStream); });
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithDefaultMargins()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupBy.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 4", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 4", excelInterface.WorkSheets[0][60, 20]);
						AssertEquals("3 of 4", excelInterface.WorkSheets[0][120, 20]);
						AssertEquals("4 of 4", excelInterface.WorkSheets[0][180, 20]);
					}
				}
			}
		}

		public void TestPageBreaksAreInCorrectPositionWithDefaultMarginsAndGroupBy()
		{
			SetUpTempDB();
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
			using (var report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);

					AssertEquals("1 of 5", excelInterface.WorkSheets[0][3, 20]);
					AssertEquals("PageFooter", excelInterface.WorkSheets[0][52, 2].ToString().Trim());
					AssertEquals("PageFooter", excelInterface.WorkSheets[0][254, 2].ToString().Trim());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithZeroMargins()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByWithZeroMargins.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 4", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 4", excelInterface.WorkSheets[0][66, 20]);
						AssertEquals("3 of 4", excelInterface.WorkSheets[0][132, 20]);
						AssertEquals("4 of 4", excelInterface.WorkSheets[0][198, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithZeroMarginsInLetterSize()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByWithZeroMarginsInLetterSize.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 4", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 4", excelInterface.WorkSheets[0][63, 20]);
						AssertEquals("3 of 4", excelInterface.WorkSheets[0][126, 20]);
						AssertEquals("4 of 4", excelInterface.WorkSheets[0][189, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithZeroMarginsInCustomsLetterSize()
		{
			SetUpTempDB();
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByWithZeroMarginsInCustomsLetterSize.xls", TestFilesSubFolder.ReportTestFiles);

			using (var report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);

					AssertEquals("1 of 4", excelInterface.WorkSheets[0][3, 20]);
					AssertEquals("2 of 4", excelInterface.WorkSheets[0][60, 20]);
					AssertEquals("3 of 4", excelInterface.WorkSheets[0][120, 20]);
					AssertEquals("4 of 4", excelInterface.WorkSheets[0][180, 20]);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithShortMargins()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByLowMargin.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 4", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 4", excelInterface.WorkSheets[0][65, 20]);
						AssertEquals("3 of 4", excelInterface.WorkSheets[0][130, 20]);
						AssertEquals("4 of 4", excelInterface.WorkSheets[0][195, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithBigMargins()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByBigMargin.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 9", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 9", excelInterface.WorkSheets[0][37, 20]);
						AssertEquals("3 of 9", excelInterface.WorkSheets[0][74, 20]);
						AssertEquals("4 of 9", excelInterface.WorkSheets[0][111, 20]);
						AssertEquals("5 of 9", excelInterface.WorkSheets[0][148, 20]);
						AssertEquals("6 of 9", excelInterface.WorkSheets[0][185, 20]);
						AssertEquals("7 of 9", excelInterface.WorkSheets[0][222, 20]);
						AssertEquals("8 of 9", excelInterface.WorkSheets[0][259, 20]);
						AssertEquals("9 of 9", excelInterface.WorkSheets[0][297, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionLandscapeBigMargin()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByLandscapeBigMargin.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 8", excelInterface.WorkSheets[0][2, 20]);
						AssertEquals("2 of 8", excelInterface.WorkSheets[0][32, 20]);
						AssertEquals("3 of 8", excelInterface.WorkSheets[0][64, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionLandscapeDefaultMargin()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByLandscapeDefaultMargin.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 8", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 8", excelInterface.WorkSheets[0][35, 20]);
						AssertEquals("3 of 8", excelInterface.WorkSheets[0][75, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionLandscapeSmallMargin()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByLandscapeSmallMargin.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 7", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("2 of 7", excelInterface.WorkSheets[0][41, 20]);
						AssertEquals("3 of 7", excelInterface.WorkSheets[0][87, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoHeight()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("AutoHeight.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("CONTAINERS", excelInterface.WorkSheets[0][21, 2]);
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][27, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][28, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][29, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][30, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][31, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][32, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][33, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoHeightWithExpand()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightExpandRows.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("CONTAINERS", excelInterface.WorkSheets[0][21, 2]);
						AssertEquals("DESCRIPTION", excelInterface.WorkSheets[0][23, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][91, 2]);
					}
				}
			}
		}

		public void TestAutoHeightInPageHeader()
		{
			SetUpTempDB();
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.AutoHeightInPageHeader.xls", "AutoHeightInPageHeader.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightInPageHeader.xls", Path.GetFullPath(tempFileName));
			using (var report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);

					AssertEquals("PageFooter ", excelInterface.WorkSheets[0][54, 2]);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHiddenRowsNotToBeCalculatedInPageHeight()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("HiddenRowsNotToBeCalculatedInPageHeight.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("1 of 1", excelInterface.WorkSheets[0][2, 20]);
						AssertEquals(0, excelInterface.WorkSheets[0].GetRowHeight(13));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoHeightWithExtraLinesAtTheEnd()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightWithExtraLinesAtTheEnd.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("CONTAINERS", excelInterface.WorkSheets[0][21, 2]);
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][26, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][27, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][28, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][29, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][30, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][31, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][32, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConditionalAreas()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ConditionalAreas.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 5", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("CLIENT ORDER NUMBER(S)", excelInterface.WorkSheets[0][9, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageHeaderStartFromSecondPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageHeaderStartingFromSecondPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("SHIPPER", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("TestTemplate Header1                                 ", excelInterface.WorkSheets[0][54, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageHeaderStartFromSecondPage2()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageHeaderStartFromSecondPageFaulty.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("document header", excelInterface.WorkSheets[0][0, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreaksAreInCorrectPositionWithDefaultMarginsAndSmallRows()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupByAndSmallRows.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals(1993, excelInterface.WorkSheets[0].GetRowHeight(201));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBigDocumentHeaderAsOnlyArea()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BigDocumentHeaderAsOnlyArea.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals(false, report.ErrorManager.HasErrors);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooterShouldNotThrowException()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterShouldNotThrowException.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown(delegate { report.Save(outputStream); });
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooterPrintInTheLastPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterPrintInTheLastPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("PageFooter ", excelInterface.WorkSheets[0][254, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooterPrintInTheLastPage2()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterPrintInTheLastPage2.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals(1980, excelInterface.WorkSheets[0].GetRowHeight(50));
						AssertEquals("RECEIVED IN GOOD ORDER AND CONDITION", excelInterface.WorkSheets[0][52, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooterPrintInTheSecondLastPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterPrintInTheSecondLastPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][28, 1]);
						AssertEquals("Unit test Line number 19                ", excelInterface.WorkSheets[0][47, 1]);
						AssertEquals("Notes", excelInterface.WorkSheets[0][50, 1]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideRowIfCellIsEmpty()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("HideRowIfCellIsEmpty.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals(255, excelInterface.WorkSheets[0].GetRowHeight(30));
						AssertEquals(0, excelInterface.WorkSheets[0].GetRowHeight(53));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionHeaderWithBreakPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("SectionHeaderWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("PageFooter  ", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("PageFooter  ", excelInterface.WorkSheets[0][23, 2]);
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][24, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByWithBreakPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 6", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][23, 2]);
						AssertEquals("Unit test Line number 135               ", excelInterface.WorkSheets[0][64, 2]);
						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][69, 2]);
						AssertEquals("Unit test Line number 1                 ", excelInterface.WorkSheets[0][85, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][117, 2]);
						AssertEquals("Unit test Line number 2                 ", excelInterface.WorkSheets[0][132, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][164, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupTitleByWithBreakPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupTitleByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 6", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][26, 2]);
						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][27, 2]);
						AssertEquals("Unit test Line number 140               ", excelInterface.WorkSheets[0][69, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][88, 2]);
						AssertEquals("Unit test Line number 1                 ", excelInterface.WorkSheets[0][89, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][135, 2]);
						AssertEquals("Unit test Line number 2                 ", excelInterface.WorkSheets[0][136, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBigMarginWithBigRows()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BigMarginWithBigRows.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Load List Report", excelInterface.WorkSheets[0][9, 2]);
						AssertEquals("Child 0", excelInterface.WorkSheets[0][20, 2]);
						AssertEquals("Booking No.\n(Shipment No.)", excelInterface.WorkSheets[0][26, 2]);
						AssertEquals("Load List Report", excelInterface.WorkSheets[0][41, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCountInGroupBys()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("CountInGroupBys.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Count=1", excelInterface.WorkSheets[0][64, 20]);
						AssertEquals("Count=32", excelInterface.WorkSheets[0][101, 20]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageHeaderGettingDataFromFirstDataSection()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageHeaderGettingDataFromFirstDataSection.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][23, 2]);
						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][8, 2]);
						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][60, 2]);
						AssertEquals("Unit test Line number 135               ", excelInterface.WorkSheets[0][63, 4]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreakInMiddleOfSection()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageBreakInMiddleOfSection.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 2", excelInterface.WorkSheets[0][9, 34]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalInSeconSection()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TotalInSeconSection.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("=SUM(U17:U26)", excelInterface.WorkSheets[0].GetCellFormula(26, 6));
						AssertEquals("=SUM(U29:U38)", excelInterface.WorkSheets[0].GetCellFormula(38, 6));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExcelFormulaGetsReplacedWithText()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ExcelFormulaGetsReplacedWithText.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("=TRIM(C5)", excelInterface.WorkSheets[0].GetCellFormula(4, 10));
						AssertEquals("=TRIM(C4)", excelInterface.WorkSheets[0].GetCellFormula(3, 10));
						AssertEquals("=TRIM(C3)", excelInterface.WorkSheets[0].GetCellFormula(2, 10));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMacroInFieldResult()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("<Now>");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("<Now>", excelInterface.WorkSheets[0][0, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackPage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BackPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("1 of 5", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("this is the back of the page", excelInterface.WorkSheets[0][56, 4]);
						AssertEquals("this is the back of the page", excelInterface.WorkSheets[0][131, 4]);
						AssertEquals("this is the back of the page", excelInterface.WorkSheets[0][206, 4]);
						AssertEquals("this is the back of the page", excelInterface.WorkSheets[0][281, 4]);
						AssertEquals("this is the back of the page", excelInterface.WorkSheets[0][318, 4]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackPageOnePageOnly()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BackPageOnePageOnly.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 1", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("this is the back of the page", excelInterface.WorkSheets[0][43, 4]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSimpleReportWithZStringArray()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Main");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ZstringArrayDataSource.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Main", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("str1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("str2", excelInterface.WorkSheets[0][2, 2]);
						AssertEquals("str3", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("str4", excelInterface.WorkSheets[0][4, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConditionalColumns()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ConditionalColumns.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals(1974, excelInterface.WorkSheets[0].GetColWidth(4));
						AssertEquals(0, excelInterface.WorkSheets[0].GetColWidth(6));
						AssertEquals(0, excelInterface.WorkSheets[0].GetColWidth(8));
						AssertEquals(0, excelInterface.WorkSheets[0].GetColWidth(18));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmptyArea()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("EmptyArea.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][2, 2]);
						AssertEquals("Accounting Group 3                      ", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("Accounting Group 4                      ", excelInterface.WorkSheets[0][4, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooterMakingThePageSizeWrong()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Value");
			dummyWrapper.Children.Add(new DocumentWrapperForTesting("Child 3"));
			dummyWrapper.Children.Add(new DocumentWrapperForTesting("Child 4"));
			dummyWrapper.Children.Add(new DocumentWrapperForTesting("Child 5"));
			dummyWrapper.Children.Add(new DocumentWrapperForTesting("Child 6"));
			dummyWrapper.Children.Add(new DocumentWrapperForTesting("Child 7"));
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterMakingThePageSizeWrong.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("HBL:", excelInterface.WorkSheets[0][76, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreprocessorMacrosInConditionalAreas()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Main");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PreprocessorMacrosInConditionalAreas.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.DEP, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("WEIGHT", excelInterface.WorkSheets[0][18, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBigAutoHeightCellScrewsPaging()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting(@"Creating other note types
so that the page will go to second page.
Obviously this is to test if the second page is as expected.
If not tested and the client found out, we are in trouble.
If we are in trouble, we're not gonna be happy.
Obviously this is to test if the second page is as expected.
If not tested and the client found out, we are in trouble.
If we are in trouble, we're not gonna be happy.
Obviously this is to test if the second page is as expected.
If not tested and the client found out, we are in trouble.
If we are in trouble, we're not gonna be happy.
Then we'll whinge whinge whinge all day, like what happen just this morning.
So testing the second page is very important for us.
Testing blank line: here it comes....

There you go.");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BigAutoHeightCellScrewsPaging.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.DEP, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("If not tested and the client found out, we are in trouble.", excelInterface.WorkSheets[0][59, 8].ToString());
						AssertEquals("If we are in trouble, we're not gonna be happy.", excelInterface.WorkSheets[0][81, 8].ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoHeightLinesMakePageBreakWrong()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Main");
			for (int i = 3; i < 32; i++)
			{
				dummyWrapper.Children.Add(new DocumentWrapperForTesting("Child " + i));
			}
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightLinesMakePageBreakWrong.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.DEP, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 2", excelInterface.WorkSheets[0][6, 18]);
						AssertEquals("Total", excelInterface.WorkSheets[0][47, 16]);
						AssertEquals("2 of 2", excelInterface.WorkSheets[0][61, 18]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCellRefrencesAreOKInGroupTitleBys()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("CellRefrenceError.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("=U24", excelInterface.WorkSheets[0].GetCellFormula(23, 28));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSecondPageHeaderAfterSectionBody()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplateWithSecondPageHeaderAfterSectionBody.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown(delegate { report.Save(outputStream); });
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFirstPageFooter()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("FirstPageFooter.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount", 1600));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("First page footer", excelInterface.WorkSheets[0][53, 2]);
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][112, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][171, 2].ToString().Trim());
						AssertEquals("Last Page Footer", excelInterface.WorkSheets[0][229, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFirstPageFooterOnlyOnePage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("FirstPageFooter.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount", 30));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("OnlyOnePage footer", excelInterface.WorkSheets[0][18, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSeaImportDeliveryOrder()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Sea Import Delivery Order.Xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("FirstPageFooter", excelInterface.WorkSheets[0][66, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooters_OnlyPageFooterPresent_ShowPageFooterInTheLastPage_OnlyOnePage()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterInTheLastPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount", 50));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowPageFooter", "Yes"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowFirstPageFooter", "No"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowOnlyOnePageFooter", "No"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowLastPageFooter", "No"));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("PageFooter", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageFooters_OnlyPageFooterPresent_ShowPageFooterInTheLastPage_TwoPages()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterInTheLastPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount", 150));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowPageFooter", "Yes"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowFirstPageFooter", "No"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowOnlyOnePageFooter", "No"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShowLastPageFooter", "No"));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("PageFooter", excelInterface.WorkSheets[0][53, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][69, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNewStyleTemplate_NumberOfRowsToShow()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate_NumberOfRowsToShow.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("1 of 1", excelInterface.WorkSheets[0][3, 20]);
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
						AssertEquals("Unit test Line number 11", excelInterface.WorkSheets[0][34, 2].ToString().Trim());
						AssertEquals("section footer:", excelInterface.WorkSheets[0][35, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNewStyleTemplate_StartingRowToShow()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("StartingRowToShow.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Unit test Line number 3", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNewStyleTemplate_StartingMaximumMultiple()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("StartingMaximumMultiple.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Unit test Line number 3", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStickyGroupByArea()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed[41, 0] = "#GroupBy:Lines.AccountingGroupName:Sticky";
				report.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 140 * from ##LinesTest";
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("container1, 2, 4, 1120", excelInterface.WorkSheets[0][22, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][24, 2].ToString().Trim());

						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][36, 2].ToString().Trim());
						AssertEquals("Unit test Line number 1", excelInterface.WorkSheets[0][79, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStickyGroupTitleByArea()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed[41, 0] = "#GroupBy:Lines.AccountingGroupName:Sticky:GroupTitle";
				report.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 140 * from ##LinesTest";
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("container1, 2, 4, 1120", excelInterface.WorkSheets[0][22, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][24, 2].ToString().Trim());

						AssertEquals("Accounting Group 0", excelInterface.WorkSheets[0][36, 2].ToString().Trim());
						AssertEquals("Accounting Group 1", excelInterface.WorkSheets[0][79, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupTitleByAreaWithLongSticky()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByWithLongSticky.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 140 * from ##LinesTest";
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("container1, 2, 4, 1120", excelInterface.WorkSheets[0][22, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][24, 2].ToString().Trim());

						AssertEquals("Accounting Group 0", excelInterface.WorkSheets[0][107, 2].ToString().Trim());
						AssertEquals("Accounting Group 1", excelInterface.WorkSheets[0][221, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupTitleByAreaWithShortSticky()
		{
			SetUpTempDB();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByWithShortSticky.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 140 * from ##LinesTest";
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("container1, 2, 4, 1120", excelInterface.WorkSheets[0][22, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][24, 2].ToString().Trim());

						AssertEquals("Accounting Group 0", excelInterface.WorkSheets[0][37, 2].ToString().Trim());
						AssertEquals("Accounting Group 1", excelInterface.WorkSheets[0][81, 2].ToString().Trim());
					}
				}
			}
		}

		public void TestSimpleReportUsingDBFunctionGetCustomFieldByName()
		{
			using (Report.TemporarilyUseMainConnection())
			{
				var helper = new TemplateTestHelper();
				helper.AddWorkSheet(@"Report",
	@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT Z0_VarCharMax, Custom1.XV_Data as CustomField1, Custom2.XV_Data as CustomField2 FROM dbo.DummyBizo OUTER APPLY dbo.GetCustomFieldByName(Z0_PK, 'a custom bool') as Custom1 OUTER APPLY dbo.GetCustomFieldByName(Z0_PK, 'a custom string') as Custom2]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>, <ReportData.CustomField1>, <ReportData.CustomField2>]
{A}-[#EndOfReport]");

				var template = helper.CreateTemplate(Factory, "TestSimpleReportUsingDBFunctionGetCustomFieldByName");
				template.SO_DataContext = "UnitTest";

				var reportCommand = Factory.New<ReportCommand>();
				reportCommand.SU_MenuName = "TestSimpleReportUsingDBFunctionGetCustomFieldByName";
				var document = reportCommand.Documents.AddNew();
				document.SI_SU = reportCommand.PK;
				document.SI_SO = template.PK;

				var dummyA = Factory.New<DummyBusinessObject>();
				dummyA.Z0_VarCharMax = "I am the first dummy";

				var customFieldA1 = Factory.New<GenCustomAddOnValue>();
				customFieldA1.XV_Name = "a custom bool";
				customFieldA1.XV_Type = AddOnColumnDataType.Codes.Boolean;
				customFieldA1.XV_Data = "Y";
				customFieldA1.XV_IsRuleEnabled = true;
				customFieldA1.XV_ParentTableCode = "Z0";
				customFieldA1.XV_ParentID = dummyA.PK;

				var customFieldA2 = Factory.New<GenCustomAddOnValue>();
				customFieldA2.XV_Name = "a custom string";
				customFieldA2.XV_Type = AddOnColumnDataType.Codes.String;
				customFieldA2.XV_Data = "blah blah blah";
				customFieldA2.XV_IsRuleEnabled = true;
				customFieldA2.XV_ParentTableCode = "Z0";
				customFieldA2.XV_ParentID = dummyA.PK;

				var dummyB = Factory.New<DummyBusinessObject>();
				dummyB.Z0_VarCharMax = "And me the second one!";

				var customFieldB1 = Factory.New<GenCustomAddOnValue>();
				customFieldB1.XV_Name = "a custom bool";
				customFieldB1.XV_Type = AddOnColumnDataType.Codes.Boolean;
				customFieldB1.XV_Data = "N";
				customFieldB1.XV_IsRuleEnabled = true;
				customFieldB1.XV_ParentTableCode = "Z0";
				customFieldB1.XV_ParentID = dummyB.PK;

				var customFieldB2 = Factory.New<GenCustomAddOnValue>();
				customFieldB2.XV_Name = "a custom string";
				customFieldB2.XV_Type = AddOnColumnDataType.Codes.String;
				customFieldB2.XV_Data = "Oooooops";
				customFieldB2.XV_IsRuleEnabled = true;
				customFieldB2.XV_ParentTableCode = "Z0";
				customFieldB2.XV_ParentID = dummyB.PK;

				Factory.Save();

				var printJobs = DeliveryTestHelper.DeliverReport(reportCommand);
				using (var excelInterface = new ExcelInterface(printJobs.First().SP_CustomProperties))
				{
					AssertMultilineASCIIEquals("There should be two rows found.",
	@"{B}-[I am the first dummy, Y, blah blah blah]
{B}-[And me the second one!, N, Oooooops]",
						excelInterface.WorkSheets.First().ToString());
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			temporarilyUseMainConnection?.Dispose();
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		IDisposable temporarilyUseMainConnection;

		void SetUpTempDB()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		Report GetNewReportWithNoExceptionOnErrors(ExcelTemplate excelTemplate)
		{
			Report report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest);
			((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
			return report;
		}

		const string expectedTotalWorksWhenItConvertsFormulaeToARealValue = @"
{B}-[100100.100]

{B}-[1010.10]

{B}-[101101.101]

{B}-[102102.102]

{B}-[103103.103]

{B}-[104104.104]

{B}-[105105.105]

{B}-[106106.106]

{B}-[107107.107]

{B}-[108108.108]

{B}-[109109.109]

{B}-[11.1]

{B}-[110110.110]

{B}-[1111.11]

{B}-[111111.111]

{B}-[112112.112]

{B}-[113113.113]

{B}-[114114.114]

{B}-[115115.115]

{B}-[116116.116]

{B}-[117117.117]

{B}-[118118.118]

{B}-[119119.119]

{B}-[120120.120]

{B}-[121121.121]

{B}-[1212.12]

{B}-[122122.122]

{B}-[123123.123]

{B}-[124124.124]

{B}-[125125.125]

{B}-[126126.126]

{B}-[127127.127]

{B}-[128128.128]

{B}-[129129.129]

{B}-[130130.130]

{B}-[131131.131]

{B}-[1313.13]

{B}-[132132.132]

{B}-[133133.133]

{B}-[134134.134]

{B}-[135135.135]

{B}-[136136.136]

{B}-[137137.137]

{B}-[138138.138]

{B}-[139139.139]

{B}-[140140.140]

{B}-[141141.141]

{B}-[1414.14]

{B}-[142142.142]

{B}-[143143.143]

{B}-[144144.144]

{B}-[145145.145]

{B}-[146146.146]

{B}-[147147.147]

{B}-[148148.148]

{B}-[149149.149]

{B}-[150150.150]

{B}-[151151.151]

{B}-[1515.15]

{B}-[152152.152]

{B}-[153153.153]

{B}-[154154.154]

{B}-[155155.155]

{B}-[156156.156]

{B}-[157157.157]

{B}-[158158.158]

{B}-[159159.159]

{B}-[160160.160]

{B}-[161161.161]

{B}-[1616.16]

{B}-[162162.162]

{B}-[163163.163]

{B}-[164164.164]

{B}-[165165.165]

{B}-[166166.166]

{B}-[167167.167]

{B}-[168168.168]

{B}-[169169.169]

{B}-[170170.170]

{B}-[171171.171]

{B}-[1717.17]

{B}-[172172.172]

{B}-[173173.173]

{B}-[174174.174]

{B}-[175175.175]

{B}-[176176.176]

{B}-[177177.177]

{B}-[178178.178]

{B}-[179179.179]

{B}-[180180.180]

{B}-[181181.181]

{B}-[1818.18]

{B}-[182182.182]

{B}-[183183.183]

{B}-[184184.184]

{B}-[185185.185]

{B}-[186186.186]

{B}-[187187.187]

{B}-[188188.188]

{B}-[189189.189]

{B}-[190190.190]

{B}-[191191.191]

{B}-[1919.19]

{B}-[192192.192]

{B}-[193193.193]

{B}-[194194.194]

{B}-[195195.195]

{B}-[196196.196]

{B}-[197197.197]

{B}-[198198.198]

{B}-[199199.199]

{B}-[200200.200]

{B}-[201201.201]

{B}-[2020.20]

{B}-[202202.202]

{B}-[203203.203]

{B}-[204204.204]

{B}-[205205.205]

{B}-[206206.206]

{B}-[207207.207]

{B}-[208208.208]

{B}-[209209.209]

{B}-[210210.210]

{B}-[211211.211]

{B}-[2121.21]

{B}-[212212.212]

{B}-[213213.213]

{B}-[214214.214]

{B}-[215215.215]

{B}-[216216.216]

{B}-[217217.217]

{B}-[218218.218]

{B}-[219219.219]

{B}-[22.2]

{B}-[220220.220]

{B}-[221221.221]

{B}-[2222.22]

{B}-[222222.222]

{B}-[223223.223]

{B}-[224224.224]

{B}-[225225.225]

{B}-[226226.226]

{B}-[227227.227]

{B}-[228228.228]

{B}-[229229.229]

{B}-[230230.230]

{B}-[231231.231]

{B}-[232232.232]

{B}-[2323.23]

{B}-[233233.233]

{B}-[234234.234]

{B}-[235235.235]

{B}-[236236.236]

{B}-[237237.237]

{B}-[238238.238]

{B}-[239239.239]

{B}-[240240.240]

{B}-[241241.241]

{B}-[242242.242]

{B}-[2424.24]

{B}-[243243.243]

{B}-[244244.244]

{B}-[245245.245]

{B}-[246246.246]

{B}-[247247.247]

{B}-[248248.248]

{B}-[249249.249]

{B}-[250250.250]

{B}-[251251.251]

{B}-[252252.252]

{B}-[2525.25]

{B}-[253253.253]

{B}-[254254.254]

{B}-[255255.255]

{B}-[256256.256]

{B}-[257257.257]

{B}-[258258.258]

{B}-[259259.259]

{B}-[260260.260]

{B}-[261261.261]

{B}-[262262.262]

{B}-[2626.26]

{B}-[263263.263]

{B}-[264264.264]

{B}-[265265.265]

{B}-[266266.266]

{B}-[267267.267]

{B}-[268268.268]

{B}-[269269.269]

{B}-[270270.270]

{B}-[271271.271]

{B}-[272272.272]

{B}-[2727.27]

{B}-[273273.273]

{B}-[274274.274]

{B}-[275275.275]

{B}-[276276.276]

{B}-[277277.277]

{B}-[278278.278]

{B}-[279279.279]

{B}-[280280.280]

{B}-[281281.281]

{B}-[282282.282]

{B}-[2828.28]

{B}-[283283.283]

{B}-[284284.284]

{B}-[285285.285]

{B}-[286286.286]

{B}-[287287.287]

{B}-[288288.288]

{B}-[289289.289]

{B}-[290290.290]

{B}-[291291.291]

{B}-[292292.292]

{B}-[2929.29]

{B}-[293293.293]

{B}-[294294.294]

{B}-[295295.295]

{B}-[296296.296]

{B}-[297297.297]

{B}-[298298.298]

{B}-[299299.299]

{B}-[300300.300]

{B}-[301301.301]

{B}-[302302.302]

{B}-[3030.30]

{B}-[303303.303]

{B}-[304304.304]

{B}-[305305.305]

{B}-[306306.306]

{B}-[307307.307]

{B}-[308308.308]

{B}-[309309.309]

{B}-[310310.310]

{B}-[311311.311]

{B}-[312312.312]

{B}-[3131.31]

{B}-[313313.313]

{B}-[314314.314]

{B}-[315315.315]

{B}-[316316.316]

{B}-[317317.317]

{B}-[318318.318]

{B}-[319319.319]

{B}-[320320.320]

{B}-[321321.321]

{B}-[322322.322]

{B}-[3232.32]

{B}-[323323.323]

{B}-[324324.324]

{B}-[325325.325]

{B}-[326326.326]

{B}-[327327.327]

{B}-[328328.328]

{B}-[329329.329]

{B}-[33.3]

{B}-[330330.330]

{B}-[331331.331]

{B}-[332332.332]

{B}-[3333.33]

{B}-[333333.333]

{B}-[334334.334]

{B}-[335335.335]

{B}-[336336.336]

{B}-[337337.337]

{B}-[338338.338]

{B}-[339339.339]

{B}-[340340.340]

{B}-[341341.341]

{B}-[342342.342]

{B}-[343343.343]

{B}-[3434.34]

{B}-[344344.344]

{B}-[345345.345]

{B}-[346346.346]

{B}-[347347.347]

{B}-[348348.348]

{B}-[349349.349]

{B}-[350350.350]

{B}-[351351.351]

{B}-[352352.352]

{B}-[353353.353]

{B}-[3535.35]

{B}-[354354.354]

{B}-[355355.355]

{B}-[356356.356]

{B}-[357357.357]

{B}-[358358.358]

{B}-[359359.359]

{B}-[360360.360]

{B}-[361361.361]

{B}-[362362.362]

{B}-[363363.363]

{B}-[3636.36]

{B}-[364364.364]

{B}-[365365.365]

{B}-[366366.366]

{B}-[367367.367]

{B}-[368368.368]

{B}-[369369.369]

{B}-[370370.370]

{B}-[371371.371]

{B}-[372372.372]

{B}-[373373.373]

{B}-[3737.37]

{B}-[374374.374]

{B}-[375375.375]

{B}-[376376.376]

{B}-[377377.377]

{B}-[378378.378]

{B}-[379379.379]

{B}-[380380.380]

{B}-[381381.381]

{B}-[382382.382]

{B}-[383383.383]

{B}-[3838.38]

{B}-[384384.384]

{B}-[385385.385]

{B}-[386386.386]

{B}-[387387.387]

{B}-[388388.388]

{B}-[389389.389]

{B}-[390390.390]

{B}-[391391.391]

{B}-[392392.392]

{B}-[393393.393]

{B}-[3939.39]

{B}-[394394.394]

{B}-[395395.395]

{B}-[396396.396]

{B}-[397397.397]

{B}-[398398.398]

{B}-[399399.399]

{B}-[400400.400]

{B}-[401401.401]

{B}-[402402.402]

{B}-[403403.403]

{B}-[4040.40]

{B}-[404404.404]

{B}-[405405.405]

{B}-[406406.406]

{B}-[407407.407]

{B}-[408408.408]

{B}-[409409.409]

{B}-[410410.410]

{B}-[411411.411]

{B}-[412412.412]

{B}-[413413.413]

{B}-[4141.41]

{B}-[414414.414]

{B}-[415415.415]

{B}-[416416.416]

{B}-[417417.417]

{B}-[418418.418]

{B}-[419419.419]

{B}-[420420.420]

{B}-[421421.421]

{B}-[422422.422]

{B}-[423423.423]

{B}-[4242.42]

{B}-[424424.424]

{B}-[425425.425]

{B}-[426426.426]

{B}-[427427.427]

{B}-[428428.428]

{B}-[429429.429]

{B}-[430430.430]

{B}-[431431.431]

{B}-[432432.432]

{B}-[433433.433]

{B}-[4343.43]

{B}-[434434.434]

{B}-[435435.435]

{B}-[436436.436]

{B}-[437437.437]

{B}-[438438.438]

{B}-[439439.439]

{B}-[44.4]

{B}-[440440.440]

{B}-[441441.441]

{B}-[442442.442]

{B}-[443443.443]

{B}-[4444.44]

{B}-[444444.444]

{B}-[445445.445]

{B}-[446446.446]

{B}-[447447.447]

{B}-[448448.448]

{B}-[449449.449]

{B}-[450450.450]

{B}-[451451.451]

{B}-[452452.452]

{B}-[453453.453]

{B}-[454454.454]

{B}-[4545.45]

{B}-[455455.455]

{B}-[456456.456]

{B}-[457457.457]

{B}-[458458.458]

{B}-[459459.459]

{B}-[460460.460]

{B}-[461461.461]

{B}-[462462.462]

{B}-[463463.463]

{B}-[464464.464]

{B}-[4646.46]

{B}-[465465.465]

{B}-[466466.466]

{B}-[467467.467]

{B}-[468468.468]

{B}-[469469.469]

{B}-[470470.470]

{B}-[471471.471]

{B}-[472472.472]

{B}-[473473.473]

{B}-[474474.474]

{B}-[4747.47]

{B}-[475475.475]

{B}-[476476.476]

{B}-[477477.477]

{B}-[478478.478]

{B}-[479479.479]

{B}-[480480.480]

{B}-[481481.481]

{B}-[482482.482]

{B}-[483483.483]

{B}-[484484.484]

{B}-[4848.48]

{B}-[485485.485]

{B}-[486486.486]

{B}-[487487.487]

{B}-[488488.488]

{B}-[489489.489]

{B}-[490490.490]

{B}-[491491.491]

{B}-[492492.492]

{B}-[493493.493]

{B}-[494494.494]

{B}-[4949.49]

{B}-[495495.495]

{B}-[496496.496]

{B}-[497497.497]

{B}-[498498.498]

{B}-[499499.499]

{B}-[500500.500]

{B}-[5050.50]

{B}-[5151.51]

{B}-[5252.52]

{B}-[5353.53]

{B}-[5454.54]

{B}-[55.5]

{B}-[5555.55]

{B}-[5656.56]

{B}-[5757.57]

{B}-[5858.58]

{B}-[5959.59]

{B}-[6060.60]

{B}-[6161.61]

{B}-[6262.62]

{B}-[6363.63]

{B}-[6464.64]

{B}-[6565.65]

{B}-[66.6]

{B}-[6666.66]

{B}-[6767.67]

{B}-[6868.68]

{B}-[6969.69]

{B}-[7070.70]

{B}-[7171.71]

{B}-[7272.72]

{B}-[7373.73]

{B}-[7474.74]

{B}-[7575.75]

{B}-[7676.76]

{B}-[77.7]

{B}-[7777.77]

{B}-[7878.78]

{B}-[7979.79]

{B}-[8080.80]

{B}-[8181.81]

{B}-[8282.82]

{B}-[8383.83]

{B}-[8484.84]

{B}-[8585.85]

{B}-[8686.86]

{B}-[8787.87]

{B}-[88.8]

{B}-[8888.88]

{B}-[8989.89]

{B}-[9090.90]

{B}-[9191.91]

{B}-[9292.92]

{B}-[9393.93]

{B}-[9494.94]

{B}-[9595.95]

{B}-[9696.96]

{B}-[9797.97]

{B}-[9898.98]

{B}-[99.9]

{B}-[9999.99]

{B}-[>>>>>> GRAND TOTAL <<<<<<]
{B}-[120916373.85]
";

		const string ExpectedOptionalColumnsWithGroupByOnValueFromHiddenColumn = @"
{B}-[Header]
{B}-[GroupTitle]   {C}-[Accounting Group 0                      ]
{B}-[Unit test Line number 0                 ]
{B}-[Unit test Line number 5                 ]
{B}-[Unit test Line number 10                ]
{B}-[Unit test Line number 15                ]
{B}-[Unit test Line number 20                ]
{B}-[Unit test Line number 25                ]
{B}-[Unit test Line number 30                ]
{B}-[Unit test Line number 35                ]
{B}-[Unit test Line number 40                ]
{B}-[Unit test Line number 45                ]
{B}-[Unit test Line number 50                ]
{B}-[Unit test Line number 55                ]
{B}-[Unit test Line number 60                ]
{B}-[Unit test Line number 65                ]
{B}-[Unit test Line number 70                ]
{B}-[Unit test Line number 75                ]
{B}-[Unit test Line number 80                ]
{B}-[Unit test Line number 85                ]
{B}-[Unit test Line number 90                ]
{B}-[Unit test Line number 95                ]
{B}-[Unit test Line number 100               ]
{B}-[Unit test Line number 105               ]
{B}-[Unit test Line number 110               ]
{B}-[Unit test Line number 115               ]
{B}-[Unit test Line number 120               ]
{B}-[Unit test Line number 125               ]
{B}-[Unit test Line number 130               ]
{B}-[Unit test Line number 135               ]
{B}-[Unit test Line number 140               ]
{B}-[Unit test Line number 145               ]
{B}-[Unit test Line number 150               ]
{B}-[Unit test Line number 155               ]
{B}-[GroupTotal]   {C}-[Accounting Group 0                      ]

{B}-[GroupTitle]   {C}-[Accounting Group 1                      ]
{B}-[Unit test Line number 1                 ]
{B}-[Unit test Line number 6                 ]
{B}-[Unit test Line number 11                ]
{B}-[Unit test Line number 16                ]
{B}-[Unit test Line number 21                ]
{B}-[Unit test Line number 26                ]
{B}-[Unit test Line number 31                ]
{B}-[Unit test Line number 36                ]
{B}-[Unit test Line number 41                ]
{B}-[Unit test Line number 46                ]
{B}-[Unit test Line number 51                ]
{B}-[Unit test Line number 56                ]
{B}-[Unit test Line number 61                ]
{B}-[Unit test Line number 66                ]
{B}-[Unit test Line number 71                ]
{B}-[Unit test Line number 76                ]
{B}-[Unit test Line number 81                ]
{B}-[Unit test Line number 86                ]
{B}-[Unit test Line number 91                ]
{B}-[Unit test Line number 96                ]
{B}-[Unit test Line number 101               ]
{B}-[Unit test Line number 106               ]
{B}-[Unit test Line number 111               ]
{B}-[Unit test Line number 116               ]
{B}-[Unit test Line number 121               ]
{B}-[Unit test Line number 126               ]
{B}-[Unit test Line number 131               ]
{B}-[Unit test Line number 136               ]
{B}-[Unit test Line number 141               ]
{B}-[Unit test Line number 146               ]
{B}-[Unit test Line number 151               ]
{B}-[Unit test Line number 156               ]
{B}-[GroupTotal]   {C}-[Accounting Group 1                      ]

{B}-[GroupTitle]   {C}-[Accounting Group 2                      ]
{B}-[Unit test Line number 2                 ]
{B}-[Unit test Line number 7                 ]
{B}-[Unit test Line number 12                ]
{B}-[Unit test Line number 17                ]
{B}-[Unit test Line number 22                ]
{B}-[Unit test Line number 27                ]
{B}-[Unit test Line number 32                ]
{B}-[Unit test Line number 37                ]
{B}-[Unit test Line number 42                ]
{B}-[Unit test Line number 47                ]
{B}-[Unit test Line number 52                ]
{B}-[Unit test Line number 57                ]
{B}-[Unit test Line number 62                ]
{B}-[Unit test Line number 67                ]
{B}-[Unit test Line number 72                ]
{B}-[Unit test Line number 77                ]
{B}-[Unit test Line number 82                ]
{B}-[Unit test Line number 87                ]
{B}-[Unit test Line number 92                ]
{B}-[Unit test Line number 97                ]
{B}-[Unit test Line number 102               ]
{B}-[Unit test Line number 107               ]
{B}-[Unit test Line number 112               ]
{B}-[Unit test Line number 117               ]
{B}-[Unit test Line number 122               ]
{B}-[Unit test Line number 127               ]
{B}-[Unit test Line number 132               ]
{B}-[Unit test Line number 137               ]
{B}-[Unit test Line number 142               ]
{B}-[Unit test Line number 147               ]
{B}-[Unit test Line number 152               ]
{B}-[Unit test Line number 157               ]
{B}-[GroupTotal]   {C}-[Accounting Group 2                      ]

{B}-[GroupTitle]   {C}-[Accounting Group 3                      ]
{B}-[Unit test Line number 3                 ]
{B}-[Unit test Line number 8                 ]
{B}-[Unit test Line number 13                ]
{B}-[Unit test Line number 18                ]
{B}-[Unit test Line number 23                ]
{B}-[Unit test Line number 28                ]
{B}-[Unit test Line number 33                ]
{B}-[Unit test Line number 38                ]
{B}-[Unit test Line number 43                ]
{B}-[Unit test Line number 48                ]
{B}-[Unit test Line number 53                ]
{B}-[Unit test Line number 58                ]
{B}-[Unit test Line number 63                ]
{B}-[Unit test Line number 68                ]
{B}-[Unit test Line number 73                ]
{B}-[Unit test Line number 78                ]
{B}-[Unit test Line number 83                ]
{B}-[Unit test Line number 88                ]
{B}-[Unit test Line number 93                ]
{B}-[Unit test Line number 98                ]
{B}-[Unit test Line number 103               ]
{B}-[Unit test Line number 108               ]
{B}-[Unit test Line number 113               ]
{B}-[Unit test Line number 118               ]
{B}-[Unit test Line number 123               ]
{B}-[Unit test Line number 128               ]
{B}-[Unit test Line number 133               ]
{B}-[Unit test Line number 138               ]
{B}-[Unit test Line number 143               ]
{B}-[Unit test Line number 148               ]
{B}-[Unit test Line number 153               ]
{B}-[Unit test Line number 158               ]
{B}-[GroupTotal]   {C}-[Accounting Group 3                      ]

{B}-[GroupTitle]   {C}-[Accounting Group 4                      ]
{B}-[Unit test Line number 4                 ]
{B}-[Unit test Line number 9                 ]
{B}-[Unit test Line number 14                ]
{B}-[Unit test Line number 19                ]
{B}-[Unit test Line number 24                ]
{B}-[Unit test Line number 29                ]
{B}-[Unit test Line number 34                ]
{B}-[Unit test Line number 39                ]
{B}-[Unit test Line number 44                ]
{B}-[Unit test Line number 49                ]
{B}-[Unit test Line number 54                ]
{B}-[Unit test Line number 59                ]
{B}-[Unit test Line number 64                ]
{B}-[Unit test Line number 69                ]
{B}-[Unit test Line number 74                ]
{B}-[Unit test Line number 79                ]
{B}-[Unit test Line number 84                ]
{B}-[Unit test Line number 89                ]
{B}-[Unit test Line number 94                ]
{B}-[Unit test Line number 99                ]
{B}-[Unit test Line number 104               ]
{B}-[Unit test Line number 109               ]
{B}-[Unit test Line number 114               ]
{B}-[Unit test Line number 119               ]
{B}-[Unit test Line number 124               ]
{B}-[Unit test Line number 129               ]
{B}-[Unit test Line number 134               ]
{B}-[Unit test Line number 139               ]
{B}-[Unit test Line number 144               ]
{B}-[Unit test Line number 149               ]
{B}-[Unit test Line number 154               ]
{B}-[Unit test Line number 159               ]
{B}-[GroupTotal]   {C}-[Accounting Group 4                      ]
";
	}
}
