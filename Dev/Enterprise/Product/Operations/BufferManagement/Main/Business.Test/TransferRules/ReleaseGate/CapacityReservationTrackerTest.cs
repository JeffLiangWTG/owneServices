using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	abstract class CapacityReservationTrackerTest : BMSTestCaseWithFactory
	{
		#region Listing All Considered Resources

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestListingAllConsideredResourcesInCapacityReservationDetails()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = CreateCapability("SNP", "Snappy Snyappy");
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1);
			var resource3 = CreateStaffInCurrentBranchDept("ZOP", "Bups", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var task1 = CreateTask(workflow, string.Empty, 10, capability: capability1, estVariationFactor: 1);
			var task2 = CreateTask(workflow, string.Empty, 10, capability: capability1, estVariationFactor: 1);
			var task3 = CreateTask(workflow, string.Empty, 10, capability: capability1, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);
			AssertResourceCapacityReservationDetails(resource1.GS_Code, config.Buffer,
@"Release Gate Run at 29-Dec-14 12:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours, Other resources considered: Baps, Bups.
");
		}

		[TestDate(2015, 1, 2)]
		public void TestListingAllConsideredResourcesInReleaseNote()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var loginResource = CreateStaffInCurrentBranchDept("MAN", "The Man");
			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BAP", "Baps", capability);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var task1 = CreateTask(workflow, string.Empty, 60, capability: capability);
			var task2 = CreateTask(workflow, string.Empty, 60, capability: capability);
			var task3 = CreateTask(workflow, string.Empty, 60, capability: capability);

			Factory.Save();

			task1.P9_TaskID = "T00002000";
			task2.P9_TaskID = "T00002001";
			task3.P9_TaskID = "T00002002";

			Factory.Save();

			AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

			using (Env.SetTemporaryUserContext(loginResource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				workflow.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

				Factory.Save();
			}

			AssertMultilineASCIIEquals("Should log release details once workflow is saved",
@"It was manually released by [The Man].", workflow.GetSuccessfulReleaseNotes());
		}

		#region Capability Tasks

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForMultipleUnAssignedCapabilityTasks()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes", autoAssignTasks: true);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes", autoAssignTasks: true);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes", autoAssignTasks: true);
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1, capability3);
			CreateStaffInCurrentBranchDept("ZOP", "Bups", capability1, capability2, capability3); // Has a missing capability for the capability tasks, and he shouldn't be ignored because of this
			CreateStaffInCurrentBranchDept("BUP", "Blurps", capability1);
			CreateStaffInCurrentBranchDept("GUP", "Gurps", capability1);
			CreateStaffInCurrentBranchDept("SUP", "Shlurps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			CreateTask(workflow, string.Empty, 10, capability: capability1, estVariationFactor: 1);
			CreateTask(workflow, string.Empty, 10, capability: capability2, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails("Other resources should be considered as all capability tasks have no assigned resource.", resource1.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.11 hours, Remaining Capacity: 47.89 hours, Other resources considered: Baps, Blurps, Bups, Gurps, Shlurps.");

			AssertResourceCapacityReservationDetails("Other resources should be considered as all capability tasks have no assigned resource.", resource2.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.03 hours, Remaining Capacity: 47.97 hours, Other resources considered: Bendy, Blurps, Bups, Gurps, Shlurps.");
		}

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForSingleAssignedCapabilityTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes", autoAssignTasks: true);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes", autoAssignTasks: true);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes", autoAssignTasks: true);
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2);
			CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1, capability3);
			CreateStaffInCurrentBranchDept("ZOP", "Bups", capability2, capability3);
			CreateStaffInCurrentBranchDept("BUP", "Blurps", capability1);
			CreateStaffInCurrentBranchDept("GUP", "Gurps", capability1);
			CreateStaffInCurrentBranchDept("SUP", "Shlurps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);

			CreateTask(workflow, resource1.GS_Code, 10, capability: capability1, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails("Other resources should NOT be considered as all capability tasks have an assigned resource.", resource1.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours");
		}

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForSingleUnAssignedCapabilityTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes", autoAssignTasks: true);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes", autoAssignTasks: true);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes", autoAssignTasks: true);
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1, capability3);
			CreateStaffInCurrentBranchDept("ZOP", "Bups", capability1, capability2, capability3);
			CreateStaffInCurrentBranchDept("BUP", "Blurps", capability1);
			CreateStaffInCurrentBranchDept("GUP", "Gurps", capability1);
			CreateStaffInCurrentBranchDept("SUP", "Shlurps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			CreateTask(workflow, string.Empty, 10, capability: capability1, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails(resource1.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.03 hours, Remaining Capacity: 47.97 hours, Other resources considered: Baps, Blurps, Bups, Gurps, Shlurps.");

			AssertResourceCapacityReservationDetails(resource2.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.03 hours, Remaining Capacity: 47.97 hours, Other resources considered: Bendy, Blurps, Bups, Gurps, Shlurps.");
		}

		#endregion

		#region Staff Tasks

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForMultipleStaffTasks()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes");
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes");
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes");
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1, capability3);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);

			CreateTask(workflow, resource1.GS_Code, 10, estVariationFactor: 1);
			CreateTask(workflow, resource2.GS_Code, 10, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails(resource1.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours, Other resources considered: Baps.");

			AssertResourceCapacityReservationDetails(resource2.GS_Code, config.Buffer,
@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours, Other resources considered: Bendy.");
		}

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForSingleStaffTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes");
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes");
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes");
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2);
			CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1, capability3);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			CreateTask(workflow, resource1.GS_Code, 10, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails(resource1.GS_Code, config.Buffer,
				@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours");
		}

		#endregion

		#region Capability & Staff Tasks

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForSingleUnAssignedCapabilityTaskAndStaffTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes", autoAssignTasks: true);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes", autoAssignTasks: true);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes", autoAssignTasks: true);
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1, capability3);
			var resource3 = CreateStaffInCurrentBranchDept("ZOP", "Bups", capability2, capability3); // Does not possess capability of the unassigned capability task
			CreateStaffInCurrentBranchDept("BUP", "Blurps", capability1);
			CreateStaffInCurrentBranchDept("GUP", "Gurps", capability1);
			CreateStaffInCurrentBranchDept("SUP", "Shlurps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);

			CreateTask(workflow, string.Empty, 10, capability: capability1, estVariationFactor: 1); // Capability Task
			CreateTask(workflow, resource3.GS_Code, 10, estVariationFactor: 1);                     // Staff Task

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails("Other resources should be considered as all capability tasks have no assigned resource.", resource1.GS_Code, config.Buffer,
				@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.03 hours, Remaining Capacity: 47.97 hours, Other resources considered: Baps, Blurps, Bups, Gurps, Shlurps.");

			AssertResourceCapacityReservationDetails("Other resources should be considered as all capability tasks have no assigned resource.", resource2.GS_Code, config.Buffer,
				@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.03 hours, Remaining Capacity: 47.97 hours, Other resources considered: Bendy, Blurps, Bups, Gurps, Shlurps.");

			AssertResourceCapacityReservationDetails("Other resources should be considered as all capability tasks have no assigned resource.", resource3.GS_Code, config.Buffer,
				@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours, Other resources considered: Baps, Bendy, Blurps, Gurps, Shlurps.");
		}

		[TestDate(2017, 5, 30)]
		public void TestListingAllConsideredResources_InCapacityReservationDetails_IsLoggingCorrectly_ForSingleAssignedCapabilityTaskAndStaffTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = BMSTestHelper.CreateCapability(Factory, "WED", "It is Wednesday, my dudes", autoAssignTasks: true);
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability2 = BMSTestHelper.CreateCapability(Factory, "THU", "Inferior day, my dudes", autoAssignTasks: true);
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var capability3 = BMSTestHelper.CreateCapability(Factory, "FRI", "Pretty awesome day, my dudes", autoAssignTasks: true);
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1);
			CreateStaffInCurrentBranchDept("ZOP", "Bups", capability1, capability2, capability3);
			CreateStaffInCurrentBranchDept("BUP", "Blurps", capability1);
			CreateStaffInCurrentBranchDept("GUP", "Gurps", capability1);
			CreateStaffInCurrentBranchDept("SUP", "Shlurps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);

			CreateTask(workflow, resource1.GS_Code, 10, capability: capability1, estVariationFactor: 1); // Capability Task
			CreateTask(workflow, resource2.GS_Code, 10, estVariationFactor: 1);                          // Staff Task

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer);

			AssertResourceCapacityReservationDetails("Other resources should NOT be considered as all capability tasks have an assigned resource.", resource1.GS_Code, config.Buffer,
				@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours, Other resources considered: Baps.");

			AssertResourceCapacityReservationDetails("Other resources should be considered as there are other tasks with capability in this workflow.", resource2.GS_Code, config.Buffer,
				@"Release Gate Run at 30-May-17 00:00 UTC
RELEASED, XVBQP68SIYXQ - workflow, Available Capacity: 48 hours, Reservation: 0.17 hours, Remaining Capacity: 47.83 hours, Other resources considered: Bendy.");
		}

		#endregion

		#endregion

		#region Capability Transfers

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_MultipleCapabilities()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = CreateCapability("SNP", "Snappy Snyappy");
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var capability2 = CreateCapability("PAP", "Pappy Pappppp");
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var capability3 = CreateCapability("CAP", "Capitalism Bender");
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2, capability3);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 1, capability: capability1, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 1, capability: capability1, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2_1 = CreateTask(workflow2, string.Empty, 10, capability: capability1, estVariationFactor: 1);
			var task2_2 = CreateTask(workflow2, string.Empty, 10, capability: capability2, estVariationFactor: 1);
			var task2_3 = CreateTask(workflow2, string.Empty, 10, capability: capability3, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer);

			AssertResourceCapacityReservationDetails("", resource1.GS_Code, config.Buffer,
@"Release Gate Run at 29-Dec-14 12:00 UTC
RELEASED, XVBQP68SIYXQ - workflow2, Available Capacity: 47.98 hours, Reservation: 0.42 hours, Remaining Capacity: 47.56 hours, Other resources considered: Baps.");
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_MultipleCapabilitiesAndSingleTaskAssignedDirectly()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = CreateCapability("SNP", "Snappy Snyappy");
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var capability2 = CreateCapability("PAP", "Pappy Pappppp");
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var capability3 = CreateCapability("CAP", "Capitalism Bender");
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability1, capability2, capability3);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 1, capability: capability1, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 1, capability: capability1, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2_1 = CreateTask(workflow2, resource1.GS_Code, 10, estVariationFactor: 1);
			var task2_2 = CreateTask(workflow2, string.Empty, 10, capability: capability2, estVariationFactor: 1);
			var task2_3 = CreateTask(workflow2, string.Empty, 10, capability: capability3, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer);

			AssertResourceCapacityReservationDetails("", resource1.GS_Code, config.Buffer,
@"Release Gate Run at 29-Dec-14 12:00 UTC
RELEASED, XVBQP68SIYXQ - workflow2, Available Capacity: 47.98 hours, Reservation: 0.5 hours, Remaining Capacity: 47.48 hours");
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_CapabilityHasJustEnoughCapacity()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = CreateCapability("SNP", "Snappy Snyappy");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability);
			var resource3 = CreateStaffInCurrentBranchDept("ZIP", "Bapss", capability);
			var resource4 = CreateStaffInCurrentBranchDept("ZOP", "Bapsss", capability);
			var resource5 = CreateStaffInCurrentBranchDept("ZUP", "Bapssssssssssss", capability);

			const int fullCapacityMinutes = 48 * 60;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, fullCapacityMinutes, capability: capability, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, fullCapacityMinutes, capability: capability, estVariationFactor: 1);
			var task1_3 = CreateTask(workflow1, resource3.GS_Code, fullCapacityMinutes, capability: capability, estVariationFactor: 1);
			var task1_4 = CreateTask(workflow1, resource4.GS_Code, fullCapacityMinutes, capability: capability, estVariationFactor: 1);
			var task1_5 = CreateTask(workflow1, resource5.GS_Code, fullCapacityMinutes - 9, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 20, capability: capability, estVariationFactor: 1);

			Factory.Save();

			CombineAssertions("Pre-condition: available capacity", () =>
			{
				AssertAvailableCapacity("resource1", resource1, config.Buffer, 0m);
				AssertAvailableCapacity("resource2", resource2, config.Buffer, 0m);
				AssertAvailableCapacity("resource3", resource3, config.Buffer, 0m);
				AssertAvailableCapacity("resource4", resource4, config.Buffer, 0m);
				AssertAvailableCapacity("resource5", resource5, config.Buffer, 0.15m);
			});

			AssertCanReleaseToBuffer("There is 9 minutes of capacity across the capability, only. We need there to be 10 minutes in order to allow the 20 minute task to be released.", workflow2, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);

			var oldValue = task1_5.P9_EstDuration;
			task1_5.P9_EstDuration = new ZInt((int)task1_5.P9_EstDuration.GetMinutesFromDateTimeSpan() - 1).GetDateTimeFromMinutes();

			Factory.Save();

			AssertCanReleaseToBuffer("Now there are 10 minutes of capacity across the capability, so the workflow can be released.", workflow2, config.Buffer);
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_CapabilityServerelyOverloaded_OneResourceSeemsLikeItHasCapacity()
		{
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = CreateCapability("HAT", "High Accuracy Tree");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var resource1 = CreateStaffInCurrentBranchDept("BUS", "Bush", capability);
			var resource2 = CreateStaffInCurrentBranchDept("FUR", "Fern", capability);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 40 * 60, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 10, capability: capability, estVariationFactor: 1);

			Factory.Save();

			AssertCanReleaseToBuffer("resource 2 has excess capacity, but the negativeness of the other capacities does not stop release.", workflow2, config.Buffer);
			AssertMultilineASCIIEquals("", string.Empty, workflow2.GetSuccessfulReleaseNotes());
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_CapabilityServerelyOverloaded_LogTest()
		{
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.2m);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = CreateCapability("SNP", "Snappy Snyappy");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability);
			var resource3 = CreateStaffInCurrentBranchDept("ZIP", "Bapss", capability);
			var resource4 = CreateStaffInCurrentBranchDept("ZOP", "Bapsss", capability);
			var resource5 = CreateStaffInCurrentBranchDept("ZUP", "Bapssssssssssss", capability);
			var resource6 = CreateStaffInCurrentBranchDept("ZMP", "Bapssssssssssss", capability);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 40000, capability: capability, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 40000, capability: capability, estVariationFactor: 1);
			var task1_3 = CreateTask(workflow1, resource3.GS_Code, 40000, capability: capability, estVariationFactor: 1);
			var task1_4 = CreateTask(workflow1, resource4.GS_Code, 40000, capability: capability, estVariationFactor: 1);
			var task1_5 = CreateTask(workflow1, resource5.GS_Code, 40000, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 10, capability: capability, estVariationFactor: 1);

			Factory.Save();

			AssertCanReleaseToBuffer("resource 6 has excess capacity, but the negativeness of the other capacities stop release.", workflow2, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
			AssertMultilineASCIIEquals("", string.Empty, workflow2.GetSuccessfulReleaseNotes());
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_NegativeCapacityIsAddedProperly()
		{
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.2m);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(config.System, group);
			var capability = CreateCapability("SNP", "Snappy Snyappy");
			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability);
			var resource3 = CreateStaffInCurrentBranchDept("ZIP", "Bapss", capability);

			const decimal defaultAvailableCapacityHours = 48m;
			const int largeTaskEstimateInMinutes = (int)(defaultAvailableCapacityHours * 10 * 60); // This will be capped at -1.2x the full capacity, using the MaximumCapabilityTaskOverloadLimit registry item.
			const int taskEstimateInMinutesWhichLeavesEnoughSpareCapacity = (int)(33.35m * 60); // Two tasks at this estimate leaves 1 hour free capacity across all resources with this capability, assuming we cap the impact of the really overloaded resource by the MaximumCapabilityTaskOverloadLimit registry item.

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer, releaseGroupPK: group.PK);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, largeTaskEstimateInMinutes, capability: capability, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, taskEstimateInMinutesWhichLeavesEnoughSpareCapacity, capability: capability, estVariationFactor: 1);
			var task1_3 = CreateTask(workflow1, resource3.GS_Code, taskEstimateInMinutesWhichLeavesEnoughSpareCapacity, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			const int task2InitialEstimateMinutes = 60;
			var task2 = CreateTask(workflow2, string.Empty, task2InitialEstimateMinutes, capability: capability, estVariationFactor: 1);

			Factory.Save();

			CombineAssertions("Pre-condition: available capacity", () =>
			{
				AssertAvailableCapacity("resource1", resource1, config.Buffer, -432m);
				AssertAvailableCapacity("resource2", resource2, config.Buffer, 14.65m);
				AssertAvailableCapacity("resource3", resource3, config.Buffer, 14.65m);
			});

			AssertCanReleaseToBuffer(workflow2, config.Buffer);

			task2.P9_EstDuration = new ZInt(task2InitialEstimateMinutes + 2).GetDateTimeFromMinutes();
			Factory.Save();

			AssertCanReleaseToBuffer("Increasing the estimate by two minutes should ensure release fails", workflow2, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_CapabilityScopeIsConsideredByCapacityCalculations_GroupIgnoresOverloadedResourcesOutsideReleaseGroup()
		{
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(config.System, group);
			var capability = CreateCapability("SNP", "Snappy Snyappy");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability);
			group.Staff.Add(resource1);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability);
			var resource3 = CreateStaffInCurrentBranchDept("ZIP", "Bapss", capability);
			var resource4 = CreateStaffInCurrentBranchDept("ZOP", "Bapsss", capability);
			var resource5 = CreateStaffInCurrentBranchDept("ZUP", "Bapssssssssssss", capability);
			group.Staff.Add(resource5);

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource2.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource3.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource4.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource5.PK);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer, releaseGroupPK: group.PK);

			var taskSizeToConsumeEntireCapacity = (int)(config.Buffer.FC_BufferTimespanInMinutes * 0.5);

			var task1_1 = CreateTask(workflow1, resource1.GS_Code, taskSizeToConsumeEntireCapacity, capability: capability, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 1, capability: capability, estVariationFactor: 1);
			var task1_3 = CreateTask(workflow1, resource3.GS_Code, 1, capability: capability, estVariationFactor: 1);
			var task1_4 = CreateTask(workflow1, resource4.GS_Code, 1, capability: capability, estVariationFactor: 1);
			var task1_5 = CreateTask(workflow1, resource5.GS_Code, taskSizeToConsumeEntireCapacity, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 10, capability: capability, estVariationFactor: 1);

			Factory.Save();

			CombineAssertions("Pre-condition: available capacity", () =>
			{
				AssertAvailableCapacity("resource1", resource1, config.Buffer, 0m);
				AssertAvailableCapacity("resource2", resource2, config.Buffer, 47.98m);
				AssertAvailableCapacity("resource3", resource3, config.Buffer, 47.98m);
				AssertAvailableCapacity("resource4", resource4, config.Buffer, 47.98m);
				AssertAvailableCapacity("resource5", resource5, config.Buffer, 0m);
			});

			AssertCanReleaseToBuffer("Both resource1 and resource5 have no capacity. Resources 2 to 4 do have capacity, but they're outside the release group.", workflow2, config.Buffer, expectedReleaseOutcome: BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Bapssssssssssss: required 0.08 hours, currently has 0 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Snappy Snyappy]: T00001005 (0.08 hours)
	Bendy: required 0.08 hours, currently has 0 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Snappy Snyappy]: T00001005 (0.08 hours)");

			var oldValue = task1_5.P9_EstDuration;
			task1_5.P9_EstDuration = new ZInt(taskSizeToConsumeEntireCapacity - 6).GetDateTimeFromMinutes();

			Factory.Save();

			CombineAssertions("Updated available capacity", () =>
			{
				AssertAvailableCapacity("resource1", resource1, config.Buffer, 0m);
				AssertAvailableCapacity("resource2", resource2, config.Buffer, 47.98m);
				AssertAvailableCapacity("resource3", resource3, config.Buffer, 47.98m);
				AssertAvailableCapacity("resource4", resource4, config.Buffer, 47.98m);
				AssertAvailableCapacity("resource5", resource5, config.Buffer, 0.1m);
			});

			AssertCanReleaseToBuffer("resource 5 now has excess capacity", workflow2, config.Buffer);
			AssertMultilineASCIIEquals("", string.Empty, workflow2.GetSuccessfulReleaseNotes());
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_CapabilityScopeIsConsideredByCapacityCalculations_GroupIgnoresOverloadedResourcesOutsideReleaseGroup_NoReleaseGroup()
		{
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = CreateCapability("SNP", "Snappy Snyappy");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("ZUP", "Bapssssssssssss", capability);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);

			var taskSizeToConsumeEntireCapacity = (int)(config.Buffer.FC_BufferTimespanInMinutes * 0.5);

			var task1_1 = CreateTask(workflow1, resource1.GS_Code, taskSizeToConsumeEntireCapacity, capability: capability, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, taskSizeToConsumeEntireCapacity, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 10, capability: capability, estVariationFactor: 1);

			Factory.Save();

			CombineAssertions("Pre-condition: available capacity", () =>
			{
				AssertAvailableCapacity("resource1", resource1, config.Buffer, 0m);
				AssertAvailableCapacity("resource2", resource2, config.Buffer, 0m);
			});

			AssertCanReleaseToBuffer("Both resource1 and resource5 have no capacity.", workflow2, config.Buffer, expectedReleaseOutcome: BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Bapssssssssssss: required 0.08 hours, currently has 0 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Snappy Snyappy]: T00001002 (0.08 hours)
	Bendy: required 0.08 hours, currently has 0 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Snappy Snyappy]: T00001002 (0.08 hours)");

			var oldValue1 = task1_1.P9_EstDuration;
			var oldValue2 = task1_2.P9_EstDuration;
			task1_1.P9_EstDuration = new ZInt(taskSizeToConsumeEntireCapacity + 5).GetDateTimeFromMinutes(); // Slightly over capacity
			task1_2.P9_EstDuration = new ZInt(taskSizeToConsumeEntireCapacity - 10).GetDateTimeFromMinutes(); // Has enough capacity for the task, and to account for resource1's overload.

			Factory.Save();

			AssertCanReleaseToBuffer(workflow2, config.Buffer);
			AssertMultilineASCIIEquals("", string.Empty, workflow2.GetSuccessfulReleaseNotes());
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_CapabilityScopeIsConsideredByCapacityCalculations_GroupCaresForOverloadedResourcesInsideReleaseGroup()
		{
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.0m);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(config.System, group);
			var capability = CreateCapability("SNP", "Snappy Snyappy");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var resource1 = CreateStaffInCurrentBranchDept("ZAP", "Bendy", capability);
			group.Staff.Add(resource1);
			var resource2 = CreateStaffInCurrentBranchDept("ZEP", "Baps", capability);
			group.Staff.Add(resource2);
			var resource3 = CreateStaffInCurrentBranchDept("ZIP", "Bapss", capability);
			var resource4 = CreateStaffInCurrentBranchDept("ZOP", "Bapsss", capability);
			var resource5 = CreateStaffInCurrentBranchDept("ZUP", "Bapssssssssssss", capability);
			group.Staff.Add(resource5);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 2880, capability: capability, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 3505, capability: capability, estVariationFactor: 1);
			var task1_3 = CreateTask(workflow1, resource3.GS_Code, 3505, capability: capability, estVariationFactor: 1);
			var task1_4 = CreateTask(workflow1, resource4.GS_Code, 3505, capability: capability, estVariationFactor: 1);
			var task1_5 = CreateTask(workflow1, resource5.GS_Code, 2234, capability: capability, estVariationFactor: 1);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket, releaseGroupPK: group.PK);
			var task2 = CreateTask(workflow2, string.Empty, 10, capability: capability, estVariationFactor: 1);

			Factory.Save();

			CombineAssertions("Pre-condition: available capacities", () =>
			{
				AssertAvailableCapacity("resource1", resource1, config.Buffer, 0m);
				AssertAvailableCapacity("resource2", resource2, config.Buffer, -10.42m);
				AssertAvailableCapacity("resource3", resource3, config.Buffer, -10.42m);
				AssertAvailableCapacity("resource4", resource4, config.Buffer, -10.42m);
				AssertAvailableCapacity("resource5", resource5, config.Buffer, 10.77m);
			});

			AssertCanReleaseToBuffer("resource 5 has excess capacity and does not care about resources2 overloadedness", workflow2, config.Buffer);
			AssertMultilineASCIIEquals("", string.Empty, workflow2.GetSuccessfulReleaseNotes());
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_InactiveResourceExists_WorkingResourceIsOverloaded()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BAP", "Baps", capability);
			resource2.GS_IsActive = false;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 3000, capability: capability);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer, shouldBeReleasedToBuffer: false, expectedSuccessfullyReleasedNotes: string.Empty);

			// Close the task overloading the working resource, and workflow should be releasable
			var oldValue = task1.P9_Status;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer, expectedSuccessfullyReleasedNotes:
