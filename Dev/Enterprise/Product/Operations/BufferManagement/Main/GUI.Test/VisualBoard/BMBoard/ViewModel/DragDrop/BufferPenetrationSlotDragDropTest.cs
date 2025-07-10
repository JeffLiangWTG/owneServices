using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestDate(2015, 7, 14)]
	class BufferPenetrationSlotDragDropTest : BMSTestCaseWithFactory
	{
		public void TestDragDropWithinBufferChannel_TaskTicket_WhenNoSecurity()
		{
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);

			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(ZDateTime.UtcNow, workflow.FH_ReleaseDateTime);

			Env.Security.WorkflowHeadersDragDropInBuffer.IsAllowed = false;
			Env.Security.WorkflowHeadersDragDropInBucket.IsAllowed = false; // Ensure the correct flag is checked.

			AssertEquals(false, TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent));
			AssertEquals(ZDateTime.UtcNow, workflow.FH_ReleaseDateTime);
			AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Workflow & Process -> Job Workflows -> Drag/Drop Tickets in Buffer Time Slots", UnitTestUserNotification.Instance.LastMessage.Text);

			Env.Security.WorkflowHeadersDragDropInBuffer.IsAllowed = true;

			var newValue = ZDateTime.UtcNow.AddDays(-4).AddHours(4);

			AssertEquals(true, TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent));
			AssertEquals(newValue, workflow.FH_ReleaseDateTime);
			AssertConfirmationMessageShown(newValue);
		}

		public void TestDragDropWithinBufferChannel_TaskTicket_When_TimeProgressionField_Is_WorkingTimeSinceStartable_ShouldNotMove()
		{
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			config.BufferSection.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.WorkingTimeSinceStartable;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);

			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(ZDateTime.UtcNow, workflow.FH_ReleaseDateTime);

			Env.Security.WorkflowHeadersDragDropInBuffer.IsAllowed = true;

			AssertEquals(false, TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent));
		}

		public void TestDragDropWithinBufferChannel_WhenEstimatesHaveIncreased_ShouldMoveToCellTicketWasDraggedInto()
		{
			var section = config.BufferSection;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			Factory.Save();

			AssertTasksInCell("Pre-condition: task should fall within the first time index (lowest cell)", section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(13, 2, new[] { task }));

			task.P9_EstimatedTimeToComplete = new ZInt(9 * 60 + 30).GetDateTimeFromMinutes(); // Make the remaining estimate 8 hours larger than the planned duration of 1.5 hours.
			Factory.Save();

			AssertTasksInCell("After upping the estimate, the task should now fall within the second time index (second-lowest cell) as its risk has increased", section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(12, 2, new[] { task }));

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var startingCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var draggedToCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 2 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(true, TryDragBetweenCells(viewModel, startingCell, draggedToCell, cardContent));
			AssertTasksInCell("After drag/dropping the ticket, the task should now fall within the third time index", section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(11, 2, new[] { task }));

			var newValue = ZDateTime.UtcNow.AddDays(-4).AddHours(4); // Only need to add enough time to move it to the second time index slot since the increased estimate causes it to move to the third time index slot where the user dragged it.
			AssertEquals(newValue, workflow.FH_ReleaseDateTime);
			AssertConfirmationMessageShown(newValue);
		}

		public void TestDragDropWithinBufferChannel_WhenEstimatesHaveDecreased_ShouldMoveToCellTicketWasDraggedInto()
		{
			var section = config.BufferSection;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			task.P9_EstDuration = new ZInt(9 * 60 + 30).GetDateTimeFromMinutes();
			workflow.FH_FC_CurrentComponent = config.Bucket.PK; // Move the workflow into the bucket so its planned duration re-calculates.

			Factory.Save();

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-1);

			Factory.Save();

			AssertTasksInCell("Pre-condition: task should fall within the second time index (second-lowest cell)", section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(12, 2, new[] { task }));

			task.P9_EstimatedTimeToComplete = new ZInt(60).GetDateTimeFromMinutes(); // Make the remaining estimate 8 hours lower than the planned duration of 9.5 hours.
			Factory.Save();

			AssertTasksInCell("After lowering the estimate, the task should fall within the first time index as its risk has reduced", section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(13, 2, new[] { task }));

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var startingCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var draggedToCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(true, TryDragBetweenCells(viewModel, startingCell, draggedToCell, cardContent));
			AssertTasksInCell("After drag/dropping the ticket, the task should now fall within the second time index", section, BMSTestHelper.CreateViewModel(section), new[] { task }, Tuple.Create(12, 2, new[] { task }));

			var newValue = ZDateTime.UtcNow.AddDays(-6).AddHours(6).AddMinutes(45); // Need to move the release datetime further back into the past, as the reduction in risk has made the workflow naturally move down the board.
			AssertEquals(newValue, workflow.FH_ReleaseDateTime);
			AssertConfirmationMessageShown(newValue);
		}

		public void TestDragDropWithinBufferChannel_WhenMultipleOverlaidBuffersExist_ShouldUseWorkflowCurrentComponentForCalculations()
		{
			var buffer1 = BMSTestHelper.CreateBuffer(config.System, "3 hour buffer", timespanMinutes: 60 * 3);
			var buffer2 = BMSTestHelper.CreateBuffer(config.System, "6 hour buffer", timespanMinutes: 60 * 6);
			var buffer3 = BMSTestHelper.CreateBuffer(config.System, "9 hour buffer", timespanMinutes: 60 * 9);

			var section = BMSTestHelper.CreateBoardSection(buffer1, cellsPerSubsection: 4); // One cell per zone
			BMSTestHelper.CreateAdditionalComponent(section, buffer2);
			BMSTestHelper.CreateAdditionalComponent(section, buffer3);

			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var workflowInBuffer1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow in 3 hour buffer", buffer1);
			var workflowInBuffer2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow in 6 hour buffer", buffer2);
			var workflowInBuffer3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow in 9 hour buffer", buffer3);

			var task1 = BMSTestHelper.CreateTask(workflowInBuffer1, description: "Task in 3 hour buffer");
			var task2 = BMSTestHelper.CreateTask(workflowInBuffer2, description: "Task in 6 hour buffer");
			var task3 = BMSTestHelper.CreateTask(workflowInBuffer3, description: "Task in 9 hour buffer");

			Factory.Save();

			var allTasks = new[] { task1, task2, task3 };
			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertTasksInCell("Pre-condition: all tickets are in the first cell", section, viewModel, allTasks, Tuple.Create(0, 2, allTasks));

			var sourceCell = viewModel.ComponentGrid[0, 2];
			var destinationCell = viewModel.ComponentGrid[1, 2];

			AssertEquals(3, sourceCell.Zone.Value);
			AssertEquals(2, destinationCell.Zone.Value);

			AssertEquals(true, TryDragBetweenCells(viewModel, sourceCell, destinationCell, new TaskCardContent(task1, viewModel)));
			AssertEquals(true, TryDragBetweenCells(viewModel, sourceCell, destinationCell, new TaskCardContent(task2, viewModel)));
			AssertEquals(true, TryDragBetweenCells(viewModel, sourceCell, destinationCell, new TaskCardContent(task3, viewModel)));

			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertTasksInCell("After drag/dropping each ticket, all should now be in the second cell", section, viewModel, allTasks, Tuple.Create(1, 2, allTasks));

			CombineAssertions("After drag/dropping each ticket, should adjust the last transfer time of each workflow according to its Current Component buffer, rather than using the board section's primary buffer for all tickets.", () =>
			{
				AssertEquals("workflowInBuffer1", new ZDateTime(2015, 7, 13, 6, 30, 0), workflowInBuffer1.FH_ReleaseDateTime);
				AssertEquals("workflowInBuffer2", new ZDateTime(2015, 7, 13, 5, 0, 0), workflowInBuffer2.FH_ReleaseDateTime);
				AssertEquals("workflowInBuffer3", new ZDateTime(2015, 7, 13, 3, 30, 0), workflowInBuffer3.FH_ReleaseDateTime);
			});
		}

		#region Implementation

		VisualBoardTestConfig config;

		ProcessJobHeader jobHeader;
		ProcessHeader workflow;
		ProcessTask task;

		readonly MultiActionButtonDialogWrapper<CrossChannelTaskAssignments> dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();

		bool TryDragBetweenCells(BMBoardSectionViewModel viewModel, CellContent sourceCell, CellContent destinationCell, ICardContent cardContent)
		{
			return new TicketDragDropHandler(Factory, viewModel, sourceCell, destinationCell, cardContent, dialogProvider, viewModel).TryMoveToDestinationCell();
		}

		void AssertConfirmationMessageShown(ZDateTime newFieldValueUTC)
		{
			var previousMessage = UnitTestUserNotification.Instance.PreviousMessages[0];

			AssertEquals($"This will change the Last Transfer Time of the workflow to {newFieldValueUTC.ToLocalBranchTime(Factory)}.", previousMessage.Text);
			AssertEquals("Change Time Slot", previousMessage.Caption);

			AssertEquals("BUFFER", previousMessage.Context.Context.ToUTF8());
			AssertEquals(new ZGuid("2c8b56e4-2f04-4394-868a-cd64a5d40e0c"), previousMessage.Context.DialogIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Dragee", config.Buffer);
			task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
		}

		#endregion
	}
}
