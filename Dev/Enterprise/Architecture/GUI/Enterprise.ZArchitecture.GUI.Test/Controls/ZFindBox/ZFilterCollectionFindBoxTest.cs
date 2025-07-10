using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFilterCollectionFindBoxTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestShowEditUserDefinedFilterPopupWithCorruptedUserDefinedFilter()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published", true, false, true);

			StmModuleFilter layout = null;
			var newStaff = new BusinessObjectFactory().LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentUserPK));
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.FilterStrips.AddNew("[USR]Published");
				layout = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "My Filter", true, false, false);

				var savedLayoutUserData = layout.GetLayoutUserData(module.FilterBusinessObject.LayoutsHelper);
				const string xmlValues = @"<?xml version=""1.0""?>
<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <LayoutName>Publish Renamed</LayoutName>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";
				savedLayoutUserData.S0_FilterDataValues = Encoding.ASCII.GetBytes(xmlValues);
				savedLayoutUserData.Factory.Save();
			}

			EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed = false;

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				using (var form = (ZForm)module.ShowPopup())
				{
					Application.DoEvents();

					var filterControl = form.FindSingle<ZFilterStripBaseControl>();
					var filterStrip = filterControl.FindSingle<ZFilterStrip>();

					var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
					filterFindBox.PopupButton.PerformClick();
				}
			}
		}

		public void TestShowEditUserDefinedFilterPopup()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published", true, false, true);
			EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed = false;

			var newStaff = new BusinessObjectFactory().LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentUserPK));

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = "Published [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (UserDefinedFilterEmbeddedModulePopup)dialog;

					popup.Shown += (_, x_) =>
					{
						var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();
						AssertEquals(true, embeddedFilterControl.IsFilterReadonly);
						AssertEquals("View Published", popup.Text);
					};
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
			}
		}

		public void TestEllipsisButton_ShouldShowEmbeddedModuleForm()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				form.Controls.Add(findBox);
				form.Show();

				findBox.PopupButton.PerformClick();
				AssertWindowOpen("Test Module");
			}
		}

		public void TestDescription_ShouldShowZeroFiltersSelectedOnShow()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				form.Controls.Add(findBox);
				form.Show();
				AssertEquals("0 filters applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);
			}
		}

		public void TestSelectFilters_ShouldUpdateSelectedFiltersAndDescription()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			using (var form = new ZChildForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				// Add one filter
				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);
					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Walla Walla");

					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters.ActiveModuleFilters.Single();
					AssertEquals("Z0_Description", savedFilter.Description);
					AssertEquals("Walla Walla", savedFilter.Property);
				}

				// Add another filter
				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);
					AssertEquals("Just showing the popup should not clear any layout already selected, and yet...", "1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);
					AssertEquals("The filter strip that was first selected should still be there, and yet...", 1, popup.Module.FilterBusinessObject.FilterStrips.Count);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Code", f => f.Property = "Keyokuk");

					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("The additional filter should have been added to the selection, and yet...", "2 filters applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(2, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters.ModuleFilters["Z0_Description"];
					AssertEquals(true, savedFilter.IsActive);
					AssertEquals("Walla Walla", savedFilter.Property);

					savedFilter = (ModuleTextFilter)selectedFilters.ModuleFilters["Z0_Code"];
					AssertEquals(true, savedFilter.IsActive);
					AssertEquals("Keyokuk", savedFilter.Property);
				}

				// Change one of the filters
				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);
					((ModuleTextFilter)popup.Module.FilterBusinessObject.ModuleFilters["Z0_Description"]).Property = "Cucamonga";
					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("2 filters applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(2, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters.ModuleFilters["Z0_Description"];
					AssertEquals(true, savedFilter.IsActive);
					AssertEquals("The filter's property was changed so the business object should have been updated, and yet...", "Cucamonga", savedFilter.Property);

					savedFilter = (ModuleTextFilter)selectedFilters.ModuleFilters["Z0_Code"];
					AssertEquals(true, savedFilter.IsActive);
					AssertEquals("Keyokuk", savedFilter.Property);
				}

				// Remove a filter
				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.RemoveAllFilters("Z0_Description");

					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("One filter was made inactive, so it should have been removed from the selected layout, and yet...", "1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters.ActiveModuleFilters.Single();
					AssertEquals("Z0_Code", savedFilter.Description);
					AssertEquals("Keyokuk", savedFilter.Property);
				}
			}
		}

		public void TestCancelPopup_ShouldNotAffectPreviouslySelectedFilters()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Seattle");

					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters.ActiveModuleFilters.Single();
					AssertEquals("Z0_Description", savedFilter.Description);
					AssertEquals("Seattle", savedFilter.Property);
				}

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Code", f => f.Property = "Keyokuk");
					((ModuleTextFilter)popup.Module.FilterBusinessObject.ModuleFilters["Z0_Description"]).Property = "Cucamonga";

					popup.CancelButtonForTest.PerformClick();
					AssertEquals("The filters were changed but the popup was cancelled so the selected filters should not have changed, and yet...", "1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters.ActiveModuleFilters.Single();
					AssertEquals("Z0_Description", savedFilter.Description);
					AssertEquals("Seattle", savedFilter.Property);
				}
			}
		}

		public void TestMultipleFilterStripsForOneModuleFilter_ShouldStoreIndividualValues()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Walla Walla");
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Keyokuk");

					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("2 filters applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(2, selectedFilters.ActiveModuleFilters.Count);

					var savedFilter = (ModuleTextFilter)selectedFilters["Z0_Description"];
					AssertEquals(true, savedFilter.IsActive);
					AssertEquals("Walla Walla", savedFilter.Property);

					savedFilter = (ModuleTextFilter)selectedFilters["Z0_Description (1)"];
					AssertEquals(true, savedFilter.IsActive);
					AssertEquals("Keyokuk", savedFilter.Property);
				}
			}
		}

		public void TestFactorySave_ShouldNotSaveStmModuleFilters()
		{
			var startingFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var startingLayoutsCount = startingFactory.GetDatabaseCount(typeof(StmModuleFilter));

			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Cucamonga");

					popup.ExposedOKButtonForTesting.PerformClick();

					var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
					var finalLayoutsCount = newFactory.GetDatabaseCount(typeof(StmModuleFilter));

					AssertEquals("The layout shouldn't have been saved in the database, and yet...", startingLayoutsCount, finalLayoutsCount);
				}
			}
		}

		public void TestCodeBox_ShouldNotBeVisible()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Z0_Guid");

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				form.Controls.Add(findBox);
				form.Show();

				AssertEquals(false, findBox.CodeBox.Visible);
			}
		}

		public void TestConstructorWithSelectedLayout_ShouldSetSelectedLayoutAndUpdateDescription()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				AssertEquals("0 filters applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Cucamonga");

					popup.ExposedOKButtonForTesting.PerformClick();
				}
			}

			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				AssertEquals("1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

				var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
				AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);

				var savedFilter = (ModuleTextFilter)selectedFilters["Z0_Description"];
				AssertEquals(true, savedFilter.IsActive);
				AssertEquals("Cucamonga", savedFilter.Property);
			}
		}

		[GuiTest]
		public void TestSelectSavedLayout_ShouldStillCreateNewLayoutAsResult()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			// Create and save a new layout
			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Keyokuk");

					popup.Module.FilterBusinessObject.SaveLayout("Funny Place Names");

					popup.CancelButtonForTest.PerformClick();
				}

				var savedLayout = new BusinessObjectFactory().Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, SQLComparisonOperator.Equal, "Funny Place Names")).Single();
				AssertNotNull(savedLayout);

				// Select filters based on the saved layout
				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);
					var filterBizo = popup.Module.FilterBusinessObject;
					AssertEquals(1, filterBizo.FilterStrips.Count);
					AssertNull(filterBizo.FilterStrips.Cast<FilterStrip>().Single().CurrentModuleFilter);

					var filterControl = popup.Module.DisplayGrid.GetParentFilterControl();
					var findButton = filterControl.ToolStripFindDropButton_ForTest;
					AssertEquals("Find", findButton.Text);

					findButton.ShowDropDown(); // so that it doesn't click on the Favorite image
					findButton.DropDownItems[1].PerformClick();
					AssertEquals("Find (Funny Place Names)", findButton.Text);

					AssertEquals(1, filterBizo.FilterStrips.Count);
					AssertEquals("Z0_Description", filterBizo.FilterStrips.Cast<FilterStrip>().Single().CurrentModuleFilterDescription);

					popup.ExposedOKButtonForTesting.PerformClick();
				}

				var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
				AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);

				var savedFilter = (ModuleTextFilter)selectedFilters["Z0_Description"];
				AssertEquals(true, savedFilter.IsActive);
				AssertEquals("Keyokuk", savedFilter.Property);
			}

			// Change the saved layout
			var newSavedLayoutFilterBizo = new DummyFilterBusinessObject();
			newSavedLayoutFilterBizo.AddTextFilterStrip("Z0_Code", "Walla Walla");
			newSavedLayoutFilterBizo.SaveLayout("Funny Place Names");

			// Check that the selected filters haven't changed
			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);
					AssertEquals("The layout that the selection was based on was changed, but the selection should remain the same, and yet...", 1, popup.Module.FilterBusinessObject.FilterStrips.Count);

					var strip = popup.Module.FilterBusinessObject.FilterStrips.Cast<FilterStrip>().Single();
					AssertEquals("The layout that the selection was based on was changed, but the selection should remain the same, and yet...", "Z0_Description", strip.CurrentModuleFilterDescription);
					AssertEquals("The layout that the selection was based on was changed, but the selection should remain the same, and yet...", "Keyokuk", ((ModuleTextFilter)strip.CurrentModuleFilter).Property);
				}
			}
		}

		public void TestResultGrid_DoubleClickShouldDoNothing()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Description = "Keyokuk";
			factory.Save();

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = "Keyokuk");

					var filterControl = popup.Module.DisplayGrid.GetParentFilterControl();
					filterControl.Find();

					var grid = filterControl.Grid;
					grid.SetSelectedRows(new[] { 0 });
					var result = grid.GetFirstSelectedRow();
					AssertNotNull(result);

					grid.PerformMouseDownForTest(0, 2);
					AssertEquals("Double clicking a row in the grid shouldn't select anything or close the window because the purpose of the window is simply to define filter strips, and yet...", "0 filters applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);
				}
			}
		}

		public void TestDescription_ShouldNotCountAlwaysIncludedFilters()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var parentFilterBizo = module.FilterBusinessObject;
				var filter = parentFilterBizo.AddGuidFilterStrip("Group");
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				using (var form = new ZForm())
				using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
				{
					form.Controls.Add(findBox);
					form.Show();

					using (var popup = findBox.PopupForm_Exposed())
					{
						popup.ShowModal(findBox, form);

						var helper = new EmbeddedModuleTestHelper(popup);
						helper.AddFilter<ModuleTextFilter>("Description", f => f.Property = "As long as it rhymes, everything will be fines");

						popup.ExposedOKButtonForTesting.PerformClick();

						AssertEquals("1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

						var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
						AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);
						AssertEquals("There should be extra filters for this module that don't appear in the description but are included in the query, and yet...", true, selectedFilters.ActiveModuleFiltersForQuery.Count > 1);
					}
				}
			}
		}

		public void TestDescription_ShouldIncludeEmptyQueryFiltersWithStrips()
		{
			var parentFilterBizObj = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(parentFilterBizObj.Embeddedfilter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				form.Controls.Add(findBox);
				form.Show();

				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.ShowModal(findBox, form);

					var helper = new EmbeddedModuleTestHelper(popup);
					helper.AddFilter<ModuleTextFilter>("Z0_Description", f => f.Property = null);

					popup.ExposedOKButtonForTesting.PerformClick();

					AssertEquals("1 filter applied", findBox.ModuleFilter_Exposed.SelectedFiltersDescription);

					var selectedFilters = findBox.ModuleFilter_Exposed.SelectedFilters;
					AssertEquals(1, selectedFilters.ActiveModuleFilters.Count);
					AssertEquals("The filter should be selected even though the value was left blank, resulting in an empty query, and yet...", 1, selectedFilters.ActiveModuleFiltersForQuery.Count);
					AssertEquals(true, selectedFilters.ActiveModuleFilters.Single().Query.IsEmpty);
				}
			}
		}

		public void TestEditUserDefinedFilter_WhenNotAllowed_ShouldUseViewModePopup()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Published", true, false, true);
			EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = false;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = "Published [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (UserDefinedFilterEmbeddedModulePopup)dialog;

					popup.Shown += (_, x_) =>
					{
						var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();
						AssertEquals(true, embeddedFilterControl.IsFilterReadonly);
						AssertEquals("View Published", popup.Text);
					};
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
			}

			EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = true;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = "Published [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (UserDefinedFilterEmbeddedModulePopup)dialog;

					popup.Shown += (_, x_) =>
					{
						var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();
						AssertEquals(false, embeddedFilterControl.IsFilterReadonly);
						AssertEquals("Edit Published", popup.Text);
					};
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
			}
		}

		public void TestCreateEmbeddedPopup_ForFilterRuleFilterStrip_ShouldSetFilterRuleModeOnPopupObject()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				module.FilterBusinessObject.IsInFilterRuleMode = true;

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Guid");
				filterControl.AddFilterStrip(strip);
				var filter = (ModuleGuidFilter)strip.CurrentModuleFilter;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Application.DoEvents();

				var filterFindBox = form.FindSingle<ZFilterCollectionFindBox>();
				var wasFilterRuleModeCopied = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (FilterCollectionEmbeddedModulePopup)dialog;

					popup.Shown += (_, x_) =>
					{
						wasFilterRuleModeCopied = popup.Module.FilterBusinessObject.IsInFilterRuleMode;
					};
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();

				AssertEquals("Filter rule mode should be transferred from the parent module to the embedded one.", true, wasFilterRuleModeCopied);
			}
		}

		public void TestUserDefendFilterShouldNotLoadFilterBizOIndexSearchFilterIfSqlLayout()
		{
			var factory = new BusinessObjectFactory();
			var dummyLayout = factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_FilterName = "filter";

			AssertEquals(false, dummyLayout.S9_IsIndexSearch);
			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				form.Controls.Add(findBox);
				form.Show();

				using var popup = findBox.PopupForm_Exposed();
				AssertEquals(false, popup.Module.ShouldLoadFilterBizOIndexSearchFilter);
			}
		}

		public void TestUserDefendFilterShouldLoadFilterBizOIndexSearchFilterIfIndexLayout()
		{
			var factory = new BusinessObjectFactory();
			var dummyLayout = factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_FilterName = "filter";
			dummyLayout.S9_IsIndexSearch = true;
			AssertEquals(true, dummyLayout.S9_IsIndexSearch);

			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				form.Controls.Add(findBox);
				form.Show();

				using var popup = findBox.PopupForm_Exposed();
				AssertEquals(true, popup.Module.ShouldLoadFilterBizOIndexSearchFilter);
			}
		}

		public void TestUserDefendFilterAllowToggleIndexSearchFilterMenuItemVisibility()
		{
			var factory = new BusinessObjectFactory();
			var dummyLayout = factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_FilterName = "filter";
			dummyLayout.S9_IsIndexSearch = true;
			AssertEquals(true, dummyLayout.S9_IsIndexSearch);

			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				form.Controls.Add(findBox);
				form.Show();

				using var popup = findBox.PopupForm_Exposed();
				AssertEquals(false, popup.Module.AllowToggleIndexSearchFilterMenuItemVisibility);
			}
		}

		public void TesModuleGuidFilterAllowToggleIndexSearchFilterMenuItemVisibility()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(new BusinessObjectFactory()));

			using (var form = new ZForm())
			using (var findBox = new ZFilterCollectionFindBoxForTest(filter))
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				form.Controls.Add(findBox);
				form.Show();

				using var popup = findBox.PopupForm_Exposed();
				AssertEquals(false, popup.Module.AllowToggleIndexSearchFilterMenuItemVisibility);
			}
		}

		#region Implementation

		class DummyBizoCollection : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummyBizoCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter) { }
		}

		class ModuleFilterForTest : ModuleGuidFilter
		{
			public ModuleFilterForTest()
				: base("", DummyModuleIDs.Dummy, (ZGuid value) => { return new ZQuery(); }, new DummyBusinessObjectCollection(new BusinessObjectFactory()))
			{
			}
		}

		class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			public FilterStripBusinessObjectForTest()
			{
				QueryObjectType = typeof(FilterStripBusinessObjectForTest);
			}

			public ModuleGuidFilter Embeddedfilter => embeddedfilter ?? (embeddedfilter = AddGuidFilterStrip("Test Filter"));
			ModuleGuidFilter embeddedfilter;

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = new ModuleFilterCollection();
				filters.AddGuidFilter("Test Filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBizoCollection(new BusinessObjectFactory(), new ZQuery()));

				return filters;
			}
		}

		internal class ZFilterCollectionFindBoxForTest : ZFilterCollectionFindBox
		{
			public ZFilterCollectionFindBoxForTest(ModuleGuidFilter moduleFilter)
				: base(moduleFilter)
			{
			}

			public IModuleFilterWithSelectedFilters ModuleFilter_Exposed => ModuleFilter;

			public EmbeddedModulePopup PopupForm_Exposed()
			{
				return (EmbeddedModulePopup)PopupForm;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		static void AssertWindowOpen(string windowTitleStartsWith)
		{
			if (Application.OpenForms.Cast<Form>().Any(form => form.Text.StartsWith(windowTitleStartsWith)))
			{
				Assert(true);
			}
			else
			{
				Assert("There are no open forms that have a title that starts with " + windowTitleStartsWith, false);
			}
		}

		#endregion
	}
}
