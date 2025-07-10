using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region BoardFilterTestCase

	[TestedType(typeof(CurrentTaskFilter))]
	class CurrentTaskFilterTest : BoardFilterTestCase<CurrentTaskFilter>
	{
		public override void TestFilterName()
		{
			AssertEquals("Show only startable tasks", new CurrentTaskFilter().FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, new CurrentTaskFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals("Any instance should equal another", new CurrentTaskFilter(), new CurrentTaskFilter());
		}

		protected override CurrentTaskFilter GetFilter()
		{
			return new CurrentTaskFilter();
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

				var currentTaskMenuItem = (ZToolStripMenuItem)control.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
				AssertEquals(false, currentTaskMenuItem.Checked);
				AssertNull(currentTaskMenuItem.Image);

				currentTaskMenuItem.PerformClick();
				AssertEquals(true, currentTaskMenuItem.Checked);
				AssertEquals(new Size(16, 16), currentTaskMenuItem.Image.Size);
				AssertImagesAreEqual(new Bitmap(Properties.Resources.tick), new Bitmap(currentTaskMenuItem.Image));

				viewModel.FilterManager.Clear();
				AssertEquals(false, currentTaskMenuItem.Checked);
				AssertNull(currentTaskMenuItem.Image);
			}
		}

		public void TestWhenRefreshFrom_RestoreVisualState()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item2;

			var currentTaskMenuItem = new CurrentTaskFilterMenuItemTest(viewModel);
			AssertEquals(false, currentTaskMenuItem.Checked);
			AssertNull(currentTaskMenuItem.Image);

			var filter = currentTaskMenuItem.GetFilter();
			viewModel.FilterManager.ApplyFilter(filter);
			currentTaskMenuItem.UpdateAppliedVisualState(); // Manually fire update visual state (in onclick we add to filter and update the state)

			AssertEquals(true, currentTaskMenuItem.Checked);
			AssertEquals(new Size(16, 16), currentTaskMenuItem.Image.Size);
			AssertImagesAreEqual(new Bitmap(Properties.Resources.tick), new Bitmap(currentTaskMenuItem.Image));

			currentTaskMenuItem = new CurrentTaskFilterMenuItemTest(viewModel);
			AssertEquals("When create new menu, it gets the current filter from FilterManager.", true, ReferenceEquals(currentTaskMenuItem.GetFilter(), filter));

			viewModel.FilterManager.Clear();
			AssertEquals(false, currentTaskMenuItem.Checked);
			AssertNull(currentTaskMenuItem.Image);

			currentTaskMenuItem = new CurrentTaskFilterMenuItemTest(viewModel);
			AssertNotEquals("Should create new filter when filter is not exists in FilterManager.", true, ReferenceEquals(currentTaskMenuItem.GetFilter(), filter));
		}

		void AssertImagesAreEqual(Bitmap expected, Bitmap actual)
		{
			var expectedPixels = GetPixels(expected);
			var actualPixels = GetPixels(actual);

			AssertEquals(expectedPixels, actualPixels);
		}

		byte[] GetPixels(Bitmap bmp)
		{
			var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
			var length = bitmapData.Stride * bitmapData.Height;

			byte[] bytes = new byte[length];
			System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
			bmp.UnlockBits(bitmapData);
			return bytes;
		}
	}

	#endregion

	#region FilterApplicatorTestCase

	[TestedType(typeof(CurrentTaskFilter))]
	class CurrentTaskFilterApplicatorTest : TaskVisibilityFilterApplicatorTestCase<CurrentTaskFilter>
	{
		public override void TestApply_DbHits()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			CreateTask(CreateWorkflow(jobHeader, "workflow1"), "", 15);
			CreateTask(CreateWorkflow(jobHeader, "workflow2"), "", 15);
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

			var applicator = new CurrentTaskFilter();
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

			var task1 = workflow.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow.PK;
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var task2 = workflow.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertEquals(true, task1.IsStartable());
			AssertEquals(false, task2.IsStartable());

			var filter = new CurrentTaskFilter();
			AssertEquals(true, filter.IsApplicable(task1, new CellContent(0, 0, CellContentType.Cards), null));
			AssertEquals(false, filter.IsApplicable(task2, new CellContent(0, 0, CellContentType.Cards), null));
		}

		protected override CurrentTaskFilter GetFilter()
		{
			return new CurrentTaskFilter();
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield break;
		}

		public void TestShowWorkflow_AfterFilterCurrentTask()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = CreateStaffInCurrentBranchDept("S01", "Developer1");
			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, (bool?)null, resource1);
			var viewModel = sectionAndViewModel.Item2;
			var boardSection = sectionAndViewModel.Item1;
			var boardViewModel = VisualBoardFormTest.GetViewModel(boardSection.Board);
			boardSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = BMBoardSectionTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0, description: "Task1");
			var task2 = CreateTask(workflow1, resource1.GS_Code, 0, description: "Task2");

			var workflow2 = BMBoardSectionTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var task3 = CreateTask(workflow2, resource1.GS_Code, 0, description: "Task3");
			var task4 = CreateTask(workflow2, resource1.GS_Code, 0, description: "Task4");
			workflow1.MakePrerequisiteOf(workflow2);

			AssertEquals(false, workflow1.HasOpenPrerequisites);
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			workflow1.FH_FC_CurrentComponent = viewModel.ComponentPK;
			workflow2.FH_FC_CurrentComponent = viewModel.ComponentPK;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();

				var formTaskCards = form.FindAll<TaskCardControl>();
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, true);

				boardViewModel.FilterManager.ApplyFilter(new CurrentTaskFilter());
				form.RefreshNow_ForTest(forceReload: false);

				formTaskCards = form.FindAll<TaskCardControl>();
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, false);

				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
				form.RefreshNow_ForTest(forceReload: false);

				formTaskCards = form.FindAll<TaskCardControl>();
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, false);
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, true);
			}
		}

		public void TestShowJobWorkflow_AfterFilterCurrentTask()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = CreateStaffInCurrentBranchDept("S01", "Developer1");
			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, (bool?)null, resource1);
			var viewModel = sectionAndViewModel.Item2;
			var boardSection = sectionAndViewModel.Item1;
			var boardViewModel = VisualBoardFormTest.GetViewModel(boardSection.Board);
			boardSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = BMBoardSectionTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0, description: "Task1");
			var task2 = CreateTask(workflow1, resource1.GS_Code, 0, description: "Task2");

			var workflow2 = BMBoardSectionTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var task3 = CreateTask(workflow2, resource1.GS_Code, 0, description: "Task3");
			var task4 = CreateTask(workflow2, resource1.GS_Code, 0, description: "Task4");
			workflow1.FH_FC_CurrentComponent = viewModel.ComponentPK;
			workflow2.FH_FC_CurrentComponent = viewModel.ComponentPK;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();

				var formTaskCards = form.FindAll<TaskCardControl>();
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, true);

				boardViewModel.FilterManager.ApplyFilter(new CurrentTaskFilter());
				form.RefreshNow_ForTest(forceReload: false);

				formTaskCards = form.FindAll<TaskCardControl>();
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
				AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, true);

				boardSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
				Factory.Save();
				form.RefreshNow_ForTest(forceReload: false);

				formTaskCards = form.FindAll<TaskCardControl>();
				AssertEquals("Should be only one Job Workflow card", 1, formTaskCards.Count());
				var card = formTaskCards.FirstOrDefault();
				AssertNotEquals(card, workflow1);
				AssertNotEquals(card, workflow2);
			}
		}
	}

	#endregion

	#region CurrentTaskFilterMenuItemTest

	class CurrentTaskFilterMenuItemTest : CurrentTaskFilterMenuItem
	{
		public CurrentTaskFilterMenuItemTest(BMBoardSectionViewModel sectionViewModel)
			: base(sectionViewModel)
		{ }

		public CurrentTaskFilter GetFilter()
		{
			return (CurrentTaskFilter)Filter;
		}
		public new void UpdateAppliedVisualState()
		{
			base.UpdateAppliedVisualState();
		}
	}

	#endregion
}
