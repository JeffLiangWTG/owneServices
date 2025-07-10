using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TaskStatusControlTest : BMSGUITestCase
	{
		public void TestAddTagAddsTag()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(tagGroup, "GRN", nudge: 200, color: Color.Green);

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow1.FH_GG_ReleaseGroup = group.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = BMSTestHelper.CreateWorkflow(workflow1.JobHeader, "Can we do it?");
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			task1.P9_Sequence = 1;
			task2.P9_Sequence = 1;

			workflow1.AddTag(green);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var card1 = form.FindAll<TaskCardControl>().Single(t => t.CardContent.TaskIdentifier == task1.PK);
				var card2 = form.FindAll<TaskCardControl>().Single(t => t.CardContent.TaskIdentifier == task2.PK);

				AssertBitmapNotEquals("They have different tags!", card1, card2);

				TaskCardControlTest.AddTagViaMenu(card2);
				Application.DoEvents();

				card1 = form.FindAll<TaskCardControl>().Single(t => t.CardContent.TaskIdentifier == task1.PK);
				card2 = form.FindAll<TaskCardControl>().Single(t => t.CardContent.TaskIdentifier == task2.PK);

				AssertBitmapEquals("Tagging should make them the same!", card1, card2);
			}
		}

		[ExpectNoExceptions]
		public void TestNullTask()
		{
			ProcessTask task = null;
			var parent = new StatusControlComponentParent(task);
			using (var form = new ZForm())
			using (var control = new TaskStatusControl(parent))
			{
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(task, string.Empty);
				form.Controls.Add(control);
				control.Width = -1;
				control.WorkingStatusButton.Width = -1;
				form.Show();
				control.Show();
				Application.DoEvents();
			}
		}

		public void TestSetStatus()
		{
			var task = Factory.New<ProcessTask>();
			var parent = new StatusControlComponentParent(task);
			var buttonBehaviour = StatusButtonBehaviorOptionsList.Codes.Manual;

			using (var form = new ZForm())
			using (var control = new TaskStatusControl(parent, buttonBehaviour, buttonBehaviour, buttonBehaviour))
			{
				control.SetDataBinding(task, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.WorkingStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(false, control.ActualTimeEdit.Visible);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.play_selected, (Bitmap)control.WorkingStatusButton.Controls[0].BackgroundImage);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.pause, (Bitmap)control.SuspendedStatusButton.Controls[0].BackgroundImage);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.tick, (Bitmap)control.ClosedStatusButton.Controls[0].BackgroundImage);

				control.SuspendedStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(false, control.ActualTimeEdit.Visible);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.play, (Bitmap)control.WorkingStatusButton.Controls[0].BackgroundImage);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.pause_selected, (Bitmap)control.SuspendedStatusButton.Controls[0].BackgroundImage);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.tick, (Bitmap)control.ClosedStatusButton.Controls[0].BackgroundImage);

				control.ClosedStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(true, control.ActualTimeEdit.Visible);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.play, (Bitmap)control.WorkingStatusButton.Controls[0].BackgroundImage);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.pause, (Bitmap)control.SuspendedStatusButton.Controls[0].BackgroundImage);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(BufferManagement.GUI.Properties.Resources.tick_selected, (Bitmap)control.ClosedStatusButton.Controls[0].BackgroundImage);
			}
		}

		public void TestSetStatus_EmptyAssignedStaff()
		{
			var staff1 = CreateStaffInCurrentBranchDept("SME", "Smeagol");
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);
			statusIndicatorLine.Orientation = "Vertical";

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);
			var parent = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task);

			const string buttonBehaviour = StatusButtonBehaviorOptionsList.Codes.Manual;

			using (var form = new ZForm())
			using (var control = new TaskStatusControl(parent, buttonBehaviour, buttonBehaviour, buttonBehaviour))
			{
				control.SetDataBinding(task, string.Empty);
				form.Controls.Add(control);
				form.Show();

				task.P9_GS_NKAssignedStaffMember = string.Empty;

				AssertEquals(string.Empty, task.P9_GS_NKAssignedStaffMember);

				// Working status
				control.WorkingStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(staff1.GS_Code, task.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.ActualTimeEdit.Visible);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

				task.P9_GS_NKAssignedStaffMember = string.Empty;

				AssertEquals(string.Empty, task.P9_GS_NKAssignedStaffMember);

				// Suspended status
				control.SuspendedStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(staff1.GS_Code, task.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.ActualTimeEdit.Visible);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);

				task.P9_GS_NKAssignedStaffMember = string.Empty;

				AssertEquals(string.Empty, task.P9_GS_NKAssignedStaffMember);

				// Closed status
				control.ClosedStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals(staff1.GS_Code, task.P9_GS_NKAssignedStaffMember);
				AssertEquals(true, control.ActualTimeEdit.Visible);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			}
		}

		public void TestSetStatus_ToggleClaimButtonVisibility()
		{
			var staff1 = CreateStaffInCurrentBranchDept("SME", "Smeagol");
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var claimButton = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.CapabilityAssignmentButton, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var capability = CreateCapability("ABC", "Dummy capability");
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended, capability: capability);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			var parent = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task);

			using (var form = new ZForm())
			using (var control = new TaskStatusControl(parent))
			using (var userControl = CustomisedControlRenderer.Render(parent))
			{
				control.SetDataBinding(task, string.Empty);
				form.Controls.Add(control);
				form.Controls.Add(userControl);
				form.Show();

				var capabilityAssignmentButton = userControl.Find(c => c.Tag == claimButton).First();

				AssertEquals(typeof(CapabilityAssignmentButton), capabilityAssignmentButton.GetType());
				AssertEquals(true, capabilityAssignmentButton.Visible);

				// Working status
				control.WorkingStatusButton.PerformClick_ForTest();
				AssertEquals(false, capabilityAssignmentButton.Visible);

				capabilityAssignmentButton.Visible = true;
				AssertEquals(true, capabilityAssignmentButton.Visible);

				// Suspended status
				control.SuspendedStatusButton.PerformClick_ForTest();
				AssertEquals(false, capabilityAssignmentButton.Visible);

				capabilityAssignmentButton.Visible = true;
				AssertEquals(true, capabilityAssignmentButton.Visible);

				// Closed status
				control.ClosedStatusButton.PerformClick_ForTest();
				AssertEquals(false, capabilityAssignmentButton.Visible);
			}
		}

		void TestSetStatusAndShowActualDurationControl(string assertionMessage, string buttonBehaviour, bool enterActualDuration, bool showActualDurationControl)
		{
			_ = DummyWorkflowDescriptor.Instance;
			var workflowTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var categorisedWorkflowTypes = workflowTypes.Cast<CategorisedWorkflowTaskTypes>().First(n => n.Code == "DUM");

			var categorisedWorkflowTaskTypes = categorisedWorkflowTypes.TaskTypes;
			categorisedWorkflowTaskTypes.RemoveAndDeleteAll();

			var workflowTaskType = categorisedWorkflowTaskTypes.AddNew();
			workflowTaskType.IsRequireActualDuration = true;
			workflowTaskType.Code = "UDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowTypes);

			var workflow = CreateJobHeader<DummyWithWorkflow>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60);

			if (enterActualDuration)
			{
				task.P9_ActualDuration = new ZDateTime(2000, 1, 1, 1, 0, 0);
			}
			task.P9_Type = "UDF";

			var parent = new StatusControlComponentParent(task);

			using (var form = new ZForm())
			using (var control = new TaskStatusControl(parent, buttonBehaviour, buttonBehaviour, buttonBehaviour))
			{
				control.SetDataBinding(task, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var wasActualTimeEditEverVisible = false;
				control.ActualTimeEdit.VisibleChanged += (sender, args) => wasActualTimeEditEverVisible = true;

				control.ClosedStatusButton.PerformClick_ForTest();
				Application.DoEvents();
				Application.DoEvents();

				AssertEquals(assertionMessage, showActualDurationControl, wasActualTimeEditEverVisible);
			}
		}

		public void TestSetStatus_WithAutoSaveStatusButtons_ShouldNotShowActualDurationControl_WhenActualDurationEnteredInTask()
		{
			TestSetStatusAndShowActualDurationControl("When using Auto-Save behaviour, clicking Close Task should not show the Actual Duration field at all. SAD!",
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, enterActualDuration: true, showActualDurationControl: false);
		}

		public void TestSetStatus_WithAutoSaveStatusButtons_ShouldShowActualDurationControl_WhenActualDurationNotEnteredInTask()
		{
			TestSetStatusAndShowActualDurationControl("When using Auto-Save behaviour, clicking Close Task should show the Actual Duration field. SAD!",
				StatusButtonBehaviorOptionsList.Codes.SaveAndClose, enterActualDuration: false, showActualDurationControl: true);
		}

		public void TestSetStatus_WithOpenJobStatusButtons_ShouldNotShowActualDurationControl_WhenActualDurationEnteredInTask()
		{
			TestSetStatusAndShowActualDurationControl("When using Save and Open Job behaviour, clicking Close Task should not show the Actual Duration field at all. SAD!",
				StatusButtonBehaviorOptionsList.Codes.OpenJob, enterActualDuration: true, showActualDurationControl: false);
		}

		public void TestSetStatus_WithOpenJobStatusButtons_ShouldShowActualDurationControl_WhenActualDurationNotEnteredInTask()
		{
			TestSetStatusAndShowActualDurationControl("When using Save and Open Job behaviour, clicking Close Task should show the Actual Duration field. SAD!",
				StatusButtonBehaviorOptionsList.Codes.OpenJob, enterActualDuration: false, showActualDurationControl: true);
		}

		public void TestSetStatus_WithManualStatusButtons_ShouldShowActualDurationControl()
		{
			var task = Factory.New<ProcessTask>();
			var parent = new StatusControlComponentParent(task);
			var behaviourCode = StatusButtonBehaviorOptionsList.Codes.Manual;

			using (var form = new ZForm())
			using (var control = new TaskStatusControl(parent, behaviourCode, behaviourCode, behaviourCode))
			{
				control.SetDataBinding(task, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var wasActualTimeEditEverVisible = false;
				control.ActualTimeEdit.VisibleChanged += (sender, args) => wasActualTimeEditEverVisible = true;

				control.ClosedStatusButton.PerformClick_ForTest();
				Application.DoEvents();

				AssertEquals("When using manual behaviour, clicking Close Task should show the Actual Duration field. SAD!", true, wasActualTimeEditEverVisible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;
		}

		class StatusControlComponentParent : ITaskCardComponentParent
		{
			internal StatusControlComponentParent(ProcessTask task)
			{
				this.task = task;
			}

			readonly ProcessTask task;

			public void ShowParent()
			{
			}

			public void Save(object sender = null)
			{
				task.Factory.Save();
				Saved?.Invoke(sender, new TasksSavedArgs(null));
			}

			public void Close()
			{
				throw new NotImplementedException();
			}

			public void UpdateStatus(string status)
			{
				task.P9_Status = status;
				StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(status));
			}

			public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;

			public event EventHandler<TasksSavedArgs> Saved;

			public ICardContent CardContent
			{
				get { throw new NotImplementedException(); }
			}

			public CellContent Cell
			{
				get { throw new NotImplementedException(); }
			}

			public void VoteUp(bool jobCardsShown)
			{
				throw new NotImplementedException();
			}

			public void VoteDown(bool jobCardsShown)
			{
				throw new NotImplementedException();
			}

			public bool IsPreview
			{
				get { throw new NotImplementedException(); }
			}

			public string GetColorTag(Control colorTagControl)
			{
				throw new NotImplementedException();
			}

			public ProcessTask Task
			{
				get { return task; }
			}

			public ProcessHeader Workflow
			{
				get { return task.GetProcessHeader(); }
			}
		}
	}
}
