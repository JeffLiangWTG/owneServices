using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing.Testing
{
	[TestedType(typeof(CustomizeSectionManager))]
	sealed class CustomizeSectionManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCategoryFilter()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN:Category 2, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);
			AssertEquals("manager.CategoryFilter", "", manager.CategoryFilter);

			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (EN)
My Section 2 (EN)", GetSectionsString(manager.Sections));

			manager.CategoryFilter = "Category 1";
			AssertEquals("manager.CategoryFilter", "Category 1", manager.CategoryFilter);
			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (EN)", GetSectionsString(manager.Sections));

			manager.CategoryFilter = "Category 2";
			AssertEquals("manager.CategoryFilter", "Category 2", manager.CategoryFilter);
			AssertMultilineASCIIEquals("Sections",
@"My Section 2 (EN)", GetSectionsString(manager.Sections));
		}

		public void TestCategories()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN:Category 2, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);
			AssertEquals("manager.Categories.Count", 3, manager.Categories.Count);

			CombineAssertions(delegate()
			{
				AssertEquals("manager.Categories[0].Code", "", manager.Categories[0].Code);
				AssertEquals("manager.Categories[1].Code", "Category 1", manager.Categories[1].Code);
				AssertEquals("manager.Categories[2].Code", "Category 2", manager.Categories[2].Code);
			});
		}

		public void TestLanguage()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			DocumentEngineTestHelper.CreateTemplateFromString(Factory, "System Document Elements [FR-FR]",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);
			AssertEquals("manager.Language", Enterprise.Core.SharedConstants.Languages.EnglishAmerican, manager.Language);
			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (EN)
My Section 2 (EN)", GetSectionsString(manager.Sections));

			manager.Language = Enterprise.Core.SharedConstants.Languages.French;
			AssertEquals("manager.Language", Enterprise.Core.SharedConstants.Languages.French, manager.Language);

			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (EN)
My Section 2 (FR-FR)", GetSectionsString(manager.Sections));
		}

		public void TestSections()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);

			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (EN)
My Section 2 (EN)", GetSectionsString(manager.Sections));
		}

		public void TestSectionsForOtherLanguage()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			DocumentEngineTestHelper.CreateTemplateFromString(Factory, "System Document Elements [FR-FR]",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.French);

			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (EN)
My Section 2 (FR-FR)", GetSectionsString(manager.Sections));
		}

		public void TestSectionTemplates()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			DocumentEngineTestHelper.CreateTemplateFromString(Factory, "System Document Elements [FR-FR]",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.French);

			AssertMultilineASCIIEquals("Sections",
@"My Section 1 (System Document Elements)
My Section 2 (System Document Elements [FR-FR])", GetSectionTemplatesString(manager.SectionTemplates));
		}

		public void TestLanguageList()
		{
			var expectedList = new AvailableDocBuilderLanguageList(Factory);
			foreach (CodeDescriptionPair item in expectedList.ToArray())
			{
				if (Res.IsEnglish(item.Code))
				{
					expectedList.Remove(item);
				}
			}
			expectedList.Add(new CodeDescriptionPairList(OLookUpEditType.Language)[Res.DefaultLanguage]);
			AssertContainsExactElementsInAnyOrder(
				expectedList,
				new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.EnglishAmerican).LanguageList);
		}

		#region Implementation
		string GetSectionTemplatesString(Dictionary<string, StmTemplateBase> sectionTemplates)
		{
			var result = new StringBuilder();

			foreach (var templateSection in sectionTemplates)
			{
				result.AppendLine(string.Format("{0} ({1})", templateSection.Key, templateSection.Value.SO_Name));
			}

			return result.ToString().Trim();
		}

		string GetSectionsString(TemplateSectionCollectionView sections)
		{
			var result = new StringBuilder();

			foreach (TemplateSection section in sections)
			{
				result.AppendLine(string.Format("{0} ({1})", section.SectionName, section.Language));
			}

			return result.ToString().Trim();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);
		}

		#endregion
	}
}
