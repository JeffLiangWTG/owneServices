using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TaskCardDetailControlTest : BMSGUITestCase
	{
		#region Customised Layout

		public void TestCustomisedLayout_MinimumAndMaximumSize()
		{
			using (var taskCardDetailControl = new TaskCardDetailControl())
			{
				AssertEquals("TaskCardDetailControl Minimum size should be 25x25", taskCardDetailControl.MinimumSize, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 25, true));
				AssertEquals("TaskCardDetailControl Maximum size should be 800x600", taskCardDetailControl.MaximumSize, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true));
			}
		}

		public void TestCustomisedLayout_ShouldUseJobTypeOverride()
		{
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			layout.BackgroundColor = ColorList.NameFromColor(Color.PeachPuff);
			layout.FM_JobType = "INQ";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var detailedTicket = FindOrShowDetailedTicket(form, task);
				AssertBackColor("There is no customised layout for task cards yet", detailedTicket, Color.White);

				var link = BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);
				Factory.Save();

				form.ReloadBoard();

				detailedTicket = FindOrShowDetailedTicket(form, task);
				AssertBackColor("Customised layout is configured for all job types", detailedTicket, Color.PeachPuff);

				link.FML_JobType = "WKI";
				Factory.Save();

				form.ReloadBoard();

				detailedTicket = FindOrShowDetailedTicket(form, task);
				AssertBackColor("Customised layout is configured for a different job type", detailedTicket, Color.White);

				link.FML_JobType = "ORG";
				Factory.Save();

				form.ReloadBoard();

				detailedTicket = FindOrShowDetailedTicket(form, task);
				AssertBackColor("Customised layout is configured for the task's job type", detailedTicket, Color.PeachPuff);
			}
		}

		static void AssertBackColor(string message, TaskCardDetailControl control, Color expectedColor)
		{
			var backColor = control.Controls.OfType<ZUserControl>().Single().BackColor;

			AssertColorEquals(message, expectedColor, backColor);
		}

		#endregion

		#region Close

		public void TestClose_SaveChanges()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var newFactory = Factory.CreateNewFactory();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var parentCard = new TaskCardControl(task, viewModel))
			using (var detailedCard = new TaskCardDetailControl(parentCard.CardContent.GetTask(newFactory), parentCard.CardContent.GetWorkflow(newFactory), parentCard, BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard), sectionViewModel))
			{
				form.Controls.Add(detailedCard);
				task = detailedCard.ProcessTask;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task.P9_GS_NKAssignedStaffMember = "E";
				Assert(task.HasChanges);

				var userNotificationMock = new Mock<IUserNotification>();
				userNotificationMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.Yes);
				detailedCard.UserNotification = userNotificationMock.Object;
				detailedCard.Close();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				Assert(detailedCard.IsDisposed);
			}
		}

		public void TestClose_SaveChangesForVoteUpDown()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var newFactory = Factory.CreateNewFactory();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var parentCard = new TaskCardControl(task, viewModel))
			using (var detailedCard = new TaskCardDetailControl(task, task.GetProcessHeader(), parentCard, BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard), sectionViewModel))
			{
				form.Controls.Add(detailedCard);
				task.GetProcessHeader().NudgeUp();
				detailedCard.Close();

				AssertEquals(1, (int)task.ProcessHeader.FH_VoteUpDownAmount);
			}
		}

		public void TestClose_CancelChanges()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var newFactory = Factory.CreateNewFactory();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var parentCard = new TaskCardControl(task, viewModel))
			using (var detailedCard = new TaskCardDetailControl(task, task.GetProcessHeader(), parentCard, BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard), sectionViewModel))
			{
				form.Controls.Add(detailedCard);
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				Assert(detailedCard.ProcessTask.HasChanges);

				var userNotificationMock = new Mock<IUserNotification>();
				userNotificationMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(),
					It.IsAny<ZDialogResult>())).Returns(ZDialogResult.No);
				detailedCard.UserNotification = userNotificationMock.Object;
				detailedCard.Close();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				Assert(!task.HasChanges);
				Assert(detailedCard.IsDisposed);
			}
		}

		public void TestClose_CancelClose()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var newFactory = Factory.CreateNewFactory();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var parentCard = new TaskCardControl(task, viewModel))
			using (var detailedCard = new TaskCardDetailControl(parentCard.CardContent.GetTask(newFactory), parentCard.CardContent.GetWorkflow(newFactory), parentCard, BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard), sectionViewModel))
			{
				form.Controls.Add(detailedCard);
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				Assert(detailedCard.ProcessTask.HasChanges);

				var userNotificationMock = new Mock<IUserNotification>();
				userNotificationMock
					.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
						It.IsAny<ZDialogResult>()))
					.Returns(ZDialogResult.Cancel);
				detailedCard.UserNotification = userNotificationMock.Object;
				detailedCard.Close();
				AssertEquals("Should have kept changes", ProcessTaskStatusCodeList.Codes.Assigned, detailedCard.ProcessTask.P9_Status);
				AssertEquals("Should not have persisted changes", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
				Assert(!detailedCard.IsDisposed);
			}
		}

		public void TestErrorsActuallyAppear()
		{
			var section = config.BucketSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var task = CreateTask(workflow, string.Empty, 0);

			Factory.Save();

			AssertEquals(false, workflow.HasOpenPrerequisites);

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var taskCard = new TaskCardControl(new WorkflowCardContent(workflow, task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			using (var detailedCard = new TaskCardDetailControl(task, task.GetProcessHeader(), taskCard, BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard), viewModel))
			{
				form.Controls.Add(detailedCard);

				form.Show();

				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

				var textBox = detailedCard.FindAll<ZDateEdit>().Single(x => x.BindTo == "EstimatedHandoverTimeLocal");
				var anotherTextBox = detailedCard.FindAll<ZTextBox>().Single(x => x.BindTo == "P9_Description");

				textBox.Focus();
				textBox.Text = "";

				detailedCard.ProcessTask.Validation.ValidateAll();

				form.Focus();

				anotherTextBox.Focus();

				textBox.Focus();
				textBox.Text = "1";

				detailedCard.ProcessTask.Validation.ValidateAll();

				form.Focus();

				anotherTextBox.Focus();

				Application.DoEvents();

				var dateTextBox = textBox.Controls.OfType<ZTextBox>().First();

				AssertEquals("ffffd7d7", dateTextBox.BackColor.Name);
			}
		}

		public void TestParentCardDispose_ShouldUnhookWhenDetailedCardCloses()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var newFactory = Factory.CreateNewFactory();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var parentCard = new TaskCardControl(task, viewModel))
			{
				var detailedCard = CreateAndDisposeDetailedCard(parentCard);

				GC.Collect(); // Test case - go away
				GC.WaitForFullGCComplete();

				AssertNull(detailedCard.Target);
			}
		}

		public void TestUpdateStatusWhenTaskAlreadyAssigned()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "y'all up for completion?", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var staff = BMSTestHelper.CreateStaff(Factory, "TST");
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(channel, Factory);

			AssertEquals(GlbStaff.CurrentUser.GS_Code, cell.Channel.ChannelEntityCode);

			using (var form = new ZForm())
			using (var parent = new TaskCardControl(new TaskCardContent(task, sectionViewModel), sectionViewModel, cell, canUseBitmapCache: true))
			using (var detailedCard = new TaskCardDetailControl(task, workflow, parent, customisation, sectionViewModel))
			{
				form.Controls.Add(detailedCard);
				form.Show();

				AssertEquals(3, detailedCard.FindAll<GenericStatusChangeButton>().Count());

				var control = detailedCard.FindSingleOrDefault<GenericStatusChangeButton>(b => b.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				control.PerformClick_ForTest();

				AssertEquals(string.Format("This task is already assigned to {0}.", staff.GS_Code), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		static WeakReference CreateAndDisposeDetailedCard(TaskCardControl taskCard)
		{
			var factory = new BusinessObjectFactory();
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(factory);
			var detailedCard = new TaskCardDetailControl(taskCard.CardContent.GetTask(factory), taskCard.CardContent.GetWorkflow(factory), taskCard, BMControlCustomisation.GetNewDefaultCardLayout(factory, CustomisedControlTypeList.Codes.DetailedCard), sectionViewModel);
			detailedCard.Dispose();

			return new WeakReference(detailedCard);
		}

		#endregion

		#region Save

		public void TestSave_ShouldSaveTaskFactory()
		{
			var task = GetSavedTask();
			var section = config.BucketSection;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				var detailedTicketTask = detailedTicket.ProcessTask;

				detailedTicketTask.P9_GS_NKAssignedStaffMember = "E";
				detailedTicketTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				AssertEquals(true, detailedTicketTask.HasChanges);
				AssertEquals("Pre-condition: task in original factory has not been updated yet", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);

				detailedTicket.Save_ForTest();

				AssertEquals(false, detailedTicketTask.HasChanges);
				AssertEquals("Should have updated original task via DataRefreshBus", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
				AssertEquals("Saving the detailed ticket should also close and dispose it", true, detailedTicket.IsDisposed);
			}
		}

		public void TestSave_ShouldUpdateParentNote()
		{
			var task = GetSavedTask();
			var section = config.BucketSection;
			var newFactory = Factory.CreateNewFactory();

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				var parentCardStartingHeight = ticket.Height;

				form.Controls.Add(detailedTicket);
				detailedTicket.ProcessTask.P9_CardNote = "Mai Note";
				Assert(detailedTicket.ProcessTask.HasChanges);

				AssertEquals(string.Empty, task.VisualBoardNoteText);
				detailedTicket.Save_ForTest();

				AssertEquals("Mai Note", task.VisualBoardNoteText);
				Assert(!task.HasChanges);
				Assert(detailedTicket.IsDisposed);
			}
		}

		public void TestSave_DistinctWorkflows_NoArgumentException()
		{
			var capability = CreateCapability("DCP", "DummyCapability");
			var staff = CreateStaffInCurrentBranchDept("LOL", "Living Out Loud", capability);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = CreateTask(workflow, "", 60, taskStatus: "OPN", capability: capability);
			var task2 = CreateTask(workflow, "", 60, taskStatus: "OPN", capability: capability);
			var task3 = CreateTask(workflow, "", 60, taskStatus: "OPN", capability: capability);
			var section = config.BucketSection;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = GetAndShowVisualBoardForm(section))
			{
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals(ZString.Empty, task2.P9_GS_NKAssignedStaffMember);
				AssertEquals(ZString.Empty, task3.P9_GS_NKAssignedStaffMember);

				var ticket = FindTaskCardControl(form, task1);
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				var control = detailedTicket.FindAll<CapabilityAssignmentButton>().Single();
				control.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkfloWithForClaimOrAssign;
				control.PerformClick();

				detailedTicket.Save_ForTest();
				Application.DoEvents();

				AssertEquals(staff.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals(staff.GS_Code, task2.P9_GS_NKAssignedStaffMember);
				AssertEquals(staff.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestSave_HandleConcurrencyError()
		{
			var task = GetSavedTask();
			var section = config.BucketSection;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				detailedTicket.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				taskInNewFactory.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

				newFactory.Save();
				detailedTicket.Save_ForTest();

				AssertEquals("Another user has modified this task. Please make your changes and then try saving again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Status Change (play/pause/close buttons)

		#region Auto-Save basic behaviour

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviourOnStatusButtonSet_ShouldSaveAndCloseCardAutomatically()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Working);
		}

		public void TestClickSuspendButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviourOnStatusButtonSet_ShouldSaveAndCloseCardAutomatically()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestClickCloseTaskButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviourOnStatusButtonSet_ShouldSaveAndCloseCardAutomatically()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Closed);
		}

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviourOnSingleButton_ShouldSaveAndCloseCardAutomatically()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(StaticControlTypeList.Codes.WorkingStatusButton, ProcessTaskStatusCodeList.Codes.Working);
		}

		public void TestClickSuspendButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviourOnSingleButton_ShouldSaveAndCloseCardAutomatically()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(StaticControlTypeList.Codes.SuspendStatusButton, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestClickCloseTaskButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviourOnSingleButton_ShouldSaveAndCloseCardAutomatically()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(StaticControlTypeList.Codes.CompletedStatusButton, ProcessTaskStatusCodeList.Codes.Closed);
		}

		void AssertClickButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_ShouldSaveAndCloseCardAutomatically(string controlTypeForCustomisation, string expectedStatusToChangeTo)
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var buttonCustomisation = BMSTestHelper.CreateStaticControlCustomisation(customisation, controlTypeForCustomisation, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			buttonCustomisation.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;
			buttonCustomisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;
			buttonCustomisation.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;

			var task = GetSavedTask();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
				var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == expectedStatusToChangeTo);
				button.PerformClick_ForTest();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Clicking the button should have closed the ticket automatically. SAD!", true, detailedTicketControl.IsDisposed);
			}

			task.Reload();
			AssertEquals($"The task should have been set to {expectedStatusToChangeTo} and saved automatically when the button was clicked. SAD!", expectedStatusToChangeTo, task.P9_Status);
			AssertNull("Auto-Save behaviour should not open the job form when the button is clicked. SAD!", BMSFormTestHelper.GetOpenForms<ZTemplateForm>().FirstOrDefault());
		}

		#endregion

		#region Manual basic behaviour

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesManualBehaviourOnStatusButtonSet_ShouldUpdateStatusButNotSaveOrCloseCard()
		{
			AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Working);
		}

		public void TestClickSuspendButton_WhenCustomisedLayoutSpecifiesManualBehaviourOnStatusButtonSet_ShouldUpdateStatusButNotSaveOrCloseCard()
		{
			AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestClickCloseTaskButton_WhenCustomisedLayoutSpecifiesManualBehaviourOnStatusButtonSet_ShouldUpdateStatusButNotSaveOrCloseCard()
		{
			AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Closed);
		}

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesManualBehaviourOnSingleButton_ShouldUpdateStatusButNotSaveOrCloseCard()
		{
			AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(StaticControlTypeList.Codes.WorkingStatusButton, ProcessTaskStatusCodeList.Codes.Working);
		}

		public void TestClickSuspendButton_WhenCustomisedLayoutSpecifiesManualBehaviourOnSingleButton_ShouldUpdateStatusButNotSaveOrCloseCard()
		{
			AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(StaticControlTypeList.Codes.SuspendStatusButton, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		public void TestClickCloseTaskButton_WhenCustomisedLayoutSpecifiesManualBehaviourOnSingleButton_ShouldUpdateStatusButNotSaveOrCloseCard()
		{
			AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(StaticControlTypeList.Codes.CompletedStatusButton, ProcessTaskStatusCodeList.Codes.Closed);
		}

		void AssertButton_WhenCustomisedLayoutSpecifiesManualBehaviour_ShouldUpdateStatusButNotSaveOrCloseCard(string controlTypeForCustomisation, string expectedStatusThatWouldBeAppliedIfSaveWereClicked)
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var buttonCustomisation = BMSTestHelper.CreateStaticControlCustomisation(customisation, controlTypeForCustomisation, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			buttonCustomisation.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			buttonCustomisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			buttonCustomisation.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;

			var task = GetSavedTask();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
				var playButton = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == expectedStatusThatWouldBeAppliedIfSaveWereClicked);
				playButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Clicking the button should not have closed the ticket automatically, because it doesn't have a customisation that tells it to do so. SAD!", false, detailedTicketControl.IsDisposed);
			}

			task.Reload();
			AssertEquals($"The task should not have been set to {expectedStatusThatWouldBeAppliedIfSaveWereClicked} because it shouldn't have been saved automatically when the button was clicked. SAD!", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertNull("Manual behaviour should not open the job form when the button is clicked. SAD!", BMSFormTestHelper.GetOpenForms<ZTemplateForm>().FirstOrDefault());
		}

		#endregion

		#region Open Job behaviour

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviourOnStatusButtonSet_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Working,
				(customisation) => customisation.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob);
		}

		public void TestClickSuspendButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviourOnStatusButtonSet_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Suspended,
				(customisation) => customisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob);
		}

		public void TestClickCloseTaskButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviourOnStatusButtonSet_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(StaticControlTypeList.Codes.StatusButtons, ProcessTaskStatusCodeList.Codes.Closed,
				(customisation) => customisation.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob);
		}

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviourOnSingleButton_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(StaticControlTypeList.Codes.WorkingStatusButton, ProcessTaskStatusCodeList.Codes.Working,
				(customisation) => customisation.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob);
		}

		public void TestClickSuspendButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviourOnSingleButton_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(StaticControlTypeList.Codes.SuspendStatusButton, ProcessTaskStatusCodeList.Codes.Suspended,
				(customisation) => customisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob);
		}

		public void TestClickCloseTaskButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviourOnSingleButton_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm()
		{
			AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(StaticControlTypeList.Codes.CompletedStatusButton, ProcessTaskStatusCodeList.Codes.Closed,
				(customisation) => customisation.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob);
		}

		void AssertClickButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_ShouldSaveAndCloseCardAutomaticallyAndOpenJobForm(string controlTypeForCustomisation, string expectedStatusToChangeTo, Action<StaticControlCustomisation> changeBehaviourAction)
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var buttonCustomisation = BMSTestHelper.CreateStaticControlCustomisation(customisation, controlTypeForCustomisation, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			changeBehaviourAction(buttonCustomisation);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK, currentComponent: config.Bucket);
			var task1 = BMSTestHelper.CreateTask(workflow, description: "Task 1", sequence: 1, staffCode: GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow, description: "Task 2", sequence: 2, staffCode: GlbStaff.CurrentUser.GS_Code);

			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>(x => x.CardContent.TaskIdentifier == task2.PK);
				ticket.ShowDetailedCard();

				ZOrganisationsForm jobForm = null;

				try
				{
					var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
					var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == expectedStatusToChangeTo);
					button.PerformClick_ForTest();
					Application.DoEvents();

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Clicking the button should have closed the ticket automatically. SAD!", true, detailedTicketControl.IsDisposed);

					task2.Reload();
					AssertEquals($"The task should have been set to {expectedStatusToChangeTo} and saved automatically when the button was clicked. SAD!", expectedStatusToChangeTo, task2.P9_Status);

					jobForm = BMSFormTestHelper.GetOpenForms<ZOrganisationsForm>().FirstOrDefault();
					AssertNotNull("The job form should have been opened because that's the mode we selected for the buttons. SAD!", jobForm);

					var workflowTabPage = jobForm.FindSingle<ZWorkflowTabPage>();
					var selectedTab = ((TabControl)workflowTabPage.Parent).SelectedTab;
					AssertEquals("The system should have automatically opened the workflow tab. SAD!", workflowTabPage, selectedTab);

					var taskGrid = workflowTabPage.FindSingle<TaskDetailsUserControl.TaskCustomZGrid>();
					AssertEquals("The relevant task should be automatically selected. SAD!", "Task 2", ((ProcessTask)taskGrid.ListManager.Current)?.P9_Description);
				}
				finally
				{
					jobForm?.Dispose();
				}
			}
		}

		#endregion

		#region Validation

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesAutoSaveBehaviour_WithValidationErrors_ShouldNotSaveAndNotCloseCardAutomatically_AndShowErrors()
		{
			var task = GetSavedTask();
			var section = config.BucketSection;

			Factory.Save();

			var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertHasError("Precondition", loadedTask.P9_GG_AssignedGroupInfo, "For working and closed tasks, you must specify either an assigned resource or group.");
			AssertHasError("Precondition", loadedTask.P9_GS_NKAssignedStaffMemberInfo, "For working and closed tasks, you must specify either an assigned resource or group.");
			AssertHasError("Precondition", loadedTask.P9_StatusInfo, "For working and closed tasks, you must specify either an assigned resource or group.");

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
				var playButton = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				playButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(@"Errors on Task T00001000 Undefined - You can modify this in the System Reg [Undefined - You can modify this in the System Reg]:
Error - P9_GG_AssignedGroup: For working and closed tasks, you must specify either an assigned resource or group.
Error - P9_GS_NKAssignedStaffMember: For working and closed tasks, you must specify either an assigned resource or group.
Error - P9_Status: For working and closed tasks, you must specify either an assigned resource or group.

".StripTaskIds(), UnitTestUserNotification.Instance.LastMessage.Text.StripTaskIds());
				AssertEquals("Clicking Play should not have closed the ticket automatically because there are validation errors. SAD!", false, detailedTicketControl.IsDisposed);
			}

			task.Reload();
			AssertEquals("The task should not have been set to WRK and saved automatically when the Play button was clicked because there were validation errors. SAD!", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
		}

		public void TestClickPlayButton_WhenCustomisedLayoutSpecifiesOpenJobBehaviour_WithValidationErrors_ShouldNotSaveAndNotCloseCardAutomatically_AndShowErrors_AndNotShowJobForm()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var buttonCustomisation = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.StatusButtons, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			buttonCustomisation.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob;
			buttonCustomisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob;
			buttonCustomisation.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob;

			var task = GetSavedTask();
			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertHasError("Precondition", loadedTask.P9_GG_AssignedGroupInfo, "For working and closed tasks, you must specify either an assigned resource or group.");
			AssertHasError("Precondition", loadedTask.P9_GS_NKAssignedStaffMemberInfo, "For working and closed tasks, you must specify either an assigned resource or group.");
			AssertHasError("Precondition", loadedTask.P9_StatusInfo, "For working and closed tasks, you must specify either an assigned resource or group.");

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				ZOrganisationsForm jobForm = null;

				try
				{
					var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
					var playButton = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
					playButton.PerformClick_ForTest();
					Application.DoEvents();

					AssertEquals(@"Errors on Task T00001000 Undefined - You can modify this in the System Reg [Undefined - You can modify this in the System Reg]:
Error - P9_GG_AssignedGroup: For working and closed tasks, you must specify either an assigned resource or group.
Error - P9_GS_NKAssignedStaffMember: For working and closed tasks, you must specify either an assigned resource or group.
Error - P9_Status: For working and closed tasks, you must specify either an assigned resource or group.

".StripTaskIds(), UnitTestUserNotification.Instance.LastMessage.Text.StripTaskIds());
					AssertEquals("Clicking Play should not have closed the ticket automatically because there are validation errors. SAD!", false, detailedTicketControl.IsDisposed);

					task.Reload();
					AssertEquals("The task should not have been set to WRK and saved automatically when the Play button was clicked because there were validation errors. SAD!", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);

					jobForm = BMSFormTestHelper.GetOpenForms<ZOrganisationsForm>().FirstOrDefault();
					AssertNull("Validation errors should prevent the job form from opening. SAD!", jobForm);
				}
				finally
				{
					jobForm?.Dispose();
				}
			}
		}

		#endregion

		#region UX

		public void TestButtonBehaviourThatAutoSavesAndClosesTicket_ShouldUseBeginInvoke()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.WorkingStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK, currentComponent: config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, description: "Task 1", sequence: 1, staffCode: GlbStaff.CurrentUser.GS_Code);

			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
				var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				button.PerformClick_ForTest();

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("The task shouldn't be saved yet because we should be using BeginInvoke in order to cause a pleasant user experience. SAD!", ProcessTaskStatusCodeList.Codes.Assigned, loadedTask.P9_Status);
				AssertEquals("The ticket shouldn't be closed yet because we should be using BeginInvoke in order to cause a pleasant user experience. SAD!", false, detailedTicketControl.IsDisposed);

				Application.DoEvents();

				loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("The task should now be saved because the Application.DoEvents allowed our code inside BeginInvoke to be executed. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
				AssertEquals("The ticket should now be closed because the Application.DoEvents allowed our code inside BeginInvoke to be executed. SAD!", true, detailedTicketControl.IsDisposed);
			}
		}

		public void TestButtonBehaviourThatOpensJob_ShouldUseBeginInvoke()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.WorkingStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK, currentComponent: config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, description: "Task 1", sequence: 1, staffCode: GlbStaff.CurrentUser.GS_Code);

			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				ZOrganisationsForm jobForm = null;

				try
				{
					var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
					var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
					button.PerformClick_ForTest();

					jobForm = BMSFormTestHelper.GetOpenForms<ZOrganisationsForm>().SingleOrDefault();
					AssertNull("The job shouldn't have been opened yet because we should have used BeginInvoke in order to cause a pleasant user experience. SAD!", jobForm);

					var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
					AssertEquals("The task shouldn't be saved yet because we should be using BeginInvoke in order to cause a pleasant user experience. SAD!", ProcessTaskStatusCodeList.Codes.Assigned, loadedTask.P9_Status);
					AssertEquals("The ticket shouldn't be closed yet because we should be using BeginInvoke in order to cause a pleasant user experience. SAD!", false, detailedTicketControl.IsDisposed);

					Application.DoEvents();

					jobForm = BMSFormTestHelper.GetOpenForms<ZOrganisationsForm>().SingleOrDefault();
					AssertNotNull("The job should now be open because the Application.DoEvents allowed our code inside BeginInvoke to be executed. SAD!", jobForm);

					loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
					AssertEquals("The task should now be saved because the Application.DoEvents allowed our code inside BeginInvoke to be executed. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
					AssertEquals("The ticket should now be closed because the Application.DoEvents allowed our code inside BeginInvoke to be executed. SAD!", true, detailedTicketControl.IsDisposed);
				}
				finally
				{
					jobForm?.Dispose();
				}
			}
		}

		#endregion

		#region No Status Change

		public void TestAutoSaveBehaviour_WhenStatusUnchanged_ShouldDoNothing()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.WorkingStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;
			line.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob;
			line.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK, currentComponent: config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, description: "Task 1", sequence: 1, staffCode: GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
				var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
				button.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals("The ticket should not close because the status didn't actually update. SAD!", false, detailedTicketControl.IsDisposed);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("The task should still be set to WRK, nothing has changed. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
			}
		}

		public void TestJobOpenBehaviour_WhenStatusUnchanged_ShouldDoNothing()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.WorkingStatusButton, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			line.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.OpenJob;
			line.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			line.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.SaveAndClose;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK, currentComponent: config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, description: "Task 1", sequence: 1, staffCode: GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				ZOrganisationsForm jobForm = null;

				try
				{
					var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();
					var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x => x.StatusChangeValue == ProcessTaskStatusCodeList.Codes.Working);
					button.PerformClick_ForTest();
					Application.DoEvents();

					jobForm = BMSFormTestHelper.GetOpenForms<ZOrganisationsForm>().SingleOrDefault();
					AssertNull("The job shouldn't have been opened because we didn't actually change the task's status. SAD!", jobForm);

					var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
					AssertEquals("The task should still be set to WRK, nothing has changed. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
				}
				finally
				{
					jobForm?.Dispose();
				}
			}
		}
		#endregion

		#region Status Change Behaviour

		void AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(ZString changedStatus, string controlType1, string controlType2,
			Action<StaticControlCustomisation, ZString> setBehaviour,
			ZString behaviour1, ZString behaviour2,
			bool clickLeftButton, Action<ZForm> assertJobForm)
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line1 = BMSTestHelper.CreateStaticControlCustomisation(customisation, controlType1, "Left", 0, 10, 10, 10, string.Empty, string.Empty, 8, false, false);
			setBehaviour(line1, behaviour1);
			var line2 = BMSTestHelper.CreateStaticControlCustomisation(customisation, controlType2, "Right", 180, 10, 10, 10, string.Empty, string.Empty, 8, false, false);
			setBehaviour(line2, behaviour2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Yas lass StmData", releaseGroupPK: config.ReleaseGroup.PK, currentComponent: config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, description: "Task 1", sequence: 1, staffCode: GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			var section = config.BucketSection;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				ZOrganisationsForm jobForm = null;

				try
				{
					var detailedTicketControl = form.FindSingle<TaskCardDetailControl>();

					var button = detailedTicketControl.FindSingle<GenericStatusChangeButton>(x =>
							x.StatusChangeValue == changedStatus &&
						((x.Left == (clickLeftButton ? 0 : CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180))) ||
						(controlType1 == StaticControlTypeList.Codes.StatusButtons && clickLeftButton && x.Parent is TaskStatusControl)), 99);
					button.PerformClick_ForTest();
					Application.DoEvents();

					CombineAssertions(() =>
					{
						jobForm = BMSFormTestHelper.GetOpenForms<ZOrganisationsForm>().SingleOrDefault();
						assertJobForm(jobForm);

						var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
						AssertEquals(changedStatus, loadedTask.P9_Status);
					});
				}
				finally
				{
					jobForm?.Dispose();
				}
			}
		}

		void AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(ZString changedStatus, string controlType,
			Action<StaticControlCustomisation, ZString> setBehaviour,
			ZString behaviour1, ZString behaviour2,
			bool clickLeftButton, Action<ZForm> assertJobForm)
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(changedStatus, controlType, controlType, setBehaviour, behaviour1, behaviour2, clickLeftButton, assertJobForm);
		}

		#region Working Status

		public void TestJobOpenBehaviour_WithTwoWorkingStatusButtonsAndDifferentPlayButtonBehaviour_WhenRightButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: false,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the rightmost working status button because it shouldn't matter that the leftmost working status button's play button behaviour has been set to save and close...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoWorkingStatusButtonsAndDifferentPlayButtonBehaviour_WhenRightButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: false,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the rightmost working status button because it shouldn't matter that the leftmost working status button's play button behaviour has been set to save and open the form...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoWorkingStatusButtonsAndDifferentPlayButtonBehaviour_WhenLeftButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: true,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the leftmost working status button...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoWorkingStatusButtonsAndDifferentPlayButtonBehaviour_WhenLeftButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: true,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the leftmost working status button...and yet!!",
							jobForm));
		}

		#endregion

		#region Suspended status

		public void TestJobOpenBehaviour_WithTwoSuspendStatusButtonsAndDifferentPlayButtonBehaviour_WhenRightButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Suspended,
				StaticControlTypeList.Codes.SuspendStatusButton,
				(line, behaviour) => line.SuspendButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: false,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the rightmost suspended status button because it shouldn't matter that the leftmost suspended status button's play button behaviour has been set to save and close...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoSuspendStatusButtonsAndDifferentSuspendButtonBehaviour_WhenRightButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Suspended,
				StaticControlTypeList.Codes.SuspendStatusButton,
				(line, behaviour) => line.SuspendButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: false,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the rightmost suspended status button because it shouldn't matter that the leftmost suspended status button's play button behaviour has been set to save and open the form...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoSuspendStatusButtonsAndDifferentSuspendButtonBehaviour_WhenLeftButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Suspended,
				StaticControlTypeList.Codes.SuspendStatusButton,
				(line, behaviour) => line.SuspendButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: true,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the leftmost suspended status button...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoSuspendStatusButtonsAndDifferentSuspendButtonBehaviour_WhenLeftButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Suspended,
				StaticControlTypeList.Codes.SuspendStatusButton,
				(line, behaviour) => line.SuspendButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: true,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the leftmost suspended status button...and yet!!",
							jobForm));
		}

		#endregion

		#region Completed status

		public void TestJobOpenBehaviour_WithTwoCompletedStatusButtonsAndDifferentCompletedButtonBehaviour_WhenRightButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Closed,
				StaticControlTypeList.Codes.CompletedStatusButton,
				(line, behaviour) => line.CloseTaskButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: false,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the rightmost completed status button because it shouldn't matter that the leftmost completed status button's play button behaviour has been set to save and close...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoCompletedStatusButtonsAndDifferentCompletedButtonBehaviour_WhenRightButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Closed,
				StaticControlTypeList.Codes.CompletedStatusButton,
				(line, behaviour) => line.CloseTaskButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: false,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the rightmost completed status button because it shouldn't matter that the leftmost completed status button's play button behaviour has been set to save and open the form...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoCompletedStatusButtonsAndDifferentCompletedButtonBehaviour_WhenLeftButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Closed,
				StaticControlTypeList.Codes.CompletedStatusButton,
				(line, behaviour) => line.CloseTaskButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: true,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the leftmost completed status button...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithTwoCompletedStatusButtonsAndDifferentCompletedButtonBehaviour_WhenLeftButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Closed,
				StaticControlTypeList.Codes.CompletedStatusButton,
				(line, behaviour) => line.CloseTaskButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: true,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the leftmost completed status button...and yet!!",
							jobForm));
		}
		#endregion

		#region Status buttons

		public void TestJobOpenBehaviour_WithStandaloneStatusButton_AndStatusButtonsCollection_WhenStandaloneButtonConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.StatusButtons,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: false,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the rightmost working status button because it shouldn't matter that the leftmost working status button's play button behaviour has been set to save and close...and yet!!",
					jobForm));
		}

		public void TestJobOpenBehaviour_WithStandaloneStatusButton_AndStatusButtonsCollection_WhenStandaloneButtonConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.StatusButtons,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: false,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the rightmost working status button because it shouldn't matter that the leftmost working status button's play button behaviour has been set to save and open the form...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithStandaloneStatusButton_AndStatusButtonsCollection_WhenButtonsCollectionConfiguredToSaveAndCloseIsClicked_ShouldNotOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.StatusButtons,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob,
				clickLeftButton: true,
				(jobForm) => AssertNull("The job should NOT have been opened when clicking the leftmost working status button...and yet!!",
							jobForm));
		}

		public void TestJobOpenBehaviour_WithStandaloneStatusButton_AndStatusButtonsCollection_WhenButtonsCollectionConfiguredToOpenJobIsClicked_ShouldOpenJob()
		{
			AssertJobOpenBehaviour_WithTwoStatusButtonsAndDifferentButtonBehaviour(
				ProcessTaskStatusCodeList.Codes.Working,
				StaticControlTypeList.Codes.StatusButtons,
				StaticControlTypeList.Codes.WorkingStatusButton,
				(line, behaviour) => line.PlayButtonBehavior = behaviour,
				StatusButtonBehaviorOptionsList.Codes.OpenJob, StatusButtonBehaviorOptionsList.Codes.SaveAndClose,
				clickLeftButton: true,
				(jobForm) => AssertNotNull("The job should have been opened when clicking the leftmost working status button...and yet!!",
							jobForm));
		}
		#endregion

		#endregion

		#endregion

		#region Deleted Tasks

		public void TestDeletedTasksShouldNotCauseExceptionsBeforeRefresh()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_FH_ProcessHeader = header.PK;
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, header);
			var factory = Factory.CreateNewFactory();

			using (var card = new TaskCardControl(task1, viewModel))
			using (var detailedCard = new TaskCardDetailControl(card.CardContent.GetTask(factory), card.CardContent.GetWorkflow(factory), card, BMControlCustomisation.GetNewDefaultCardLayout(factory, CustomisedControlTypeList.Codes.DetailedCard), viewModel))
			{
				task1.Delete();
				Factory.Save();
				AssertNoExceptionThrown(() => detailedCard.Close());
			}
		}

		#endregion

		#region Performance

		[TestDate(2015, 7, 14)]
		public void TestDbHits()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var newFactory = Factory.CreateNewFactory();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var parentCard = new TaskCardControl(task, viewModel))
			using (var detailedCard = new TaskCardDetailControl(parentCard.CardContent.GetTask(newFactory), parentCard.CardContent.GetWorkflow(newFactory), parentCard, BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard), sectionViewModel))
			{
				form.Controls.Add(detailedCard);
				var loadedTask = detailedCard.ProcessTask;

				var initialDbHitsAllowed = new Dictionary<string, int>
				{
					{ BMComponentSchema.Constants.TableName, 0 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
					{ ProcessHeaderSchema.Constants.TableName, 1 },
				};
				AssertDbHits(initialDbHitsAllowed, loadedTask.Factory);

				loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				loadedTask.P9_GS_NKAssignedStaffMember = "E";

				var userNotificationMock = new Mock<IUserNotification>();
				userNotificationMock
					.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
						It.IsAny<ZDialogResult>()))
					.Returns(ZDialogResult.Yes);
				detailedCard.UserNotification = userNotificationMock.Object;
				detailedCard.Close();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				Assert(detailedCard.IsDisposed);

				var moreDbHitsAllowed = new Dictionary<string, int>
				{
					{ BMComponentSchema.Constants.TableName, 0 },
					{ BMSystemSchema.Constants.TableName, 1 },
					{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
					{ GlbGroupSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 2 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 4 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				};
				AssertDbHits(moreDbHitsAllowed, loadedTask.Factory);
			}
		}

		#endregion

		#region Implementation

		ProcessTask GetSavedTask()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No moar StmData", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			return task;
		}

		VisualBoardTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			disposables = new DisposableList(new[] { DisableAsyncBehaviour() });
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		DisposableList disposables;

		#endregion
	}

	#region Non-Transactioned Test Case

	class TaskCardDetailControlNonTransactionedTest : NonTransactionedTestCase
	{
		public void TestSaveDetailedTicket_ShouldNotRefreshChannelHeadingMultipleTimes_EvenWhenSavingOneFactorySavesAnother()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This workflow is not a workflow", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code);

			var section = config.BufferSection;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				detailedTicket.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				detailedTicket.ProcessTask.Factory.Saved += delegate
				{
					// Similar to how the ABC consequence engine tacks onto factories saving and saves other things.
					var someOtherFactory = new BusinessObjectFactory();
					var dumy = someOtherFactory.NewWithValidTestData<DummyBusinessObject>();

					someOtherFactory.Save();
				};

				using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
				{
					var factoryNamesGetter = new Func<string[]>(() => PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Select(f => f.NameForDebugging).OrderBy(s => s).ToArray());
					var assertionMessageGetter = new Func<string>(() => "Active factories:" + System.Environment.NewLine + string.Join(System.Environment.NewLine, factoryNamesGetter()));

					AssertEquals(assertionMessageGetter(), 0, factoryNamesGetter().Count(name => name == "RefreshHeadingAsync"));

					detailedTicket.Save_ForTest();

					AssertEquals(assertionMessageGetter(), 1, factoryNamesGetter().Count(name => name == "RefreshHeadingAsync"));

					task.Reload();
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
					task.Factory.Save();

					form.AwaitAll();

					AssertEquals(assertionMessageGetter(), 2, factoryNamesGetter().Count(name => name == "RefreshHeadingAsync"));
				}
			}
		}

		[TestDate(2013, 8, 19, 9, 0, 0)]
		public void TestEndToEnd()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, releaseGroup);
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(bucket, board, row: 0);
			var section2 = BMSTestHelper.CreateBoardSection(buffer, board, row: 1);

			var resource1 = BMSTestHelper.GetOrCreateStaff(Factory, "AAA");
			var resource2 = BMSTestHelper.GetOrCreateStaff(Factory, "BBB");

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var buttonCustomisation = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.StatusButtons, string.Empty, 50, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			buttonCustomisation.PlayButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			buttonCustomisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			buttonCustomisation.CloseTaskButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.NudgeControls, string.Empty, 50, 40, 0, 0, string.Empty, string.Empty, 8, false, false);
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.SaveButton, string.Empty, 50, 80, 0, 0, string.Empty, string.Empty, 8, false, false);
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.CloseButton, string.Empty, 50, 120, 0, 0, string.Empty, string.Empty, 8, false, false);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section1, customisation);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section2, customisation);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Coughocalypse", bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Right next to me", bucket);

			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, description: "task2");

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var task1Card = taskCards.First(t => t.CardContent.TaskIdentifier == task1.PK);
				var task2Card = taskCards.First(t => t.CardContent.TaskIdentifier == task2.PK);
				AssertNotEquals(task1Card, task2Card);

				task1Card.ShowDetailedCard();

				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				var nudgeControls = detailedCard.FindAll<NudgeControls>().Single();
				AssertEquals(true, nudgeControls.Visible);

				var statusControls = detailedCard.FindAll<TaskStatusControl>().Single();

				statusControls.ClosedStatusButton.PerformClick_ForTest();

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, detailedCard.ProcessTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals(false, detailedCard.IsDisposed);
				AssertEquals(false, task1Card.IsDisposed);

				detailedCard.Save_ForTest();
				form.AwaitAll();

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				AssertEquals(true, detailedCard.IsDisposed);
				AssertEquals(true, task1Card.IsDisposed);

				task2Card.ShowDetailedCard();

				detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				statusControls = detailedCard.FindAll<TaskStatusControl>().Single();
				statusControls.WorkingStatusButton.PerformClick_ForTest();
				form.AwaitAll();

				AssertEquals(true, detailedCard.ProcessTask.HasChanges);

				var closeButton = detailedCard.FindAll<CloseCardButton>().Single();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				closeButton.PerformClick();
				form.AwaitAll();

				AssertEquals(false, detailedCard.IsDisposed);
				AssertEquals(false, task2Card.IsDisposed);
				AssertMultilineASCIIEquals("HasChanges message",
	@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				closeButton.PerformClick();
				form.AwaitAll();

				AssertEquals(true, detailedCard.IsDisposed);
				AssertEquals(false, task2Card.IsDisposed);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}
	}

	#endregion
}
