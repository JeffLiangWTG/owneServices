using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaModuleMappingCollection))]
	internal sealed class ProductAreaModuleMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ProductAreaModuleMappingCollection>
	{
		public void TestAddIfNotExists()
		{
			var collection = new ProductAreaModuleMappingCollection();
			collection.AddNew("XXX", "D1", "P1", false);
			collection.AddIfNotExists("XXX", "D2", "P2", true);

			AssertEquals(1, collection.Count);
			AssertEquals("D1", collection[0].ModuleDescription);
			AssertEquals("P1", collection[0].ProductArea);
			AssertEquals(false, collection[0].IsModuleReadOnly);
		}

		public void TestAddNewWithValue()
		{
			ProductAreaModuleMappingCollection collection = new ProductAreaModuleMappingCollection();

			ProductAreaModuleMapping mapping1 = collection.AddNew("AAA", "Module AAA", "ARC", false);
			AssertEquals("AAA", mapping1.ModuleCode);
			AssertEquals("Module AAA", mapping1.ModuleDescription);
			AssertEquals("ARC", mapping1.ProductArea);
			AssertEquals(false, mapping1.IsModuleReadOnly);

			ProductAreaModuleMapping mapping2 = collection.AddNew("BBB", "Module BBB", "", false);
			AssertEquals("BBB", mapping2.ModuleCode);
			AssertEquals("Module BBB", mapping2.ModuleDescription);
			AssertEquals("", mapping2.ProductArea);
			AssertEquals(false, mapping2.IsModuleReadOnly);
		}

		public void TestGetModuleListDefaultSettings()
		{
			ProductAreaModuleMappingCollection collection1 = new ProductAreaModuleMappingCollection();
			ProductAreaModuleMapping mapping1 = collection1.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			ProductAreaModuleMapping mapping2 = collection1.AddNew("BBB", "Module BBB Enabled", "ARC", false);
			ProductAreaModuleMapping mapping3 = collection1.AddNew("CCC", "Module CCC Enabled", "", false);

			CodeDescriptionPairList moduleList = collection1.GetFullModuleList("");
			AssertEquals(3, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
			AssertEquals(true, moduleList.ContainsCode("CCC"));

			moduleList = collection1.GetFullModuleList("ARC");
			AssertEquals(2, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));

			ProductAreaModuleMappingCollection collection2 = new ProductAreaModuleMappingCollection();
			mapping1 = collection2.AddNew("AAA", "Module AAA Disabled", "ARC", true, false, false);
			mapping2 = collection2.AddNew("BBB", "Module BBB Disabled", "ARC", false, false, false);
			mapping3 = collection2.AddNew("CCC", "Module CCC Disabled", "", false, false, false);

			moduleList = collection2.GetFullModuleList("");
			AssertEquals(3, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
			AssertEquals(true, moduleList.ContainsCode("CCC"));

			moduleList = collection2.GetFullModuleList("ARC");
			AssertEquals(2, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));

			ProductAreaModuleMappingCollection collection3 = new ProductAreaModuleMappingCollection();
			mapping1 = collection3.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			mapping2 = collection3.AddNew("BBB", "Module BBB Disabled", "ARC", false, false, false);
			mapping3 = collection3.AddNew("CCC", "Module CCC Enabled", "", false);

			moduleList = collection3.GetFullModuleList("");
			AssertEquals(3, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
			AssertEquals(true, moduleList.ContainsCode("CCC"));

			moduleList = collection3.GetFullModuleList("ARC");
			AssertEquals(2, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
		}

		public void TestGetModuleListExcludeDisabledArg()
		{
			ProductAreaModuleMappingCollection collection1 = new ProductAreaModuleMappingCollection();
			ProductAreaModuleMapping mapping1 = collection1.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			ProductAreaModuleMapping mapping2 = collection1.AddNew("BBB", "Module BBB Enabled", "ARC", false);
			ProductAreaModuleMapping mapping3 = collection1.AddNew("CCC", "Module CCC Enabled", "", false);

			CodeDescriptionPairList moduleListIncludeAllEnabledDisabled = collection1.GetFullModuleList("");
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			CodeDescriptionPairList moduleListExcludeDisabled = collection1.GetFullModuleList("", false, true);
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			ProductAreaModuleMappingCollection collection2 = new ProductAreaModuleMappingCollection();
			mapping1 = collection2.AddNew("AAA", "Module AAA Disabled", "ARC", true, false, false);
			mapping2 = collection2.AddNew("BBB", "Module BBB Disabled", "ARC", false, false, false);
			mapping3 = collection2.AddNew("CCC", "Module CCC Disabled", "", false, false, false);

			moduleListIncludeAllEnabledDisabled = collection2.GetFullModuleList("");
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			moduleListExcludeDisabled = collection2.GetFullModuleList("", false, true);
			AssertEquals(0, moduleListExcludeDisabled.Count);

			ProductAreaModuleMappingCollection collection3 = new ProductAreaModuleMappingCollection();
			mapping1 = collection3.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			mapping2 = collection3.AddNew("BBB", "Module BBB Disabled", "ARC", false, false, false);
			mapping3 = collection3.AddNew("CCC", "Module CCC Enabled", "", false);

			moduleListIncludeAllEnabledDisabled = collection3.GetFullModuleList("");
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			moduleListExcludeDisabled = collection3.GetFullModuleList("", false, true);
			AssertEquals(2, moduleListExcludeDisabled.Count);
			AssertEquals(true, moduleListExcludeDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListExcludeDisabled.ContainsCode("CCC"));

			moduleListExcludeDisabled = collection3.GetFullModuleList("ARC", false, true);
			AssertEquals(1, moduleListExcludeDisabled.Count);
			AssertEquals(true, moduleListExcludeDisabled.ContainsCode("AAA"));
		}

		public void TestGetModuleListExcludeSourceModuleMappingNotOverrideable()
		{
			#region Test Data

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("Menu Item A", "Menu Item A", "", ModuleListType.MenuSection, "MS1", true, true, "ENT");
			sourceModules.AddNew("Menu Item B", "Menu Item B", "", ModuleListType.MenuSection, "MS1", false, true, "ENT");

			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			productAreas.AddPair("PA3", "Product Area 3");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			ProductAreaModuleMappingCollection collection = new ProductAreaModuleMappingCollection();

			ProductAreaModuleMapping mapping1 = collection.AddNew("AAA", "Module AAA", "PA1", false);
			mapping1.SourceModuleMappings.AddNew("Menu Item A", "PA2");
			mapping1.SourceModuleMappings.AddNew("Menu Item B", "PA3");

			collection.AddNew("BBB", "Module BBB", "PA2", false);
			collection.AddNew("CCC", "Module BBB", "PA3", false);

			#endregion

			var pa1Modules = collection.GetModuleList(Factory, ModuleListType.MenuSection, "ENT", "PA1");
			var pa2Modules = collection.GetModuleList(Factory, ModuleListType.MenuSection, "ENT", "PA2");
			var pa3Modules = collection.GetModuleList(Factory, ModuleListType.MenuSection, "ENT", "PA3");

			AssertEquals("Only module mapping", 1, pa1Modules.Count);
			AssertEquals(true, pa1Modules.ContainsCode("AAA"));

			AssertEquals("Module mapping and source module mapping", 2, pa2Modules.Count);
			AssertEquals(true, pa2Modules.ContainsCode("AAA"));
			AssertEquals(true, pa2Modules.ContainsCode("BBB"));

			AssertEquals("Only module mapping as source module is not overrideable", 1, pa3Modules.Count);
			AssertEquals(true, pa3Modules.ContainsCode("CCC"));
		}

		public void TestGetClone()
		{
			ProductAreaModuleMappingCollection collection = new ProductAreaModuleMappingCollection();
			collection.AddNew("AAA", "Module AAA", "ARC", true);
			collection.AddNew("BBB", "Module BBB", "ARC", false);

			ProductAreaModuleMappingCollection clone = (ProductAreaModuleMappingCollection)collection.Clone(collection.CurrentFallbackLevel, collection.Factory);
			AssertEquals(2, clone.Count);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ProductAreaModuleMappingCollection GetCollectionToTest()
		{
			return new ProductAreaModuleMappingCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProductAreaModuleMapping(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
