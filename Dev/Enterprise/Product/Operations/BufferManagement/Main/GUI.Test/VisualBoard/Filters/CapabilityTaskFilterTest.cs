using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region BoardFilterTestCase

	[TestedType(typeof(CapabilityTaskFilter))]
	class CapabilityTaskFilterTest : BoardFilterTestCase<CapabilityTaskFilter>
	{
		public override void TestFilterName()
		{
			AssertEquals("Showing All Tasks", new CapabilityTaskFilter().FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(true, new CapabilityTaskFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals("Any instance should equal another", new CapabilityTaskFilter(), new CapabilityTaskFilter());
		}

		protected override CapabilityTaskFilter GetFilter()
		{
			return new CapabilityTaskFilter();
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var system = CreateSystem();
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (DisableAsyncBehaviour())
			using (var form = new ZForm { Width = 800, Height = 800 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var capabilityTaskMenuItem = (ZToolStripMenuItem)control.ContextMenuStrip.Items[2];
				AssertEquals("Capability Tasks", capabilityTaskMenuItem.Text);

				capabilityTaskMenuItem.ShowDropDown();

				AssertEquals("Exclude Capability Tasks", capabilityTaskMenuItem.DropDownItems[0].Text);
				AssertEquals("Show Only Capability Tasks", capabilityTaskMenuItem.DropDownItems[1].Text);

				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 1, false);

				capabilityTaskMenuItem.PerformClick();
				capabilityTaskMenuItem.ShowDropDown();

				capabilityTaskMenuItem.DropDownItems[0].PerformClick();
				capabilityTaskMenuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, true);
				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 1, false);

				AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(CapabilityTaskFilter)));

				var capabilityFilter = viewModel.FilterManager.AppliedAndChildFilters.OfType<CapabilityTaskFilter>().Single();
				AssertEquals("Excluding Capability Tasks", capabilityFilter.FilterName);

				capabilityTaskMenuItem.DropDownItems[1].PerformClick();
				capabilityTaskMenuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 1, true);

				AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(CapabilityTaskFilter)));
				AssertEquals("Showing Capability Tasks", capabilityFilter.FilterName);

				capabilityTaskMenuItem.DropDownItems[1].PerformClick();
				capabilityTaskMenuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 1, false);

				AssertEquals(false, viewModel.FilterManager.IsApplied(typeof(CapabilityTaskFilter)));

				capabilityTaskMenuItem.DropDownItems[1].PerformClick();
				capabilityTaskMenuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 1, true);

				capabilityTaskMenuItem.HideDropDown();
				viewModel.FilterManager.Clear();
				capabilityTaskMenuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 1, false);
			}
		}

		public void TestWhenRefreshFrom_RestoreVisualState()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item2;

			var capabilityTaskMenuItem = new CapabilityTaskFilterMenuItemForTest(viewModel);
			capabilityTaskMenuItem.ShowDropDown();
			BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItem, 0, false);

			var filter = new CapabilityTaskFilter { CurrentlySelectedOption = TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks };
			viewModel.FilterManager.ApplyFilter(filter);

			var capabilityTaskMenuItemFiltered = new CapabilityTaskFilterMenuItemForTest(viewModel);
			capabilityTaskMenuItemFiltered.ShowDropDown();
			AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(CapabilityTaskFilter)));

			var filterInMenuItem = new CapabilityTaskFilterMenuItemForTest(viewModel).GetFilter();
			AssertEquals("When create new menu, it gets the current filter from FilterManager.", true, ReferenceEquals(filterInMenuItem, filter));
			AssertEquals(TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks, filterInMenuItem.CurrentlySelectedOption);
			BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItemFiltered, 0, true);
			capabilityTaskMenuItemFiltered.HideDropDown();

			viewModel.FilterManager.Clear();
			capabilityTaskMenuItemFiltered.ShowDropDown();
			BMSGUITestCase.AssertSubMenuItemChecked(capabilityTaskMenuItemFiltered, 0, false);
			capabilityTaskMenuItemFiltered.HideDropDown();

			var newCapabilityTaskMenuItem = new CapabilityTaskFilterMenuItemForTest(viewModel);
			AssertNotEquals("Should create new filter when filter is not exists in FilterManager.", true, ReferenceEquals(newCapabilityTaskMenuItem.GetFilter(), filter));
		}
	}

	#endregion

	#region FilterApplicatorTestCase

	[TestedType(typeof(CapabilityTaskFilter))]
	class CapabilityTaskFilterApplicatorTest : TaskVisibilityFilterApplicatorTestCase<CapabilityTaskFilter>
	{
		public void TestApplyFilter_WhenCcrChannelExists_ShouldNotClearCcrIcon()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section.PK);

				void AssertChannelHeadingCcrIconsShown()
				{
					var channelHeading1 = control.FindSingle<ChannelHeaderControl>(c => c.Channel.EntityPK == config.CCR.PK);
					var channelHeading2 = control.FindSingle<ChannelHeaderControl>(c => c.Channel.EntityPK == config.NonCCR1.PK);

					AssertNotNull(channelHeading1.CcrPictureBox.Image);
					AssertNull(channelHeading2.CcrPictureBox.Image);
				}

				AssertChannelHeadingCcrIconsShown();

				var menuItem = control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Capability Tasks");

				menuItem.ShowDropDown();
				menuItem.DropDownItems[0].PerformClick();

				AssertChannelHeadingCcrIconsShown();
			}
		}

		public override void TestApply_DbHits()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			CreateTask(CreateWorkflow(jobHeader, "workflow1"), "", 15, capability: capability);
			CreateTask(CreateWorkflow(jobHeader, "workflow2"), "", 15, capability: capability);
			CreateTask(CreateWorkflow(jobHeader, "workflow3"), "", 15);
			CreateTask(CreateWorkflow(jobHeader, "workflow4"), "", 15);

			var cells = new[]
			{
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
			};

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			var applicator = GetFilter();
			foreach (var group in cells.Zip(loadedJobHeader.ProcessHeaders, (c, w) => new { Workflow = w, Cell = c, Task = w.GetTasksWithoutAccessingWorkflowParent().Single() }))
			{
				applicator.IsApplicable(group.Task, group.Cell, null);
			}

			var moreDbHitsAllowed = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
			};

			AssertDbHits(moreDbHitsAllowed, newFactory);
		}

		public override void TestIsApplicable()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";

			var task1 = workflow.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow.PK;
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_G4_RequiredCapability = ZGuid.Empty;

			var task2 = workflow.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task2.P9_G4_RequiredCapability = capability.PK;

			var filter = GetFilter();
			filter.TaskOption = TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks;
			AssertEquals(false, filter.IsApplicable(task1, new CellContent(0, 0, CellContentType.Cards), null));
			AssertEquals(true, filter.IsApplicable(task2, new CellContent(0, 0, CellContentType.Cards), null));

			filter.TaskOption = TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks;
			AssertEquals(true, filter.IsApplicable(task1, new CellContent(0, 0, CellContentType.Cards), null));
			AssertEquals(false, filter.IsApplicable(task2, new CellContent(0, 0, CellContentType.Cards), null));
		}

		protected override CapabilityTaskFilter GetFilter()
		{
			return new CapabilityTaskFilter();
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield return new KeyValuePair<string, int>(ProcessTasksSchema.Constants.TableName, 1);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}

	#endregion

	#region CapabilityTaskFilterMenuItemForTest

	class CapabilityTaskFilterMenuItemForTest : CapabilityTaskFilterMenuItem
	{
		public CapabilityTaskFilterMenuItemForTest(BMBoardSectionViewModel sectionViewModel)
			: base(sectionViewModel)
		{ }

		public CapabilityTaskFilter GetFilter()
		{
			return (CapabilityTaskFilter)Filter;
		}
	}

	#endregion
}
