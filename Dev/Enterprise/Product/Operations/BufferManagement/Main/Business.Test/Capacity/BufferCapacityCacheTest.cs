using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class BufferCapacityCacheTest : BMSTestCaseWithFactory
	{
		[TestDate(2019, 1, 1)]
		public void TestRestoreCapacityFromPersistedCache_ShouldPreserveNonCcrOverloadProperly()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "IAmA Workflow AMA");
			BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code);
			BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code);
			BMSTestHelper.CreateTask(workflow, config.NonCCR2.GS_Code);

			Factory.Save();

			AssertFullCapacity(config.CCR, config.Buffer, 56m, 56m);

			ReleaseGateKeeperTest.RunReleaseGate(config.System);
			BufferCapacityCache.Clear(); // Ensure we definitely go back to the DB.

			AssertFullCapacity(config.CCR, config.Buffer, 56m, 56m);
		}

		public void TestBufferCapacityCache_MemoryCache_ShouldGetCapacityAfterPopulatingCache()
		{
			Assert("There should not be any entries in MemoryCache", CapacityCacheIsEmpty);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Bucket);

			BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var context = WorkingTimeContext.Create(config.Buffer);

			AssertEquals("CapacityCalculator should have a valid AvailableCapacity", 19.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			workflow.Reload();
			AssertEquals(config.Buffer, workflow.CurrentComponent);

			var newFactory = Factory.CreateNewFactory();
			var cachedCapacity = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource.GS_Code, newFactory);

			AssertNotNull("BufferCapacityCache should not be null", cachedCapacity);
			AssertEquals("Retrieved Capacity should be valid post cache population", 17.5m, cachedCapacity.AvailableCapacity);
			var memoryCacheCount = MemoryCache.Default.Select(kvp => kvp.Key).Where(key => key.Contains(config.Buffer.PK.ToString())).ToList().Count;
			AssertEquals("There should be one (1) cache entry (the new BufferCapacityCache) in MemoryCache", 1, memoryCacheCount);
		}

		public void TestBufferCapacityCache_DatabaseData()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Bucket);
			BMSTestHelper.CreateTask(workflow, staff1.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow, staff2.GS_Code, 90);
			Factory.Save();

			var context = WorkingTimeContext.Create(config.Buffer);
			BufferCapacityCacheTest.PurgeCachedCapacity();
			AssertEquals("Should have no cache entries in DB", 0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM BMCapacityCache"));

			ReleaseGateKeeperTest.RunReleaseGate(config.System);
			AssertEquals("Should have 2 entries in DB: 1 for each staff", 2, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM BMCapacityCache"));

			ReleaseGateKeeperTest.RunReleaseGate(config.System);
			AssertEquals("Should have same 2 entries in DB: 1 for each staff", 2, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM BMCapacityCache"));
		}

		public void TestBufferCapacityCache_GetCapacityIfCachedOrCalculateIfNot_WhenCallGetUtilisedCapacityBreakdown()
		{
			Assert("There should not be any entries in MemoryCache", CapacityCacheIsEmpty);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "re1", "Resource 1", capabilities: capability);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1", config.Buffer, staffCode: resource1.GS_Code);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, lowEstMinutes: 500, capability: capability, description: "W1_C1");

			Factory.Save();

			var calculatedAllocatedCapacity_resource1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer, useCache: false);
			AssertNull("BufferCapacityCache for resource1 should be null", BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource1.GS_Code, Factory));

			CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer, useCache: true);
			AssertNotNull("BufferCapacityCache for resource1 should not be null", BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource1.GS_Code, Factory));

			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "re2", "Resource 2", capabilities: capability);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2", config.Buffer, staffCode: resource2.GS_Code);

			Factory.Save();

			var calculatedAllocatedCapacity_resource2 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer, useCache: false);
			AssertNull("BufferCapacityCache for resource2 should be null", BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource2.GS_Code, Factory));

			var calculatedAllocatedCapacities = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2 }, config.Buffer);

			CombineAssertions("Capacity should be equal when calculate for one and for more resources.", () =>
			{
				AssertEquals("resource 1", 12.5m, calculatedAllocatedCapacity_resource1.UtilisedCapacity);
				AssertEquals("resource 2", 6.25m, calculatedAllocatedCapacity_resource2.UtilisedCapacity);
				AssertEquals("resource 1", calculatedAllocatedCapacity_resource1.UtilisedCapacity, calculatedAllocatedCapacities[resource1].UtilisedCapacity);
				AssertEquals("resource 2", calculatedAllocatedCapacity_resource2.UtilisedCapacity, calculatedAllocatedCapacities[resource2].UtilisedCapacity);
			});
		}

		public void TestBufferCapacityCache_MergeCapacityLocalCache()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, workflowType: "DUM");

			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "re1", "Resource 1");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "re2", "Resource 2");

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1", config.Buffer, staffCode: resource1.GS_Code);
			var task1 = workflow1.Tasks.First();
			task1.P9_EstimateVariationFactor = 1;
			task1.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2", config.Buffer, staffCode: resource2.GS_Code);
			var task2 = workflow2.Tasks.First();
			task2.P9_EstimateVariationFactor = 1;
			task2.P9_EstDuration = new ZInt(300).GetDateTimeFromMinutes();

			Factory.Save();

			var bufferPK = config.Buffer.PK;

			IEnumerable<string> GetCapacityQuery(IEnumerable<string> queries) => queries.Where(s => s.Contains("CAPACITY CALCULATION SIMPLE QUERY")).ToArray();

			using (Db.Connection.TrackExecutedCommands())
			{
				var calculatedAllocatedCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1.WrapWithEnumerable(), config.Buffer, useCache: true);
				var executedCommands = GetCapacityQuery(Db.Connection.ExecutedCommands);

				AssertEquals(1, executedCommands.Count());
			}

			var resource1MemoryCapacity = BufferCapacityCache.Get(bufferPK).GetCapacity(resource1.GS_Code, Factory);
			AssertNotNull(resource1MemoryCapacity);

			using (var commands = Db.Connection.TrackExecutedCommands())
			{
				var calculatedAllocatedCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2.WrapWithEnumerable(), config.Buffer, useCache: true);
				var executedCommands = GetCapacityQuery(Db.Connection.ExecutedCommands);

				AssertEquals(1, executedCommands.Count());
			}

			var resource2MemoryCapacity = BufferCapacityCache.Get(bufferPK).GetCapacity(resource2.GS_Code, Factory);
			AssertNotNull(resource2MemoryCapacity);

			using (var commands = Db.Connection.TrackExecutedCommands())
			{
				var calculatedAllocatedCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2 }, config.Buffer, useCache: true);
				var executedCommands = GetCapacityQuery(Db.Connection.ExecutedCommands);

				AssertEquals(0, executedCommands.Count());
			}
		}

		#region Implementation

		static bool CapacityCacheIsEmpty => !MemoryCache.Default.Any(c => c.Key.StartsWith("CapacityMemoryCache"));

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		public static void PurgeCachedCapacity()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.BMCapacityCache");
			BufferCapacityCache.Clear();
		}

		#endregion
	}
}
