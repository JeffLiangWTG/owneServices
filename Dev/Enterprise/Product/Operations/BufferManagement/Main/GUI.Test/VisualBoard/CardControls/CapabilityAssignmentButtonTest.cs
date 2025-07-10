using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.BufferManagement.GUI.Test
{
	class CapabilityAssignmentButtonTest : BMSTestCaseWithFactory
	{
		public void TestButton_NoRequiredCapability()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(false, task.RequiresResourceWithCapability);

			var cardContent = new TaskCardContent(task, null);
			var parent = new Mock<ITaskCardComponentParent>();

			parent.Setup(m => m.CardContent).Returns(cardContent);
			parent.Setup(m => m.Task).Returns(cardContent.Task);
			using (var control = new CapabilityAssignmentButton(parent.Object))
			{
				AssertEquals(false, control.Visible);
			}
		}

		public void TestButton_Preview()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(false, task.RequiresResourceWithCapability);

			var cardContent = new TaskCardContent(task, null);
			var parent = new Mock<ITaskCardComponentParent>();

			parent.Setup(m => m.CardContent).Returns(cardContent);
			parent.Setup(m => m.Task).Returns(cardContent.Task);
			parent.Setup(m => m.IsPreview).Returns(true);
			using (var control = new CapabilityAssignmentButton(parent.Object))
			{
				AssertEquals(true, control.Visible);
				control.PerformClick();
				AssertEquals(true, control.Visible);
			}
		}

		public void TestButton_WithUnChanneledChannel()
		{
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var capability = Factory.New<GlbCapability>();
			task.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task.RequiresResourceWithCapability);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var viewModel = CreateViewModelAllowNull(staff, task, null);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Claim", control.Text);
				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				control.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign;
				control.PerformClick();

				AssertEquals(GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.Visible);
			}
		}

		public void TestButton_WithRequiredCapability()
		{
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var capability = Factory.New<GlbCapability>();
			task.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task.RequiresResourceWithCapability);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var viewModel = CreateViewModel(staff, task);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Claim", control.Text);
				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				control.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign;
				control.PerformClick();

				AssertEquals(GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.Visible);
			}
		}

		public void TestButton_WithRequiredCapability_InMyChannel()
		{
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task2 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task3 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task4 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var capability = Factory.New<GlbCapability>();
			task1.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task1.RequiresResourceWithCapability);
			task2.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task2.RequiresResourceWithCapability);
			task3.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task3.RequiresResourceWithCapability);
			AssertEquals(false, task4.RequiresResourceWithCapability);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var viewModel = CreateViewModel(staff, task1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			using (var controlForAllTask = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Claim", control.Text);
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);

				control.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign;
				control.PerformClick();

				AssertEquals(staff.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.Visible);

				controlForAllTask.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkfloWithForClaimOrAssign;
				controlForAllTask.PerformClick();

				AssertEquals(staff.GS_Code, task2.AssignedStaffMember.GS_Code);
				AssertEquals(staff.GS_Code, task3.AssignedStaffMember.GS_Code);
				AssertNull(task4.AssignedStaffMember);
			}
		}

		public void TestButton_WithRequiredCapability_InOtherChannel()
		{
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task2 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task3 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task4 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var capability = Factory.New<GlbCapability>();
			task1.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task1.RequiresResourceWithCapability);
			task2.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task2.RequiresResourceWithCapability);
			task3.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task3.RequiresResourceWithCapability);
			AssertEquals(false, task4.RequiresResourceWithCapability);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FRO";
			var viewModel = CreateViewModel(staff, task1);
			Factory.Save();

			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			using (var controlForAllTask = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Assign to FRO", control.Text);
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);

				control.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign;
				control.PerformClick();
				AssertEquals(staff.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.Visible);

				controlForAllTask.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkfloWithForClaimOrAssign;
				controlForAllTask.PerformClick();

				AssertEquals(staff.GS_Code, task2.AssignedStaffMember.GS_Code);
				AssertEquals(staff.GS_Code, task3.AssignedStaffMember.GS_Code);
				AssertNull(task4.AssignedStaffMember);
			}
		}

		public void TestButton_SingleTaskWithinWorkflow()
		{
			var capability = CreateCapability("CA1", "Capability1");
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);

			AssertEquals(true, task1.RequiresResourceWithCapability);

			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo");
			var viewModel = CreateViewModel(staff, task1);
			Factory.Save();

			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Assign to FRO", control.Text);
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);

				control.PerformClick();
				AssertNull("No options displayed", control.DialogWrapperForTest.ButtonStripActions);
				AssertEquals("Task assigned to staff without the need to specify options", staff.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.Visible);
			}
		}

		public void TestButton_MultipleTaskWithinWorkflow_SingleTaskUnassigned()
		{
			var capability = CreateCapability("CA1", "Capability1");
			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo", capability);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, staff.GS_Code, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);
			var task2 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);

			AssertEquals(false, task1.RequiresResourceWithCapability);
			AssertEquals(true, task2.RequiresResourceWithCapability);

			var viewModel = CreateViewModel(staff, task2);
			Factory.Save();

			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Assign to FRO", control.Text);
				AssertEquals(ZString.Empty, task2.P9_GS_NKAssignedStaffMember);

				control.PerformClick();
				AssertNull("No options displayed", control.DialogWrapperForTest.ButtonStripActions);
				AssertEquals("Task assigned to staff without the need to specify options", staff.GS_Code, task2.P9_GS_NKAssignedStaffMember);
				AssertEquals(false, control.Visible);
			}
		}

		public void TestButton_MultipleTaskWithinWorkflow_MultipleTasksUnassigned()
		{
			var capability = CreateCapability("CA1", "Capability1");
			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo", capability);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);
			var task2 = CreateTask(workflow, string.Empty, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);

			AssertEquals(true, task1.RequiresResourceWithCapability);
			AssertEquals(true, task2.RequiresResourceWithCapability);

			var viewModel = CreateViewModel(staff, task1);
			Factory.Save();

			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Assign to FRO", control.Text);
				AssertEquals(ZString.Empty, task1.P9_GS_NKAssignedStaffMember);

				control.PerformClick();
				AssertEquals("All buttons displayed", 3, control.DialogWrapperForTest.ButtonStripActions.Length);
				AssertEquals("Selected task only button", "Assign this task", control.DialogWrapperForTest.ButtonStripActions[0].Text);
				AssertEquals("Selected task only button", "Assign tasks requiring Capability1 capability", control.DialogWrapperForTest.ButtonStripActions[1].Text);
				AssertEquals("Selected task only button", "Cancel", control.DialogWrapperForTest.ButtonStripActions[2].Text);
			}
		}

		public void TestButton_WhenMultipleCapabilityTasksExist_ShouldNotReassignOrReopenClosedTasks()
		{
			var capability = CreateCapability("CA1", "Capability1");
			var staff1 = CreateStaffInCurrentBranchDept("FRO", "Frodo", capability);
			var staff2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo", capability);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, staffCode: staff2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, staffCode: string.Empty, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);
			var task3 = BMSTestHelper.CreateTask(workflow, staffCode: string.Empty, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);

			AssertEquals("The workflow has to have multiple open tasks for this defect to happen.", 2, workflow.Tasks.Count(x => x.IsOpen));

			var viewModel = CreateViewModel(staff1, task2);
			Factory.Save();

			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(viewModel))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Assign to FRO", control.Text);

				control.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkfloWithForClaimOrAssign; // "Assign tasks requiring Capability1 capability"

				control.PerformClick();
				Application.DoEvents();

				AssertEquals("All buttons displayed", 3, control.DialogWrapperForTest.ButtonStripActions.Length);
				AssertEquals("Selected task only button", "Assign this task", control.DialogWrapperForTest.ButtonStripActions[0].Text);
				AssertEquals("Selected task only button", "Assign tasks requiring Capability1 capability", control.DialogWrapperForTest.ButtonStripActions[1].Text);
				AssertEquals("Selected task only button", "Cancel", control.DialogWrapperForTest.ButtonStripActions[2].Text);

				AssertEquals("The task clicked should have been assigned.", "FRO", task2.P9_GS_NKAssignedStaffMember);
				AssertEquals("The closed task should not have been reassigned.", "BIL", task1.P9_GS_NKAssignedStaffMember);
				AssertEquals("The closed task should not have been reopened.", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			}
		}

		public void TestTooltip()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var capability = BMSTestHelper.CreateCapability(Factory, "PEN", "Peanut eating with your nose");
			task.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task.RequiresResourceWithCapability);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(CreateBucket(CreateSystem())));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var cardContent = new TaskCardContent(task, null);
				var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(staff) };
				var parent = new Mock<ITaskCardComponentParent>();

				parent.Setup(m => m.CardContent).Returns(cardContent);
				parent.Setup(m => m.Task).Returns(cardContent.Task);
				parent.Setup(m => m.Cell).Returns(cell);
				using (var parentControl = new ZPanel())
				using (var control = new CapabilityAssignmentButton(parent.Object))
				{
					parentControl.Controls.Add(control);

					AssertEquals(true, control.Visible);
					AssertEquals("Claim", control.Text);
					AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

					AssertEquals(
@"This task is not assigned to a particular channel, however is configured to require the Peanut eating with your nose capability.
By clicking this button, you will assign this task directly to yourself.", ToolTipService.GetToolTip(control));
				}
			}
		}

		public void TestTooltip_AnotherChannel()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var capability = BMSTestHelper.CreateCapability(Factory, "PEN", "Peanut eating with your nose");
			task.P9_G4_RequiredCapability = capability.PK;
			AssertEquals(true, task.RequiresResourceWithCapability);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Frodo Baggins";
			staff.GS_Code = "FRO";
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(CreateBucket(CreateSystem())));

			var cardContent = new TaskCardContent(task, null);
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = viewModel.CreateChannelForTest(staff) };
			var parent = new Mock<ITaskCardComponentParent>();

			parent.Setup(m => m.CardContent).Returns(cardContent);
			parent.Setup(m => m.Task).Returns(cardContent.Task);
			parent.Setup(m => m.Cell).Returns(cell);
			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(parent.Object))
			{
				parentControl.Controls.Add(control);

				AssertEquals(true, control.Visible);
				AssertEquals("Assign to FRO", control.Text);
				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				AssertEquals(
@"This task is not assigned to a particular channel, however is configured to require the Peanut eating with your nose capability.
By clicking this button, you will assign this task to Frodo Baggins.", ToolTipService.GetToolTip(control));
			}
		}

		public void TestTooltip_AnotherChannelThatSsNotStaff()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "PEN", "Peanut eating with your nose");
			var task = BMSTestHelper.CreateTask(BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "Peanuts go in your ear."), capability: capability);

			AssertEquals(true, task.RequiresResourceWithCapability);

			Factory.Save();

			var cardContent = new TaskCardContent(task, null);
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = new UnchanneledChannel(ChannelTypeList.Codes.Resource) };
			var parent = new Mock<ITaskCardComponentParent>();

			parent.Setup(m => m.CardContent).Returns(cardContent);
			parent.Setup(m => m.Task).Returns(Factory.New<ProcessTask>());
			parent.Setup(m => m.Cell).Returns(cell);
			using (var parentControl = new ZPanel())
			using (var control = new CapabilityAssignmentButton(parent.Object))
			{
				parentControl.Controls.Add(control);

				AssertEquals(false, control.Visible);
				AssertEquals(string.Empty, ToolTipService.GetToolTip(control));
			}
		}

		public ControlCustomisationViewModel CreateViewModel(GlbStaff staff, ProcessTask task)
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = sectionViewModel.CreateChannelForTest(staff) };
			var viewModel = CreateViewModelCode(cell, task, sectionViewModel);

			return viewModel;
		}

		public ControlCustomisationViewModel CreateViewModelAllowNull(GlbStaff staff, ProcessTask task, IVisualBoardChannel channel)
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var cell = new CellContent(0, 0, CellContentType.Cards) { Channel = channel };
			var viewModel = CreateViewModelCode(cell, task, sectionViewModel);

			return viewModel;
		}

		ControlCustomisationViewModel CreateViewModelCode(CellContent cell, ProcessTask task, BMBoardSectionViewModel sectionViewModel)
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);
			statusIndicatorLine.Orientation = "Vertical";

			return new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task);
		}
	}
}
