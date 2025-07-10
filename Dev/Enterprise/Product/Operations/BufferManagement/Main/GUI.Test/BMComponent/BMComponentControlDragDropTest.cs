using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestDate(2015, 7, 14)]
	class BMComponentControlDragDropTest : BMSGUITestCase
	{
		#region Job-Level Workflow Tickets

		public void TestDragDrop_JobTickets_WhenMultipleWorkflowsInDifferentComponents()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scarn", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scotch", config.Buffer);

			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddHours(4);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "BBB");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "AAA"); // Task type is significant as the ordering in collections triggered this defect.

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var board = newFactory.Load<BMBoard>(section.MS_MB_Board);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var taskTicket = componentControl.FindSingle<TaskCardControl>();

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var secondCell = componentControl.ViewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);
				var dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, secondCell);

				AssertEquals(true, dragDropResult);
				AssertEquals(ZDateTime.UtcNow.AddDays(1).AddHours(4), jobHeader.FH_DoNotStartBeforeDate);

				taskTicket = componentControl.FindSingle<TaskCardControl>();
				AssertEquals(1, taskTicket.Cell.TimeIndex);

				dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, firstCell);

				AssertEquals(true, dragDropResult);
				AssertEquals(ZDateTime.UtcNow.AddHours(4), jobHeader.FH_DoNotStartBeforeDate);

				taskTicket = componentControl.FindSingle<TaskCardControl>();
				AssertEquals(0, taskTicket.Cell.TimeIndex);
			}
		}

		public void TestDragDrop_JobTickets_WhenMultipleWorkflowsInDifferentComponents_SectionHasAdditionalComponents()
		{
			var secondBucket = BMSTestHelper.CreateBucket(config.System, "Schmucket");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scarn", secondBucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scotch", config.Buffer);

			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddHours(4);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "BBB");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "AAA"); // Task type is significant as the ordering in collections triggered this defect.

			BMSTestHelper.CreateAdditionalComponent(section, secondBucket);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var board = newFactory.Load<BMBoard>(section.MS_MB_Board);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var taskTicket = componentControl.FindSingle<TaskCardControl>();

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var secondCell = componentControl.ViewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);
				var dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, secondCell);

				AssertEquals(true, dragDropResult);
				AssertEquals(ZDateTime.UtcNow.AddDays(1).AddHours(4), jobHeader.FH_DoNotStartBeforeDate);

				taskTicket = componentControl.FindSingle<TaskCardControl>();
				AssertEquals(1, taskTicket.Cell.TimeIndex);

				dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, firstCell);

				AssertEquals(true, dragDropResult);
				AssertEquals(ZDateTime.UtcNow.AddHours(4), jobHeader.FH_DoNotStartBeforeDate);

				taskTicket = componentControl.FindSingle<TaskCardControl>();
				AssertEquals(0, taskTicket.Cell.TimeIndex);
			}
		}

		public void TestDragDrop_JobTickets_IntoSameCell()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scarn", config.Bucket);

			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddHours(4);

			BMSTestHelper.CreateTask(workflow1);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var board = newFactory.Load<BMBoard>(section.MS_MB_Board);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var taskTicket = componentControl.FindSingle<TaskCardControl>();

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, firstCell);

				AssertEquals(false, dragDropResult);
				AssertNull("Don't bug the user - they just returned the ticket to the starting cell", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDateTime.UtcNow.AddHours(4), jobHeader.FH_DoNotStartBeforeDate);
			}
		}

		public void TestDragDrop_JobTickets_IntoSameCell_SectionHasAdditionalComponents()
		{
			var secondBucket = BMSTestHelper.CreateBucket(config.System, "Schmucket");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scarn", secondBucket);

			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddHours(4);

			BMSTestHelper.CreateTask(workflow1);

			BMSTestHelper.CreateAdditionalComponent(section, secondBucket);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var board = newFactory.Load<BMBoard>(section.MS_MB_Board);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var taskTicket = componentControl.FindSingle<TaskCardControl>();

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, firstCell);

				AssertEquals(false, dragDropResult);
				AssertNull("Don't bug the user - they just returned the ticket to the starting cell", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDateTime.UtcNow.AddHours(4), jobHeader.FH_DoNotStartBeforeDate);
			}
		}

		public void TestDragDrop_JobTickets_IntoAnotherChannelOfSameSection()
		{
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MSR", "Michael Scarn");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MSH", "Michael Scotch");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.ResourcesWithCapability.AddRange(resource1, resource2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Michael Scott", config.Bucket);

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddHours(4);

			BMSTestHelper.CreateTask(workflow, capability: capability);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var board = newFactory.Load<BMBoard>(section.MS_MB_Board);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var taskTicket = componentControl.FindSingle<TaskCardControl>(t => t.Cell.Channel.EntityPK == resource1.PK);

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var cellInOtherChannel = componentControl.ViewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c != firstCell);
				var dragDropResult = DragTaskTicketToCell(taskTicket, componentControl, cellInOtherChannel);

				AssertEquals(false, dragDropResult);
				AssertEquals("Drag/drop between channels is not supported for job-level workflow tickets.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDateTime.UtcNow.AddHours(4), jobHeader.FH_DoNotStartBeforeDate);
			}
		}

		#endregion

		#region Workflow Hierarchy

		public void TestDragDrop_QualityIterationWorkflow_ShouldMoveIterationAndAncestorWorkflows()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			BMSTestHelper.AddTaskTypesToRegistry("ORG", "ONE", "TWO");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("TWO", "ORG");

			var board = config.BufferBoard;
			board.MB_Name = "Turncolm Malbull";

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskType: "ONE", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskType: "TWO", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var qiWorkflow1 = CreateQualityIteration(task1, task2, iterationWorkflowDescription: "qiWorkflow1");
			var qiWorkflow2 = CreateQualityIteration(qiWorkflow1.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow1.Tasks.Single(t => t.P9_Type == "TWO"), iterationWorkflowDescription: "qiWorkflow2");
			var qiWorkflow3 = CreateQualityIteration(qiWorkflow2.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow2.Tasks.Single(t => t.P9_Type == "TWO"), iterationWorkflowDescription: "qiWorkflow3");
			var qiWorkflow4 = CreateQualityIteration(qiWorkflow3.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow3.Tasks.Single(t => t.P9_Type == "TWO"), iterationWorkflowDescription: "qiWorkflow4");
			var qiWorkflow5 = CreateQualityIteration(qiWorkflow4.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow4.Tasks.Single(t => t.P9_Type == "TWO"), iterationWorkflowDescription: "qiWorkflow5");

			Factory.Save();

			const decimal initialPenetration = 0.125m;
			var context = config.Buffer.GetRelevantContext();

			CombineAssertions("Initial buffer penetrations", () =>
			{
				AssertEquals("workflow", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory).Penetration);
				AssertEquals("qiWorkflow1", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow1, context, Factory).Penetration);
				AssertEquals("qiWorkflow2", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow2, context, Factory).Penetration);
				AssertEquals("qiWorkflow3", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow3, context, Factory).Penetration);
				AssertEquals("qiWorkflow4", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow4, context, Factory).Penetration);
				AssertEquals("qiWorkflow5", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow5, context, Factory).Penetration);
			});

			CombineAssertions("Initial values of FH_ReleaseDateTime", () =>
			{
				AssertEquals("workflow", ZDateTime.UtcNow, workflow.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow1", ZDateTime.UtcNow, qiWorkflow1.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow2", ZDateTime.UtcNow, qiWorkflow2.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow3", ZDateTime.UtcNow, qiWorkflow3.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow4", ZDateTime.UtcNow, qiWorkflow4.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow5", ZDateTime.UtcNow, qiWorkflow5.FH_ReleaseDateTime);
			});

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				CombineAssertions("Before drag/drop, tickets should all be in the first time index", () =>
				{
					AssertEquals("qiWorkflow1", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow1.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow2", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow2.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow3", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow3.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow4", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow4.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow5", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow5.Tasks.First()).Cell.TimeIndex);
				});

				var taskTicket = BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow3.Tasks.First());

				AssertEquals(1, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var secondCell = componentControl.ViewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 2);
				AssertEquals(true, DragTaskTicketToCell(taskTicket, componentControl, secondCell));

				CombineAssertions("After drag/drop, should move tickets for all QI workflows up one cell", () =>
				{
					AssertEquals("qiWorkflow1", 2, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow1.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow2", 2, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow2.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow3", 2, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow3.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow4", 2, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow4.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow5", 2, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow5.Tasks.First()).Cell.TimeIndex);
				});
			}

			const decimal newPenetration = 0.2083333333333333333333333333m;
			Factory.Save(); // Clear the buffer penetration cache.

			CombineAssertions("After drag/drop, buffer penetration of all workflows should be updated", () =>
			{
				AssertEquals("workflow", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory).Penetration);
				AssertEquals("qiWorkflow1", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow1, context, Factory).Penetration);
				AssertEquals("qiWorkflow2", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow2, context, Factory).Penetration);
				AssertEquals("qiWorkflow3", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow3, context, Factory).Penetration);
				AssertEquals("qiWorkflow4", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow4, context, Factory).Penetration);
				AssertEquals("qiWorkflow5", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow5, context, Factory).Penetration);
			});

			CombineAssertions("After drag/drop, only FH_ReleaseDateTime of the parent workflow should be updated", () =>
			{
				AssertEquals("workflow", new ZDateTime(2015, 7, 13, 0, 0, 0), workflow.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow1", ZDateTime.UtcNow, qiWorkflow1.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow2", ZDateTime.UtcNow, qiWorkflow2.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow3", ZDateTime.UtcNow, qiWorkflow3.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow4", ZDateTime.UtcNow, qiWorkflow4.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow5", ZDateTime.UtcNow, qiWorkflow5.FH_ReleaseDateTime);
			});
		}

		public void TestDragDrop_ParentWorkflowWhichHasQualityIterations_ShouldMoveWorkflowAndIterations_WhenDataRefreshOn()
		{
			DragDrop_ParentWorkflowWhichHasQualityIterations_ShouldMoveWorkflowAndIterations(true);
		}

		public void TestDragDrop_ParentWorkflowWhichHasQualityIterations_ShouldMoveWorkflowAndIterations_WhenDataRefreshOff()
		{
			DragDrop_ParentWorkflowWhichHasQualityIterations_ShouldMoveWorkflowAndIterations(false);
		}

		void DragDrop_ParentWorkflowWhichHasQualityIterations_ShouldMoveWorkflowAndIterations(bool dataRefreshValue)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dataRefreshValue);
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "ONE", "TWO");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("TWO", "ORG");

			var board = config.BufferBoard;
			board.MB_Name = "Turncolm Malbull";

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskType: "ONE");
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskType: "TWO");

			Factory.Save();

			var qiWorkflow1 = CreateQualityIteration(task1, task2);
			var qiWorkflow2 = CreateQualityIteration(qiWorkflow1.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow1.Tasks.Single(t => t.P9_Type == "TWO"));

			Factory.Save();

			const decimal initialPenetration = 0.0625m;
			var context = config.Buffer.GetRelevantContext();

			CombineAssertions("Initial buffer penetrations", () =>
			{
				AssertEquals("workflow", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory).Penetration);
				AssertEquals("qiWorkflow1", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow1, context, Factory).Penetration);
				AssertEquals("qiWorkflow2", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow2, context, Factory).Penetration);
			});

			CombineAssertions("Initial values of FH_ReleaseDateTime", () =>
			{
				AssertEquals("workflow", ZDateTime.UtcNow, workflow.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow1", ZDateTime.UtcNow, qiWorkflow1.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow2", ZDateTime.UtcNow, qiWorkflow2.FH_ReleaseDateTime);
			});

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();

				CombineAssertions("Before drag/drop, tickets should all be in the first time index", () =>
				{
					AssertEquals("workflow", 0, BMSGUITestCase.FindTaskCardControl(componentControl, workflow.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow1", 0, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow1.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow2", 0, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow2.Tasks.First()).Cell.TimeIndex);
				});

				var taskTicket = BMSGUITestCase.FindTaskCardControl(componentControl, workflow.Tasks.First());

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var secondCell = componentControl.ViewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);
				AssertEquals(true, DragTaskTicketToCell(taskTicket, componentControl, secondCell));

				CombineAssertions("After drag/drop, should move tickets for all QI workflows up one cell", () =>
				{
					AssertEquals("workflow", 1, BMSGUITestCase.FindTaskCardControl(componentControl, workflow.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow1", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow1.Tasks.First()).Cell.TimeIndex);
					AssertEquals("qiWorkflow2", 1, BMSGUITestCase.FindTaskCardControl(componentControl, qiWorkflow2.Tasks.First()).Cell.TimeIndex);
				});
			}

			const decimal newPenetration = 0.125m;
			Factory.Save(); // Clear the buffer penetration cache.

			CombineAssertions("After drag/drop, buffer penetration of all workflows should be updated", () =>
			{
				AssertEquals("workflow", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory).Penetration);
				AssertEquals("qiWorkflow1", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow1, context, Factory).Penetration);
				AssertEquals("qiWorkflow2", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow2, context, Factory).Penetration);
			});

			CombineAssertions("After drag/drop, only FH_ReleaseDateTime of the parent workflow should be updated", () =>
			{
				AssertEquals("workflow", new ZDateTime(2015, 7, 13, 2, 0, 0), workflow.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow1", ZDateTime.UtcNow, qiWorkflow1.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow2", ZDateTime.UtcNow, qiWorkflow2.FH_ReleaseDateTime);
			});
		}

		public void TestDragDrop_ChildWorkflow_ShouldMoveChildWorkflowOnly()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var board = config.BufferBoard;
			board.MB_Name = "Turncolm Malbull";

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Silence!", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Less silence!", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, 60);

			workflow2.GetOrCreateLinkToParent(workflow1);

			Factory.Save();

			var context = config.Buffer.GetRelevantContext();

			CombineAssertions("Initial buffer penetrations", () =>
			{
				AssertEquals("workflow1", 0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow1, context, Factory).Penetration);
				AssertEquals("workflow2", 0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow2, context, Factory).Penetration);
			});

			CombineAssertions("Initial values of FH_ReleaseDateTime", () =>
			{
				AssertEquals("workflow1", ZDateTime.UtcNow, workflow1.FH_ReleaseDateTime);
				AssertEquals("workflow2", ZDateTime.UtcNow, workflow2.FH_ReleaseDateTime);
			});

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();

				CombineAssertions("Before drag/drop, tickets should all be in the first time index", () =>
				{
					AssertEquals("workflow1", 0, BMSGUITestCase.FindTaskCardControl(componentControl, workflow1.Tasks.First()).Cell.TimeIndex);
					AssertEquals("workflow2", 0, BMSGUITestCase.FindTaskCardControl(componentControl, workflow2.Tasks.First()).Cell.TimeIndex);
				});

				var taskTicket = BMSGUITestCase.FindTaskCardControl(componentControl, task2);

				AssertEquals(0, taskTicket.Cell.TimeIndex);

				var firstCell = taskTicket.Cell;
				var secondCell = componentControl.ViewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);
				AssertEquals(true, DragTaskTicketToCell(taskTicket, componentControl, secondCell));

				CombineAssertions("After drag/drop, should move ticket for only the workflow that was dragged", () =>
				{
					AssertEquals(0, BMSGUITestCase.FindTaskCardControl(componentControl, workflow1.Tasks.First()).Cell.TimeIndex);
					AssertEquals(1, BMSGUITestCase.FindTaskCardControl(componentControl, workflow2.Tasks.First()).Cell.TimeIndex);
				});
			}

			CombineAssertions("After drag/drop, only buffer penetration of the workflow that was actually drag/dropped should be updated", () =>
			{
				AssertEquals("workflow1", 0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow1, context, Factory).Penetration);
				AssertEquals("workflow2", 0.125m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow2, context, Factory).Penetration);
			});

			CombineAssertions("After drag/drop, only FH_ReleaseDateTime of the workflow that was actually drag/dropped should be updated", () =>
			{
				AssertEquals("workflow1", ZDateTime.UtcNow, workflow1.FH_ReleaseDateTime);
				AssertEquals("workflow2", new ZDateTime(2015, 7, 10, 4, 0, 0), workflow2.FH_ReleaseDateTime);
			});
		}

		public void TestDragDrop_TaskCardTypeBucketToJobLevelWorkflowCardTypeBucket_ShouldMoveCorrectly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, Array.Empty<string>());
			system.FS_Name = "TDD";

			var bucket1 = BMSTestHelper.CreateBucket(system, "Waiting on Capacity");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Ready to Release");

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			system.ReleaseGroups.AddNew().FSG_GG_Group = releaseGroup.PK;

			var board = VisualBoardsTestHelper.CreateBoard(system, "My board");
			var section1 = BMSTestHelper.CreateBoardSection(bucket1, board);
			section1.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section1.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			section1.Row = 0;
			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board);
			section2.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section2.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section2.Row = 1;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "W1", bucket1, releaseGroupPK: releaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, description: "My First Task");
			var job = (ProcessJobHeader)workflow.ParentHeader;
			job.FH_GG_ReleaseGroup = releaseGroup.PK;

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControls = form.FindAll<BMComponentControl>();
				AssertEquals("Board should contain 2 sections.", 2, componentControls.Count());

				var taskSection = componentControls.SingleOrDefault(c => c.ViewModel.Row == 0);
				AssertNotNull("The board should contain a section at row 0.", taskSection);
				var jobLevelWorkflowSection = componentControls.SingleOrDefault(c => c.ViewModel.Row == 1);
				AssertNotNull("The board should contain a section at row 1.", taskSection);

				var taskTicket = BMSGUITestCase.FindTaskCardControl(taskSection, task);
				CombineAssertions("Before drag/drop, ticket should be in the first section", () =>
				{
					AssertNotNull("Section at row 0 should contain a task card.", taskTicket);
					AssertEquals("Section at row 0 should contain a task card.", 0, taskTicket.Cell.TimeIndex);
					AssertNull("Section at row 1 shouldn't contain a job card.", BMSGUITestCase.FindStackedTaskCardControl(jobLevelWorkflowSection, job));
				});

				AssertEquals(true, DragTaskTicketToCell(taskTicket, jobLevelWorkflowSection));

				var jobTicket = BMSGUITestCase.FindStackedTaskCardControl(jobLevelWorkflowSection, job);
				CombineAssertions("After drag/drop, there should only be a ticket in the second section.", () =>
				{
					AssertNull("Section at row 0 shouldn't contain a task card.", BMSGUITestCase.FindTaskCardControl(taskSection, task));
					AssertNotNull("Section at row 1 should contain a job card.", jobTicket);
					AssertEquals("Section at row 1 should contain a job card.", 0, jobTicket.Cell.TimeIndex);
				});
			}
		}

		public void TestDragDrop_TaskCardTypeBucketToWorkflowCardTypeBucket_ShouldMoveCorrectly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, Array.Empty<string>());
			system.FS_Name = "TDD";

			var bucket1 = BMSTestHelper.CreateBucket(system, "Waiting on Capacity");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Ready to Release");

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			system.ReleaseGroups.AddNew().FSG_GG_Group = releaseGroup.PK;

			var board = VisualBoardsTestHelper.CreateBoard(system, "My board");
			var section1 = BMSTestHelper.CreateBoardSection(bucket1, board);
			section1.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section1.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			section1.Row = 0;
			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board);
			section2.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section2.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section2.Row = 1;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "W1", bucket1, releaseGroupPK: releaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, description: "My First Task");

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;

				Application.DoEvents();

				var componentControls = form.FindAll<BMComponentControl>();
				AssertEquals("Board should contain 2 sections.", 2, componentControls.Count());

				var taskSection = componentControls.SingleOrDefault(c => c.ViewModel.Row == 0);
				AssertNotNull("The board should contain a section at row 0.", taskSection);
				var workflowSection = componentControls.SingleOrDefault(c => c.ViewModel.Row == 1);
				AssertNotNull("The board should contain a section at row 1.", taskSection);

				var taskTicket = BMSGUITestCase.FindTaskCardControl(taskSection, task);
				CombineAssertions("Before drag/drop, ticket should be in the first section", () =>
				{
					AssertNotNull("Section at row 0 should contain a card for the task.", taskTicket);
					AssertEquals("Section at row 0 should contain a card for the task.", 0, taskTicket.Cell.TimeIndex);
					AssertNull("Section at row 1 shouldn't contain a card for the workflow", BMSGUITestCase.FindStackedTaskCardControl(workflowSection, workflow));
				});

				AssertEquals(true, DragTaskTicketToCell(taskTicket, workflowSection));

				var workflowTicket = BMSGUITestCase.FindStackedTaskCardControl(workflowSection, workflow);
				CombineAssertions("After drag/drop, there should only be a ticket in the second section.", () =>
				{
					AssertNull("Section at row 0 shouldn't contain a task card.", BMSGUITestCase.FindTaskCardControl(taskSection, task));
					AssertNotNull("Section at row 1 should contain a workflow card.", workflowTicket);
					AssertEquals("Section at row 1 should contain a workflow card.", 0, workflowTicket.Cell.TimeIndex);
				});
			}
		}

		#endregion

		#region Implementation

		DisposableList disposables;
		VisualBoardTestConfig config;
		BMBoardSection section;

		protected override void SetUp()
		{
			base.SetUp();

			disposables = new DisposableList(new[] { DisableAsyncBehaviour() });

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			section = config.BucketSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section.SectionConfiguration.CellsPerSubsection = 10;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.DoNotStartBeforeDate;
			section.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
		}

		protected override void TearDown()
		{
			disposables.Dispose();

			base.TearDown();
		}

		#endregion
	}
}
