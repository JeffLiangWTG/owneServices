using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing
{
	class ReportRendererTest : TestCaseWithFactory
	{
		public void TestLegacyDocumentPageOrientation()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[PageStyle=Landscape]
{A}-[#SectionBody]
{B}-[TestCell]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();
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

						AssertEquals("Excel page orientation should be Landscape", true, excelInterface.Xls.PrintLandscape);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRender_WhenForcedLanguageExists_ShouldBeTranslatedByForcedLanguage()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ForcedLanguageTranslateTestWithValidLanguageCode.xls", TestFilesSubFolder.DocumentTestFiles);

			var dataSource = Factory.New<DummyBusinessObject>();

			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dataSource), "Test", null, DocumentDirection.ANY, false))
			{
				var documentTemplate = Factory.New<StmTemplateBase>();
				documentTemplate.SO_Name = "Report";
				documentTemplate.SO_IsSystemDefined = true;
				documentTemplate.SO_ExcelTemplatePath = "Customs+CH+Customs Duties (eVV)";
				report.StTemplate = documentTemplate;

				using (var outputStream = new MemoryStream())
				{
					var resouceStringKey = "LegacyDocLabel|Customs+CH+Customs Duties (eVV)|QWNjZXB0YW5jZSBEYXRlOg==";
					using (var mockChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
					{
						mockChs.Put(resouceStringKey, new ResourceStringData(resouceStringKey, "接受日期"));
						report.Save(outputStream);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(outputStream);
							AssertEquals("ForcedLanguage in template should be used to translate document", "{B}-[接受日期]", excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestReportPageOrientation()
		{
			var helper = new TemplateTestHelper();

			helper.AddWorkSheet("Test",
@"{A}-[#Config]
{A}-[PageStyle=Landscape]
{A}-[Data:ReportData=select PortName = 'Test Port']
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.PortName>]
{A}-[#EndOfReport]");

			var template = helper.CreateTemplate(Factory, "Test");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack.GetFirstReport();
				report.PrepareForRender();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertEquals("Excel page orientation should be Landscape", true, excelInterface.Xls.PrintLandscape);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestReportRendererRearrangeColumnsWithMultipleSheet()
		{
			var template = new ExcelTemplateForUnitTesting("TestReportRendererRearrangeColumnsWithMultipleSheet.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = new Report(new DocumentPack(), template))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreakWithUnfitAndUnSplittableDataArea()
		{
			AssertTemplateAndOutputForUnSplittableDataArea("PageBreakWithUnfitAndUnSplittableSectionBodyArea.xls", "ExpectedOutputForPageBreakWithUnfitAndUnSplittableSectionBodyArea.xls");
			AssertTemplateAndOutputForUnSplittableDataArea("PageBreakForUnSplittableSectionBodyAreaWithMergedCells.xlsx", "ExpectedOutputForPageBreakForUnSplittableSectionBodyAreaWithMergedCells.xlsx");
		}

		void AssertTemplateAndOutputForUnSplittableDataArea(string inputTemplate, string outputDocument)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Main BusinessObject";
			dummy.Collection.AddNew().Z0_VarCharMax = "Sub Collection";

			var excelTemplate = new ExcelTemplateForUnitTesting(inputTemplate, TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						var expectedTemplate = new ExcelTemplateForUnitTesting(outputDocument, TestFilesSubFolder.ExpectedDocuments);
						using (var expectExcelInterface = new ExcelInterface())
						{
							expectExcelInterface.LoadExcelFile(expectedTemplate.FullTemplateSourceLocation);
							AssertEquals(expectExcelInterface.WorkSheets[0].ToString(), excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFinalPageHeightAndWidthForCustomAndContinuousPageStyle()
		{
			var template = new ExcelTemplateForUnitTesting("TestFinalPageHeightAndWidthForCustomAndContinuousPageStyle.xls", TestFilesSubFolder.DocumentTestFiles);

			var dummy = Factory.New<DummyBODocSupportable>();

			for (var i = 0; i < 100; i++)
			{
				var bo = dummy.Collection.AddNew();
				bo.Z0_Number = i;
			}

			using (var report = new Report(Pack, template, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
			{
				using (var stream = new MemoryStream())
				{
					report.PrepareForRender();
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals(2, excelInterface.SheetCount);

						excelInterface.ActiveWorksheet = 0;
						AssertEquals("CustomPageStyle", excelInterface.Xls.SheetName);
						AssertContains("TotalPages: 1", excelInterface.WorkSheets[0].ToString());

						var widthInTenthsOfMillimeter = (int)(excelInterface.Xls.PrintPaperDimensions.Width * 2.54);
						var heightInTenthsOfMillimeter = (int)(excelInterface.Xls.PrintPaperDimensions.Height * 2.54);
						AssertEquals("Paper size should be Undefined", TPaperSize.Undefined, excelInterface.Xls.PrintPaperSize);
						AssertEquals("Paper width should be 570 tenths of millimeters", 570, widthInTenthsOfMillimeter);
						AssertEquals("Paper height should be 900 tenths of millimeters", 900, heightInTenthsOfMillimeter);

						excelInterface.ActiveWorksheet = 1;
						AssertEquals("ContinuousPageStyleAutoHeight", excelInterface.Xls.SheetName);
						AssertContains("TotalPages: 1", excelInterface.WorkSheets[1].ToString());
						AssertContains("Number: 0", excelInterface.WorkSheets[1].ToString());
						AssertContains("Number: 99", excelInterface.WorkSheets[1].ToString());

						widthInTenthsOfMillimeter = (int)(excelInterface.Xls.PrintPaperDimensions.Width * 2.54);
						heightInTenthsOfMillimeter = (int)(excelInterface.Xls.PrintPaperDimensions.Height * 2.54);
						var heightInAllPages = report.Renderer.Pages.Sum(p => p.Areas.Height) / FlxConsts.RowMult;
						var realHeightInTenthsOfMm = (int)(heightInAllPages * 2.54 / 96 * 100);
						AssertEquals("Paper size should be Undefined", TPaperSize.Undefined, excelInterface.Xls.PrintPaperSize);
						AssertEquals("Paper width should be 570 tenths of millimeters", 570, widthInTenthsOfMillimeter);
						NUnit.Framework.Assert.That(heightInTenthsOfMillimeter, NUnit.Framework.Is.EqualTo(realHeightInTenthsOfMm).Within(105), "Paper height should be equal to the real height of the content");
					}
				}
			}
		}

		public void TestLastRowIndexBeResetInSecondPass()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[WhatEver]
{B}-[<Collection.Code>]
{B}-[<AbriBAR128sBarCode('<Collection.Code>',UseOptimisedEncoding,IsGS1128Barcode)>]
{B}-[WhatEver]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			AssertNoExceptionThrown(() => DeliveryTestHelper.DeliverDocument(documentCommand));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTranslateAreasBeforeLoadRows()
		{
			Factory.New<DummyBusinessObject>();
			Factory.New<DummyBusinessObject>();
			Factory.Save();

			var counter = 0;
			var key = "";
			Res.ResourceDemanded += (sender, e) =>
			{
				if (e.Key == key)
				{
					counter++;
				}
			};

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PrintTitlesSingle.xlsx", TestFilesSubFolder.ReportTestFiles);

			using (var report = new Report(Pack, excelTemplate, Guid.Empty, DataContext.UnitTest))
			using (var resourceStrings = Res.UseMockData())
			{
				var stTemplate = Factory.New<StmTemplateBase>();
				stTemplate.SO_ExcelTemplatePath = excelTemplate.TemplateSourceLocation;
				stTemplate.SO_IsSystemDefined = true;
				report.StTemplate = stTemplate;

				key = DocBuilderResourceStrings.GetKey(Path.GetFileNameWithoutExtension(excelTemplate.TemplateSourceLocation), DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestCell");
				resourceStrings.Put(key, new ResourceStringData(key, "Translation for \"TestCell\""));

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);
					AssertEquals("Only translate \"TestCell\" once", 1, counter);
				}
			}
		}

		public void TestLegacyDocumentTranslation()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[#SectionBody]
{B}-[TestCell]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			using (var resourceStrings = Res.UseMockData())
			{
				var stTemplate = Factory.New<StmTemplateBase>();
				stTemplate.SO_Name = "TranslateLegacyDocument";
				stTemplate.SO_ExcelTemplatePath = "TranslateLegacyDocument.xls";
				stTemplate.SO_IsSystemDefined = true;
				report.StTemplate = stTemplate;

				var key = DocBuilderResourceStrings.GetKey("TranslateLegacyDocument", DocBuilderResourceStrings.LegacyDocLabelKeyPrefix, "TestCell");
				resourceStrings.Put(key, new ResourceStringData(key, "Translation for TestCell"));
				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertMultilineASCIIEquals("", @"{B}-[Translation for TestCell]", excelInterface.WorkSheets.First().ToString());
					}
				}
			}
		}

		public void TestGroupBy2SeparateFieldsCanBreakPage()
		{
			var helper = new TemplateTestHelper();

			helper.AddWorkSheet("Test",
@"{A}-[#Config]
{A}-[PageStyle=Continuous]
{A}-[Data:ReportData=select Port = 'Port 1', Bill = 'Bill 3' union all select Port = 'Port 1', Bill = 'Bill 3' union all select Port = 'Port 1', Bill = 'Bill 4' union all select Port = 'Port 2', Bill = 'Bill 4' union all select Port = 'Port 2', Bill = 'Bill 5' union all select Port = 'Port 2', Bill = 'Bill 5']
{A}-[#SectionPageHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{A}-[#GroupBy:ReportData.Port]
{B}-[<ReportData.Port>]
{A}-[#GroupBy:ReportData.Bill]
{B}-[<ReportData.Bill>]
{A}-[#EndOfReport]");

			helper.AddWorkSheet("GroupBys",
@"{A}-[Port,Bill]   {B}-[ReportData.Port,ReportData.Bill]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Test");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack.GetFirstReport();

				report.PrepareForRender();
				report.GroupByCollection.BreakPageOverride = ZBool.True;

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There should be a new page for each group.",
@"{B}-[Page 1 of 3]
{B}-[Port 1]
{B}-[Bill 3]
{B}-[Page 2 of 3]
{B}-[Port 1]
{B}-[Port 2]
{B}-[Bill 4]
{B}-[Page 3 of 3]
{B}-[Port 2]
{B}-[Bill 5]", excelInterface.WorkSheets.First().ToString());
					}
				}
			}
		}

		public void TestGroupBy2FieldsCanBreakPage()
		{
			var helper = new TemplateTestHelper();

			helper.AddWorkSheet("Test",
@"{A}-[#Config]
{A}-[PageStyle=Continuous]
{A}-[Data:ReportData=select Port = 'Port 1', Bill = 'Bill 1' union all select Port = 'Port 1', Bill = 'Bill 3' union all select Port = 'Port 1', Bill = 'Bill 5' union all select Port = 'Port 2', Bill = 'Bill 2' union all select Port = 'Port 2', Bill = 'Bill 4' union all select Port = 'Port 2', Bill = 'Bill 6']
{A}-[#SectionPageHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Port>]   {C}-[<ReportData.Bill>]
{A}-[#GroupBy:ReportData.Port+ReportData.Bill]
{B}-[Break]
{A}-[#EndOfReport]");

			helper.AddWorkSheet("GroupBys",
@"{A}-[Port+Bill]   {B}-[ReportData.Port+ReportData.Bill]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Test");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack.GetFirstReport();

				report.PrepareForRender();
				report.GroupByCollection.BreakPageOverride = ZBool.True;

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There should be a new page for each group.",
@"{B}-[Page 1 of 6]
{B}-[Port 1]   {C}-[Bill 1]
{B}-[Break]
{B}-[Page 2 of 6]
{B}-[Port 1]   {C}-[Bill 3]
{B}-[Break]
{B}-[Page 3 of 6]
{B}-[Port 1]   {C}-[Bill 5]
{B}-[Break]
{B}-[Page 4 of 6]
{B}-[Port 2]   {C}-[Bill 2]
{B}-[Break]
{B}-[Page 5 of 6]
{B}-[Port 2]   {C}-[Bill 4]
{B}-[Break]
{B}-[Page 6 of 6]
{B}-[Port 2]   {C}-[Bill 6]
{B}-[Break]", excelInterface.WorkSheets.First().ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionBodyAreaHasOneRowCannotBreakPage()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();

			var excelTemplate = new ExcelTemplateForUnitTesting("LastAreaHasOneRowCannotBreakPage.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var documentPack = new DocumentPack())
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			using (var outputStream = new MemoryStream())
			{
				AssertNoExceptionThrown(() => report.Save(outputStream));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionBodyAreaHasOneRowCannotBreakPageAndCannotFitInNewPage()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();

			var excelTemplate = new ExcelTemplateForUnitTesting("LastAreaHasOneRowCannotBreakPageAndCannotFitInNewPage.xlsx", TestFilesSubFolder.DocumentTestFiles);

			using (var documentPack = new DocumentPack())
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				var firstPage = report.Renderer.Pages[0];

				AssertType<SectionBodyArea>("SectionBodyArea should be in first page", firstPage.Areas[3]);
			}
		}

		public void TestPageFooterWithSectionBodyWithNoData()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Code>]
{A}-[#PageFooter]
{B}-[END OF DOCUMENT]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected = @"{B}-[END OF DOCUMENT]";

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

						AssertMultilineASCIIEquals("Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestTwoSectionsWithGroupByGroupTitlePageBreakDoesNotBreakBetweenEachSection()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			dummy.Collection.AddNew("A", "Aye");
			dummy.Collection.AddNew("B", "Bee");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be 4 pages.",
@"{B}-[Page 1 of 4]
{B}-[A]
{B}-[Aye]
{B}-[Page 2 of 4]
{B}-[B]
{B}-[Bee]
{B}-[Page 3 of 4]
{B}-[A]
{B}-[Aye]
{B}-[Page 4 of 4]
{B}-[B]
{B}-[Bee]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestTwoSectionsWithGroupByGroupTitlePageBreakDoesNotBreakOnSecondSectionIfFirstSectionEmpty()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=CollectionWithPublicSetter]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			dummy.CollectionWithPublicSetter = new DummyChildBusinessObjectCollection(Factory);
			dummy.Collection.AddNew("A", "Aye");
			dummy.Collection.AddNew("B", "Bee");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be no blank page.",
@"{B}-[Page 1 of 2]
{B}-[A]
{B}-[Aye]
{B}-[Page 2 of 2]
{B}-[B]
{B}-[Bee]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestGroupTitlePageBreakDoesNotBreakIfFirstSectionIsNotGroupByAndNotEmpty()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Code>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A", "Aye");
			dummy.Collection.AddNew("B", "Bee");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be no break in first page.",
@"{B}-[Page 1 of 2]
{B}-[A]
{B}-[B]
{B}-[A]
{B}-[Aye]
{B}-[Page 2 of 2]
{B}-[B]
{B}-[Bee]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestThreeSectionsWithGroupByGroupTitlePageBreakShouldBreakEvenWithOneEmptySectionInBetween()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#SectionBody:Data=CollectionWithPublicSetter]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A", "Aye");
			dummy.Collection.AddNew("B", "Bee");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be break in all pages.",
@"{B}-[Page 1 of 4]
{B}-[A]
{B}-[Aye]
{B}-[Page 2 of 4]
{B}-[B]
{B}-[Bee]
{B}-[Page 3 of 4]
{B}-[A]
{B}-[Aye]
{B}-[Page 4 of 4]
{B}-[B]
{B}-[Bee]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreakWillRecalculateSplitAreaWhenComeAcrossHPageBreak()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageBreakWillRecalculateSplitAreaWhenComeAcrossHPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(new DocumentPack(), excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				using (var excelInterfaceForExpectedDocument = new ExcelInterface())
				{
					report.Save(outputStream);
					excelInterfaceForExpectedDocument.LoadExcelFile(outputStream);
					AssertEquals("Should have 1 Sheet", 1, excelInterfaceForExpectedDocument.WorkSheets.Count);
					AssertEquals("There should have 11 PageBreaks (exactly 10 pages)", 11, excelInterfaceForExpectedDocument.WorkSheets[0].HPageBreakCount);
				}
			}
		}

		public void TestSectionHeaderPageBreakWillBeAddedEvenNextSectionWithoutData()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[Test Document Header]
{A}-[#PageHeader]
{B}-[Test Page Header]
{A}-[#PageFooter]
{B}-[Test Page Footer <CurrentPage>]
{A}-[#SectionHeader:PageBreak]
{B}-[Header 1]
{A}-[#SectionBody:Data=CollectionWithPublicSetter]
{B}-[<CollectionWithPublicSetter.Z0_Description>]
{A}-[#SectionHeader:PageBreak]
{B}-[Header 2A]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#SectionHeader:PageBreak]
{B}-[Header 2B]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#SectionHeader:PageBreak]
{B}-[Header 3]
{A}-[#SectionBody:Data=CollectionWithPublicSetter]
{B}-[<CollectionWithPublicSetter.Z0_Description>]
{A}-[#SectionHeader:PageBreak]
{B}-[Header 4]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			dummy.CollectionWithPublicSetter = new DummyChildBusinessObjectCollection(Factory);
			dummy.Collection.AddNew("A", "Aye");
			dummy.Collection.AddNew("B", "Bee");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("",
@"{B}-[Test Document Header]
{B}-[Test Page Header]
{B}-[Test Page Footer 1]
{B}-[Test Page Header]
{B}-[Header 2A]
{B}-[Aye]
{B}-[Bee]
{B}-[Test Page Footer 2]
{B}-[Test Page Header]
{B}-[Header 2B]
{B}-[Aye]
{B}-[Bee]
{B}-[Test Page Footer 3]
{B}-[Test Page Header]
{B}-[Header 4]
{B}-[Aye]
{B}-[Bee]
{B}-[Test Page Footer 4]", excelInterface.WorkSheets[0].ToString());
				AssertEquals(4, excelInterface.WorkSheets[0].HPageBreakCount);
			}
		}

		public void TestParameterRemoveFirstPageIfNoData()
		{
			var expectedResult =
@"{B}-[Test Document Header]
{B}-[Test Page Header]


{B}-[Test Page Footer1]
{B}-[Test Page Header]
{B}-[Header 2]
{B}-[Aye]
{B}-[Bee]
{B}-[Test Page Footer2]";

			AssertPrintDocumentWithParameterRemoveFirstPageIfNoDataOrNot(false, expectedResult, 2);

			expectedResult =
@"{B}-[Test Document Header]
{B}-[Test Page Header]
{B}-[Header 2]
{B}-[Aye]
{B}-[Bee]
{B}-[Test Page Footer1]";

			AssertPrintDocumentWithParameterRemoveFirstPageIfNoDataOrNot(true, expectedResult, 1);
		}

		void AssertPrintDocumentWithParameterRemoveFirstPageIfNoDataOrNot(bool useRemoveFirstPageIfNoDataParameter, string expectedResult, int expectedPages)
		{
			var contents =
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]";

			if (useRemoveFirstPageIfNoDataParameter)
			{
				contents += @"
{A}-[RemoveFirstPageIfNoData]";
			}

			contents += @"
{A}-[#DocumentHeader]
{B}-[Test Document Header]
{A}-[#PageHeader]
{B}-[Test Page Header]
{A}-[#PageFooter]
{B}-[Test Page Footer<CurrentPage>]
{A}-[#SectionBody:Data=CollectionWithPublicSetter]
{B}-[<CollectionWithPublicSetter.Z0_Description>]
{A}-[#SectionHeader:PageBreak]
{B}-[Header 2]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#EndOfReport]";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test", contents);
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			dummy.CollectionWithPublicSetter = new DummyChildBusinessObjectCollection(Factory);
			dummy.Collection.AddNew("A", "Aye");
			dummy.Collection.AddNew("B", "Bee");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("", expectedResult, excelInterface.WorkSheets[0].ToString());
				AssertEquals(expectedPages, excelInterface.WorkSheets[0].HPageBreakCount);
			}
		}

		public void TestDataRowIndexWithRowRangesShouldBeUpdatedAccordinglyWhenProcessingPageBreak()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#BackPage]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#SectionPageHeader]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#SectionBody:Data=Collection]
{B}-[<AbriBAR128sBarCode('<Collection.Code>',UseOptimisedEncoding,IsGS1128Barcode)>]
{A}-[#GroupBy:Collection.Code:PageBreak]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			AssertNoExceptionThrown(() => DeliveryTestHelper.DeliverDocument(documentCommand));
		}

		public void TestGroupBySectionWithNoDataDoesNotGenerateEmptyPages()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[Placeholder]
{A}-[#GroupBy:Collection.Z0_Number]
{B}-[Placeholder]
{A}-[#GroupBy:Collection.Z0_VarCharMax:PageBreak]
{B}-[Placeholder]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(Factory.New<DummyBODocSupportable>()), "Test", null, DocumentDirection.ANY, false))
				{
					using (var stream = new MemoryStream())
					{
						report.PrepareForRender();
						report.Save(stream);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream);

							AssertMultilineASCIIEquals("There should only be one page.", "{B}-[Page 1 of 1]", excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestGroupBySectionAsGroupTitleWithData()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>: <Collection.Z0_VarCharMax>]
{A}-[#GroupBy:Collection.Z0_Number:GroupTitle]
{B}-[<Collection.Z0_Number>]
{A}-[#GroupBy:Collection.Z0_VarCharMax:GroupTitle:PageBreak]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBODocSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;
			child1.Z0_VarCharMax = "Child 1";

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;
			child2.Z0_VarCharMax = "Child 2";

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					using (var stream = new MemoryStream())
					{
						report.PrepareForRender();
						report.Save(stream);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream);

							AssertMultilineASCIIEquals("There should be 2 pages.",
@"{B}-[Page 1 of 2]
{B}-[Child 1]
{B}-[1]
{B}-[1: Child 1]
{B}-[Page 2 of 2]
{B}-[Child 2]
{B}-[2]
{B}-[2: Child 2]",
								excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestHideRowIfInSectionPageHeaderDoesNotEffectPageHeight()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew().Z0_VarCharMax = "Hello World";

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionPageHeader]
{B}-[The section page header should be hidden.] {C}-[<HideRowIf(1 == 1)>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#PageFooter]
{B}-[This is the page footer.]
{A}-[#EndOfReport]");

			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
			{
				using (var stream = new MemoryStream())
				{
					report.PrepareForRender();
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						var workSheet = excelInterface.WorkSheets[0];

						AssertMultilineASCIIEquals("Pre-condition",
@"{B}-[Hello World]


{B}-[This is the page footer.]",
							workSheet.ToString());

						AssertEquals("Pre-condition", "This is the page footer.", workSheet.GetCell(4, 1).ValueSourceText);

						var pageHeight = 0;
						for (var row = 0; row <= 4; row++)
						{
							pageHeight += workSheet.GetRowHeight(row);
						}

						AssertEquals("pageHeight", 14096, pageHeight);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRearrangeColumnsWorksFineWithNonExistantOptionalColumns()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("RearrangeWithNonExistantOptionalColumns.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.PrepareForRender();
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].CurrentPosition = 0;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].CurrentPosition = 1;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].CurrentPosition = 1;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[3].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[3].CurrentPosition = 2;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[4].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[4].CurrentPosition = 3;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[5].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[5].CurrentPosition = 4;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[6].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[6].CurrentPosition = 5;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[7].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[7].CurrentPosition = 5;

					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Column 1 on worksheet 0", "ID", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("Column 2 on worksheet 0", "Client Size", excelInterface.WorkSheets[0][3, 3]);
						AssertEquals("Column 3 on worksheet 0", "Outcome Desc.", excelInterface.WorkSheets[0][3, 4]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRearrangeColumnsWorksFineWithHideIfDescriptionEmpty()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("RearrangeWithHideIfDescriptionEmpty.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					report.PrepareForRender();

					//ID, not hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].CurrentPosition = 0;

					//Client Name, hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].CurrentPosition = 1;

					//Client Size, not hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].CurrentPosition = 1;

					//Outcome Description, should be hidden cause description is empty and column flagged as HideIfDescriptionEmpty
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[3].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[3].CurrentPosition = 1;

					//Estimated Value 1, not hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[4].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[4].CurrentPosition = 2;

					//Estimated Value 2, not hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[5].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[5].CurrentPosition = 3;

					//Estimated Value 3, hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[6].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[6].CurrentPosition = 4;

					//Estimated Value 4, not hidden
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[7].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[7].CurrentPosition = 4;

					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						CombineAssertions(() =>
						{
							AssertEquals("Column 1 on worksheet 0", "ID", excelInterface.WorkSheets[0][3, 2]);
							AssertEquals("Column 2 on worksheet 0", "Client Size", excelInterface.WorkSheets[0][3, 3]);
							AssertEquals("Column 3 on worksheet 0", "Estimated Value 1", excelInterface.WorkSheets[0][3, 4]);
							AssertEquals("Column 4 on worksheet 0", "Estimated Value 2", excelInterface.WorkSheets[0][3, 5]);
							AssertEquals("Column 5 on worksheet 0", "Estimated Value 4", excelInterface.WorkSheets[0][3, 6]);
							AssertEquals("Column 6 on worksheet 0", "", excelInterface.WorkSheets[0][3, 7]);
							AssertEquals("Column 7 on worksheet 0", "", excelInterface.WorkSheets[0][3, 8]);
							AssertEquals("Column 8 on worksheet 0", "", excelInterface.WorkSheets[0][3, 9]);
						});
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRearrangeColumnsWorksFineWithOriginalColumnNumberLessThanDestinationColumnNumber()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("RearrangeWithHideIfDescriptionEmpty.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					report.PrepareForRender();

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].CurrentPosition = 7;

					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						CombineAssertions(() =>
						{
							AssertEquals("Column 1 on worksheet 0", "Client Name", excelInterface.WorkSheets[0][3, 2]);
							AssertEquals("Column 2 on worksheet 0", "Client Size", excelInterface.WorkSheets[0][3, 3]);
							AssertEquals("Column 3 on worksheet 0", "", excelInterface.WorkSheets[0][3, 4]);
							AssertEquals("Column 4 on worksheet 0", "Estimated Value 1", excelInterface.WorkSheets[0][3, 5]);
							AssertEquals("Column 5 on worksheet 0", "Estimated Value 2", excelInterface.WorkSheets[0][3, 6]);
							AssertEquals("Column 6 on worksheet 0", "Estimated Value 3", excelInterface.WorkSheets[0][3, 7]);
							AssertEquals("Column 7 on worksheet 0", "ID", excelInterface.WorkSheets[0][3, 8]);
							AssertEquals("Column 8 on worksheet 0", "Estimated Value 4", excelInterface.WorkSheets[0][3, 9]);
							AssertEquals("Column 9 on worksheet 0", "", excelInterface.WorkSheets[0][3, 10]);
						});
					}
				}
			}
		}

		public void TestRemoveNonExistantObjectFromWorkSheetReportsWarningGracefully()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Shi Lian Dan";

			IDocumentSupportable shipment = (IDocumentSupportable)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			IBODocDataProvider[] dataProviders = shipment.DocumentSupporter.GetBODocDataProviders(new DataContextValue(nameof(DataContext.Shipment)), menuItem);
			AssertNotNull("Precondition: DataContext.Shipment should return one DocumentWrapper", dataProviders);
			AssertEquals("Precondition: DataContext.Shipment should return one DocumentWrapper", 1, dataProviders.Length);
			IBODocDataProvider dataProvider = dataProviders[0];

			AssertEquals("Precondition: shipment.GetType().Name", "ForwardingShipment", shipment.GetType().Name);
			AssertEquals("Precondition: dataProvider.GetType().Name", "DocForwardingShipment", dataProvider.GetType().Name);

			DocumentPack pack = new DocumentPack(menuItem);

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#DocumentHeader";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);

					ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
					using (Report report = new Report(pack, excelTemplate, dataProvider, "TestOnly", null, DocumentDirection.ANY, false))
					{
						using (MemoryStream outputStream = new MemoryStream())
						{
							((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
							AssertNoExceptionThrown(delegate
							{ report.Save(outputStream); });
							AssertEquals("Report.Errors", "Severity: [Warning (without error report)] Message: [Couldn't remove image [DockReceipt.FaceImage] as directed by the [ImageNamesToRemove] property on DataSource: [DocForwardingShipment]. Object [DockReceipt.FaceImage] does not exist.] Cell: [N/A]",
														report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
						}
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveNoGroupBys()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleWithNoGroupBy.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("", excelInterface.WorkSheets[0][0, 1].ToString().Trim());
						AssertEquals("TestTemplate Header1", excelInterface.WorkSheets[0][1, 2].ToString().Trim());
						AssertEquals("Here", excelInterface.WorkSheets[0][16, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
						AssertEquals("=SUM(U24:U55)", excelInterface.WorkSheets[0].GetCellFormula(56, 20));
						AssertEquals("1488", excelInterface.WorkSheets[0][56, 20].ToString());
						AssertEquals("=SUM(Q24:Q55)", excelInterface.WorkSheets[0].GetCellFormula(57, 16));
						AssertEquals("112", excelInterface.WorkSheets[0][57, 16].ToString());
						AssertEquals("=SUM(U24:U55,U70:U115,U130:U175,U190:U225)", excelInterface.WorkSheets[0].GetCellFormula(225, 6));
						AssertEquals("38160", excelInterface.WorkSheets[0][225, 6].ToString());
						AssertEquals("=G226", excelInterface.WorkSheets[0].GetCellFormula(234, 16));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith0X0Data_ShowEvenWithNoData()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSectionsShowHeaderFootersEvenWithNoData.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 0));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 0));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][2, 2]);
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][5, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith2X0Data_ShowEvenWithNoData()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSectionsShowHeaderFootersEvenWithNoData.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 6));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 0));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][4, 20].ToString());
						AssertEquals("=SUM(U3:U4)", excelInterface.WorkSheets[0].GetCellFormula(4, 20));
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][5, 2]);
						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][6, 2]);
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][7, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][7, 20].ToString());
						AssertEquals("=U5", excelInterface.WorkSheets[0].GetCellFormula(7, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith0X2Data_ShowEvenWithNoData()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSectionsShowHeaderFootersEvenWithNoData.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 0));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 6));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][2, 2]);
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][6, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][6, 20].ToString());
						AssertEquals("=SUM(U5:U6)", excelInterface.WorkSheets[0].GetCellFormula(6, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][7, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][7, 20].ToString());
						AssertEquals("=U7", excelInterface.WorkSheets[0].GetCellFormula(7, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith2X2Data_ShowEvenWithNoData()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSectionsShowHeaderFootersEvenWithNoData.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 6));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 6));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][4, 20].ToString());
						AssertEquals("=SUM(U3:U4)", excelInterface.WorkSheets[0].GetCellFormula(4, 20));
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][5, 2]);
						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][8, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][8, 20].ToString());
						AssertEquals("=SUM(U7:U8)", excelInterface.WorkSheets[0].GetCellFormula(8, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][9, 2]);
						AssertEquals("6", excelInterface.WorkSheets[0][9, 20].ToString());
						AssertEquals("=U5+U9", excelInterface.WorkSheets[0].GetCellFormula(9, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith0X0Data()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSections.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 0));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 0));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][1, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith2X0Data()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSections.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 6));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 0));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][4, 20].ToString());
						AssertEquals("=SUM(U3:U4)", excelInterface.WorkSheets[0].GetCellFormula(4, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][5, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][5, 20].ToString());
						AssertEquals("=U5", excelInterface.WorkSheets[0].GetCellFormula(5, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith0X2Data()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSections.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 0));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 6));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][4, 20].ToString());
						AssertEquals("=SUM(U3:U4)", excelInterface.WorkSheets[0].GetCellFormula(4, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][5, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][5, 20].ToString());
						AssertEquals("=U5", excelInterface.WorkSheets[0].GetCellFormula(5, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith2X2Data()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSections.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 6));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 6));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][4, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][4, 20].ToString());
						AssertEquals("=SUM(U3:U4)", excelInterface.WorkSheets[0].GetCellFormula(4, 20));
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][5, 2]);
						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][8, 2]);
						AssertEquals("3", excelInterface.WorkSheets[0][8, 20].ToString());
						AssertEquals("=SUM(U7:U8)", excelInterface.WorkSheets[0].GetCellFormula(8, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][9, 2]);
						AssertEquals("6", excelInterface.WorkSheets[0][9, 20].ToString());
						AssertEquals("=U5+U9", excelInterface.WorkSheets[0].GetCellFormula(9, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith40X40Data()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSections.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 120));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 120));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("section footer1:", excelInterface.WorkSheets[0][42, 2]);
						AssertEquals("2340", excelInterface.WorkSheets[0][42, 20].ToString());
						AssertEquals("=SUM(U3:U42)", excelInterface.WorkSheets[0].GetCellFormula(42, 20));
						AssertEquals("2340", excelInterface.WorkSheets[0][42, 20].ToString());
						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][43, 2]);

						AssertEquals("#SectionPageFooter 2", excelInterface.WorkSheets[0][58, 10]);
						AssertEquals("=SUM(U45:U57)", excelInterface.WorkSheets[0].GetCellFormula(58, 20));

						AssertEquals("PageFooter  ", excelInterface.WorkSheets[0][59, 2]);
						AssertEquals("=SUM(U3:U42,U45:U57)", excelInterface.WorkSheets[0].GetCellFormula(59, 20));

						AssertEquals("SectionPageHeader 2", excelInterface.WorkSheets[0][60, 2]);

						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][88, 2]);
						AssertEquals("2340", excelInterface.WorkSheets[0][88, 20].ToString());
						AssertEquals("=SUM(U45:U57,U62:U88)", excelInterface.WorkSheets[0].GetCellFormula(88, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][89, 2]);
						AssertEquals("4680", excelInterface.WorkSheets[0][89, 20].ToString());
						AssertEquals("=U43+U89", excelInterface.WorkSheets[0].GetCellFormula(89, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTwoSectionsWith80X40Data()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TwoSections.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount1", 240));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("MaxAmount2", 120));
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Document header", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("SectionHeader1", excelInterface.WorkSheets[0][1, 2]);

						AssertEquals("#SectionPageFooter 1", excelInterface.WorkSheets[0][58, 10]);
						AssertEquals("=SUM(U3:U57)", excelInterface.WorkSheets[0].GetCellFormula(58, 20));
						AssertEquals("PageFooter  ", excelInterface.WorkSheets[0][59, 2]);
						AssertEquals("=SUM(U3:U57)", excelInterface.WorkSheets[0].GetCellFormula(59, 20));

						AssertEquals("SectionPageHeader1", excelInterface.WorkSheets[0][60, 2]);

						AssertEquals("section footer1:", excelInterface.WorkSheets[0][86, 2]);
						AssertEquals("9480", excelInterface.WorkSheets[0][86, 20].ToString());
						AssertEquals("=SUM(U3:U57,U62:U86)", excelInterface.WorkSheets[0].GetCellFormula(86, 20));

						AssertEquals("SectionHeader 2", excelInterface.WorkSheets[0][87, 2]);

						AssertEquals("PageFooter  ", excelInterface.WorkSheets[0][119, 2]);
						AssertEquals("=SUM(U62:U86,U89:U117)", excelInterface.WorkSheets[0].GetCellFormula(119, 20));

						AssertEquals("SectionPageHeader 2", excelInterface.WorkSheets[0][120, 2]);

						AssertEquals("section footer 2:", excelInterface.WorkSheets[0][132, 2]);
						AssertEquals("2340", excelInterface.WorkSheets[0][132, 20].ToString());
						AssertEquals("=SUM(U89:U117,U122:U132)", excelInterface.WorkSheets[0].GetCellFormula(132, 20));
						AssertEquals("DocumentFooter", excelInterface.WorkSheets[0][133, 2]);
						AssertEquals("11820", excelInterface.WorkSheets[0][133, 20].ToString());
						AssertEquals("=U87+U133", excelInterface.WorkSheets[0].GetCellFormula(133, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleGroupBys()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleGroupBys.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Group title ", excelInterface.WorkSheets[0][0, 1]);
						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][0, 3]);
						AssertEquals("Group By", excelInterface.WorkSheets[0][33, 1]);
						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][33, 3]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleGroupBysTotalInSecondGroupBy()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleGroupBysTotalInSecondGroupBy.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("=G1+G3+G5+G7+G9+G11+G13+G15+G17+G19+G21+G23+G25+G27+G29+G31+G33+G35+G37+G39+G41+G43+G45+G47+G49+G51+G53+G55+G57+G59+G61+G63", excelInterface.WorkSheets[0].GetCellFormula(64, 6));
					}
				}
			}
		}

		public void TestSave()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));

			using (var report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("", excelInterface.WorkSheets[0][0, 1].ToString().Trim());
						AssertEquals("TestTemplate Header1", excelInterface.WorkSheets[0][1, 2].ToString().Trim());
						AssertEquals("Here", excelInterface.WorkSheets[0][16, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
						AssertEquals("=SUM(Q24:Q50)", excelInterface.WorkSheets[0].GetCellFormula(51, 16));
						AssertEquals("=SUM(U24:U50,U65:U69)", excelInterface.WorkSheets[0].GetCellFormula(69, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalGettingTheFormulaInsteadOfField()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TotalGettingTheFormulaInsteadOfField.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("=SUM(Q24:Q50)", excelInterface.WorkSheets[0].GetCellFormula(51, 16));
						AssertEquals("=SUM(U24:U50,U65:U69)", excelInterface.WorkSheets[0].GetCellFormula(69, 20));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContinuousLayout()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("CONTINUOUSLayout.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("TestTemplate Header1                                 ", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][26, 2]);
						AssertEquals("Unit test Line number 159               ", excelInterface.WorkSheets[0][189, 2]);
						AssertEquals("section footer:", excelInterface.WorkSheets[0][191, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoesntRenderEmptySectionsFooter()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BookingSummaryDoc.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Vanguard Logistics Services (Aust) Pty Ltd", excelInterface.WorkSheets[0][0, 7].ToString().Trim());
						AssertEquals("Summary Booking Sheet", excelInterface.WorkSheets[0][3, 1].ToString().Trim());
						AssertEquals("Vessel Name:", excelInterface.WorkSheets[0][4, 1].ToString().Trim());
						AssertEquals("Booking No", excelInterface.WorkSheets[0][9, 1].ToString().Trim());
						AssertEquals("Warehouse Status: Non Received", excelInterface.WorkSheets[0][12, 1].ToString().Trim());
						AssertEquals("Summary Booking Sheet", excelInterface.WorkSheets[0][3, 1].ToString().Trim());
						AssertEquals("Summary Booking Sheet", excelInterface.WorkSheets[0][3, 1].ToString().Trim());
						AssertEquals("Summary Booking Sheet", excelInterface.WorkSheets[0][3, 1].ToString().Trim());
						AssertEquals("Summary Booking Sheet", excelInterface.WorkSheets[0][3, 1].ToString().Trim());
						AssertEquals("=SUM(M14:M15)", excelInterface.WorkSheets[0].GetCellFormula(15, 12));
						AssertEquals("=SUM(M19:M21)", excelInterface.WorkSheets[0].GetCellFormula(21, 12));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupBy()
		{
			TestData.CreateLinesTestTable();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupBy1.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][33, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][34, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][67, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][68, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][101, 2]);
						AssertEquals("Accounting Group 3                      ", excelInterface.WorkSheets[0][102, 2]);
						AssertEquals("Accounting Group 3                      ", excelInterface.WorkSheets[0][135, 2]);
						AssertEquals("Accounting Group 4                      ", excelInterface.WorkSheets[0][136, 2]);
						AssertEquals("Accounting Group 4                      ", excelInterface.WorkSheets[0][169, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupTitleByBeforeGroupBy()
		{
			TestData.CreateLinesTestTable();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupBy2.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("Accounting Group 0                      ", excelInterface.WorkSheets[0][33, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][34, 2]);
						AssertEquals("Accounting Group 1                      ", excelInterface.WorkSheets[0][67, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][68, 2]);
						AssertEquals("Accounting Group 2                      ", excelInterface.WorkSheets[0][101, 2]);
						AssertEquals("Accounting Group 3                      ", excelInterface.WorkSheets[0][102, 2]);
						AssertEquals("Accounting Group 3                      ", excelInterface.WorkSheets[0][135, 2]);
						AssertEquals("Accounting Group 4                      ", excelInterface.WorkSheets[0][136, 2]);
						AssertEquals("Accounting Group 4                      ", excelInterface.WorkSheets[0][169, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLandscape()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("LandscapeReport.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("", excelInterface.WorkSheets[0][0, 1].ToString().Trim());
						AssertEquals("TestTemplate Header1", excelInterface.WorkSheets[0][1, 2].ToString().Trim());
						AssertEquals("Here", excelInterface.WorkSheets[0][16, 2].ToString().Trim());
						AssertEquals("Unit test Line number 0", excelInterface.WorkSheets[0][23, 2].ToString().Trim());
						AssertEquals("=SUM(Q24:Q31)", excelInterface.WorkSheets[0].GetCellFormula(32, 16));
						AssertEquals("=U84+U131+U192+U253+U300", excelInterface.WorkSheets[0].GetCellFormula(300, 6));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExcelFormulasAreReplaced()
		{
			DocumentWrapperForTesting dummyWrapper = new DocumentWrapperForTesting("Val\"ue");
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("replaceExcelFormulasInCells.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, dummyWrapper, "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("=UPPER(\"Val\"\"ue\")", ((TFormula)excelInterface.WorkSheets[0][0, 4]).Text);
						AssertEquals("=UPPER(\"test\")", ((TFormula)excelInterface.WorkSheets[0][0, 6]).Text);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreakOnlyInFirstPage()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("PageFooterOnlyInFirstPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("", excelInterface.WorkSheets[0][0, 1].ToString().Trim());
						AssertEquals("TestTemplate Header1", excelInterface.WorkSheets[0][1, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][52, 2].ToString().Trim());
						AssertEquals("PageFooter", excelInterface.WorkSheets[0][52, 2].ToString().Trim());
						AssertEquals("TestTemplate Header1", excelInterface.WorkSheets[0][113, 2].ToString().Trim());
					}
				}
			}
		}

		public void TestBackPageOnlyInFirstPage()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#BackPage:FirstPageOnly]
{B}-[BackPageContent]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>: <Collection.Z0_VarCharMax>]
{A}-[#GroupBy:Collection.Z0_Number:GroupTitle]
{B}-[<Collection.Z0_Number>]
{A}-[#GroupBy:Collection.Z0_VarCharMax:GroupTitle:PageBreak]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBODocSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;
			child1.Z0_VarCharMax = "Child 1";

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;
			child2.Z0_VarCharMax = "Child 2";

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					using (var stream = new MemoryStream())
					{
						report.PrepareForRender();
						report.Save(stream);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream);

							AssertMultilineASCIIEquals("There should be 2 pages with 1 back page.",
@"{B}-[Page 1 of 2]
{B}-[Child 1]
{B}-[1]
{B}-[1: Child 1]
{B}-[BackPageContent]
{B}-[Page 2 of 2]
{B}-[Child 2]
{B}-[2]
{B}-[2: Child 2]",
								excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestBackPageInAllPages()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#BackPage]
{B}-[BackPageContent]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>: <Collection.Z0_VarCharMax>]
{A}-[#GroupBy:Collection.Z0_Number:GroupTitle]
{B}-[<Collection.Z0_Number>]
{A}-[#GroupBy:Collection.Z0_VarCharMax:GroupTitle:PageBreak]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBODocSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;
			child1.Z0_VarCharMax = "Child 1";

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;
			child2.Z0_VarCharMax = "Child 2";

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					using (var stream = new MemoryStream())
					{
						report.PrepareForRender();
						report.Save(stream);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream);

							AssertMultilineASCIIEquals("There should be 2 pages with 2 back pages.",
@"{B}-[Page 1 of 2]
{B}-[Child 1]
{B}-[1]
{B}-[1: Child 1]
{B}-[BackPageContent]
{B}-[Page 2 of 2]
{B}-[Child 2]
{B}-[2]
{B}-[2: Child 2]
{B}-[BackPageContent]",
								excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastPageFooter()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("LastPageFooter.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("", excelInterface.WorkSheets[0][0, 1].ToString().Trim());
						AssertEquals(10258, excelInterface.WorkSheets[0].GetRowHeight(71) + excelInterface.WorkSheets[0].GetRowHeight(72));
						AssertEquals("LastPageFooter", excelInterface.WorkSheets[0][73, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastPageFooterWithBorder()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("LastPageFooterWithBorder.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals(12958, excelInterface.WorkSheets[0].GetRowHeight(58) + excelInterface.WorkSheets[0].GetRowHeight(59));
						AssertEquals(TFlxBorderStyle.Medium_dashed, excelInterface.Xls.GetFormat(excelInterface.Xls.GetCellFormat(59, 3)).Borders.Top.Style);
						AssertEquals(TFlxBorderStyle.None, excelInterface.Xls.GetFormat(excelInterface.Xls.GetCellFormat(60, 3)).Borders.Top.Style);
						AssertEquals(TFlxBorderStyle.Dotted, excelInterface.Xls.GetFormat(excelInterface.Xls.GetCellFormat(61, 3)).Borders.Top.Style);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBiggerThanAPageDocumentFooter()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BiggerThanAPageDocumentFooter.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("† For the purpose of the IMDG Code, see 5.4.2 (see also note 2 on notes page).", excelInterface.WorkSheets[0][46, 1].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBiggerThanAPageLastPageFooter()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("IMO Template.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("* DANGEROUS GOODS: You must specify - UN Number, proper shipping name, class or division and packing group, (where assigned) marine pollutant and observe the mandatory requirements under applicable national and international governmental regulations. For the purposes of the IMDG Code see 5.4.1.4 (see note 1 on notes page).", excelInterface.WorkSheets[0][46, 1].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRemoveSpecifiedImages()
		{
			using (StreamReader streamReader = new StreamReader(UnitTestingConstants.TestFilesDir + "RemoveSpecifiedImages.xls"))
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(streamReader.BaseStream);
					string[] embeddedObjects = excelInterface.WorkSheets[0].GetObjectNames();
					AssertEquals("Precondition: There should be 3 embedded objects", 3, embeddedObjects.Length);
					Assert("Precondition: Embedded objects should contain 'Little Rectangle'", ((IList)embeddedObjects).Contains("Little Rectangle"));
					Assert("Precondition: Embedded objects should contain 'Big Rectangle'", ((IList)embeddedObjects).Contains("Big Rectangle"));
					Assert("Precondition: Embedded objects should contain 'Draft Image'", ((IList)embeddedObjects).Contains("Draft Image"));
				}
			}

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("RemoveSpecifiedImages.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, new RemoveImagesDocWrapperForTesting(), "Test Report", new UserControlProviderList(), DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						string[] embeddedObjects = excelInterface.WorkSheets[0].GetObjectNames();

						AssertEquals("There should only be one embedded object - two should have been removed on report save", 1, embeddedObjects.Length);
						Assert("Embedded objects should contain 'Big Rectangle'", ((IList)embeddedObjects).Contains("Big Rectangle"));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCorrectImageIfSectionBodyAreaIsSplit()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObjectWithImage>();

			var image1 = Factory.New<DummyImage>();
			image1.Image = new System.Drawing.Bitmap(500, 500);
			var image2 = Factory.New<DummyImage>();
			image2.Image = new System.Drawing.Bitmap(600, 600);
			var image3 = Factory.New<DummyImage>();
			image3.Image = new System.Drawing.Bitmap(700, 700);

			topLevelDataSource.Images = new ImageWrapperList(Factory) { image1, image2, image3 };

			var excelTemplate = new ExcelTemplateForUnitTesting("ImagesInSectionBodyArea.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test Report", new UserControlProviderList(), DocumentDirection.ANY, false))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals(3, excelInterface.WorkSheets[0].GetImageCount());
						AssertEquals(new System.Drawing.Size(500, 500), excelInterface.WorkSheets[0].GetImage(0).Size);
						AssertEquals(new System.Drawing.Size(600, 600), excelInterface.WorkSheets[0].GetImage(1).Size);
						AssertEquals(new System.Drawing.Size(700, 700), excelInterface.WorkSheets[0].GetImage(2).Size);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRemoveSpecifiedImagesExceptionHandling()
		{
			using (StreamReader streamReader = new StreamReader(UnitTestingConstants.TestFilesDir + "RemoveSpecifiedImages.xls"))
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(streamReader.BaseStream);
					string[] embeddedObjects = excelInterface.WorkSheets[0].GetObjectNames();
					AssertEquals("Precondition: There should be 3 embedded objects", 3, embeddedObjects.Length);
					Assert("Precondition: Embedded objects should contain 'Little Rectangle'", ((IList)embeddedObjects).Contains("Little Rectangle"));
					Assert("Precondition: Embedded objects should contain 'Big Rectangle'", ((IList)embeddedObjects).Contains("Big Rectangle"));
					Assert("Precondition: Embedded objects should contain 'Draft Image'", ((IList)embeddedObjects).Contains("Draft Image"));
				}
			}

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("RemoveSpecifiedImages.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, new InvalidRemoveImagesDocWrapperForTesting(), "Test Report", new UserControlProviderList(), DocumentDirection.ANY, false))
			{
				report.StTemplate = Factory.NewWithValidTestData<StmTemplate>();
				report.StTemplate.SO_IsClientSpecific = false;
				report.StTemplate.SO_IsSystemDefined = true;

				using (MemoryStream outputStream = new MemoryStream())
				{
					try
					{
						report.Save(outputStream);
						Fail("Saving should fail as report renderer is trying to remove a component that doens't exist on the report");
					}
					catch (Exception ex)
					{
						AssertNotNull(ex);
						Assert($"Expected not to send error report as document is not client specific. Exception message: {ex.Message}", ex.ToString().Contains("Severity: [Warning (without error report)]"));
					}
				}
			}

			using (Report clientReport = new Report(Pack, excelTemplate, new InvalidRemoveImagesDocWrapperForTesting(), "Test Report", new UserControlProviderList(), DocumentDirection.ANY, false))
			{
				clientReport.StTemplate = Factory.New<StmTemplate>();
				clientReport.StTemplate.SO_IsClientSpecific = true;
				clientReport.StTemplate.SO_IsSystemDefined = true;

				using (MemoryStream outputStream = new MemoryStream())
				{
					try
					{
						clientReport.Save(outputStream);
						Fail("Saving should fail as report renderer is trying to remove a component that doens't exist on the report");
					}
					catch (Exception ex)
					{
						AssertNotNull(ex);
						Assert("Expected not to send error report as document is not client specific", ex.ToString().Contains("Severity: [Warning]"));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollection1()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.GroupByCollection[2].Selected = false;
				report.GroupByCollection[0].Selected = true;
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("N", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("Y", excelInterface.WorkSheets[0][7, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollection2()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.GroupByCollection[2].Selected = false;
				report.GroupByCollection[1].Selected = true;
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("N", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("Y", excelInterface.WorkSheets[0][3, 2]);
						AssertEquals("Test1", excelInterface.WorkSheets[0][4, 2].ToString().Trim());

						AssertEquals("N", excelInterface.WorkSheets[0][6, 2]);
						AssertEquals("Y", excelInterface.WorkSheets[0][8, 2]);
						AssertEquals("Test2", excelInterface.WorkSheets[0][9, 2].ToString().Trim());

						AssertEquals("N", excelInterface.WorkSheets[0][11, 2]);
						AssertEquals("Y", excelInterface.WorkSheets[0][13, 2]);
						AssertEquals("Test3", excelInterface.WorkSheets[0][14, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollection3()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.GroupByCollection[2].Selected = true;
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals("", excelInterface.WorkSheets[0][33, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollection4()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.GroupByCollection[2].Selected = false;
				report.GroupByCollection[3].Selected = true;
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Test1", excelInterface.WorkSheets[0][2, 2].ToString().Trim());
						AssertEquals("Test2", excelInterface.WorkSheets[0][5, 2].ToString().Trim());
						AssertEquals("Test3", excelInterface.WorkSheets[0][8, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreakOverride()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPageWithContinuousPageStyle.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				foreach (GroupBy groupby in report.GroupByCollection)
				{
					groupby.Selected = false;
				}
				report.GroupByCollection[3].Selected = true;
				report.GroupByCollection.BreakPageOverride = true;
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
				}
				Assert("BreakPage in this area should be true", report.Analyser.Areas[2].BreakPage);
				Assert("BreakPage in this area should be true", report.Analyser.Areas[4].BreakPage);
				Assert("BreakPage in this area should be true", report.Analyser.Areas[6].BreakPage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollectionWithRearrange()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPageWithRearrange.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Test4", excelInterface.WorkSheets[0][50, 2].ToString().Trim());
						AssertEquals("RL_HasRail", excelInterface.WorkSheets[0][46, 1].ToString().Trim());
						AssertEquals("RL_HasRoad", excelInterface.WorkSheets[0][42, 1].ToString().Trim());
						AssertEquals("RL_HasPost", excelInterface.WorkSheets[0][39, 1].ToString().Trim());
						AssertEquals("Airport", excelInterface.WorkSheets[0][44, 1].ToString().Trim());

						AssertEquals("Test1", excelInterface.WorkSheets[0][10, 2].ToString().Trim());
						AssertEquals("Test2", excelInterface.WorkSheets[0][21, 2].ToString().Trim());
						AssertEquals("Test3", excelInterface.WorkSheets[0][32, 2].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollectionWithRearrangeAndGroupTitle()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPageWithRearrangeAndGroupTitle.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Virgin Islands, U.S.", excelInterface.WorkSheets[0][31, 2].ToString().Trim());
						AssertEquals("Airport Start", excelInterface.WorkSheets[0][0, 1].ToString().Trim());
						AssertEquals("RL_HasRail start", excelInterface.WorkSheets[0][1, 1].ToString().Trim());
						AssertEquals("RL_HasRail end", excelInterface.WorkSheets[0][8, 1].ToString().Trim());
						AssertEquals("RL_HasRoad", excelInterface.WorkSheets[0][9, 1].ToString().Trim());
						AssertEquals("RL_HasRail start", excelInterface.WorkSheets[0][10, 1].ToString().Trim());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnReArrangeDeletesColumns()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("SingleTemplateWithHiddenReferences.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.PrepareForRender();
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Incorrect hidden column deleting", (string)(excelInterface.WorkSheets[0][0, 1]), "Column2");
						AssertEquals("Column should not be hidden", false, excelInterface.WorkSheets[0].IsColumnHidden(1));
						AssertEquals("Incorrect hidden column deleting", (string)(excelInterface.WorkSheets[0][0, 2]), "Column4");
						AssertEquals("Column should not be hidden", false, excelInterface.WorkSheets[0].IsColumnHidden(2));
						AssertEquals("Incorrect hidden column deleting", (string)(excelInterface.WorkSheets[0][0, 3]), "Column7");
						AssertEquals("Column should not be hidden", false, excelInterface.WorkSheets[0].IsColumnHidden(3));
						AssertEquals("Incorrect hidden column deleting", (string)(excelInterface.WorkSheets[0][0, 4]), "Column1");
						AssertEquals("Column should be hidden", true, excelInterface.WorkSheets[0].IsColumnHidden(4));
						AssertEquals("Incorrect hidden column deleting", (string)(excelInterface.WorkSheets[0][0, 5]), "Column3");
						AssertEquals("Column should be hidden", true, excelInterface.WorkSheets[0].IsColumnHidden(5));
						AssertEquals("Incorrect hidden column deleting", (string)(excelInterface.WorkSheets[0][0, 6]), "");
						AssertEquals("Column should be deleted", false, excelInterface.WorkSheets[0].IsColumnHidden(6));
					}
				}
			}
		}

		public void TestColumnReArrangeMovesColumns()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalColumns.xls", "MultipleTemplatesWithOptionalColumns.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalColumns.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			using (var outputStream = new MemoryStream())
			{
				report.PrepareForRender();
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].CurrentPosition = 1;
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].CurrentPosition = 0;
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].CurrentPosition = 2;

				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[0].CurrentPosition = 0;
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[1].CurrentPosition = 1;
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[2].CurrentPosition = 2;

				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[0].CurrentPosition = 2;
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[1].CurrentPosition = 1;
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[2].CurrentPosition = 0;

				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);

					AssertEquals("Column 2 on worksheet 0 now in first position", "Column 2", excelInterface.WorkSheets[0][1, 1]);
					AssertEquals("Column 1 on worksheet 0 now in second position", "Column 1", excelInterface.WorkSheets[0][1, 2]);
					AssertEquals("Column 3 on worksheet 0 still in third position", "Column 3", excelInterface.WorkSheets[0][1, 3]);

					AssertEquals("Column 1 on worksheet 1 still in first position", "Column 1", excelInterface.WorkSheets[1][1, 1]);
					AssertEquals("Column 2 on worksheet 1 still in second position", "Column 2", excelInterface.WorkSheets[1][1, 2]);
					AssertEquals("Column 3 on worksheet 1 still in third position", "Column 3", excelInterface.WorkSheets[1][1, 3]);

					AssertEquals("Column 3 on worksheet 2 now in first position", "Column 3", excelInterface.WorkSheets[2][1, 1]);
					AssertEquals("Column 2 on worksheet 2 still in second position", "Column 2", excelInterface.WorkSheets[2][1, 2]);
					AssertEquals("Column 1 on worksheet 2 now in third position", "Column 1", excelInterface.WorkSheets[2][1, 3]);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnRearrangeSetsColumnWidth()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.WorkSheetCurrentlyBeingProcessed[5, 0] = "ColumnHeadings";
				report.WorkSheetCurrentlyBeingProcessed[5, 4] = "DisplayLabel = \"Port\"";
				report.WorkSheetCurrentlyBeingProcessed[5, 5] = "DisplayLabel = \"IATA\", hidden";
				report.WorkSheetCurrentlyBeingProcessed[5, 6] = "DisplayLabel = \"Country\"";

				report.WorkSheetCurrentlyBeingProcessed[7, 0] = "#DocumentHeader";
				report.WorkSheetCurrentlyBeingProcessed[8, 4] = "Port";
				report.WorkSheetCurrentlyBeingProcessed[8, 5] = "IATA";
				report.WorkSheetCurrentlyBeingProcessed[8, 6] = "Country";

				report.PrepareForRender();
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0].WidthInPixels = 100;

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("Column 4 Value", "Port", excelInterface.WorkSheets[0][0, 4]);
						AssertEquals("Column 4 Width", 3657, excelInterface.WorkSheets[0].GetColWidth(4));
						AssertEquals("Column 5 Value", "Country", excelInterface.WorkSheets[0][0, 5]);
						AssertEquals("Column 5 Width", 2304, excelInterface.WorkSheets[0].GetColWidth(5));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAreaTranslated()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TranslationTest.xls", TestFilesSubFolder.ReportTestFiles);
			var reportTranslateHelper = new ReportTemplateTranslationHelper();

			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			using (var document = new Report(Pack, excelTemplate, new DataProviderList(new DeliveryInstructions()), "Anything", null, DocumentDirection.ANY, true))
			using (var resourceStrings = Res.UseMockData())
			{
				var stTemplate = Factory.New<StmTemplateBase>();
				stTemplate.SO_ExcelTemplatePath = excelTemplate.TemplateSourceLocation;
				stTemplate.SO_IsSystemDefined = true;
				report.StTemplate = stTemplate;

				var key1 = DocBuilderResourceStrings.GetKey(Path.GetFileNameWithoutExtension(excelTemplate.TemplateSourceLocation), DocBuilderResourceStrings.ReportLabelKeyPrefix, "Hello");
				var key2 = DocBuilderResourceStrings.GetKey(Path.GetFileNameWithoutExtension(excelTemplate.TemplateSourceLocation), DocBuilderResourceStrings.ReportLabelKeyPrefix, "Hello world");
				resourceStrings.Put(key1, new ResourceStringData(key1, "Bonjour"));
				resourceStrings.Put(key2, new ResourceStringData(key2, "Bonjour tout le monde"));

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("Cell Content should have been translated for report", "Bonjour", excelInterface.WorkSheets[0][0, 1]);
						AssertEquals("Cell Content should have been translated for report", "Bonjour tout le monde", excelInterface.WorkSheets[0][1, 1]);
					}
				}

				document.StTemplate = stTemplate;
				using (MemoryStream stream = new MemoryStream())
				{
					document.Save(stream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("Cell Content should not have been translated for document", "Hello", excelInterface.WorkSheets[0][0, 1]);
						AssertEquals("Cell Content should not have been translated for document", "Hello world", excelInterface.WorkSheets[0][1, 1]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderWithReallyLargeColumnDisplayWidth()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				var sheet = report.ColumnHeadingManager.CurrentConfiguration.Worksheets.AddNew("Sheet1");

				var reallyLargeColumnWidthInPixels = int.MaxValue;
				sheet.ColumnHeadings.Add(new ColumnHeading("1", "1", "1", 1, 1, reallyLargeColumnWidthInPixels, false));

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					AssertEquals(false, report.ErrorManager.HasErrors);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderWithRemovingAllPropertiesForXls()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("BookingSummaryDoc.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = GetNewReportWithNoExceptionOnErrors(excelTemplate))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.Author)?.ToString());
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.Category)?.ToString());
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.Comments)?.ToString());
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.Company)?.ToString());
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.Manager)?.ToString());
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.Notes)?.ToString());
						AssertNullOrEmpty(excelInterface.Xls.DocumentProperties.GetStandardProperty(TPropertyId.LastSavedBy)?.ToString());
					}
				}
			}
		}

		[GuiTest]
		public void TestMaxConcurrentReportsRaisesError()
		{
			SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			Globals.IsWeb = false;

			using (var mutex = new ReportMutex())
			using (var reportToLock = new Report(new DocumentPack(), SimpleTestTemplate))
			{
				mutex.Lock(reportToLock);

				using (var reportToRender = new Report(new DocumentPack(), SimpleTestTemplate, "Data provider test report", ContactType.All, false))
				using (var stream = new MemoryStream())
				using (Report.TemporarilyStopErrorsThrowingAnException())
				{
					reportToRender.Save(stream);

					string expectedMessage = "Maximum Concurrent Report Limit Reached.\r\n\r\nThe following reports are currently running...\r\n\r\n" + mutex.GetFormattedCurrentLocks() + "\r\nPlease try again in a few minutes.";
					AssertEquals("Report.Errors", string.Format(@"Severity: [Fatal Error (without error report)] Message: [{0}] Cell: [N/A] Sheetname: [(unknown)]", expectedMessage),
								reportToRender.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}
			}
		}

		public void TestMaxConcurrentReportsForWebDontRaisesError()
		{
			SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			Globals.IsWeb = true;

			using (var mutex = new ReportMutex())
			using (var reportToLock = new Report(new DocumentPack(), SimpleTestTemplate))
			{
				mutex.Lock(reportToLock);

				using (var reportToRender = new Report(new DocumentPack(), SimpleTestTemplate, "Data provider test report", ContactType.All, false))
				{
					using (var stream = new MemoryStream())
					{
						reportToRender.Save(stream);
						AssertNotContains("Maximum Concurrent Report Limit Reached.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestNotMaxConcurrentReportsDontRaisesError()
		{
			SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			Globals.IsWeb = false;

			using (var report = new Report(new DocumentPack(), SimpleTestTemplate, "Data provider test report", ContactType.All, false))
			using (var reportToRender = new Report(new DocumentPack(), SimpleTestTemplate, "Data provider test report", ContactType.All, false))
			{
				using (var stream = new MemoryStream())
				{
					reportToRender.Save(stream);
					AssertNotContains("Maximum Concurrent Report Limit Reached.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParametersArePassedToParametrizedDocWrapper()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ExactTextFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			ParametrizedDocumentWrapperForTesting dummyParametrizedDocWrapper = new ParametrizedDocumentWrapperForTesting();
			using (Report report = new Report(Pack, excelTemplate, dummyParametrizedDocWrapper, "Test Report", null, DocumentDirection.ANY, false))
			{
				AssertEquals("Prerequisite: filters collection should be empty", 0, report.FilterCollection.Count);
				report.PrepareForRender();
				AssertEquals("Count", 1, report.FilterCollection.Count);
				AssertNull("Parameters are not populated yet", dummyParametrizedDocWrapper.Parameters);
				report.Renderer.Render();
				AssertEquals("Parameters are now populated", 1, dummyParametrizedDocWrapper.Parameters.Count);
				AssertEquals(dummyParametrizedDocWrapper.Parameters["Some description"], "Hello");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplateShapeOutOfRangeWithData()
		{
			var dummy = Factory.New<DummyBODocSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;
			child1.Z0_Code = "Code1";
			child1.Z0_Description = "description";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BVC Barcode Label Test.xls", TestFilesSubFolder.ReportTestFiles);
			ParametrizedDocumentWrapperForTesting dummyParametrizedDocWrapper = new ParametrizedDocumentWrapperForTesting();
			using (Report report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "Test Report", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Renderer.Render();

				Assert(!report.HasErrors);
				Assert(report.GetReportErrors().Any(e => e.Message.Contains("may cross the boundary of #SectionBody")));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplateRenderWithoutData()
		{
			var dummy = Factory.New<DummyBODocSupportable>();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BVC Barcode Label Test.xls", TestFilesSubFolder.ReportTestFiles);
			ParametrizedDocumentWrapperForTesting dummyParametrizedDocWrapper = new ParametrizedDocumentWrapperForTesting();
			using (Report report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "Test Report", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Renderer.Render();

				Assert(!report.HasErrors);
			}
		}

		public void TestUpdateSheetNameOverride()
		{
			using (var report = new Report(new DocumentPack(), SimpleTestTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed[2, 0] = "SheetNameOverride=Hello World Report";

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("Hello World Report", excelInterface.WorkSheets[0].SheetName);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMacrosInAutoShapesInSectionBody()
		{
			DummyBODocSupportable dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_NVarChar = "Comment1";
			for (var i = 0; i < 6; i++)
			{
				dummy.ZStringList.Add("String" + i);
			}

			var excelTemplate = new ExcelTemplateForUnitTesting("MacrosInAutoShapesInSectionBodyTest.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown("Should save with no exception", () => report.Save(outputStream));

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						excelInterface.Xls.ActiveSheet = 1;

						AssertEquals("Precondition", 12, excelInterface.Xls.ObjectCount);

						var shapes = new List<TShapeProperties>();
						for (int i = 1; i <= 12; i++)
						{
							shapes.Add(excelInterface.Xls.GetObjectProperties(i, false));
						}

						var commentShapes = shapes.Where(s => s.ObjectType == TObjectType.Comment).ToList();
						AssertEquals(6, commentShapes.Count);

						var drawnShapes = shapes.Where(s => s.ObjectType == TObjectType.MicrosoftOfficeDrawing).ToList();
						AssertEquals(6, drawnShapes.Count);

						CombineAssertions("Macros in Comments & Drawn Shapes should be translated correctly", () =>
						{
							for (var i = 0; i < 6; i++)
							{
								var comment = excelInterface.Xls.GetComment(1 + i, 3);
								AssertNotNull(comment);
								Assert(comment.Value.Contains("String" + i));

								Assert(drawnShapes[i].TextAsRichString(excelInterface.Xls).Value.Contains("String" + i));
							}
						});
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMacrosInAutoShapes()
		{
			DummyBODocSupportable dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_NVarChar = "Comment1";
			for (var i = 0; i < 6; i++)
			{
				dummy.ZStringList.Add("String" + i);
			}

			var excelTemplate = new ExcelTemplateForUnitTesting("MacrosInAutoShapesTest.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown("Should save with no exception", () => report.Save(outputStream));

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						excelInterface.Xls.ActiveSheet = 1;

						AssertEquals("Precondition", 10, excelInterface.Xls.ObjectCount);

						List<TShapeProperties> shapes = new List<TShapeProperties>();
						for (int i = 1; i <= excelInterface.Xls.ObjectCount; i++)
						{
							shapes.Add(excelInterface.Xls.GetObjectProperties(i, false));
						}
						TShapeProperties textBox = shapes.Find((s) => s.ShapeName == "TextBox");
						AssertNotNull(textBox);
						AssertEquals("Macro should be processed", "1", textBox.Text.Value);
						TShapeProperties star = shapes.Find((s) => s.ShapeName == "Star");
						AssertNotNull(star);
						AssertEquals("No macros specified", "", star.Text == null ? "" : star.Text.Value);
						TShapeProperties heart = shapes.Find((s) => s.ShapeName == "Heart");
						AssertNotNull(heart);
						AssertEquals("Macro should be processed", GlbCompany.CurrentCompany.GC_Code, heart.Text.Value);

						CombineAssertions("Macros in Comments should be translated correctly", () =>
						{
							var comment = excelInterface.Xls.GetComment(11, 3);
							AssertNotNull(comment);
							Assert(comment.Value.Contains("Comment1"));

							for (var i = 0; i < 6; i++)
							{
								comment = excelInterface.Xls.GetComment(12 + i, 3);
								AssertNotNull(comment);
								Assert(comment.Value.Contains("String" + i));
							}
						});
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderMultiSheetsWithDifferentObjectCount()
		{
			DummyBODocSupportable dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_NVarChar = "Comment1";
			dummy.ZStringList.Add("String");

			var excelTemplate = new ExcelTemplateForUnitTesting("MacrosInAutoShapesMultiSheetsTest.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "", null, DocumentDirection.ANY, false))
			using (MemoryStream outputStream = new MemoryStream())
			{
				AssertNoExceptionThrown("Should save with no exception", () => report.Save(outputStream));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFooters()
		{
			AssertFooters(1);
			AssertFooters(50);
			AssertFooters(55);
			AssertFooters(70);
			AssertFooters(150);
			AssertFooters(200);
		}

		public void TestEmptySectionWithGroupByGroupTitlePageBreakDoesNotGenerateEmptyPage()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{A}-[#GroupBy:Collection.RelatedDummy.Z0_Code:GroupTitle:PageBreak]
{B}-[<Collection.RelatedDummy.Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			dummy.Collection.AddNew("A", "Aye");
			var b = dummy.Collection.AddNew("B", "Bee");
			var relatedDummy = Factory.New<DummyBusinessObject>();
			b.Z0_Guid = relatedDummy.PK;
			b.RelatedDummy.Z0_Code = "X";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be 1 page.",
@"{B}-[Page 1 of 1]

{B}-[X]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBarcodes()
		{
			var dummy = Factory.New<DummyBODocSupportable>();

			var excelTemplate = new ExcelTemplateForUnitTesting("BarcodesTest.xlsx", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), "", null, DocumentDirection.ANY, false))
			{
				using (var outputStream = new MemoryStream())
				{
					AssertNoExceptionThrown("Should save with no exception", () => report.Save(outputStream));

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals(6, excelInterface.Xls.ObjectCount);
					}
				}
			}
		}

		public void TestCurrentAreaToProcessNeedBackToOriginalAreaAfterProcess()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=TestMacro]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection]
{B}-[<Z0_Bool>]
{A}-[#EndOfReport]");

			var dummy = Factory.NewWithValidTestData<DummyBODocSupportable>();
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_ContactType = ContactType.Consignor.Code;
			documentCommand.SU_EmailSubjectLine = "<Collection.Z0_Bool>";

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var child = dummy.Collection.AddNew();
			Factory.Save();

			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var instructions = new DeliveryInstructions() { Destination = DeliveryInstructionDestination.TakenFromContact };
				var pack = printTask.GetFirstDocumentPack();
				var report = pack.GetFirstReport();
				report.PrepareForRender();

				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				AssertNotNull("converter.Components", converter.Components);

				var tableName = VisualiserDataSet.GetTableName("Collection");
				var dataRow = report.OverridingDataSet.Tables[tableName].Rows[0];
				report.OverridingDataSet.Tables[tableName].Rows.Remove(dataRow);
				pack.SaveVisualizerContentNote();
				report.Analyser.Sections[0].SectionBody.SetDataSource(null);

				AssertNoExceptionThrown(() => printTask.Run(instructions));
			}
		}

		public void TestExcelRowsAreHidedWhenHideRowsInsteadOfRemoveIsOn()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideRowsInsteadOfRemove]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Z0_Code]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			for (int i = 0; i < 10; i++)
			{
				dummy.Collection.AddNew(i.ToString(), "Description" + i);
			}

			using (var report = new Report(DocumentPack.EmptyPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("No row is removed", 32, excelInterface.WorkSheets[0].RowCount);
						AssertEquals("Row is hidden", 0, excelInterface.WorkSheets[0].GetRowHeight(0));
					}
				}
			}
		}

		public void TestNoExceptionThrownWhenSticky()
		{
			var contents =
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideRowsInsteadOfRemove]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[PageStyle=Landscape]
{A}-[#DocumentHeader]
{A}-[#PageHeader]
{A}-[#SectionPageHeader]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{A}-[#GroupBy:Collection.Z0_Number:GroupTitle:Sticky]
{B}-[logo1]
{B}-[logo2]
{A}-[#GroupBy:Collection.Z0_Code:GroupTitle:PageBreak]
{A}-[#GroupBy:Collection.Z0_Code]
{A}-[#LastPageFooter]
{A}-[#EndOfReport]";
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, contents);

			var dummy = Factory.New<DummyBODocSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;
			child1.Z0_Code = "Code1";
			child1.Z0_Description = "description";

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 1;
			child2.Z0_Code = "Code1";
			child2.Z0_Description = "description";

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					var renderer = new ReportRenderer(report);
					AssertNoExceptionThrown("Should save with no exception", () => renderer.Render());
				}
			}
		}

		public void TestInsertBackPages_WhenTemplateHasBackPageOnly_ShouldNotThrowException()
		{
			var contents =
				@"{A}-[#Config]
{A}-[#BackPage]
{B}-[BackPageContent]
{A}-[#EndOfReport]";
			var backPageOnlyTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, contents);

			using (var report = new Report(Pack, backPageOnlyTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				var reportRender = new ReportRenderer(report);

				AssertNoExceptionThrown(reportRender.Render);
			}
		}

		public void TestCurrentPageMacroCanGetCorrectPageNumber()
		{
			var contents =
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionHeader]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#SectionBody]
{B}-[Page Number: <CurrentPage>]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[Page Number: <CurrentPage>]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[Page Number: <CurrentPage>]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{B}-[Page Number: <CurrentPage>]
{A}-[<HPageBreak>]
{B}-[WhatEver]
{B}-[WhatEver]
{A}-[#EndOfReport]";
			var reportTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, contents);

			using (var report = new Report(Pack, reportTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					var reportContentsAsString = excelInterface.WorkSheets[0].ToString();
					AssertContains("Should contains Page Number 1", "Page Number: 1", reportContentsAsString);
					AssertContains("Should contains Page Number 2", "Page Number: 2", reportContentsAsString);
					AssertContains("Should contains Page Number 3", "Page Number: 3", reportContentsAsString);
					AssertContains("Should contains Page Number 4", "Page Number: 4", reportContentsAsString);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestMergeCells_WhenCustomColumns_ShouldSameAsTemplate()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MergeCellsCustomColumns.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				report.PrepareForRender();
				var columnHeadings = report.ColumnHeadingManager.CurrentConfiguration
					.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings;
				columnHeadings[0].Hidden = true;
				columnHeadings[1].CurrentPosition = 0;
				columnHeadings[2].CurrentPosition = 2;
				columnHeadings[6].CurrentPosition = 4;
				columnHeadings[5].CurrentPosition = 1;
				columnHeadings[7].CurrentPosition = 3;
				columnHeadings[8].CurrentPosition = 5;

				var xls = report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.Xls;

				AssertEquals("The count of the merged list should be 5.", 5, xls.CellMergedListCount);

				report.Renderer.Render();

				AssertEquals("The count of the merged list should be 4.", 4, xls.CellMergedListCount);
				CombineAssertions(() =>
				{
					CheckCellRange(xls.CellMergedList(1), 2, 3, 2, 4);
					CheckCellRange(xls.CellMergedList(2), 3, 4, 3, 6);
					CheckCellRange(xls.CellMergedList(3), 4, 4, 4, 7);
					CheckCellRange(xls.CellMergedList(4), 5, 2, 5, 7);
				});
			}
		}

		void CheckCellRange(TXlsCellRange cellRange, int top, int left, int bottom, int right)
		{
			AssertEquals("Start row should be " + top, top, cellRange.Top);
			AssertEquals("Start column should be " + left, left, cellRange.Left);
			AssertEquals("End row should be " + bottom, bottom, cellRange.Bottom);
			AssertEquals("End column should be " + right, right, cellRange.Right);
		}

		#region Implementation

		void AssertFooters(int rowCount)
		{
			DummyBODocSupportable dummy = Factory.New<DummyBODocSupportable>();
			while (dummy.Collection.Count < rowCount)
			{
				dummy.Collection.AddNew();
			}

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Footers.xls", TestFilesSubFolder.AreaTestFiles);
			List<string> footers = new List<string>();

			using (Report report = new Report(Pack, excelTemplate, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			using (MemoryStream stream = new MemoryStream())
			{
				report.Save(stream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);
					for (int i = 1; i <= excelInterface.Xls.RowCount; i++)
					{
						string value = excelInterface.Xls.GetCellValue(i, 3) as string;
						if ((value != null) && value.EndsWith("Footer"))
						{
							footers.Add(value);
						}
					}
				}
			}

			Assert("No footers were found.", footers.Count > 0);
			if (footers.Count == 1)
			{
				AssertEquals("Only Page Footer", "OnlyOnePageFooter", footers[0]);
			}
			else
			{
				AssertEquals("First Page Footer", "FirstPageFooter", footers[0]);
				for (int i = 1; i < footers.Count - 1; i++)
				{
					AssertEquals("Middle Page Footer", "PageFooter", footers[i]);
				}
				AssertEquals("Last Page Footer", "LastPageFooter", footers[footers.Count - 1]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRemoveNonExistantObjectFromBillOfLading()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "BILL OF LADING";

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_HouseBillOfLadingType = "FIP";

			IBODocDataProvider[] dataProviders = ((IDocumentSupportable)shipment).DocumentSupporter.GetBODocDataProviders(new DataContextValue(nameof(DataContext.Shipment)), menuItem);
			AssertNotNull("Precondition: DataContext.Shipment should return one DocumentWrapper", dataProviders);
			AssertEquals("Precondition: DataContext.Shipment should return one DocumentWrapper", 1, dataProviders.Length);

			IBODocDataProvider dataProvider = dataProviders[0];
			AssertEquals("Precondition: shipment.GetType().Name", "ForwardingShipment", shipment.GetType().Name);
			AssertEquals("Precondition: dataProvider.GetType().Name", "DocForwardingShipment", dataProvider.GetType().Name);

			DocumentPack pack = new DocumentPack(menuItem);
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TLU Bill Of Lading TLU", "TLU Bill Of Lading TLU.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate, dataProvider, "TestOnly", null, DocumentDirection.ANY, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					Assert("Error manager has errors", !report.ErrorManager.HasErrors);
				}
			}
		}

		class ParametrizedDocumentWrapperForTesting : DocumentWrapperForTesting, IParametrizedDocWrapper
		{
			public ParametrizedDocumentWrapperForTesting()
				: base("value")
			{
			}

			#region IParametrizedDocWrapper Members

			public Dictionary<string, object> Parameters { get; set; }

			#endregion
		}

		DocumentPack Pack;

		protected override void SetUp()
		{
			base.SetUp();

			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			TestData.CreateEmptyLinesTestTable();
			Pack = new DocumentPack();
			CreateTestRefUNLOCOData();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
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

		IDisposable temporarilyUseMainConnection;
		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting simpleTestTemplate;
		ExcelTemplateForUnitTesting SimpleTestTemplate
		{
			get
			{
				if (simpleTestTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls", "SimpleTest.xls");
					simpleTestTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", Path.GetFullPath(tempFileName));
				}
				return simpleTestTemplate;
			}
		}

		void CreateTestRefUNLOCOData()
		{
			var refCountry1 = Factory.NewWithValidTestData<RefCountry>();
			refCountry1.RN_Code = "T1";
			refCountry1.RN_Desc = "Test1";

			var refCountry2 = Factory.NewWithValidTestData<RefCountry>();
			refCountry2.RN_Code = "T2";
			refCountry2.RN_Desc = "Test2";

			var refCountry3 = Factory.NewWithValidTestData<RefCountry>();
			refCountry3.RN_Code = "T3";
			refCountry3.RN_Desc = "Test3";

			var refCountry4 = Factory.NewWithValidTestData<RefCountry>();
			refCountry4.RN_Code = "T4";
			refCountry4.RN_Desc = "Test4";

			var refUNLOCO1_HasAirPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1_HasAirPort.RL_RN_NKCountryCode = "T1";
			refUNLOCO1_HasAirPort.RL_HasAirport = true;

			var refUNLOCO1_HasNoAirPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1_HasNoAirPort.RL_RN_NKCountryCode = "T1";
			refUNLOCO1_HasNoAirPort.RL_HasAirport = false;

			var refUNLOCO2_HasAirPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO2_HasAirPort.RL_RN_NKCountryCode = "T2";
			refUNLOCO2_HasAirPort.RL_HasAirport = true;

			var refUNLOCO2_HasNoPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO2_HasNoPort.RL_RN_NKCountryCode = "T2";
			refUNLOCO2_HasNoPort.RL_HasAirport = false;

			var refUNLOCO3_HasAirPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO3_HasAirPort.RL_RN_NKCountryCode = "T3";
			refUNLOCO3_HasAirPort.RL_HasAirport = true;

			var refUNLOCO3_HasNoAirPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO3_HasNoAirPort.RL_RN_NKCountryCode = "T3";
			refUNLOCO3_HasNoAirPort.RL_HasAirport = false;

			var refUNLOCO4_HasAirPort = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO4_HasAirPort.RL_RN_NKCountryCode = "T4";
			refUNLOCO4_HasAirPort.RL_HasAirport = true;

			var refUNLOCO4_HasRail = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO4_HasRail.RL_RN_NKCountryCode = "T4";
			refUNLOCO4_HasRail.RL_HasRail = true;

			var refUNLOCO4_HasRoad = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO4_HasRoad.RL_RN_NKCountryCode = "T4";
			refUNLOCO4_HasRoad.RL_HasRoad = true;

			var refUNLOCO4_HasPost = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO4_HasPost.RL_RN_NKCountryCode = "T4";
			refUNLOCO4_HasPost.RL_HasPost = true;

			Factory.Save();
		}

		Report GetNewReportWithNoExceptionOnErrors(ExcelTemplate excelTemplate)
		{
			Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest);
			((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
			return report;
		}

		class RemoveImagesDocWrapperForTesting : DocumentWrapper
		{
			protected override string[] ImageNamesToRemove
			{
				get { return new string[] { "Little Rectangle", "Draft Image" }; }
			}

			public override string ToString()
			{
				return "Test";
			}
		}

		class InvalidRemoveImagesDocWrapperForTesting : DocumentWrapper
		{
			protected override string[] ImageNamesToRemove
			{
				get { return new string[] { "Little Rectangle", "Draft Image", "Invalid element" }; }
			}

			public override string ToString()
			{
				return "Test";
			}
		}

		public class DummyBusinessObjectWithImage : DummyBusinessObject
		{
			public DummyBusinessObjectWithImage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ImageWrapperList Images { get; set; }
		}

		public class ImageWrapperList : BusinessObjectCollection<DummyImage>
		{
			public ImageWrapperList(BusinessObjectFactory factory) : base(factory) { }

			public new DummyImage this[int i]
			{
				get { return (DummyImage)Elements[i]; }
			}
		}

		public class DummyImage : DummyBusinessObject
		{
			public DummyImage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public System.Drawing.Image Image { get; set; }
		}

		#endregion
	}
}
