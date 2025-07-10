using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionBodyAreaTreeDataTest : TreeDataAreaAbstractTest
	{
		public void TestDataRowSourceFromSpecificPageRanges()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = "Shipment";
			var reportTempate = Factory.New<StmTemplateBase>();
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");

			reportTempate.SO_Template = template1;
			reportTempate.SO_DataContext = "GenericFreightJob";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document1";
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = reportTempate.PK;

			Factory.Save();

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			docSupportedBO.Collection.AddNew();
			docSupportedBO.Collection.AddNew();
			docSupportedBO.Collection.AddNew();
			using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
			{
				var report = documentPack.OfType<Report>().First();
				AssertEquals(true, report.CanSpecifyPageRanges);
				var sectionBodyArea = report.Analyser.Areas.OfType<SectionBodyArea>().First();
				AssertEquals(3, sectionBodyArea.RowCount);
				report.PageRangesSpecified = true;

				sectionBodyArea.SetDataSource(null);
				report.UpdateSpecifiedDataRowSource(new int[1] { 1 });
				AssertEquals(1, sectionBodyArea.RowCount);

				sectionBodyArea.SetDataSource(null);
				report.UpdateSpecifiedDataRowSource(new int[2] { 1, 2 });
				AssertEquals(2, sectionBodyArea.RowCount);

				sectionBodyArea.SetDataSource(null);
				report.UpdateSpecifiedDataRowSource(new int[3] { 1, 2, 3 });
				AssertEquals(3, sectionBodyArea.RowCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHeightInXlsShouldNotContainForeachAreaIdentifierRows()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DummyBusinessObjectAsDataSourceForNestedLoop.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report rpt = new Report(DocumentPack.EmptyPack, excelTemplate, System.Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				SectionBodyArea testArea = rpt.Analyser.Sections[0].SectionBody;

				var allRowsHeightInSectionBody = 0;
				for (int row = testArea.StartOfBody; row <= testArea.End; row++)
				{
					allRowsHeightInSectionBody += testArea.ParentReport.WorkSheetCurrentlyBeingProcessed.GetRowHeight(row);
				}

				AssertEquals("The height of All rows (including the #BeginLoop/#EndLoop identifier lines) should be 9768.", 9768, allRowsHeightInSectionBody);
				AssertEquals("The HeightInXls of SectionBodyArea should not contain the #BeginLoop/#EndLoop lines, and it should be less than 9768", 7128, testArea.HeightInXls);
			}
		}

		public void TestSectionForeachTags()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();
				var sectionBody = report.Analyser.Areas.OfType<SectionBodyArea>().First();
				AssertEquals(4, sectionBody.GetSectionForeachTags().Count);
				AssertEquals("(#BeginLoop:Data=DummyCollection, 3)", sectionBody.GetSectionForeachTags()[0].ToString());
				AssertEquals("(#BeginLoop:Data=DummyCollection, 4)", sectionBody.GetSectionForeachTags()[1].ToString());
				AssertEquals("(#EndLoop, 5)", sectionBody.GetSectionForeachTags()[2].ToString());
				AssertEquals("(#EndLoop, 6)", sectionBody.GetSectionForeachTags()[3].ToString());
			}
		}

		public void TestValidateSectionForeachTags()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, true);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, true);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, false);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, false);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, false);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, false);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			AssertValidateSectionForeachTags(templateContents, true);

			void AssertValidateSectionForeachTags(string template, bool expectedResult)
			{
				using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, template)))
				{
					report.PrepareForRender();
					var sectionBody = report.Analyser.Areas.OfType<SectionBodyArea>().First();
					AssertEquals(expectedResult, sectionBody.ValidateSectionForeachTags(sectionBody.GetSectionForeachTags()));
				}
			}
		}

		public void TestProcessSectionForeachAreas()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			var treeViewString =
@"SectionBodyArea:Start=2,End=6
  SectionForeachArea:Start=3,End=6
    SectionForeachArea:Start=4,End=5";
			AssertProcessSectionForeachAreas(templateContents, treeViewString, 2);

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndOfReport]";
			treeViewString = "Whatever";
			AssertProcessSectionForeachAreas(templateContents, treeViewString, -1, true);

			templateContents =
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			treeViewString =
@"SectionBodyArea:Start=2,End=12
  SectionForeachArea:Start=3,End=10
    SectionForeachArea:Start=4,End=7
      SectionForeachArea:Start=5,End=6
    SectionForeachArea:Start=8,End=9
  SectionForeachArea:Start=11,End=12";
			AssertProcessSectionForeachAreas(templateContents, treeViewString, 5);

			templateContents =
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[WhatEver]
{A}-[WhatEver]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[WhatEver]
{A}-[WhatEver]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[WhatEver]
{A}-[#EndLoop]
{A}-[WhatEver]
{A}-[#EndLoop]
{A}-[WhatEver]
{A}-[WhatEver]
{A}-[#EndOfReport]";
			treeViewString =
@"SectionBodyArea:Start=2,End=14
  SectionForeachArea:Start=5,End=12
    SectionForeachArea:Start=8,End=10";
			AssertProcessSectionForeachAreas(templateContents, treeViewString, 2);

			void AssertProcessSectionForeachAreas(string template, string resultTrueString, int allForeachCount, bool hasError = false)
			{
				using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, template)))
				{
					report.PrepareForRender();
					report.Renderer.Render();
					if (hasError)
					{
						AssertEquals("Severity: [Error] Message: [Error when processing SectionForeachArea, please check documentation of SectionForeachArea for how to use it.] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString());
						return;
					}

					var sectionBody = report.Analyser.Areas.OfType<SectionBodyArea>().First();
					AssertEquals(resultTrueString, sectionBody.ToTreeViewString());
					AssertEquals(allForeachCount, sectionBody.AllChildForeachAreas.Count);
				}
			}
		}

		public void TestHasCorrectParent()
		{
			var treeArea = GetNewAreaToTest() as TreeDataArea;
			AssertNull(treeArea.Parent);
		}

		public void TestRowRangesWithIndexHasRightData()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=WI00173823]
{A}-[Data:ReportData=SELECT 'aaaaaa bbbbbb cccccc dddddd' AS LongText UNION ALL SELECT 'eeeeee ffffff gggggg hhhhhh' AS LongText UNION ALL SELECT 'iiiiii jjjjjj kkkkkk llllll' AS LongText]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.LongText>]
{B}-[<ReportData.LongText>]
{A}-[#EndOfReport]");

			var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", template.SO_Template);
			using (var report = new Report(DocumentPack.EmptyPack, excelTemplate))
			{
				report.PrepareForRender();
				report.Renderer.Render();
				var rowRangesWithIndex = report.Analyser.Areas.OfType<SectionBodyArea>().First().DataRowIndexWithRowRanges;
				AssertEquals(3, rowRangesWithIndex.Count);
				AssertEquals("Start:5,End:6", $"Start:{rowRangesWithIndex[0].Start},End:{rowRangesWithIndex[0].End}");
				AssertEquals("Start:7,End:8", $"Start:{rowRangesWithIndex[1].Start},End:{rowRangesWithIndex[1].End}");
				AssertEquals("Start:9,End:10", $"Start:{rowRangesWithIndex[2].Start},End:{rowRangesWithIndex[2].End}");
			}
		}

		public void TestRowRangesWithIndexHasRightDataWithExpanding()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=WI00173823]
{A}-[Data:ReportData=SELECT 'aaaaaa bbbbbb cccccc dddddd' AS LongText UNION ALL SELECT 'eeeeee ffffff gggggg hhhhhh' AS LongText UNION ALL SELECT 'iiiiii jjjjjj kkkkkk llllll' AS LongText]
{A}-[#SectionBody:Data=ReportData]
{B}-[<AutoHeight><ReportData.LongText>]
{B}-[<AutoHeight><ReportData.LongText>]
{A}-[#EndOfReport]");

			var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", template.SO_Template);
			using (var report = new Report(DocumentPack.EmptyPack, excelTemplate))
			{
				report.PrepareForRender();
				report.Renderer.Render();
				var rowRangesWithIndex = report.Analyser.Areas.OfType<SectionBodyArea>().First().DataRowIndexWithRowRanges;
				AssertEquals(3, rowRangesWithIndex.Count);
				AssertEquals("Start:5,End:12", $"Start:{rowRangesWithIndex[0].Start},End:{rowRangesWithIndex[0].End}");
				AssertEquals("Start:13,End:20", $"Start:{rowRangesWithIndex[1].Start},End:{rowRangesWithIndex[1].End}");
				AssertEquals("Start:21,End:26", $"Start:{rowRangesWithIndex[2].Start},End:{rowRangesWithIndex[2].End}");
			}
		}

		public void TestDataValuesInSectionPageHeadersStillWorkWithSectionBodyThatHasVeryLargeCellUsingAutoHeight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PageStyle=Landscape]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionPageHeader]
{B}-[Page <Current Page> of <TotalPages> Code: <Collection.Z0_Code>]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Z0_Description>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			#region Test Data Setup
			var child1 = dummy.Collection.AddNew();
			child1.Z0_Code = "A";
			child1.Z0_Description =
