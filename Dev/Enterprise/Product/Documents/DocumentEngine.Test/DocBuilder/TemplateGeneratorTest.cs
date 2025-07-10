using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplateGeneratorTest : TestCaseWithFactory
	{
		public void TestLocalLanguageSectionsShouldNotBeTranslated()
		{
			DocumentEngineTestHelper.ClearTemplates();

			var systemTemplateHelper = new TemplateTestHelper();
			systemTemplateHelper.AddWorkSheet("Document",
				@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[This is my section.]
{A}-[#EndOfReport]");

			var systemTemplate = systemTemplateHelper.CreateTemplate(Factory, "System Document Elements");

			var customizedTempateHelper = new TemplateTestHelper();
			customizedTempateHelper.AddWorkSheet("Document",
				@"{A}-[#config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[Hello]
{A}-[#ConfigurableSection:GEN:Test, My Section Translated]
{B}-[World]
{A}-[#EndOfReport]");

			var customizedTemplate = customizedTempateHelper.CreateTemplate(Factory, "Customized Document Elements");

			var customizedChineseTempateHelper = new TemplateTestHelper();
			customizedChineseTempateHelper.AddWorkSheet("Document",
				@"{A}-[#config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[Hello]
{A}-[#EndOfReport]");

			customizedChineseTempateHelper.CreateTemplate(Factory, "Customized Document Elements [ZH-CN]");

			var documentCommand = Factory.New<DocumentCommand>();
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = systemTemplate.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = pivot.DocConfigs.AddNew();
			config.S3_IsSystem = ZBool.True;
			config.ConfigItems.AddFromTemplateSection(customizedTemplate.TemplateSections.Find("My Section"));
			config.ConfigItems.AddFromTemplateSection(customizedTemplate.TemplateSections.Find("My Section Translated"));

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "Hello"), new ResourceStringData("", "你好"));
				resourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "World"), new ResourceStringData("", "世界"));

				var generator = new TemplateGenerator(pivot, null, Enterprise.Core.SharedConstants.Languages.ChineseSimplified);
				var excelTemplate = generator.Generate(new FilterEvaluator());

				using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
				{
					AssertMultilineASCIIEquals(
						@"{A}-[#config]
{A}-[ContainsCustomisedSections=True]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[Hello]
{A}-[#SectionBody]
{B}-[世界]
{A}-[#EndOfReport]",
						excelInterface.WorkSheets[0].ToString());
				}
			}
		}

		public void TestIgnoreUserCustomizedTemplatesAndConfigurations()
		{
			DocumentEngineTestHelper.ClearTemplates();

			var systemTemplateHelper = new TemplateTestHelper();
			systemTemplateHelper.AddWorkSheet("Document",
@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[This is my section.]
{A}-[#EndOfReport]");

			var systemTemplate = systemTemplateHelper.CreateTemplate(Factory, "System Document Elements");

			var customizedTempateHelper = new TemplateTestHelper();
			customizedTempateHelper.AddWorkSheet("Document",
@"{A}-[#config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[This is my customized section.]
{A}-[#EndOfReport]");

			customizedTempateHelper.CreateTemplate(Factory, "Customized Document Elements");

			var documentCommand = Factory.New<DocumentCommand>();
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = systemTemplate.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = pivot.DocConfigs.AddNew();
			config.S3_IsSystem = ZBool.True;
			config.ConfigItems.AddFromTemplateSection(systemTemplate.TemplateSections.Find("My Section"));

			TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = true;
			try
			{
				var generator = new TemplateGenerator(pivot, null, Enterprise.Core.SharedConstants.Languages.English);
				var excelTemplate = generator.Generate(new FilterEvaluator());

				using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
				{
					AssertMultilineASCIIEquals("Generate with no customized sections.",
@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is my section.]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
				}
			}
			finally
			{
				TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = false;
			}

			{
				var generator = new TemplateGenerator(pivot, null, Enterprise.Core.SharedConstants.Languages.English);
				var excelTemplate = generator.Generate(new FilterEvaluator());

				using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
				{
					AssertMultilineASCIIEquals("Generate with customized sections.",
@"{A}-[#config]
{A}-[ContainsCustomisedSections=True]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is my customized section.]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
				}
			}
		}

		public void TestGenerateCustomisedTemplateAfterUpdateSystemTemplate()
		{
			DocumentEngineTestHelper.ClearTemplates();

			TemplateCache.ShouldUseFileSystemCacheInUnitTests.Value = true;

			var systemTemplateHelper = new TemplateTestHelper();
			systemTemplateHelper.AddWorkSheet("Document",
@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[This is my section.]
{A}-[#EndOfReport]");

			var systemTemplate = systemTemplateHelper.CreateTemplate(Factory, "System Document Elements");

			var customizedTempateHelper = new TemplateTestHelper();
			customizedTempateHelper.AddWorkSheet("Document",
@"{A}-[#config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN:Test,Test My Section]
{B}-[This is my customized section.]
{A}-[#EndOfReport]");

			customizedTempateHelper.CreateTemplate(Factory, "Customized Document Elements");

			string previousWorkSheet, currentWorkSheet;

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(systemTemplate);
			var configItem = config.ConfigItems.AddNew();
			configItem.S4_SectionItemName = "Test My Section";

			var generator = new TemplateGenerator(config, systemTemplate, Enterprise.Core.SharedConstants.Languages.EnglishAmerican, false);
			var excelTemplate = generator.Generate(null);
			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				previousWorkSheet = excelInterface.WorkSheets[0].ToString();
			}

			var newGenerator = new TemplateGenerator(config, systemTemplate, Enterprise.Core.SharedConstants.Languages.EnglishAmerican, true);
			excelTemplate = newGenerator.Generate(null);
			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				currentWorkSheet = excelInterface.WorkSheets[0].ToString();
			}

			AssertNotEquals("previous template should not equal current template", previousWorkSheet, currentWorkSheet);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestMultiThreadNoThrowException()
		{
			var systemTemplateHelper = new TemplateTestHelper();
			systemTemplateHelper.AddWorkSheet("Document",
				@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[#ConfigurableSection:GEN:Test, My Section]
{B}-[This is my section.]
{A}-[#EndOfReport]");
			var systemTemplate = systemTemplateHelper.CreateTemplate(Factory, "System Document Elements");
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Document";

			var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = systemTemplate.PK;
			pivot.SI_SU = documentCommand.PK;
			var newGenerator = new TemplateGeneratorForMultiThreadTest(pivot, null, Enterprise.Core.SharedConstants.Languages.English);
			var startEvent = new ManualResetEventSlim(false);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, "ReGenerateTemplateTest.xls"));
				var threads = new List<Thread>();
				for (int i = 0; i < 10; i++)
				{
					var thread = new Thread(() =>
					{
						startEvent.Wait();
						newGenerator.GenerateAndStyleTemplateFromExcelFile_Exposed(excelInterface, systemExcelTemplate);
					});
					thread.Start();
					threads.Add(thread);
				}

				startEvent.Set();
				foreach (var thread in threads)
				{
					if (thread.IsAlive)
					{
						thread.Join(1000);
					}
				}
			}
		}

		public void TestGenerateWithGenericSections()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, SectionRepositoryTemplateNames.System, ConfigurableTemplateTestHelper.ConfigurableStripsForTesting);
			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 2")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 3")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 4")).S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 5")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterFirstPageOnly;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 6")).S4_SectionType = GenericSectionUsageList.Codes.PageHeaderStartFromSecondPage;

			var generator = new TemplateGenerator(config, template, Enterprise.Core.SharedConstants.Languages.English, true);
			var excelTemplate = generator.Generate(new FilterEvaluator(BODocDataProvider.Get(Factory.New<OrgHeader>())));

			using (var excelFile = new ExcelInterface())
			{
				using (var stream = excelTemplate.GetAsTemplateStream())
				{
					excelFile.LoadExcelFile(stream);
				}

				AssertMultilineASCIIEquals("excelFile.WorkSheets[0].ToString()",
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#DocumentHeader]
{B}-[This is Generic Section 4.]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[This is Generic Section 4.]
{B}-[This is Generic Section 6.]
{A}-[#SectionBody]
{B}-[This is Generic Section 1.]
{A}-[#SectionBody]
{B}-[This is Generic Section 3.]
{A}-[#FirstPageFooter]
{B}-[This is Generic Section 5.]
{B}-[This is Generic Section 2.]
{A}-[#PageFooter]
{B}-[This is Generic Section 2.]
{A}-[#OnlyOnePageFooter]
{B}-[This is Generic Section 5.]
{B}-[This is Generic Section 2.]
{A}-[#LastPageFooter]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]", excelFile.WorkSheets[0].ToString());
			}
		}

		public void TestMissingSectionResultsInAWarningInTheReportErrorManager()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=GenericFreightJob]
{A}-[#ConfigurableSection:BOD, Test]
{A}-[#SectionBody]
{B}-[This is a Test.]
{A}-[#EndOfReport]");
			template.SO_Name = SectionRepositoryTemplateNames.User;
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Document";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = pivot.DocConfigs.AddNew();
			config.S3_IsSystem = ZBool.True;

			var configItem = config.ConfigItems.AddNew();
			configItem.S4_SectionType = "BOD";
			configItem.S4_SectionItemName = "Test";

			var missingConfigItem = config.ConfigItems.AddNew();
			missingConfigItem.S4_SectionType = "BOD";
			missingConfigItem.S4_SectionItemName = "Missing Section";

			BusinessObject declaration = Factory.New(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			documentCommand.Parent = (IDocumentSupportable)declaration;

			var templateGenerater = new TemplateGenerator(pivot, null, Enterprise.Core.SharedConstants.Languages.English);
			templateGenerater.Generate(new FilterEvaluator(BODocDataProvider.Get(declaration)));
			AssertEquals("templateGenerater.Errors.Count", 1, templateGenerater.Errors.Length);
			AssertEquals("templateGenerater.Errors[0].Message", "Section [Missing Section] could not be found and has been ignored.", templateGenerater.Errors[0].Message);
		}

		public void TestIsDocumentConfigValid()
		{
			var pivot = Factory.New<StmMenuTemplatePivotBase>();

			var docBuilderTemplate = GetTestTemplate(SectionRepositoryTemplateNames.System, "No need for cellText here");
			docBuilderTemplate.SO_DataContext = "DT1";
			Assert(docBuilderTemplate.IsDocBuilderStyle);

			var nonDocBuilderTemplate = Factory.New<StmTemplateBase>();
			nonDocBuilderTemplate.SO_DataContext = "DT2";
			Assert(!nonDocBuilderTemplate.IsDocBuilderStyle);

			// No document config and docbuilder document => invalid config
			pivot.SI_SO = docBuilderTemplate.PK;
			var templateGenerator = new TemplateGenerator(pivot, null, SharedConstants.Languages.French);
			Assert(!templateGenerator.IsDocumentConfigValid);
			Assert(!templateGenerator.IsDocumentConfigTemplate);

			// No document config and non-docbuilder document => valid config
			pivot.SI_SO = nonDocBuilderTemplate.PK;
			templateGenerator = new TemplateGenerator(pivot, null, SharedConstants.Languages.French);
			Assert(templateGenerator.IsDocumentConfigValid);
			Assert(!templateGenerator.IsDocumentConfigTemplate);

			// Has applicable Document config and docbuilder document => valid config
			pivot.SI_SO = docBuilderTemplate.PK;
			StmMenuDocumentConfig docConfig = pivot.DocConfigs.AddNew();
			templateGenerator = new TemplateGenerator(pivot, null, SharedConstants.Languages.French);
			Assert(templateGenerator.IsDocumentConfigValid);
			Assert(!templateGenerator.IsDocumentConfigTemplate);

			// Has un-applicable template Document config and docbuilder document => invalid config
			docConfig.S3_IsTemplate = true;
			templateGenerator = new TemplateGenerator(pivot, null, SharedConstants.Languages.French);
			Assert(!templateGenerator.IsDocumentConfigValid);
			Assert(templateGenerator.IsDocumentConfigTemplate);

			// Has un-applicable Document config and docbuilder document => invalid config
			docConfig.S3_IsTemplate = false;
			pivot.SI_SO = docBuilderTemplate.PK;
			docConfig.S3_OH = Guid.NewGuid();
			templateGenerator = new TemplateGenerator(pivot, null, SharedConstants.Languages.French);
			Assert(!templateGenerator.IsDocumentConfigValid);
			Assert(!templateGenerator.IsDocumentConfigTemplate);
		}

		public void TestDocumentTitle()
		{
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			var docBuilderTemplate = GetTestTemplate(SectionRepositoryTemplateNames.System, "No need for cellText here");
			pivot.SI_SO = docBuilderTemplate.PK;
			var docConfig = pivot.DocConfigs.AddNew();
			docConfig.S3_OverrideEmailSubject = "Override Document Name";
			var templateGenerator = new TemplateGenerator(pivot, null, SharedConstants.Languages.French);
			AssertEquals("Override Document Name", templateGenerator.DocumentTitle);
		}

		public void TestFiltersBySection()
		{
			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			using (TestGenerateForManager testManager = new TestGenerateForManager(pivot, null, null))
			{
				testManager.ParentBOForFilters.Z0_VarCharMax = "THEORY OF CONSTRAINTS";

				StmMenuDocumentConfig docConfig = testManager.AddNewDocConfig(pivot, null, null, true, "Got No Filters");
				StmMenuDocumentConfigItem configItemMatchingFilter = testManager.AddConfigItem(docConfig, "Filter Matches");
				configItemMatchingFilter.S4_FilterList = "'<Z0_VarCharMax>' == 'THEORY OF CONSTRAINTS'".Replace('\'', '"');
				StmMenuDocumentConfigItem configItemNonMatchingFilter = testManager.AddConfigItem(docConfig, "Filter Doesn't Match");
				configItemNonMatchingFilter.S4_FilterList = "'<Z0_VarCharMax>' != 'THEORY OF CONSTRAINTS'".Replace('\'', '"');

				testManager.AssertGotRightBodySectionInserted("Got No Filters", "Filter Matches");
			}
		}

		public void TestDataContextPerConfiguration()
		{
			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			StmTemplateBase template = Factory.New<StmTemplateBase>();
			pivot.SI_SO = template.PK;
			template.SO_DataContext = "GenericFreightJob";

			AssertEquals("DataContext from Template where there is no docConfig", Core.Constants.DataContext.GenericFreightJob, new TemplateGenerator(pivot, null, SharedConstants.Languages.English).DataContext.DataContext);

			StmMenuDocumentConfig docConfig = pivot.DocConfigs.AddNew();
			docConfig.S3_IsSystem = true;
			AssertEquals("DataContext from Template where no DataContext exists on docConfig", Core.Constants.DataContext.GenericFreightJob, new TemplateGenerator(pivot, null, SharedConstants.Languages.English).DataContext.DataContext);

			docConfig.S3_OverrideDataContext = "GenericFreightJobByContainerIfFCL";
			AssertEquals("DataContext from docConfig when it's there", Core.Constants.DataContext.GenericFreightJobByContainerIfFCL, new TemplateGenerator(pivot, null, SharedConstants.Languages.English).DataContext.DataContext);

			docConfig.S3_OverrideDataContext = "XXX";
			AssertEquals("DataContext from Template when DataContext on docConfig is invalid", Core.Constants.DataContext.GenericFreightJob, new TemplateGenerator(pivot, null, SharedConstants.Languages.English).DataContext.DataContext);
		}

		public void TestForeignLanguageOnlyKicksInWhenCompanyAndClientSpeakTheSameForeignLanguage()
		{
			TemplateCache.SuspendTemplateCache = true;
			OrgHeader companyOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
			ZString saveLanguage = companyOrgProxy.OH_Language;
			try
			{
				StmTemplateBase systemSectionsTemplate = GetTestTemplate(
						SectionRepositoryTemplateNames.System,
						"ENGLISH MUSTARD");

				GetTestTemplate(
						SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.System, French),
						"FRENCH MUSTARD");

				StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SO = systemSectionsTemplate.PK;

				StmMenuDocumentConfig docConfig = pivot.DocConfigs.AddNew();
				docConfig.S3_IsSystem = true;

				StmMenuDocumentConfigItem configItem = docConfig.ConfigItems.AddNew();
				configItem.S4_SectionType = "AAA";
				configItem.S4_SectionItemName = "AAA Section";

				OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

				companyOrgProxy.OH_Language = English;
				client.OH_Language = English;
				AssertTemplate("ENGLISH MUSTARD", new TemplateGenerator(pivot, client, English).Generate(new FilterEvaluator()));

				companyOrgProxy.OH_Language = French;
				AssertTemplate("ENGLISH MUSTARD", new TemplateGenerator(pivot, client, English).Generate(new FilterEvaluator()));

				client.OH_Language = French;
				AssertTemplate("FRENCH MUSTARD", new TemplateGenerator(pivot, client, French).Generate(new FilterEvaluator()), true);

				companyOrgProxy.OH_Language = German;
				AssertTemplate("ENGLISH MUSTARD", new TemplateGenerator(pivot, client, German).Generate(new FilterEvaluator()));

				client.OH_Language = German;
				AssertTemplate("ENGLISH MUSTARD", new TemplateGenerator(pivot, client, German).Generate(new FilterEvaluator()));

				GetTestTemplate(
						SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.System, German),
						"GERMAN SAUERKRAUT");
				AssertTemplate("GERMAN SAUERKRAUT", new TemplateGenerator(pivot, client, German).Generate(new FilterEvaluator()), true);
			}
			finally
			{
				companyOrgProxy.OH_Language = saveLanguage;
			}
		}

		public void TestGenerateForeignLanguageFallback()
		{
			TemplateCache.SuspendTemplateCache = true;
			StmTemplateBase systemSectionsTemplate = GetTestTemplate(
					SectionRepositoryTemplateNames.System,
					"ENGLISH MUSTARD BURNS");

			GetTestTemplate(
					SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.System, German),
					"EAT GERMAN BREAD");

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = systemSectionsTemplate.PK;

			StmMenuDocumentConfig docConfig = pivot.DocConfigs.AddNew();

			StmMenuDocumentConfigItem configItem = docConfig.ConfigItems.AddNew();
			configItem.S4_SectionType = "AAA";
			configItem.S4_SectionItemName = "AAA Section";

			AssertTemplate("ENGLISH MUSTARD BURNS", new TemplateGenerator(docConfig, pivot, ZString.Empty).Generate(new FilterEvaluator()));

			AssertTemplate("EAT GERMAN BREAD", new TemplateGenerator(docConfig, pivot, German).Generate(new FilterEvaluator()), true);

			GetTestTemplate(
					SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.User, German),
					"DRINK GERMAN BEER");

			AssertTemplate("DRINK GERMAN BEER", new TemplateGenerator(docConfig, pivot, German).Generate(new FilterEvaluator()), true);
		}

		StmTemplateBase GetTestTemplate(string templateName, string cellText)
		{
			StmTemplateBase systemSectionsTemplate = Factory.New<StmTemplateBase>();
			systemSectionsTemplate.SO_Name = templateName;
			systemSectionsTemplate.SO_DataContext = "!!!";
			systemSectionsTemplate.SO_Template = SectionRepositoryTestHelper.GetConfigurableTemplateBlob(cellText);
			return systemSectionsTemplate;
		}

		public void TestGenerate()
		{
			OrgHeader orgRight = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgWrong = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany companyRight = GlbCompany.CurrentCompany;
			GlbCompany companyWrong = Factory.New<GlbCompany>();

			StmMenuTemplatePivotBase pivotRight = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			StmMenuTemplatePivotBase pivotWrong = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			StmMenuTemplatePivotBase loadedPivotRight = newFactory.Load<StmMenuTemplatePivotBase>(pivotRight.PK);

			using (TestGenerateForManager testManager = new TestGenerateForManager(pivotRight, pivotWrong, orgRight))
			{
				testManager.AddNewDocConfig(pivotRight, null, null, true, "SystemRight", "TestDocBuilderTemplate");
				testManager.AddNewDocConfig(pivotWrong, null, null, true, "SystemWrong", "TestDocBuilder");
				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("SystemRight");

				testManager.AddNewDocConfig(pivotRight, null, companyWrong, false, "CompanyWrong");
				testManager.AddNewDocConfig(pivotRight, null, companyRight, false, "CompanyRight");
				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("CompanyRight");

				testManager.AddNewDocConfig(pivotRight, orgRight, null, false, "ClientRight");
				testManager.AddNewDocConfig(pivotRight, orgWrong, null, false, "ClientWrong");
				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("ClientRight");

				testManager.AddNewDocConfig(pivotRight, orgWrong, companyWrong, false, "ClientCompanyWrong");
				StmMenuDocumentConfig latestDocConfig = testManager.AddNewDocConfig(pivotRight, orgRight, companyRight, false, "ClientCompanyRight");
				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("ClientCompanyRight");

				StmMenuDocumentConfigItem latestConfigItem = testManager.AddConfigItem(latestDocConfig, "Moo");
				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("ClientCompanyRight", "Moo");

				foreach (StmMenuDocumentConfig docConfig in loadedPivotRight.DocConfigs)
				{
					StmMenuDocumentConfigItemCollection dummy = docConfig.ConfigItems; // Initialise the propety.
				}

				latestDocConfig.ConfigItems.MoveUp(1);
				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("Moo", "ClientCompanyRight");
				testManager.AssertGotRightBodySectionInserted(latestDocConfig, "Moo", "ClientCompanyRight");

				Factory.Save();
				testManager.AssertGotRightBodySectionInserted("Moo", "ClientCompanyRight");
			}
		}

		public void TestGenerateForDoesNotCrashIfConfigRowDoesNotExist()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var template = Factory.New<StmTemplateBase>();
				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				var docConfig = pivot.DocConfigs.AddNew();
				template.SO_Template = resourceRetriever.GetBytes("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls");
				pivot.SI_SO = template.PK;
				AssertNotNull("GenerateFor()", new TemplateGenerator(pivot, GlbCompany.CurrentCompany.OrgProxy, SharedConstants.Languages.English).Generate(new FilterEvaluator()));
			}
		}

		public void TestGenerateForSystemSectionRepositoryFallsBackToUserSectionRepository()
		{
			StmTemplateBase systemSectionsTemplate = Factory.New<StmTemplateBase>();
			StmTemplateBase userSectionsTemplate = Factory.New<StmTemplateBase>();
			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			StmMenuDocumentConfig docConfig = pivot.DocConfigs.AddNew();

			pivot.SI_SO = systemSectionsTemplate.PK;
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			userSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			systemSectionsTemplate.SO_DataContext = "!!!";
			userSectionsTemplate.SO_DataContext = "!!!";
			systemSectionsTemplate.SO_Template = SectionRepositoryTestHelper.GetSystemRepositoryTemplateBlob();
			userSectionsTemplate.SO_Template = SectionRepositoryTestHelper.GetUserRepositoryTemplateBlob();

			StmMenuDocumentConfigItem configItem1 = docConfig.ConfigItems.AddNew();
			StmMenuDocumentConfigItem configItem2 = docConfig.ConfigItems.AddNew();
			StmMenuDocumentConfigItem configItem3 = docConfig.ConfigItems.AddNew();

			configItem1.S4_SectionType = "CCC";
			configItem1.S4_SectionItemName = "CCC Section";
			configItem2.S4_SectionType = "AAA";
			configItem2.S4_SectionItemName = "AAA Section";
			configItem3.S4_SectionType = "BBB";
			configItem3.S4_SectionItemName = "BBB Section";

			ExcelTemplate generatedTemplate = new TemplateGenerator(docConfig, pivot, ZString.Empty).Generate(new FilterEvaluator());

			using (ExcelInterface excel = new ExcelInterface())
			{
				using (Stream stream = generatedTemplate.GetAsTemplateStream())
				{
					excel.LoadExcelFile(stream);
				}

				ExcelWorkSheet workSheet = excel.WorkSheets[0];
				AssertMultilineASCIIEquals("Merged Template Contents should match", @"
{A}-[#Config]
{A}-[ContainsCustomisedSections=True]
{B}-[CCC 1-1]
{C}-[CCC 2-2]
{B}-[ZZZ 1-1]
{B}-[BBB 1-1]   {C}-[BBB 1-2]
{A}-[#EndOfReport]"
					.Trim(), workSheet.ToString());
			}
		}

		public void TestGenerateTemplateWithCustomisableSections_ShouldSetFlag()
		{
			TemplateCache.SuspendTemplateCache = true;

			var systemSectionsTemplate = Factory.New<StmTemplateBase>();
			var userSectionsTemplate = Factory.New<StmTemplateBase>();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			var docConfig = pivot.DocConfigs.AddNew();

			pivot.SI_SO = systemSectionsTemplate.PK;
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			userSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			systemSectionsTemplate.SO_DataContext = "!!!";
			userSectionsTemplate.SO_DataContext = "!!!";
			systemSectionsTemplate.SO_Template = SectionRepositoryTestHelper.GetSystemRepositoryTemplateBlob();
			userSectionsTemplate.SO_Template = SectionRepositoryTestHelper.GetUserRepositoryTemplateBlob();

			var configItem1 = docConfig.ConfigItems.AddNew();
			var configItem2 = docConfig.ConfigItems.AddNew();
			var configItem3 = docConfig.ConfigItems.AddNew();
			var configItem4 = docConfig.ConfigItems.AddNew();

			configItem1.S4_SectionType = "CCC";
			configItem1.S4_SectionItemName = "CCC Section";
			configItem2.S4_SectionType = "AAA";
			configItem2.S4_SectionItemName = "AAA Section";
			configItem3.S4_SectionType = "BBB";
			configItem3.S4_SectionItemName = "BBB Section";

			var generatedTemplate = new TemplateGenerator(docConfig, pivot, ZString.Empty).Generate(new FilterEvaluator());
			Assert("Should contain customised sections", generatedTemplate.ContainsCustomisedSections);
		}

		public void TestGenerateTemplateWithOnlySystemSections_ShouldNotSetFlag()
		{
			TemplateCache.SuspendTemplateCache = true;

			var systemSectionsTemplate = Factory.New<StmTemplateBase>();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			var docConfig = pivot.DocConfigs.AddNew();

			pivot.SI_SO = systemSectionsTemplate.PK;
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			systemSectionsTemplate.SO_DataContext = "!!!";
			systemSectionsTemplate.SO_Template = SectionRepositoryTestHelper.GetSystemRepositoryTemplateBlob();

			var configItem1 = docConfig.ConfigItems.AddNew();
			var configItem2 = docConfig.ConfigItems.AddNew();
			var configItem3 = docConfig.ConfigItems.AddNew();

			configItem1.S4_SectionType = "CCC";
			configItem1.S4_SectionItemName = "CCC Section";
			configItem2.S4_SectionType = "AAA";
			configItem2.S4_SectionItemName = "AAA Section";
			configItem3.S4_SectionType = "BBB";
			configItem3.S4_SectionItemName = "BBB Section";

			var generatedTemplate = new TemplateGenerator(docConfig, pivot, ZString.Empty).Generate(new FilterEvaluator());
			Assert("Should not contain customised sections", !generatedTemplate.ContainsCustomisedSections);
		}

		[ExpectNoExceptions]
		public void TestGenerateTemplateNotIncludingCustomSections_ShouldNotCacheResultingTemplate()
		{
			AssertCacheKeyShouldBeReadOnly(false, true);
			AssertCacheKeyShouldBeReadOnly(true, false);
		}

		void AssertCacheKeyShouldBeReadOnly(bool areCustomizedSectionsIncluded, bool isReadOnlyCacheKey)
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;

			var config = Factory.New<StmMenuDocumentConfig>();
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN:Test, Section]
{B}-[This is customized section.]
{A}-[#EndOfReport]");
			var template = helper.CreateTemplate(Factory, "Customized Document Elements");

			var generator = new TemplateGenerator(config, template, SharedConstants.Languages.English, areCustomizedSectionsIncluded);
			using (var excelInterface = new ExcelInterface(template.GetExcelTemplate().GetAsByteArray(), false))
			{
				var startingCacheCount = TemplateCache.CachingTemplateCache.Cache.Count();

				generator.Generate(null);

				var expectedCacheCount = isReadOnlyCacheKey ? startingCacheCount : startingCacheCount + 1;
				AssertEquals(expectedCacheCount, TemplateCache.CachingTemplateCache.Cache.Count());
			}
		}

		class TemplateGeneratorForMultiThreadTest : TemplateGenerator
		{
			public TemplateGeneratorForMultiThreadTest(StmMenuTemplatePivot menuTemplatePivot, OrgHeader client, ZString language) : base(menuTemplatePivot, client, language)
			{
			}

			public TemplateGeneratorForMultiThreadTest(IDocumentConfig documentConfig, StmMenuTemplatePivot menuTemplatePivot, ZString language) : base(documentConfig, menuTemplatePivot, language)
			{
			}

			public TemplateGeneratorForMultiThreadTest(IDocumentConfig documentConfig, StmTemplate template, ZString language, bool isCustomizedSectionsIncluded = true) : base(documentConfig, template, language, isCustomizedSectionsIncluded)
			{
			}

			public void GenerateAndStyleTemplateFromExcelFile_Exposed(ExcelInterface docBuilderFile, ExcelTemplate sourceTemplate) => base.GenerateAndStyleTemplateFromExcelFile(docBuilderFile, sourceTemplate);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(StmMenuDocumentConfigItemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuDocumentConfigSchema.Constants.TableName);

			var query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, ZBool.True);
			query.AddToFilter(StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.System);

			foreach (StmTemplate template in Factory.Load<StmTemplate>(query))
			{
				StmMenuTemplatePivot[] pivots = Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SO, template.PK));
				foreach (StmMenuTemplatePivot pivot in pivots)
				{
					pivot.Delete();
				}
				template.Delete();
			}
		}
		const string German = Enterprise.Core.SharedConstants.Languages.German;
		const string French = Enterprise.Core.SharedConstants.Languages.French;
		const string English = Enterprise.Core.SharedConstants.Languages.English;

		static void AssertTemplate(ZString expectedAAASectionText, ExcelTemplate generatedTemplate, bool containCustomisedSections = false)
		{
			using (ExcelInterface excel = new ExcelInterface())
			{
				using (Stream stream = generatedTemplate.GetAsTemplateStream())
				{
					excel.LoadExcelFile(stream);
				}

				ExcelWorkSheet workSheet = excel.WorkSheets[0];
				string actualText = workSheet[containCustomisedSections ? 2 : 1, 1].ToString();
				AssertEquals("Section AAA Text Content", expectedAAASectionText, actualText);
			}
		}

		class TestGenerateForManager : IDisposable
		{
			public TestGenerateForManager(StmMenuTemplatePivotBase pivotRight, StmMenuTemplatePivotBase pivotWrong, OrgHeader client)
			{
				Pivot = pivotRight;
				Client = client;
				BusinessObjectFactory factory = Pivot.Factory;
				TemplateRecord = factory.New<StmTemplateBase>();
				TemplateRecord.SO_Name = SectionRepositoryTemplateNames.User;
				pivotRight.SI_SO = TemplateRecord.PK;
				if (pivotWrong != null)
				{
					pivotWrong.SI_SO = TemplateRecord.PK;
				}
				ParentBOForFilters = factory.New<DummyBusinessObject>();
			}
			readonly StmMenuTemplatePivotBase Pivot;
			readonly OrgHeader Client;
			readonly StmTemplateBase TemplateRecord;
			public readonly DummyBusinessObject ParentBOForFilters;

			public void AssertGotRightBodySectionInserted(params string[] bodySectionIDs)
			{
				AssertGotRightBodySectionInserted(Pivot, bodySectionIDs);
			}

			void AssertGotRightBodySectionInserted(StmMenuTemplatePivotBase pivot, params string[] bodySectionIDs)
			{
				string expected = GetExpectedBodySection(bodySectionIDs);
				string actual = GetFirstWorkSheetAsString(new TemplateGenerator(pivot, Client, SharedConstants.Languages.English).Generate(new FilterEvaluator(BODocDataProvider.Get(ParentBOForFilters))));
				TestCaseWithFactory.AssertMultilineASCIIEquals("Expecting just " + string.Join(", ", bodySectionIDs), expected, actual);
			}

			public void AssertGotRightBodySectionInserted(StmMenuDocumentConfig configRow, params string[] bodySectionIDs)
			{
				string expected = GetExpectedBodySection(bodySectionIDs);
				string actual = GetFirstWorkSheetAsString(new TemplateGenerator(configRow, configRow.MenuTemplatePivot, ZString.Empty).Generate(new FilterEvaluator(BODocDataProvider.Get(ParentBOForFilters))));
				TestCaseWithFactory.AssertMultilineASCIIEquals("Expecting just " + string.Join(", ", bodySectionIDs), expected, actual);
			}

			string GetExpectedBodySection(string[] bodySectionIDs)
			{
				UpdateTemplateRecordWithContentsOfGeneratedSpreadSheet();
				string fullBodySection = null;
				foreach (string bodySectionID in bodySectionIDs)
				{
					string currentBodySection = "{A}-[#SectionBody:Data=" + bodySectionID + "Collection]\r\n{B}-[" + bodySectionID + "]\r\n";
					fullBodySection += currentBodySection;
				}
				return ExpectedGeneratedLayout.Trim().Replace("{_0_}", fullBodySection.Trim());
			}

			void UpdateTemplateRecordWithContentsOfGeneratedSpreadSheet()
			{
				using (Stream stream = new MemoryStream())
				{
					SourceSpreadSheet.SaveToStream(stream);
					stream.Position = 0;
					TemplateRecord.SO_Template = AttachmentDef.StreamToByteArray(stream);
				}
			}

			const string ExpectedGeneratedLayout = @"
{A}-[#config]
{A}-[Name=SourceWorkSheet]
{A}-[DataContext=.DummyBusinessObject]
{_0_}
{A}-[#EndOfReport]
";

			static string GetFirstWorkSheetAsString(ExcelTemplate template)
			{
				string result = "";
				using (ExcelInterface outputSpreadSheet = new ExcelInterface())
				using (Stream outputAsStream = template.GetAsTemplateStream())
				{
					outputSpreadSheet.LoadExcelFile(outputAsStream);
					result = outputSpreadSheet.WorkSheets[0].ToString();
				}
				return result;
			}

			public StmMenuDocumentConfig AddNewDocConfig(StmMenuTemplatePivotBase pivot, OrgHeader org, GlbCompany company, ZBool isSystem, ZString bodySectionID, string description = "")
			{
				StmMenuDocumentConfig result = pivot.DocConfigs.AddNew();
				result.S3_GC = company == null ? ZGuid.Empty : company.PK;
				result.S3_OH = org == null ? ZGuid.Empty : org.PK;
				result.S3_IsSystem = isSystem;
				result.S3_Description = description;
				AddConfigItem(result, bodySectionID);
				return result;
			}

			public StmMenuDocumentConfigItem AddConfigItem(StmMenuDocumentConfig docConfig, ZString bodySectionID)
			{
				StmMenuDocumentConfigItem result = docConfig.ConfigItems.AddNew();
				result.S4_PrintOrder = docConfig.ConfigItems.Count;
				result.S4_SectionItemName = bodySectionID;
				result.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
				AddBodySection(docConfig, bodySectionID);
				return result;
			}

			void AddBodySection(StmMenuDocumentConfig docConfig, ZString bodySectionID)
			{
				int row = docConfig.ConfigItems.Count * 3;
				SourceWorkSheet.InsertRows(row, 3);
				SourceWorkSheet[row, 0] = "#ConfigurableSection:BOD, " + bodySectionID;
				SourceWorkSheet[row + 1, 0] = "#SectionBody:Data=" + bodySectionID + "Collection";
				SourceWorkSheet[row + 2, 1] = bodySectionID;
			}

			public ExcelWorkSheet SourceWorkSheet
			{
				get { return fSourceWorkSheet ?? (fSourceWorkSheet = GetNewSourceWorkSheet()); }
			}
			ExcelWorkSheet fSourceWorkSheet;
			ExcelWorkSheet GetNewSourceWorkSheet()
			{
				SourceSpreadSheet.NewExcelFile(1);
				var result = SourceSpreadSheet.WorkSheets[0];
				result[0, 0] = "#config";
				result[1, 0] = "Name=SourceWorkSheet";
				result[2, 0] = "DataContext=.DummyBusinessObject";
				result[3, 0] = "#EndOfReport";
				return result;
			}

			public ExcelInterface SourceSpreadSheet
			{
				get { return fSourceSpeadsheet ?? (fSourceSpeadsheet = new ExcelInterface()); }
			}
			ExcelInterface fSourceSpeadsheet;

			void IDisposable.Dispose()
			{
				if (fSourceSpeadsheet != null)
				{
					fSourceSpeadsheet.Dispose();
				}
			}
		}
		#endregion
	}
}