@"Released [workflow2] into [buffer] at 29-Dec-14 12:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bendy: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability []: T00001001 (1.5 hours)
");
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForTaskRequiringCapability_NonWorkingResourceExists_WorkingResourceIsOverloaded()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BAP", "Baps", capability);
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.UtcNow.Date, ZDateTime.UtcNow.AddDays(1).Date, resource2.PK, availabilityFactor: 0);

			AssertEquals(true, resource1.IsWorkingRightNow);
			AssertEquals(false, resource2.IsWorkingRightNow);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 3000, capability: capability);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, string.Empty, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer, shouldBeReleasedToBuffer: false, expectedSuccessfullyReleasedNotes: "");

			// Close the task overloading the working resource, and workflow should be releasable
			var oldValue = task1.P9_Status;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer, expectedSuccessfullyReleasedNotes:
@"Released [workflow2] into [buffer] at 29-Dec-14 12:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bendy: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability []: T00001001 (1.5 hours)
");
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForMultipleTasksRequiringCapabilities_OneCapabilityOverloaded()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Description = "Spooning";
			capability2.G4_Description = "Forking";

			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Bendy", capability1);
			var resource2 = CreateStaffInCurrentBranchDept("BAP", "Baps", capability2);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1 = CreateTask(workflow1, resource2.GS_Code, 3000);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2_1 = CreateTask(workflow2, string.Empty, 60, capability: capability1);
			var task2_2 = CreateTask(workflow2, string.Empty, 60, capability: capability2);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer, shouldBeReleasedToBuffer: false);

			// Close the task overloading the resource, and workflow should be releasable
			var oldValue = task1.P9_Status;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow2, config.Buffer, shouldBeReleasedToBuffer: true, expectedSuccessfullyReleasedNotes:
