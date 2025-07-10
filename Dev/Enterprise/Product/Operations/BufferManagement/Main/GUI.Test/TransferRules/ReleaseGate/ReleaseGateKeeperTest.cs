using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public abstract class ReleaseGateKeeperGUITest : ReleaseGateKeeperTest
	{
		#region Release

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_WithCapabilityTasks_OneStaff_WhenInsufficientCapacity_ShouldNotBeReleased_CapabilityTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);

			// OBSERVE the extra estimated minute on taskCapability. It should make a world of difference!
			var taskCapability = BMSTestHelper.CreateTask(workflowToBeReleased, string.Empty, 81, capability: capability);

			var workflowAlreadyReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = 43 * 60;
			BMSTestHelper.CreateTask(workflowAlreadyReleased, staff.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should not be released", config.Bucket, workflowToBeReleased.CurrentComponent);

				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code));

#if !WINZOR
				AssertMultilineASCIIEquals("Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10.03 hours, Remaining Capacity: -5.03 hours", link.CapacityReservationDetails);
#else
				AssertMultilineASCIIEquals("Should not be released",
					@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10.02 hours, Remaining Capacity: -5.02 hours", link.CapacityReservationDetails);
#endif
			});

#if !WINZOR
			AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 10.03 hours, currently has 5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001001 (2 hours), T00001002 (2 hours), T00001003 (2 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001004 (2.03 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
#else
			AssertMultilineASCIIEquals("Release Gate log",
				@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 10.02 hours, currently has 5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001001 (2 hours), T00001002 (2 hours), T00001003 (2 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001004 (2.03 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
#endif
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_WithCapabilityTasks_OneStaff_WhenInsufficientCapacity_ShouldNotBeReleased_ResourceTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 81); // OBSERVE the extra estimated minute on this task. It should make a world of difference!

			var taskCapability = BMSTestHelper.CreateTask(workflowToBeReleased, string.Empty, 80, capability: capability);

			var workflowAlreadyReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = 43 * 60;
			BMSTestHelper.CreateTask(workflowAlreadyReleased, staff.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should not be released", config.Bucket, workflowToBeReleased.CurrentComponent);

				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code));

#if !WINZOR
				AssertMultilineASCIIEquals("Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10.03 hours, Remaining Capacity: -5.03 hours", link.CapacityReservationDetails);
#else
				AssertMultilineASCIIEquals("Should not be released",
					@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10.02 hours, Remaining Capacity: -5.02 hours", link.CapacityReservationDetails);
#endif
			});

#if !WINZOR
			AssertMultilineASCIIEquals("Bad Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 10.03 hours, currently has 5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001001 (2 hours), T00001002 (2 hours), T00001003 (2.03 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001004 (2 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
#else
			AssertMultilineASCIIEquals("Bad Release Gate log",
				@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 10.02 hours, currently has 5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001001 (2 hours), T00001002 (2 hours), T00001003 (2.03 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001004 (2 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
#endif
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_OneCapabilityTask_NoResourceTask_WhenInsufficientCapacity_ShouldBeBlocked()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			var taskCapability = BMSTestHelper.CreateTask(workflowToBeReleased, string.Empty, 401, capability: capability);

			var workflowAlreadyReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = 43 * 60;
			BMSTestHelper.CreateTask(workflowAlreadyReleased, staff.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should be blocked", config.Bucket, workflowToBeReleased.CurrentComponent);
				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code));
#if !WINZOR
				AssertMultilineASCIIEquals("Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10.03 hours, Remaining Capacity: -5.03 hours", link.CapacityReservationDetails);
#else
				AssertMultilineASCIIEquals("Should not be released",
					@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10.02 hours, Remaining Capacity: -5.02 hours", link.CapacityReservationDetails);
#endif

			});

#if !WINZOR
			AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 10.03 hours, currently has 5 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Orange Ocelot Ordering]: T00001000 (10.03 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
#else
			AssertMultilineASCIIEquals("Release Gate log",
				@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 10.02 hours, currently has 5 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Orange Ocelot Ordering]: T00001000 (10.02 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
#endif
		}

#endregion

		#region Process

		public void TestProcess_WhenMoreWorkflowsThanBatchSize_ShouldSaveAppropriateFactory()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.ReleaseGateBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, shouldUseExistingSystem: true);

			for (int i = 0; i < batchSize; i++)
			{
				var workflowForResource1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				var taskForResource1 = CreateTask(workflowForResource1, resource1.GS_Code, 60);
				workflowForResource1.AddTag(config.RedTag);
			}

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0]; // Will be the first item in release sequence, passing golden rules, and causing subsequent workflows to get loaded in the first batch
			var task1 = CreateTask(workflow1, resource2.GS_Code, 60);
			workflow1.FH_VoteUpDownAmount = 1000;

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0]; // Will be the second item in release sequence, getting relegated to second batch by platinum golden rule
			var task2 = CreateTask(workflow2, resource2.GS_Code, 60);
			workflow2.AddTag(config.PlatinumTag);

			Factory.Save();

			AssertNoExceptionThrown(() => RunReleaseGate(config.System));

			workflow2.Reload();

			AssertSamePK(config.Buffer, workflow2.CurrentComponent);
		}

		public void TestProcess_NumberOfWorkflowsInReleaseGateTransferRuleRunnerFactory_ShouldNotExceedBatchSize()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.ReleaseGateBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, shouldUseExistingSystem: true);

			for (int j = 0; j < 3; j++)
			{
				for (int i = 0; i < batchSize; i++)
				{
					var workflowForResource1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
					var taskForResource1 = CreateTask(workflowForResource1, resource.GS_Code, 60);
					workflowForResource1.AddTag(config.RedTag);
				}
			}

			Factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				AssertNoExceptionThrown(() => RunReleaseGate(config.System));

				var trackedFactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Where(f =>
					f.NameForDebugging == "ReleaseGateTransferRuleRunner" || f.NameForDebugging.Contains(nameof(ServiceTaskFactoryProviderWrapper)) || f.NameForDebugging.Contains(nameof(TransferRuleRunnerDataAccessor))
					&& f.BusinessObjectsInformation.Contains("ProcessHeader"));

				AssertEquals(3, trackedFactories.Count());

				foreach (var trackedFactory in trackedFactories)
				{
					CombineAssertions("The number of workflows and job-level workflows in the factory should not exceed the batch size.", () =>
					{
						AssertContains("ProcessHeader, 10", trackedFactory.BusinessObjectsInformation);
						AssertContains("ProcessJobHeader, 10", trackedFactory.BusinessObjectsInformation);
					});
				}
			}
		}

		public void TestProcess_ShouldNeverHaveNullWorkflowPropertyOnReleaseGateRequestNodeAfterWorkflowLoad()
		{
			BMSRegistry.Instance.ReleaseGateStaleTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);

			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, shouldUseExistingSystem: true);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-4);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-3);

			jobHeader.AddTag(config.PlatinumTag);

			Factory.Save();

			void DeleteAWorkflowToImpedeProcessRun()
			{
				workflow2.Delete();
				Factory.Save();
			}

			var logger = new BufferManagementLogger();
			RunReleaseGate(config.System, logger, coordinator: new ReleaseGateTestCoordinator { PreWorkflowReloadAction = DeleteAWorkflowToImpedeProcessRun });

			var deletedWorkflow = Factory.Load(typeof(ProcessHeader), workflow2.PK);

			AssertNull("We should have killed this workflow.", deletedWorkflow);
			AssertEquals("We should have no error reported, but instead...", "", ErrorReporter.LastMessageReported);
		}

		#endregion

		#region Error Handling

		public void TestLinkWithFilterWithCountrySpecificModule_WhenModuleNotAvailableInServiceTaskContext_ShouldDisableLink_AndNotThrowExceptions()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var bucket1 = CreateBucket(system, "bucket");
			var bucket2 = CreateBuffer(system, "buffer");
			bucket2.FC_GB_AgingBranch = branch.PK;

			var link = LinkComponents(bucket1, bucket2, sequence: 0, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(link.FilterRule, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			var logger = new BufferManagementLogger();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNoExceptionThrown("Processing links with unavailable company-specific modules should not throw exceptions. SAD!", () => RunReleaseGate(system, logger));
			}

			AssertContains("The following component links have been deactivated since they have no filters or invalid filters:", logger.ToString());

			link.Reload();
			AssertEquals("The link should be disabled. SAD!", false, link.FL_TransferRulesEnabled);
		}

		#endregion
	}
}
