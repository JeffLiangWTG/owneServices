using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class RoadRunnerStatusCalculatorTest : BMSTestCaseWithFactory
	{
		#region Status Calculation

		[TestDate(2015, 7, 14)]
		public void TestGetRoadRunnerStatus_WhenStandaloneTaskInvolved_ShouldConsiderStandaloneTaskActivityTimes()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var section = config.BufferSection;
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var workflowOnBuffer = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This workflow ensures the resource gets the alert icon", config.Buffer);
			var taskClosedAgesAgo = BMSTestHelper.CreateTask(workflowOnBuffer, resource.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var bufferTask = BMSTestHelper.CreateTask(workflowOnBuffer, resource.GS_Code);

			taskClosedAgesAgo.CompletedTimeLocal = ZDateTime.Now.AddDays(-10);

			var standaloneTask = BMSTestHelper.CreateStandaloneTask(Factory, resource.GS_Code, ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			AssertRoadRunnerChannelStatus("Resource is working on a standalone task, but there is a buffer task pending, so should indicate alert state.", resource, section, RoadRunnerStatus.Alert, "Alert");

			standaloneTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			AssertRoadRunnerChannelStatus("Resource recently suspended a standalone task, so should indicate idle state and elapsed time since it was suspended.", resource, section, RoadRunnerStatus.Stopped, "Idle for 5 minutes");

			standaloneTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);

			AssertRoadRunnerChannelStatus("Resource recently closed a standalone task, so should indicate idle state and elapsed time since it was closed.", resource, section, RoadRunnerStatus.Stopped, "Idle for 10 minutes");
		}

		[TestDate(2015, 7, 14)]
		public void TestGetRoadRunnerStatus_OnlyStandbyWhenTargetOfCountdown()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1", currentComponent: config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow1, resource2.GS_Code);

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Buffer).Item2;

			config.System.FS_ResourceCountdownHours = new ZInt(300).GetDateTimeFromMinutes();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();
			var roadRunnerTaskCache = new RoadRunnerTaskCache(viewModel.ComponentPK, resource1);
			AssertEquals(RoadRunnerStatus.FullSpeed, GetRoadRunnerStatus(resource1, viewModel, roadRunnerTaskCache).Status);
			AssertEquals(RoadRunnerStatus.Stopped, GetRoadRunnerStatus(resource2, viewModel, roadRunnerTaskCache).Status);

			workflow1.FH_IsCriticalHandover = true;
			task1.P9_EstimatedHandoverTime = ZDateTimeOffset.UtcNow.AddHours(2);
			var roadRunnerTaskCache2 = new RoadRunnerTaskCache(viewModel.ComponentPK, resource1);
			Factory.Save();

			AssertEquals(RoadRunnerStatus.FullSpeed, GetRoadRunnerStatus(resource1, viewModel, roadRunnerTaskCache2).Status);
			AssertEquals(RoadRunnerStatus.Stopped, GetRoadRunnerStatus(resource2, viewModel, roadRunnerTaskCache2).Status);
		}

		[TestDate(2015, 7, 14)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestGetRoadRunnerStatus_UseUtcTime()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(system).Item2;

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, resource.GS_Code, 60);
			var task2 = CreateTask(workflow2, resource.GS_Code, 60);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			task1.P9_SuspendedAtForBinding = ZDateTimeOffset.Now.AddHours(-1);
			resource.GS_Code = "fro";
			Factory.Save();

			AssertEquals(TimeSpan.FromHours(1), GetRoadRunnerStatus(resource, viewModel).RelevantActivityTime);
		}

		[TestDate(2015, 7, 14)]
		public void TestGetRoadRunnerStatus()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(buffer, channels: staff).Item2;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, staff.GS_Code, 60);
			var task2 = CreateTask(workflow2, staff.GS_Code, 60);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			staff.GS_Code = "fro";
			Factory.Save();

			AssertRoadRunnerDetail("Has a working task in a buffer", staff, viewModel, RoadRunnerStatus.FullSpeed, task1);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertRoadRunnerDetail("Has no working tasks", staff, viewModel, RoadRunnerStatus.Stopped, null);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			BMSTestHelper.CacheTasksStartability(viewModel, task1, task2);

			AssertRoadRunnerDetail("Has a current task in a buffer, and is working on a task outside the buffer", staff, viewModel, RoadRunnerStatus.Alert, task2);

			task1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task1.P9_G4_RequiredCapability = capability1.PK;
			Factory.Save();

			AssertRoadRunnerDetail("Has a current task with a capability possessed by the resource in a buffer, and is working on a task outside the buffer", staff, viewModel, RoadRunnerStatus.Alert, task2);

			task1.P9_G4_RequiredCapability = capability2.PK;
			workflow2.FH_IsStandby = true;
			Factory.Save();
			BMSTestHelper.CacheTasksStartability(viewModel, task1, task2); // Refresh the cache Simulate Board Refresh

			AssertRoadRunnerDetail("Has no current task in any buffer (assigned to a capability resource does not have), and is working on a standby task", staff, viewModel, RoadRunnerStatus.WorkingOnStandbyTask, task2);

			workflow2.FH_IsStandby = false;
			Factory.Save();
			BMSTestHelper.CacheTasksStartability(viewModel, task1, task2); // Refresh the cache Simulate Board Refresh

			AssertRoadRunnerDetail("Has no current task in any buffer, and is working on a non-standby task", staff, viewModel, RoadRunnerStatus.WorkingInOtherComponent, task2);

			staff.GS_IsActive = false;
			Factory.Save();

			AssertRoadRunnerDetail("Resource is inactive", staff, viewModel, RoadRunnerStatus.None, null);
		}

		[TestDate(2019, 2, 4, 10, 15, 00)] // Monday, 4th Feb 2019, 10:15am
		public void TestGetRoadRunnerStatus_ShouldUseHomeDepartmentOrComponentDepartment_WhenCalculatingWorkingTime()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var branchForBufferComponent = Factory.NewWithValidTestData<GlbBranch>();
			var departmentForBufferComponent = Factory.NewWithValidTestData<GlbDepartment>();

			branchForBufferComponent.GB_RL_NKHomePort = "AUSYD";

			WorkingDaysTestHelper.SetDefaultDepartmentWorkTimeWeek(Factory, departmentForBufferComponent.PK);

			var branchForResource2 = Factory.NewWithValidTestData<GlbBranch>();
			var departmentForResource2 = Factory.NewWithValidTestData<GlbDepartment>();

			branchForResource2.GB_RL_NKHomePort = "AUBNE";

			WorkingDaysTestHelper.SetDefaultDepartmentWorkTimeWeek(Factory, departmentForResource2.PK);
			WorkingDaysTestHelper.CreateHoliday(Factory, branchForResource2.PK, new DateTime(2019, 2, 4), "Its someones birthday");

			config.Buffer.FC_GB_AgingBranch = branchForBufferComponent.PK;
			config.Buffer.FC_GE_AgingDepartment = departmentForBufferComponent.PK;

			var resource1 = BMSTestHelper.CreateStaff(Factory, "R1", "Resource1", branchForBufferComponent, departmentForBufferComponent);
			var resource2 = BMSTestHelper.CreateStaff(Factory, "R2", "Resource2", branchForBufferComponent, departmentForBufferComponent);

			resource2.GS_GB_HomeBranch = branchForResource2.PK;
			resource2.GS_GE_HomeDepartment = departmentForResource2.PK;

			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(config.System);
			var viewModel = sectionAndViewModel.Item2;
			var section = sectionAndViewModel.Item1;

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, "W1", config.Buffer, staffCode: resource1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, "W2", config.Buffer, staffCode: resource2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			AssertRoadRunnerDetail("Should be working based on the default department work time week", resource1, viewModel, RoadRunnerStatus.FullSpeed, workflow1.Tasks.Single());
			AssertRoadRunnerDetail("Should be away based on the updated department work time week", resource2, viewModel, RoadRunnerStatus.Away, null);
		}

		[TestDate(2019, 2, 4, 4, 15, 00)] // Monday, 4th Feb 2019, 4:15am
		[TestUtcOffset(10, 0, 0)] // Monday, 4th Feb 2019, 14:15am
		public void TestGetRoadRunnerStatus_ShouldUseHomeBranchTime_WhenCalculatingWorkingTime()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var branchForBufferComponent = Factory.NewWithValidTestData<GlbBranch>();
			var departmentForBufferComponent = Factory.NewWithValidTestData<GlbDepartment>();

			branchForBufferComponent.GB_RL_NKHomePort = "AUSYD";

			WorkingDaysTestHelper.SetDefaultDepartmentWorkTimeWeek(Factory, departmentForBufferComponent.PK);

			var branchForResource2 = Factory.NewWithValidTestData<GlbBranch>();
			branchForResource2.GB_RL_NKHomePort = "USCHI"; // USA, Chicago: GMT-6 - Sunday, 3rd Feb 2019, 22:15am

			config.Buffer.FC_GB_AgingBranch = branchForBufferComponent.PK;
			config.Buffer.FC_GE_AgingDepartment = departmentForBufferComponent.PK;

			var resource1 = BMSTestHelper.CreateStaff(Factory, "R1", "Resource1", branchForBufferComponent, departmentForBufferComponent);
			var resource2 = BMSTestHelper.CreateStaff(Factory, "R2", "Resource2", branchForBufferComponent, departmentForBufferComponent);

			resource2.GS_GB_HomeBranch = branchForResource2.PK;

			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(config.System);
			var viewModel = sectionAndViewModel.Item2;
			var section = sectionAndViewModel.Item1;

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, "W1", config.Buffer, staffCode: resource1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, "W2", config.Buffer, staffCode: resource2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			AssertRoadRunnerDetail("Should be working based on the default department work time week", resource1, viewModel, RoadRunnerStatus.FullSpeed, workflow1.Tasks.Single());
			AssertRoadRunnerDetail("Should be away based on the different time zone 8 hours behind", resource2, viewModel, RoadRunnerStatus.Away, null);
		}

		[TestDate(2015, 7, 14)]
		public void TestGetRoadRunnerStatus_ShouldUseStandbyStatusOnJob()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(system).Item2;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, ZString.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, capability: capability2);
			var task2 = CreateTask(workflow2, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			jobHeader.FH_IsStandby = true;
			Factory.Save();

			AssertRoadRunnerDetail("Has no current task in any buffer (assigned to a capability resource does not have), and is working on a standby task", resource, viewModel, RoadRunnerStatus.WorkingOnStandbyTask, task2);
		}

		[TestDate(2016, 12, 23)]
		public void TestWorkingOutOfBufferWithNoPendingTasks_StatusShouldShowWorking()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var component = Factory.NewWithValidTestData<BMComponent>();
			LinkComponents(bucket, buffer);

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(system).Item2;
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, component);

			var onBoardTask1 = CreateTask(workflow1, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1);
			var onBoardTask2 = CreateTask(workflow1, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 2);
			var offBoardTask = CreateTask(workflow2, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			Factory.Save();
			AssertRoadRunnerDetail("Resource is not working on any tasks. Status should be 'Stopped'.", staff, viewModel, RoadRunnerStatus.Stopped, null);

			offBoardTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			BMSTestHelper.CacheTasksStartability(viewModel, onBoardTask1, onBoardTask2, offBoardTask);

			AssertRoadRunnerDetail("Resource is working on a task outside the buffer with no current tasks. Status should be 'WorkingInOtherComponent'.", staff, viewModel, RoadRunnerStatus.WorkingInOtherComponent, offBoardTask);
		}

		[TestDate(2016, 12, 23)]
		public void TestWorkingOutOfBufferWithNoPendingTasks_StatusShouldShowAlert()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var component = Factory.NewWithValidTestData<BMComponent>();
			LinkComponents(bucket, buffer);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(buffer, channels: staff).Item2;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, component);

			var onBoardTask1 = CreateTask(workflow1, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1);
			var onBoardTask2 = CreateTask(workflow1, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 2);
			var offBoardTask = CreateTask(workflow2, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			AssertRoadRunnerDetail("Resource is not working on any tasks. Status should be 'Stopped'.", staff, viewModel, RoadRunnerStatus.Stopped, null);

			offBoardTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			onBoardTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			BMSTestHelper.CacheTasksStartability(viewModel, onBoardTask1, onBoardTask2, offBoardTask);

			AssertRoadRunnerDetail("Resource is working on a task outside the buffer while there are current tasks. Status should be 'Alert'.", staff, viewModel, RoadRunnerStatus.Alert, offBoardTask);
		}

		[TestDate(2016, 12, 23)]
		public void TestWorkingOutOfBufferWithNoPendingTasks_AndDifferentReleaseGroup_StatusShouldShowAlert()
		{
			var releaseGroupA = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroupA.GG_Desc = "GroupA";

			var releaseGroupB = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroupB.GG_Desc = "GroupB";

			var system = CreateSystem("ORG");

			var buffer = CreateBuffer(system);
			var linkA = buffer.ReleaseGroupLinks.AddNew();
			linkA.FO_GG_ReleaseGroup = releaseGroupA.PK;

			var bucket = CreateBucket(system);
			var linkB = bucket.ReleaseGroupLinks.AddNew();
			linkB.FO_GG_ReleaseGroup = releaseGroupB.PK;

			LinkComponents(bucket, buffer);

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Groups.Add(releaseGroupA);
			staff.Capabilities.Add(capability);

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(buffer, channels: staff).Item2;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, buffer, releaseGroupPK: releaseGroupB.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, bucket, releaseGroupPK: releaseGroupB.PK);

			var onBoardTask1 = CreateTask(workflow1, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1);
			var onBoardTask2 = CreateTask(workflow1, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 2);
			var offBoardTask = CreateTask(workflow2, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			AssertRoadRunnerDetail("Resource is not working on any tasks. Status should be 'Stopped'.", staff, viewModel, RoadRunnerStatus.Stopped, null);

			offBoardTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			onBoardTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			BMSTestHelper.CacheTasksStartability(viewModel, onBoardTask1, onBoardTask2, offBoardTask);

			AssertRoadRunnerDetail("Resource is working on a task outside the buffer while there are current tasks. Status should be 'Alert'.", staff, viewModel, RoadRunnerStatus.Alert, offBoardTask);
		}

		[TestDate(2019, 1, 1)]
		public void TestCalculateStatus_WhenWorkingOnTaskInBuffer_AndBufferInDifferentSystemFromBoard_ShouldIndicateWorkingStatus()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system1, "Bucket 1");
			var buffer1 = BMSTestHelper.CreateBuffer(system1, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system1, "Buffer 2");
			BMSTestHelper.LinkComponents(bucket1, buffer1);
			BMSTestHelper.LinkComponents(bucket1, buffer2);

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var bucket2 = BMSTestHelper.CreateBucket(system2, "Bucket 2");
			var buffer3 = BMSTestHelper.CreateBuffer(system2, "Buffer 3");
			BMSTestHelper.LinkComponents(bucket2, buffer3);

			var resource1 = BMSTestHelper.CreateStaff(Factory, "AAA");
			var resource2 = BMSTestHelper.CreateStaff(Factory, "BBB");
			var resource3 = BMSTestHelper.CreateStaff(Factory, "CCC");
			var resource4 = BMSTestHelper.CreateStaff(Factory, "DDD");
			var resource5 = BMSTestHelper.CreateStaff(Factory, "EEE");

			var allResources = new[] { resource1, resource2, resource3, resource4, resource5 };
			var allComponents = new[] { bucket1, buffer1, buffer2, bucket2, buffer3 };

			foreach (var tuple in allResources.Zip(allComponents, (res, cmp) => Tuple.Create(res, cmp)))
			{
				// Each resource has a working task in one of the various components.

				BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow with working task", tuple.Item2, staffCode: tuple.Item1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			}

			foreach (var resource in allResources)
			{
				// Each resource also has a startable task in the board section's primary component, which will cause the Alert state to exist for some resources.

				BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow with startable task", buffer3, staffCode: resource.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			}

			var board = BMSTestHelper.CreateBoard(system1);
			var section = BMSTestHelper.CreateBoardSection(buffer3, board);
			BMSTestHelper.CreateAdditionalComponent(section, buffer1);
			BMSTestHelper.CreateAdditionalComponent(section, buffer2);

			foreach (var resource in allResources)
			{
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			}

			Factory.Save();

			AssertRoadRunnerChannelStatus("Resource1 is working in a bucket when a buffer task is startable, so their status is 'alert'.", resource1, section, RoadRunnerStatus.Alert, "Alert");
			AssertRoadRunnerChannelStatus("Resource2 is working in a buffer shown as an additional component on the board section, so their status is 'working'.", resource2, section, RoadRunnerStatus.FullSpeed, "Working");
			AssertRoadRunnerChannelStatus("Resource3 is working in a buffer shown as an additional component on the board section, so their status is 'working'.", resource3, section, RoadRunnerStatus.FullSpeed, "Working");
			AssertRoadRunnerChannelStatus("Resource4 is working in a bucket when a buffer task is startable, so their status is 'alert'.", resource4, section, RoadRunnerStatus.Alert, "Alert");
			AssertRoadRunnerChannelStatus("Resource5 is working in a buffer shown as the primary component on the board section, so their status is 'working'.", resource5, section, RoadRunnerStatus.FullSpeed, "Working");
		}

		[TestDate(2015, 7, 14)]
		public void TestGetRoadRunnerStatus_WhenMultipleResourcesInvolved_ShouldCalculateMultipleIdleTimes()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var section = config.BufferSection;
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflowOnBuffer1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This workflow ensures the resource gets the alert icon", config.Buffer);
			var taskClosedAgesAgo1 = BMSTestHelper.CreateTask(workflowOnBuffer1, resource1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var bufferTask1 = BMSTestHelper.CreateTask(workflowOnBuffer1, resource1.GS_Code);

			var workflowOnBuffer2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This workflow ensures the resource gets the alert icon", config.Buffer);
			var taskClosedAgesAgo2 = BMSTestHelper.CreateTask(workflowOnBuffer2, resource2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var bufferTask2 = BMSTestHelper.CreateTask(workflowOnBuffer2, resource2.GS_Code);

			taskClosedAgesAgo1.CompletedTimeLocal = ZDateTime.Now.AddDays(-10);
			taskClosedAgesAgo2.CompletedTimeLocal = ZDateTime.Now.AddDays(-10);

			var standaloneTask1 = BMSTestHelper.CreateStandaloneTask(Factory, resource1.GS_Code, ProcessTaskStatusCodeList.Codes.Working);
			var standaloneTask2 = BMSTestHelper.CreateStandaloneTask(Factory, resource2.GS_Code, ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			var expectedStatusList = new[]
			{
				(resource1, RoadRunnerStatus.Alert, "Alert"),
				(resource2, RoadRunnerStatus.Alert, "Alert")
			};
			AssertRoadRunnerChannelStatuses("Resources are working on a standalone task, but there is a buffer task pending, so should indicate alert state.", section, expectedStatusList);

			standaloneTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1); // standaloneTask1 has been suspended for 1 minutes
			standaloneTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(4); // standaloneTask1 has been suspended for 5 minutes, standaloneTask2 for 4 minutes

			expectedStatusList = new[]
			{
				(resource1, RoadRunnerStatus.Stopped, "Idle for 5 minutes"),
				(resource2, RoadRunnerStatus.Stopped, "Idle for 4 minutes")
			};
			AssertRoadRunnerChannelStatuses("Resources recently suspended a standalone task, so should indicate idle state and elapsed time since it was suspended.", section, expectedStatusList);

			standaloneTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(7); // standaloneTask2 has been closed for 7 minutes
			standaloneTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5); // standaloneTask1 has been suspended for 5 minutes, standaloneTask2 for 12 minutes

			expectedStatusList = new[]
			{
				(resource1, RoadRunnerStatus.Stopped, "Idle for 5 minutes"),
				(resource2, RoadRunnerStatus.Stopped, "Idle for 12 minutes")
			};
			AssertRoadRunnerChannelStatuses("Resources recently suspended a standalone task, so should indicate idle state and elapsed time since it was suspended.", section, expectedStatusList);
		}

		#endregion

		#region Cache

		[TestDate(2017, 10, 23)]
		public void TestGetRoadRunnerStatus_ShouldUseCache()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, bucket);

			var closedTask = CreateTask(workflow2, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var onBoardTask1 = CreateTask(workflow1, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1);
			var onBoardTask2 = CreateTask(workflow1, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 2);
			var offBoardTask = CreateTask(workflow2, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			closedTask.CompletedTimeLocal = ZDateTime.Now.AddHours(-2);

			var viewModel1 = BMSTestHelper.CreateSectionAndViewModel(buffer).Item2;

			Factory.Save();
			var service = new RoadRunnerStatusCacheService();
			Factory.ServiceContainer.AddService(service);

			AssertEquals("Precondition: Nothing in service cache", 0, service.GetStaffCache_ForTesting(staff).Count);

			var roadRunnerDetails = GetRoadRunnerStatus(staff, viewModel1);

			AssertContainsExactElementsInAnyOrder("Cache is populated", new List<string>() { "GetWorkingTask", "GetClosedTask", "GetSuspendedTask", "GetWorkingTaskInAnyBuffer", "GetRelevantActivityTime", "IsWorkingInThisComponent" + buffer.PK }, service.GetStaffCache_ForTesting(staff).Keys);

			var executedFunc = false;
			var closedTaskPKCached = service.GetOrCacheValue(staff, "GetClosedTask", () => { executedFunc = true; return ZGuid.Empty; });

			Assert("Closed task was cached; func was not executed", !executedFunc);
			AssertEquals("Correct closed task was cached", closedTask.PK, closedTaskPKCached);

			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			roadRunnerDetails = GetRoadRunnerStatus(staff, viewModel1);
			closedTaskPKCached = service.GetOrCacheValue(staff, "GetClosedTask", () => { executedFunc = true; return ZGuid.Empty; });

			Assert("Closed task was still cached; func was not executed", !executedFunc);
			AssertEquals("Previous closed task was cached", closedTask.PK, closedTaskPKCached);
		}

		#endregion

		#region Performance

		[TestDate(2019, 1, 1)]
		public void TestCalculationFunctions_ShouldUseOptimalQueryPlan()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Let's get some stepped up personal space up in this place!!");
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, sequence: 1, description: "Personal Space.");
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, sequence: 2, description: "Personal Space.");
			var task3 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, sequence: 3, description: "Stay out of my personal space!!");
			var task4 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, sequence: 4, description: "Keep away from my personal space!!");
			var task5 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, sequence: 5, description: "Get outta dat personal space!!");
			var task6 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, sequence: 6, description: "Stay away from my personal space!!");
			var task7 = BMSTestHelper.CreateTask(workflow, resource3.GS_Code, sequence: 7, description: "Keep away from dat personal space!!");
			var task8 = BMSTestHelper.CreateTask(workflow, resource3.GS_Code, sequence: 8, description: "Personal Space.");
			var task9 = BMSTestHelper.CreateTask(workflow, resource3.GS_Code, sequence: 9, description: "Personal Space.");

			task1.P9_Status = task2.P9_Status = task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = task5.P9_Status = task6.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task7.P9_Status = task8.P9_Status = task9.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			task1.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-10);
			task2.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-20);
			task3.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-30);

			task4.P9_CompletedTimeUtc = task7.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-10);
			task5.P9_CompletedTimeUtc = task8.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-20);
			task6.P9_CompletedTimeUtc = task9.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-30);

			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				AssertRoadRunnerDetail("Road runner details for resource1", resource1, viewModel, RoadRunnerStatus.Stopped, null);
				AssertRoadRunnerDetail("Road runner details for resource2", resource2, viewModel, RoadRunnerStatus.Stopped, null);
				AssertRoadRunnerDetail("Road runner details for resource3", resource3, viewModel, RoadRunnerStatus.Stopped, null);

				var queries = TestConnection.ExecutedCommandsAndQueryPlans.Where(t => t.Item1.Contains("SELECT TOP 1 P9_PK FROM dbo.ProcessTasks")).ToArray();
				var suspendedTasksQuery = queries.First(t => t.Item1.Contains("'SUS'"));
				var closedTasksQuery = queries.First(t => t.Item1.Contains("'CLS'"));

				var suspendedTasksQueryAnalyzer = new QueryPlanalyzer(suspendedTasksQuery.Item2.Single());
				var closedTasksQueryAnalyzer = new QueryPlanalyzer(closedTasksQuery.Item2.Single());

				AssertQueryPlanOptimalAndIncludesFilteredIndexPredicate(suspendedTasksQueryAnalyzer, suspendedTasksQuery.Item1, ProcessTasksSchema.Constants.P9_SuspendedAt);
				AssertQueryPlanOptimalAndIncludesFilteredIndexPredicate(closedTasksQueryAnalyzer, closedTasksQuery.Item1, ProcessTasksSchema.Constants.P9_CompletedTimeUtc);
			}
		}

		[TestDate(2019, 6, 25)]
		public void TestQueryForOneOrderedRow_ShouldUseTableValuedParameters()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Let's get some stepped up personal space up in this place!!");
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, sequence: 1, description: "Personal Space.");
			var task4 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, sequence: 4, description: "Keep away from my personal space!!");
			var task9 = BMSTestHelper.CreateTask(workflow, resource3.GS_Code, sequence: 9, description: "Personal Space.");

			task1.P9_Status = task4.P9_Status = task9.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task1.P9_CompletedTimeUtc = task4.P9_CompletedTimeUtc = task9.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-30);

			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				RoadRunnerStatusCalculator.GetResourceCodeAndRoadRunnerDetailsDictionary(new[] { resource1, resource2 }, viewModel.ComponentPK, viewModel.Cache, new BusinessObjectFactory());

				var queries = TestConnection.ExecutedCommandsAndQueryPlans.Where(t => t.Item1.Contains("P9_Type <> 'MIL' and P9_Type <> 'TRG' and P9_Type <> 'EXC'")).ToArray();
				AssertContains("Query should make use of a table-valued parameter", "SELECT Value FROM @StaffCodes".ToLower(), queries.First().Item1.ToLower());
			}
		}

		[TestDate(2022, 7, 12)]
		public void TestRelevantActivityTimeForIdleResources()
		{
			TestDateAttribute.UseUNLOCO = true;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Let's get some stepped up personal space up in this place!!");
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, sequence: 1, description: "Task 1");
			var task2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, sequence: 2, description: "Task 2");
			var task3 = BMSTestHelper.CreateTask(workflow, resource3.GS_Code, sequence: 3, description: "Task 3");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task1.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddHours(-3.5);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task2.P9_SuspendedAt = ZDateTime.Now.AddMinutes(-1);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task3.P9_SuspendedAtUtc = ZDateTime.UtcNow.AddMinutes(-10);

			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var dict = RoadRunnerStatusCalculator.GetResourceCodeAndRoadRunnerDetailsDictionary(new[] { resource1, resource2, resource3 }, viewModel.ComponentPK, viewModel.Cache, new BusinessObjectFactory());

				CombineAssertions(() =>
				{
					AssertEquals(new TimeSpan(3, 30, 0), dict[resource1.GS_Code].RelevantActivityTime);
					AssertEquals(new TimeSpan(0, 1, 0), dict[resource2.GS_Code].RelevantActivityTime);
					AssertEquals(new TimeSpan(0, 10, 0), dict[resource3.GS_Code].RelevantActivityTime);
				});
			}
		}

		#endregion

		#region Implementation

		void AssertQueryPlanOptimalAndIncludesFilteredIndexPredicate(QueryPlanalyzer planalyzer, string query, string relevantDateColumnName)
		{
			AssertEquals("Should be no RID lookups to get the columns in the select list only.", 0, planalyzer.RowIDLookups.Count());

			var indexSeek = planalyzer.IndexSeeks.Single(i => i.IndexName.Contains(relevantDateColumnName));
			var predicate = GetIndexFilterPredicate(indexSeek.IndexName)
				.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "") // Don't worry about the extra brackets...
				.Replace("<>'", " <> '").Replace("='", " = '"); // Format the SQL a bit nicer.

			AssertContains("Executed query should use same predicate as filtered index", predicate, query, ignoreCase: true);
		}

		string GetIndexFilterPredicate(string indexName)
		{
			var sql = $"SELECT filter_definition from sys.indexes where name = '{indexName}'";

			return TestConnection.ExecuteScalar<string>(sql);
		}

		static void AssertRoadRunnerDetail(string message, GlbStaff staff, BMBoardSectionViewModel viewModel, RoadRunnerStatus roadRunnerStatus, ProcessTask relevantTask)
		{
			var detail = GetRoadRunnerStatus(staff, viewModel);

			AssertEquals(message, roadRunnerStatus, detail.Status);

			if (relevantTask == null)
			{
				AssertEquals(message + ": Relevant task", ZString.Empty, detail.RelevantTaskDescription);
			}
			else
			{
				AssertEquals(message + ": Relevant task", RoadRunnerStatusCalculator.GetRoadRunnerTaskDescriptor(relevantTask, new BusinessObjectFactory()), detail.RelevantTaskDescription);
			}
		}

		static void AssertRoadRunnerChannelStatus(string assertionMessage, GlbStaff resource, BMBoardSection section, RoadRunnerStatus expectedStatus, string expectedChannelStatus)
		{
			AssertRoadRunnerChannelStatuses(assertionMessage, section, new[] { (resource, expectedStatus, expectedChannelStatus) });
		}

		static void AssertRoadRunnerChannelStatuses(string assertionMessage, BMBoardSection section, (GlbStaff staff, RoadRunnerStatus status, string channelStatus)[] resourceExpectedStatusTuples)
		{
			var viewModel = BMSTestHelper.CreateViewModel(section, populatePropertyCache: true);

			var roadRunnerStatus = GetRoadRunnerStatus(resourceExpectedStatusTuples.Select(tup => tup.staff).ToArray(), viewModel);

			foreach (var (staff, expectedRoadRunnerStatus, expectedChannelStatus) in resourceExpectedStatusTuples)
			{
				var channel = viewModel.PrimaryChannels.Single(c => c.EntityPK == staff.PK);

				CombineAssertions(assertionMessage, () =>
				{
					AssertEquals("Road runner status", expectedRoadRunnerStatus, roadRunnerStatus.Status);
					AssertEquals("Channel status", expectedChannelStatus, channel.Status);
				});
			}
		}

		public static RoadRunnerDetails GetRoadRunnerStatus(GlbStaff resource, BMBoardSectionViewModel viewModel, RoadRunnerTaskCache cache = null)
		{
			return GetRoadRunnerStatus(new[] { resource }, viewModel, cache);
		}

		public static RoadRunnerDetails GetRoadRunnerStatus(GlbStaff[] resources, BMBoardSectionViewModel viewModel, RoadRunnerTaskCache cache = null)
		{
			return RoadRunnerStatusCalculator.GetResourceCodeAndRoadRunnerDetailsDictionary(resources, viewModel.ComponentPK, viewModel.Cache, new BusinessObjectFactory(), cache).Values.First();
		}

		#endregion
	}
}
