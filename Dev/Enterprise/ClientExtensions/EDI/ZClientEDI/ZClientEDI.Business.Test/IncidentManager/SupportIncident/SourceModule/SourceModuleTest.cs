using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SourceModule))]
	sealed class SourceModuleTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var source = new SourceModule();
			CombineAssertions("Default Values", () =>
			{
				AssertEquals("IsSelectableForOverride", true, source.IsSelectableForOverride);
				AssertEquals("IsSearchable", true, source.IsSearchable);
			});
		}

		public void TestGetPath()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Maintain > Account >", SourceModule.GetPath(ModuleTree.Tree.FindByID(ModuleIDs.AccountReports.ToString())));
				AssertEquals("Maintain > Master Data >", SourceModule.GetPath(ModuleTree.Tree.FindByID(ModuleIDs.Organisation.ToString())));
			});
		}

		public void TestIsModuleTypeCompatible()
		{
			AssertEquals(true, SourceModule.IsModuleTypeCompatible(ModuleListType.MenuSection, ModuleListType.MenuSection));
			AssertEquals(true, SourceModule.IsModuleTypeCompatible(ModuleListType.MenuSection, ModuleListType.DetectedMenuItem));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.MenuSection, ModuleListType.Cr8));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.MenuSection, ModuleListType.Cr9));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.MenuSection, ModuleListType.Unspecified));

			AssertEquals(true, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr8, ModuleListType.Cr8));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr8, ModuleListType.Cr9));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr8, ModuleListType.MenuSection));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr8, ModuleListType.DetectedMenuItem));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr8, ModuleListType.Unspecified));

			AssertEquals(true, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr9, ModuleListType.Cr9));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr9, ModuleListType.Cr8));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr9, ModuleListType.MenuSection));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr9, ModuleListType.DetectedMenuItem));
			AssertEquals(false, SourceModule.IsModuleTypeCompatible(ModuleListType.Cr9, ModuleListType.Unspecified));
		}

		public void TestModuleListTypeDescriptions()
		{
			var sourceModule1 = new SourceModule();
			AssertContainsExactElementsInAnyOrder("Should contain all ModuleListTypes except Unspecified and Detected Menu Item",
				new[]
				{
					"Menu Section",
					"Requirement",
					"Service"
				},
				sourceModule1.ModuleListTypeDescriptions.Cast<ICodeDescription>().Select(pair => pair.Code));

			var sourceModule2 = new SourceModule();
			sourceModule2.ModuleListType = ModuleListType.DetectedMenuItem;
			AssertContainsExactElementsInAnyOrder("Should contain all ModuleListTypes except Unspecified and Detected Menu Item",
				new[]
				{
					"Detected Menu Item"
				},
				sourceModule2.ModuleListTypeDescriptions.Cast<ICodeDescription>().Select(pair => pair.Code));
		}

		public void TestModulesList()
		{
			var sourceModule = new SourceModule();
			AssertNotNull(sourceModule.ModulesList);
		}

		public void TestModulesListShouldNotIncludeDisabledRecordsForMenuSection()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.MenuSection, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true, true); // isInternal
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA2", "", true, false, true); // external, enabled
			var moduleAA3 = productAAAModules.ModuleMappings.AddNew("AA3", (NoResString)"AA3", "", true, false, false); // external, disabled

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			AssertContainsExactElementsInAnyOrder(new[] { moduleAA1.ModuleCode, moduleAA2.ModuleCode },
				sourceModule.ModulesList.Cast<ICodeDescription>().Select(p => p.Code));
		}

		public void TestModulesListShouldNotIncludeDisabledRecordsForCr8()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.Cr8, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true, true); // isInternal
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA2", "", true, false, true); // external, enabled
			var moduleAA3 = productAAAModules.ModuleMappings.AddNew("AA3", (NoResString)"AA3", "", true, false, false); // external, disabled

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			AssertContainsExactElementsInAnyOrder(new[] { moduleAA1.ModuleCode, moduleAA2.ModuleCode },
				sourceModule.ModulesList.Cast<ICodeDescription>().Select(p => p.Code));
		}

		public void TestModulesListShouldNotIncludeDisabledRecordsForCr9()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.Cr9, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true, true); // isInternal
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA2", "", true, false, true); // external, enabled
			var moduleAA3 = productAAAModules.ModuleMappings.AddNew("AA3", (NoResString)"AA3", "", true, false, false); // external, disabled

			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			AssertContainsExactElementsInAnyOrder(new[] { moduleAA1.ModuleCode, moduleAA2.ModuleCode },
				sourceModule.ModulesList.Cast<ICodeDescription>().Select(p => p.Code));
		}

		public void TestModulesListForDetectedMenuItemShouldMatchMenuSectionModulesList()
		{
			var sourceModule1 = new SourceModule { ModuleListType = ModuleListType.MenuSection, Product = "AAA" };
			var sourceModule2 = new SourceModule { ModuleListType = ModuleListType.DetectedMenuItem, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true, true); // isInternal
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA2", "", true, false, true); // external, enabled
			var moduleAA3 = productAAAModules.ModuleMappings.AddNew("AA3", (NoResString)"AA3", "", true, false, false); // external, disabled

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			AssertContainsExactElementsInAnyOrder(sourceModule1.ModulesList.Cast<ICodeDescription>().Select(p => p.Code), sourceModule2.ModulesList.Cast<ICodeDescription>().Select(p => p.Code));
		}

		public void TestProductList()
		{
			var sourceModule = new SourceModule();
			AssertNotNull(sourceModule.ProductList);
		}

		public void TestReadOnlyWhenTypeIsDetectedMenuItem()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.DetectedMenuItem };
			Assert(sourceModule.Code_ReadOnly);
			Assert(sourceModule.Description_ReadOnly);
			Assert(sourceModule.ModuleListTypeDescription_ReadOnly);
			Assert(sourceModule.Product_ReadOnly);
			Assert(sourceModule.Path_ReadOnly);
		}
	}

	sealed class SourceModuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProduct()
		{
			var sourceModule = new SourceModule();

			sourceModule.Product = "XXX";
			AssertHasErrors(sourceModule.ProductInfo);

			sourceModule.Product = "ENT";
			AssertNoErrors(sourceModule.ProductInfo);
		}

		public void TestValidateCode()
		{
			var sourceModule = new SourceModule();

			sourceModule.Code = "";
			AssertMandatoryValidationError(sourceModule.CodeInfo, true);

			sourceModule.Code = "DummyMenuItem";
			AssertMandatoryValidationError(sourceModule.CodeInfo, false);
		}

		public void TestValidateDescription()
		{
			var sourceModule = new SourceModule();

			sourceModule.Description = "";
			AssertMandatoryValidationError(sourceModule.DescriptionInfo, true);

			sourceModule.Description = "Dummy Menu Item";
			AssertMandatoryValidationError(sourceModule.DescriptionInfo, false);
		}

		public void TestValidateModuleTypeCode()
		{
			var sourceModule = new SourceModule();

			sourceModule.ModuleTypeCode = "";
			AssertMandatoryValidationError(sourceModule.ModuleTypeCodeInfo, true);

			sourceModule.ModuleTypeCode = nameof(ModuleListType.MenuSection);
			AssertMandatoryValidationError(sourceModule.ModuleTypeCodeInfo, false);
		}

		public void TestValidateDefaultMenuSectionForMenuSection()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.MenuSection, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true); 
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA1", "", true, false);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			sourceModule.DefaultMenuSection = moduleAA1.ModuleCode;
			AssertHasWarning(sourceModule.DefaultMenuSectionInfo, "Section code is internal only");

			sourceModule.DefaultMenuSection = moduleAA2.ModuleCode;
			AssertNoWarnings(sourceModule);
		}

		public void TestValidateDefaultMenuSectionForCr8()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.Cr8, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true);
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA1", "", true, false);

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			sourceModule.DefaultMenuSection = moduleAA1.ModuleCode;
			AssertHasWarning(sourceModule.DefaultMenuSectionInfo, "Section code is internal only");

			sourceModule.DefaultMenuSection = moduleAA2.ModuleCode;
			AssertNoWarnings(sourceModule);
		}

		public void TestValidateDefaultMenuSectionForCr9()
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.Cr9, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true);
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA1", "", true, false);

			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			sourceModule.DefaultMenuSection = moduleAA1.ModuleCode;
			AssertHasWarning(sourceModule.DefaultMenuSectionInfo, "Section code is internal only");

			sourceModule.DefaultMenuSection = moduleAA2.ModuleCode;
			AssertNoWarnings(sourceModule);
		}

		public void TestValidateDefaultMenuSectionForDetectedMenuItem() // should be the same behaviour as Menu Section
		{
			var sourceModule = new SourceModule { ModuleListType = ModuleListType.DetectedMenuItem, Product = "AAA" };

			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true);
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA1", "", true, false);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			sourceModule.DefaultMenuSection = moduleAA1.ModuleCode;
			AssertHasWarning(sourceModule.DefaultMenuSectionInfo, "Section code is internal only");

			sourceModule.DefaultMenuSection = moduleAA2.ModuleCode;
			AssertNoWarnings(sourceModule);
		}
	}
}