@"1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26";

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Code = "B";
			child2.Z0_Description =
@"1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26";

			var child3 = dummy.Collection.AddNew();
			child3.Z0_Code = "C";
			child3.Z0_Description =
@"1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26";
			#endregion

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				var result = excelInterface.WorkSheets[0].ToString();

				// The each header should evaluate the code A, B & C correctly.
				Assert(result, result.Contains("{B}-[Page 1 of 3 Code: A]"));
				Assert(result, result.Contains("{B}-[Page 2 of 3 Code: B]"));
				Assert(result, result.Contains("{B}-[Page 3 of 3 Code: C]"));
			}
		}

		public void TestParametersAreSplitProperly()
		{
			var filter = @"(<E2_AddressSequence>!=0 || ""<E2_AddressType>""==""BKD"") && ""<Substitute(""<E2_CompanyName>"", """""", """")>""!=""""";
			var area = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=DocAddresses:FilterBy(" + filter + ")");
			AssertEquals("area.FilteredDataSourceBy", filter, area.FilteredDataSourceBy);
		}

		public void TestEmptyDataSourceWithFilterByDoesNotThrowNullReferenceException()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody:Data=RelatedDummy.Collection:FilterBy(\"<Z0_VarCharMax>\" != \"\")";
					workSheet[4, 1] = "<RelatedDummy.Collection.Z0_VarCharMax>";
					workSheet[5, 0] = "#SectionFooter";
					workSheet[6, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(delegate
					{ report.Save(outputStream); });
					AssertEquals("Report should not have errors", report.ErrorManager.HasErrors, false);
				}
			}
		}

		public void TestEmptyDataSourceWithFilterByAndNumberOfRowsToShowDoesNotThrowNullReferenceException()
		{
			var bo = Factory.New<DummyBusinessObject>();
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody:Data=RelatedDummy.Collection:NumberOfRowsToShow=12,FilterBy(\"<Z0_VarCharMax>\" != \"\")";
					workSheet[4, 1] = "<RelatedDummy.Collection.Z0_VarCharMax>";
					workSheet[5, 0] = "#SectionFooter";
					workSheet[6, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(delegate
					{ report.Save(outputStream); });
					AssertEquals("Report should not have errors", report.ErrorManager.HasErrors, false);
				}
			}
		}

		public void TestDataSourceWithFilterByAndNumberOfRowsToShowDoesNotThrowNullReferenceException()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PageStyle=Landscape]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection:NumberOfRowsToShow=5,FilterBy(""<Z0_VarCharMax>"" == """")""]
{B}-[<AutoHeight><Collection.Z0_Description>]
{B}-[<AutoHeight>Debra]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			var child = dummy.Collection.AddNew();
			child.Z0_Code = "Dex";
			child.Z0_Description = "Morgan";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				var expectedResult = @"{B}-[Morgan]
{B}-[Debra]

{B}-[Debra]

{B}-[Debra]

{B}-[Debra]

{B}-[Debra]";
				var result = excelInterface.WorkSheets[0].ToString();
				AssertEquals(expectedResult, result);
			}
		}

		public void TestSectionBodyAreaWithFilterByInTurkish()
		{
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.Turkish)))
			{
				var area = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=DocAddresses:FilterBy(<E2_AddressSequence>!=0)");
				AssertEquals("area.FilteredDataSourceBy", "<E2_AddressSequence>!=0", area.FilteredDataSourceBy);
			}
		}

		public void TestSectionBodyAreaWithOrderByInTurkish()
		{
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.Turkish)))
			{
				var area = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=DocAddresses:OrderBy(E2_AddressSequence)");
				AssertEquals("area.FilteredDataSourceBy", "E2_AddressSequence", area.sortByColumn);
			}
		}

		public void TestDataSourceErrorInSectionBodyAreaGetReportedGracefully()
		{
			var bo = Factory.New<DummyBusinessObject>();
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody:Data=AnUnknownField";
					workSheet[4, 1] = "<Collection.Z0_Number>";
					workSheet[5, 0] = "#SectionFooter";
					workSheet[6, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(() => report.Save(outputStream));
					AssertEquals("Report.Errors", @"
Severity: [Fatal Error (without error report)] Message: [The report could not be generated. Please ensure that the report is being run in the correct company. Error details:
The data source for this document (CargoWise.EntityFramework.Testing.DummyBusinessObject) does not contain a collection called [AnUnknownField]. Please check the template 'TemplateFromStream'. Typical syntax would be '#SectionBody:Data=AnUnknownField'.]
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				}
			}
		}

		public void TestDataSourceErrorInConfigurationSectionGetReportedGracefully()
		{
			var dummy = Factory.New<DummyBODocSupportable>();

			var customizedTemplate = Factory.New<StmTemplateBase>();
			customizedTemplate.SO_Name = "Customized Document Elements";

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[ContainsCustomisedSections=True]
{A}-[#ConfigurableSection:GEN, Test Invalid Field]
{A}-[#SectionBody:Data=BLah]
{B}-[<BLah.MarksAndNumbers>]
{A}-[#EndOfReport]", DocBuilderTemplateType.Customized);
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Invalid Field"));

			var ex = AssertExceptionThrown<DocumentEngineException>(() => DeliveryTestHelper.DeliverDocument(documentCommand));
			AssertContains("The data source for this document (Enterprise.DocumentEngine.Testing.DummyBODocSupportable) does not contain a collection called [BLah]. Please check all your Customized Document Elements templates for a section containing [BLah] as its data source. Typical syntax would be '#SectionBody:Data=BLah'.",
				ex.Message);
		}

		public void TestGetGroupsWithBizObjDataSource()
		{
			var topLevelDataSource = GetNewBizObjDataSourceForGetGroups();

			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), BizObjDataSourceForGetGroups))
			{
				report.PrepareForRender();

				var bodyArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				var groupByArea = report.Analyser.Sections[0].DataAreas[1] as GroupByArea;
				var groupedDataSources = bodyArea.GetGroups(groupByArea.GroupByColumns);

				AssertEquals("groupedDataSources.Length", 3, groupedDataSources.Length);

				var bodyAreaWithFormat = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				var groupByAreaWithFormat = report.Analyser.Sections[1].DataAreas[1] as GroupByArea;
				var groupedDataSourcesWithFormat = bodyAreaWithFormat.GetGroups(groupByAreaWithFormat.GroupByColumns);

				AssertEquals("groupedDataSourcesWithFormat.Length", 4, groupedDataSourcesWithFormat.Length);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetGroupingAndSortingWithBizObjDataSource()
		{
			DummyBusinessObject topLevelDataSource = GetNewBizObjDataSourceForGetGroups();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BizObjDataSourceForGroupingAndSorting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();

				SectionBodyArea bodyArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				GroupByArea groupByArea = report.Analyser.Sections[0].DataAreas[1] as GroupByArea;
				bodyArea.SortForBusinessObjectDataSourceIfNeeded();
				IDataRowSource[] groupedDataSources = bodyArea.GetGroups(groupByArea.GroupByColumns);

				AssertEquals("Z0_NVarCharMax", bodyArea.sortByColumn);
				AssertEquals("Desc", bodyArea.sortDirection);

				foreach (IDataRowSource source in groupedDataSources)
				{
					var bizOSrouce = source as BusinessObjectDataSource;

					for (int i = 0; i < bizOSrouce.GroupedOrFilteredCollection.Count - 1; i++)
					{
						Assert(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i][bodyArea.sortByColumn]).
							CompareTo(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i + 1][bodyArea.sortByColumn])) >= 0);
					}
				}

				AssertEquals("groupedDataSources.Length", 3, groupedDataSources.Length);

				SectionBodyArea bodyAreaWithFormat = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				GroupByArea groupByAreaWithFormat = report.Analyser.Sections[1].DataAreas[1] as GroupByArea;
				bodyAreaWithFormat.SortForBusinessObjectDataSourceIfNeeded();
				IDataRowSource[] groupedDataSourcesWithFormat = bodyAreaWithFormat.GetGroups(groupByAreaWithFormat.GroupByColumns);

				AssertEquals("groupedDataSourcesWithFormat.Length", 4, groupedDataSourcesWithFormat.Length);

				AssertEquals("Grouping and sorting by the same column.", "Z0_NVarCharMax", bodyAreaWithFormat.sortByColumn);
				AssertEquals("Asc", bodyAreaWithFormat.sortDirection);

				for (int i = 0; i < groupedDataSourcesWithFormat.Length - 1; i++)
				{
					var source1 = groupedDataSourcesWithFormat[i] as BusinessObjectDataSource;
					var source2 = groupedDataSourcesWithFormat[i + 1] as BusinessObjectDataSource;

					Assert(((IComparable)source1.GroupedOrFilteredCollection[0][bodyAreaWithFormat.sortByColumn]).
						CompareTo(((IComparable)source2.GroupedOrFilteredCollection[0][bodyAreaWithFormat.sortByColumn])) <= 0);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetGroupingAndSortingWithBizObjDataSourceAndClone()
		{
			var topLevelDataSource = GetNewBizObjDataSourceForGetGroups();

			var excelTemplate = new ExcelTemplateForUnitTesting("BizObjDataSourceForGroupingAndSorting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();

				var bodyAreaOriginal = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				var bodyAreaClone = bodyAreaOriginal.Clone(0) as SectionBodyArea;
				var groupByAreaOriginal = report.Analyser.Sections[0].DataAreas[1] as GroupByArea;
				var groupByArea = groupByAreaOriginal.Clone(0) as GroupByArea;
				bodyAreaClone.SortForBusinessObjectDataSourceIfNeeded();
				var groupedDataSources = bodyAreaClone.GetGroups(groupByArea.GroupByColumns);

				AssertEquals("Z0_NVarCharMax", bodyAreaClone.sortByColumn);
				AssertEquals("Desc", bodyAreaClone.sortDirection);

				foreach (var source in groupedDataSources)
				{
					var bizOSrouce = source as BusinessObjectDataSource;

					for (var i = 0; i < bizOSrouce.GroupedOrFilteredCollection.Count - 1; i++)
					{
						Assert(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i][bodyAreaClone.sortByColumn]).
							CompareTo(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i + 1][bodyAreaClone.sortByColumn])) >= 0);
					}
				}

				AssertEquals("groupedDataSources.Length", 3, groupedDataSources.Length);

				var bodyAreaWithFormatOriginal = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				var bodyAreaWithFormatClone = bodyAreaWithFormatOriginal.Clone(0) as SectionBodyArea;
				var groupByAreaWithFormatOriginal = report.Analyser.Sections[1].DataAreas[1] as GroupByArea;
				var groupByAreaWithFormat = groupByAreaWithFormatOriginal.Clone(0) as GroupByArea;
				bodyAreaWithFormatClone.SortForBusinessObjectDataSourceIfNeeded();
				var groupedDataSourcesWithFormat = bodyAreaWithFormatClone.GetGroups(groupByAreaWithFormat.GroupByColumns);

				AssertEquals("groupedDataSourcesWithFormat.Length", 4, groupedDataSourcesWithFormat.Length);

				AssertEquals("Grouping and sorting by the same column.", "Z0_NVarCharMax", bodyAreaWithFormatClone.sortByColumn);
				AssertEquals("Asc", bodyAreaWithFormatClone.sortDirection);

				for (var i = 0; i < groupedDataSourcesWithFormat.Length - 1; i++)
				{
					var source1 = groupedDataSourcesWithFormat[i] as BusinessObjectDataSource;
					var source2 = groupedDataSourcesWithFormat[i + 1] as BusinessObjectDataSource;

					Assert(((IComparable)source1.GroupedOrFilteredCollection[0][bodyAreaWithFormatClone.sortByColumn]).
						CompareTo(((IComparable)source2.GroupedOrFilteredCollection[0][bodyAreaWithFormatClone.sortByColumn])) <= 0);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortingWithBizObjDataSourceWithoutGrouping()
		{
			DummyBusinessObject topLevelDataSource = GetNewBizObjDataSourceForGetGroups();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BizObjDataSourceForGroupingAndSorting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();

				SectionBodyArea bodyArea = report.Analyser.Sections[2].DataAreas[0] as SectionBodyArea;
				bodyArea.SortForBusinessObjectDataSourceIfNeeded();

				AssertEquals("Z0_NVarCharMax", bodyArea.sortByColumn);
				AssertEquals("Desc", bodyArea.sortDirection);

				var bizOSrouce = bodyArea.DataRowSource as BusinessObjectDataSource;

				for (int i = 0; i < bizOSrouce.GroupedOrFilteredCollection.Count - 1; i++)
				{
					Assert(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i][bodyArea.sortByColumn]).
						CompareTo(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i + 1][bodyArea.sortByColumn])) >= 0);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortingWithBizObjDataSourceWithCloneArea()
		{
			var topLevelDataSource = GetNewBizObjDataSourceForGetGroups();

			var excelTemplate = new ExcelTemplateForUnitTesting("BizObjDataSourceForGroupingAndSorting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();

				var bodyArea = report.Analyser.Sections[2].DataAreas[0] as SectionBodyArea;

				var cloneArea = bodyArea.Clone(29) as SectionBodyArea;
				cloneArea.SortForBusinessObjectDataSourceIfNeeded();

				AssertEquals("Z0_NVarCharMax", cloneArea.sortByColumn);
				AssertEquals("Desc", cloneArea.sortDirection);

				var bizOSrouce = cloneArea.DataRowSource as BusinessObjectDataSource;

				for (var i = 0; i < bizOSrouce.GroupedOrFilteredCollection.Count - 1; i++)
				{
					Assert(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i][cloneArea.sortByColumn]).
						CompareTo(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i + 1][cloneArea.sortByColumn])) >= 0);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortingReportsErrorForInvalidColumn()
		{
			DummyBusinessObject topLevelDataSource = GetNewBizObjDataSourceForGetGroups();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BizObjDataSourceForGroupingAndSorting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();

				SectionBodyArea bodyArea = report.Analyser.Sections[3].DataAreas[0] as SectionBodyArea;

				AssertEquals("ViveLaResistance", bodyArea.sortByColumn);
				AssertEquals("Desc", bodyArea.sortDirection);

				AssertNoExceptionThrown(() => { bodyArea.SortForBusinessObjectDataSourceIfNeeded(); });

				AssertEquals("Reports invalid column", "Severity: [Warning (without error report)] Message: [Column [ViveLaResistance] specified in OrderBy is invalid.] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortingWithBizObjDataSourceWithNumberOfRowsToShow()
		{
			var topLevelDataSource = GetNewBizObjDataSourceForGetGroups();
			var originalRowCount = topLevelDataSource.Collection.Count;

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BizObjDataSourceForGroupingAndSorting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();

				var bodyArea = report.Analyser.Sections[4].DataAreas[0] as SectionBodyArea;
				AssertNoExceptionThrown(() => { bodyArea.SortForBusinessObjectDataSourceIfNeeded(); }); // NullReferenceException was originally thrown

				AssertEquals("Z0_NVarCharMax", bodyArea.sortByColumn);
				AssertEquals("ASC", bodyArea.sortDirection);

				var bizOSrouce = bodyArea.DataRowSource as BusinessObjectDataSource;

				AssertGreaterThan(bizOSrouce.GroupedOrFilteredCollection.Count, originalRowCount);
				AssertEquals("NumberOfRowsToShow=25", 25, bizOSrouce.GroupedOrFilteredCollection.Count);

				for (int i = 1; i < bizOSrouce.GroupedOrFilteredCollection.Count; i++)
				{
					if (i <= originalRowCount - 1) // 20 original rows in the DataRowSource
					{
						Assert(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i - 1][bodyArea.sortByColumn]).
						CompareTo(((IComparable)bizOSrouce.GroupedOrFilteredCollection[i][bodyArea.sortByColumn])) <= 0);
					}
					else // Null rows stay at the bottom even for ASC sort order
					{
						AssertNull("Row populated with null to provide NumberOfRowsToShow=25", bizOSrouce.GroupedOrFilteredCollection[i]);
					}
				}
			}
		}

		public void TestGetGroupsWithVisualiserDataSource()
		{
			var topLevelDataSource = GetNewBizObjDataSourceForGetGroups();
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), BizObjDataSourceForGetGroups))
			{
				report.PrepareForRender();

				var bodyArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				var groupByArea = report.Analyser.Sections[0].DataAreas[1] as GroupByArea;
				var groupedDataSources = bodyArea.GetGroups(groupByArea.GroupByColumns);

				AssertEquals("groupedDataSources.Length", 3, groupedDataSources.Length);

				var bodyAreaWithFormat = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				var groupByAreaWithFormat = report.Analyser.Sections[1].DataAreas[1] as GroupByArea;
				var groupedDataSourcesWithFormat = bodyAreaWithFormat.GetGroups(groupByAreaWithFormat.GroupByColumns);

				AssertEquals("groupedDataSourcesWithFormat.Length", 4, groupedDataSourcesWithFormat.Length);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetGroupsWithADODataSource()
		{
			DocumentPack pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ADODataSourceForGetGroups.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (Report report = new Report(pack, excelTemplate, System.Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				SectionBodyArea bodyArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				GroupByArea groupByArea = report.Analyser.Sections[0].DataAreas[1] as GroupByArea;
				IDataRowSource[] groupedDataSources = bodyArea.GetGroups(groupByArea.GroupByColumns);

				AssertEquals("groupedDataSources.Length", 5, groupedDataSources.Length);

				SectionBodyArea bodyAreaWithFormat = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				GroupByArea groupByAreaWithFormat = report.Analyser.Sections[1].DataAreas[1] as GroupByArea;
				IDataRowSource[] groupedDataSourcesWithFormat = bodyAreaWithFormat.GetGroups(groupByAreaWithFormat.GroupByColumns);

				AssertEquals("groupedDataSources.Length", 8, groupedDataSourcesWithFormat.Length);
			}
		}

		public override void TestVisualisationManagerHasAppropriateVisualisationRenderer()
		{
			var bodyArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody");
			AssertEquals("Precondition: bodyArea", typeof(SectionBodyArea), bodyArea.GetType());
			AssertEquals(typeof(RendererGeneral), AreaVisualisationManagerFactory.New(bodyArea).VisualisationRenderer.GetType());

			bodyArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Test");
			AssertEquals("Precondition: bodyArea", typeof(SectionBodyArea), bodyArea.GetType());
			AssertEquals(typeof(RendererSectionBody), AreaVisualisationManagerFactory.New(bodyArea).VisualisationRenderer.GetType());

			bodyArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=DummyCollection");
			AssertEquals("Precondition: bodyArea", typeof(SectionBodyArea), bodyArea.GetType());
			AssertEquals(typeof(RendererGeneral), AreaVisualisationManagerFactory.New(bodyArea).VisualisationRenderer.GetType());
		}

		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Test");
			Assert(createdArea is SectionBodyArea);
			AssertEquals("Test", ((SectionBodyArea)createdArea).TableName);
			Assert(!((SectionBodyArea)createdArea).ShouldApplyCustomSorting);
		}

		public void TestParameterShouldApplyCustomSortingInConstructor()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Test:CustomSorting");
			Assert(createdArea is SectionBodyArea);
			Assert(((SectionBodyArea)createdArea).ShouldApplyCustomSorting);

			AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Test:CustomSorting:OrderBy(What)");
			AssertContains("CustomSorting cannot be used together with OrderBy in the same SectionBody Area", TestReport.ErrorManager.ToString());
		}

		public void TestParameterShouldApplyCustomSortingIsAppliedToGroupBy()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PageStyle=Landscape]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection:CustomSorting]
{B}-[<Collection.Z0_Code>]
{A}-[#GroupBy:Collection.Z0_Number]
{B}-[<Collection.Z0_Number>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description", 2);
			dummy.Collection.AddNew("BBB", "BBB Description", 2);
			dummy.Collection.AddNew("CCC", "CCC Description", 1);
			dummy.Collection.AddNew("DDD", "DDD Description", 1);
			dummy.Collection.AddNew("EEE", "EEE Description", 3);
			dummy.Collection.AddNew("FFF", "FFF Description", 3);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				var expectedResult = @"{B}-[AAA]
{B}-[BBB]
{B}-[2]
{B}-[CCC]
{B}-[DDD]
{B}-[1]
{B}-[EEE]
{B}-[FFF]
{B}-[3]";
				var result = excelInterface.WorkSheets[0].ToString();
				AssertEquals(expectedResult, result);
			}
		}

		public void TestBadSectionData()
		{
			AssertExceptionThrown(typeof(DocumentEngineException), delegate
			{
				SectionBodyArea createdArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:lksadjhflkahjd:Data=Test");
			});
		}

		public void TestSplitAndReturnNewArea()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();
				var createdArea = rpt.Analyser.Sections[0].SectionBody;
				createdArea.ExpandForDataRows(9);
				Assert(!createdArea.IsClonedForPageBreaking);

				SectionBodyArea secondPart;
				using ((createdArea.ParentReport.Renderer as ReportRenderer).TrackProcessingPageBreaks())
				{
					secondPart = createdArea.SplitAndReturnNewArea(2000, new Page(), null) as SectionBodyArea;
				}

				Assert(secondPart.IsClonedForPageBreaking);
				AssertEquals(createdArea.DataRowIndexWithRowRanges, secondPart.DataRowIndexWithRowRanges);
				AssertEquals(7, createdArea.HeightInRows);
				AssertEquals(3, secondPart.HeightInRows);
			}
		}

		public void TestSplitArea_CheckIfErrorReportWorks()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();
				var createdArea = rpt.Analyser.Sections[0].SectionBody;

				Assert("Precondition: There should be no error", !createdArea.ParentReport.ErrorManager.HasErrors);

				createdArea.SplitAndReturnNewArea(2000, new Page(), null);

				Assert("There should be one error", createdArea.ParentReport.ErrorManager.HasErrors);
				AssertEquals("Error Message", "Severity: [Error] Message: [If this issue can be reproduced, please raise an incident and provide essential information for the replication.]", createdArea.ParentReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			}
		}

		public void TestSplitAndReturnNewAreaStickyWithNoOtherDataAreasInPage()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed[39, 0] = "#SectionBody:Data=Lines:Sticky";
				rpt.PrepareForRender();
				var createdArea = rpt.Analyser.Sections[0].SectionBody;
				createdArea.ExpandForDataRows(9);
				var secondPart = createdArea.SplitAndReturnNewArea(2000, new Page(), null) as SectionBodyArea;
				AssertEquals(7, createdArea.HeightInRows);
				AssertEquals(3, secondPart.HeightInRows);
			}
		}

		public void TestSplitAndReturnNewAreaStickyWithOtherDataAreasInPage()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed[39, 0] = "#SectionBody:Data=Lines:Sticky";
				rpt.PrepareForRender();
				var createdArea = rpt.Analyser.Sections[0].SectionBody;
				createdArea.ExpandForDataRows(9);
				var fakePage = new Page();
				var fakeSectionBodyArea = AreaFactory.InstantiateArea(20, 21, TestReport, "#SectionBody:Data=Test");
				fakePage.Areas.Add(fakeSectionBodyArea);
				var secondPart = createdArea.SplitAndReturnNewArea(2000, fakePage, null) as SectionBodyArea;
				AssertEquals(10, createdArea.HeightInRows);
				AssertNull(secondPart);
			}
		}

		public void TestSplitAndReturnNewAreaStickyWithDocumentHeaderInPage()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed[39, 0] = "#SectionBody:Data=Lines:Sticky";
				rpt.PrepareForRender();
				var createdArea = rpt.Analyser.Sections[0].SectionBody;
				createdArea.ExpandForDataRows(9);
				var fakePage = new Page();
				var fakeSectionDocumentHeader = AreaFactory.InstantiateArea(20, 21, TestReport, "#DocumentHeader");
				fakePage.Areas.Add(fakeSectionDocumentHeader);
				var secondPart = createdArea.SplitAndReturnNewArea(2000, fakePage, null) as SectionBodyArea;
				AssertEquals(10, createdArea.HeightInRows);
				AssertNull(secondPart);
			}
		}

		public void TestInstantiateAreaWithRowCountAMultipleOf()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, RowCountAMultipleOf = 6");
			createdArea.SetDataSource(dS);
			AssertEquals(24, createdArea.RowCount);
		}

		public void TestInstantiateAreaWithRowCountAMultipleOf2()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, RowCountAMultipleOf = 20");
			createdArea.SetDataSource(dS);
			AssertEquals(20, createdArea.RowCount);
		}

		public void TestInstantiateAreaWithRowCountAMultipleOfWithZreoRows()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, RowCountAMultipleOf = 20");
			var dS2 = dS.GetFirstNRows(0);
			createdArea.SetDataSource(dS2);
			AssertEquals(0, createdArea.RowCount);
		}

		public void TestNumberOfRowsToShow()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, NumberOfRowsToShow = 12");
			createdArea.SetDataSource(dS);
			AssertEquals(12, createdArea.RowCount);
		}

		public void TestNumberOfRowsToShow2()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines,NumberOfRowsToShow=2");
			createdArea.SetDataSource(dS);
			AssertEquals(2, createdArea.RowCount);
		}

		public void TestInvalidNumberOfRowsToShow()
		{
			var dS = CreateTestDataSource();
			AssertExceptionThrown<DocumentEngineException>(() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, NumberOfRowsToShow = "));
		}

		public void TestInvalidMaximumNumberOfRowsToShow()
		{
			var dS = CreateTestDataSource();
			AssertExceptionThrown<DocumentEngineException>(() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, MaximumNumberOfRowsToShow = "));
		}

		public void TestInvalidRowCountAMultipleOf()
		{
			var dS = CreateTestDataSource();
			AssertExceptionThrown<DocumentEngineException>(() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, RowCountAMultipleOf = "));
		}

		public void TestInvalidStartingRowToShow()
		{
			var dS = CreateTestDataSource();
			AssertExceptionThrown<DocumentEngineException>(() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines, StartingRowToShow = "));
		}

		public void TestMaxNumberOfRowsToShow()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines,MaximumNumberOfRowsToShow=2");
			createdArea.SetDataSource(dS);
			AssertEquals(2, createdArea.RowCount);
		}

		public void TestMaxNumberOfRowsToShow2()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines,MaximumNumberOfRowsToShow=22");
			createdArea.SetDataSource(dS);
			AssertEquals(20, createdArea.RowCount);
		}

		public void TestNoNumberOfRowsToShow()
		{
			var dS = CreateTestDataSource();
			var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Lines");
			createdArea.SetDataSource(dS);
			AssertEquals(20, createdArea.RowCount);
		}

		public void TestStartingRowToShow()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, rpt, "#SectionBody:Data=" + TestData.LinesTempTableName + ",StartingRowToShow=3");
				var dS = rpt.DataProvider.GetDataRowSource("Lines");
				createdArea.SetDataSource(dS);
				AssertEquals(158, createdArea.RowCount);

				var obj = createdArea.GetColumnValue(0, "Lines.Description");
				AssertEquals("Unit test Line number 2                 ", obj);
			}
		}

		public void TestMaximumMultipleNotMultiple()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, rpt, "#SectionBody:Data=" + TestData.LinesTempTableName + ",RowCountAMultipleof=5,MaximumNumberOfRowsToShow=31");
				var dS = rpt.DataProvider.GetDataRowSource("Lines");
				createdArea.SetDataSource(dS);

				AssertEquals("Rpt.Errors", "Severity: [Error] Message: [MaximumNumberOfRowsToShow must be a multiple of RowCountAMultipleOf] Cell: [A1]", rpt.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		public void TestStartGreaterThanMaximum()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, rpt, "#SectionBody:Data=" + TestData.LinesTempTableName + ",StartingRowToShow=4,MaximumNumberOfRowsToShow=3");
				var dS = rpt.DataProvider.GetDataRowSource("Lines");
				createdArea.SetDataSource(dS);

				AssertEquals("Rpt.Errors", "Severity: [Error] Message: [The StartingRowToShow can not be more than MaximumNumberOfRowsToShow] Cell: [A1]", rpt.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		public void TestStartingGreaterThanMultiple()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, rpt, "#SectionBody:Data=" + TestData.LinesTempTableName + ",StartingRowToShow=10,RowCountAMultipleof=5");
				var dS = rpt.DataProvider.GetDataRowSource("Lines");
				createdArea.SetDataSource(dS);
				AssertEquals(0, createdArea.RowCount);
			}
		}

		public void TestStartingMaximumMultiple()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, rpt, "#SectionBody:Data=" + TestData.LinesTempTableName + ",StartingRowToShow=4,RowCountAMultipleof=5,MaximumNumberOfRowsToShow=30");
				var dS = rpt.DataProvider.GetDataRowSource("Lines");
				createdArea.SetDataSource(dS);
				AssertEquals(30, createdArea.RowCount);

				var obj = createdArea.GetColumnValue(0, "Lines.Description");
				AssertEquals("Unit test Line number 3                 ", obj);
			}
		}

		public void TestSplittedWillHaveAHeight()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();
				var testArea = rpt.Analyser.Sections[0].SectionBody;

				AssertEquals(255, testArea.HeightInXls);

				testArea.SetDataSource(new ReportDataSource(new DataTable("Empty")));
				AssertEquals(255, testArea.HeightInXls);
			}
		}

		public void TestGetRowRanges()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = rpt.Analyser.Sections[0].SectionBody;

				var expansionSize = 6;
				createdArea.ExpandForDataRows(expansionSize);

				AssertEquals(1, createdArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description")[0].Start);//Excel formula is 1 based
				AssertEquals(createdArea.StartOfBody + 1 + expansionSize, createdArea.GetRowRanges(0, "Lines.Description")[0].End);
			}
		}

		public void TestGetRowRangesWithMaxDBRows()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = rpt.Analyser.Sections[0].SectionBody;

				var expansionSize = 6;
				createdArea.ExpandForDataRows(expansionSize);

				AssertEquals(1, createdArea.GetRowRanges(0, "Lines.Description", 2).Count);
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description", 2)[0].Start);//Excel formula is 1 based
				AssertEquals(createdArea.StartOfBody + 1 + 1, createdArea.GetRowRanges(0, "Lines.Description", 2)[0].End);
			}
		}

		public void TestGetRowRangesWithMoreThanOneXlsRowForOneDBRow()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = (SectionBodyArea)AreaFactory.InstantiateArea(rpt.Analyser.Sections[0].SectionBody.StartingRow, rpt.Analyser.Sections[0].SectionBody.End + 2, rpt, "#SectionBody:Data=" + TestData.LinesTempTableName);

				var expansionSize = 5;
				createdArea.ExpandForDataRows(expansionSize);

				AssertEquals(expansionSize + 1, createdArea.GetRowRanges(0, "Lines.Description").Count);

				for (var i = 0; i < expansionSize; i++)
				{
					AssertEquals(createdArea.StartOfBody + 1 + 3 * i, createdArea.GetRowRanges(0, "Lines.Description")[i].Start);//Excel formula is 1 based
					AssertEquals(createdArea.StartOfBody + 1 + 3 * i, createdArea.GetRowRanges(0, "Lines.Description")[i].End);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleDataSourceWhereClause()
		{
			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("MultipleDataSectionsWithFilters.xls", TestFilesSubFolder.ReportTestFiles)))
			{
				report.PrepareForRender();
				AssertEquals("there should only be one section", 2, report.Analyser.Sections.Count);
				AssertEquals("there should be 1 data areas in first section ", 1, report.Analyser.Sections[0].DataAreas.Count);
				var firstDataArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				AssertNotNull("First data area should be of type DataArea", firstDataArea);
				AssertEquals("First data area table name should be test1", "test1", firstDataArea.TableName);
				var source1 = firstDataArea.DataRowSource;
				Assert("No filtering yet applied should be serveral records", source1.RowCount > 1);

				AssertEquals("there should be 1 data areas in first section ", 1, report.Analyser.Sections[1].DataAreas.Count);
				var secondDataArea = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				AssertNotNull("First data area should be of type DataArea", secondDataArea);
				AssertEquals("First data area table name should be test2", "test2", secondDataArea.TableName);
				var source2 = secondDataArea.DataRowSource;
				Assert("No filtering yet applied should be serveral records", source2.RowCount > 1);
			}

			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("MultipleDataSectionsWithFilters.xls", TestFilesSubFolder.ReportTestFiles)))
			{
				report.PrepareForRender();
				((TextField)report.FilterCollection[1]).Value = "JobShipmentPreplanning";
				AssertEquals("there should only be one section", 2, report.Analyser.Sections.Count);
				AssertEquals("there should be 1 data areas in first section ", 1, report.Analyser.Sections[0].DataAreas.Count);
				var firstDataArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				AssertNotNull("First data area should be of type DataArea", firstDataArea);
				AssertEquals("First data area table name should be test1", "test1", firstDataArea.TableName);
				var source1 = firstDataArea.DataRowSource;
				AssertEquals("filtering applied should be only 1 record ", 1, source1.RowCount);
				AssertEquals("JobShipmentPreplanning", report.DataProvider.GetColumnValue(source1, 0, "test1.Table_Name"));

				try
				{
					var field = (string)report.Analyser.DocumentHeader.GetColumnValue(0, "Header.JS_UniqueConsignRef");
				}
				catch
				{
					Fail("If an exception is reaised here it's because an attempt was made to filter the Header data source");
				}

				AssertEquals("there should be 1 data areas in first section ", 1, report.Analyser.Sections[1].DataAreas.Count);
				var secondDataArea = report.Analyser.Sections[1].DataAreas[0] as SectionBodyArea;
				AssertNotNull("First data area should be of type DataArea", secondDataArea);
				AssertEquals("First data area table name should be test1", "test2", secondDataArea.TableName);
				var source2 = secondDataArea.DataRowSource;
				AssertEquals("filtering applied should be only 1 record for source 2", 1, source2.RowCount);
				AssertEquals("Filtering is applied to both sections", "JobShipmentPreplanning", report.DataProvider.GetColumnValue(source2, 0, "test2.Table_Name"));
			}
		}

		public void TestSectionBodyFilteredDataWithFilteredReportDataSource()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();
				var createdArea = new SectionBodyArea(rpt.Analyser.Sections[0].SectionBody.StartingRow, rpt.Analyser.Sections[0].SectionBody.End, rpt, "#SectionBody:Data=Lines:FilterBy(<GSTRate>==5)");// Rpt.Analyser.Sections[0].SectionBody;				}
				AssertEquals(16, createdArea.DataRowSource.RowCount);
				createdArea = new SectionBodyArea(rpt.Analyser.Sections[0].SectionBody.StartingRow, rpt.Analyser.Sections[0].SectionBody.End, rpt, "#SectionBody:Data=Lines:FilterBy((<GSTRate>==5 && <GST>==5))");// Rpt.Analyser.Sections[0].SectionBody;				}
				AssertEquals(4, createdArea.DataRowSource.RowCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionBodyFilteredDataWithFilteredBizObjDataSource()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();
			var normalBool = true;
			var filteredBool = false;
			var normalDecimal = new ZDecimal(3.14m);
			var filteredDecimal = new ZDecimal(0.04m);
			var normalText = new ZString("Don't filter me out!");
			var filteredText = new ZString("Filter this.");

			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, filteredBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, filteredBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, filteredBool, normalDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, filteredBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, normalDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, normalText);
			AddChildToDummyObject(topLevelDataSource, filteredBool, normalDecimal, filteredText);
			AddChildToDummyObject(topLevelDataSource, normalBool, filteredDecimal, normalText);

			var excelTemplate = new ExcelTemplateForUnitTesting("SectionBodyFilteredDataWithFilteredBizObjDataSource.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();
				var sections = report.Analyser.Sections;

				AssertEquals("Precondition: sections.Count", 6, sections.Count);

				var bodyAreaFilteredBool = sections[0].DataAreas[0] as SectionBodyArea;
				AssertEquals("bodyAreaFilteredBool.DataRowSource.RowCount", 5, bodyAreaFilteredBool.DataRowSource.RowCount);

				var bodyAreaFilteredDecimal = sections[1].DataAreas[0] as SectionBodyArea;
				AssertEquals("bodyAreaFilteredDecimal.DataRowSource.RowCount", 8, bodyAreaFilteredDecimal.DataRowSource.RowCount);

				var bodyAreaFilteredText = sections[2].DataAreas[0] as SectionBodyArea;
				AssertEquals("bodyAreaFilteredText.DataRowSource.RowCount", 9, bodyAreaFilteredText.DataRowSource.RowCount);

				var bodyAreaFilteredTextAndDecimal = sections[3].DataAreas[0] as SectionBodyArea;
				AssertEquals("bodyAreaFilteredTextAndDecimal.DataRowSource.RowCount", 4, bodyAreaFilteredTextAndDecimal.DataRowSource.RowCount);

				var bodyAreaFilteredBoolAndText = sections[4].DataAreas[0] as SectionBodyArea;
				AssertEquals("bodyAreaFilteredBoolAndText.DataRowSource.RowCount", 2, bodyAreaFilteredBoolAndText.DataRowSource.RowCount);

				var bodyAreaFilteredBoolOrText = sections[5].DataAreas[0] as SectionBodyArea;
				AssertEquals("bodyAreaFilteredBoolOrText.DataRowSource.RowCount", 12, bodyAreaFilteredBoolOrText.DataRowSource.RowCount);
			}
		}

		public void TestReportRowIndexIsMinusOne()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Number>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");

			using (var documentPack = new DocumentPack(menuItem))
			using (var rpt = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				rpt.PrepareForRender();
				var bodyArea = rpt.Analyser.Areas.OfType<SectionBodyArea>().FirstOrDefault();

				bodyArea.ReportRowIndexIsMinusOneOrUnmatchedWithStartEndRow("WhatEver");

				AssertContains("Has Starting Row", "StartingRow: 2", ErrorReporter.LastMessageReported);
				AssertContains("Has Start Of Body", "StartOfBody: 3", ErrorReporter.LastMessageReported);
				AssertContains("Has End", "End: 3", ErrorReporter.LastMessageReported);
				AssertContains("Has Area Header Text", "AreaHeaderText: #SectionBody", ErrorReporter.LastMessageReported);
				AssertContains("Has Macro that is being translated", "Macro that is being translated: WhatEver", ErrorReporter.LastMessageReported);
				AssertContains("Has children", "Has children: False", ErrorReporter.LastMessageReported);
				AssertContains("Has Current Row", "CurrentRow: 0", ErrorReporter.LastMessageReported);
				AssertContains("Has Current Column", "CurrentColumn: 0", ErrorReporter.LastMessageReported);
				AssertContains("Has CellContent", "CellContent: #Config", ErrorReporter.LastMessageReported);
				AssertContains("Has Last Row Index", "LastRowIndex: 0", ErrorReporter.LastMessageReported);
				AssertContains("Has Is Processing Macro", "IsProcessingMacro: False", ErrorReporter.LastMessageReported);
				AssertContains("Has DBRowCount", "DBRowCount: 1", ErrorReporter.LastMessageReported);
				AssertContains("Has CurrentPass", "CurrentPass: FirstPass", ErrorReporter.LastMessageReported);
				AssertContains("Has IsClonedForPageBreaking", "IsClonedForPageBreaking: False", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestIsDataRowIndexWithRowRangesMatchedWithStartEndRow()
		{
			var bodyArea = new SectionBodyArea(15, 21, Report.NewForTesting(DocumentPack.EmptyPack), "");
			Assert(bodyArea.IsDataRowIndexWithRowRangesMatchedWithStartEndRow());
			bodyArea.DataRowIndexWithRowRanges.Add(0, new RowRange(16, 17));
			bodyArea.DataRowIndexWithRowRanges.Add(1, new RowRange(18, 19));
			bodyArea.DataRowIndexWithRowRanges.Add(2, new RowRange(20, 21));
			Assert(bodyArea.IsDataRowIndexWithRowRangesMatchedWithStartEndRow());

			bodyArea.Shift(-1);
			Assert(!bodyArea.IsDataRowIndexWithRowRangesMatchedWithStartEndRow());
			bodyArea.Shift(2);
			Assert(!bodyArea.IsDataRowIndexWithRowRangesMatchedWithStartEndRow());
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Test");

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}

		ExcelTemplateForUnitTesting bizObjDataSourceForGetGroups;
		ExcelTemplateForUnitTesting BizObjDataSourceForGetGroups
		{
			get
			{
				if (bizObjDataSourceForGetGroups == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.BizObjDataSourceForGetGroups.xls", "BizObjDataSourceForGetGroups.xls");
					bizObjDataSourceForGetGroups = new ExcelTemplateForUnitTesting("BizObjDataSourceForGetGroups.xls", Path.GetFullPath(tempFileName));
				}
				return bizObjDataSourceForGetGroups;
			}
		}

		DummyBusinessObject GetNewBizObjDataSourceForGetGroups()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();
			var zBools = new List<ZBool>();
			zBools.Add(new ZBool(true));
			zBools.Add(new ZBool(false));
			var zDecimals = new List<ZDecimal>();
			zDecimals.Add(new ZDecimal(3.14m));
			zDecimals.Add(new ZDecimal(0.04m));
			zDecimals.Add(new ZDecimal(9.872m));
			var zStrings = new List<ZString>();
			zStrings.Add(new ZString("foo"));
			zStrings.Add(new ZString("bar"));
			zStrings.Add(new ZString("hello"));
			zStrings.Add(new ZString("world"));

			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[2], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[1], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[1], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[1], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[1], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[2], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[1], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[2], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[1], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, zBools[1], zDecimals[0], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[2], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, zBools[0], zDecimals[0], zStrings[0]);
			return topLevelDataSource;
		}

		DummyChildBusinessObject AddChildToDummyObject(DummyBusinessObject topLevelDataSource, ZBool valueBool, ZDecimal valueDecimal, ZString valueNText)
		{
			var result = topLevelDataSource.Collection.AddNew();
			result.Z0_Bool = valueBool;
			result.Z0_AnotherDecimal = valueDecimal;
			result.Z0_NVarCharMax = valueNText;
			return result;
		}

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplate excelTemplate)
		{
			return new Report(new DocumentPack(Factory.New<DocumentCommand>()), excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}

		IDataRowSource CreateTestDataSource()
		{
			var testData = new DataTable();
			testData.Columns.Add("Column1", typeof(string));

			for (var i = 0; i < 20; i++)
			{
				testData.Rows.Add(testData.NewRow());
				testData.Rows[i]["Column1"] = "Row" + i;
			}

			return new ReportDataSource(testData);
		}
	}
}
