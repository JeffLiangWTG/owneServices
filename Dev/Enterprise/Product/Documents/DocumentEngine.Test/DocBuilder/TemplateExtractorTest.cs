using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder.Styling;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplateExtractorTest : TestCaseWithFactory
	{
		public void TestGenerateInPortraitAndLandscape()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[Testing Orientation]
{A}-[#EndOfReport]");
			var sectionRepository = new SectionRepository(template.GetExcelTemplate());
			var extractor = new TemplateExtractor(sectionRepository);

			var parent = Factory.New<DummyBODocSupportable>();
			var parentBOForFiltering = BODocDataProvider.Get(parent);
			var errors = new List<TemplateGenerationError>();

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.S3_IsSystem = ZBool.True;

			var configItem1 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "My Section");

			config.S3_PageStyle = DocumentConfigPageStyleList.Codes.Portrait;
			using (var excelFile = extractor.GenerateExcelFile(config, new FilterEvaluator(parentBOForFiltering), errors))
			{
				AssertEquals("Orientation should be portrait.", FlexCel.Core.TPrintOptions.Orientation, excelFile.Xls.PrintOptions & FlexCel.Core.TPrintOptions.Orientation);
				var workSheet = excelFile.WorkSheets[0];
				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[#config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Testing Orientation]
{A}-[#EndOfReport]", workSheet.ToString());
			}

			config.S3_PageStyle = DocumentConfigPageStyleList.Codes.Landscape;
			using (var excelFile = extractor.GenerateExcelFile(config, new FilterEvaluator(parentBOForFiltering), errors))
			{
				AssertEquals("Orientation should be landscape.", FlexCel.Core.TPrintOptions.None, excelFile.Xls.PrintOptions & FlexCel.Core.TPrintOptions.Orientation);
				var workSheet = excelFile.WorkSheets[0];
				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[#config]
{A}-[Name=Test]
{A}-[PAGESTYLE=Landscape]
{A}-[#SectionBody]
{B}-[Testing Orientation]
{A}-[#EndOfReport]", workSheet.ToString());
			}
		}

		public void TestGenerateExcelTemplateWithGenericSections()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			var sectionRepository = new SectionRepository(template.GetExcelTemplate());
			var extractor = new TemplateExtractor(sectionRepository);

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			var configItem1 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 1");
			var configItem2 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			configItem2.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			var configItem3 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");
			configItem3.S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;
			var configItem4 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 3");
			configItem4.S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;

			var parentBOForFiltering = BODocDataProvider.Get(Factory.New<DummyBODocSupportable>());
			var errors = new List<TemplateGenerationError>();

			using (var excelFile = extractor.GenerateExcelFile(config, new FilterEvaluator(parentBOForFiltering), errors))
			{
				var workSheet = excelFile.WorkSheets[0];
				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#DocumentHeader]
{B}-[This is Generic Section 3.]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[This is Generic Section 3.]
{B}-[#SectionBody]
{B}-[This is Section Body 1.]
{A}-[#SectionBody]
{B}-[This is Generic Section 1.]
{A}-[#PageFooter]
{B}-[This is Generic Section 2.]
{A}-[#FirstPageFooter]
{B}-[This is Generic Section 2.]
{A}-[#OnlyOnePageFooter]
{B}-[This is Generic Section 2.]
{A}-[#LastPageFooter]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]", workSheet.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddOverridingSectionRepositoryOnlyIfTemplateIsUserConfigurable()
		{
			var repositoryExcelTemplate = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			var repository = new SectionRepository(repositoryExcelTemplate);

			var helper = new TemplateTestHelper();
			helper.DataContext = nameof(Core.Constants.DataContext.Cheques);
			helper.AddWorkSheet("Sheet",
				@"{A}-[#Config]
{A}-[DataContext=Cheques]
{A}-[Name=Cheque]
{A}-[#EndOfReport]");
			helper.CreateTemplate(Factory, SectionRepositoryTemplateNames.User);

			helper = new TemplateTestHelper();
			helper.AddWorkSheet("Sheet",
				@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:DHD, My Head Hurts]	
{B}-[My Head Hurts]	
{A}-[#EndOfReport]");

			helper.DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);
			helper.CreateTemplate(Factory, SectionRepositoryTemplateNames.User);

			var templateExtractor = new TemplateExtractor(repository);
			templateExtractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.User, Factory);

			var config = Factory.New<StmMenuDocumentConfig>();
			config.ConfigItems.AddFromTemplateSection(repository.AllSections[1]);

			using (var excelFile = templateExtractor.GenerateExcelFile(config, new FilterEvaluator(BODocDataProvider.Get(Factory.New<DummyBODocumentSupportable>())), new List<TemplateGenerationError>()))
			{
				var workSheet = excelFile.WorkSheets[0];
				AssertContains("My Head Hurts", workSheet.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainsCustomisedSectionsInConfigSection()
		{
			var repositoryExcelTemplate = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			var repository = new SectionRepository(repositoryExcelTemplate);

			var customizedExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorCustomized.xls", TestFilesSubFolder.DocumentTestFiles);
			var customizedSectionRepository = new SectionRepository(customizedExcelTemplate);

			var customizedStmTemplate = Factory.New<StmTemplateBase>();
			customizedStmTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			customizedStmTemplate.SO_Template = customizedExcelTemplate.GetAsByteArray();

			var templateExtractor = new TemplateExtractor(repository);
			templateExtractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.User, Factory);

			var config = Factory.New<StmMenuDocumentConfig>();
			config.ConfigItems.AddFromTemplateSection(customizedSectionRepository.AllSections[0]);
			config.ConfigItems.AddFromTemplateSection(repository.AllSections[1]);
			config.ConfigItems.AddFromTemplateSection(customizedSectionRepository.AllSections[1]);
			config.ConfigItems.AddFromTemplateSection(customizedSectionRepository.AllSections[2]);
			config.ConfigItems.AddFromTemplateSection(customizedSectionRepository.AllSections[3]);
			config.ConfigItems.AddFromTemplateSection(customizedSectionRepository.AllSections[4]);

			using (var excelFile = templateExtractor.GenerateExcelFile(config, new FilterEvaluator(BODocDataProvider.Get(Factory.New<DummyBODocumentSupportable>())), new List<TemplateGenerationError>()))
			{
				var workSheet = excelFile.WorkSheets[0];
				AssertContains("Contains ContainsCustomisedSections", "ContainsCustomisedSections=True", workSheet.ToString());
				AssertContains("ContainsCustomisedSections placed first", "ContainsCustomisedSections=True", workSheet[1, 0].ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateExcelTemplate()
		{
			var repositoryExcelTemplate = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			var repository = new SectionRepository(repositoryExcelTemplate);
			AssertEquals("Precondition: repository.AllSections.Count", 6, repository.AllSections.Count);

			var config = Factory.New<StmMenuDocumentConfig>();
			config.ConfigItems.AddFromTemplateSection(repository.AllSections[0]);
			config.ConfigItems.AddFromTemplateSection(repository.AllSections[3]);
			config.ConfigItems.AddFromTemplateSection(repository.AllSections[2]);
			config.ConfigItems.AddFromTemplateSection(repository.AllSections[5]);

			var templateExtractor = new TemplateExtractor(repository);
			using (ExcelInterface excelFile = templateExtractor.GenerateExcelFile(config, new FilterEvaluator(BODocDataProvider.Get(Factory.New<DummyBODocumentSupportable>())), new List<TemplateGenerationError>()))
			{
				ExcelWorkSheet workSheet = excelFile.WorkSheets[0];
				AssertMultilineASCIIEquals(workSheet.SheetName, @"
{A}-[#config]
{A}-[Name=CustomisableSectionTest]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[EmailSubject=Customisable Section Test]
{A}-[PageStyle=LetterPortrait]
{A}-[Version=4]
{A}-[DATA:Raindrops keep falling on my head]
{A}-[#SectionPageHeader]
{B}-[<Z0_Description>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Format(""{Z0_VarCharMax}"")>]
{A}-[#GroupBy:Collection.Z0_Number:GroupTitle]
{B}-[<Collection.Z0_FK_Code>]
{A}-[#GroupBy:Collection.Z0_Number]
{B}-[<Collection.Z0_NVarCharMax>]
{A}-[#EndOfReport]
".Trim(), workSheet.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestGenerateExcelTemplate_DoesNotThrowTooManyCellStyleException()
		{
			var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
			var systemSectionRepository = new SectionRepository(systemExcelTemplate);
			Assert("Precondition: systemSectionRepository.AllSections.Count", systemSectionRepository.AllSections.Count > 0);

			var customizedExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorCustomized.xls", TestFilesSubFolder.DocumentTestFiles);
			var customizedSectionRepository = new SectionRepository(customizedExcelTemplate);
			Assert("Precondition: customizedSectionRepository.AllSections.Count", customizedSectionRepository.AllSections.Count > 0);

			var customizedStmTemplate = Factory.New<StmTemplateBase>();
			customizedStmTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			customizedStmTemplate.SO_Template = customizedExcelTemplate.GetAsByteArray();

			var config = Factory.New<StmMenuDocumentConfig>();
			var sections = systemSectionRepository.AllSections.Cast<TemplateSection>().Where(s => s.SectionName == "Company Logo"
																								|| s.SectionName == "Document Title with Mode + Departure/Arrival (Short)"
																								|| s.SectionName == "Recipient with Consignee + Terms"
																								|| s.SectionName == "Recipient with Job Number"
																								|| s.SectionName == "Separator - Blank Row (Solid Background)"
																								|| s.SectionName == "Shipment Details Section Header"
																							).ToList();
			sections.ForEach(section => config.ConfigItems.AddFromTemplateSection(section));
			AssertEquals(6, config.ConfigItems.Count);

			var templateExtractor = new TemplateExtractor(systemSectionRepository);
			templateExtractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.User, Factory);

			using (ExcelInterface excelFile = templateExtractor.GenerateExcelFile(config, new FilterEvaluator(BODocDataProvider.Get(Factory.New<DummyBODocumentSupportable>())), new List<TemplateGenerationError>()))
			using (var stream = new MemoryStream())
			{
				var stylizer = new TemplateStylizer(excelFile, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);
				stylizer.Stylize();
				excelFile.SaveToStream(stream, ExcelFileFormatOptionList.Codes.XLS);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBestDestinationSectionRepository()
		{
			var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
			var systemSectionRepository = new SectionRepository(systemExcelTemplate);
			Assert("Precondition: systemSectionRepository.AllSections.Count", systemSectionRepository.AllSections.Count > 0);

			var customizedExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorCustomized.xls", TestFilesSubFolder.DocumentTestFiles);
			var customizedSectionRepository = new SectionRepository(customizedExcelTemplate);
			Assert("Precondition: customizedSectionRepository.AllSections.Count", customizedSectionRepository.AllSections.Count > 0);

			var customizedStmTemplate = Factory.New<StmTemplateBase>();
			customizedStmTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			customizedStmTemplate.SO_Template = customizedExcelTemplate.GetAsByteArray();

			var config = Factory.New<StmMenuDocumentConfig>();
			var sections = systemSectionRepository.AllSections.Cast<TemplateSection>().Where(s => s.SectionName == "Company Logo" // 1 row
																								|| s.SectionName == "Consignment Details - Pickup Date" // 6 rows
																								|| s.SectionName == "Packages (ID, Type, Description, Weight, UoW, Volume, UoV)" // 6 rows
																							).ToList();
			sections.ForEach(section => config.ConfigItems.AddFromTemplateSection(section));
			AssertEquals(3, config.ConfigItems.Count);

			var templateExtractor = new TemplateExtractor(systemSectionRepository);
			templateExtractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.User, Factory);

			var parent = Factory.New<DummyBODocSupportable>();
			var parentBOForFiltering = BODocDataProvider.Get(parent);
			var evaluator = new FilterEvaluator(parentBOForFiltering);

			AssertNotEquals(systemSectionRepository.AllSections.Count, customizedSectionRepository.AllSections.Count);

			AssertEquals("Should pick the system one", systemSectionRepository.AllSections.Count, templateExtractor.GetBestDestinationSectionRepository(config, evaluator).AllSections.Count);

			var config2 = Factory.New<StmMenuDocumentConfig>();
			var sections2 = systemSectionRepository.AllSections.Cast<TemplateSection>().Where(s => s.SectionName == "Invoice Body (inc. Grouping, Roll-Ups and Subtotalling)" // 152 rows
																								  || s.SectionName == "Consignment Details - Pickup Date" // 6 rows
																								  || s.SectionName == "Packages (ID, Type, Description, Weight, UoW, Volume, UoV)" // 6 rows
			).ToList();
			sections2.ForEach(section => config2.ConfigItems.AddFromTemplateSection(section));
			AssertEquals(3, config2.ConfigItems.Count);

			AssertEquals("Should pick the customized one because its one section has more rows than the other two sections combined", customizedSectionRepository.AllSections.Count, templateExtractor.GetBestDestinationSectionRepository(config2, evaluator).AllSections.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestGenerateTemplate_DoesNotThrowTooManyColumnsException()
		{
			var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
			var systemSectionRepository = new SectionRepository(systemExcelTemplate);
			Assert("Precondition: systemSectionRepository.AllSections.Count", systemSectionRepository.AllSections.Count > 0);

			var customizedExcelTemplate = new ExcelTemplateForUnitTesting("TooManyColumns.xlsx", TestFilesSubFolder.DocumentTestFiles);
			var customizedSectionRepository = new SectionRepository(customizedExcelTemplate);
			Assert("Precondition: customizedSectionRepository.AllSections.Count", customizedSectionRepository.AllSections.Count > 0);

			var systemStmTemplate = Factory.New<StmTemplateBase>();
			systemStmTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			systemStmTemplate.SO_Template = systemExcelTemplate.GetAsByteArray();

			var customizedStmTemplate = Factory.New<StmTemplateBase>();
			customizedStmTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			customizedStmTemplate.SO_Template = customizedExcelTemplate.GetAsByteArray();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Too Many Columns";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = systemStmTemplate.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = Factory.New<StmMenuDocumentConfig>();
			var configItem = config.ConfigItems.AddNew();
			configItem.S4_SectionItemName = "Test My Section";

			var sections = systemSectionRepository.AllSections.Cast<TemplateSection>().Where(s => s.SectionName == "Company Logo" || s.SectionName == "Recipient with Job Number").ToList();
			sections.ForEach(section => config.ConfigItems.AddFromTemplateSection(section));
			AssertEquals(3, config.ConfigItems.Count);

			TemplateCache.ShouldUseFileSystemCacheInUnitTests.Value = true;

			var generator = new TemplateGenerator(config, pivot, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);
			generator.Generate(null);

			TemplateCache.SetTemplateCacheTimeoutInMinutesForTest(0);
			generator.Generate(null);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateTemplate_DoesNotThrowThereIsNoOpenFileException()
		{
			var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
			var systemStmTemplate = TemplateTestHelper.CreateTemplate(Factory, SectionRepositoryTemplateNames.System, systemExcelTemplate.GetAsByteArray(), "GenericFreightJob");
			var template = TemplateTestHelper.CreateTemplate(Factory, SectionRepositoryTemplateNames.User, null, "GenericFreightJob");
			template.SO_IsUserConfigurable = true;

			template = TemplateTestHelper.CreateTemplate(Factory, SectionRepositoryTemplateNames.User + " [ZH-CN]", null, "GenericFreightJob");
			template.SO_IsUserConfigurable = true;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Generate Template";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = systemStmTemplate.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = Factory.New<StmMenuDocumentConfig>();
			var configItem = config.ConfigItems.AddNew();
			configItem.S4_SectionItemName = "Test My Section";

			TemplateCache.ShouldUseFileSystemCacheInUnitTests.Value = true;
			var generator = new TemplateGenerator(config, pivot, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);
			AssertExceptionThrown(typeof(ExcelInterfaceException), "Your Customized Document Elements template seems to be corrupted. Please delete it or reload it from a backup.", delegate
			{ generator.Generate(null); });

			generator = new TemplateGenerator(config, pivot, Enterprise.Core.SharedConstants.Languages.ChineseSimplified);
			AssertExceptionThrown(typeof(ExcelInterfaceException), "您的 Customized Document Elements [ZH-CN] 模板似乎已损坏。请删除它，或从备份重新载入它。", delegate
			{ generator.Generate(null); });
		}
	}
}