@"Released [workflow2] into [buffer] at 29-Dec-14 12:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Baps: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Forking]: T00001002 (1.5 hours)
	Bendy: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Spooning]: T00001001 (1.5 hours)");
		}

		[TestDate(2014, 12, 29, 12, 0, 0)]
		public void TestCanTransfer_ForMultipleTasksRequiringSameCapability_JustOneResourceAvailable()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Spooning";

			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Bendy", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BAP", "Baps");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var task1 = CreateTask(workflow, string.Empty, 60, capability: capability);
			var task2 = CreateTask(workflow, string.Empty, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer, expectedSuccessfullyReleasedNotes:
@"Released [workflow] into [buffer] at 29-Dec-14 12:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bendy: required 3 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Spooning]: T00001000 (1.5 hours), T00001001 (1.5 hours)");
		}

		#endregion

		[TestDate(2014, 3, 26)]
		public void TestCanTransfer_WhenTasksHaveNoEstimate()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);

			LinkComponents(bucket, buffer);

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var task1 = CreateTask(workflow1, resource.GS_Code, buffer.FC_BufferTimespanInMinutes);

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			var task2 = CreateTask(workflow2, resource.GS_Code, 0);

			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow3.FH_FC_CurrentComponent = bucket.PK;
			var task3 = CreateTask(workflow3, resource.GS_Code, 1);

			Factory.Save();

			AssertCanReleaseToBuffer("Should be able to transfer workflow with zero capacity required", workflow2, buffer);
			AssertCanReleaseToBuffer("Should NOT be able to transfer workflow with > zero capacity required", workflow3, buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2015, 4, 10)]
		public void TestCanTransfer_TaskReservationHoursBasedOnZoneCapacityMultiplier()
		{
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "I.S", "Pop Sound", capability1);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);

			var zoneMultiplier = BMSTestHelper.CreateZoneMultiplier(config.Buffer, zone3Multiplier: 8);

			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "BLEH", currentComponent: config.Bucket, releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, "", 600, capability: capability1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow1, config.Buffer, shouldBeReleasedToBuffer: false);

			AssertResourceCapacityReservationDetails(staff1.GS_Code, config.Buffer, @"Release Gate Run at 10-Apr-15 00:00 UTC
BLOCKED, XVBQP68SIYXQ - BLEH, Available Capacity: 48 hours, Reservation: 120 hours, Remaining Capacity: -72 hours");

			zoneMultiplier.BZC_Zone3Multiplier = 1.5;

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow1, config.Buffer, shouldBeReleasedToBuffer: true, expectedSuccessfullyReleasedNotes:
@"Released [BLEH] into [buffer] at 10-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Pop Sound: required 22.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability []: T00001000 (22.5 hours)");

			AssertResourceCapacityReservationDetails(staff1.GS_Code, config.Buffer, @"Release Gate Run at 10-Apr-15 00:00 UTC
RELEASED, XVBQP68SIYXQ - BLEH, Available Capacity: 48 hours, Reservation: 22.5 hours, Remaining Capacity: 25.5 hours");
		}

		[TestDate(2015, 4, 10)]
		public void TestCanTransfer_TaskReservationHoursBasedOnZoneCapacityMultiplier_WithReleaseGroupOverride()
		{
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "I.S", "Pop Sound", capability1);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);

			BMSTestHelper.CreateZoneMultiplier(config.Buffer, zone3Multiplier: 8m);

			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "BLEH", currentComponent: config.Bucket, releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, "", 600, capability: capability1);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow1, config.Buffer, shouldBeReleasedToBuffer: false);

			AssertResourceCapacityReservationDetails(staff1.GS_Code, config.Buffer, @"Release Gate Run at 10-Apr-15 00:00 UTC
BLOCKED, XVBQP68SIYXQ - BLEH, Available Capacity: 48 hours, Reservation: 120 hours, Remaining Capacity: -72 hours");

			BMSTestHelper.CreateZoneMultiplier(config.Buffer, zone3Multiplier: 1.5m, releaseGroupPK: group.PK);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow1, config.Buffer, shouldBeReleasedToBuffer: true, expectedSuccessfullyReleasedNotes:
@"Released [BLEH] into [buffer] at 10-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Pop Sound: required 22.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability []: T00001000 (22.5 hours)");

			AssertResourceCapacityReservationDetails(staff1.GS_Code, config.Buffer, @"Release Gate Run at 10-Apr-15 00:00 UTC
RELEASED, XVBQP68SIYXQ - BLEH, Available Capacity: 48 hours, Reservation: 22.5 hours, Remaining Capacity: 25.5 hours");
		}

		[TestDate(2013, 10, 29)]
		public void TestCapabilitiesWithSimilarStaffShouldStillTransfer()
		{
			UpdateDepartmentWeekDaysTo9To5ExceptMonday9To4(GlbDepartment.CurrentDepartment.PK);

			var buffer = BMSTestHelper.CreateBuffer(system);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "I.S", "Pop Sound", capability1, capability2);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, ";;;", "Wow Sound", capability1, capability2);
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "L.T", "Bam Sound", capability1);
			var staff4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SSS", "Sha Sound", capability1);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			group.Staff.Add(staff1);
			group.Staff.Add(staff2);
			group.Staff.Add(staff3);
			group.Staff.Add(staff4);

			Factory.Save();
			AssertEquals(4, WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability1.PK.ToGuid(), Factory, WorkingTimeContext.Create(Factory), group.PK).Count());
			AssertEquals(2, WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability2.PK.ToGuid(), Factory, WorkingTimeContext.Create(Factory), group.PK).Count());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var job = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = job.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "BLEH";
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, "", 60, capability: capability1, sequence: 0);
			var task2 = BMSTestHelper.CreateTask(workflow, "", 60, capability: capability1, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, "", 60, capability: capability2, sequence: 1);
			var task4 = BMSTestHelper.CreateTask(workflow, "", 60, capability: capability2, sequence: 3);

			Factory.Save();

			AssertCanReleaseToBuffer(workflow, buffer);
		}

		[TestDate(2013, 10, 29)]
		public void TestRule_WhenNoOpenTasksExistOnWorkflow_ShouldNotRelease()
		{
			var buffer = BMSTestHelper.CreateBuffer(system);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertCanReleaseToBuffer(workflow, buffer, BufferReleaseOutcome.BlockedByWorkflowEmptiness);
		}

		[TestDate(2013, 2, 7, 23, 30, 0)] // UTC time - ie 10:30am Friday 8th Feb 2013 Sydney time
		public void TestRunRule()
		{
			UpdateDepartmentWeekDaysTo9To5ExceptMonday9To4(GlbDepartment.CurrentDepartment.PK);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff1.GS_FullName = "Zayden";

			UpdateStaffWeekDaysTo9To5ExceptMonday9To4(staff1.PK);

			var component1 = BMSTestHelper.CreateBucket(dummySystem, "Bucket 1");
			var component2 = BMSTestHelper.CreateBucket(dummySystem, "Bucket 2");
			component2.FC_BufferTimespanInMinutes = 3600;
			component2.FC_Type = BMComponentTypeList.Codes.Buffer;
			var link1 = component1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = component2.PK;

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var workflow1 = ProcessJobHeader.GetForParent(dummy1, Factory).ProcessHeaders[0];
			var dummy1Task = dummy1.WorkflowItems.Tasks.AddNew();
			dummy1Task.P9_EstimatedTimeToComplete = new ZDateTime(2012, 1, 1, 1, 0, 0);     // 0.5hrs
			dummy1Task.P9_EstDuration = new ZDateTime(2012, 1, 1, 1, 0, 0);
			dummy1Task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var workflow2 = ProcessJobHeader.GetForParent(dummy2, Factory).ProcessHeaders[0];
			var dummy2Task = dummy2.WorkflowItems.Tasks.AddNew();
			dummy2Task.P9_EstDuration = new ZDateTime(2012, 1, 2, 16, 0, 0);                // 60 hrs
			dummy2Task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			var dummy3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var workflow3 = ProcessJobHeader.GetForParent(dummy3, Factory).ProcessHeaders[0];
			var dummy3Task = dummy3.WorkflowItems.Tasks.AddNew();
			dummy3Task.P9_EstDuration = new ZDateTime(2012, 1, 1, 2, 0, 0);                 // 3 hrs
			dummy3Task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			Factory.Save();

			var tracker = Create_ForTest(component2);

			AssertCanReleaseToBuffer("Can bring dummy3 into component as capacity is available", workflow3, component2, tracker: tracker);

			AssertCanReleaseToBuffer("Cannot bring dummy2 into component as then staff1 exceeds 0.5*60hrs of capacity (from the buffer load limit)", workflow2, component2, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Zayden: required 60 hours, currently has 27 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: T00001001 (60 hours)", tracker);

			AssertCanReleaseToBuffer("Cannot bring dummy1 into component as dummy2 has already reserved capacity", workflow1, component2, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Zayden: required 1 hours, currently has -33 hours of available capacity at 3rd place in the queue for this resource.
		Tasks assigned: T00001000 (1 hours)", tracker);
		}

		[TestDate(2013, 2, 6, 14, 31, 0)]
		public void TestRule_ThrottleZoneCapacity_DefaultZoneMultipliers()
		{
			var throttleTester = new ThrottleZoneCapacityTester(Factory, system);
			throttleTester.AssertTransferForZone(3, true, true);
			throttleTester.AssertTransferForZone(2, true, true);
			throttleTester.AssertTransferForZone(1, true, true);
			throttleTester.AssertTransferForZone(0, true, true);

			throttleTester.WorkflowInBuffer.FH_GG_ReleaseGroup = throttleTester.Group.PK;

			throttleTester.AssertTransferForZone(3, true, true);
			throttleTester.AssertTransferForZone(2, true, true);
			throttleTester.AssertTransferForZone(1, true, true);
			throttleTester.AssertTransferForZone(0, true, true);
		}

		#region Throttle Tester

		protected class ThrottleZoneCapacityTester
		{
			public ThrottleZoneCapacityTester(BusinessObjectFactory factory, BMSystem system)
			{
				Setup(factory, system);
			}

			void Setup(BusinessObjectFactory factory, BMSystem system)
			{
				WorkingDaysTestHelper.UpdateWeekDaysTo9To5(factory, GlbDepartment.CurrentDepartment.PK);

				var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "CEL", "Princess Celestia");
				var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "TWI", "Twilight Sparkle");

				Buffer = BMSTestHelper.CreateBuffer(system, loadLimitPercent: 20);
				Bucket = BMSTestHelper.CreateBucket(system);

				Group = factory.NewWithValidTestData<GlbGroup>();

				var job = factory.NewWithValidTestData<OrgHeader>();
				var jobHeader = ProcessJobHeader.GetForParent(job, factory);

				WorkflowInBuffer = jobHeader.ProcessHeaders[0];
				WorkflowInBuffer.FH_FC_CurrentComponent = Buffer.PK;

				var task1 = job.WorkflowItems.AddNew();
				task1.P9_FH_ProcessHeader = WorkflowInBuffer.PK;
				task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
				task1.P9_EstimatedTimeToComplete = new ZInt(60 * 9).GetDateTimeFromMinutes();

				BigEstimateWorkflow = jobHeader.ProcessHeaders.AddNew();
				BigEstimateWorkflow.FH_FC_CurrentComponent = Bucket.PK;
				BigEstimateWorkflow.FH_ReleaseDateTime = ZDateTime.Today;
				var task2 = job.WorkflowItems.AddNew();
				task2.P9_FH_ProcessHeader = BigEstimateWorkflow.PK;
				task2.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
				task2.P9_EstimatedTimeToComplete = new ZInt(60 * 10).GetDateTimeFromMinutes();

				SmallEstimateWorkflow = jobHeader.ProcessHeaders.AddNew();
				SmallEstimateWorkflow.FH_FC_CurrentComponent = Bucket.PK;
				SmallEstimateWorkflow.FH_ReleaseDateTime = ZDateTime.Today;
				var task3 = job.WorkflowItems.AddNew();
				task3.P9_FH_ProcessHeader = SmallEstimateWorkflow.PK;
				task3.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
				task3.P9_EstimatedTimeToComplete = new ZInt(60).GetDateTimeFromMinutes();
			}

			public ProcessHeader WorkflowInBuffer { get; set; }
			public ProcessHeader SmallEstimateWorkflow { get; set; }
			public ProcessHeader BigEstimateWorkflow { get; set; }
			public BMComponent Buffer { get; set; }
			public BMComponent Bucket { get; set; }
			public GlbGroup Group { get; set; }

			public void AssertTransferForZone(int zone, bool smallWorkflowTransfers, bool bigWorkflowTransfers)
			{
				WorkflowInBuffer.FH_ReleaseDateTime = ZDateTime.Today.AddDays((3 - zone) * -6);

				WorkflowInBuffer.Factory.Save();

				AssertEquals(zone, WorkflowInBuffer.BufferZone);
				AssertCapacity(zone, SmallEstimateWorkflow, Buffer, smallWorkflowTransfers);
				AssertCapacity(zone, BigEstimateWorkflow, Buffer, bigWorkflowTransfers);
			}

			static void AssertCapacity(int zone, ProcessHeader workflow, BMComponent destination, bool canTransfer)
			{
				var message = string.Format("{0} is in zone {1} with a x{2} multiplier, so {3} tranfer",
					workflow,
					zone,
					BMZoneCapacityMultiplier.GetZoneMultiplier(workflow.Factory, destination.PK, workflow.ReleaseGroup != null ? workflow.ReleaseGroup.PK : ZGuid.Empty, zone),
					canTransfer ? "can" : "can't");
				AssertCanReleaseToBuffer(message, workflow, destination, canTransfer ? BufferReleaseOutcome.Releasable : BufferReleaseOutcome.BlockedByResourceCapacity);
			}
		}

		#endregion

		[TestDate(2013, 2, 8, 4, 31, 0)]// Friday
		public void TestRule_TaskWithRequiredCapability()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DEA", "Big Dave");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			capability.G4_Description = "Coding";
			staff1.Capabilities.Add(capability);

			var bucket1 = CreateBucket(system, "bucket1");
			var buffer = CreateBuffer(system, timespanMinutes: 3600, loadLimitPercent: 20);
			var bucket2 = CreateBucket(system, "bucket2");

			LinkComponents(bucket1, buffer);
			LinkComponents(buffer, bucket2);

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			var task1_1 = CreateTask(workflow1, staff1.GS_Code, 60, "COD");
			task1_1.P9_EstimatedTimeToComplete = new ZDateTime(2012, 1, 1, 2, 0, 0);    // 2
			task1_1.P9_TaskID = "T00001000";
			task1_1.P9_Description = "task1_1";

			var task1_2 = CreateTask(workflow1, staff1.GS_Code, 0, "COD");
			task1_2.P9_TaskID = "T00001002";

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			var task2_1 = CreateTask(workflow2, ZString.Empty, 0, "COD", capability: capability);
			task2_1.P9_EstimatedTimeToComplete = new ZDateTime(2012, 1, 2, 10, 30, 0);  // 34.5
			task2_1.P9_TaskID = "T00001001";
			task2_1.P9_Description = "task2_1";

			Factory.Save();

			AssertCanReleaseToBuffer("Cannot bring dummy1 into component as then staff1 exceeds capacity", workflow1, buffer, BufferReleaseOutcome.BlockedByResourceCapacity, expectedLogMessage:
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Big Dave: required 2 hours, currently has -30 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001002 (0 hours)");

			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff2.GS_Code = staff2.GS_FullName = "ZA";

			var oldValue = task2_1.P9_GS_NKAssignedStaffMember;
			task2_1.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			Factory.Save();

			AssertCanReleaseToBuffer("Resource capability reduction no longer relevant - can move into buffer", workflow1, buffer);

			task1_1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task1_1.P9_G4_RequiredCapability = capability.PK;
			task1_1.P9_EstimatedTimeToComplete = new ZDateTime(2012, 1, 5, 12, 0, 0);       // 108 hours

			task1_2.Delete();

			Factory.Save();

			AssertCanReleaseToBuffer("Cannot bring dummy1 into component as then staff1 exceeds capacity", workflow1, buffer, BufferReleaseOutcome.BlockedByResourceCapacity, expectedLogMessage:
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Big Dave: required 108 hours, currently has 4.5 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Coding]: T00001000 (108 hours)");

			var hours = staff1.WorkTimes.FridayWorkingHours;
			staff1.WorkTimes.FridayWorkingHours = "";
			Factory.Save();
			AssertCanReleaseToBuffer("There isn't anyone available to work on the capability task, so the outcome is the same but the log is different.", workflow1, buffer, BufferReleaseOutcome.BlockedByResourceCapacity, expectedLogMessage:
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Capability [Coding]: required 108 hours, but there are currently no working resources who possess this capability.
		Tasks requiring this capability: T00001000 (108 hours)");

			staff1.WorkTimes.FridayWorkingHours = hours;

			task1_1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1_1.P9_EstimatedTimeToComplete = new ZDateTime(2012, 1, 1, 0, 0, 0);
			Factory.Save();

			AssertCanReleaseToBuffer("Resource capability reduction no longer relevant - can move into buffer", workflow1, buffer, expectedLogMessage:
@"Released [Job Workflow] into [buffer] at 08-Feb-13 04:31 UTC. It was considered releasable based on the capacity of the resources listed below.
	Big Dave: required 1.5 hours, currently has 4.5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (1.5 hours)");
		}

		[TestDate(2015, 7, 14)]
		public void TestAvailableCapacityForCapabilityTask_ShouldNotIncludeSecondAccuracy()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = BMSTestHelper.CreateCapability(Factory, "CMD", "Battlestar Command");
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "WAD", "William Adama", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "LAD", "Lee Adama", capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "So say we all.", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "So say we all!", config.Bucket);

			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 22);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 6000, capability: capability);

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "All of this has happened before", config.Buffer);
			var workflow4 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "And will happen again", config.Bucket);

			var task3 = BMSTestHelper.CreateTask(workflow3, resource2.GS_Code, lowEstMinutes: 60 * 30);
			var task4 = BMSTestHelper.CreateTask(workflow4, resource2.GS_Code, lowEstMinutes: 10);

			Factory.Save();

			var tracker = Create_ForTest(config.Buffer);

			AssertCanReleaseToBuffer(workflow4, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Lee Adama: required 0.25 hours, currently has -3 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001003 (0.25 hours)
".StripTaskIds(), tracker: tracker);

			AssertCanReleaseToBuffer(workflow2, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Lee Adama: required 75 hours, currently has -3.25 hours of available capacity at 2nd place in the queue for this resource.
		Tasks requiring capability [Battlestar Command]: T00001001 (75 hours)
	William Adama: required 75 hours, currently has 41.45 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Battlestar Command]: T00001001 (75 hours)".StripTaskIds(), tracker: tracker);
		}

		[TestDate(2013, 2, 8, 14, 31, 0)]// Friday
		public void TestRule_FailureCondition_NoResourcesAssignedToCapability()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BOG", "Bogan");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "PRI";
			capability.G4_Description = "Princess";
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "COM";
			capability2.G4_Description = "Sister";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "SLARGLE";
			Factory.Save();

			var component1 = BMSTestHelper.CreateBucket(system, "Bucket1");
			var component2 = BMSTestHelper.CreateBuffer(system, timespanMinutes: 3600);
			var component3 = BMSTestHelper.CreateBucket(system, "Bucket2");
			BMSTestHelper.LinkComponents(component1, component2);
			BMSTestHelper.LinkComponents(component2, component3);

			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var dummy1 = Factory.NewWithValidTestData<OrgHeader>();
			var workflow1 = ProcessJobHeader.GetForParent(dummy1, Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = component1.PK;
			workflow1.FH_GG_ReleaseGroup = releaseGroup.FSG_GG_Group;
			var dummy1Task = dummy1.WorkflowItems.Tasks.AddNew();
			dummy1Task.P9_Type = "COD";
			dummy1Task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			dummy1Task.P9_EstimateVariationFactor = 2m;
			dummy1Task.P9_TaskID = "T00001000";
			dummy1Task.P9_Description = "Turn down banana";

			dummy1Task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			dummy1Task.P9_G4_RequiredCapability = capability2.PK;

			Factory.Save();

			AssertCanReleaseToBuffer("Fail, because the capability assigned to this task has no resources.", workflow1, component2, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Capability [Sister]: required 1.5 hours, but there are currently no working resources who possess this capability.
		Tasks requiring this capability: T00001000 (1.5 hours)");

			var dummy1Task2 = dummy1.WorkflowItems.Tasks.AddNew();
			dummy1Task2.P9_Type = "COD";
			dummy1Task2.P9_Description = "Send sister to the moon";
			dummy1Task2.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			dummy1Task2.P9_EstimateVariationFactor = 2m;
			dummy1Task2.P9_TaskID = "T00001001";
			dummy1Task2.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			AssertCanReleaseToBuffer("Fail, because the capabilities assigned to this both tasks have no resources.", workflow1, component2, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Capability [Princess]: required 1.5 hours, but there are currently no working resources who possess this capability.
		Tasks requiring this capability: T00001001 (1.5 hours)
	Capability [Sister]: required 1.5 hours, but there are currently no working resources who possess this capability.
		Tasks requiring this capability: T00001000 (1.5 hours)");
		}

		[TestDate(2013, 2, 8, 14, 31, 0)]// Friday
		public void TestRule_FailureCondition_OneInvalidTask()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CEL", "Celestia");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "PRI";
			capability.G4_Description = "Princess";
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "COM";
			capability2.G4_Description = "Sister";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "SLARGLE";
			Factory.Save();

			var component1 = BMSTestHelper.CreateBucket(system, "Bucket1");
			var component2 = BMSTestHelper.CreateBuffer(system, timespanMinutes: 3600);
			var component3 = BMSTestHelper.CreateBucket(system, "Bucket2");
			BMSTestHelper.LinkComponents(component1, component2);
			BMSTestHelper.LinkComponents(component2, component3);

			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var dummy1 = Factory.NewWithValidTestData<OrgHeader>();
			var workflow1 = ProcessJobHeader.GetForParent(dummy1, Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = component1.PK;
			workflow1.FH_GG_ReleaseGroup = releaseGroup.FSG_GG_Group;
			var dummy1Task = dummy1.WorkflowItems.Tasks.AddNew();
			dummy1Task.P9_Type = "COD";
			dummy1Task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			dummy1Task.P9_EstimateVariationFactor = 2m;
			dummy1Task.P9_TaskID = "T00001000";
			dummy1Task.P9_Description = "Turn down banana";
			dummy1Task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			dummy1Task.P9_G4_RequiredCapability = capability2.PK;

			var dummy1Task2 = dummy1.WorkflowItems.Tasks.AddNew();
			dummy1Task2.P9_Type = "COD";
			dummy1Task2.P9_Description = "Send sister to the moon";
			dummy1Task2.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			dummy1Task2.P9_EstimateVariationFactor = 2m;
			dummy1Task2.P9_TaskID = "T00001001";
			dummy1Task2.P9_G4_RequiredCapability = capability.PK;
			dummy1Task2.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			Factory.Save();

			AssertCanReleaseToBuffer("Fail, because one of the tasks of this workflow has an empty capability", workflow1, component2, BufferReleaseOutcome.BlockedByResourceCapacity,
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Celestia: required 1.5 hours, currently has 11.25 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)
	Capability [Sister]: required 1.5 hours, but there are currently no working resources who possess this capability.
		Tasks requiring this capability: T00001000 (1.5 hours)");
		}

		[TestDate(2013, 7, 10)]
		public void TestRule_ShouldAllowTransfersContainingOnlyIgnoredTasks()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer", 1000);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			const string ignoredTaskType = "COD";

			var task1 = CreateTask(job, workflow, 1.0m, staff.GS_Code, ignoredTaskType);
			var task2 = CreateTask(job, workflow, 10000.0m, staff.GS_Code, ignoredTaskType);
			var task3 = CreateTask(job, workflow, 7.0m, staff.GS_Code, ignoredTaskType);

			IgnoreTaskTypesForProviderType("ORG", task1.P9_Type);

			Factory.Save();

			AssertCanReleaseToBuffer("Should be able to transfer because all tasks in workflow are set to ignored.", workflow, buffer);
		}

		[TestDate(2013, 3, 22)]
		public void TestRule_ShouldNotReserveCapacityForWorkflowsIneligibleForRelease()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer", 1000);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.Now.AddHours(1);
			var workflow = jobHeader.ProcessHeaders[0];

			var task1 = CreateTask(job, workflow, 1.0m, staff.GS_Code, "COD");
			var task2 = CreateTask(job, workflow, 10000.0m, staff.GS_Code, "CHK");

			Factory.Save();

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertCanReleaseToBuffer("Should be able to transfer since the task with a big estimate is closed.", workflow, buffer);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertCanReleaseToBuffer("Should not be able to transfer since the task with a big estimate is now open.", workflow, buffer, BufferReleaseOutcome.BlockedByResourceCapacity);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertCanReleaseToBuffer("Should be able to transfer since the task with a big estimate is now closed again and we haven't made a reservation.", workflow, buffer);
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacityReductionByCapability()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff.Capabilities.Add(capability);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Blame Canada!");
			BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: capability); // 90 mins std estimate

			Factory.Save();

			var totalCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer).FullCapacity;
			RunReleaseGateAndAssertReleaseOutcome("Should be able to release workflow1 since resources have full capacity currently.", workflow1, config.Buffer);

			var expectedCapacity = totalCapacity - 1.5m;
			var actualCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer).AvailableCapacity;
			AssertEquals("Should reduce capacity by the STD estimate of the released task", expectedCapacity, actualCapacity);
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacityReductionByCapability0MinuteTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff1.Capabilities.Add(capability);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Spare a square");
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 0, capability: capability); // 0 min std estimate

			Factory.Save();

			AssertCanReleaseToBuffer("0 Estimate Tasks should still be allowed to release", workflow1, config.Buffer);
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacityReductionByCapability2Resources()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff1.Capabilities.Add(capability);
			staff2.Capabilities.Add(capability);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Teenage angst", config.Bucket);
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: capability); // 90 mins std estimate

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pure morning", config.Buffer);
			var task2 = BMSTestHelper.CreateTask(workflow2, staff3.GS_Code, lowEstMinutes: 96 * 60, capability: capability); // A task that fills the buffer

			Factory.Save();

			var totalCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff1, config.Buffer).FullCapacity;
			RunReleaseGateAndAssertReleaseOutcome("Should be able to transfer workflow1 since all resources have full capacity currently.", workflow1, config.Buffer);

			var expectedCapacity = totalCapacity - (decimal)(0.25 * 3);
			var actualCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff1, config.Buffer).AvailableCapacity;
			AssertEquals("Should reduce capacity by 30% of the STD estimate only", expectedCapacity, actualCapacity);
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacityReductionByResource()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff.Capabilities.Add(capability);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Teenage angst", config.Bucket);
			BMSTestHelper.CreateTask(workflow1, staff.GS_Code, lowEstMinutes: 60, capability: capability); // 90 mins std estimate

			Factory.Save();

			var totalCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer).FullCapacity;
			RunReleaseGateAndAssertReleaseOutcome("Should be able to transfer workflow1 since resources have full capacity currently.", workflow1, config.Buffer);

			var expectedCapacity = totalCapacity - 1.5m;
			var actualCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer).AvailableCapacity;
			AssertEquals("Should reduce capacity by the full STD estimate", expectedCapacity, actualCapacity);
		}

		[TestDate(2020, 9, 12)]
		public void TestCapacityReductionByCapability_ShouldReduceByReleaseGroup()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2m);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);

			var tenHours = WorkingDaysTestHelper.GetWorkingHoursString(0, 10);
			WorkingDaysTestHelper.DeleteAllStaffWorkTimes(Factory, resource1.PK);
			WorkingDaysTestHelper.DeleteAllStaffWorkTimes(Factory, resource2.PK);

			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource1.PK, TestDateAttribute.Date.DayOfWeek, tenHours);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource2.PK, TestDateAttribute.Date.DayOfWeek, tenHours);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.ResourcesWithCapability.AddRange(resource1, resource2);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Teenage angst", config.Bucket, releaseGroupPK: group.PK);
			var small2HoursTask = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 2 * 60, estVariationFactor: 1, capability: capability);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "City on a rainy day", config.Buffer);

			Factory.Save();

			var bufferPK = config.Buffer.PK;
			var bucketPK = config.Bucket.PK;

			decimal resouce1AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity;
			decimal resouce2AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity;

			AssertEquals("Pre-condition: resource 1 capacity should be 10 hours no task was assigned yet", 10m, resouce1AvailableCapacity);
			AssertEquals("Pre-condition: resource 2 capacity should be 10 hours no task was assigned yet", 10m, resouce2AvailableCapacity);

			// Really big estimate, consuming all of this resource's available capacity. But it won't block release of the other workflow because we're filtering by release group.
			var big110HoursTask = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, lowEstMinutes: 110 * 60, estVariationFactor: 1, capability: capability);

			Factory.Save();
			BufferCapacityCacheTest.PurgeCachedCapacity();

			resouce1AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity;
			resouce2AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity;

			AssertEquals("Pre-condition: resource 1 capacity should still be 10 hours the big task was assigned to resource 2", 10m, resouce1AvailableCapacity);
			AssertEquals("Pre-condition: resource 2 capacity should be -100 hours (10h fullCapacity - 110h of big110HoursTask)", -100m, resouce2AvailableCapacity);

			RunReleaseGateAndAssertReleaseOutcome("Should NOT be able to transfer workflow1 since resource2 is overloaded which consumes all available capacity for the capability.", workflow1, config.Buffer, shouldBeReleasedToBuffer: false);

			//move the work to the buffer to assert if the capacity are split
			workflow1.FH_FC_CurrentComponent = bufferPK;

			Factory.Save();
			BufferCapacityCacheTest.PurgeCachedCapacity();

			resouce1AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity;
			resouce2AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity;

			AssertEquals("Pre-condition: resource 1 capacity should still be 9 hours (10h full capacity - small2HoursTask /2 resources)", 9m, resouce1AvailableCapacity);
			AssertEquals("Pre-condition: resource 2 capacity should be -101 hours (10h full capacity - 110h of big110HoursTask) - small2HoursTask /2 resources ", -101m, resouce2AvailableCapacity);

			// Remove resource2 from the release group, thus proving that the release gate only considers resources in the workflow's release group.
			var groupLink = BMSTestHelper.GetOrCreateGroupLink(Factory, group.PK, resource2.PK);
			group.Staff.Remove(resource2);

			//move the work back to the bucket to test if we can release to the buffer after remove the staff2 from the release group (staff2 is the bad guy that consume all capacity)
			workflow1.FH_FC_CurrentComponent = config.Bucket.PK;

			Factory.Save();
			BufferCapacityCacheTest.PurgeCachedCapacity();

			resouce1AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity;
			resouce2AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity;

			AssertEquals("Resource 1 capacity should still be 10 hours no more tasks in the buffer", 10m, resouce1AvailableCapacity);
			AssertEquals("Resource 2 capacity should be -100 hours (10h full capacity - 100h of big110HoursTask) no more tasks in the buffer", -100m, resouce2AvailableCapacity);

			RunReleaseGateAndAssertReleaseOutcome("Should be able to transfer workflow1 since resource1 has available capacity, " +
				"and although resource2 is overloaded and also has the same capability, the capability filters by release group when finding resources with capacity.", workflow1, config.Buffer);

			resouce1AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity;
			resouce2AvailableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity;

			AssertEquals("Resource 1 capacity should be 8 hour (10h full capacity - small2HoursTask)." +
				"The release gate is taking into account the exact resources considered for capacity." +
				"This consider Capability Scope.", 8m, resouce1AvailableCapacity);
			AssertEquals("Resource 2 capacity should be -100 hours (10h full capacity - 110h of big110HoursTask)" +
				"Happily, now the CapacityCalculator consider GlbCapability.G4_CapacityScope when calculating utilised capacity for capability tasks in workflows assigned to Release Groups of which resources that are not a member." +
				"so, resource1 have the entire standard estimate reserved, and resource2 capability were not affected by the workflow being released.", -100m, resouce2AvailableCapacity);
		}

		[TestDate(2013, 7, 12)]
		public void TestResourceLeaveWindowforReleasingWork_AtMinValue_NoWorkShouldBeReleased()
		{
			// test that a resource on leave today and back tomorrow will be assigned no work when ResourceLeaveWindowforReleasingWork is zero 
			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = CreateStaffInCurrentBranchDept("BOB", "Bobby", capability);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource.PK);

			AssertEquals("Resource should have 48 hrs capacity", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).FullCapacity); //48 based on 12 day buffer * 50% buffer load limit
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(1), resource.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Leave should reduce capacity to 44.5 hrs", 44.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).FullCapacity);
			AssertEquals("Resource is not working due to leave", false, resource.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource.IsOnLeave);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource.GS_Code, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome("resource1 is outside the configured leave window, so should block release.", workflow1, config.Buffer, shouldBeReleasedToBuffer: false, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Should be blank as no work is released", string.Empty, workflow1.GetSuccessfulReleaseNotes());
			AssertMultilineASCIIEquals("Should explain that resource is on leave",
@"One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.
	Bobby: required 1.5 hours, currently has 44.5 hours of available capacity at 1st place in the queue for this resource. On leave until 12 Jul 2013 14:00 UTC. The earliest work can be released is 12 Jul 2013 14:00 UTC. This is determined using the Resource Leave Window for Releasing Work registry item.
		Tasks assigned: T00001000 (1.5 hours)".StripTaskIds(), workflow1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();
			BMSTestHelper.ClearCachedCapacity(config.Buffer.PK);

			AssertEquals("Resource should have 48 hrs capacity again", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).FullCapacity);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome("Leave record has been deleted, so should not block release.", workflow1, config.Buffer, shouldBeReleasedToBuffer: true, failureLogService: ReleaseLogFailureServiceForTest);
			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow1] into [buffer] at 12-Jul-13 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bobby: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (1.5 hours)
