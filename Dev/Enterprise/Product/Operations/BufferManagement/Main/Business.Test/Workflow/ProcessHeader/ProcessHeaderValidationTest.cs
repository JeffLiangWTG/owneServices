using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderValidationTest : BusinessObjectValidationTestCase
	{
		#region Date Defaults From

		public void TestFH_AgreedDeliveryDateDefaultsFrom_WhenInvalidValueThenShouldError()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			Factory.Save();

			workflow.FH_AgreedDeliveryDateDefaultsFrom = "???";
			AssertListValidationInvalidCodeError(workflow.FH_AgreedDeliveryDateDefaultsFromInfo, isExpectingError: true);

			workflow.FH_AgreedDeliveryDateDefaultsFrom = "ETA";
			AssertNoErrors($"When value is valid THEN FH_AgreedDeliveryDateDefaultsFrom should not error", workflow.FH_AgreedDeliveryDateDefaultsFromInfo);
		}

		public void TestFH_AgreedDeliveryDateDefaultsFrom_WhenProcessTypeModifiedThenListShouldBeUpdated()
		{
			var brkSystem = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode);
			var shpSystem = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			Factory.Save();

			workflow.FH_AgreedDeliveryDateDefaultsFrom = "ETD";
			AssertListValidationInvalidCodeError(workflow.FH_AgreedDeliveryDateDefaultsFromInfo, isExpectingError: true);

			workflow.FH_AgreedDeliveryDateDefaultsFrom = "ETA";
			AssertNoErrors($"When value is valid THEN FH_AgreedDeliveryDateDefaultsFrom should not error", workflow.FH_AgreedDeliveryDateDefaultsFromInfo);

			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			workflow.FH_AgreedDeliveryDateDefaultsFrom = "ETD";
			AssertNoErrors($"When value is valid THEN FH_AgreedDeliveryDateDefaultsFrom should not error", workflow.FH_AgreedDeliveryDateDefaultsFromInfo);
		}

		public void TestFH_EarliestStartDateDefaultsFrom_WhenInvalidValueThenShouldError()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: "SHP");
			var workflow = BMSTestHelper.CreateWorkflow(template);

			Factory.Save();

			workflow.FH_EarliestStartDateDefaultsFrom = "???";
			AssertListValidationInvalidCodeError(workflow.FH_EarliestStartDateDefaultsFromInfo, isExpectingError: true);

			workflow.FH_EarliestStartDateDefaultsFrom = "ETA";
			AssertNoErrors($"When value is valid THEN FH_EarliestStartDateDefaultsFrom should not error", workflow.FH_EarliestStartDateDefaultsFromInfo);
		}

		public void TestFH_EarliestStartDateDefaultsFrom_WhenProcessTypeModifiedThenListShouldBeUpdated()
		{
			var brkSystem = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode);
			var shpSystem = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: "BRK");
			var workflow = BMSTestHelper.CreateWorkflow(template);

			Factory.Save();

			workflow.FH_EarliestStartDateDefaultsFrom = "???";
			AssertListValidationInvalidCodeError(workflow.FH_EarliestStartDateDefaultsFromInfo, isExpectingError: true);

			workflow.FH_EarliestStartDateDefaultsFrom = "ETA";
			AssertNoErrors($"When value is valid THEN FH_EarliestStartDateDefaultsFrom should not error", workflow.FH_EarliestStartDateDefaultsFromInfo);

			template.P0_ProcessType = "SHP";
			workflow.FH_EarliestStartDateDefaultsFrom = "ETD";
			AssertNoErrors($"When value is valid THEN FH_EarliestStartDateDefaultsFrom should not error", workflow.FH_EarliestStartDateDefaultsFromInfo);
		}

		public void TestFH_AgreedDeliveryDate_ShouldWarnUser_WhenADDDefaultsFromIsSpecified()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Tom's mandarin tree");

			workflow.Lookups.DatesDefaultsFromList.AddPair("ETA", "Estimated Time to Aquaplane");

			AssertEquals(true, workflow.FH_AgreedDeliveryDate.IsEmpty);

			workflow.FH_AgreedDeliveryDateDefaultsFrom = "ETA";
			AssertNoWarnings(workflow.FH_AgreedDeliveryDateInfo);

			workflow.FH_AgreedDeliveryDate = ZDateTime.Now;
			AssertHasWarning(workflow.FH_AgreedDeliveryDateInfo, "This field is currently configured to match the Estimated Time to Aquaplane field. Your changes will be automatically discarded on Save. In order to ensure your changes are not discarded, please remove the 'ADD Defaults From' value from this workflow.");

			workflow.FH_AgreedDeliveryDateDefaultsFrom = string.Empty;
			AssertNoWarnings(workflow.FH_AgreedDeliveryDateInfo);
		}

		public void TestFH_DoNotStartBeforeDate_ShouldWarnUser_WhenESDDefaultsFromIsSpecified()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Tom's chilli plant");

			workflow.Lookups.DatesDefaultsFromList.AddPair("ETD", "Estimated Time to Dance");

			AssertEquals(true, workflow.FH_DoNotStartBeforeDate.IsEmpty);

			workflow.FH_EarliestStartDateDefaultsFrom = "ETD";
			AssertNoWarnings(workflow.FH_DoNotStartBeforeDateInfo);

			workflow.FH_DoNotStartBeforeDate = ZDateTime.Now;
			AssertHasWarning(workflow.FH_DoNotStartBeforeDateInfo, "This field is currently configured to match the Estimated Time to Dance field. Your changes will be automatically discarded on Save. In order to ensure your changes are not discarded, please remove the 'ESD Defaults From' value from this workflow.");

			workflow.FH_EarliestStartDateDefaultsFrom = string.Empty;
			AssertNoWarnings(workflow.FH_DoNotStartBeforeDateInfo);
		}

		#endregion

		#region CurrentComponent

		public void TestCurrentComponent_ForProcessHeadersNotLinkedToAJob()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var nonLinkedWorkflow = Factory.NewWithValidTestData<ProcessHeader>();
			nonLinkedWorkflow.FH_FH_ParentHeader = jobHeader.PK;

			AssertEquals(bucket, workflow.CurrentComponent);
			AssertNull(nonLinkedWorkflow.CurrentComponent);

			nonLinkedWorkflow.Validation.ValidateAll();
			AssertNoErrors(nonLinkedWorkflow.FH_FC_CurrentComponentInfo);

			nonLinkedWorkflow.FH_ParentId = jobHeader.FH_ParentId;

			AssertNoErrors(nonLinkedWorkflow.FH_FC_CurrentComponentInfo);
			AssertEquals(bucket, nonLinkedWorkflow.CurrentComponent);
		}

		public void TestCurrentComponent_ShouldBeChangableWithoutSecurityOnlyWhenBlank()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket2.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				workflow.FH_FC_CurrentComponent = bucket1.PK;
				AssertHasError(workflow.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");

				var newWorkflow = workflow.JobHeader.ProcessHeaders.AddNew();
				AssertEquals(bucket1.PK, newWorkflow.FH_FC_CurrentComponent);
				AssertNoErrors(newWorkflow.FH_FC_CurrentComponentInfo);
			}
		}

		public void TestCurrentComponent_ShouldBeMandatoryOnWorkflowsWithBMSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket = system.Components.AddNew();

			var orgWorkflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var salesEnquiryWorkflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<SalesEnquiry>(), Factory).ProcessHeaders.AddNew();

			orgWorkflow.FH_FC_CurrentComponent = ZGuid.Empty;

			AssertHasError(orgWorkflow.FH_FC_CurrentComponentInfo, "Please enter a Current Component.");
			AssertNoErrors(salesEnquiryWorkflow.FH_FC_CurrentComponentInfo);
		}

		public void TestTemplateWorkflowsShouldNotValidateCurrentComponent()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var component1 = system.Components.AddNew();
			component1.FC_Name = "bucket1";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "bucket2";

			var link1_2 = component1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_ProcessType = "ORG";

				var workflow = (ProcessHeader)template.ProcessHeaders.AddNew();
				workflow.Validation.ValidateAll();

				AssertNoErrors(workflow.FH_FC_CurrentComponentInfo);
			}
		}

		public void TestCurrentComponent_WhenSecurityDenied_ShouldHaveError()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "Bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "Bucket2";
			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			Factory.Save();

			Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

			header.FH_FC_CurrentComponent = bucket2.PK;
			header.Validation.ValidateFH_FC_CurrentComponent();
			AssertHasErrors(header.FH_FC_CurrentComponentInfo);

			Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = true;

			header.Validation.ValidateFH_FC_CurrentComponent();
			AssertNoErrors(header.FH_FC_CurrentComponentInfo);
		}

		public void TestCurrentComponentSystemPK()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system1.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			bucket1.FC_DisplaySequence = 1;
			var bucket11 = system1.Components.AddNew();
			bucket11.FC_Name = "bucket11";
			bucket11.FC_DisplaySequence = 2;
			var link1 = bucket1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = bucket11.PK;

			var system2 = BMSTestHelper.CreateSystem(Factory, "INQ");
			var bucket2 = system2.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			bucket2.FC_DisplaySequence = 1;
			var bucket22 = system2.Components.AddNew();
			bucket22.FC_DisplaySequence = 2;
			bucket22.FC_Name = "bucket22";
			var link2 = bucket2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = bucket22.PK;

			Factory.Save();

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			AssertEquals(system1.PK, workflow.CurrentComponentSystemPK);

			workflow.CurrentComponentSystemPK = system2.PK;
			AssertNoErrors(workflow.CurrentComponentSystemPKInfo);
			AssertNoErrors(workflow.FH_FC_CurrentComponentInfo);
			AssertEquals(bucket2.PK, workflow.FH_FC_CurrentComponent);

			workflow.CurrentComponentSystemPK = ZGuid.Invalid;
			AssertHasError(workflow.CurrentComponentSystemPKInfo, "Enter a valid System.");
			AssertHasError(workflow.FH_FC_CurrentComponentInfo, "Enter a valid Current Component.");

			workflow.CurrentComponentSystemPK = ZGuid.Empty;
			AssertHasError(workflow.CurrentComponentSystemPKInfo, "Please enter a System.");
			AssertHasError(workflow.FH_FC_CurrentComponentInfo, "Enter a valid Current Component.");
		}

		#endregion

		#region ReleaseGroup

		public void TestReleaseGroup_ForTemplateProcessHeaders()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var workflow = template.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			AssertNoErrors(workflow.FH_GG_ReleaseGroupInfo);
		}

		public void TestReleaseGroup_ForProcessHeadersNotLinkedToAJob()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var nonLinkedWorkflow = Factory.NewWithValidTestData<ProcessHeader>();
			nonLinkedWorkflow.FH_FH_ParentHeader = jobHeader.PK;

			nonLinkedWorkflow.Validation.ValidateAll();
			AssertNoErrors(nonLinkedWorkflow.FH_GG_ReleaseGroupInfo);

			nonLinkedWorkflow.FH_ParentId = jobHeader.FH_ParentId;

			AssertNoErrors(nonLinkedWorkflow.FH_GG_ReleaseGroupInfo);
		}

		public void TestValidateReleaseGroup_WorkflowTemplates_ShouldNotBeMandatory()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.ReleaseGroups.AddNew();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var workflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			workflow.Validation.ValidateFH_GG_ReleaseGroup();
			AssertNoErrors(workflow.FH_GG_ReleaseGroupInfo);
		}

		public void TestValidateReleaseGroup_NoReleaseGroupsDefined()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.New<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.Validation.ValidateAll();
			AssertNoError(workflow.FH_GG_ReleaseGroupInfo, "Please enter a Release Group.");

			var group = Factory.New<GlbGroup>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.Validation.ValidateAll();
			AssertEquals(ZGuid.Empty, workflow.FH_GG_ReleaseGroup);
			AssertHasError(workflow.FH_GG_ReleaseGroupInfo, "Please enter a Release Group.");
		}

		public void TestValidateReleaseGroup_NotRequiredInRegistry()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.New<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

			var group = Factory.New<GlbGroup>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			workflow.Validation.ValidateFH_GG_ReleaseGroup();
			AssertEquals(ZGuid.Empty, workflow.FH_GG_ReleaseGroup);
			AssertHasError(workflow.FH_GG_ReleaseGroupInfo, "Please enter a Release Group.");

			BMSRegistry.Instance.RequireReleaseGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			workflow.Validation.ValidateFH_GG_ReleaseGroup();
			AssertNoError(workflow.FH_GG_ReleaseGroupInfo, "Please enter a Release Group.");
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "You have not entered a Release Group.");
		}

		public void TestValidateReleaseGroup_InactiveGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var group = Factory.New<GlbGroup>();
			group.GG_IsActive = false;
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var job = Factory.New<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

			workflow.FH_GG_ReleaseGroup = group.PK;
			AssertHasError(workflow.FH_GG_ReleaseGroupInfo, "This Release Group is inactive - it may not be used.");
		}

		public void TestValidateReleaseGroup_WhenProcessHeaderDeleted_ShouldNotReportErrors()
		{
			var releaseGroup = BMSTestHelper.CreateGroup(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: releaseGroup.PK);

			workflow.Delete();
			AssertNoExceptionThrown(() => workflow.Validation.ValidateFH_GG_ReleaseGroup());
		}

		#region Release Group Has No Members

		public void TestFH_GG_ReleaseGroup_WhenReleaseGroupHasNoMembers_TriggeredByChangingReleaseGroup()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = BMSTestHelper.CreateStaff(Factory, "S1", "Staff");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");

			group1.Staff.Add(staff);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			workflow.Validation.ValidateAll();
			AssertNoNotifications("The selected group is blank so this notification should not be shown. SAD!", workflow.FH_GG_ReleaseGroupInfo);

			workflow.FH_GG_ReleaseGroup = group1.PK;
			AssertNoNotifications("The selected group has members so no notifications are necessary. SAD!", workflow.FH_GG_ReleaseGroupInfo);

			workflow.FH_GG_ReleaseGroup = group2.PK;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The assigned release group has no members.");

			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workflow.Validation.ValidateFH_GG_ReleaseGroup();
			AssertHasError(workflow.FH_GG_ReleaseGroupInfo, "The assigned release group has no members.");
		}

		public void TestFH_GG_ReleaseGroup_WhenReleaseGroupHasNoMembers_TriggeredByChangingTaskStaffOnAnyTask()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = BMSTestHelper.CreateStaff(Factory, "S1", "Staff");
			var group = BMSTestHelper.CreateGroup(Factory, "G2", "Group");

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group.PK);
			var task = BMSTestHelper.CreateTask(workflow, staffCode: staff.GS_Code);

			Factory.Save();

			workflow.Validation.ValidateAll();
			AssertNoNotifications("There is an assigned staff member so there should't be notifications on the release group. SAD!", workflow.FH_GG_ReleaseGroupInfo);

			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The assigned release group has no members.");
		}

		public void TestFH_GG_ReleaseGroup_WhenReleaseGroupHasNoMembers_TriggeredByChangingTaskGroupOnAnyTask()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = BMSTestHelper.CreateStaff(Factory, "S1", "Staff");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");

			group1.Staff.Add(staff);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group2.PK);
			var task = BMSTestHelper.CreateTask(workflow, taskGroupPK: group1.PK);

			Factory.Save();

			workflow.Validation.ValidateAll();
			AssertNoNotifications("The task has an assigned group so this notification should not be shown. SAD!", workflow.FH_GG_ReleaseGroupInfo);

			task.P9_GG_AssignedGroup = ZGuid.Empty;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The assigned release group has no members.");
		}

		public void TestFH_GG_ReleaseGroup_WhenReleaseGroupHasNoMembers_TriggeredByChangingTaskCapabilityOnAnyTask()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = BMSTestHelper.CreateStaff(Factory, "S1", "Staff");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");

			group1.Staff.Add(staff);

			var capability = BMSTestHelper.CreateCapability(Factory, "C1", "Capability");

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group2.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability);

			Factory.Save();

			workflow.Validation.ValidateAll();
			AssertNoNotifications("The task has an assigned capability so this notification should not be shown. SAD!", workflow.FH_GG_ReleaseGroupInfo);

			task.P9_G4_RequiredCapability = ZGuid.Empty;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The assigned release group has no members.");
		}

		public void TestFH_GG_ReleaseGroup_WhenReleaseGroupHasNoMembers_AndAllTasksHaveGlobalCapabilitiesAssigned_ShouldNotHaveNotification()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");
			var capability = BMSTestHelper.CreateCapability(Factory, "C1", "Capability");
			AssertEquals(GlbCapabilityScopeList.Codes.GlobalScope, capability.G4_CapacityScope);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			AssertHasWarning("There should be a warning because one of the tasks doesn't have any capability assigned. SAD!", workflow.FH_GG_ReleaseGroupInfo, "The assigned release group has no members.");

			task2.P9_G4_RequiredCapability = capability.PK;
			AssertNoNotifications("All tasks are now assigned to a global capability, so no warning should be shown. SAD!", workflow.FH_GG_ReleaseGroupInfo);
		}

		public void TestCheckReleaseGroupHasMembers_ShouldNotLoadGlbStaffRecords()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(BMSTestHelper.CreateStaff(Factory, "S1"));
			group.Staff.Add(BMSTestHelper.CreateStaff(Factory, "S2"));
			var capability = BMSTestHelper.CreateCapability(Factory, isGroupScope: true);

			Factory.Save();

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group.PK);
			BMSTestHelper.CreateTask(workflow1, capability: capability);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group.PK);
			BMSTestHelper.CreateTask(workflow1, capability: capability);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Miss Rona" };
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow1.PK);
			var validation = new ProcessHeaderValidation_ForTest(loadedWorkflow);
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckReleaseGroupHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbGroupLink"));
				AssertEquals("All other conditions were met, so we should have executed a query to check for group membership. SAD!", 1, pivotCommands.Count());
			}

			var allRowsQuery = new ZQuery { FetchOnlyFromLocalCache = true };
			var loadedStaff = newFactory.Load<GlbStaff>(allRowsQuery);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Release Group. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivots = newFactory.Load<GlbGroupLink>(allRowsQuery);
			AssertEquals("One pivot row should be loaded the first time the validation runs (if there isn't one in the factory already). SAD!", 1, loadedPivots.Length);

			loadedWorkflow = newFactory.Load<ProcessHeader>(workflow2.PK); // the other workflow
			validation = new ProcessHeaderValidation_ForTest(loadedWorkflow);
			expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories("No new hits should be required since we've already loaded the same pivots for checking the previous workflow. SAD!", expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckReleaseGroupHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbGroupLink"));
				AssertEquals("No new hits should be required since we've already loaded the same pivots for checking the previous workflow. SAD!", 0, pivotCommands.Count());
			}

			loadedStaff = newFactory.Load<GlbStaff>(allRowsQuery);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Release Group. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			loadedPivots = newFactory.Load<GlbGroupLink>(allRowsQuery);
			AssertEquals("One pivot row should be loaded the first time the validation runs (if there isn't one in the factory already). SAD!", 1, loadedPivots.Length);
		}

		class ProcessHeaderValidation_ForTest : ProcessHeaderValidation
		{
			public ProcessHeaderValidation_ForTest(AutoProcessHeader parent)
				: base(parent)
			{
			}

			public void CheckReleaseGroupHasMembers_Exposed()
			{
				CheckReleaseGroupHasMembers();
			}
		}

		#endregion

		#region Intersection Release Group and Capability Has No Members

		public void TestFH_GG_ReleaseGroup_WhenIntersectionOfReleaseGroupAndCapabilityHasNoMembers_TriggeredByChangingReleaseGroup()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = BMSTestHelper.CreateStaff(Factory, "S1", "Staff 1");
			var staff2 = BMSTestHelper.CreateStaff(Factory, "S2", "Staff 2");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");
			var capability1 = BMSTestHelper.CreateCapability(Factory, "C1", "Capability 1", isGroupScope: true);
			var capability2 = BMSTestHelper.CreateCapability(Factory, "C2", "Capability 2", isGroupScope: true);

			group1.Staff.Add(staff1);
			group2.Staff.Add(staff2);
			capability1.ResourcesWithCapability.Add(staff1);
			capability2.ResourcesWithCapability.Add(staff2);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group1.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability1);

			Factory.Save();

			task.Validation.ValidateAll();
			workflow.Validation.ValidateAll();
			AssertNoNotifications("The group/capability combination has a common resource, so no notifications are needed. SAD!", workflow.FH_GG_ReleaseGroupInfo);

			workflow.FH_GG_ReleaseGroup = group2.PK;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");

			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workflow.Validation.ValidateFH_GG_ReleaseGroup();
			AssertHasError(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");
		}

		public void TestFH_GG_ReleaseGroup_WhenIntersectionOfReleaseGroupAndCapabilityHasNoMembers_TriggeredByChangingTaskStaffOnAnyTask()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = BMSTestHelper.CreateStaff(Factory, "S1", "Staff 1");
			var staff2 = BMSTestHelper.CreateStaff(Factory, "S2", "Staff 2");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");
			var capability1 = BMSTestHelper.CreateCapability(Factory, "C1", "Capability 1", isGroupScope: true);
			var capability2 = BMSTestHelper.CreateCapability(Factory, "C2", "Capability 2", isGroupScope: true);

			group1.Staff.Add(staff1);
			group2.Staff.Add(staff2);
			capability1.ResourcesWithCapability.Add(staff1);
			capability2.ResourcesWithCapability.Add(staff2);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group1.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability2);

			Factory.Save();

			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");

			task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			AssertNoNotifications(workflow.FH_GG_ReleaseGroupInfo);

			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");
		}

		public void TestFH_GG_ReleaseGroup_WhenIntersectionOfReleaseGroupAndCapabilityHasNoMembers_TriggeredByChangingTaskGroupOnAnyTask()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = BMSTestHelper.CreateStaff(Factory, "S1", "Staff 1");
			var staff2 = BMSTestHelper.CreateStaff(Factory, "S2", "Staff 2");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");
			var capability1 = BMSTestHelper.CreateCapability(Factory, "C1", "Capability 1", isGroupScope: true);
			var capability2 = BMSTestHelper.CreateCapability(Factory, "C2", "Capability 2", isGroupScope: true);

			group1.Staff.Add(staff1);
			group2.Staff.Add(staff2);
			capability1.ResourcesWithCapability.Add(staff1);
			capability2.ResourcesWithCapability.Add(staff2);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group1.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability2);

			Factory.Save();

			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");

			task.P9_GG_AssignedGroup = group2.PK;
			AssertNoNotifications(workflow.FH_GG_ReleaseGroupInfo);
		}

		public void TestFH_GG_ReleaseGroup_WhenIntersectionOfReleaseGroupAndCapabilityHasNoMembers_TriggeredByChangingTaskCapabilityOnAnyTask()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = BMSTestHelper.CreateStaff(Factory, "S1", "Staff 1");
			var staff2 = BMSTestHelper.CreateStaff(Factory, "S2", "Staff 2");
			var group1 = BMSTestHelper.CreateGroup(Factory, "G1", "Group 1");
			var group2 = BMSTestHelper.CreateGroup(Factory, "G2", "Group 2");
			var capability1 = BMSTestHelper.CreateCapability(Factory, "C1", "Capability 1", isGroupScope: true);
			var capability2 = BMSTestHelper.CreateCapability(Factory, "C2", "Capability 2", isGroupScope: true);

			group1.Staff.Add(staff1);
			group2.Staff.Add(staff2);
			capability1.ResourcesWithCapability.Add(staff1);
			capability2.ResourcesWithCapability.Add(staff2);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", releaseGroupPK: group1.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability2);

			Factory.Save();

			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");

			task.P9_G4_RequiredCapability = capability1.PK;
			AssertNoNotifications(workflow.FH_GG_ReleaseGroupInfo);
		}

		#endregion

		#endregion

		#region Date Acceptability

		public void TestDateAcceptability_WhenNoAgreedDeliveryDateSpecified()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			jobHeader.FH_DateAcceptability = DateAcceptabilityList.Codes.GraduatedStartGraduatedFinish;
			AssertHasWarning(jobHeader.FH_DateAcceptabilityInfo, "Specifying a Date Acceptability value may be redundant without an Agreed Delivery Date on this workflow or the job-level workflow.");

			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow;
			AssertNoNotifications(jobHeader.FH_DateAcceptabilityInfo);
		}

		public void TestDateAcceptability_WhenNoAgreedDeliveryDateSpecified_ForTemplateWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflow(template);

			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.GraduatedStartGraduatedFinish;
			AssertNoNotifications(workflow.FH_DateAcceptabilityInfo);
		}

		[TestDate(2015, 7, 14, 17, 0, 0)]
		public void TestDateAcceptability_WhenAgreedDeliveryDateSpecifiedOnJob()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Dan's cat");

			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.GraduatedStartGraduatedFinish;
			AssertHasWarning(workflow.FH_DateAcceptabilityInfo, "Specifying a Date Acceptability value may be redundant without an Agreed Delivery Date on this workflow or the job-level workflow.");

			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow;
			AssertHasWarning(workflow.FH_DateAcceptabilityInfo, "This Date Acceptability will be used in conjunction with the Agreed Delivery Date of the job, which is 14-Jul-15 17:00.");

			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow;
			AssertNoWarnings(workflow.FH_DateAcceptabilityInfo);
		}

		[TestDate(2015, 7, 14)]
		public void TestAgreedDeliveryDate_WhenDateAcceptabilityIndicatesHardDueDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddMinutes(-1);
			AssertNoNotifications(jobHeader.FH_AgreedDeliveryDateInfo);

			jobHeader.FH_DateAcceptability = DateAcceptabilityList.Codes.SharpStartSharpFinish;
			AssertHasWarning(jobHeader.FH_AgreedDeliveryDateInfo, "This workflow was due to be completed in the past, and the Date Acceptability indicates this is a 'hard' due date.");

			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddMinutes(2);
			AssertNoNotifications(jobHeader.FH_AgreedDeliveryDateInfo);
		}

		#endregion

		#region Other Properties

		public void TestCompletionStatement()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = string.Empty;
			AssertHasErrors(workflow.FH_CompletionStatementInfo);
			workflow.FH_CompletionStatement = "MmmmmmMMMmmMMmmMMMMmMMMmMmMmMMMMMMmmmmMm";
			AssertNoErrors(workflow.FH_CompletionStatementInfo);
		}

		public void TestDateAcceptability()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders[0];

			header.Validation.ValidateFH_DateAcceptability();
			AssertNoError(header.FH_DateAcceptabilityInfo, "Enter a valid Date Acceptability.");

			header.FH_DateAcceptability = "ZZZ";
			AssertHasError(header.FH_DateAcceptabilityInfo, "Enter a valid Date Acceptability.");

			header.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			AssertNoError(header.FH_DateAcceptabilityInfo, "Enter a valid Date Acceptability.");
		}

		public void TestDeadlineType()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders[0];

			header.Validation.ValidateFH_DeadlineType();
			AssertNoError(header.FH_DeadlineTypeInfo, "Enter a valid Deadline Type.");

			header.FH_DeadlineType = "ZZZ";
			AssertHasError(header.FH_DeadlineTypeInfo, "Enter a valid Deadline Type.");

			header.FH_DeadlineType = DeadlineTypeList.Codes.Soft;
			AssertNoError(header.FH_DeadlineTypeInfo, "Enter a valid Deadline Type.");
		}

		public void TestQualityIterationReasonValidationNoWarning()
		{
			AssertQualityIterationReasonValidation(IterationReasonValidationList.Codes.None, false, false);
		}

		public void TestQualityIterationReasonValidationWarning()
		{
			AssertQualityIterationReasonValidation(IterationReasonValidationList.Codes.Warning, true, false);
		}

		public void TestQualityIterationReasonValidationError()
		{
			AssertQualityIterationReasonValidation(IterationReasonValidationList.Codes.Error, false, true);
		}

		void AssertQualityIterationReasonValidation(ZString validation, bool warning, bool error)
		{
			const string jobType = "DUM";

			_ = DummyWorkflowDescriptor.Instance;
			var iterationReasonsColl = WorkflowDataRegistry.Instance.IterationReasons.Value;
			var iterationReasons = iterationReasonsColl.OfType<CategorisedWorkflowIterationReasons>().First(n => n.Code == jobType);

			iterationReasons.Code = jobType;
			var iterationReason1 = iterationReasons.IterationReasons.AddNew();
			iterationReason1.Code = "RS1";
			iterationReason1.Description = (NoResString)"Reason 1";
			var iterationReason2 = iterationReasons.IterationReasons.AddNew();
			iterationReason2.Code = "RS2";
			iterationReason2.Description = (NoResString)"Reason 2";

			iterationReasons.IterationReasonValidation = validation;

			WorkflowDataRegistry.Instance.IterationReasons.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, iterationReasonsColl);

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", jobType);
			var job = Factory.New<DummyWithWorkflow>();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];

			workflow.FH_ParentTableCode = jobType;
			workflow.FH_ParentId = job.PK;
			task1.P9_Type = "QCB";

			var pivot1 = (IProcessTaskIterationLink)task1.IterationLinks.AddNew();
			pivot1.P9I_P9_IterationTask = task2.PK;
			pivot1.P9I_FH_IterationWorkflow = workflow.PK;
			pivot1.P9I_IterationReason = "RS1";

			Factory.Save();

			// Empty Reason
			workflow.IterationReason = "";
			AssertEquals("", workflow.IterationReason);
			if (warning)
			{
				AssertHasWarning(workflow.IterationReasonInfo, "You have not entered an Iteration Reason.");
			}
			else
			{
				AssertNoWarnings(workflow.IterationReasonInfo);
			}
			if (error)
			{
				AssertHasError(workflow.IterationReasonInfo, "Please enter an Iteration Reason.");
			}
			else
			{
				AssertNoErrors(workflow.IterationReasonInfo);
			}

			// Valid Reason
			workflow.IterationReason = "RS1";
			AssertEquals("RS1", workflow.IterationReason);
			AssertNoWarnings(workflow.IterationReasonInfo);
			AssertNoErrors(workflow.IterationReasonInfo);

			// Invalid Reason
			workflow.IterationReason = "ABC";
			AssertEquals("ABC", workflow.IterationReason);
			AssertNoWarnings(workflow.IterationReasonInfo);
			AssertHasError(workflow.IterationReasonInfo, "Enter a valid Iteration Reason.");

			// Valid Reason
			workflow.IterationReason = "RS2";
			AssertEquals("RS2", workflow.IterationReason);
			AssertNoWarnings(workflow.IterationReasonInfo);
			AssertNoErrors(workflow.IterationReasonInfo);
		}

		#endregion

		#region FH_Category

		public void TestCheckFH_Category_RaisesNoError_WhenAllIsOk()
		{
			var (code, description) = ("TST", "Test category");
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, code, description);

			workflow.FH_Category = code;
			Factory.Save();

			AssertEquals(code, workflow.FH_Category);
			AssertNoWarnings(workflow.FH_CategoryInfo);
			AssertNoErrors(workflow.FH_CategoryInfo);
		}

		public void TestCheckFH_Category_RaisesNoError_WhenNoCategoryConfiguredAndNoValueSet()
		{
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			Factory.Save();

			AssertEquals("UDF", workflow.FH_Category);
			AssertNoWarnings(workflow.FH_CategoryInfo);
			AssertNoErrors(workflow.FH_CategoryInfo);
		}

		public void TestCheckFH_Category_RaisesError_WhenCategoryIsEmpty()
		{
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			workflow.FH_Category = ZString.Empty;
			Factory.Save();

			AssertEquals(ZString.Empty, workflow.FH_Category);
			AssertNoWarnings(workflow.FH_CategoryInfo);
			AssertHasError(workflow.FH_CategoryInfo, "Please enter a Category.");
		}

		public void TestCheckFH_Category_RaisesError_WhenCategoryIsNotInValidList()
		{
			var code = "BAD";
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			workflow.FH_Category = code;
			Factory.Save();

			AssertEquals(code, workflow.FH_Category);
			AssertNoWarnings(workflow.FH_CategoryInfo);
			AssertHasError(workflow.FH_CategoryInfo, "Enter a valid Category.");
		}

		public void TestCheckFH_Category_RaisesError_WhenCategoryIsJOB()
		{
			var code = BMConstants.JobLevelWorkflowCategoryCode;
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			workflow.FH_Category = code;
			Factory.Save();

			AssertEquals(code, workflow.FH_Category);
			AssertNoWarnings(workflow.FH_CategoryInfo);
			AssertHasError(workflow.FH_CategoryInfo, "The JOB category is reserved for job level workflows only.");
		}

		#endregion

		#region FH_MilestoneCompletionKey

		public void TestFH_MilestoneCompletionKey()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var milestone = jobHeader.Parent.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "A bit of the noit";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			workflow.FH_MilestoneCompletionPivotKey = "";
			AssertNoWarnings(workflow.FH_MilestoneCompletionPivotKeyInfo);

			workflow.FH_MilestoneCompletionPivotKey = "Frogs";
			AssertHasWarning(workflow.FH_MilestoneCompletionPivotKeyInfo, "Does not match any milestone.");

			workflow.FH_MilestoneCompletionPivotKey = milestone.P9_MilestoneCompletionPivotKey;
			AssertNoWarnings(workflow.FH_MilestoneCompletionPivotKeyInfo);
		}

		#endregion

		public void TestCanSaveNotActiveWorkflowTemplateWithoutErrors()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DTY");
			template.P0_IsActive = false;

			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow.Validation.ValidateFH_P0_Template();

			AssertNoErrors(templateWorkflow.FH_P0_TemplateInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
