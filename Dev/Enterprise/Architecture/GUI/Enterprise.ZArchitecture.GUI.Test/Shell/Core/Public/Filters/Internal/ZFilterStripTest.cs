using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZFilterStripTest : TestCaseWithFactory
	{
		#region FilterChangedEvent

		public void TestOrCategoryFiresFilterChangedEvent()
		{
			var strip = new FilterStrip(new MockFilterStripBizO().GetModuleFilters());
			using (var zstrip = new ZFilterStrip())
			{
				zstrip.FilterOrCategoryChanged += new EventHandler(zstrip_EventFired);
				zstrip.SetDataBinding(strip, "");
				strip.OrCategory = FilterOrCategory.Green;
				AssertEquals(true, eventFired);
			}
		}

		public void TestOrCategoryFiresFilterEditedEvent()
		{
			var strip = new FilterStrip(new MockFilterStripBizO().GetModuleFilters());
			using (var zstrip = new ZFilterStrip())
			{
				zstrip.FilterEdited += new EventHandler(zstrip_EventFired);
				zstrip.SetDataBinding(strip, "");
				strip.OrCategory = FilterOrCategory.Green;
				AssertEquals(true, eventFired);
			}
		}

		bool eventFired;

		void zstrip_EventFired(object sender, EventArgs e)
		{
			eventFired = true;
		}

		#endregion

		public void TestToKnownColorShouldNotContainFilterOrCategoryColor()
		{
			var cp = new ColourProperties();
			cp = ColourConverter.ToKnownColour(cp, SystemColors.ActiveCaptionText);
			AssertNotEquals(cp.Colour, SystemColors.ActiveCaptionText);
		}

		public void TestIsDeleteButtonEnabled()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var filterStrip = form.AddFilterStrip("Number Range (Decimal)");
				filterStrip.IsDeleteButtonEnabled = false;
				AssertEquals(false, filterStrip.DeleteStripButton.Enabled);
				AssertEquals(Icons.GetImage(IconTypes.MinusButtonDisabled), filterStrip.DeleteStripButton.BackgroundImage);

				filterStrip.IsDeleteButtonEnabled = true;
				AssertEquals(true, filterStrip.DeleteStripButton.Enabled);
				AssertEquals(Icons.GetImage(IconTypes.MinusButtonRest), filterStrip.DeleteStripButton.BackgroundImage);
			}
		}

		public void TestTextRangeControls()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				var filterStrip = form.AddFilterStrip("Text Range");
				AssertEquals("filterStrip.Controls.Count", 9, filterStrip.Controls.Count);

				var fromEdit = (ZTextBox)filterStrip.Controls[6];
				AssertEquals("fromEdit.CoreZ.BindToType", typeof(string), fromEdit.BindTo.GetType());

				var toEdit = (ZTextBox)filterStrip.Controls[8];
				AssertEquals("toCalcEdit.CoreZ.BindToType", typeof(string), fromEdit.BindTo.GetType());
			}
		}

		public void TestTabOrder()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var filterStrip = form.AddFilterStrip("Flags Filter");

				var controls = new Control[filterStrip.Controls.Count];
				filterStrip.Controls.CopyTo(controls, 0);
				Array.Sort(controls, (c1, c2) => c1.TabIndex - c2.TabIndex);

				CombineAssertions(delegate
				{
					AssertEquals("The description drop down should be first.", filterStrip.Controls["FilterDescriptionDropEdit"], controls[0]);
					AssertEquals("The delete strip button should be third last.", filterStrip.DeleteStripButton, controls[controls.Length - 3]);
					AssertEquals("The lock strip button should be second last.", filterStrip.FilterPropertyLockButton, controls[controls.Length - 2]);
					AssertEquals("The category button should be last.", filterStrip.Controls["FilterCategoriesToolStrip"], controls[controls.Length - 1]);
				});
			}
		}

		public void TestFlagsModuleFilterAndOrRadioButtons()
		{
			var result = new ModuleFilterCollection();
			GetFlagsQuery query = delegate { return new ZQuery(); };
			var flagsFilter = result.AddFlagsFilter("Flags Filter",
				new string[] { "Flag1", "Flag2", "Flag3", "Flag4", "Flag5", "Flag6", "Flag7", "Flag8", "Flag9", "Flag10" },
				new GetFlagsQuery[] { query, query, query, query, query, query, query, query, query, query });
			flagsFilter.ShowAddOrRadioBox = true;

			var strip = new FilterStrip(result);
			strip.FilterDescription = "Flags Filter";
			using (var zstrip = new ZFilterStrip())
			{
				zstrip.SetDataBinding(strip, "");
				AssertEquals(1, zstrip.Controls.Find("ORRadioButton", true).Length);
				AssertEquals(1, zstrip.Controls.Find("ANDRadioButton", true).Length);
			}

			flagsFilter.ShowAddOrRadioBox = false;
			strip = new FilterStrip(result);
			strip.FilterDescription = "Flags Filter";
			using (var zstrip = new ZFilterStrip())
			{
				zstrip.SetDataBinding(strip, "");
				AssertEquals(0, zstrip.Controls.Find("ORRadioButton", true).Length);
				AssertEquals(0, zstrip.Controls.Find("ANDRadioButton", true).Length);
			}
		}

		public void TestFireHeightChangedIfNecessary()
		{
			var filterBizO = new MockFilterStripBizO();
			using (var form = new MockFilterStripForm(filterBizO))
			{
				var strip = form.AddFilterStrip("Number Range (Decimal)");
				Assert("Initial Height of FilterPanel", Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiY(49) - strip.Parent.Height) < 3);

				strip.SetHeight(ControlDpiScalingHelper.ScaleToCurrentDpiY(66));
				Assert("New Height of FilterPanel", Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiY(92) - strip.Parent.Height) < 3);

				strip.SetHeight(ControlDpiScalingHelper.ScaleToCurrentDpiY(23));
				Assert("Initial Height of FilterPanel", Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiY(49) - strip.Parent.Height) < 3);
			}
		}

		public void TestFiltersMatch_WhenReadOnly_ShouldLeavePopupButtonEnabled()
		{
			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var tagDef = helper.CreateTagDefinition(factory, "TRA");
			var tagMag = helper.CreateTagMagnitude(tagDef, "LAL");
			var tagRule = factory.New<ITagRule>();
			tagRule.TagRuleTemplate.TGL_TGM_Magnitude = tagMag.PK;

			var tagRuleBizo = (BusinessObject)tagRule;
			tagRuleBizo.FillWithValidTestData();
			var filter = new FilterRuleProvider(ModuleIDs.BMFilterRule, (IRelatedModuleFilterSupportable)tagRuleBizo).GetOrCreateAndCacheFilter();
			FilterStripsTestHelper.AddFilterStrip<ModuleFilter>(filter, "Release Group", comparisonOperatorSetter: f => ((ModuleGuidFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);
			factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(tagRuleBizo);

			using (var form = (Form)controller.ShowEditForm(tagRuleBizo))
			{
				AssertReadOnlyFilterCollectionControl(form, false);
			}

			using (var form = (Form)controller.ShowViewForm(tagRuleBizo))
			{
				AssertReadOnlyFilterCollectionControl(form, true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		static void AssertReadOnlyFilterCollectionControl(Form form, bool shouldSubModuleFiltersBeReadOnly)
		{
			var findBoxes = form.FindAll<ZFilterCollectionFindBox>();

			foreach (var findBox in findBoxes)
			{
				var button = findBox.PopupButton;
				AssertEquals("The popup button should always be enabled, even when its control is read-only, and yet...", true, button.Enabled);

				button.PerformClick();
				var popup = Application.OpenForms.OfType<EmbeddedModulePopup>().SingleOrDefault();

				AssertNotNull(popup);

				var stripControl = popup.FindSingle<ZFilterStripControl>();
				AssertEquals(shouldSubModuleFiltersBeReadOnly, stripControl.IsFilterReadonly);
				AssertEquals(!shouldSubModuleFiltersBeReadOnly, popup.ExposedOKButtonForTesting.Visible);
				AssertEquals(shouldSubModuleFiltersBeReadOnly ? "Close" : "Cancel", popup.CancelButtonForTest.Text);

				popup?.Dispose();
			}
		}

		public void TestFiltersMatch_ShouldShowCorrectControls_Guid()
		{
			AssertFiltersMatchOperator_ShouldShowCorrectControls(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestFiltersMatch_ShouldShowCorrectControls_Nk()
		{
			AssertFiltersMatchOperator_ShouldShowCorrectControls(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestFiltersMatch_ShouldShowCorrectControls_TextAndNk()
		{
			AssertFiltersMatchOperator_ShouldShowCorrectControls(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		static void AssertFiltersMatchOperator_ShouldShowCorrectControls<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);

				AssertEquals("The guid find box should be visible when the default comparison operator is selected, and yet...", true, findBox.Visible);
				AssertEquals("The filter collection find box should not be visible when the default comparison operator is selected, and yet...", false, filterFindBox.Visible);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				AssertEquals("The guid box should not be visible when the 'matches filter' comparison operator is selected, and yet...", false, findBox.Visible);
				AssertEquals("The filter collection find box should be visible when the 'matches filter' comparison operator is selected, and yet...", true, filterFindBox.Visible);
			}
		}

		public void TestZFilterStripDisposeShouldNotCreateNewFilterStripBizo()
		{
			using (var form = new ZForm())
			{
				var filters = new ModuleFilterCollection();
				filters.AddGuidFilter("related incidents", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
				var strip = new FilterStrip(filters);
				strip.FilterDescription = "related incidents";
				var zStrip = new ZFilterStrip();
				zStrip.SetDataBinding(strip, "");
				form.Controls.Add(zStrip);

				form.Show();
			}

			var performanceStats = new PerformanceStatistic();
			var factoryName = performanceStats.FactoryStatistics.Select(x => x.Name).ToArray();

			AssertEquals("FilterStripBusinessObject Constructor should not exist", false, factoryName.Contains("FilterStripBusinessObject (Constructor)"));
		}

		public void TestResizeWidth_ShouldKeepControlCustomisations_Guid()
		{
			AssertResizeWidth_ShouldKeepControlCustomisations(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestResizeWidth_ShouldKeepControlCustomisations_Nk()
		{
			AssertResizeWidth_ShouldKeepControlCustomisations(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestResizeWidth_ShouldKeepControlCustomisations_TextAndNk()
		{
			AssertResizeWidth_ShouldKeepControlCustomisations(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		void AssertResizeWidth_ShouldKeepControlCustomisations<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				Application.DoEvents();

				var strip = form.AddFilterStrip("AAA");

				filter.IsActive = true;
				var selectedFiltersFilter = (ModuleFilterWithSelectedFilters<T>)filterBizo.ModuleFilters["AAA"];
				var comparisonList = form.GetComparisonList(strip);
				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);

				selectedFiltersFilter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Walla Walla");
				selectedFiltersFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				comparisonList.CodeBox.Text = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				AssertEquals("The code box should not be visible when the 'matches filter' comparison operator is selected, and yet...", false, findBox.Visible);
				AssertEquals("The filter collection find box should be visible when the 'matches filter' comparison operator is selected, and yet...", true, filterFindBox.Visible);
				AssertEquals("The filter description must be set", "1 filter applied", filterFindBox.DescriptionBox.Text);

				form.FilterStripControl.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
				Application.DoEvents();

				findBox = form.GetFindBox(strip);
				filterFindBox = form.GetFilterCollectionFindBox(strip);

				AssertEquals("Resizing the form, which calls UpdateLayout, should not restore the original control visibilities, and yet...", false, findBox.Visible);
				AssertEquals("Resizing the form, which calls UpdateLayout, should not restore the original control visibilities, and yet...", true, filterFindBox.Visible);
			}
		}

		public void TestUserDefinedFilter_ShouldHaveCorrectControls()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Keyokuk";
				strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Code");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "BBB";
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "Me filter", false, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				form.Show();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = "Me filter [+]";
				filterControl.ToolStripFindDropButton_ForTest.PerformClick(); // takes focus off the drop down so the filter loads
				Application.DoEvents();

				var comparisonOperatorDropDowns = filterStrip.Controls.Find("OperatorDropEdit", true);
				AssertEquals(0, comparisonOperatorDropDowns.Length);

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				AssertNotNull(filterFindBox);
				AssertEquals(true, filterFindBox.Visible);
				FilterStripsTestHelper.AssertFindBoxText("The description should be updated, and yet...", filterFindBox, "2 filters applied");
			}
		}

		public void TestFilter_ShouldBindFilterCollectionFindBox_Guid()
		{
			AssertFilter_ShouldBindFilterCollectionFindBox(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestFilter_ShouldBindFilterCollectionFindBox_Nk()
		{
			AssertFilter_ShouldBindFilterCollectionFindBox(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestFilter_ShouldBindFilterCollectionFindBox_TextAndNk()
		{
			AssertFilter_ShouldBindFilterCollectionFindBox(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		void AssertFilter_ShouldBindFilterCollectionFindBox<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Walla Walla");

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				SelectComparisonOperator(form, strip, ModuleTextFilter.ComparisonConstants.FiltersMatch);
				Application.DoEvents();
				var findBox = form.GetFilterCollectionFindBox(strip);

				FilterStripsTestHelper.AssertFindBoxText("The description should be updated, and yet...", findBox, "1 filter applied");
			}
		}

		public void TestFilterCollectionStrip_ShouldNotOverlapRemoveButton_Guid()
		{
			AssertFilterCollectionStrip_ShouldNotOverlapRemoveButton(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestFilterCollectionStrip_ShouldNotOverlapRemoveButton_Nk()
		{
			AssertFilterCollectionStrip_ShouldNotOverlapRemoveButton(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestFilterCollectionStrip_ShouldNotOverlapRemoveButton_TextAndNk()
		{
			AssertFilterCollectionStrip_ShouldNotOverlapRemoveButton(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		static void AssertFilterCollectionStrip_ShouldNotOverlapRemoveButton<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var comparisonList = form.GetComparisonList(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);
				var removeButton = strip.DeleteStripButton;

				comparisonList.CodeBox.Text = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Application.DoEvents();

				var findBoxRight = ControlTestHelper.GetControlAbsoluteRight(filterFindBox, form);
				var removeButtonLeft = ControlTestHelper.GetControlAbsoluteLeft(removeButton, form);

				AssertEquals(true, findBoxRight < removeButtonLeft);
			}
		}

		public void TestClear_ShouldClearGuidSelectedFilters()
		{
			AssertClear_ShouldClearSelectedFilters(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestClear_ShouldClearNkSelectedFilters()
		{
			AssertClear_ShouldClearSelectedFilters(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestClear_ShouldClearTextAndNkSelectedFilters()
		{
			AssertClear_ShouldClearSelectedFilters(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		static void AssertClear_ShouldClearSelectedFilters<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Keokuk");

			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				Application.DoEvents();

				var strip = form.AddFilterStrip("AAA");
				var filterFindBox = form.GetFilterCollectionFindBox(strip);

				SelectComparisonOperator(form, strip, ModuleTextFilter.ComparisonConstants.FiltersMatch);
				Application.DoEvents();

				FilterStripsTestHelper.AssertFindBoxText("The description should be updated, and yet...", filterFindBox, "1 filter applied");
				form.FilterStripControl.ToolStripClearButton_ForTest.PerformClick();
				FilterStripsTestHelper.AssertFindBoxText("The description should be updated, and yet...", filterFindBox, "0 filters applied");
				AssertEquals(0, ((ModuleFilterWithSelectedFilters<T>)strip.CurrentDataItem.CurrentModuleFilter).SelectedFilterCount);
			}
		}

		public void TestClear_ShouldNotClearGuidSelectedFilters_WhenStripIsLocked()
		{
			AssertClear_ShouldNotClearSelectedFilters_WhenStripIsLocked(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestClear_ShouldNotClearNkSelectedFilters_WhenStripIsLocked()
		{
			AssertClear_ShouldNotClearSelectedFilters_WhenStripIsLocked(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestClear_ShouldNotClearTextAndNkSelectedFilters_WhenStripIsLocked()
		{
			AssertClear_ShouldNotClearSelectedFilters_WhenStripIsLocked(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		static void AssertClear_ShouldNotClearSelectedFilters_WhenStripIsLocked<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Keokuk");

			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				Application.DoEvents();

				var strip = form.AddFilterStrip("AAA");
				strip.FilterPropertyLockButton.PerformClick();
				var filterFindBox = form.GetFilterCollectionFindBox(strip);

				SelectComparisonOperator(form, strip, ModuleTextFilter.ComparisonConstants.FiltersMatch);
				Application.DoEvents();

				FilterStripsTestHelper.AssertFindBoxText("The description should be updated, and yet...", filterFindBox, "1 filter applied");
				form.FilterStripControl.ToolStripClearButton_ForTest.PerformClick();
				FilterStripsTestHelper.AssertFindBoxText("The description should be updated, and yet...", filterFindBox, "1 filter applied");
				AssertEquals(1, ((ModuleFilterWithSelectedFilters<T>)strip.CurrentDataItem.CurrentModuleFilter).SelectedFilterCount);
			}
		}

		public void TestFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException_Guid()
		{
			AssertFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException(GetNewGuidFilterForSelectedFiltersTests());
		}

		public void TestFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException_Nk()
		{
			AssertFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException(GetNewNkFilterForSelectedFiltersTests());
		}

		public void TestFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException_TextAndNk()
		{
			AssertFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException(GetNewTextAndNkFilterForSelectedFiltersTests());
		}

		static void AssertFiltersMatchOnMultipleFilters_OnDispose_ShouldNotCauseException<T>(ModuleFilterWithSelectedFilters<T> filter)
			where T : IZType
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				Application.DoEvents();

				var strip1 = form.AddFilterStrip("AAA");
				var filter1 = (ModuleFilterWithSelectedFilters<T>)strip1.CurrentDataItem.CurrentModuleFilter;
				filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				var strip2 = form.AddFilterStrip("AAA");
				var filter2 = (ModuleFilterWithSelectedFilters<T>)strip2.CurrentDataItem.CurrentModuleFilter;
				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			} // Should not throw exception on dispose

			Assert(true);
		}

		public void TestTextFilter_HasComparisonOperatorIsTrue_ShouldHaveDropdown()
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddTextFilter("moo", DummyBizoSchema.Z0_NVarChar);
			filterBizo.ModuleFilters["moo"].Visibility = FilterVisibility.AlwaysVisible;

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				var filterControl = form.FilterStripControl;
				var filterStrip = filterControl.Strips.FirstOrDefault(s => s.CurrentDataItem != null && s.CurrentDataItem.FilterDescription.ToString() == "moo");
				var comparisionOperatorDropDowns = filterStrip.Controls.Find("OperatorDropEdit", true);
				AssertEquals(1, comparisionOperatorDropDowns.Length);
			}
		}
		public void TestTextFilter_HasComparisonOperatorIsFalse_ShouldHaveNoDropdown()
		{
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddFilter(new DummyModuleTextFilterHasNoComparisonOperator("moo", DummyBizoSchema.Z0_NVarChar));
			filterBizo.ModuleFilters["moo"].Visibility = FilterVisibility.AlwaysVisible;

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				var filterControl = form.FilterStripControl;
				var filterStrip = filterControl.Strips.FirstOrDefault(s => s.CurrentDataItem != null && s.CurrentDataItem.FilterDescription.ToString() == "moo");
				var comparisionOperatorDropDowns = filterStrip.Controls.Find("OperatorDropEdit", true);
				AssertEquals(0, comparisionOperatorDropDowns.Length);
			}
		}

		public void TestFiltersMatch_NkFilter_ShouldHaveReadOnlyTextBox()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			using (var form = (Control)module.ShowPopup())
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Staff");
				((ZFilterStripCommonControl)module.EmbeddedControl).AddFilterStrip(strip);
				var filter = (ModuleNkFilter)strip.CurrentModuleFilter;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				Application.DoEvents();
				UserIdleWorker.Flush();

				var textBox = form.FindSingle<ZTextBox>(x => x.Text == "0 filters applied");
				AssertEquals(true, textBox.ReadOnly);
			}
		}

		ModuleGuidFilter GetNewGuidFilterForSelectedFiltersTests(string description = "AAA") => new ModuleGuidFilter(description, DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
		ModuleNkFilter GetNewNkFilterForSelectedFiltersTests(string description = "AAA") => new ModuleNkFilter(description, DummyDependentBizoSchema.ZD1_Code, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));
		ModuleTextAndNkFilter GetNewTextAndNkFilterForSelectedFiltersTests(string description = "AAA") => new ModuleTextAndNkFilter(description, DummyDependentBizoSchema.ZD1_Code, DummyDependentBizoSchema.ZD1_Code, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));

		static void SelectComparisonOperator(MockFilterStripForm form, ZFilterStrip strip, string operatorValue)
		{
			var comparisonList = form.GetComparisonList(strip);
			comparisonList.SelectItem(operatorValue);
			comparisonList.OnItemSelected(comparisonList.LastSelectedItem, true);
		}

		#region ModuleGuidModuleSpecifiedFilter Tests

		static object FindItemStartsWith(ZDropEdit dropEdit, string text)
		{
			return AutocompleteSearchHelper.FindItem(dropEdit, dropEdit.List, text, AutoCompleteStringComparisonType.StartsWith);
		}

		public void TestModuleGuidModuleSpecifiedFilter_ShouldHaveModuleSelectionDropDown()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0);
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var moduleList = form.GetModuleList(strip);

				AssertNotNull(moduleList);
				AssertEquals(true, moduleList.Visible);

				AssertNotNull(FindItemStartsWith(moduleList, "Dummy"));
			}
		}

		public void TestGuidFilter_ShouldNotHaveModuleSelectionDropDown()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, GuidQueryWithOperatorAndOption, new DummyBusinessObjectCollection(Factory));
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");

				var dropEdits = ControlTestHelper.FindControls<ZDropEdit>(strip).ToArray();
				AssertEquals(true, dropEdits.Any());
				foreach (var dropEdit in dropEdits)
				{
					AssertNull("A regular ModuleGuidFilter should not have a drop edit for selecting a module, and yet...", FindItemStartsWith(dropEdit, "Dummy"));
				}

				Assert(true);
			}
		}

		public void TestGuidFilter_PropertyCode()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, GuidQueryWithOperatorAndOption, new DummyBusinessObjectCollection(Factory));
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo))
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				var findBox = form.GetFindBox(strip);
				findBox.CodeBox.Text = "TEXT";

				AssertEquals("TEXT", filter.PropertyCode);
			}
		}

		public void TestModuleGuidModuleSpecifiedFilter_ShouldHaveSpecifiedModulesListed()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0, new[] { DummyModuleIDs.Dummy, ModuleIDs.JobShipment });
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var moduleList = form.GetModuleList(strip);

				AssertNotNull(FindItemStartsWith(moduleList, "Dummy"));
				AssertNotNull(FindItemStartsWith(moduleList, "JobShipment"));
			}
		}

		public void TestClear_ShouldClearModuleSelectionDropDown()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0, new[] { DummyModuleIDs.Dummy, ModuleIDs.JobShipment });
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var moduleList = form.GetModuleList(strip);
				moduleList.CodeBox.Text = "Dummy";

				form.FilterStripControl.ToolStripClearButton_ForTest.PerformClick();

				AssertEquals(string.Empty, moduleList.CodeBox.Text);
			}
		}

		public void TestModuleSelectedOnFilter_ShouldUpdateBoundControl()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0, new[] { DummyModuleIDs.Dummy, ModuleIDs.JobShipment });
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var moduleList = form.GetModuleList(strip);

				AssertEquals(string.Empty, moduleList.CodeBox.Text);

				filter.SelectedModule = DummyModuleIDs.Dummy.Name;

				AssertEquals(DummyModuleIDs.Dummy.Name, moduleList.CodeBox.Text);
			}
		}

		public void TestModuleSelectionDropDown_ShouldNotShowDescriptionBox()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0, new[] { DummyModuleIDs.Dummy, ModuleIDs.JobShipment });
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var moduleList = form.GetModuleList(strip);

				AssertEquals(false, moduleList.DescriptionBox.Visible);
			}
		}

		public void TestNoModuleSelected_ShouldShowEmptyStringInDropDown()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0, new[] { DummyModuleIDs.Dummy, ModuleIDs.JobShipment });
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;
				var moduleList = form.GetModuleList(strip);

				AssertEquals("The text should be an empty string and not 'Not Assigned'", string.Empty, moduleList.CodeBox.Text);
			}
		}

		public void TestSelectModule_ShouldUpdateModuleIdOnOtherControls()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0);
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);

				AssertEquals(ModuleIDs.NotAssigned, findBox.ModuleID);
				AssertEquals(ModuleIDs.NotAssigned, filterFindBox.ModuleID);

				filter.SelectedModule = "WorkItem";

				AssertEquals(ModuleIDs.WorkItem, findBox.ModuleID);
				AssertEquals(ModuleIDs.WorkItem, filterFindBox.ModuleID);
			}
		}

		public void TestModuleGuidModuleSpecifiedFilter_ShouldNotHaveOverlappingControls()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0);
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);
				var comparisonList = form.GetComparisonList(strip);
				var moduleList = form.GetModuleList(strip);
				var removeButton = strip.DeleteStripButton;

				var comparisonListRight = ControlTestHelper.GetControlAbsoluteRight(comparisonList, form);
				var moduleListLeft = ControlTestHelper.GetControlAbsoluteLeft(moduleList, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, comparisonListRight < moduleListLeft);

				var moduleListRight = ControlTestHelper.GetControlAbsoluteRight(moduleList, form);
				var findBoxLeft = ControlTestHelper.GetControlAbsoluteLeft(findBox, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, moduleListRight <= findBoxLeft);

				var findBoxRight = ControlTestHelper.GetControlAbsoluteRight(findBox, form);
				var removeButtonLeft = ControlTestHelper.GetControlAbsoluteLeft(removeButton, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, findBoxRight < removeButtonLeft);

				var filterFindBoxLeft = ControlTestHelper.GetControlAbsoluteLeft(filterFindBox, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, moduleListRight <= filterFindBoxLeft);

				var filterFindBoxRight = ControlTestHelper.GetControlAbsoluteRight(filterFindBox, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, filterFindBoxRight < removeButtonLeft);
			}
		}

		public void TestSpecifiedModuleBlank_ShouldMakeFindButtonsReadOnly()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0);
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);
				var comparisonList = form.GetComparisonList(strip);

				AssertEquals(true, findBox.PopupButton.ReadOnly);
				AssertEquals(true, findBox.CodeBox.ReadOnly);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				AssertEquals(true, filterFindBox.PopupButton.ReadOnly);

				filter.SelectedModule = "ProcessHeader";
				AssertEquals(false, filterFindBox.PopupButton.ReadOnly);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				AssertEquals(false, findBox.PopupButton.ReadOnly);
				AssertEquals(false, findBox.CodeBox.ReadOnly);
			}
		}

		public void TestOrganisationModuleSelection_ShouldUseSpecialControl()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0);
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);

				AssertEquals("The default find box should be a regular ZGuidFindBox, and yet...", typeof(ZGuidFindBox), findBox.GetType());
				AssertEquals(true, findBox.Visible);
				AssertEquals(false, filterFindBox.Visible);

				var oldFindBox = findBox;
				filter.SelectedModule = "Organisation";
				findBox = form.GetFindBox(strip);
				AssertEquals("When the Organisation module is selected, the find box should be a ZOrganisationFindBox, and yet...", ObjectFactory.GetType("ZOrganisationFindBox"), findBox.GetType());
				AssertEquals(true, findBox.Visible);
				AssertEquals(false, filterFindBox.Visible);
				AssertEquals(true, oldFindBox.IsDisposed);

				oldFindBox = findBox;
				filter.SelectedModule = "Dummy";
				findBox = form.GetFindBox(strip);
				AssertEquals("The find box for any module other than Organisation should be a regular ZGuidFindBox, and yet...", typeof(ZGuidFindBox), findBox.GetType());
				AssertEquals(true, findBox.Visible);
				AssertEquals(false, filterFindBox.Visible);
				AssertEquals(true, oldFindBox.IsDisposed);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				findBox = form.GetFindBox(strip);
				AssertEquals("The find box for any module other than Organisation should be a regular ZGuidFindBox, and yet...", typeof(ZGuidFindBox), findBox.GetType());
				AssertEquals(false, findBox.Visible);
				AssertEquals(true, filterFindBox.Visible);

				filter.SelectedModule = "Organisation";
				findBox = form.GetFindBox(strip);
				AssertEquals("When the Organisation module is selected, the find box should be a ZOrganisationFindBox, and yet...", ObjectFactory.GetType("ZOrganisationFindBox"), findBox.GetType());
				AssertEquals("Though the type of find box has changed, it shoudl still be invisible since the 'filters match' option is selected, and yet...", false, findBox.Visible);
				AssertEquals(true, filterFindBox.Visible);

				filter.SelectedModule = "Dummy";
				findBox = form.GetFindBox(strip);
				AssertEquals("The find box for any module other than Organisation should be a regular ZGuidFindBox, and yet...", typeof(ZGuidFindBox), findBox.GetType());
				AssertEquals("Though the type of find box has changed, it shoudl still be invisible since the 'filters match' option is selected, and yet...", false, findBox.Visible);
				AssertEquals(true, filterFindBox.Visible);

				oldFindBox = findBox;
				filter.SelectedModule = "Dummy2";
				findBox = form.GetFindBox(strip);
				AssertEquals("The module change was between 2 non-organisation modules, so the find box should have been left alone, and yet...", oldFindBox, findBox);
			}
		}

		#endregion

		#region ModuleGuidInSubCollectionFilter Tests

		public void TestModuleGuidModuleSpecifiedFilter_ControlTabIndicesShouldBeSequencedCorrectly()
		{
			var filter = new DummyModuleGuidInSubCollectionFilter(Factory, new CodeDescriptionPairList());
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var controlsOrderedByPosition =
					strip.Controls.Cast<Control>()
					.Where(c => c.TabStop && c.Name != "DeleteStripButton" && c.Name != "FilterPropertyLockButton")
					.OrderBy(c => c.Left)
					.ToArray();

				CombineAssertions("Control tab indexes should be sequenced by their left offsets", () =>
				{
					for (var i = 0; i < controlsOrderedByPosition.Length; i++)
					{
						var control = controlsOrderedByPosition[i];
						AssertEquals(control.Name, i, control.TabIndex);
					}
				});
			}
		}

		class DummyModuleGuidInSubCollectionFilter : ModuleGuidInSubCollectionFilter
		{
			internal DummyModuleGuidInSubCollectionFilter(BusinessObjectFactory factory, CodeDescriptionPairList dropDownTypeNameList = null)
				: base("AAA", FilterCategories.Other, DummyModuleIDs.Dummy, new GetGuidQueryWithNotIn((_, x_) => new ZQuery()), new DummyBusinessObjectCollection(factory))
			{
				this.dropDownTypeNameList = dropDownTypeNameList;
			}

			readonly CodeDescriptionPairList dropDownTypeNameList;

			protected override CodeDescriptionPairList CreateDropDownTypeNameList()
			{
				return dropDownTypeNameList;
			}
		}

		#endregion

		#region ModuleGuidForeignCollectionFilter Tests

		public void TestForeignCollectionFilter_ShouldShowComparisonOperatorDropDown()
		{
			var filter = new ModuleGuidForeignCollectionFilter("AAA", DummyModuleIDs.DummyDependent, DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, new DummyDependentBusinessObjectCollection(Factory), typeof(DummyBusinessObject));
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var comparisonList = form.GetComparisonList(strip);

				AssertNotNull(comparisonList);
				AssertEquals(true, comparisonList.Visible);
			}
		}

		public void TestForeignCollectionFilter_ShouldShowCorrectControlsForAllComparisons()
		{
			var filter = new ModuleGuidForeignCollectionFilter("AAA", DummyModuleIDs.DummyDependent, DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, new DummyDependentBusinessObjectCollection(Factory), typeof(DummyBusinessObject));
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var findBox = form.GetFindBox(strip);
				var filterFindBox = form.GetFilterCollectionFindBox(strip);
				var comparisonList = form.GetComparisonList(strip);
				var moduleList = form.GetModuleList(strip);

				foreach (var option in filter.AllowedComparisonOperators)
				{
					comparisonList.CodeBox.Text = option;

					AssertNotNull(findBox);
					AssertEquals(false, findBox.Visible);

					AssertNotNull(filterFindBox);
					AssertEquals(true, filterFindBox.Visible);

					AssertNull(moduleList);
				}
			}
		}

		public void TestForeignCollectionFilter_ShouldNotHaveOverlappingControls()
		{
			var filter = new ModuleGuidModuleSpecifiedFilter("AAA", DummyDependentBizoSchema.ZD1_Z0);
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var filterFindBox = form.GetFilterCollectionFindBox(strip);
				var comparisonList = form.GetComparisonList(strip);
				var removeButton = strip.DeleteStripButton;

				var comparisonListRight = ControlTestHelper.GetControlAbsoluteRight(comparisonList, form);
				var filterFindBoxLeft = ControlTestHelper.GetControlAbsoluteLeft(filterFindBox, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, comparisonListRight < filterFindBoxLeft);

				var filterFindBoxRight = ControlTestHelper.GetControlAbsoluteRight(filterFindBox, form);
				var removeButtonLeft = ControlTestHelper.GetControlAbsoluteLeft(removeButton, form);
				AssertEquals("Controls should not overlap. Check GetGuidFilterControls() and possibly dpi scaling issues.", true, filterFindBoxRight < removeButtonLeft);
			}
		}

		#endregion

		static ZQuery GuidQueryWithOperatorAndOption(SQLComparisonOperator comparisonOperator, object value, ZString option)
		{
			return new ZQuery(DummyBizoSchema.PK, SQLComparisonOperator.Equal, value);
		}

		#region TestHookValueChangedFromCurrentModule

		public void TestHookValueChangedFromCurrentModule()
		{
			using (var parentsParent = new Control())
			{
				using (var ctr = new StripControl())
				{
					parentsParent.Controls.Add(ctr);
					using (var strip = new zFilterStripForTest())
					{
						ctr.Controls.Add(strip);
						var obj = new MyObject();

						strip.SetDataBinding(obj, "AllMyFilters");

						strip.FilterEdited += new EventHandler(strip_FilterEdited);

						strip.OnCurrentDataItemChanging_Exposed();
						strip.TestMethod();

						var filterStrip = strip.CurrentDataItem;
						filterStrip.FilterDescription = "<select something to filter by>";

						testEventFired = false;

						((ModuleFilterForTest)strip.CurrentDataItem.CurrentModuleFilter).PropertyForTest = 9;

						AssertEquals(true, testEventFired);
					}
				}
			}
		}

		public void TestHookValueChangedFromCurrentModuleWithoutParentControl()
		{
			using (var ctr = new StripControl())
			{
				using (var strip = new zFilterStripForTest())
				{
					ctr.Controls.Add(strip);
					var obj = new MyObject();

					strip.SetDataBinding(obj, "AllMyFilters");

					strip.FilterEdited += new EventHandler(strip_FilterEdited);

					strip.OnCurrentDataItemChanging_Exposed();
					strip.TestMethod();

					var filterStrip = strip.CurrentDataItem;
					filterStrip.FilterDescription = "<select something to filter by>";

					testEventFired = false;

					((ModuleFilterForTest)strip.CurrentDataItem.CurrentModuleFilter).PropertyForTest = 9;

					AssertEquals(true, testEventFired);
				}
			}
		}

		public void TestChangingModuleFilterShouldStopFiringOnFilterEdited()
		{
			using (var ctr = new StripControl())
			{
				using (var strip = new zFilterStripForTest())
				{
					ctr.Controls.Add(strip);
					strip.SetDataBinding(new MyObject(), "AllMyFilters");

					var filterStrip = strip.CurrentDataItem;
					filterStrip.FilterDescription = "<select something to filter by>";
					var selectSomethingToFilterByModuleFilter = (ModuleFilterForTest)filterStrip.CurrentModuleFilter;

					bool testEventFired;
					strip.FilterEdited += (sender, e) => testEventFired = true;

					testEventFired = false;
					selectSomethingToFilterByModuleFilter.PropertyForTest = 9;
					AssertEquals("Precondition: should have fired", true, testEventFired);

					filterStrip.FilterDescription = "";
					AssertNotEquals("Precondition", selectSomethingToFilterByModuleFilter, filterStrip.CurrentModuleFilter);
					testEventFired = false;
					selectSomethingToFilterByModuleFilter.PropertyForTest = 3;
					AssertEquals("Should not fire because current module filter has changed", false, testEventFired);
				}
			}
		}

		public void TestChangingModuleFilterShouldUpdateOrCategoryVisibility()
		{
			using (var ctr = new StripControl())
			{
				using (var strip = new zFilterStripForTest())
				{
					ctr.Controls.Add(strip);
					strip.SetDataBinding(new MyObject(), "AllMyFilters");

					var filterStrip = strip.CurrentDataItem;
					filterStrip.FilterDescription = "<select something to filter by>";
					var selectSomethingToFilterByModuleFilter = (ModuleFilterForTest)filterStrip.CurrentModuleFilter;
					AssertEquals(strip.FilterCategoriesToolStripDropDown.Visible, true);

					strip.SetDataBinding(new MyObject(), "AllMyFilters_OrCategoryReadOnly");
					filterStrip = strip.CurrentDataItem;
					filterStrip.FilterDescription = "<select something to filter by>";
					selectSomethingToFilterByModuleFilter = (ModuleFilterForTest)filterStrip.CurrentModuleFilter;
					AssertEquals(strip.FilterCategoriesToolStripDropDown.Visible, false);
				}
			}
		}

		public void TestOnCurrentDataItemChangingDoesNotFireOnFilterEdited()
		{
			using (var parentsParent = new Control())
			{
				using (var ctr = new StripControl())
				{
					parentsParent.Controls.Add(ctr);
					using (var strip = new zFilterStripForTest())
					{
						ctr.Controls.Add(strip);
						var obj = new MyObject();

						strip.SetDataBinding(obj, "AllMyFilters");

						strip.FilterEdited += new EventHandler(strip_FilterEdited);
						strip.OnCurrentDataItemChanging_Exposed();
						AssertEquals(false, testEventFired);
					}
				}
			}
		}

		public void TestDragDropFilterStrip()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var panel = form.FilterStripControl.Controls.Find("FilterStripsPanel", false)[0];

				const string selectSomething = "<select something to filter by>";
				const string numberRange = "Number Range (Decimal)";
				const string stage = "Stage";
				const string city = "City";

				form.AddFilterStrip(numberRange);
				var filterStripStage = form.AddFilterStrip(stage);
				var filterStripCity = form.AddFilterStrip(city);

				AssertEquals("panel.Controls.Count", 4, panel.Controls.Count);

				// Precondition
				AssertEquals("Precondition - " + selectSomething, form.FilterStripControl.Strips[0].FilterDescriptionDropEdit.Text, selectSomething);
				AssertEquals("Precondition - " + numberRange, form.FilterStripControl.Strips[1].FilterDescriptionDropEdit.Text, numberRange);
				AssertEquals("Precondition - " + stage, form.FilterStripControl.Strips[2].FilterDescriptionDropEdit.Text, stage);
				AssertEquals("Precondition - " + city, form.FilterStripControl.Strips[3].FilterDescriptionDropEdit.Text, city);

				AssertEquals("FilterBusinessObject Precondition - " + selectSomething, form.FilterStripControl.FilterBusinessObject.FilterStrips[0].FilterDescription, selectSomething);
				AssertEquals("FilterBusinessObject Precondition - " + numberRange, form.FilterStripControl.FilterBusinessObject.FilterStrips[1].FilterDescription, numberRange);
				AssertEquals("FilterBusinessObject Precondition - " + stage, form.FilterStripControl.FilterBusinessObject.FilterStrips[2].FilterDescription, stage);
				AssertEquals("FilterBusinessObject Precondition - " + city, form.FilterStripControl.FilterBusinessObject.FilterStrips[3].FilterDescription, city);

				var dataObj = new DataObject();
				dataObj.SetData(filterStripCity);
				var drgevent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);
				filterStripCity.UserControl_DragDrop(null, drgevent);

				AssertEquals("Order 1 - " + city, form.FilterStripControl.Strips[0].FilterDescriptionDropEdit.Text, city);
				AssertEquals("Order 1 - " + selectSomething, form.FilterStripControl.Strips[1].FilterDescriptionDropEdit.Text, selectSomething);
				AssertEquals("Order 1 - " + numberRange, form.FilterStripControl.Strips[2].FilterDescriptionDropEdit.Text, numberRange);
				AssertEquals("Order 1 - " + stage, form.FilterStripControl.Strips[3].FilterDescriptionDropEdit.Text, stage);

				AssertEquals("FilterBusinessObject Order 1 - " + city, form.FilterStripControl.FilterBusinessObject.FilterStrips[0].FilterDescription, city);
				AssertEquals("FilterBusinessObject Order 1 - " + selectSomething, form.FilterStripControl.FilterBusinessObject.FilterStrips[1].FilterDescription, selectSomething);
				AssertEquals("FilterBusinessObject Order 1 - " + numberRange, form.FilterStripControl.FilterBusinessObject.FilterStrips[2].FilterDescription, numberRange);
				AssertEquals("FilterBusinessObject Order 1 - " + stage, form.FilterStripControl.FilterBusinessObject.FilterStrips[3].FilterDescription, stage);

				var dataObj2 = new DataObject();
				dataObj2.SetData(filterStripStage);
				var drgevent2 = new DragEventArgs(dataObj2, 0, 1, 20, DragDropEffects.Move, DragDropEffects.Move);
				filterStripCity.UserControl_DragDrop(null, drgevent2);

				AssertEquals("Order 2 - " + stage, form.FilterStripControl.Strips[0].FilterDescriptionDropEdit.Text, stage);
				AssertEquals("Order 2 - " + city, form.FilterStripControl.Strips[1].FilterDescriptionDropEdit.Text, city);
				AssertEquals("Order 2 - " + selectSomething, form.FilterStripControl.Strips[2].FilterDescriptionDropEdit.Text, selectSomething);
				AssertEquals("Order 2 - " + numberRange, form.FilterStripControl.Strips[3].FilterDescriptionDropEdit.Text, numberRange);

				AssertEquals("FilterBusinessObject Order 2 - " + stage, form.FilterStripControl.FilterBusinessObject.FilterStrips[0].FilterDescription, stage);
				AssertEquals("FilterBusinessObject Order 2 - " + city, form.FilterStripControl.FilterBusinessObject.FilterStrips[1].FilterDescription, city);
				AssertEquals("FilterBusinessObject Order 2 - " + selectSomething, form.FilterStripControl.FilterBusinessObject.FilterStrips[2].FilterDescription, selectSomething);
				AssertEquals("FilterBusinessObject Order 2 - " + numberRange, form.FilterStripControl.FilterBusinessObject.FilterStrips[3].FilterDescription, numberRange);

				AssertEquals("panel.Controls.Count", 4, panel.Controls.Count);
			}
		}

		public void TestDragDropFilterStrip_WitGroup()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var panel = form.FilterStripControl.Controls.Find("FilterStripsPanel", false)[0];

				const string groupFirst = "First";
				const string groupSecond = "Second";
				const string capability = "Capability";
				const string numberRange = "Number Range (Decimal)";
				const string stage = "Stage";
				const string city = "City";

				var groupControlFirst = form.AddGroupStripControl(groupFirst);
				var groupControlSecond = form.AddGroupStripControl(groupSecond);
				var filterStripCapability = form.AddFilterStrip(capability, groupControlFirst);
				form.AddFilterStrip(numberRange, groupControlFirst);
				var filterStripStage = form.AddFilterStrip(stage, groupControlSecond);
				var filterStripCity = form.AddFilterStrip(city, groupControlSecond);

				AssertEquals("form.FilterStripControl.GroupStrips.Count", 2, form.FilterStripControl.GroupStrips.Count);

				// Precondition
				AssertEquals("Precondition - " + capability, form.FilterStripControl.GroupStrips[groupFirst][0].FilterDescriptionDropEdit.Text, capability);
				AssertEquals("Precondition - " + numberRange, form.FilterStripControl.GroupStrips[groupFirst][1].FilterDescriptionDropEdit.Text, numberRange);
				AssertEquals("Precondition - " + stage, form.FilterStripControl.GroupStrips[groupSecond][0].FilterDescriptionDropEdit.Text, stage);
				AssertEquals("Precondition - " + city, form.FilterStripControl.GroupStrips[groupSecond][1].FilterDescriptionDropEdit.Text, city);

				AssertEquals("FilterBusinessObject Precondition - " + capability, form.FilterStripControl.FilterBusinessObject.FilterStrips[1].FilterDescription, capability);
				AssertEquals("FilterBusinessObject Precondition - " + numberRange, form.FilterStripControl.FilterBusinessObject.FilterStrips[2].FilterDescription, numberRange);
				AssertEquals("FilterBusinessObject Precondition - " + stage, form.FilterStripControl.FilterBusinessObject.FilterStrips[3].FilterDescription, stage);
				AssertEquals("FilterBusinessObject Precondition - " + city, form.FilterStripControl.FilterBusinessObject.FilterStrips[4].FilterDescription, city);

				var dataObj = new DataObject();
				dataObj.SetData(filterStripCity);
				var drgevent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);
				filterStripCity.UserControl_DragDrop(null, drgevent);

				// Reorder in the same group
				AssertEquals("Order 1 - " + capability, form.FilterStripControl.GroupStrips[groupFirst][0].FilterDescriptionDropEdit.Text, capability);
				AssertEquals("Order 1 - " + numberRange, form.FilterStripControl.GroupStrips[groupFirst][1].FilterDescriptionDropEdit.Text, numberRange);
				AssertEquals("Order 1 - " + city, form.FilterStripControl.GroupStrips[groupSecond][0].FilterDescriptionDropEdit.Text, city);
				AssertEquals("Order 1 - " + stage, form.FilterStripControl.GroupStrips[groupSecond][1].FilterDescriptionDropEdit.Text, stage);

				AssertEquals("FilterBusinessObject Order 1 - " + capability, form.FilterStripControl.FilterBusinessObject.FilterStrips[1].FilterDescription, capability);
				AssertEquals("FilterBusinessObject Order 1 - " + numberRange, form.FilterStripControl.FilterBusinessObject.FilterStrips[2].FilterDescription, numberRange);
				AssertEquals("FilterBusinessObject Order 1 - " + city, form.FilterStripControl.FilterBusinessObject.FilterStrips[3].FilterDescription, city);
				AssertEquals("FilterBusinessObject Order 1 - " + stage, form.FilterStripControl.FilterBusinessObject.FilterStrips[4].FilterDescription, stage);

				var dataObj2 = new DataObject();
				dataObj2.SetData(filterStripStage);
				var drgevent2 = new DragEventArgs(dataObj2, 0, 1, 20, DragDropEffects.Move, DragDropEffects.Move);
				filterStripCapability.UserControl_DragDrop(null, drgevent2);

				// Change group and reorder
				AssertEquals("Order 2 - " + stage, form.FilterStripControl.GroupStrips[groupFirst][0].FilterDescriptionDropEdit.Text, stage);
				AssertEquals("Order 2 - " + capability, form.FilterStripControl.GroupStrips[groupFirst][1].FilterDescriptionDropEdit.Text, capability);
				AssertEquals("Order 2 - " + numberRange, form.FilterStripControl.GroupStrips[groupFirst][2].FilterDescriptionDropEdit.Text, numberRange);
				AssertEquals("Order 2 - " + city, form.FilterStripControl.GroupStrips[groupSecond][0].FilterDescriptionDropEdit.Text, city);

				AssertEquals("FilterBusinessObject Order 2 - " + stage, form.FilterStripControl.FilterBusinessObject.FilterStrips[1].FilterDescription, stage);
				AssertEquals("FilterBusinessObject Order 2 - " + capability, form.FilterStripControl.FilterBusinessObject.FilterStrips[2].FilterDescription, capability);
				AssertEquals("FilterBusinessObject Order 2 - " + numberRange, form.FilterStripControl.FilterBusinessObject.FilterStrips[3].FilterDescription, numberRange);
				AssertEquals("FilterBusinessObject Order 2 - " + city, form.FilterStripControl.FilterBusinessObject.FilterStrips[4].FilterDescription, city);
			}
		}

		void strip_FilterEdited(object sender, EventArgs e)
		{
			testEventFired = true;
		}

		bool testEventFired;

		class MyObject
		{
			public SchemaStringColumn filterColumn { get; set; }
			public FilterStrip AllMyFilters => AllMyFiltersCore(false);
			public FilterStrip AllMyFilters_OrCategoryReadOnly => AllMyFiltersCore(true);
			public FilterStrip AllMyFilters_List => AllMyFiltersCore(false, new ArrayList() { "ABCDE", "FGHIJ" });
			public FilterStrip AllMyFilters_TextNk5 => AllMyFiltersCore(false, new ArrayList() { 5 });
			public FilterStrip AllMyFilters_TextNk0 => AllMyFiltersCore(false, new ArrayList() { 0 });

			FilterStrip AllMyFiltersCore(bool isOrCategoryReadOnly, IList list = null)
			{
				var coll = new ModuleFilterCollection();
				ModuleFilter filter;
				if (list != null && filterColumn != null)
				{
					filter = new ModuleFilterForTest("<select something to filter by>", filterColumn, list);
				}
				else if (filterColumn != null)
				{
					filter = new ModuleFilterForTest("<select something to filter by>", filterColumn);
				}
				else if (list != null)
				{
					filter = new ModuleTextAndNkFilter("<select something to filter by>", SomeQueryDelegate, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(new BusinessObjectFactory()));
					if ((int)list[0] > 0)
					{
						filter.MaxLength = (int)list[0];
					}
				}
				else
				{
					filter = new ModuleFilterForTest("<select something to filter by>");
				}

				filter.Visibility = FilterVisibility.Visible;
				filter.Category = new FilterCategory((NoResString)"categer");
				filter.IsActive = true;
				filter.IsOrCategoryReadOnly = isOrCategoryReadOnly;
				coll.AddFilter(filter);

				var strip = new FilterStripForTest(coll);
				return strip;
			}

			ZQuery SomeQueryDelegate(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString nKVessel)
			{
				return new ZQuery();
			}
		}

		class ModuleFilterForTest : ModuleTextFilter
		{
			public ModuleFilterForTest(string desc)
				: base(desc, delegate { return new ZQuery(); })
			{
			}

			public ModuleFilterForTest(string desc, SchemaStringColumn filterColumn)
				: base(desc, filterColumn)
			{
			}

			public ModuleFilterForTest(string desc, SchemaStringColumn filterColumn, IList list)
				: base(desc, filterColumn, list)
			{
			}

			public ZInt PropertyForTest
			{
				get { return fPropertyForTest; }
				set { fPropertyForTest = value; PropertyForTestInfo.RefreshBinding(); }
			}
			public ZInt fPropertyForTest;

			public ZPropertyInfo PropertyForTestInfo
			{
				get { return GetZPropertyInfo(nameof(PropertyForTest)); }
			}

			#region abstract methods

			protected override void ClearCore()
			{
				PropertyForTest = 0;
			}

			protected override bool IsEmptyCore => false;

			protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				return null;
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
			{
			}

			public override bool IsExpensiveQuery
			{
				get { return ZBool.False; }
			}

			protected override FilterCategory DefaultCategory
			{
				get { return null; }
			}

			protected override ModuleFilterValidation GetNewValidation()
			{
				return null;
			}

			protected override object[] QueryDelegateParameters
			{
				get { return null; }
			}

			protected override ZQuery GetQueryUsingFilterColumns()
			{
				return null;
			}

			protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
			{
			}

			protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
			{
			}

			#endregion
		}

		class zFilterStripForTest : ZFilterStrip
		{
			public zFilterStripForTest()
				: base()
			{
			}

			public void TestMethod()
			{
				OnCurrentDataItemChanged(new EventArgs());
			}

			public void OnCurrentDataItemChanging_Exposed()
			{
				OnCurrentDataItemChanging(new EventArgs());
			}

			public int DeleteButtonDefaultLeft_Exposed => DeleteButtonDefaultLeft;

			public int FilterButtonDefaultLeft_Exposed => FilterButtonDefaultLeft;

			public int MaxFilterStripWidth_Exposed => MaxFilterStripWidth;
		}

		class FilterStripForTest : FilterStrip
		{
			public FilterStripForTest(ModuleFilterCollection coll)
				: base(coll)
			{
			}
		}

		#endregion

		#region TestTextFilter

		public void TestTextFilter()
		{
			using (var ctr = new StripControl())
			{
				using (var strip = new zFilterStripForTest())
				{
					ctr.Controls.Add(strip);
					var obj = new MyObject();
					obj.filterColumn = DummyBizoSchema.Z0_Code;
					strip.SetDataBinding(obj, "AllMyFilters");
					strip.CurrentDataItem.FilterDescription = "<select something to filter by>";

					var stripControls = strip.GetTextOrNumberFilterControls();

					try
					{
						AssertEquals(5, ((ZTextBox)(stripControls[1])).MaxLength);
						AssertEquals(true, ((ZTextBox)(stripControls[1])).IsOnFilterStrip);
					}
					finally
					{
						foreach (var c in stripControls)
						{
							c.Dispose();
						}
					}
				}
			}
		}

		#endregion

		#region TestTextWithListFilter

		public void TestTextWithListFilter()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var strip = form.AddFilterStrip("Some Text Filter");
				var obj = new MyObject() { filterColumn = DummyBizoSchema.Z0_Code };
				strip.SetDataBinding(obj, "AllMyFilters_List");
				strip.CurrentDataItem.FilterDescription = "<select something to filter by>";

				try
				{
					var dropEdit = (ZDropEdit)strip.Controls[6];
					AssertEquals(5, dropEdit.MaxLength);
				}
				finally
				{
					foreach (Control c in strip.Controls)
					{
						c.Dispose();
					}
				}
			}
		}

		#endregion

		public void TestComparisonListCodeBoxManualEntry()
		{
			var filter = GetNewTextAndNkFilterForSelectedFiltersTests();
			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var strip = form.AddFilterStrip("AAA");
				filter.IsActive = true;

				var comparisonList = form.GetComparisonList(strip);
				var nextControl = form.GetNextControl(comparisonList, true);
				AssertNotNull(comparisonList);
				AssertNotNull(nextControl);

				AssertEquals("Default should be " + ModuleTextFilter.ComparisonConstants.Default + ".", ModuleTextFilter.ComparisonConstants.Default, comparisonList.Text);

				comparisonList.Text = "FlibbertyGibbit";
				comparisonList.CommitBoundValue();
				AssertEquals("When invalid value entered, comparison operator should not be set to new value, and should remain as previous value.", ModuleTextFilter.ComparisonConstants.Default, comparisonList.Text);

				comparisonList.Text = ModuleTextFilter.ComparisonConstants.Contains;
				comparisonList.CommitBoundValue();
				AssertEquals("When a valid value entered, comparison operator should be set to new value.", ModuleTextFilter.ComparisonConstants.Contains, comparisonList.Text);
			}
		}

		#region TestTextAndNkFilter

		public void TestTextAndNkFilter()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var strip5 = form.AddFilterStrip("Some Text Filter");
				var obj = new MyObject();
				strip5.SetDataBinding(obj, "AllMyFilters_TextNk5");
				strip5.CurrentDataItem.FilterDescription = "<select something to filter by>";
				try
				{
					var dropEdit = (ZTextBox)strip5.Controls[7];
					AssertEquals(5, dropEdit.MaxLength);
				}
				finally
				{
					foreach (Control c in strip5.Controls)
					{
						c.Dispose();
					}
				}

				var strip0 = form.AddFilterStrip("Some Text Filter");
				strip0.SetDataBinding(obj, "AllMyFilters_TextNk0");
				strip0.CurrentDataItem.FilterDescription = "<select something to filter by>";
				try
				{
					var dropEdit = (ZTextBox)strip0.Controls[7];
					AssertEquals(0, dropEdit.MaxLength);
				}
				finally
				{
					foreach (Control c in strip0.Controls)
					{
						c.Dispose();
					}
				}
			}
		}

		#endregion

		#region TestUpdateLayout

		public void TestUpdateLayoutWhenCurrentDataItemIsNull()
		{
			ZFilterStrip filterStrip = null;
			try
			{
				filterStrip = new ZFilterStrip();
				filterStrip.UpdateLayout(ControlDpiScalingHelper.ScaleToCurrentDpiX(450));
				AssertEquals(filterStrip.CurrentDataItem, null);
				Assert(Math.Abs(filterStrip.FilterControlBoxWidth - 142) < 3);
				filterStrip.UpdateLayout(ControlDpiScalingHelper.ScaleToCurrentDpiX(663));
				Assert(Math.Abs(filterStrip.FilterControlBoxWidth - 284) < 3);
			}
			finally
			{
				filterStrip?.Dispose();
			}
		}

		public void TestUpdateLayout_TextFilter()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var filterStrip = form.AddFilterStrip("Text");
				AssertEquals(typeof(ZDropEdit), filterStrip.CurrentFilterControls[0].GetType());

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth), filterStrip.Width);
				AssertEquals(ZFilterStrip.FilterControlBoxMaxWidth, filterStrip.FilterControlBoxWidth);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft), filterStrip.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft), filterStrip.FilterCategoriesToolStrip.Left);

				var newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MinFilterStripWidth - 100);
				filterStrip.UpdateLayout(newWidth);

				const int shift = ZFilterStrip.MaxFilterStripWidth - ZFilterStrip.MinFilterStripWidth;
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MinFilterStripWidth), filterStrip.Width);

				Assert(Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft - shift) - filterStrip.DeleteStripButton.Left) < 3);

				Assert(Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft - shift) - filterStrip.FilterCategoriesToolStrip.Left) < 3);

				Assert(string.Format("{0} should be greater than {1}, but <= 4*(DPI)", filterStrip.CurrentFilterControls[0].Left, filterStrip.FilterDescriptionDropEdit.Right),
					(filterStrip.CurrentFilterControls[0].Left - filterStrip.FilterDescriptionDropEdit.Right) <= ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl));

				newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth - 100);
				filterStrip.UpdateLayout(newWidth);

				AssertEquals(newWidth, filterStrip.Width);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft - 100), filterStrip.DeleteStripButton.Left);
				Assert(Math.Abs((ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft - 100) - filterStrip.FilterCategoriesToolStrip.Left)) < 3);

				Assert(string.Format("{0} should be greater than {1}, but <= 4*(DPI)", filterStrip.CurrentFilterControls[0].Left, filterStrip.FilterDescriptionDropEdit.Right),
					(filterStrip.CurrentFilterControls[0].Left - filterStrip.FilterDescriptionDropEdit.Right) <= ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl));

				newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth + 100);
				filterStrip.UpdateLayout(newWidth);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth), filterStrip.Width);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft), filterStrip.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft), filterStrip.FilterCategoriesToolStrip.Left);

				Assert(string.Format("{0} should be greater than {1}, but <= 4*(DPI)", filterStrip.CurrentFilterControls[0].Left, filterStrip.FilterDescriptionDropEdit.Right),
					(filterStrip.CurrentFilterControls[0].Left - filterStrip.FilterDescriptionDropEdit.Right) <= ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl));

				newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MinFilterStripWidth - 100);
				filterStrip.UpdateLayout(newWidth);
			}
		}

		public void TestUpdateLayout()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var filterStrip = form.AddFilterStrip("Text Range");

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth), filterStrip.Width);
				AssertEquals(ZFilterStrip.FilterControlBoxMaxWidth, filterStrip.FilterControlBoxWidth);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft), filterStrip.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft), filterStrip.FilterCategoriesToolStrip.Left);

				var newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth - 100);
				filterStrip.UpdateLayout(newWidth);

				AssertEquals(newWidth, filterStrip.Width);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft - 100), filterStrip.DeleteStripButton.Left);
				Assert(Math.Abs((ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft - 100) - filterStrip.FilterCategoriesToolStrip.Left)) < 3);

				newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth + 100);
				filterStrip.UpdateLayout(newWidth);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MaxFilterStripWidth), filterStrip.Width);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft), filterStrip.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft), filterStrip.FilterCategoriesToolStrip.Left);

				newWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MinFilterStripWidth - 100);
				filterStrip.UpdateLayout(newWidth);

				const int shift = ZFilterStrip.MaxFilterStripWidth - ZFilterStrip.MinFilterStripWidth;
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.MinFilterStripWidth), filterStrip.Width);

				Assert(Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.DeleteButtonDefaultLeft - shift) - filterStrip.DeleteStripButton.Left) < 3);

				Assert(Math.Abs(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.FilterButtonDefaultLeft - shift) - filterStrip.FilterCategoriesToolStrip.Left) < 3);
			}
		}

		#endregion

		#region ModuleDateFilter

		public void TestDateDayOffsetRangeControls()
		{
			var filterBizo = new MockFilterStripBizO();

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var filterStrip = form.AddFilterStrip("Date Range");
				var dateRangeControl = filterStrip.Controls.OfType<ZDateRangeControl>().FirstOrDefault();
				var filter = (ModuleDateFilter)dateRangeControl.CurrentDataItem;
				filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
				Application.DoEvents();

				AssertDateDayOffsetRangeControls(form, filterStrip, dateRangeControl);

				filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
				Application.DoEvents();

				AssertDateHourOffsetRangeControls(form, filterStrip, dateRangeControl);
			}
		}

		public void TestDateHourOffsetRangeControls()
		{
			var filterBizo = new MockFilterStripBizO();

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();

				var filterStrip = form.AddFilterStrip("Date Range");
				var dateRangeControl = filterStrip.Controls.OfType<ZDateRangeControl>().FirstOrDefault();
				var filter = (ModuleDateFilter)dateRangeControl.CurrentDataItem;
				filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
				Application.DoEvents();

				AssertDateHourOffsetRangeControls(form, filterStrip, dateRangeControl);

				filter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
				Application.DoEvents();

				AssertDateDayOffsetRangeControls(form, filterStrip, dateRangeControl);
			}
		}

		static void AssertDateHourOffsetRangeControls(Form form, ZFilterStrip filterStrip, Control dateRangeControl)
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(96, 96))
			{
				AssertEquals("DateRangeControl tabindex should be 1.", 1, dateRangeControl.TabIndex);

				var pastDropEdit = dateRangeControl.Controls.OfType<ZDropEdit>().FirstOrDefault(x => x.Text == "Past");
				var removeButton = filterStrip.DeleteStripButton;
				Application.DoEvents();

				var pastDropEditRight = ControlTestHelper.GetControlAbsoluteRight(pastDropEdit, form);
				var removeButtonLeft = ControlTestHelper.GetControlAbsoluteLeft(removeButton, form);

				AssertEquals(true, pastDropEditRight < removeButtonLeft);
				AssertEquals("In the", pastDropEdit.CaptionResourceString.Caption);
			}
		}

		static void AssertDateDayOffsetRangeControls(Form form, ZFilterStrip filterStrip, Control dateRangeControl)
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(96, 96))
			{
				AssertEquals("DateRangeControl tabindex should be 1.", 1, dateRangeControl.TabIndex);

				var visibleControls = dateRangeControl.FindAll<Control>(x => x.Visible).ToArray();
				var calcEdits = visibleControls.OfType<ZCalcEdit>().ToArray();
				AssertEquals(2, calcEdits.Length);

				foreach (var calcEdit in calcEdits)
				{
					AssertEquals(2, calcEdit.DecimalPlaces);
					AssertEquals(9999m, calcEdit.MaxValue);
				}

				var timeEdits = visibleControls.OfType<ZTimeEdit>();
				AssertEquals(0, timeEdits.Count());

				var pastDropEdit = dateRangeControl.FindSingle<ZDropEdit>(x => x.Text == "Past");
				var removeButton = filterStrip.DeleteStripButton;
				var pastDropEditRight = ControlTestHelper.GetControlAbsoluteRight(pastDropEdit, form);
				var removeButtonLeft = ControlTestHelper.GetControlAbsoluteLeft(removeButton, form);

				AssertEquals(true, pastDropEditRight < removeButtonLeft);
				AssertEquals(true, pastDropEdit.Visible);
				AssertEquals("Days in the", pastDropEdit.CaptionResourceString.Caption);
			}
		}

		#endregion

		#region SingleDateFilter

		public void TestSingleDateFilter_DateTimeFormat()
		{
			var shortDateFormatBizO = new MockFilterStripBizO();
			using (var mockFormShort = new MockFilterStripForm(shortDateFormatBizO))
			{
				mockFormShort.Show();
				var zFilterStrip = mockFormShort.AddFilterStrip("Single Date");
				var filterStrip = (FilterStrip)shortDateFormatBizO.FilterStrips.Last();
				var filter = (ModuleSingleDateFilter)filterStrip.CurrentModuleFilter;
				AssertEquals("Default DateTimeFormat is Short", ZDateTimePickerFormat.Short, filter.DateTimeFormat);

				var dateControl = (ZSingleDateControl)zFilterStrip.Controls.Find("ZSingleDateControl", false)[0];
				var dateEdit = (ZFilterStripDateEdit)dateControl.Controls.Find("DateEdit", false)[0];
				AssertEquals("Default DateTimeFormat is Short", ZDateTimePickerFormat.Short, dateEdit.DateTimeFormat);
			}

			var longDateFormatBizO = new MockFilterStripBizO();
			((ModuleSingleDateFilter)longDateFormatBizO.ModuleFilters["Single Date"]).DateTimeFormat = ZDateTimePickerFormat.Long;
			using (var mockFormLong = new MockFilterStripForm(longDateFormatBizO))
			{
				mockFormLong.Show();
				var zFilterStrip = mockFormLong.AddFilterStrip("Single Date");
				var filterStrip = (FilterStrip)longDateFormatBizO.FilterStrips.Last();
				var filter = (ModuleSingleDateFilter)filterStrip.CurrentModuleFilter;
				AssertEquals("Has DateTimeFormat Long", ZDateTimePickerFormat.Long, filter.DateTimeFormat);

				var dateControl = (ZSingleDateControl)zFilterStrip.Controls.Find("ZSingleDateControl", false)[0];
				var dateEdit = (ZFilterStripDateEdit)dateControl.Controls.Find("DateEdit", false)[0];
				AssertEquals("Has DateTimeFormat Long", ZDateTimePickerFormat.Long, dateEdit.DateTimeFormat);
			}
		}

		#endregion

		#region Custom SQL Filter

		public void TestCustomSqlFilter_WhenReadOnly_ShouldAllowExpansion_ButNotEdit()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var tagDef = helper.CreateTagDefinition(Factory, "TRA");
			var tagMag = helper.CreateTagMagnitude(tagDef, "LAL");
			var tagRule = Factory.New<ITagRule>();
			tagRule.TagRuleTemplate.TGL_TGM_Magnitude = tagMag.PK;

			var tagRuleBizo = (BusinessObject)tagRule;
			tagRuleBizo.FillWithValidTestData();
			var filter = new FilterRuleProvider(ModuleIDs.BMFilterRule, (IRelatedModuleFilterSupportable)tagRuleBizo).GetOrCreateAndCacheFilter();
			FilterStripsTestHelper.AddFilterStrip<ModuleSQLFilter>(filter, FilterStripBusinessObject.CustomSqlFilterDescription);
			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(tagRuleBizo);

			using (var form = (Form)controller.ShowEditForm(tagRuleBizo))
			{
				AssertCustomSqlStripState(form, true);
			}

			using (var form = (Form)controller.ShowViewForm(tagRuleBizo))
			{
				AssertCustomSqlStripState(form, false);
			}
		}

		static void AssertCustomSqlStripState(Form form, bool shouldBeEnabled)
		{
			form.Show();
			Application.DoEvents();

			var stripControl = form.FindSingle<StripControl>();
			var strip = stripControl.FindSingle<ZFilterStrip>();
			var textBox = strip.FindSingle<ZTextBox>(x => x.CaptionResourceString.Caption == "SQL Text");

			AssertEquals(true, textBox.IsDynamicMultiline);
			AssertEquals("The ReadOnly property of the textbox should be changed in view mode, not the Enabled property because Enabled false makes the control un-expandable, and yet...", true, textBox.Enabled);
			AssertEquals(!shouldBeEnabled, textBox.ReadOnly);
		}

		#endregion

		#region IsOrCategoryReadOnly

		public void TestFiltersWithIsOrCategoryReadOnly()
		{
			var filters1 = GetNewGuidFilterForSelectedFiltersTests("AAA");
			var filters2 = GetNewGuidFilterForSelectedFiltersTests("BBB");
			filters2.IsOrCategoryReadOnly = true;

			filters1.SelectedFilters.AddTextFilterStrip("Z0_Description", "Keokuk");

			var filterBizo = new MockFilterStripBizO();
			filterBizo.ModuleFilters.AddCustomFilter(filters1);
			filterBizo.ModuleFilters.AddCustomFilter(filters2);

			using (var form = new MockFilterStripForm(filterBizo) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			{
				form.Show();
				Application.DoEvents();

				var strip1 = form.AddFilterStrip("AAA");
				var strip2 = form.AddFilterStrip("BBB");
				AssertEquals(true, strip1.FilterCategoriesToolStripDropDown.Visible);
				AssertEquals(false, strip2.FilterCategoriesToolStripDropDown.Visible);
			}
		}

		#endregion

		public void TestConstants()
		{
			using (var strip = new zFilterStripForTest())
			{
				var deleteButtonLeft = ControlDpiScalingHelper.NewScaledPoint(strip.DeleteButtonDefaultLeft_Exposed, 1, true);
				AssertEquals(deleteButtonLeft.X, strip.DeleteStripButton.Location.X);

				var filterButtonLeft = ControlDpiScalingHelper.NewScaledPoint(strip.FilterButtonDefaultLeft_Exposed, 1, true);
				AssertEquals(filterButtonLeft.X, strip.FilterCategoriesToolStrip.Location.X);

				var maxFilterStripWidth = ControlDpiScalingHelper.NewScaledSize(strip.MaxFilterStripWidth_Exposed, 23, true);
				AssertEquals(maxFilterStripWidth.Width, strip.Size.Width);

				var filterDescriptionLocation = strip.FilterDescriptionDropEdit.Location;
				strip.SetFilterDescriptionDropEditLocation();
				AssertEquals(filterDescriptionLocation, strip.FilterDescriptionDropEdit.Location);
			}
		}

		public void TestFilterStripSizeConstant()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			using (var dateEdit = new ZFilterStripDateEdit())
			using (var textBox = new ZTextBox())
			{
				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				AssertEquals(textBox.Height, filterStrip.FilterControlBoxHeight);
				AssertEquals(dateEdit.Width, filterStrip.FilterControlBoxWidthSmall);
			}
		}
	}
}
