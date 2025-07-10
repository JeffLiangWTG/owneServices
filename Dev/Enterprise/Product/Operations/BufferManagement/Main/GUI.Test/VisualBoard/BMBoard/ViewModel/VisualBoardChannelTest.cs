using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class VisualBoardChannelTest : BMSTestCaseWithFactory
	{
		#region Capability

		public void TestVisualBoardChannel_CapabilityAssignmentButtons()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var compontent = system.Components.AddNew();
			compontent.FC_Name = "Mai Component";
			compontent.FC_Type = BMComponentTypeList.Codes.Bucket;

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(compontent, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var currentUserChannel = sectionViewModel.CreateChannelForTest(GlbStaff.CurrentUser);
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>();
			var anotherUserChannel = sectionViewModel.CreateChannelForTest(anotherUser);

			var capability = CreateCapability("NDC", "Nested Doll Capability");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var task1 = BMSTestHelper.CreateTask(jobHeader.ProcessHeaders[0], capability: capability);
			var task2 = BMSTestHelper.CreateTask(jobHeader.ProcessHeaders[0], capability: capability);

			AssertEquals(true, task1.RequiresResourceWithCapability);
			AssertEquals(true, task2.RequiresResourceWithCapability);

			Factory.Save();

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();

			ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task1, currentUserChannel, sectionViewModel, dialogProvider, "", t => { }, currentUserChannel);

			AssertEquals("Claim this task", dialogProvider.ButtonStripActions[0].Text);
			AssertContains("Claim this task together with all tasks bound by task auto assignment restrictions of the SAM type.", dialogProvider.ButtonStripActions[0].ToolTip);
			AssertContains("Claim all tasks requiring the Nested Doll Capability capability together with all tasks bound by task auto assignment restrictions of the SAM type.", dialogProvider.ButtonStripActions[1].ToolTip);

			ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task1, anotherUserChannel, sectionViewModel, dialogProvider, "", t => { }, anotherUserChannel);

			AssertEquals("Assign this task", dialogProvider.ButtonStripActions[0].Text);
			AssertContains("Assign this task together with all tasks bound by task auto assignment restrictions of the SAM type.", dialogProvider.ButtonStripActions[0].ToolTip);
			AssertContains("Assign all tasks requiring the Nested Doll Capability capability together with all tasks bound by task auto assignment restrictions of the SAM type.", dialogProvider.ButtonStripActions[1].ToolTip);
		}

		public void TestVisualBoardChannel_CapabilityAssignmentButtonTooltips()
		{
			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CBC";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = ScopeList.Codes.Workflow;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var samTaskType1 = restriction.TaskTypesCollection.AddNew();
			samTaskType1.Code = "CDU";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var system = BMSTestHelper.CreateSystem(Factory);
			var component = system.Components.AddNew();
			component.FC_Name = "Pandora's Box";
			component.FC_Type = BMComponentTypeList.Codes.Bucket;

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(component, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var currentUserChannel = sectionViewModel.CreateChannelForTest(GlbStaff.CurrentUser);
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>();
			var anotherUserChannel = sectionViewModel.CreateChannelForTest(anotherUser);

			var capability = CreateCapability("PDR", "Pandora Capability");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var task1 = BMSTestHelper.CreateTask(jobHeader.ProcessHeaders[0], lowEstMinutes: 10, taskType: "INV", capability: capability);
			var task2 = BMSTestHelper.CreateTask(jobHeader.ProcessHeaders[0], lowEstMinutes: 20, taskType: "CBC", capability: capability);
			var task3 = BMSTestHelper.CreateTask(jobHeader.ProcessHeaders[0], lowEstMinutes: 30, taskType: "CDU");

			AssertEquals("Precondition - task1 should require a capability", true, task1.RequiresResourceWithCapability);
			AssertEquals("Precondition - task2 should require a capability", true, task2.RequiresResourceWithCapability);
			AssertEquals("Precondition - task3 should not require a capability", false, task3.RequiresResourceWithCapability);
			AssertEquals("Precondition - task1 std estimated duration", 15.0, task1.StandardEstimatedDuration.GetMinutesFromDateTimeSpan(), 0.000001);
			AssertEquals("Precondition - task2 std estimated duration", 30.0, task2.StandardEstimatedDuration.GetMinutesFromDateTimeSpan(), 0.000001);
			AssertEquals("Precondition - task3 std estimated duration", 45.0, task3.StandardEstimatedDuration.GetMinutesFromDateTimeSpan(), 0.000001);
			AssertEquals(false, BMSRegistry.Instance.DisableCapacityCalculations.Value);

			Factory.Save();

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();

			Action<IVisualBoardChannel, string> checkTooltipsCapacityCalculationsEnabled = (channel, actionNameForMessage) =>
			{
				var resourceName = channel == currentUserChannel ? "CargoWise Support" : "";

				ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task1, channel, sectionViewModel, dialogProvider, "", t => { }, channel);
				CombineAssertions($@"{actionNameForMessage} buttons,
						task1", () =>
				{
					AssertEquals($@"Tooltip for the {actionNameForMessage} this task button should display standard capacity required to both task 1 and task 3
							since they are under SAM restrictions", $@"{actionNameForMessage} this task together with all tasks bound by task auto assignment restrictions of the SAM type. This will consume 1 hours of capacity.
Resource [{resourceName}] has 0 hours of available capacity in this buffer. Move this selected task into this channel."
							, dialogProvider.ButtonStripActions[0].ToolTip);
					AssertEquals($@"Tooltip for the {actionNameForMessage} all tasks... button should display standard capacity required to both task 1, task 2 and task 3
							since task 1 and task 2 require the same capability and task 1 and task 3 are under SAM restrictions",
						$@"{actionNameForMessage} all tasks requiring the Pandora Capability capability together with all tasks bound by task auto assignment restrictions of the SAM type. This will consume 1.5 hours of capacity.
Resource [{resourceName}] has 0 hours of available capacity in this buffer. Move all tasks from this workflow assigned to the capability [Pandora Capability] that are not yet started into this channel", dialogProvider.ButtonStripActions[1].ToolTip);
				});

				ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task2, channel, sectionViewModel, dialogProvider, "", t => { }, channel);
				CombineAssertions($@"{actionNameForMessage} buttons,
						task2", () =>
				{
					AssertEquals($@"Tooltip for the {actionNameForMessage} this task button should display standard capacity required to task 2 only",
						$@"{actionNameForMessage} this task together with all tasks bound by task auto assignment restrictions of the SAM type. This will consume 0.5 hours of capacity.
Resource [{resourceName}] has 0 hours of available capacity in this buffer. Move this selected task into this channel.", dialogProvider.ButtonStripActions[0].ToolTip);
					AssertEquals($@"Tooltip for the {actionNameForMessage} all tasks... button should display standard capacity required to both task 2, task 1 and task 3 since task 2 and task 1 require the same capability and task 1 and task 3 are under SAM restrictions",
						$@"{actionNameForMessage} all tasks requiring the Pandora Capability capability together with all tasks bound by task auto assignment restrictions of the SAM type. This will consume 1.5 hours of capacity.
Resource [{resourceName}] has 0 hours of available capacity in this buffer. Move all tasks from this workflow assigned to the capability [Pandora Capability] that are not yet started into this channel", dialogProvider.ButtonStripActions[1].ToolTip);
				});
			};

			checkTooltipsCapacityCalculationsEnabled(currentUserChannel, "Claim");
			checkTooltipsCapacityCalculationsEnabled(anotherUserChannel, "Assign");
			Action<IVisualBoardChannel, string> checkTooltipsCapacityCalculationsDisabled = (channel, actionNameForMessage) =>
			{
				ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task1, channel, sectionViewModel, dialogProvider, "", t => { }, channel);
				CombineAssertions($@"{actionNameForMessage} buttons,
						task1", () =>
				{
					AssertEquals($@"Tooltip for the {actionNameForMessage} this task button should not display standard capacity
							since DisableCapacityCalculations is enabled in the registry",
						$"{actionNameForMessage} this task together with all tasks bound by task auto assignment restrictions of the SAM type. Move this selected task into this channel.", dialogProvider.ButtonStripActions[0].ToolTip);
					AssertEquals($@"Tooltip for the {actionNameForMessage} all tasks... button should not display standard capacity
							since DisableCapacityCalculations is enabled in the registry",
						$"{actionNameForMessage} all tasks requiring the Pandora Capability capability together with all tasks bound by task auto assignment restrictions of the SAM type. Move all tasks from this workflow assigned to the capability [Pandora Capability] that are not yet started into this channel", dialogProvider.ButtonStripActions[1].ToolTip);
				});

				ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task2, channel, sectionViewModel, dialogProvider, "", t => { }, channel);
				CombineAssertions($@"{actionNameForMessage} buttons,
						task2", () =>
				{
					AssertEquals($@"Tooltip for the {actionNameForMessage} this task button should not display standard capacity
							since DisableCapacityCalculations is enabled in the registry",
						$"{actionNameForMessage} this task together with all tasks bound by task auto assignment restrictions of the SAM type. Move this selected task into this channel.", dialogProvider.ButtonStripActions[0].ToolTip);
					AssertEquals($@"Tooltip for the {actionNameForMessage} all tasks... button should not display standard capacity
							since DisableCapacityCalculations is enabled in the registry",
						$@"{actionNameForMessage} all tasks requiring the Pandora Capability capability together with all tasks bound by task auto assignment restrictions of the SAM type. Move all tasks from this workflow assigned to the capability [Pandora Capability] that are not yet started into this channel", dialogProvider.ButtonStripActions[1].ToolTip);
				});
			};

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			checkTooltipsCapacityCalculationsDisabled(currentUserChannel, "Claim");
			checkTooltipsCapacityCalculationsDisabled(anotherUserChannel, "Assign");
			AssertNotEquals("Capacity was calculated even though the DisableCapacityCalculations registry item was enabled.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Resource

		public static string[] TablesNotAllowedToBeHitInPreview => new[]
		{
			BMComponentResourceLinkSchema.Constants.TableName,
			GlbCapabilitySchema.Constants.TableName,
			GlbResourceCapabilityPivotSchema.Constants.TableName,
			GlbStaffHolidaySchema.Constants.TableName,
			ProcessTasksSchema.Constants.TableName,
			StmModuleFilterSchema.Constants.TableName,
		};

		#endregion

		#region Drag/Drop

		#region Batch Assignment

		public void TestDragDrop_BatchAssignment_TasksAssignedToStaff()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var otherCapability = Factory.New<GlbCapability>();
			otherCapability.G4_Description = "Ekle Peckle";
			otherCapability.G4_Code = "EJK";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			resource.GS_FriendlyName = "Frodo";

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, capability: otherCapability, description: "task1");
			var task2_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task2");
			var task3_withResourceOnly = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, description: "Task3");
			Factory.Save();

			var task2Content = new TaskCardContent(task2_withCapabilityAndResource, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var capabilityChannel = viewModel.CreateChannelForTest(capability);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.TasksAssignedToStaff;

			AssertEquals(true, BMSTestHelper.HandleTaskAssignment(capabilityChannel, Factory, viewModel, task2Content, resourceChannel, dialogProvider, viewModel));
			AssertEquals(otherCapability.PK, task1_withCapabilityOnly.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task2_withCapabilityAndResource.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task3_withResourceOnly.P9_G4_RequiredCapability);

			AssertEquals(@"Would you like to return task 'task2' to the capability 'Saying Boo-urns'?

This will remove the assignment of Frodo Baggins from the task.", dialogProvider.LastMessage);
		}

		public void TestDragDrop_BatchAssignment_TasksAssignedToGroup()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "BLE";

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var otherResource = CreateStaffInCurrentBranchDept("Yam", "Yam Man", capability);

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withResourceOnly = CreateTask(workflow, staffCode: otherResource.GS_Code, lowEstMinutes: 90, description: "task1");
			var task2_withGroupAndResource = CreateTask(workflow, staffCode: otherResource.GS_Code, lowEstMinutes: 90, description: "task2");
			var task3_withGroupOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, description: "Task3");
			var task4_withGroupOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, description: "Task4");
			task4_withGroupOnly.P9_GG_AssignedGroup = group.PK;
			task3_withGroupOnly.P9_GG_AssignedGroup = group.PK;
			task2_withGroupAndResource.P9_GG_AssignedGroup = group.PK;

			var workflow2 = CreateWorkflow(workflow.JobHeader, "The mighty Panly");
			var task = CreateTask(workflow2, resource.GS_Code, 90, description: "Don't mess with yam man");
			Factory.Save();

			var task2Content = new TaskCardContent(task2_withGroupAndResource, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var groupChannel = viewModel.CreateChannelForTest(group);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.TasksAssignedToGroup;

			AssertEquals(true, BMSTestHelper.HandleTaskAssignment(resourceChannel, Factory, viewModel, task2Content, groupChannel, dialogProvider, viewModel));
			AssertEquals(otherResource.GS_Code, task1_withResourceOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2_withGroupAndResource.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3_withGroupOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task4_withGroupOnly.P9_GS_NKAssignedStaffMember);

			AssertEquals("Would you like to assign tasks related to 'task2' that are not yet started to Frodo Baggins?", dialogProvider.LastMessage);
		}

		public void TestDragDrop_BatchAssignment_AllTasksInWorkflow()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var otherResource = CreateStaffInCurrentBranchDept("Yam", "Yam Man", capability);

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, capability: capability, description: "task1");
			var task2_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task2");
			var task3_withResourceOnly = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, description: "Task3");
			var task4_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task4");
			var task5_withResourceOnly = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, description: "Task5", taskStatus: "SUS");
			var task6_withResourceOnly = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, description: "Task6", taskStatus: "WRK");
			var task7_withCapabilityandResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task7", taskStatus: "SUS");
			var task8_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task8");

			var workflow2 = CreateWorkflow(workflow.JobHeader, "The mighty Panly");
			var task = CreateTask(workflow2, resource.GS_Code, 90, description: "Don't mess with yam man");
			Factory.Save();

			var task2Content = new TaskCardContent(task2_withCapabilityAndResource, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var otherResourceChannel = viewModel.CreateChannelForTest(otherResource);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkflow;

			AssertEquals(true, BMSTestHelper.HandleTaskAssignment(otherResourceChannel, Factory, viewModel, task2Content, resourceChannel, dialogProvider, viewModel));
			AssertEquals(otherResource.GS_Code, task1_withCapabilityOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(otherResource.GS_Code, task2_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
			AssertEquals(otherResource.GS_Code, task3_withResourceOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(otherResource.GS_Code, task4_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task5_withResourceOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals("SUS", task5_withResourceOnly.P9_Status);
			AssertEquals(resource.GS_Code, task6_withResourceOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals("WRK", task6_withResourceOnly.P9_Status);
			AssertEquals(resource.GS_Code, task7_withCapabilityandResource.P9_GS_NKAssignedStaffMember);
			AssertEquals("SUS", task7_withCapabilityandResource.P9_Status);
			AssertEquals(otherResource.GS_Code, task8_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
			AssertEquals("ASN", task8_withCapabilityAndResource.P9_Status);

			AssertEquals("Would you like to assign tasks related to 'task2' that are not yet started to Yam Man?", dialogProvider.LastMessage);
		}

		public void TestDragDrop_BatchAssignment_TasksAssignedToCapability()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "BLE";

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var otherResource = CreateStaffInCurrentBranchDept("Yam", "Yam Man", capability);

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, capability: capability, description: "task1");
			var task2_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task2");
			var task3_withResourceOnly = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, description: "Task3");
			var task4_withCapabilityOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, capability: capability, description: "task4");
			task3_withResourceOnly.P9_GG_AssignedGroup = group.PK;
			task2_withCapabilityAndResource.P9_GG_AssignedGroup = group.PK;

			var workflow2 = CreateWorkflow(workflow.JobHeader, "The mighty Panly");
			var task = CreateTask(workflow2, resource.GS_Code, 90, description: "Don't mess with yam man");

			Factory.Save();

			var task2Content = new TaskCardContent(task2_withCapabilityAndResource, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var otherResourceChannel = viewModel.CreateChannelForTest(otherResource);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.TasksAssignedToCapability;

			AssertEquals(true, BMSTestHelper.HandleTaskAssignment(otherResourceChannel, Factory, viewModel, task2Content, resourceChannel, dialogProvider, viewModel));
			AssertEquals(otherResource.GS_Code, task1_withCapabilityOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(otherResource.GS_Code, task2_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
			AssertEquals(otherResource.GS_Code, task4_withCapabilityOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3_withResourceOnly.P9_GS_NKAssignedStaffMember);

			AssertEquals("Would you like to assign tasks related to 'task2' that are not yet started to Yam Man?", dialogProvider.LastMessage);
		}

		public void TestDragDrop_BatchAssignment_TasksAssignedToCapability_ShouldNotReassignOrReopenClosedTasks()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "BLE";

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var staff1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var staff2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins", capability);

			BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, staff1.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, staffCode: staff2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, staffCode: string.Empty, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);
			var task3 = BMSTestHelper.CreateTask(workflow, staffCode: string.Empty, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability);

			Factory.Save();

			var task2Content = new TaskCardContent(task2, viewModel);

			var staff1Channel = viewModel.CreateChannelForTest(staff1);
			var staff2Channel = viewModel.CreateChannelForTest(staff2);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.TasksAssignedToCapability;

			AssertEquals(true, BMSTestHelper.HandleTaskAssignment(staff1Channel, Factory, viewModel, task2Content, staff2Channel, dialogProvider, viewModel));
			AssertEquals("The task we dragged should have been assigned.", "FRO", task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("The closed task should not have been reassigned.", "BIL", task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("The closed task should not have been reopened.", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
		}

		public void TestDragDrop_BatchAssignment_None()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			resource.GS_FriendlyName = "Frodo";

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 90, capability: capability, description: "task1");
			var task2_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task2");
			var task3_withResourceOnly = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, description: "Task3");

			Factory.Save();

			var task2Content = new TaskCardContent(task2_withCapabilityAndResource, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var capabilityChannel = viewModel.CreateChannelForTest(capability);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.None;

			AssertEquals(false, BMSTestHelper.HandleTaskAssignment(capabilityChannel, Factory, viewModel, task2Content, resourceChannel, dialogProvider, viewModel));

			AssertEquals(@"Would you like to return task 'task2' to the capability 'Saying Boo-urns'?

This will remove the assignment of Frodo Baggins from the task.", dialogProvider.LastMessage);
		}

		public void TestDragDrop_FromResourceToCapability_ResourceNotInAChannel_CapabilityIsChannelled_ShouldNotRemoveResourceFromTask_ShouldNotThrowException()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			resource.GS_FriendlyName = "Frodo";

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task_withCapabilityAndResource = CreateTask(workflow, staffCode: resource.GS_Code, lowEstMinutes: 90, capability: capability, description: "task");

			Factory.Save();

			var taskContent = new TaskCardContent(task_withCapabilityAndResource, viewModel);

			var capabilityChannel = viewModel.CreateChannelForTest(capability);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.None;

			AssertEquals(false, BMSTestHelper.HandleTaskAssignment(capabilityChannel, Factory, viewModel, taskContent, dialogProvider: dialogProvider, sourceSectionViewModel: viewModel));
			AssertEquals(resource.GS_Code, task_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
			AssertNull(dialogProvider.LastMessage);
		}

		#endregion

		#region Resource Assignment

		public void TestDragDrop_CantReassignToResourcesWithoutCapability()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var capability = BMSTestHelper.CreateCapability(Factory, "CP1", "capability");
			var resource1 = CreateStaffInCurrentBranchDept("RS1", "Resource with capability");
			resource1.Capabilities.Add(capability);
			var resource2 = CreateStaffInCurrentBranchDept("RS2", "Resource without capability");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource1.GS_Code, 90, capability: capability);

			Factory.Save();

			var task2 = CreateTask(workflow, resource1.GS_Code, 90);
			AssertEquals(false, task2.IsInDatabase);

			RowFactory.ResetCacheAfterDbUpgrade();

			var newFactoryForHitCountTest = Factory.CreateNewFactory();
			var loadedBucketSection = newFactoryForHitCountTest.Load<BMBoardSection>(boardConfig.BucketSection.PK);
			var loadedTask = newFactoryForHitCountTest.Load<ProcessTask>(task1.PK);
			var loadedResource1 = newFactoryForHitCountTest.Load<GlbStaff>(resource1.PK);
			var loadedResource2 = newFactoryForHitCountTest.Load<GlbStaff>(resource2.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedBucketSection);

			newFactoryForHitCountTest.ResetDatabaseLoadCount();

			var task1Content = new TaskCardContent(loadedTask, viewModel);

			var visualBoardChannel1 = viewModel.CreateChannelForTest(loadedResource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(loadedResource2);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;

			using (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, BMSTestHelper.HandleTaskAssignment(visualBoardChannel2, Factory, viewModel, task1Content, visualBoardChannel1, dialogProvider, viewModel));
				AssertEquals("Resource without capability does not have the capability to work on 'capability' tasks", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var hits = new Dictionary<string, int>
			{
				{ GlbCapabilitySchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 0 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
			};

			AssertDbHits(hits, newFactoryForHitCountTest);
		}

		public void TestDragDrop_CantReassignToResourcesWithoutCapability_AllTasksInWorkflow()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var capability = BMSTestHelper.CreateCapability(Factory, "CP1", "capability");
			var resource1 = CreateStaffInCurrentBranchDept("RS1", "Resource with capability");
			resource1.Capabilities.Add(capability);
			var resource2 = CreateStaffInCurrentBranchDept("RS2", "Resource without capability");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource1.GS_Code, 90);
			var task2 = CreateTask(workflow, resource1.GS_Code, 90, capability: capability);

			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			var newFactoryForHitCountTest = Factory.CreateNewFactory();
			var loadedBucketSection = newFactoryForHitCountTest.Load<BMBoardSection>(boardConfig.BucketSection.PK);
			var loadedTask = newFactoryForHitCountTest.Load<ProcessTask>(task1.PK);
			var loadedResource1 = newFactoryForHitCountTest.Load<GlbStaff>(resource1.PK);
			var loadedResource2 = newFactoryForHitCountTest.Load<GlbStaff>(resource2.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedBucketSection);

			var task1Content = new TaskCardContent(loadedTask, viewModel);

			var visualBoardChannel1 = viewModel.CreateChannelForTest(loadedResource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(loadedResource2);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkflow;

			newFactoryForHitCountTest.ResetDatabaseLoadCount();

			using (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, BMSTestHelper.HandleTaskAssignment(visualBoardChannel2, Factory, viewModel, task1Content, visualBoardChannel1, dialogProvider, viewModel));
				AssertEquals(@"Resource without capability does not have the capability to do the following tasks, and therefore they have not been reassigned:
Task T00001001 Undefined - You can modify this in the System Reg".StripTaskIds(), UnitTestUserNotification.Instance.LastMessage.Text.StripTaskIds());
			}

			var hits = new Dictionary<string, int>
			{
				{ GlbCapabilitySchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 0 },
			};

			AssertDbHits(hits, newFactoryForHitCountTest);
		}

		public void TestDragDrop_CantReassignToResourcesWithoutCapability_TasksAssignedToStaff()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability = BMSTestHelper.CreateCapability(Factory, "CP1", "capability");
			var resource1 = CreateStaffInCurrentBranchDept("RS1", "Resource with capability");
			resource1.Capabilities.Add(capability);
			var resource2 = CreateStaffInCurrentBranchDept("RS2", "Resource without capability");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource1.GS_Code, 90);
			var task2 = CreateTask(workflow, resource1.GS_Code, 90, capability: capability);

			Factory.Save();

			var task1Content = new TaskCardContent(task1, viewModel);

			var visualBoardChannel1 = viewModel.CreateChannelForTest(resource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(resource2);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.TasksAssignedToStaff;

			using (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, BMSTestHelper.HandleTaskAssignment(visualBoardChannel2, Factory, viewModel, task1Content, visualBoardChannel1, dialogProvider, viewModel));
				AssertEquals(@"Resource without capability does not have the capability to do the following tasks, and therefore they have not been reassigned:
Task T00001001 Undefined - You can modify this in the System Reg".StripTaskIds(), UnitTestUserNotification.Instance.LastMessage.Text.StripTaskIds());
			}
		}

		public void TestDragDrop_ShouldNotSaveOriginalFactory()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var resource1 = CreateStaffInCurrentBranchDept("AAA", "Adam state");
			var resource2 = CreateStaffInCurrentBranchDept("EEE", "EEEEEE");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource1.GS_Code, 90);

			Factory.Save();

			var task2 = CreateTask(workflow, resource1.GS_Code, 90);
			AssertEquals(false, task2.IsInDatabase);

			var task1Content = new TaskCardContent(task1, viewModel);

			var visualBoardChannel1 = viewModel.CreateChannelForTest(resource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(resource2);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
			AssertEquals(true, BMSTestHelper.HandleTaskAssignment(visualBoardChannel2, Factory, viewModel, task1Content, visualBoardChannel1, dialogProvider, viewModel));
			AssertEquals(false, task2.IsInDatabase);
		}

		public void TestDragDrop_ResourceToCapabilityChannel_ShouldRemoveStaffAssignment()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			resource.GS_FriendlyName = "Frodo";

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, string.Empty, 90, capability: capability);
			var task2_withCapabilityAndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability);
			var task3_withResourceOnly = CreateTask(workflow, resource.GS_Code, 90);
			task1_withCapabilityOnly.P9_Description = "task1";
			task2_withCapabilityAndResource.P9_Description = "task2";
			task3_withResourceOnly.P9_Description = "task3";
			Factory.Save();

			var task1Content = new TaskCardContent(task1_withCapabilityOnly, viewModel);
			var task2Content = new TaskCardContent(task2_withCapabilityAndResource, viewModel);
			var task3Content = new TaskCardContent(task3_withResourceOnly, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var capabilityChannel = viewModel.CreateChannelForTest(capability);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			AssertEquals("Task1 is already assigned to capability and has no staff record - nothing to do.", false, BMSTestHelper.HandleTaskAssignment(capabilityChannel, Factory, viewModel, task1Content, resourceChannel, dialogProvider, viewModel));

			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
			AssertEquals("Task2 can be handed back to the capability pool", true, BMSTestHelper.HandleTaskAssignment(capabilityChannel, Factory, viewModel, task2Content, resourceChannel, dialogProvider, viewModel));
			AssertEquals(string.Empty, task2_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
			AssertMultilineASCIIEquals("",
@"Would you like to return task 'task2' to the capability 'Saying Boo-urns'?

This will remove the assignment of Frodo Baggins from the task.", dialogProvider.LastMessage);

			AssertEquals("Task3 can be added to the capability and resource should be removed", true, BMSTestHelper.HandleTaskAssignment(capabilityChannel, Factory, viewModel, task3Content, resourceChannel, dialogProvider, viewModel));
			AssertMultilineASCIIEquals("",
@"Would you like to assign task 'task3' to the capability 'Saying Boo-urns'?

This will remove the assignment of Frodo Baggins from the task.", dialogProvider.LastMessage);
			AssertEquals(string.Empty, task3_withResourceOnly.P9_GS_NKAssignedStaffMember);
			AssertEquals(capability.PK, task3_withResourceOnly.P9_G4_RequiredCapability);
		}

		public void TestDragDrop_CapabilityToCapabilityChannel_ShouldLeaveStaffAssignment()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var capability1 = Factory.New<GlbCapability>();
			capability1.G4_Description = "Saying Boo-urns";
			capability1.G4_Code = "BOO";

			var capability2 = Factory.New<GlbCapability>();
			capability2.G4_Description = "Reciting the alphabet";
			capability2.G4_Code = "ABC";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);
			resource.GS_FriendlyName = "Frodo";

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, string.Empty, 90, capability: capability1);
			var task2_withCapabilityAndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability1);
			task1_withCapabilityOnly.P9_Description = "task1";
			task2_withCapabilityAndResource.P9_Description = "task2";
			Factory.Save();

			var task1Content = new TaskCardContent(task1_withCapabilityOnly, viewModel);
			var task2Content = new TaskCardContent(task2_withCapabilityAndResource, viewModel);

			var resourceChannel = viewModel.CreateChannelForTest(resource);
			var capability2Channel = viewModel.CreateChannelForTest(capability2);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
			AssertEquals("Should reasssign task1 capability - resource is already blank", true, BMSTestHelper.HandleTaskAssignment(capability2Channel, Factory, viewModel, task1Content, resourceChannel, dialogProvider, viewModel));
			AssertEquals("Would you like to assign task 'task1' to the capability 'Reciting the alphabet'?", dialogProvider.LastMessage);
			AssertEquals(capability2, task1_withCapabilityOnly.RequiredCapability);
			AssertEquals(string.Empty, task1_withCapabilityOnly.P9_GS_NKAssignedStaffMember);

			AssertEquals("Should reassign task2 capability and clear assigned resource", true, BMSTestHelper.HandleTaskAssignment(capability2Channel, Factory, viewModel, task2Content, resourceChannel, dialogProvider, viewModel));
			AssertMultilineASCIIEquals("",
@"Would you like to assign task 'task2' to the capability 'Reciting the alphabet'?

This will remove the assignment of Frodo Baggins from the task, and replace the existing required capability of 'Saying Boo-urns'.", dialogProvider.LastMessage);
			AssertEquals(capability2, task2_withCapabilityAndResource.RequiredCapability);
			AssertEquals(string.Empty, task2_withCapabilityAndResource.P9_GS_NKAssignedStaffMember);
		}

		#endregion

		public void TestDbHitsOnCapability()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var capability = BMSTestHelper.CreateCapability(Factory, "CP1", "capability 1");
			var resource1 = CreateStaffInCurrentBranchDept("RS1", "Resource One");
			resource1.Capabilities.Add(capability);
			var capability2 = BMSTestHelper.CreateCapability(Factory, "CP2", "capability 1");
			var resource2 = CreateStaffInCurrentBranchDept("RS2", "Resource Two");
			resource2.Capabilities.Add(capability);
			resource2.Capabilities.Add(capability2);
			var resource3 = CreateStaffInCurrentBranchDept("RS3", "Resource Three");
			resource3.Capabilities.Add(capability);

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);
			var channel3 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			Factory.Save();

			var newFactoryForHitCountTest = Factory.CreateNewFactory();
			var section = Factory.Load<BMBoardSection>(boardConfig.BucketSection.PK);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var c1 = viewModel.CreateChannelForTest(resource1);
			var c2 = viewModel.CreateChannelForTest(resource2);
			var c3 = viewModel.CreateChannelForTest(resource3);

			VisualBoardChannelFetchHintProvider.FetchChannelEntities(newFactoryForHitCountTest, new[] { c1, c2, c3 });

			var hits = new Dictionary<string, int>
			{
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 }
			};

			AssertDbHits(hits, newFactoryForHitCountTest);
		}

		#region Tags

		public void TestDragDrop_BetweenComponents_TagChannels()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var board = CreateBoard(config.System);
			var bucketSection = CreateBoardSection(config.Bucket, board);
			var bufferSection = CreateBoardSection(config.Buffer, board);

			var bucketViewModel = BMSTestHelper.CreateViewModel(bucketSection);
			var bufferViewModel = BMSTestHelper.CreateViewModel(bufferSection);

			var channel1_1 = BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Tag, config.RedTag.PK);
			var channel1_2 = BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Tag, config.PlatinumTag.PK);

			var channel2_1 = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Tag, config.RedTag.PK);
			var channel2_2 = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Tag, config.PlatinumTag.PK);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			workflow1.AddTag(config.RedTag);
			workflow2.AddTag(config.PlatinumTag);

			var task1 = CreateTask(workflow1, string.Empty, 60);
			var task2 = CreateTask(workflow2, string.Empty, 60);

			Factory.Save();

			var task1Content = new TaskCardContent(task1, bucketViewModel);
			var task2Content = new TaskCardContent(task2, bucketViewModel);

			var redBucketChannel = bucketViewModel.CreateChannelForTest(config.RedTag);
			var pltBucketChannel = bucketViewModel.CreateChannelForTest(config.PlatinumTag);

			var redBufferChannel = bufferViewModel.CreateChannelForTest(config.RedTag);
			var pltBufferChannel = bufferViewModel.CreateChannelForTest(config.PlatinumTag);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			AssertEquals("Cannot move tasks between tag channels in same component", false, BMSTestHelper.HandleTaskAssignment(redBucketChannel, Factory, bucketViewModel, task2Content, pltBucketChannel, dialogProvider, bucketViewModel));
			AssertEquals("Cannot move tasks between tag channels in same component", false, BMSTestHelper.HandleTaskAssignment(pltBucketChannel, Factory, bucketViewModel, task1Content, redBucketChannel, dialogProvider, bucketViewModel));

			AssertEquals("Moving task to channel for same tag in another component is supported", true, BMSTestHelper.HandleTaskAssignment(redBufferChannel, Factory, bufferViewModel, task1Content, redBucketChannel, dialogProvider, bufferViewModel));
			AssertEquals("Moving task to channel for same tag in another component is supported", true, BMSTestHelper.HandleTaskAssignment(pltBufferChannel, Factory, bufferViewModel, task2Content, pltBucketChannel, dialogProvider, bufferViewModel));

			AssertEquals("Moving task to channel for different tag in another component is supported", true, BMSTestHelper.HandleTaskAssignment(redBufferChannel, Factory, bufferViewModel, task1Content, redBucketChannel, dialogProvider, bufferViewModel));
			AssertEquals("Moving task to channel for different tag in another component is supported", true, BMSTestHelper.HandleTaskAssignment(pltBufferChannel, Factory, bufferViewModel, task2Content, pltBucketChannel, dialogProvider, bufferViewModel));
		}

		public void TestDragDrop_TagChannelToResourceChannel()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, config.RedTag.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = CreateTask(workflow, string.Empty, 60);

			workflow.AddTag(config.RedTag);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var taskContent = new TaskCardContent(task, viewModel);

			var tagChannel = viewModel.CreateChannelForTest(config.RedTag);
			var resourceChannel = viewModel.CreateChannelForTest(resource);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			AssertEquals("Cannot move tasks between tag channel and another type of channel", false, BMSTestHelper.HandleTaskAssignment(resourceChannel, Factory, viewModel, taskContent, tagChannel, dialogProvider, viewModel));
			AssertNull(dialogProvider.LastMessage);
		}

		public void TestDragDrop_ResourceChannelToTagChannel()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, config.RedTag.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = CreateTask(workflow, string.Empty, 60);

			workflow.AddTag(config.RedTag);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var taskContent = new TaskCardContent(task, viewModel);

			var tagChannel = viewModel.CreateChannelForTest(config.RedTag);
			var resourceChannel = viewModel.CreateChannelForTest(resource);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			AssertEquals("Cannot move tasks between tag channel and another type of channel", false, BMSTestHelper.HandleTaskAssignment(tagChannel, Factory, viewModel, taskContent, resourceChannel, dialogProvider, viewModel));
			AssertNull(dialogProvider.LastMessage);
		}

		#endregion

		#region Workflow Card

		public void TestDragDrop_WorkflowCardIntoAnyChannelTypeInAnotherComponent()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2);

			var board = system.Boards.AddNew();
			var section1 = CreateBoardSection(bucket1, board);
			var section2 = CreateBoardSection(bucket2, board);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Nnnnnow!", bucket1, releaseGroupPK: group.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, capability: capability);

			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Group, group.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Capability, capability.PK);

			Factory.Save();

			var viewModel1 = BMSTestHelper.CreateViewModel(section1);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel2);
			var sourceCell = viewModel1.ComponentGrid.CardCells.Single();

			var groupDestCell = viewModel2.ComponentGrid.CardCells.Single(c => c.Channel.EntityPK == group.PK);
			var resourceDestCell = viewModel2.ComponentGrid.CardCells.Single(c => c.Channel.EntityPK == resource.PK);
			var capabilityDestCell = viewModel2.ComponentGrid.CardCells.Single(c => c.Channel.EntityPK == capability.PK);

			const string message = "Should be able to move a workflow from one component to another - regardless of the type of destination channel";

			MoveToComponentAndAssertCanTransfer(message, cardContent, sourceCell, groupDestCell, viewModel2, viewModel1);
			MoveToComponentAndAssertCanTransfer(message, cardContent, sourceCell, resourceDestCell, viewModel2, viewModel1);
			MoveToComponentAndAssertCanTransfer(message, cardContent, sourceCell, capabilityDestCell, viewModel2, viewModel1);
		}

		public void TestDragDrop_JobWorkflowCardIntoAnyChannelTypeInAnotherComponent()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2);

			var board = system.Boards.AddNew();
			var section1 = CreateBoardSection(bucket1, board);
			section1.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			var section2 = CreateBoardSection(bucket2, board);
			section2.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Test 1", bucket1, releaseGroupPK: group.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, capability: capability);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Test 2", bucket2, releaseGroupPK: group.PK);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60, capability: capability);

			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Group, group.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Capability, capability.PK);

			Factory.Save();

			var viewModel1 = BMSTestHelper.CreateViewModel(section1);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2);

			var cardContent = new WorkflowCardContent(workflow, task, viewModel1);
			var sourceCell = viewModel1.ComponentGrid.CardCells.Single();

			var groupDestCell = viewModel2.ComponentGrid.CardCells.Single(c => c.Channel.EntityPK == group.PK);
			var resourceDestCell = viewModel2.ComponentGrid.CardCells.Single(c => c.Channel.EntityPK == resource.PK);
			var capabilityDestCell = viewModel2.ComponentGrid.CardCells.Single(c => c.Channel.EntityPK == capability.PK);

			const string message = "Should be able to move a workflow from one component to another - regardless of the type of destination channel";

			var sourceComponent = Factory.Load<BMComponent>(viewModel2.ComponentPK);
			MoveToComponentAndAssertCanTransfer(message, cardContent, sourceCell, groupDestCell, viewModel1, viewModel2);

			sourceComponent = Factory.Load<BMComponent>(viewModel1.ComponentPK);
			MoveToComponentAndAssertCanTransfer(message, cardContent, sourceCell, resourceDestCell, viewModel2, viewModel1);

			//moving with additional components
			var buffer_additionalComponent = BMSTestHelper.CreateBuffer(system, "Another Buffer");
			CreateZoneMultiplier(buffer_additionalComponent, zone0Multiplier: 20, zone1Multiplier: 10, zone2Multiplier: 5, zone3Multiplier: 2);

			var workflow_additionalComponent = BMSTestHelper.CreateWorkflow(jobHeader, "Test 1", buffer_additionalComponent, releaseGroupPK: group.PK);
			var task_additionalComponent = CreateTask(workflow_additionalComponent, GlbStaff.CurrentUser.GS_Code, 60, capability: capability);

			section1.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer_additionalComponent.PK;
			viewModel1 = BMSTestHelper.CreateViewModel(section1);

			cardContent = new WorkflowCardContent(workflow, task, viewModel1);
			sourceCell = viewModel1.ComponentGrid.CardCells.Single();
			AssertNotEquals("Precondition: to ensure workflow_additionalComponent belongs to additional component", viewModel2.ComponentPK, workflow_additionalComponent.FH_FC_CurrentComponent);
			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			var result = new TicketDragDropHandler(Factory, viewModel2, sourceCell, resourceDestCell, cardContent, dialogProvider, viewModel1).TryMoveToDestinationCell();

			AssertEquals(true, result);
			AssertEquals(viewModel2.ComponentPK, workflow.FH_FC_CurrentComponent);
			AssertEquals(viewModel2.ComponentPK, workflow_additionalComponent.FH_FC_CurrentComponent);
		}

		void MoveToComponentAndAssertCanTransfer(string message, WorkflowCardContent cardContent, CellContent sourceCell, CellContent destinationCell, BMBoardSectionViewModel viewModel, BMBoardSectionViewModel sourceViewModel, bool canMove = true)
		{
			var startingComponentPK = cardContent.Workflow.FH_FC_CurrentComponent;
			cardContent.Workflow.FH_FC_CurrentComponent = viewModel.ComponentPK;
			try
			{
				var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
				var result = new TicketDragDropHandler(Factory, viewModel, sourceCell, destinationCell, cardContent, dialogProvider, sourceViewModel).TryMoveToDestinationCell();

				AssertEquals(message, canMove, result);
				AssertEquals(viewModel.ComponentPK, cardContent.Workflow.FH_FC_CurrentComponent);
			}
			finally
			{
				cardContent.Workflow.FH_FC_CurrentComponent = startingComponentPK;
			}
		}

		#endregion

		#endregion
	}
}
