using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(FilterStripCollection))]
	public class FilterStripCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FilterStripCollection>
	{
		#region AddNewHooksupEvent

		public void TestAddNewHooksUpEvent()
		{
			Collection.ModuleFilterChanged += new EventHandler(Collection_ModuleFilterChanged);

			var strip = Collection.AddNew("d");
			var filter = strip.ModuleFilters.AddNumberRangeFilter("desc", delegate { return new ZQuery(); });
			strip.FilterDescription = filter.Description;
			filter.Decimals = 2;
			AssertEquals(true, eventFired);
			eventFired = false;
			filter.PropertyType = ZCalcEditPropertyType.Int;
			AssertEquals(true, eventFired);
		}

		void Collection_ModuleFilterChanged(object sender, EventArgs e)
		{
			eventFired = true;
		}

		bool eventFired;

		#endregion

		#region Test AddNew

		public void TestAddNewWithDescription()
		{
			AssertEquals("description", Collection.AddNew("description").FilterDescription);
		}

		public void TestAddNewWithModuleFilter()
		{
			var filter = new DummyModuleFilter("A Filter (46290)", DummyBizoSchema.Z0_Code);
			filter.OrCategory = FilterOrCategory.Red;

			var newFilterStrip = Collection.AddNew(filter);
			AssertEquals(FilterOrCategory.Red, newFilterStrip.OrCategory);
			AssertEquals(false, newFilterStrip.IncludeDuplicatesWhenGettingModuleFilter);
			AssertEquals("A Filter (46290)", newFilterStrip.FilterDescription);
		}

		public void TestAddNewWithDuplicateModuleFilter()
		{
			var filter = new DummyModuleFilter("A Filter", DummyBizoSchema.Z0_Code);
			ModuleFilters.AddCustomFilter(new DummyModuleFilter("A Filter", DummyBizoSchema.Z0_Code));
			ModuleFilters.AddNewDuplicateFilter("A Filter (46290)", filter);

			var duplicateFilter = new DummyModuleFilter("A Filter (46290)", DummyBizoSchema.Z0_Code);
			duplicateFilter.OrCategory = FilterOrCategory.Red;
			duplicateFilter.IsDuplicateDefault = true;

			var newFilterStrip = Collection.AddNew(duplicateFilter);
			AssertEquals(FilterOrCategory.Red, newFilterStrip.OrCategory);
			AssertEquals(true, newFilterStrip.IncludeDuplicatesWhenGettingModuleFilter);
			AssertEquals("Duplicate suffix should have been stripped from description", "A Filter", newFilterStrip.FilterDescription);
			AssertEquals("Setting the filter description should not have changed the module filter", "A Filter (46290)", newFilterStrip.CurrentModuleFilter.Description);
		}

		#endregion

		#region TestLoadFromXml

		public void TestLoadFromXmlManyGroups()
		{
			var moduleFilters = new ModuleFilterCollection();

			// Wont search just have filters to match
			moduleFilters.AddTextFilter("Custom SQL Filter", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Tag Magnitude", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Prerequisite Status", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Queue Status", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Job Property", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Workflow Type", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Open Task Estimate Range", DummyBizoSchema.Z0_Description);

			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var filter = userData.Layout;
			filter.S9_FilterData = Encoding.ASCII.GetBytes(xmlValues);

			var newStrips = new FilterStripCollection(moduleFilters);

			newStrips.LoadFromXml(filter, userData);

			var activeFilters = moduleFilters.Where(f => f.IsActive).ToArray();
			AssertEquals(13, activeFilters.Length);

			var actual = newStrips.Select(f => new FilterStripTestObject(f.CurrentModuleFilterDescription, f.GroupName, f.GroupOrCategory, f.OrCategory, f.FilterPropertyLockStatus));

			var expected = new[] {
				new FilterStripTestObject("Custom SQL Filter", groupName: "", groupOrCategory: FilterOrCategory.Blue, orCategory: FilterOrCategory.None, filterPropertyLockStatus: true),
				new FilterStripTestObject("Tag Magnitude", groupName: "", groupOrCategory: FilterOrCategory.Blue, orCategory: FilterOrCategory.None, filterPropertyLockStatus: false),
				new FilterStripTestObject("Prerequisite Status", groupName: "Workflow", groupOrCategory: FilterOrCategory.Green, orCategory: FilterOrCategory.None, filterPropertyLockStatus: false),
				new FilterStripTestObject("Queue Status", groupName: "Workflow", groupOrCategory: FilterOrCategory.Green, orCategory: FilterOrCategory.None, filterPropertyLockStatus: false),
				new FilterStripTestObject("Job Property", groupName: "3 Day Buffer Teams", groupOrCategory: FilterOrCategory.Blue, orCategory: FilterOrCategory.Red, filterPropertyLockStatus: true),
				new FilterStripTestObject("Tag Magnitude (1)", groupName: "3 Day Buffer Teams", groupOrCategory: FilterOrCategory.Blue, orCategory: FilterOrCategory.None, filterPropertyLockStatus: false),
				new FilterStripTestObject("Tag Magnitude (2)", groupName: "CCPM", groupOrCategory: FilterOrCategory.Green, orCategory: FilterOrCategory.None, filterPropertyLockStatus: false),
				new FilterStripTestObject("Job Property (1)", groupName: "3 Day Buffer Teams", groupOrCategory: FilterOrCategory.Blue, orCategory: FilterOrCategory.Red, filterPropertyLockStatus: true),
				new FilterStripTestObject("Job Property (2)", groupName: "3 Day Buffer Teams", groupOrCategory: FilterOrCategory.Blue, orCategory: FilterOrCategory.Red, filterPropertyLockStatus: false),
				new FilterStripTestObject("Workflow Type", groupName: "Incididents go to 3Day unless explicitly told otherwise", groupOrCategory: FilterOrCategory.None, orCategory: FilterOrCategory.Brown, filterPropertyLockStatus: true),
				new FilterStripTestObject("Tag Magnitude (3)", groupName: "Incididents go to 3Day unless explicitly told otherwise", groupOrCategory: FilterOrCategory.None, orCategory: FilterOrCategory.Brown, filterPropertyLockStatus: true),
				new FilterStripTestObject("Open Task Estimate Range", groupName: "Chunked", groupOrCategory: FilterOrCategory.None, orCategory: FilterOrCategory.None, filterPropertyLockStatus: false),
				new FilterStripTestObject("Tag Magnitude (4)", groupName: "Chunked", groupOrCategory: FilterOrCategory.None, orCategory: FilterOrCategory.None, filterPropertyLockStatus: true),
			};

			AssertContainsExactElementsInAnyOrder(new FilterStripTestObjectComparer(), expected, actual);
		}

		class FilterStripTestObjectComparer : IEqualityComparer<FilterStripTestObject>
		{
			public bool Equals(FilterStripTestObject x, FilterStripTestObject y)
			{
				return
					x.CurrentModuleFilterDescription == y.CurrentModuleFilterDescription &&
					x.GroupName == y.GroupName &&
					x.GroupOrCategory == y.GroupOrCategory &&
					x.OrCategory == y.OrCategory &&
					x.FilterPropertyLockStatus == y.FilterPropertyLockStatus;
			}

			public int GetHashCode(FilterStripTestObject obj)
			{
				throw new NotImplementedException();
			}
		}

		class FilterStripTestObject
		{
			public FilterStripTestObject(ZString currentModuleFilterDescription, ZString groupName, FilterOrCategory groupOrCategory, FilterOrCategory orCategory, bool filterPropertyLockStatus)
			{
				CurrentModuleFilterDescription = currentModuleFilterDescription;
				GroupName = groupName;
				GroupOrCategory = groupOrCategory;
				OrCategory = orCategory;
				FilterPropertyLockStatus = filterPropertyLockStatus;
			}

			public readonly ZString CurrentModuleFilterDescription;
			public readonly ZString GroupName;
			public readonly FilterOrCategory GroupOrCategory;
			public readonly FilterOrCategory OrCategory;
			public readonly bool FilterPropertyLockStatus;

			public override string ToString()
			{
				return string.Format(CultureInfo.InvariantCulture, "CurrentModuleFilterDescription:{0}, GroupName:{1}, GroupOrCategory:{2}, OrCategory:{3}, FilterPropertyLockStatus:{4}", CurrentModuleFilterDescription, GroupName, GroupOrCategory, OrCategory, FilterPropertyLockStatus);
			}
		}

		#region Filter Values

		const string xmlValues = @"<?xml version=""1.0""?>
<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>Custom SQL Filter</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>Blue</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>true</FilterPropertyLockStatus>
			<Filter />
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Tag Magnitude</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>Blue</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Prerequisite Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>Green</GroupOrCategory>
      <GroupName>Workflow</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Queue Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>Green</GroupOrCategory>
      <GroupName>Workflow</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>false</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Job Property</FilterDescription>
      <OrCategory>Red</OrCategory>
      <GroupOrCategory>Blue</GroupOrCategory>
      <GroupName>3 Day Buffer Teams</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>true</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Tag Magnitude</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>Blue</GroupOrCategory>
      <GroupName>3 Day Buffer Teams</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>false</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Tag Magnitude</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>Green</GroupOrCategory>
      <GroupName>CCPM</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>false</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Job Property</FilterDescription>
      <OrCategory>Red</OrCategory>
      <GroupOrCategory>Blue</GroupOrCategory>
      <GroupName>3 Day Buffer Teams</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>true</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Job Property</FilterDescription>
      <OrCategory>Red</OrCategory>
      <GroupOrCategory>Blue</GroupOrCategory>
      <GroupName>3 Day Buffer Teams</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Workflow Type</FilterDescription>
      <OrCategory>Brown</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName>Incididents go to 3Day unless explicitly told otherwise</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>true</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Tag Magnitude</FilterDescription>
      <OrCategory>Brown</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName>Incididents go to 3Day unless explicitly told otherwise</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>true</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Open Task Estimate Range</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName>Chunked</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>false</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Tag Magnitude</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName>Chunked</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>true</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>
";

		#endregion

		public void TestLoadFromXml()
		{
			// simulate module developer creating module filters
			var moduleFilters = new ModuleFilterCollection();
			moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

			// simluate a user creating and populating a filter strip
			var strips = new FilterStripCollection(moduleFilters);
			var textStrip = strips.AddNew("text filter");
			((ModuleTextFilter)textStrip.CurrentModuleFilter).Property = "Dolce";

			// create layout objects
			var layoutWithNoUserData = TestDataHelper.NewLayout("layout - no userdata");
			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var layoutWithUserData = userData.Layout;

			// serialize everything
			layoutWithNoUserData.S9_FilterData = strips.GetLayoutAsXml();
			layoutWithUserData.S9_FilterData = strips.GetLayoutAsXml();
			userData.S0_FilterDataValues = strips.GetLayoutValuesAsXml();

			// test
			var newFactory = new BusinessObjectFactory();
			var newStrips = new FilterStripCollection(moduleFilters);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithUserData, userData);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			AssertEquals("Dolce", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithNoUserData, null);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			AssertEquals("", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);
		}

		public void TestLoadFromXmlWithFailedFilter()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new ModuleFlagsFilterTest.DummyModuleFlagsFilter();

			filter.SaveBrokenData = true;
			filter.IsActive = true;
			filter.Property0 = ZBool.True;
			filter.Property1 = ZBool.True;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			// create layout objects
			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var layoutWithUserData = userData.Layout;

			// serialize everything
			layoutWithUserData.S9_FilterData = filterStripBizO.FilterStrips.GetLayoutAsXml();
			userData.S0_FilterDataValues = filterStripBizO.FilterStrips.GetLayoutValuesAsXml();

			var newFactory = new BusinessObjectFactory();
			var newStrips = new FilterStripCollection(filterStripBizO.ModuleFilters);
			newStrips.LoadFromXml(layoutWithUserData, userData);

			AssertMultilineASCIIEquals("should show message",
@"The following filter strips failed to load:
DummyFlagsFilter (2)

These filters will no longer have any values you may have assigned to them.
Please enter their values before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		public void TestValidationIsSuspendedDuringLoadingFromXml()
		{
			var moduleFilters = new ModuleFilterCollection();

			moduleFilters.AddTextFilter("Custom SQL Filter", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Tag Magnitude", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Prerequisite Status", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Queue Status", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Job Property", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Workflow Type", DummyBizoSchema.Z0_Description);
			moduleFilters.AddTextFilter("Open Task Estimate Range", DummyBizoSchema.Z0_Description);

			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var filter = userData.Layout;
			filter.S9_FilterData = Encoding.ASCII.GetBytes(xmlValues);

			var newStrips = new FilterStripCollection(moduleFilters);

			newStrips.LoadFromXml(filter, userData);
			Assert("Validation should be suspended during LoadXML().", newStrips.ValidationHasBeenSuspended);
			Assert("Validation has been resumed.", !newStrips.IsValidationSuspended);
		}

		#region TestLoadFromXmlWithOrCategory

		public void TestLoadFromXmlWithOrCategory()
		{
			// simulate module developer creating module filters
			var moduleFilters = new ModuleFilterCollection();
			moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

			// simluate a user creating and populating a filter strip
			var strips = new FilterStripCollection(moduleFilters);
			var textStrip = strips.AddNew("text filter");
			textStrip.OrCategory = FilterOrCategory.Blue;
			((ModuleTextFilter)textStrip.CurrentModuleFilter).Property = "Dolce";

			// create layout objects
			var layoutWithNoUserData = TestDataHelper.NewLayout("layout - no userdata");
			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var layoutWithUserData = userData.Layout;

			// serialize everything
			layoutWithNoUserData.S9_FilterData = strips.GetLayoutAsXml();
			layoutWithUserData.S9_FilterData = strips.GetLayoutAsXml();
			userData.S0_FilterDataValues = strips.GetLayoutValuesAsXml();

			// test
			var newFactory = new BusinessObjectFactory();
			var newStrips = new FilterStripCollection(moduleFilters);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithUserData, userData);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			AssertEquals(FilterOrCategory.Blue, newStrips[0].OrCategory);
			AssertEquals("Dolce", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithNoUserData, null);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			AssertEquals(FilterOrCategory.Blue, newStrips[0].OrCategory);
			AssertEquals("", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);
		}

		public void TestLoadFromXmlWithGroupProperties()
		{
			//  simulate module developer creating module filters
			var moduleFilters = new ModuleFilterCollection();
			moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

			//  simulate a user creating and populating a filter strip
			var strips = new FilterStripCollection(moduleFilters);
			var textStrip = strips.AddNew("text filter");
			textStrip.OrCategory = FilterOrCategory.Red;
			((ModuleTextFilter)textStrip.CurrentModuleFilter).Property = "Gabana";
			textStrip.GroupOrCategory = FilterOrCategory.HotPink;
			textStrip.GroupName = "My Group";
			textStrip.AdditionalColourName = "HotPink";
			textStrip.AdditionalGroupColourName = "Aqua";

			//  create layout objects
			var layoutWithNoUserData = TestDataHelper.NewLayout("layout - no userdata");
			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var layoutWithUserData = userData.Layout;

			//  serialize everything
			layoutWithNoUserData.S9_FilterData = strips.GetLayoutAsXml();
			layoutWithUserData.S9_FilterData = strips.GetLayoutAsXml();
			userData.S0_FilterDataValues = strips.GetLayoutValuesAsXml();

			//  test
			var newFactory = new BusinessObjectFactory();
			var newStrips = new FilterStripCollection(moduleFilters);

			moduleFilters["text filter"].IsActive = false;  //  normally a new collection would be created
			newStrips.LoadFromXml(layoutWithUserData, userData);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			AssertEquals(FilterOrCategory.Red, newStrips[0].OrCategory);
			AssertEquals("Gabana", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);
			AssertEquals(FilterOrCategory.HotPink, newStrips[0].GroupOrCategory);
			AssertEquals("My Group", newStrips[0].GroupName);
			AssertEquals("HotPink", newStrips[0].AdditionalColourName);
			AssertEquals("Aqua", newStrips[0].AdditionalGroupColourName);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithNoUserData, null);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			AssertEquals(FilterOrCategory.Red, newStrips[0].OrCategory);
			AssertEquals("", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);
			AssertEquals(FilterOrCategory.HotPink, newStrips[0].GroupOrCategory);
			AssertEquals("My Group", newStrips[0].GroupName);
			AssertEquals("HotPink", newStrips[0].AdditionalColourName);
			AssertEquals("Aqua", newStrips[0].AdditionalGroupColourName);
		}

		public void TestLoadFromXml_WhenFiltersHaveSameDescription_AndAreInSameFilterGroup_ShouldRestoreOrCategoryCorrectly()
		{
			// simulate module developer creating module filters
			var moduleFilters = new ModuleFilterCollection();
			moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

			// simluate a user creating and populating a filter strip
			var strips = new FilterStripCollection(moduleFilters);

			var textStripYellow = strips.AddNew("text filter");
			textStripYellow.GroupName = "group 1";
			textStripYellow.OrCategory = FilterOrCategory.Yellow;

			// another filter strip with the same description and in the same group
			var textStripGreen = strips.AddNew("text filter");
			textStripGreen.GroupName = "group 1";
			textStripGreen.OrCategory = FilterOrCategory.Green;

			// create layout object
			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var layoutWithUserData = userData.Layout;

			// serialize everything
			layoutWithUserData.S9_FilterData = strips.GetLayoutAsXml();

			// test
			var newStrips = new FilterStripCollection(moduleFilters);
			newStrips.LoadFromXml(layoutWithUserData, userData);

			AssertEquals(FilterOrCategory.Yellow, newStrips[0].OrCategory);
			AssertEquals(FilterOrCategory.Green, newStrips[1].OrCategory);
		}

		#endregion

		#region TestLoadFromXmlWithOrCategory

		public void TestLoadFromXmlWithFilterPropertyLockStatus()
		{
			// simulate module developer creating module filters
			var moduleFilters = new ModuleFilterCollection();
			moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

			// simluate a user creating and populating a filter strip
			var strips = new FilterStripCollection(moduleFilters);
			var textStrip = strips.AddNew("text filter");
			textStrip.FilterPropertyLockStatus = true;
			((ModuleTextFilter)textStrip.CurrentModuleFilter).Property = "Dolce";

			// create layout objects
			var layoutWithNoUserData = TestDataHelper.NewLayout("layout - no userdata");
			var userData = TestDataHelper.NewLayoutDataWithLayout("layout with userdata");
			var layoutWithUserData = userData.Layout;

			// serialize everything
			layoutWithNoUserData.S9_FilterData = strips.GetLayoutAsXml();
			layoutWithUserData.S9_FilterData = strips.GetLayoutAsXml();
			userData.S0_FilterDataValues = strips.GetLayoutValuesAsXml();

			// test
			var newStrips = new FilterStripCollection(moduleFilters);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithUserData, userData);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			Assert("true", newStrips[0].FilterPropertyLockStatus);
			AssertEquals("Dolce", ((ModuleTextFilter)newStrips[0].CurrentModuleFilter).Property);

			moduleFilters["text filter"].IsActive = false; // normally a new collection would be created
			newStrips.LoadFromXml(layoutWithNoUserData, null);
			AssertEquals(1, newStrips.Count);
			AssertEquals("text filter", newStrips[0].FilterDescription);
			Assert("true", newStrips[0].FilterPropertyLockStatus);
		}

		#endregion

		#region Implementation

		protected override FilterStripCollection GetCollectionToTest()
		{
			return new FilterStripCollection(ModuleFilters);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FilterStrip(ModuleFilters);
		}

		public new FilterStripCollection Collection
		{
			get { return base.Collection; }
		}

		ModuleFilterCollection ModuleFilters
		{
			get { return fModuleFilters ?? (fModuleFilters = new ModuleFilterCollection()); }
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		ModuleFilterCollection fModuleFilters;
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
