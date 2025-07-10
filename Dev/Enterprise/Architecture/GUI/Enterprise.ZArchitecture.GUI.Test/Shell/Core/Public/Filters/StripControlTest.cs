using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GUI.TileBar;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class FilterStripControlTest : TestCaseWithFactory
	{
		public void TestDeleteFilterStripWhenCurrentDataItemIsNull()
		{
			using (var filterStrip = new ZFilterStrip())
			using (var stripControl = new StripControl())
			{
				filterStrip.SetDataBinding(null, "");
				AssertNull(filterStrip.CurrentDataItem);

				AssertNoExceptionThrown(() => stripControl.DeleteFilterStrip(filterStrip));
				AssertEquals("FilterStrip should be disposed", true, filterStrip.IsDisposed);
			}
		}

		#region TestLoadOnlyOnce

		public void TestLoadOnlyOnce()
		{
			// Need this test, because of weird behaviour of ZFilterStripCOntrol when InitializeStrips() are called more than once. WI00004936.

			AssertEquals(0, FilterControl.InitializeStripCalledCount);

			FilterControl.OnLoad(EventArgs.Empty);
			AssertEquals(1, FilterControl.InitializeStripCalledCount);

			FilterControl.OnLoad(EventArgs.Empty);
			AssertEquals(1, FilterControl.InitializeStripCalledCount);
		}

		#endregion

		public void TestAddAlwaysVisibleFilterStrips_WhenSearchTypeIsIndexSearch_ShouldNotNewDuplicateFilterStrip()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.GlbBranch))
			{
				var registryMock = new Mock<IGlowRegistry>();
				_ = registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
				_ = registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
				ObjectFactory.Substitute(registryMock.Object);

				var filterBizO = module.FilterBusinessObject;
				filterBizO.SearchType = SearchType.Index;
				filterBizO.AddAlwaysVisibleFilters();

				using (var filtserStripControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO))
				{
					filtserStripControl.OnLoad(EventArgs.Empty);

					AssertEquals("ZFilterStripBaseControl.AddAlwaysVisibleFilterStrips should not create duplicate filterStrip", 0, filterBizO.FilterStrips.Count(strip => ((FilterStrip)strip).CurrentModuleFilterDescription == "Company (1)"));
				}
			}
		}

		#region DummyZFilterStripControl

		public class DummyZFilterStripControl : ZFilterStripControl
		{
			public DummyZFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
				: base(gridCollection, filterBusinessObject)
			{
				FilteredGrid.Columns.AddTextColumn("Z0_Code", 80);
			}

			public new List<ZFilterStrip> Strips
			{
				get { return (List<ZFilterStrip>)typeof(ZFilterStripControl).GetField("Strips", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this); }
			}

			public new List<GroupStripControl> GroupControls
			{
				get { return (List<GroupStripControl>)typeof(ZFilterStripControl).GetField("GroupControls", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this); }
			}

			internal protected override void InitializeStrips()
			{
				InitializeStripCalledCount++;
				base.InitializeStrips();
			}

			internal int InitializeStripCalledCount;

			internal protected override void AddItemToFindDropList(StmModuleFilter filter)
			{
				AddItemToFindDropListCalledCount++;
				base.AddItemToFindDropList(filter);
			}

			internal int AddItemToFindDropListCalledCount;

			public new void OnLoad(EventArgs args)
			{
				base.OnLoad(args);
			}

			public void HandleFindButtonDropDownItemClickExposed(ToolStripItem item)
			{
				HandleFindButtonDropDownItemClick(item);
			}

			public new ZLabel AutoRefreshWarningLabel => base.AutoRefreshWarningLabel;
			public ToolStripSplitButton ToolStripManageDropButtonExposed => ToolStripManageDropButton;
			public ToolStripButton ToolStripResetLayoutButtonExposed => ToolStripResetLayoutButton;
			public ToolStripButton ToolStripSaveLayoutButtonExposed => ToolStripSaveLayoutButton;
			public ToolStripButton ToolStripAddGroupButtonExposed => ToolStripAddGroupButton;
			public ToolStripSplitButton ToolStripFindDropButtonExposed => ToolStripFindDropButton;
			public ZFilterStripAddButton AddStripButtonExposed => AddStripButton;
			public ToolStrip ToolStripExposed => ToolStrip;
			public Panel FilterStripsPanelExposed => FilterStripsPanel;
			public int LastFilterStripBottomExposed => LastFilterStripBottom;
			public int LastGroupControlBottomPlusPaddingExposed => LastGroupControlBottomPlusPadding;
			public int? MinFilterStripPanelHeightExposed { get; set; }
			public void ItemClicked_Exposed(object sender, EventArgs e) => ItemClicked(sender, e);
			protected override int MinFilterStripPanelHeight
			{
				get
				{
					return MinFilterStripPanelHeightExposed ?? base.MinFilterStripPanelHeight;
				}
			}
		}

		#endregion

		#region TestReset

		public void TestResetAddsAlwaysVisibleFilters()
		{
			var alwaysVisibleFilter = new ModuleTextFilter("alwaysVisibleFilter", DummyBizoSchema.GenericStringSchemaColumn);
			alwaysVisibleFilter.Visibility = FilterVisibility.AlwaysVisible;

			var nonAlwaysVisibleFilter = new ModuleTextFilter("nonAlwaysVisibleFilter", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(alwaysVisibleFilter);
			filterBizO.AddModuleFilterForTest(nonAlwaysVisibleFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				AssertEquals("alwaysVisibleFilter added and so should be active", true, alwaysVisibleFilter.IsActive);
				AssertEquals("nonAlwaysVisibleFiler not added and so should not be active", false, nonAlwaysVisibleFilter.IsActive);

				AssertEquals("2 filter strips should be in place (1 always visible + 1 empty)", 2, filterControl.Strips.Count);

				AssertEquals("first strip should show the always visible filter", alwaysVisibleFilter, filterControl.Strips[0].CurrentDataItem.CurrentModuleFilter);
				AssertEquals("always visible filters should hide the delete button", false, GetDeleteStripButton(filterControl.Strips[0]).Visible);

				AssertEquals("second strip should be empty", null, filterControl.Strips[1].CurrentDataItem.CurrentModuleFilter);
				AssertEquals("non-AlwaysVisible filter should show the delete button", true, GetDeleteStripButton(filterControl.Strips[1]).Visible);
			}
		}

		public void TestResetClicked_PermanentlyClearsDefaultValues()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var defaultFilterName = "hasDefaultValueFilter";
			var savedValue = "saved value";
			var defaultValue = new ZString("default value");

			var defaults = new FilterBusinessObjectDefaults();
			var filterDefault = new FilterBusinessObjectDefault("hasDefaultValueFilter", "Property", defaultValue);
			defaults.Add(filterDefault);
			filterBizO.SetExternalDefaults(defaults);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				AssertEquals("Precondition: default values are set", true, filterBizO.ContainsDefaults);
				var defaultFilterStrip = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == defaultFilterName);
				AssertEquals("Precondition: default value is populated in the filter", defaultValue, ((ModuleTextFilter)defaultFilterStrip.CurrentModuleFilter).Property);

				var moduleTextFilter = (ModuleTextFilter)defaultFilterStrip.CurrentModuleFilter;
				moduleTextFilter.Property = savedValue;
				var savedLayout = filterBizO.SaveLayout("savedFilter");

				filterControl.ToolStripResetLayoutButtonExposed.PerformClick();

				filterBizO.LoadLayout(savedLayout);
				defaultFilterStrip = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == defaultFilterName);
				AssertEquals("The saved value of the layout is populated in the filter, meaning the default values did not persist after the reset", savedValue, ((ModuleTextFilter)defaultFilterStrip.CurrentModuleFilter).Property);
			}
		}

		public void TestResetClicked_DefaultFilterStripsRemain()
		{
			var defaultFilterDesc = "hasDefaultValueFilter";
			var normalFilterDesc = "someFilter";
			var defaultValue = new ZString("default value");
			var savedValue = "saved value";

			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();

			var normalFilter = filterBizO.FilterStrips.AddNew();
			normalFilter.FilterDescription = normalFilterDesc;
			((ModuleTextFilter)normalFilter.CurrentModuleFilter).Property = savedValue;

			var savedLayout = filterBizO.SaveLayout("savedFilter");

			var defaults = new FilterBusinessObjectDefaults();
			var filterDefault = new FilterBusinessObjectDefault(defaultFilterDesc, "Property", defaultValue);
			var secondDefault = new FilterBusinessObjectDefault(normalFilterDesc, "Property", defaultValue);
			defaults.Add(filterDefault);
			defaults.Add(secondDefault);
			filterBizO.SetExternalDefaults(defaults);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				AssertEquals("Precondition: default values are set", true, filterBizO.ContainsDefaults);

				var defaultFilterStrip = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == defaultFilterDesc);
				AssertEquals("Precondition: default value is populated in the filter", defaultValue, ((ModuleTextFilter)defaultFilterStrip.CurrentModuleFilter).Property);

				normalFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == normalFilterDesc);
				AssertEquals("Precondition: default value is populated in the filter", defaultValue, ((ModuleTextFilter)normalFilter.CurrentModuleFilter).Property);

				filterControl.ToolStripResetLayoutButtonExposed.PerformClick();

				filterBizO.LoadLayout(savedLayout);

				normalFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == normalFilterDesc);
				AssertNotNull("The first filter strip exists", normalFilter);
				AssertEquals("The first filter strip exists has the saved value", savedValue, ((ModuleTextFilter)normalFilter.CurrentModuleFilter).Property);

				defaultFilterStrip = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == defaultFilterDesc);
				AssertNotNull("The second filter strip exists", defaultFilterStrip);
				AssertEquals("The second filter strip is empty", string.Empty, ((ModuleTextFilter)defaultFilterStrip.CurrentModuleFilter).Property);
			}
		}

		public void TestResetClicked_PermanentlyClearsInitialCode_FilterColumn()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var savedValue = "saved value";
			var initialCodeValue = "initial code value";
			var initialCodeFilterDesc = "hasInitialCodeForSearchFilter";

			var initialCodeFilter = filterBizO.FilterStrips.AddNew();
			initialCodeFilter.FilterDescription = initialCodeFilterDesc;

			filterBizO.SetInitialCodeForSearch(initialCodeValue, typeof(DummyBusinessObject));

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				initialCodeFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == initialCodeFilterDesc);
				var moduleFilter = (ModuleTextFilter)initialCodeFilter.CurrentModuleFilter;
				AssertEquals("Precondition: initial code value is populated in the filter", initialCodeValue, moduleFilter.Property);

				moduleFilter.Property = savedValue;
				var savedLayout = filterBizO.SaveLayout("savedFilter");

				filterControl.ToolStripResetLayoutButtonExposed.PerformClick();

				filterBizO.LoadLayout(savedLayout);

				initialCodeFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == initialCodeFilterDesc);
				moduleFilter = (ModuleTextFilter)initialCodeFilter.CurrentModuleFilter;
				AssertEquals("The saved value of the layout is populated in the filter, meaning the initial code value did not persist after the reset", savedValue, moduleFilter.Property);
			}
		}

		public void TestResetClicked_PermanentlyClearsInitialCode_Prefix()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var savedValue = "saved value";
			var propertyValue = "PropertyValue";
			var prefix = "Prefix";
			var initialCodeValue = prefix + ":" + propertyValue;
			var initialCodeFilterDesc = "hasInitialCodeForSearchFilter";

			var initialCodeFilter = filterBizO.FilterStrips.AddNew();
			initialCodeFilter.FilterDescription = initialCodeFilterDesc;
			var moduleFilter = (ModuleTextFilter)initialCodeFilter.CurrentModuleFilter;

			AssertEquals("Precondition: The prefix is set", prefix, moduleFilter.Prefix);
			filterBizO.SetInitialCodeForSearch(initialCodeValue, typeof(DummyBusinessObject));

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				initialCodeFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == initialCodeFilterDesc);
				moduleFilter = (ModuleTextFilter)initialCodeFilter.CurrentModuleFilter;
				AssertEquals("Precondition: property value is populated in the filter", propertyValue, moduleFilter.Property);

				moduleFilter.Property = savedValue;
				var savedLayout = filterBizO.SaveLayout("savedFilter");

				filterControl.ToolStripResetLayoutButtonExposed.PerformClick();

				AssertEquals("Precondition: The prefix is set", prefix, moduleFilter.Prefix);
				filterBizO.LoadLayout(savedLayout);

				initialCodeFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == initialCodeFilterDesc);
				moduleFilter = (ModuleTextFilter)initialCodeFilter.CurrentModuleFilter;
				AssertEquals("The saved value of the layout is populated in the filter, meaning the initial code value did not persist after the reset", savedValue, moduleFilter.Property);
			}
		}

		public void TestResetClicked_InitialCodeFilterStripRemains()
		{
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var savedValue = "saved value";
			var initialCodeValue = "initial code value";
			var initialCodeFilterDesc = "hasInitialCodeForSearchFilter";
			var normalFilterDesc = "someFilter";

			var normalFilter = filterBizO.FilterStrips.AddNew();
			normalFilter.FilterDescription = normalFilterDesc;
			((ModuleTextFilter)normalFilter.CurrentModuleFilter).Property = "saved value";

			AssertEquals("Precondition: Only one filter exists", 1, filterBizO.FilterStrips.Count);
			AssertEquals("Precondition: The filter is the normal filter", normalFilter.FilterDescription, filterBizO.FilterStrips[0].FilterDescription);

			var savedLayout = filterBizO.SaveLayout("savedFilter");

			var initialCodeFilter = filterBizO.FilterStrips.AddNew();
			initialCodeFilter.FilterDescription = initialCodeFilterDesc;

			filterBizO.SetInitialCodeForSearch(initialCodeValue, typeof(DummyBusinessObject));

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				initialCodeFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == initialCodeFilterDesc);
				var moduleFilter = (ModuleTextFilter)initialCodeFilter.CurrentModuleFilter;
				AssertEquals("Precondition: initial code value is populated in the filter", initialCodeValue, moduleFilter.Property);

				filterControl.ToolStripResetLayoutButtonExposed.PerformClick();

				filterBizO.LoadLayout(savedLayout);

				normalFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == normalFilterDesc);
				AssertEquals("The normal filter is populated with the saved value", savedValue, ((ModuleTextFilter)normalFilter.CurrentModuleFilter).Property);

				initialCodeFilter = filterBizO.FilterStrips.Cast<FilterStrip>().FirstOrDefault(x => x.FilterDescription == initialCodeFilterDesc);
				AssertEquals("The initial code filter exists and is empty", string.Empty, ((ModuleTextFilter)initialCodeFilter.CurrentModuleFilter).Property);
			}
		}

		#endregion

		#region Toggle Filter Visibility

		public void TestToggleFilterVisibility()
		{
			var filterBizO = new DummyFilterStripBusinessObject();
			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				using (var filterControl = new DummyZFilterStripControl(collection, filterBizO))
				{
					AssertEquals(DockStyle.None, filterControl.FilteredGrid.Dock);
					AssertEquals(true, filterControl.FilteredGrid.Visible);
					AssertControlVisibility(filterControl, true, "FilterStripsPanel");
					AssertControlVisibility(filterControl, true, "ToolStripHelp");
					AssertEquals(true, filterControl.IsFilterVisible);

					filterControl.IsFilterVisible = !filterControl.IsFilterVisible;
					AssertEquals(DockStyle.Fill, filterControl.FilteredGrid.Dock);
					AssertEquals(true, filterControl.FilteredGrid.Visible);
					AssertControlVisibility(filterControl, false, "FilterStripsPanel");
					AssertControlVisibility(filterControl, false, "ToolStripHelp");
					AssertEquals(false, filterControl.IsFilterVisible);

					filterControl.IsFilterVisible = !filterControl.IsFilterVisible;
					AssertEquals(DockStyle.None, filterControl.FilteredGrid.Dock);
					AssertEquals(true, filterControl.FilteredGrid.Visible);
					AssertControlVisibility(filterControl, true, "FilterStripsPanel");
					AssertControlVisibility(filterControl, true, "ToolStripHelp");
					AssertEquals(true, filterControl.IsFilterVisible);
				}
			}
		}

		void AssertControlVisibility(ZFilterStripControl filterControl, bool expectedVisible, string controlKey)
		{
			AssertEquals(expectedVisible, filterControl.Controls.Find(controlKey, true)[0].Visible);
		}

		#endregion
		#region FilterStrip Wrapping Groups

		public void TestAddGroup()
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			textFilter.Visibility = FilterVisibility.AlwaysVisible;

			var textFilter2 = new ModuleTextFilter("more filter", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(textFilter);
			filterBizO.AddModuleFilterForTest(textFilter2);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				AssertEquals("2 filter strips should be in place (1 always visible + 1 empty)", 2, filterControl.Strips.Count);

				filterControl.CurrentGroupName = "New Group";

				AssertEquals("2 Group must exist as a result of default empty groupName", 2, filterControl.GroupControls.Count);
				AssertEquals("2 filter strips should exist (Originally added 2 filterStrips on new Group", 2, filterControl.Strips.Count);

				AssertEquals("2 ZFilterStrips should be found in first Group container + 1 Filter Color label", 3, filterControl.GroupControls[0].FilterStripGroupBox.Controls.Count);
				AssertEquals("0 ZFilterStrips should be found in second Group container + 1 Filter Color label", 1, filterControl.GroupControls[1].FilterStripGroupBox.Controls.Count);
			}
		}

		public void TestDeletingLastStripFromAGroupMakesItInactive()
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			var textFilter2 = new ModuleTextFilter("more filter", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(textFilter);
			filterBizO.AddModuleFilterForTest(textFilter2);

			using (var form = new ZChildForm())
			using (var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO))
			{
				filterControl.Size = ControlDpiScalingHelper.NewScaledSize(1350, 655);
				form.Controls.Add(filterControl);
				form.Size = ControlDpiScalingHelper.NewScaledSize(1366, 720);

				form.Show();

				filterControl.AddStrip();
				filterControl.CurrentGroupName = "Doing this makes a group out of the current strips"; // SOOOO wtf
				filterControl.AddStrip();
				Application.DoEvents();

				var groupContainer = filterControl.GroupControls.First().FilterStripGroupBox;
				var stripsInGroup = groupContainer.Controls.OfType<ZFilterStrip>().ToList();
				AssertEquals("PRE: Should have selected the group with all those strips", 2, stripsInGroup.Count);

				var filterStrip1 = stripsInGroup[0].CurrentDataItem;
				var filterStrip2 = stripsInGroup[1].CurrentDataItem;
				filterStrip1.FilterDescription = "Text filter";
				filterStrip2.FilterDescription = "more filter";
				var moduleFilter1 = filterStrip1.CurrentModuleFilter;
				var moduleFilter2 = filterStrip2.CurrentModuleFilter;

				AssertEquals("Text filter", filterStrip1.FilterDescription);
				AssertEquals("more filter", filterStrip2.FilterDescription);
				Assert(moduleFilter1.IsActive);
				Assert(moduleFilter2.IsActive);

				stripsInGroup[1].DeleteStripButton.PerformClick();
				AssertEquals("Text filter", filterStrip1.FilterDescription);
				AssertEquals(FilterStrip.SelectFilterDescriptionText, filterStrip2.FilterDescription);
				Assert(moduleFilter1.IsActive);
				Assert(!moduleFilter2.IsActive);

				stripsInGroup[0].DeleteStripButton.PerformClick();
				AssertEquals(FilterStrip.SelectFilterDescriptionText, filterStrip1.FilterDescription);
				AssertEquals(FilterStrip.SelectFilterDescriptionText, filterStrip2.FilterDescription);
				Assert(!moduleFilter1.IsActive);
				Assert(!moduleFilter2.IsActive);
			}
		}

		public void TestMakingGroupFromManyExistingFilterStrips_AllStripsAreWithinBounds()
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			var textFilter2 = new ModuleTextFilter("more filter", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(textFilter);
			filterBizO.AddModuleFilterForTest(textFilter2);

			using (var form = new ZChildForm())
			using (var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), filterBizO))
			{
				filterControl.Size = ControlDpiScalingHelper.NewScaledSize(1350, 655);
				form.Controls.Add(filterControl);
				form.Size = ControlDpiScalingHelper.NewScaledSize(1366, 720);

				form.Show();

				for (var i = 0; i < 30; i++)
				{
					filterControl.AddStrip();
					Application.DoEvents();
				}

				filterControl.CurrentGroupName = "Doing this makes a group out of the current strips"; // SOOOO wtf
				filterControl.AddStrip();
				Application.DoEvents();

				var groupContainer = filterControl.GroupControls.First().FilterStripGroupBox;
				var stripsInGroup = groupContainer.Controls.OfType<ZFilterStrip>().ToList();
				AssertEquals("PRE: Should have selected the group with all those strips", 31, stripsInGroup.Count);

				var parentBounds = ControlDpiScalingHelper.NewScaledRectangle(Point.Empty, groupContainer.Size, false);
				foreach (var strip in stripsInGroup)
				{
					Assert("All strips should be fully contained within their parent group control", parentBounds.Contains(strip.Bounds));
				}
			}
		}

		public void TestAddingNewFilterStrip_ToNamedGroup_PreservesGroupOrCategory()
		{
			AssertAddFilterStripToGroup("name");
		}

		public void TestAddingNewFilterStrip_ToUnnamedGroup_PreservesGroupOrCategory()
		{
			AssertAddFilterStripToGroup("");
		}

		static void AssertAddFilterStripToGroup(string name)
		{
			using (var form = new Form())
			using (var stripControl = new StripControl_ForTest(new DummyFilterStripBusinessObject()))
			{
				form.Controls.Add(stripControl);
				stripControl.AddGroupFilterControl(name);
				var group = stripControl.GroupControlsExposed.First();

				var filterStrip1 = stripControl.AddNewFilterStripToGroup(group);
				filterStrip1.CurrentDataItem.FilterDescription = "A";

				group.GroupOrCategory = FilterOrCategory.Red;

				var filterStrip2 = stripControl.AddNewFilterStripToGroup(group);
				filterStrip1.CurrentDataItem.FilterDescription = "B";

				AssertEquals(FilterOrCategory.Red, group.GroupOrCategory);
			}
		}

		public void TestFilterWithOrCategory_GeneratesCorrectQuery()
		{
			var businessObject = new DummyFilterStripBusinessObject();
			using (var form = new Form())
			using (var stripControl = new StripControl_ForTest(businessObject))
			{
				businessObject.AddModuleFilterForTest(new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description));
				businessObject.AddModuleFilterForTest(new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code));

				form.Controls.Add(stripControl);
				stripControl.AddGroupFilterControl("Group 1");
				var group1 = stripControl.GroupControlsExposed.First();
				var filterStrip1 = stripControl.AddNewFilterStripToGroup(group1);
				filterStrip1.CurrentDataItem.FilterDescription = "Code";
				filterStrip1.CurrentDataItem.OrCategory = FilterOrCategory.Red;
				(filterStrip1.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).Property = "1";
				(filterStrip1.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).ComparisonOperator = "exact";

				stripControl.AddGroupFilterControl("Group 2");
				var group2 = stripControl.GroupControlsExposed.Skip(1).First();

				var filterStrip2 = stripControl.AddNewFilterStripToGroup(group2);
				filterStrip2.CurrentDataItem.FilterDescription = "Description";
				filterStrip2.CurrentDataItem.OrCategory = FilterOrCategory.Red;
				(filterStrip2.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).Property = "2";
				(filterStrip2.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).ComparisonOperator = "exact";

				var filterStrip3 = stripControl.AddNewFilterStripToGroup(group2);
				filterStrip3.CurrentDataItem.FilterDescription = "Code";
				filterStrip3.CurrentDataItem.OrCategory = FilterOrCategory.None;
				(filterStrip3.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).Property = "3";
				(filterStrip3.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).ComparisonOperator = "exact";

				var filterStrip4 = stripControl.AddNewFilterStripToGroup(group2);
				filterStrip4.CurrentDataItem.FilterDescription = "Description";
				filterStrip4.CurrentDataItem.OrCategory = FilterOrCategory.Red;
				(filterStrip4.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).Property = "4";
				(filterStrip4.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter).ComparisonOperator = "exact";

				var group1Query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "1");
				var group2DescriptionCondition = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, new string[] { "2", "4" });
				var group2CodeCondition = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "3");
				var group2Query = new ZQuery(group2CodeCondition, JoinCondition.And, group2DescriptionCondition);
				var expectedQuery = new ZQuery(group2Query, JoinCondition.And, group1Query);

				AssertEquals(expectedQuery.LiteralTextSqlFormatted, stripControl.FilterBusinessObject.Filter.LiteralTextSqlFormatted);
			}
		}

		public void TestGroupsWithIsGroupOrCategoryReadOnly()
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			textFilter.Visibility = FilterVisibility.AlwaysVisible;
			textFilter.IsGroupOrCategoryReadOnly = true;

			var textFilter2 = new ModuleTextFilter("more filter", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(textFilter);
			filterBizO.AddModuleFilterForTest(textFilter2);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				AssertEquals("2 filter strips should be in place (1 always visible + 1 empty)", 2, filterControl.Strips.Count);

				filterControl.CurrentGroupName = "New Group";

				AssertEquals("2 Group must exist as a result of default empty groupName", 2, filterControl.GroupControls.Count);
				AssertEquals("2 filter strips should exist (Originally added 2 filterStrips on new Group", 2, filterControl.Strips.Count);

				CombineAssertions(() =>
				{
					AssertEquals("2 ZFilterStrips should be found in first Group container + 1 Filter Color label", 3, filterControl.GroupControls[0].FilterStripGroupBox.Controls.Count);
					AssertEquals(false, filterControl.GroupControls[0].IsFilterCategoriesToolStripDropDownVisible);
					AssertEquals("0 ZFilterStrips should be found in second Group container + 1 Filter Color label", 1, filterControl.GroupControls[1].FilterStripGroupBox.Controls.Count);
					AssertEquals(true, filterControl.GroupControls[1].IsFilterCategoriesToolStripDropDownVisible);
				});
			}
		}

		public void TestRemoveIfOnlyGroup()
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			textFilter.Visibility = FilterVisibility.AlwaysVisible;

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(textFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				filterControl.CurrentGroupName = "New Group";

				AssertEquals("2 Group must exist as a result of default empty groupName", 2, filterControl.GroupControls.Count);

				filterControl.RemoveEmptyGroupStripControl("New Group");

				AssertEquals("No Group should be found because there should never be one group control", 0, filterControl.GroupControls.Count);
			}
		}

		#endregion

		#region Filter Layouts are user friendly
		public void TestGroupsAndSubGroupsAreWellConstructed()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			#region define filters
			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "TestFilter";
			filterA.S9_IsPublished = true;

			var filterB = dummyFilterStripBizO.Layouts.AddNew();
			filterB.S9_FilterName = "Test/Filter";
			filterB.S9_IsPublished = true;

			var filterC = dummyFilterStripBizO.Layouts.AddNew();
			filterC.S9_FilterName = "Test/Filter/First";
			filterC.S9_IsPublished = true;

			var filterD = dummyFilterStripBizO.Layouts.AddNew();
			filterD.S9_FilterName = "Test/Filter/Second";
			filterD.S9_IsPublished = true;

			var filterE = dummyFilterStripBizO.Layouts.AddNew();
			filterE.S9_FilterName = "Test/NewFilter";
			filterE.S9_IsPublished = true;

			var filterF = dummyFilterStripBizO.Layouts.AddNew();
			filterF.S9_FilterName = "TestFilter2";
			filterF.S9_IsPublished = true;

			var filterG = dummyFilterStripBizO.Layouts.AddNew();
			filterG.S9_FilterName = "Test";
			filterG.S9_IsPublished = true;

			var filterH = dummyFilterStripBizO.Layouts.AddNew();
			filterH.S9_FilterName = "TestFilter2/";
			filterH.S9_IsPublished = true;

			var filterI = dummyFilterStripBizO.Layouts.AddNew();
			filterI.S9_FilterName = "TestFilter3";
			filterI.S9_IsSystem = true;

			var filterJ = dummyFilterStripBizO.Layouts.AddNew();
			filterJ.S9_FilterName = "TestFilter3/Filter6";
			filterJ.S9_IsSystem = true;

			var filterK = dummyFilterStripBizO.Layouts.AddNew();
			filterK.S9_FilterName = "TestFilter3/Filter7";
			filterK.S9_IsPublished = true;

			var filterL = dummyFilterStripBizO.Layouts.AddNew();
			filterL.S9_FilterName = "TestFilter4";
			filterL.S9_IsPublished = false;
			#endregion

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				CombineAssertions(() =>
				{
					AssertEquals("The tool strip should have 7 layouts at the first level", 7, filterLayouts.Count);

					AssertEquals("The first layout should be My Filter Layouts", "My Filter Layouts".PadRight(37), filterLayouts[0].Text);
					AssertEquals("The second layout should be TestFilter4", "TestFilter4".PadLeft(15), filterLayouts[1].Text);
					AssertEquals("The third layout should be Shared Filter Layouts", "Shared Filter Layouts".PadRight(41), filterLayouts[2].Text);
					AssertEquals("The fourth layout should be Test", "Test".PadLeft(8), filterLayouts[3].Text);
					AssertEquals("The fifth layout should be TestFilter", "TestFilter".PadLeft(14), filterLayouts[4].Text);
					AssertEquals("The sixth layout should be TestFilter2", "TestFilter2".PadLeft(15), filterLayouts[5].Text);
					AssertEquals("The seventh layout should be TestFilter3", "TestFilter3".PadLeft(15), filterLayouts[6].Text);

					AssertEquals("The first layout should have 0 drop down item", 0, ((ZToolStripMenuItem)filterLayouts[0]).DropDownItems.Count);
					AssertEquals("The first layout should not be enabled", false, ((ZToolStripMenuItem)filterLayouts[0]).Enabled);
					AssertEquals("The second layout should have 0 drop down item", 0, ((ZToolStripMenuItem)filterLayouts[1]).DropDownItems.Count);
					AssertEquals("The third layout should have 0 drop down item", 0, ((ZToolStripMenuItem)filterLayouts[2]).DropDownItems.Count);
					AssertEquals("The third layout should not be enabled", false, ((ZToolStripMenuItem)filterLayouts[2]).Enabled);
					AssertEquals("The fourth layout should have 2 drop down items", 2, ((ZToolStripMenuItem)filterLayouts[3]).DropDownItems.Count);
					AssertEquals("The fifth layout should have 0 drop down items", 0, ((ZToolStripMenuItem)filterLayouts[4]).DropDownItems.Count);
					AssertEquals("The sixth layout should have 1 drop down item", 1, ((ZToolStripMenuItem)filterLayouts[5]).DropDownItems.Count);
					AssertEquals("The seven layout should have 1 drop down item", 2, ((ZToolStripMenuItem)filterLayouts[6]).DropDownItems.Count);
				});

				var testLayout = (ZToolStripMenuItem)filterLayouts[3];
				AssertEquals("The first sub menu should be Filter", "Filter", testLayout.DropDownItems[0].Text);
				AssertEquals("The second sub menu should be NewFilter", "NewFilter", testLayout.DropDownItems[1].Text);

				var subMenu = (ZToolStripMenuItem)testLayout.DropDownItems[0];
				AssertEquals("The 'Filter' layout should have 2 drop down items", 2, subMenu.DropDownItems.Count);
				AssertEquals("The first sub menu should be First", "First", subMenu.DropDownItems[0].Text);
				AssertEquals("The second sub menu should be Second", "Second", subMenu.DropDownItems[1].Text);

				AssertNotNull(subMenu.DropDownItems[0].Tag);

				AssertEquals("Public filter layouts should have correct ToolTipText.", "The filter layout 'Filter' is published system-wide.", testLayout.DropDownItems[0].ToolTipText);
				AssertImageEquals("Public filter layouts should have correct Image.", ZFilterControlImages.FilterImageList.Images["FavoriteUnselected"], testLayout.DropDownItems[0].Image);

				var systemLayout = (ZToolStripMenuItem)filterLayouts[6];
				AssertEquals("System-wide filter layouts should have correct ToolTipText.", "The filter layout Filter6 ships with " + Constants.ProductName + " and is published system-wide.", systemLayout.DropDownItems[0].ToolTipText);
				AssertImageEquals("System-wide filter layouts should have correct Image.", ZFilterControlImages.FilterImageList.Images["FavoriteUnselected"], systemLayout.DropDownItems[0].Image);

				var privateLayout = (ZToolStripMenuItem)filterLayouts[1];
				AssertEquals("Private filter layouts should have correct ToolTipText.", "This is your private filter layout 'TestFilter4'", privateLayout.ToolTipText);
				AssertImageEquals("Private filter layouts should have no Image.", ZFilterControlImages.FilterImageList.Images["FavoriteUnselected"], privateLayout.Image);
			}
		}

		public void TestUnpublishedFiltersAreBeforePublishedFilters()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "A";
			filterA.S9_IsPublished = false;

			var filterB = dummyFilterStripBizO.Layouts.AddNew();
			filterB.S9_FilterName = "B";
			filterB.S9_IsPublished = true;

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				CombineAssertions(() =>
				{
					AssertEquals("The tool strip should have 4 layouts", 4, filterLayouts.Count);
					AssertEquals("The first layout should be My Filter Layouts", "My Filter Layouts", filterLayouts[0].Text.Trim());
					AssertEquals("The first layout should be A", "A", filterLayouts[1].Text.Trim());
					AssertEquals("The third layout should be Shared Filter Layouts", "Shared Filter Layouts", filterLayouts[2].Text.Trim());
					AssertEquals("The second layout should be B", "B", filterLayouts[3].Text.Trim());
				});
			}
		}

		public void TestShouldInsertFiltersAtTheRightPlace()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "A";
			filterA.S9_IsPublished = false;

			var filterC = dummyFilterStripBizO.Layouts.AddNew();
			filterC.S9_FilterName = "C";
			filterC.S9_IsPublished = false;

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				AssertEquals("The tool strip should have 3 layouts", 3, filterLayouts.Count);
				AssertEquals("The My Filter Layouts title should first", "My Filter Layouts", filterLayouts[0].Text.Trim());
				AssertEquals("The first layout should be A", "A", filterLayouts[1].Text.Trim());
				AssertEquals("The second layout should be C", "C", filterLayouts[2].Text.Trim());

				var filterB = Factory.New<StmModuleFilter>();
				filterB.S9_FilterName = "B";
				filterB.S9_IsPublished = false;

				filterControl.AddItemToFindDropList(filterB);

				AssertEquals("The tool strip should have 4 layouts", 4, filterLayouts.Count);
				AssertEquals("The first layout should be A", "A", filterLayouts[1].Text.Trim());
				AssertEquals("The second layout should be B", "B", filterLayouts[2].Text.Trim());
				AssertEquals("The third layout should be C", "C", filterLayouts[3].Text.Trim());

				var filterD = Factory.New<StmModuleFilter>();
				filterD.S9_FilterName = "B/D";
				filterD.S9_IsPublished = false;

				filterControl.AddItemToFindDropList(filterD);

				AssertEquals("The tool strip should have 4 layouts", 4, filterLayouts.Count);
				AssertEquals("The first layout should be A", "A", filterLayouts[1].Text.Trim());
				AssertEquals("The second layout should be B", "B", filterLayouts[2].Text.Trim());
				AssertEquals("The third layout should be C", "C", filterLayouts[3].Text.Trim());
				AssertEquals("The second layout should have sub menu D", "D", ((ZToolStripMenuItem)filterLayouts[2]).DropDownItems[0].Text.Trim());

				var filterAAA = Factory.New<StmModuleFilter>();
				filterAAA.S9_FilterName = "AAA";
				filterAAA.S9_IsPublished = true;

				filterControl.AddItemToFindDropList(filterAAA);

				AssertEquals("The tool strip should have 6 layouts", 6, filterLayouts.Count);
				AssertEquals("The fifth layout should be 'Shared Filter Layouts'", "Shared Filter Layouts", filterLayouts[4].Text.Trim());
				AssertEquals("The sixth layout should be AAA", "AAA", filterLayouts[5].Text.Trim());

				var filterE = Factory.New<StmModuleFilter>();
				filterE.S9_FilterName = "E";
				filterE.S9_IsPublished = false;

				filterControl.AddItemToFindDropList(filterE);

				AssertEquals("The tool strip should have 7 layouts", 7, filterLayouts.Count);
				AssertEquals("The last layout should have been inserted at position 5", "E", filterLayouts[4].Text.Trim());
			}
		}

		public void TestShouldInsertFiltersAtTheRightPlace_WhenChangingPrivacy()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filter1 = dummyFilterStripBizO.Layouts.AddNew();
			filter1.S9_FilterName = "Active/A";
			filter1.S9_IsPublished = false;

			var filter2 = dummyFilterStripBizO.Layouts.AddNew();
			filter2.S9_FilterName = "Test";
			filter2.S9_IsPublished = true;

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				form.Controls.Add(filterControl);
				form.Show();

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				AssertEquals("The drop down should have 4 layouts", 4, filterLayouts.Count);

				var filter3 = Factory.New<StmModuleFilter>();
				filter3.S9_FilterName = "Active/B";
				filter3.S9_IsPublished = true;

				filterControl.AddItemToFindDropList(filter3);

				CombineAssertions(() =>
				{
					AssertEquals("The drop down should have 5 layouts", 5, filterLayouts.Count);
					AssertEquals("There should be one 'Active' menu item in private", "Active", filterLayouts[1].Text.Trim());
					AssertEquals("There should be one 'Active' menu item in public, inserted at the right spot", "Active", filterLayouts[3].Text.Trim());
				});
			}
		}

		public void TestSpaceAroundForwardCharacterInPublishedFiltersDoesntThrowException()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			#region define filters
			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "Test1 / Filter1";
			filterA.S9_IsPublished = false;

			var filterB = dummyFilterStripBizO.Layouts.AddNew();
			filterB.S9_FilterName = "Test2 / Filter2";
			filterB.S9_IsPublished = true;

			var filterC = dummyFilterStripBizO.Layouts.AddNew();
			filterC.S9_FilterName = "Test2/Filter2/Level3A";
			filterC.S9_IsPublished = true;

			var filterD = dummyFilterStripBizO.Layouts.AddNew();
			filterD.S9_FilterName = "Test2 / Filter2 / Level3B";
			filterD.S9_IsPublished = true;

			var filterE = dummyFilterStripBizO.Layouts.AddNew();
			filterE.S9_FilterName = "Test2 / Filter3";
			filterE.S9_IsPublished = true;

			var filterF = dummyFilterStripBizO.Layouts.AddNew();
			filterF.S9_FilterName = " Test2/Filter4";
			filterF.S9_IsPublished = true;

			var filterG = dummyFilterStripBizO.Layouts.AddNew();
			filterG.S9_FilterName = "Test1/Filter5";
			filterG.S9_IsSystem = true;

			var filterH = dummyFilterStripBizO.Layouts.AddNew();
			filterH.S9_FilterName = "Test3 / Filter6";
			filterH.S9_IsSystem = true;
			#endregion

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				form.Controls.Add(filterControl);
				AssertNoExceptionThrown(() => form.Show());

				var filterLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;

				AssertEquals("The tool strip should have 6 layouts at the first level", 6, filterLayouts.Count);
			}
		}

		#endregion

		#region TestSaveLayout

		public void TestSaveLayout()
		{
			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);
			testFilter.Visibility = FilterVisibility.AlwaysVisible;
			testFilter.IsActive = true;
			testFilter.Property = "ABC";

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var mockFilterControl = new Mock<DummyZFilterStripControl>(collection, filterBizO) { CallBase = true };
				var filterControl = mockFilterControl.Object;

				form.Controls.Add(filterControl);
				form.Show();

				var mockHandler = new Mock<SaveLayoutUserQueryHandler>() { CallBase = true };
				var mockSaveLayoutBizObj = new Mock<SaveLayoutBizO>(filterBizO) { CallBase = true };
				mockSaveLayoutBizObj.Object.LayoutName = "hiwre7984wrhuj";

				using (var layoutForm = new SaveLayoutForm(mockSaveLayoutBizObj.Object, true, true))
				{
					layoutForm.IsOkToSave = true;

					mockHandler.Protected().Setup<SaveLayoutBizO>("GetSaveLayoutBizObj", ItExpr.IsAny<IModifyModuleAndGridLayout>()).Returns(mockSaveLayoutBizObj.Object);
					mockHandler.Protected().Setup<SaveLayoutForm>("GetSaveLayoutForm", ItExpr.IsAny<SaveLayoutBizO>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns(layoutForm);
					mockFilterControl.Protected().Setup<SaveLayoutUserQueryHandler>("GetQueryHander").Returns(mockHandler.Object);

					AssertNull("PreCondition", filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, mockSaveLayoutBizObj.Object.PublishLayout));

					filterControl.ToolStripSaveLayoutButtonExposed.PerformClick();

					AssertNotNull("There should be a stmModuleFilter with S9_FilterName", filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, mockSaveLayoutBizObj.Object.PublishLayout));
				}
			}
		}

		public void TestAddSingleFilter_DoesBulkInsert()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "TestFilter";
			filterA.S9_IsPublished = true;

			AssertEquals("There should be 1 published layout", 1, dummyFilterStripBizO.Layouts_PublishedOnly.Count);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				var initialCount = filterControl.AddItemToFindDropListCalledCount;
				form.Controls.Add(filterControl);
				form.Show();

				var addedLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				CombineAssertions(() =>
				{
					AssertEquals("The tool strip should have 2 layouts", 2, addedLayouts.Count);
					AssertEquals("Shared Filter Layouts should be 1st", addedLayouts[0].Text.Trim(), "Shared Filter Layouts");
					AssertEquals("Then Layout name should match", addedLayouts[1].Text.Trim(), filterA.DisplayName);
					AssertEquals("Items should not have been inserted in order", initialCount, filterControl.AddItemToFindDropListCalledCount);
				});
			}
		}

		public void TestAddMultipleFiltersInOrder_DoesBulkInsert()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "AAA";
			filterA.S9_IsPublished = true;
			var filterB = dummyFilterStripBizO.Layouts.AddNew();
			filterB.S9_FilterName = "BBB";
			filterB.S9_IsPublished = true;
			var filterC = dummyFilterStripBizO.Layouts.AddNew();
			filterC.S9_FilterName = "CCC";
			filterC.S9_IsPublished = true;

			AssertEquals("There should be 3 published layouts", 3, dummyFilterStripBizO.Layouts_PublishedOnly.Count);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				var initialCount = filterControl.AddItemToFindDropListCalledCount;
				form.Controls.Add(filterControl);
				form.Show();

				var addedLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				CombineAssertions(() =>
				{
					AssertEquals("The tool strip should have 4 layouts", 4, addedLayouts.Count);
					AssertEquals("Shared Filter Layouts should be 1st", addedLayouts[0].Text.Trim(), "Shared Filter Layouts");
					AssertEquals("Layout A should be 2nd", addedLayouts[1].Text.Trim(), filterA.DisplayName);
					AssertEquals("Layout B should be 3rd", addedLayouts[2].Text.Trim(), filterB.DisplayName);
					AssertEquals("Layout C should be 4th", addedLayouts[3].Text.Trim(), filterC.DisplayName);
					AssertEquals("Items should not have been inserted in order", initialCount, filterControl.AddItemToFindDropListCalledCount);
				});
			}
		}

		public void TestAddMultipleFiltersOutOfOrder_DoesBulkInsert()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterZ = dummyFilterStripBizO.Layouts.AddNew();
			filterZ.S9_FilterName = "ZZZ";
			filterZ.S9_IsPublished = true;
			var filterY = dummyFilterStripBizO.Layouts.AddNew();
			filterY.S9_FilterName = "YYY";
			filterY.S9_IsPublished = true;
			var filterX = dummyFilterStripBizO.Layouts.AddNew();
			filterX.S9_FilterName = "XXX";
			filterX.S9_IsPublished = true;

			AssertEquals("There should be 3 published layouts", 3, dummyFilterStripBizO.Layouts_PublishedOnly.Count);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				var initialCount = filterControl.AddItemToFindDropListCalledCount;
				form.Controls.Add(filterControl);
				form.Show();

				var addedLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;
				CombineAssertions(() =>
				{
					AssertEquals("The tool strip should have 4 layouts", 4, addedLayouts.Count);
					AssertEquals("Shared Filter Layouts should be 1st", addedLayouts[0].Text.Trim(), "Shared Filter Layouts");
					AssertEquals("Layout X should be 2nd", addedLayouts[1].Text.Trim(), filterX.DisplayName);
					AssertEquals("Layout Y should be 3rd", addedLayouts[2].Text.Trim(), filterY.DisplayName);
					AssertEquals("Layout Z should be 4th", addedLayouts[3].Text.Trim(), filterZ.DisplayName);
					AssertEquals("Items should not have been inserted in order", initialCount, filterControl.AddItemToFindDropListCalledCount);
				});
			}
		}

		public void TestAddMixOfPublishedAndUnpublishedFilters_DoesBulkInsert()
		{
			var dummyFilterStripBizO = new DummyFilterStripBusinessObject();

			var filterA = dummyFilterStripBizO.Layouts.AddNew();
			filterA.S9_FilterName = "AP";
			filterA.S9_IsPublished = true;
			var filterF = dummyFilterStripBizO.Layouts.AddNew();
			filterF.S9_FilterName = "FP";
			filterF.S9_IsPublished = true;
			var filterB = dummyFilterStripBizO.Layouts.AddNew();
			filterB.S9_FilterName = "BP";
			filterB.S9_IsPublished = true;

			var filterE = dummyFilterStripBizO.Layouts.AddNew();
			filterE.S9_FilterName = "EU";
			filterE.S9_IsPublished = false;
			var filterC = dummyFilterStripBizO.Layouts.AddNew();
			filterC.S9_FilterName = "CU";
			filterE.S9_IsPublished = false;
			var filterD = dummyFilterStripBizO.Layouts.AddNew();
			filterD.S9_FilterName = "DU";
			filterE.S9_IsPublished = false;

			AssertEquals("There should be 3 published layouts", 3, dummyFilterStripBizO.Layouts_PublishedOnly.Count);
			AssertEquals("There should be 3 unpublished layouts", 3, dummyFilterStripBizO.Layouts_UnpublishedOnly.Count);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, dummyFilterStripBizO);

				var initialCount = filterControl.AddItemToFindDropListCalledCount;
				form.Controls.Add(filterControl);
				form.Show();

				var addedLayouts = filterControl.ToolStripFindDropButton_ForTest.DropDownItems;

				//unpublished layouts are added before Published layouts
				CombineAssertions(() =>
				{
					AssertEquals("The tool strip should have 8 layouts", 8, addedLayouts.Count);
					AssertEquals("My Filter Layouts should be 1st", addedLayouts[0].Text.Trim(), "My Filter Layouts");
					AssertEquals("Layout CU should be 2nd", addedLayouts[1].Text.Trim(), filterC.DisplayName);
					AssertEquals("Layout DU should be 3rd", addedLayouts[2].Text.Trim(), filterD.DisplayName);
					AssertEquals("Layout EU should be 4th", addedLayouts[3].Text.Trim(), filterE.DisplayName);
					AssertEquals("Shared Filter Layouts should be 5th", addedLayouts[4].Text.Trim(), "Shared Filter Layouts");
					AssertEquals("Layout AP should be 6th", addedLayouts[5].Text.Trim(), filterA.DisplayName);
					AssertEquals("Layout BP should be 7th", addedLayouts[6].Text.Trim(), filterB.DisplayName);
					AssertEquals("Layout FP should be 8th", addedLayouts[7].Text.Trim(), filterF.DisplayName);
					AssertEquals("Items should not have been inserted in order", initialCount, filterControl.AddItemToFindDropListCalledCount);
				});
			}
		}

		#endregion

		#region TestUpdateLayout

		public void TestUpdateLayout()
		{
			using (var stripControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), new DummyFilterStripBusinessObject()))
			{
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(StripControl.MaxWorkWidth), stripControl.AddStripButtonExposed.Left);
				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, stripControl.ToolStripManageDropButtonExposed.DisplayStyle);
				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, stripControl.ToolStripSaveLayoutButtonExposed.DisplayStyle);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(StripControl.MaxWorkWidth - 8) - stripControl.ToolStripExposed.Width, stripControl.ToolStripExposed.Left);

				stripControl.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(619);

				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, stripControl.ToolStripManageDropButtonExposed.DisplayStyle);
				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, stripControl.ToolStripSaveLayoutButtonExposed.DisplayStyle);
				AssertEquals(stripControl.AddStripButtonExposed.Left - stripControl.ToolStripExposed.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(8), stripControl.ToolStripExposed.Left);

				stripControl.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(StripControl.MaxWorkWidth + 100);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(StripControl.MaxWorkWidth), stripControl.AddStripButtonExposed.Left);
				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, stripControl.ToolStripManageDropButtonExposed.DisplayStyle);
				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, stripControl.ToolStripSaveLayoutButtonExposed.DisplayStyle);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(StripControl.MaxWorkWidth) - stripControl.ToolStripExposed.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(8), stripControl.ToolStripExposed.Left);
			}
		}

		public void TestUpdateFilterStripPanelWidth()
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			textFilter.Visibility = FilterVisibility.AlwaysVisible;

			var textFilter2 = new ModuleTextFilter("more filter", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(textFilter);
			filterBizO.AddModuleFilterForTest(textFilter2);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Size = new Size(1100, 600);
				form.Show();

				filterControl.CurrentGroupName = "New Group";
				AssertEquals("2 Group must exist as a result of default empty groupName", 2, filterControl.GroupControls.Count);
				AssertEquals(filterControl.GroupControls[0].Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(StripControl.PanelGapWidth), filterControl.FilterStripsPanelExposed.Width);
			}
		}

		#endregion

		#region RecentItems

		public void TestRecentItemsPanelIsCorrectlySizedAndPositioned()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(2000);
				var toolbarRightPanelControl = new ZPanel();
				var mainFormToolbarStripControl = new ZToolStrip();
				toolbarRightPanelControl.Name = "ToolbarRightPanel";
				mainFormToolbarStripControl.Name = "MainFormToolbarStrip";
				form.Controls.Add(mainFormToolbarStripControl);
				form.Controls.Add(toolbarRightPanelControl);
				form.Controls.Add(module.EmbeddedControl);
				var recentItemsPanel = form.Controls.Find("RecentItemsPanel", false).FirstOrDefault();
				var recordsFoundLabel = form.Find(x => x.Name == "ToolStripRecordsFoundLabel").FirstOrDefault();
				var mainFormToolbarStrip = form.Find(x => x.Name == "MainFormToolbarStrip").FirstOrDefault() ?? form.Find(x => x.Name == "ToolBarPanel").FirstOrDefault()?.Find(x => x.Name == "Toolstrip").FirstOrDefault();
				var mainFormFilterStripsPanel = form.Find(x => x.Name == "FilterStripsPanel").FirstOrDefault();
				var moduleHeadingLabel = form.Find(x => x.Name == "ModuleHeadingLabel").FirstOrDefault();

				//Ensure the recent items panel does not cover the items found result label:
				var recentItemsPanelX =
					Math.Max(mainFormFilterStripsPanel.Right,
						(recordsFoundLabel?.Left ?? 0) + (recordsFoundLabel?.MaximumSize.Width ?? 0));

				//If the toolbar is so huge that it starts to overlap the Recent Items box, push it back until it no longer is:
				recentItemsPanelX = Math.Max(recentItemsPanelX, GetToolStripButtonsWidth((ZToolStrip)mainFormToolbarStrip) + (moduleHeadingLabel?.Right ?? 0));

				AssertEquals(form.Width - recentItemsPanel.Location.X - ControlDpiScalingHelper.ScaleToCurrentDpiY(24), recentItemsPanel.Width);
				AssertEquals(recentItemsPanel.Location,
					ControlDpiScalingHelper.NewScaledPoint(recentItemsPanelX,
						ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false));
			}
		}

		int GetToolStripButtonsWidth(ZToolStrip toolStrip)
		{
			var width = 0;
			if (toolStrip is { Items.Count: > 0 })
			{
				width += toolStrip.Items.Cast<ToolStripItem>().Sum(item => item.Width);
			}
			return width;
		}

		public void TestRecentItemsLabelDoesNotCoverSearchResults()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				var toolbarRightPanelControl = new ZPanel();
				var mainFormToolbarStripControl = new ZToolStrip();
				toolbarRightPanelControl.Name = "ToolbarRightPanel";
				mainFormToolbarStripControl.Name = "MainFormToolbarStrip";
				form.Controls.Add(mainFormToolbarStripControl);
				form.Controls.Add(toolbarRightPanelControl);
				form.Controls.Add(module.EmbeddedControl);
				var recentItemsPanel = form.Controls.Find("RecentItemsPanel", false).FirstOrDefault();
				var recordsFoundLabel = form.Find(x => x.Name == "ToolStripRecordsFoundLabel").FirstOrDefault();

				AssertLessThan(recordsFoundLabel.Location.X + recordsFoundLabel.MaximumSize.Width,
					recentItemsPanel.Location.X);
			}
		}

		public void TestRecentItemsLabelDoesNotCoverToolbar()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				var toolbarRightPanelControl = new ZPanel();
				var mainFormToolbarStripControl = new ZToolStrip();
				toolbarRightPanelControl.Name = "ToolbarRightPanel";
				mainFormToolbarStripControl.Name = "MainFormToolbarStrip";
				form.Controls.Add(mainFormToolbarStripControl);
				form.Controls.Add(toolbarRightPanelControl);
				form.Controls.Add(module.EmbeddedControl);
				var recentItemsPanel = form.Controls.Find("RecentItemsPanel", false).FirstOrDefault();
				var mainFormToolbarStrip = form.Find(x => x.Name == "MainFormToolbarStrip").FirstOrDefault();

				AssertLessThan(mainFormToolbarStrip.Location.X + mainFormToolbarStrip.Width,
					recentItemsPanel.Location.X);
			}
		}

		public void TestRecentItemsAddedToParent()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				var toolbarRightPanelControl = new ZPanel();
				var mainFormToolbarStripControl = new ZToolStrip();
				toolbarRightPanelControl.Name = "ToolbarRightPanel";
				mainFormToolbarStripControl.Name = "MainFormToolbarStrip";

				form.Controls.Add(mainFormToolbarStripControl);
				form.Controls.Add(toolbarRightPanelControl);

				var recentItemsPanel = form.Controls.Find("RecentItemsPanel", false);
				AssertEquals("Recent Panel not found on parent form", 0, recentItemsPanel.Length);

				form.Controls.Add(module.EmbeddedControl);

				var hostControl = form.Controls.Find("HostControl", true);
				AssertEquals("Host control found", 1, hostControl.Length);

				recentItemsPanel = form.Controls.Find("RecentItemsPanel", false);
				AssertEquals("Recent Panel found on parent form", 1, recentItemsPanel.Length);
			}
		}

		public void TestRecentItemsPanelUsesThemeColors()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				var hostControl = form.Controls.Find("HostControl", true);
				AssertEquals("Host control found", 1, hostControl.Length);

				var recentItemsControl = (RecentItemsControl)((ElementHost)hostControl[0]).Child;
				AssertNotNull(recentItemsControl);

				AssertEquals("Background Color", SystemDataRegistry.Instance.ColorTheme.NavBarRecentPanelBackground, recentItemsControl.PanelBackgroundColor);
				AssertEquals("Border Color", SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1, recentItemsControl.PanelBorderColor);
			}
		}

		public void TestRecentItemsPanelUsesSystemFont()
		{
			OFont.UseFontFromReg();
			AssertRecentItemControlFont("Tahoma");
			AssertRecentItemControlFont("Times New Roman");

			void AssertRecentItemControlFont(string font)
			{
				EnvProxy.Instance.Registry.SystemFontRegItem = font;
				using (var form = new Form())
				using (var module = new DummyFilterGridModule())
				{
					form.Controls.Add(module.EmbeddedControl);
					var hostControl = form.Controls.Find("HostControl", true);
					AssertEquals("Host control found", 1, hostControl.Length);

					var recentItemsControl = (RecentItemsControl)((ElementHost)hostControl[0]).Child;
					AssertNotNull(recentItemsControl);
					AssertEquals(font, OFont.NormalFontName);
					AssertEquals(OFont.NormalFontName, recentItemsControl.FontFamily.ToString());
				}
			}
		}

		#endregion

		#region MinFilterStripPanelHeight

		public void TestMinFilterStripPanelHeight()
		{
			using (var stripControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(Factory), new DummyFilterStripBusinessObject()))
			{
				AssertEquals(3, stripControl.LastFilterStripBottomExposed);
				AssertEquals(3 + ControlDpiScalingHelper.ScaleToCurrentDpiY(15), stripControl.LastGroupControlBottomPlusPaddingExposed);

				stripControl.MinFilterStripPanelHeightExposed = 200;

				AssertEquals(200, stripControl.LastFilterStripBottomExposed);
				AssertEquals(200 + ControlDpiScalingHelper.ScaleToCurrentDpiY(15), stripControl.LastGroupControlBottomPlusPaddingExposed);
			}
		}

		#endregion

		#region Control Collections

		public void TestNewControls_AreAddedToApproprateCollection()
		{
			var controlNamesThatDontNeedToBeInASplitContainerInInheritingClasses = Array.Empty<string>();

			using (var stripControl = new StripControl_ForTest(new DummyFilterStripBusinessObject()))
			{
				var neglectedControls = new List<string>();

				foreach (Control control in stripControl.Controls)
				{
					if (!stripControl.FilterStripControlsExposed.Contains(control) &&
						!stripControl.ToolStripControlsExposed.Contains(control) &&
						!controlNamesThatDontNeedToBeInASplitContainerInInheritingClasses.Contains(control.Name))
					{
						neglectedControls.Add(control.Name);
					}
				}

				AssertEquals(@"
A control was added to StripControl which may need to be added to the FilterStripControls or ToolStripControls collections.
These collections are used in implementations of StripControl (specifically FilterRuleFilterStripControl) to move their referenced controls into a StripContainer.
Controls added to FilterStripControls are moved to the top panel of the SplitContainer, while controls added to ToolStripControls are moved to the bottom panel.
Please add any new controls to the appropriate collection. For example, if you add a new ToolStrip to the toolbar (the one with the Find button), add it to ToolStripControls.
If the new control is meant to be loose and not move as new filter strips are added (like RecentItemsPanel), add it to controlNamesThatDontNeedToBeInASplitContainerInInheritingClasses in this test.
The following is a list of the controls that need to be dealt with:

" + string.Join("\n", neglectedControls) + "\n", 0, neglectedControls.Count);
			}
		}

		#endregion

		#region Toolbar Button Appearance

		public void TestDisabledStripControl_AddButton_ShouldAppearAndActDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var tagDef = helper.CreateTagDefinition(Factory, "TRA");
			var tagMag = helper.CreateTagMagnitude(tagDef, "LAL");
			var tagRule = Factory.New<ITagRule>();
			tagRule.TagRuleTemplate.TGL_TGM_Magnitude = tagMag.PK;

			var tagRuleBizo = (BusinessObject)tagRule;
			tagRuleBizo.FillWithValidTestData();
			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(tagRuleBizo);

			using (var form = (Form)controller.ShowEditForm(tagRuleBizo))
			{
				AssertAddButtonState(form, true);
			}

			using (var form = (Form)controller.ShowViewForm(tagRuleBizo))
			{
				AssertAddButtonState(form, false);
			}
		}

		static void AssertAddButtonState(Form form, bool shouldBeEnabled)
		{
			Application.DoEvents();
			var button = form.FindSingle<ZFilterStripAddButton>();
			AssertEquals(shouldBeEnabled, button.Enabled);

			var expectedRestIcon = shouldBeEnabled ? IconTypes.AddButtonRest : IconTypes.AddButtonDisabled;
			AssertEquals(Icons.GetImage(expectedRestIcon), button.AddButton.BackgroundImage);
		}

		#endregion

		#region Implementation

		static ZButton GetDeleteStripButton(ZFilterStrip filterStrip)
		{
			return (ZButton)typeof(ZFilterStrip).GetField(
				"DeleteStripButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(filterStrip);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fFilterControl != null)
			{
				fFilterControl.Dispose();
			}
		}

		DummyZFilterStripControl FilterControl
		{
			get
			{
				if (fFilterControl == null)
				{
					var filterBizO = new DummyFilterStripBusinessObject();
					var collection = new DummyBusinessObjectCollection(Factory);

					fFilterControl = new DummyZFilterStripControl(collection, filterBizO);
				}
				return fFilterControl;
			}
		}

		DummyZFilterStripControl fFilterControl;

		class StripControl_ForTest : StripControl
		{
			public StripControl_ForTest(FilterStripBusinessObject filterBizo)
				: base(filterBizo)
			{
			}

			public ZFilterStrip AddNewFilterStripToGroup(GroupStripControl control)
			{
				var strip = NewZFilterStrip();
				AddFilterStrip(strip, FilterBusinessObject.FilterStrips.AddNew(), control);
				return strip;
			}

			public IEnumerable<Control> FilterStripControlsExposed => FilterStripControls;
			public IEnumerable<Control> ToolStripControlsExposed => ToolStripControls;
			public List<GroupStripControl> GroupControlsExposed => GroupControls;
		}

		#endregion

		#region TestClear

		public void TestClear()
		{
			//Create instance of strips
			//Populate the strips with data/conditions
			var testFilter = new ModuleTextFilter("testFilter", DummyBizoSchema.GenericStringSchemaColumn);
			testFilter.Visibility = FilterVisibility.AlwaysVisible;

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			//Sets comparison operator as 'Contains'
			testFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;

			using (var form = new Form())
			{
				var filterControl = new DummyZFilterStripControl(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();

				//Assert that the strips contain this data
				AssertEquals("testFilter added and so should be active", true, testFilter.IsActive);
				AssertEquals("contains", testFilter.ComparisonOperator);
				filterControl.ClearStrips_Exposed();

				//Assert that the strips have now been cleared except for 'contains' field.
				AssertEquals("contains", testFilter.ComparisonOperator);
			}
		}
		#endregion

		#region Test Minimize

		public void TestEnsureStripWidthIsNotShortenedAfterMinimized()
		{
			using (var form = new Form())
			using (var module = new DummyFilterGridModule())
			{
				form.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1600, 900);
				form.Size = form.MinimumSize;

				var toolbarRightPanelControl = new ZPanel();
				var mainFormToolbarStripControl = new ZToolStrip();
				toolbarRightPanelControl.Name = "ToolbarRightPanel";
				mainFormToolbarStripControl.Name = "MainFormToolbarStrip";
				form.Controls.Add(mainFormToolbarStripControl);
				form.Controls.Add(toolbarRightPanelControl);
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var filterStripsPanel = form.Find(x => x.Name == "FilterStripsPanel").FirstOrDefault();
				AssertNotNull(filterStripsPanel);

				var originalWidth = filterStripsPanel.Size.Width;
				var originalHeight = filterStripsPanel.Size.Height;

				form.WindowState = FormWindowState.Minimized;
				form.Show();

				var currentWidth = filterStripsPanel.Size.Width;
				var currentHeight = filterStripsPanel.Size.Height;

				AssertEquals("Original width should not be shortened.", originalWidth, currentWidth);
				AssertEquals("Original height should not be shortened.", originalHeight, currentHeight);
			}
		}

		#endregion
	}

	public abstract class FilterStripControlSharedTest : TestCaseWithFactory
	{
		protected abstract IZDummyFilterStripForm GetNewFilterForm();
		protected abstract IZDummyFilterStripForm GetNewFilterForm(FilterStripBusinessObject filterBizo);

		#region Disable Search

		[ExpectNoExceptions]
		public void TestDisableSearchStopsLoadingOnOpen()
		{
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			using (var form = GetNewFilterForm())
			{
				form.GetFilterControl().PerformSearch += delegate
				{ Fail("Should not search"); };
				form.GetFilterControl().DisableSearch();
				((ZForm)form).Show();
			}
		}

		#endregion

		#region TestProcessDialogKeyWhenActiveControlIsNull

		[ExpectNoExceptions]
		public void TestProcessDialogKeyWhenActiveControlIsNull()
		{
			using (var form = GetNewFilterForm())
			{
				((ZForm)form).Show();
				form.GetAnotherTextBox().Focus();
				form.GetFilterControlExposed().ProcessDialogKeyExposed(Keys.Control | Keys.Enter);
			}
		}

		#endregion

		#region TestShowAutoRefresh

		public void TestShowAutoRefresh()
		{
			using (var form = GetNewFilterForm())
			{
				((ZForm)form).Show();

				AssertEquals(AutoRefreshWarningType.None, form.GetFilterControl().AutoRefreshWarning);
				AssertEquals(false, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);

				form.GetFilterControl().AutoRefreshWarning = AutoRefreshWarningType.SlowQuery;
				AssertEquals(true, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(form.GetFilterControl().AutoRefreshSlowQueryWarningMessage, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Text);

				form.GetFilterControl().AutoRefreshWarning = AutoRefreshWarningType.ErrorQuery;
				AssertEquals(true, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(form.GetFilterControl().AutoRefreshQueryQueryErrorMessage, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Text);

				form.GetFilterControl().AutoRefreshWarning = AutoRefreshWarningType.None;
				AssertEquals(false, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);

				// when filter is hidden
				form.GetFilterControl().IsFilterVisible = false;
				AssertEquals(AutoRefreshWarningType.None, form.GetFilterControl().AutoRefreshWarning);
				AssertEquals(false, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(DockStyle.Fill, form.GetFilterControlExposed().RelatedGrid.Dock);

				form.GetFilterControl().AutoRefreshWarning = AutoRefreshWarningType.SlowQuery;
				AssertEquals(true, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(form.GetFilterControl().AutoRefreshSlowQueryWarningMessage, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Text);
				AssertEquals(0, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Top);
				AssertEquals(form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), form.GetFilterControlExposed().RelatedGrid.Top);
				AssertEquals(DockStyle.None, form.GetFilterControlExposed().RelatedGrid.Dock);

				form.GetFilterControl().AutoRefreshWarning = AutoRefreshWarningType.ErrorQuery;
				AssertEquals(true, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(form.GetFilterControl().AutoRefreshQueryQueryErrorMessage, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Text);
				AssertEquals(0, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Top);
				AssertEquals(form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), form.GetFilterControlExposed().RelatedGrid.Top);
				AssertEquals(DockStyle.None, form.GetFilterControlExposed().RelatedGrid.Dock);

				form.GetFilterControl().AutoRefreshWarning = AutoRefreshWarningType.None;
				AssertEquals(false, form.GetFilterControlExposed().AutoRefreshWarningLabelExposed.Visible);
				AssertEquals(DockStyle.Fill, form.GetFilterControlExposed().RelatedGrid.Dock);
			}
		}

		#endregion

		#region TestTrimFindButtonText

		public void TestTrimFindButtonText()
		{
			var filterName = "This is a really long text that cannot fit in.";

			var filterBizO = new DummyFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext = DummyModuleIDs.Dummy.Name;

			using (var form = GetNewFilterForm(filterBizO))
			{
				((ZForm)form).Show();
				var descriptionOnlyLayout = CreateLayout(form.GetFilterControl().RelatedGrid, filterName, false);
				descriptionOnlyLayout.S9_FilterData = form.GetFilterControl().FilterBusinessObject.FilterStrips.GetLayoutAsXml();

				Factory.Save();
			}

			using (var form = GetNewFilterForm(filterBizO))
			{
				((ZForm)form).StartPosition = FormStartPosition.CenterScreen;
				((ZForm)form).Show();

				var buttonText = form.GetFilterControlExposed().FindButtonExposed.Text;

				AssertNotEquals("Text should trim to avoid overlap with other controls", filterName, buttonText);
				Assert("Text should trim to avoid overlap with other controls, was: " + buttonText, buttonText.StartsWith("Find (This is a really long text"));
				Assert("Trimmed text should should end with three dots, was: " + buttonText, buttonText.EndsWith("..."));
			}
		}

		#endregion

		#region TestSaveLayout

		StmModuleFilter CreateLayout(ZGrid grid, string filterName, bool saveColumnLayout)
		{
			var serialiser = new DataGridLayoutDataSetSerialiser();

			var layout = Factory.New<StmModuleFilter>();
			layout.S9_ModuleID = DummyModuleIDs.Dummy.Name;
			layout.S9_FilterName = filterName;
			layout.S9_IsPublished = true;

			layout.S9_SaveColumnLayout = saveColumnLayout;

			if (saveColumnLayout)
			{
				layout.S9_ColumnLayoutData = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(grid)).ToArray();
			}
			return layout;
		}

		public void TestSaveLayoutWhenSaveColumnLayoutChanges()
		{
			using (var form = GetNewFilterForm())
			{
				var layoutFactory = ((IModifyModuleAndGridLayout)form.GetFilterBizO()).Factory;
				var layout = layoutFactory.New<StmModuleFilter>();
				layout.S9_ModuleID = "Dummy";
				layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layout.S9_FilterName = "Test";
				layout.S9_SaveColumnLayout = true;
				layout.S9_ColumnLayoutData = ZBlob.FromAscii("AHJD*");
				layoutFactory.Save();

				((ZForm)form).Show();

				var grid = form.GetFilterControl().RelatedGrid;
				grid.CurrentColumnLayout = layout;

				form.GetFilterControlExposed().AddOrUpdateExistingFindDropListItemExposed(layout);

				var currentlySelectedFilter = layoutFactory.New<StmData>();
				foreach (var key in new GridLayoutContextKeyProviderHelper().GetAllGridIDsForStmData(grid))
				{
					currentlySelectedFilter.SD_Name = key;
					break;
				}

				currentlySelectedFilter.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedFilter.SD_GuidValue = layout.PK;

				layout.S9_SaveColumnLayout = false;

				layoutFactory.Save();

				AssertOnS9ColumnLayoutData(layout);
				AssertNotEquals("currentlyselectedFilter should have the grid layout information instead", ZBlob.Empty, currentlySelectedFilter.SD_BinaryValue);
			}
		}

		public virtual void AssertOnS9ColumnLayoutData(StmModuleFilter layout)
		{
			Assert("Form is not in the expected format", false);
		}

		public void TestFindButtonDropDownItemClickedNotSavesLayout()
		{
			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);
			testFilter.Visibility = FilterVisibility.AlwaysVisible;
			testFilter.IsActive = true;
			testFilter.Property = "ABC";

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			using (var form = GetNewFilterForm())
			{
				var mockFilterControl = new Mock<DummyZFilterStripControlCommon>(filterBizO) { CallBase = true };
				var filterControl = mockFilterControl.Object;

				((ZForm)form).Controls.Add(filterControl);
				((ZForm)form).Show();

				var mockHandler = new Mock<SaveLayoutUserQueryHandler>() { CallBase = true };
				var mockSaveLayoutBizObj = new Mock<SaveLayoutBizO>(filterBizO) { CallBase = true };
				mockSaveLayoutBizObj.Object.LayoutName = "hiwre7984wrhuj";
				mockSaveLayoutBizObj.Object.PublishLayout = ZBool.True;

				using (var layoutForm = new SaveLayoutForm(mockSaveLayoutBizObj.Object, true, true))
				{
					layoutForm.IsOkToSave = true;

					mockHandler.Protected().Setup<SaveLayoutBizO>("GetSaveLayoutBizObj", ItExpr.IsAny<IModifyModuleAndGridLayout>()).Returns(mockSaveLayoutBizObj.Object);
					mockHandler.Protected().Setup<SaveLayoutForm>("GetSaveLayoutForm", ItExpr.IsAny<SaveLayoutBizO>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns(layoutForm);
					mockFilterControl.Protected().Setup<SaveLayoutUserQueryHandler>("GetQueryHander").Returns(mockHandler.Object);

					AssertNull("PreCondition", filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, true));

					filterControl.HandleFindButtonDropDownItemClickExposed(filterControl.ToolStripFindDropButtonExposed.DropDownItems[0]);

					AssertEquals("LayoutManaget.SaveLayout not called", 0, filterBizO.Layouts.Count);
				}
			}
		}

		public void TestSaveGridLayout()
		{
			var filterBizO = new DummyFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext = DummyModuleIDs.Dummy.Name;

			using (var form = GetNewFilterForm(filterBizO))
			{
				((ZForm)form).Show();

				var moduleFilters = form.GetFilterControl().FilterBusinessObject.ModuleFilters;
				moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

				//  simulate a user creating and populating a filter strip
				var textStrip = form.GetFilterControl().FilterBusinessObject.FilterStrips[0];
				textStrip.FilterDescription = DummyBizoSchema.Z0_Description.Name;

				form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;
				form.GetFilterControl().RelatedGrid.RefreshTableStyles();

				var numberOnlyLayout = CreateLayout(form.GetFilterControl().RelatedGrid, "Number Only", true);
				numberOnlyLayout.S9_FilterData = form.GetFilterControl().FilterBusinessObject.FilterStrips.GetLayoutAsXml();

				var descriptionOnlyLayout = CreateLayout(form.GetFilterControl().RelatedGrid, "Description Only", false);
				descriptionOnlyLayout.S9_FilterData = form.GetFilterControl().FilterBusinessObject.FilterStrips.GetLayoutAsXml();

				for (var i = 0; i < 26; ++i)
				{
					var layout = CreateLayout(form.GetFilterControl().RelatedGrid, new string((char)('a' + i), 1), false);
					layout.S9_FilterData = form.GetFilterControl().FilterBusinessObject.FilterStrips.GetLayoutAsXml();
				}

				Factory.Save();

				form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.GetFilterControl().RelatedGrid.Columns.HasLayoutChanged = true; // last saved grid layout will be 'number: Invisible, Description:Visible's
			}

			filterBizO = new DummyFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext = DummyModuleIDs.Dummy.Name;
			using (var form = GetNewFilterForm(filterBizO))
			{
				((ZForm)form).Show();

				new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

				AssertEquals("Shared Filter Layouts", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[0].Text.Trim());
				AssertEquals("a", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[1].Text.Trim());
				AssertEquals("b", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[2].Text.Trim());
				AssertEquals("c", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[3].Text.Trim());
				AssertEquals("d", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[4].Text.Trim());

				AssertEquals("Description Only", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[5].Text.Trim());
				form.GetFilterControlExposed().FindButtonExposed.ShowDropDown();
				form.GetFilterControlExposed().FindButtonExposed.DropDownItems[5].PerformClick();

				AssertEquals("GridLayoutName should be 'Default' as this layout does not have SaveColumnLayout ticked", true, StmDataGridLayoutStorage.IsDefaultLayout(form.GetFilterControl().RelatedGrid.CurrentColumnLayout.ColumnLayoutName));

				AssertEquals(false, form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.GetFilterControl().RelatedGrid.Columns.HasLayoutChanged = true; // should save grid layout before changing to the selected one

				AssertEquals("e", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[6].Text.Trim());
				AssertEquals("f", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[7].Text.Trim());
				AssertEquals("g", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[8].Text.Trim());
				AssertEquals("h", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[9].Text.Trim());
				AssertEquals("i", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[10].Text.Trim());
				AssertEquals("j", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[11].Text.Trim());
				AssertEquals("k", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[12].Text.Trim());
				AssertEquals("l", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[13].Text.Trim());
				AssertEquals("m", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[14].Text.Trim());
				AssertEquals("n", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[15].Text.Trim());

				AssertEquals("Number Only", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[16].Text.Trim());
				form.GetFilterControlExposed().FindButtonExposed.DropDownItems[16].PerformClick();

				AssertEquals("GridLayoutName should be 'Number Only' as this layout does not have SaveColumnLayout ticked", "Number Only", form.GetFilterControl().RelatedGrid.CurrentColumnLayout.ColumnLayoutName);

				AssertEquals(true, form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.GetFilterControlExposed().FindButtonExposed.DropDownItems[5].PerformClick();

				AssertEquals("Z0_Number was visible before layout changed to Number Only and should have been saved", true, form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.GetFilterControl().RelatedGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				AssertEquals("o", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[17].Text.Trim());
				AssertEquals("p", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[18].Text.Trim());
				AssertEquals("q", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[19].Text.Trim());
				AssertEquals("r", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[20].Text.Trim());
				AssertEquals("s", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[21].Text.Trim());
				AssertEquals("t", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[22].Text.Trim());
				AssertEquals("u", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[23].Text.Trim());
				AssertEquals("v", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[24].Text.Trim());
				AssertEquals("w", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[25].Text.Trim());
				AssertEquals("x", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[26].Text.Trim());
				AssertEquals("y", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[27].Text.Trim());
				AssertEquals("z", form.GetFilterControlExposed().FindButtonExposed.DropDownItems[28].Text.Trim());
			}
		}

		public void TestSaveLayoutGUI_SaveColumnLayoutCheckBox()
		{
			// This just tests that the default S9_SaveColumnLayout is FALSE if CanSaveColumnLayouts is FALSE (true by default for both)
			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);
			testFilter.IsActive = true;

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var mockFilterControl = new Mock<ZFilterStripControlWithDummyTest.OverridenFilterControl>(collection, filterBizO) { CallBase = true };
				var filterControl = mockFilterControl.Object;

				filterControl.SetCanSaveColumnLayouts(false);
				form.Controls.Add(filterControl);
				form.Show();

				var mockHandler = new Mock<SaveLayoutUserQueryHandler>() { CallBase = true };
				var mockSaveLayoutBizObj = new Mock<SaveLayoutBizO>(filterBizO) { CallBase = true };
				mockSaveLayoutBizObj.Object.LayoutName = "LayoutWithNoColumns";

				using (var layoutForm = new SaveLayoutForm(mockSaveLayoutBizObj.Object, true, true))
				{
					layoutForm.IsOkToSave = true;

					mockHandler.Protected().Setup<SaveLayoutBizO>("GetSaveLayoutBizObj", ItExpr.IsAny<IModifyModuleAndGridLayout>()).Returns(mockSaveLayoutBizObj.Object);
					mockHandler.Protected().Setup<SaveLayoutForm>("GetSaveLayoutForm", ItExpr.IsAny<SaveLayoutBizO>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns(layoutForm);
					mockFilterControl.Protected().Setup<SaveLayoutUserQueryHandler>("GetQueryHander").Returns(mockHandler.Object);

					AssertNull("PreCondition", filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, mockSaveLayoutBizObj.Object.PublishLayout));
					filterControl.ToolStripSaveLayoutButtonExposed.PerformClick();

					AssertNotNull("There should be a stmModuleFilter with S9_FilterName", filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, mockSaveLayoutBizObj.Object.PublishLayout));
					AssertEquals("There should be no grid layout saved.", false, filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, mockSaveLayoutBizObj.Object.PublishLayout).S9_SaveColumnLayout);
				}
			}
		}

		public void TestSaveLayoutGUI_SaveColumnLayoutCheckBoxVisibility()
		{
			TestSaveLayoutGUI_SaveColumnLayoutCheckBoxVisibility(canSaveColumnLayouts: true);
			TestSaveLayoutGUI_SaveColumnLayoutCheckBoxVisibility(canSaveColumnLayouts: false);
		}

		void TestSaveLayoutGUI_SaveColumnLayoutCheckBoxVisibility(bool canSaveColumnLayouts)
		{
			var filterBizO = new DummyFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext = DummyModuleIDs.Dummy.Name;

			using (var form = GetNewFilterForm(filterBizO))
			{
				bool? saveColumnsCheckBoxVisibility = null;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var saveLayoutForm = f as SaveLayoutForm;

					saveLayoutForm.Shown += (_, x_) =>
					{
						saveColumnsCheckBoxVisibility = saveLayoutForm.FindSingleOrDefault<ZCheckBox>("SaveColumnsCheckBox").Visible;
					};
				});

				form.GetFilterControlExposed().SetCanSaveColumnLayouts(canSaveColumnLayouts);
				((ZForm)form).Show();

				var moduleFilters = form.GetFilterControl().FilterBusinessObject.ModuleFilters;
				moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description).IsActive = true; // Simluate a user selecting the filter strip
				form.GetFilterControlExposed().ToolStripSaveLayoutButtonExposed.PerformClick();

				AssertEquals(canSaveColumnLayouts, saveColumnsCheckBoxVisibility);
			}
		}

		public void TestManageLayoutGUI_SaveColumnLayoutCheckBoxVisibility()
		{
			TestManageLayoutGUI_SaveColumnLayoutCheckBoxVisibility(canSaveColumnLayouts: true);
			TestManageLayoutGUI_SaveColumnLayoutCheckBoxVisibility(canSaveColumnLayouts: false);
		}

		void TestManageLayoutGUI_SaveColumnLayoutCheckBoxVisibility(bool canSaveColumnLayouts)
		{
			var filterBizO = new DummyFilterBusinessObject();
			((IFilterStripBusinessObjectInternals)filterBizO).LayoutContext = DummyModuleIDs.Dummy.Name;

			using (var form = GetNewFilterForm(filterBizO))
			{
				form.GetFilterControlExposed().SetCanSaveColumnLayouts(canSaveColumnLayouts);
				((ZForm)form).Show();

				var moduleFilters = form.GetFilterControl().FilterBusinessObject.ModuleFilters;
				moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description).IsActive = true; // Simluate a user selecting the filter strip

				using (ZFormModaliser.SuspendDispose())
				{
					form.GetFilterControlExposed().ToolStripManageLayoutsButtonExposed.PerformClick();
					var manageLayoutsForm = (ManageLayoutsForm)ZFormModaliser.LastFormShownForTest;
					AssertNotNull(manageLayoutsForm);
					AssertEquals(canSaveColumnLayouts, manageLayoutsForm.SaveColumnsCheckBox.Visible);
				}
			}
		}

		public void TestCanSaveColumnLayouts()
		{
			using (var form = GetNewFilterForm())
			{
				AssertEquals("Should be able to save ColumnLayouts by default.", true, form.GetFilterControlExposed().CanSaveColumnLayoutsExposed);
			}
		}

		#endregion

		#region TestFirePerformSearch

		[ExpectNoExceptions]
		public void TestShouldPerformSearch()
		{
			var searched = false;

			using (var form = GetNewFilterForm())
			{
				((ZForm)form).Show();

				form.GetFilterControl().PerformSearch += delegate
				{ searched = true; };
				form.GetFilterControl().FirePerformSearch();
				Assert("Should have searched", searched);

				searched = false;
				form.GetFilterControlExposed().SetShouldPerformSearch(false);
				form.GetFilterControl().FirePerformSearch();
				Assert("Should not have searched", !searched);
			}
		}

		public void TestHasCustomSqlFilterAsDefault()
		{
			using (var form = GetNewFilterForm())
			{
				((ZForm)form).Show();
				Assert(form.GetFilterControl().FilterBusinessObject.HasCustomSqlFilter);
			}
		}
		#endregion

		#region IsFilterReadonly

		public void TestIsFilterReadonly()
		{
			using (var form = GetNewFilterForm())
			{
				form.GetFilterControl().IsFilterReadonly = true;
				((ZForm)form).Show();

				AssertEquals(true, form.GetFilterControlExposed().FilterStripsPanelExposed.Enabled);
				AssertEquals(false, form.GetFilterControlExposed().ToolStripHelpExposed.Enabled);
				AssertEquals(false, form.GetFilterControlExposed().ToolStripClearButtonExposed.Enabled);
				AssertEquals(false, form.GetFilterControlExposed().ToolStripFindDropButtonExposed.Enabled);
			}
		}

		#endregion

		#region TestDropDownItemsCanBeFound

		public void TestDropDownItemsCanBeFound()
		{
			using (var form = GetNewFilterForm())
			{
				var layoutFactory = ((IModifyModuleAndGridLayout)form.GetFilterBizO()).Factory;
				var layout = layoutFactory.New<StmModuleFilter>();
				layout.S9_ModuleID = "Dummy";
				layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layout.S9_FilterName = "Test";
				layout.S9_SaveColumnLayout = true;
				layout.S9_ColumnLayoutData = ZBlob.FromAscii("AHJD*");
				layoutFactory.Save();

				((ZForm)form).Show();

				var grid = form.GetFilterControl().RelatedGrid;
				grid.CurrentColumnLayout = layout;

				form.GetFilterControlExposed().AddOrUpdateExistingFindDropListItemExposed(layout);

				var currentlySelectedFilter = layoutFactory.New<StmData>();
				foreach (var key in new GridLayoutContextKeyProviderHelper().GetAllGridIDsForStmData(grid))
				{
					currentlySelectedFilter.SD_Name = key;
					break;
				}

				currentlySelectedFilter.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedFilter.SD_GuidValue = layout.PK;

				layout.S9_SaveColumnLayout = true;

				form.GetFilterControlExposed().ToolStripFindDropButtonExposed.DropDownItems.Clear();     //Make the GetItemFromFindDropButton function return null

				AssertNoExceptionThrown(() => layoutFactory.Save());
			}
		}

		#endregion

		#region TestAfterPerformSearchAsyncIconAndTextChange
		protected internal Task AsyncTask { get; private set; }

		[ExpectNoExceptions]
		public void TestAfterPerformSearchAsyncIconAndTextChange()
		{
			IFilterStripBaseControlForTest filterControl;
			using (var form = GetNewFilterForm())
			{
				((ZForm)form).Show();

				filterControl = form.GetFilterControlExposed();
				var originalText = filterControl.ToolStripFindDropButtonExposed.Text;
				var originalIcon = filterControl.ToolStripFindDropButtonExposed.Image;

				form.GetFilterControl().PerformSearchAsync += PerformSearchAsync;
				form.GetFilterControl().FirePerformSearch();

				AssertEquals("Search button text should be 'Cancel Search'", "Cancel Search",
					filterControl.ToolStripFindDropButtonExposed.Text);
				AssertEquals("Search button icon should be BlackWhite_Cross", Icons.GetImage(IconTypes.BlackWhite_Cross),
					filterControl.ToolStripFindDropButtonExposed.Image);

				AsyncTask.Wait();
				Application.DoEvents();

				AssertEquals("Search button text should be revert to original text", originalText,
					filterControl.ToolStripFindDropButtonExposed.Text);
				AssertEquals("Search button text should be revert to original icon", originalIcon,
					filterControl.ToolStripFindDropButtonExposed.Image);
			}

			void PerformSearchAsync(object sender, PerformSearchAsyncEventArgs e)
			{
				e?.OnBeginPerformSearchAsync?.Invoke();	
				AsyncTask = new Task(() =>
				{
					Thread.Sleep(1000);
					filterControl?.RelatedGrid?.BeginInvokeSafe(() =>
					{
						e?.OnAfterPerformSearchAsync?.Invoke();
					});
				});
				AsyncTask.Start();
			}
		}

		#endregion
	}
}
