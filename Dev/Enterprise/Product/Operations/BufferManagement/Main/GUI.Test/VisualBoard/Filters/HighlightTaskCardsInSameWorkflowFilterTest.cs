using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region BoardFilterTestCase

	[TestedType(typeof(HighlightTaskCardsInSameWorkflowFilter))]
	class HighlightTaskCardsInSameWorkflowFilterTest : BoardFilterTestCase<HighlightTaskCardsInSameWorkflowFilter>
	{
		public override void TestFilterName()
		{
			var job = Factory.New<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "Workflow1";

			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var filter = new HighlightTaskCardsInSameWorkflowFilter(workflow);
			AssertEquals("Highlighting tasks in workflow Workflow1", filter.FilterName);
		}

		public override void TestEquals()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var workflow1 = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			var workflow2 = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

			var task1_1 = job.WorkflowItems.AddNew();
			task1_1.P9_FH_ProcessHeader = workflow1.PK;
			var task1_2 = job.WorkflowItems.AddNew();
			task1_2.P9_FH_ProcessHeader = workflow1.PK;

			var task2_1 = job.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = workflow2.PK;

			var filter1 = new HighlightTaskCardsInSameWorkflowFilter(workflow1);
			var filter2 = new HighlightTaskCardsInSameWorkflowFilter(workflow1);
			var filter3 = new HighlightTaskCardsInSameWorkflowFilter(workflow2);

			AssertEquals(filter1, filter2);
			AssertNotEquals(filter1, filter3);
			AssertNotEquals(filter2, filter3);
		}

		public override void TestAllowMultiple()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var task = Factory.New<ProcessTask>();
			task.P9_FH_ProcessHeader = workflow.PK;
			AssertEquals("Should not allow multiple - filter should turn off other instances", false, new HighlightTaskCardsInSameWorkflowFilter(workflow).AllowMultiple);
		}

		protected override HighlightTaskCardsInSameWorkflowFilter GetFilter()
		{
			return new HighlightTaskCardsInSameWorkflowFilter(CreateJobHeader<OrgHeader>().ProcessHeaders[0]);
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				taskCards[0].TaskCardContextMenuStrip_OnOpening_ForTest();
				var contextMenu = taskCards[0].ContextMenuStrip;
				AssertNotNull(contextMenu);

				var menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().ElementAt(0);

				AssertEquals("Highlight Tasks in Workflow", menuItem.Text);
				AssertEquals(false, menuItem.Checked);

				menuItem.PerformClick();

				taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				contextMenu = taskCards[0].ContextMenuStrip;
				taskCards[0].TaskCardContextMenuStrip_OnOpening_ForTest();

				menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().ElementAt(0);
				AssertEquals("Highlight Tasks in Workflow", menuItem.Text);
				AssertEquals(true, menuItem.Checked);

				form.BoardViewModel.FilterManager.Clear();

				taskCards = form.FindAll<TaskCardControl>().ToArray();
				taskCards[0].TaskCardContextMenuStrip_OnOpening_ForTest();
				AssertEquals(2, taskCards.Length);
				contextMenu = taskCards[0].ContextMenuStrip;
				menuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().ElementAt(0);
				AssertEquals(false, menuItem.Checked);
			}
		}
	}

	#endregion

	#region FilterApplicatorTestCase

	[TestedType(typeof(HighlightTaskCardsInSameWorkflowFilter))]
	class HighlightTaskCardsInSameWorkflowFilterApplicatorTest : TaskVisibilityFilterApplicatorTestCase<HighlightTaskCardsInSameWorkflowFilter>
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

			var applicator = new HighlightTaskCardsInSameWorkflowFilter(jobHeader.ProcessHeaders.First());
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
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var workflow1 = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			var workflow2 = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

			var task1_1 = job.WorkflowItems.AddNew();
			task1_1.P9_FH_ProcessHeader = workflow1.PK;
			var task1_2 = job.WorkflowItems.AddNew();
			task1_2.P9_FH_ProcessHeader = workflow1.PK;

			var task2_1 = job.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = workflow2.PK;

			var filter = new HighlightTaskCardsInSameWorkflowFilter(workflow1);

			AssertEquals("Same task - is applicable", true, filter.IsApplicable(task1_1, null, null));
			AssertEquals("Same workflow - is applicable", true, filter.IsApplicable(task1_2, null, null));
			AssertEquals("Different workflow - not applicable", false, filter.IsApplicable(task2_1, null, null));
		}

		protected override HighlightTaskCardsInSameWorkflowFilter GetFilter()
		{
			return new HighlightTaskCardsInSameWorkflowFilter(CreateWorkflow(CreateJobHeader<OrgHeader>(), "Bork"));
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield break;
		}
	}

	#endregion
}
