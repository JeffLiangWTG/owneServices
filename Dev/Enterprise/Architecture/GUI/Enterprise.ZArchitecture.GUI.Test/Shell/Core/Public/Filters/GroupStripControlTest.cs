using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GroupStripControlTest : TestCaseWithFactory
	{
		public void TestIsAddButtonEnabled()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var groupControl = form.AddGroupStripControl("Content Management");
				groupControl.IsAddButtonEnabled = false;
				AssertEquals(false, groupControl.IsAddButtonEnabled);
				AssertEquals(false, groupControl.AddStripButton.Enabled);
				AssertEquals(Icons.GetImage(IconTypes.AddButtonDisabled), groupControl.AddStripButton.BackgroundImage);

				groupControl.IsAddButtonEnabled = true;
				AssertEquals(true, groupControl.IsAddButtonEnabled);
				AssertEquals(true, groupControl.AddStripButton.Enabled);
				AssertEquals(Icons.GetImage(IconTypes.AddButtonRest), groupControl.AddStripButton.BackgroundImage);
			}
		}
		public void TestIsDeleteButtonEnabled()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var groupControl = form.AddGroupStripControl("Content Management");
				groupControl.IsDeleteButtonEnabled = false;
				AssertEquals(false, groupControl.IsDeleteButtonEnabled);
				AssertEquals(false, groupControl.DeleteStripButton.Enabled);
				AssertEquals(Icons.GetImage(IconTypes.MinusButtonDisabled), groupControl.DeleteStripButton.BackgroundImage);

				groupControl.IsDeleteButtonEnabled = true;
				AssertEquals(true, groupControl.IsDeleteButtonEnabled);
				AssertEquals(true, groupControl.DeleteStripButton.Enabled);
				AssertEquals(Icons.GetImage(IconTypes.MinusButtonRest), groupControl.DeleteStripButton.BackgroundImage);
			}
		}

		public void TestIsFilterCategoriesToolStripDropDownVisible()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var groupControl = form.AddGroupStripControl("Content Management");
				groupControl.IsFilterCategoriesToolStripDropDownVisible = false;
				AssertEquals(false, groupControl.IsFilterCategoriesToolStripDropDownVisible);
				AssertEquals(false, groupControl.FilterCategoriesToolStripDropDown.Visible);

				groupControl.IsFilterCategoriesToolStripDropDownVisible = true;
				AssertEquals(true, groupControl.IsFilterCategoriesToolStripDropDownVisible);
				AssertEquals(true, groupControl.FilterCategoriesToolStripDropDown.Visible);
			}
		}

		public void TestNumberRangeControls()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var groupControl = form.AddGroupStripControl("Content Dev");
				AssertEquals("groupControl.Controls.Count", 5, groupControl.Controls.Count);

				var filterStrip = form.AddFilterStrip("Number Range (Decimal)", groupControl);
				AssertEquals("groupControl.Controls.Count", 5, groupControl.Controls.Count);
			}
		}

		public void TestDragDropEvent()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var panel = form.FilterStripControl.Controls.Find("FilterStripsPanel", false)[0];

				var groupControl1 = form.AddGroupStripControl("First");
				var groupControl2 = form.AddGroupStripControl("Second");

				var filterStrip1 = form.AddFilterStrip("Number Range (Decimal)", groupControl1);
				var filterStrip2 = form.AddFilterStrip("Stage", groupControl2);

				AssertEquals("panel.Controls.Count", 3, panel.Controls.Count);

				var dataObj = new DataObject();
				dataObj.SetData(filterStrip2);

				var drgevent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);
				groupControl1.FilterStripGroupBox_DragDrop(groupControl1.FilterStripGroupBox, drgevent);

				AssertEquals(filterStrip2.CurrentDataItem.GroupName, filterStrip1.CurrentDataItem.GroupName);
				AssertEquals(filterStrip2.CurrentDataItem.GroupOrCategory, filterStrip1.CurrentDataItem.GroupOrCategory);
				AssertEquals(filterStrip2.CurrentDataItem.AdditionalGroupColourName, filterStrip1.CurrentDataItem.AdditionalGroupColourName);

				AssertEquals("panel.Controls.Count", 3, panel.Controls.Count);
			}
		}

		public void TestOnLoaded()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();

				var groupControl = form.AddGroupStripControl("");
				AssertEquals("FilterStripGroupBox.Top = 0", 0, groupControl.FilterStripGroupBox.Top);
				AssertEquals("GroupNameLabel.Visible = false", false, groupControl.GroupNameLabel.Visible);
			}
		}

		#region MockFilterStripBizO

		class MockFilterStripBizO : FilterStripBusinessObject
		{
			DummyBusinessObjectCollection dummies;
			public List<FilterStrip> FilterStripRecords { get; set; }

			public MockFilterStripBizO()
			{
				FilterStripRecords = new List<FilterStrip>();
			}

			public DummyBusinessObjectCollection Dummies
			{
				get { return dummies ?? (dummies = new DummyBusinessObjectCollection(Factory)); }
			}

			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var result = new ModuleFilterCollection();
				result.AddNumberRangeFilter("Number Range (Decimal)", DummyBizoSchema.Z0_Decimal);
				result.AddNumberRangeFilter("Number Range (Short)", DummyBizoSchema.Z0_Short);
				result.AddTextRangeFilter("Text Range", DummyBizoSchema.Z0_Description);

				GetFlagsQuery filter = delegate
				{ return new ZQuery(); };
				result.AddFlagsFilter("Flags Filter",
					new string[] { "Flag1", "Flag2", "Flag3", "Flag4", "Flag5", "Flag6", "Flag7", "Flag8", "Flag9", "Flag10" },
					new GetFlagsQuery[] { filter, filter, filter, filter, filter, filter, filter, filter, filter, filter });

				return result;
			}
		}

		#endregion

		#region MockFilterStripControl

		class MockFilterStripControl : ZFilterStripControl
		{
			public MockFilterStripControl(DummyBusinessObjectCollection dummies, MockFilterStripBizO dataSource) : base(dummies, dataSource)
			{
			}

			protected override void FilterEdited()
			{
				base.FilterEdited();
				SaveFilterState();
			}

			public void SaveFilterState()
			{
				var records = ((MockFilterStripBizO)FilterBusinessObject).FilterStripRecords;
				records.Clear();
				foreach (FilterStrip filterStrip in ((MockFilterStripForm)ParentForm).DataSource.FilterStrips)
				{
					if (!GroupExists(filterStrip.GroupName))
					{
						continue; // deleted groups
					}

					var filterStripRecord = new FilterStrip(filterStrip.ModuleFilters)
					{
						GroupName = filterStrip.GroupName,
						FilterDescription = filterStrip.FilterDescription,
						GroupOrCategory = filterStrip.GroupOrCategory,
						OrCategory = filterStrip.OrCategory
					};
					records.Add(filterStripRecord);
				}
			}
		}

		#endregion

		#region MockFilterStripForm

		class MockFilterStripForm : ZForm
		{
			readonly MockFilterStripControl filterStripControl;

			public MockFilterStripForm(MockFilterStripBizO businessEntity)
				: base(businessEntity)
			{
				filterStripControl = new MockFilterStripControl(DataSource.Dummies, DataSource);
				var columnStyleInfo = new ZTextBoxColumnStyleInfo();
				columnStyleInfo.ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax;
				filterStripControl.FilteredGrid.BindTo = "Dummies";
				filterStripControl.FilteredGrid.ColumnStyles.Add(columnStyleInfo);
				Controls.Add(filterStripControl);
			}

			public void LoadFilterStripsFromBizO()
			{
				DataSource.FilterStrips.RemoveAll();
				foreach (var bizO in ((MockFilterStripBizO)BusinessEntity).FilterStripRecords.ToArray())
				{
					if (GetGroupStripControl(bizO.GroupName) == null)
					{
						AddGroupStripControl(bizO.GroupName);
					}
					AddFilterStrip(bizO);
				}
			}

			public new MockFilterStripBizO DataSource
			{
				get { return (MockFilterStripBizO)base.DataSource; }
			}

			public MockFilterStripControl FilterStripControl
			{
				get { return filterStripControl; }
			}

			void AddFilterStrip(FilterStrip filterStripBizO)
			{
				DataSource.FilterStrips.Add(filterStripBizO);
				FilterStripControl.AddFilterStrip(filterStripBizO);
			}

			public ZFilterStrip AddFilterStrip(string filterDescription, GroupStripControl group)
			{
				var filterStripBizO = DataSource.FilterStrips.AddNew();
				filterStripBizO.FilterDescription = filterDescription;
				filterStripBizO.GroupName = group.GroupName;
				FilterStripControl.AddFilterStrip(filterStripBizO);
				return (ZFilterStrip)group.Controls[3].Controls[group.Controls[3].Controls.Count - 1];
			}

			public GroupStripControl AddGroupStripControl(string groupName)
			{
				FilterStripControl.AddGroupFilterControl(groupName);
				return GetGroupStripControl(groupName);
			}

			public GroupStripControl GetGroupStripControl(string groupName)
			{
				return GroupStripControls.FirstOrDefault(groupStripControl => groupStripControl.GroupName == groupName);
			}

			IEnumerable<GroupStripControl> GroupStripControls
			{
				get { return FilterStripsPanel.Controls.OfType<GroupStripControl>(); }
			}

			Control FilterStripsPanel
			{
				get { return FilterStripControl.Controls.Find("FilterStripsPanel", false)[0]; }
			}

			public override ContinueWithSave FireSaveButton(object sender = null)
			{
				FilterStripControl.SaveFilterState();
				return base.FireSaveButton(sender);
			}
		}

		#endregion

		#region TestUpdateLayout

		public void TestUpdateLayout()
		{
			using (var form = new MockFilterStripForm(new MockFilterStripBizO()))
			{
				form.Show();
				var groupControl = form.AddGroupStripControl("New Group");

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.DeleteButtonDefaultLeft), groupControl.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.AddButtonDefaultLeft), groupControl.AddStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.FilterButtonDefaultLeft), groupControl.FilterCategoriesToolStrip.Left);

				groupControl.UpdateLayout(GroupStripControl.MaxFilterStripWidth - 100);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.DeleteButtonDefaultLeft - 100), groupControl.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.AddButtonDefaultLeft - 100), groupControl.AddStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.FilterButtonDefaultLeft - 100), groupControl.FilterCategoriesToolStrip.Left);

				groupControl.UpdateLayout(GroupStripControl.MaxFilterStripWidth + 100);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.DeleteButtonDefaultLeft), groupControl.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.AddButtonDefaultLeft), groupControl.AddStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.FilterButtonDefaultLeft), groupControl.FilterCategoriesToolStrip.Left);

				groupControl.UpdateLayout(GroupStripControl.MinFilterStripWidth - 100);

				var shift = ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.MaxFilterStripWidth - GroupStripControl.MinFilterStripWidth);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.DeleteButtonDefaultLeft) - shift, groupControl.DeleteStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.AddButtonDefaultLeft) - shift, groupControl.AddStripButton.Left);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(GroupStripControl.FilterButtonDefaultLeft) - shift, groupControl.FilterCategoriesToolStrip.Left);
			}
		}

		#endregion

		#region TestSaveChanges

		public void TestSaveChanges()
		{
			var bizO = new MockFilterStripBizO();
			const string groupName1 = "First", groupName2 = "Second", groupName3 = "Third";
			bizO.Factory.ShouldPerformFullQueryCacheCleanOnSave = true;
			FilterOrCategory groupOrCategory;

			using (var form = new MockFilterStripForm(bizO))
			{
				var groupControl1 = form.AddGroupStripControl(groupName1);
				form.AddFilterStrip("Number Range (Decimal)", groupControl1);
				form.AddFilterStrip("Number Range (Short)", groupControl1);

				var groupControl2 = form.AddGroupStripControl(groupName2);
				form.AddFilterStrip("Number Range (Decimal)", groupControl2);
				form.AddFilterStrip("Number Range (Short)", groupControl2);

				var groupControl3 = form.AddGroupStripControl(groupName3);
				form.AddFilterStrip("Number Range (Decimal)", groupControl3);
				form.AddFilterStrip("Number Range (Short)", groupControl3);

				form.FireSaveButton();
				AssertEquals("There should be 6 filter strip records saved.", 6, bizO.FilterStripRecords.Count);

				groupOrCategory = groupControl1.GroupOrCategory;
				AssertEquals("The first group's or category should be 'none' before it is changed.", FilterOrCategory.None, groupOrCategory);

				var dropDown = (ZFilterStripToolStripDropDownButton)groupControl1.FilterCategoriesToolStrip.Items[0];
				dropDown.DropDownItems[3].PerformClick();

				groupOrCategory = groupControl1.GroupOrCategory;
				AssertNotEquals("The first group's or category should not be 'none' after it has been changed.", FilterOrCategory.None, groupOrCategory);
			}

			using (var form = new MockFilterStripForm(bizO))
			{
				form.LoadFilterStripsFromBizO();
				var groupControl1 = form.GetGroupStripControl(groupName1);
				AssertEquals("Initialy, there should have 3 filter strips for first group.", 3, groupControl1.Controls[3].Controls.Count);
				groupControl1.AddStripButton.PerformClick();
				AssertEquals("After perform click on add strip button, there should have 4 filter stips for first gorup.", 4, groupControl1.Controls[3].Controls.Count);

				var groupControl2 = form.GetGroupStripControl(groupName2);
				AssertEquals("Initialy, there should have 3 filter strips for second group.", 3, groupControl2.Controls[3].Controls.Count);
				groupControl2.AddStripButton.PerformClick();
				AssertEquals("After perform click on add strip button, there should have 4 filter stips for second gorup.", 4, groupControl2.Controls[3].Controls.Count);

				var groupControl3 = form.GetGroupStripControl(groupName3);
				AssertEquals("Initialy, there should have 3 filter strips for third group.", 3, groupControl3.Controls[3].Controls.Count);
				groupControl3.AddStripButton.PerformClick();
				AssertEquals("After perform click on add strip button, there should have 4 filter stips for third gorup.", 4, groupControl3.Controls[3].Controls.Count);
				groupControl3.AddStripButton.PerformClick();
				AssertEquals("After perform click on add strip button again, there should have 5 filter stips for third gorup.", 5, groupControl3.Controls[3].Controls.Count);

				form.FireSaveButton();
			}

			using (var form = new MockFilterStripForm(bizO))
			{
				form.LoadFilterStripsFromBizO();
				var groupControl1 = form.GetGroupStripControl(groupName1);
				var groupControl2 = form.GetGroupStripControl(groupName2);
				AssertEquals("The first group's or category should still be the changed colour when reloaded.", groupOrCategory, groupControl1.OrCategory);

				groupControl2.DeleteStripButton.PerformClick();
				AssertEquals("There should be 7 filter strip records saved after the second group was deleted.", 7, bizO.FilterStripRecords.Count);
			}

			using (var form = new MockFilterStripForm(bizO))
			{
				form.LoadFilterStripsFromBizO();
				Assert("The second group control should have been deleted.", !form.FilterStripControl.GroupExists(groupName2));
				Assert("The first group should still exist on the form.", form.FilterStripControl.GroupExists(groupName1));
				Assert("The third group should still exist on the form.", form.FilterStripControl.GroupExists(groupName3));

				var groupControl1 = form.GetGroupStripControl(groupName1);
				groupControl1.DeleteStripButton.PerformClick();
			}

			using (var form = new MockFilterStripForm(bizO))
			{
				form.LoadFilterStripsFromBizO();
				Assert("The first group control should have been deleted.", !form.FilterStripControl.GroupExists(groupName1));
				Assert("The third group control should have been automatically removed because it was the last group.", !form.FilterStripControl.GroupExists(groupName3));
			}
		}

		#endregion
	}
}
