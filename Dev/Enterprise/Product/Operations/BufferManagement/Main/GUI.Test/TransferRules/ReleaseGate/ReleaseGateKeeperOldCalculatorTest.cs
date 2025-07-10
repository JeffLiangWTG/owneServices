using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ReleaseGateKeeperOldCalculatorTest : ReleaseGateKeeperGUITest
	{
		protected override void LocalSetUp()
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#region Old Cache

		[TestDate(2015, 7, 14)]
		public void TestProcess_ShouldCauseCapacityToBeCached_Correctly()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var capability = BMSTestHelper.CreateCapability(Factory, "PDV", "Pediveh");

			capability.ResourcesWithCapability.AddRange(resource1, resource2, resource3);
			config.ReleaseGroup.Staff.AddRange(resource1, resource2, resource3);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "INFJ", config.Buffer);

			workflow1.FH_VoteUpDownAmount = 100;
			workflow2.FH_VoteUpDownAmount = -100;

			BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 6000); // Should prevent release, forcing capacity to stay the same.
			BMSTestHelper.CreateTask(workflow1, resource2.GS_Code, 6000); // Should prevent release, forcing capacity to stay the same.
			BMSTestHelper.CreateTask(workflow1, resource3.GS_Code, 6000); // Should prevent release, forcing capacity to stay the same.
			BMSTestHelper.CreateTask(workflow2, string.Empty, 60, capability: capability);
			BMSTestHelper.CreateTask(workflow2, resource3.GS_Code, 60, capability: capability); // This task is assigned directly to resource3, so shouldn't affect capacity of other resources.

			Factory.Save();

			CombineAssertions("Pre-condition: initial resource capacity is the same - all resources have a bit deducted for capability task", () =>
			{
				AssertEquals("resource1", 41.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity);
				AssertEquals("resource2", 41.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity);
				AssertEquals("resource3", 40m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource3, config.Buffer).AvailableCapacity);
			});

			RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var cachedCapacity1 = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource1.GS_Code, newFactory);
			var cachedCapacity2 = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource2.GS_Code, newFactory);
			var cachedCapacity3 = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource3.GS_Code, newFactory);

			AssertNotNull(cachedCapacity1);
			AssertNotNull(cachedCapacity2);
			AssertNotNull(cachedCapacity3);

			CombineAssertions("Resource capacity should be the same, considering nothing was released.", () =>
			{
				AssertEquals("resource1", 41.5m, cachedCapacity1.AvailableCapacity);
				AssertEquals("resource2", 41.5m, cachedCapacity2.AvailableCapacity);
				AssertEquals("resource3", 40m, cachedCapacity3.AvailableCapacity);
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestProcess_ShouldBeBatchedByStaff()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var staffs = new List<GlbStaff>();
			var workflows = new List<ProcessHeader>();

			for (int i = 0; i < 40; i++)
			{
				var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, $"workflow{i}", config.Bucket);
				workflows.Add(workflow);

				BMSTestHelper.CreateTask(workflow, staff.GS_Code, 60);
				staffs.Add(staff);
			}

			Factory.Save();

			BMSRegistry.Instance.CapacityCalculatorStaffBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);

			foreach (var staff in staffs)
			{
				AssertEquals("Pre - condition: initial resource capacity", 42.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer).AvailableCapacity);
			}

			using (Db.Connection.TrackExecutedCommands())
			{
				var expectedHits = 2;
				var executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains("CAPACITY CALCULATION")).ToArray();
				AssertEquals(0, executedParameterizedQueries.Length);

				RunReleaseGate(config.System);

				executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains("CAPACITY CALCULATION")).ToArray();
				AssertEquals("GIVEN 10 staff and batch=5 WHEN RunReleaseGate THEN queries should be called 40/20=2 and occurs in EnsureCapacityCacheBuilt"
					, expectedHits
					, executedParameterizedQueries.Length);
			}

			var newFactory = Factory.CreateNewFactory();
			foreach (var staff in staffs)
			{
				var cachedCapacity = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(staff.GS_Code, newFactory);
				AssertNotNull(cachedCapacity);
				AssertEquals(40.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer).AvailableCapacity);
			}
		}

		#endregion

		#region Process

		[TestDate(2019, 1, 1)]
		public void TestCapacityCache_ShouldNotDisappearMidRun()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Kate Miller-Heidke");
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Zero");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Gravity");
			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60, estVariationFactor: 1);

			workflow1.FH_VoteUpDownAmount = 2;
			workflow2.FH_VoteUpDownAmount = 1;

			Factory.Save();

			RunReleaseGate(config.System); // Force capacity to be cached.

			workflow1.Reload();
			workflow2.Reload();
			workflow1.FH_FC_CurrentComponent = config.Bucket.PK;
			workflow2.FH_FC_CurrentComponent = config.Bucket.PK;

			// Create extra tasks so the capacity numbers are different on the next run.
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60, estVariationFactor: 1);
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();

			var coordinator = new ReleaseGateTestCoordinator
			{
				IsWorkflowReleasableFunc = w =>
				{
					if (w.PK == workflow2.PK)
					{
						BufferCapacityCache.Clear();
					}

					return true;
				}
			};

			AssertAvailableCapacity(resource, config.Buffer, 46m);

			AssertSamePK(config.Bucket, workflow1.CurrentComponent);
			AssertSamePK(config.Bucket, workflow2.CurrentComponent);

			RunReleaseGate(config.System, coordinator: coordinator);

			workflow1.Reload();
			workflow2.Reload();

			AssertSamePK(config.Buffer, workflow1.CurrentComponent);
			AssertSamePK(config.Buffer, workflow2.CurrentComponent);

			AssertExpectedCapacity();

			BufferCapacityCache.Clear(); // Ensure that we actually persisted the updated capacity, rather than just update it in memory.
			AssertExpectedCapacity();

			void AssertExpectedCapacity()
			{
				AssertCapacityBreakdown("Clearing the capacity cache mid-run shouldn't ruin everything", resource, config.Buffer, fullCapacityHours: 48m, availableCapacityHours: 44m, availableCapacityHoursForCCRWork: 92m,
					zone3ReservedHours: 4, zone3AllocatedHours: 4);
			}
		}

		#endregion
	}
}
