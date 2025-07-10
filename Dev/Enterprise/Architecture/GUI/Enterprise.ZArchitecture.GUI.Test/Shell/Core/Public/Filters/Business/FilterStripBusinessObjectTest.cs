using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.Testing.ModuleTextFilterTest;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class FilterStripBusinessObjectTest : TestCaseWithFactory
	{
		public void TestLoadLayoutWhenTypeOfTopBusinessObjectIsNull()
		{
			var user = Factory.New<IGlbStaff>();
			((BusinessObject)user).FillWithValidTestData();
			user.GS_WorkingLanguage = "ZH-CN";

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.CA.CAHouseBilleManifest))
			{
				var filterStrip = module.FilterBusinessObject;
				var strip = filterStrip.FilterStrips.AddNew();
				strip.FilterDescription = "Code";
				var codeFilter = filterStrip.SaveLayout("codeFilter");

				using (Env.SetTemporaryUserContext(new UserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					AssertNoExceptionThrown(() => { filterStrip.LoadLayout(codeFilter); });
				}
			}
		}

		public void TestLoadLayoutWouldNotThrowExceptionWhenTypeOfTopLevelBusinessObjectIsNull()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WIPAccruals))
			{
				Env.Security.WipsAndAccruals.IsAllowed = false;
				var filterStrip = module.FilterBusinessObject;
				var strip = filterStrip.FilterStrips.AddNew();
				strip.FilterDescription = "desc1";
				var stmModuleSavedFilter = filterStrip.SaveLayout("savedFilter");

				AssertNoExceptionThrown(() => { filterStrip.LoadLayout(stmModuleSavedFilter); });
			}
		}

		public void TestLimitedFiltersShouldContainsAlwaysAppliedFilters()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AccBankAccount))
			{
				var filterStrip = module.FilterBusinessObject;
				var bankAccountType = Factory.New<IAccBankAccount>().GetType();
				module.LimitedColumns = new ZLimitedColumnsProvider(module.GetElementType());
				var alwaysAppliedFilters = filterStrip.AlwaysAppliedModuleFilters;
				AssertEquals(2, alwaysAppliedFilters.Count);
				Assert("all alwaysAppliedFilters should be hidden.", alwaysAppliedFilters.All(filter => filter.Visibility == FilterVisibility.AlwaysAppliedAndHidden));
				AssertContainsExactElementsInAnyOrder(new string[] { "Company", "Active Status" }, alwaysAppliedFilters.Select(filter => filter.Code));
			}
		}

		public void TestModuleSqlFilterProperty1ValidateSql_MandatoryFilters()
		{
			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = true;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AccBankAccount))
			{
				var filterBO = module.FilterBusinessObject;
				filterBO.FilterStrips.AddNew("Custom SQL Filter");
				var bankAccountType = Factory.New<IAccBankAccount>().GetType();
				module.LimitedColumns = new ZLimitedColumnsProvider(module.GetElementType());
				var moduleSqlFilter = (ModuleSQLFilter)filterBO["Custom SQL Filter"];
				moduleSqlFilter.IsActive = true;
				moduleSqlFilter.Property1 = "1 = 1";
				filterBO.RunPreSaveValidation();

				moduleSqlFilter.Validation.ValidateSqlOnFind();
			}

			Assert("Should Contain Mandatory Filters", SqlEventTracker.Instance.SqlEventList.Any(x => x.Contains(@$"SELECT TOP 1 * FROM (SELECT TOP 1 * FROM dbo.AccBankAccount) arbitraryView92674a06d7d34d9d905c63c2baaf8d19 WHERE (
	1 = 1
	 /* 573EC813-9CB8-4C68-9652-8C03F510993D - This is a query Executed as a Reader. Please verify the owner of the query. If the query was created by the customer. Any errors need to be handled but the quality of the statement is out of scope. */ 
)
AND
AB_GC = '{Env.Instance.CurrentCompanyPK}' 
AND
AB_IsActive = 1")));
		}

		public void TestContainsBanned()
		{
			var filter1 = new ModuleTextFilter("filter1", DummyBizoSchema.Z0_Description);
			filter1.Category = FilterCategories.NumbersAndReferences;
			FilterStripBizO.AddModuleFilterForTest(filter1);

			var filter2 = new ModuleTextFilter("filter2", DummyBizoSchema.Z0_Description);
			filter2.Category = FilterCategories.Other;
			FilterStripBizO.AddModuleFilterForTest(filter2);

			var filter = FilterStripBizO.SaveLayout("Random Filter");
			
			Env.Security.ContainsInNumbersAndReferences.IsAllowed = true;

			FilterStripBizO.LoadLayout(filter);

			AssertEquals(false, ((ModuleTextBaseFilter)FilterStripBizO.ModuleFilters["filter1"]).ContainsBanned);
			AssertEquals(false, ((ModuleTextBaseFilter)FilterStripBizO.ModuleFilters["filter2"]).ContainsBanned);

			Env.Security.ContainsInNumbersAndReferences.IsAllowed = false;

			FilterStripBizO.LoadLayout(filter);

			AssertEquals(true, ((ModuleTextBaseFilter)FilterStripBizO.ModuleFilters["filter1"]).ContainsBanned);
			AssertEquals(false, ((ModuleTextBaseFilter)FilterStripBizO.ModuleFilters["filter2"]).ContainsBanned);
		}

		#region TestSetInitialCodeForSearch

		public void TestSetInitialCodeForSearch()
		{
			var someFilter = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_Code);
			FilterStripBizO.AddModuleFilterForTest(someFilter);
			FilterStripBizO.SetInitialCodeForSearch("YEA", typeof(DummyBusinessObject));
			AssertEquals("Code set", "YEA", ((ModuleTextFilter)FilterStripBizO["Some Filter"]).Property);
		}

		public void TestSetInitialCodeForSearchWithPrefix()
		{
			FilterStripBizO.AddModuleFilterForTest(new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code));
			FilterStripBizO.AddModuleFilterForTest(new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description) { Prefix = "D" });

			FilterStripBizO.SetInitialCodeForSearch("YEA", typeof(DummyBusinessObject));
			AssertEquals("YEA", ((ModuleTextFilter)FilterStripBizO["Code"]).Property);
			AssertEquals("", ((ModuleTextFilter)FilterStripBizO["Description"]).Property);

			FilterStripBizO.SetInitialCodeForSearch("D:NEA", typeof(DummyBusinessObject));
			AssertEquals("YEA", ((ModuleTextFilter)FilterStripBizO["Code"]).Property);
			AssertEquals("NEA", ((ModuleTextFilter)FilterStripBizO["Description"]).Property);
		}

		#endregion

		#region TestUsersFilterStripsDoesNotResetInitialCodeForSearch

		public void TestUsersFilterStripsDoesNotResetInitialCodeForSearch()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();

			var someFilter = ((ModuleTextFilter)filterBizO["someFilter"]);
			var anotherSomeFilter = ((ModuleTextFilter)filterBizO["anotherSomeFilter"]);

			var strip1 = filterBizO.FilterStrips.AddNew();
			strip1.FilterDescription = someFilter.Description;

			var strip2 = filterBizO.FilterStrips.AddNew();
			strip2.FilterDescription = anotherSomeFilter.Description;

			var stmModuleSavedFilter = filterBizO.SaveLayout("savedFilter");
			stmModuleSavedFilter.S9_IsPublished = false;

			var collection = new DummyBusinessObjectCollection(Factory);

			AssertNull(filterBizO.LastUsedLayout);

			filterBizO.SetInitialCodeForSearch("YEA", typeof(DummyBusinessObject));
			AssertEquals("Code set", "YEA", ((ModuleTextFilter)filterBizO["hasInitialCodeForSearchFilter"]).Property);

			filterBizO.LoadLayout(stmModuleSavedFilter);

			AssertEquals("savedFilter", filterBizO.LastUsedLayout.S9_FilterName);

			FilterStripBizO.SetInitialCodeForSearch("YEA", typeof(DummyBusinessObject));
			AssertEquals("Code set", "YEA", ((ModuleTextFilter)filterBizO["hasInitialCodeForSearchFilter"]).Property);

			AssertEquals("Amount of AlwaysVisible filters (Defaults)", filterBizO.AlwaysVisibleModuleFilters.Count, 1);
			AssertCollectionContains((ModuleTextFilter)filterBizO["hasInitialCodeForSearchFilter"], filterBizO.AlwaysVisibleModuleFilters);

			AssertEquals("Initial code's value should remain", ((ModuleTextFilter)filterBizO["hasInitialCodeForSearchFilter"]).Property, "YEA");
			AssertEquals("someFilter still remanis active", true, ((ModuleTextFilter)filterBizO["someFilter"]).IsActive);
			AssertEquals("anotherSomeFilter still remanis active", true, ((ModuleTextFilter)filterBizO["anotherSomeFilter"]).IsActive);
		}

		#endregion

		#region Test External Defaults

		public void TestContainsDefaultsTrueAllowLoadingUsersFilterStrips()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();

			var someFilter = ((ModuleTextFilter)filterBizO["someFilter"]);
			var anotherSomeFilter = ((ModuleTextFilter)filterBizO["anotherSomeFilter"]);

			var strip1 = filterBizO.FilterStrips.AddNew();
			strip1.FilterDescription = someFilter.Description;

			var strip2 = filterBizO.FilterStrips.AddNew();
			strip2.FilterDescription = anotherSomeFilter.Description;

			var stmModuleSavedFilter = filterBizO.SaveLayout("savedFilter");
			stmModuleSavedFilter.S9_IsPublished = false;

			var collection = new DummyBusinessObjectCollection(Factory);

			AssertNull(filterBizO.LastUsedLayout);

			var defaults = new FilterBusinessObjectDefaults();
			var filterDefault = new FilterBusinessObjectDefault("hasDefaultValueFilter", "Property", new ZString("someValue"));
			defaults.Add(filterDefault);

			AssertEquals("filterBizO.ContainsDefaults", false, filterBizO.ContainsDefaults);
			filterBizO.SetExternalDefaults(defaults);
			AssertEquals("filterBizO.ContainsDefaults", true, filterBizO.ContainsDefaults);

			filterBizO.LoadLayout(stmModuleSavedFilter);

			AssertEquals("savedFilter", filterBizO.LastUsedLayout.S9_FilterName);

			AssertEquals("Amount of AlwaysVisible filters (Defaults)", filterBizO.AlwaysVisibleModuleFilters.Count, 1);
			AssertCollectionContains((ModuleTextFilter)filterBizO["hasDefaultValueFilter"], filterBizO.AlwaysVisibleModuleFilters);

			AssertEquals("Default value should remain", ((ModuleTextFilter)filterBizO["hasDefaultValueFilter"]).Property, "someValue");
			AssertEquals("someFilter still remanis active", true, ((ModuleTextFilter)filterBizO["someFilter"]).IsActive);
			AssertEquals("anotherSomeFilter still remanis active", true, ((ModuleTextFilter)filterBizO["anotherSomeFilter"]).IsActive);

			filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var hasDefaultValueFilter = ((ModuleTextFilter)filterBizO["hasDefaultValueFilter"]);
			hasDefaultValueFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			AssertEquals("filterBizO.ContainsDefaults", false, filterBizO.ContainsDefaults);
			filterBizO.SetExternalDefaults(defaults);
			AssertEquals("filterBizO.ContainsDefaults", true, filterBizO.ContainsDefaults);
			AssertEquals("Amount of AlwaysVisible filters (Defaults)", 0, filterBizO.AlwaysVisibleModuleFilters.Count);
		}

		public void TestDefaultOverrideLoadedUsersFilterStrips()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();

			var someFilter = ((ModuleTextFilter)filterBizO["someFilter"]);
			var anotherSomeFilter = ((ModuleTextFilter)filterBizO["anotherSomeFilter"]);
			var hasDefaultValueFilter = ((ModuleTextFilter)filterBizO["hasDefaultValueFilter"]);

			hasDefaultValueFilter.Property = "someUserSavedValue";

			var strip1 = filterBizO.FilterStrips.AddNew();
			strip1.FilterDescription = someFilter.Description;

			var strip2 = filterBizO.FilterStrips.AddNew();
			strip2.FilterDescription = anotherSomeFilter.Description;

			var strip3 = filterBizO.FilterStrips.AddNew();
			strip3.FilterDescription = hasDefaultValueFilter.Description;

			var stmModuleSavedFilter = filterBizO.SaveLayout("savedFilter");
			stmModuleSavedFilter.S9_IsPublished = false;

			var collection = new DummyBusinessObjectCollection(Factory);

			AssertNull(filterBizO.LastUsedLayout);

			var defaults = new FilterBusinessObjectDefaults();
			var filterDefault = new FilterBusinessObjectDefault("hasDefaultValueFilter", "Property", new ZString("someValue"));
			defaults.Add(filterDefault);

			AssertEquals("filterBizO.ContainsDefaults", false, filterBizO.ContainsDefaults);
			filterBizO.SetExternalDefaults(defaults);
			AssertEquals("filterBizO.ContainsDefaults", true, filterBizO.ContainsDefaults);

			strip1.FilterDescription = "";
			strip1.Delete();

			strip2.FilterDescription = "";
			strip2.Delete();

			strip3.FilterDescription = "";
			strip3.Delete();

			filterBizO.LoadLayout(stmModuleSavedFilter);

			AssertEquals("savedFilter", filterBizO.LastUsedLayout.S9_FilterName);

			AssertEquals("Amount of AlwaysVisible filters (Defaults)", filterBizO.AlwaysVisibleModuleFilters.Count, 1);
			AssertCollectionContains((ModuleTextFilter)filterBizO["hasDefaultValueFilter"], filterBizO.AlwaysVisibleModuleFilters);

			AssertEquals("Default value should remain", ((ModuleTextFilter)filterBizO["hasDefaultValueFilter"]).Property, "someValue");
			AssertEquals("someFilter still remanis active", true, ((ModuleTextFilter)filterBizO["someFilter"]).IsActive);
			AssertEquals("anotherSomeFilter still remanis active", true, ((ModuleTextFilter)filterBizO["anotherSomeFilter"]).IsActive);
		}

		public void TestSetExternalDefaults()
		{
			var filter = new ModuleFlagsFilter("Some Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });
			var otherFilter = new ModuleFlagsFilter("Some Other Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });
			var yetAnotherFilter = new ModuleFlagsFilter("Yet Another Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });

			filter.Property1 = ZBool.True;

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property0", ZBool.True));
			defaults.Add(new FilterBusinessObjectDefault("Some Other Filter", "Property1", ZBool.True));

			FilterStripBizO.AddModuleFilterForTest(filter);
			FilterStripBizO.AddModuleFilterForTest(otherFilter);
			FilterStripBizO.AddModuleFilterForTest(yetAnotherFilter);

			AssertEquals(FilterStripBizO.ContainsDefaults, false);

			FilterStripBizO.SetExternalDefaults(defaults);

			AssertEquals(FilterStripBizO.ContainsDefaults, true);

			AssertCollectionContains(filter, FilterStripBizO.AlwaysVisibleModuleFilters);
			AssertCollectionContains(otherFilter, FilterStripBizO.AlwaysVisibleModuleFilters);
			AssertCollectionNotContains(yetAnotherFilter, FilterStripBizO.AlwaysVisibleModuleFilters);

			AssertEquals(filter.Property0, ZBool.True);
			AssertEquals(filter.Property1, ZBool.False); // SetExternalDefaults should reset all properties except those which specified in defauls.
			AssertEquals(otherFilter.Property1, ZBool.True);
		}

		public void TestExternalDefaults_ImplimentMultipleOfSameFilter()
		{
			var filter = new ModuleFlagsFilter("Some Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });
			FilterStripBizO.AddModuleFilterForTest(filter);

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property0", ZBool.True, FilterOrCategory.Blue, instance: 1));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property1", ZBool.False, FilterOrCategory.Blue, instance: 1));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property0", ZBool.False, FilterOrCategory.Blue, instance: 2));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property1", ZBool.True, FilterOrCategory.Blue, instance: 2));

			AssertEquals("Not all filter defaults were added", 4, defaults.Count);
			FilterStripBizO.SetExternalDefaults(defaults);

			AssertEquals("Filter Bizo should have two filters", 2, FilterStripBizO.AlwaysVisibleModuleFilters.Count);

			AssertEquals("First Filter property 0 should be set to true", ((ModuleFlagsFilter)FilterStripBizO.AlwaysVisibleModuleFilters[0]).Property0, ZBool.True);
			AssertEquals("First Filter property 1 should be set to false", ((ModuleFlagsFilter)FilterStripBizO.AlwaysVisibleModuleFilters[0]).Property1, ZBool.False);
			AssertEquals("Second Filter property 0 should be set to false", ((ModuleFlagsFilter)FilterStripBizO.AlwaysVisibleModuleFilters[1]).Property0, ZBool.False);
			AssertEquals("Second Filter property 1 should be set to true", ((ModuleFlagsFilter)FilterStripBizO.AlwaysVisibleModuleFilters[1]).Property1, ZBool.True);
		}

		public void TestExternalDefaults_CategoryColorGetsSetCorrectly()
		{
			var filter = new ModuleFlagsFilter("Some Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });
			var otherFilter = new ModuleFlagsFilter("Some Other Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });

			FilterStripBizO.AddModuleFilterForTest(filter);
			FilterStripBizO.AddModuleFilterForTest(otherFilter);

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Other Filter", "Property1", ZBool.True, FilterOrCategory.Blue));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property0", ZBool.True, FilterOrCategory.Red, 1));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property1", ZBool.True, FilterOrCategory.Red, 1));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property0", ZBool.False, FilterOrCategory.Red, 2));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property1", ZBool.False, FilterOrCategory.Red, 2));

			AssertEquals("Not all filter defaults were added", 5, defaults.Count);
			FilterStripBizO.SetExternalDefaults(defaults);

			AssertEquals("Filter Bizo should have three filters", 3, FilterStripBizO.AlwaysVisibleModuleFilters.Count);
			AssertEquals("Filter 1 Description", "Some Other Filter", FilterStripBizO.AlwaysVisibleModuleFilters[0].Description);
			AssertEquals("Filter 2 Description", "Some Filter (1)", FilterStripBizO.AlwaysVisibleModuleFilters[1].Description);
			AssertEquals("Filter 3 Description", "Some Filter (2)", FilterStripBizO.AlwaysVisibleModuleFilters[2].Description);

			AssertEquals("Filter 1 Description", FilterOrCategory.Blue, FilterStripBizO.AlwaysVisibleModuleFilters[0].OrCategory);
			AssertEquals("Filter 2 Description", FilterOrCategory.Red, FilterStripBizO.AlwaysVisibleModuleFilters[1].OrCategory);
			AssertEquals("Filter 3 Description", FilterOrCategory.Red, FilterStripBizO.AlwaysVisibleModuleFilters[2].OrCategory);
		}

		public void TestExternalDefaults_ModuleFilterAndFilterStripHaveSameOrCategory()
		{
			var filter = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(filter);

			var strip = FilterStripBizO.FilterStrips.AddNew("Some Filter");
			strip.OrCategory = FilterOrCategory.Blue;
			strip.CurrentModuleFilter.IsActive = false;
			((ModuleTextFilter)strip.CurrentModuleFilter).Property = "111";
			strip.CurrentModuleFilter.Visibility = FilterVisibility.AlwaysVisible;

			var strip2 = FilterStripBizO.FilterStrips.AddNew("Some Filter");
			strip2.OrCategory = FilterOrCategory.Blue;
			strip2.CurrentModuleFilter.IsActive = false;
			((ModuleTextFilter)strip2.CurrentModuleFilter).Property = "222";
			strip2.CurrentModuleFilter.Visibility = FilterVisibility.AlwaysVisible;

			var layout = new LayoutsTestDataHelper(Factory).NewLayout("bla");
			FilterStripBizO.WriteFilterStripsToXml(layout, FilterStripBizO.FilterStrips, new FilterStripLayoutsHelper());

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property", new ZString("333"), FilterOrCategory.Red));
			FilterStripBizO.SetExternalDefaults(defaults);

			FilterStripBizO.LoadLayout(layout);

			AssertEquals(2, FilterStripBizO.FilterStrips.Count);
			AssertEquals(FilterOrCategory.Blue, FilterStripBizO.FilterStrips[0].OrCategory);
			AssertEquals(FilterOrCategory.Blue, FilterStripBizO.FilterStrips[0].CurrentModuleFilter.OrCategory);
			AssertEquals(FilterOrCategory.Blue, FilterStripBizO.FilterStrips[1].OrCategory);
			AssertEquals(FilterOrCategory.Blue, FilterStripBizO.FilterStrips[1].CurrentModuleFilter.OrCategory);
			AssertEquals("Z0_Description like '333%' or Z0_Description like '222%'", FilterStripBizO.Filter.LiteralTextADO);
		}

		public void TestMultiplePropertiesCanBeSetAsDefaultsForOneFilter()
		{
			var filter = new ModuleFlagsFilter("Some Filter", new string[] { "flag1", "flag2" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });

			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property0", ZBool.True));
			defaults.Add(new FilterBusinessObjectDefault("Some Filter", "Property1", ZBool.True));

			FilterStripBizO.AddModuleFilterForTest(filter);
			AssertEquals(FilterStripBizO.ContainsDefaults, false);

			FilterStripBizO.SetExternalDefaults(defaults);
			AssertEquals(FilterStripBizO.ContainsDefaults, true);
			AssertCollectionContains(filter, FilterStripBizO.AlwaysVisibleModuleFilters);
			AssertEquals(filter.Property0, ZBool.True);
			AssertEquals(filter.Property1, ZBool.True);
		}

		#region SetExternalDefaults with ChildDefaults

		const string ParentFilterDescription = "Parent filter";

		public void TestSetExternalDefaults_SetsChildDefaults()
		{
			SetupParentGuidFilter();

			var childDefaults = new FilterBusinessObjectDefaults();
			childDefaults.Add(new FilterBusinessObjectDefault(DummyBizoSchema.Z0_Code.Name, "Property", (ZString)"Shrek"));
			childDefaults.Add(new FilterBusinessObjectDefault(DummyBizoSchema.Z0_Description.Name, "Property", (ZString)"Fiona"));

			var parentDefaults = new FilterBusinessObjectDefaults();
			parentDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: ParentFilterDescription,
				propertyName: "Property",
				childDefaults: childDefaults,
				comparisonOperator: ModuleGuidFilter.ComparisonConstants.AnyMatch));

			FilterStripBizO.SetExternalDefaults(parentDefaults);
			FilterStripBizO.AddAlwaysVisibleFilters();

			AssertEquals("The parent filter should have been added as an always visible filter.", FilterStripBizO.AlwaysVisibleModuleFilters.Count, 1);
			AssertEquals("The parent filter should have been added through AddAlwaysVisibleFilters.", FilterStripBizO.FilterStrips.Count, 1);

			var parentFilter = FilterStripBizO.ModuleFilters[ParentFilterDescription] as ModuleGuidFilter;
			AssertNotNull(parentFilter);
			AssertEquals(parentFilter.SelectedFilters.FilterStrips.Count, 2);

			AssertEquals("The child filters should have been added to the parent filter as always visible filters.", parentFilter.SelectedFilters.AlwaysVisibleModuleFilters.Count, 2);
			AssertEquals("The child filters should have been added through AddAlwaysVisibleFilters.", parentFilter.SelectedFilters.FilterStrips.Count, 2);
		}

		public void TestSetExternalDefaults_SelectedFiltersAreNotShared()
		{
			SetupParentGuidFilter();

			var childDefaults1 = new FilterBusinessObjectDefaults();
			childDefaults1.Add(new FilterBusinessObjectDefault(DummyBizoSchema.Z0_Code.Name, "Property", (ZString)"SHK"));

			var childDefaults2 = new FilterBusinessObjectDefaults();
			childDefaults2.Add(new FilterBusinessObjectDefault(DummyBizoSchema.Z0_Description.Name, "Property", (ZString)"Shrek"));

			var parentDefaults = new FilterBusinessObjectDefaults();
			parentDefaults.Add(
				FilterBusinessObjectDefault.Create(
					filterName: ParentFilterDescription,
					propertyName: "Property",
					childDefaults: childDefaults1,
					category: FilterOrCategory.Blue,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch,
					instance: 1));
			parentDefaults.Add(
				FilterBusinessObjectDefault.Create(
					filterName: ParentFilterDescription,
					propertyName: "Property",
					childDefaults: childDefaults2,
					category: FilterOrCategory.Blue,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch,
					instance: 2));

			FilterStripBizO.SetExternalDefaults(parentDefaults);
			FilterStripBizO.AddAlwaysVisibleFilters();

			var parentFilter1 = FilterStripBizO.ModuleFilters.GetDuplicateFilter($"{ParentFilterDescription} (1)") as ModuleGuidFilter;
			AssertNotNull(parentFilter1);
			AssertEquals(parentFilter1.SelectedFilters.FilterStrips.Count, 1);

			var childFilter1 = parentFilter1.SelectedFilters.FilterStrips[0].CurrentModuleFilter as ModuleTextFilter;
			AssertNotNull(childFilter1);
			AssertEquals(childFilter1.Property, "SHK");

			var parentFilter2 = FilterStripBizO.ModuleFilters.GetDuplicateFilter($"{ParentFilterDescription} (2)") as ModuleGuidFilter;
			AssertNotNull(parentFilter2);
			AssertEquals(parentFilter2.SelectedFilters.FilterStrips.Count, 1);

			var childFilter2 = parentFilter2.SelectedFilters.FilterStrips[0].CurrentModuleFilter as ModuleTextFilter;
			AssertNotNull(childFilter2);
			AssertEquals(childFilter2.Property, "Shrek");
		}

		void SetupParentGuidFilter()
		{
			var guidModuleFilter = new ModuleGuidFilter(ParentFilterDescription, DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory));
			FilterStripBizO.AddModuleFilterForTest(guidModuleFilter);
		}

		public void TestFilterStripBusinessObjectHaveCorrectSearchTypeWhenSetExternalDefaultsWithChildDefaults()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var guidModuleFilter = new ModuleGuidPivotFilter("Named Account Clients", ModuleIDs.Organisation, DummyBizoSchema.Z0_Guid, DummyBizoSchema.Z0_ComputedGuid, new OrgHeaderCollection(Factory), typeof(DummyBusinessObject), typeof(OrgHeader));
				FilterStripBizO.AddModuleFilterForTest(guidModuleFilter);

				var childDefaults1 = new FilterBusinessObjectDefaults();
				childDefaults1.Add(new FilterBusinessObjectDefault("Code", "Property", (ZString)"SHK"));

				var childDefaults2 = new FilterBusinessObjectDefaults();
				childDefaults2.Add(new FilterBusinessObjectDefault("Name", "Property", (ZString)"Shrek"));

				var parentDefaults = new FilterBusinessObjectDefaults();
				parentDefaults.Add(
					FilterBusinessObjectDefault.Create(
						filterName: "Named Account Clients",
						propertyName: "Property",
						childDefaults: childDefaults1,
						category: FilterOrCategory.Blue,
						comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch,
						instance: 1));
				parentDefaults.Add(
					FilterBusinessObjectDefault.Create(
						filterName: "Named Account Clients",
						propertyName: "Property",
						childDefaults: childDefaults2,
						category: FilterOrCategory.Blue,
						comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch,
						instance: 2));

				FilterStripBizO.SetExternalDefaults(parentDefaults);
				FilterStripBizO.AddAlwaysVisibleFilters();

				var parentFilter1 = FilterStripBizO.ModuleFilters["Named Account Clients"] as ModuleGuidPivotFilter;

				AssertNotNull(parentFilter1);
				AssertEquals(FilterStripBizO.SearchType, parentFilter1.SelectedFilters.SearchType);
			}
		}

		#endregion

		public void TestSetExternalDefaults_SetsComparisonOperator()
		{
			var filter = new DummyModuleTextFilter();
			filter.OverriddenAllowedComparisonOperators.Add(ModuleTextFilter.ComparisonConstants.Default);
			filter.OverriddenAllowedComparisonOperators.Add(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			FilterStripBizO.AddModuleFilterForTest(filter);

			var moduleFilter = SetupDefaultWithOperatorAndGetFilter(ZString.Empty);
			AssertNotNull(moduleFilter);
			AssertEquals(moduleFilter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.Default);

			moduleFilter = SetupDefaultWithOperatorAndGetFilter("invalid operator");
			AssertNotNull(moduleFilter);
			AssertEquals(moduleFilter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.Default);

			moduleFilter = SetupDefaultWithOperatorAndGetFilter(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			AssertNotNull(moduleFilter);
			AssertEquals(moduleFilter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		IModuleFilterWithComparisonOperator SetupDefaultWithOperatorAndGetFilter(ZString comparisonOperator)
		{
			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(FilterBusinessObjectDefault.Create(
				filterName: "DummyTextFilter",
				propertyName: "Property",
				value: (ZString)"SHREK",
				comparisonOperator: comparisonOperator));

			FilterStripBizO.SetExternalDefaults(defaults);
			FilterStripBizO.AddAlwaysVisibleFilters();

			var filterStrip = FilterStripBizO.FilterStrips.SingleOrDefault() as FilterStrip;
			return filterStrip?.CurrentModuleFilter as IModuleFilterWithComparisonOperator;
		}

		#region  TestActiveModuleFiltersForQuery_ChecksForAlwaysAppliedAndHiddenClones

		public void TestActiveModuleFiltersForQuery_ChecksForAlwaysAppliedAndHiddenClones()
		{
			var alwaysAppliedFilter = new ModuleTextFilter("Client", DummyBizoSchema.Z0_Description)
			{
				IsActive = false,
				Visibility = FilterVisibility.AlwaysApplied
			};

			var activeFilter1 = new ModuleTextFilter("Active", DummyBizoSchema.Z0_Description)
			{
				IsActive = true
			};

			var activeFilter2 = new ModuleTextFilter("Client (1)", DummyBizoSchema.Z0_Description)
			{
				IsActive = true
			};

			var activeFilter3 = new ModuleTextFilter("Client Any", DummyBizoSchema.Z0_Description)
			{
				IsActive = true
			};

			var activeFilter4 = new ModuleTextFilter("Client (1) ", DummyBizoSchema.Z0_Description)
			{
				IsActive = true
			};

			var activeFilter5 = new ModuleTextFilter("Client (a)", DummyBizoSchema.Z0_Description)
			{
				IsActive = true
			};

			FilterStripBizO.AddModuleFilterForTest(alwaysAppliedFilter);
			FilterStripBizO.AddModuleFilterForTest(activeFilter1);
			FilterStripBizO.AddModuleFilterForTest(activeFilter2);
			FilterStripBizO.AddModuleFilterForTest(activeFilter3);
			FilterStripBizO.AddModuleFilterForTest(activeFilter4);
			FilterStripBizO.AddModuleFilterForTest(activeFilter5);

			var expectedFilters = new List<ModuleTextFilter>()
			{
				activeFilter1,
				activeFilter2,
				activeFilter3,
				activeFilter4,
				activeFilter5
			};

			AssertEquals(5, FilterStripBizO.ActiveModuleFilters.Count);
			AssertContainsExactElementsInAnyOrder(expectedFilters, FilterStripBizO.ActiveModuleFiltersForQuery);

			var newFilterStripBizO = new DummyFilterStripBusinessObject();
			newFilterStripBizO.AddModuleFilterForTest(alwaysAppliedFilter);
			newFilterStripBizO.AddModuleFilterForTest(activeFilter1);
			newFilterStripBizO.AddModuleFilterForTest(activeFilter3);
			newFilterStripBizO.AddModuleFilterForTest(activeFilter4);
			newFilterStripBizO.AddModuleFilterForTest(activeFilter5);

			expectedFilters.Remove(activeFilter2);
			expectedFilters.Add(alwaysAppliedFilter);

			AssertEquals(4, newFilterStripBizO.ActiveModuleFilters.Count);
			AssertContainsExactElementsInAnyOrder(expectedFilters, newFilterStripBizO.ActiveModuleFiltersForQuery);
		}

		#endregion

		public void TestSetExternalDefaultsWithSearchTypeChange()
		{
			using (var mocker = new GlowIndexQueryEngineMock())
			{
				var filterBO = new DummyFilterStripBusinessObjectForDefaultsTesting();
				filterBO.IndexSearchFields = mocker.SearchFields;

				var provider = new DummyCollection(Factory);

				AssertEquals(SearchType.Sql, filterBO.SearchType);

				filterBO.SetExternalDefaults(provider);

				var sqlFilter = (ModuleTextFilter)filterBO["someFilter"];
				AssertNotNull(sqlFilter);
				AssertCollectionContains(sqlFilter, filterBO.AlwaysVisibleModuleFilters);
				AssertEquals("TestValue", sqlFilter.Property);

				filterBO.SearchType = SearchType.Index;
				filterBO.SetExternalDefaults(provider);

				AssertEquals(SearchType.Index, filterBO.SearchType);
				AssertNull(filterBO["Z0_Code"]);

				var indexFilter = (ModuleTextFilter)filterBO["CODE"];
				AssertNotNull(indexFilter);

				AssertCollectionContains(indexFilter, filterBO.AlwaysVisibleModuleFilters);
				AssertEquals("TestValue", indexFilter.Property);
			}
		}

		class DummyCollection : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummyCollection(BusinessObjectFactory factory) : base(factory)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("someFilter", "Property", new ZString("TestValue")));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("CODE", "Property", new ZString("TestValue"), SearchType.Index));
			}
		}

		#endregion

		#region TestModuleFiltersHasLimitedColumns

		public void TestModuleFiltersHasLimitedColumns()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				FilterStripBizO.ParentModule = module;
				FilterStripBizO.ParentModule.LimitedColumns = new ZLimitedColumnsProvider(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);

				AssertContainsExactElementsInAnyOrder(new string[] { "Code", "Description" }, FilterStripBizO.ModuleFilters.Select(filter => filter.Description));
				Assert(FilterStripBizO.ModuleFilters.All(filter => filter.Visibility == FilterVisibility.AlwaysVisible));
			}
		}

		#endregion

		#region TestAlwaysVisibleModuleFilters

		public void TestAlwaysVisibleModuleFilters()
		{
			var alwaysVisibleFilter = new ModuleTextFilter("alwaysVisible", DummyBizoSchema.Z0_Description);
			alwaysVisibleFilter.Visibility = FilterVisibility.AlwaysVisible;
			FilterStripBizO.AddModuleFilterForTest(alwaysVisibleFilter);

			var nonAlwaysVisibleFilter = new ModuleTextFilter("nonAlwaysVisible", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(nonAlwaysVisibleFilter);

			AssertCollectionContains(alwaysVisibleFilter, FilterStripBizO.AlwaysVisibleModuleFilters);
			AssertCollectionNotContains(nonAlwaysVisibleFilter, FilterStripBizO.AlwaysVisibleModuleFilters);
		}

		#endregion

		#region TestLoadLayoutWithEmptyLayoutName

		[ExpectNoExceptions]
		public void TestLoadLayoutWithEmptyLayoutName()
		{
			FilterStripBizO.LoadLayout(null);
		}

		#endregion

		#region TestLoadingLayoutsIncludesAlwaysVisibleFilters

		public void TestLoadingLayoutsIncludesAlwaysVisibleFilters()
		{
			var nonAlwaysVisibleFilter1 = new ModuleTextFilter("nonAlwaysVisible1", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(nonAlwaysVisibleFilter1);

			var nonAlwaysVisibleFilter2 = new ModuleTextFilter("nonAlwaysVisible2", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(nonAlwaysVisibleFilter2);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = nonAlwaysVisibleFilter1.Description;
			var filter = FilterStripBizO.SaveLayout("Random Filter");

			var alwaysVisibleFilter = new ModuleTextFilter("alwaysVisible", DummyBizoSchema.Z0_Description);
			alwaysVisibleFilter.Visibility = FilterVisibility.AlwaysVisible;
			FilterStripBizO.AddModuleFilterForTest(alwaysVisibleFilter);

			strip.FilterDescription = "";
			strip.Delete();
			AssertEquals("precondition:", false, alwaysVisibleFilter.IsActive);
			AssertEquals("precondition:", false, nonAlwaysVisibleFilter1.IsActive);
			AssertEquals("precondition:", false, nonAlwaysVisibleFilter2.IsActive);

			FilterStripBizO.LoadLayout(filter);

			AssertEquals("alwaysVisibleFilter should be active", true, alwaysVisibleFilter.IsActive);
			AssertEquals("nonAlwaysVisibleFilter1 should be active", true, nonAlwaysVisibleFilter1.IsActive);
			AssertEquals("nonAlwaysVisibleFilter2 should not be active", false, nonAlwaysVisibleFilter2.IsActive);
		}

		#endregion

		#region TestLoadLayoutMaySkipLayoutIfHasInitialCode

		public void TestLoadLayoutMaySkipLayoutIfHasInitialCode()
		{
			var filter1 = new ModuleTextFilter("Filter 1", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(filter1);

			var filter2 = new ModuleTextFilter("Filter 2", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(filter2);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter1.Description;
			var filter = FilterStripBizO.SaveLayout("Random Filter");

			Assert("Layout should be set", FilterStripBizO.LoadLayout(filter, false));
			Assert("Layout should be set", FilterStripBizO.LoadLayout(filter, true));
			Assert("Layout should be set", FilterStripBizO.LoadLayout(null, false));

			FilterStripBizO.SetInitialCodeForSearch("XYZ", typeof(DummyBusinessObject));

			Assert("Layout should be set", FilterStripBizO.LoadLayout(filter, false));
			Assert("Layout should not be set because of initial code", !FilterStripBizO.LoadLayout(filter, true));
		}

		#endregion

		#region TestLoadLayoutWithDeletedStmModuleFilter

		public void TestLoadLayoutWithDeletedStmModuleFilter()
		{
			var filter1 = new ModuleTextFilter("Filter 1", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(filter1);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter1.Description;

			var filter = FilterStripBizO.SaveLayout("Random Filter");
			filter.Delete();

			Assert("Layout should be set", FilterStripBizO.LoadLayout(filter, false));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		#endregion

		#region TestActiveFilterIsIgnoredWhenUsingModuleFilterThatOverridesAllOtherFilters

		public void TestActiveFilterIsIgnoredWhenUsingModuleFilterThatOverridesAllOtherFilters()
		{
			var filterOverridingAllOthers = (ModuleTextFilter)FilterStripBizO[DummyFilterStripBusinessObject.FilterThatOverridesAllOthers];
			filterOverridingAllOthers.IsActive = true;
			filterOverridingAllOthers.Property = "";

			AssertEquals(FilterStripBizO.Filter.IgnoreActiveFilter, false);

			filterOverridingAllOthers.Property = "sthing";
			AssertEquals(FilterStripBizO.Filter.IgnoreActiveFilter, true);

			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		#endregion

		public void TestMoreThanOneExclusiveFilterCanBeApplied()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "description1";
			dummy2.Z0_Description = "description2";
			dummy1.Z0_Code = "code1";
			dummy2.Z0_Code = "code2";

			var codeFilter0 = (ModuleTextFilter)FilterStripBizO[DummyFilterStripBusinessObject.FilterThatOverridesAllOthers];
			var codeFilter1 = new ModuleTextFilter(DummyFilterStripBusinessObject.FilterThatOverridesAllOthers + "(1)", DummyBizoSchema.Z0_Code);
			codeFilter0.IsExclusiveHelper = true;
			codeFilter1.IsExclusiveHelper = true;

			codeFilter0.IsActive = true;
			codeFilter1.IsActive = true;
			FilterStripBizO.AddModuleFilterForTest(codeFilter1);

			codeFilter0.Property = "code1";
			AssertExpectedDummies(dummy1, dummy2, true, false);

			codeFilter1.Property = "code2";
			AssertExpectedDummies(dummy1, dummy2, false, false);

			codeFilter0.OrCategory = FilterOrCategory.Blue;
			codeFilter1.OrCategory = FilterOrCategory.Green;
			AssertExpectedDummies(dummy1, dummy2, false, false); // filters are in different categories and should be AND'ed

			codeFilter0.OrCategory = FilterOrCategory.Red;
			codeFilter1.OrCategory = FilterOrCategory.Red;
			AssertExpectedDummies(dummy1, dummy2, true, true); // filters are grouped in the red category and should be OR'd
		}

		public void TestMandatorySecurityFilterCanBeAppliedWithExclusiveFilter()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "description1";
			dummy2.Z0_Description = "description2";
			dummy1.Z0_Code = "code1";
			dummy2.Z0_Code = "code2";

			var exclusiveFilter = (ModuleTextFilter)FilterStripBizO[DummyFilterStripBusinessObject.FilterThatOverridesAllOthers];
			var mandatorySecurityFilter = new ModuleTextFilter("MandatorySecurityFilte 1", DummyBizoSchema.Z0_Code);
			exclusiveFilter.IsExclusiveHelper = true;
			mandatorySecurityFilter.IsExclusiveHelper = false;
			mandatorySecurityFilter.IsMandatorySecurityFilter = true;

			exclusiveFilter.IsActive = true;
			mandatorySecurityFilter.IsActive = true;
			FilterStripBizO.AddModuleFilterForTest(mandatorySecurityFilter);

			exclusiveFilter.Property = "code1";
			mandatorySecurityFilter.Property = "code1";
			AssertExpectedDummies(dummy1, dummy2, true, false);

			exclusiveFilter.Property = "code2";
			mandatorySecurityFilter.Property = "code2";
			AssertExpectedDummies(dummy1, dummy2, false, true);

			exclusiveFilter.Property = "code1";
			mandatorySecurityFilter.Property = "code2";
			AssertExpectedDummies(dummy1, dummy2, false, false);

			exclusiveFilter.Property = "code2";
			mandatorySecurityFilter.Property = "code1";
			AssertExpectedDummies(dummy1, dummy2, false, false);
		}

		public void TestHasCustomSqlFilter()
		{
			var filterStripBusinessObject1 = new DummyFilterStripBusinessObject();
			Assert(!filterStripBusinessObject1.HasCustomSqlFilter);

			var filterStripBusinessObject2 = new DummyFilterStripBusinessObject();
			filterStripBusinessObject2.QueryObjectType = typeof(DummyBusinessObject);
			Assert(filterStripBusinessObject2.HasCustomSqlFilter);
		}

		#region User-defined Filters

		public void TestShouldAddUserDefinedFilter()
		{
			var bizO = new DummyFilterStripBusinessObject();

			bizO.ModuleType = typeof(ZFilterModule);
			AssertEquals("ShouldAddUserDefinedFilters property should be true when ModuleType is type of ZFilterModule.", bizO.ShouldAddUserDefinedFilters, true);

			bizO.ModuleType = null;
			AssertEquals("ShouldAddUserDefinedFilters property should be false when ModuleType is null.", bizO.ShouldAddUserDefinedFilters, false);

			bizO.ModuleType = typeof(object);
			AssertEquals("ShouldAddUserDefinedFilters property should be false when ModuleType is not subclass of ZFilterModule.", bizO.ShouldAddUserDefinedFilters, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				AssertEquals("ShouldAddUserDefinedFilters property should be true when ModuleType is subclass of ZFilterModule.", filterBizo.ShouldAddUserDefinedFilters, true);
			}
		}

		public void TestHasUserDefinedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published!", true, true, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.DummyDependent, "Me other filters", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.DummyDependent, "Delivered!", true, true, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = filterBizo["[USR]Me filters"];
				AssertNotNull(filter);

				filter = filterBizo["[USR]Published!"];
				AssertNotNull(filter);

				filter = filterBizo["[USR]Me other filters!"];
				AssertNull(filter);

				filter = filterBizo["[USR]Delivered!"];
				AssertNull(filter);
			}

			var user = Factory.New<IGlbStaff>();
			((BusinessObject)user).FillWithValidTestData();
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = filterBizo["[USR]Me filters"];
				AssertNull(filter);

				filter = filterBizo["[USR]Published!"];
				AssertNotNull(filter);

				filter = filterBizo["[USR]Me other filters!"];
				AssertNull(filter);

				filter = filterBizo["[USR]Delivered!"];
				AssertNull(filter);
			}
		}

		public void TestHasUserDefinedFilters_NoParentModule()
		{
			var moduleFilter = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);
			moduleFilter.S9_ModuleID = "";
			moduleFilter.Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.ParentModule = null;
				((IFilterStripBusinessObjectInternals)filterBizo).LayoutContext = "";
				AssertNoExceptionThrown(delegate
				{
					filterBizo.RefreshUserDefinedFilters();
				});
			}
		}

		public void TestRefreshUserDefinedFilters_WhenNewFilterAdded_ShouldSetFilterBusinessObjectProperty()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);

			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var userDefinedFilter1 = filterBizo["[USR]Me filters"];

				AssertEquals("The filter strip business object should be set on all pre-existing user-defined filters when the filter strip business object is initialised. SAD!", filterBizo, userDefinedFilter1.FilterBusinessObject);

				FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "You filters", false, false, true);
				AssertNull("The user-defined filters haven't been refreshed yet, so the new filter shouldn't be in the list. SAD!", filterBizo["[USR]You filters"]);

				filterBizo.RefreshUserDefinedFilters();
				var userDefinedFilter2 = filterBizo["[USR]You filters"];
				AssertNotNull("The user-defined filters have been refreshed, so the new filter should be in the list. SAD!", userDefinedFilter2);
				AssertEquals("New user-defined filters that were added the list by refreshing should have their FilterBusinessObject property set. SAD!", filterBizo, userDefinedFilter2.FilterBusinessObject);
			}
		}

		public void TestRefreshUserDefinedFilters_OnlyDeletesCorrespondingDeletedUserDefinedFilters()
		{
			var filterA = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterA", true, true, true);
			var filterB = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterB", true, true, true);
			var filterC = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterC", true, true, true);
			var filterD = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.DummyDependent, "filterD", true, true, true);
			var filterE = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterE", true, true, true);

			filterC.S9_ModuleID = "";

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;

				var userDeFilterA = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterA)];
				var userDeFilterB = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterB)];
				var userDeFilterC = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterC)];
				var userDeFilterD = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterD)];
				var userDeFilterE = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterE)];

				AssertNull("Filter D doesn't exist in this module, and is already null", userDeFilterD);

				foreach (var filter in new[] { filterB, filterC, filterD, filterE })
				{
					filter.Delete();
				}

				Factory.Save();

				filterBizo.RefreshUserDefinedFilters();

				AssertNotNull("I wasnot deleted", userDeFilterA);

				foreach (var deleteTheFilter in new[] { userDeFilterB, userDeFilterC, userDeFilterE })
				{
					AssertNotNull("We were deleted", deleteTheFilter);
				}
			}
		}

		public void TestRefreshUserDefinedFilters_OnlyDeletesCorrespondingDeletedUserDefinedFilters_EvenWhenParentModuleIsNull()
		{
			var filterA = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterA", true, true, true);
			var filterB = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterB", true, true, true);
			var filterC = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterC", true, true, true);
			var filterD = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.DummyDependent, "filterD", true, true, true);
			var filterE = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "filterE", true, true, true);

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;

				var userDeFilterA = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterA)];
				var userDeFilterB = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterB)];
				var userDeFilterC = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterC)];
				var userDeFilterD = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterD)];
				var userDeFilterE = (ModuleUserDefinedFilter)filterBizo[ModuleUserDefinedFilter.GetPrefixedDescription(filterE)];

				AssertNull("Filter D doesn't exist in this module, and is already null", userDeFilterD);

				foreach (var filter in new[] { filterB, filterC, filterD, filterE })
				{
					filter.Delete();
				}

				Factory.Save();

				filterBizo.ParentModule = null;
				((IFilterStripBusinessObjectInternals)filterBizo).LayoutContext = "";
				AssertNoExceptionThrown(delegate
				{
					filterBizo.RefreshUserDefinedFilters();
				});

				foreach (var noTouchFilter in new[] { userDeFilterA, userDeFilterB, userDeFilterC, userDeFilterE })
				{
					AssertNotNull("We were not deleted, because our moduleID didn't match", noTouchFilter);
				}
			}
		}

		#endregion

		public void TestMaximumForFavoriteFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filter", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "My publish", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "dummy", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "dummy1", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "dummy2", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "dummy3", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "dummy4", true, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published!", true, true, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me other filters", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Delivered!", true, true, true);

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				foreach (var filter in filterBizo.Layouts)
				{
					filterBizo.AddOrRemoveFavoriteFilter(filter);
					Assert("Favorites should contain filter", filterBizo.FavoriteLayouts.Contains(filter));
				}
				AssertEquals("Favorites should contain all 10 filters", 10, filterBizo.FavoriteLayouts.Count);

				var issueFilter = Factory.NewWithValidTestData<StmModuleFilter>();
				issueFilter.S9_FilterName = "ErrorFilter";
				var expectedMessage = "Limit of 10 favorites reached." + System.Environment.NewLine + "You first need to remove a favorite if you want to add 'ErrorFilter'";
				AssertExceptionThrown<InvalidOperationException>("Should not allow more than 10 filters", expectedMessage, () => filterBizo.AddOrRemoveFavoriteFilter(issueFilter));
				foreach (var filter in filterBizo.Layouts)
				{
					filterBizo.AddOrRemoveFavoriteFilter(filter);
					Assert("Favorites shouldn't contain filter", !filterBizo.FavoriteLayouts.Contains(filter));
				}
				AssertEquals("Favorites should contain of 0 filters", 0, filterBizo.FavoriteLayouts.Count);
			}
		}

		public void TestRefreshUserDefinedFilters_NoExceptionIfRunWithoutQueryObjectType()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "UserDefinedBadness", true, true, true);
			var filterStrip = new DummyFilterStripBusinessObject();
			((IFilterStripBusinessObjectInternals)filterStrip).LayoutContext = "Dummy";

			AssertNull("Pre-condition: When filter strip bizo is used without a module it's Query Object Type isn't set.", filterStrip.QueryObjectType);
			AssertNoExceptionThrown("Expected no null reference exception when refreshing user defined filters without a QueryObjectType", () => filterStrip.RefreshUserDefinedFilters());
		}

		public void TestFilterRuleFilterBusinessObjects_WhenInFilterRuleMode_ShouldNotIncludeUnpublishedUserDefinedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "It's a secret", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "I declared it", true, true, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filters = module.FilterBusinessObject.ModuleFilters.Where(x => x is ModuleUserDefinedFilter);
				AssertContainsExactElementsInAnyOrder(new[] { "[USR]It's a secret", "[USR]I declared it" }, filters.Select(x => x.Description));
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				((IRelatedModuleFilterBusinessObject)module.FilterBusinessObject).IsInFilterRuleMode = true;
				var filters = module.FilterBusinessObject.ModuleFilters.Where(x => x is ModuleUserDefinedFilter);
				AssertContainsExactElementsInAnyOrder(new[] { "[USR]I declared it" }, filters.Select(x => x.Description));
			}
		}

		public void TestLayouts_PublishedOnly_ShouldNotIncludeUserDefinedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published!", true, true, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				AssertEquals(0, module.FilterBusinessObject.Layouts_PublishedOnly.Count);
			}
		}

		public void TestLayouts_UnpublishedOnly_ShouldNotIncludeUserDefinedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published!", true, true, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				AssertEquals(0, module.FilterBusinessObject.Layouts_UnpublishedOnly.Count);
			}
		}

		public void TestLayouts_ShouldNotIncludeFilterRuleLayouts()
		{
			FilterStripsTestHelper.DropStmModuleFilterRuleConstraints();
			var layout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				AssertEquals("Me filters", module.FilterBusinessObject.Layouts.Single().S9_FilterName);
			}

			layout.S9_FilterType = StmModuleFilterTypes.Codes.FilterRule;
			layout.Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				AssertNull(module.FilterBusinessObject.Layouts.SingleOrDefault());
			}
		}

		public void TestActiveFilterUsedByDefault_IsBlank()
		{
			var moduleFilters = FilterStripBizO.ModuleFilters;
			moduleFilters.AddTextFilter("Z0_Description", DummyBizoSchema.Z0_Description);
			var textFilter = (ModuleTextFilter)moduleFilters["Z0_Description"];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Z0_Description = ''", FilterStripBizO.Filter.LiteralTextADO);
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, FilterStripBizO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestActiveFilterUsedByDefault()
		{
			FilterStripBizO.SetActiveStatusFilter(DummyBizoSchema.Z0_IsValid, false);
			// Needed to poke filters into action
			var moduleFilters = FilterStripBizO.ModuleFilters;
			AssertEquals("Z0_IsValid = 0", FilterStripBizO.Filter.LiteralTextADO);
		}

		public void TestReversalLogicForActiveFilter()
		{
			FilterStripBizO.SetActiveStatusFilter(DummyBizoSchema.Z0_IsValid, true);
			// Needed to poke filters into action
			var moduleFilters = FilterStripBizO.ModuleFilters;
			AssertEquals("Z0_IsValid = 1", FilterStripBizO.Filter.LiteralTextADO);
		}

		public void TestActiveStatusFilterDefaultProperty()
		{
			FilterStripBizO.SetActiveStatusFilter(DummyBizoSchema.Z0_IsValid, false);
			// Needed to poke filters into action
			var moduleFilters = FilterStripBizO.ModuleFilters;
			var filter = (ModuleTextFilter)FilterStripBizO[FilterDescriptions.ActiveStatus];
			AssertEquals("Active", filter.DefaultProperty);
		}

		#region TestActiveStatusFilterMultilinguaCode

		public void TestActiveStatusFilterMultilingualCode()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.SetActiveStatusFilter(DummyBizoSchema.Z0_Bool, false);
			var filter = filterStripBizO[FilterDescriptions.ActiveStatus];
			AllLanguages.ForEach(savingLanguage =>
			{
				using (Res.TemporarilySwitchLanguage(savingLanguage))
				{
					AssertEquals(filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusActive].Code, ((ModuleTextFilter)filter).Property);

					var strip = filterStripBizO.FilterStrips.AddNew();
					strip.FilterDescription = filter.Description;

					var savedFilter = filterStripBizO.SaveLayout("savedFilter");

					strip.FilterDescription = "";
					strip.Delete();

					AllLanguages.ForEach(loadingLanguage =>
					{
						using (Res.TemporarilySwitchLanguage(loadingLanguage))
						{
							filterStripBizO.LoadLayout(savedFilter);
							var loadedFilter = (ModuleTextFilter)filterStripBizO[filter.Description];

							AssertEquals(filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusActive].Code, loadedFilter.Property);
						}
					});
				}
			});
		}

		#endregion

		#region TestActiveStatusFilterDefaultValue

		public void TestActiveStatusFilterMultilingualDefaultValue()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.SetActiveStatusFilter(DummyBizoSchema.Z0_Bool, false);
			var filter = filterStripBizO[FilterDescriptions.ActiveStatus] as ModuleTextFilter;
			AllLanguages.ForEach(savingLanguage =>
			{
				using (Res.TemporarilySwitchLanguage(savingLanguage))
				{
					AssertEquals(filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusActive].Code, filter.DefaultProperty);
					AssertEquals(filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusActive].Code, filter.Property);

					filter.Property = filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusAll].Code;
					filter.Clear();

					AssertEquals(filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusActive].Code, filter.DefaultProperty);
					AssertEquals(filterStripBizO.CancelledStatusList[FilterStripBusinessObject.StatusActive].Code, filter.Property);
				}
			});
		}

		#endregion

		#region TestActiveStatusQuery

		public void TestInactiveOnly()
		{
			AllLanguages.ForEach(lan => AssertActiveStatusFilterQuery(lan, FilterStripBusinessObject.StatusInactive, "Z0_IsValid = 1"));
		}

		public void TestActiveOnly()
		{
			AllLanguages.ForEach(lan => AssertActiveStatusFilterQuery(lan, FilterStripBusinessObject.StatusActive, "Z0_IsValid = 0"));
		}

		public void TestAll()
		{
			AllLanguages.ForEach(lan => AssertActiveStatusFilterQuery(lan, FilterStripBusinessObject.StatusAll, "Z0_IsValid = 0 OR Z0_IsValid = 1"));
		}

		void AssertActiveStatusFilterQuery(string language, MultilingualString activeStatusValue, string expectedLiteralTextADO)
		{
			using (Res.TemporarilySwitchLanguage(language))
			{
				FilterStripBizO.SetActiveStatusFilter(DummyBizoSchema.Z0_IsValid, false);
				// Needed to poke filters into action
				var moduleFilters = FilterStripBizO.ModuleFilters;
				var filter = (ModuleTextFilter)FilterStripBizO[FilterDescriptions.ActiveStatus];
				filter.Property = activeStatusValue;
				AssertEquals(expectedLiteralTextADO.ToUpper(), FilterStripBizO.Filter.LiteralTextADO.ToUpper());

				filter.Property = activeStatusValue.GetUnresolvedString();
				AssertEquals(expectedLiteralTextADO.ToUpper(), FilterStripBizO.Filter.LiteralTextADO.ToUpper());
			}
		}

		protected List<string> AllLanguages
		{
			get
			{
				if (allLanguages == null)
				{
					allLanguages = typeof(Enterprise.Core.SharedConstants.Languages)
					.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
					.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
					.Select(x => (string)x.GetRawConstantValue())
					.ToList();
				}
				return allLanguages;
			}
		}

		List<string> allLanguages;

		#endregion

		public void TestHasFiltersFromHelpers_NoCustomHelpers()
		{
			var bizO = new DummyFilterStripBusinessObject();
			AssertNull("QueryObjectType wasn't set (this can happen when creating a FilterStripBusinessObject without a module) so automatic filter strip helpers shouldn't have been used, and yet...", bizO["Milestone Date"]);

			bizO = new DummyFilterStripBusinessObject { QueryObjectType = typeof(DummyBusinessObject) };
			AssertNull("QueryObjectType didn't implement the interface necessary for the automatic workflow filter strip helper, so one shouldn't have been used, and yet...", bizO["Milestone Date"]);

			var type = ObjectFactory.GetType("IDummyWithWorkflow");
			bizO = new DummyFilterStripBusinessObject { QueryObjectType = type };
			AssertNotNull("QueryObjectType implements the interface necessary for the automatic workflow filter strip helper, so one have been used, and yet...", bizO["Milestone Date"]);
		}

		public void TestHasFiltersFromHelpers_WithCustomFilter()
		{
			var helper = new DummyFilterStripsHelper();

			var bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();
			bizO.AddHelperForTest(helper);
			AssertNull("BusinessObjectType wasn't set on the helper, so it should not have been used to create filter strips, and yet...", bizO["Awesomeness Factor"]);

			bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();
			helper.Initialise(typeof(DummyBusinessObject), Factory);
			bizO.AddHelperForTest(helper);
			AssertNull("BusinessObjectType was set to a type that doesn't implement the required interface, so the helper specified should not have been used to create filter strips, and yet...", bizO["Awesomeness Factor"]);

			bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();
			helper.Initialise(typeof(DummyBusinessObjectWithInterface), Factory);
			bizO.AddHelperForTest(helper);
			AssertNotNull("BusinessObjectType was set to a type that implements the required interface, so the helper specified should have been used to create filter strips, and yet...", bizO["Awesomeness Factor"]);
		}

		public void TestHasIndexSearchFiltersFromHelpers_WhenUseIndexSearch()
		{
			using (var mocker = new GlowIndexQueryEngineMock())
			{
				var helper = new DummyFilterStripsHelper();
				var bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();

				bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();
				AssertEquals(SearchType.Sql, bizO.SearchType);
				helper.Initialise(typeof(DummyBusinessObjectWithInterface), Factory);
				bizO.AddHelperForTest(helper);
				AssertNotNull("BusinessObjectType was set to a type that implements the required interface, so the helper specified should have been used to create filter strips, and yet...", bizO["Awesomeness Factor"]);

				bizO.IndexSearchFields = GetTestSearchFields();
				bizO.SearchType = SearchType.Index;

				AssertEquals(1, bizO.IndexSearchFields.DefaultHiddenIndexSearchFields.Where(field => field.FieldName == IndexSearchFilterHelper.DefaultHiddenPrefix + "TestFilter").Count());
				var filter = (IndexSearchModuleTextFilter)bizO[IndexSearchFilterHelper.DefaultHiddenPrefix + "TestFilter"];
				AssertNotNull("Has index search filters from helpers", filter);
				AssertEquals(DummyFilterStripsHelper.TestFilter.Description, filter.Category.Description);
			}
		}

		SearchFieldCollection GetTestSearchFields()
		{
			var field1 = SearchField.Create(IndexSearchFilterHelper.DefaultHiddenPrefix + "TestFilter", "Test Filter");
			return new SearchFieldCollection("IDummyBusinessObject", new SearchField[] { field1 });
		}

		public void TestHasFiltersFromHelpers_AddingExternalCustomHelpers()
		{
			var helper = new DummyFilterStripsHelper();

			var bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();
			helper.Initialise(typeof(DummyBusinessObject), Factory);
			AssertNull("BusinessObjectType wasn't set on the helper, so it should not have been used to create filter strips, and yet...", bizO["Awesomeness Factor"]);

			bizO = new DummyFilterStripBusinessObjectForAddedHelperTesting();
			bizO.CustomFilterStripsHelpers.Add(helper);
			helper.Initialise(typeof(DummyBusinessObject), Factory);
			AssertNull("BusinessObjectType was set to a type that doesn't implement the required interface, so the helper specified should not have been used to create filter strips, and yet...", bizO["Awesomeness Factor"]);
		}

		#region TestIsExpensiveQuery

		public void TestIsExpensiveQuery()
		{
			var textFilter = new ModuleTextFilter("text", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(textFilter);

			textFilter.IsActive = true;
			AssertEquals("Precondition", false, textFilter.IsExpensiveQuery);
			AssertEquals(false, FilterStripBizO.IsExpensiveQuery);

			textFilter.Property = "some value";
			textFilter.ComparisonOperator = "contains";
			AssertEquals("Precondition", true, textFilter.IsExpensiveQuery);
			AssertEquals(true, FilterStripBizO.IsExpensiveQuery);
		}

		public void TestIsExpensiveQueryWithStripThatIsAlwaysExpensive()
		{
			var textFilter = new ModuleTextFilterAlwaysExpensive("text", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(textFilter);

			textFilter.IsActive = true;
			textFilter.Property = "some value";
			AssertEquals("Precondition", true, textFilter.IsExpensiveQuery);
			AssertEquals(true, FilterStripBizO.IsExpensiveQuery);

			textFilter.Property = "";
			AssertEquals("Precondition - always expensive", true, textFilter.IsExpensiveQuery);
			AssertEquals("Precondition", true, textFilter.IsEmpty);
			AssertEquals("Excluded from calculation of being expensive if IsEmpty", false, FilterStripBizO.IsExpensiveQuery);
		}

		class ModuleTextFilterAlwaysExpensive : ModuleTextFilter
		{
			public ModuleTextFilterAlwaysExpensive(ZString description, SchemaStringColumn filterColumn)
				: base(description, filterColumn)
			{ }

			public override bool IsExpensiveQuery
			{
				get { return true; }
			}
		}

		#endregion

		public void TestAreModuleFiltersLoaded()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				Assert(!module.FilterBusinessObject.AreModuleFiltersLoaded);

				module.FilterBusinessObject.LoadModuleFilters();

				Assert(module.FilterBusinessObject.AreModuleFiltersLoaded);
			}
		}

		#region TestLayouts_PublishedOnly

		public void TestLayouts_PublishedOnly()
		{
			var unpublishedFilter = FilterStripBizO.Layouts.AddNew();
			var publishedFilter = FilterStripBizO.Layouts.AddNew();
			unpublishedFilter.S9_IsPublished = false;
			publishedFilter.S9_IsPublished = true;

			AssertCollectionNotContains(unpublishedFilter, FilterStripBizO.Layouts_PublishedOnly);
			AssertCollectionContains(publishedFilter, FilterStripBizO.Layouts_PublishedOnly);
		}

		#endregion

		#region TestLayouts_UnpublishedOnly

		public void TestLayouts_UnpublishedOnly()
		{
			var unpublishedFilter = FilterStripBizO.Layouts.AddNew();
			var publishedFilter = FilterStripBizO.Layouts.AddNew();
			unpublishedFilter.S9_IsPublished = false;
			publishedFilter.S9_IsPublished = true;

			AssertCollectionContains(unpublishedFilter, FilterStripBizO.Layouts_UnpublishedOnly);
			AssertCollectionNotContains(publishedFilter, FilterStripBizO.Layouts_UnpublishedOnly);
		}

		#endregion

		#region TestLayoutsHelperSetter

		public void TestLayoutsHelperSetter()
		{
			FilterStripBizO.LayoutsHelperAsDummy = null;
			AssertNotNull(FilterStripBizO.LayoutsHelperAsDummy);

			var helper = new DummyFilterStripLayoutsHelper();
			FilterStripBizO.LayoutsHelperAsDummy = helper;
			AssertEquals(helper, FilterStripBizO.LayoutsHelperAsDummy);

			var layoutsHash = FilterStripBizO.Layouts.GetHashCode();
			FilterStripBizO.LayoutsHelperAsDummy = new DummyFilterStripLayoutsHelper();
			AssertNotEquals("Setting a layouts helper should reset the Layouts collection.", layoutsHash, FilterStripBizO.Layouts.GetHashCode());
		}

		#endregion

		#region TestCreatingModuleFiltersCallsOnModuleFiltersCreated

		[ExpectNoExceptions]
		public void TestCreatingModuleFiltersCallsOnModuleFiltersCreated()
		{
			var mock = new Mock<DummyFilterStripBusinessObject> { CallBase = true };
			var filterStripBizOFromMock = mock.Object;

			filterStripBizOFromMock.AddModuleFilterForTest(new ModuleTextFilter("Dummy", DummyBizoSchema.Z0_Description));

			var accessFilterToCreateModules = filterStripBizOFromMock["Dummy"];
			mock.Protected().Verify("OnModuleFiltersCreated", Times.Once());
		}

		#endregion

		#region TestSaveLastUsedLayoutName

		public void TestSaveLastUsedLayoutName()
		{
			var scoobyLayout = TestDataHelper.NewLayoutWithUserData("Scooby");
			var shaggyLayout = TestDataHelper.NewLayoutWithUserData("Shaggy");
			var lastUsedLayout = TestDataHelper.NewLastUsedLayout(scoobyLayout);
			Factory.Save();

			AssertEquals("Precondition", "Scooby", FilterStripBizO.LastUsedLayout.S9_FilterName);
			AssertEquals("Precondition", scoobyLayout.PK, lastUsedLayout.SD_GuidValue);

			FilterStripBizO.SaveLastUsedLayout(shaggyLayout.PK);
			AssertEquals("Shaggy", FilterStripBizO.LastUsedLayout.S9_FilterName);
			AssertEquals(shaggyLayout.PK, lastUsedLayout.SD_GuidValue);

			FilterStripBizO.SaveLastUsedLayout(ZGuid.Empty);
			AssertEquals("Shaggy", FilterStripBizO.LastUsedLayout.S9_FilterName);
			AssertEquals(shaggyLayout.PK, lastUsedLayout.SD_GuidValue);
		}

		public void TestGetLastUsedLayoutName_SD_Type()
		{
			var scoobyLayout = TestDataHelper.NewLayoutWithUserData("Scooby");
			var colorSchemeLayout = TestDataHelper.NewLastUsedLayout(scoobyLayout);
			colorSchemeLayout.SD_Type = "_CS";
			Factory.Save();

			AssertNotNull("can find _CS StmData, even though they are used for color schemes", FilterStripBizO.LastUsedLayout);
		}

		#endregion

		#region TestSaveLastUsedLayoutDoesNotSaveLayoutWithNoStrips

		public void TestSaveLastUsedLayoutDoesNotSaveLayoutWithNoStrips()
		{
			var layoutName = "someLayout";

			var someFilter = new ModuleTextFilter("someFilter", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(someFilter);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = someFilter.Description;

			AssertEquals("Precondition: Amount of FilterStrips in FBO", 1, FilterStripBizO.FilterStrips.Count);
			var layout = FilterStripBizO.SaveLayout(layoutName);

			FilterStripBizO.FilterStrips.RemoveAll();
			AssertEquals("Precondition: We are trying to save Layout without FilterStrips", 0, FilterStripBizO.FilterStrips.Count);
			AssertNull(FilterStripBizO.SaveLayout(layoutName));

			FilterStripBizO.LoadLayout(layout);
			AssertEquals("Amount of loaded FilterStrips", 1, FilterStripBizO.FilterStrips.Count);
		}

		#endregion

		#region TestLayoutDoesNotAddUnexistingFilters

		public void TestLayoutDoesNotAddUnexistingFilters()
		{
			var layoutName = "someLayout";

			var someFilter = new ModuleTextFilter("someFilter", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(someFilter);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = someFilter.Description;

			AssertEquals("Precondition: Amount of FilterStrips in FBO", 1, FilterStripBizO.FilterStrips.Count);
			var layout = FilterStripBizO.SaveLayout(layoutName);

			FilterStripBizO.FilterStrips.RemoveAll();

			FilterStripBizO.LoadLayout(layout);
			AssertEquals("Amount of loaded FilterStrips", 1, FilterStripBizO.FilterStrips.Count);

			var newFilterStripBizO = new DummyFilterStripBusinessObject();
			newFilterStripBizO.LoadLayout(layout);
			AssertEquals("Amount of loaded FilterStrips", 0, newFilterStripBizO.FilterStrips.Count);
		}

		#endregion

		#region TestLayoutWillAddCodeAndDescriptionFilters
		public void TestLayoutWillAddCodeAndDescriptionFilters()
		{
			var layoutName = "someLayout";

			var someFilter = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			FilterStripBizO.AddModuleFilterForTest(someFilter);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = someFilter.Code;

			AssertEquals("Precondition: Amount of FilterStrips in FBO", 1, FilterStripBizO.FilterStrips.Count);
			var layout = FilterStripBizO.SaveLayout(layoutName);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var newFilterStripBizO = new DummyFilterStripBusinessObject();
				newFilterStripBizO.ParentModule = module;
				newFilterStripBizO.LoadLayout(layout);
				AssertEquals("Amount of loaded FilterStrips", 2, newFilterStripBizO.FilterStrips.Count);
			}
		}

		#endregion

		#region TestSaveLastUsedLayoutNameUsesHelperToDetermineCurrentUser

		public void TestSaveLastUsedLayoutNameUsesHelperToDetermineCurrentUser()
		{
			var userPk = ZGuid.NewZGuid();

			var layoutHelper = new DummyFilterStripLayoutsHelper();
			layoutHelper.CurrentUserPkForTest = userPk;
			FilterStripBizO.LayoutsHelper = layoutHelper;

			var created = TestDataHelper.NewLayoutWithUserData("Optimus Prime", userPk, GlbStaffSchema.Constants.Prefix);
			Factory.Save();
			AssertNull("Precondition", Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, userPk)));

			FilterStripBizO.SaveLastUsedLayout(created.PK);
			AssertNotNull(Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, userPk)));

			FilterStripBizO.LastUsedLayout = null;
			AssertEquals("Optimus Prime", FilterStripBizO.LastUsedLayout.S9_FilterName);
		}

		#endregion

		#region TestResetLastUsedLayout

		public void TestResetLastUsedLayout()
		{
			var scoobyLayout = TestDataHelper.NewLayoutWithUserData("Scooby");
			var lastUsedFilterLayout = TestDataHelper.NewLastUsedLayout(scoobyLayout);
			Factory.Save();

			AssertEquals("Precondition", "Scooby", FilterStripBizO.LastUsedLayout.S9_FilterName);
			AssertEquals("Precondition", scoobyLayout.PK, lastUsedFilterLayout.SD_GuidValue);

			FilterStripBizO.ResetLastUsedLayout();
			AssertNull("", FilterStripBizO.LastUsedLayout);
			AssertEquals(true, lastUsedFilterLayout.IsDeleted);
		}

		#endregion

		#region grid colour

		public void TestGetFilterStripsHooksEvent()
		{
			var filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "it";
			Factory.Save();

			FilterStripBizO.LayoutChanged += new EventHandler(FilterStripBizO_LayoutChanged);
			filter = FilterStripBizO.Layouts.AddNew();
			filter.S9_FilterName = "it";
			var strips = FilterStripBizO.GetFilterStrips(filter);
			strips.ModuleFilterChanged += new EventHandler(FilterStripBizO_LayoutChanged);
			var strip = strips.AddNew("it");
			var numberFilter = strip.ModuleFilters.AddNumberRangeFilter("it", delegate
			{ return new ZQuery(); });
			numberFilter.Decimals = 3;
			strip.FilterDescription = "it2";
			strip.FilterDescription = "it";
			numberFilter.Decimals = 5;
			filter.S9_IsPublished = true;
			AssertEquals(true, eventFired);
		}

		void FilterStripBizO_LayoutChanged(object sender, EventArgs e)
		{
			eventFired = true;
		}

		bool eventFired;

		#endregion

		#region TestHookOnModuleFiltersCreated

		public void TestHookOnModuleFiltersCreated()
		{
			FilterStripBizO.ModuleFiltersCreated += FilterStripBizO_ModuleFiltersCreated;
			var filters = FilterStripBizO.ModuleFilters;
		}

		void FilterStripBizO_ModuleFiltersCreated(IFilterStripBusinessObject filterStripBizO)
		{
			AssertNotNull(filterStripBizO);
		}

		#endregion

		#region TestOnModuleFiltersCreatedHooks

		public void TestOnModuleFiltersCreatedHooks()
		{
			bool hook1IsCalled = false;
			bool hook2IsCalled = false;
			FilterStripBizO.AddModuleFiltersCreatedHook((x) => hook1IsCalled = true);
			FilterStripBizO.AddModuleFiltersCreatedHook((x) => hook2IsCalled = true);
			var filters = FilterStripBizO.ModuleFilters;

			Assert(hook1IsCalled);
			Assert(hook2IsCalled);
		}

		#endregion

		public void TestReportErrorIfNothingToSave()
		{
			IModifyModuleAndGridLayout savable = FilterStripBizO;
			AssertEquals("All filters are empty, there is nothing to save.", savable.ValidateAndGetErrorsForSavingLayout());
		}

		public void TestReportErrorIfNoFiltersAreActive()
		{
			FilterStripBizO.AddModuleFilterForTest(new ModuleTextFilter("text filter", DummyBizoSchema.Z0_Description));
			IModifyModuleAndGridLayout savable = FilterStripBizO;
			AssertEquals("All filters are empty, there is nothing to save.", savable.ValidateAndGetErrorsForSavingLayout());
		}

		#region TestClearLayoutsCache()

		public void TestClearLayoutsCache()
		{
			var layouts = FilterStripBizO.Layouts;
			AssertEquals("Precondition", layouts, FilterStripBizO.Layouts);

			((IFilterStripBusinessObjectInternals)FilterStripBizO).ClearLayoutsCache();
			AssertNotEquals(layouts, FilterStripBizO.Layouts);
		}

		#endregion

		#region TestOrFilters

		public void TestOrFilters()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "description1";
			dummy2.Z0_Description = "description2";
			dummy1.Z0_Code = "code1";
			dummy2.Z0_Code = "code2";

			var descFilter = new ModuleTextFilter("description", DummyBizoSchema.Z0_Description);
			var codeFilter = new ModuleTextFilter("code", DummyBizoSchema.Z0_Code);
			descFilter.IsActive = true;
			codeFilter.IsActive = true;
			FilterStripBizO.AddModuleFilterForTest(descFilter);
			FilterStripBizO.AddModuleFilterForTest(codeFilter);

			descFilter.Property = "description1";
			AssertExpectedDummies(dummy1, dummy2, true, false);

			codeFilter.Property = "code2";
			AssertExpectedDummies(dummy1, dummy2, false, false);

			descFilter.OrCategory = FilterOrCategory.Blue;
			codeFilter.OrCategory = FilterOrCategory.Green;
			AssertExpectedDummies(dummy1, dummy2, false, false); // filters are in different categories and should be AND'ed

			descFilter.OrCategory = FilterOrCategory.Red;
			codeFilter.OrCategory = FilterOrCategory.Red;
			AssertExpectedDummies(dummy1, dummy2, true, true); // filters are grouped in the red category and should be OR'd
		}

		void AssertExpectedDummies(DummyBusinessObject dummy1, DummyBusinessObject dummy2, bool expectDummy1, bool expectDummy2)
		{
			var dummies = new DummyBusinessObjectCollection(Factory);
			dummies.LoadWithMoreFiltering(FilterStripBizO.Filter);

			AssertEquals(expectDummy1, dummies.FindByPK(dummy1.PK) != null);
			AssertEquals(expectDummy2, dummies.FindByPK(dummy2.PK) != null);
		}

		#endregion

		#region TestDuplicatedFiltersAreValidated

		public void TestDuplicatedFiltersAreValidated()
		{
			var someFilter = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_Code);
			FilterStripBizO.ModuleFilters.AddFilter(someFilter);
			someFilter.IsActive = true;

			var someFilterDuplicated = (ModuleTextFilter)FilterStripBizO.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Some Filter");
			someFilterDuplicated.IsActive = true;
			someFilterDuplicated.PropertyValidation = info => info.AddError("Some Error");

			AssertEquals("Precondition", false, FilterStripBizO.HasErrors);

			FilterStripBizO.RunPreSaveValidation();
			AssertEquals(true, FilterStripBizO.HasErrors);
			AssertNoErrors(someFilter);
			AssertEquals(true, someFilterDuplicated.HasErrors);
		}

		#endregion

		#region TestExclusiveFiltersValidation

		public void TestExclusiveFiltersValidation()
		{
			var someFilter = new ModuleTextFilter("Some Filter", DummyBizoSchema.Z0_VarCharMax) { Property = "qwerty" };
			FilterStripBizO.ModuleFilters.AddFilter(someFilter);
			someFilter.IsActive = true;

			var exclusiveFilter = new ModuleTextFilter("Exclusive Filter", DummyBizoSchema.Z0_Code) { IsExclusiveHelper = true };
			FilterStripBizO.ModuleFilters.AddFilter(exclusiveFilter);
			exclusiveFilter.IsActive = true;

			var exclusiveFilter2 = (ModuleTextFilter)FilterStripBizO.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Exclusive Filter");
			exclusiveFilter2.IsActive = true;

			exclusiveFilter.Property = "abc";

			AssertEquals("Precondition", false, FilterStripBizO.HasErrors);
			AssertEquals("Precondition", false, FilterStripBizO.HasWarnings);

			FilterStripBizO.RunPreSaveValidation();

			AssertEquals(false, FilterStripBizO.HasErrors);
			AssertEquals(true, FilterStripBizO.HasWarnings);
			AssertEquals(true, someFilter.HasWarnings);

			exclusiveFilter2.Property = "bcd";

			FilterStripBizO.RunPreSaveValidation();

			AssertEquals(true, FilterStripBizO.HasErrors);
			AssertEquals(true, FilterStripBizO.HasWarnings);
			AssertEquals(true, someFilter.HasWarnings);
			AssertEquals(true, exclusiveFilter.HasErrors);
			AssertEquals(true, exclusiveFilter2.HasErrors);

			exclusiveFilter.OrCategory = FilterOrCategory.Red;
			exclusiveFilter2.OrCategory = FilterOrCategory.Red;

			FilterStripBizO.RunPreSaveValidation();

			AssertEquals(false, FilterStripBizO.HasErrors);
			AssertEquals(true, FilterStripBizO.HasWarnings);
			AssertEquals(true, someFilter.HasWarnings);

			exclusiveFilter.Property = ZString.Empty;
			exclusiveFilter2.Property = ZString.Empty;

			FilterStripBizO.RunPreSaveValidation();

			AssertEquals(false, FilterStripBizO.HasErrors);
			AssertEquals(false, FilterStripBizO.HasWarnings);
		}

		#endregion

		#region TestChangeLayoutClearsChildrenOnFilterStripBizO

		public void TestChangeLayoutClearsChildrenOnFilterStripBizO()
		{
			var layoutName = "someLayout";
			var someFilter = new ModuleTextFilter("someFilter", DummyBizoSchema.GenericStringSchemaColumn);
			FilterStripBizO.AddModuleFilterForTest(someFilter);

			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = someFilter.Description;
			someFilter.Property = "Populated Property for validation";
			someFilter.PropertyValidation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError("Test error notification");
				}
			};

			var layout = FilterStripBizO.SaveLayout(layoutName);
			FilterStripBizO.FilterStrips.RemoveAll();

			someFilter.Property = ZString.Empty;
			someFilter.Validation.ValidateProperty();
			AssertEquals(true, FilterStripBizO.HasErrors);

			FilterStripBizO.LoadLayout(layout);
			Assert(!FilterStripBizO.FilterStrips.HasNotifications());
		}

		#endregion

		#region TestSaveLastUsedLayoutNameExceptionHandling

		public void TestSaveLastUsedLayoutNameExceptionHandling()
		{
			var filterStripBizo = new DummyFilterStripBusinessObject();
			((IFilterStripBusinessObjectInternals)filterStripBizo).LayoutContext = "ExceptionHandlingTest";
			var layout = filterStripBizo.Layouts.AddNew();
			layout.S9_FilterName = "Layout1";
			filterStripBizo.LastUsedLayoutFactory.RefreshEnabled = false;

			filterStripBizo.ResetLastUsedLayout(); // Calls GetLastUsedLayoutName() and stores query in factoryOverride cache

			// Save same layout into db outside fo current factories
			TestConnection.ExecuteNonQuery(
				string.Format(
					"insert into dbo.StmData(SD_PK, SD_Name, SD_Type, SD_Owner, SD_DepartmentGuid) values(newid(), 'ExceptionHandlingTest', '', '{0}', '{1}')",
					filterStripBizo.LayoutsHelper.CurrentUserPk,
					EnvProxy.Instance.CurrentCompany.PK
				));

			AssertNoExceptionThrown("Should save layout and handle duplicate error", () => filterStripBizo.SaveLastUsedLayout(layout.PK));
		}

		#endregion

		#region TestGetFilterStripsWithNullModuleFiltersCollectionBlowsUp

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetFilterStripsWithNullModuleFiltersCollectionBlowsUp()
		{
			FilterStripBizO.ReadFilterStripsFromXml(new LayoutsTestDataHelper(Factory).NewLayout("bla"), null, new FilterStripLayoutsHelper());
		}

		#endregion

		#region Serialisation

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetFilterStripsWithNullLayoutsHelperBlowsUp()
		{
			FilterStripBizO.ReadFilterStripsFromXml(new LayoutsTestDataHelper(Factory).NewLayout("bla"), new ModuleFilterCollection(), null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestWriteFilterStripsToXmlWithNullFilterStripsBlowsUp()
		{
			FilterStripBizO.WriteFilterStripsToXml(new LayoutsTestDataHelper(Factory).NewLayout("bla"), null, new FilterStripLayoutsHelper());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestWriteFilterStripsToXmlWithNullLayoutsHelperBlowsUp()
		{
			FilterStripBizO.WriteFilterStripsToXml(new LayoutsTestDataHelper(Factory).NewLayout("bla"), new FilterStripCollection(new ModuleFilterCollection()), null);
		}

		/// <summary>
		/// This is the main test that ensures serialization / deserialization of layouts works.
		/// </summary>
		public void TestReadFilterStripsFromXml_and_WriteFilterStripsToXml()
		{
			// serialize everything

			var moduleFilters = new ModuleFilterCollection();
			moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);
			moduleFilters.AddNumberRangeFilter("number range filter", DummyBizoSchema.Z0_Decimal);

			var strips = new FilterStripCollection(moduleFilters);
			var textStrip = strips.AddNew("text filter");
			var numberStrip = strips.AddNew("number range filter");

			var moduleTextFilter = (ModuleTextFilter)textStrip.CurrentModuleFilter;
			moduleTextFilter.Property = "Dolce";
			moduleTextFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var moduleNumberFilter = (ModuleNumberRangeFilter)numberStrip.CurrentModuleFilter;
			moduleNumberFilter.Property1 = 18M;
			moduleNumberFilter.Property2 = 29M;

			var layout = new LayoutsTestDataHelper(Factory).NewLayout("bla");
			FilterStripBizO.WriteFilterStripsToXml(layout, strips, new FilterStripLayoutsHelper());
			Factory.Save();

			// deserialize everything

			var newFactory = new BusinessObjectFactory();
			var newModuleFilters = new ModuleFilterCollection();
			newModuleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);
			newModuleFilters.AddNumberRangeFilter("number range filter", DummyBizoSchema.Z0_Decimal);
			var layoutInNewFactory = newFactory.Load<StmModuleFilter>(layout.PK);

			var deserialisedStrips = FilterStripBizO.ReadFilterStripsFromXml(layoutInNewFactory, newModuleFilters, new FilterStripLayoutsHelper());

			var deserialisedModuleTextFilter = (ModuleTextFilter)deserialisedStrips[0].CurrentModuleFilter;
			var deserialisedModuleNumberFilter = (ModuleNumberRangeFilter)deserialisedStrips[1].CurrentModuleFilter;

			AssertEquals(2, deserialisedStrips.Count);

			AssertEquals("text filter", deserialisedStrips[0].FilterDescription);
			AssertEquals("Dolce", deserialisedModuleTextFilter.Property);
			AssertEquals(SQLComparisonOperator.Contains, deserialisedModuleTextFilter.SqlComparisonOperator);

			AssertEquals("number range filter", deserialisedStrips[1].FilterDescription);
			AssertEquals(18M, deserialisedModuleNumberFilter.Property1);
			AssertEquals(29M, deserialisedModuleNumberFilter.Property2);
		}

		public void TestReadFilterStripsFromXmlDoesNotBlowUpOnBadXml()
		{
			var moduleFilters = new ModuleFilterCollection();
			var textFilter = moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);
			textFilter.IsActive = true;

			var layout = new LayoutsTestDataHelper(Factory).NewLayout("bla");
			layout.S9_FilterData = new ZBlob();
			Factory.Save();

			var strips = FilterStripBizO.ReadFilterStripsFromXml(layout, new ModuleFilterCollection(), new FilterStripLayoutsHelper());
			AssertEquals("Should not blow up when deserialising bad xml.", 0, strips.Count);
		}

		public void TestGetOrCreateLayoutUserDataForColourScheme()
		{
			var layout = new LayoutsTestDataHelper(Factory).NewLayout("bla");
			AssertNull("Precondition", FilterStripBizO.GetLayoutUserDataForColorScheme(layout));
			var data = Factory.New<StmModuleFilterUserData>();
			data.S0_S9 = layout.PK;
			Factory.Save();
			AssertNotNull("Post condition", FilterStripBizO.GetLayoutUserDataForColorScheme(layout));
			data.S0_RelatedEntityID = ZGuid.NewZGuid();
			AssertNotNull("Post condition", FilterStripBizO.GetLayoutUserDataForColorScheme(layout));
		}

		public void TestReplaceStripsWithOthers()
		{
			var filterBizo1 = new DummyFilterBusinessObject();
			filterBizo1.AddTextFilterStrip("Z0_Description", "Moosay");

			var filterBizo2 = new DummyFilterBusinessObject();
			filterBizo2.AddTextFilterStrip("Z0_Code", "Greeeaaaahhhh");

			filterBizo1.CopyDetailsAndFiltersFrom(filterBizo2);
			AssertEquals(1, filterBizo1.FilterStrips.Count);
			AssertEquals("Greeeaaaahhhh", ((ModuleTextFilter)filterBizo1.ActiveModuleFilters.Single()).Property);

			AssertEquals(1, filterBizo2.FilterStrips.Count);
			AssertEquals("Greeeaaaahhhh", ((ModuleTextFilter)filterBizo2.ActiveModuleFilters.Single()).Property);
		}

		public void TestCopyDetailsAndFiltersFrom_IsInFilterRuleMode()
		{
			var filterBizo1 = new DummyFilterBusinessObject { IsInFilterRuleMode = false };
			var filterBizo2 = new DummyFilterBusinessObject { IsInFilterRuleMode = true };

			filterBizo1.CopyDetailsAndFiltersFrom(filterBizo2);
			AssertEquals(true, filterBizo1.IsInFilterRuleMode);
		}

		public void TestModuleFiltersHaveRefrenceTo_TheirParentFilterBusinessObject()
		{
			var filterBizo = new DummyFilterBusinessObject();

			filterBizo.AddTextFilterStrip("Z0_Description", "Test description");
			filterBizo.AddTextFilterStrip("Z0_Description", "Test description");
			filterBizo.AddTextFilterStrip("Z0_Code", "Test  Code");
			filterBizo.FilterStrips.AddNew("Custom SQL Filter");
			filterBizo.FilterStrips.AddNew("Custom SQL Filter");

			foreach (var moduleFilter in filterBizo.ActiveModuleFilters)
			{
				AssertEquals("Each module filter should have refrence to it's parent filter business object ", moduleFilter.FilterBusinessObject, filterBizo);
			}
		}

		public void TestReplaceStripsWithOthers_WithDifferentTypeOfFilterStripBusinessObject_ShouldThrowException()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var dummyFilterBizo = new DummyFilterBusinessObject();
				AssertExceptionThrown<InvalidOperationException>(() => module.FilterBusinessObject.CopyDetailsAndFiltersFrom(dummyFilterBizo));
			}
		}

		#endregion

		#region TestIsFilterStripsHelperExcluded

		public void TestIsFilterStripsHelperExcluded()
		{
			var bizo = new DummyFilterStripBusinessObjectForAddedHelperTesting();
			bizo.AddHelperTypeToExcludeFromAutomaticAddingOfFilters(ObjectFactory.GetType("IBMFilterStripsHelper"));

			AssertEquals(false, bizo.IsFilterStripsHelperExcluded(typeof(DummyFilterStripsHelper)));

			bizo.AddHelperTypeToExcludeFromAutomaticAddingOfFilters(typeof(IFilterStripsHelper));

			AssertEquals(true, bizo.IsFilterStripsHelperExcluded(typeof(DummyFilterStripsHelper)));
		}

		#endregion

		#region TestIsSystemDefinedStatus

		public void TestIsSystemDefinedFilterForAll()
		{
			DummyBusinessObject bizo1;
			DummyBusinessObject bizo2;
			PrepareTestDataForTestingIsSystemDefinedFilter(out bizo1, out bizo2);
			DummyBusinessObject[] bizoData;
			DummyBusinessObjectCollection bizoDataCollection;
			DoFilter(BizObjectForTest.DefinedStatusAllCodeForTest, out bizoData, out bizoDataCollection);

			AssertCollectionContains("The data which is system defined should be retrieved", bizo1, bizoData);
			AssertCollectionContains("The data which is not system defined should be retrieved", bizo2, bizoData);
			AssertCollectionContains("The data which is system defined should be retrieved", bizo1, bizoDataCollection);
			AssertCollectionContains("The data which is not system defined should be retrieved", bizo2, bizoDataCollection);
		}

		public void TestIsSystemDefinedFilterForSystem()
		{
			DummyBusinessObject bizo1;
			DummyBusinessObject bizo2;
			PrepareTestDataForTestingIsSystemDefinedFilter(out bizo1, out bizo2);
			DummyBusinessObject[] bizoData;
			DummyBusinessObjectCollection bizoDataCollection;
			DoFilter(BizObjectForTest.DefinedStatusSystemCodeForTest, out bizoData, out bizoDataCollection);

			AssertCollectionContains("The data which is system defined should be retrieved", bizo1, bizoData);
			AssertCollectionNotContains("The data which is not system defined should not be retrieved", bizo2, bizoData);
			AssertCollectionContains("The data which is system defined should be retrieved", bizo1, bizoDataCollection);
			AssertCollectionNotContains("The data which is not system defined should not be retrieved", bizo2, bizoDataCollection);
		}

		public void TestIsSystemDefinedFilterForNotSystem()
		{
			DummyBusinessObject bizo1;
			DummyBusinessObject bizo2;
			PrepareTestDataForTestingIsSystemDefinedFilter(out bizo1, out bizo2);
			DummyBusinessObject[] bizoData;
			DummyBusinessObjectCollection bizoDataCollection;
			DoFilter(BizObjectForTest.DefinedStatusNotSystemCodeForTest, out bizoData, out bizoDataCollection);

			AssertCollectionNotContains("The data which is system defined should not be retrieved", bizo1, bizoData);
			AssertCollectionContains("The data which is not system defined should be retrieved", bizo2, bizoData);
			AssertCollectionNotContains("The data which is system defined should not be retrieved", bizo1, bizoDataCollection);
			AssertCollectionContains("The data which is not system defined should be retrieved", bizo2, bizoDataCollection);
		}

		void PrepareTestDataForTestingIsSystemDefinedFilter(out DummyBusinessObject bizo1, out DummyBusinessObject bizo2)
		{
			bizo1 = Factory.New<DummyBusinessObject>();
			bizo2 = Factory.New<DummyBusinessObject>();

			bizo1.Z0_IsSystem = true;
			bizo1.Z0_Code = "bizo1";

			bizo2.Z0_IsSystem = false;
			bizo2.Z0_Code = "bizo2";
		}

		void DoFilter(string filterProperty, out DummyBusinessObject[] bizoData, out DummyBusinessObjectCollection bizoDataCollection)
		{
			var filterBizo = new DummyFilterStripBusinessObject();
			filterBizo.QueryObjectType = typeof(DummyBusinessObject);
			var isSystemFilter = (ModuleTextFilter)filterBizo["Is System Defined"];
			isSystemFilter.IsActive = true;
			isSystemFilter.Property = filterProperty;

			bizoData = Factory.Load<DummyBusinessObject>(filterBizo.Filter);
			bizoDataCollection = new DummyBusinessObjectCollection(Factory, filterBizo.Filter);
			bizoDataCollection.Load();
		}

		public void TestIsSystemDefinedFilterIsAlwaysApplied()
		{
			FilterStripBizO.IsSystemDefinedStatusFilterVisibilityOverride = FilterVisibility.AlwaysApplied;
			FilterStripBizO.QueryObjectType = typeof(DummyBusinessObject);
			var filter = (ModuleTextFilter)FilterStripBizO["Is System Defined"];
			AssertEquals("The visibility should be true", FilterVisibility.AlwaysApplied, filter.Visibility);
		}

		public void TestIsSystemDefinedFilterIsNotAlwaysApplied()
		{
			FilterStripBizO.QueryObjectType = typeof(DummyBusinessObject);
			var filter = (ModuleTextFilter)FilterStripBizO["Is System Defined"];
			AssertEquals("The visibility should be false", FilterVisibility.Visible, filter.Visibility);
		}
		FilterStripBusinessObjectForTest BizObjectForTest => filterStripBusinessObjectForTest ?? (filterStripBusinessObjectForTest = new FilterStripBusinessObjectForTest());
		FilterStripBusinessObjectForTest filterStripBusinessObjectForTest;
		#endregion

		#region FindFiltersOfTypeInGroup

		public void TestFindFiltersOfTypeInGroup_SameGroupAndNoOrCategory_SingleMatch()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter = filterBizo.AddDateFilterStrip("Z0_Date");
			var guidFilter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder(new[] { dateFilter }, results);
		}

		public void TestFindFiltersOfTypeInGroup_SameGroupAndNoOrCategory_MultipleMatches()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter1 = filterBizo.AddDateFilterStrip("Z0_Date");
			var dateFilter2 = filterBizo.AddDateFilterStrip("Z0_Date");

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder(new[] { dateFilter1, dateFilter2 }, results);
		}

		public void TestFindFiltersOfTypeInGroup_SameGroupAndOrCategory_ShouldReturnEmpty()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter = filterBizo.AddDateFilterStrip("Z0_Date");
			var guidFilter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			textFilter.OrCategory = FilterOrCategory.Chocolate;
			dateFilter.OrCategory = FilterOrCategory.Chocolate;
			guidFilter.OrCategory = FilterOrCategory.Chocolate;

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder(Array.Empty<ModuleDateFilter>(), results);
		}

		public void TestFindFiltersOfTypeInGroup_SameGroupButDifferentOrCategory_SingleMatch_ShouldReturnMatch()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter = filterBizo.AddDateFilterStrip("Z0_Date");
			var guidFilter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			textFilter.OrCategory = FilterOrCategory.AliceBlue;
			dateFilter.OrCategory = FilterOrCategory.Aquamarine;
			guidFilter.OrCategory = FilterOrCategory.AliceBlue;

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder(new[] { dateFilter }, results);
		}

		public void TestFindFiltersOfTypeInGroup_SameGroupButDifferentOrCategory_MultipleMatchesInSameOrCategory_ShouldReturnEmpty()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter1 = filterBizo.AddDateFilterStrip("Z0_Date");
			var dateFilter2 = filterBizo.AddDateFilterStrip("Z0_Date");

			textFilter.OrCategory = FilterOrCategory.AliceBlue;
			dateFilter1.OrCategory = FilterOrCategory.Aquamarine;
			dateFilter2.OrCategory = FilterOrCategory.Aquamarine;

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder("There's a match but it's in a different OR category (which has a colour) and there's another filter in that OR category with it, so it shouldn't count. SAD!", Array.Empty<ModuleDateFilter>(), results);
		}

		public void TestFindFiltersOfTypeInGroup_SameGroupButDifferentOrCategory_MultipleMatchesInDifferentOrCategories_ShouldReturnMathes()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter1 = filterBizo.AddDateFilterStrip("Z0_Date");
			var dateFilter2 = filterBizo.AddDateFilterStrip("Z0_Date");

			textFilter.OrCategory = FilterOrCategory.AliceBlue;
			dateFilter1.OrCategory = FilterOrCategory.Aquamarine;
			dateFilter2.OrCategory = FilterOrCategory.DarkSeaGreen;

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder("The matches are each in OR categories by themselves, so they're not really or-ing anything. So they should be considered matches. SAD!", new[] { dateFilter1, dateFilter2 }, results);
		}

		public void TestFindFiltersOfTypeInGroup_DifferentGroup_ShouldNotMatch()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter = filterBizo.AddDateFilterStrip("Z0_Date");

			textFilter.GroupName = "Shmlonathan";
			textFilter.GroupOrCategory = FilterOrCategory.AliceBlue;
			dateFilter.GroupName = "Shmlangela";
			dateFilter.GroupOrCategory = FilterOrCategory.Aquamarine;

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder(Array.Empty<ModuleDateFilter>(), results);
		}

		public void TestFindFiltersOfTypeInGroup_WhenFilterInOrCategory_WithTargetFilterInClearCategory_WithOtherFilter_ShouldMatch()
		{
			var filterBizo = new DummyFilterBusinessObject();

			var textFilter = filterBizo.AddTextFilterStrip("Z0_Description");
			var dateFilter = filterBizo.AddDateFilterStrip("Z0_Date");
			var guidFilter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			textFilter.OrCategory = FilterOrCategory.Chocolate;

			var results = filterBizo.FindFiltersOfTypeInGroup<ModuleDateFilter>(textFilter);

			AssertContainsExactElementsInAnyOrder("The presence of the third sourceFilter (one that is in the same OR category as the correctly typed one, but where that category is blank) should not prevent the sourceFilter from being matched. SAD!", new[] { dateFilter }, results);
		}

		#endregion

		#region Filter Rules

		public void TestLoadLayout_ForFilterRule_WhenModuleAndModuleIDNotPopulated_ShouldReportError()
		{
			StmModuleFilter layout;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				module.FilterBusinessObject.AddTextFilterStrip("Description", "Squanch");
				layout = module.FilterBusinessObject.SaveLayout("Shmlona");
			}

			var filterBizo = (FilterStripBusinessObject)ObjectFactory.Get("ProcessTasks_FilterStripBusinessObject");
			AssertNull(filterBizo.ModuleType);
			AssertNull(filterBizo.ParentModule);

			filterBizo.LoadLayout(layout);

			AssertContainsExactElementsInAnyOrder("The layout should have successfully loaded. SAD!", new[] { "Description" }, filterBizo.ActiveModuleFilters.Select(x => x.Description));
			AssertEquals("Loading a normal layout should not cause errors to be reported. SAD!", string.Empty, ErrorReporter.LastMessageReported);

			layout.S9_FilterType = StmModuleFilterTypes.Codes.FilterRule;
			filterBizo = (FilterStripBusinessObject)ObjectFactory.Get("ProcessTasks_FilterStripBusinessObject");
			filterBizo.LoadLayout(layout);

			AssertEquals("Filter Rule layouts must only be loaded into FilterStripBusinessObjects created via RelatedModuleFiltersHelper. It looks like you've created the FilterStripBusinessObject some other way, and a few things won't be set up correctly as a result. SAD!", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			AssertContainsExactElementsInAnyOrder("The layout should have successfully loaded, even though the error was reported. SAD!", new[] { "Description" }, filterBizo.ActiveModuleFilters.Select(x => x.Description));

			var filterBizoCreatedCorrectly = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(ModuleIDs.ProcessTasks);
			filterBizoCreatedCorrectly.LoadLayout(layout);

			AssertContainsExactElementsInAnyOrder("The layout should have successfully loaded. SAD!", new[] { "Description" }, filterBizo.ActiveModuleFilters.Select(x => x.Description));
			AssertEquals("The filter bizo was created using the helper, so a filter rule layout should load without reporting errors. SAD!", string.Empty, ErrorReporter.LastMessageReported);
		}

		#endregion

		#region IndexSearch

		public void TestSqlNotLoadGlowLayout()
		{
			var sqlLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "sql filters", false, false, false);
			var glowLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Glow Filter", false, false, false);
			glowLayout.S9_IsIndexSearch = true;
			glowLayout.Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				AssertEquals(1, module.FilterBusinessObject.Layouts.Count);
				AssertEquals("sql filters", module.FilterBusinessObject.Layouts[0].S9_FilterName);
			}
		}

		public void TestIndexLoadGlowLayout()
		{
			var sqlLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "sql filters", false, false, false);
			var glowLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Glow Filter", false, false, false);
			glowLayout.S9_IsIndexSearch = true;
			glowLayout.Factory.Save();

			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.IndexSearchFields = GlowIndexQueryEngineMock.GetTestSearchFields();
				AssertEquals(2, module.FilterBusinessObject.Layouts.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "sql filters", "Glow Filter" }, module.FilterBusinessObject.Layouts.Select(f => f.S9_FilterName));
			}
		}

		public void TestContainsDefaultsInDifferentSearchType()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			filterBizO.IndexSearchFields = GlowIndexQueryEngineMock.GetTestSearchFields();
			AssertEquals(SearchType.Sql, filterBizO.SearchType);

			var defaults = new FilterBusinessObjectDefaults();
			var filterDefault = new FilterBusinessObjectDefault("hasDefaultValueFilter", "Property", new ZString("someValue"));
			defaults.Add(filterDefault);

			AssertEquals("filterBizO.ContainsDefaults", false, filterBizO.ContainsDefaults);
			filterBizO.SetExternalDefaults(defaults);
			AssertEquals(true, filterBizO.ContainsDefaults);

			using (new GlowIndexQueryEngineMock())
			{
				filterBizO.SearchType = SearchType.Index;
				AssertEquals(SearchType.Index, filterBizO.SearchType);
				AssertEquals(false, filterBizO.ContainsDefaults);

				filterBizO.SearchType = SearchType.Sql;
				AssertEquals(SearchType.Sql, filterBizO.SearchType);
				AssertEquals(true, filterBizO.ContainsDefaults);
			}
		}

		public void TestFilterStripBusinessObjectFilterHaveDefaultErrorReportAction()
		{
			var errorMessage = "User Not Logged In, Unexpected authentication result: ContextChangeRequired";

			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>())).Returns(
					new GlowIndexQueryResultCollection()
					{
						Status = GlowIndexQueryStatus.UserNotLoggedIn,
						ErrorMessage = errorMessage
					});
			}))
			{
				var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();

				AssertEquals("Void ReportGlowIndexQueryError(System.String)", filterBizO.OnGlowIndexQueryErrorAction.Method.ToString());

				filterBizO.IndexSearchFields = GetSearchFieldCollection();
				filterBizO.SearchType = SearchType.Index;
				var filter = filterBizO.Filter;
				AssertEquals("Error When Getting FilterStripBusinessObject.Filter for Index Search", ErrorReporter.LastKeyReported);
				AssertEquals("ErrorMessage: User Not Logged In, Unexpected authentication result: ContextChangeRequired\r\nGlowQueryUri: odata/Index/EntityInfos?$top=50&$filter=EntityType eq 'IDummyBusinessObject'", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestFilterStripBusinessObjectFilterHaveShowErrorActionInFilterGridModule()
		{
			var errorMessage = "User Not Logged In, Unexpected authentication result: ContextChangeRequired";

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject" });
				_ = mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>())).Returns(
					new GlowIndexQueryResultCollection()
					{
						Status = GlowIndexQueryStatus.UserNotLoggedIn,
						ErrorMessage = errorMessage
					});
			}))
			{
				var filter = module.FilterBusinessObject.Filter;
				AssertEquals(string.Empty, filter.LiteralTextSqlFormatted);
				Assert(filter.IsNoResultQuery);
				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFilterStripBusinessObjectFilterReturnCorrectQueryWhenUseIndexSearch()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject" });
				_ = mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>())).Returns(
					new GlowIndexQueryResultCollection()
					{
						Status = GlowIndexQueryStatus.Success,
						Results = new GlowIndexQueryResult[]
						{
						new GlowIndexQueryResult(bizo.PK.ToString(), "IDummyBusinessObject")
						}
					});
			}))
			{
				AssertEquals($"Z0_PK = '{bizo.PK}'\r\n", module.FilterBusinessObject.Filter.LiteralTextSqlFormatted);
			}
		}

		public void TestIndexSearchParentTypeChange()
		{
			using (var mocker = new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBO = module.FilterBusinessObject;
				filterBO.IndexSearchFields = GetSearchFieldCollection();
				AssertEquals(module.FilterBusinessObject.ParentType, typeof(DummyBusinessObject));

				filterBO.SearchType = SearchType.Index;
				filterBO.LoadModuleFilters();

				filterBO.ParentType = ObjectFactory.GetType<IGlbStaff>();

				var filter = filterBO["CODE"] as IndexSearchModuleTextFilter;
				filter.Property = "HAY";
				filter.IsActive = true;

				var query = filterBO.DoGlowQuery();
				AssertContains(DummyBizoSchema.PK.Name, query.LiteralTextADO);
			}
		}

		public void TestIndexSearchQuery()
		{
			var filterBO = new DummyFilterStripBizoWithIndexSearchQuery();
			filterBO.IndexSearchQueryForTest = new EqualQuery(new Term("SOMEFLAG", "true"));
			var queries = filterBO.GetIndexSearchQueries();

			Assert("the index search query should be applied.", queries.Any(q => q.ToUrlComponent() == "(SOMEFLAG eq 'true')"));
		}

		public void TestDoGlowQuery()
		{
			using var mocker = new GlowIndexQueryEngineMock();
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);
			var filterBO = module.FilterBusinessObject;
			filterBO.IndexSearchFields = GetSearchFieldCollection();
			filterBO.SearchType = SearchType.Index;
			filterBO.LoadModuleFilters();

			var filter = filterBO["CODE"] as IndexSearchModuleTextFilter;
			filter.Property = "HAY";
			filter.IsActive = true;

			var query = filterBO.DoGlowQuery();
			AssertContains(DummyBizoSchema.PK.Name, query.LiteralTextADO);
		}

		public void TestDoGlowQueryWithWarning()
		{
			using var mocker = new GlowIndexQueryEngineMock();
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);

			mocker.Results.WarningMessage = "Warning Message";

			var filterBO = module.FilterBusinessObject;
			filterBO.IndexSearchFields = GetSearchFieldCollection();
			filterBO.SearchType = SearchType.Index;
			filterBO.LoadModuleFilters();

			var filter = filterBO["CODE"] as IndexSearchModuleTextFilter;
			filter.Property = "HAY";
			filter.IsActive = true;

			filterBO.AddFilterStrip<IndexSearchModuleTextFilter>("CODE");
			var query = filterBO.DoGlowQuery();
			AssertContains(DummyBizoSchema.PK.Name, query.LiteralTextADO);
			CombineAssertions("All Filter Strip Has Warning", () =>
			{
				foreach (var strip in filterBO.FilterStrips.Cast<FilterStrip>())
				{
					AssertEquals(true, strip.HasWarnings);
					AssertEquals("Warning - FilterDescription: Warning Message", strip.GetWarnings().First().Message);
				}
			});
		}

		public void TestDoGlowQueryNoDeveloperNotificationException()
		{
			using var mocker = new GlowIndexQueryEngineMock();
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);

			mocker.Results.WarningMessage = "Warning Message";

			var filterBO = module.FilterBusinessObject;
			filterBO.IndexSearchFields = GetSearchFieldCollection();
			filterBO.SearchType = SearchType.Index;
			filterBO.LoadModuleFilters();

			var filter = filterBO["CODE"] as IndexSearchModuleTextFilter;
			filter.Property = "HAY";
			filter.IsActive = true;

			filterBO.AddFilterStrip<IndexSearchModuleTextFilter>("CODE");
			filterBO.DoGlowQuery();

			Assert(Core.Testing.ExceptionReporterTestListener.Instance.Count == 0);
		}

		public void TestLoadUserDefinedFilterInDifferentSearchType()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBO = module.FilterBusinessObject;

				filterBO.SearchType = SearchType.Index;
				AssertEquals(SearchType.Index, filterBO.SearchType);

				var strip = filterBO.FilterStrips.AddNew("CODE");
				((IndexSearchModuleTextFilter)strip.CurrentModuleFilter).Property = "HAY";
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "My Glow Filter", true, false, true);

				filterBO.SearchType = SearchType.Sql;
				AssertEquals(SearchType.Sql, filterBO.SearchType);
				strip = filterBO.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "HAY";
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "My SQL Filter", true, false, true);

				filterBO.SearchType = SearchType.Index;
			}

			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBO = module.FilterBusinessObject;

				filterBO.SearchType = SearchType.Index;
				AssertEquals(SearchType.Index, filterBO.SearchType);
				AssertNotNull(filterBO["[USR]My Glow Filter"]);
				AssertNull(filterBO["[USR]My SQL Filter"]);

				filterBO.SearchType = SearchType.Sql;
				AssertEquals(SearchType.Sql, filterBO.SearchType);
				AssertNull(filterBO["[USR]My Glow Filter"]);
				AssertNotNull(filterBO["[USR]My SQL Filter"]);
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");
			var ret = new SearchFieldCollection("IDummyBusinessObject", new SearchField[] { field1, field2 });
			return ret;
		}

		class DummyFilterStripBizoWithIndexSearchQuery : DummyFilterStripBusinessObject
		{
			public IGlowQuery IndexSearchQueryForTest;
			public override IGlowQuery AdditionalIndexSearchQuery => IndexSearchQueryForTest;
		}

		#endregion

		#region Implementation

		protected DummyFilterStripBusinessObject FilterStripBizO
		{
			get { return fFilterStripBizO ?? (fFilterStripBizO = new DummyFilterStripBusinessObject()); }
		}

		DummyFilterStripBusinessObject fFilterStripBizO;

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
