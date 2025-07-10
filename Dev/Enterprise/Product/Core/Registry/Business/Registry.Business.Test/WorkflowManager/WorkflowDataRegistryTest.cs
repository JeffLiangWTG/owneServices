using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowDataRegistry))]
	sealed class WorkflowDataRegistryTest : RegistryItemSetTestCaseWithFactory<WorkflowDataRegistry>
	{
		public void TestTaskTypesRegistryRefresh()
		{
			WorkflowDataRegistryTestHelper.SetupTaskTypesRegistry();

			var collection = WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value;
			var item = collection.AddNew();
			item.WorkflowType = "WKI";
			AssertEquals(4, item.TaskTypeList.Count);

			var taskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var codes = taskTypes.GetTaskTypesFromWorkflowCode("WKI");
			var newType = codes.AddNew();
			newType.Code = "OOG";
			newType.Description = (NoResString)"Oooga";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taskTypes);

			AssertEquals(5, item.TaskTypeList.Count);
		}

		#region ConditionallyVisibleRegistryItems

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment";
				yield return "CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff";
				yield return "CreateNewWorkflowsForQualityIterationsByDefault";
			}
		}

		#endregion

		#region RequireHasCapabilityTests

		public void TestRequireResourceToHaveCapability_DefaultValue_IsFalse()
		{
			AssertEquals(false, ItemSet.RequireResourceToHaveCapability.DefaultValue);
		}

		public void TestRequireResourceToHaveCapability_SetValues()
		{
			ItemSet.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.RequireResourceToHaveCapability.Value);

			ItemSet.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.RequireResourceToHaveCapability.DefaultValue);
		}

		#endregion

		public void TestRegistryItemDefaultValues()
		{
			AssertEquals(false, ItemSet.PreventMilestoneFutureActualStart.DefaultValue);
			AssertEquals(true, ItemSet.DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave.DefaultValue);
			AssertEquals(false, ItemSet.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.DefaultValue);
			AssertEquals(false, ItemSet.AllowTriggersToFireForExistingEvents.DefaultValue);
			AssertEquals(true, ItemSet.EnableWorkflowTemplateAppliedEvent.DefaultValue);
			AssertEquals(true, ItemSet.EnableWorkflowTemplateScopeRestrictions.DefaultValue);
			AssertEquals(true, ItemSet.EnableUserDefinedConditionFastFailing.DefaultValue);
			AssertEquals(TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly, ItemSet.EnableTemplateApplicationConcurrencyProtection.DefaultValue);
			AssertEquals(true, ItemSet.EnableTemplateApplicationRaceConditionHandlerProcessTasksLock.DefaultValue);
			AssertEquals(false, ItemSet.EnableWorkflowEstimateMeasurement.DefaultValue);
			AssertEquals(true, ItemSet.AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates.DefaultValue);
			AssertEquals(false, ItemSet.ExceptionEXRReference.DefaultValue);
			AssertEquals(true, ItemSet.FilterCustomFieldsByParentTable.DefaultValue);
			AssertEquals(true, ItemSet.EnableTemplatePotentialLoopValidation.DefaultValue);
			AssertEquals(TaskDefaultOpeningBehaviourOptions.Codes.Job, ItemSet.TaskDefaultOpeningBehaviour.DefaultValue);
		}

		public void TestValidationRuleTimeout()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.ValidationRuleTimeout,
				nameof(WorkflowDataRegistry.Instance.ValidationRuleTimeout),
				WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates_ValidationTools,
				"Validation Rule Timeout",
				"The maximum number of seconds a workflow validation rule will run before timing out. Any rule that breaches this threshold will be considered as failing and handled accordingly.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				5,
				1,
				int.MaxValue);
		}

		public void TestEnableWorkflowValidation()
		{
			TestGenericRegistryItem(ItemSet.EnableWorkflowValidation,
				"EnableWorkflowValidation",
				WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates_ValidationTools,
				"Enable Workflow Validation",
				"Use this registry to manage what workflow templates can access the custom validation rules. Process types listed below will have the validation tab available and will cause all applicable jobs to search for existing rules. ",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);
		}

		public void TestEnableDateLimitsOnWorkflowTemplates()
		{
			AssertEquals(true, ItemSet.EnableDateLimitsOnWorkflowTemplates.DefaultValue);

			var template = Factory.New<IProcessTaskTemplate>();
			var bizo = (BusinessObject)template;
			bizo.FillWithValidTestData();
			bizo[ProcessTaskTemplateSchema.P0_IsActive] = true;
			bizo[ProcessTaskTemplateSchema.P0_EffectiveStartDateUtc] = ZDateTime.UtcNow;

			Factory.Save();

			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false));
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false));
		}

		public void TestEnableWorkflowEstimateMeasurement()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement,
				nameof(WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement),
				RawDataRegistry.Categories.WorkflowManager,
				"Enable Workflow Task Estimate to Actual Measurement",
				"When enabled, task estimates and actuals will be measured showing changes in estimates from when work production starts.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableTriggerUserContextConfiguration()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration,
				nameof(WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration),
				RawDataRegistry.Categories.WorkflowManager,
				"Enable Controlling Trigger User Context",
				"Allow Triggers and Milestones to configure the user context used when processing Completion Trigger Actions. When this is disabled, any existing Triggers and Milestones with a non-default user context will be ignored.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
		}

		public void TestEnableTemplateApplicationRaceConditionHandlerProcessTasksLock()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.EnableTemplateApplicationRaceConditionHandlerProcessTasksLock,
				nameof(WorkflowDataRegistry.Instance.EnableTemplateApplicationRaceConditionHandlerProcessTasksLock),
				WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates,
				"Template Application Race Condition Handler Process Tasks Lock",
				"When enabled race conditions in template application will have locks on related Process Tasks.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				true);
		}

		public void TestEnableWorkflowTemplateApplicationConcurrencyProtection()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection,
				nameof(WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection),
				WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates,
				"Workflow Template Concurrency Protection",
				"When enabled race conditions in workflow template application will be detected and handled during save.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				true);
		}

		public void TestEnableTemplateApplicationInMemoryFiltering()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.EnableTemplateApplicationInMemoryFiltering,
				nameof(WorkflowDataRegistry.Instance.EnableTemplateApplicationInMemoryFiltering),
				WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates,
				"Workflow Template In Memory Filtering",
				"Workflow Templates are cached and then filtered in memory for application. Disabling this registry item uses the database to do the filtering, resulting in more database reads and less in memory caching.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				true);
		}

		public void TestEnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock()
		{
			TestRegistryItem(
				WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock,
				nameof(WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock),
				WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates,
				"Workflow Template Application Race Condition Handler Process Header Lock",
				"When enabled race conditions in template application will have locks on related Process Headers (workflows).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				true);
		}

		#region TaskAssignmentTests
		public void TestTaskAssignmentRestrictions()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			TestGenericRegistryItem(ItemSet.TaskAssignmentRestrictions, "ProcessManagerTaskTypeRestrictions", "Workflow Manager/Task Assignment", "Task Assignment Restrictions",
				"Defines restrictions on whether the same or different resources should be assigned to tasks of the specified task types.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			var newValue = new TaskTypeRestrictionsCollection();
			var restriction = newValue.AddNew();

			restriction.Active = true;
			restriction.WorkflowType = "WKI";
			restriction.TaskType = "UDF";
			restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			restriction.Scope = ScopeList.Codes.Workflow;
			restriction.NotificationType = NotificationTypeList.Codes.Error;

			ItemSet.TaskAssignmentRestrictions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Count", 1, ItemSet.TaskAssignmentRestrictions.Value.Count);
			AssertEquals("Active", true, ItemSet.TaskAssignmentRestrictions.Value[0].Active);
			AssertEquals("WorkflowType", "WKI", ItemSet.TaskAssignmentRestrictions.Value[0].WorkflowType);
			AssertEquals("TaskType", "UDF", ItemSet.TaskAssignmentRestrictions.Value[0].TaskType);
			AssertEquals("RestrictionType", RestrictionTypeList.Codes.DifferentResource, ItemSet.TaskAssignmentRestrictions.Value[0].RestrictionType);
			AssertEquals("Scope", ScopeList.Codes.Workflow, ItemSet.TaskAssignmentRestrictions.Value[0].Scope);
			AssertEquals("NotificationType", NotificationTypeList.Codes.Error, ItemSet.TaskAssignmentRestrictions.Value[0].NotificationType);
		}

		public void TestTaskAssignmentAutoAssignStaff()
		{
			AssertEquals("Name", "TaskAssignmentAutoAssignStaff", ItemSet.TaskAssignmentAutoAssignStaff.Name);
			AssertEquals("Auto Assign Staff or Group Code", ItemSet.TaskAssignmentAutoAssignStaff.Caption);
			AssertEquals("By default, succeeding tasks that are created have the staff code or group code applied from the preceding task. Turning this setting off will leave the fields blank.", ItemSet.TaskAssignmentAutoAssignStaff.Hint);
			AssertEquals("Workflow Manager/Task Assignment", ItemSet.TaskAssignmentAutoAssignStaff.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TaskAssignmentAutoAssignStaff.Storage);
			AssertEquals("Default value for TaskAssignmentAutoAssignStaff", true, ItemSet.TaskAssignmentAutoAssignStaff.DefaultValue);
		}

		#endregion

		public void TestTaskTypes()
		{
			TestGenericRegistryItem(ItemSet.TaskTypes, "ProcessManagerTaskTypes", "Workflow Manager", "Task Types",
				"This is the list of task types for Workflow Manager. " +
				"You may select whether a task type creates an appointment with the assigned staff member. " +
				"You may also specify whether a task type restricts user from closing tasks that are not assigned to them. " +
				"It is also possible to link the task types to specific Resource Capabilities, for Buffer Management purposes. " +
				"\'Allow Working Status Changes in Buckets\' works in conjunction with the \'Allow working on any task in any component\' security setting.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			Assert(ItemSet.TaskTypes.Value.Any(element => ((RegistryBusinessObject)element).Code == "STA"));

			var newValue = new CategorisedWorkflowTaskTypesCollection();
			newValue.AddNew().Code = "X";

			ItemSet.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 1, ItemSet.TaskTypes.Value.Count);
			AssertEquals("Value[0].Code", "X", ItemSet.TaskTypes.Value[0].Code);
		}

		public void TestIterationReasons()
		{
			TestGenericRegistryItem(ItemSet.IterationReasons, "ProcessManagerIterationReasons", "Workflow Manager/Quality Iterations", "Quality Iteration Reasons",
				"The list of reasons that can be given for triggering a quality iteration.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			Assert(ItemSet.IterationReasons.Value.Any(element => ((RegistryBusinessObject)element).Code == "STA"));

			var newValue = new CategorisedWorkflowIterationReasonsCollection();
			newValue.AddNew().Code = "X";

			ItemSet.IterationReasons.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 1, ItemSet.IterationReasons.Value.Count);
			AssertEquals("Value[0].Code", "X", ItemSet.IterationReasons.Value[0].Code);
		}

		public void TestCreateNewWorkflowsForQualityIterationsByDefault_WhenBufferManagementEnabled()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var item = ItemSet.CreateNewWorkflowsForQualityIterationsByDefault;
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(true, item.DefaultValue);
		}

		public void TestCreateNewWorkflowsForQualityIterationsByDefault_WhenBufferManagementDisabled()
		{
			AssertEquals("We expect Buffer Management to be disabled by default.", false, ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled);

			var item = ItemSet.CreateNewWorkflowsForQualityIterationsByDefault;
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly, item.Options);
			AssertEquals(false, item.DefaultValue);
		}

		public void TestTurnOnFunctionalityUnderDevelopment()
		{
			var item = ItemSet.TurnOnFunctionalityUnderDevelopment;
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(false, item.DefaultValue);
		}

		public void TestPrefillResourceUnderReview()
		{
			AssertEquals("Default should be true", true, ItemSet.PrefillResourceUnderReview.DefaultValue);
		}

		public void TestRequireResourceUnderReview()
		{
			AssertEquals("Default should be true", true, ItemSet.RequireResourceUnderReview.DefaultValue);
		}

		public void TestWorkflowManagerNotificationGroup()
		{
			AssertEquals("Workflow Manager Notification Group", ItemSet.WorkflowManagerNotificationGroup.Caption);
			AssertEquals("The staff group that will be notified about Workflow Manager failures, warnings and other information.", ItemSet.WorkflowManagerNotificationGroup.Hint);
			AssertEquals("Workflow Manager", ItemSet.WorkflowManagerNotificationGroup.Category);
			AssertEquals(RegistryOptions.IsValueMandatory, ItemSet.WorkflowManagerNotificationGroup.Options);
			AssertEquals(RegistryStorageFlags.All, ItemSet.WorkflowManagerNotificationGroup.Storage);

			AssertEquals("Default value for Workflow Manager Notification Group", Core.Constants.Groups.AllPK, ItemSet.WorkflowManagerNotificationGroup.DefaultValue);

			Guid newGuid = Guid.NewGuid();
			ItemSet.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals(newGuid, ItemSet.WorkflowManagerNotificationGroup.Value);
		}

		public void TestClientInTemplateSelectionCriteria()
		{
			string expectedHint = "Some Workflow Templates can be set for a specific \"Client\".\r\n\r\n"
					+ "Use this registry to match the \"Client\" on a specified Workflow Template Type to an organization on a job the template is to be applied for. For certain Template types the \"Client\" may be interpreted in several different roles – this registry allows you to set up the order in which organizations on the job should be matched to the \"Client\".\r\n\r\n"
					+ "For example, Shipment Workflow Template Type will try to match the \"Client\" field first to the \"Consignor/Consignee\" organization on a Shipment (depending on the job direction), if no Consignor (or Consignee) present, it will try to match \"Client\" to the \"Local Client\" organization, etc.";

			AssertEquals("Client in Template Selection Criteria", ItemSet.ClientInTemplateSelection.Caption);
			AssertEquals(expectedHint, ItemSet.ClientInTemplateSelection.Hint);
			AssertEquals("Workflow Manager/Workflow Templates", ItemSet.ClientInTemplateSelection.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ClientInTemplateSelection.Storage);
		}

		public void TestEDICommunicationModeFallbackToOrganizationEmail()
		{
			AssertEquals(false, ItemSet.EDICommunicationModeFallbackToOrganizationEmail.DefaultValue);
		}

		public void TestShowCustomFieldsFromCurrentTemplateOnly()
		{
			AssertEquals("Default should be false", false, ItemSet.ShowCustomFieldsFromCurrentTemplateOnly.DefaultValue);
		}

		public void TestEnableTemplateReleaseGroupRules()
		{
			TestRegistryItem(ItemSet.EnableTemplateReleaseGroupRules, "EnableTemplateReleaseGroupRules", WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates, "Enable Template Release Group Rules", "When set to Yes, the Release Group Rules tab will be shown on Workflow Templates, allowing the Release Group of workflows on jobs which match the template's selection criteria to be defaulted according to the rules.", RegistryStorageFlags.System, true);
		}

		public void TestCheckOnlyNonSecurityGroupsOnRemovalOfLastStaff()
		{
			TestRegistryItem(WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff,
				"CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff",
				WorkflowDataRegistry.Categories.WorkflowManager_ResourceCapabilities,
				"Check only non-security groups on removal of last staff",
				"When removing a staff member from a GRP-scoped capability or a group, if this removal leaves the capability-group intersection without any staff, a confirmation message is shown. By default, the confirmation is shown for security and non-security groups. When this value is set to YES, the message is only shown for non-security groups.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestDeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave()
		{
			TestRegistryItem(ItemSet.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave, "DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave", WorkflowDataRegistry.Categories.WorkflowManager, "Defer Firing Workflow on Consolidation AWB Action", "When enabled, the Consolidation's Print Final Master Action will be executed without waiting for Workflow Actions and Template application to be completed.", RegistryStorageFlags.System, false);
		}

		public void TestCompletionMilestoneOutcome()
		{
			AssertEquals("Name", "CompletionMilestoneOutcome", ItemSet.CompletionMilestoneOutcome.Name);
			AssertEquals("Category", "Workflow Manager", ItemSet.CompletionMilestoneOutcome.Category);
			AssertEquals("Caption", "Completion Milestone Outcome", ItemSet.CompletionMilestoneOutcome.Caption);
			AssertEquals("Description", @"This controls what happens when a Completion Milestone occurs. Options are:
- (Code: CAN) The system attempts to change the task status to canceled (""CAN"").
- (Code: CLS) The system attempts to change the task status to closed (""CLS"") if it has a non-Zero Actual Duration and it is assigned to a Staff or Group; otherwise, the status will remain unchanged.
- (Code: FBK) The system attempts to change the task status to closed (""CLS"") if it has a non-zero Actual Duration and it is assigned to a Staff or Group; otherwise, the system will attempt to change the task status to canceled (""CAN"") (default option).

Tasks have mandatory and optional conditions that need to be met in order to have a CLS status. This includes tasks requiring a staff or group being assigned to them. Optionally, if a task is set to require an actual duration greater than zero, this can prevent a CLS status. Failure to meet these or other validation conditions will mean that users will be presented with errors to resolve, or an automated process will be unsuccessful in setting the status of tasks to CLS.", ItemSet.CompletionMilestoneOutcome.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.CompletionMilestoneOutcome.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CompletionMilestoneOutcome.Options);
			AssertEquals("Default Value", CompletionMilestoneOutcomeModes.Codes.AttemptToClose, ItemSet.CompletionMilestoneOutcome.DefaultValue);
		}

		#region ExceptionTests

		public void TestExceptionCategories()
		{
			TestGenericRegistryItem(ItemSet.ExceptionCategories, "ExceptionCategories", "Workflow Manager/Exceptions", "Exception Categories",
				"Categories that the exceptions can be organized into",
				RegistryStorageFlags.System);

			AssertEquals("Empty by default", 0, ItemSet.ExceptionCategories.Value.Count);

			var newValue = new CodeDescriptionPairList();
			newValue.AddPair("XYZ", "My favourite category");

			ItemSet.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 1, ItemSet.ExceptionCategories.Value.Count);
			AssertEquals("Value[0].Code", "XYZ", ItemSet.ExceptionCategories.Value[0].Code);
			AssertEquals("Value[0].Descritpion", "My favourite category", ItemSet.ExceptionCategories.Value[0].Description);

			newValue.AddPair("XYZ", "My favourite category");
			AssertExceptionThrown<Exception>(() => ItemSet.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue));
			newValue.RemoveAt(1);

			newValue.AddPair("XY1", "My favourite category");
			AssertExceptionThrown<Exception>(() => ItemSet.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue));
			newValue.RemoveAt(1);

			newValue.AddPair("   ", "My favouritest category");
			AssertExceptionThrown<Exception>(() => ItemSet.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue));
			newValue.RemoveAt(1);

			newValue.AddPair("XY1", "");
			AssertExceptionThrown<Exception>(() => ItemSet.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue));
			newValue.RemoveAt(1);

			newValue.AddPair("XY1", "My favouritest category");
			ItemSet.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value.Count", 2, ItemSet.ExceptionCategories.Value.Count);
		}

		public void TestExceptionRequireType()
		{
			AssertEquals("Name", "ExceptionRequireType", ItemSet.ExceptionRequireType.Name);
			AssertEquals("Exception Require Type", ItemSet.ExceptionRequireType.Caption);
			AssertEquals("When enabled exception require type to be saved.", ItemSet.ExceptionRequireType.Hint);
			AssertEquals("Workflow Manager/Exceptions", ItemSet.ExceptionRequireType.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ExceptionRequireType.Storage);
			AssertEquals("Default value for ExceptionRequireType", false, ItemSet.ExceptionRequireType.DefaultValue);
		}

		public void TestExceptionEXRReference()
		{
			AssertEquals("Name", "ExceptionEXRReference", ItemSet.ExceptionEXRReference.Name);
			AssertEquals("Exception EXR Reference", ItemSet.ExceptionEXRReference.Caption);
			AssertEquals(@"When set to Yes, the Reference for EXR events will be a list of parameters in the form ""|TYP=Exception Type|EVT=Event Code|DES=Milestone Description"".
When set to No, the Reference will be a string in the form ""Type: [Exception Type]; Event: [Event Code]"".", ItemSet.ExceptionEXRReference.Hint);
			AssertEquals("Workflow Manager/Exceptions", ItemSet.ExceptionEXRReference.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ExceptionEXRReference.Storage);
			AssertEquals("Default value for ExceptionEXRReference", false, ItemSet.ExceptionEXRReference.DefaultValue);
		}

		public void TestExceptionRequireStaffGroup()
		{
			AssertEquals("Name", "ExceptionRequireStaffGroup", ItemSet.ExceptionRequireStaffGroup.Name);
			AssertEquals("Exception Requires Staff and/or Group", ItemSet.ExceptionRequireStaffGroup.Caption);
			AssertEquals(@"When enabled, Staff and/or Group are required to be saved.", ItemSet.ExceptionRequireStaffGroup.Hint);
			AssertEquals("Workflow Manager/Exceptions", ItemSet.ExceptionRequireStaffGroup.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ExceptionRequireStaffGroup.Storage);
			AssertEquals("Default value for ExceptionRequireStaffGroup", false, ItemSet.ExceptionRequireStaffGroup.DefaultValue);
		}

		public void TestExceptionAutoAssignStaff()
		{
			AssertEquals("Name", "ExceptionAutoAssignStaff", ItemSet.ExceptionAutoAssignStaff.Name);
			AssertEquals("Exception Staff Auto-Assignment", ItemSet.ExceptionAutoAssignStaff.Caption);
			AssertEquals(@"When enabled, the current User is assigned if no other Staff and/or Group field(s) are assigned when Exception(s) is selected as Actioned.", ItemSet.ExceptionAutoAssignStaff.Hint);
			AssertEquals("Workflow Manager/Exceptions", ItemSet.ExceptionAutoAssignStaff.Category);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ExceptionAutoAssignStaff.Storage);
			AssertEquals("Default value for ExceptionAutoAssignStaff", true, ItemSet.ExceptionAutoAssignStaff.DefaultValue);
		}
		//registry items based on Workflow Types populated from the global workflow descriptors list
		//where Description has a resource string.
		//therefore we need to add some exclusions to allow for resource string loading.
		protected override IEnumerable<string> AllowDemandResourceStrings { get; } = new[]
		{
				nameof(WorkflowDataRegistry.IterationReasons),
				nameof(WorkflowDataRegistry.TaskTypes)
		};

		#endregion
	}
}
