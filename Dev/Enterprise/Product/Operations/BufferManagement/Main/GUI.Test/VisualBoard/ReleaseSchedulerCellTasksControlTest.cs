using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ReleaseSchedulerCellTasksControlTest : BMSTestCaseWithFactory
	{
		[TestDate(2017, 10, 11)]
		public void TestReleaseSchedulerReorder_DbHits()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>(false);

			var workflows = new List<ProcessHeader>();
			for (var i = 0; i < 8; i++)
			{
				var workflow = CreateWorkflow(jobHeader, "workflow" + i);
				CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
				workflows.Add(workflow);
			}

			Factory.Save();

			var hits = new Dictionary<string, int>
			{
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 1 },
			};

			var factoryForControl = Factory.CreateNewFactory();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflows.ToArray());
			var cardContents = factoryForControl.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, ZGuid.Empty)).Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, factoryForControl))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				factoryForControl.ResetDatabaseLoadCount();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(8, taskCards.Length);

				// Drag last card to first position
				var card8 = taskCards[7];
				card8.Top = 0;

				control.OnDragDropFinished(card8);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((ZButton)control.Controls.Find("CloseTaskCardButton", true)[0]).PerformClick();
				AssertEquals(true, control.IsDisposed);

				AssertEquals("There are unsaved sequencing changes. Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertDbHits(hits, factoryForControl);
		}

		public void TestClose_WithNoChanges_ShouldNotPrompt()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);
			var cardContents = new[] { workflow1, workflow2 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				((ZButton)control.Controls.Find("CloseTaskCardButton", true)[0]).PerformClick();
				AssertEquals(true, control.IsDisposed);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSetupTasks_IfFilterIsAppliedShouldFilterBeforeChoosingWhich100()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var oneHundredAndOneWorkflowsAndTasks = Enumerable.Range(0, 101).Select(i =>
				{
					var workflow = CreateWorkflow(jobHeader, "w" + i);
					workflow.FH_VoteUpDownAmount = 100;
					var task = CreateTask(workflow, config.CCR.GS_Code, 60);

					return new
					{
						WorkFlow = workflow,
						Task = task
					};
				}).ToArray();

			var lastWorkflowAndTask = oneHundredAndOneWorkflowsAndTasks[100];
			var description = "translucent";
			lastWorkflowAndTask.WorkFlow.FH_CompletionStatement = description;
			lastWorkflowAndTask.WorkFlow.FH_VoteUpDownAmount = -100;
			lastWorkflowAndTask.Task.P9_Description = description;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (VisualBoardsTestHelper.UseBizoCardContents())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControlFactory = VisualBoardsTestHelper.GetActiveFactory("LoadCardContents");
				componentControlFactory.ThreadSentry.TakeThreadOwnership();

				var filter1 = new SearchFilter(description);
				form.BoardViewModel.FilterManager.ApplyFilter(filter1);

				var totalTasksButton = form.FindAll<ZButton>().Single(b => b.Name == "TotalTasksButton");
				totalTasksButton.PerformClick();
				Application.DoEvents();

				var cellTaskControl = form.FindAll<ReleaseSchedulerCellTasksControl>().Single();
				var card = cellTaskControl.FindAll<TaskCardControl>().Single();

				AssertEquals(description, ((IBizoCardContent)card.CardContent).Task.P9_Description);
			}
		}

		public void TestNudgeWontArithmeticallyOverflow()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);
			var cardContents = new[] { workflow1, workflow2 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				workflow1.FH_VoteUpDownAmount = short.MaxValue;

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				AssertNoExceptionThrown(() => control.OnDragDropFinished(card2));
			}
		}

		public void TestClose_WithChanges_ShouldPromptWhenClosingWithoutSave_AnsweredCancel()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);
			var cardContents = new[] { workflow1, workflow2 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				var task1Image = ((PictureBox)taskCards[0].Controls.Find("VoteUpDownImage", true)[0]);
				var task2Image = ((PictureBox)taskCards[1].Controls.Find("VoteUpDownImage", true)[0]);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.up_vote, (Bitmap)task1Image.Image);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.down_vote, (Bitmap)task2Image.Image);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				((ZButton)control.Controls.Find("CloseTaskCardButton", true)[0]).PerformClick();
				AssertEquals(false, control.IsDisposed);

				AssertEquals("There are unsaved sequencing changes. Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var loadedWorkflows = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, new[] { workflow1.PK, workflow2.PK }));
				AssertEquals(2, loadedWorkflows.Length);
				AssertEquals((short)0, loadedWorkflows[0].FH_VoteUpDownAmount);
				AssertEquals((short)0, loadedWorkflows[1].FH_VoteUpDownAmount);
			}
		}

		public void TestClose_MovingBackAndForward_ShouldNotPrompt()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);
			var cardContents = new[] { workflow1, workflow2 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				var task1Image = ((PictureBox)taskCards[0].Controls.Find("VoteUpDownImage", true)[0]);
				var task2Image = ((PictureBox)taskCards[1].Controls.Find("VoteUpDownImage", true)[0]);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.up_vote, (Bitmap)task1Image.Image);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.down_vote, (Bitmap)task2Image.Image);

				// Drag second card to first position (back to original positions)
				card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				AssertEquals("Should not add an image", 0, taskCards[0].Controls.Find("VoteUpDownImage", true).Length);
				AssertEquals("Should not add an image", 0, taskCards[1].Controls.Find("VoteUpDownImage", true).Length);

				((ZButton)control.Controls.Find("CloseTaskCardButton", true)[0]).PerformClick();
				AssertEquals(true, control.IsDisposed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClose_WithChanges_ShouldPromptWhenClosingWithoutSave_AnsweredNo()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);
			var cardContents = new[] { workflow1, workflow2 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				((ZButton)control.Controls.Find("CloseTaskCardButton", true)[0]).PerformClick();
				AssertEquals(true, control.IsDisposed);

				AssertEquals("There are unsaved sequencing changes. Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var loadedWorkflows = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, new[] { workflow1.PK, workflow2.PK }));
				AssertEquals(2, loadedWorkflows.Length);
				AssertEquals((short)0, loadedWorkflows[0].FH_VoteUpDownAmount);
				AssertEquals((short)0, loadedWorkflows[1].FH_VoteUpDownAmount);
			}
		}

		public void TestClose_WithChanges_ShouldPromptWhenClosingWithoutSave_AnsweredYes()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);
			var cardContents = new[] { workflow1, workflow2 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((ZButton)control.Controls.Find("CloseTaskCardButton", true)[0]).PerformClick();
				AssertEquals(true, control.IsDisposed);

				AssertEquals("There are unsaved sequencing changes. Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(ProcessHeaderSchema.PK, new[] { workflow1.PK, workflow2.PK });
				query.OrderBy = ProcessHeaderSchema.Constants.FH_CompletionStatement;
				var loadedWorkflows = newFactory.Load<ProcessHeader>(query);
				AssertEquals(2, loadedWorkflows.Length);
				AssertEquals((short)0, loadedWorkflows[0].FH_VoteUpDownAmount);
				AssertEquals((short)1, loadedWorkflows[1].FH_VoteUpDownAmount);
			}
		}

		public void TestDragDrop_ShouldReloadWorkflowsAndReorder()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			group.Staff.Add(resource);

			resource.DesignateAsCCR(buffer);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(priorityTags, "GRN", "Green", nudge: 30);
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", "Platinum", nudge: 20);
			var gold = BMSTestHelper.CreateTagMagnitude(priorityTags, "GLD", "Gold", nudge: 10);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", bucket);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", bucket);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", bucket);
			workflow1.AddTag(green);
			workflow2.AddTag(platinum);
			workflow3.AddTag(gold);

			workflow1.FH_GG_ReleaseGroup = workflow2.FH_GG_ReleaseGroup = workflow3.FH_GG_ReleaseGroup = group.PK;

			CreateTask(workflow1, resource.GS_Code, 60);
			CreateTask(workflow2, resource.GS_Code, 60);
			CreateTask(workflow3, resource.GS_Code, 60);

			var sortedWorkflows = WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2, workflow3 }).ToArray();
			AssertEquals(workflow1, sortedWorkflows[0]);
			AssertEquals(workflow2, sortedWorkflows[1]);
			AssertEquals(workflow3, sortedWorkflows[2]);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow1.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow2.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);

				var taskPanel = (TaskPanel)taskCards[0].Parent;
				taskPanel.FindAll<ZButton>().Single().PerformClick();

				var cellTasksControl = form.FindAll<ReleaseSchedulerCellTasksControl>().Single();
				taskCards = cellTasksControl.FindAll<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);

				taskCards[0].Top = taskCards[1].Top + 1;
				cellTasksControl.OnDragDropFinished(taskCards[0]);
				cellTasksControl.Save();

				Application.DoEvents();

				AssertEquals(true, cellTasksControl.IsDisposed);
				taskCards = taskPanel.FindAll<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow2.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow1.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);
			}
		}

		public void TestDragDrop_ShouldReorderWorkflows()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(priorityTags, "GRN", "Green", nudge: 30);
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", "Platinum", nudge: 20);
			var gold = BMSTestHelper.CreateTagMagnitude(priorityTags, "GLD", "Gold", nudge: 10);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.AddTag(green);
			workflow2.AddTag(platinum);
			workflow3.AddTag(gold);
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var sortedWorkflows = WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2, workflow3 }).ToArray();
			AssertEquals(workflow1, sortedWorkflows[0]);
			AssertEquals(workflow2, sortedWorkflows[1]);
			AssertEquals(workflow3, sortedWorkflows[2]);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2, workflow3);
			var cardContents = new[] { workflow1, workflow2, workflow3 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow1.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow2.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);

				var labels = control.TaskCardsPanel.Controls.OfType<ZLabel>().ToArray();
				AssertEquals(3, labels.Length);
				AssertEquals("Queue hours: 1:30", labels[0].Text);
				AssertEquals("Queue hours: 3:00", labels[1].Text);
				AssertEquals("Queue hours: 4:30", labels[2].Text);

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				labels = control.TaskCardsPanel.Controls.OfType<ZLabel>().ToArray();
				AssertEquals(3, labels.Length);
				AssertEquals("Queue hours: 1:30", labels[0].Text);
				AssertEquals("Queue hours: 3:00", labels[1].Text);
				AssertEquals("Queue hours: 4:30", labels[2].Text);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow2.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow1.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);

				// Drag second card to last position
				card2 = taskCards[1];
				card2.Top = taskCards[2].Top + 1;

				control.OnDragDropFinished(card2);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow2.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow1.PK, taskCards[2].CardContent.WorkflowIdentifier);

				// Drag second card to same position
				control.OnDragDropFinished(taskCards[1]);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow2.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow1.PK, taskCards[2].CardContent.WorkflowIdentifier);
			}
		}

		[TestDate(2013, 11, 25)]
		public void TestDragDrop_WhenNudgingWouldMoveMoreThanThePlacesSpecified_ShouldNudgeOtherWorkflowsToo()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";
			workflow1.FH_VoteUpDownAmount = 1;
			workflow2.FH_VoteUpDownAmount = 0;
			workflow3.FH_VoteUpDownAmount = 0;

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var sortedWorkflows = WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2, workflow3 }).ToArray();
			AssertEquals(workflow1, sortedWorkflows[0]);
			AssertEquals(workflow2, sortedWorkflows[1]);
			AssertEquals(workflow3, sortedWorkflows[2]);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2, workflow3);
			var cardContents = new[] { workflow1, workflow2, workflow3 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, Factory))
			{
				control.SetupTasks();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow1.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow2.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);

				// Drag first card to second position
				var card1 = taskCards[0];
				card1.Top = taskCards[1].Top + 1;

				control.OnDragDropFinished(card1);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals("Should drag card to the second position (it could have gone to second or third based on nudge values, but we're a bit smarter than that...)", workflow1.PK, taskCards[1].CardContent.WorkflowIdentifier);

				sortedWorkflows = WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2, workflow3 }).ToArray();
				AssertEquals(workflow2, sortedWorkflows[0]);
				AssertEquals(workflow1, sortedWorkflows[1]);
				AssertEquals(workflow3, sortedWorkflows[2]);

				AssertEquals((short)1, workflow1.FH_VoteUpDownAmount);
				AssertEquals((short)2, workflow2.FH_VoteUpDownAmount);
				AssertEquals((short)0, workflow3.FH_VoteUpDownAmount);
			}
		}

		public void TestDragDrop_InAnotherFactoryContext_ShouldReorderWorkflows()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(priorityTags, "GRN", "Green", nudge: 30);
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", "Platinum", nudge: 20);
			var gold = BMSTestHelper.CreateTagMagnitude(priorityTags, "GLD", "Gold", nudge: 10);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.AddTag(green);
			workflow2.AddTag(platinum);
			workflow3.AddTag(gold);
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var sortedWorkflows = WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2, workflow3 }).ToArray();
			AssertEquals(workflow1, sortedWorkflows[0]);
			AssertEquals(workflow2, sortedWorkflows[1]);
			AssertEquals(workflow3, sortedWorkflows[2]);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2, workflow3);
			var cardContents = new[] { workflow1, workflow2, workflow3 }.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			using (var menuStrip = TaskCardControl.CreateMenuStrip())
			using (var control = ReleaseSchedulerCellTasksControl.GetCellTasksControl(cell, cardContents, viewModel, menuStrip))
			{
				control.SetupTasks();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow1.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow2.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);

				// Drag second card to first position
				var card2 = taskCards[1];
				card2.Top = 0;

				control.OnDragDropFinished(card2);

				taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(3, taskCards.Length);
				AssertEquals(workflow2.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow1.PK, taskCards[1].CardContent.WorkflowIdentifier);
				AssertEquals(workflow3.PK, taskCards[2].CardContent.WorkflowIdentifier);

				AssertEquals((short)11, ((IBizoCardContent)taskCards[0].CardContent).Workflow.FH_VoteUpDownAmount);
				AssertEquals("Should not change state of workflow not in same factory context", (short)0, workflow2.FH_VoteUpDownAmount);
				AssertEquals(false, control.IsDisposed);

				control.Save();
				AssertEquals((short)11, workflow2.FH_VoteUpDownAmount);
				AssertEquals(true, control.IsDisposed);
			}
		}

		[TestDate(2013, 11, 15, 9, 0, 0)]
		public void TestWorkflowsShouldShowRunningTotals()
		{
			var scaledCardPadding = ControlDpiScalingHelper.ScaleToCurrentDpiY(CellTasksControl.CardPadding);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON";

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			buffer.FC_GB_AgingBranch = branch.PK;

			var constraint = CreateConstraint(buffer, offsetMinutes: 13 * 60);
			LinkComponents(bucket, buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var resource_ccr = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource_nonCcr = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			resource_ccr.DesignateAsCCR(buffer);
			group.Staff.AddRange(resource_ccr, resource_nonCcr);

			var releasedTasks = new ProcessTask[5];
			var unreleasedTasks = new ProcessTask[5];

			for (int i = 0; i < 5; i++)
			{
				var releasedWorkflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				releasedWorkflow.FH_FC_CurrentComponent = buffer.PK;
				releasedTasks[i] = CreateTask(releasedWorkflow, resource_ccr.GS_Code, 60);
				CreateTask(releasedWorkflow, resource_nonCcr.GS_Code, 60);
				CreateTask(releasedWorkflow, resource_ccr.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

				var unreleasedWorkflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				unreleasedWorkflow.FH_FC_CurrentComponent = bucket.PK;
				unreleasedTasks[i] = CreateTask(unreleasedWorkflow, resource_ccr.GS_Code, 60);
				CreateTask(unreleasedWorkflow, resource_nonCcr.GS_Code, 60);
			}

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, releasedTasks.Concat(unreleasedTasks).ToArray());

			var workflowCardContents = releasedTasks.Select(t => new WorkflowCardContent(t.GetProcessHeader(), t, viewModel)).ToArray();
			var releasedCellContent = viewModel.ComponentGrid[1, 1];

			AssertEquals("Frodo Baggins", releasedCellContent.Channel.GetChannelName(DisplayNameType.FullName));
			AssertEquals("Released", releasedCellContent.SecondaryChannel.GetChannelName(DisplayNameType.FullName));

			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(releasedCellContent, workflowCardContents, viewModel, Factory))
			{
				control.SetupTasks();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(5, taskCards.Length);

				var labels = control.TaskCardsPanel.Controls.OfType<ZLabel>().ToArray();
				AssertEquals(5, labels.Length);

				var controls = taskCards.Zip(labels, (c1, c2) => Tuple.Create(c1, c2)).ToArray();

				CombineAssertions("Released work", () =>
				{
					AssertEquals(0, controls[0].Item1.Top);
					AssertEquals(controls[0].Item1.Height + controls[0].Item1.Top, controls[0].Item2.Top);
					AssertEquals("Queue hours: 1:30", controls[0].Item2.Text);

					AssertEquals(controls[0].Item2.Height + controls[0].Item2.Top + scaledCardPadding, controls[1].Item1.Top);
					AssertEquals(controls[1].Item1.Height + controls[1].Item1.Top, controls[1].Item2.Top);
					AssertEquals("Queue hours: 3:00", controls[1].Item2.Text);

					AssertEquals(controls[1].Item2.Height + controls[1].Item2.Top + scaledCardPadding, controls[2].Item1.Top);
					AssertEquals(controls[2].Item1.Height + controls[2].Item1.Top, controls[2].Item2.Top);
					AssertEquals("Queue hours: 4:30", controls[2].Item2.Text);

					AssertEquals(controls[2].Item2.Height + controls[2].Item2.Top + scaledCardPadding, controls[3].Item1.Top);
					AssertEquals(controls[3].Item1.Height + controls[3].Item1.Top, controls[3].Item2.Top);
					AssertEquals("Queue hours: 6:00", controls[3].Item2.Text);

					AssertEquals(controls[3].Item2.Height + controls[3].Item2.Top + scaledCardPadding, controls[4].Item1.Top);
					AssertEquals(controls[4].Item1.Height + controls[4].Item1.Top, controls[4].Item2.Top);
					AssertEquals("Queue hours: 7:30", controls[4].Item2.Text);
				});
			}

			var unreleasedCellContent = viewModel.ComponentGrid[2, 1];
			AssertEquals("Frodo Baggins", unreleasedCellContent.Channel.GetChannelName(DisplayNameType.FullName));
			AssertEquals("Un-Released", unreleasedCellContent.SecondaryChannel.GetChannelName(DisplayNameType.FullName));

			workflowCardContents = unreleasedTasks.Select(t => new WorkflowCardContent(t.GetProcessHeader(), t, viewModel)).ToArray();

			using (var control = BMSGUITestCase.CreateReleaseSchedulerCellTasksControl(unreleasedCellContent, workflowCardContents, viewModel, Factory))
			{
				control.SetupTasks();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(5, taskCards.Length);

				var labels = control.TaskCardsPanel.Controls.OfType<ZLabel>().ToArray();
				AssertEquals(5, labels.Length);

				var controls = taskCards.Zip(labels, (c1, c2) => Tuple.Create(c1, c2)).ToArray();

				CombineAssertions("Un-released work", () =>
				{
					AssertEquals(0, controls[0].Item1.Top);
					AssertEquals(controls[0].Item1.Height + controls[0].Item1.Top, controls[0].Item2.Top);
					AssertColorEquals(SystemColors.ControlText, controls[0].Item2.ForeColor);
					AssertMultilineASCIIEquals("", @"Queue hours: 9:00
Estimated completion: 15-Nov-13 18:00", controls[0].Item2.Text);

					AssertEquals(controls[0].Item2.Height + controls[0].Item2.Top + scaledCardPadding, controls[1].Item1.Top);
					AssertEquals(controls[1].Item1.Height + controls[1].Item1.Top, controls[1].Item2.Top);
					AssertColorEquals(SystemColors.ControlText, controls[1].Item2.ForeColor);
					AssertMultilineASCIIEquals("", @"Queue hours: 10:30
Estimated completion: 18-Nov-13 10:00", controls[1].Item2.Text);

					AssertEquals(controls[1].Item2.Height + controls[1].Item2.Top + scaledCardPadding, controls[2].Item1.Top);
					AssertEquals(controls[2].Item1.Height + controls[2].Item1.Top, controls[2].Item2.Top);
					AssertColorEquals(SystemColors.ControlText, controls[2].Item2.ForeColor);
					AssertMultilineASCIIEquals("", @"Queue hours: 12:00
Estimated completion: 18-Nov-13 11:30", controls[2].Item2.Text);

					AssertEquals(controls[2].Item2.Height + controls[2].Item2.Top + scaledCardPadding, controls[3].Item1.Top);
					AssertEquals(controls[3].Item1.Height + controls[3].Item1.Top, controls[3].Item2.Top);
					AssertColorEquals(Color.Fuchsia, controls[3].Item2.ForeColor);
					AssertMultilineASCIIEquals("", @"Queue hours: 13:30
Estimated completion: 18-Nov-13 13:00", controls[3].Item2.Text);

					AssertEquals(controls[3].Item2.Height + controls[3].Item2.Top + scaledCardPadding, controls[4].Item1.Top);
					AssertEquals(controls[4].Item1.Height + controls[4].Item1.Top, controls[4].Item2.Top);
					AssertColorEquals(SystemColors.ControlText, controls[4].Item2.ForeColor);
					AssertMultilineASCIIEquals("", @"Queue hours: 15:00
Estimated completion: 18-Nov-13 14:30", controls[4].Item2.Text);
				});
			}
		}

		[TestDate(2013, 11, 15, 9, 0, 0)]
		public void TestWorkflowsShouldShowRunningTotals_AndIncludeFilteredOutTasksInRunningTotals()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON";

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(priorityTags, "GRN", "Green", nudge: 30);
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", "Platinum", nudge: 20);

			config.Buffer.FC_GB_AgingBranch = branch.PK;

			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow1.AddTag(green);
			workflow2.AddTag(platinum);
			workflow1.FH_ReleaseDateTime = workflow1.FH_ReleaseDateTime.AddDays(-1); //To make sure they are released at a slightly different time

			var task1 = CreateTask(workflow1, config.CCR.GS_Code, 60);
			var task2 = CreateTask(workflow2, config.CCR.GS_Code, 60);

			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var componentControl = form.FindAll<BMComponentControl>().Single();
				var releasedTaskListButton = componentControl.FindAll<ZButton>().First();
				releasedTaskListButton.PerformClick();

				using (var releasedTaskListControl = form.Controls.OfType<ReleaseSchedulerCellTasksControl>().Single())
				{
					var taskCards = releasedTaskListControl.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
					AssertEquals(2, taskCards.Length);

					var labels = releasedTaskListControl.TaskCardsPanel.Controls.OfType<ZLabel>().OrderBy(l => l.Top).ToArray();
					AssertEquals(2, labels.Length);

					AssertEquals("Queue hours: 1:30", labels[0].Text);
					AssertEquals("Queue hours: 3:00", labels[1].Text);
				}

				form.BoardViewModel.FilterManager.ApplyFilter(new SearchFilter("workflow2"));
				releasedTaskListButton = componentControl.FindAll<ZButton>().First();
				releasedTaskListButton.PerformClick();

				using (var releasedTaskListControl = form.Controls.OfType<ReleaseSchedulerCellTasksControl>().Single())
				{
					var taskCards = releasedTaskListControl.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
					AssertEquals(1, taskCards.Length);

					var labels = releasedTaskListControl.TaskCardsPanel.Controls.OfType<ZLabel>().ToArray();
					AssertEquals(1, labels.Length);

					AssertEquals("Queue hours: 3:00", labels[0].Text);
				}
			}
		}

		[TestDate(2015, 5, 8)]
		public void TestShouldIgnoreCardsWhenThereIsNoMoreCardContent()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2, workflow3);
			var cardContents = new[] { workflow1, workflow2, workflow3 }
				.Select(w => new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel))
				.ToArray();
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) };

			AssertEquals(cardContents.First().Workflow.TaskCollection.Count, 1);

			cardContents.First().Workflow.TaskCollection.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(cardContents.First().Workflow.TaskCollection.Count, 0);

			using (var form = new ZForm())
			using (var menuStrip = TaskCardControl.CreateMenuStrip())
			using (var control = ReleaseSchedulerCellTasksControl.GetCellTasksControl(cell, cardContents, viewModel, menuStrip))
			{
				control.SetupTasks();
				form.Controls.Add(control);
				form.Show();

				var taskCards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
			}
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}
}
