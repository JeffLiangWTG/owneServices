using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class ConfigAreaTest : AreaAbstractTest
	{
		[TestDate(2022, 8, 26, 1, 1, 1, 100)]
		public void TestReportColumnHeadingsReplaceMacrosWithZDateTime()
		{
			using (OrganisationsDataRegistry.Instance.PotentialLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PotentialLable"))
			{
				var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
					"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT 'A' as ColumnOne]
{A}-[ColumnHeadings:] {B}-[DisplayLabel=""DisplayLabel <Now> <OtherUnkonwColumn> <ReportData.ColumnOne>"", HeadingText=""HeadingText <Now> <OtherUnkonwColumn> <ReportData.ColumnOne>"",Description=""Description <Now> <OtherUnkonwColumn> <ReportData.ColumnOne>"",TagName=""TagName <Now> <OtherUnkonwColumn> <ReportData.ColumnOne>""] ]
{A}-[#DocumentHeader]
{B}-[OpportunityPotentialLablePotentialLable]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.ColumnOne>]
{A}-[#EndOfReport]");
				using (var documentPack = new DocumentPack(Factory.New<StmMenuItem>()))
				using (var report = new Report(documentPack, excelTemplate))
				{
					var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
					using (var outputStream = new MemoryStream())
					{
						var now = ZDateTime.Now.ToString();
						AssertNoExceptionThrown(() => report.Save(outputStream));
						AssertEquals($"DisplayLabel {now} <OtherUnkonwColumn> A", configArea.CurrentColumnHeadingsExposedForTest[0].DisplayLabel);
						AssertEquals($"HeadingText {now} <OtherUnkonwColumn> A", configArea.CurrentColumnHeadingsExposedForTest[0].HeadingText);
						AssertEquals($"Description {now} <OtherUnkonwColumn> A", configArea.CurrentColumnHeadingsExposedForTest[0].Description);
						AssertEquals($"TagName {now} <OtherUnkonwColumn> A", configArea.CurrentColumnHeadingsExposedForTest[0].TagName);
					}
				}
			}
		}

		public void TestForcedLanguage_WhenCodeIsValid_ShouldBeParsed()
		{
			AssertProcessForcedLanguageResult(@"{A}-[#Config]
{A}-[Name=Test]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=ZH-CN]", false, expectedForcedLanguage: "ZH-CN");
		}

		public void TestForcedLanguage_WhenCodeIsInvalid_ShouldShowWarningMessage()
		{
			AssertProcessForcedLanguageResult(@"{A}-[#Config]
{A}-[Name=Test]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=DE]", true, expectedErrorMessage: "Severity: [Warning] Message: [Forced language 'DE' is not valid, it has been changed to default empty.] Cell: [A4] Sheetname: [Document]");
		}

		public void TestForcedLanguage_WhenMacroContainsValidCode_ShouldBeParsed()
		{
			AssertProcessForcedLanguageResult(@"{A}-[#Config]
{A}-[Name=Test]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=<Z0_AddInfo>]", false, macro: "ZH-TW", expectedForcedLanguage: "ZH-TW");
		}

		public void TestForcedLanguage_WhenMacroContainsInvalidCode_ShouldShowWarningMessage()
		{
			AssertProcessForcedLanguageResult(@"{A}-[#Config]
{A}-[Name=Test]
{A}-[TranslateLegacyDocument]
{A}-[ForcedLanguage=<Z0_AddInfo>]", true, macro: "ZH", expectedErrorMessage: "Severity: [Warning] Message: [Forced language 'ZH' is not valid, it has been changed to default empty.] Cell: [A4] Sheetname: [Document]");
		}

		public void TestForcedLanguage_WhenTranslateLegacyDocumentNotExists_ShouldShowErrorMessage()
		{
			AssertProcessForcedLanguageResult(@"{A}-[#Config]
{A}-[Name=Test]
{A}-[ForcedLanguage=ZH-CN]", true, expectedErrorMessage: "Severity: [Warning] Message: [ForcedLanguage only works when used together with TranslateLegacyDocument.] Cell: [] Sheetname: [Document]");
		}

		void AssertProcessForcedLanguageResult(string templateContents, bool shouldHasErrorMessage, string macro = "ZH-CN", string expectedForcedLanguage = null, string expectedErrorMessage = "")
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents);

			var dataSource = Factory.New<DummyBusinessObject>();
			dataSource.Z0_AddInfo = macro;

			using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(dataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				var configArea = AreaFactory.InstantiateArea(1, 3, report, "#Config") as ConfigArea;

				if (!shouldHasErrorMessage)
				{
					AssertEquals("ForcedLanguage can be parsed correctly", expectedForcedLanguage, configArea.ForcedLanguage);
				}
				else
				{
					AssertEquals("There should be error or warning message", expectedErrorMessage, configArea.ParentReport.ErrorManager.ToString());
				}
			}
		}

		public void TestReportColumnHeadingsReplaceMacros()
		{
			using (OrganisationsDataRegistry.Instance.PotentialLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PotentialLable"))
			{
				var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
					"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT 'A' as ColumnOne]
{A}-[ColumnHeadings:] {B}-[DisplayLabel=""DisplayLabel <ReportData.ColumnOne>"", HeadingText=""HeadingText <ReportData.ColumnOne>"",Description=""Description <ReportData.ColumnOne>"",TagName=""TagName <ReportData.ColumnOne>""] {C}-[DisplayLabel=""Opportunity<RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)><RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)>"", HeadingText=""HeadingText <RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)> <RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)>"",Description=""Description <RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)> <RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)>"",TagName=""TagName <RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)> <RegistryItem(OrganisationsDataRegistry.Instance.PotentialLabel)>""]
{A}-[#DocumentHeader]
{B}-[OpportunityPotentialLablePotentialLable]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.ColumnOne>]
{A}-[#EndOfReport]");
				using (var documentPack = new DocumentPack(Factory.New<StmMenuItem>()))
				using (var report = new Report(documentPack, excelTemplate))
				{
					var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;

					AssertEquals("Should have 1 column heading", 2, configArea.CurrentColumnHeadingsExposedForTest.Count);

					AssertEquals("DisplayLabel <ReportData.ColumnOne>", configArea.CurrentColumnHeadingsExposedForTest[0].DisplayLabel);
					AssertEquals("HeadingText <ReportData.ColumnOne>", configArea.CurrentColumnHeadingsExposedForTest[0].HeadingText);
					AssertEquals("Description <ReportData.ColumnOne>", configArea.CurrentColumnHeadingsExposedForTest[0].Description);
					AssertEquals("TagName <ReportData.ColumnOne>", configArea.CurrentColumnHeadingsExposedForTest[0].TagName);

					AssertSecondColumnHeading(configArea);

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);

						AssertEquals("DisplayLabel A", configArea.CurrentColumnHeadingsExposedForTest[0].DisplayLabel);
						AssertEquals("HeadingText A", configArea.CurrentColumnHeadingsExposedForTest[0].HeadingText);
						AssertEquals("Description A", configArea.CurrentColumnHeadingsExposedForTest[0].Description);
						AssertEquals("TagName A", configArea.CurrentColumnHeadingsExposedForTest[0].TagName);

						AssertSecondColumnHeading(configArea);
					}
				}
			}

			void AssertSecondColumnHeading(ConfigArea configArea)
			{
				AssertEquals("OpportunityPotentialLablePotentialLable", configArea.CurrentColumnHeadingsExposedForTest[1].DisplayLabel);
				AssertEquals("HeadingText PotentialLable PotentialLable", configArea.CurrentColumnHeadingsExposedForTest[1].HeadingText);
				AssertEquals("Description PotentialLable PotentialLable", configArea.CurrentColumnHeadingsExposedForTest[1].Description);
				AssertEquals("TagName PotentialLable PotentialLable", configArea.CurrentColumnHeadingsExposedForTest[1].TagName);
			}
		}

		public void TestReportHasDuplicatedColumnHeadings()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[ColumnHeadings:] {B}-[DisplayLabel=""Company Code"", HeadingText=""Company Code""] {C}-[DisplayLabel=""Company Code"", HeadingText=""Company Code""]
{A}-[#SectionHeader]
{B}-[CompanyCode] {C}-[Salesperson]
{A}-[#SectionBody]
{B}-[123] {C}-[=SUBSTITUTE(B7,1,2)]
{A}-[#SectionFooter]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				AssertEquals(1, report.ErrorManager.ErrorsCount);
				Assert(report.ErrorManager.ToString().Contains("Duplicated column heading display label: Company Code"));
				AssertEquals("should only contains 1 column.", 1, configArea.CurrentColumnHeadingsExposedForTest.Count);
				AssertEquals("should not have referencedby.", 0, configArea.CurrentColumnHeadingsExposedForTest[0].ReferencedBy.Count);
			}
		}

		public void TestSqlTimeoutNotSet()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				AssertEquals("configArea.SqlTimeout", -1, configArea.SqlTimeout);
			}
		}

		public void TestSqlTimeout()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[SqlTimeout=300]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				AssertEquals("configArea.SqlTimeout", 300, configArea.SqlTimeout);
			}
		}

		public void TestIsCustomized()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[ContainsCustomisedSections=true]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				Assert("configArea.ContainsCustomisedSections", configArea.ContainsCustomisedSections);
				Assert("ParentReport.ContainsAnyCustomisation", configArea.ParentReport.ContainsAnyCustomisation);
			}

			excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[SqlTimeout=29]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				Assert("configArea.ContainsNoCustomisedSections", !configArea.ContainsCustomisedSections);
				Assert("ParentReport.ContainsNoCustomisation", !configArea.ParentReport.ContainsAnyCustomisation);
			}
		}

		public void TestSqlTimeoutToLowUsesDefault()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[SqlTimeout=29]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				AssertEquals("configArea.SqlTimeout", -1, configArea.SqlTimeout);
				AssertEquals("report.ErrorManager.ToString()", "Severity: [Warning] Message: [SQL Timeout [29] must be set between [30] and [21600].] Cell: [A3] Sheetname: [Document]", report.ErrorManager.ToString());
			}
		}

		public void TestSqlTimeoutToHighUsesDefault()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[SqlTimeout=21601]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				AssertEquals("configArea.SqlTimeout", -1, configArea.SqlTimeout);
				AssertEquals("report.ErrorManager.ToString()", "Severity: [Warning] Message: [SQL Timeout [21601] must be set between [30] and [21600].] Cell: [A3] Sheetname: [Document]", report.ErrorManager.ToString());
			}
		}

		public void TestSqlQueryHints()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[SqlQueryHints=recompile, fast 10]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var configArea = AreaFactory.InstantiateArea(1, 2, report, "#Config") as ConfigArea;
				AssertEquals("configArea.SqlQueryHints", "recompile, fast 10", configArea.SqlQueryHints);
			}
		}

		public void TestSuppressDraftWatermark()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.SuppressDraftWatermark", false, config.SuppressDraftWatermark);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "SuppressDraftWatermark";
			var configWithSuppressDraftWatermark = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.SuppressDraftWatermark", true, configWithSuppressDraftWatermark.SuppressDraftWatermark);
		}

		public void TestDisableCSVExport()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.DisableCSVExport", false, config.DisableCSVExport);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "DisableCSVExport";
			var configWithDisableCSVExport = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.DisableCSVExport", true, configWithDisableCSVExport.DisableCSVExport);
		}

		public void TestDisableFixedValueCacheExpression()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.DisableFixedValueCacheExpression", "", config.DisableFixedValueCacheExpression);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = @"DisableFixedValueCache=""<CompanyCode>"" == ""EDI""";
			var configWithDisableFixedValueCache = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.DisableFixedValueCacheExpression", @"""<CompanyCode>"" == ""EDI""", configWithDisableFixedValueCache.DisableFixedValueCacheExpression);
		}

		public void TestRemoveFirstPageIfNoData()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.RemoveFirstPageIfNoData", false, config.RemoveFirstPageIfNoData);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "RemoveFirstPageIfNoData";
			var configWithRemoveFirstPageIfNoData = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.RemoveFirstPageIfNoData", true, configWithRemoveFirstPageIfNoData.RemoveFirstPageIfNoData);
		}

		public void TestTranslateLegacyDocument()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.TranslateLegacyDocument", false, config.TranslateLegacyDocument);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "TranslateLegacyDocument";
			var configWithTranslateLegacyDocument = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.TranslateLegacyDocument", true, configWithTranslateLegacyDocument.TranslateLegacyDocument);
		}

		public void TestHideRowsInsteadOfRemove()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.HideRowsInsteadOfRemove", false, config.HideRowsInsteadOfRemove);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "HideRowsInsteadOfRemove";
			var configWithTranslateLegacyDocument = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.HideRowsInsteadOfRemove", true, configWithTranslateLegacyDocument.HideRowsInsteadOfRemove);
		}

		public void TestHideSheetIfExpression()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "HideSheetIf='Moon'=='Cheese'";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.HideSheetIfExpression", "'Moon'=='Cheese'", config.HideSheetIfExpression);
		}

		public void TestGetConfigAreaAsString()
		{
			var excelTemplate = GetExcelTemplateSetupFor("ConfigAreaAsStringWS1", "Portrait", "ConfigAreaAsStringWS2", "Landscape");
			var topLevelDataSource = Factory.New<DummyBusinessObject>();

			using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertMultilineASCIIEquals("Should be the first sheet's config and not truncated", ExpectedConfigAreaString, report.Analyser.Config.GetRawConfigAreaParameters());
			}
		}

		public void TestColumnHeadingsTranslation()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				var templateName = Path.GetFileNameWithoutExtension(TestReport.Template.TemplateSourceLocation);
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test1B"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test1B"), "测试标题1"));
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test1"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test1"), "测试1"));
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test2C"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test2C"), "测试2"));
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test3"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test3"), "测试3"));
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test4"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test4"), "测试4"));
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test5"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test5"), "测试5"));
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test6"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "Test6"), "测试6"));

				var template = Factory.New<StmTemplate>();
				template.SO_IsSystemDefined = true;
				template.SO_ExcelTemplatePath = TestReport.Template.TemplateSourceLocation;
				TestReport.StTemplate = template;
				TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
				TestReport.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\", HeadingText = \"Test1B\"";
				TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "DisplayLabel = \"Test2\",description=\"Test2C\"";
				TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \" Test3 \", Description=\" Test4 \", HeadingText=\" Test5 \", TagName=\" Test6 \"";
				TestReport.ColumnHeadingsProcessedByReportAnalyser = false;

				AreaFactory.InstantiateArea(1, 2, TestReport, "#Config");

				ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0], "Test1", "测试1", "测试标题1", 1, 0, 63, false);
				ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[1], "Test2", "测试2", "Test2", 2, 1, 63, false);
				ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[2], "Test3", "测试4", "测试5", 3, 2, 63, false, "Test6");
				AssertEquals("EnglishHeadingText", "Test1B", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0].EnglishHeadingText);
				AssertEquals("EnglishHeadingText", "Test2", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[1].EnglishHeadingText);
				AssertEquals("EnglishHeadingTextWithBlankSpace", "Test5", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[2].EnglishHeadingText);

				template.SO_IsSystemDefined = false;
				TestReport.ColumnHeadingManager.CurrentConfiguration.Clear();
				TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
				AreaFactory.InstantiateArea(1, 2, TestReport, "#Config");

				ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[3], "Test1", "Test1", "Test1B", 1, 0, 63, false);
				ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[4], "Test2", "Test2C", "Test2", 2, 1, 63, false);
				AssertEquals("EnglishHeadingText", "Test1B", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[3].EnglishHeadingText);
				AssertEquals("EnglishHeadingText", "Test2", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[4].EnglishHeadingText);
			}
		}

		public void TestReportTitleTranslation()
		{
			using (var mockData = Res.UseMockData())
			{
				var key = DocBuilderResourceStrings.ReportTitleKeyPrefix + "Test Title";
				mockData.Put(key, new ResourceStringData(key, "测试标题"));

				var template = Factory.New<StmTemplate>();
				template.SO_IsSystemDefined = true;
				template.SO_ExcelTemplatePath = TestReport.Template.TemplateSourceLocation;
				TestReport.StTemplate = template;
				TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ReportTitle=Test Title";
				TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
				AreaFactory.InstantiateArea(1, 2, TestReport, "#Config");
				TestReport.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
				AssertEquals("测试标题", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].Title);
			}
		}

		public void TestHeightInRows()
		{
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			AssertEquals(8, areaToTest.HeightInRows);
		}

		public void TestShift()
		{
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			areaToTest.Shift(10);
			AssertEquals(14, areaToTest.StartingRow);
			AssertEquals(15, areaToTest.StartOfBody);
			AssertEquals(22, areaToTest.End);
		}

		public void TestReplaceMacros()
		{
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestAmount", 50));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", 12, Passes.SecondPass));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Currency", "USD"));
			TestReport.Renderer.CurrentPass = Passes.FirstPass;
			var areaToTest = new ConfigArea(4, 12, TestReport, "");
			AssertEquals("<CURRENCY TO WORDS(<Amount>, USD)>", areaToTest.ReplaceMacros("<CURRENCY TO WORDS(<Amount>, <Currency>)>"));
			AssertEquals(50, areaToTest.ReplaceMacros("<TestAmount>"));
			TestReport.Renderer.CurrentPass = Passes.SecondPass;
			AssertEquals("test twelve dollars only", areaToTest.ReplaceMacros("test <CURRENCY TO WORDS(<Amount>, <Currency>)>"));
			AssertEquals("twelve dollars only", areaToTest.ReplaceMacros("<CURRENCY TO WORDS(<Amount>, <Currency>)>"));
		}

		public override void TestVisualisationManagerHasAppropriateVisualisationRenderer()
		{
			AssertNull(AreaVisualisationManagerFactory.New(GetNewAreaToTest()));
		}

		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config");
			Assert("CreatedArea is ConfigArea", createdArea is ConfigArea);
			createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config:someOption");
			Assert("CreatedArea is ConfigArea", createdArea is ConfigArea);
		}

		[ExpectException(typeof(CloneAreaException))]
		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config");
			var clone = createdArea.Clone(10);
			AssertNull(clone);
		}

		public void TestForceWebPublish()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ForceWebPublish";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			TestReport.Analyser.Areas.Clear();
			TestReport.Analyser.Areas.Add(config);
			Assert("ForceWebPublish must be set to true", config.ForceWebPublish);
			Assert("ForceWebPublish must be set to true", TestReport.ForceWebPublish);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "";
			config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			TestReport.Analyser.Areas.Clear();
			TestReport.Analyser.Areas.Add(config);
			Assert("ForceWebPublish must be set to false", !config.ForceWebPublish);
			Assert("ForceWebPublish must be set to false", !TestReport.ForceWebPublish);
		}

		public void TestColumnHeadings()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\", hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test3\",Hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 6] = "DisplayLabel = \"Test4\",headingText=\"Test4B\" , HiddEn";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 7] = "DisplayLabel = \"Test5\", HeadingText = \"Test5B\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 8] = "DisplayLabel = \"Test6\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 9] = "DisplayLabel = \"Test7\",description=\"Test7D\" , HiddEn";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 10] = "DisplayLabel = \"Test8\", Description = \"Test8D\"";
			TestReport.WorkSheetCurrentlyBeingProcessed.SetColWidth(7, 2500);
			TestReport.WorkSheetCurrentlyBeingProcessed.SetColWidth(8, 2000);
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;

			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;

			AssertEquals("ColumnHeadingManager.Headings.Count", 8, TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0], "Test1", "Test1", "Test1", 3, 0, 63, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[1], "Test2", "Test2", "Test2", 4, 1, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[2], "Test3", "Test3", "Test3", 5, 1, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[3], "Test4", "Test4", "Test4B", 6, 1, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[4], "Test5", "Test5", "Test5B", 7, 1, 68, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[5], "Test6", "Test6", "Test6", 8, 2, 54, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[6], "Test7", "Test7D", "Test7", 9, 3, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[7], "Test8", "Test8D", "Test8", 10, 3, 63, false);

			TestReport.ColumnHeadingManager.CurrentConfiguration.Clear();
			TestReport.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
			AssertEquals("ColumnHeadingManager.Headings.Count", 8, TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0], "Test1", "Test1", "Test1", 3, 0, 63, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[1], "Test2", "Test2", "Test2", 4, 1, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[2], "Test3", "Test3", "Test3", 5, 1, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[3], "Test4", "Test4", "Test4B", 6, 1, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[4], "Test5", "Test5", "Test5B", 7, 1, 68, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[5], "Test6", "Test6", "Test6", 8, 2, 54, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[6], "Test7", "Test7D", "Test7", 9, 3, 63, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[7], "Test8", "Test8D", "Test8", 10, 3, 63, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnHeadings_WhenAllSheetsHaveColumnHeadings()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplateWithThreeSheets.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			using (var stream = new MemoryStream())
			{
				Action<int> setupSheet = (int sheetIndex) =>
				{
					report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(report.XlInterface.WorkSheets[sheetIndex]);
					report.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
					report.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
					report.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test1\"";
					report.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\"";
					report.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test3\"";
				};

				setupSheet(0);
				setupSheet(1);
				setupSheet(2);

				report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(report.XlInterface.WorkSheets[0]);
				report.Save(stream);

				var worksheets = report.ColumnHeadingManager.CurrentConfiguration.Worksheets;
				AssertEquals(3, worksheets.Count);

				AssertEquals("Sheet1", worksheets[0].Name);
				AssertNotNull(worksheets[0].ColumnHeadings);

				AssertEquals("Sheet2", worksheets[1].Name);
				AssertNotNull(worksheets[1].ColumnHeadings);

				AssertEquals("Sheet3", worksheets[2].Name);
				AssertNotNull(worksheets[2].ColumnHeadings);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnHeadings_WhenAllSheetsButTheFirstOneHaveColumnHeadings()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplateWithThreeSheets.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			using (var stream = new MemoryStream())
			{
				report.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";

				Action<int> setupSheet = (int sheetIndex) =>
				{
					report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(report.XlInterface.WorkSheets[sheetIndex]);
					report.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
					report.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
					report.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test1\"";
					report.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\"";
					report.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test3\"";
				};

				setupSheet(1);
				setupSheet(2);

				report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(report.XlInterface.WorkSheets[0]);
				report.Save(stream);

				var worksheets = report.ColumnHeadingManager.CurrentConfiguration.Worksheets;
				AssertEquals(2, worksheets.Count);

				AssertEquals("Sheet2", worksheets[0].Name);
				AssertNotNull(worksheets[0].ColumnHeadings);

				AssertEquals("Sheet3", worksheets[1].Name);
				AssertNotNull(worksheets[1].ColumnHeadings);
			}
		}

		public void TestColumnHeadings_HideIfDescriptionEmpty()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\", HeadingText = \"Test2Head\", Description = \"Test2Desc\", HideIfDescriptionEmpty";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test3\", HeadingText = \"Test3Head\", Description = \" \", HideIfDescriptionEmpty";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;

			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;

			//Columns with HideIfDescriptionEmpty and an empty description should be still be added to ColumnHeadings collection so that DocEngine can hide them later when rendering.
			//However, thoses columns should not appear in the Column Configuration manager
			AssertEquals("ColumnHeadingManager.Headings.Count", 3, TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0], "Test1", "Test1", "Test1", 3, 0, 63, false, false);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[1], "Test2", "Test2Desc", "Test2Head", 4, 1, 63, false, true);
			ColumnHeadingTest.AssertPropertyValues(TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[2], "Test3", "", "Test3Head", 5, 2, 63, false, true);
		}

		public void TestProcessDocumentCurrency_ReportWithColumnHeadings()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "DisplayLabel = \"Test2\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test3\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test4\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test5\",Hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 6] = "DisplayLabel = \"Test6\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "DocumentCurrency=<CompanyCurrencyCode>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 2] = "ApplyDocumentCurrency";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 3] = "ApplyDocumentCurrency";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 5] = "ApplyDocumentCurrency";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 6] = "ApplyDocumentCurrency";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;

			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("There should be 4 columns in collection", 4, config.ColumnsWithCurrencySetup.Count);
			Assert("Collection should contain column 2, that's on place 2", config.ColumnsWithCurrencySetup.Contains(2));
			Assert("Collection should contain column 3, that's on place 3", config.ColumnsWithCurrencySetup.Contains(3));
			Assert("Collection should contain column 5, that's on place 5", config.ColumnsWithCurrencySetup.Contains(5));
			Assert("Collection should contain column 6, that's on place 5, because column 5 is hidden", config.ColumnsWithCurrencySetup.Contains(5));
			AssertEquals("Currency format should be correct", config.DocumentCurrencyPattern, new Currency().GetFormatPatternForCurrency(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
		}

		public void TestProcessDocumentCurrency_ReportWithOutColumnHeadings()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "HIDECOLUMNIF";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "1==1";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "1==1";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "DocumentCurrency=<CompanyCurrencyCode>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 2] = "ApplyDocumentCurrency";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 3] = "ApplyDocumentCurrency";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 5] = "ApplyDocumentCurrency";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 6] = "ApplyDocumentCurrency";

			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("There should be 4 columns in collection", 4, config.ColumnsWithCurrencySetup.Count);
			Assert("Collection should contain column 2, that's on place 2", config.ColumnsWithCurrencySetup.Contains(2));
			Assert("Collection should contain column 3, that's on place 3", config.ColumnsWithCurrencySetup.Contains(3));
			Assert("Collection should contain column 5, that's on place 5", config.ColumnsWithCurrencySetup.Contains(5));
			Assert("Collection should contain column 6, that's on place 6", config.ColumnsWithCurrencySetup.Contains(6));
			AssertEquals("Currency format should be correct", config.DocumentCurrencyPattern, new Currency().GetFormatPatternForCurrency(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
		}

		public void TestProcessDocumentCurrency_CustomCurrency()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "DocumentCurrency=USD";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "ApplyDocumentCurrency";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Currency format should be USD", config.DocumentCurrencyPattern, new Currency().GetFormatPatternForCurrency("USD"));
		}

		public void TestProcessDocumentCurrency_IncorrectCurrencyCodeWillCauseAnError()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "DocumentCurrency=BLA";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "ApplyDocumentCurrency";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Currency format is not calculated", "", config.DocumentCurrencyPattern);
			AssertEquals("Report.Errors", "Severity: [Error] Message: [Currency code not found : BLA]", TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestGetEndOfConfigArea_XLS()
		{
			var expectedEndOfConfigArea = 12345;
			var workSheet = GetPreparedWorkSheetForTestGetEndOfConfigArea(TestReport, expectedEndOfConfigArea);
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals(expectedEndOfConfigArea, config.GetEndOfConfigArea_ExposedForTest(workSheet));
		}

		public void TestGetEndOfConfigArea_XLS_MaxRowCountSupported97_2003Reached()
		{
			var expectedEndOfConfigArea = ZArchitecture.Environment.Excel.MaxRowCountSupported97_2003 + 50;
			var workSheet = GetPreparedWorkSheetForTestGetEndOfConfigArea(TestReport, expectedEndOfConfigArea);
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals(string.Format("Excel 2003 row limit reached, should have return max supported row [{0}]", ZArchitecture.Environment.Excel.MaxRowCountSupported97_2003),
				ZArchitecture.Environment.Excel.MaxRowCountSupported97_2003 - 1,
				config.GetEndOfConfigArea_ExposedForTest(workSheet));
		}

		public void TestGetEndOfConfigArea_XLSX()
		{
			var expectedEndOfConfigArea = 12345;
			var workSheet = GetPreparedWorkSheetForTestGetEndOfConfigArea(TestReportXLSX, expectedEndOfConfigArea);
			var config = AreaFactory.InstantiateArea(1, 10, TestReportXLSX, "#Config") as ConfigArea;
			AssertEquals(expectedEndOfConfigArea, config.GetEndOfConfigArea_ExposedForTest(workSheet));
		}

		public void TestGetEndOfConfigArea_XLSX_MaxRowCountSupported97_2003Reached()
		{
			var expectedEndOfConfigArea = ZArchitecture.Environment.Excel.MaxRowCountSupported97_2003 + 50;
			var workSheet = GetPreparedWorkSheetForTestGetEndOfConfigArea(TestReportXLSX, expectedEndOfConfigArea);
			var config = AreaFactory.InstantiateArea(1, 10, TestReportXLSX, "#Config") as ConfigArea;
			AssertEquals(expectedEndOfConfigArea, config.GetEndOfConfigArea_ExposedForTest(workSheet));
		}

		public void TestCurrentColumnHeadings()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "DisplayLabel = \"Test2\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test3\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test4\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test5\",Hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 6] = "DisplayLabel = \"Test6\"";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertNotNull("Precondition: Column headings should not be null", config.CurrentColumnHeadingsExposedForTest);
			AssertEquals("Property should return correct value", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings, config.CurrentColumnHeadingsExposedForTest);
		}

		public void TestColumnCurrentPositionsIndexedByOriginalColumnPositions()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "DisplayLabel = \"Test2\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test3\",Hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test4\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test5\",Hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 6] = "DisplayLabel = \"Test6\"";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			Dictionary<int, int> testDictonary = config.ColumnCurrentPositionsIndexedByOriginalColumnPositionsExposedForTest;
			AssertNotNull("Precondition: Column headings should not be null", config.CurrentColumnHeadingsExposedForTest);
			AssertNotNull("Precondition: collection should not be null", testDictonary);
			AssertEquals("Dictonary should contain 6 items", 6, testDictonary.Count);

			int amendment = config.CurrentColumnHeadingsExposedForTest[0].OriginalColumnNumber;
			AssertEquals("1 element:", config.CurrentColumnHeadingsExposedForTest[0].CurrentPosition + amendment, testDictonary[1]);
			AssertEquals("1 element:", testDictonary[1] + 1, testDictonary[2]);
			AssertEquals("1 element:", testDictonary[2] + 1, testDictonary[3]);
			AssertEquals("1 element:", testDictonary[3], testDictonary[4]);
			AssertEquals("1 element:", testDictonary[4] + 1, testDictonary[5]);
			AssertEquals("1 element:", testDictonary[5], testDictonary[6]);
		}

		public void TestColumnHeadingsWithNoDisplayLabel()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\", hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 5] = "DisplayLabel = \"Test3\",Hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 6] = "DisplayLabel = \"Test4\" , HiddEn";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 7] = "DisplayLabel = \"Test5\"";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;

			AssertEquals("Report.Errors", "Severity: [Error] Message: [Column heading must have a display label keyword.] Cell: [D3]", TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestColumnHeadingsWithGapInBetween()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\", hidden";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 6] = "DisplayLabel = \"Test4\" , HiddEn";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 7] = "DisplayLabel = \"Test5\"";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;

			AssertEquals("Report.Errors", "Severity: [Error] Message: [All column headings should group together, no empty column headings allowed in between.] Cell: [G3]", TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestMacrosAreReplacedInDisplayLabel()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"<numberToWords(0)>\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\", hidden";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;

			AssertEquals("zero", TestReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[TestReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0].ToString());
		}

		public void TestSaveToKeyword()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 3] = "DisplayLabel = \"<numberToWords(0)>\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel = \"Test2\", hidden";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals(null, TestReport.ColumnHeadingManager.SaveToFilterField);

			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings:SavesTO=Test";
			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Test", TestReport.ColumnHeadingManager.SaveToFilterField);
		}

		public void TestReportTitle()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalColumns.xls", "MultipleTemplatesWithOptionalColumns.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalColumns.xls", Path.GetFullPath(tempFileName));
			using (var report = new Report(new DocumentPack(), excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.ColumnHeadingManager.CurrentConfiguration.Clear();
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
				AssertEquals("The report should have 3 templates", 3, report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Count);
				AssertEquals("The report should have changeable title set to true", true, report.ColumnHeadingManager.IsReportTitleChangeable);
				AssertEquals("The title of the first template should be - Sheet Number 1", "Sheet Number 1", report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].Title);
				AssertEquals("The title of the second template should be - Sheet Number 2", "Sheet Number 2", report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].Title);
				AssertEquals("The title of the third template should be empty", string.Empty, report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].Title);
			}
		}

		public void TestColumnHeadingsWithMacros()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 4] = "DisplayLabel =\"Container Class 1\", HeadingText=\"<CodePairValue(StorageClass, Code, 1)>\"";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			_ = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;

			AssertEquals("First-pass macros in column heading should have been processed.", TestReport.ColumnHeadingManager.GetHeadingText("Sheet1", "Container Class 1"), "20F");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnHeadingReferences()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SingleTemplateWithHiddenReferences.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.ColumnHeadingManager.CurrentConfiguration.Clear();
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
				AssertEquals("ColumnHeadingManager.Headings.Count", 7, report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings.Count);

				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[0].ReferencedBy.Count, 1);
				AssertEquals("Incorrect cell reference by members", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[3], report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[0].ReferencedBy[0]);
				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[1].ReferencedBy.Count, 1);
				AssertEquals("Incorrect cell reference by members", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[3], report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[1].ReferencedBy[0]);
				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[2].ReferencedBy.Count, 2);
				AssertEquals("Incorrect cell reference by members", true, report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[2].ReferencedBy.Contains(report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[3].DisplayLabel));
				AssertEquals("Incorrect cell reference by members", true, report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[2].ReferencedBy.Contains(report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[4].DisplayLabel));
				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[3].ReferencedBy.Count, 1);
				AssertEquals("Incorrect cell reference by members", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[5], report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[3].ReferencedBy[0]);
				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[4].ReferencedBy.Count, 1);
				AssertEquals("Incorrect cell reference by members", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[5], report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[4].ReferencedBy[0]);
				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[5].ReferencedBy.Count, 0);
				AssertEquals("Incorrect cell reference by count", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[6].ReferencedBy.Count, 1);
				AssertEquals("Incorrect cell reference by members", report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[4], report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[6].ReferencedBy[0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnHeadingReferencesDetectCircularReferences()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SingleTemplateWithCircularReferences.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				report.ColumnHeadingManager.CurrentConfiguration.Clear();
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();

				AssertEquals("ColumnHeadingManager.Headings.Count", 4, report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings.Count);

				AssertEquals("Report.Errors", @"
Severity: [Error] Message: [Column [Column 1] is a part of circular reference.] Cell: []
Severity: [Error] Message: [Column [Column 2] is a part of circular reference.] Cell: []
Severity: [Error] Message: [Column [Column 3] is a part of circular reference.] Cell: []
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionalColumnsAreHandledCorrectlyOnMultipleTemplates()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplatesAndOptionalColumns.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(Factory.New<StmMenuItem>()), excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Should have found optional columns on both templates", 2, report.ColumnHeadingManager.DefaultTemplateConfigurationManager.GetCopyOfHeadings().Worksheets.Count);
			}
		}

		public void TestSheetNameOverride()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "SheetNameOverride=Hello World Report";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Sheet Name Override", "Hello World Report", config.SheetNameOverride);
		}

		public void TestEDWDataSourceSignature()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData1=SELECT 'A' as ColumnOne]
{A}-[EDWData:ReportData2=SELECT 'B' as ColumnTwo]
{A}-[EDWOnlyData:ReportData3=SELECT 'C' as ColumnThree]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack(Factory.New<StmMenuItem>()))
			using (var report = new Report(documentPack, excelTemplate))
			{
				var config = AreaFactory.InstantiateArea(1, 10, report, "#Config") as ConfigArea;
				AssertEquals("3 element in DataSourceStrings", 3, config.DataSourceStrings.Count);
				AssertContainsExactElementsInAnyOrder(config.DataSourceStrings, new [] { "Data:ReportData1=SELECT 'A' as ColumnOne", "EDWData:ReportData2=SELECT 'B' as ColumnTwo", "EDWOnlyData:ReportData3=SELECT 'C' as ColumnThree" });
			}
		}

		public void TestVersionNumber()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "Version=1.4";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Version", 140, config.ReportVersion);
			AssertEquals(ReportErrorManager.HasNoErrors, TestReport.ErrorManager.ToString());
		}

		public void TestVersionNumberWithBigNumber()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "Version=" + 21474836.47;   // 21474836.47 = int.MaxValue / 100
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Version", 2147483647, config.ReportVersion);
			AssertEquals(ReportErrorManager.HasNoErrors, TestReport.ErrorManager.ToString());

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "Version=" + 21474836.48;   // 21474836.48 = (int.MaxValue + 1) / 100
			config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Version", 100, config.ReportVersion);
			TestReport.PrepareForRender();
			AssertEquals("Report.Errors", "Severity: [Warning] Message: [Parameter 'Version' must be a positive decimal (between 0.00 and 21474836.47) with format as nn.nn but its value is '21474836.48'] Cell: [A3]",
				TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestVersionNumberMustBeAPositiveNumber()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "Version=" + "-2.00";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Version", 100, config.ReportVersion);
			TestReport.PrepareForRender();
			AssertEquals("Report.Errors", "Severity: [Warning] Message: [Parameter 'Version' must be a positive decimal (between 0.00 and 21474836.47) with format as nn.nn but its value is '-2.00'] Cell: [A3]",
				TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestVersionNumberUnderCultureUsingCommaAsDecimalSeparator()
		{
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.French)))
			{
				TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "Version=1.4";
				var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
				AssertEquals("Version", 140, config.ReportVersion);
				AssertEquals(ReportErrorManager.HasNoErrors, TestReport.ErrorManager.ToString());
			}
		}

		public void TestDisableXLSXExport()
		{
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.DisableXLSXExport", false, config.DisableXLSXExport);

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "DisableXLSXExport";
			var configWithDisableCSVExport = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("config.DisableXLSXExport", true, configWithDisableCSVExport.DisableXLSXExport);
		}

		public void TestVersionNumber_DoesntThrowOnInvalidNumber_DisplaysWarningMessage()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "Version=1.3a";
			var config = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			AssertEquals("Invalid version number should be replaced with 100", 100, config.ReportVersion);

			TestReport.PrepareForRender();
			AssertEquals("Report.Errors", "Severity: [Warning] Message: [Parameter 'Version' must be a positive decimal (between 0.00 and 21474836.47) with format as nn.nn but its value is '1.3a'] Cell: [A3]",
				TestReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestDoubleHideColumnIf()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString(Name, string.Empty,
@"{A}-[#Config]
{A}-[Name=TestDoubleHideColumnIf]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=Test]
{A}-[#SectionBody]
{B}-[Blah]
{A}-[#EndOfReport]");
			using (var report = new Report(new DocumentPack(), template))
			{
				var config = new ConfigArea(1, 10, report, "#Config");
				AssertEquals(1, config.HideColumnExpressions.Count);
				AssertEquals("1==1", config.HideColumnExpressions[3]);
			}
		}

		public void TestCustomPageHeightAndWidth()
		{
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 0] = "PageStyle=Custom";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "PageHeight=\"Test1\"";
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 0] = "PageWidth=\"Test1\"";
			TestReport.ColumnHeadingsProcessedByReportAnalyser = false;
			AreaFactory.InstantiateArea(1, 10, TestReport, "#Config");
			AssertContains("Page height should be a valid number.", TestReport.ErrorManager.ToString());
			AssertContains("Page width should be a valid number.", TestReport.ErrorManager.ToString());

			TestReport.ErrorManager.ClearErrors();
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "PageHeight=-1";
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 0] = "PageWidth=-1";
			AreaFactory.InstantiateArea(1, 10, TestReport, "#Config");
			AssertContains("Page height should be larger than zero.", TestReport.ErrorManager.ToString());
			AssertContains("Page width should be larger than zero.", TestReport.ErrorManager.ToString());

			TestReport.ErrorManager.ClearErrors();
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 0] = "PageHeight=500";
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 0] = "PageWidth=200";
			var configArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#Config") as ConfigArea;
			Assert("Report should not have error if PageHeight is valid", !TestReport.ErrorManager.HasErrors);
			AssertEquals(500, configArea.CustomPageHeightInMillimeters);
			AssertEquals(200, configArea.CustomPageWidthInMillimeters);
		}

		public void TestProcessPageStyle_WhenCodeIsInvalid_ShouldHaveErrorMessage()
		{
			var templateContentsWithInvalidPageStyle = @"{A}-[#Config]
{A}-[Name=Test]
{A}-[PageStyle=ABC]";
			var excelTemplateWithInvalidPageStyle = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContentsWithInvalidPageStyle);
			using (var report = new Report(new DocumentPack(), excelTemplateWithInvalidPageStyle))
			{
				var configArea = AreaFactory.InstantiateArea(1, 3, report, "#Config") as ConfigArea;
				AssertEquals("Severity: [Error] " +
					"Message: [Page style can only be one of: 'Portrait', 'Landscape', 'Continuous', 'LetterPortrait', 'LetterLandscape', 'LetterPortraitMatrix', 'CustomsLetterPortrait', 'Label', 'Custom'.] " +
					"Cell: [A3] " +
					"Sheetname: [Document]", report.ErrorManager.ToString());
			}
		}

		public void TestProcessPageStyle_WhenCodeIsValid_ShouldNotHaveErrorMessage()
		{
			var templateContentsWithValidPageStyle = @"{A}-[#Config]
{A}-[Name=Test]
{A}-[PageStyle=Portrait]";
			var excelTemplateWithValidPageStyle = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContentsWithValidPageStyle);
			using (var report = new Report(new DocumentPack(), excelTemplateWithValidPageStyle))
			{
				var configArea = AreaFactory.InstantiateArea(1, 3, report, "#Config") as ConfigArea;
				AssertEquals(ReportErrorManager.HasNoErrors, report.ErrorManager.ToString());
			}
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#Config");

		protected override void TearDown()
		{
			base.TearDown();
			testReportXLSX?.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplate GetExcelTemplateSetupFor(ZString name1, ZString pageStyle1, ZString name2, ZString pageStyle2)
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(2);
				var workSheet1 = xlInterface.WorkSheets[0];
				workSheet1[0, 0] = "#Config";
				workSheet1[1, 0] = "Name=" + name1;
				workSheet1[2, 0] = "PageStyle=" + pageStyle1;
				workSheet1[3, 0] = "DataContext=.DummyBusinessObject";
				workSheet1[4, 0] = "Version=1.00";
				workSheet1[5, 0] = "HideColumnIf";
				workSheet1[6, 0] = "#DocumentHeader";
				workSheet1[7, 1] = "<Z0_VarCharMax>";
				workSheet1[8, 0] = "#EndOfReport";
				var workSheet2 = xlInterface.WorkSheets[1];
				workSheet2[0, 0] = "#Config";
				workSheet2[1, 0] = "Name=" + name2;
				workSheet2[2, 0] = "PageStyle=" + pageStyle2;
				workSheet2[3, 0] = "DataContext=.DummyBusinessObject";
				workSheet2[4, 0] = "#DocumentHeader";
				workSheet2[5, 1] = "<Z0_VarCharMax>";
				workSheet2[6, 0] = "#EndOfReport";

				using (var saveStream = new MemoryStream())
				{
					xlInterface.SaveToStream(saveStream);
					return new ExcelTemplateReadFromByteArray("DummyTemplate", "GeneratedUsingFlexCel", saveStream.ToArray());
				}
			}
		}

		ExcelWorkSheet GetPreparedWorkSheetForTestGetEndOfConfigArea(Report report, int expectedEndOfConfigArea)
		{
			var workSheet = report.WorkSheetCurrentlyBeingProcessed;
			workSheet[0, 0] = "#Config";
			workSheet[1, 0] = "Name=TestTemplate";
			workSheet[3, 0] = "Version=1.3";
			workSheet[4, 0] = "PageStyle=Portrait";
			workSheet[5, 0] = "HideColumnIf";
			workSheet[expectedEndOfConfigArea + 1, 0] = "#DocumentHeader";
			workSheet[expectedEndOfConfigArea + 2, 1] = "<Z0_VarCharMax>";
			workSheet[expectedEndOfConfigArea + 3, 0] = "#EndOfReport";

			return workSheet;
		}

		Report testReportXLSX;
		Report TestReportXLSX
		{
			get
			{
				if (testReportXLSX == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xlsx", "EmptyAndValidTemplate.xlsx");
					var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xlsx", Path.GetFullPath(tempFileName));
					testReportXLSX = new Report(new DocumentPack(), excelTemplate);
					testReportXLSX.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
					testReportXLSX.PrepareForRender();
					testReportXLSX.Renderer.CurrentPass = Passes.SecondPass;
				}
				return testReportXLSX;
			}
		}

		const string ExpectedConfigAreaString = @"#Config
Name=ConfigAreaAsStringWS1
PageStyle=Portrait
DataContext=.DummyBusinessObject
Version=1.00
HideColumnIf";
	}
}
