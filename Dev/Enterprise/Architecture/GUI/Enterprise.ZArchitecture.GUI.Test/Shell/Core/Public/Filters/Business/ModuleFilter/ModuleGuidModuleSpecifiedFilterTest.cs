using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidModuleSpecifiedFilter))]
	public class ModuleGuidModuleSpecifiedFilterTest : ModuleFilterTestCase<ModuleGuidModuleSpecifiedFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public void TestNewFilter_ShouldHaveModuleIdNotSpecified()
		{
			AssertEquals(ModuleIDs.NotAssigned, Filter.ModuleId);
		}

		public void TestConstructorWithNoModuleList_ShouldHaveAllModules()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("No list", ProcessTasksSchema.P9_ParentID);

			AssertEquals(true, filter.ModuleOptions.Count > 300);
		}

		public void TestAllModuleOptions_ShouldNotContainNotAssigned()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("Show All Modules", ProcessTasksSchema.P9_ParentID);
			AssertEquals(false, filter.ModuleOptions.ContainsCode(ModuleIDs.NotAssigned.Name));
		}

		public void TestConstructorWithModuleList()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("List", ProcessTasksSchema.P9_ParentID, new[] { DummyModuleIDs.Dummy });

			AssertEquals(1, filter.ModuleOptions.Count);
			AssertEquals(DummyModuleIDs.Dummy.Name, filter.ModuleOptions[0].Code);
		}

		public void TestConstructorWithModuleListDelegate()
		{
			ModuleGuidModuleSpecifiedFilter.GetModuleOptionsDelegate listDelegate = () => new[] { DummyModuleIDs.Dummy };
			var filter = new ModuleGuidModuleSpecifiedFilter("Delegate", ProcessTasksSchema.P9_ParentID, listDelegate);

			AssertEquals(1, filter.ModuleOptions.Count);
			AssertEquals(DummyModuleIDs.Dummy.Name, filter.ModuleOptions[0].Code);
		}

		public void TestModuleOptions_ShouldBeSortedAlphabetically()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("Delegate", ProcessTasksSchema.P9_ParentID, new[] { ModuleIDs.JobShipment, ModuleIDs.BMTagMagnitude, DummyModuleIDs.Dummy, ModuleIDs.ProcessHeader });

			AssertArrayEqualsByElements("Module options should be sorted alphabetically, and yet...", new[] { ModuleIDs.BMTagMagnitude.Name, DummyModuleIDs.Dummy.Name, ModuleIDs.JobShipment.Name, ModuleIDs.ProcessHeader.Name }, filter.ModuleOptions.GetAllCodes());
		}

		public void TestDefaultModuleOptions_ShouldBeSortedAlphabetically()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("Sort test", ProcessTasksSchema.P9_ParentID);

			for (var i = 1; i < filter.ModuleOptions.Count; i++)
			{
				AssertEquals($"Module options should be sorted alphabetically, and yet '{filter.ModuleOptions[i].Code}' came before '{filter.ModuleOptions[i - 1].Code}'.", true, string.Compare(filter.ModuleOptions[i].Code, filter.ModuleOptions[i - 1].Code, StringComparison.InvariantCultureIgnoreCase) >= 0);
			}
		}

		public void TestClear()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("List", ProcessTasksSchema.P9_ParentID, new[] { DummyModuleIDs.Dummy });
			filter.SelectedModule = filter.ModuleOptions[0].Code;

			AssertEquals(DummyModuleIDs.Dummy, filter.ModuleId);

			filter.Clear();

			AssertEquals(ModuleIDs.NotAssigned, filter.ModuleId);
		}

		public void TestNoModuleSelected_ShouldHaveValidationError()
		{
			Filter.Validation.ValidateAll();
			AssertHasError(Filter.SelectedModuleInfo, "Please select a module.");

			Filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			AssertNoErrors(Filter.SelectedModuleInfo);
		}

		public void TestSetUnlistedModule_ShouldHaveValidationError()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("List", ProcessTasksSchema.P9_ParentID, new[] { DummyModuleIDs.Dummy });
			filter.SelectedModule = ModuleIDs.JobShipment.Name;

			AssertEquals(ModuleIDs.JobShipment, filter.ModuleId);
			AssertHasError(filter.SelectedModuleInfo, "Please select a supported module.");

			Filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			AssertNoErrors(Filter.SelectedModuleInfo);
		}

		public void TestSetModuleNotAllowedToAddCompanyRelatedFilters()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("List", ProcessTasksSchema.P9_ParentID, new[] { ModuleIDs.Customs.AU.HouseSeaCargo, ModuleIDs.Customs.AU.HouseAirCargo });
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedModule = ModuleIDs.Customs.AU.HouseSeaCargo.Name;
			AssertNoErrors("Should not have this error when ShouldAddCompanyRelatedFilters is true", filter.SelectedModuleInfo);

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (Env.Instance.TemporaryServiceTaskContext("TAG", canRunInAnyBranch: true))
			{
				filter.Validation.ValidateSelectedModule();
				AssertHasError("Should have this error", filter.SelectedModuleInfo, "Company filters are not allowed for this module.");

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filter.Validation.ValidateSelectedModule();
				AssertNoErrors("Should not have this error when ComparisonOperator is not FiltersMatch", filter.SelectedModuleInfo);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				filter.SelectedModule = ModuleIDs.Customs.AU.HouseAirCargo.Name;
				AssertNoErrors("Should not have this error when the selected module is not in ModuleIDsNotAllowedToAddCompanyRelatedFilters", filter.SelectedModuleInfo);
			}
		}

		public void TestSetModuleWithInvalidCode_ShouldSetDefaultModuleAndHaveValidationError()
		{
			Filter.SelectedModule = "Convenient";

			AssertEquals(ModuleIDs.NotAssigned, Filter.ModuleId);
			AssertHasError(Filter.SelectedModuleInfo, "Please select a module.");

			Filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			AssertNoErrors(Filter.SelectedModuleInfo);
		}

		public void TestSetModuleWithEmptyString_ShouldSetDefaultModuleAndHaveValidationError()
		{
			Filter.SelectedModule = "";

			AssertEquals(ModuleIDs.NotAssigned, Filter.ModuleId);
			AssertHasError(Filter.SelectedModuleInfo, "Please select a module.");

			Filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			AssertNoErrors(Filter.SelectedModuleInfo);
		}

		public void TestSetToCountrySpecificModule_ShouldHaveCauseValidationError_AndNotThrowException()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name;
				AssertNoExceptionThrown("Validating the filter when the module doesn't exist for the current country shouldn't throw exceptions. SAD!", () => Filter.Validation.ValidateAll());
				AssertHasError("Selected filters can't be found for an unavailable module, so we should just return null. SAD!", Filter.SelectedModuleInfo, "Please select a supported module.");
			}
		}

		public void TestSetToCountrySpecificModule_WhenModuleExistsForThatCountry_ShouldNotHaveExceptionsOrErrors()
		{
			Filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name;
			AssertNoExceptionThrown("Validating the filter when the exists for the current country obviously shouldn't throw exceptions also. SAD!", () => Filter.Validation.ValidateAll());
			AssertNoErrors(Filter.SelectedModuleInfo);
		}

		public void TestSetSelectedModuleToUnavailableModule_ShouldNotThrowExceptions_GuiTest()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			using (module.ShowPopup())
			{
				Application.DoEvents();

				var control = (ZFilterStripCommonControl)module.EmbeddedControl;
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Parent Job");
				control.AddFilterStrip(strip);
				var filter = (ModuleGuidModuleSpecifiedFilter)strip.CurrentModuleFilter;
				Application.DoEvents();

				filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name;

				var findBox = control.FindSingle<ZGuidFindBox>();
				AssertEquals("The module is invalid, so the popup button should be disabled (just like if the user enters a nonsense value for the selected module). SAD!", true, findBox.PopupButton.ReadOnly);

				control.FirePerformSearch();
				Application.DoEvents();
			}

			AssertContainsExactElementsInAnyOrder("Setting the module to an unavailable country-specific module should not throw an exception. SAD!", Array.Empty<string>(), ErrorReporter.LastExceptionsReported());
		}

		public void TestModuleOptions_ShouldNotContainCompanySpecificModules_WhenLoggedInToCompanyInDifferentCountry()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNull("The options shouldn't include an AU-only module when logged in to a non-AU company. SAD!", Filter.ModuleOptions.GetDescriptionFromCode(ModuleIDs.Customs.AU.AirCargoOutturnBills.Name));
			}
		}

		public void TestSerialisation_Guid()
		{
			var filterBizo = new DummyDependentFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Dummy with Specified Module"];
			filterBizo.FilterStrips.AddNew(filter.Description);

			var guid = ZGuid.NewZGuid();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			filter.Property = guid;

			AssertEquals(false, filterBizo.Filter.IsEmpty);

			var savedFilter = filterBizo.SaveLayout("savedFilter");
			var newFilterBizo = new DummyDependentFilterBusinessObject();
			newFilterBizo.LoadLayout(savedFilter);

			AssertEquals(filterBizo.Filter.LiteralTextSqlFormatted, newFilterBizo.Filter.LiteralTextSqlFormatted);

			var loadedFilter = (ModuleGuidFilter)newFilterBizo[filter.Description];

			AssertEquals(filter.Query.LiteralTextSqlFormatted, loadedFilter.Query.LiteralTextSqlFormatted);
			AssertEquals(guid, loadedFilter.Property);
		}

		public void TestSerialisation_SelectedFilters()
		{
			var filterBizo = new DummyDependentFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Dummy with Specified Module"];
			filterBizo.FilterStrips.AddNew(filter.Description);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Keokuk");

			AssertEquals(false, filterBizo.Filter.IsEmpty);

			var savedFilter = filterBizo.SaveLayout("Saved Filter");
			var newFilterBizo = new DummyDependentFilterBusinessObject();
			newFilterBizo.LoadLayout(savedFilter);

			AssertEquals(filterBizo.Filter.LiteralTextSqlFormatted, newFilterBizo.Filter.LiteralTextSqlFormatted);

			var loadedFilter = (ModuleGuidModuleSpecifiedFilter)newFilterBizo[filter.Description];

			AssertEquals(filter.Query.LiteralTextSqlFormatted, loadedFilter.Query.LiteralTextSqlFormatted);
			AssertEquals(filter.ModuleId, loadedFilter.ModuleId);
			AssertEquals(DummyModuleIDs.Dummy, loadedFilter.ModuleId);
		}

		public void TestBlankOrNotBlankOperators_ShouldMakeModuleSelectionReadOnly()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter_ForTest("Test", DummyDependentBizoSchema.ZD1_Z0);
			AssertEquals(false, filter.SelectedModule_ReadOnly_Exposed);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals(true, filter.SelectedModule_ReadOnly_Exposed);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals(false, filter.SelectedModule_ReadOnly_Exposed);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals(true, filter.SelectedModule_ReadOnly_Exposed);
		}

		public void TestNoModuleSelected_ShouldMakeFilterReadOnly()
		{
			AssertEquals(true, Filter.PropertyInfo.ReadOnly);

			Filter.SelectedModule = DummyModuleIDs.Dummy.Name;

			AssertEquals(false, Filter.PropertyInfo.ReadOnly);
		}

		public void TestChangeModuleSelection_ShouldClearProperties()
		{
			Filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			Filter.Property = ZGuid.NewZGuid();

			AssertNotEquals(ZGuid.Empty, Filter.Property);

			Filter.SelectedFilters.AddTextFilterStrip("Z0_Code");
			AssertEquals(1, Filter.SelectedFilters.ActiveModuleFilters.Count);

			Filter.SelectedModule = ModuleIDs.JobShipment.Name;

			AssertEquals(ZGuid.Empty, Filter.Property);
			AssertEquals(0, Filter.SelectedFilters.ActiveModuleFilters.Count);
		}

		public void TestReadOnlyModuleSelection_ShouldNotHaveValidationErrors()
		{
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals(ModuleIDs.NotAssigned, Filter.ModuleId);
			AssertNoErrors(Filter.SelectedModuleInfo);
		}

		public void TestModuleOptions_OnlyListsZFilterGridModules()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter_ForTest("AAA", DummyDependentBizoSchema.ZD1_Z0);

			foreach (var code in filter.ModuleOptions.GetAllCodes())
			{
				var id = ModuleIDs.AllExcludingClientModules.First(x => x.Name == code);

				AssertEquals($"Only ZFilterGridModules can be listed because stuff breaks with other types of modules. {code} is not a ZFilterGridModule", true, ZModuleFactory.Instance.IsZFilterGridModule(id));
			}

			Assert(true);
		}

		public void TestClientModuleSelected_ShouldBindToClientModuleId()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var clientHook = ClientHookLoader.Instance.ClientHook;
				var moduleId = clientHook.NewClientModules.FirstOrDefault(x => x.ID.Name == "SupportIncident");
				AssertNotNull(moduleId);

				Filter.SelectedModule = "SupportIncident";
				AssertEquals(moduleId.ID, Filter.ModuleId);
			}
		}

		public void TestSerialise_WhenNoModuleSelected_ShouldNotThrowException()
		{
			var filterBizo = new DummyDependentFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>("Dummy with Specified Module");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

			AssertEquals(string.Empty, filter.SelectedModule);
			AssertNoExceptionThrown(() => filterBizo.FilterStrips.GetLayoutValuesAsXml());
		}

		public void TestDeserialisation_ShouldValidateSelectedModule()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			var filterBizo = new DummyDependentFilterBusinessObject();
			var strip1 = filterBizo.FilterStrips.AddNew("Dummy with Specified Module");
			var filter1 = (ModuleGuidModuleSpecifiedFilter)strip1.CurrentModuleFilter;
			filter1.SelectedModule = DummyModuleIDs.Dummy.Name;
			filter1.Property = dummy.PK;

			var strip2 = filterBizo.FilterStrips.AddNew("Dummy with Specified Module");
			var filter2 = (ModuleGuidModuleSpecifiedFilter)strip2.CurrentModuleFilter;
			filter2.SelectedModule = DummyModuleIDs.Dummy.Name;
			filter2.Property = dummy.PK;

			AssertNoErrors(filter1.SelectedModuleInfo);
			AssertNoErrors(filter2.SelectedModuleInfo);

			var layout = filterBizo.SaveLayout("AAA");

			var filterBizo2 = new DummyDependentFilterBusinessObject();
			filterBizo2.LoadLayout(layout);
			var loadedFilter1 = (ModuleGuidModuleSpecifiedFilter)filterBizo2.FilterStrips[0].CurrentModuleFilter;
			var loadedFilter2 = (ModuleGuidModuleSpecifiedFilter)filterBizo2.FilterStrips[1].CurrentModuleFilter;

			AssertNoErrors(loadedFilter1.SelectedModuleInfo);
			AssertNoErrors(loadedFilter2.SelectedModuleInfo);

			loadedFilter1.SelectedModule = ZString.Empty;
			AssertHasError(loadedFilter1.SelectedModuleInfo, "Please select a module.");

			layout = filterBizo2.SaveLayout("AAA");

			var filterBizo3 = new DummyDependentFilterBusinessObject();
			filterBizo3.LoadLayout(layout);
			var reloadedFilter1 = (ModuleGuidModuleSpecifiedFilter)filterBizo3.FilterStrips[0].CurrentModuleFilter;
			var reloadedFilter2 = (ModuleGuidModuleSpecifiedFilter)filterBizo3.FilterStrips[1].CurrentModuleFilter;

			AssertHasError(reloadedFilter1.SelectedModuleInfo, "Please select a module.");
			AssertNoErrors(reloadedFilter2.SelectedModuleInfo);
		}

		public void TestDeserialiseFilter_ThenAddASecondOfTheSameFilter_ShouldNotThrowModuleNotAssignedIDException()
		{
			using (var mainModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			{
				var filterBizo = mainModule.FilterBusinessObject;
				var strip1 = filterBizo.FilterStrips.AddNew("Parent Job");
				var filter1 = (ModuleGuidModuleSpecifiedFilter)strip1.CurrentModuleFilter;
				filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				filter1.SelectedModule = ModuleIDs.WorkItem.Name;

				filter1.SelectedFilters.AddTextFilterStrip("Summary", "Shalala");

				filterBizo.SaveLayout("Bragadocious");
			}

			using (var mainModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			using (var form = (ZForm)mainModule.ShowPopup())
			{
				form.Show();
				Application.DoEvents();

				var stripControl = form.FindSingle<StripControl>();
				var strip1 = stripControl.FindSingle<ZFilterStrip>();
				var filter1 = (ModuleGuidModuleSpecifiedFilter)strip1.CurrentDataItem.CurrentModuleFilter;
				AssertEquals(ModuleIDs.WorkItem.Name, filter1.SelectedModule);
				AssertEquals(ModuleIDs.WorkItem, filter1.ModuleId);

				var strip2 = stripControl.AddNewFilterStrip();

				AssertNoExceptionThrown("Cloning the first filter strip should not throw a ModuleNotAssignedIDException, and yet...", () => strip2.CurrentDataItem.FilterDescription = "Parent Job");

				Application.DoEvents();
				AssertNotNull(strip2.CurrentDataItem.CurrentModuleFilter);
				AssertEquals(true, strip2.CurrentDataItem.CurrentModuleFilter is ModuleGuidModuleSpecifiedFilter);
			}
		}

		#region Implementation

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ModuleGuidModuleSpecifiedFilter GetNewModuleFilter()
		{
			return new ModuleGuidModuleSpecifiedFilter("moo", ProcessTasksSchema.P9_ParentID, ModuleIDs.AllIncludingClientModules.Concat(new[] { DummyModuleIDs.Dummy }));
		}

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(ModuleGuidModuleSpecifiedFilter filter)
		{
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.SelectedModule), new ZString(ModuleIDs.ProcessTasks.Name));

			return values;
		}

		class ModuleGuidModuleSpecifiedFilter_ForTest : ModuleGuidModuleSpecifiedFilter
		{
			public ModuleGuidModuleSpecifiedFilter_ForTest(ZString description, SchemaGuidColumn filterColumn, IEnumerable<ModuleIdentifier> moduleOptionsList = null) : base(description, filterColumn, moduleOptionsList)
			{
			}

			public bool SelectedModule_ReadOnly_Exposed => SelectedModule_ReadOnly;
		}

		#endregion
	}
}