".StripTaskIds(), workflow1.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2020, 9, 8, 22, 1, 0)]
		public void TestReleasingWork_DifferentTimezones_Window0_ShouldNotBeOnLeave_Release()
		{
			// The logic is:
			// - leave ends 9/Sep/2020 00:00 NED = 8/Sep/2020 22:00 UTC, so not on holiday on 8/Sep/2020 22:01 UTC
			// - leave until 9/Sep/2020 00:00 NED = 9/Sep/2020 06:00 CHI, so earliest release boundary is 9/Sep/2020 06:00 CHI = 8/Sep/2020 22:00 UTC, so release

			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bufferBranch = Factory.NewWithValidTestData<GlbBranch>();
			bufferBranch.GB_Code = "BR1";
			bufferBranch.GB_RL_NKHomePort = "CNSHA"; // China/Shanghai - difference with UTC +8:00

			var resourceBranch = Factory.NewWithValidTestData<GlbBranch>();
			resourceBranch.GB_Code = "BR2";
			resourceBranch.GB_RL_NKHomePort = "NLRTM"; // Netherlands/Rotterdam - difference with UTC +2:00

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_GB_AgingBranch = config.Bucket.FC_GB_AgingBranch = bufferBranch.PK;

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.Capabilities.Add(capability);
			resource1.GS_GB_HomeBranch = resourceBranch.PK;
			resource1.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			resource1.GS_FullName = "Bobby";
			resource1.GS_Code = "BOB";
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK);

			BMSTestHelper.CreateStaffHoliday(Factory, new ZDateTime(2020, 9, 8), new ZDateTime(2020, 9, 9), resource1.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Resource is working due to leave finished", true, resource1.IsWorkingRightNow);
			AssertEquals("Resource is working due to leave finished", false, resource1.IsOnLeave);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome("Should not block release, as we are 1 minute past resource's leave", workflow1, config.Buffer, shouldBeReleasedToBuffer: true, failureLogService: ReleaseLogFailureServiceForTest);
			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow1] into [buffer] at 08-Sep-20 22:01 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bobby: required 1.5 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (1.5 hours)
