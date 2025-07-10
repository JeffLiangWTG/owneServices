using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.BufferManagement.Business.BufferPenetrationCalculator;

namespace Enterprise.BufferManagement.Business.Test
{
	public class CapacitySimpleQueryContentProviderTest : TestCaseWithFactory
	{
		[GuiTest, TestDate(2023, 02, 1)]
		public void Test_ZoneMultiplier_WhenCreateQI()
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COD", "CBC");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBC", WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 100, loadLimitPercent: 100);

			EnableSimpleCapacityUsingExperimentalSettings(system.PK, false);

			BMSTestHelper.CreateZoneMultiplier(buffer, zone3Multiplier: 1, zone2Multiplier: 2, zone1Multiplier: 3, zone0Multiplier: 4);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "workflow", buffer);

			var task1 = BMSTestHelper.CreateTask(workflow, staff1.GS_Code, lowEstMinutes: 120, estVariationFactor: 1, taskType: "COD");
			var task2 = BMSTestHelper.CreateTask(workflow, staff2.GS_Code, 60, estVariationFactor: 1, taskType: "CBC");

			Factory.Save();

			var zone = workflow.BufferZone;
			AssertEquals("Zone 3", 3, zone);

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(2m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 2, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 1, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(80);
			zone = workflow.BufferZone;
			AssertEquals("Zone 1", 1, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(6m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(3m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 6, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 3, zone0Hours: 0);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var qualityIterationWorkflow = BMSTestCaseWithFactory.CreateQualityIteration(task1, task2);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			AssertEquals(1, qualityIterationWorkflow.BufferZone);
			AssertEquals("Parent workflow has no open tasks, buy QI workflow does have open tasks", 0, workflow.Tasks.Count(t => t.IsOpen));
			AssertEquals("Release date of the QI is correct same as parent", workflow.FH_ReleaseDateTime, qualityIterationWorkflow.FH_ReleaseDateTime);

			Factory.Save();

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(6m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(3m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("Capacity should be same as the parent workflow's tasks are closed, and the QI has replaced it with tasks of the same estimated size",
				capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 6, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("Capacity should be same as the parent workflow's tasks are closed, and the QI has replaced it with tasks of the same estimated size",
				capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 3, zone0Hours: 0);

			workflow.FH_FC_CurrentComponent = bucket.PK;
			Factory.Save();

			AssertEquals("Quality iteration should still be in zone 1 since it had its Last Transfer Date set to the same value as its parent workflow at the time", 1, qualityIterationWorkflow.BufferZone);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("Even though the parent workflow Last Transfer Time would place it in zone 3, it's actually not in the buffer so can be ignored",
				  capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 6, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("Even though the parent workflow Last Transfer Time would place it in zone 3, it's actually not in the buffer so can be ignored",
				  capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 3, zone0Hours: 0);

			workflow.FH_FC_CurrentComponent = buffer.PK;
			Factory.Save();

			AssertEquals("Re-releasing the parent workflow should move it to zone 3", 3, workflow.BufferZone);
			AssertEquals("Re-releasing the parent workflow should cause the QI workflow to also be in zone 3", 3, qualityIterationWorkflow.BufferZone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);

			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("Capacity usage should now be all within zone 3",
				   capacityForBothResources[staff1], zone3Hours: 2, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("Capacity usage should now be all within zone 3",
				   capacityForBothResources[staff2], zone3Hours: 1, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
		}

		[TestDate(2023, 02, 1)]
		public void TestCapacity_WhenZoneMultipliersDisabled()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			EnableSimpleCapacityUsingExperimentalSettings(system.PK, true);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 100, loadLimitPercent: 100);

			BMSTestHelper.CreateZoneMultiplier(buffer, zone3Multiplier: 1, zone2Multiplier: 2, zone1Multiplier: 3, zone0Multiplier: 4);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "workflow", buffer);

			VisualBoardsTestHelper.CreateTask(workflow, staff1.GS_Code, 120, estVariationFactor: 1);
			VisualBoardsTestHelper.CreateTask(workflow, staff2.GS_Code, 60, estVariationFactor: 1);

			Factory.Save();

			var zone = workflow.BufferZone;
			AssertEquals("Zone 3", 3, zone);

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(2m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(50);
			zone = workflow.BufferZone;
			AssertEquals("Zone 2", 2, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(2m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(30);
			zone = workflow.BufferZone;
			AssertEquals("Zone 1", 1, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(2m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(20);
			zone = workflow.BufferZone;
			AssertEquals("Zone 0", 0, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(2m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
		}

		[TestDate(2023, 02, 1)]
		public void TestCapacity_WhenZoneMultipliersEnabled()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 100, loadLimitPercent: 100);

			BMSTestHelper.CreateZoneMultiplier(buffer, zone3Multiplier: 1, zone2Multiplier: 2, zone1Multiplier: 3, zone0Multiplier: 4);

			EnableSimpleCapacityUsingExperimentalSettings(system.PK, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "workflow", buffer);

			VisualBoardsTestHelper.CreateTask(workflow, staff1.GS_Code, 120, estVariationFactor: 1);
			VisualBoardsTestHelper.CreateTask(workflow, staff2.GS_Code, 60, estVariationFactor: 1);

			Factory.Save();

			var zone = workflow.BufferZone;
			AssertEquals("Zone 3", 3, zone);

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(2m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 2, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 1, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(50);
			zone = workflow.BufferZone;
			AssertEquals("Zone 2", 2, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(4m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(2m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 4, zone1Hours: 0, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 2, zone1Hours: 0, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(30);
			zone = workflow.BufferZone;
			AssertEquals("Zone 1", 1, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(6m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(3m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 6, zone0Hours: 0);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 3, zone0Hours: 0);

			Factory.ClearCachedValue<BufferPenetrationCache>();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(20);
			zone = workflow.BufferZone;
			AssertEquals("Zone 0", 0, zone);

			capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);
			AssertEquals(8m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(4m, capacityForBothResources[staff2].UtilisedCapacity);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff1], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 8);
			CapacityCalculatorTestHelper.AssertReservedCapacityBreakdown("reserved capacity breakdown", capacityForBothResources[staff2], zone3Hours: 0, zone2Hours: 0, zone1Hours: 0, zone0Hours: 4);
		}

		[TestDate(2023, 02, 1)]
		public void TestDBHits_ShouldHitStmDataOnlyOnce_WhenUsingExperimentalSettings()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 60, loadLimitPercent: 100);
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = TestDateAttribute.Date;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var taskForStaff1 = VisualBoardsTestHelper.CreateTask(workflow, staff1.GS_Code, 15, estVariationFactor: 1);
			var taskForStaff2 = VisualBoardsTestHelper.CreateTask(workflow, staff2.GS_Code, 30, estVariationFactor: 1);
			var taskForStaff3 = VisualBoardsTestHelper.CreateTask(workflow, staff3.GS_Code, 60, estVariationFactor: 1);

			EnableSimpleCapacityUsingExperimentalSettings(system.PK, true);

			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ StmDataSchema.Constants.TableName, 1 },
			};

			BMSTestHelper.ClearUberFactory();

			var newFactory = new BusinessObjectFactory();

			var componentFromNewFactory = newFactory.Load<BMComponent>(buffer.PK);
			var staff1FromNewFactory = newFactory.Load<GlbStaff>(staff1.PK);
			var staff2FromNewFactory = newFactory.Load<GlbStaff>(staff2.PK);
			var staff3FromNewFactory = newFactory.Load<GlbStaff>(staff3.PK);

			using (AssertDbHitsForAllFactories(hits, ignoreUnspecified: true, thresholdForUnspecified: 10, includeFactoryPredicate: f => f == newFactory))
			{
				CapacityCalculator.GetUtilisedCapacityBreakdown(staff1FromNewFactory, componentFromNewFactory, useCache: false);
				CapacityCalculator.GetUtilisedCapacityBreakdown(staff2FromNewFactory, componentFromNewFactory, useCache: false);
				CapacityCalculator.GetUtilisedCapacityBreakdown(staff3FromNewFactory, componentFromNewFactory, useCache: false);
			}
		}

		[TestDate(2023, 02, 1)]
		public void TestDBHits()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 100, loadLimitPercent: 100);
			var staffPKs = new List<ZGuid>();

			for (var i = 0; i < 5; i++)
			{
				var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

				var group1 = VisualBoardsTestHelper.CreateGroup(Factory, "G1" + i);
				var group2 = VisualBoardsTestHelper.CreateGroup(Factory, "G2" + i);
				var capability1 = VisualBoardsTestHelper.CreateCapability(Factory, "C1" + i, isGroupScope: false);
				var capability2 = VisualBoardsTestHelper.CreateCapability(Factory, "C2" + i, isGroupScope: true);

				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				var staff2 = Factory.NewWithValidTestData<GlbStaff>();

				staff1.Capabilities.AddRange(capability1);
				staff2.Capabilities.AddRange(capability2);
				staff1.Groups.AddRange(group1, group2);

				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = buffer.PK;
				workflow.FH_GG_ReleaseGroup = group1.PK;

				var taskForStaff1 = VisualBoardsTestHelper.CreateTask(workflow, staff1.GS_Code, 60, estVariationFactor: 1);
				var taskForStaff2 = VisualBoardsTestHelper.CreateTask(workflow, staff2.GS_Code, 60, estVariationFactor: 1);
				var taskForCapability1 = VisualBoardsTestHelper.CreateTask(workflow, capability: capability1, lowEstMinutes: 60, estVariationFactor: 1);
				var taskForCapability2 = VisualBoardsTestHelper.CreateTask(workflow, capability: capability2, lowEstMinutes: 60, estVariationFactor: 1);
				var taskForCapability1AndStaff1 = VisualBoardsTestHelper.CreateTask(workflow, staff1.GS_Code, 60, capability: capability1, estVariationFactor: 1);
				var taskForCapability2AndStaff2 = VisualBoardsTestHelper.CreateTask(workflow, staff2.GS_Code, 60, capability: capability2, estVariationFactor: 1);
				var taskForCapability2InGroup2 = VisualBoardsTestHelper.CreateTask(workflow, capability: capability2, lowEstMinutes: 60, estVariationFactor: 1);
				taskForCapability2InGroup2.P9_GG_AssignedGroup = group2.PK;

				staffPKs.Add(staff1.PK);
				staffPKs.Add(staff2.PK);
			}

			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ GlbCapabilitySchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
				{ ProcessTasksSchema.Constants.TableName, 0 },
				{ ProcessHeaderSchema.Constants.TableName, 0 },
				{ StmDataSchema.Constants.TableName, 1 },
			};

			BMSTestHelper.ClearUberFactory();

			var newFactory = new BusinessObjectFactory();

			var componet = newFactory.Load<BMComponent>(buffer.PK);
			var staff = newFactory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPKs));

			using (AssertDbHitsForAllFactories(hits, ignoreUnspecified: false, includeFactoryPredicate: f => f == newFactory))
			{
				CapacitySimpleQueryContentProvider.Load(componet, staff);
			}
		}

		[TestDate(2023, 02, 1)]
		public void TestStaff()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 100, loadLimitPercent: 50);
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var taskForStaff1 = VisualBoardsTestHelper.CreateTask(workflow1, staff1.GS_Code, 120, estVariationFactor: 2); // Std: 3h
			var taskForStaff2 = VisualBoardsTestHelper.CreateTask(workflow1, staff2.GS_Code, 60, estVariationFactor: 2); // Std: 1.5h

			AssertEquals(3m, taskForStaff1.StandardEstimateHours);
			AssertEquals(1.5m, taskForStaff2.StandardEstimateHours);

			Factory.Save();

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);

			AssertEquals(3m, capacityForBothResources[staff1].UtilisedCapacity);
			AssertEquals(1.5m, capacityForBothResources[staff2].UtilisedCapacity);
		}

		[TestDate(2023, 02, 1)]
		public void TestCapability()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 240, loadLimitPercent: 100); // 4h
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var capability = VisualBoardsTestHelper.CreateCapability(Factory, "CAP", isGroupScope: false);
			staff1.Capabilities.Add(capability);
			staff2.Capabilities.Add(capability);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var taskForStaff1 = VisualBoardsTestHelper.CreateTask(workflow1, staff1.GS_Code, 120, estVariationFactor: 1); // Std: 2h
			var taskForStaff2 = VisualBoardsTestHelper.CreateTask(workflow1, staff2.GS_Code, 60, estVariationFactor: 1); // Std: 1h

			var taskForCapability = VisualBoardsTestHelper.CreateTask(workflow1, capability: capability, lowEstMinutes: 240, estVariationFactor: 1); // Std: 4h

			AssertEquals(2m, taskForStaff1.StandardEstimateHours);
			AssertEquals(1m, taskForStaff2.StandardEstimateHours);
			AssertEquals(4m, taskForCapability.StandardEstimateHours);

			Factory.Save();

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);

			AssertEquals(4m, capacityForBothResources[staff1].UtilisedCapacity); //2h for the task + 4/2=2 for the capabilityTask
			AssertEquals(3m, capacityForBothResources[staff2].UtilisedCapacity); //1h for the task + 4/2=2 for the capabilityTask
		}

		public void TestGroupCapability()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system, timespanMinutes: 240, loadLimitPercent: 100); // 4h
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var capability = VisualBoardsTestHelper.CreateCapability(Factory, "CAP", isGroupScope: true);
			staff1.Capabilities.Add(capability);
			staff2.Capabilities.Add(capability);
			staff3.Capabilities.Add(capability);

			var group = VisualBoardsTestHelper.CreateGroup(Factory, "GP1");
			staff1.Groups.Add(group);
			staff2.Groups.Add(group);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			workflow1.FH_GG_ReleaseGroup = group.PK;

			var taskForStaff1 = VisualBoardsTestHelper.CreateTask(workflow1, staff1.GS_Code, 120, estVariationFactor: 1); // Std: 2h
			var taskForStaff2 = VisualBoardsTestHelper.CreateTask(workflow1, staff2.GS_Code, 60, estVariationFactor: 1); // Std: 1h

			var taskForCapability = VisualBoardsTestHelper.CreateTask(workflow1, capability: capability, lowEstMinutes: 240, estVariationFactor: 1); // Std: 4h

			AssertEquals(2m, taskForStaff1.StandardEstimateHours);
			AssertEquals(1m, taskForStaff2.StandardEstimateHours);
			AssertEquals(4m, taskForCapability.StandardEstimateHours);

			Factory.Save();

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { staff1, staff2 }, buffer, useCache: false);

			AssertEquals(4m, capacityForBothResources[staff1].UtilisedCapacity); //2h for the task + 4/2=2 for the capabilityTask
			AssertEquals(3m, capacityForBothResources[staff2].UtilisedCapacity); //1h for the task + 4/2=2 for the capabilityTask
		}

		public void EnableSimpleCapacityUsingExperimentalSettings(ZGuid systemPK, bool disableZoneMultipliers) => EnableSimpleCapacityUsingExperimentalSettings(Factory, systemPK, disableZoneMultipliers);

		public static void EnableSimpleCapacityUsingExperimentalSettings(BusinessObjectFactory factory, ZGuid systemPK, bool disableZoneMultipliers)
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var provider = new ExperimentalSettingsProvider(systemPK, factory);
			var simpleCapacitySetting = new ExperimentalSetting { Key = ExperimentalSettingsProvider.SimpleCapacityExperimentalSettingsKey };
			var disableZoneMultipliersSetting = new ExperimentalSetting { Key = ExperimentalSettingsProvider.DisableZoneMultipliersExperimentalSettingsKey };

			simpleCapacitySetting.Value = true.ToString();
			disableZoneMultipliersSetting.Value = disableZoneMultipliers.ToString();
			provider.ExperimentalSettings.Add(simpleCapacitySetting);
			provider.ExperimentalSettings.Add(disableZoneMultipliersSetting);
			provider.SaveSettings();
		}

		protected override void SetUp()
		{
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();

			base.SetUp();
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.PlanningManagement);
			BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
