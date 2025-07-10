using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
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
	class TimeSlotTicketDragDropHandlerTest_EarliestStartDate : BucketTicketDragDropHandlerTest_TimeSlotTestCase
	{
		protected override string TimeField => TimeProgressionFieldList.Codes.DoNotStartBeforeDate;
		protected override string TimeProgressionMode => TimeProgressionModeList.Codes.Due;

		protected override ZPropertyInfo GetDatePropertyInfo(ProcessHeader workflow) => workflow.FH_DoNotStartBeforeDateInfo;

		protected override ZDateTime GetInitialValueToPlaceInFirstSlot() => ZDateTime.UtcNow.AddHours(4);

		protected override ZDateTime GetNewValueAfterDraggingToSecondSlot() => ZDateTime.UtcNow.AddDays(1).AddHours(4);
	}

	class TimeSlotTicketDragDropHandlerTest_AgreedDeliveryDate : BucketTicketDragDropHandlerTest_TimeSlotTestCase
	{
		protected override string TimeField => TimeProgressionFieldList.Codes.AgreedDeliveryDate;
		protected override string TimeProgressionMode => TimeProgressionModeList.Codes.Due;

		protected override ZPropertyInfo GetDatePropertyInfo(ProcessHeader workflow) => workflow.FH_AgreedDeliveryDateInfo;

		protected override ZDateTime GetInitialValueToPlaceInFirstSlot() => ZDateTime.UtcNow.AddHours(4);

		protected override ZDateTime GetNewValueAfterDraggingToSecondSlot() => ZDateTime.UtcNow.AddDays(1).AddHours(4);
	}

	class TimeSlotTicketDragDropHandlerTest_LastTransferTime : BucketTicketDragDropHandlerTest_TimeSlotTestCase
	{
		protected override string TimeField => TimeProgressionFieldList.Codes.TransferTime;
		protected override string TimeProgressionMode => TimeProgressionModeList.Codes.Age;

		protected override ZPropertyInfo GetDatePropertyInfo(ProcessHeader workflow) => workflow.FH_ReleaseDateTimeInfo;

		protected override ZDateTime GetInitialValueToPlaceInFirstSlot() => ZDateTime.UtcNow.AddDays(-1).AddHours(4);

		protected override ZDateTime GetNewValueAfterDraggingToSecondSlot() => ZDateTime.UtcNow.AddDays(-4).AddHours(4);
	}

	class TimeSlotTicketDragDropHandlerTest_WorkflowCreationTime : BucketTicketDragDropHandlerTest_TimeSlotTestCase
	{
		protected override string TimeField => TimeProgressionFieldList.Codes.CreateTime;
		protected override string TimeProgressionMode => TimeProgressionModeList.Codes.Age;

		protected override ZPropertyInfo GetDatePropertyInfo(ProcessHeader workflow) => workflow.FH_SystemCreateTimeUtcInfo;

		protected override ZDateTime GetInitialValueToPlaceInFirstSlot() => ZDateTime.UtcNow;

		protected override ZDateTime GetNewValueAfterDraggingToSecondSlot() => ZDateTime.UtcNow.AddDays(-4).AddHours(4);

		protected override bool DragDropShouldChangeDate => false;
	}

	[TestDate(2015, 7, 14)]
	abstract class BucketTicketDragDropHandlerTest_TimeSlotTestCase : BMSTestCaseWithFactory
	{
		public void TestDragDropWithinChannel_JobTickets()
		{
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);
			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			GetDatePropertyInfo(jobHeader).Value = initialValue;
			GetDatePropertyInfo(workflow).Value = initialValue;

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(jobHeader, task, viewModel);

			AssertEquals(initialValue, GetDatePropertyInfo(jobHeader).Value);

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(newValue);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_WhenNoSecurity()
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);

			Env.Security.WorkflowHeadersDragDropInBucket.IsAllowed = false;
			Env.Security.WorkflowHeadersDragDropInBuffer.IsAllowed = false; // Ensure the correct flag is checked.

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			AssertEquals(false, result);
			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);

			if (DragDropShouldChangeDate)
			{
				AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Workflow & Process -> Job Workflows -> Drag/Drop Tickets in Bucket Time Slots", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.WorkflowHeadersDragDropInBucket.IsAllowed = true;

				result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(newValue);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_Override_TimeProgressionField_By_WorkingTimeSinceStartable_ShouldNotMove()
		{
			config.BucketSection.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.WorkingTimeSinceStartable;
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);

			Env.Security.WorkflowHeadersDragDropInBucket.IsAllowed = true;

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			AssertEquals(false, result);
			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_ShouldUpdateRelevantDateValue()
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(newValue);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_WhenDateBeyondFirstTimeSlotBoundary()
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var firstSlotValue = GetInitialValueToPlaceInFirstSlot();
			var startingValue = firstSlotValue.AddDays(TimeProgressionMode == TimeProgressionModeList.Codes.Age ? 10 : -10);

			GetDatePropertyInfo(workflow).Value = startingValue;

			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(startingValue, GetDatePropertyInfo(workflow).Value);

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(newValue);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(firstSlotValue, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(startingValue, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_WithOverdueCells_WhenDateBeyondZeroTimeSlotBoundary()
		{
			config.BucketSection.SectionConfiguration.MaxOverdueSlots = 2;
			config.BucketSection.Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var zeroIndexCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var negativeOneIndexCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == -1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var startingValue = GetInitialValueToPlaceInFirstSlot();
			var negativeOneIndexValue = TimeProgressionMode == TimeProgressionModeList.Codes.Age ? startingValue.AddDays(1) : startingValue.AddDays(-1);

			AssertEquals(startingValue, GetDatePropertyInfo(workflow).Value);

			var result = TryDragBetweenCells(viewModel, zeroIndexCell, negativeOneIndexCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(negativeOneIndexValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(negativeOneIndexValue);

				result = TryDragBetweenCells(viewModel, negativeOneIndexCell, zeroIndexCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(startingValue, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(startingValue, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_WithOverriddenAgingDept()
		{
			var otherDept = Factory.NewWithValidTestData<GlbDepartment>();

			config.BucketBoard.MB_GE_AgingDepartment = otherDept.PK;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var initialValue = TimeProgressionMode == TimeProgressionModeList.Codes.Age ? ZDateTime.UtcNow.AddHours(-4) : ZDateTime.UtcNow.AddHours(4);
			var newValue = TimeProgressionMode == TimeProgressionModeList.Codes.Age ? initialValue.AddHours(-8) : initialValue.AddHours(8);

			GetDatePropertyInfo(workflow).Value = initialValue;
			Factory.Save();

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(newValue);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_AnsweringNoToConfirmation_ShouldLeaveRelevantDateValue()
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			AssertEquals(false, result);
			AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);

			if (DragDropShouldChangeDate)
			{
				AssertConfirmationMessageShown(newValue);
			}
		}

		public void TestDragDropWithinNonChanneledSection_JobTicket_AnsweringNoToConfirmation_ShouldLeaveRelevantDateValue()
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);
			var initialValue = GetInitialValueToPlaceInFirstSlot();

			GetDatePropertyInfo(jobHeader).Value = initialValue;

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(jobHeader, task, viewModel);

			AssertEquals(initialValue, GetDatePropertyInfo(jobHeader).Value);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			AssertEquals(false, result);
			AssertEquals(initialValue, GetDatePropertyInfo(jobHeader).Value);
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_DateInheritedFromJob_ShouldUpdateRelevantDateValue()
		{
			GetDatePropertyInfo(workflow).Value = ZDateTime.Empty;
			GetDatePropertyInfo(jobHeader).Value = GetInitialValueToPlaceInFirstSlot();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);
			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(initialValue, GetDatePropertyInfo(jobHeader).Value);

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(jobHeader).Value);
				AssertConfirmationMessageShown(newValue, dateInheritedFromJobHeader: true);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(initialValue, GetDatePropertyInfo(jobHeader).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(initialValue, GetDatePropertyInfo(jobHeader).Value);
			}
		}

		public void TestDragDropWithinNonChanneledSection_WorkflowTicket_EmptyInitialDate_ShouldUpdateRelevantDateValueToCurrentTimePlusOffset()
		{
			GetDatePropertyInfo(workflow).Value = ZDateTime.Empty;

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel);

			var initialValue = GetInitialValueToPlaceInFirstSlot();
			var newValue = GetNewValueAfterDraggingToSecondSlot();

			AssertEquals(ZDateTime.Empty, GetDatePropertyInfo(workflow).Value);

			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(newValue, GetDatePropertyInfo(workflow).Value);
				AssertConfirmationMessageShown(newValue);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(initialValue, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(ZDateTime.Empty, GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinChanneledSection_TaskTicket_SameChannel_ShouldUpdateRelevantDateValue()
		{
			var otherResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BucketSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BucketSection, ChannelTypeList.Codes.Resource, otherResource.PK);

			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);

			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(GetNewValueAfterDraggingToSecondSlot(), GetDatePropertyInfo(workflow).Value);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropWithinChanneledSection_TaskTicket_DifferentChannel_ShouldLeaveRelevantDateValueAndJustReAssignTask()
		{
			var otherResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BucketSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BucketSection, ChannelTypeList.Codes.Resource, otherResource.PK);

			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == otherResource.PK);

			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			AssertEquals(true, result);
			AssertEquals(otherResource.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);

			result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

			AssertEquals(true, result);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
		}

		public void TestDragDropBetweenSections_WorkflowTicket_ShouldLeaveRelevantDateValueAndJustChangeComponent()
		{
			var otherSection = BMSTestHelper.CreateBoardSection(config.Buffer, config.BucketBoard);
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			Factory.Save();

			var viewModel1 = BMSTestHelper.CreateViewModel(config.BucketSection);
			var viewModel2 = BMSTestHelper.CreateViewModel(config.BufferSection);

			var firstCell = viewModel2.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel1.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel2);

			if (TimeField == TimeProgressionFieldList.Codes.TransferTime)
			{
				AssertEquals(ZDateTime.UtcNow, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}

			var result = TryDragBetweenCells(viewModel1, firstCell, secondCell, cardContent);

			AssertEquals(true, result);
			AssertEquals(config.Bucket.PK, workflow.FH_FC_CurrentComponent);

			if (TimeField == TimeProgressionFieldList.Codes.TransferTime)
			{
				AssertEquals(ZDateTime.UtcNow, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}

			result = TryDragBetweenCells(viewModel2, secondCell, firstCell, cardContent);

			AssertEquals(true, result);
			AssertEquals(config.Buffer.PK, workflow.FH_FC_CurrentComponent);

			if (TimeField == TimeProgressionFieldList.Codes.TransferTime)
			{
				AssertEquals(ZDateTime.UtcNow, GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}
		}

		public void TestDragDropBetweenSections_WorkflowTicket_AnsweringNo()
		{
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			var viewModel1 = BMSTestHelper.CreateViewModel(config.BucketSection);

			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			var viewModel2 = BMSTestHelper.CreateViewModel(config.BufferSection);

			var firstCell = viewModel2.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var secondCell = viewModel1.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel2);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var result = new TicketDragDropHandler(Factory, viewModel1, firstCell, secondCell, cardContent, dialogProvider, viewModel2).TryMoveToDestinationCell();
			AssertEquals(false, result);
			AssertEquals(config.Buffer.PK, workflow.FH_FC_CurrentComponent);

			var previousMessage = UnitTestUserNotification.Instance.PreviousMessages[0];
			AssertEquals("Move workflow", previousMessage.Caption);
		}

		public void TestDragDropWithinChanneledSection_JobWorkflow_SameChannel_ShouldUpdateRelevantDateValue()
		{
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var otherResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BucketSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BucketSection, ChannelTypeList.Codes.Resource, otherResource.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection);

			var firstCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);
			var secondCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1 && c.Channel.EntityPK == GlbStaff.CurrentUser.PK);

			var cardContent = new TaskCardContent(task, viewModel);

			AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			var result = TryDragBetweenCells(viewModel, firstCell, secondCell, cardContent);

			if (DragDropShouldChangeDate)
			{
				AssertEquals(true, result);
				AssertEquals(GetNewValueAfterDraggingToSecondSlot(), GetDatePropertyInfo(workflow).Value);

				result = TryDragBetweenCells(viewModel, secondCell, firstCell, cardContent);

				AssertEquals(true, result);
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}
			else
			{
				AssertEquals(false, result);
				AssertEquals(GetInitialValueToPlaceInFirstSlot(), GetDatePropertyInfo(workflow).Value);
			}
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

		void AssertConfirmationMessageShown(ZDateTime newFieldValueUTC, bool dateInheritedFromJobHeader = false)
		{
			var previousMessage = UnitTestUserNotification.Instance.PreviousMessages[0];
			var workflowName = dateInheritedFromJobHeader ? "job-level workflow" : "workflow";

			AssertEquals($"This will change the {FieldName} of the {workflowName} to {MasterFiles.Business.Extensions.ToLocalBranchTime(newFieldValueUTC, Factory)}.", previousMessage.Text);
			AssertEquals("Change Time Slot", previousMessage.Caption);

			AssertEquals(TimeField, previousMessage.Context.Context.ToUTF8());
			AssertEquals(new ZGuid("a1f3ce29-5c13-4fc7-a414-87f3db06a6c7"), previousMessage.Context.DialogIdentifier);
		}

		string FieldName
		{
			get
			{
				switch (TimeField)
				{
					case TimeProgressionFieldList.Codes.AgreedDeliveryDate:
						return "Agreed Delivery Date";

					case TimeProgressionFieldList.Codes.DoNotStartBeforeDate:
						return "Earliest Start Date";

					case TimeProgressionFieldList.Codes.TransferTime:
						return "Last Transfer Time";

					default:
						throw new ArgumentException("Invalid TimeField: " + TimeField, nameof(TimeField));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BucketSection.SectionConfiguration.TimeProgressionMode = TimeProgressionMode;
			config.BucketSection.SectionConfiguration.TimeField = TimeField;
			config.BucketSection.SectionConfiguration.CellsPerSubsection = 10;
			config.BucketSection.SectionConfiguration.TimePerCell = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes(); // One day per slot.
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Dragee");
			task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			GetDatePropertyInfo(workflow).Value = GetInitialValueToPlaceInFirstSlot();

			Factory.Save();
		}

		protected abstract string TimeField { get; }
		protected abstract string TimeProgressionMode { get; }

		protected abstract ZPropertyInfo GetDatePropertyInfo(ProcessHeader workflow);
		protected abstract ZDateTime GetInitialValueToPlaceInFirstSlot();
		protected abstract ZDateTime GetNewValueAfterDraggingToSecondSlot();

		protected virtual bool DragDropShouldChangeDate => true;

		#endregion
	}
}