".StripTaskIds(), workflow1.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2020, 9, 8, 22, 0, 0)]
		public void TestReleasingWork_DifferentTimezones_Window0_ShouldBeOnLeave_DoNotRelease()
		{
			// The logic is:
			// - leave ends 9/Sep/2020 00:00 NED = 8/Sep/2020 22:00 UTC, so still on holiday on 8/Sep/2020 22:00 UTC
			// - leave until 9/Sep/2020 00:00 NED = 9/Sep/2020 06:00 CHI, so earliest release boundary is 9/Sep/2020 06:00 CHI = 8/Sep/2020 22:00 UTC, so do not release

			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bufferBranch = Factory.NewWithValidTestData<GlbBranch>();
			bufferBranch.GB_Code = "BR1";
			bufferBranch.GB_RL_NKHomePort = "CNSHA"; // China/Shanghai - difference with UTC +8:00

			var resourceBranch = Factory.NewWithValidTestData<GlbBranch>();
			resourceBranch.GB_Code = "BR2";
			resourceBranch.GB_RL_NKHomePort = "NLRTM"; // Netherlands/Rotterdam - difference with UTC +2:00

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_GB_AgingBranch = config.Bucket.FC_GB_AgingBranch = bufferBranch.PK;

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.Capabilities.Add(capability);
			resource1.GS_GB_HomeBranch = resourceBranch.PK;
			resource1.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			resource1.GS_FullName = "Bobby";
			resource1.GS_Code = "BOB";
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK);

			BMSTestHelper.CreateStaffHoliday(Factory, new ZDateTime(2020, 9, 8), new ZDateTime(2020, 9, 9), resource1.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Resource is not working due to leave", false, resource1.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource1.IsOnLeave);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome("resource1 is outside the configured leave window, so should block release.", workflow1, config.Buffer, shouldBeReleasedToBuffer: false, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Should be blank as no work is released", string.Empty, workflow1.GetSuccessfulReleaseNotes());
			AssertMultilineASCIIEquals("Should explain that resource is on leave",
@"One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.
	Bobby: required 1.5 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource. On leave until 08 Sep 2020 22:00 UTC. The earliest work can be released is 08 Sep 2020 22:00 UTC. This is determined using the Resource Leave Window for Releasing Work registry item.
		Tasks assigned: T00001000 (1.5 hours)".StripTaskIds(), workflow1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2020, 9, 7, 7, 25, 0)]
		public void TestReleasingWork_DifferentTimezones_Window10_ShouldBeOnLeave_Release()
		{
			// The logic is:
			// - leave ends 9/Sep/2020 00:00 NED = 8/Sep/2020 22:00 UTC, so still on holiday on 7/Sep/2020 7:25 UTC
			// - leave until 9/Sep/2020 00:00 NED = 9/Sep/2020 06:00 CHI, so earliest release boundary is
			//   closest working time to 9/Sep/2020 06:00 CHI which is 8/Sep/2020 17:00 CHI,
			//   subtract 10% buffer (9.6 hrs) = 7/Sep/2020 15:24 CHI = 7/Sep/2020 7:24 UTC, so release

			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bufferBranch = Factory.NewWithValidTestData<GlbBranch>();
			bufferBranch.GB_Code = "BR1";
			bufferBranch.GB_RL_NKHomePort = "CNSHA"; // China/Shanghai - difference with UTC +8:00

			var resourceBranch = Factory.NewWithValidTestData<GlbBranch>();
			resourceBranch.GB_Code = "BR2";
			resourceBranch.GB_RL_NKHomePort = "NLRTM"; // Netherlands/Rotterdam - difference with UTC +2:00

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_GB_AgingBranch = config.Bucket.FC_GB_AgingBranch = bufferBranch.PK;

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.Capabilities.Add(capability);
			resource1.GS_GB_HomeBranch = resourceBranch.PK;
			resource1.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			resource1.GS_FullName = "Bobby";
			resource1.GS_Code = "BOB";
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK);

			BMSTestHelper.CreateStaffHoliday(Factory, new ZDateTime(2020, 9, 5), new ZDateTime(2020, 9, 9), resource1.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Resource is working due to leave finished", false, resource1.IsWorkingRightNow);
			AssertEquals("Resource is working due to leave finished", true, resource1.IsOnLeave);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome("Should not block release, as we are 1 minute past resource's leave", workflow1, config.Buffer, shouldBeReleasedToBuffer: true, failureLogService: ReleaseLogFailureServiceForTest);
			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow1] into [buffer] at 07-Sep-20 07:25 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bobby: required 1.5 hours, currently has 40.21 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (1.5 hours)
".StripTaskIds(), workflow1.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2020, 9, 7, 7, 24, 0)]
		public void TestReleasingWork_DifferentTimezones_Window10_ShouldBeOnLeave_DoNotRelease()
		{
			// The logic is:
			// - leave ends 9/Sep/2020 00:00 NED = 8/Sep/2020 22:00 UTC, so still on holiday on 7/Sep/2020 7:24 UTC
			// - leave until 9/Sep/2020 00:00 NED = 9/Sep/2020 06:00 CHI, so earliest release boundary is
			//   closest working time to 9/Sep/2020 06:00 CHI which is 8/Sep/2020 17:00 CHI,
			//   subtract 10% buffer (9.6 hrs) = 7/Sep/2020 15:24 CHI = 7/Sep/2020 7:24 UTC, so do not release

			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bufferBranch = Factory.NewWithValidTestData<GlbBranch>();
			bufferBranch.GB_Code = "BR1";
			bufferBranch.GB_RL_NKHomePort = "CNSHA"; // China/Shanghai - difference with UTC +8:00

			var resourceBranch = Factory.NewWithValidTestData<GlbBranch>();
			resourceBranch.GB_Code = "BR2";
			resourceBranch.GB_RL_NKHomePort = "NLRTM"; // Netherlands/Rotterdam - difference with UTC +2:00

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_GB_AgingBranch = config.Bucket.FC_GB_AgingBranch = bufferBranch.PK;

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.Capabilities.Add(capability);
			resource1.GS_GB_HomeBranch = resourceBranch.PK;
			resource1.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			resource1.GS_FullName = "Bobby";
			resource1.GS_Code = "BOB";
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK);

			BMSTestHelper.CreateStaffHoliday(Factory, new ZDateTime(2020, 9, 5), new ZDateTime(2020, 9, 9), resource1.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Resource is not working due to leave", false, resource1.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource1.IsOnLeave);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome("resource1 is outside the configured leave window, so should block release.", workflow1, config.Buffer, shouldBeReleasedToBuffer: false, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Should be blank as no work is released", string.Empty, workflow1.GetSuccessfulReleaseNotes());
			AssertMultilineASCIIEquals("Should explain that resource is on leave",
@"One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.
	Bobby: required 1.5 hours, currently has 40.2 hours of available capacity at 1st place in the queue for this resource. On leave until 08 Sep 2020 22:00 UTC. The earliest work can be released is 07 Sep 2020 07:24 UTC. This is determined using the Resource Leave Window for Releasing Work registry item.
		Tasks assigned: T00001000 (1.5 hours)".StripTaskIds(), workflow1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2020, 2, 20)]
		public void TestResourceOnLeave_WithoutStaffWorkingHours_ShouldNotBeReleased_AndShouldDisplayNextAvailableDateBasedOnDepartmentWorkingHours()
		{
			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = CreateStaffInCurrentBranchDept("BOB", "Bobby", capability);
			var staffPK = resource.PK;

			Factory.Save();

			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffPK, WorkingDaysTestHelper.WeekEndHours);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			AssertEquals("Resource should have 48 hrs capacity", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).FullCapacity);

			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(1), resource.PK, availabilityFactor: 0);

			Factory.Save();

			AssertEquals("Resource is not working due to leave", false, resource.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource.IsOnLeave);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			CreateTask(workflow, resource.GS_Code, 60, capability: capability);

			Factory.Save();

			AssertEquals("Leave should reduce capacity to 44.5 hrs", 44.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).FullCapacity);

			ReleaseGateKeeperTest.RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);

			workflow.Reload();

			AssertMultilineASCIIEquals("Should be blank as no work is released", string.Empty, workflow.GetSuccessfulReleaseNotes());
			AssertMultilineASCIIEquals("Should explain that resource is on leave",
@"One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.
	Bobby: required 1.5 hours, currently has 44.5 hours of available capacity at 1st place in the queue for this resource. On leave until 20 Feb 2020 14:00 UTC. The earliest work can be released is 20 Feb 2020 14:00 UTC. This is determined using the Resource Leave Window for Releasing Work registry item.
		Tasks assigned: T00001000 (1.5 hours)".StripTaskIds(), workflow.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2013, 7, 12)]
		public void TestResourceLeaveWindowforReleasingWork_AtMaxValue_WorkShouldBeReleasedAsUsual()
		{
			// test that a resource on leave today will be assigned work when ResourceLeaveWindowforReleasingWork is 100 and usual capacity rules 
			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = CreateStaffInCurrentBranchDept("BOB", "Bobby", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BUB", "Bubbly", capability);

			AssertEquals("Resource should have 48 hrs capacity", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity); //48 based on 12 day buffer * 50% buffer load limit
			AssertEquals("Resource should have 48 hrs capacity", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(1), resource1.PK, availabilityFactor: 0);
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(14), resource2.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Leave should reduce capacity to 48 hrs", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity);
			AssertEquals("Leave should reduce capacity to 10.25 hrs", 10.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
			AssertEquals("Resource is not working due to leave", false, resource1.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", false, resource2.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource1.IsOnLeave);
			AssertEquals("Resource is not working due to leave", true, resource2.IsOnLeave);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, resource2.GS_Code, 60, capability: capability);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals("resource1 is within the configured leave window, so should not block release.", config.Buffer, workflow1.CurrentComponent);
			AssertEquals("resource2 is within the configured leave window, so should not block release.", config.Buffer, workflow2.CurrentComponent);

			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow1] into [buffer] at 12-Jul-13 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bobby: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (1.5 hours)
".StripTaskIds(), workflow1.GetSuccessfulReleaseNotes().StripTaskIds());

			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow2] into [buffer] at 12-Jul-13 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bubbly: required 1.5 hours, currently has 10.25 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)
".StripTaskIds(), workflow2.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2013, 7, 12)]
		public void TestResourceLeaveWindowforReleasingWork_AtDefaultValue_WorkShouldBeReleasedAt40Percent()
		{
			//test 40% - test either side of limit - 4.8 days in 12 day buffer
			//leave of 5 days and no work assigned (7 days includes weekend)
			//leave of 4 days and work is released (6 days includes weekend)
			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = CreateStaffInCurrentBranchDept("BOB", "Bobby", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BIB", "Bibby", capability);

			AssertEquals("Resource should have 48 hrs capacity", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity);
			AssertEquals("Resource should have 48 hrs capacity", 48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(7), resource1.PK, availabilityFactor: 0);
			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(6), resource2.PK, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Leave should reduce capacity to 34 hrs", 34.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity);
			AssertEquals("Leave should reduce capacity to 38.75 hrs", 38.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
			AssertEquals("Resource is not working due to leave", false, resource1.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource1.IsOnLeave);
			AssertEquals("Resource is not working due to leave", false, resource2.IsWorkingRightNow);
			AssertEquals("Resource is not working due to leave", true, resource2.IsOnLeave);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, resource2.GS_Code, 60, capability: capability);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals("resource1 is outside the configured leave window, so should block release.", config.Bucket, workflow1.CurrentComponent);
			AssertEquals("resource2 is within the configured leave window, so should not block release.", config.Buffer, workflow2.CurrentComponent);

			AssertMultilineASCIIEquals("Should be blank as no work is released", string.Empty, workflow1.GetSuccessfulReleaseNotes());
			AssertMultilineASCIIEquals("Should explain that resource is on leave",
@"One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.
	Bobby: required 1.5 hours, currently has 34 hours of available capacity at 1st place in the queue for this resource. On leave until 18 Jul 2013 14:00 UTC. The earliest work can be released is 12 Jul 2013 00:36 UTC. This is determined using the Resource Leave Window for Releasing Work registry item.
		Tasks assigned: T00001000 (1.5 hours)".StripTaskIds(), workflow1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow2] into [buffer] at 12-Jul-13 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bibby: required 1.5 hours, currently has 38.75 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)
".StripTaskIds(), workflow2.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2017, 1, 7)]
		public void TestResourceLeaveWindowforReleasingWork_WorkShouldBeReleasedWhenResourceReturnsDuringAWorkingDay()
		{
			//test 50% of 3 day buffer - test either side of limit - before and after middle of day
			BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_BufferTimespanInMinutes = 8 * 3 * 60;
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = CreateStaffInCurrentBranchDept("BOB", "Bobby", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BIB", "Bibby", capability);
			var bufferWorkingHours = WorkingDays.GetInstance(resource1.Factory, Env.CurrentDepartment.PK, Env.CurrentBranch.PK);
			var halfBufferTime = new ZDateTime(bufferWorkingHours.GetDateTimeInWorkingHoursFutureOrPast(ZDateTime.Today.ToDateTime(), 12));

			CombineAssertions(() =>
			{
				AssertEquals("Resource should have 13.5 hrs capacity", 12m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity);
				AssertEquals("Resource should have 13.5 hrs capacity", 12m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
				BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, halfBufferTime.AddHours(1), resource1.PK, availabilityFactor: 0);
				BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, halfBufferTime.AddHours(-1), resource2.PK, availabilityFactor: 0);
				Factory.Save();

				AssertEquals("Leave should reduce capacity to 6.25 hrs", 6.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity);
				AssertEquals("Leave should reduce capacity to 7.25 hrs", 7.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
				AssertEquals("Resource is not working due to leave", false, resource1.IsWorkingRightNow);
				AssertEquals("Resource is not working due to leave", true, resource1.IsOnLeave);
				AssertEquals("Resource is not working due to leave", false, resource2.IsWorkingRightNow);
				AssertEquals("Resource is not working due to leave", true, resource2.IsOnLeave);
			});

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Bucket);
			var task1 = CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", config.Bucket);
			var task2 = CreateTask(workflow2, resource2.GS_Code, 60, capability: capability);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals("resource1 is outside the configured leave window, so should block release.", config.Bucket, workflow1.CurrentComponent);
			AssertEquals("resource2 is within the configured leave window, so should not block release.", config.Buffer, workflow2.CurrentComponent);

			AssertMultilineASCIIEquals("Should be blank as no work is released", string.Empty, workflow1.GetSuccessfulReleaseNotes());
			AssertMultilineASCIIEquals("Should explain that resource is on leave",
@"One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.
	Bobby: required 1.5 hours, currently has 6.25 hours of available capacity at 1st place in the queue for this resource. On leave until 10 Jan 2017 04:00 UTC. The earliest work can be released is 09 Jan 2017 00:00 UTC. This is determined using the Resource Leave Window for Releasing Work registry item.
		Tasks assigned: T00001000 (1.5 hours)".StripTaskIds(), workflow1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			AssertMultilineASCIIEquals("Should explain details of work released",
@"Released [workflow2] into [buffer] at 07-Jan-17 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Bibby: required 1.5 hours, currently has 7.25 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)
".StripTaskIds(), workflow2.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2013, 7, 12)]
		public void TestResourceLeaveWindowForReleasingWork_RangeOfValues_AllBuffers()
		{
			// [Buffer Size] => { [Resource Leave Window Value] => { [Leave days] => expected value for releasing work }}					
			var expectedReleaseValues = new Dictionary<int, Dictionary<int, Dictionary<int, bool>>>()
			{
				[3] = new Dictionary<int, Dictionary<int, bool>>()
				{
					[0] = new Dictionary<int, bool>() { [0] = true, [1] = false },
					[25] = new Dictionary<int, bool>() { [0] = true, [1] = false },
					[50] = new Dictionary<int, bool>() { [0] = true, [1] = true, [2] = false },
					[75] = new Dictionary<int, bool>() { [0] = true, [2] = true, [3] = false },
					[100] = new Dictionary<int, bool>() { [0] = true, [2] = true, [3] = false }
				},
				[12] = new Dictionary<int, Dictionary<int, bool>>()
				{
					[0] = new Dictionary<int, bool>() { [0] = true, [1] = false },
					[25] = new Dictionary<int, bool>() { [0] = true, [3] = true, [4] = false },
					[50] = new Dictionary<int, bool>() { [0] = true, [6] = true, [7] = false },
					[75] = new Dictionary<int, bool>() { [0] = true, [9] = true, [10] = false },
					[100] = new Dictionary<int, bool>() { [0] = true, [11] = true, [12] = false }
				},
			};

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			foreach (var bufferDays in expectedReleaseValues.Keys)
			{
				config.Buffer.FC_BufferTimespanInMinutes = 8 * bufferDays * 60;
				AssertEquals("Should be correct buffer span time", 8.0 * bufferDays, config.Buffer.BufferTimeSpanHours);

				Factory.Save();

				var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
				var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);

				foreach (int leaveWindow in expectedReleaseValues[bufferDays].Keys)
				{
					BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, leaveWindow);

					foreach (int staffLeave in expectedReleaseValues[bufferDays][leaveWindow].Keys)
					{
						var resource = CreateStaffInCurrentBranchDept(null, "Loopy ", capability);
						AssertEquals("Resource should have full capacity (8 hours a day, for the size of the buffer, with a 50% load limit)", (decimal)(8 * bufferDays * 0.5), CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).FullCapacity);

						var resourceWorkingHoursContext = WorkingTimeContext.Create(config.Buffer, resource);
						var bufferWorkingHours = WorkingDays.GetInstance(resource.Factory, resourceWorkingHoursContext.Department.PK, resourceWorkingHoursContext.Branch.PK);
						BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, bufferWorkingHours.GetAnotherStandardWorkingDay(ZDateTime.Today.ToDateTime(), staffLeave).Date, resource.PK, availabilityFactor: 0);

						if (staffLeave > 0)
						{
							AssertEquals("Resource is not working due to leave", true, resource.IsOnLeave);
						}

						var task1 = CreateTask(workflow1, resource.GS_Code, 60, capability: capability);

						Factory.Save();

						var report = GetBufferReservationReport(workflow1, config.Buffer);
						var message = FormattableString.Invariant($"Buffer Size is {bufferDays} days, Leave window is {leaveWindow} percent, staff has {staffLeave} days of leave booked, release outcome is {report.BufferReleaseOutcome}, report log:\r\n{report.CreateOutcomeLogMessage()}.");

						if (expectedReleaseValues[bufferDays][leaveWindow][staffLeave])
						{
							AssertEquals("Should release work as there is capacity and resource is returning before leave window: " + message, BufferReleaseOutcome.Releasable, report.BufferReleaseOutcome);
						}
						else
						{
							AssertNotEquals("Should prevent the release of work due to resource on leave in leave window: " + message, BufferReleaseOutcome.Releasable, report.BufferReleaseOutcome);
						}

						task1.Delete();
					}
				}
			}
		}

		[TestDate(2013, 7, 12)]
		public void TestResourceCapacity_ShouldCalculate_WhenSomeResourcesAreAdded()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE1", "staff1");
			var bucket1 = CreateBucket(system, "bucket1");
			var buffer = CreateBuffer(system, timespanMinutes: 3600, loadLimitPercent: 20);
			LinkComponents(bucket1, buffer);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			var task1 = CreateTask(workflow, staffCode: staff1.GS_Code, 60, "COD");

			Factory.Save();

			var tracker = Create_ForTest(buffer, trackResourceQueues: true, new[] { staff1 });

			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE2", "staff2");
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE3", "staff3");
			var task2 = CreateTask(workflow, staffCode: staff2.GS_Code, 60, "COD");
			var task3 = CreateTask(workflow, staffCode: staff3.GS_Code, 60, "COD");

			Factory.Save();

			AssertEquals("Precondition: Should have only one staff capacity in cache", 1, tracker.TrackedAvailableCapacityByResource.Count);
			Assert("Precondition: Should have staff1 capacity calculated", tracker.TrackedAvailableCapacityByResource.ContainsKey(staff1.GS_Code));

			var newFactory = workflow.Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			tracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(loadedWorkflow, newFactory);

			AssertEquals("Should have all staffs capacity in cache", 3, tracker.TrackedAvailableCapacityByResource.Count);
			AssertContainsExactElementsInAnyOrder("Should have all capacity calculated", new[] { staff1.GS_Code, staff2.GS_Code, staff3.GS_Code }, tracker.TrackedAvailableCapacityByResource.Keys.ToArray());
		}

		[TestDate(2019, 5, 7, 9, 0, 0)]
		public void TestResourceCapacity_AccountsForResources_InWorkflowReleaseGroupAndTaskGroup()
		{
			var buffer = CreateBuffer(system, timespanMinutes: 3600, loadLimitPercent: 20);

			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", groupCapability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", groupCapability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", groupCapability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FOR", "Four resource", groupCapability);

			releaseGroup1.Staff.AddRange(resource1, resource2);
			releaseGroup2.Staff.AddRange(resource3, resource4);

			var tracker = Create_ForTest(buffer, trackResourceQueues: true, new[] { resource1, resource2, resource3, resource4 });

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pizza", buffer, releaseGroupPK: releaseGroup2.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: releaseGroup1.PK);
			var task2 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: releaseGroup2.PK);
			var task3 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: releaseGroup2.PK);
			var taskWithoutGroup = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 90, capability: groupCapability, estVariationFactor: 1);

			Factory.Save();

			tracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(workflow1, Factory);

			AssertEquals("Should have 4 staffs capacity", 4, tracker.TrackedAvailableCapacityByResource.Count);
			CombineAssertions(() =>
			{
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource1.GS_Code].FullCapacity);
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource2.GS_Code].FullCapacity);
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource3.GS_Code].FullCapacity);
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource4.GS_Code].FullCapacity);

				AssertEquals(4.5m, 3.8m * 4 - tracker.TrackedAvailableCapacityByResource.Sum(t => t.Value.AvailableCapacity));

				AssertEquals(3.3m, tracker.TrackedAvailableCapacityByResource[resource1.GS_Code].AvailableCapacity); //Total Capacity: 3.8 -  0.5 Consumed from Task1 (1h / 2 resources) = 3.3hours
				AssertEquals(3.3m, tracker.TrackedAvailableCapacityByResource[resource2.GS_Code].AvailableCapacity); //Total Capacity: 3.8 -  0.5 Consumed from Task1 (1h / 2 resources) = 3.3hours
				AssertEquals(2.05m, tracker.TrackedAvailableCapacityByResource[resource3.GS_Code].AvailableCapacity);//Total Capacity: 3.8 - 1.75 Consumed from task2,task3,taskWithoutGroup (3.5h / 2 resources) = 2.05
				AssertEquals(2.05m, tracker.TrackedAvailableCapacityByResource[resource4.GS_Code].AvailableCapacity);//Total Capacity: 3.8 - 1.75 Consumed from task2,task3,taskWithoutGroup (3.5h / 2 resources) = 2.05
			});
		}

		[TestDate(2019, 5, 7, 9, 0, 0)]
		public void TestResourceGroupCapacity_AccountsForResources_WhenWorkflowAndTaskDoesntHaveGroup()
		{
			var buffer = CreateBuffer(system, timespanMinutes: 3600, loadLimitPercent: 20);

			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", groupCapability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", groupCapability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", groupCapability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FOR", "Four resource", groupCapability);

			releaseGroup.Staff.AddRange(resource1, resource2);

			var tracker = Create_ForTest(buffer, trackResourceQueues: true, new[] { resource1, resource2, resource3, resource4 });

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pizza", buffer, releaseGroupPK: null);
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: null);
			var task2 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: null);
			var task3 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: releaseGroup.PK);
			var task4 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, capability: groupCapability, estVariationFactor: 1, taskGroupPK: releaseGroup.PK);

			Factory.Save();

			tracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(workflow1, Factory);

			AssertEquals("Should have 4 staffs capacity", 4, tracker.TrackedAvailableCapacityByResource.Count);
			CombineAssertions(() =>
			{
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource1.GS_Code].FullCapacity);
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource2.GS_Code].FullCapacity);
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource3.GS_Code].FullCapacity);
				AssertEquals(3.8m, tracker.TrackedAvailableCapacityByResource[resource4.GS_Code].FullCapacity);

				AssertEquals(4m, 3.8m * 4 - tracker.TrackedAvailableCapacityByResource.Sum(t => t.Value.AvailableCapacity));

				AssertEquals(2.3m, tracker.TrackedAvailableCapacityByResource[resource1.GS_Code].AvailableCapacity); //Total Capacity: 3.8 - 1 Consumed from Task1,2 (2h / 2 resources) - 0.5 consumed from Task3,4 (2h / 4 resources) = 2.3hours
				AssertEquals(2.3m, tracker.TrackedAvailableCapacityByResource[resource2.GS_Code].AvailableCapacity); //Total Capacity: 3.8 - 1 Consumed from Task1,2 (2h / 2 resources) - 0.5 consumed from Task3,4 (2h / 4 resources) = 2.3hours
				AssertEquals(3.3m, tracker.TrackedAvailableCapacityByResource[resource3.GS_Code].AvailableCapacity); //Total Capacity: 3.8 - 0.5 Consumed from Task1,2 (2h / 4 resources) = 3.3hours
				AssertEquals(3.3m, tracker.TrackedAvailableCapacityByResource[resource4.GS_Code].AvailableCapacity); //Total Capacity: 3.8 - 0.5 Consumed from Task1,2 (2h / 2 resources) = 3.3hours
			});
		}

		#region Capacity Constraint Threshold Multiplier

		[TestDate(2019, 10, 24)]
		public void TestShouldReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToCCR_EvenIfNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			BMSTestHelper.CreateTask(config.WorkflowToRelease, staffCode: config.Ccr.GS_Code, description: "CCR task");

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.Releasable);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasNoTasksAssignedToCCR_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			BMSTestHelper.CreateTask(config.WorkflowToRelease, staffCode: config.AnotherNonCcr.GS_Code, description: "another non CCR task");

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();
			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToGlobalCapabilityContainingCCRsOnlyAmongActiveUsers_EvenIfNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var globalCapability = Factory.NewWithValidTestData<GlbCapability>();
			globalCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			new GlbStaffCapabilityMembersCollection(globalCapability)
			{
				config.Ccr,
				config.InactiveCcr
			};

			config.TaskGroup.Staff.Add(BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)); // non CCR - does not affect as global capabilities ignore task groups and release groups

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: globalCapability, taskGroupPK: config.TaskGroup.PK, description: "CCR capability task");

			config.WorkflowReleaseGroup.Staff.Add(BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)); // non CCR - does not affect as global capabilities ignore task groups and release groups

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.Releasable);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToGlobalCapabilityContainingBothActiveCCRsAndNonCCRs_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var globalCapability = Factory.NewWithValidTestData<GlbCapability>();
			globalCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			new GlbStaffCapabilityMembersCollection(globalCapability)
			{
				config.Ccr,
				BMSTestHelper.CreateStaffInCurrentBranchDept(Factory) // non CCR
			};

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: globalCapability, description: "CCR capability task");

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToGlobalCapabilityContainingInactiveStaffOnly_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var globalCapability = Factory.NewWithValidTestData<GlbCapability>();
			globalCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			new GlbStaffCapabilityMembersCollection(globalCapability)
			{
				config.InactiveCcr
			};

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: globalCapability, description: "CCR capability task");

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToGroupCapabilityAndTaskGroup_WhichIntersectionContainsCCRsOnlyAmongActiveUsers_EvenIfNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			new GlbStaffCapabilityMembersCollection(groupCapability)
			{
				config.Ccr,
				config.InactiveCcr,
				BMSTestHelper.CreateStaffInCurrentBranchDept(Factory) // non CCR
			};

			config.TaskGroup.Staff.Add(config.Ccr);
			config.TaskGroup.Staff.Add(config.InactiveCcr);
			config.TaskGroup.Staff.Add(BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)); // non CCR

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: groupCapability, taskGroupPK: config.TaskGroup.PK, description: "CCR capability task");

			config.WorkflowReleaseGroup.Staff.Add(BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)); // non CCR - does not affect as task group overrides release group

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.Releasable);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToGroupCapabilityAndTaskGroup_WhichIntersectionContainsBothActiveCCRsAndNonCCRs_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			new GlbStaffCapabilityMembersCollection(groupCapability)
			{
				config.Ccr,
				config.AnotherNonCcr,
			};

			config.TaskGroup.Staff.Add(config.Ccr);
			config.TaskGroup.Staff.Add(config.AnotherNonCcr);

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: groupCapability, taskGroupPK: config.TaskGroup.PK, description: "CCR capability task");

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowHasTaskAssignedToGroupCapabilityAndTaskGroup_WhichIntersectionContainsInactiveStaffOnly_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			new GlbStaffCapabilityMembersCollection(groupCapability)
			{
				config.InactiveCcr
			};

			config.TaskGroup.Staff.Add(config.InactiveCcr);

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: groupCapability, taskGroupPK: config.TaskGroup.PK, description: "CCR capability task");

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowAssignedToReleaseGroupAndHasTaskAssignedToGroupCapability_WhichIntersectionContainsCCRsOnlyAmongActiveUsers_EvenIfNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			new GlbStaffCapabilityMembersCollection(groupCapability)
			{
				config.Ccr,
				config.InactiveCcr,
				BMSTestHelper.CreateStaffInCurrentBranchDept(Factory) // non CCR
			};

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: groupCapability, description: "CCR capability task");

			config.WorkflowReleaseGroup.Staff.Add(config.Ccr);
			config.WorkflowReleaseGroup.Staff.Add(config.InactiveCcr);
			config.WorkflowReleaseGroup.Staff.Add(BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)); // non CCR

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.Releasable);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowAssignedToReleaseGroupAndHasTaskAssignedToGroupCapability_WhichIntersectionContainsBothActiveCCRsAndNonCCRs_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			new GlbStaffCapabilityMembersCollection(groupCapability)
			{
				config.Ccr,
				config.AnotherNonCcr,
			};

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: groupCapability, description: "CCR capability task");

			config.WorkflowReleaseGroup.Staff.Add(config.Ccr);
			config.WorkflowReleaseGroup.Staff.Add(config.AnotherNonCcr);

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		[TestDate(2019, 10, 24)]
		public void TestShouldNotReleaseBasedOnCapacityConstraintThresholdMultiplier_WhenWorkflowAssignedToReleaseGroupAndHasTaskAssignedToGroupCapability_WhichIntersectionContainsInactiveStaffOnly_AndNonCCRWithinWorkflowIsFullyUtilised()
		{
			var config = new ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(Factory, system);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			new GlbStaffCapabilityMembersCollection(groupCapability)
			{
				config.InactiveCcr
			};

			BMSTestHelper.CreateTask(config.WorkflowToRelease, capability: groupCapability, description: "CCR capability task");

			config.WorkflowReleaseGroup.Staff.Add(config.InactiveCcr);

			Factory.Save();

			config.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations();
			config.AssertNonCcrCapacityAfterReservation();

			AssertCanReleaseToBuffer(config.WorkflowToRelease, config.Buffer, BufferReleaseOutcome.BlockedByResourceCapacity);
		}

		class ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig
		{
			readonly BusinessObjectFactory factory;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const decimal fullCapacityHours = 48m;
			const decimal overloadMultiplier = 1.5m;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const decimal hoursNeededForRelease = 3m;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const decimal hoursToUtiliseEntireNonCcrCapacityWithOverloadMultiplier = fullCapacityHours * overloadMultiplier;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const decimal hoursToUtiliseNonCcrCapacityExceptHoursNeededForRelease = hoursToUtiliseEntireNonCcrCapacityWithOverloadMultiplier - hoursNeededForRelease;

			public readonly BMSystem System;
			public readonly BMComponent Buffer;
			public readonly GlbStaff NonCcrWithUtilisedCapacity;
			public readonly GlbStaff AnotherNonCcr;
			public readonly GlbStaff Ccr;
			public readonly GlbStaff InactiveCcr;
			public readonly GlbGroup TaskGroup;
			public readonly GlbGroup WorkflowReleaseGroup;
			readonly GlbGroup componentReleaseGroup;

			public ProcessHeader WorkflowToRelease;

			CapacityReservationTracker tracker;

			public ReleaseBasedOnCapacityConstraintThresholdMultiplierTestConfig(BusinessObjectFactory factory, BMSystem system)
			{
				this.factory = factory;

				WorkingDaysTestHelper.UpdateWeekDaysTo9To5(factory, Env.CurrentDepartment.PK);

				AssertEquals(72m, hoursToUtiliseEntireNonCcrCapacityWithOverloadMultiplier);
				AssertEquals(69m, hoursToUtiliseNonCcrCapacityExceptHoursNeededForRelease);

				System = system;
				Buffer = BMSTestHelper.CreateBuffer(System);
				var bucket = BMSTestHelper.CreateBucket(System);

				Buffer.FC_NonCCRTemporaryOverloadLimitMultiplier = overloadMultiplier;

				NonCcrWithUtilisedCapacity = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, code: "UTI", fullName: nameof(NonCcrWithUtilisedCapacity));

				AnotherNonCcr = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, code: "ANO", fullName: nameof(AnotherNonCcr));

				Ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, code: "CCR", fullName: nameof(Ccr));
				Ccr.DesignateAsCCR(Buffer);

				InactiveCcr = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, code: "INA", fullName: nameof(InactiveCcr));
				InactiveCcr.DesignateAsCCR(Buffer);
				InactiveCcr.GS_IsActive = false;

				TaskGroup = factory.NewWithValidTestData<GlbGroup>();

				WorkflowReleaseGroup = factory.NewWithValidTestData<GlbGroup>();
				CreateReleaseGroup(System, WorkflowReleaseGroup); // workflow release group does not need to be constrained - it just defines members to assign

				componentReleaseGroup = factory.NewWithValidTestData<GlbGroup>(); // release group that makes ccr ccr
				componentReleaseGroup.Staff.Add(Ccr);
				componentReleaseGroup.Staff.Add(InactiveCcr);
				CreateReleaseGroup(System, componentReleaseGroup, constrainedModeComponent: Buffer); // has to be constrained

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);

				var releasedWorkflow = CreateWorkflow(jobHeader, "Released Workflow", Buffer);
				BMSTestHelper.CreateTask(releasedWorkflow, NonCcrWithUtilisedCapacity.GS_Code, (int)(hoursToUtiliseNonCcrCapacityExceptHoursNeededForRelease * 60), description: "released task", estVariationFactor: 1);

				WorkflowToRelease = CreateWorkflow(jobHeader, "Workflow to Release", bucket);
				WorkflowToRelease.FH_GG_ReleaseGroup = WorkflowReleaseGroup.PK;
				BMSTestHelper.CreateTask(WorkflowToRelease, NonCcrWithUtilisedCapacity.GS_Code, (int)(hoursNeededForRelease * 60), description: "non CCR task", estVariationFactor: 1);
			}

			public void BuildCapacityReservationReportAndUpdateTrackedCapacityReservations()
			{
				tracker = Create_ForTest(Buffer, trackResourceQueues: true, new[] { NonCcrWithUtilisedCapacity });
				tracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(WorkflowToRelease, factory);
			}

			public void AssertNonCcrCapacityAfterReservation()
			{
				CombineAssertions("Capacity of the utilised non CCR after reservation for the workflow to release", () =>
				{
					AssertEquals("FullCapacity", fullCapacityHours, tracker.TrackedAvailableCapacityByResource[NonCcrWithUtilisedCapacity.GS_Code].FullCapacity);

					const decimal nonCcrAvailableCapacityHours = fullCapacityHours - hoursToUtiliseNonCcrCapacityExceptHoursNeededForRelease - hoursNeededForRelease;
					AssertEquals(-24m, nonCcrAvailableCapacityHours);
					AssertEquals("AvailableCapacity", nonCcrAvailableCapacityHours, tracker.TrackedAvailableCapacityByResource[NonCcrWithUtilisedCapacity.GS_Code].AvailableCapacity);

					AssertEquals("FullCapacityForWorkInvolvingCCR", hoursToUtiliseEntireNonCcrCapacityWithOverloadMultiplier, tracker.TrackedAvailableCapacityByResource[NonCcrWithUtilisedCapacity.GS_Code].FullCapacityForWorkInvolvingCCR);
					AssertEquals("AvailableCapacityForWorkInvolvingCCR", 0m, tracker.TrackedAvailableCapacityByResource[NonCcrWithUtilisedCapacity.GS_Code].AvailableCapacityForWorkInvolvingCCR);
				});
			}
		}

		#endregion

		#region DbHits

		[TestDate(2015, 2, 2)]
		public void TestDbHitsWhenResourcesNotOnLeave()
		{
			AssertDbHitsWhenResourcesOnLeave(false);
		}

		[TestDate(2015, 2, 2)]
		public void TestDbHitsWhenResourcesOnLeave()
		{
			AssertDbHitsWhenResourcesOnLeave(true);
		}

		void AssertDbHitsWhenResourcesOnLeave(bool testLeave)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "STU", "Stumptavian Roboclick", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "LMT", "Takittothu' Limit", capability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "QUZ", "Quisperny G'Dunzoid Sr.", capability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FDS", "Faux Doadles", capability);
			var resource5 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TOM", "Tom Cat", capability);
			var resource6 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JER", "Jerry Mouse", capability);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", config.Bucket);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4", config.Bucket);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, capability: capability);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, capability: capability);
			var task1_3 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, capability: capability);

			if (testLeave)
			{   //add leave that will still get work assigned to the resource, and another that will not
				BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(3), resource5.PK, availabilityFactor: 0);
				BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(14), resource6.PK, availabilityFactor: 0);
				AssertEquals("Resource is not working due to leave", false, resource5.IsWorkingRightNow);
				AssertEquals("Resource is not working due to leave", true, resource5.IsOnLeave);
				AssertEquals("Resource is not working due to leave", false, resource6.IsWorkingRightNow);
				AssertEquals("Resource is not working due to leave", true, resource6.IsOnLeave);
			}

			var task1_5 = BMSTestHelper.CreateTask(workflow1, resource5.GS_Code, 60); //assigning to resource on leave
			var task1_6 = BMSTestHelper.CreateTask(workflow1, resource6.GS_Code, 60); //assigning to resource on leave
			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource3.GS_Code, 60); // Ensures capacity will be pre-fetched for this resource
			var task2_2 = BMSTestHelper.CreateTask(workflow2, resource4.GS_Code, 60); // Ensures capacity will be pre-fetched for this resource
			var task3_1 = BMSTestHelper.CreateTask(workflow3, resource5.GS_Code, 60); //assigning to resource on leave
			var task4_1 = BMSTestHelper.CreateTask(workflow4, resource6.GS_Code, 60); //assigning to resource on leave

			Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();
			Factory.ResetDatabaseLoadCount();

			var newFactory = Factory.CreateNewFactory();
			newFactory.NameForDebugging = "ReleaseGate_Test";
			var loadedSystem = newFactory.Load<BMSystem>(config.System.PK);

			var hits = new Dictionary<string, int>
			{
				{ BMComponentLinkSchema.Constants.TableName, 2 },
				{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
				{ BMComponentSchema.Constants.TableName, 2 },
				{ BMSystemSchema.Constants.TableName, 0 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 0 },
				{ GlbDepartmentSchema.Constants.TableName, 0 },
				{ GlbCapabilitySchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 1 },
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 2 },
				{ GlbStaffHolidaySchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 3 },
				{ GlbWorkTimeSchema.Constants.TableName, 2 },
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 4 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 1 },
				{ TagDefinitionSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 1 },
				{ ViewProcessHeaderSchema.Constants.TableName, 1 },
				{ ViewProcessTaskSchema.Constants.TableName, 1 },
				{ StmDataSchema.Constants.TableName, UsingSimpleQuery ? 1 : 0 },
			};

			TagProvider.ResetCCPMReadyToReleaseTagMagnitudePK_ForTest();

			using (AssertDbHitsForAllFactories(hits, ignoreUnspecified: true, thresholdForUnspecified: 1, includeFactoryPredicate: BMSTestHelper.IsPAVEFactory))
			{
				ReleaseGateKeeperTest.RunReleaseGate(loadedSystem);
			}

			workflow1.Reload();
			workflow3.Reload();
			workflow4.Reload();

			if (testLeave)
			{
				AssertEquals("Workflow should be blocked when leave is added", config.Bucket, workflow1.CurrentComponent);
				AssertEquals("Workflow should always transfer as leave isn't enough to block", config.Buffer, workflow3.CurrentComponent);
				AssertEquals("Workflow should be blocked when leave is added", config.Bucket, workflow4.CurrentComponent);
			}
			else
			{
				AssertEquals("Workflow should transfer when no leave is added", config.Buffer, workflow1.CurrentComponent);
				AssertEquals("Workflow should always transfer as leave isn't enough to block", config.Buffer, workflow3.CurrentComponent);
				AssertEquals("Workflow should transfer when no leave is added", config.Buffer, workflow4.CurrentComponent);
			}
		}

		[TestDate(2015, 2, 2)]
		public void TestResourceCapacity_ShouldHitDatabaseJustOnceToGetComponentContents_WhenNewResourcesAreAdded()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE1", "staff1");
			var bucket1 = CreateBucket(system, "bucket1");
			var buffer = CreateBuffer(system, timespanMinutes: 3600, loadLimitPercent: 20);
			LinkComponents(bucket1, buffer);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			var task1 = CreateTask(workflow, staffCode: staff1.GS_Code, 60, "COD");

			Factory.Save();

			var tracker = Create_ForTest(buffer, trackResourceQueues: true, new[] { staff1 });

			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE2", "staff2");
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE3", "staff3");
			var task2 = CreateTask(workflow, staffCode: staff2.GS_Code, 60, "COD");
			var task3 = CreateTask(workflow, staffCode: staff3.GS_Code, 60, "COD");

			Factory.Save();

			AssertEquals("Precondition: Should have only one staff capacity in cache", 1, tracker.TrackedAvailableCapacityByResource.Count);
			Assert("Precondition: Should have staff1 capacity calculated", tracker.TrackedAvailableCapacityByResource.ContainsKey(staff1.GS_Code));

			var newFactory = workflow.Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			using (TestConnection.TrackExecutedCommands())
			{
				tracker.BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(loadedWorkflow, newFactory);
				var executedCommands = TestConnection.ExecutedCommands.Count(command => command.Contains("CAPACITY CALCULATION"));
				var expectedHits = UsingSimpleQuery ? 2 : 1;
				AssertEquals(expectedHits, executedCommands);
			}
		}

		#endregion

		[TestDate(2013, 12, 15, 23, 59, 0)] //Sunday
		public void TestGetWorkingResourcesWithCapability_ShouldUseBranchFromDestinationComponent_AndNotEnvironmentBranch()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = CreateCapability("FSM", "Flying Spaghetti Monster");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.Capabilities.Add(capability);
			resource.ResetBranchAndDepartment();

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "Add Meatballs", config.Bucket);
			var task1 = CreateTask(workflow, string.Empty, 10, capability: capability, estVariationFactor: 1);

			var destinationComponent = Factory.NewWithValidTestData<BMComponent>();
			destinationComponent.FC_Type = BMComponentTypeList.Codes.Buffer;
			destinationComponent.FC_BufferTimespanInMinutes = 1337;

			var branchUsedInEnvironmentContext = Factory.NewWithValidTestData<GlbBranch>();
			branchUsedInEnvironmentContext.GB_Code = "EVB";
			branchUsedInEnvironmentContext.GB_RL_NKHomePort = "USSFO"; //GMT -5 (Still Sunday)

			var branchUsedForDestinationComponent = Factory.NewWithValidTestData<GlbBranch>();
			branchUsedForDestinationComponent.GB_Code = "DCB";
			branchUsedForDestinationComponent.GB_RL_NKHomePort = "AUSYD"; //GMT +10 (Now Monday)

			destinationComponent.FC_GB_AgingBranch = branchUsedInEnvironmentContext.PK;
			Factory.Save();

			AssertCanReleaseToBuffer("Precondition - Shouldn't be able to transfer if calculator uses environment branch", workflow, destinationComponent, BufferReleaseOutcome.BlockedByResourceCapacity);

			destinationComponent.FC_GB_AgingBranch = branchUsedForDestinationComponent.PK;

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), branchUsedInEnvironmentContext.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertCanReleaseToBuffer("Should be able to transfer, as calculator should use destination branch (in which resource will be working)", workflow, destinationComponent);
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		static ProcessTask CreateTask(IWorkflowProvider parent, ProcessHeader header, decimal estHours, string assignedResource = "", string taskType = "")
		{
			var task = parent.WorkflowItems.AddNew();
			task.P9_EstDuration = new ZInt((int)(estHours * 60)).GetDateTimeFromMinutes();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_GS_NKAssignedStaffMember = assignedResource;
			task.P9_Type = taskType;

			return task;
		}

		void UpdateDepartmentWeekDaysTo9To5ExceptMonday9To4(ZGuid departmentPK)
		{
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Monday, NineToFour);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Tuesday, NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Wednesday, NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Thursday, NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Friday, NineToFive);
		}

		void UpdateStaffWeekDaysTo9To5ExceptMonday9To4(ZGuid staffPK)
		{
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Monday, NineToFour);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Tuesday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Wednesday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Thursday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Friday, NineToFive);
		}

		static string NineToFive
		{
			get { return WorkingDaysTestHelper.NineToFive; }
		}

		static string NineToFour
		{
			get { return NineToFive.Substring(0, NineToFive.Length - 2); }
		}

		protected BMSystem system, dummySystem;

		public static CapacityReservationTracker Create_ForTest(BMComponent buffer, bool trackResourceQueues = true, GlbStaff[] staffInvolvedInWorkflow = null)
		{
			if (staffInvolvedInWorkflow == null)
			{
				staffInvolvedInWorkflow = buffer.Factory.Load<GlbStaff>(new ZQuery());
			}

			var capacities = CapacityCalculator.GetUtilisedCapacityBreakdown(staffInvolvedInWorkflow, buffer);
				
			return new CapacityReservationTracker(buffer, capacities, trackResourceQueues);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSTestCaseWithFactory.SetupAndClearTables();
			system = BMSTestHelper.CreateSystem(Factory, "ORG");

			dummySystem = BMSTestHelper.CreateSystem(Factory, "DUM");
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		protected virtual bool UsingSimpleQuery => false;

		#endregion
	}
}
