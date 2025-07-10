using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Core.Modules.Testing
{
	sealed class NavigationTest : TestCase
	{
		public void TestSecurityChekPointDiapalyTextShouldBeSameAsModule()
		{
			AssertNotEquals("Shoud not be same", "DisplayText", EnvProxy.Instance.Security.None.DisplayText);
			ModuleNodeBase moduleNodeBase = new ModuleNodeBase("ModuleNodeBase", (NoResString)"DisplayText", EnvProxy.Instance.Security.None);
			AssertEquals("Shoud be same", true, "DisplayText" == EnvProxy.Instance.Security.None.DisplayText);
		}

		public void TestModuleSectionAddOn()
		{
			ModuleSectionAddOn addOn = new ModuleSectionAddOn("CategoryName", null, (NoResString)"DisplayText", null, EnvProxy.Instance.Security.None, IconTypes.None, IconTypes.None, ModuleTreeLoaderConstant.Subcategory.Account);
			AssertNotNull("ModuleSectionAddOn", addOn);
			AssertEquals("CategoryName", addOn.CategoryName);
			AssertEquals("Subcategory", ModuleTreeLoaderConstant.Subcategory.Account.Name, addOn.Subcategory.Name);
		}

		public void TestModuleTreeCategories()
		{
			ModuleTree testTree = new ModuleTree();
			ModuleCategory category1 = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			ModuleCategory category2 = new ModuleCategory("Category2", (NoResString)"Category 2", EnvProxy.Instance.Security.None);

			testTree.Categories.Add(category1);
			testTree.Categories.Add(category2);

			foreach (ModuleCategory category in testTree.Categories.Values)
			{
				Assert(category == category1 || category == category2);
			}

			AssertEquals("Category1", category1, testTree.Categories["Category1"]);
			AssertEquals("Category2", category2, testTree.Categories["Category2"]);
		}

		public void TestModuleCategorySections()
		{
			ModuleCategory category1 = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			ModuleSection section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			ModuleSection section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.GeneralLedger);
			category1.Sections.Add(section1);
			category1.Sections.Add(section2);

			foreach (ModuleSection section in category1.Sections.Values)
			{
				Assert(section == section1 || section == section2);
			}

			AssertEquals("Section1", section1, category1.Sections["Section1"]);
			AssertEquals("Section2", section2, category1.Sections["Section2"]);
			AssertEquals("Subcategory", ModuleTreeLoaderConstant.Subcategory.Forwarding.Name, category1.Sections["Section1"].Subcategory.Name);
			AssertEquals("Subcategory", ModuleTreeLoaderConstant.Subcategory.GeneralLedger.Name, category1.Sections["Section2"].Subcategory.Name);
		}

		[ExpectException(typeof(ModuleAlreadyExistsException))]
		public void TestAddDuplicateModuleCategory_ThrowException()
		{
			ModuleTree testTree = new ModuleTree();
			ModuleCategory category1 = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			ModuleCategory category2 = new ModuleCategory("Category1", (NoResString)"Category 2", EnvProxy.Instance.Security.None);

			testTree.Categories.Add(category1);
			testTree.Categories.Add(category2);
		}

		public void TestModuleCategoryValuesIncludingHiddenInRegistrationOrder()
		{
			var tree = new ModuleTree();
			var category1 = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var category2 = new ModuleCategory("Category2", (NoResString)"Category 2", EnvProxy.Instance.Security.None);
			var category3 = new ModuleCategory("Category3", (NoResString)"Category 3", EnvProxy.Instance.Security.None);
			var category4 = new ModuleCategory("Category4", (NoResString)"Category 4", EnvProxy.Instance.Security.None);

			tree.Categories.Add(category1);
			tree.Categories.AddIf(true, () => category2);
			tree.Categories.AddIf(false, () => category3);
			tree.Categories.Add(category4);

			AssertSequencesEqual(new[] { "Category1", "Category2", "Category4" }, tree.Categories.Values.Cast<ModuleCategory>().Select(s => s.Name));
			AssertSequencesEqual(new[] { "Category1", "Category2", "Category4", "Category3" }, tree.Categories.ValuesIncludingHidden.Select(s => s.Name));
			AssertSequencesEqual(new[] { "Category1", "Category2", "Category3", "Category4" }, tree.Categories.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
		}

		public void TestModuleSectionValuesIncludingHiddenInRegistrationOrder()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.AddIf(true, () => section2);
			category.Sections.AddIf(false, () => section3);
			category.Sections.Add(section4);

			AssertSequencesEqual(new[] { "Section1", "Section2", "Section4" }, category.Sections.Values.Cast<ModuleSection>().Select(s => s.Name));
			AssertSequencesEqual(new[] { "Section1", "Section2", "Section4", "Section3" }, category.Sections.ValuesIncludingHidden.Select(s => s.Name));
			AssertSequencesEqual(new[] { "Section1", "Section2", "Section3", "Section4" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
		}

		public void TestMainformModuleValuesIncludingHiddenInRegistrationOrder()
		{
			var section = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var module1 = new Mock<IMainFormModule>();
			var module2 = new Mock<IMainFormModule>();
			var module3 = new Mock<IMainFormModule>();
			var module4 = new Mock<IMainFormModule>();
			module1.Setup(m => m.ID).Returns("Module1");
			module2.Setup(m => m.ID).Returns("Module2");
			module3.Setup(m => m.ID).Returns("Module3");
			module4.Setup(m => m.ID).Returns("Module4");
			module1.SetupProperty(m => m.ParentSection);
			module2.SetupProperty(m => m.ParentSection);
			module3.SetupProperty(m => m.ParentSection);
			module4.SetupProperty(m => m.ParentSection);

			section.Modules.Add(module1.Object);
			section.Modules.AddIf(true, () => module2.Object);
			section.Modules.AddIf(false, () => module3.Object);
			section.Modules.Add(module4.Object);

			AssertSequencesEqual(new[] { "Module1", "Module2", "Module4" }, section.Modules.Values.Cast<IMainFormModule>().Select(s => s.ID));
			AssertSequencesEqual(new[] { "Module1", "Module2", "Module4", "Module3" }, section.Modules.ValuesIncludingHidden.Select(s => s.ID));
			AssertSequencesEqual(new[] { "Module1", "Module2", "Module3", "Module4" }, section.Modules.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.ID));

			foreach (var module in section.Modules.ValuesIncludingHiddenInRegistrationOrder)
			{
				AssertEquals(section, module.ParentSection);
			}
		}

		public void TestInsertBefore()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.Add(section2);
			category.Sections.InsertBefore("Section2", section3);
			category.Sections.InsertBefore("Section1", section4);

			AssertSequencesEqual(new[] { "Section4", "Section1", "Section3", "Section2" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
			foreach (var section in category.Sections.ValuesIncludingHiddenInRegistrationOrder)
			{
				AssertEquals(category, section.ParentCategory);
			}
		}

		public void TestInsertAfter()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.Add(section2);
			category.Sections.InsertAfter("Section2", section3);
			category.Sections.InsertAfter("Section1", section4);

			AssertSequencesEqual(new[] { "Section1", "Section4", "Section2", "Section3" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
			foreach (var section in category.Sections.ValuesIncludingHiddenInRegistrationOrder)
			{
				AssertEquals(category, section.ParentCategory);
			}
		}

		public void TestInsertBeforeIf()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.Add(section2);
			category.Sections.InsertBeforeIf("Section2", true, () => section3);
			category.Sections.InsertBeforeIf("Section1", false, () => section4);

			AssertSequencesEqual(new[] { "Section1", "Section3", "Section2" }, category.Sections.Values.Select(s => s.Name));
			AssertSequencesEqual(new[] { "Section4", "Section1", "Section3", "Section2" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
			AssertSequencesEqual(new[] { null, category, category, category }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.ParentCategory));
		}

		public void TestInsertAfterIf()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.Add(section2);
			category.Sections.InsertAfterIf("Section2", true, () => section3);
			category.Sections.InsertAfterIf("Section1", false, () => section4);

			AssertSequencesEqual(new[] { "Section1", "Section2", "Section3" }, category.Sections.Values.Select(s => s.Name));
			AssertSequencesEqual(new[] { "Section1", "Section4", "Section2", "Section3" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
			AssertSequencesEqual(new[] { category, null, category, category }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.ParentCategory));
		}

		public void TestInsertBeforeHiddenItem()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.AddIf(false, () => section2);
			category.Sections.InsertBefore("Section2", section3);
			category.Sections.InsertBefore("Section1", section4);

			AssertSequencesEqual(new[] { "Section4", "Section1", "Section3" }, category.Sections.Values.Cast<ModuleSection>().Select(s => s.Name));
			AssertSequencesEqual(new[] { "Section4", "Section1", "Section3", "Section2" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
		}

		public void TestInsertAfterHiddenItem()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section3 = new ModuleSection("Section3", (NoResString)"Section 3", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section4 = new ModuleSection("Section4", (NoResString)"Section 4", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			category.Sections.AddIf(false, () => section2);
			category.Sections.InsertAfter("Section2", section3);
			category.Sections.InsertAfter("Section1", section4);

			AssertSequencesEqual(new[] { "Section1", "Section4", "Section3" }, category.Sections.Values.Cast<ModuleSection>().Select(s => s.Name));
			AssertSequencesEqual(new[] { "Section1", "Section4", "Section2", "Section3" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
		}

		public void TestInsertBeforeNotFoundKey()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			AssertExceptionThrown<KeyNotFoundException>(() => category.Sections.InsertBefore("Not found", section2));
			AssertSequencesEqual(new[] { "Section1" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
		}

		public void TestInsertAfterNotFoundKey()
		{
			var category = new ModuleCategory("Category1", (NoResString)"Category 1", EnvProxy.Instance.Security.None);
			var section1 = new ModuleSection("Section1", (NoResString)"Section 1", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			var section2 = new ModuleSection("Section2", (NoResString)"Section 2", null, EnvProxy.Instance.Security.None, IconTypes.Book, IconTypes.Book20x16, ModuleTreeLoaderConstant.Subcategory.Forwarding);

			category.Sections.Add(section1);
			AssertExceptionThrown<KeyNotFoundException>(() => category.Sections.InsertAfter("Not found", section2));
			AssertSequencesEqual(new[] { "Section1" }, category.Sections.ValuesIncludingHiddenInRegistrationOrder.Select(s => s.Name));
		}
	}
}
