using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(TemporaryStmMenuDocumentConfig))]
	sealed class TemporaryStmMenuDocumentConfigTest : StmMenuDocumentConfigTestCase<TemporaryStmMenuDocumentConfig>
	{
		public void TestCategoryFilter()
		{
			var systemSectionsTemplate = Source.MenuTemplatePivot.Template;
			systemSectionsTemplate.SO_DataContext = "Moo";
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			systemSectionsTemplate.SO_Template = CreateExcelTemplate(new string[]
			{
				"DHD:AAA, System Document Header",
				"BOD:BBB, System Body 1",
				"BOD:BBB, System Body 2",
				"PFT:DDD, System Page Footer 1",
				"PFT:EEE, System Page Footer 2",
			});

			AssertEquals("AvailableSections.Count", 5, DocConfig.AvailableSections.Count);
			CombineAssertions(() =>
			{
				AssertEquals("AvailableSections[0].TypeCode", "DHD", DocConfig.AvailableSections[0].TypeCode);
				AssertEquals("AvailableSections[0].SectionName", "System Document Header", DocConfig.AvailableSections[0].SectionName);
				AssertEquals("AvailableSections[0].SectionName", "AAA", DocConfig.AvailableSections[0].Category);
				AssertEquals("AvailableSections[1].TypeCode", "BOD", DocConfig.AvailableSections[1].TypeCode);
				AssertEquals("AvailableSections[1].SectionName", "System Body 1", DocConfig.AvailableSections[1].SectionName);
				AssertEquals("AvailableSections[1].SectionName", "BBB", DocConfig.AvailableSections[1].Category);
				AssertEquals("AvailableSections[2].TypeCode", "BOD", DocConfig.AvailableSections[2].TypeCode);
				AssertEquals("AvailableSections[2].SectionName", "System Body 2", DocConfig.AvailableSections[2].SectionName);
				AssertEquals("AvailableSections[2].SectionName", "BBB", DocConfig.AvailableSections[2].Category);
				AssertEquals("AvailableSections[3].TypeCode", "PFT", DocConfig.AvailableSections[3].TypeCode);
				AssertEquals("AvailableSections[3].SectionName", "System Page Footer 1", DocConfig.AvailableSections[3].SectionName);
				AssertEquals("AvailableSections[3].SectionName", "DDD", DocConfig.AvailableSections[3].Category);
				AssertEquals("AvailableSections[4].TypeCode", "PFT", DocConfig.AvailableSections[4].TypeCode);
				AssertEquals("AvailableSections[4].SectionName", "System Page Footer 2", DocConfig.AvailableSections[4].SectionName);
				AssertEquals("AvailableSections[4].SectionName", "EEE", DocConfig.AvailableSections[4].Category);
			});

			DocConfig.CategoryFilter = "AAA";
			AssertEquals("AvailableSections.Count", 1, DocConfig.AvailableSections.Count);
			CombineAssertions(() =>
			{
				AssertEquals("AvailableSections[0].TypeCode", "DHD", DocConfig.AvailableSections[0].TypeCode);
				AssertEquals("AvailableSections[0].SectionName", "System Document Header", DocConfig.AvailableSections[0].SectionName);
				AssertEquals("AvailableSections[0].SectionName", "AAA", DocConfig.AvailableSections[0].Category);
			});

			DocConfig.CategoryFilter = "BBB";
			AssertEquals("AvailableSections.Count", 2, DocConfig.AvailableSections.Count);
			CombineAssertions(() =>
			{
				AssertEquals("AvailableSections[0].TypeCode", "BOD", DocConfig.AvailableSections[0].TypeCode);
				AssertEquals("AvailableSections[0].SectionName", "System Body 1", DocConfig.AvailableSections[0].SectionName);
				AssertEquals("AvailableSections[0].SectionName", "BBB", DocConfig.AvailableSections[0].Category);
				AssertEquals("AvailableSections[1].TypeCode", "BOD", DocConfig.AvailableSections[1].TypeCode);
				AssertEquals("AvailableSections[1].SectionName", "System Body 2", DocConfig.AvailableSections[1].SectionName);
				AssertEquals("AvailableSections[1].SectionName", "BBB", DocConfig.AvailableSections[1].Category);
			});
		}

		public void TestCategories()
		{
			var systemSectionsTemplate = Source.MenuTemplatePivot.Template;
			systemSectionsTemplate.SO_DataContext = "Moo";
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			systemSectionsTemplate.SO_Template = CreateExcelTemplate(new string[]
			{
				"DHD:AAA, System Document Header",
				"BOD:BBB, System Body 1",
				"BOD:BBB, System Body 2",
				"PFT:DDD, System Page Footer 1",
				"PFT:EEE, System Page Footer 2",
			});

			AssertEquals("AvailableSections.Count", 5, DocConfig.AvailableSections.Count);
			CombineAssertions(() =>
			{
				AssertEquals("AvailableSections[0].TypeCode", "DHD", DocConfig.AvailableSections[0].TypeCode);
				AssertEquals("AvailableSections[0].SectionName", "System Document Header", DocConfig.AvailableSections[0].SectionName);
				AssertEquals("AvailableSections[0].SectionName", "AAA", DocConfig.AvailableSections[0].Category);
				AssertEquals("AvailableSections[1].TypeCode", "BOD", DocConfig.AvailableSections[1].TypeCode);
				AssertEquals("AvailableSections[1].SectionName", "System Body 1", DocConfig.AvailableSections[1].SectionName);
				AssertEquals("AvailableSections[1].SectionName", "BBB", DocConfig.AvailableSections[1].Category);
				AssertEquals("AvailableSections[2].TypeCode", "BOD", DocConfig.AvailableSections[2].TypeCode);
				AssertEquals("AvailableSections[2].SectionName", "System Body 2", DocConfig.AvailableSections[2].SectionName);
				AssertEquals("AvailableSections[2].SectionName", "BBB", DocConfig.AvailableSections[2].Category);
				AssertEquals("AvailableSections[3].TypeCode", "PFT", DocConfig.AvailableSections[3].TypeCode);
				AssertEquals("AvailableSections[3].SectionName", "System Page Footer 1", DocConfig.AvailableSections[3].SectionName);
				AssertEquals("AvailableSections[3].SectionName", "DDD", DocConfig.AvailableSections[3].Category);
				AssertEquals("AvailableSections[4].TypeCode", "PFT", DocConfig.AvailableSections[4].TypeCode);
				AssertEquals("AvailableSections[4].SectionName", "System Page Footer 2", DocConfig.AvailableSections[4].SectionName);
				AssertEquals("AvailableSections[4].SectionName", "EEE", DocConfig.AvailableSections[4].Category);
			});

			AssertEquals("DocConfig.Categories.Length", 5, DocConfig.Categories.Count);
			CombineAssertions(() =>
			{
				AssertEquals("DocConfig.Categories[0]", string.Empty, DocConfig.Categories[0].Code);
				AssertEquals("DocConfig.Categories[1]", "AAA", DocConfig.Categories[1].Code);
				AssertEquals("DocConfig.Categories[2]", "BBB", DocConfig.Categories[2].Code);
				AssertEquals("DocConfig.Categories[3]", "DDD", DocConfig.Categories[3].Code);
				AssertEquals("DocConfig.Categories[4]", "EEE", DocConfig.Categories[4].Code);
			});
		}

		public void TestCategories_ListIsSortedAlphabetically()
		{
			var systemSectionsTemplate = Source.MenuTemplatePivot.Template;
			systemSectionsTemplate.SO_DataContext = "Moo";
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			systemSectionsTemplate.SO_Template = CreateExcelTemplate(new string[]
			{
				"BOD:BBB, System Body 1",
				"BOD:BBB, System Body 2",
				"PFT:EEE, System Page Footer 2",
				"DHD:AAA, System Document Header",
			});

			AssertEquals("DocConfig.Categories.Length", 4, DocConfig.Categories.Count);
			CombineAssertions(() =>
			{
				AssertEquals("DocConfig.Categories[0]", string.Empty, DocConfig.Categories[0].Code);
				AssertEquals("DocConfig.Categories[1]", "AAA", DocConfig.Categories[1].Code);
				AssertEquals("DocConfig.Categories[2]", "BBB", DocConfig.Categories[2].Code);
				AssertEquals("DocConfig.Categories[3]", "EEE", DocConfig.Categories[3].Code);
			});
		}

		public void TestTemporaryStmMenuDocumentConfigHasNewlyCreatedSections()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var userTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, SectionRepositoryTemplateNames.User,
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");

			Factory.Save();

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(systemTemplate);

			var temp = TemporaryStmMenuDocumentConfig.New(config);
			AssertEquals("temp.TemplateSections.Contains(\"Generic Section 1\")", true, temp.TemplateSections.Contains("Generic Section 1"));
			AssertEquals("temp.TemplateSections.Contains(\"Generic Section 2\")", false, temp.TemplateSections.Contains("Generic Section 2"));

			userTemplate.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");

			temp = TemporaryStmMenuDocumentConfig.New(config);
			var section1 = temp.TemplateSections.Find("Generic Section 1");
			var section2 = temp.TemplateSections.Find("Generic Section 2");
			AssertNotNull(section1);
			AssertNotNull(section2);

			var configItem1 = temp.ConfigItems.AddFromTemplateSection(section1);
			var configItem2 = temp.ConfigItems.AddFromTemplateSection(section2);

			CombineAssertions(() =>
			{
				AssertEquals("configItem1.HasErrors: " + configItem1.GetErrors().ToMessageListString(), false, configItem1.HasErrors);
				AssertEquals("configItem2.HasErrors: " + configItem2.GetErrors().ToMessageListString(), false, configItem2.HasErrors);
			});
		}

		public void TestAvailableSections()
		{
			AssertEquals("AvailableSections.Count", 4, DocConfig.AvailableSections.Count);
			AssertEquals("AvailableSections.ReadOnly", true, DocConfig.AvailableSections.ReadOnly);
		}

		public void TestAvailableSectionsForSystemSectionRepository()
		{
			var systemSectionsTemplate = Source.MenuTemplatePivot.Template;
			systemSectionsTemplate.SO_DataContext = "Moo";
			systemSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			systemSectionsTemplate.SO_Template = CreateExcelTemplate(new string[]
			{
				"DHD, System Document Header",
				"BOD, System Body 1",
				"BOD, System Body 2",
				"PFT, System Page Footer 1",
				"PFT, System Page Footer 2",
			});

			AssertEquals("AvailableSections.Count", 5, DocConfig.AvailableSections.Count);
			AssertEquals("AvailableSections[0].TypeCode", "DHD", DocConfig.AvailableSections[0].TypeCode);
			AssertEquals("AvailableSections[0].SectionName", "System Document Header", DocConfig.AvailableSections[0].SectionName);
			AssertEquals("AvailableSections[1].TypeCode", "BOD", DocConfig.AvailableSections[1].TypeCode);
			AssertEquals("AvailableSections[1].SectionName", "System Body 1", DocConfig.AvailableSections[1].SectionName);
			AssertEquals("AvailableSections[2].TypeCode", "BOD", DocConfig.AvailableSections[2].TypeCode);
			AssertEquals("AvailableSections[2].SectionName", "System Body 2", DocConfig.AvailableSections[2].SectionName);
			AssertEquals("AvailableSections[3].TypeCode", "PFT", DocConfig.AvailableSections[3].TypeCode);
			AssertEquals("AvailableSections[3].SectionName", "System Page Footer 1", DocConfig.AvailableSections[3].SectionName);
			AssertEquals("AvailableSections[4].TypeCode", "PFT", DocConfig.AvailableSections[4].TypeCode);
			AssertEquals("AvailableSections[4].SectionName", "System Page Footer 2", DocConfig.AvailableSections[4].SectionName);

			StmTemplateBase userSectionsTemplate = Factory.New<StmTemplateBase>();
			userSectionsTemplate.SO_DataContext = "Moo";
			userSectionsTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			userSectionsTemplate.SO_Template = CreateExcelTemplate(new string[]
			{
				"DHD, User Document Header",
				"BOD, System Body 2",
				"BOD, User Body 1",
				"BOD, User Body 2",
				"PFT, User Page Footer",
				"PFT, System Page Footer 2",
			});

			TemporaryStmMenuDocumentConfig newDocConfig = (TemporaryStmMenuDocumentConfig)GetNewBusinessObject();
			var results = new List<string>();
			foreach (TemplateSection section in newDocConfig.AvailableSections)
			{
				results.Add(section.TypeCode + " - " + section.SectionName);
			}
			results.Sort();
			AssertMultilineASCIIEquals("newDocConfig.AvailableSections"
				, @"
BOD - System Body 1
BOD - System Body 2
BOD - User Body 1
BOD - User Body 2
DHD - System Document Header
DHD - User Document Header
PFT - System Page Footer 1
PFT - System Page Footer 2
PFT - User Page Footer
".Trim()
				, String.Join("\r\n", results.ToArray()));
		}

		public new void TestCallsBaseSetDefaultValues()
		{
			Assert("SetDefaultValues isn't called because we're importing from another factory.", true);
		}

		public void TestCannotSaveFactory()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), () => DocConfig.Factory.Save());
		}

		public override void TestGetRelatedDocConfigs()
		{
			var menuTemplatePivot = (StmMenuTemplatePivotBase)Source.MenuTemplatePivot;
			var relatedDocConfig1 = menuTemplatePivot.DocConfigs.AddNew();
			var relatedDocConfig2 = menuTemplatePivot.DocConfigs.AddNew();
			var nonRelatedDocConfig = Factory.New<StmMenuDocumentConfig>();
			nonRelatedDocConfig.S3_SI = Factory.New<StmMenuTemplatePivotBase>().PK;

			var relatedDocConfigs = DocConfig.GetRelatedDocConfigs();
			AssertEquals("GetRelatedDocConfigs().Length", 2, relatedDocConfigs.Length);

			var relatedDocConfig1Copy = FindRelatedDocConfig(relatedDocConfig1, relatedDocConfigs);
			var relatedDocConfig2Copy = FindRelatedDocConfig(relatedDocConfig1, relatedDocConfigs);
			AssertNotNull("GetRelatedDocConfigs() should contain relatedDocConfig1.", relatedDocConfig1Copy);
			AssertNotNull("GetRelatedDocConfigs() should contain relatedDocConfig2.", relatedDocConfig2Copy);
			AssertNotEquals("GetRelatedDocConfigs() should not contain instances of the original objects.", relatedDocConfig1, relatedDocConfig1Copy);
			AssertNotEquals("GetRelatedDocConfigs() should not contain instances of the original objects.", relatedDocConfig2, relatedDocConfig2Copy);
		}

		public void TestIsSavedByFactory()
		{
			AssertEquals("IsSavedByFactory", false, DocConfig.IsSavedByFactory);
		}

		public void TestDeleteAndReorderAddThenCommit()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(
				Factory,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, A]
{B}-[A]
{A}-[#ConfigurableSection:GEN, B]
{B}-[B]
{A}-[#ConfigurableSection:GEN, C]
{B}-[C]
{A}-[#ConfigurableSection:GEN, D]
{B}-[D]
{A}-[#ConfigurableSection:GEN, E]
{B}-[E]
{A}-[#EndOfReport]");

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("A"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("B"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("C"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("D"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("E"));

			AssertMultilineASCIIEquals("Pre-condition: GetConfigDetails(config)",
@"1: A - BDY ()
2: B - BDY ()
3: C - BDY ()
4: D - BDY ()
5: E - BDY ()",
				GetConfigDetails(config));

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			temporaryConfig.ConfigItems.DeleteAndReorder(temporaryConfig.ConfigItems.ToArray<StmMenuDocumentConfigItem>());
			temporaryConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("C"));
			AssertMultilineASCIIEquals("GetConfigDetails(temporaryConfig)",
@"1: C - BDY ()",
				GetConfigDetails(temporaryConfig));

			temporaryConfig.Commit();

			AssertMultilineASCIIEquals("GetConfigDetails(config)",
@"1: C - BDY ()",
				GetConfigDetails(config));
		}

		public void TestNewAndCommitWithDocConfig()
		{
			var guid = ZGuid.NewZGuid();
			Source.S3_GC = guid;
			Source.S3_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			Source.MenuTemplatePivot.Template.SO_Template = CreateExcelTemplate(new string[] { "BOD,x", "BOD,y" });

			var item1 = Source.ConfigItems.AddNew();
			var item2 = Source.ConfigItems.AddNew();

			item1.S4_PrintOrder = 0;
			item2.S4_PrintOrder = 1;
			item1.S4_SectionItemName = "x";
			item2.S4_SectionItemName = "y";
			item1.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
			item2.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;

			AssertNotEquals("Factory should be different.", Factory, DocConfig.Factory);

			AssertEquals("S3_GC", guid, DocConfig.S3_GC);
			AssertEquals("S3_OH", GlbCompany.CurrentCompany.OrgProxy.PK, DocConfig.S3_OH);

			AssertEquals("ConfigItems.Count", 2, DocConfig.ConfigItems.Count);
			AssertEquals("ConfigItems[0].PK", item1.PK, DocConfig.ConfigItems[0].PK);
			AssertEquals("ConfigItems[1].PK", item2.PK, DocConfig.ConfigItems[1].PK);

			AssertEquals("MenuTemplatePivot.PK", Source.MenuTemplatePivot.PK, DocConfig.MenuTemplatePivot.PK);
			AssertEquals("MenuTemplatePivot.MenuItem.PK", Source.MenuTemplatePivot.MenuItem.PK, DocConfig.MenuTemplatePivot.MenuItem.PK);
			AssertEquals("MenuTemplatePivot.Template.PK", Source.MenuTemplatePivot.Template.PK, DocConfig.MenuTemplatePivot.Template.PK);

			DocConfig.S3_GC = GlbCompany.CurrentCompany.PK;

			var item3 = DocConfig.ConfigItems.AddNew();
			item3.S4_PrintOrder = 2;
			item3.S4_SectionItemName = "z";
			item3.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
			DocConfig.ConfigItems[1].S4_SectionItemName = "a";
			DocConfig.ConfigItems.MoveDown(1);
			DocConfig.ConfigItems[0].Delete();

			AssertEquals("Commit()", Source, DocConfig.Commit());
			AssertEquals("S3_GC", GlbCompany.CurrentCompany.PK, Source.S3_GC);
			AssertEquals("ConfigItems.Count", 2, Source.ConfigItems.Count);
			AssertEquals("ConfigItems[0].S4_SectionItemName", "z", Source.ConfigItems[0].S4_SectionItemName);
			AssertEquals("ConfigItems[1].S4_SectionItemName", "a", Source.ConfigItems[1].S4_SectionItemName);
		}

		public void TestCommitHasChangesWithDocConfig()
		{
			var tempDocConfig = TemporaryStmMenuDocumentConfig.New(DocConfig);
			tempDocConfig.ConfigItems.AddNew();

			Factory.Save();
			Assert("Pre-condition: DocConfig should not have changes as it was just saved", !DocConfig.HasChanges);
			AssertEquals("Pre-condition: DocConfig should not have changes as it was just saved", 0, DocConfig.ConfigItems.Count);
			tempDocConfig.Commit();
			Assert("tempDocConfig changes should have copied over and set DocConfig HasChanges to true", DocConfig.HasChanges);
			AssertEquals("tempDocConfig changes should have copied over", 1, DocConfig.ConfigItems.Count);
		}

		public void TestNewAndCommitWithMenuTemplatePivot()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();
			StmTemplateBase template = Factory.New<StmTemplateBase>();
			StmMenuTemplatePivotBase pivot = menu.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;

			Factory.Save();

			TemporaryStmMenuDocumentConfig docConfig = TemporaryStmMenuDocumentConfig.New(pivot);
			AssertEquals("ConfigItems.Count", 0, docConfig.ConfigItems.Count);
			AssertEquals("MenuTemplatePivot.PK", pivot.PK, docConfig.MenuTemplatePivot.PK);
			AssertEquals("MenuTemplatePivot.MenuItem.PK", menu.PK, docConfig.MenuTemplatePivot.MenuItem.PK);
			AssertEquals("MenuTemplatePivot.Template.PK", template.PK, docConfig.MenuTemplatePivot.Template.PK);

			docConfig.S3_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			docConfig.ConfigItems.AddNew().S4_SectionItemName = "x";

			StmMenuDocumentConfig committedDocConfig = docConfig.Commit();
			AssertEquals("Commit()", pivot.DocConfigs[0], committedDocConfig);
			AssertEquals("pivot.DocConfigs.Count", 1, pivot.DocConfigs.Count);
			AssertEquals("S3_OH", GlbCompany.CurrentCompany.OrgProxy.PK, pivot.DocConfigs[0].S3_OH);
			AssertEquals("ConfigItems.Count", 1, pivot.DocConfigs[0].ConfigItems.Count);
			AssertEquals("ConfigItems[0].S4_SectionItemName", "x", pivot.DocConfigs[0].ConfigItems[0].S4_SectionItemName);
		}

		public void TestObsoleteConfigItemsHaveErrors()
		{
			var item1 = Source.ConfigItems.AddNew();
			var item2 = Source.ConfigItems.AddNew();
			var item3 = Source.ConfigItems.AddNew();

			item1.S4_PrintOrder = 1;
			item1.S4_SectionType = ConfigurableSectionTypeList.Codes.DocumentHeader;
			item1.S4_SectionItemName = "My Head Hurts";

			item2.S4_PrintOrder = 2;
			item2.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
			item2.S4_SectionItemName = "I Don't Exist";

			item3.S4_PrintOrder = 3;
			item3.S4_SectionType = ConfigurableSectionTypeList.Codes.PageFooter;
			item3.S4_SectionItemName = ConfigurableSectionTypeList.Descriptions.PageFooter;

			AssertEquals("ConfigItems.Count", 3, DocConfig.ConfigItems.Count);
			CombineAssertions(delegate
			{
				AssertEquals("ConfigItems[0].S4_PrintOrder", 1, DocConfig.ConfigItems[0].S4_PrintOrder);
				AssertEquals("ConfigItems[0].S4_SectionType", ConfigurableSectionTypeList.Codes.DocumentHeader, DocConfig.ConfigItems[0].S4_SectionType);
				AssertEquals("ConfigItems[0].S4_SectionItemName", "My Head Hurts", DocConfig.ConfigItems[0].S4_SectionItemName);
				Assert("ConfigItems[0] Validation error", !DocConfig.ConfigItems[0].HasRowErrors);
				AssertEquals("ConfigItems[1].S4_PrintOrder", 2, DocConfig.ConfigItems[1].S4_PrintOrder);
				AssertEquals("ConfigItems[1].S4_SectionType", ConfigurableSectionTypeList.Codes.BodySection, DocConfig.ConfigItems[1].S4_SectionType);
				AssertEquals("ConfigItems[1].S4_SectionItemName", "I Don't Exist", DocConfig.ConfigItems[1].S4_SectionItemName);
				AssertEquals("ConfigItems[1] Validation error", 1, DocConfig.ConfigItems[1].RowErrors.Count());
				AssertEquals("ConfigItems[1] Validation error", "This section is neither a system defined config item nor a customized config item and should be removed.", DocConfig.ConfigItems[1].RowErrors.First().Message);
				AssertEquals("ConfigItems[2].S4_PrintOrder", 3, DocConfig.ConfigItems[2].S4_PrintOrder);
				AssertEquals("ConfigItems[2].S4_SectionType", ConfigurableSectionTypeList.Codes.PageFooter, DocConfig.ConfigItems[2].S4_SectionType);
				AssertEquals("ConfigItems[2].S4_SectionItemName", ConfigurableSectionTypeList.Descriptions.PageFooter, DocConfig.ConfigItems[2].S4_SectionItemName);
				Assert("ConfigItems[2] Validation error", !DocConfig.ConfigItems[2].HasRowErrors);
			});
		}

		public void TestProxiedProperties()
		{
			AssertEquals("S3_Calc_DocumentTitle", "Pivot", DocConfig.S3_Calc_DocumentTitle);
			AssertEquals("S3_Calc_MenuName", "Menu", DocConfig.S3_Calc_MenuName);
		}

		public void TestReadOnlyIfIsSystemAndNotEditable()
		{
			AssertEquals("ReadOnly", false, DocConfig.ReadOnly);

			Source.S3_IsSystem = true;
			AssertEquals("ReadOnly", true, TemporaryStmMenuDocumentConfig.New(Source).ReadOnly);

			((StmMenuTemplatePivotBase)Source.MenuTemplatePivot).EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, TemporaryStmMenuDocumentConfig.New(Source).ReadOnly);
		}

		public void TestS3_IsSystem()
		{
			var menuTemplatePivot = (StmMenuTemplatePivotBase)Source.MenuTemplatePivot;

			menuTemplatePivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("S3_IsSystemInfo.ReadOnly", true, DocConfig.S3_IsSystemInfo.ReadOnly);

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("S3_IsSystemInfo.ReadOnly", false, DocConfig.S3_IsSystemInfo.ReadOnly);

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("S3_IsSystemInfo.ReadOnly", true, DocConfig.S3_IsSystemInfo.ReadOnly);

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("S3_IsSystemInfo.ReadOnly", false, DocConfig.S3_IsSystemInfo.ReadOnly);
		}

		public void TestS3_IsTemplate()
		{
			var menuTemplatePivot = (StmMenuTemplatePivotBase)Source.MenuTemplatePivot;

			menuTemplatePivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("S3_IsTemplateInfo.ReadOnly", true, DocConfig.S3_IsTemplateInfo.ReadOnly);

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("S3_IsTemplateInfo.ReadOnly", false, DocConfig.S3_IsTemplateInfo.ReadOnly);

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("S3_IsTemplateInfo.ReadOnly", true, DocConfig.S3_IsTemplateInfo.ReadOnly);

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("S3_IsTemplateInfo.ReadOnly", false, DocConfig.S3_IsTemplateInfo.ReadOnly);
		}

		public void TestConfigItemsSystemAndClientFlags()
		{
			AssertConfigItemsSystemAndClientFlags("Non System Defined - Non Client Specific", false, false);
			AssertConfigItemsSystemAndClientFlags("Non System Defined - Client Specific", false, true);
			AssertConfigItemsSystemAndClientFlags("System Defined - Non Client Specific", true, false);
			AssertConfigItemsSystemAndClientFlags("System Defined - Client Specific", true, true);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Tests are in the TestNewAndCommitWithParent and TestNewAndCommitWithSource methods.", true);
		}

		public override void TestFetchForLoad()
		{
			Assert("This test is meaningless because this business object cannot be saved to begiin with.", true);
		}

		void AssertConfigItemsSystemAndClientFlags(string name, bool isSystemDefined, bool isClientSpecific)
		{
			var menu = Factory.New<StmMenuItemBase>();
			menu.SU_MenuName = name;
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = name;
			var pivot = menu.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;

			Factory.Save();

			var docConfig = TemporaryStmMenuDocumentConfig.New(pivot);
			docConfig.S3_IsSystem = isSystemDefined;
			docConfig.S3_IsClientSpecific = isClientSpecific;
			docConfig.S3_Description = "TestDoc";
			docConfig.ConfigItems.AddNew();
			docConfig.Commit();

			AssertEquals("Configs.Count", 1, pivot.DocConfigs.Count);
			AssertEquals("ConfigItems.Count", 1, pivot.DocConfigs[0].ConfigItems.Count);
			AssertEquals("ConfigItems[0].S4_IsSystemDefined", isSystemDefined, pivot.DocConfigs[0].ConfigItems[0].S4_IsSystemDefined);
			AssertEquals("ConfigItems[0].S4_IsClientSpecific", isClientSpecific, pivot.DocConfigs[0].ConfigItems[0].S4_IsClientSpecific);
		}

		string GetConfigDetails(StmMenuDocumentConfig config)
		{
			var result = new StringBuilder();

			foreach (StmMenuDocumentConfigItem configItem in config.ConfigItems)
			{
				result.AppendLine(GetConfigItemDetails(configItem));
			}

			return result.ToString().Trim();
		}

		string GetConfigItemDetails(StmMenuDocumentConfigItem configItem)
		{
			return string.Format(
				"{0}: {1} - {2} ({3})",
				configItem.S4_PrintOrder,
				configItem.S4_SectionItemName,
				configItem.S4_SectionType,
				configItem.S4_FilterList);
		}

		StmMenuDocumentConfig source;
		StmMenuDocumentConfig Source => source ?? (source = TemporaryStmMenuDocumentConfigTestHelper.GetNewSource(Factory));

		byte[] CreateExcelTemplate(string[] sections)
		{
			using (ExcelInterface excel = new ExcelInterface())
			{
				excel.NewExcelFile(1);
				ExcelWorkSheet workSheet = excel.WorkSheets[0];
				workSheet[0, 0] = "#Config";
				for (int i = 0; i < sections.Length; i++)
				{
					workSheet[i + 1, 0] = "#ConfigurableSection:" + sections[i];
				}
				workSheet[sections.Length + 1, 0] = "#EndOfReport";
				using (MemoryStream stream = new MemoryStream())
				{
					excel.SaveToStream(stream);
					return stream.ToArray();
				}
			}
		}

		StmMenuDocumentConfig FindRelatedDocConfig(StmMenuDocumentConfig relatedDocConfig, StmMenuDocumentConfig[] relatedDocConfigs)
		{
			return Array.Find(relatedDocConfigs, (StmMenuDocumentConfig docConfigInArray) =>
			{
				return docConfigInArray.PK == relatedDocConfig.PK;
			});
		}

		protected override BusinessObject GetNewBusinessObject() => TemporaryStmMenuDocumentConfig.New(Source);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() => GetNewBusinessObject();
	}
}
