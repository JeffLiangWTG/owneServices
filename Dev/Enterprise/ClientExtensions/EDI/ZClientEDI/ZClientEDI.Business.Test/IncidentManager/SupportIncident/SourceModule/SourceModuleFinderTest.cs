using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SourceModuleFinder))]
	sealed class SourceModuleFinderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSourceModules()
		{
			#region Test Data

			var sourceModules = new SourceModuleCollection();
			var wwwMenuSectionSourceModule = sourceModules.AddNew("WWW", "WWW", "Parent > Section1 >", ModuleListType.MenuSection, "MS1", isSelectableForOverride: true, isSearchable: true);
			var xxxMenuSectionSourceModule = sourceModules.AddNew("XXX", "XXX", "Parent > Section1 >", ModuleListType.MenuSection, "MS1", isSelectableForOverride: true, isSearchable: true);
			var yyyMenuSectionSourceModule = sourceModules.AddNew("YYY", "YYY", "[Licence]", ModuleListType.MenuSection, "", isSelectableForOverride: false, isSearchable: true);
			var zzzMenuSectionSourceModule = sourceModules.AddNew("ZZZ", "ZZZ", "Parent > Section2 >", ModuleListType.MenuSection, "MS2", isSelectableForOverride: true, isSearchable: false);
			var reportMenuSectionSourceModule = sourceModules.AddNew("Report", "Test Report", "Parent > Report >", ModuleListType.DetectedMenuItem, "MS2", isSelectableForOverride: true, isSearchable: true);

			var cr8MenuItemASourceModule = sourceModules.AddNew("CR8 Item A", "CR8 Item A", "", ModuleListType.Cr8, "RQ1", isSelectableForOverride: true, isSearchable: true);
			var cr8MenuItemBSourceModule = sourceModules.AddNew("CR8 Item B", "CR8 Item B", "", ModuleListType.Cr8, "RQ1", isSelectableForOverride: true, isSearchable: true);
			var cr8MenuItemCSourceModule = sourceModules.AddNew("CR8 Item C", "CR8 Item C", "", ModuleListType.Cr8, "", isSelectableForOverride: false, isSearchable: true);
			var cr8MenuItemDSourceModule = sourceModules.AddNew("CR8 Item D", "CR8 Item D", "", ModuleListType.Cr8, "RQ2", isSelectableForOverride: true, isSearchable: false);

			var cr9MenuItemASourceModule = sourceModules.AddNew("CR9 Item A", "CR9 Item A", "", ModuleListType.Cr9, "SV1", isSelectableForOverride: true, isSearchable: true);
			var cr9MenuItemBSourceModule = sourceModules.AddNew("CR9 Item B", "CR9 Item B", "", ModuleListType.Cr9, "SV1", isSelectableForOverride: true, isSearchable: true);
			var cr9MenuItemCSourceModule = sourceModules.AddNew("CR9 Item C", "CR9 Item C", "", ModuleListType.Cr9, "", isSelectableForOverride: false, isSearchable: true);
			var cr9MenuItemDSourceModule = sourceModules.AddNew("CR9 Item D", "CR9 Item D", "", ModuleListType.Cr9, "SV2", isSelectableForOverride: true, isSearchable: false);
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			productAreas.AddPair("PA3", "Product Area 3");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", isProductReadOnly: true);
			var ms1AreaMapping = product.ModuleMappings.AddNew("MS1", "Section 1", "PA1", isModuleReadOnly: true);
			ms1AreaMapping.SourceModuleMappings.AddNew("XXX", "PA2");
			var ms2AreaMapping = product.ModuleMappings.AddNew("MS2", "Section 2", "PA2", isModuleReadOnly: true);
			var ms3AreaMapping = product.ModuleMappings.AddNew("MS3", "Section 3", "PA3", isModuleReadOnly: true);
			ms3AreaMapping.SourceModuleMappings.AddNew("WWW", "PA2");
			ms3AreaMapping.SourceModuleMappings.AddNew("Report", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", isProductReadOnly: true);
			var req1AreaMapping = product.ModuleMappings.AddNew("RQ1", "Requirement 1", "PA1", isModuleReadOnly: true);
			req1AreaMapping.SourceModuleMappings.AddNew("CR8 Item A", "PA2");
			var req2AreaMapping = product.ModuleMappings.AddNew("RQ2", "Requirement 2", "PA2", isModuleReadOnly: true);
			var req3AreaMapping = product.ModuleMappings.AddNew("RQ3", "Requirement 3", "PA3", isModuleReadOnly: true);
			req3AreaMapping.SourceModuleMappings.AddNew("CR8 Item B", "PA2");
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", isProductReadOnly: true);
			var srv1AreaMapping = product.ModuleMappings.AddNew("SV1", "Service 1", "PA1", isModuleReadOnly: true);
			srv1AreaMapping.SourceModuleMappings.AddNew("CR9 Item B", "PA2");
			var srv2AreaMapping = product.ModuleMappings.AddNew("SV2", "Service 2", "PA2", isModuleReadOnly: true);
			var srv3AreaMapping = product.ModuleMappings.AddNew("SV3", "Service 3", "PA3", isModuleReadOnly: true);
			srv3AreaMapping.SourceModuleMappings.AddNew("CR9 Item A", "PA2");
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			#endregion

			#region Menu Section

			var finder = new SourceModuleFinder("ENT", ModuleListType.MenuSection, "", Factory);
			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder("Should not include Source Modules that are not a valid override option", ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(ms1AreaMapping, wwwMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms1AreaMapping, xxxMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms2AreaMapping, zzzMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms2AreaMapping, reportMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms3AreaMapping, wwwMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms3AreaMapping, reportMenuSectionSourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "MS1";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(ms1AreaMapping, wwwMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms1AreaMapping, xxxMenuSectionSourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "PA2";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(ms1AreaMapping, xxxMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms2AreaMapping, zzzMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms2AreaMapping, reportMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms3AreaMapping, wwwMenuSectionSourceModule),
					new ModuleMappingWithSourceModule(ms3AreaMapping, reportMenuSectionSourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "MS1";
			finder.ProductAreaFilter = "PA2";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(ms1AreaMapping, xxxMenuSectionSourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "MS3";
			finder.ProductAreaFilter = "PA3";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ModuleMappingWithSourceModule>(), finder.SourceModules.Cast<ModuleMappingWithSourceModule>());
			#endregion

			#region Cr8

			finder = new SourceModuleFinder("ENT", ModuleListType.Cr8, "", Factory);
			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(req1AreaMapping, cr8MenuItemASourceModule),
					new ModuleMappingWithSourceModule(req1AreaMapping, cr8MenuItemBSourceModule),
					new ModuleMappingWithSourceModule(req3AreaMapping, cr8MenuItemBSourceModule),
					new ModuleMappingWithSourceModule(req2AreaMapping, cr8MenuItemDSourceModule),
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "RQ1";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(req1AreaMapping, cr8MenuItemASourceModule),
					new ModuleMappingWithSourceModule(req1AreaMapping, cr8MenuItemBSourceModule),
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "PA2";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(req1AreaMapping, cr8MenuItemASourceModule),
					new ModuleMappingWithSourceModule(req3AreaMapping, cr8MenuItemBSourceModule),
					new ModuleMappingWithSourceModule(req2AreaMapping, cr8MenuItemDSourceModule),
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "RQ1";
			finder.ProductAreaFilter = "PA2";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(req1AreaMapping, cr8MenuItemASourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "RQ3";
			finder.ProductAreaFilter = "PA3";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ModuleMappingWithSourceModule>(), finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			#endregion

			#region Cr9

			finder = new SourceModuleFinder("ENT", ModuleListType.Cr9, "", Factory);
			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(srv1AreaMapping, cr9MenuItemASourceModule),
					new ModuleMappingWithSourceModule(srv1AreaMapping, cr9MenuItemBSourceModule),
					new ModuleMappingWithSourceModule(srv3AreaMapping, cr9MenuItemASourceModule),
					new ModuleMappingWithSourceModule(srv2AreaMapping, cr9MenuItemDSourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "SV1";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(srv1AreaMapping, cr9MenuItemASourceModule),
					new ModuleMappingWithSourceModule(srv1AreaMapping, cr9MenuItemBSourceModule),
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "PA2";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(srv1AreaMapping, cr9MenuItemBSourceModule),
					new ModuleMappingWithSourceModule(srv3AreaMapping, cr9MenuItemASourceModule),
					new ModuleMappingWithSourceModule(srv2AreaMapping, cr9MenuItemDSourceModule),
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "SV1";
			finder.ProductAreaFilter = "PA2";
			AssertContainsExactElementsInAnyOrder(ModuleMappingWithSourceModuleComparer, ModuleMappingWithSourceModuleDisplayTextProvider,
				new[]
				{
					new ModuleMappingWithSourceModule(srv1AreaMapping, cr9MenuItemBSourceModule)
				},
				finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			finder.ModuleFilter = "SV3";
			finder.ProductAreaFilter = "PA3";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ModuleMappingWithSourceModule>(), finder.SourceModules.Cast<ModuleMappingWithSourceModule>());

			#endregion

			finder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			finder.ModuleFilter = "";
			finder.ProductAreaFilter = "";
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ModuleMappingWithSourceModule>(), finder.SourceModules.Cast<ModuleMappingWithSourceModule>());
		}

		#region Implementation

		ModuleMappingWithSourceModulePropertyComparer ModuleMappingWithSourceModuleComparer
		{
			get { return moduleMappingWithSourceModuleComparer ?? (moduleMappingWithSourceModuleComparer = new ModuleMappingWithSourceModulePropertyComparer()); }
		}
		ModuleMappingWithSourceModulePropertyComparer moduleMappingWithSourceModuleComparer;

		Converter<ModuleMappingWithSourceModule, string> ModuleMappingWithSourceModuleDisplayTextProvider
		{
			get { return moduleMappingWithSourceModulePDisplayTextProvider ?? (moduleMappingWithSourceModulePDisplayTextProvider = (ModuleMappingWithSourceModule x) => string.Format(CultureInfo.CurrentCulture, "{0}, {1}", x.ModuleCode, x.SourceModuleCode)); }
		}
		Converter<ModuleMappingWithSourceModule, string> moduleMappingWithSourceModulePDisplayTextProvider;

		#region Classes

		class ModuleMappingWithSourceModulePropertyComparer : IEqualityComparer<ModuleMappingWithSourceModule>
		{
			public bool Equals(ModuleMappingWithSourceModule x, ModuleMappingWithSourceModule y)
			{
				return x.ModuleCode == y.ModuleCode && x.SourceModuleCode == y.SourceModuleCode;
			}

			public int GetHashCode(ModuleMappingWithSourceModule obj)
			{
				return (obj.ModuleCode + obj.SourceModuleCode).GetHashCode();
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
		}

		#endregion

		#endregion
	}
}
