using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.Registry.ProductAreaModuleMapping;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(SupportIncidentFilterBusinessObject))]
	sealed class SupportIncidentFilterBusinessObjectTest : IncidentMainFilterBusinessObjectTestCase
	{
		public void TestBuildIncidentModuleFilterItems_WithProductFilters()
		{
			var filterBizObjWithOverridenModulesLookup = new SupportIncidentFilterBusinessObject_WithOverridenModuleListLookup(new FakeModuleMapping("|ENT|CR2|ARC|", "ENT", "CR2", "ARC", true), new FakeModuleMapping("|NON|CR8|___|", "NON", "CR8", "", true), new FakeModuleMapping("|ENT|___|CUS|", "ENT", "", "CUS", false), new FakeModuleMapping("|NON|___|___|", "NON", "", "", true), new FakeModuleMapping("|___|CR9|ARC|", "", "CR9", "ARC", false), new FakeModuleMapping("|___|CR3|___|", "", "CR3", "", false), new FakeModuleMapping("|___|___|CUS|", "", "", "CUS", true), new FakeModuleMapping("|___|___|___|", "", "", "", false));
			var productFilters = NewModuleTextFiltersWithValues("ENT");
			var productAreaFilters = Enumerable.Empty<ModuleTextFilter>();
			var actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Unspecified, productFilters, productAreaFilters);
			var expectedCodes = new string[] { "|ENT|CR2|ARC|", "|ENT|___|CUS|", };
			AssertFilterItemCodes("Should return modules satisfying Product filters", actualFilterItems, expectedCodes);
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.MenuSection, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|ENT|CR2|ARC|" };
			AssertFilterItemCodes("Should return menu sections satisfying Product filters", actualFilterItems, expectedCodes);
			productFilters = NewModuleTextFiltersWithValues("XXX");
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Unspecified, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|ENT|CR2|ARC|", "|NON|CR8|___|", "|ENT|___|CUS|", "|NON|___|___|", "|___|CR9|ARC|", "|___|CR3|___|", "|___|___|CUS|", "|___|___|___|" };
			AssertFilterItemCodes("If there are no modules satisfying Product filters fallback to displaying all modules", actualFilterItems, expectedCodes);
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.MenuSection, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|ENT|CR2|ARC|", "|___|CR3|___|" };
			AssertFilterItemCodes("If there are no modules satisfying Product filters fallback to displaying all categories", actualFilterItems, expectedCodes);
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Cr8, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|NON|CR8|___|" };
			AssertFilterItemCodes("If there are no modules satisfying Product filters fallback to displaying all cr8 modules", actualFilterItems, expectedCodes);
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Cr9, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|___|CR9|ARC|" };
			AssertFilterItemCodes("If there are no modules satisfying Product filters fallback to displaying all cr9 modules", actualFilterItems, expectedCodes);
		}

		public void TestBuildIncidentModuleFilterItems_WithProductFilters_WithProductAreaFilters()
		{
			var filterBizObjWithOverridenModulesLookup = new SupportIncidentFilterBusinessObject_WithOverridenModuleListLookup(new FakeModuleMapping("|ENT|CR2|ARC|", "ENT", "CR2", "ARC", true), new FakeModuleMapping("|NON|CR8|___|", "NON", "CR8", "", false), new FakeModuleMapping("|ENT|___|CUS|", "ENT", "", "CUS", false), new FakeModuleMapping("|NON|___|___|", "NON", "", "", true), new FakeModuleMapping("|___|CR8|ARC|", "", "CR8", "ARC", true), new FakeModuleMapping("|___|CR2|___|", "", "CR2", "", true), new FakeModuleMapping("|___|___|CUS|", "", "", "CUS", true), new FakeModuleMapping("|___|___|___|", "", "", "", true));
			var productFilters = NewModuleTextFiltersWithValues("ENT");
			var criticalityFilters = NewModuleTextFiltersWithValues("CR2");
			var productAreaFilters = NewModuleTextFiltersWithValues("ARC");
			var actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Unspecified, productFilters, productAreaFilters);
			var expectedCodes = new string[] { "|ENT|CR2|ARC|", };
			AssertFilterItemCodes("Product Area filters should override Product filters", actualFilterItems, expectedCodes);
			productAreaFilters = NewModuleTextFiltersWithValues("XXX");
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Unspecified, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|ENT|CR2|ARC|", "|ENT|___|CUS|" };
			AssertFilterItemCodes("If there are no modules satisfying Product Area filters fallback to Product filters", actualFilterItems, expectedCodes);
			productFilters = NewModuleTextFiltersWithValues("XXX");
			actualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.Unspecified, productFilters, productAreaFilters);
			expectedCodes = new string[] { "|ENT|CR2|ARC|", "|NON|CR8|___|", "|ENT|___|CUS|", "|NON|___|___|", "|___|CR8|ARC|", "|___|CR2|___|", "|___|___|CUS|", "|___|___|___|" };
			AssertFilterItemCodes("If there are no modules satisfying any of filters fallback to displaying all modules", actualFilterItems, expectedCodes);
		}

		public void TestBuildIncidentModuleFilterItems_WithEmptyProductAreaFilters()
		{
			var filterBizObjWithOverridenModulesLookup = new SupportIncidentFilterBusinessObject_WithOverridenModuleListLookup(new FakeModuleMapping("ARM", "ENT", "CR4", "ENT", true), new FakeModuleMapping("DCM", "ENT", "CR4", "ENT", true), new FakeModuleMapping("LCT", "ENT", "CR4", "ENT", true), new FakeModuleMapping("BAC", "IST", "CR4", "INF", true), new FakeModuleMapping("ZSO", "ZSO", "CR4", "ZSO", true));
			var entProductAreaFilters = NewModuleTextFiltersWithValues("ENT");
			var product = Enumerable.Empty<ModuleTextFilter>();
			var entProductAreaActualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.MenuSection, product, entProductAreaFilters);
			var entProductAreaExpectedCodes = new string[] { "ARM", "DCM", "LCT" };
			AssertFilterItemCodes("ENT ProductAreaFilter Should Not Contain Other Product Area Menu Section", entProductAreaActualFilterItems, entProductAreaExpectedCodes);

			var emptyProductAreaFilters = NewModuleTextFiltersWithValues("");
			var emptyProductAreaActualFilterItems = filterBizObjWithOverridenModulesLookup.BuildIncidentModuleFilterItems_Exposed(ModuleListType.MenuSection, product, emptyProductAreaFilters);
			var emptyProductAreaExpectedCodes = new string[] { "ARM", "DCM", "LCT" , "BAC", "ZSO" };
			AssertFilterItemCodes("empty ProductAreaFilter Should Not Contain Other Product Area Menu Section", emptyProductAreaActualFilterItems, emptyProductAreaExpectedCodes);
		}

		#region Implementation
		IEnumerable<ModuleTextFilter> NewModuleTextFiltersWithValues(params string[] values)
		{
			var result = new List<ModuleTextFilter>();
			foreach (var value in values)
			{
				var filter = new ModuleTextFilter("Dummy Text Filter", IncidentMainSchema.IM_Description);
				filter.Property = value;
				result.Add(filter);
			}

			return result;
		}

		void AssertFilterItemCodes(string message, CodeDescriptionPairList actualFilterItems, IEnumerable<string> expectedCodes)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Count", expectedCodes.Count(), actualFilterItems.Count);
				foreach (var code in expectedCodes)
				{
					Assert("Should contain: " + code, actualFilterItems.ContainsCode(code));
					actualFilterItems.RemoveCode(code);
				}

				foreach (ICodeDescription filterItem in actualFilterItems)
				{
					Fail("Should not contain: " + filterItem.Code);
				}
			});
		}

		class SupportIncidentFilterBusinessObject_WithOverridenModuleListLookup : SupportIncidentFilterBusinessObject
		{
			public SupportIncidentFilterBusinessObject_WithOverridenModuleListLookup(params FakeModuleMapping[] moduleList)
			{
				this.moduleList = moduleList;
			}

			readonly FakeModuleMapping[] moduleList;
			public CodeDescriptionPairList BuildIncidentModuleFilterItems_Exposed(ModuleListType moduleListType, IEnumerable<ModuleTextFilter> productFilters, IEnumerable<ModuleTextFilter> productAreaFilters)
			{
				return BuildIncidentModuleFilterItems(moduleListType, productFilters, productAreaFilters);
			}

			protected override string IncidentType
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			protected override IncidentMainLookups GetNewLookups()
			{
				return new SupportIncidentLookups_WithOverridenModuleList(Factory.New<SupportIncident>(), moduleList);
			}

			class SupportIncidentLookups_WithOverridenModuleList : SupportIncidentLookups
			{
				public SupportIncidentLookups_WithOverridenModuleList(SupportIncident parent, FakeModuleMapping[] moduleList) : base(parent)
				{
					this.moduleList.AddRange(moduleList);
				}

				readonly List<FakeModuleMapping> moduleList = new List<FakeModuleMapping>();
				protected override CodeDescriptionPairList GetModuleListCore(ModuleListType moduleListType, ZString product, ZString productArea)
				{
					var list = new CodeDescriptionPairList();
					foreach (var module in moduleList)
					{
						if ((moduleListType == ModuleListType.Unspecified || IncidentApprovalLookups.GetModuleListType(module.Criticality) == moduleListType) && (product.IsEmpty || module.Product == product) && (productArea.IsEmpty || module.ProductArea == productArea))
						{
							var newModule = new CodeDescriptionPair(module.Code, "");
							list.Add(newModule);
						}
					}

					return list;
				}
			}
		}

		class FakeModuleMapping
		{
			public readonly string Code;
			public readonly ZString Product;
			public readonly ZString Criticality;
			public readonly ZString ProductArea;
			public readonly bool IsEnabled;
			public FakeModuleMapping(string code, ZString product, ZString criticality, ZString productArea, bool isEnabled)
			{
				Code = code;
				Product = product;
				Criticality = criticality;
				ProductArea = productArea;
				IsEnabled = isEnabled;
			}
		}

		#endregion
		#region Text Filters

		public void TestFilterContainsGetCurrentTasks_WhenQIEnabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var filterBizo = new SupportIncidentFilterBusinessObject();
			var moduleFlagsFilter = (ModuleFlagsFilter)filterBizo["Current Task Only"];
			moduleFlagsFilter.Property0 = true;
			moduleFlagsFilter.IsActive = true;

			var query = filterBizo.Filter;
			CombineAssertions("The query should contain a reference to the GetCurrentTasks function when Quality Iterations is enabled.", () =>
			{
				AssertContains(BMGlobalConstants.CurrentTasksSQLFunctionText + "()", query.LiteralTextADO);
				AssertNotContains(BMGlobalConstants.CurrentTasksIgnoringIterationsSQLFunctionText + "()", query.LiteralTextADO);
			});
		}

		public void TestFilterContainsGetCurrentTasksIgnoringIterations_WhenQIDisabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var filterBizo = new SupportIncidentFilterBusinessObject();
			var moduleFlagsFilter = (ModuleFlagsFilter)filterBizo["Current Task Only"];
			moduleFlagsFilter.Property0 = true;
			moduleFlagsFilter.IsActive = true;

			var query = filterBizo.Filter;
			CombineAssertions("The query should contain a reference to the GetCurrentTasksIgnoringIterations function when Quality Iterations is disabled.", () =>
			{
				AssertNotContains(BMGlobalConstants.CurrentTasksSQLFunctionText + "()", query.LiteralTextADO);
				AssertContains(BMGlobalConstants.CurrentTasksIgnoringIterationsSQLFunctionText + "()", query.LiteralTextADO);
			});
		}
		public void TestProductFilter()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Product, "AAA").SetValue(IncidentMainSchema.IM_Module, EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList("AAA")[0].Code);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Product, "AAA").SetValue(IncidentMainSchema.IM_Module, EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList("AAA")[1].Code);
			SupportIncidentForTest.New(Factory, "103").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise).SetValue(IncidentMainSchema.IM_Module, ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise).SetValue(IncidentMainSchema.IM_Module, ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager);
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter productFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Product");
			productFilter.Property = ProductTypes.Codes.Enterprise;
			AssertFilteredResult(filterBizO, "103", "104");
			productFilter.Property = "AAA";
			AssertFilteredResult(filterBizO, "101", "102");
		}

		public void TestProductAreaListShouldBeInAlphabeticalOrder()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ZZZ", "Aardvark");
			list.AddPair("AAA", "Zebra");
			list.AddPair("CCC", "Cobra");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var productAreaList = filterBizO.ProductAreaList;
			AssertEquals("Should be in alphabetical order", "AAA", productAreaList[0].Code);
			AssertEquals("Should be in alphabetical order", "Zebra", productAreaList[0].Description);
			AssertEquals("Should be in alphabetical order", "CCC", productAreaList[1].Code);
			AssertEquals("Should be in alphabetical order", "Cobra", productAreaList[1].Description);
			AssertEquals("Should be in alphabetical order", "ZZZ", productAreaList[2].Code);
			AssertEquals("Should be in alphabetical order", "Aardvark", productAreaList[2].Description);
		}

		public void TestClientReferenceFilter()
		{
			var incident = SupportIncidentForTest.New(Factory, "101");
			var request = Factory.NewWithValidTestData<IncidentRequest>();
			incident.IM_INC_Request = request.PK;
			request.INC_ClientReference = "SPC109";
			Factory.Save();
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var clientReferenceFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Client Reference");
			clientReferenceFilter.Property = "SPC";
			clientReferenceFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertFilteredResult(filterBizO, "101");
		}

		#region Module Section / Requirement / Service
		public void TestMenuSectionFilter()
		{
			CreateTestDataForModuleTesting();
			var filterBizObj = new SupportIncidentFilterBusinessObject();
			var menuSectionFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Menu Section");
			menuSectionFilter.Property = "E01";
			var filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, false);
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[1].IsCurrentModuleEnabled, false);
			AssertFilteredResult(filterBizObj, "101", "106");
			menuSectionFilter.Property = "E02";
			filteredIncidents = PerformSearch(filterBizObj);
			filteredIncidents.Sort("IM_IncidentNumber");
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "102");
			menuSectionFilter.Property = "E03";
			filteredIncidents = PerformSearch(filterBizObj);
			filteredIncidents.Sort("IM_IncidentNumber");
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, false);
			AssertFilteredResult(filterBizObj, "107");
			menuSectionFilter.Property = "ZZ1";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "103");
			menuSectionFilter.Property = "ZZ2";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "104");
			menuSectionFilter.Property = "ZZ3";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "105");
		}

		public void TestRequirementFilter()
		{
			CreateTestDataForModuleTesting();
			var filterBizObj = new SupportIncidentFilterBusinessObject();
			var requirementFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Requirement");
			requirementFilter.Property = "E83";
			var filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: module belonging to this incident is disabled. " + "Incident should be returned even if associated module is disabled", filteredIncidents[0].IsCurrentModuleEnabled, false);
			AssertFilteredResult(filterBizObj, "108");
			requirementFilter.Property = "Z81";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: module belonging to this incident is enabled. " + "Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "109");
			requirementFilter.Property = "Z82";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: module belonging to this incident is disabled. " + "Incident should be returned even if associated module is disabled", filteredIncidents[0].IsCurrentModuleEnabled, false);
			AssertFilteredResult(filterBizObj, "110");
		}

		public void TestServiceFilter()
		{
			CreateTestDataForModuleTesting();
			var filterBizObj = new SupportIncidentFilterBusinessObject();
			var serviceFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Service");
			serviceFilter.Property = "Z83";
			var filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: module belonging to this incident is disabled. " + "Incident should be returned even if associated module is disabled", filteredIncidents[0].IsCurrentModuleEnabled, false);
			AssertFilteredResult(filterBizObj, "111");
			serviceFilter.Property = "E91";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: module belonging to this incident is enabled. " + "Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "112");
			serviceFilter.Property = "E92";
			filteredIncidents = PerformSearch(filterBizObj);
			AssertEquals("Precondition: module belonging to this incident is enabled. " + "Incident should be returned regardless of whether its associated module is enabled/disabled", filteredIncidents[0].IsCurrentModuleEnabled, true);
			AssertFilteredResult(filterBizObj, "113");
		}

		public void TestFilterItemListNotImpactedByEnabledDisabledModules()
		{
			CreateProductsAndEnabledDisabledModulesForTest();
			TestGetIncidentMenuSectionFilterItemList();
			TestGetIncidentRequirementFilterItemList();
			TestGetIncidentServiceFilterItemList();
		}

		void TestGetIncidentMenuSectionFilterItemList()
		{
			var filterBizObj = new SupportIncidentFilterBusinessObject();
			var incidentMenuSectionFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Menu Section");
			var incidentMenuSectionFilterList = incidentMenuSectionFilter.List;
			var incidentMenuSectionFilterListAsArray = new object[incidentMenuSectionFilterList.Count];
			incidentMenuSectionFilterList.CopyTo(incidentMenuSectionFilterListAsArray, 0);
			var enterpriseDefaults = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var fullModuleList = enterpriseDefaults.GetFullModuleList();
			fullModuleList.AddPair("ZZ1", "[ZZZ Product] ZZZ Module 1 Enabled");
			fullModuleList.AddPair("ZZ2", "[ZZZ Product] ZZZ Module 2 Enabled");
			fullModuleList.AddPair("ZZ3", "[ZZZ Product] ZZZ Module 3 Internal Enabled");
			fullModuleList.AddPair("E01", "[ediEnterprise / CargoWise One] Ent 1 Disabled");
			fullModuleList.AddPair("E02", "[ediEnterprise / CargoWise One] Ent 2 Internal Enabled");
			fullModuleList.AddPair("E03", "[ediEnterprise / CargoWise One] Ent 3 Disabled");
			fullModuleList.Sort();
			AssertArrayEqualsByElements(incidentMenuSectionFilterListAsArray, fullModuleList.ToArray());
		}

		void TestGetIncidentRequirementFilterItemList()
		{
			var filterBizObj = new SupportIncidentFilterBusinessObject();
			var incidentRequirementFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Requirement");
			var incidentRequirementFilterList = incidentRequirementFilter.List;
			var incidentRequirementFilterListAsArray = new object[incidentRequirementFilterList.Count];
			incidentRequirementFilterList.CopyTo(incidentRequirementFilterListAsArray, 0);
			var expectedIncidentRequirementFilterItems = new CodeDescriptionPairList();
			expectedIncidentRequirementFilterItems.AddPair("E81", "[ediEnterprise / CargoWise One] Ent CR8 Mod 1 Disabled");
			expectedIncidentRequirementFilterItems.AddPair("E82", "[ediEnterprise / CargoWise One] Ent CR8 Mod 2 Enabled");
			expectedIncidentRequirementFilterItems.AddPair("E83", "[ediEnterprise / CargoWise One] Ent CR8 Mod 3 Internal Disabled");
			expectedIncidentRequirementFilterItems.AddPair("Z81", "[ZZZ Product] ZZZ CR8 Mod 1 Enabled");
			expectedIncidentRequirementFilterItems.AddPair("Z82", "[ZZZ Product] ZZZ CR8 Mod 2 Disabled");
			expectedIncidentRequirementFilterItems.AddPair("Z83", "[ZZZ Product] ZZZ CR8 Mod 3 Disabled");
			expectedIncidentRequirementFilterItems.Sort();
			AssertArrayEqualsByElements(incidentRequirementFilterListAsArray, expectedIncidentRequirementFilterItems.ToArray());
		}

		void TestGetIncidentServiceFilterItemList()
		{
			var filterBizObj = new SupportIncidentFilterBusinessObject();

			ModuleTextFilter productAreaFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Product Area");
			productAreaFilter.Property = ProductAreaList.Codes.CUS;
			productAreaFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			ModuleTextFilter productFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Product");
			productFilter.Property = "ZZZ";
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var incidentServiceFilter = ActivateFilter<ModuleTextFilter>(filterBizObj, "Service");
			var incidentServiceFilterList = incidentServiceFilter.List;
			var incidentServiceFilterListAsArray = new object[incidentServiceFilterList.Count];
			incidentServiceFilterList.CopyTo(incidentServiceFilterListAsArray, 0);

			var expectedIncidentServiceFilterItems = new CodeDescriptionPairList();
			expectedIncidentServiceFilterItems.AddPair("E91", "[ediEnterprise / CargoWise One] Ent CR9 Mod 1 Enabled");
			expectedIncidentServiceFilterItems.AddPair("E92", "[ediEnterprise / CargoWise One] Ent CR9 Mod 2 Enabled");
			expectedIncidentServiceFilterItems.AddPair("E93", "[ediEnterprise / CargoWise One] Ent CR9 Mod 3 Internal Enabled");
			expectedIncidentServiceFilterItems.AddPair("Z91", "[ZZZ Product] ZZZ CR9 Mod 1 Disabled");
			expectedIncidentServiceFilterItems.AddPair("Z92", "[ZZZ Product] ZZZ CR9 Mod 2 Disabled");
			expectedIncidentServiceFilterItems.AddPair("Z93", "[ZZZ Product] ZZZ CR9 Mod 3 Internal Disabled");
			expectedIncidentServiceFilterItems.AddPair("Z94", "[ZZZ Product] ZZZ CR9 Mod 4 Enabled");
			expectedIncidentServiceFilterItems.Sort();
			AssertArrayEqualsByElements(incidentServiceFilterListAsArray, expectedIncidentServiceFilterItems.ToArray());
		}

		#region Set Up Test Environment
		void ClearRegistry()
		{
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
		}

		void CreateProductsAndEnabledDisabledModulesForTest()
		{
			ClearRegistry();
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1 Enabled", "", true, false, true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2 Enabled", "", true, false, true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal Enabled", "", true, true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1 Disabled", ZString.Empty, true, false, false);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal Enabled", ZString.Empty, true, true, true);
			enterpriseProduct.ModuleMappings.AddNew("E03", "Ent 3 Disabled", ZString.Empty, true, false, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var cr8Products = new SystemProductCollection();
			{
				var ent = cr8Products.AddNew(ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("E81", "Ent CR8 Mod 1 Disabled", ZString.Empty, true, false, false);
				ent.ModuleMappings.AddNew("E82", "Ent CR8 Mod 2 Enabled", ZString.Empty, true, false, true);
				ent.ModuleMappings.AddNew("E83", "Ent CR8 Mod 3 Internal Disabled", ZString.Empty, true, true, false);
				var zzz = cr8Products.AddNew("ZZZ", "ZZZ Product", true);
				zzz.ModuleMappings.AddNew("Z81", "ZZZ CR8 Mod 1 Enabled", ZString.Empty, true, false, true);
				zzz.ModuleMappings.AddNew("Z82", "ZZZ CR8 Mod 2 Disabled", ZString.Empty, true, false, false);
				zzz.ModuleMappings.AddNew("Z83", "ZZZ CR8 Mod 3 Disabled", ZString.Empty, true, true, false);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			}

			var cr9Products = new SystemProductCollection();
			{
				var ent = cr9Products.AddNew(ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("E91", "Ent CR9 Mod 1 Enabled", ZString.Empty, true, false, true);
				ent.ModuleMappings.AddNew("E92", "Ent CR9 Mod 2 Enabled", ZString.Empty, true, false, true);
				ent.ModuleMappings.AddNew("E93", "Ent CR9 Mod 3 Internal Enabled", ProductAreaList.Codes.CUS, true, true, true);
				var zzz = cr9Products.AddNew("ZZZ", "ZZZ Product", true);
				zzz.ModuleMappings.AddNew("Z91", "ZZZ CR9 Mod 1 Disabled", ZString.Empty, true, false, false);
				zzz.ModuleMappings.AddNew("Z92", "ZZZ CR9 Mod 2 Disabled", ZString.Empty, true, false, false);
				zzz.ModuleMappings.AddNew("Z93", "ZZZ CR9 Mod 3 Internal Disabled", ProductAreaList.Codes.CUS, true, true, false);
				zzz.ModuleMappings.AddNew("Z94", "ZZZ CR9 Mod 4 Enabled", ProductAreaList.Codes.CUS, true, false, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			}
		}

		void CreateTestDataForModuleTesting()
		{
			CreateProductsAndEnabledDisabledModulesForTest();
			#region Menu Sections (101 to 107)
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR1_SystemDown).SetValue(IncidentMainSchema.IM_Module, "E01").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR2_ModuleDown).SetValue(IncidentMainSchema.IM_Module, "E02").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise);
			SupportIncidentForTest.New(Factory, "103").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround).SetValue(IncidentMainSchema.IM_Module, "ZZ1").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround).SetValue(IncidentMainSchema.IM_Module, "ZZ2").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			SupportIncidentForTest.New(Factory, "105").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR5_Training).SetValue(IncidentMainSchema.IM_Module, "ZZ3").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			SupportIncidentForTest.New(Factory, "106").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest).SetValue(IncidentMainSchema.IM_Module, "E01").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise);
			SupportIncidentForTest.New(Factory, "107").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest).SetValue(IncidentMainSchema.IM_Module, "E03").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise);
			#endregion
			#region Compliance Requirements (108 to 110)
			SupportIncidentForTest.New(Factory, "108").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement).SetValue(IncidentMainSchema.IM_Module, "E83").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			SupportIncidentForTest.New(Factory, "109").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement).SetValue(IncidentMainSchema.IM_Module, "Z81").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			SupportIncidentForTest.New(Factory, "110").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement).SetValue(IncidentMainSchema.IM_Module, "Z82").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			#endregion
			#region Service Requests (111 to 113)
			SupportIncidentForTest.New(Factory, "111").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest).SetValue(IncidentMainSchema.IM_Module, "Z83").SetValue(IncidentMainSchema.IM_Product, "ZZZ");
			SupportIncidentForTest.New(Factory, "112").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest).SetValue(IncidentMainSchema.IM_Module, "E91").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise);
			SupportIncidentForTest.New(Factory, "113").SetValue(IncidentMainSchema.IM_Priority, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest).SetValue(IncidentMainSchema.IM_Module, "E92").SetValue(IncidentMainSchema.IM_Product, ProductTypes.Codes.Enterprise);
			#endregion
		}

		#endregion
		#endregion
		public void TestProductAreaFilter()
		{
			#region Test Data
			SupportIncidentForTest.New(Factory, "100").SetValue(IncidentMainSchema.IM_ProgramArea, "ARC");
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_ProgramArea, "ARC");
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_ProgramArea, "ARC");
			SupportIncidentForTest.New(Factory, "200").SetValue(IncidentMainSchema.IM_ProgramArea, "CRM");
			SupportIncidentForTest.New(Factory, "201").SetValue(IncidentMainSchema.IM_ProgramArea, "CRM");
			SupportIncidentForTest.New(Factory, "202").SetValue(IncidentMainSchema.IM_ProgramArea, "CRM");
			SupportIncidentForTest.New(Factory, "300").SetValue(IncidentMainSchema.IM_ProgramArea, "CUS");
			SupportIncidentForTest.New(Factory, "301").SetValue(IncidentMainSchema.IM_ProgramArea, "CUS");
			SupportIncidentForTest.New(Factory, "302").SetValue(IncidentMainSchema.IM_ProgramArea, "CUS");
			SupportIncidentForTest.New(Factory, "950").SetValue(IncidentMainSchema.IM_ProgramArea, "");
			SupportIncidentForTest.New(Factory, "960").SetValue(IncidentMainSchema.IM_ProgramArea, "");
			#endregion
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter areaFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Product Area");
			areaFilter.Property = ProductAreaList.Codes.CUS;
			areaFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertFilteredResult(filterBizO, "300", "301", "302");
			areaFilter.Property = ProductAreaList.Codes.CRM;
			areaFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertFilteredResult(filterBizO, "200", "201", "202");
			areaFilter.Property = ProductAreaList.Codes.ARC;
			areaFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertFilteredResult(filterBizO, "100", "101", "102");
			areaFilter.Property = "";
			areaFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertFilteredResult(filterBizO, "950", "960");
			areaFilter.Property = "";
			areaFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertFilteredResult(filterBizO, "100", "101", "102", "200", "201", "202", "300", "301", "302");
			areaFilter.Property = ProductAreaList.Codes.CRM;
			areaFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			AssertFilteredResult(filterBizO, "100", "101", "102", "300", "301", "302", "950", "960");
		}

		public void TestCountryFilter()
		{
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_RN_NKCountry, "AU");
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_RN_NKCountry, "US");
			SupportIncidentForTest.New(Factory, "103").SetValue(IncidentMainSchema.IM_RN_NKCountry, "AU");
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_RN_NKCountry, "CA");
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var countryFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Country");
			countryFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			countryFilter.Property = "AU";
			AssertFilteredResult(filterBizO, "101", "103");
			countryFilter.Property = "US";
			AssertFilteredResult(filterBizO, "102");
		}

		public void TestSourceFilter()
		{
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Source, SupportIncidentLookups.SourceListConstants.CreatedFromProject);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Source, SupportIncidentLookups.SourceListConstants.ERequestPortal);
			SupportIncidentForTest.New(Factory, "103").SetValue(IncidentMainSchema.IM_Source, SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd);
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_Source, SupportIncidentLookups.SourceListConstants.CreatedFromProject);
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter sourceFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Source");
			sourceFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			sourceFilter.Property = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			AssertFilteredResult(filterBizO, "101", "104");
			sourceFilter.Property = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			AssertFilteredResult(filterBizO, "102");
		}

		public void TestProblemDescriptionFilter()
		{
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleFilter filter = filterBizO["Problem Description"];
			AssertEquals(typeof(ModuleTextFilter), filter.GetType());
			AssertEquals(IncidentMainSchema.IM_Description, filter.FilterColumn);
		}

		public void TestResolutionCommentFilter()
		{
			SupportIncident supportIncident1 = Factory.New<SupportIncident>();
			supportIncident1.IM_Product = "ENT";
			supportIncident1.IM_Status = SupportIncidentLookups.Status.Closed;
			supportIncident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			supportIncident1.IM_Description = "Something wrong is happening with this World!";
			supportIncident1.ResolutionNoteText = "Works as designed";
			SupportIncident supportIncident2 = Factory.New<SupportIncident>();
			supportIncident2.IM_Product = "ENT";
			supportIncident2.IM_Status = SupportIncidentLookups.Status.Closed;
			supportIncident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			supportIncident2.IM_Description = "Something REALLY wrong is happening with this World!";
			supportIncident2.ResolutionNoteText = "Client has been moved to another world";
			Factory.Save();
			SupportIncidentCollection incidentCollection;
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			incidentCollection = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Resolution Comment"]).Property = "";
			((ModuleTextFilter)filter["Resolution Comment"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			((ModuleTextFilter)filter["Resolution Comment"]).Property = "as designed";
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			((ModuleTextFilter)filter["Resolution Comment"]).Property = "has been moved to another world";
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident2, incidentCollection);
			((ModuleTextFilter)filter["Resolution Comment"]).Property = "as";
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
		}

		#endregion
		public void TestAddWorkflowCustomFieldsFilters()
		{
			ModuleFilterCollection filterCollection = new SupportIncidentFilterBusinessObject().ModuleFilters;
			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNull(filterCollection["C21"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["C31"]);
			PrepareTemplates();
			filterCollection = new SupportIncidentFilterBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["C12"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["C21"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C22"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertNull(filterCollection["C31"]);
		}

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = IncidentConstants.IncidentType.SupportIncident;
			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;
			GenCustomColumnDefinition def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;
			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = IncidentConstants.IncidentType.SupportIncident;
			GenCustomColumnDefinition def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;
			GenCustomColumnDefinition def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;
			GenCustomColumnDefinition defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;
			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";
			GenCustomColumnDefinition def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
		}

		public void TestClientManagementGroupFilter()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Test org full name";
			var anotherOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			anotherOrganisation.OH_FullName = "Test another org full name";
			var managementOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			managementOrganisation.OH_FullName = "Test management org full name";
			organisation.SetRelatedParty(managementOrganisation, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder);
			var supportIncident1 = Factory.New<SupportIncident>();
			supportIncident1.IM_Product = "ENT";
			supportIncident1.IM_Status = SupportIncidentLookups.Status.Closed;
			supportIncident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			supportIncident1.IM_Description = "Something wrong is happening with this World!";
			supportIncident1.ResolutionNoteText = "Works as designed";
			supportIncident1.IM_OH_Client = organisation.PK;
			var supportIncident2 = Factory.New<SupportIncident>();
			supportIncident2.IM_Product = "ENT";
			supportIncident2.IM_Status = SupportIncidentLookups.Status.Closed;
			supportIncident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			supportIncident2.IM_Description = "Something REALLY wrong is happening with this World!";
			supportIncident2.ResolutionNoteText = "Client has been moved to another world";
			supportIncident2.IM_OH_Client = organisation.PK;
			var supportIncident3 = Factory.New<SupportIncident>();
			supportIncident3.IM_Product = "ENT";
			supportIncident3.IM_Status = SupportIncidentLookups.Status.Working;
			supportIncident3.IM_Description = "Who am I?";
			supportIncident3.IM_OH_Client = anotherOrganisation.PK;
			Factory.Save();
			SupportIncidentCollection incidentCollection;
			var filter = new SupportIncidentFilterBusinessObject();
			incidentCollection = new SupportIncidentCollection(Factory);
			CreateOrganisationFilter(filter, "Test org full name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			((ModuleFlagsFilter)filter["Client Management Group"]).Property0 = true;
			((ModuleFlagsFilter)filter["Client Management Group"]).IsActive = true;
			CreateOrganisationFilter(filter, "Test org full name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			((ModuleFlagsFilter)filter["Client Management Group"]).Property0 = true;
			CreateOrganisationFilter(filter, "Test another org full name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionContains(supportIncident3, incidentCollection);
			((ModuleFlagsFilter)filter["Client Management Group"]).Property0 = true;
			CreateOrganisationFilter(filter, "Test management org full name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			((ModuleFlagsFilter)filter["Client Management Group"]).Property0 = false;
			CreateOrganisationFilter(filter, "Test management org full name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			supportIncident1.Delete();
			supportIncident2.Delete();
			supportIncident3.Delete();
			Factory.Save();
		}

		void CreateOrganisationFilter(SupportIncidentFilterBusinessObject filter, string nameValue)
		{
			var organisationFilter = (ModuleGuidFilter)filter["Organisation"];
			organisationFilter.IsActive = true;
			organisationFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var nameFilter = organisationFilter.SelectedFilters["Name"] as ModuleTextFilter;
			if (nameFilter == null || string.IsNullOrEmpty(nameFilter.Property))
			{
				organisationFilter.SelectedFilters.AddTextFilterStrip("Name", nameValue);
			}
			else
			{
				nameFilter.Property = nameValue;
			}
		}

		#region Staff Assignment Filters
		public void TestAssignedToFilter()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff dev = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident supportIncident = Factory.New<SupportIncident>();
			supportIncident.IM_Category = "SUP";
			supportIncident.IM_Product = "ENT";
			supportIncident.IM_Status = SupportIncidentLookups.Status.Open;
			supportIncident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			supportIncident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			SupportIncident defectIncident = Factory.New<SupportIncident>();
			defectIncident.IM_Category = "DEF";
			defectIncident.IM_Product = "ENT";
			defectIncident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			defectIncident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			defectIncident.IM_GS_NKAssignedToCurrent = dev.GS_Code;
			SupportIncident supportIncidentReturnedFromDefect = Factory.New<SupportIncident>();
			supportIncidentReturnedFromDefect.IM_Category = "SUP";
			supportIncidentReturnedFromDefect.IM_Product = "ENT";
			supportIncidentReturnedFromDefect.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			supportIncidentReturnedFromDefect.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			supportIncidentReturnedFromDefect.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			supportIncidentReturnedFromDefect.IM_GS_NKAssignedToCurrent = dev.GS_Code;
			SupportIncident supportIncidentReturnedFromFeature = Factory.New<SupportIncident>();
			supportIncidentReturnedFromFeature.IM_Category = "SUP";
			supportIncidentReturnedFromFeature.IM_Product = "ENT";
			supportIncidentReturnedFromFeature.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			supportIncidentReturnedFromFeature.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			supportIncidentReturnedFromFeature.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			supportIncidentReturnedFromFeature.IM_GS_NKAssignedToCurrent = dev.GS_Code;
			Factory.Save();
			SupportIncidentCollection incidents;
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			// All
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = "All Stages";
			filter["Stage"].IsActive = true;
			incidents.Load(filter.Filter);
			AssertEquals(4, incidents.Count);
			// All, Dev Staff
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = "All Stages";
			((ModuleNkFilter)filter["Assigned To"]).Property = dev.GS_Code;
			filter["Assigned To"].IsActive = true;
			incidents.Load(filter.Filter);
			AssertEquals(1, incidents.Count);
			AssertEquals(defectIncident.PK, incidents[0].PK);
			// All, Support Staff
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = "All Stages";
			((ModuleNkFilter)filter["Assigned To"]).Property = supportStaff.GS_Code;
			incidents.Load(filter.Filter);
			AssertEquals(3, incidents.Count);
			AssertCollectionContains(supportIncident, incidents);
			AssertCollectionContains(supportIncidentReturnedFromDefect, incidents);
			AssertCollectionContains(supportIncidentReturnedFromFeature, incidents);
			// Defect, Support Staff
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = SupportIncidentCategoriesList.Codes.Defect;
			((ModuleNkFilter)filter["Assigned To"]).Property = supportStaff.GS_Code;
			incidents.Load(filter.Filter);
			AssertEquals(0, incidents.Count);
			// Defect, Dev Staff
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = SupportIncidentCategoriesList.Codes.Defect;
			((ModuleNkFilter)filter["Assigned To"]).Property = dev.GS_Code;
			incidents.Load(filter.Filter);
			AssertEquals(1, incidents.Count);
			AssertEquals(defectIncident.PK, incidents[0].PK);
			// Support, Dev Staff
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = SupportIncidentCategoriesList.Codes.Support;
			((ModuleNkFilter)filter["Assigned To"]).Property = dev.GS_Code;
			incidents.Load(filter.Filter);
			AssertEquals(0, incidents.Count);
			// Support, Support Staff
			incidents = new SupportIncidentCollection(Factory);
			((ModuleTextFilter)filter["Stage"]).Property = SupportIncidentCategoriesList.Codes.Support;
			((ModuleNkFilter)filter["Assigned To"]).Property = supportStaff.GS_Code;
			incidents.Load(filter.Filter);
			AssertEquals(3, incidents.Count);
			AssertCollectionContains(supportIncident, incidents);
			AssertCollectionContains(supportIncidentReturnedFromDefect, incidents);
			AssertCollectionContains(supportIncidentReturnedFromFeature, incidents);
		}

		public void TestAssignedToFilter_IsBlankAndIsNotBlank()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff devStaff = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident support = Factory.New<SupportIncident>();
			support.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			SupportIncident defect = Factory.New<SupportIncident>();
			defect.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			defect.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			SupportIncident feature = Factory.New<SupportIncident>();
			feature.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			feature.IM_GS_NKAssignedToCurrent = devStaff.GS_Code;
			Factory.Save();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			SupportIncidentFilterBusinessObject filters = new SupportIncidentFilterBusinessObject();
			var assignedToFilter = (ModuleNkFilter)filters["Assigned To"];
			assignedToFilter.IsActive = true;
			assignedToFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			collection.Load(filters.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(support, collection);
			AssertCollectionContains(defect, collection);
			assignedToFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(feature, collection);
			support.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			Factory.Save();
			assignedToFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			collection.Load(filters.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(support, collection);
			AssertCollectionContains(feature, collection);
		}

		public void TestAssignedToFilter_EqualAndNotEqual()
		{
			var supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			var devStaff = Factory.NewWithValidTestData<GlbStaff>();
			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident1.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident2.IM_GS_NKAssignedToCurrent = devStaff.GS_Code;
			Factory.Save();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			SupportIncidentFilterBusinessObject filters = new SupportIncidentFilterBusinessObject();
			var assignedToFilter = (ModuleNkFilter)filters["Assigned To"];
			assignedToFilter.IsActive = true;
			assignedToFilter.Property = supportStaff.GS_Code;
			assignedToFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident1, collection);
			assignedToFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident2, collection);
		}

		public void TestAssignedToFilter_ClosedStatus()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff devStaff = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, supportStaff.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.Completed);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, supportStaff.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, devStaff.GS_Code);
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, GlbStaff.CurrentUser.GS_Code);
			SupportIncidentForTest.New(Factory, "105").SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, supportStaff.GS_Code);
			SupportIncidentForTest.New(Factory, "106").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, devStaff.GS_Code);
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleNkFilter assignedToFilter = ActivateFilter<ModuleNkFilter>(filterBizO, "Assigned To");
			assignedToFilter.Property = supportStaff.GS_Code;
			AssertFilteredResult(filterBizO, "101", "105");
			ModuleTextFilter statusFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Status");
			statusFilter.Property = SupportIncidentLookups.Status.Closed;
			AssertFilteredResult(filterBizO, "101");
			assignedToFilter.Property = devStaff.GS_Code;
			AssertFilteredResult(filterBizO, "102");
			statusFilter.IsActive = false;
			AssertFilteredResult(filterBizO, "102", "106");
		}

		public void TestAssignedToFilter_AllStages()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff devStaff = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, supportStaff.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, devStaff.GS_Code);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, supportStaff.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, devStaff.GS_Code);
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, devStaff.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, GlbStaff.CurrentUser.GS_Code);
			SupportIncidentForTest.New(Factory, "105").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, supportStaff.GS_Code);
			SupportIncidentForTest.New(Factory, "106").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, devStaff.GS_Code);
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter stageFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			stageFilter.Property = SupportIncidentFilterBusinessObject.AllStages;
			ModuleTextFilter statusFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Status");
			statusFilter.Property = IncidentMainLookups.Status.NotClosed;
			ModuleNkFilter assignedToFilter = ActivateFilter<ModuleNkFilter>(filterBizO, "Assigned To");
			assignedToFilter.Property = supportStaff.GS_Code;
			AssertFilteredResult(filterBizO, "105");
			assignedToFilter.Property = devStaff.GS_Code;
			AssertFilteredResult(filterBizO, "101", "106");
			assignedToFilter.Property = GlbStaff.CurrentUser.GS_Code;
			AssertFilteredResult(filterBizO, "104");
		}

		public void TestStaffUnassignedFilter()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident support1 = Factory.New<SupportIncident>();
			support1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			support1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			SupportIncident support2 = Factory.New<SupportIncident>();
			support2.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			support2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			SupportIncident support3 = Factory.New<SupportIncident>();
			support3.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			support3.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			support3.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			SupportIncident defect = Factory.New<SupportIncident>();
			defect.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			defect.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			SupportIncident feature = Factory.New<SupportIncident>();
			feature.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			feature.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			Factory.Save();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			SupportIncidentFilterBusinessObject filters = new SupportIncidentFilterBusinessObject();
			var staffUassignedFilter = (ModuleFlagsFilter)filters["Disposition (No Staff Assigned)"];
			staffUassignedFilter.IsActive = true;
			staffUassignedFilter["Disposition: Added Awaiting Assignment"] = true;
			staffUassignedFilter["Disposition: Assigned - Awaiting Action"] = false;
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(support1, collection);
			staffUassignedFilter["Disposition: Added Awaiting Assignment"] = false;
			staffUassignedFilter["Disposition: Assigned - Awaiting Action"] = true;
			collection.Load(filters.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(support2, collection);
			AssertCollectionContains(feature, collection);
			staffUassignedFilter["Disposition: Added Awaiting Assignment"] = true;
			staffUassignedFilter["Disposition: Assigned - Awaiting Action"] = true;
			collection.Load(filters.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(support1, collection);
			AssertCollectionContains(support2, collection);
			AssertCollectionContains(feature, collection);
		}

		public void TestLastTaskClosedByStaffFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			SupportIncident support1 = Factory.New<SupportIncident>();
			var task11 = support1.WorkflowItems.AddNew();
			task11.P9_GS_NKAssignedStaffMember = "ST1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task11.P9_Sequence = 10;
			var task12 = support1.WorkflowItems.AddNew();
			task12.P9_GS_NKAssignedStaffMember = "ST2";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task12.P9_Sequence = 20;
			SupportIncident support2 = Factory.New<SupportIncident>();
			var task21 = support2.WorkflowItems.AddNew();
			task21.P9_GS_NKAssignedStaffMember = "ST1";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task21.P9_Sequence = 10;
			var task22 = support2.WorkflowItems.AddNew();
			task22.P9_GS_NKAssignedStaffMember = "ST2";
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task22.P9_Sequence = 20;
			SupportIncident support3 = Factory.New<SupportIncident>();
			support3.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			SupportIncidentFilterBusinessObject filters = new SupportIncidentFilterBusinessObject();
			var lastClosedByFilter = (ModuleNkFilter)filters["Last Task Closed By Staff"];
			lastClosedByFilter.IsActive = true;
			lastClosedByFilter.Property = "ST1";
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(support2, collection);
			lastClosedByFilter.Property = "ST2";
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(support1, collection);
			lastClosedByFilter.Property = GlbStaff.CurrentUser.GS_Code;
			collection.Load(filters.Filter);
			AssertEquals(0, collection.Count);
		}

		#endregion
		public void TestRelatedProjectFilter()
		{
			#region Prepare Test Data
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			SupportIncident supportIncident1 = Factory.New<SupportIncident>();
			supportIncident1.IM_OH_Client = client.PK;
			EDIProject project1 = Factory.NewWithValidTestData<EDIProject>();
			supportIncident1.RelatedProjectPK = project1.PK;
			SupportIncident supportIncident2 = Factory.New<SupportIncident>();
			supportIncident2.IM_OH_Client = client.PK;
			EDIProject project2 = Factory.NewWithValidTestData<EDIProject>();
			supportIncident2.RelatedProjectPK = project2.PK;
			SupportIncident supportIncident3 = Factory.New<SupportIncident>();
			supportIncident3.IM_OH_Client = client.PK;
			Factory.Save();
			#endregion
			SupportIncidentCollection incidentCollection;
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			incidentCollection = new SupportIncidentCollection(Factory);
			ModuleGuidFilter relatedProjectFilter = (ModuleGuidFilter)filter["Related Project"];
			relatedProjectFilter.IsActive = true;
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionContains(supportIncident3, incidentCollection);
			relatedProjectFilter.Property = project1.PK;
			relatedProjectFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			relatedProjectFilter.Property = project2.PK;
			relatedProjectFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			relatedProjectFilter.Property = project2.PK;
			relatedProjectFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			relatedProjectFilter.Property = ZGuid.Empty;
			relatedProjectFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionContains(supportIncident3, incidentCollection);
			relatedProjectFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
		}

		#region Relationsip Manager Filters
		public void TestRelationshipManagerPrimaryIncidentFilter()
		{
			TestRelationshipManagerIncidentFilter("RM1", "Key Account Manager - Primary");
		}

		public void TestRelationshipManagerSecondaryIncidentFilter()
		{
			TestRelationshipManagerIncidentFilter("RM2", "Key Account Manager - Secondary");
		}

		void TestRelationshipManagerIncidentFilter(string staffRole, string filterName)
		{
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			OrgStaffAssignments assignment = org1.StaffAssignments.AddNew();
			assignment.O8_OH = org1.PK;
			assignment.O8_Department = "ALL";
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = staffRole;
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			OrgStaffAssignments assignment2 = org2.StaffAssignments.AddNew();
			assignment2.O8_OH = org2.PK;
			assignment2.O8_Department = "ALL";
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = staffRole;
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org1.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO[filterName];
			filter.IsActive = true;
			filter.Property = staff1.PK;
			SupportIncidentCollection incidentCollection = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidentCollection.Load();
			AssertCollectionNotContains("Should not contain incident2.", incident2, incidentCollection);
			AssertCollectionContains("Should contain incident1.", incident1, incidentCollection);
			AssertCollectionContains("Should contain incident3.", incident3, incidentCollection);
			filter.Property = staff2.PK;
			incidentCollection = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidentCollection.Load();
			AssertCollectionContains("Should contain incident2.", incident2, incidentCollection);
			AssertCollectionNotContains("Should not contain incident1.", incident1, incidentCollection);
			AssertCollectionNotContains("Should not contain incident3.", incident3, incidentCollection);
		}

		#endregion
		#region Stage / Status / Disposition
		public void TestStageFilter()
		{
			CreateTestDataForStageStatusDispositionTests();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter stageFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Support;
			AssertFilteredResult(filterBizO, "101", "102", "103", "107");
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Defect;
			AssertFilteredResult(filterBizO, "108", "109", "110", "111", "112", "113");
			stageFilter.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			AssertFilteredResult(filterBizO, "114", "115", "116", "117", "118", "119", "120");
		}

		public void TestStageFilter_WithPersistentStageInfo()
		{
			SupportIncident incident1 = Factory.New<SupportIncident>();
			SupportIncident incident2 = Factory.New<SupportIncident>();
			SupportIncident incident3 = Factory.New<SupportIncident>();
			SupportIncident incident4 = Factory.New<SupportIncident>();
			SupportIncident incident5 = Factory.New<SupportIncident>();
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident3.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident4.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident5.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter filter = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			filter.Property = SupportIncidentCategoriesList.Codes.Support;
			collection.Load(filterBizO.Filter);
			AssertCollectionContains(incident1, collection);
			AssertCollectionNotContains(incident2, collection);
			AssertCollectionNotContains(incident3, collection);
			AssertCollectionNotContains(incident4, collection);
			AssertCollectionContains(incident5, collection);
			filter.Property = SupportIncidentCategoriesList.Codes.Defect;
			collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(incident1, collection);
			AssertCollectionNotContains(incident2, collection);
			AssertCollectionContains(incident3, collection);
			AssertCollectionContains(incident4, collection);
			AssertCollectionNotContains(incident5, collection);
			filter.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			AssertCollectionNotContains(incident3, collection);
			AssertCollectionNotContains(incident4, collection);
			AssertCollectionNotContains(incident5, collection);
			filter.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			collection.Load(filterBizO.Filter);
			AssertCollectionNotContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			AssertCollectionNotContains(incident3, collection);
			AssertCollectionNotContains(incident4, collection);
			AssertCollectionNotContains(incident5, collection);
			filter.Property = "All Stages";
			collection.Load(filterBizO.Filter);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			AssertCollectionContains(incident3, collection);
			AssertCollectionContains(incident4, collection);
			AssertCollectionContains(incident5, collection);
		}

		public void TestStatusFilter()
		{
			CreateTestDataForStageStatusDispositionTests();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter statusFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Status");
			ModuleTextFilter stageFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			#region All Stages
			statusFilter.Property = SupportIncidentLookups.Status.Open;
			AssertFilteredResult(filterBizO, "101", "108", "109", "114", "117");
			statusFilter.Property = SupportIncidentLookups.Status.Working;
			AssertFilteredResult(filterBizO, "102", "110", "111", "112", "115", "116");
			statusFilter.Property = SupportIncidentLookups.Status.NotClosed;
			AssertFilteredResult(filterBizO, "101", "102", "108", "109", "110", "111", "112", "114", "115", "116", "117", "120");
			statusFilter.Property = SupportIncidentLookups.Status.Closed;
			AssertFilteredResult(filterBizO, "103", "107", "113", "118", "119");
			stageFilter.Property = SupportIncidentFilterBusinessObject.AllStages;
			AssertFilteredResult(filterBizO, "103", "107", "113", "118", "119");
			statusFilter.Property = SupportIncidentLookups.Status.Open;
			AssertFilteredResult(filterBizO, "101", "108", "109", "114", "117");
			statusFilter.Property = SupportIncidentLookups.Status.Working;
			AssertFilteredResult(filterBizO, "102", "110", "111", "112", "115", "116");
			statusFilter.Property = SupportIncidentLookups.Status.NotClosed;
			AssertFilteredResult(filterBizO, "101", "102", "108", "109", "110", "111", "112", "114", "115", "116", "117", "120");
			#endregion
			#region Support
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Support;
			statusFilter.Property = SupportIncidentLookups.Status.Open;
			AssertFilteredResult(filterBizO, "101");
			statusFilter.Property = SupportIncidentLookups.Status.Working;
			AssertFilteredResult(filterBizO, "102");
			statusFilter.Property = SupportIncidentLookups.Status.NotClosed;
			AssertFilteredResult(filterBizO, "101", "102");
			statusFilter.Property = SupportIncidentLookups.Status.Closed;
			AssertFilteredResult(filterBizO, "103", "107");
			#endregion
			#region Defect
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Defect;
			statusFilter.Property = SupportIncidentLookups.Status.Open;
			AssertFilteredResult(filterBizO, "108", "109");
			statusFilter.Property = SupportIncidentLookups.Status.Working;
			AssertFilteredResult(filterBizO, "110", "111", "112");
			statusFilter.Property = SupportIncidentLookups.Status.NotClosed;
			AssertFilteredResult(filterBizO, "108", "109", "110", "111", "112");
			statusFilter.Property = SupportIncidentLookups.Status.Closed;
			AssertFilteredResult(filterBizO, "113");
			#endregion
			#region Feature
			stageFilter.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			statusFilter.Property = SupportIncidentLookups.Status.Open;
			AssertFilteredResult(filterBizO, "114", "117");
			statusFilter.Property = SupportIncidentLookups.Status.Working;
			AssertFilteredResult(filterBizO, "115", "116");
			statusFilter.Property = SupportIncidentLookups.Status.NotClosed;
			AssertFilteredResult(filterBizO, "114", "115", "116", "117", "120");
			statusFilter.Property = SupportIncidentLookups.Status.Closed;
			AssertFilteredResult(filterBizO, "118", "119");
			#endregion
		}

		public void TestDispositionFilter()
		{
			CreateTestDataForStageStatusDispositionTests();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter dispFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Disposition");
			dispFilter.Property = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			AssertFilteredResult(filterBizO, "101", "108", "114");
			ModuleTextFilter stageFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Support;
			AssertFilteredResult(filterBizO, "101");
		}

		public void TestERequestStatusFilter()
		{
			CreateTestDataForStageStatusDispositionTests();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter eRequestStatusFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "eRequest Status");
			eRequestStatusFilter.Property = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			AssertFilteredResult(filterBizO, "101", "108", "114");
			ModuleTextFilter stageFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Support;
			AssertFilteredResult(filterBizO, "101");
		}

		public void TestResolutionMethodFilter()
		{
			SupportIncidentForTest.New(Factory, "880").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment);
			SupportIncidentForTest.New(Factory, "881").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed)
				.SetValue(IncidentMainSchema.IM_ClosureResolution, SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved);

			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter resolutionMethodFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Resolution Method");
			resolutionMethodFilter.Property = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			AssertFilteredResult(filterBizO, "881");
		}

		public void TestDispositionFilter_WithOperator()
		{
			CreateTestDataForStageStatusDispositionTests();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter dispFilter = ActivateFilter<ModuleTextFilter>(filterBizO, "Disposition");
			dispFilter.Property = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			dispFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertFilteredResult(filterBizO, "102", "103", "107", "109", "110", "111", "112", "113", "115", "116", "117", "118", "119", "120");
			dispFilter.Property = SupportIncidentLookups.DispositionList.Constants.Closed.NotFeatureRequest;
			dispFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertFilteredResult(filterBizO, "101", "102", "103", "107", "108", "109", "110", "111", "112", "113", "114", "115", "116", "117", "118", "119", "120");
			dispFilter.Property = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			dispFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertFilteredResult(filterBizO, "101", "103", "107", "108", "109", "110", "112", "113", "114", "116", "117", "118", "119", "120");
			dispFilter.Property = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			dispFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertFilteredResult(filterBizO, "101", "102", "103", "107", "108", "110", "111", "112", "113", "114", "115", "116", "118", "119", "120");
			dispFilter.Property = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			dispFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertFilteredResult(filterBizO, "101", "102", "103", "108", "109", "110", "111", "112", "113", "114", "115", "116", "117", "118", "119", "120");
		}

		void CreateTestDataForStageStatusDispositionTests()
		{
			#region Support Stage (101 to 107)
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress);
			SupportIncidentForTest.New(Factory, "103").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract);
			SupportIncidentForTest.New(Factory, "107").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse);
			#endregion
			#region Defect Stage (108 to 113)
			SupportIncidentForTest.New(Factory, "108").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment);
			SupportIncidentForTest.New(Factory, "109").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction);
			SupportIncidentForTest.New(Factory, "110").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated);
			SupportIncidentForTest.New(Factory, "111").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress);
			SupportIncidentForTest.New(Factory, "112").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated);
			SupportIncidentForTest.New(Factory, "113").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered);
			#endregion
			#region Feature Request Stage (114 to 120)
			SupportIncidentForTest.New(Factory, "114").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment);
			SupportIncidentForTest.New(Factory, "115").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress);
			SupportIncidentForTest.New(Factory, "116").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated);
			SupportIncidentForTest.New(Factory, "117").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction);
			SupportIncidentForTest.New(Factory, "118").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncidentForTest.New(Factory, "119").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered);
			SupportIncidentForTest.New(Factory, "120").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Suspended).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred);
			#endregion
		}

		#endregion
		#region Country Code
		public void TestMainUNLOCOFilterContents()
		{
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleNkFilter countryFilter = (ModuleNkFilter)filterBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			AssertNull(countryFilter.FilterColumn);
			AssertEquals(typeof(LocationCollection), countryFilter.List.GetType());
			AssertEquals(ModuleIDs.Location, countryFilter.ModuleId);
			AssertEquals(FilterCategories.Locations, countryFilter.Category);
			EDISecurityCheckpoints.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = false;
			filterBizO = new SupportIncidentFilterBusinessObject();
			countryFilter = (ModuleNkFilter)filterBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			AssertEquals(FilterVisibility.AlwaysVisible, countryFilter.Visibility);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, countryFilter.DefaultProperty);
			AssertNotNull(countryFilter.PropertyValidation);
			AssertNoErrors(countryFilter.PropertyInfo);
			countryFilter.Property = "JP";
			string expectedError = string.Format(@"Your current security rights only allow you to view incidents relating to organizations based in your current login country ({0}).
If you think this is incorrect, please contact your administrator.", "AU");
			AssertHasError(countryFilter.PropertyInfo, expectedError);
			countryFilter.Property = "AUSYD";
			AssertNoErrors(countryFilter.PropertyInfo);
		}

		public void TestMainUNLOCOFilter()
		{
			#region Test Data
			SupportIncident incident1;
			SupportIncident incident2;
			SupportIncident incident3;
			SupportIncident incident4;
			OrgHeader clientEnt = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();
			clientEnt.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Australia;
			client1.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Australia;
			client2.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Australia;
			client3.OH_RL_NKClosestPort = Core.Constants.CountryCodes.UnitedKingdom;
			incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = clientEnt.PK;
			incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = client1.PK;
			incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = client2.PK;
			incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.IM_OH_Client = client3.PK;
			Factory.Save();
			#endregion
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			filterBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort].IsActive = true;
			((ModuleNkFilter)filterBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort]).Property = Core.Constants.CountryCodes.Australia;
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filterBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			AssertCollectionContains(incident3, collection);
			((ModuleNkFilter)filterBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort]).Property = Core.Constants.CountryCodes.UnitedKingdom;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(incident4, collection[0]);
		}

		#endregion
		#region OR-ing
		public void TestORingStages()
		{
			CreateTestDataForStageStatusDispositionTests();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter stageFilter1 = ActivateFilter<ModuleTextFilter>(filterBizO, "Stage");
			stageFilter1.Property = SupportIncidentCategoriesList.Codes.Support;
			stageFilter1.OrCategory = FilterOrCategory.Red;
			ModuleTextFilter stageFilter2 = (ModuleTextFilter)filterBizO.CreateDuplicateFor("Stage");
			stageFilter2.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			stageFilter2.OrCategory = FilterOrCategory.Red;
			stageFilter2.IsActive = true;
			AssertFilteredResult(filterBizO, "101", "102", "103", "107", "114", "115", "116", "117", "118", "119", "120");
			stageFilter1.Property = SupportIncidentCategoriesList.Codes.Defect;
			AssertFilteredResult(filterBizO, "108", "109", "110", "111", "112", "113", "114", "115", "116", "117", "118", "119", "120");
			stageFilter2.Property = SupportIncidentCategoriesList.Codes.Support;
			AssertFilteredResult(filterBizO, "101", "102", "103", "107", "108", "109", "110", "111", "112", "113");
		}

		public void TestORingStatusAndAssignedTos()
		{
			CreateTestDataForTestORingStatusAndAssignedTos();
			ZString staff1Code = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "IK").GS_Code;
			ZString staff2Code = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "PJW").GS_Code;
			ZString staff3Code = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "TB").GS_Code;
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			#region AssignedTo Filters
			ModuleNkFilter assignedToFilter1 = ActivateFilter<ModuleNkFilter>(filterBizO, "Assigned To");
			assignedToFilter1.Property = staff1Code;
			assignedToFilter1.OrCategory = FilterOrCategory.Blue;
			ModuleNkFilter assignedToFilter2 = (ModuleNkFilter)filterBizO.CreateDuplicateFor("Assigned To");
			assignedToFilter2.IsActive = true;
			assignedToFilter2.Property = staff2Code;
			assignedToFilter2.OrCategory = FilterOrCategory.Blue;
			ModuleNkFilter assignedToFilter3 = (ModuleNkFilter)filterBizO.CreateDuplicateFor("Assigned To");
			assignedToFilter3.IsActive = true;
			assignedToFilter3.Property = staff3Code;
			assignedToFilter3.OrCategory = FilterOrCategory.Blue;
			#endregion
			AssertFilteredResult(filterBizO, "102", "105", "107", "109", "111", "114");
			ActivateFilter<ModuleTextFilter>(filterBizO, "Stage").Property = SupportIncidentCategoriesList.Codes.Defect;
			AssertFilteredResult(filterBizO, "102", "107", "111");
			#region Disposition Filters
			ModuleTextFilter dispFilter1 = ActivateFilter<ModuleTextFilter>(filterBizO, "Disposition");
			dispFilter1.Property = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
			dispFilter1.OrCategory = FilterOrCategory.Red;
			ModuleTextFilter dispFilter2 = (ModuleTextFilter)filterBizO.CreateDuplicateFor("Disposition");
			dispFilter2.IsActive = true;
			dispFilter2.Property = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			dispFilter2.OrCategory = FilterOrCategory.Red;
			ModuleTextFilter dispFilter3 = (ModuleTextFilter)filterBizO.CreateDuplicateFor("Disposition");
			dispFilter3.IsActive = true;
			dispFilter3.Property = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			dispFilter3.OrCategory = FilterOrCategory.Red;
			ModuleTextFilter dispFilter4 = (ModuleTextFilter)filterBizO.CreateDuplicateFor("Disposition");
			dispFilter4.IsActive = true;
			dispFilter4.Property = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			dispFilter4.OrCategory = FilterOrCategory.Red;
			#endregion
			AssertFilteredResult(filterBizO, "102", "107", "111");
		}

		void CreateTestDataForTestORingStatusAndAssignedTos()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "IK";
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "PJW";
			GlbStaff staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "TB";
			#region Staff == "IK" (101 to 105)
			SupportIncidentForTest.New(Factory, "101").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff1.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "102").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff1.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction);
			SupportIncidentForTest.New(Factory, "103").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff1.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "104").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff1.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "105").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff1.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress);
			#endregion
			#region Staff == "PJW" (106 - 109)
			SupportIncidentForTest.New(Factory, "106").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff2.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "107").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff2.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress);
			SupportIncidentForTest.New(Factory, "108").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff2.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "109").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff2.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction);
			#endregion
			#region Staff == "TB" (110 - 114)
			SupportIncidentForTest.New(Factory, "110").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff3.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "111").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff3.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated);
			SupportIncidentForTest.New(Factory, "112").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff3.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "113").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff3.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "114").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, staff3.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Working).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated);
			#endregion
			#region Others (115 - 117)
			SupportIncidentForTest.New(Factory, "115").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, GlbStaff.CurrentUser.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			SupportIncidentForTest.New(Factory, "116").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support).SetValue(IncidentMainSchema.IM_GS_NKCustServiceContact, GlbStaff.CurrentUser.GS_Code).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved);
			SupportIncidentForTest.New(Factory, "117").SetValue(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Defect).SetValue(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Open).SetValue(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment).SetValue(IncidentMainSchema.IM_GS_NKAssignedToCurrent, ZString.Empty);
			#endregion
		}

		#endregion
		public void TestDispositionList()
		{
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair stage in new SupportIncidentCategoriesList())
			{
				AddExpectedDispositionList(expectedList, stage.Code, string.Empty);
			}

			AssertContainsExactElementsInAnyOrder("If no Stage and Status filters are selected - then return all possible values", expectedList.Cast<ICodeDescription>().Select(disposition => disposition.Code), filter.GetStatusDispositionList().Cast<ICodeDescription>().Select(disposition => disposition.Code));
			ModuleTextFilter statusFilter1 = (ModuleTextFilter)filter["Status"];
			statusFilter1.IsActive = true;
			statusFilter1.Property = SupportIncidentLookups.Status.Closed;
			ModuleTextFilter statusFilter2 = (ModuleTextFilter)filter.CreateDuplicateFor("Status");
			statusFilter2.IsActive = true;
			statusFilter2.Property = SupportIncidentLookups.Status.Working;
			statusFilter1.OrCategory = FilterOrCategory.Green;
			statusFilter2.OrCategory = FilterOrCategory.Green;
			expectedList.Clear();
			foreach (CodeDescriptionPair stage in new SupportIncidentCategoriesList())
			{
				AddExpectedDispositionList(expectedList, stage.Code, SupportIncidentLookups.Status.Closed);
				AddExpectedDispositionList(expectedList, stage.Code, SupportIncidentLookups.Status.Working);
			}

			AssertContainsExactElementsInAnyOrder("If only Status filters are selected - then return values for these Status in each possible Stage", expectedList.Cast<ICodeDescription>().Select(disposition => disposition.Code), filter.GetStatusDispositionList().Cast<ICodeDescription>().Select(disposition => disposition.Code));
			filter = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter stageFilter1 = (ModuleTextFilter)filter["Stage"];
			stageFilter1.IsActive = true;
			stageFilter1.Property = SupportIncidentCategoriesList.Codes.Defect;
			ModuleTextFilter stageFilter2 = (ModuleTextFilter)filter.CreateDuplicateFor("Stage");
			stageFilter2.IsActive = true;
			stageFilter2.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			stageFilter2.OrCategory = FilterOrCategory.Brown;
			stageFilter2.OrCategory = FilterOrCategory.Brown;
			expectedList.Clear();
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.Defect, string.Empty);
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.FeatureRequest, string.Empty);
			AssertContainsExactElementsInAnyOrder("If only Stage filters are selected - then return disp. for all statuses for this Stages", expectedList.Cast<ICodeDescription>().Select(disposition => disposition.Code), filter.GetStatusDispositionList().Cast<ICodeDescription>().Select(disposition => disposition.Code));
			filter = new SupportIncidentFilterBusinessObject();
			stageFilter1 = (ModuleTextFilter)filter["Stage"];
			stageFilter1.IsActive = true;
			stageFilter1.Property = SupportIncidentCategoriesList.Codes.Defect;
			stageFilter2 = (ModuleTextFilter)filter.CreateDuplicateFor("Stage");
			stageFilter2.IsActive = true;
			stageFilter2.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			stageFilter2.OrCategory = FilterOrCategory.Brown;
			stageFilter2.OrCategory = FilterOrCategory.Brown;
			statusFilter1 = (ModuleTextFilter)filter["Status"];
			statusFilter1.IsActive = true;
			statusFilter1.Property = SupportIncidentLookups.Status.Closed;
			statusFilter2 = (ModuleTextFilter)filter.CreateDuplicateFor("Status");
			statusFilter2.IsActive = true;
			statusFilter2.Property = SupportIncidentLookups.Status.Working;
			statusFilter1.OrCategory = FilterOrCategory.Green;
			statusFilter2.OrCategory = FilterOrCategory.Green;
			expectedList.Clear();
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed);
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Working);
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed);
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Working);
			AssertContainsExactElementsInAnyOrder("If only Stage filters are selected - then return disp. for all statuses for this Stages", expectedList.Cast<ICodeDescription>().Select(disposition => disposition.Code), filter.GetStatusDispositionList().Cast<ICodeDescription>().Select(disposition => disposition.Code));
		}

		public void TestDispositionList_FilteredByCriticalityAndProduct()
		{
			const string ALL = Enterprise.ProcessManagement.Business.CodeDescriptionBoolTreeNode.AllCode;
			var nonEDIProductsCollection = new SystemProductCollection();
			var product1 = nonEDIProductsCollection.AddNew();
			product1.Code = "SPH";
			product1.Description = "SPH desc";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nonEDIProductsCollection);
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "BB1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "BB2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR2", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR2", "ENT", "CC1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR2", "SPH");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR2", "SPH", "CC2");
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			var stageFilter = (ModuleTextFilter)filter["Stage"];
			stageFilter.IsActive = true;
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Defect;
			var statusFilter = (ModuleTextFilter)filter["Status"];
			statusFilter.IsActive = true;
			statusFilter.Property = SupportIncidentLookups.Status.Closed;
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("AA1", "AA1 depth 4");
			expectedList.AddPair("BB1", "BB1 depth 4");
			expectedList.AddPair("BB2", "BB2 depth 4");
			expectedList.AddPair("CC1", "CC1 depth 4");
			expectedList.AddPair("CC2", "CC2 depth 4");
			expectedList.AddPair("NRC", "No Response from Client");
			expectedList.AddPair("CWR", "Awaiting Client Response");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, "Resolved");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");
			expectedList.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			expectedList.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			AssertContainsExactElementsInAnyOrder("aaa", new CodeDescriptionComparer(), expectedList.Cast<ICodeDescription>(), filter.GetStatusDispositionList().Cast<ICodeDescription>());
			var criticalityFilter = (ModuleTextFilter)filter["Criticality"];
			criticalityFilter.IsActive = true;
			criticalityFilter.Property = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			var productFilter = (ModuleTextFilter)filter["Product"];
			productFilter.IsActive = true;
			productFilter.Property = "ENT";
			expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("AA1", "AA1 depth 4");
			expectedList.AddPair("NRC", "No Response from Client");
			expectedList.AddPair("CWR", "Awaiting Client Response");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, "Resolved");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");
			expectedList.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			expectedList.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expectedList.Cast<ICodeDescription>(), filter.GetStatusDispositionList().Cast<ICodeDescription>());
			criticalityFilter.Property = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("CC1", "CC1 depth 4");
			expectedList.AddPair("NRC", "No Response from Client");
			expectedList.AddPair("CWR", "Awaiting Client Response");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, "Resolved");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");
			expectedList.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			expectedList.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expectedList.Cast<ICodeDescription>(), filter.GetStatusDispositionList().Cast<ICodeDescription>());
			productFilter.Property = "SPH";
			expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("CC2", "CC2 depth 4");
			expectedList.AddPair("NRC", "No Response from Client");
			expectedList.AddPair("CWR", "Awaiting Client Response");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, "Resolved");
			expectedList.AddPair(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");
			expectedList.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			expectedList.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expectedList.Cast<ICodeDescription>(), filter.GetStatusDispositionList().Cast<ICodeDescription>());
		}

		public void TestDispositionList_ClosedDirectlyBySupport()
		{
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			ActivateFilter<ModuleTextFilter>(filter, "Status").Property = SupportIncidentLookups.Status.ClosedDirectlyInSupport;
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList();
			AddExpectedDispositionList(expectedList, SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Closed);
			AssertContainsExactElementsInAnyOrder("Should only include Support Closed dispositions", new CodeDescriptionComparer(), expectedList.Cast<ICodeDescription>(), filter.GetStatusDispositionList().Cast<ICodeDescription>());
		}

		public void TestGetResolutionMethodList_OnlyFromRegistry_WithSLVAndCLS()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA3");

			var inactiveNode = tree.Find("DEF", "CR1", "ENT", "AA2");
			inactiveNode.Bool = false;

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
			var parent = tree.Find("DEF", "CR1", "ENT");

			var filter = new SupportIncidentFilterBusinessObject();
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var expectedList = registryValue.Cast<IncidentClosureDisposition>().Where(x => x.ParentID == parent.PK).Cast<ICodeDescription>()
											.Append(new CodeDescriptionPair("SLV", "Resolved"))
											.Append(new CodeDescriptionPair("CLS", "Closed"));
			AssertContainsExactElementsInAnyOrder("Should only contain dispositions in the Registry", new CodeDescriptionComparer(), expectedList, filter.GetResolutionMethodList().Cast<ICodeDescription>());
		}

		void AddExpectedDispositionList(CodeDescriptionPairList list, string stage, string status)
		{
			foreach (ICodeDescription disp in ((SupportIncidentLookups)base.Lookups).GetStatusDispositionList(stage, status, "", ""))
			{
				list.AddOverwriteIfExists(disp);
			}
		}

		class CodeDescriptionComparer : IEqualityComparer<ICodeDescription>
		{
			public bool Equals(ICodeDescription x, ICodeDescription y)
			{
				return x != null && y != null && x.Code == y.Code && x.Description == y.Description;
			}

			public int GetHashCode(ICodeDescription obj)
			{
				return 0;
			}
		}

		#region TestStatusList
		public void TestStatusList_ClosedDirectlyInSupport()
		{
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			ModuleTextFilter stageFilter = ActivateFilter<ModuleTextFilter>(filter, "Stage");
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Defect;
			Assert(!filter.GetStatusList().ContainsCode(SupportIncidentLookups.Status.ClosedDirectlyInSupport));
			stageFilter.Property = SupportIncidentCategoriesList.Codes.FeatureRequest;
			Assert(!filter.GetStatusList().ContainsCode(SupportIncidentLookups.Status.ClosedDirectlyInSupport));
			stageFilter.Property = SupportIncidentCategoriesList.Codes.Support;
			Assert(filter.GetStatusList().ContainsCode(SupportIncidentLookups.Status.ClosedDirectlyInSupport));
		}

		protected override IReadOnlyList<CodeDescriptionPair> ExpectedStatusList
		{
			get
			{
				return base.ExpectedStatusList
					.Concat(new[] { new CodeDescriptionPair(SupportIncidentLookups.Status.ClosedDirectlyInSupport, "Closed Directly in Support") })
					.ToList()
					.AsReadOnly();
			}
		}

		#endregion
		[StressTest]
		public void TestEnterpriseCodeFilter()
		{
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "AAB";
			org3.LicenceEnterpriseCode = "BAB";
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org3.PK;
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.IM_OH_Client = org4.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO["Enterprise Code"];
			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			SupportIncidentCollection incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionContains(incident1, incidents);
			AssertCollectionNotContains(incident2, incidents);
			AssertCollectionNotContains(incident3, incidents);
			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionNotContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionNotContains(incident1, incidents);
			AssertCollectionNotContains(incident2, incidents);
			AssertCollectionNotContains(incident3, incidents);
			AssertCollectionContains(incident4, incidents);
			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			AssertCollectionNotContains(incident4, incidents);
		}

		[StressTest]
		public void TestEnterpriseIDFilter()
		{
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "AAB";
			org3.LicenceEnterpriseCode = "BAB";
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org3.PK;
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.IM_OH_Client = org4.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO["Enterprise ID"];
			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			SupportIncidentCollection incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionContains(incident1, incidents);
			AssertCollectionNotContains(incident2, incidents);
			AssertCollectionNotContains(incident3, incidents);
			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionNotContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionNotContains(incident1, incidents);
			AssertCollectionNotContains(incident2, incidents);
			AssertCollectionNotContains(incident3, incidents);
			AssertCollectionContains(incident4, incidents);
			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertCollectionContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			AssertCollectionNotContains(incident4, incidents);
		}

		public void TestDatabaseHostedLocationFilter()
		{
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org4.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "BBB";
			org3.LicenceEnterpriseCode = "CCC";
			org4.LicenceEnterpriseCode = "DDD";
			LicenceDatabase database1 = org1.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_HostedLocation = "SYD";
			LicenceDatabase database2 = org2.LicCompany.LicDatabases.AddNew();
			database2.FillWithValidTestData();
			database2.LD_HostedLocation = "CHI";
			LicenceDatabase database3 = org3.LicCompany.LicDatabases.AddNew();
			database3.FillWithValidTestData();
			database3.LD_HostedLocation = "CHI";
			LicenceDatabase database4 = org4.LicCompany.LicDatabases.AddNew();
			database4.FillWithValidTestData();
			database4.LD_HostedLocation = "NCW";
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_LD = database1.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			incident2.IM_LD = database2.PK;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org3.PK;
			incident3.IM_LD = database3.PK;
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.IM_OH_Client = org4.PK;
			incident4.IM_LD = database4.PK;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO["Database Hosted Location"];
			filter.Property = "SYD";
			filter.IsActive = true;
			SupportIncidentCollection incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertEquals(1, incidents.Count);
			AssertCollectionContains(incident1, incidents);
			filter.Property = "CHI";
			incidents.Load(filterBizO.Filter);
			AssertEquals(2, incidents.Count);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			filter.Property = "NCW";
			incidents.Load(filterBizO.Filter);
			AssertEquals(1, incidents.Count);
			AssertCollectionContains(incident4, incidents);
			filter.Property = "ALL";
			incidents.Load(filterBizO.Filter);
			AssertEquals(3, incidents.Count);
			AssertCollectionContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
		}

		public void TestContactNameEmailFilter()
		{
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "Contact1@cw1.com";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Contact 2";
			contact2.OC_Email = "Contact2@cw1.com";
			var database1 = org1.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_LD = database1.PK;
			incident1.IM_OC_Contact = contact1.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org1.PK;
			incident2.IM_LD = database1.PK;
			incident2.IM_OC_Contact = contact2.PK;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBizO["Contact Name"];
			filter.Property = "Contact 2";
			filter.IsActive = true;
			var incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertEquals(1, incidents.Count);
			AssertCollectionContains(incident2, incidents);
			filter.IsActive = false;
			filter = (ModuleTextFilter)filterBizO["Contact Email"];
			filter.Property = "contact1@cw1.com";
			filter.IsActive = true;
			incidents = new SupportIncidentCollection(Factory, filterBizO.Filter);
			incidents.Load();
			AssertEquals(1, incidents.Count);
			AssertCollectionContains(incident1, incidents);
		}

		public void TestSalesClientSizeFilter()
		{
			#region Prepare Test Data
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("AAA", "AAA Size");
			list.AddPair("BBB", "BBB Size");
			list.AddPair("CCC", "CCC Size");
			OrganisationsDataRegistry.Instance.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgMiscServ misc1 = Factory.NewWithValidTestData<OrgMiscServ>();
			misc1.OM_OH = client1.PK;
			misc1.OM_CMClientSize = "AAA";
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgMiscServ misc2 = Factory.NewWithValidTestData<OrgMiscServ>();
			misc2.OM_OH = client2.PK;
			misc2.OM_CMClientSize = "CCC";
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgMiscServ misc3 = Factory.NewWithValidTestData<OrgMiscServ>();
			misc3.OM_OH = client3.PK;
			misc3.OM_CMClientSize = "BBB";
			OrgHeader client4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgMiscServ misc4 = Factory.NewWithValidTestData<OrgMiscServ>();
			misc4.OM_OH = client4.PK;
			misc4.OM_CMClientSize = "BBB";
			SupportIncident supportIncident1 = Factory.New<SupportIncident>();
			supportIncident1.IM_OH_Client = client1.PK;
			SupportIncident supportIncident2 = Factory.New<SupportIncident>();
			supportIncident2.IM_OH_Client = client2.PK;
			SupportIncident supportIncident3 = Factory.New<SupportIncident>();
			supportIncident3.IM_OH_Client = client3.PK;
			SupportIncident supportIncident4 = Factory.New<SupportIncident>();
			supportIncident4.IM_OH_Client = client3.PK;
			Factory.Save();
			#endregion
			SupportIncidentCollection incidentCollection;
			SupportIncidentFilterBusinessObject filter = new SupportIncidentFilterBusinessObject();
			incidentCollection = new SupportIncidentCollection(Factory);
			ModuleTextFilter clientSizeFilter = (ModuleTextFilter)filter["Sales Client Size"];
			clientSizeFilter.IsActive = true;
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionContains(supportIncident3, incidentCollection);
			AssertCollectionContains(supportIncident4, incidentCollection);
			clientSizeFilter.Property = "AAA";
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			AssertCollectionNotContains(supportIncident4, incidentCollection);
			clientSizeFilter.Property = "BBB";
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionContains(supportIncident3, incidentCollection);
			AssertCollectionContains(supportIncident4, incidentCollection);
			clientSizeFilter.Property = "CCC";
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			AssertCollectionNotContains(supportIncident4, incidentCollection);
		}

		public void TestLanguageFilter()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Language = Core.SharedConstants.Languages.ChineseSimplified;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Language = Core.SharedConstants.Languages.EnglishAmerican;
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_Language = "EBS";
			Factory.Save();
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var collection = new SupportIncidentCollection(Factory);
			var filter = (ModuleTextFilter)filterBizO["Language"];
			filter.Property = filterBizO.LanguageList[Core.SharedConstants.Languages.ChineseSimplified].Code;
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionNotContains(incident2, collection);
			AssertCollectionNotContains(incident3, collection);
			filter.Property = filterBizO.LanguageList["ANY"].Code;
			collection.Load(filterBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			AssertCollectionContains(incident3, collection);
		}

		[TestDate(2010, 9, 26, 22, 32, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIncidentRaisedFilter()
		{
			var port1 = SetUpPortAndTimeZone("AA", new ZShort(600));
			port1.RL_RN_NKCountryCode = "AU";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = port1.Code;
			branch1.GB_Code = "BR1";
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";
			var port2 = SetUpPortAndTimeZone("BB", new ZShort(60));
			port2.RL_RN_NKCountryCode = "GB";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_RL_NKHomePort = port2.Code;
			branch2.GB_Code = "BR2";
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "DP2";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			staff.GS_LoginName = "Samuel";
			staff.GS_GB_HomeBranch = branch1.PK;
			Factory.Save();
			SupportIncident incident;
			SupportIncident incident2;
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
				AssertEquals("AAVVV", Env.CurrentBranch.NKUNLOCO);
				incident = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
				AssertEquals("26-Sep-10 22:32", incident.IM_SystemCreateTimeUtc.ToLongTimeString());
				AssertEquals("26-Sep-10 22:32", incident.IM_InstallDate.ToLongTimeString());
			}

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				TestUtcOffsetAttribute.Time = new TimeSpan(1, 0, 0);
				AssertEquals("BBVVV", Env.CurrentBranch.NKUNLOCO);
				incident2 = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
				AssertEquals("26-Sep-10 22:32", incident2.IM_SystemCreateTimeUtc.ToLongTimeString());
				AssertEquals("26-Sep-10 22:32", incident2.IM_InstallDate.ToLongTimeString());
			}

			TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
			var filter = new SupportIncidentFilterBusinessObject();
			var incidentRaisedFilter = filter["Incident Raised"] as ModuleDateFilter;
			incidentRaisedFilter.IsActive = true;
			incidentRaisedFilter.PropertySearch = "Date Range";
			var testDate = new ZDateTime(2010, 9, 28);
			var offset = -TestUtcOffsetAttribute.Time;
			incidentRaisedFilter.Property1 = testDate.Add(offset);
			incidentRaisedFilter.Property2 = testDate.Add(offset);
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filter.Filter);
			AssertCollectionContains(incident, collection);
			AssertCollectionContains(incident2, collection);
		}

		[TestDate(2010, 9, 26, 22, 32, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestResolvedTimeFilter()
		{
			var port1 = SetUpPortAndTimeZone("AA", new ZShort(600));
			port1.RL_RN_NKCountryCode = "AU";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = port1.Code;
			branch1.GB_Code = "BR1";
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";
			var port2 = SetUpPortAndTimeZone("BB", new ZShort(60));
			port2.RL_RN_NKCountryCode = "GB";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_RL_NKHomePort = port2.Code;
			branch2.GB_Code = "BR2";
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "DP2";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			staff.GS_LoginName = "Samuel";
			staff.GS_GB_HomeBranch = branch1.PK;
			Factory.Save();
			SupportIncident incident;
			SupportIncident incident2;
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
				AssertEquals("AAVVV", Env.CurrentBranch.NKUNLOCO);
				incident = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
				incident.IM_ResolveTimeUtc = incident.IM_SystemCreateTimeUtc;
				Factory.Save();
				AssertEquals("26-Sep-10 22:32", incident.IM_SystemCreateTimeUtc.ToLongTimeString());
				AssertEquals("26-Sep-10 22:32", incident.IM_ResolveTimeUtc.ToLongTimeString());
			}

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				TestUtcOffsetAttribute.Time = new TimeSpan(1, 0, 0);
				AssertEquals("BBVVV", Env.CurrentBranch.NKUNLOCO);
				incident2 = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
				incident2.IM_ResolveTimeUtc = incident.IM_SystemCreateTimeUtc;
				Factory.Save();
				AssertEquals("26-Sep-10 22:32", incident2.IM_SystemCreateTimeUtc.ToLongTimeString());
				AssertEquals("26-Sep-10 22:32", incident2.IM_ResolveTimeUtc.ToLongTimeString());
			}

			TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
			var filter = new SupportIncidentFilterBusinessObject();
			var incidentRaisedFilter = filter["Resolved Time"] as ModuleDateFilter;
			incidentRaisedFilter.IsActive = true;
			incidentRaisedFilter.PropertySearch = "Date Range";
			var testDate = new ZDateTime(2010, 9, 28);
			var offset = -TestUtcOffsetAttribute.Time;
			incidentRaisedFilter.Property1 = testDate.Add(offset);
			incidentRaisedFilter.Property2 = testDate.Add(offset);
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filter.Filter);
			AssertCollectionContains(incident, collection);
			AssertCollectionContains(incident2, collection);
		}

		RefUNLOCO SetUpPortAndTimeZone(ZString code, ZShort uTCOffset)
		{
			RefTimeZone timeZone = Factory.New<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = code + "T";
			timeZone.R2_OffsetMinutesFromUTC = uTCOffset;
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;
			timeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			timeZoneSet.R3_TimeZoneSetName = code;
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = code + "VVV";
			port.RL_R3 = timeZoneSet.PK;
			return port;
		}

		public void TestRelatedItemsFilterSection()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();

			var relatedIncidentFilter = filterObj["Related Incidents"];
			var relatedManagementGroupFilter = filterObj["Related Incident Management Groups"];
			var relatedOpportunitiesFilter = filterObj["Related Opportunities"];
			var relatedWorkItemFilter = filterObj["Related Work Items"];
			var relatedProjectFilter = filterObj["Related Project"];
			var relatedProjectsFilter = filterObj["Related Projects"];
			var relatedTriageNodesFilter = filterObj["Related Triage Nodes"];

			AssertNotNull(relatedIncidentFilter);
			AssertNotNull(relatedManagementGroupFilter);
			AssertNotNull(relatedOpportunitiesFilter);
			AssertNotNull(relatedWorkItemFilter);
			AssertNotNull(relatedProjectFilter);
			AssertNotNull(relatedProjectsFilter);
			AssertNotNull(relatedTriageNodesFilter);

			AssertEquals(relatedIncidentFilter.Category.ToString(), "Related Items");
			AssertEquals(relatedManagementGroupFilter.Category.ToString(), "Related Items");
			AssertEquals(relatedOpportunitiesFilter.Category.ToString(), "Related Items");
			AssertEquals(relatedWorkItemFilter.Category.ToString(), "Related Items");
			AssertEquals(relatedProjectFilter.Category.ToString(), "Related Items");
			AssertEquals(relatedProjectsFilter.Category.ToString(), "Related Items");
			AssertEquals(relatedTriageNodesFilter.Category.ToString(), "Related Items");
		}

		public void TestRelatedIncidentManagementGroupFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterObj["Related Incident Management Groups"];
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();
			var incident5 = Factory.NewWithValidTestData<SupportIncident>();
			var incident6 = Factory.NewWithValidTestData<SupportIncident>();
			var managementGroup1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var managementGroup2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			managementGroup1.ING_Description = "Group 1";
			managementGroup2.ING_Description = "Group 2";

			managementGroup1.RelatedItems.Add(incident1);
			managementGroup1.RelatedItems.Add(incident2);
			managementGroup1.RelatedItems.Add(incident3);
			managementGroup2.RelatedItems.Add(incident4);
			managementGroup2.RelatedItems.Add(incident5);
			managementGroup2.RelatedItems.Add(incident6);
			Factory.Save();

			AssertEquals(filter.ComparisonOperator_List.CodesAsString, "any match, all match, none match");

			var filterResults = new SupportIncidentCollection(Factory);
			filter.SelectedFilters.AddTextFilterStrip("Description", "Group 1");
			filter.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;
			filterResults.Load(filterObj.Filter);

			AssertEquals(3, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
			Assert("Filter should not return incident4", !filterResults.Contains(incident4));
			Assert("Filter should not return incident5", !filterResults.Contains(incident5));
			Assert("Filter should not return incident6", !filterResults.Contains(incident6));

			filter.SelectedFilters.ActiveModuleFilters.ElementAt(0).SetPossiblyCustomProperty("Property", "Group 2");
			filter.IsActive = true;
			filterResults.Load(filterObj.Filter);
			AssertEquals(3, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));
			Assert("Filter should return incident4", filterResults.Contains(incident4));
			Assert("Filter should return incident5", filterResults.Contains(incident5));
			Assert("Filter should return incident6", filterResults.Contains(incident6));
		}

		public void TestRelatedIncidentFilter()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();
			var incident5 = Factory.NewWithValidTestData<SupportIncident>();
			var incident6 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.RelatedItems.Add(incident5);
			incident2.RelatedItems.Add(incident5);
			incident3.RelatedItems.Add(incident6);
			incident4.RelatedItems.Add(incident6);
			incident5.IM_Product = "CW1";
			incident6.IM_Product = "ENT";
			Factory.Save();

			var filterObj = new SupportIncidentFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterObj["Related Incidents"];

			AssertEquals(filter.ComparisonOperator_List.CodesAsString, "any match, all match, none match");

			var filterResults = new SupportIncidentCollection(Factory);
			filter.IsActive = true;
			filter.SelectedFilters.AddTextFilterStrip("Product", "CW1");
			filter.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
			filterResults.Load(filterObj.Filter);

			AssertEquals(2, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));
			Assert("Filter should not return incident4", !filterResults.Contains(incident4));
			Assert("Filter should not return incident5", !filterResults.Contains(incident5));
			Assert("Filter should not return incident6", !filterResults.Contains(incident6));

			filter.SelectedFilters.ActiveModuleFilters.ElementAt(0).SetPossiblyCustomProperty("Property", "ENT");
			filter.IsActive = true;
			filterResults.Load(filterObj.Filter);

			AssertEquals(2, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
			Assert("Filter should return incident4", filterResults.Contains(incident4));
			Assert("Filter should not return incident5", !filterResults.Contains(incident5));
			Assert("Filter should not return incident6", !filterResults.Contains(incident6));
		}

		public void TestRelatedTriageNodesFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "triage 1";
			triage2.IMT_SupportDescription = "triage 2";

			incident1.IM_IMT_Triage = triage1.PK;
			incident2.IM_IMT_Triage = triage2.PK;
			Factory.Save();

			var filterResults = new SupportIncidentCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Related Triage Nodes"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Triage Description");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "triage 1";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbDescriptionFilter.Property = "Desc 2";
			filterResults.Load(filterObj.Filter);

			AssertEquals(3, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
		}

		public void TestRelatedOpportunitiesFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();
			var filter = (ModuleGuidPivotFilter)filterObj["Related Opportunities"];
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();
			var incident5 = Factory.NewWithValidTestData<SupportIncident>();
			var incident6 = Factory.NewWithValidTestData<SupportIncident>();
			var opportunity1 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opportunity1.P8_OpportunityDescription = "Opp 1";
			opportunity2.P8_OpportunityDescription = "Opp 2";

			incident1.RelatedItems.Add(opportunity1);
			incident2.RelatedItems.Add(opportunity1);
			incident3.RelatedItems.Add(opportunity1);
			incident4.RelatedItems.Add(opportunity2);
			incident5.RelatedItems.Add(opportunity2);
			incident6.RelatedItems.Add(opportunity2);
			Factory.Save();

			AssertEquals(filter.ComparisonOperator_List.CodesAsString, "any match, all match, none match");

			var filterResults = new SupportIncidentCollection(Factory);
			filter.SelectedFilters.AddTextFilterStrip("Description", "Opp 1");
			filter.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;
			filterResults.Load(filterObj.Filter);

			AssertEquals(3, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
			Assert("Filter should not return incident4", !filterResults.Contains(incident4));
			Assert("Filter should not return incident5", !filterResults.Contains(incident5));
			Assert("Filter should not return incident6", !filterResults.Contains(incident6));

			filter.SelectedFilters.ActiveModuleFilters.ElementAt(0).SetPossiblyCustomProperty("Property", "Opp 2");
			filter.IsActive = true;
			filterResults.Load(filterObj.Filter);

			AssertEquals(3, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));
			Assert("Filter should return incident4", filterResults.Contains(incident4));
			Assert("Filter should return incident5", filterResults.Contains(incident5));
			Assert("Filter should return incident6", filterResults.Contains(incident6));
		}

		#region Participant Filters

		public void TestContactParticipantOfIncidentFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "Contact 1";
			incident1.EConversation.Conversation.RelatedParties.AddNewParticipant(orgContact);
			Factory.Save();

			var filterResults = new SupportIncidentCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Contact Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Name");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "Contact 1";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbDescriptionFilter.Property = "Contact 1";
			filterResults.Load(filterObj.Filter);

			AssertEquals(2, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
		}

		public void TestStaffParticipantOfIncidentFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "CG";
			incident1.EConversation.Conversation.RelatedParties.AddNewParticipant(staff);
			Factory.Save();

			var filterResults = new SupportIncidentCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Staff Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Code");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "CG";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbDescriptionFilter.Property = "CG";
			filterResults.Load(filterObj.Filter);

			AssertEquals(2, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
		}

		public void TestOrganizationParticipantsOfIncidentFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DEMOSYD";
			var participant = incident1.EConversation.Conversation.RelatedParties.AddNewParticipant(orgHeader);
			participant.RelatedPartyTypeName = "Organization";
			AssertEquals(false, participant.JCP_IsSubscribed);
			Factory.Save();

			var filterResults = new SupportIncidentCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Organization Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Code");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "DEMOSYD";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbDescriptionFilter.Property = "DEMOSYD";
			filterResults.Load(filterObj.Filter);

			AssertEquals(2, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
		}

		public void TestGroupParticipantOfIncidentFilter()
		{
			var filterObj = new SupportIncidentFilterBusinessObject();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			var glbGroup = Factory.NewWithValidTestData<GlbGroup>();
			glbGroup.GG_Code = "PMG55";
			incident1.EConversation.Conversation.RelatedParties.AddNewParticipant(glbGroup);
			Factory.Save();

			var filterResults = new SupportIncidentCollection(Factory);
			var filter = (ModuleGuidForeignCollectionFilter)filterObj["Group Participants"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.IsActive = true;

			var dbDescriptionFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Code");
			dbDescriptionFilter.IsActive = true;
			dbDescriptionFilter.Property = "PMG55";

			filterResults.Load(filterObj.Filter);

			AssertEquals(1, filterResults.Count);
			Assert("Filter should return incident1", filterResults.Contains(incident1));
			Assert("Filter should not return incident2", !filterResults.Contains(incident2));
			Assert("Filter should not return incident3", !filterResults.Contains(incident3));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			dbDescriptionFilter.Property = "PMG55";
			filterResults.Load(filterObj.Filter);

			AssertEquals(2, filterResults.Count);
			Assert("Filter should not return incident1", !filterResults.Contains(incident1));
			Assert("Filter should return incident2", filterResults.Contains(incident2));
			Assert("Filter should return incident3", filterResults.Contains(incident3));
		}

		public void TestHasEmailParticipantOfIncidentFilter()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			incident1.EConversation.Conversation.RelatedParties.AddNewParticipant("417666@hh.com");
			Factory.Save();

			var incidentFilter = new SupportIncidentFilterBusinessObject();
			var filter = (ModuleTextFilter)incidentFilter["Has Email Participant"];
			var incidentCollection = new SupportIncidentCollection(Factory);

			filter.Property = "417666@hh.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			incidentCollection.Load(incidentFilter.Filter);

			Assert("Should contain incident1", incidentCollection.Contains(incident1.PK));
			Assert("Should not contain incident2", !incidentCollection.Contains(incident2.PK));
			Assert("Should only contain incident3", !incidentCollection.Contains(incident3.PK));

			filter.Property = "417666@hh.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			incidentCollection.Load(incidentFilter.Filter);

			Assert("Should contain incident1", incidentCollection.Contains(incident1.PK));
			Assert("Should not contain incident2", !incidentCollection.Contains(incident2.PK));
			Assert("Should not contain incident3", !incidentCollection.Contains(incident3.PK));
		}

		#endregion

		#region Task Filters
		public void TestTaskAssignedToFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task2 = incident2.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task3 = incident2.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			ProcessTask task4 = incident2.WorkflowItems.AddNew();
			task4.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask assignedTask = incident3.WorkflowItems.AddNew();
			assignedTask.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filterBizO.Filter);
			AssertEquals(3, collection.Count);
			((ModuleNkFilter)filterBizO["Task Assigned To"]).Property = staff1.GS_Code;
			((ModuleNkFilter)filterBizO["Task Assigned To"]).IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
		}

		public void TestTaskAssignedTeamFilter()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GG_AssignedGroup = group1.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task2 = incident2.WorkflowItems.AddNew();
			task2.P9_GG_AssignedGroup = group2.PK;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task3 = incident2.WorkflowItems.AddNew();
			task3.P9_GG_AssignedGroup = group2.PK;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			ProcessTask task4 = incident2.WorkflowItems.AddNew();
			task4.P9_GG_AssignedGroup = group1.PK;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask assignedTask = incident3.WorkflowItems.AddNew();
			assignedTask.P9_GG_AssignedGroup = group2.PK;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filterBizO.Filter);
			AssertEquals(3, collection.Count);
			((ModuleGuidFilter)filterBizO["Task Assigned Team"]).Property = group2.PK;
			((ModuleGuidFilter)filterBizO["Task Assigned Team"]).IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(incident2, collection);
			AssertCollectionContains(incident3, collection);
		}

		public void TestCurrentTaskFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST2";
			SupportIncident incident1 = Factory.New<SupportIncident>();
			ProcessTask task1 = incident1.WorkflowItems.Tasks.AddNew();
			task1.P9_GS_NKAssignedStaffMember = "ST1";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			SupportIncident incident2 = Factory.New<SupportIncident>();
			ProcessTask task2 = incident2.WorkflowItems.Tasks.AddNew();
			task2.P9_GS_NKAssignedStaffMember = "ST1";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			SupportIncident incident3 = Factory.New<SupportIncident>();
			ProcessTask task3 = incident3.WorkflowItems.Tasks.AddNew();
			task3.P9_GS_NKAssignedStaffMember = "ST2";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			SupportIncident incident4 = Factory.New<SupportIncident>();
			ProcessTask task4 = incident4.WorkflowItems.Tasks.AddNew();
			task4.P9_Sequence = 1;
			task4.P9_GS_NKAssignedStaffMember = "ST2";
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask task4b = incident4.WorkflowItems.Tasks.AddNew();
			task4b.P9_Sequence = 1;
			task4b.P9_GS_NKAssignedStaffMember = "ST1";
			task4b.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			SupportIncident incident5 = Factory.New<SupportIncident>();
			ProcessTask task5 = incident5.WorkflowItems.Tasks.AddNew();
			task5.P9_GS_NKAssignedStaffMember = "ST1";
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleNkFilter assignedToFilter = (ModuleNkFilter)filterBizO["Task Assigned To"];
			assignedToFilter.IsActive = true;
			assignedToFilter.Property = "ST1";
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filterBizO.Filter);
			AssertEquals(4, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			AssertCollectionContains(incident4, collection);
			AssertCollectionContains(incident5, collection);
			assignedToFilter.Property = "ST2";
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(incident3, collection);
			AssertCollectionContains(incident4, collection);
			ModuleFlagsFilter currentTaskOnlyFilter = (ModuleFlagsFilter)filterBizO["Current Task Only"];
			currentTaskOnlyFilter.IsActive = true;
			currentTaskOnlyFilter.Property0 = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident4, collection);
			assignedToFilter.Property = "ST1";
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
		}

		public void TestCurentTaskFilter_Startable()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INC");
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST2";
			var incident1 = Factory.New<SupportIncident>();
			var jobHeader1 = helper.GetJobHeaderForParent(incident1, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader1, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(jobHeader1, "Workflow 2");
			helper.CreateLink(workflow1, workflow2);
			var task1 = incident1.WorkflowItems.Tasks.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Sequence = 10;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task2 = incident1.WorkflowItems.Tasks.AddNew();
			task2.P9_FH_ProcessHeader = workflow1.PK;
			task2.P9_GS_NKAssignedStaffMember = "ST1";
			task2.P9_Sequence = 20;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var incident2 = Factory.New<SupportIncident>();
			var jobHeader2 = helper.GetJobHeaderForParent(incident2, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow3 = helper.CreateWorkflow(jobHeader2, "Workflow 3");
			var task3 = incident2.WorkflowItems.Tasks.AddNew();
			task3.P9_FH_ProcessHeader = workflow3.PK;
			task3.P9_GS_NKAssignedStaffMember = "ST1";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			var assignedToFilter = (ModuleNkFilter)filterBizO["Task Assigned To"];
			assignedToFilter.IsActive = true;
			assignedToFilter.Property = "ST1";
			var currentTaskOnlyFilter = (ModuleFlagsFilter)filterBizO["Current Task Only"];
			currentTaskOnlyFilter.IsActive = true;
			currentTaskOnlyFilter.Property0 = true;
			var collection = new SupportIncidentCollection(Factory);
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident2, collection);
		}

		public void TestTaskStatusFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task11 = incident1.WorkflowItems.AddNew();
			task11.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask task12 = incident1.WorkflowItems.AddNew();
			task12.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task21 = incident2.WorkflowItems.AddNew();
			task21.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task22 = incident2.WorkflowItems.AddNew();
			task22.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask task23 = incident2.WorkflowItems.AddNew();
			task23.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task23.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task31 = incident3.WorkflowItems.AddNew();
			task31.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filterBizO.Filter);
			AssertEquals(3, collection.Count);
			((ModuleTextFilter)filterBizO["Task Status"]).Property = "NCM";
			((ModuleTextFilter)filterBizO["Task Status"]).IsActive = true;
			ModuleFlagsFilter currentTaskOnlyFilter = (ModuleFlagsFilter)filterBizO["Current Task Only"];
			currentTaskOnlyFilter.IsActive = true;
			currentTaskOnlyFilter.Property0 = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
			((ModuleTextFilter)filterBizO["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Working;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident1, collection);
			((ModuleTextFilter)filterBizO["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Cancelled;
			currentTaskOnlyFilter.IsActive = false;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident2, collection);
		}

		public void TestTaskCapabilityFilter()
		{
			GlbCapability capability1 = Factory.NewWithValidTestData<GlbCapability>();
			GlbCapability capability2 = Factory.NewWithValidTestData<GlbCapability>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task11 = incident1.WorkflowItems.AddNew();
			task11.P9_GS_NKAssignedStaffMember = "";
			task11.P9_G4_RequiredCapability = capability1.PK;
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task21 = incident2.WorkflowItems.AddNew();
			task21.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task21.P9_G4_RequiredCapability = capability1.PK;
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task31 = incident3.WorkflowItems.AddNew();
			task31.P9_GS_NKAssignedStaffMember = "";
			task31.P9_G4_RequiredCapability = capability1.PK;
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task32 = incident3.WorkflowItems.AddNew();
			task32.P9_GS_NKAssignedStaffMember = "";
			task32.P9_G4_RequiredCapability = capability2.PK;
			task32.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task41 = incident4.WorkflowItems.AddNew();
			task41.P9_GS_NKAssignedStaffMember = "";
			task41.P9_G4_RequiredCapability = capability1.PK;
			task41.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask task42 = incident4.WorkflowItems.AddNew();
			task42.P9_GS_NKAssignedStaffMember = "";
			task42.P9_G4_RequiredCapability = capability2.PK;
			task42.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident5 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			ModuleGuidFilter capabilityFilter = ((ModuleGuidFilter)filterBizO["Capability Only (Current Task)"]);
			capabilityFilter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(5, collection.Count);
			AssertCollectionContains("Pre-condition", incident1, collection);
			AssertCollectionContains("Pre-condition", incident2, collection);
			AssertCollectionContains("Pre-condition", incident3, collection);
			AssertCollectionContains("Pre-condition", incident4, collection);
			AssertCollectionContains("Pre-condition", incident5, collection);
			capabilityFilter.Property = capability1.PK;
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains("Incident 1 - current task is assigned to capability 1", incident1, collection);
			AssertCollectionNotContains("Incident 2 - has staff assigned", incident2, collection);
			AssertCollectionNotContains("Incident 3 - current task is assigned to capability 2", incident3, collection);
			AssertCollectionContains("Incident 4 - current task is assigned to capability 1", incident4, collection);
			AssertCollectionNotContains("Incident 5 - no task", incident5, collection);
			capabilityFilter.Property = capability2.PK;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionNotContains("Incident 1 - current task is assigned to capability 1", incident1, collection);
			AssertCollectionNotContains("Incident 2 - has staff assigned", incident2, collection);
			AssertCollectionContains("Incident 3 - current task is assigned to capability 2", incident3, collection);
			AssertCollectionNotContains("Incident 4 - current task is assigned to capability 1", incident4, collection);
			AssertCollectionNotContains("Incident 5 - no task", incident5, collection);
		}

		public void TestTaskStaffCapabilityFilter()
		{
			GlbCapability capability1 = Factory.NewWithValidTestData<GlbCapability>();
			GlbCapability capability2 = Factory.NewWithValidTestData<GlbCapability>();
			GlbCapability capability3 = Factory.NewWithValidTestData<GlbCapability>();
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.Capabilities.Add(capability2);
			staff2.Capabilities.Add(capability3);
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task11 = incident1.WorkflowItems.AddNew();
			task11.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task11.P9_G4_RequiredCapability = capability1.PK;
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task21 = incident2.WorkflowItems.AddNew();
			task21.P9_GS_NKAssignedStaffMember = "";
			task21.P9_G4_RequiredCapability = capability1.PK;
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task31 = incident3.WorkflowItems.AddNew();
			task31.P9_GS_NKAssignedStaffMember = "";
			task31.P9_G4_RequiredCapability = capability1.PK;
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task32 = incident3.WorkflowItems.AddNew();
			task32.P9_GS_NKAssignedStaffMember = "";
			task32.P9_G4_RequiredCapability = capability2.PK;
			task32.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task41 = incident4.WorkflowItems.AddNew();
			task41.P9_GS_NKAssignedStaffMember = "";
			task41.P9_G4_RequiredCapability = capability3.PK;
			task41.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			ModuleNkFilter staffCapabilityFilter = ((ModuleNkFilter)filterBizO["Staff Capability Only (Current Task)"]);
			staffCapabilityFilter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertEquals(4, collection.Count);
			AssertCollectionContains("Pre-condition", incident1, collection);
			AssertCollectionContains("Pre-condition", incident2, collection);
			AssertCollectionContains("Pre-condition", incident3, collection);
			AssertCollectionContains("Pre-condition", incident4, collection);
			staffCapabilityFilter.Property = staff1.GS_Code;
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionNotContains("Incident 1 - has staff assigned", incident1, collection);
			AssertCollectionContains("Incident 2 - current task is assigned to capability 1", incident2, collection);
			AssertCollectionContains("Incident 3 - current task is assigned to capability 2", incident3, collection);
			AssertCollectionNotContains("Incident 4 - current task is assigned to capability 3", incident4, collection);
			staffCapabilityFilter.Property = staff2.GS_Code;
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionNotContains("Incident 1 - has staff assigned", incident1, collection);
			AssertCollectionNotContains("Incident 2 - current task is assigned to capability 1", incident2, collection);
			AssertCollectionContains("Incident 3 - current task is assigned to capability 2", incident3, collection);
			AssertCollectionContains("Incident 4 - current task is assigned to capability 3", incident4, collection);
		}

		#endregion
		#region Date Filters
		public void TestCloseInSupportDateFilter()
		{
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			ModuleDateFilter filter = ActivateFilter<ModuleDateFilter>(filterBizO, "Support Closed");
			filter.PropertySearch = "Has Date";
			SupportIncidentCollection collection = new SupportIncidentCollection(Factory);
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);
			incident1.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(incident1, collection);
			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ThirdPartySystemProblem, "");
			Factory.Save();
			collection.Load(filter.Query);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(incident1, collection);
			AssertCollectionContains(incident2, collection);
		}

		public void TestEstimateQuoteDateFilter()
		{
			var incident1 = SupportIncidentForTest.New(Factory, "10001");
			var incident2 = SupportIncidentForTest.New(Factory, "10002");
			var incident3 = SupportIncidentForTest.New(Factory, "10003");
			var incident4 = SupportIncidentForTest.New(Factory, "10004");
			var incident5 = SupportIncidentForTest.New(Factory, "10005");
			var incident6 = SupportIncidentForTest.New(Factory, "10006");
			incident1.Estimate.CIE_EstimateSentDateLocal = new ZDateTime(2000, 1, 1);
			incident1.Estimate.CIE_EstimateExpiryDateLocal = ZDateTime.Empty;
			incident2.Estimate.CIE_EstimateExpiryDateLocal = new ZDateTime(2000, 1, 2);
			incident3.Estimate.CIE_QuoteRequestedLocal = new ZDateTime(2000, 1, 3);
			incident4.Quote.CIQ_QuoteSentDateLocal = new ZDateTime(2000, 1, 4);
			incident4.Quote.CIQ_QuoteExpiryDateLocal = ZDateTime.Empty;
			incident5.Quote.CIQ_QuoteExpiryDateLocal = new ZDateTime(2000, 1, 5);
			incident6.Quote.CIQ_QuoteAcceptedDateLocal = new ZDateTime(2000, 1, 6);
			Factory.Save();
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var incidents = new Dictionary<string, SupportIncident>();
			incidents["Estimate Sent Date"] = incident1;
			incidents["Estimate Expiry Date"] = incident2;
			incidents["Quote Requested Date"] = incident3;
			incidents["Quote Sent Date"] = incident4;
			incidents["Quote Expiry Date"] = incident5;
			incidents["Quote Accepted Date"] = incident6;
			var idx = 10001;
			foreach (var entry in incidents)
			{
				var filter = ActivateFilter<ModuleDateFilter>(filterBizO, entry.Key);
				filter.PropertySearch = "Has Date";
				AssertFilteredResult(filterBizO, $"{idx++}");
				filter.IsActive = false;
			}
		}

		#endregion
		#region Licence Filters
		public void TestGoLiveCompleteFilter()
		{
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_OH = org1.PK;
			var licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			licence1.LA_SiteLiveDate = new ZDateTime(2011, 1, 1);
			licence1.LA_LD = database.PK;
			licence1.LA_LC = company1.PK;
			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_LD = database.PK;
			clientCompany1.LCC_OH = org1.PK;
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_LCC = clientCompany1.PK;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var company2 = Factory.NewWithValidTestData<LicenceCompany>();
			company2.LC_OH = org2.PK;
			var licence2 = Factory.NewWithValidTestData<LicenceHeader>();
			licence2.LA_SiteLiveDate = new ZDateTime(2012, 2, 2);
			licence2.LA_LD = database.PK;
			licence2.LA_LC = company2.PK;
			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_LD = database.PK;
			clientCompany2.LCC_OH = org2.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			incident2.IM_LCC = clientCompany2.PK;
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var company3 = Factory.NewWithValidTestData<LicenceCompany>();
			company3.LC_OH = org3.PK;
			var licence3 = Factory.NewWithValidTestData<LicenceHeader>();
			licence3.LA_SiteLiveDate = new ZDateTime(2013, 3, 3);
			licence3.LA_LD = database.PK;
			licence3.LA_LC = company3.PK;
			var clientCompany3 = Factory.New<ClientCompany>();
			clientCompany3.LCC_Code = "CCC";
			clientCompany3.LCC_LD = database.PK;
			clientCompany3.LCC_OH = org3.PK;
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org3.PK;
			incident3.IM_LCC = clientCompany3.PK;
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.IM_LCC = Factory.NewWithValidTestData<ClientCompany>().PK;
			Factory.Save();
			SupportIncidentFilterBusinessObject filterBizO = new SupportIncidentFilterBusinessObject();
			SupportIncidentCollection incidents = new SupportIncidentCollection(Factory);
			ModuleDateFilter filter = (ModuleDateFilter)filterBizO["Go-Live Complete"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2010, 9, 10);
			filter.Property2 = ZDateTime.Empty;
			incidents.Load(filterBizO.Filter);
			AssertEquals(3, incidents.Count);
			AssertCollectionContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			AssertCollectionNotContains(incident4, incidents);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2011, 9, 10);
			filter.Property2 = new ZDateTime(2012, 9, 10);
			incidents.Load(filterBizO.Filter);
			AssertEquals(1, incidents.Count);
			AssertCollectionNotContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionNotContains(incident3, incidents);
			AssertCollectionNotContains(incident4, incidents);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2011, 9, 10);
			filter.Property2 = ZDateTime.Empty;
			incidents.Load(filterBizO.Filter);
			AssertCollectionNotContains(incident1, incidents);
			AssertCollectionContains(incident2, incidents);
			AssertCollectionContains(incident3, incidents);
			AssertCollectionNotContains(incident4, incidents);
		}

		public void TestGoLiveCompleteFilterWithExtendedLogic()
		{
			//1.create incident without org
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = ZGuid.Empty;
			Factory.Save();
			AssertGoLiveCompleteFilter(null, ZDate.Today, ZDate.Today);
			//2.create incident with org(IM_OH_Client = OH_PK) + LicCompany(LC_OH = OH_PK) + LicHeader(LA_LC = LC_PK and LA_LD = IM_LD)
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var licCompany2 = Factory.NewWithValidTestData<LicenceCompany>();
			var licHeader2 = Factory.NewWithValidTestData<LicenceHeader>();
			var licDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			incident2.IM_OH_Client = org2.PK;
			licCompany2.LC_OH = org2.PK;
			licHeader2.LA_LC = licCompany2.PK;
			licHeader2.LA_LD = incident2.IM_LD = licDatabase2.PK;
			licHeader2.LA_SiteLiveDate = new ZDate(2017, 1, 3);
			Factory.Save();
			AssertGoLiveCompleteFilter(incident2, new ZDate(2017, 1, 3), new ZDate(2017, 1, 3));
			//3.create incident with org(IM_OH_Client = OH_PK) + LicCompany(LC_OH = OH_PK) + LicHeader(LA_LC = LC_PK and IM_LD = null)
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var licCompany3 = Factory.NewWithValidTestData<LicenceCompany>();
			var licHeader3 = Factory.NewWithValidTestData<LicenceHeader>();
			incident3.IM_OH_Client = org3.PK;
			licCompany3.LC_OH = org3.PK;
			licHeader3.LA_LC = licCompany3.PK;
			incident3.IM_LD = ZGuid.Empty;
			licHeader3.LA_SiteLiveDate = new ZDate(2017, 1, 6);
			Factory.Save();
			AssertGoLiveCompleteFilter(incident3, new ZDate(2017, 1, 6), new ZDate(2017, 1, 6));
			//4.create incident with org(IM_OH_Client = OH_PK) + LicenceDatabase(IM_LD = LD_PK) + LicHeader(LA_LD = LD_PK, LA_IsActive = 1)
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var licHeader4 = Factory.NewWithValidTestData<LicenceHeader>();
			var licDatabase4 = Factory.NewWithValidTestData<LicenceDatabase>();
			incident4.IM_OH_Client = org4.PK;
			incident4.IM_LD = licDatabase4.PK;
			licHeader4.LA_LD = licDatabase4.PK;
			licHeader4.LA_IsActive = true;
			licHeader4.LA_SiteLiveDate = new ZDate(2017, 1, 9);
			Factory.Save();
			AssertGoLiveCompleteFilter(incident4, new ZDate(2017, 1, 9), new ZDate(2017, 1, 9));
		}

		void AssertGoLiveCompleteFilter(SupportIncident incident, ZDate startDate, ZDate endDate)
		{
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var incidents = new SupportIncidentCollection(Factory);
			var filter = (ModuleDateFilter)filterBizO["Go-Live Complete"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = startDate;
			filter.Property2 = endDate;
			incidents.Load(filterBizO.Filter);
			if (incident == null)
			{
				AssertEquals(0, incidents.Count);
			}
			else
			{
				AssertEquals(1, incidents.Count);
				AssertCollectionContains(incident, incidents);
			}
		}

		public void TestGoLiveCompleteFilterWithIndex()
		{
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var licCompany2 = Factory.NewWithValidTestData<LicenceCompany>();
			var licHeader2 = Factory.NewWithValidTestData<LicenceHeader>();
			var licDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			incident2.IM_OH_Client = org2.PK;
			licCompany2.LC_OH = org2.PK;
			licHeader2.LA_LC = licCompany2.PK;
			licHeader2.LA_LD = incident2.IM_LD = licDatabase2.PK;
			licHeader2.LA_SiteLiveDate = new ZDate(2017, 1, 3);
			Factory.Save();
			AssertGoLiveCompleteFilter(incident2, new ZDate(2017, 1, 3), new ZDate(2017, 1, 3));
			var filterBizO = new SupportIncidentFilterBusinessObject();
			var filter = (ModuleDateFilter)filterBizO["Go-Live Complete"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = new ZDate(2017, 1, 3);
			filter.Property2 = new ZDate(2017, 1, 3);
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var newFactory = Factory.CreateNewFactory();
				newFactory.Load<SupportIncident>(filterBizO.Filter);
				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("Make the SQL clean & readable"));
				var planalyser = new QueryPlanalyzer(queryPlans?.Item2.Last());
				AssertCollectionContains("Index seeks on FK_RC__IM_OH_Client__IM_LD", "FK_RC__IM_OH_Client__IM_LD", planalyser.IndexSeeks.Select(x => x.IndexName));
			}
		}

		public void TestGoLiveDateFilterMultipleLicenceHeadersShouldMatchWithEarliest()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			var licHeader1 = Factory.NewWithValidTestData<LicenceHeader>();
			var licHeader2 = Factory.NewWithValidTestData<LicenceHeader>();
			var licHeader3 = Factory.NewWithValidTestData<LicenceHeader>();
			incident.IM_OH_Client = org.PK;
			licCompany.LC_OH = org.PK;
			licHeader1.LA_LC = licCompany.PK;
			licHeader1.LA_SiteLiveDate = ZDate.Empty;
			licHeader2.LA_LC = licCompany.PK;
			licHeader2.LA_SiteLiveDate = new ZDate(2020, 05, 01);
			licHeader3.LA_LC = licCompany.PK;
			licHeader3.LA_SiteLiveDate = new ZDate(2020, 07, 01);
			Factory.Save();
			AssertGoLiveCompleteFilter(incident, new ZDate(2020, 02, 01), new ZDate(2020, 06, 01));
			var licHeader4 = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader4.LA_LC = licCompany.PK;
			licHeader4.LA_SiteLiveDate = new ZDate(2020, 01, 01);
			Factory.Save();
			AssertGoLiveCompleteFilter(null, new ZDate(2020, 02, 01), new ZDate(2020, 06, 01));
		}

		public void TestLicenceDatabaseFilterMatch()
		{
			//test
			//var org1 = Factory.NewWithValidTestData<OrgHeader>();
			//org1.OH_FullName = "Test ld server name";
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_ReportedHostServerName = "Test ld server name";
			//var org2 = Factory.NewWithValidTestData<OrgHeader>();
			//org2.OH_FullName = "Test another ld server name";
			var anotherLicenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			anotherLicenceDatabase.LD_ReportedHostServerName = "Test another ld server name";
			var supportIncident1 = Factory.New<SupportIncident>();
			supportIncident1.IM_Product = "ENT";
			supportIncident1.IM_Status = SupportIncidentLookups.Status.Closed;
			supportIncident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			supportIncident1.IM_Description = "Something wrong is happening with this World!";
			supportIncident1.ResolutionNoteText = "Works as designed";
			supportIncident1.IM_LD = licenceDatabase.PK;
			var supportIncident2 = Factory.New<SupportIncident>();
			supportIncident2.IM_Product = "ENT";
			supportIncident2.IM_Status = SupportIncidentLookups.Status.Closed;
			supportIncident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			supportIncident2.IM_Description = "Something REALLY wrong is happening with this World!";
			supportIncident2.ResolutionNoteText = "Client has been moved to another world";
			supportIncident2.IM_LD = licenceDatabase.PK;
			var supportIncident3 = Factory.New<SupportIncident>();
			supportIncident3.IM_Product = "ENT";
			supportIncident3.IM_Status = SupportIncidentLookups.Status.Working;
			supportIncident3.IM_Description = "Who am I?";
			supportIncident3.IM_LD = anotherLicenceDatabase.PK;
			Factory.Save();
			var filter = new SupportIncidentFilterBusinessObject();
			var incidentCollection = new SupportIncidentCollection(Factory);
			CreateLicenceDatabaseFilter(filter, "Test ld server name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			((ModuleFlagsFilter)filter["Client Management Group"]).Property0 = true;
			((ModuleFlagsFilter)filter["Client Management Group"]).IsActive = true;
			CreateLicenceDatabaseFilter(filter, "Test ld server name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionContains(supportIncident1, incidentCollection);
			AssertCollectionContains(supportIncident2, incidentCollection);
			AssertCollectionNotContains(supportIncident3, incidentCollection);
			((ModuleFlagsFilter)filter["Client Management Group"]).Property0 = true;
			CreateLicenceDatabaseFilter(filter, "Test another ld server name");
			incidentCollection.Load(filter.Filter);
			AssertCollectionNotContains(supportIncident1, incidentCollection);
			AssertCollectionNotContains(supportIncident2, incidentCollection);
			AssertCollectionContains(supportIncident3, incidentCollection);
			supportIncident1.Delete();
			supportIncident2.Delete();
			supportIncident3.Delete();
			Factory.Save();
		}

		public void TestReleaseBuildFilterValidation()
		{
			var releaseBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			releaseBuild.VersionNumber = new VersionNumber(1, 2, 3, 4);
			Factory.Save();

			var filterBizO = new SupportIncidentFilterBusinessObject();
			var filter = (ModuleGuidsFilter)filterBizO["Patched to Upgrade"];
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			filter.Validation.ValidateAll();
			AssertHasError(filter.Property1Info, "Please enter a value.");
			AssertHasError(filter.Property2Info, "Please enter a value.");

			filter.Property1 = ZGuid.NewZGuid();
			filter.Property2 = ZGuid.NewZGuid();
			filter.Validation.ValidateAll();
			AssertHasError(filter.Property1Info, "Enter a valid selection.");
			AssertHasError(filter.Property2Info, "Enter a valid selection.");

			filter.Property1 = releaseBuild.PK;
			filter.Property2 = releaseBuild.PK;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);
		}

		void CreateLicenceDatabaseFilter(SupportIncidentFilterBusinessObject filter, string nameValue)
		{
			var licenceDatabaseFilter = (ModuleGuidFilter)filter["Reported Database"];
			licenceDatabaseFilter.IsActive = true;
			licenceDatabaseFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var nameFilter = licenceDatabaseFilter.SelectedFilters["Database Server Name"] as ModuleTextFilter;
			if (nameFilter == null || string.IsNullOrEmpty(nameFilter.Property))
			{
				licenceDatabaseFilter.SelectedFilters.AddTextFilterStrip("Database Server Name", nameValue);
			}
			else
			{
				nameFilter.Property = nameValue;
			}
		}

		#endregion
		#region Implementation
		class SupportIncidentForTest : SupportIncident
		{
			public static SupportIncidentForTest New(BusinessObjectFactory factory, string incidentNumber)
			{
				SupportIncidentForTest result = factory.New<SupportIncidentForTest>();
				result.IM_IncidentNumber = incidentNumber;
				return result;
			}

			public SupportIncidentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				IM_StatusInfo.ClearValue();
				IM_ResolutionCodeInfo.ClearValue();
			}

			public SupportIncidentForTest SetValue(SchemaColumn column, object value)
			{
				this[column] = value;
				return this;
			}
		}

		T ActivateFilter<T>(SupportIncidentFilterBusinessObject filterBizO, string desc)
			where T : ModuleFilter
		{
			filterBizO[desc].IsActive = true;
			return (T)filterBizO[desc];
		}

		SupportIncidentCollection PerformSearch(SupportIncidentFilterBusinessObject filterBizO)
		{
			SupportIncidentCollection incidents = new SupportIncidentCollection(Factory);
			return PerformSearch(incidents, filterBizO);
		}

		SupportIncidentCollection PerformSearch(SupportIncidentCollection incidents, SupportIncidentFilterBusinessObject filterBizO)
		{
			incidents.Load(filterBizO.Filter);
			return incidents;
		}

		void AssertFilteredResult(SupportIncidentFilterBusinessObject filterBizO, params string[] expectedIncidents)
		{
			SupportIncidentCollection actualIncidents = PerformSearch(filterBizO);
			var actualIncidentNumbers = actualIncidents.Select(incident => incident.IM_IncidentNumber.ToString());
			AssertContainsExactElementsInAnyOrder(expectedIncidents, actualIncidentNumbers);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SupportIncidentFilterBusinessObject();
		}

		protected override string ExpectedIncidentType
		{
			get
			{
				return IncidentConstants.IncidentType.SupportIncident;
			}
		}

		#endregion
	}
}
