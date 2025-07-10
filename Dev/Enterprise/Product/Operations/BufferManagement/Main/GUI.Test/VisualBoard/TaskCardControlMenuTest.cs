using System;
using System.Collections.Generic;
using System.Data;
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
using Enterprise.MasterFiles.GUI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TaskCardControlMenuTest : BMSGUITestCase
	{
		#region JobCardType

		public void TestMenu_OperationalActions_ShouldNotShowWorkflowAndTaskMenu_WhenJobCardType()
		{
			CreateWorkflowAndTask();
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			CreateAction(ModuleIDs.ProcessHeader);
			CreateAction(ModuleIDs.ProcessTasks);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var operationalActionsMenu = GetOperationActionMenu(form);
				AssertNotNull("Should have operationalActionsMenu", operationalActionsMenu);

				var taskActionMenu = GetActionMenu(operationalActionsMenu, "Task");
				AssertNull("Should not have taskActionMenu", taskActionMenu);

				var workflowMenu = GetActionMenu(operationalActionsMenu, "Workflow");
				AssertNull("Should not have WorkflowActionMenu", workflowMenu);
			}
		}

		public void TestMenu_OperationalActions_ForJobCard_WhenJobSupportOperationAction_ShouldShowMenu()
		{
			var jobHeader = CreateWorkflowAndTask();
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var controller = WorkflowProviderHelper.GetControllerForWorkflowType(jobHeader.Parent.WorkflowType);
			var action = CreateAction(controller.ModuleID);

			using (var module = ZFilterModule.GetZFilterModule(controller.ModuleID) as ZFilterGridModule)
			{
				Assert("Precondition: Module support OperationAction", module is IOperationalActionSupportable);
			}

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var operationalActionsMenu = GetOperationActionMenu(form);
				AssertNotNull("Should have operationalActionsMenu", operationalActionsMenu);

				var jobMenuAction = GetActionMenu(operationalActionsMenu, "Job");
				AssertNotNull("Should have workflowActionMenu", jobMenuAction);

				AssertMenuHaveActionAndCustomizeItens(jobMenuAction, action);
				AssertShowFormWithAction(jobMenuAction, action);
			}
		}

		public void TestMenu_OperationalActions_ForJobCard_WhenJobNotSupportOperationAction_ShouldShowDisabledMenu()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>();
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader, "BLA", config.Buffer);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			WorkflowDescriptors.Instance.TryGetValue(jobHeader.Parent.WorkflowType, out var descriptor);
			(descriptor as DummyWorkflowDescriptor).OverriddenControllerID = DummyControllerIDs.Dummy;

			Assert("Precondition: Module should not support OperationAction", !(DummyControllerIDs.Dummy is IOperationalActionSupportable));

			Factory.Save();

			jobHeader.FH_FC_CurrentComponent = config.Buffer.PK;

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var operationalActionsMenu = GetOperationActionMenu(form);
				AssertNotNull("Should have operationalActionsMenu", operationalActionsMenu);

				var jobActionMenu = GetActionMenu(operationalActionsMenu, "Job");
				AssertNotNull("Should have JobActionMenu", jobActionMenu);

				var disabledMenuItem = jobActionMenu.DropDownItems.Cast<ZToolStripMenuItem>().Single();

				AssertNotNull("Should have DisabledMenuItem", disabledMenuItem);
				AssertEquals("Should have Caption: Operational Actions are not enabled for Process Type 'DUM'", disabledMenuItem.Text, "Operational Actions are not enabled for Process Type 'DUM'");
				Assert("DisabledMenuItem should be disable!", !disabledMenuItem.Enabled);
			}
		}

		#endregion

		#region WorkflowCardType

		public void TestMenu_OperationalActions_ShouldNotShowTaskMenu_WhenWorkflowCardType()
		{
			CreateWorkflowAndTask();
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var operationalActionsMenu = GetOperationActionMenu(form);
				AssertNotNull("Should have operationalActionsMenu", operationalActionsMenu);

				var workflowActionMenu = GetActionMenu(operationalActionsMenu, "Workflow");
				AssertNotNull("Should have workflowActionMenu", workflowActionMenu);

				var taskActionMenu = GetActionMenu(operationalActionsMenu, "Task");
				AssertNull("Should not have taskActionMenu", taskActionMenu);
			}
		}

		public void TestMenu_OperationalActions_ForWorkflowCard()
		{
			CreateWorkflowAndTask();
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var action = CreateAction(ModuleIDs.ProcessHeader);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var operationalActionsMenu = GetOperationActionMenu(form);
				AssertNotNull("Should have operationalActionsMenu", operationalActionsMenu);

				var workflowActionMenu = GetActionMenu(operationalActionsMenu, "Workflow");
				AssertNotNull("Should have workflowActionMenu", workflowActionMenu);

				AssertMenuHaveActionAndCustomizeItens(workflowActionMenu, action);
				AssertShowFormWithAction(workflowActionMenu, action);
			}
		}

		#endregion

		#region TaskCardType

		public void TestMenu_OperationalActions_ForTaskCard()
		{
			CreateWorkflowAndTask();
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var taskAction = CreateAction(ModuleIDs.ProcessTasks);
			var workflowAction = CreateAction(ModuleIDs.ProcessHeader);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var operationalActionsMenu = GetOperationActionMenu(form);
				AssertNotNull("Should have operationalActionsMenu", operationalActionsMenu);

				var workflowActionMenu = GetActionMenu(operationalActionsMenu, "Workflow");
				AssertNotNull("Should have workflowActionMenu", workflowActionMenu);

				AssertMenuHaveActionAndCustomizeItens(workflowActionMenu, workflowAction);
				AssertShowFormWithAction(workflowActionMenu, workflowAction);

				var taskActionMenu = GetActionMenu(operationalActionsMenu, "Task");
				AssertNotNull("Should have taskActionMenu", taskActionMenu);

				AssertMenuHaveActionAndCustomizeItens(taskActionMenu, taskAction);
				AssertShowFormWithAction(taskActionMenu, taskAction);
			}
		}

		#region Assist With This Task

		public void TestAssistWithThisTaskMenuItem_ForTaskCards_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			var currentUser = Env.CurrentUser;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser.Initials, 10, "INV", sequence: 20, description: "My working task", estVariationFactor: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, currentUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(task1, form, "Assist With This Task");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				var myTaskCards = form.FindAll<TaskCardControl>(x => x.CardContent.GetTask(Factory).P9_GS_NKAssignedStaffMember == currentUser.Initials);
				AssertContainsExactElementsInAnyOrder("An assist task should have been created in my channel.", new[] { "My working task", "Assist" },
					myTaskCards.Select(x => x.CardContent.GetTask(Factory).P9_Description));
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("An assist task should have been created and saved.", new[] { "Other user's task", "My working task", "Assist" }, tasks.Select(x => x.P9_Description));

			var assistTask = tasks.Single(x => x.P9_Description == "Assist");
			AssertEquals("AST", assistTask.P9_Type);
			AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
			AssertEquals(10, assistTask.P9_Sequence);
			AssertEquals(currentUser.Initials, assistTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, assistTask.P9_Status);
			AssertEquals("Assist", assistTask.P9_Description);

			var previouslyWorkingTask = tasks.Single(x => x.P9_Description == "My working task");
			AssertEquals("The system automatically set the assist task to WRK, so the previously working task should now be suspended.", ProcessTaskStatusCodeList.Codes.Suspended, previouslyWorkingTask.P9_Status);
		}

		public void TestAssistWithThisTaskMenuItem_WhenWorkflowTypeNotConfiguredInRegistry_ShouldNotAppearInMenu()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.AddTaskTypesToRegistry("INQ", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("INQ", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(task1, form, "Assist With This Task");
				AssertNull("Assist With This Task isn't set up in the registry for the ORG workflow type, so this menu item shouldn't appear for this task.", menuItem);
			}
		}

		public void TestAssistWithThisTaskMenuItem_ShouldNotAppearOnCardsForTasksAssignedToTheCurrentUser()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			var currentUser = Env.CurrentUser;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser.Initials, 10, "INV", sequence: 20, description: "My task", estVariationFactor: 2);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, currentUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(task2, form, "Assist With This Task");
				AssertNull("Assist With This Task shouldn't appear on tasks that are assigned to the current user, despite the often-quoted mantra of 'How can you help others if you can't even help yourself?'", menuItem);
			}
		}

		public void TestAssistWithThisTask_ForWorkflowCards_ShouldNotAppear()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(workflow, form, "Assist With This Task");
				AssertNull("Assist With This Task shouldn't appear for workflow tickets because that would make no sense.", menuItem);
			}
		}

		public void TestAssistWithThisTask_ForJobCards_ShouldNotAppear()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(workflow.JobHeader, form, "Assist With This Task");
				AssertNull("Assist With This Task shouldn't appear for job-level workflow tickets because that would make no sense.", menuItem);
			}
		}

		#region Avoid Duplication of Assistance Tasks When Using Assist With This Task

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusWRK_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "WRK";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusSUS_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "SUS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusASN_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}
		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusOPN_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "OPN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsClosed()
		{
			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "CLS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsCancelled()
		{
			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "CAN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithDifferentSequenceNumberButStartable()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 1;
				Assert("Precondition", assistanceTask.P9_Sequence < sourceTask.P9_Sequence);
				Assert("Precondition", assistanceTask.IsStartable());
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsNotStartable_AndHasDifferentSequenceNumber()
		{
			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 100;
				Assert("Precondition", assistanceTask.P9_Sequence > sourceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGlobalCapabilityCurrentUserPossesses()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: false);
			capability.ResourcesWithCapability.Add(currentUser);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			Assert("Precondition", !group.Staff.Select(s => s.PK).Contains(currentUser.PK));

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGlobalCapabilityCurrentUserDoesNotPossess()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: false);
			Assert("Precondition", !capability.ResourcesWithCapability.Select(s => s.PK).Contains(currentUser.PK));

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(currentUser);

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", workflow.FH_GG_ReleaseGroup.IsEmpty);

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGroupCapabilityCurrentUserPossesses_WhenTaskIsAssignedToUserGroup()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(currentUser);

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGroupCapabilityCurrentUserPossesses_ButTaskGroupDoesNotContainCurrentUser()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			Assert("Precondition", !group.Staff.Select(s => s.PK).Contains(currentUser.PK));

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGroupCapabilityCurrentUserPossesses_WhenWorkflowIsAssignedToUserReleaseGroup()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "RG1", "Release group");
			releaseGroup.Staff.Add(currentUser);

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				Assert("Precondition", assistanceTask.P9_GG_AssignedGroup.IsEmpty);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGroupCapabilityCurrentUserPossesses_ButWorkflowReleaseGroupDoesNotContainCurrentUser()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "RG1", "Release group");
			Assert("Precondition", !releaseGroup.Staff.Select(s => s.PK).Contains(currentUser.PK));

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				Assert("Precondition", assistanceTask.P9_GG_AssignedGroup.IsEmpty);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGroupCapabilityCurrentUserPossesses_WhenThereIsNeitherTaskNorReleaseGroup()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "RG1", "Release group");
			releaseGroup.Staff.Add(currentUser);

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", workflow.FH_GG_ReleaseGroup.IsEmpty);

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				Assert("Precondition", assistanceTask.P9_GG_AssignedGroup.IsEmpty);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGroupCapabilityCurrentUserDoesNotPossess()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			Assert("Precondition", !capability.ResourcesWithCapability.Select(s => s.PK).Contains(currentUser.PK));

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(currentUser);

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", workflow.FH_GG_ReleaseGroup.IsEmpty);

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		void AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			var currentUser = (GlbStaff)Env.CurrentUser;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			var assistanceTaskInitialSequence = assistanceTask.P9_Sequence;

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, currentUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(sourceTask, form, "Assist With This Task");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("An assist task should be used instead of creating a new task.", new[] { "Other user's task", "Existing assistance task" }, tasks.Select(x => x.P9_Description));

			var existingTask = tasks.Single(x => x.P9_Description == "Existing assistance task");
			AssertEquals("AST", existingTask.P9_Type);
			AssertEquals(15, (ZInt)existingTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(3m, existingTask.P9_EstimateVariationFactor);
			AssertEquals(assistanceTaskInitialSequence, existingTask.P9_Sequence);
			AssertEquals(currentUser.GS_Code, existingTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, existingTask.P9_Status);
			AssertEquals("Existing assistance task", existingTask.P9_Description);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void AssertAssistWithThisTask_CreatesNewAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			var currentUser = (GlbStaff)Env.CurrentUser;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, currentUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(sourceTask, form, "Assist With This Task");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("A new assist task should have been created and saved.", new[] { "Other user's task", "Existing assistance task", "Assist" }, tasks.Select(x => x.P9_Description));

			var newTask = tasks.Single(x => x.P9_Description == "Assist");
			AssertEquals("AST", newTask.P9_Type);
			AssertEquals(defaultAssistanceTaskEstimate, (int)newTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals((ZDecimal)defaultAssistanceTaskVariationFactor, newTask.P9_EstimateVariationFactor);
			AssertEquals(10, newTask.P9_Sequence);
			AssertEquals(currentUser.GS_Code, newTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, newTask.P9_Status);
			AssertEquals("Assist", newTask.P9_Description);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#endregion

		#region Add Assistance Task For

		public void TestAddAssistanceTaskForMenuItem_ForWorkflowCards_ShouldNotAppear()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(workflow, form, "Add Assistance Task For");
				AssertNull("Add Assistance Task For shouldn't appear for workflow tickets because that would make no sense", menuItem);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_ForJobCards_ShouldNotAppear()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(workflow.JobHeader, form, "Add Assistance Task For");
				AssertNull("Add Assistance Task For shouldn't appear for job-level workflow tickets because that would make no sense", menuItem);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_WhenWorkflowTypeNotConfiguredInRegistry_ShouldNotAppear()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.AddTaskTypesToRegistry("INQ", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("INQ", "AST", 20, 4);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(task, form, "Add Assistance Task For");
				AssertNull("Add Assistance Task For isn't set up in the registry for the ORG workflow type, so this menu item shouldn't appear for this task", menuItem);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_WhenWorkflowTypeIsConfiguredInRegistry_ShouldAppear()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(task, form, "Add Assistance Task For");
				AssertNotNull("Add Assistance Task For is set up in the registry for the ORG workflow type, so this menu item should appear for this task", menuItem);
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndNotAssignedToTaskGroup_AndWorkflowHasNoReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				// primary users
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU1 - Primary user 1", testConfig.PrimaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU3 - Primary user 3", testConfig.PrimaryUser3);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU4 - Primary user 4", testConfig.PrimaryUser4);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU5 - Primary user 5", testConfig.PrimaryUser5);
				AssertNoMenuItemForResourceToAssist("Should not contain a menu item for the user currently assigned to the task even if it is a primary user", staffSubMenuItem, "ASG - User assigned to the task to click", testConfig.UserAssignedToTaskToClick);

				// secondary users
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU3 - Secondary user 3", testConfig.SecondaryUser3);

				// current user
				AssertMenuItemForResourceToAssist("Should contain menu items for the current", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				// users with resource channels on another section of the same board
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU1 - Primary user for another section 1", testConfig.AnotherSectionUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU2 - Primary user for another section 2", testConfig.AnotherSectionUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU3 - Primary user for another section 3", testConfig.AnotherSectionUser3);

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS1 - Secondary user for another section 1", testConfig.AnotherSectionSecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS2 - Secondary user for another section 2", testConfig.AnotherSectionSecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndNotAssignedToTaskGroup_AndWorkflowHasNoReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				// primary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR1 - Primary capability with GRP scope 1 (Capability Channel)", testConfig.PrimaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR2 - Primary capability with GRP scope 2 (Capability Channel)", testConfig.PrimaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1 (Capability Channel)", testConfig.PrimaryGlbCapability1);

				// capabilities already in use in the current workflow
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW1 - Capability used in the same workflow 1 (Workflow)", testConfig.CapabilityUsedInSameWorkflow1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2 (Workflow)", testConfig.CapabilityUsedInSameWorkflow2);

				// secondary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR1 - Secondary capability with GRP scope 1 (Team Capability)", testConfig.SecondaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SL1 - Secondary capability with GLB scope 1 (Team Capability)", testConfig.SecondaryGlbCapability1);

				// tertiary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T1 - Tertiary capability 1 (Other)", testConfig.TertiaryCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T2 - Tertiary capability 2 (Other)", testConfig.TertiaryCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T3 - Tertiary capability 3 (Other)", testConfig.TertiaryCapability3);

				// current user capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU1 - Current user capability 1 (Current User)", testConfig.CurrentUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);

				// capabilities with capability channels on another section of the same board
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities with capability channels on another section of the same board", capabilitySubMenuItem, "AL1 - Primary GLB capability for another section 1", testConfig.AnotherSectionGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should still contain menu items for capabilities with capability channels on another section of the same board when the are also secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "AR1 - Primary GRP capability for another section 1 (Team Capability)", testConfig.AnotherSectionGrpCapability1);

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS1 - Secondary capability for another section 1", testConfig.AnotherSectionSecondaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS2 - Secondary capability for another section 2", testConfig.AnotherSectionSecondaryCapability2);

				// other capabilities used in another workflow
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW1 - Capability used in another workflow 1", testConfig.CapabilityUsedInAnotherWorkflow1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW2 - Capability used in another workflow 2", testConfig.CapabilityUsedInAnotherWorkflow2);

				// other capablities that the currently assigned user possess
				AssertMenuItemForCapabilityToAssist("Should contain menu items for other capabilities that the assigned user possesses when the assigned user is a primary user", testConfig.Workflow, capabilitySubMenuItem, "AU1 - Assigned user capability 1 (Team Capability)", testConfig.AssignedUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for other capabilities that the assigned user possesses when the assigned user is a primary user", testConfig.Workflow, capabilitySubMenuItem, "AU2 - Assigned user capability 2 (Team Capability)", testConfig.AssignedUserCapability2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"AU1 - Assigned user capability 1 (Team Capability)",
					"AU2 - Assigned user capability 2 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T1 - Tertiary capability 1 (Other)",
					"T2 - Tertiary capability 2 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsNotAssignedToResource_AndNotAssignedToTaskGroup_AndWorkflowHasNoReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(Factory, config); // so that the task appears on the board as a capability task

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				// primary users
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU1 - Primary user 1", testConfig.PrimaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU3 - Primary user 3", testConfig.PrimaryUser3);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU4 - Primary user 4", testConfig.PrimaryUser4);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU5 - Primary user 5", testConfig.PrimaryUser5);

				// secondary users
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU3 - Secondary user 3", testConfig.SecondaryUser3);

				// current user
				AssertMenuItemForResourceToAssist("Should contain menu items for the current", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				// users with resource channels on another section of the same board
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU1 - Primary user for another section 1", testConfig.AnotherSectionUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU2 - Primary user for another section 2", testConfig.AnotherSectionUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU3 - Primary user for another section 3", testConfig.AnotherSectionUser3);

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS1 - Secondary user for another section 1", testConfig.AnotherSectionSecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS2 - Secondary user for another section 2", testConfig.AnotherSectionSecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsNotAssignedToResource_AndNotAssignedToTaskGroup_AndWorkflowHasNoReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(Factory, config); // so that the task appears on the board as a capability task

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				// primary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR1 - Primary capability with GRP scope 1 (Capability Channel)", testConfig.PrimaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR2 - Primary capability with GRP scope 2 (Capability Channel)", testConfig.PrimaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1 (Capability Channel)", testConfig.PrimaryGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for the task capability as it is a primary capability", testConfig.Workflow, capabilitySubMenuItem, "TC - Task capability (Capability Channel)", testConfig.TaskCapability);

				// capabilities already in use in the current workflow
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW1 - Capability used in the same workflow 1 (Workflow)", testConfig.CapabilityUsedInSameWorkflow1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2 (Workflow)", testConfig.CapabilityUsedInSameWorkflow2);

				// secondary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR1 - Secondary capability with GRP scope 1 (Team Capability)", testConfig.SecondaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SL1 - Secondary capability with GLB scope 1 (Team Capability)", testConfig.SecondaryGlbCapability1);

				// tertiary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T1 - Tertiary capability 1 (Other)", testConfig.TertiaryCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T2 - Tertiary capability 2 (Other)", testConfig.TertiaryCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T3 - Tertiary capability 3 (Other)", testConfig.TertiaryCapability3);

				// current user capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU1 - Current user capability 1 (Current User)", testConfig.CurrentUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);

				// capabilities with capability channels on another section of the same board
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities with capability channels on another section of the same board", capabilitySubMenuItem, "AL1 - Primary GLB capability for another section 1", testConfig.AnotherSectionGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should still contain menu items for capabilities with capability channels on another section of the same board when the are also secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "AR1 - Primary GRP capability for another section 1 (Team Capability)", testConfig.AnotherSectionGrpCapability1);

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS1 - Secondary capability for another section 1", testConfig.AnotherSectionSecondaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS2 - Secondary capability for another section 2", testConfig.AnotherSectionSecondaryCapability2);

				// other capabilities used in another workflow
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW1 - Capability used in another workflow 1", testConfig.CapabilityUsedInAnotherWorkflow1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW2 - Capability used in another workflow 2", testConfig.CapabilityUsedInAnotherWorkflow2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",
					"TC - Task capability (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T1 - Tertiary capability 1 (Other)",
					"T2 - Tertiary capability 2 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndAssignedToTaskGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			var groupAssignedToTaskToClick = BMSTestHelper.CreateGroup(Factory, "ASG", "Group assigned to the task to click");
			groupAssignedToTaskToClick.Staff.AddRange(new[] {
				testConfig.SecondaryUser1,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser2));
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.TaskToClick.P9_GG_AssignedGroup = groupAssignedToTaskToClick.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				// primary users
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU1 - Primary user 1", testConfig.PrimaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU3 - Primary user 3", testConfig.PrimaryUser3);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU4 - Primary user 4", testConfig.PrimaryUser4);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU5 - Primary user 5", testConfig.PrimaryUser5);
				AssertNoMenuItemForResourceToAssist("Should not contain a menu item for the user currently assigned to the task even if it is a primary user", staffSubMenuItem, "ASG - User assigned to the task to click", testConfig.UserAssignedToTaskToClick);

				// secondary users
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, staffSubMenuItem, "SU3 - Secondary user 3", testConfig.SecondaryUser3);

				// current user
				AssertMenuItemForResourceToAssist("Should contain menu items for the current", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				// users with resource channels on another section of the same board
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU1 - Primary user for another section 1", testConfig.AnotherSectionUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU2 - Primary user for another section 2", testConfig.AnotherSectionUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU3 - Primary user for another section 3", testConfig.AnotherSectionUser3);

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS1 - Secondary user for another section 1", testConfig.AnotherSectionSecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS2 - Secondary user for another section 2", testConfig.AnotherSectionSecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndAssignedToTaskGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			var groupAssignedToTaskToClick = BMSTestHelper.CreateGroup(Factory, "ASG", "Group assigned to the task to click");
			groupAssignedToTaskToClick.Staff.AddRange(new[] {
				testConfig.SecondaryUser1,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser2));
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.TaskToClick.P9_GG_AssignedGroup = groupAssignedToTaskToClick.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				// primary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR1 - Primary capability with GRP scope 1 (Capability Channel)", testConfig.PrimaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR2 - Primary capability with GRP scope 2 (Capability Channel)", testConfig.PrimaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1 (Capability Channel)", testConfig.PrimaryGlbCapability1);

				// capabilities already in use in the current workflow
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW1 - Capability used in the same workflow 1 (Workflow)", testConfig.CapabilityUsedInSameWorkflow1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2 (Workflow)", testConfig.CapabilityUsedInSameWorkflow2);

				// secondary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR1 - Secondary capability with GRP scope 1 (Team Capability)", testConfig.SecondaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SL1 - Secondary capability with GLB scope 1 (Team Capability)", testConfig.SecondaryGlbCapability1);

				// tertiary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T1 - Tertiary capability 1 (Other)", testConfig.TertiaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for tertiary capabilities that come from secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", capabilitySubMenuItem, "T2 - Tertiary capability 2", testConfig.TertiaryCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities that come from secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, capabilitySubMenuItem, "T3 - Tertiary capability 3 (Other)", testConfig.TertiaryCapability3);

				// current user capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU1 - Current user capability 1 (Current User)", testConfig.CurrentUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);

				// capabilities with capability channels on another section of the same board
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities with capability channels on another section of the same board", capabilitySubMenuItem, "AL1 - Primary GLB capability for another section 1", testConfig.AnotherSectionGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should still contain menu items for capabilities with capability channels on another section of the same board when the are also secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "AR1 - Primary GRP capability for another section 1 (Team Capability)", testConfig.AnotherSectionGrpCapability1);

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS1 - Secondary capability for another section 1", testConfig.AnotherSectionSecondaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS2 - Secondary capability for another section 2", testConfig.AnotherSectionSecondaryCapability2);

				// other capabilities used in another workflow
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW1 - Capability used in another workflow 1", testConfig.CapabilityUsedInAnotherWorkflow1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW2 - Capability used in another workflow 2", testConfig.CapabilityUsedInAnotherWorkflow2);

				// other capablities that the currently assigned user possess
				AssertMenuItemForCapabilityToAssist("Should contain menu items for other capabilities that the assigned user possesses when the assigned user is a primary user", testConfig.Workflow, capabilitySubMenuItem, "AU1 - Assigned user capability 1 (Team Capability)", testConfig.AssignedUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for other capabilities that the assigned user possesses when the assigned user is a primary user", testConfig.Workflow, capabilitySubMenuItem, "AU2 - Assigned user capability 2 (Team Capability)", testConfig.AssignedUserCapability2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"AU1 - Assigned user capability 1 (Team Capability)",
					"AU2 - Assigned user capability 2 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T1 - Tertiary capability 1 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsNotAssignedToResource_ButAssignedToTaskGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(Factory, config); // so that the task appears on the board as a capability task

			var groupAssignedToTaskToClick = BMSTestHelper.CreateGroup(Factory, "ASG", "Group assigned to the task to click");
			groupAssignedToTaskToClick.Staff.AddRange(new[] {
				testConfig.SecondaryUser1,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser2));
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.TaskToClick.P9_GG_AssignedGroup = groupAssignedToTaskToClick.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				// primary users
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU1 - Primary user 1", testConfig.PrimaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU3 - Primary user 3", testConfig.PrimaryUser3);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU4 - Primary user 4", testConfig.PrimaryUser4);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU5 - Primary user 5", testConfig.PrimaryUser5);

				// secondary users
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, staffSubMenuItem, "SU3 - Secondary user 3", testConfig.SecondaryUser3);

				// current user
				AssertMenuItemForResourceToAssist("Should contain menu items for the current", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				// users with resource channels on another section of the same board
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU1 - Primary user for another section 1", testConfig.AnotherSectionUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU2 - Primary user for another section 2", testConfig.AnotherSectionUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU3 - Primary user for another section 3", testConfig.AnotherSectionUser3);

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS1 - Secondary user for another section 1", testConfig.AnotherSectionSecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS2 - Secondary user for another section 2", testConfig.AnotherSectionSecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsNotAssignedToResource_ButAssignedToTaskGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(Factory, config); // so that the task appears on the board as a capability task

			var groupAssignedToTaskToClick = BMSTestHelper.CreateGroup(Factory, "ASG", "Group assigned to the task to click");
			groupAssignedToTaskToClick.Staff.AddRange(new[] {
				testConfig.SecondaryUser1,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser2));
			Assert("Precondition: the user is outside the task group assigned to the task to click - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !groupAssignedToTaskToClick.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.TaskToClick.P9_GG_AssignedGroup = groupAssignedToTaskToClick.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				// primary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR1 - Primary capability with GRP scope 1 (Capability Channel)", testConfig.PrimaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR2 - Primary capability with GRP scope 2 (Capability Channel)", testConfig.PrimaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1 (Capability Channel)", testConfig.PrimaryGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for the task capability as it is a primary capability", testConfig.Workflow, capabilitySubMenuItem, "TC - Task capability (Capability Channel)", testConfig.TaskCapability);

				// capabilities already in use in the current workflow
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW1 - Capability used in the same workflow 1 (Workflow)", testConfig.CapabilityUsedInSameWorkflow1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2 (Workflow)", testConfig.CapabilityUsedInSameWorkflow2);

				// secondary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR1 - Secondary capability with GRP scope 1 (Team Capability)", testConfig.SecondaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SL1 - Secondary capability with GLB scope 1 (Team Capability)", testConfig.SecondaryGlbCapability1);

				// tertiary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T1 - Tertiary capability 1 (Other)", testConfig.TertiaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for tertiary capabilities that come from secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", capabilitySubMenuItem, "T2 - Tertiary capability 2", testConfig.TertiaryCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities that come from secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, capabilitySubMenuItem, "T3 - Tertiary capability 3 (Other)", testConfig.TertiaryCapability3);

				// current user capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU1 - Current user capability 1 (Current User)", testConfig.CurrentUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);

				// capabilities with capability channels on another section of the same board
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities with capability channels on another section of the same board", capabilitySubMenuItem, "AL1 - Primary GLB capability for another section 1", testConfig.AnotherSectionGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should still contain menu items for capabilities with capability channels on another section of the same board when the are also secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "AR1 - Primary GRP capability for another section 1 (Team Capability)", testConfig.AnotherSectionGrpCapability1);

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS1 - Secondary capability for another section 1", testConfig.AnotherSectionSecondaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS2 - Secondary capability for another section 2", testConfig.AnotherSectionSecondaryCapability2);

				// other capabilities used in another workflow
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW1 - Capability used in another workflow 1", testConfig.CapabilityUsedInAnotherWorkflow1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW2 - Capability used in another workflow 2", testConfig.CapabilityUsedInAnotherWorkflow2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",
					"TC - Task capability (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T1 - Tertiary capability 1 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndNotAssignedToTaskGroup_ButWorkflowHasReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "ASG", "Workflow release group");
			releaseGroup.Staff.AddRange(new[] {
				testConfig.SecondaryUser2,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the release group - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !releaseGroup.Staff.Contains(testConfig.SecondaryUser1));
			Assert("Precondition: the user is outside the release group - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !releaseGroup.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.Workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				// primary users
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU1 - Primary user 1", testConfig.PrimaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU3 - Primary user 3", testConfig.PrimaryUser3);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU4 - Primary user 4", testConfig.PrimaryUser4);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU5 - Primary user 5", testConfig.PrimaryUser5);

				// secondary users
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, staffSubMenuItem, "SU3 - Secondary user 3", testConfig.SecondaryUser3);

				// current user
				AssertMenuItemForResourceToAssist("Should contain menu items for the current", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				// users with resource channels on another section of the same board
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU1 - Primary user for another section 1", testConfig.AnotherSectionUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU2 - Primary user for another section 2", testConfig.AnotherSectionUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU3 - Primary user for another section 3", testConfig.AnotherSectionUser3);

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS1 - Secondary user for another section 1", testConfig.AnotherSectionSecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS2 - Secondary user for another section 2", testConfig.AnotherSectionSecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndResourcePossessesPrimaryCapability()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			testConfig.UserAssignedToTaskToClick.Capabilities.AddRange(testConfig.PrimaryGrpCapability2);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				AssertNoMenuItemForResourceToAssist("Should not contain a menu item for the user currently assigned to the task", staffSubMenuItem, "ASG - User assigned to the task to click", testConfig.UserAssignedToTaskToClick);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support",
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsAssignedToResource_AndNotAssignedToTaskGroup_ButWorkflowHasReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "ASG", "Workflow release group");
			releaseGroup.Staff.AddRange(new[] {
				testConfig.SecondaryUser2,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the release group - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !releaseGroup.Staff.Contains(testConfig.SecondaryUser1));
			Assert("Precondition: the user is outside the release group - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !releaseGroup.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.Workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				// primary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR1 - Primary capability with GRP scope 1 (Capability Channel)", testConfig.PrimaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR2 - Primary capability with GRP scope 2 (Capability Channel)", testConfig.PrimaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1 (Capability Channel)", testConfig.PrimaryGlbCapability1);

				// capabilities already in use in the current workflow
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW1 - Capability used in the same workflow 1 (Workflow)", testConfig.CapabilityUsedInSameWorkflow1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2 (Workflow)", testConfig.CapabilityUsedInSameWorkflow2);

				// secondary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR1 - Secondary capability with GRP scope 1 (Team Capability)", testConfig.SecondaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SL1 - Secondary capability with GLB scope 1 (Team Capability)", testConfig.SecondaryGlbCapability1);

				// tertiary capabilities
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for tertiary capabilities that come from secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", capabilitySubMenuItem, "T1 - Tertiary capability 1", testConfig.TertiaryCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T2 - Tertiary capability 2 (Other)", testConfig.TertiaryCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities that come from secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, capabilitySubMenuItem, "T3 - Tertiary capability 3 (Other)", testConfig.TertiaryCapability3);

				// current user capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU1 - Current user capability 1 (Current User)", testConfig.CurrentUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);

				// capabilities with capability channels on another section of the same board
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities with capability channels on another section of the same board", capabilitySubMenuItem, "AL1 - Primary GLB capability for another section 1", testConfig.AnotherSectionGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should still contain menu items for capabilities with capability channels on another section of the same board when the are also secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "AR1 - Primary GRP capability for another section 1 (Team Capability)", testConfig.AnotherSectionGrpCapability1);

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS1 - Secondary capability for another section 1", testConfig.AnotherSectionSecondaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS2 - Secondary capability for another section 2", testConfig.AnotherSectionSecondaryCapability2);

				// other capabilities used in another workflow
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW1 - Capability used in another workflow 1", testConfig.CapabilityUsedInAnotherWorkflow1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW2 - Capability used in another workflow 2", testConfig.CapabilityUsedInAnotherWorkflow2);

				// other capablities that the currently assigned user possess
				AssertMenuItemForCapabilityToAssist("Should contain menu items for other capabilities that the assigned user possesses when the assigned user is a primary user", testConfig.Workflow, capabilitySubMenuItem, "AU1 - Assigned user capability 1 (Team Capability)", testConfig.AssignedUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for other capabilities that the assigned user possesses when the assigned user is a primary user", testConfig.Workflow, capabilitySubMenuItem, "AU2 - Assigned user capability 2 (Team Capability)", testConfig.AssignedUserCapability2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"AU1 - Assigned user capability 1 (Team Capability)",
					"AU2 - Assigned user capability 2 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T2 - Tertiary capability 2 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsNotAssignedToResource_AndNotAssignedToTaskGroup_ButWorkflowHasReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(Factory, config); // so that the task appears on the board as a capability task

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "ASG", "Workflow release group");
			releaseGroup.Staff.AddRange(new[] {
				testConfig.SecondaryUser2,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the release group - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !releaseGroup.Staff.Contains(testConfig.SecondaryUser1));
			Assert("Precondition: the user is outside the release group - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !releaseGroup.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.Workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				// primary users
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU1 - Primary user 1", testConfig.PrimaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU3 - Primary user 3", testConfig.PrimaryUser3);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU4 - Primary user 4", testConfig.PrimaryUser4);
				AssertMenuItemForResourceToAssist("Should contain menu items for primary users", testConfig.Workflow, staffSubMenuItem, "PU5 - Primary user 5", testConfig.PrimaryUser5);

				// secondary users
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users", testConfig.Workflow, staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);
				AssertMenuItemForResourceToAssist("Should contain menu items for secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, staffSubMenuItem, "SU3 - Secondary user 3", testConfig.SecondaryUser3);

				// current user
				AssertMenuItemForResourceToAssist("Should contain menu items for the current", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				// users with resource channels on another section of the same board
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU1 - Primary user for another section 1", testConfig.AnotherSectionUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU2 - Primary user for another section 2", testConfig.AnotherSectionUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for users with resource channels on another section of the same board", staffSubMenuItem, "AU3 - Primary user for another section 3", testConfig.AnotherSectionUser3);

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS1 - Secondary user for another section 1", testConfig.AnotherSectionSecondaryUser1);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users for another section", staffSubMenuItem, "AS2 - Secondary user for another section 2", testConfig.AnotherSectionSecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_WhenClickedTaskIsNotAssignedToResource_AndNotAssignedToTaskGroup_ButWorkflowHasReleaseGroup()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(Factory, config); // so that the task appears on the board as a capability task

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "ASG", "Workflow release group");
			releaseGroup.Staff.AddRange(new[] {
				testConfig.SecondaryUser2,
				testConfig.AnotherSectionSecondaryUser1,
				testConfig.AnotherSectionSecondaryUser2
			});
			Assert("Precondition: the user is outside the release group - we want to ensure we don't have staff menu item for this user and capability menus for associated with them tertiary capabilities", !releaseGroup.Staff.Contains(testConfig.SecondaryUser1));
			Assert("Precondition: the user is outside the release group - we want to ensure we do have staff menu item for this user and capability menus for associated with them tertiary capabilities as it is a member of a primary GLB capability", !releaseGroup.Staff.Contains(testConfig.SecondaryUser3));

			testConfig.Workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				// primary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR1 - Primary capability with GRP scope 1 (Capability Channel)", testConfig.PrimaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PR2 - Primary capability with GRP scope 2 (Capability Channel)", testConfig.PrimaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for primary capabilities", testConfig.Workflow, capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1 (Capability Channel)", testConfig.PrimaryGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for the task capability as it is a primary capability", testConfig.Workflow, capabilitySubMenuItem, "TC - Task capability (Capability Channel)", testConfig.TaskCapability);

				// capabilities already in use in the current workflow
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW1 - Capability used in the same workflow 1 (Workflow)", testConfig.CapabilityUsedInSameWorkflow1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for capabilities already in use in the current workflow", testConfig.Workflow, capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2 (Workflow)", testConfig.CapabilityUsedInSameWorkflow2);

				// secondary capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR1 - Secondary capability with GRP scope 1 (Team Capability)", testConfig.SecondaryGrpCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "SL1 - Secondary capability with GLB scope 1 (Team Capability)", testConfig.SecondaryGlbCapability1);

				// tertiary capabilities
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for tertiary capabilities that come from secondary users that possess primary GRP capabilities, but are outside the task group assigned to the task", capabilitySubMenuItem, "T1 - Tertiary capability 1", testConfig.TertiaryCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities", testConfig.Workflow, capabilitySubMenuItem, "T2 - Tertiary capability 2 (Other)", testConfig.TertiaryCapability2);
				AssertMenuItemForCapabilityToAssist("Should contain menu items for tertiary capabilities that come from secondary users that are outside the task group assigned to the task, but possess primary GLB capabilities", testConfig.Workflow, capabilitySubMenuItem, "T3 - Tertiary capability 3 (Other)", testConfig.TertiaryCapability3);

				// current user capabilities
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU1 - Current user capability 1 (Current User)", testConfig.CurrentUserCapability1);
				AssertMenuItemForCapabilityToAssist("Should contain menu items the capabilities of the current user", testConfig.Workflow, capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);

				// capabilities with capability channels on another section of the same board
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities with capability channels on another section of the same board", capabilitySubMenuItem, "AL1 - Primary GLB capability for another section 1", testConfig.AnotherSectionGlbCapability1);
				AssertMenuItemForCapabilityToAssist("Should still contain menu items for capabilities with capability channels on another section of the same board when the are also secondary capabilities", testConfig.Workflow, capabilitySubMenuItem, "AR1 - Primary GRP capability for another section 1 (Team Capability)", testConfig.AnotherSectionGrpCapability1);

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS1 - Secondary capability for another section 1", testConfig.AnotherSectionSecondaryCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that the users associated with the resource channels on another section possess", capabilitySubMenuItem, "AS2 - Secondary capability for another section 2", testConfig.AnotherSectionSecondaryCapability2);

				// other capabilities used in another workflow
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW1 - Capability used in another workflow 1", testConfig.CapabilityUsedInAnotherWorkflow1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities used in another workflow", capabilitySubMenuItem, "AW2 - Capability used in another workflow 2", testConfig.CapabilityUsedInAnotherWorkflow2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",
					"TC - Task capability (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T2 - Tertiary capability 2 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ShouldNotIncludeCurrentUser_WhenCurrentUserIsAssignedToTask()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCurrentPrimaryUser(Factory, config);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				AssertNoMenuItemForResourceToAssist("Should not contain a menu item for the current user as it is assigned to the task", staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ShouldIncludeCurrentUserJustOnce_WhenTheyAreSecondaryUser()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_CurrentUserIsSecondaryUser(Factory, config);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				AssertMenuItemForResourceToAssist("Should contain a menu item for the current user who is also a secondary user", testConfig.Workflow, staffSubMenuItem, "E - CargoWise Support", testConfig.CurrentUser);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"E - CargoWise Support",
					"SU1 - Secondary user 1",
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_ShouldNotIncludeInactiveUsers()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			testConfig.PrimaryUser2.GS_IsActive = false;
			testConfig.SecondaryUser2.GS_IsActive = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				AssertNoMenuItemForResourceToAssist("Should not contain menu items for inactive users", staffSubMenuItem, "PU2 - Primary user 2", testConfig.PrimaryUser2);
				AssertNoMenuItemForResourceToAssist("Should not contain menu items for inactive users", staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU1 - Secondary user 1",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForCapabilityMenuItem_ForTaskCards_OnClick_ShouldNotIncludeInactiveCapabilities()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			testConfig.PrimaryGlbCapability1.G4_IsActive = false;
			testConfig.CapabilityUsedInSameWorkflow2.G4_IsActive = false;
			testConfig.SecondaryGrpCapability2.G4_IsActive = false;
			testConfig.TertiaryCapability3.G4_IsActive = false;
			testConfig.CurrentUserCapability2.G4_IsActive = false;
			testConfig.AssignedUserCapability1.G4_IsActive = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for inactive capabilities", capabilitySubMenuItem, "PL1 - Primary capability with GLB scope 1", testConfig.PrimaryGlbCapability1);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for inactive capabilities", capabilitySubMenuItem, "SW2 - Capability used in the same workflow 2", testConfig.CapabilityUsedInSameWorkflow2);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for inactive capabilities", capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2 (Team Capability)", testConfig.SecondaryGrpCapability2);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for inactive capabilities", capabilitySubMenuItem, "T3 - Tertiary capability 3", testConfig.TertiaryCapability3);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items the inactive capabilities", capabilitySubMenuItem, "CU2 - Current user capability 2 (Current User)", testConfig.CurrentUserCapability2);
				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for inactive capabilities", capabilitySubMenuItem, "AU1 - Assigned user capability 1 (Team Capability)", testConfig.AssignedUserCapability1);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"AU2 - Assigned user capability 2 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T1 - Tertiary capability 1 (Other)",
					"T2 - Tertiary capability 2 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_ShouldNotIncludeSecondaryUsersForInactivePrimaryCapabilities()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			testConfig.PrimaryGrpCapability1.G4_IsActive = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
				AssertNotNull(staffSubMenuItem);

				AssertNoMenuItemForResourceToAssist("Should not contain menu items for secondary users that come from inactive primary capabilities", staffSubMenuItem, "SU1 - Secondary user 1", testConfig.SecondaryUser1);
				AssertMenuItemForResourceToAssist("Should still contain menu items for secondary users that come from inactive primary capabilities if the users also possess primary capabilities", testConfig.Workflow, staffSubMenuItem, "SU2 - Secondary user 2", testConfig.SecondaryUser2);

				AssertContainsMenuItemsInExactOrder(staffSubMenuItem, new string[] {
					// primary users are sorted in accordance to the order of resource channels
					"PU5 - Primary user 5",
					"PU1 - Primary user 1",
					"PU2 - Primary user 2",
					"PU3 - Primary user 3",
					"PU4 - Primary user 4",

					// secondary users are sorted by code alphabetically
					"SU2 - Secondary user 2",
					"SU3 - Secondary user 3",

					// current user is added to the end
					"E - CargoWise Support"
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_ShouldNotIncludeSecondaryCapabilitiesForInactivePrimaryUsers()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			testConfig.PrimaryUser2.GS_IsActive = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for capabilities that come from inactive primary users", capabilitySubMenuItem, "SR2 - Secondary capability with GRP scope 2", testConfig.SecondaryGrpCapability2);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AU1 - Assigned user capability 1 (Team Capability)",
					"AU2 - Assigned user capability 2 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T1 - Tertiary capability 1 (Other)",
					"T2 - Tertiary capability 2 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)",
				});
			}
		}

		public void TestAddAssistanceTaskForStaffMenuItem_ForTaskCards_OnClick_ShouldNotIncludeTertiaryCapabilitiesForInactiveSecondaryUsers()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			testConfig.SecondaryUser1.GS_IsActive = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);

				AssertNoMenuItemForCapabilityToAssist("Should not contain menu items for tertiary capabilities that come from inactive secondary users", capabilitySubMenuItem, "T1 - Tertiary capability 1", testConfig.TertiaryCapability1);

				AssertContainsMenuItemsInExactOrder(capabilitySubMenuItem, new string[] {
					// primary capabilities are sorted in accordance to the order of capability channels
					"PR1 - Primary capability with GRP scope 1 (Capability Channel)",
					"PL1 - Primary capability with GLB scope 1 (Capability Channel)",
					"PR2 - Primary capability with GRP scope 2 (Capability Channel)",

					// workflow capabilities are sorted by code alphabetically
					"SW1 - Capability used in the same workflow 1 (Workflow)",
					"SW2 - Capability used in the same workflow 2 (Workflow)",

					// secondary capabilities are sorted by code alphabetically
					"AR1 - Primary GRP capability for another section 1 (Team Capability)",
					"AU1 - Assigned user capability 1 (Team Capability)",
					"AU2 - Assigned user capability 2 (Team Capability)",
					"SL1 - Secondary capability with GLB scope 1 (Team Capability)",
					"SR1 - Secondary capability with GRP scope 1 (Team Capability)",
					"SR2 - Secondary capability with GRP scope 2 (Team Capability)",

					// tertiary capabilities are sorted by code alphabetically
					"T2 - Tertiary capability 2 (Other)",
					"T3 - Tertiary capability 3 (Other)",

					// current user capabilities are sorted by code alphabetically
					"CU1 - Current user capability 1 (Current User)",
					"CU2 - Current user capability 2 (Current User)",
				});
			}
		}

		[TestDate(2021, 9, 8)]
		public void TestAddAssistanceTaskForStaffMenuItem_Dbhits()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull(menuItem);

				var expectedHits = new Dictionary<string, int>
				{
					{ GlbResourceCapabilityPivotSchema.Constants.TableName, 2 },
					{ GlbStaffSchema.Constants.TableName, 2 },
				};

				using (AssertDbHitsForAllFactories(expectedHits, ignoreHitsFromTablesCachedInUberFactory: true, thresholdForUnspecified: 0))
				{
					var staffSubMenuItem = GetSubMenuItem(menuItem, "Staff");
					AssertNotNull(staffSubMenuItem);
					GetSubMenuItems(staffSubMenuItem);
				}
			}
		}

		[TestDate(2021, 9, 8)]
		public void TestAddAssistanceTaskForCapabilityMenuItem_Dbhits()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var testConfig = new AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(Factory, config);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var menuItem = GetMenu(testConfig.TaskToClick, form, "Add Assistance Task For");
				AssertNotNull(menuItem);

				var expectedHits = new Dictionary<string, int>
				{
					{ GlbResourceCapabilityPivotSchema.Constants.TableName, 2 },
					{ GlbStaffSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
				};

				using (AssertDbHitsForAllFactories(expectedHits, ignoreHitsFromTablesCachedInUberFactory: true, thresholdForUnspecified: 0))
				{
					var capabilitySubMenuItem = GetSubMenuItem(menuItem, "Capability");
					AssertNotNull(capabilitySubMenuItem);
					GetSubMenuItems(capabilitySubMenuItem);
				}
			}
		}

		class AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser : AddAssistanceTaskForMenuItemTestConfig
		{
			readonly public GlbCapability AssignedUserCapability1;
			readonly public GlbCapability AssignedUserCapability2;

			readonly public GlbStaff UserAssignedToTaskToClick;

			readonly public ProcessTask TaskToClick;

			public AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser(BusinessObjectFactory factory, VisualBoardTestConfig config)
				: base(factory, config)
			{
				AssignedUserCapability1 = BMSTestHelper.CreateCapability(factory, "AU1", "Assigned user capability 1", isGroupScope: true);
				AssignedUserCapability2 = BMSTestHelper.CreateCapability(factory, "AU2", "Assigned user capability 2");

				UserAssignedToTaskToClick = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "ASG", "User assigned to the task to click", AssignedUserCapability1, AssignedUserCapability2);

				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, UserAssignedToTaskToClick.PK, overrideChannels: true);

				TaskToClick = BMSTestHelper.CreateTask(Workflow, UserAssignedToTaskToClick.GS_Code, 60, "INV", sequence: 10, description: "Task to click", estVariationFactor: 5);
			}
		}

		class AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty : AddAssistanceTaskForMenuItemTestConfig
		{
			readonly public GlbCapability TaskCapability;

			readonly public ProcessTask TaskToClick;

			public AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCapabilty(BusinessObjectFactory factory, VisualBoardTestConfig config)
				: base(factory, config)
			{
				TaskCapability = BMSTestHelper.CreateCapability(factory, "TC", "Task capability", isGroupScope: true);

				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, TaskCapability.PK, overrideChannels: true);

				TaskToClick = BMSTestHelper.CreateTask(Workflow, capability: TaskCapability, lowEstMinutes: 60, taskType: "INV", sequence: 10, description: "Task to click", estVariationFactor: 5); // so that the task appears on the board as a capability task
			}
		}

		class AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCurrentPrimaryUser : AddAssistanceTaskForMenuItemTestConfig
		{
			readonly public ProcessTask TaskToClick;

			public AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToCurrentPrimaryUser(BusinessObjectFactory factory, VisualBoardTestConfig config)
				: base(factory, config)
			{
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, CurrentUser.PK, overrideChannels: true);

				TaskToClick = BMSTestHelper.CreateTask(Workflow, CurrentUser.GS_Code, 60, "INV", sequence: 10, description: "Task to click", estVariationFactor: 5);
			}
		}

		class AddAssistanceTaskForMenuItemTestConfig_CurrentUserIsSecondaryUser : AddAssistanceTaskForMenuItemTestConfig_TaskToClickAssignedToPrimaryUser
		{
			public AddAssistanceTaskForMenuItemTestConfig_CurrentUserIsSecondaryUser(BusinessObjectFactory factory, VisualBoardTestConfig config)
				: base(factory, config)
			{
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, CurrentUserCapability1.PK, overrideChannels: true);
			}
		}

		abstract class AddAssistanceTaskForMenuItemTestConfig
		{
			readonly public GlbCapability PrimaryGrpCapability1;
			readonly public GlbCapability PrimaryGrpCapability2;
			readonly public GlbCapability PrimaryGlbCapability1;
			readonly public GlbCapability SecondaryGrpCapability1;
			readonly public GlbCapability SecondaryGrpCapability2;
			readonly public GlbCapability SecondaryGlbCapability1;
			readonly public GlbCapability TertiaryCapability1;
			readonly public GlbCapability TertiaryCapability2;
			readonly public GlbCapability TertiaryCapability3;
			readonly public GlbCapability AnotherSectionGrpCapability1;
			readonly public GlbCapability AnotherSectionGlbCapability1;
			readonly public GlbCapability AnotherSectionSecondaryCapability1;
			readonly public GlbCapability AnotherSectionSecondaryCapability2;
			readonly public GlbCapability CapabilityUsedInSameWorkflow1;
			readonly public GlbCapability CapabilityUsedInSameWorkflow2;
			readonly public GlbCapability CapabilityUsedInAnotherWorkflow1;
			readonly public GlbCapability CapabilityUsedInAnotherWorkflow2;
			readonly public GlbCapability CurrentUserCapability1;
			readonly public GlbCapability CurrentUserCapability2;
			readonly public GlbCapability IrrelevantCapability1;
			readonly public GlbCapability IrrelevantCapability2;

			readonly public GlbStaff CurrentUser;
			readonly public GlbStaff PrimaryUser1;
			readonly public GlbStaff PrimaryUser2;
			readonly public GlbStaff PrimaryUser3;
			readonly public GlbStaff PrimaryUser4;
			readonly public GlbStaff PrimaryUser5;
			readonly public GlbStaff SecondaryUser1;
			readonly public GlbStaff SecondaryUser2;
			readonly public GlbStaff SecondaryUser3;
			readonly public GlbStaff AnotherSectionUser1;
			readonly public GlbStaff AnotherSectionUser2;
			readonly public GlbStaff AnotherSectionUser3;
			readonly public GlbStaff AnotherSectionSecondaryUser1;
			readonly public GlbStaff AnotherSectionSecondaryUser2;
			readonly public GlbStaff IrrelevantUser1;
			readonly public GlbStaff IrrelevantUser2;
			readonly public GlbStaff IrrelevantUser3;

			readonly public ProcessHeader Workflow;
			readonly public ProcessTask TaskWithcapabilityInSameWorkflow1;
			readonly public ProcessTask TaskWithcapabilityInSameWorkflow2;
			readonly public ProcessTask TaskWithcapabilityInSameWorkflow3;

			readonly public ProcessHeader AnotherWorkflow;
			readonly public ProcessTask TaskWithcapabilityInAnotherWorkflow1;
			readonly public ProcessTask TaskWithcapabilityInAnotherWorkflow2;
			readonly public ProcessTask TaskWithcapabilityInAnotherWorkflow3;

			protected AddAssistanceTaskForMenuItemTestConfig(BusinessObjectFactory factory, VisualBoardTestConfig config)
			{
				// CAPABILITIES

				// "primary" capabilities - capabilities with capability channels on the buffer (main) section
				PrimaryGrpCapability1 = BMSTestHelper.CreateCapability(factory, "PR1", "Primary capability with GRP scope 1", isGroupScope: true);
				PrimaryGrpCapability2 = BMSTestHelper.CreateCapability(factory, "PR2", "Primary capability with GRP scope 2", isGroupScope: true);

				PrimaryGlbCapability1 = BMSTestHelper.CreateCapability(factory, "PL1", "Primary capability with GLB scope 1");

				// "secondary" capabilities - capabilities that the users associated with the resource channels on the buffer section possess
				SecondaryGrpCapability1 = BMSTestHelper.CreateCapability(factory, "SR1", "Secondary capability with GRP scope 1", isGroupScope: true);
				SecondaryGrpCapability2 = BMSTestHelper.CreateCapability(factory, "SR2", "Secondary capability with GRP scope 2", isGroupScope: true);

				SecondaryGlbCapability1 = BMSTestHelper.CreateCapability(factory, "SL1", "Secondary capability with GLB scope 1");

				// "tertiary" capabilities - capabilities that some of the secondary users (see below) possess
				TertiaryCapability1 = BMSTestHelper.CreateCapability(factory, "T1", "Tertiary capability 1", isGroupScope: true);
				TertiaryCapability2 = BMSTestHelper.CreateCapability(factory, "T2", "Tertiary capability 2", isGroupScope: true);
				TertiaryCapability3 = BMSTestHelper.CreateCapability(factory, "T3", "Tertiary capability 3", isGroupScope: true);

				// capabilities with capability channels on another section of the same board
				AnotherSectionGrpCapability1 = BMSTestHelper.CreateCapability(factory, "AR1", "Primary GRP capability for another section 1", isGroupScope: true);
				AnotherSectionGlbCapability1 = BMSTestHelper.CreateCapability(factory, "AL1", "Primary GLB capability for another section 1");

				// capabilities that the users associated with the resource channels on another section of the same board possess (secondary capabilities for another section)
				AnotherSectionSecondaryCapability1 = BMSTestHelper.CreateCapability(factory, "AS1", "Secondary capability for another section 1", isGroupScope: true);
				AnotherSectionSecondaryCapability2 = BMSTestHelper.CreateCapability(factory, "AS2", "Secondary capability for another section 2");

				// other capabilities used in the workflow
				CapabilityUsedInSameWorkflow1 = BMSTestHelper.CreateCapability(factory, "SW1", "Capability used in the same workflow 1", isGroupScope: true);
				CapabilityUsedInSameWorkflow2 = BMSTestHelper.CreateCapability(factory, "SW2", "Capability used in the same workflow 2", isGroupScope: true);

				// other capabilities used in another workflow
				CapabilityUsedInAnotherWorkflow1 = BMSTestHelper.CreateCapability(factory, "AW1", "Capability used in another workflow 1", isGroupScope: true);
				CapabilityUsedInAnotherWorkflow2 = BMSTestHelper.CreateCapability(factory, "AW2", "Capability used in another workflow 2", isGroupScope: true);

				// current user capabilities
				CurrentUserCapability1 = BMSTestHelper.CreateCapability(factory, "CU1", "Current user capability 1", isGroupScope: true);
				CurrentUserCapability2 = BMSTestHelper.CreateCapability(factory, "CU2", "Current user capability 2");

				// irrelevnt capabilities - not used anywhere
				IrrelevantCapability1 = BMSTestHelper.CreateCapability(factory, "I1", "Irrelevant capability 1", isGroupScope: true);
				IrrelevantCapability2 = BMSTestHelper.CreateCapability(factory, "I2", "Irrelevant capability 2", isGroupScope: true);

				// USERS

				// current user
				CurrentUser = (GlbStaff)Env.CurrentUser;
				CurrentUser.Capabilities.AddRange(CurrentUserCapability1, CurrentUserCapability2);
				CurrentUserCapability1.ResourcesWithCapability.Add(CurrentUser);
				CurrentUserCapability2.ResourcesWithCapability.Add(CurrentUser);

				// "primary" users - users with resource channels on the buffer section
				PrimaryUser1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "PU1", "Primary user 1", SecondaryGrpCapability1, SecondaryGlbCapability1);
				PrimaryUser2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "PU2", "Primary user 2", SecondaryGrpCapability2, AnotherSectionGrpCapability1); // the user possesses a capability with capability channels on another section of the same board - we want to ensure we still have a menu item for the user
				PrimaryUser3 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "PU3", "Primary user 3");
				PrimaryUser4 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "PU4", "Primary user 4", PrimaryGrpCapability1); // the user possesses a primary capability - we want to ensure we don't have duplicated menu items for this case
				PrimaryUser5 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "PU5", "Primary user 5", PrimaryGlbCapability1); // the user possesses a primary capability - we want to ensure we don't have duplicated menu items for this case

				// "secondary" users - users that possess some of the primary capabilities but don't have resource channels on the buffer section
				SecondaryUser1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "SU1", "Secondary user 1", PrimaryGrpCapability1, TertiaryCapability1);
				SecondaryUser2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "SU2", "Secondary user 2", PrimaryGrpCapability1, PrimaryGrpCapability2, TertiaryCapability2); // the same user possesses two different primary capabilities - we want to ensure we don't have duplicated menu items for this case
				SecondaryUser3 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "SU3", "Secondary user 3", PrimaryGlbCapability1, TertiaryCapability3);

				// users with resource channels on another section of the same board
				AnotherSectionUser1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "AU1", "Primary user for another section 1", AnotherSectionSecondaryCapability1);
				AnotherSectionUser2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "AU2", "Primary user for another section 2", AnotherSectionSecondaryCapability2);
				AnotherSectionUser3 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "AU3", "Primary user for another section 3", SecondaryGlbCapability1); // we want to ensure we still have a menu item for the user because the user possesses a secondary capability

				// users that possess some of the capabilities with capability channels on another section of the same board (secondary users for another section)
				AnotherSectionSecondaryUser1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "AS1", "Secondary user for another section 1", AnotherSectionGrpCapability1);
				AnotherSectionSecondaryUser2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "AS2", "Secondary user for another section 2", AnotherSectionGlbCapability1);

				// irrelevant users - not used anywhere (maybe accidently assigned to some capability tasks)
				IrrelevantUser1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "IU1", "Irrelevant user 1");
				IrrelevantUser2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "IU2", "Irrelevant user 2");
				IrrelevantUser3 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "IU3", "Irrelevant user 3");

				// MAIN SECTION

				// Buffer section resource channels
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, PrimaryUser5.PK, overrideChannels: true); // let's break the order to ensure the resulting menu items are sorted in the order of channels, not the creation order of resources
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, PrimaryUser1.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, PrimaryUser2.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, PrimaryUser3.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, PrimaryUser4.PK, overrideChannels: true);

				// Buffer section capability channels
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, PrimaryGrpCapability1.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, PrimaryGlbCapability1.PK, overrideChannels: true); // let's break the order to ensure the resulting menu items are sorted in the order of channels, not the creation order of capabilities
				BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, PrimaryGrpCapability2.PK, overrideChannels: true);

				// ANOTHER SECTION

				var anotherSection = BMSTestHelper.CreateBoardSection(config.Bucket, config.BufferBoard, row: 1);

				// Another section resource channels
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Resource, AnotherSectionUser1.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Resource, AnotherSectionUser2.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Resource, AnotherSectionUser3.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Resource, PrimaryUser3.PK, overrideChannels: true); // we want to ensure we still have menu item for this resource even if they also have a channel on this other section

				// Another section capability channels
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Capability, AnotherSectionGrpCapability1.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Capability, AnotherSectionGlbCapability1.PK, overrideChannels: true);
				BMSTestHelper.CreatePrimaryChannelForSection(anotherSection, ChannelTypeList.Codes.Capability, PrimaryGrpCapability2.PK, overrideChannels: true); // we want to ensure we still have menu item for this capability even if it also have a channel on this other section

				// MAIN WORKFLOW

				Workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Workflow", config.Buffer);

				// other tasks with capabilities in the same worflow
				TaskWithcapabilityInSameWorkflow1 = BMSTestHelper.CreateTask(Workflow, lowEstMinutes: 60, taskType: "UDF", capability: CapabilityUsedInSameWorkflow1, sequence: 10, description: "Capability task", estVariationFactor: 5);
				TaskWithcapabilityInSameWorkflow2 = BMSTestHelper.CreateTask(Workflow, IrrelevantUser1.GS_Code, lowEstMinutes: 60, taskType: "UDF", capability: CapabilityUsedInSameWorkflow2, sequence: 10, description: "Capability task with user", estVariationFactor: 5); // assigned to an irrelevant user - we want to ensure we don't have a menu item for this user 
				TaskWithcapabilityInSameWorkflow3 = BMSTestHelper.CreateTask(Workflow, lowEstMinutes: 60, taskType: "UDF", capability: PrimaryGlbCapability1, sequence: 10, description: "Capability task assigned to a primary capability", estVariationFactor: 5); // we want to ensure we don't have duplicated menu items for this primary capability

				// ANOTHER WORKFLOW WITHIN THE SAME JOB

				AnotherWorkflow = BMSTestHelper.CreateWorkflow(Workflow.JobHeader, "Another workflow within the same job", config.Buffer);

				// tasks with capabilities in this another workflow
				TaskWithcapabilityInAnotherWorkflow1 = BMSTestHelper.CreateTask(AnotherWorkflow, lowEstMinutes: 60, taskType: "UDF", capability: CapabilityUsedInAnotherWorkflow1, sequence: 10, description: "Some capability task", estVariationFactor: 5);
				TaskWithcapabilityInAnotherWorkflow2 = BMSTestHelper.CreateTask(AnotherWorkflow, lowEstMinutes: 60, taskType: "UDF", capability: CapabilityUsedInAnotherWorkflow2, sequence: 10, description: "Some capability task", estVariationFactor: 5);
				TaskWithcapabilityInAnotherWorkflow3 = BMSTestHelper.CreateTask(AnotherWorkflow, lowEstMinutes: 60, taskType: "UDF", capability: PrimaryGrpCapability2, sequence: 10, description: "Capability task assigned to a primary capability", estVariationFactor: 5); // we want to ensure we still have a menu item for this primary capability
			}
		}

		void AssertNoMenuItemForResourceToAssist(string message, ZToolStripMenuItem staffSubMenuItem, string subMenuItemDescription, GlbStaff resource)
		{
			var menuItem = GetSubMenuItem(staffSubMenuItem, subMenuItemDescription);
			var actuals = GetActualSubMenuItemsMessage(staffSubMenuItem);
			AssertNull($"{message} - {subMenuItemDescription}{actuals}", menuItem);
			Assert($"{message} - {resource.GS_Code}{actuals}", !GetSubMenuItemsContaining(staffSubMenuItem, resource.GS_Code).Any());
		}

		void AssertMenuItemForResourceToAssist(string message, ProcessHeader workflow, ZToolStripMenuItem staffSubMenuItem, string expectedSubMenuItemDescription, GlbStaff expectedAssignedResource)
		{
			var menuItem = GetSubMenuItem(staffSubMenuItem, expectedSubMenuItemDescription);
			var actuals = GetActualSubMenuItemsMessage(staffSubMenuItem);
			AssertNotNull($"{message} - {expectedSubMenuItemDescription}{actuals}", menuItem);

			AssertEquals("Precondition: the assist task should not exist yet", 0, GetAssistTasks(workflow.PK).Count(x => x.P9_GS_NKAssignedStaffMember == expectedAssignedResource.GS_Code));

			menuItem.PerformClick();
			Application.DoEvents();

			var assistTask = GetAssistTasks(workflow.PK).SingleOrDefault(x => x.P9_GS_NKAssignedStaffMember == expectedAssignedResource.GS_Code);
			AssertNotNull("An assist task should have been created and saved", assistTask);

			AssertEquals("AST", assistTask.P9_Type);
			AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
			AssertEquals(10, assistTask.P9_Sequence);
			AssertEquals(expectedAssignedResource.GS_Code, assistTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
			AssertEquals("Assist", assistTask.P9_Description);

			AssertEquals("An assistance task for the user was successfully created.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		void AssertNoMenuItemForCapabilityToAssist(string message, ZToolStripMenuItem capabilitySubMenuItem, string subMenuItemDescription, GlbCapability capability)
		{
			var menuItem = GetSubMenuItem(capabilitySubMenuItem, subMenuItemDescription);
			var actuals = GetActualSubMenuItemsMessage(capabilitySubMenuItem);
			AssertNull($"{message} - {subMenuItemDescription}{actuals}", menuItem);
			Assert($"{message} - {capability.G4_Code}{actuals}", !GetSubMenuItemsContaining(capabilitySubMenuItem, capability.G4_Code).Any());
		}

		void AssertMenuItemForCapabilityToAssist(string message, ProcessHeader workflow, ZToolStripMenuItem capabilitySubMenuItem, string expectedSubMenuItemDescription, GlbCapability expectedAssignedCapability)
		{
			var menuItem = GetSubMenuItem(capabilitySubMenuItem, expectedSubMenuItemDescription);
			var actuals = GetActualSubMenuItemsMessage(capabilitySubMenuItem);
			AssertNotNull($"{message} - {expectedSubMenuItemDescription}{actuals}", menuItem);

			AssertEquals("Precondition: the assist task should not exist yet", 0, GetAssistTasks(workflow.PK).Count(x => x.P9_G4_RequiredCapability == expectedAssignedCapability.PK));

			menuItem.PerformClick();
			Application.DoEvents();

			var assistTask = GetAssistTasks(workflow.PK).SingleOrDefault(x => x.P9_G4_RequiredCapability == expectedAssignedCapability.PK);
			AssertNotNull("An assist task should have been created and saved", assistTask);

			AssertEquals("AST", assistTask.P9_Type);
			AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
			AssertEquals(10, assistTask.P9_Sequence);
			AssertEquals(expectedAssignedCapability.PK, assistTask.P9_G4_RequiredCapability);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
			AssertEquals("Assist", assistTask.P9_Description);

			AssertEquals("An assistance task for the capability was successfully created.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		string GetActualSubMenuItemsMessage(ZToolStripMenuItem menuItem)
		{
			var builder = new ZStringBuilder(@"
Actual menu items:");

			foreach (var item in GetSubMenuItems(menuItem))
			{
				builder.Append(item.Text);
			}
			builder.AppendLine();
			return builder.ToStringWithNewLineBetweenAppends();
		}

		IEnumerable<ProcessTask> GetAssistTasks(ZGuid workflowPK)
		{
			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflowPK);
			return loadedWorkflow.Tasks.Where(x => x.P9_Description == "Assist").ToArray();
		}

		void AssertContainsMenuItemsInExactOrder(ZToolStripMenuItem staffSubMenuItem, IEnumerable<string> expectedSubMenuItemDescriptions)
		{
			AssertContainsExactElementsInExactOrder(expectedSubMenuItemDescriptions, GetSubMenuItems(staffSubMenuItem).Select(i => i.Text));
		}

		#region Avoid Duplication of Assistance Tasks When Using Add Assistance For Staff

		public void TestAddAssistanceTaskForStaff_ShouldNotDuplicateExistingAssistanceTask_WithStatusWRK_AndSameSequenceNumber()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "WRK";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAddAssistanceTaskForStaff_ShouldNotDuplicateExistingAssistanceTask_WithStatusSUS_AndSameSequenceNumber()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "SUS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAddAssistanceTaskForStaff_ShouldNotDuplicateExistingAssistanceTask_WithStatusASN_AndSameSequenceNumber()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, userToAssist);
		}
		public void TestAddAssistanceTaskForStaff_ShouldNotDuplicateExistingAssistanceTask_WithStatusOPN_AndSameSequenceNumber()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "OPN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAddAssistanceTaskForStaff_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsClosed()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "CLS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAddAssistanceTaskForStaff_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsCancelled()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "CAN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAddAssistanceTaskForStaff_ShouldNotDuplicateExistingAssistanceTask_WithDifferentSequenceNumberButStartable()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 1;
				Assert("Precondition", assistanceTask.P9_Sequence < sourceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAddAssistanceTaskForStaff_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsNotStartable_AndHasDifferentSequenceNumber()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			AssertAddAssistanceTaskForStaff_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = userToAssist.GS_Code;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 100;
				Assert("Precondition", assistanceTask.P9_Sequence > sourceTask.P9_Sequence);
			}, userToAssist);
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_EventWhenThereIsAssistanceTaskAssignedToGlobalCapabilityRequestedUserPossesses()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: false);
			capability.ResourcesWithCapability.Add(userToAssist);

			AssertAddAssistanceTaskForStaff_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
			}, userToAssist);
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_EventWhenThereIsAssistanceTaskAssignedToGroupCapabilityRequestedUserPossesses_AndTaskIsAssignedToUserGroup()
		{
			var userToAssist = Factory.NewWithValidTestData<GlbStaff>();

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(userToAssist);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(userToAssist);

			AssertAddAssistanceTaskForStaff_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			}, userToAssist);
		}

		void AssertAddAssistanceTaskForStaff_DoesNotDuplicateExistingAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup, GlbStaff userToAssist)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			var assistanceTaskInitialStatus = assistanceTask.P9_Status;
			var assistanceTaskInitialSequence = assistanceTask.P9_Sequence;

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, userToAssist.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var mainMenuItem = GetMenu(sourceTask, form, "Add Assistance Task For");
				AssertNotNull(mainMenuItem);
				var staffSubMenuItem = GetSubMenuItem(mainMenuItem, "Staff");
				AssertNotNull(staffSubMenuItem);
				var menuItem = GetSubMenuItem(staffSubMenuItem, userToAssist);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("An assist task should be used instead of creating a new task.", new[] { "Other user's task", "Existing assistance task" }, tasks.Select(x => x.P9_Description));

			var existingTask = tasks.Single(x => x.P9_Description == "Existing assistance task");
			AssertEquals("AST", existingTask.P9_Type);
			AssertEquals(15, (ZInt)existingTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(3m, existingTask.P9_EstimateVariationFactor);
			AssertEquals(assistanceTaskInitialSequence, existingTask.P9_Sequence);
			AssertEquals(userToAssist.GS_Code, existingTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("The status should not change", assistanceTaskInitialStatus, existingTask.P9_Status);
			AssertEquals("Existing assistance task", existingTask.P9_Description);

			AssertEquals("An appropriate assistance task for this user already exists.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void AssertAddAssistanceTaskForStaff_CreatesNewAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup, GlbStaff userToAssist)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, userToAssist.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var mainMenuItem = GetMenu(sourceTask, form, "Add Assistance Task For");
				AssertNotNull(mainMenuItem);
				var staffSubMenuItem = GetSubMenuItem(mainMenuItem, "Staff");
				AssertNotNull(staffSubMenuItem);
				var menuItem = GetSubMenuItem(staffSubMenuItem, userToAssist);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("A new assist task should have been created and saved.", new[] { "Other user's task", "Existing assistance task", "Assist" }, tasks.Select(x => x.P9_Description));

			var newTask = tasks.Single(x => x.P9_Description == "Assist");
			AssertEquals("AST", newTask.P9_Type);
			AssertEquals(defaultAssistanceTaskEstimate, (int)newTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals((ZDecimal)defaultAssistanceTaskVariationFactor, newTask.P9_EstimateVariationFactor);
			AssertEquals(10, newTask.P9_Sequence);
			AssertEquals(userToAssist.GS_Code, newTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, newTask.P9_Status);
			AssertEquals("Assist", newTask.P9_Description);

			AssertEquals("An assistance task for the user was successfully created.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion

		#region Avoid Duplication of Assistance Tasks When Using Add Assistance For Capability

		public void TestAddAssistanceTaskForCapability_ShouldNotDuplicateExistingAssistanceTask_WithStatusWRK_AndSameSequenceNumber()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "WRK";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		public void TestAddAssistanceTaskForCapability_ShouldNotDuplicateExistingAssistanceTask_WithStatusSUS_AndSameSequenceNumber()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "SUS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		public void TestAddAssistanceTaskForCapability_ShouldNotDuplicateExistingAssistanceTask_WithStatusASN_AndSameSequenceNumber()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, capabilityToAssist);
		}
		public void TestAddAssistanceTaskForCapability_ShouldNotDuplicateExistingAssistanceTask_WithStatusOPN_AndSameSequenceNumber()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "OPN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		public void TestAddAssistanceTaskForCapability_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsClosed()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "CLS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		public void TestAddAssistanceTaskForCapability_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsCancelled()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "CAN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		public void TestAddAssistanceTaskForCapability_ShouldNotDuplicateExistingAssistanceTask_WithDifferentSequenceNumberButStartable()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 1;
				Assert("Precondition", assistanceTask.P9_Sequence < sourceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		public void TestAddAssistanceTaskForCapability_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsNotStartable_AndHasDifferentSequenceNumber()
		{
			var capabilityToAssist = Factory.NewWithValidTestData<GlbCapability>();

			AssertAddAssistanceTaskForCapability_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_G4_RequiredCapability = capabilityToAssist.PK;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 100;
				Assert("Precondition", assistanceTask.P9_Sequence > sourceTask.P9_Sequence);
			}, capabilityToAssist);
		}

		void AssertAddAssistanceTaskForCapability_DoesNotDuplicateExistingAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup, GlbCapability capabilityToAssist)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			var assistanceTaskInitialStatus = assistanceTask.P9_Status;

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, capabilityToAssist.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var mainMenuItem = GetMenu(sourceTask, form, "Add Assistance Task For");
				AssertNotNull(mainMenuItem);
				var capabilitySubMenuItem = GetSubMenuItem(mainMenuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);
				var menuItem = GetSubMenuItem(capabilitySubMenuItem, capabilityToAssist);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("An assist task should be used instead of creating a new task.", new[] { "Other user's task", "Existing assistance task" }, tasks.Select(x => x.P9_Description));

			var existingTask = tasks.Single(x => x.P9_Description == "Existing assistance task");
			AssertEquals("AST", existingTask.P9_Type);
			AssertEquals(15, (ZInt)existingTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(3m, existingTask.P9_EstimateVariationFactor);
			Assert(existingTask.P9_GS_NKAssignedStaffMember.IsEmpty);
			AssertEquals(capabilityToAssist.PK, existingTask.P9_G4_RequiredCapability);
			AssertEquals("The status should not change", assistanceTaskInitialStatus, existingTask.P9_Status);
			AssertEquals("Existing assistance task", existingTask.P9_Description);

			AssertEquals("An appropriate assistance task for this capability already exists.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void AssertAddAssistanceTaskForCapability_CreatesNewAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup, GlbCapability capabilityToAssist)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer);
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser.GS_Code, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, otherUser.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, capabilityToAssist.PK, overrideChannels: true);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var mainMenuItem = GetMenu(sourceTask, form, "Add Assistance Task For");
				AssertNotNull(mainMenuItem);
				var capabilitySubMenuItem = GetSubMenuItem(mainMenuItem, "Capability");
				AssertNotNull(capabilitySubMenuItem);
				var menuItem = GetSubMenuItem(capabilitySubMenuItem, capabilityToAssist);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertContainsExactElementsInAnyOrder("A new assist task should have been created and saved.", new[] { "Other user's task", "Existing assistance task", "Assist" }, tasks.Select(x => x.P9_Description));

			var newTask = tasks.Single(x => x.P9_Description == "Assist");
			AssertEquals("AST", newTask.P9_Type);
			AssertEquals(defaultAssistanceTaskEstimate, (int)newTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals((ZDecimal)defaultAssistanceTaskVariationFactor, newTask.P9_EstimateVariationFactor);
			AssertEquals(10, newTask.P9_Sequence);
			Assert(newTask.P9_GS_NKAssignedStaffMember.IsEmpty);
			AssertEquals(capabilityToAssist.PK, newTask.P9_G4_RequiredCapability);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, newTask.P9_Status);
			AssertEquals("Assist", newTask.P9_Description);

			AssertEquals("An assistance task for the capability was successfully created.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		static void AssertMenuHaveActionAndCustomizeItens(ZToolStripMenuItem menu, OperationalAction expectedAction)
		{
			var taskActionList = menu.DropDownItems.Cast<ToolStripItem>().Where(item => item is ZToolStripMenuItem);

			AssertEquals("Should have 2 ZToolStripMenuItems", 2, taskActionList.Count());
			AssertArrayEqualsByElements("Should have the Action and Customize", taskActionList.Select(item => item.Text).ToArray(), new string[] { expectedAction.SU_MenuName, "Customize" });
		}

		void AssertShowFormWithAction(ZToolStripMenuItem menu, OperationalAction expectedAction)
		{
			var menuItems = menu.DropDownItems.Cast<ToolStripItem>().Where(item => item is ZToolStripMenuItem);
			var menuAction = menuItems.Single(item => item.Text == expectedAction.SU_MenuName);

			menuAction.PerformClick();

			AssertNotNull("should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("should have shown the correct form", "OperationalActionRunner", ZFormModaliser.LastFormShownDialogForTest.Text);

			OperationalActionRunner runner = (OperationalActionRunner)((ZForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest;
			AssertEquals("should have used the correct action", expectedAction.PK, runner.Action.PK);
			AssertNotEquals("runner form should not be using the menu factory", Factory, runner.Action.Factory);
		}

		static ZToolStripMenuItem GetActionMenu(ZToolStripMenuItem operationalActionsMenu, string text)
		{
			var taskActionMenu = operationalActionsMenu.DropDownItems.Cast<ZToolStripMenuItem>().FirstOrDefault(menu => menu.Text == text);
			return taskActionMenu;
		}

		static ZToolStripMenuItem GetOperationActionMenu(VisualBoardForm form)
		{
			var taskCard = form.FindAll<TaskCardControl>().Single();

			taskCard.ShouldPreloadSubMenus_ForTest = true;
			taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

			return taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().FirstOrDefault(menu => menu.Text == "Operational Actions");
		}

		static ZToolStripMenuItem GetMenu(ProcessTask task, VisualBoardForm form, string menuText)
		{
			var taskCard = form.FindAll<TaskCardControl>().Single(x => x.CardContent.GetTask(task.Factory) == task);

			return GetMenu(taskCard, menuText);
		}

		static ZToolStripMenuItem GetMenu(ProcessHeader workflow, VisualBoardForm form, string menuText)
		{
			var taskCard = form.FindAll<TaskCardControl>().Single(x => x.CardContent.GetWorkflow(workflow.Factory) == workflow);

			return GetMenu(taskCard, menuText);
		}

		static ZToolStripMenuItem GetMenu(TaskCardControl taskCard, string menuText)
		{
			taskCard.ShouldPreloadSubMenus_ForTest = true;
			taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

			return taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(menu => menu.Text == menuText);
		}

		static IEnumerable<ZToolStripMenuItem> GetSubMenuItems(ZToolStripMenuItem menuItem)
		{
			menuItem.ShowDropDown(); // to generate Popup event to force lazy population
			menuItem.HideDropDown();
			return menuItem.DropDownItems.OfType<ZToolStripMenuItem>().ToArray();
		}

		static ZToolStripMenuItem GetSubMenuItem(ZToolStripMenuItem menuItem, string menuText)
		{
			menuItem.ShowDropDown(); // to generate Popup event to force lazy population
			menuItem.HideDropDown();
			return menuItem.DropDownItems.OfType<ZToolStripMenuItem>().SingleOrDefault(menu => menu.Text == menuText);
		}

		static ZToolStripMenuItem GetSubMenuItem(ZToolStripMenuItem menuItem, GlbStaff staff)
		{
			menuItem.ShowDropDown(); // to generate Popup event to force lazy population
			menuItem.HideDropDown();
			return menuItem.DropDownItems.OfType<ZToolStripMenuItem>().SingleOrDefault(menu => menu.Text.Contains(staff.GS_Code));
		}

		static ZToolStripMenuItem GetSubMenuItem(ZToolStripMenuItem menuItem, GlbCapability capability)
		{
			menuItem.ShowDropDown(); // to generate Popup event to force lazy population
			menuItem.HideDropDown();
			return menuItem.DropDownItems.OfType<ZToolStripMenuItem>().SingleOrDefault(menu => menu.Text.Contains(capability.G4_Code));
		}

		static IEnumerable<ZToolStripMenuItem> GetSubMenuItemsContaining(ZToolStripMenuItem menuItem, string menuTextSubstring)
		{
			menuItem.ShowDropDown(); // to generate Popup event to force lazy population
			menuItem.HideDropDown();
			return menuItem.DropDownItems.OfType<ZToolStripMenuItem>().Where(menu => menu.Text.Contains(menuTextSubstring)).ToArray();
		}

		OperationalAction CreateAction(ModuleIdentifier moduleIdentifier)
		{
			var action = Factory.New<OperationalAction>();

			using (var module = ZFilterModule.GetZFilterModule(moduleIdentifier) as ZFilterGridModule)
			{
				var context = new OperationalActionContext((module as IOperationalActionSupportable).OperationalActionSupporter, module.ID.Description, module.WorkflowType);
				action.Context = context;
				action.SU_MenuName = moduleIdentifier.Name;
			}

			return action;
		}

		ProcessHeader CreateWorkflowAndTask()
		{
			return BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "Workflow1",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-20),
				description: "Task1",
				staffCode: GlbStaff.CurrentUser.GS_Code);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		VisualBoardTestConfig config;

		#endregion
	}
}
