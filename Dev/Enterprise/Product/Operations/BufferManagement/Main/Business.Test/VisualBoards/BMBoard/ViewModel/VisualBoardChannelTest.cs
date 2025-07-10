using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class VisualBoardChannelTest : BMSTestCaseWithFactory
	{
		#region Capability

		public void TestVisualBoardChannel_Capability()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var staff1_withCapability = Factory.NewWithValidTestData<GlbStaff>();
			staff1_withCapability.Capabilities.Add(capability);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var task2_requiresCapability = job.WorkflowItems.AddNew();
			task2_requiresCapability.P9_FH_ProcessHeader = workflow.PK;
			task2_requiresCapability.P9_G4_RequiredCapability = capability.PK;
			task2_requiresCapability.P9_GS_NKAssignedStaffMember = ZString.Empty;

			var task3_notRelatedToCapability = job.WorkflowItems.AddNew();
			task3_notRelatedToCapability.P9_FH_ProcessHeader = workflow.PK;
			task3_notRelatedToCapability.P9_G4_RequiredCapability = ZGuid.Empty;
			task3_notRelatedToCapability.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			var tasksInChannel = new[] { task2_requiresCapability };
			var tasksNotInChannel = new[] { task3_notRelatedToCapability };

			var viewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(CreateBucket(CreateSystem())));
			Factory.Save();

			AssertVisualBoardChannel(viewModel.CreateChannelForTest(capability), "Saying Boo-urns", string.Empty, "BOO", ChannelTypeList.Codes.Capability, null, null, tasksInChannel, tasksNotInChannel);
		}

		public void TestVisualBoardChannel_Capability_WhenHidingResourceTasks()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.HideResourceTasksFromCapabilityChannels = true;

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, string.Empty, 90, capability: capability);
			var task2_withCapabilityAndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability);
			var task3_withResourceOnly = CreateTask(workflow, resource.GS_Code, 90);

			var tasksInChannel = new[] { task1_withCapabilityOnly };
			var tasksNotInChannel = new[] { task2_withCapabilityAndResource, task3_withResourceOnly };
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertVisualBoardChannel(viewModel.GetOrCreateChannelForTest(channel, section.Factory), "Saying Boo-urns", string.Empty, "BOO", ChannelTypeList.Codes.Capability, null, null, tasksInChannel, tasksNotInChannel);
		}

		public void TestVisualBoardChannel_UpdatedStaffCapability()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;
			section.SectionConfiguration.HideCapabilityTasksFromResourceChannels = true;
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource = CreateStaffInCurrentBranchDept("YDD", "Yawn Doodle");

			var task = Factory.New<ProcessTask>();
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_Description = "standaloneTask";
			task.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(resource);

			var newFactory = new BusinessObjectFactory();
			capability = newFactory.Load<GlbCapability>(capability.PK);
			resource = newFactory.Load<GlbStaff>(resource.PK);
			AssertEquals("Task should not be in channel", false, channel.IsInChannel(task, false));
			resource.Capabilities.Add(capability);
			newFactory.Save();
			channel.SeedCacheWithChannelMatcherTest(newFactory);
			AssertEquals("Now the task should appear in channel", true, channel.IsInChannel(task, false));
		}

		#endregion

		#region Group

		public void TestVisualBoardChannel_Group()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Fellowship of the Ring";
			group.GG_Code = "FOTR";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1_forReleaseGroup = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.Add(staff1);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var task1_workflowIsForReleaseGroup = job.WorkflowItems.AddNew();
			task1_workflowIsForReleaseGroup.P9_FH_ProcessHeader = workflow1_forReleaseGroup.PK;
			workflow1_forReleaseGroup.FH_GG_ReleaseGroup = group.PK;

			var task2_assignedToGroup = job.WorkflowItems.AddNew();
			task2_assignedToGroup.P9_FH_ProcessHeader = workflow2.PK;
			task2_assignedToGroup.P9_GG_AssignedGroup = group.PK;
			task2_assignedToGroup.P9_GS_NKAssignedStaffMember = ZString.Empty;

			var task3_resourceIsInGroup = job.WorkflowItems.AddNew();
			task3_resourceIsInGroup.P9_FH_ProcessHeader = workflow2.PK;
			task3_resourceIsInGroup.P9_GG_AssignedGroup = ZGuid.Empty;
			task3_resourceIsInGroup.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			var task4_notInGroup = job.WorkflowItems.AddNew();
			task4_notInGroup.P9_FH_ProcessHeader = workflow2.PK;
			task4_notInGroup.P9_GG_AssignedGroup = ZGuid.Empty;
			task4_notInGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			var tasksInChannel = new[] { task1_workflowIsForReleaseGroup, task2_assignedToGroup, task3_resourceIsInGroup };
			var tasksNotInChannel = new[] { task4_notInGroup };
			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			AssertVisualBoardChannel(viewModel.CreateChannelForTest(group), "Fellowship of the Ring", string.Empty, "FOTR", ChannelTypeList.Codes.Group, null, null, tasksInChannel, tasksNotInChannel);
		}

		#endregion

		#region Resource
		[TestDate(2023, 5, 30)]
		public void TestStatusImage_ShouldBeCached()
		{
			var system = CreateSystem(WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var standaloneTask = Factory.New<ProcessTask>();
			standaloneTask.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			standaloneTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			standaloneTask.P9_Description = "standaloneTask";

			var workflow = CreateJobHeader<DummyWithWorkflow>().ProcessHeaders[0];
			var bufferTask = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var roadRunnerStatusIconCacheProperty = typeof(ResourceChannel).GetProperty("RoadRunnerStatusIconCache", BindingFlags.NonPublic | BindingFlags.Static);
			var cache = roadRunnerStatusIconCacheProperty.GetValue(null) as MemoryCache;
			cache.ForEach(c => cache.Remove(c.Key));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(resource);

			AssertEquals(expected: false, cache.Contains("resource_alert"));

			BMSTestHelper.CacheTasksStartability(viewModel, standaloneTask, bufferTask);
			var statusImage = channel.StatusImage;

			AssertContains("ALERT", statusImage.ImageTooltip.ToString());
			Assert(cache.Contains("resource_alert"));
		}

		[TestDate(2015, 7, 14)]
		public void TestStatusImage_WhenWorkingOnStandaloneTask_ShouldNotThrowException()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var standaloneTask = Factory.New<ProcessTask>();
			standaloneTask.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			standaloneTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			standaloneTask.P9_Description = "standaloneTask";

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var bufferTask = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(resource);

			BMSTestHelper.CacheTasksStartability(viewModel, standaloneTask, bufferTask);

			var statusImage = channel.StatusImage;

			AssertMultilineASCIIEquals("",
@"ALERT: there is a buffer task pending, but the resource is working on a task outside the buffer.
Working task: T00001000: standaloneTask
Click to open the task's job.
".StripTaskIds(), statusImage.ImageTooltip.ToString().StripTaskIds());
		}

		[TestDate(2014, 4, 25)]
		public void TestStatus_WhenInPreview_ShouldBeWorking()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			group.Staff.Add(resource);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			resource.DesignateAsCCR(buffer);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource.GS_Code, 60);
			var task2 = CreateTask(workflow, resource.GS_Code, 60);
			var task3 = CreateTask(workflow, string.Empty, 60, capability: capability);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedResource = newFactory.Load<GlbStaff>(resource.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedSection, isPreview: true);
			var channel = viewModel.CreateChannelForTest(loadedResource);

			var status = channel.Status;
			var statusImage = channel.StatusImage;

			CombineAssertions("Should not check real status in preview - no point hitting the db just for this", () =>
			{
				foreach (var table in TablesNotAllowedToBeHitInPreview)
				{
					AssertEquals(table + " hit count", 0, newFactory.GetTableHitCount(table));
				}
			});

			AssertEquals("Should just assume the resource is working for preview", "Working", status);
			AssertEquals("Don't bother with a road runner graphic in preview", default(ImageWithTooltip), statusImage);
		}

		[TestDate(2014, 4, 25)]
		public void TestStatus_WhenInPreview_WithHighRisk_ShouldNotHitDB()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resourceCCR = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var resourceNonCCR = CreateStaffInCurrentBranchDept("Bil", "Bilbo Baggins", capability);
			group.Staff.Add(resourceCCR);
			group.Staff.Add(resourceNonCCR);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			resourceCCR.DesignateAsCCR(buffer);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var taskPostConstraint = CreateTask(workflow, resourceNonCCR.GS_Code, lowEstMinutes: 60, sequence: 400, description: "Task post Constraint");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR.PK, overrideChannels: true);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedResource = newFactory.Load<GlbStaff>(resourceCCR.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedSection, isPreview: true);
			var channel = viewModel.CreateChannelForTest(loadedResource);

			CombineAssertions("no point hitting the db just for preview", () =>
			{
				foreach (var table in TablesNotAllowedToBeHitInPreview)
				{
					AssertEquals(table + " hit count", 0, newFactory.GetTableHitCount(table));
				}
			});
		}

		[TestDate(2014, 4, 25, 2, 30, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestStatus_WhenHolidayExistsDuringDay()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			resource.DesignateAsCCR(bucket);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedResource = newFactory.Load<GlbStaff>(resource.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedSection);
			var channel = viewModel.CreateChannelForTest(loadedResource);

			AssertEquals("Idle", channel.Status);

			var holiday = loadedResource.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = new ZDateTime(2014, 4, 25, 11, 30, 0);
			holiday.GA_EndTime = new ZDateTime(2014, 4, 25, 13, 30, 0);
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = "ANN";

			newFactory.Save();
			channel.ClearChannelCache();

			AssertEquals("Away until Fri 25-Apr", channel.Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(61);

			channel.ClearChannelCache();

			AssertEquals("Idle", channel.Status);
		}

		[TestDate(2015, 8, 8, 17, 59, 0)]
		public void TestStatus_WhenOnANormalWeekend()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			resource.DesignateAsCCR(bucket);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedResource = newFactory.Load<GlbStaff>(resource.PK);

			var viewModel = BMSTestHelper.CreateViewModel(loadedSection);
			var channel = viewModel.CreateChannelForTest(loadedResource);

			AssertEquals("Away until Mon 10-Aug", channel.Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			channel.ClearChannelCache();

			AssertEquals("Away until Mon 10-Aug", channel.Status);
		}

		public static string[] TablesNotAllowedToBeHitInPreview => new[]
		{
			BMComponentResourceLinkSchema.Constants.TableName,
			GlbCapabilitySchema.Constants.TableName,
			GlbResourceCapabilityPivotSchema.Constants.TableName,
			GlbStaffHolidaySchema.Constants.TableName,
			ProcessTasksSchema.Constants.TableName,
			StmModuleFilterSchema.Constants.TableName,
		};

		[TestDate(2013, 5, 24, 9, 0, 0)]
		public void TestIVisualBoardChannel_Resource()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Frodo Baggins";
			staff1.GS_FriendlyName = "Frodo";
			staff1.GS_Code = "FRO";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Samwise Gamgee";
			staff2.GS_FriendlyName = "Sam";
			staff2.GS_Code = "SAM";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			staff1.Capabilities.Add(capability1);
			staff2.Capabilities.Add(capability2);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var task1_resource1 = job.WorkflowItems.AddNew();
			var task2_resource2 = job.WorkflowItems.AddNew();
			var task3_capability1 = job.WorkflowItems.AddNew();
			var task4_capability2 = job.WorkflowItems.AddNew();
			var task5_resource1AndCapability1 = job.WorkflowItems.AddNew();
			var task6_resource2AndCapability1 = job.WorkflowItems.AddNew();

			task1_resource1.P9_GS_NKAssignedStaffMember = task5_resource1AndCapability1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task2_resource2.P9_GS_NKAssignedStaffMember = task6_resource2AndCapability1.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			task3_capability1.P9_G4_RequiredCapability = task5_resource1AndCapability1.P9_G4_RequiredCapability = task6_resource2AndCapability1.P9_G4_RequiredCapability = capability1.PK;
			task4_capability2.P9_G4_RequiredCapability = capability2.PK;
			var section = CreateBoardSection(CreateBucket(CreateSystem()));
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var staff1Channel = viewModel.CreateChannelForTest(staff1);
			var staff2Channel = viewModel.CreateChannelForTest(staff2);

			AssertVisualBoardChannel(staff1Channel, "Frodo Baggins", "Frodo", "FRO", ChannelTypeList.Codes.Resource, null, "Idle",
				new[] { task1_resource1, task3_capability1, task5_resource1AndCapability1 },
				new[] { task2_resource2, task4_capability2, task6_resource2AndCapability1 });

			AssertVisualBoardChannel(staff2Channel, "Samwise Gamgee", "Sam", "SAM", ChannelTypeList.Codes.Resource, null, "Idle",
				new[] { task2_resource2, task4_capability2, task6_resource2AndCapability1 },
				new[] { task1_resource1, task3_capability1, task5_resource1AndCapability1 });
		}

		public void TestStaffChannelWithUnChanneled_ShouldNotThrowException()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;
			section.SectionConfiguration.HideCapabilityTasksFromResourceChannels = true;
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.Capabilities.Add(capability);

			var task = Factory.New<ProcessTask>();
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_Description = "standaloneTask";

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(resource);

			AssertNoExceptionThrown(() => channel.IsInChannel(task, false));
		}

		public void TestStaffChannel_ShouldNotHitDB_WhenGetStaffImage()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.ProfileImage = new Bitmap(1024, 768);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.ProfileImage = new Bitmap(800, 600);

			group.Staff.Add(staff1);
			group.Staff.Add(staff2);

			section.MS_GG_ReleaseGroup = group.PK;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newSection = newFactory.Load<BMBoardSection>(section.PK);
			using (TestConnection.TrackExecutedCommands())
			{
				var viewModel = BMSTestHelper.CreateViewModel(newSection);
				var staff1Channel = viewModel.CreateChannelForTest(staff1);
				var staff2Channel = viewModel.CreateChannelForTest(staff2);

				AssertEquals(staff1Channel.DisplayImage, staff1Channel.DisplayImage);
				AssertEquals(staff2Channel.DisplayImage, staff2Channel.DisplayImage);

				var glbStaffDbHits = newFactory.GetTableHitCount(GlbStaffSchema.Constants.TableName);
				var profilePhotoQueries = TestConnection.ExecutedCommands.Where(q => q.StartsWith("SELECT CAST(DATALENGTH([GS_ProfilePhoto]) AS BIGINT) AS [DataLength]")).ToArray();

				CombineAssertions(() =>
				{
					AssertEquals(1, glbStaffDbHits);
					AssertEquals(0, profilePhotoQueries.Length);
				});
			}
		}

		#region Road Runner

		public void TestIVisualBoardChannel_Resource_StatusImage_BucketComponent()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
			var task2 = CreateTask(workflow2, resource.GS_Code, 60, description: "task2");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(bucket));
			var channel = viewModel.CreateChannelForTest(resource);

			AssertRoadRunnerDetail("Has a working task in a buffer, but is a bucket component, so don't show the graphic", channel, null, null);
		}

		[TestDate(2015, 7, 14)]
		public void TestIVisualBoardChannel_Resource_StatusImage_BufferComponent()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, staff.GS_Code, 60, description: "task1");
			var task2 = CreateTask(workflow2, staff.GS_Code, 60, description: "task2");
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(buffer), useStatusCache: false);
			var channel = viewModel.CreateChannelForTest(staff);

			AssertRoadRunnerDetail("Has a working task in a buffer", channel, Properties.Resources.resource_active, "Full Speed: Working on a task within a buffer");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			channel.ClearCacheAndReload();

			AssertRoadRunnerDetail("Has no working tasks", channel, Properties.Resources.resource_inactive, "Stopped: Not working on any task");

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			channel.ClearCacheAndReload();
			BMSTestHelper.CacheTasksStartability(viewModel, task1);

			AssertRoadRunnerDetail("Has a current task in a buffer, and is working on a task outside the buffer", channel, Properties.Resources.resource_alert,
@"ALERT: there is a buffer task pending, but the resource is working on a task outside the buffer.
Working task:  (MAIORGSYD), task2
In component: bucket
Click to open the task's job.
");

			task1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task1.P9_G4_RequiredCapability = capability1.PK;
			Factory.Save();
			channel.ClearCacheAndReload();
			BMSTestHelper.CacheTasksStartability(viewModel, task1);

			AssertRoadRunnerDetail("Has a current task with a capability possessed by the resource in a buffer, and is working on a task outside the buffer", channel, Properties.Resources.resource_alert,
@"ALERT: there is a buffer task pending, but the resource is working on a task outside the buffer.
Working task:  (MAIORGSYD), task2
In component: bucket
Click to open the task's job.
");

			task1.P9_G4_RequiredCapability = capability2.PK;
			workflow2.FH_IsStandby = true;
			Factory.Save();
			channel.ClearCacheAndReload();
			BMSTestHelper.CacheTasksStartability(viewModel, task1);

			AssertRoadRunnerDetail("Has no current task in any buffer (assigned to a capability resource does not have), and is working on a standby task", channel, Properties.Resources.resource_inactive,
@"Working on Standby Task. A standby task is a task with a workflow or a job marked as 'Standby'
Working task:  (MAIORGSYD), task2
In component: bucket");

			workflow2.FH_IsStandby = false;
			Factory.Save();
			channel.ClearCacheAndReload();

			AssertRoadRunnerDetail("Has no current task in any buffer, and is working on a non-standby task", channel, Properties.Resources.resource_active,
@"Working on a task outside the buffer
Working task:  (MAIORGSYD), task2
In component: bucket
Click to open the task's job.
");

			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.UtcToday.AddDays(-1), ZDateTime.UtcToday.AddDays(1), staff.PK);
			Factory.Save();

			staff.HolidaysIncBMSLeave.Load();
			AssertEquals(1, staff.HolidaysIncBMSLeave.Count);
			AssertEquals(false, staff.IsWorkingRightNow);

			channel.ClearCacheAndReload();
			AssertRoadRunnerDetail("Resource is on leave", channel, Properties.Resources.resource_away, "Away");

			staff.GS_IsActive = false;
			Factory.Save();

			channel.ClearCacheAndReload();
			AssertRoadRunnerDetail("Resource is inactive", channel, null, null);
		}

		static void AssertRoadRunnerDetail(string message, IVisualBoardChannel channel, Bitmap expectedStatusImage, ZString expectedStatusImageTooltip)
		{
			if (expectedStatusImage == null)
			{
				AssertNull(channel.StatusImage.Image);
			}
			else
			{
				AssertImagePixelsEqual(message + ": StatusImage", expectedStatusImage, (Bitmap)channel.StatusImage.Image);
			}

			AssertMultilineASCIIEquals(message + ": StatusImageTooltip", expectedStatusImageTooltip, channel.StatusImage.ImageTooltip);
		}

		[TestDate(2015, 7, 14)]
		public void TestWorkingOnNonBufferTask_WhenStartableCapabilityTaskClaimedBySomeoneElse_ShouldNotShowAlert()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var capability = BMSTestHelper.CreateCapability(Factory, "VOT", "Having your say");
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "YES", "Good person", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NO", "Bad person", capability);

			var workflowInBuffer = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Postal survey", config.Buffer);
			var workflowInBucket = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Not posting survey", config.Bucket);

			var capabilityTask = BMSTestHelper.CreateTask(workflowInBuffer, resource1.GS_Code, capability: capability);

			var badTask1 = BMSTestHelper.CreateTask(workflowInBucket, resource1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			var badTask2 = BMSTestHelper.CreateTask(workflowInBucket, resource2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, populatePropertyCache: true);

			AssertEquals("Pre-condition: viewmodel cache should consider capabilityTask as startable", true, viewModel.Cache.GetCachedValue<bool>(capabilityTask.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent));

			AssertEquals("Alert", viewModel.CreateChannelForTest(resource1).Status);
			AssertEquals("Working", viewModel.CreateChannelForTest(resource2).Status);

			capabilityTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section, populatePropertyCache: true);

			AssertEquals("Alert", viewModel.CreateChannelForTest(resource1).Status);
			AssertEquals("Alert", viewModel.CreateChannelForTest(resource2).Status);
		}

		#endregion

		#region Capability Tasks

		[TestDate(2013, 12, 15)]
		public void TestVisualBoardChannel_Resource_WhenHidingCapabilityTasks()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.HideCapabilityTasksFromResourceChannels = true;

			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.G4_Code = "BOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			resource.GS_FriendlyName = "Frodo";

			var resourceChannel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var capabilityChannel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapabilityOnly = CreateTask(workflow, string.Empty, 90, capability: capability);
			var task2_withCapabilityAndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability);
			var task3_withResourceOnly = CreateTask(workflow, resource.GS_Code, 90);

			var tasksInChannel = new[] { task2_withCapabilityAndResource, task3_withResourceOnly };
			var tasksNotInChannel = new[] { task1_withCapabilityOnly };

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertVisualBoardChannel(viewModel.GetOrCreateChannelForTest(resourceChannel, Factory), "Frodo Baggins", "Frodo", "FRO", ChannelTypeList.Codes.Resource, null, "Away until Mon 16-Dec", tasksInChannel, tasksNotInChannel);
		}

		[TestDate(2013, 12, 15)]
		public void TestVisualBoardChannel_Resource_WhenHidingCapabilityTasks_ButCapabilityIsNotShownAsAChannel()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.HideCapabilityTasksFromResourceChannels = true;

			var capability1 = Factory.New<GlbCapability>();
			capability1.G4_Description = "Saying Boo-urns";
			capability1.G4_Code = "BOO";

			var capability2 = Factory.New<GlbCapability>();
			capability2.G4_Description = "Reciting the Alphabet";
			capability2.G4_Code = "ABC";

			var capability3 = Factory.New<GlbCapability>();
			capability3.G4_Description = "Foobarring";
			capability3.G4_Code = "FOO";

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1, capability2);
			resource.GS_FriendlyName = "Frodo";

			var resourceChannel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var capability1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability1.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1_withCapability1Only = CreateTask(workflow, string.Empty, 90, capability: capability1, description: "task1_withCapability1Only");
			var task2_withCapability2Only = CreateTask(workflow, string.Empty, 90, capability: capability2, description: "task2_withCapability2Only");
			var task3_withCapability3Only = CreateTask(workflow, string.Empty, 90, capability: capability3, description: "task3_withCapability3Only");
			var task4_withCapability1AndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability1, description: "task3_withCapability1AndResource");
			var task5_withCapability2AndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability2, description: "task4_withCapability2AndResource");
			var task6_withCapability3AndResource = CreateTask(workflow, resource.GS_Code, 90, capability: capability3, description: "task6_withCapability3AndResource");

			var tasksInResourceChannel = new[] { task2_withCapability2Only, task4_withCapability1AndResource, task5_withCapability2AndResource, task6_withCapability3AndResource };
			var tasksNotInResourceChannel = new[] { task1_withCapability1Only, task3_withCapability3Only };

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertVisualBoardChannel(viewModel.GetOrCreateChannelForTest(resourceChannel, Factory), "Frodo Baggins", "Frodo", "FRO", ChannelTypeList.Codes.Resource, null, "Away until Mon 16-Dec", tasksInResourceChannel, tasksNotInResourceChannel);
		}

		public void TestVisualBoardChannel_ResourceWithCapability_WhenNotMemberOfWorkflowReleaseGroup_ShouldNotShownInChannel()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			CreateReleaseGroup(system, releaseGroup);
			var capability = CreateCapability("CPT", "Capability");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			releaseGroup.Staff.Add(staff1);
			var staff2 = CreateStaffInCurrentBranchDept("RE2", "resource2", capability);
			var staff3 = CreateStaffInCurrentBranchDept("RE3", "resource3");

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "wf1", currentComponent: buffer, releaseGroupPK: releaseGroup.PK);
			var task = CreateTask(workflow, string.Empty, 90, capability: capability, description: "this is a task");

			var staff1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			var staff2Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);
			var staff3Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff3.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var staff1VisualChannel = viewModel.CreateChannelForTest(staff1Channel, Factory);
			var staff2VisualChannel = viewModel.CreateChannelForTest(staff2Channel, Factory);
			var staff3VisualChannel = viewModel.CreateChannelForTest(staff3Channel, Factory);

			AssertEquals("Task should be in staff1 channel", true, staff1VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should not be in staff2 channel", false, staff2VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should not be in staff3", false, staff3VisualChannel.IsInChannel(task, false));

			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			staff1VisualChannel = viewModel.CreateChannelForTest(staff1Channel, Factory);
			staff2VisualChannel = viewModel.CreateChannelForTest(staff2Channel, Factory);
			staff3VisualChannel = viewModel.CreateChannelForTest(staff3Channel, Factory);

			AssertEquals("Task should be in staff1 channel", true, staff1VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should be in staff2 channel", true, staff2VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should not be in staff3 channel", false, staff3VisualChannel.IsInChannel(task, false));
		}

		public void TestVisualBoardChannel_ResourceWithCapability_WhenNotMemberOfTaskGroup_ShouldNotShownInChannel()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = CreateCapability("CPT", "Capability");
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			CreateReleaseGroup(system, releaseGroup);
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			group.Staff.Add(staff1);
			var staff2 = CreateStaffInCurrentBranchDept("RE2", "resource2", capability);
			releaseGroup.Staff.Add(staff2);
			var staff3 = CreateStaffInCurrentBranchDept("RE3", "resource3");

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "wf1", currentComponent: buffer, releaseGroupPK: releaseGroup.PK);
			var task = CreateTask(workflow, string.Empty, 90, capability: capability, description: "this is a task");
			task.P9_GG_AssignedGroup = group.PK;

			var staff1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			var staff2Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);
			var staff3Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff3.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var staff1VisualChannel = viewModel.CreateChannelForTest(staff1Channel, Factory);
			var staff2VisualChannel = viewModel.CreateChannelForTest(staff2Channel, Factory);
			var staff3VisualChannel = viewModel.CreateChannelForTest(staff3Channel, Factory);

			AssertEquals("Task should be in staff1 channel", true, staff1VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should not be in staff2 channel", false, staff2VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should not be in staff3 channel", false, staff3VisualChannel.IsInChannel(task, false));

			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			staff1VisualChannel = viewModel.CreateChannelForTest(staff1Channel, Factory);
			staff2VisualChannel = viewModel.CreateChannelForTest(staff2Channel, Factory);
			staff3VisualChannel = viewModel.CreateChannelForTest(staff3Channel, Factory);

			AssertEquals("Task should be in staff1 channel", true, staff1VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should be in staff2 channel", true, staff2VisualChannel.IsInChannel(task, false));
			AssertEquals("Task should not be in staff3 channel", false, staff3VisualChannel.IsInChannel(task, false));
		}

		public void TestVisualBoardChannel_ResourceWithCapability_WhenBoardConfigurationShowWorkInReleaseGroupOnly_ShouldShowTasksWithSameGroup()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var capability = CreateCapability("CAP", "Capability");
			var boardReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			CreateReleaseGroup(system, boardReleaseGroup);
			var otherReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			CreateReleaseGroup(system, otherReleaseGroup);

			section.SectionConfiguration.ReleaseGroupPK = boardReleaseGroup.PK;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;

			var staff1 = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "wf1", currentComponent: buffer, releaseGroupPK: otherReleaseGroup.PK);
			var task = CreateTask(workflow, string.Empty, 90, capability: capability, description: "this is a task");
			task.P9_GG_AssignedGroup = boardReleaseGroup.PK;

			var staff1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var staff1VisualChannel = viewModel.CreateChannelForTest(staff1Channel, Factory);

			AssertEquals("Task should be in staff1 channel", true, staff1VisualChannel.IsInChannel(task, true));
		}

		#endregion

		#region Status

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Overloaded_BucketSection()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var section = CreateBoardSection(bucket);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * 1.3), taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestZoneStatus_ForNonCCR_WithWorstSubcomponenZone()
		{
			var channel = CreateTestVisualBoard(0, -365);
			AssertEquals("Zone number shown must be the worst between primary-component and sub-components", "Zone 0 (Post), Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestZoneStatus_ForNonCCR_WithWorstPrimarycomponentZone()
		{
			var channel = CreateTestVisualBoard(-365, 0);
			AssertEquals("If Primary component zone is the worst, then don't display sub-component name", "Zone 0, Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestZoneStatus_ForNonCCR_ShowingSubComponent_LowestOffset()
		{
			var channel = CreateTestVisualBoard(-365, -365);
			AssertEquals("If all components' zone is 1 or 0, then show the sub-component name with the lowest Offset (pre-constraint buffer)", "Zone 0 (Pre), Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestZoneStatus_ForNonCCR_NotShowingSubComponent_AllInZone2Or3()
		{
			var channel = CreateTestVisualBoard(-5, -5);
			AssertEquals("GIVEN Channel with SubComponent zone = 2 and PrimaryComponent zone = 3, WHEN displaying channel header SHOULD show PrimaryComponent zone because SubComponent is not at risk (zone > 1)", "Zone 3, Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestZoneStatus_ForNonCCR_ShowingSubComponent_SubComponentHasWorstZone()
		{
			var channel = CreateTestVisualBoard(-5, -365);
			AssertEquals("If worst case is a sub-component, show its name in parentheses (all components' zone must not be in zone 2 and 3)", "Zone 0 (Post), Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestNonCCRZoneStatus_Constraint()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			Factory.Save();
			var viewModel = config.SectionViewModel;

			AssertEquals("workflow with CCR-task as current must penetrate constraint-buffer", "buffer - Zone 3\r\n\tbufferConstraint", config.Workflows[0].CurrentStatus);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestNonCCRZoneStatus_PostConstraint()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			var ccrTask = config.Workflows[0].Parent.WorkflowItems[0];
			ccrTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ccrTask.P9_Sequence = 1;
			var postCCRTask = BMSTestHelper.CreateTask(config.Workflows[0], "NC1", sequence: 2);
			Factory.Save();

			var viewModel = config.SectionViewModel;

			AssertEquals("Workflow with an open post-constraint NON-CCR-task as current-task AND closed pre-constraint-CCR-task, must not penetrate constraint-buffer", "buffer - Zone 3\r\n\tPost-Constraint - Zone 3", config.Workflows[0].CurrentStatus);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 31)]
		public void TestNonCCRChannelZone()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			var nonCCR1workflow = config.Workflows[1];
			nonCCR1workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-6);

			var preCCRTask = nonCCR1workflow.Parent.WorkflowItems[0];
			preCCRTask.P9_Sequence = 1;
			var ccrTask = BMSTestHelper.CreateTask(nonCCR1workflow, "CCR", sequence: 2);

			Factory.Save();
			var viewModel = config.SectionViewModel;

			var section = config.Section;
			var nonCCRChannel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && !c.Channel.IsCCRChannel(section)).Channel;

			var grid = viewModel.ComponentGrid;
			var nonCCRChannelZone = grid.GetChannelZone(nonCCRChannel, section, viewModel.ComponentGrid.CardAllocationMap);
			AssertEquals("Non CCR Channel zone must be the worst from primary and sub buffers", 1, nonCCRChannelZone);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_HighRiskStateSequence_FlowDirection_Down()
		{
			CreateAndAssertStatus_HighRiskStateSequence(FlowDirectionList.Codes.Down);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_HighRiskStateSequence_FlowDirection_Up()
		{
			CreateAndAssertStatus_HighRiskStateSequence(FlowDirectionList.Codes.Up);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_HighRiskStateSequence_FlowDirection_Right()
		{
			CreateAndAssertStatus_HighRiskStateSequence(FlowDirectionList.Codes.Right);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_HighRiskStateSequence_FlowDirection_Left()
		{
			CreateAndAssertStatus_HighRiskStateSequence(FlowDirectionList.Codes.Left);
		}

		void CreateAndAssertStatus_HighRiskStateSequence(ZString flowDirection)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			config.ReleaseGroup.Staff.AddRange(config.CCR, config.NonCCR1, config.NonCCR2);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = flowDirection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "You Better Werqflow", config.Buffer, releaseDateTime: ZDateTime.UtcNow.AddDays(-12));
			var ccrTask1 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, lowEstMinutes: 60, sequence: 20, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Workflow 1: CCR Task");
			var postCCRTask1 = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, lowEstMinutes: 2000, sequence: 30, description: "Workflow 1: Post CCR Task");

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "No, she done already done had her'ses", config.Buffer);
			var ccrTask2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60 * 8 * 2, description: "Workflow 2: CCR Task");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;

			RefreshChannelHeading(channel, viewModel, section, jobHeader);
			AssertEquals("Idle", channel.Status);

			var startDate = new ZDateTime(2014, 1, 28);
			var endDate = new ZDateTime(2014, 1, 29);
			var holiday = CreateHoliday(Factory, config.CCR, channel, startDate, endDate, "ANN");
			Factory.Save();
			RefreshChannelHeading(channel, viewModel, section, jobHeader);
			AssertEquals("post-CCR-tasks should not impact CCR status", "Away until Wed 29-Jan", channel.Status);
			holiday.Delete();
			Factory.Save();

			var preCCRTask1 = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60 * 8 * 2, sequence: 10, description: "Workflow 1: Pre CCR Task");
			ccrTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			RefreshChannelHeading(channel, viewModel, section, jobHeader);
			AssertEquals("Idle", channel.Status);

			ccrTask2.P9_EstDuration = new ZInt(8 * 60).GetDateTimeFromMinutes(); // 1 day so CCR n-th % is in zone 3
			Factory.Save();

			var ccrTaskPairs = viewModel.ComponentGrid.CardAllocationMap.CardsByCell_ForTest
				.Where(pair => pair.Value.Any(card => card.CapacityDto.AssignedResourceCode == "CCR"));
			CombineAssertions("GIVEN 2 CCR tasks task1 (not current, in zone 0) and task2 (current, in zone 3)", () =>
			{
				AssertEquals("CCR total tasks", 2, ccrTaskPairs.SelectMany(pair => pair.Value).Count());

				var task1Pair = ccrTaskPairs.Single(pair => pair.Value.Any(card => card.TaskIdentifier == ccrTask1.PK));
				AssertEquals("task1 zone", 0, task1Pair.Key.Zone);
				AssertEquals("task IsCurrent", false, task1Pair.Value.Single().IsCurrent);

				var task2Pair = ccrTaskPairs.Single(pair => pair.Value.Any(card => card.TaskIdentifier == ccrTask2.PK));
				AssertEquals("task2 zone", 3, task2Pair.Key.Zone);
				AssertEquals("task IsCurrent", true, task2Pair.Value.Single().IsCurrent);
			});

			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Up:
				case FlowDirectionList.Codes.Left:
					AssertCCRChannelFadeCell(viewModel, 13);
					break;
				case FlowDirectionList.Codes.Down:
				case FlowDirectionList.Codes.Right:
					AssertCCRChannelFadeCell(viewModel, 1);
					break;
			}

			RefreshChannelHeading(channel, viewModel, section, jobHeader);
			AssertEquals(
				"WHEN CCR n-th % is outside target zone (lower in buffer penetration) and not enough work THEN it should show not enough work status",
				"Not enough work, Idle",
				channel.Status);

			holiday = CreateHoliday(Factory, config.CCR, channel, startDate, endDate, "ANN");
			Factory.Save();
			RefreshChannelHeading(channel, viewModel, section, jobHeader);
			AssertEquals("pre-CCR-tasks should not impact CCR status", "Away until Wed 29-Jan", channel.Status);
			holiday.Delete();
			Factory.Save();

			ccrTask2.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			Factory.Save();

			RefreshChannelHeading(channel, viewModel, section, jobHeader);

			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Up:
				case FlowDirectionList.Codes.Left:
					AssertCCRChannelFadeCell(viewModel, 1);
					break;
				case FlowDirectionList.Codes.Down:
				case FlowDirectionList.Codes.Right:
					AssertCCRChannelFadeCell(viewModel, 13);
					break;
			}

			AssertEquals(
				"WHEN CCR n-th % is outside target zone (higher in buffer penetration) THEN should show outside target zone status",
				"Outside target zone, Idle",
				channel.Status);

			ccrTask2.P9_EstDuration = new ZInt(60 * 8 * 10).GetDateTimeFromMinutes();
			Factory.Save();

			RefreshChannelHeading(channel, viewModel, section, jobHeader);
			AssertEquals("Overloaded, Idle", channel.Status);
		}

		static void AssertCCRChannelFadeCell(BMBoardSectionViewModel viewModel, int secondaryAxis)
		{
			CellContent ccrChannelNthPercentageFadeCell = viewModel.ComponentGrid.Cells.Single(cell => cell.PrimaryAxis == 2 && cell.SecondaryAxis == secondaryAxis);

			CombineAssertions("CCR channel fade cell", () =>
			{
				AssertEquals("CCR channel", "CCR", ccrChannelNthPercentageFadeCell.Channel.ChannelEntityCode);
				AssertEquals("Cell Content Type", CellContentType.Cards, ccrChannelNthPercentageFadeCell.ContentType);
				AssertNotNull("Fade Color", ccrChannelNthPercentageFadeCell.BackgroundFadeColor);
			});
		}

		internal static void RefreshChannelHeading(IVisualBoardChannel channel, BMBoardSectionViewModel viewModel, BMBoardSection section, ProcessJobHeader jobHeader)
		{
			BufferCapacityCache.Clear();
			channel.ClearCacheAndReload();
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, jobHeader.Parent.WorkflowItems.Cast<ProcessTask>().ToArray());
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_HighRiskShortQueue()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			group.Staff.AddRange(resourceCCR1);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK, overrideChannels: true);
			resourceCCR1.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Now, staffCode: resourceCCR1.GS_Code, lowEstMinutes: 15);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { workflow.Parent.WorkflowItems[0] });

			AssertEquals("Not enough work, Idle", channel.Status);

			var startDate = new ZDateTime(2014, 1, 28);
			var endDate = new ZDateTime(2014, 1, 29);
			CreateHoliday(Factory, resourceCCR1, channel, startDate, endDate, "ANN");
			Factory.Save();
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { workflow.Parent.WorkflowItems[0] });
			AssertEquals("When CCR is on leave with CCR queue too short, and not post-CCR tasks and pre-CCR sequence, should not show the High Risk message", "Away until Wed 29-Jan", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_HighRiskShortQueue_IncludesAllTaskEstimates()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var resourceNonCCR = CreateStaffInCurrentBranchDept("NC1", "Non CCR staff");
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			group.Staff.AddRange(resourceCCR1);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR.PK, overrideChannels: true);
			resourceCCR1.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Now);
			var task1 = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, lowEstMinutes: 2000, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, resourceNonCCR.GS_Code, lowEstMinutes: 15, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, lowEstMinutes: 15, sequence: 3);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2, task3 });

			AssertEquals("Idle", channel.Status);

			task1.P9_Sequence = 3;
			task2.P9_Sequence = 2;
			task3.P9_Sequence = 1;

			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2, task3 });

			AssertEquals("Not enough work, Idle", channel.Status);

			task1.P9_Sequence = 2;
			task2.P9_Sequence = 3;
			task3.P9_Sequence = 1;

			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2, task3 });

			AssertEquals("Idle", channel.Status);

			task1.P9_Sequence = 1;
			task2.P9_Sequence = 1;
			task3.P9_Sequence = 1;

			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2, task3 });

			AssertEquals("Idle", channel.Status);

			task1.P9_Sequence = 6;
			task2.P9_Sequence = 5;
			task3.P9_Sequence = 4;

			var task4 = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, lowEstMinutes: 15, sequence: 1);
			var task5 = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, lowEstMinutes: 15, sequence: 2);
			var task6 = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, lowEstMinutes: 15, sequence: 3);

			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2, task3, task4, task5, task6 });

			AssertEquals("Not enough work, Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 6)]
		public void TestStatus_HighRiskLateCCRTasks()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			var section = config.Section;
			var workflow = config.Workflows[0];
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow;
			workflow.Parent.WorkflowItems[0].P9_EstDuration = new ZInt(3 * 8 * 60).GetDateTimeFromMinutes();
			Factory.Save();
			var viewModel = config.SectionViewModel;

			var channel = config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			AssertEquals("Not high risk because it just got releated", "Idle", channel.Status);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-13);
			Factory.Save();

			viewModel = config.ResetViewModel();
			channel = config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			AssertEquals("'Outside target zone' because CCR n-th % is outside target zone (higher in buffer penetration)", "Outside target zone, Idle", channel.Status);

			var startDate = new ZDateTime(2014, 1, 6);
			var endDate = new ZDateTime(2014, 1, 7);
			CreateHoliday(Factory, config.CCR, channel, startDate, endDate, "ANN");
			Factory.Save();
			viewModel = config.ResetViewModel();
			channel = config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			AssertEquals("with late CCR tasks, even if the CCR is on leave, the 'Outside target zone' message should be shown", "Outside target zone, Away until Tue 7-Jan", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 6)]
		public void TestStatus_RiskStatusOnlyForCCRChannel()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceNonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			group.Staff.AddRange(resourceNonCCR);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR.PK, overrideChannels: true);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, releaseDateTime: ZDateTime.Now, staffCode: resourceNonCCR.GS_Code, lowEstMinutes: 15);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { workflow.Parent.WorkflowItems[0] });

			AssertEquals("Zone 3, Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 6)]
		public void TestStatus_CCRChannelInCCRModeNotShowingZone()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			group.Staff.AddRange(resourceCCR);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR.PK, overrideChannels: true);
			resourceCCR.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Now, staffCode: resourceCCR.GS_Code, lowEstMinutes: 2400);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { workflow.Parent.WorkflowItems[0] });

			AssertEquals("Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Overloaded()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * 1.3), taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Overloaded, Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Zone3_NoFadePercentage()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			AssertEquals(3, workflow.BufferZone.Value);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Zone 3, Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Zone3()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			AssertEquals(3, workflow.BufferZone.Value);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Zone 3, Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2016, 9, 1)]
		public void TestBufferPenetrationForJobWorkflow()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("TST", "Test Staff");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow1, resource.GS_Code, 60);
			AssertEquals("Precondition: workflow1 is in Zone 3", 3, workflow1.BufferZone.Value);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-6);
			var task2 = CreateTask(workflow2, resource.GS_Code, 20);
			AssertEquals("Precondition: workflow2 is in Zone 2", 2, workflow2.BufferZone.Value);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow_jobHeader2 = jobHeader2.ProcessHeaders[0];
			workflow_jobHeader2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-12);
			var task_workflow_jobHeader2 = CreateTask(workflow_jobHeader2, resource.GS_Code, 20, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			AssertEquals("Precondition: workflow for jobHeader2 is in Zone 1", 1, workflow_jobHeader2.BufferZone.Value);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task, task2, task_workflow_jobHeader2 });

			AssertEquals(0.6770833333333333333333333333m, viewModel.Cache.GetCachedValue<decimal>(jobHeader2.PK, TaskJobWorkflowCacheHelper.CacheConstants.Penetration));
			AssertEquals(0.3333333333333333333333333333m, viewModel.Cache.GetCachedValue<decimal>(jobHeader.PK, TaskJobWorkflowCacheHelper.CacheConstants.Penetration));
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Zone2()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-6);
			var task = CreateTask(workflow, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			AssertEquals(2, workflow.BufferZone.Value);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Zone 2, Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Zone1()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-12);
			var task = CreateTask(workflow, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			AssertEquals(1, workflow.BufferZone.Value);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Zone 1, Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_Zone0()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-18);
			var task = CreateTask(workflow, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			AssertEquals(0, workflow.BufferZone.Value);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task });

			AssertEquals("Zone 0, Working", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28)]
		public void TestStatus_WorkingStatus()
		{
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
			var task2 = CreateTask(workflow2, resource.GS_Code, 60, description: "task2");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, useStatusCache: false);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2 });

			AssertEquals("Has a working task in a buffer", "Zone 3, Working", channel.Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			channel.ClearCacheAndReload();
			AssertEquals("Has no working tasks", "Zone 3, Idle", channel.Status);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			channel.ClearCacheAndReload();
			AssertEquals("Has a current task in a buffer, and is working on a task outside the buffer", "Zone 3, Alert", channel.Status);

			task1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task1.P9_G4_RequiredCapability = capability1.PK;
			Factory.Save();

			channel.ClearCacheAndReload();
			AssertEquals("Has a current task with a capability possessed by the resource in a buffer, and is working on a task outside the buffer", "Zone 3, Alert", channel.Status);

			task1.P9_G4_RequiredCapability = capability2.PK;
			workflow2.FH_IsStandby = true;
			Factory.Save();
			BMSTestHelper.CacheTasksStartability(viewModel, task1);

			channel.ClearCacheAndReload();
			AssertEquals("Has no current task in any buffer (assigned to a capability resource does not have), and is working on a standby task", "Zone 3, On Standby", channel.Status);

			workflow2.FH_IsStandby = false;
			Factory.Save();

			channel.ClearCacheAndReload();
			AssertEquals("Has no current task in any buffer, and is working on a non-standby task", "Zone 3, Working", channel.Status);

			var holiday = resource.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = ZDateTime.Today;
			holiday.GA_EndTime = ZDateTime.Today.AddDays(2);
			holiday.GA_ApprovalStatus = "APP";

			Factory.Save();

			channel.ClearCacheAndReload();
			AssertEquals("Resource is on leave", "Zone 3, Away until Thu 30-Jan", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 28, 12, 0, 0)]
		public void TestStatus_WorkingStatus_OutsideWorkingHours()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "DPR", "DPIB Review");

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "completionStatement1");

			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
			task1.P9_ActualDuration = new ZDateTime(2014, 1, 1).AddMinutes(32);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, useStatusCache: false);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });
			AssertEquals("Has a working task in a buffer", "Zone 3, Working for 32 minutes", channel.Status);

			TestDateAttribute.Date = new DateTime(2014, 1, 26, 0, 0, 0);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });
			AssertEquals("Has a working task in a buffer but outside working-hours, so status to Away without duration", "Zone 3, Away until Mon 27-Jan", channel.Status);

			TestDateAttribute.Date = new DateTime(2014, 1, 28, 12, 0, 0);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });
			AssertEquals("Has a working task in a buffer", "Zone 3, Working for 32 minutes", channel.Status);

			GlbStaffHoliday sickLeave = resource.HolidaysIncBMSLeave.AddNew();
			sickLeave.GA_StartTime = ZDateTime.Now.AddDays(-1);
			sickLeave.GA_EndTime = ZDateTime.Now.AddDays(1);
			sickLeave.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			sickLeave.GA_WorkHolidayType = "SIC";
			Factory.Save();
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });
			AssertEquals("Has a working task in a buffer but is SICK, so status to Away without duration", "Zone 3, Away until Wed 29-Jan", channel.Status);
		}

		public void TestStatus_WhenAnUnChanneledChannelExistsOnTheBoard_ShouldNotThrowException()
		{
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var section = CreateBoardSection(CreateBuffer(CreateSystem()));
			section.SectionConfiguration.OverrideChannels = true;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			section.SectionConfiguration.ShowUnchanneled = true;
			section.SectionConfiguration.PrimaryAxisChannels[1].MSC_ChannelType = ChannelTypeList.Codes.Resource;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals(2, viewModel.PrimaryChannels.Count());
			var resourceChannel = viewModel.PrimaryChannels.First(c => c.EntityPK == resource.PK);

			AssertNoExceptionThrown(() => { var v = resourceChannel.Status; });
		}

		[TestDate(2014, 12, 19, 9, 0, 0)]
		public void TestStatus_ForBucket_WorkingStatus()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var task1 = CreateTask(workflow1, resource.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			var task2 = CreateTask(workflow2, resource.GS_Code, 60);

			var board = CreateBoard(config.System);
			var section1 = CreateBoardSection(config.Bucket, board);
			var section2 = CreateBoardSection(config.Buffer, board);

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			Factory.Save();

			var viewModel1 = BMSTestHelper.CreateViewModel(section1, useStatusCache: false);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2, useStatusCache: false);

			var channel1 = viewModel1.CreateChannelForTest(resource);
			var channel2 = viewModel2.CreateChannelForTest(resource);

			BMSTestHelper.CacheTasksStartability(viewModel1, task1);
			BMSTestHelper.CacheTasksStartability(viewModel2, task2);

			AssertEquals("Working", channel1.Status);
			AssertEquals("Alert", channel2.Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			channel1.ClearChannelCache();
			channel2.ClearChannelCache();

			AssertEquals("Idle", channel1.Status);
			AssertEquals("Idle", channel2.Status);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			channel1.ClearChannelCache();
			channel2.ClearChannelCache();

			AssertEquals("Working", channel1.Status);
			AssertEquals("Working", channel2.Status);
		}

		[TestDate(2015, 5, 19, 12, 30, 1, 10)]
		public void TestStatus_GetFirstAvailableDateWhenTimeHasMillisecond()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var resource = CreateStaffInCurrentBranchDept("P01", "Person 1");
			resource.DesignateAsCCR(buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, useStatusCache: false);
			var channel = viewModel.CreateChannelForTest(resource);
			Factory.Save();

			AssertEquals("Idle", channel.Status);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var task1 = CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
			task1.P9_ActualDuration = new ZDateTime(2015, 1, 1).AddMinutes(10);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			channel.ClearChannelCache();
			AssertEquals("Working for 10 minutes", channel.Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(60);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			channel.ClearChannelCache();
			AssertEquals("Idle", channel.Status);

			var startdate = new ZDateTime(2015, 5, 19);
			var enddate = new ZDateTime(2015, 5, 20);
			var holiday = CreateHoliday(Factory, resource, channel, startdate, enddate, "ANN");
			Factory.Save();
			channel.ClearChannelCache();

			var holidayDBHits = Factory.GetTableHitCount(GlbHolidaySchema.Constants.TableName);
			AssertEquals("Away until Wed 20-May", channel.Status);
		}

		[TestDate(2015, 2, 23, 2, 30, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestStatus_GetFirstAvailableDateInAwayStatus()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var resource = CreateStaffInCurrentBranchDept("P01", "Person 1");
			resource.DesignateAsCCR(buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, useStatusCache: false);
			var channel = viewModel.CreateChannelForTest(resource);
			Factory.Save();

			AssertEquals("Idle", channel.Status);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var task1 = CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
			task1.P9_ActualDuration = new ZDateTime(2015, 1, 1).AddMinutes(10);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			channel.ClearChannelCache();
			AssertEquals("Working for 10 minutes", channel.Status);

			var startdate = new ZDateTime(2015, 2, 23, 11, 30, 0);
			var enddate = new ZDateTime(2015, 2, 23, 13, 30, 0);
			var holiday = CreateHoliday(Factory, resource, channel, startdate, enddate, "ANN");
			Factory.Save();
			channel.ClearChannelCache();

			var holidayDBHits = Factory.GetTableHitCount(GlbHolidaySchema.Constants.TableName);
			AssertEquals("Away until Mon 23-Feb", channel.Status);
			AssertEquals("Add tick does not effect on result", enddate.Ticks, resource.GetFirstAvailableDate(startdate).Ticks);
			AssertEquals("Factory hit counts shouldn't change from calculating away time", Factory.GetTableHitCount(GlbHolidaySchema.Constants.TableName), holidayDBHits);

			startdate = enddate;
			enddate = enddate.AddDays(5);
			var holiday2 = CreateHoliday(Factory, resource, channel, startdate, enddate, "SIC");
			Factory.Save();
			channel.ClearChannelCache();

			AssertEquals("Add two days for weekends", "Away until Mon 2-Mar", channel.Status);

			startdate = new ZDateTime(2015, 3, 2, 0, 0, 0);
			enddate = new ZDateTime(2015, 3, 3, 0, 0, 0);
			var holiday3 = CreateHoliday(Factory, resource, channel, startdate, enddate, "TRN");
			Factory.Save();
			channel.ClearChannelCache();

			AssertEquals("Away until Tue 3-Mar", channel.Status);

			startdate = new ZDateTime(2015, 3, 3, 8, 0, 0);
			enddate = new ZDateTime(2015, 3, 3, 18, 0, 0);
			var holiday4 = CreateHoliday(Factory, resource, channel, startdate, enddate, "ANN");
			Factory.Save();
			channel.ClearChannelCache();

			AssertEquals("Few hours away still is available in this day", "Away until Wed 4-Mar", channel.Status);

			startdate = new ZDateTime(2015, 3, 3, 0, 0, 0);
			enddate = new ZDateTime(2016, 3, 3, 12, 0, 0);
			var holiday5 = CreateHoliday(Factory, resource, channel, startdate, enddate, "ANN");
			Factory.Save();
			channel.ClearChannelCache();

			AssertEquals("Away for 1 year", "Away until Thu 3-Mar 2016", channel.Status);

			holiday2.GA_IsWorkingAway = true;
			Factory.Save();
			channel.ClearChannelCache();
			AssertEquals("It should back to first available day on Mon 23-Feb", "Away until Mon 23-Feb", channel.Status);
		}

		GlbStaffHoliday CreateHoliday(BusinessObjectFactory factory, GlbStaff resource, IVisualBoardChannel channel, ZDateTime startdate, ZDateTime enddate, string workHolidayType)
		{
			var holiday = resource.HolidaysIncBMSLeave.AddNew();
			holiday.GA_StartTime = startdate;
			holiday.GA_EndTime = enddate;
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_WorkHolidayType = workHolidayType;
			return holiday;
		}

		[RequiresSTA]
		[TestDate(2015, 8, 3)]
		public void TestStatus_SecondaryBuffer_PreConstraint()
		{
			var config = GetConfig();

			var viewModel = config.SectionViewModel;

			viewModel.Cache.Clear();

			// setting zone 3 primary buffer
			var nonCCRWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "non-ccr-workflow",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-1),
				staffCode: config.NonCCR1.GS_Code,
				lowEstMinutes: 30 * 60,
				sequence: 2,
				description: "non-ccr-workflow - non-ccr-task");
			config.Workflows.Add(nonCCRWorkflow);

			// setting zone 2 secondary buffer (pre-ccr)
			var preCCRWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "pre-ccr-workflow",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-4),
				staffCode: config.CCR.GS_Code,
				lowEstMinutes: 8 * 60,
				sequence: 10,
				description: "pre-ccr-workflow - ccr-task");

			var preCCRTask = CreateTask(
				workflow: preCCRWorkflow,
				staffCode: config.NonCCR1.GS_Code,
				lowEstMinutes: 60,
				sequence: 2,
				description: "pre-ccr-worflow - Non-CCR-1 task");
			config.Workflows.Add(preCCRWorkflow);

			Factory.Save();
			config.Section.Factory.ClearCachedValue<BufferPenetrationCalculator.BufferPenetrationCache>();
			viewModel = config.ResetViewModel();

			var channel = config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == config.NonCCR1.PK).Channel;

			AssertEquals("Initially Primary Component should be in zone 3", 3, config.SectionViewModel.ComponentGrid.GetPrimaryComponentZone(channel));

			var preCCRPenetration = preCCRWorkflow.GetSubComponentBufferPenetration(ConstraintStatus.PreConstraint);
			AssertEquals("Initially Secondary Component (pre-ccr) should be in zone 2", 2, ZoneCalculator.CalculateZone(preCCRPenetration.Penetration));

			AssertEquals("WHEN secondary-buffer is not at risk (zone 2), THEN channel-header should display the primary-buffer zone (zone 3)", "Zone 3, Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2015, 8, 3)]
		public void TestStatus_SecondaryBuffer_PostConstraint()
		{
			var config = GetConfig();
			var viewModel = config.SectionViewModel;

			viewModel.Cache.Clear();

			//setting zone 3 primary buffer
			var nonCCRWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "non-ccr-workflow",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-1),
				staffCode: config.NonCCR1.GS_Code,
				lowEstMinutes: 30 * 60,
				sequence: 2,
				description: "non-ccr-workflow - non-ccr-task");
			config.Workflows.Add(nonCCRWorkflow);

			//setting zone 2 secondary buffer (post-ccr)
			var postCCRWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "post-ccr-workflow",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-11),
				staffCode: config.CCR.GS_Code,
				lowEstMinutes: 8 * 60,
				sequence: 2,
				description: "post-ccr-workflow - ccr-task",
				taskStatus: "CLS");

			var postCCRTask = CreateTask(
				workflow: postCCRWorkflow,
				staffCode: config.NonCCR1.GS_Code,
				lowEstMinutes: 60,
				sequence: 3,
				description: "post-ccr-worflow - Non-CCR-1 task");
			config.Workflows.Add(postCCRWorkflow);

			Factory.Save();
			config.Section.Factory.ClearCachedValue<BufferPenetrationCalculator.BufferPenetrationCache>();
			viewModel = config.ResetViewModel();

			var channel = config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == config.NonCCR1.PK).Channel;

			AssertEquals("Initially Primary Component should be in zone 3", 3, config.SectionViewModel.ComponentGrid.GetPrimaryComponentZone(channel));

			var postCCRPenetration = postCCRWorkflow.GetSubComponentBufferPenetration(ConstraintStatus.PostConstraint);
			AssertEquals("Initially Secondary Component (post-ccr) should be in zone 2", 2, ZoneCalculator.CalculateZone(postCCRPenetration.Penetration));

			AssertEquals("WHEN secondary-buffer is not at risk (zone 2), THEN channel-header should display the primary-buffer zone (zone 3)", "Zone 3, Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2017, 10, 23, 9, 0, 0)]
		public void TestStatus_UsesCache()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("CLP", "Clippy");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var section = CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "ABCDEFG";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var task1 = CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2);

			var viewModel = BMSTestHelper.CreateViewModel(section, useStatusCache: true);
			var service = viewModel.BoardViewModel.SlideShowViewModel.Services_ExposedForTest.OfType<RoadRunnerStatusCacheService>().First();

			AssertEquals("Precondition: Nothing in cache service", 0, service.GetStaffCache_ForTesting(resource).Count);

			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1 });

			AssertEquals("Channel Status is set to working", "Zone 3, Working for 2 hours", channel.Status);
			AssertNotNull("service", service);

			var executedFunc = false;
			var cachedValue = service.GetOrCacheValue(resource, "AppendRelevantTimeText", () =>
			{
				executedFunc = true;
				return string.Empty;
			});

			Assert("Cache did not execute func to get value", !executedFunc);
			AssertEquals("' for nn hours' value is in cache", " for 2 hours", cachedValue);
		}

		ComplexConstrainedSchematicTestConfig GetConfig()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			Factory.Save();

			return config;
		}

		#endregion

		#endregion

		#region Tag

		public void TestVisualBoardChannel_Tag()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");

			var task1 = CreateTask(workflow1, string.Empty, 60, description: "task1");
			var task2 = CreateTask(workflow2, string.Empty, 60, description: "task2");
			var task3 = CreateTask(workflow3, string.Empty, 60, description: "task3");
			var task4 = CreateTask(workflow4, string.Empty, 60, description: "task4");

			workflow1.AddTag(config.RedTag); // workflow1 is in RED channel
			task2.AddTag(config.RedTag); // task2 makes workflow2 in RED channel
			workflow3.AddTag(config.GoldTag); // workflow3 is in GLD channel
			jobHeader.AddTag(config.DerpyHoovesTag); // All items are in the DRP channel
			task4.AddTag(config.PrincessCelestiaTag); // Just this task has CEL tag
			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var redChannel = viewModel.CreateChannelForTest(config.RedTag);
			var gldChannel = viewModel.CreateChannelForTest(config.GoldTag);
			var derpChannel = viewModel.CreateChannelForTest(config.DerpyHoovesTag);
			var celChannel = viewModel.CreateChannelForTest(config.PrincessCelestiaTag);

			AssertVisualBoardChannel(redChannel, "RED - Defect", "Defect", "RED", ChannelTypeList.Codes.Tag, null, null, new[] { task1, task2 }, new[] { task3, task4 });
			AssertVisualBoardChannel(gldChannel, "GLD - Gold", "Gold", "GLD", ChannelTypeList.Codes.Tag, null, null, new[] { task3 }, new[] { task1, task2, task4 });
			AssertVisualBoardChannel(derpChannel, "DRP - Derpy Hooves", "Derpy Hooves", "DRP", ChannelTypeList.Codes.Tag, null, null, new[] { task1, task2, task3, task4 }, Array.Empty<ProcessTask>());
			AssertVisualBoardChannel(celChannel, "CEL - Princess Celestia", "Princess Celestia", "CEL", ChannelTypeList.Codes.Tag, null, null, new[] { task4 }, new[] { task1, task2, task3 });
		}

		#endregion

		#region Drag/Drop

		#region Batch Assignment

		#endregion

		#endregion

		#region Implementation

		internal static void AssertVisualBoardChannel(IVisualBoardChannel channel, string fullDescription, string mediumDescription, string shortDescription, string entityType, Image displayImage, string status, ProcessTask[] tasksInChannel, ProcessTask[] tasksNotInChannel)
		{
			AssertEquals(fullDescription, channel.GetChannelName(DisplayNameType.FullName));
			AssertEquals(mediumDescription, channel.GetChannelName(DisplayNameType.FriendlyName));
			AssertEquals(shortDescription, channel.GetChannelName(DisplayNameType.ChannelCode));
			AssertEquals(entityType, channel.EntityType);
			AssertEquals(displayImage, channel.DisplayImage);
			AssertEquals(status, channel.Status);

			foreach (var task in tasksInChannel)
			{
				AssertEquals("Task should be in channel: " + task.P9_Description, true, channel.IsInChannel(task, false));
			}
			foreach (var task in tasksNotInChannel)
			{
				AssertEquals("Task should not be in channel: " + task.P9_Description, false, channel.IsInChannel(task, false));
			}
		}

		IVisualBoardChannel CreateTestVisualBoard(int preConstraintWorkflowReleaseDateOffset, int postConstraintWorkflowReleaseDateOffset)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			var buffer = section.Component;

			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, buffer.PK);

			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR");
			ccr.DesignateAsCCR(buffer);
			var nonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCR", "Non CCR");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, nonCCR.PK, overrideChannels: true);

			var preConstraintWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(preConstraintWorkflowReleaseDateOffset), staffCode: nonCCR.GS_Code, lowEstMinutes: 600, sequence: 1, description: "pre-ccr-workflow - task 1");
			BMSTestHelper.CreateTask(preConstraintWorkflow, ccr.GS_Code, sequence: 2, lowEstMinutes: 15);

			var postConstraintWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(postConstraintWorkflowReleaseDateOffset), staffCode: nonCCR.GS_Code, lowEstMinutes: 100, sequence: 2, description: "post-ccr-workflow - task 1");
			BMSTestHelper.CreateTask(postConstraintWorkflow, ccr.GS_Code, sequence: 1, lowEstMinutes: 15, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading && c.Column == 4).Channel;
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, preConstraintWorkflow.Tasks.Concat(postConstraintWorkflow.Tasks).ToArray());

			return channel;
		}

		#endregion
	}
}
