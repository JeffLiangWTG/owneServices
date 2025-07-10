using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class WorkflowLoaderTest : BMSTestCaseWithFactory
	{
		#region LoadTasks

		public void TestQueryHeader_LoadTasks_ContainsBoardMessage()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			using (TestConnection.TrackExecutedCommands())
			{
				WorkflowLoader.LoadTasks(config.BufferSection, config.BufferSection.SectionConfiguration.PrimaryAxisChannels, null, null);

				var query = TestConnection.ExecutedCommands.SingleOrDefault(x => x.Contains("CurrentComponent ="));
				AssertNotNull(query);
				AssertContains("The query should include the database name, board name, and section name so that we can track down bad queries that show up in Kibana. SAD!", $@"
			-- Database: {TestConnection.CurrentDatabase}
			-- Board:    Buffer Board
			-- Section:  buffer", query);
			}
		}

		//This is for performance reasons, FH_P0_Template will always be null in this query
		public void TestQueryHeader_LoadTasks_DontCheckIfTemplate()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			using (TestConnection.TrackExecutedCommands())
			{
				WorkflowLoader.LoadTasks(config.BufferSection, config.BufferSection.SectionConfiguration.PrimaryAxisChannels, null, null);

				var query = TestConnection.ExecutedCommands.SingleOrDefault(x => x.Contains("CurrentComponent ="));
				AssertNotNull(query);
				AssertNotContains("FH_P0_Template is NULL", query);
			}
		}

		public void TestLoadTasks_ShouldUseTVPForTaskPKs_WhenTaskScope()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = CreateWorkflows(config.BufferSection.Component, 5, 10);

			var allTasks = new List<ProcessTask>();

			foreach (var workflow in workflows)
			{
				var tasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK));
				AssertEquals("PRE: Each of our workflows should have 10 tasks.", 10, tasks.Length);
				allTasks.AddRange(tasks);
			}

			Factory.Save();

			var taskPKs = allTasks.Select(t => t.PK).ToArray();
			var sectionToDisplay = new BusinessObjectFactory().Load<BMBoardSection>(config.BufferSection.PK);

			using (Db.Connection.TrackExecutedCommands())
			{
				WorkflowLoader.LoadTasks(sectionToDisplay, sectionToDisplay.SectionConfiguration.PrimaryAxisChannels, null, null, taskPKs);
				var executedCommand = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Contains("FROM dbo.ProcessTasks"));
				AssertNotNull("Command should be executed", executedCommand);
				AssertContains("WorkflowLoader.LoadTasks Should use TVP", "(P9_PK in (SELECT Value FROM @CWO", executedCommand);
			}
		}

		public void TestQueryHeader_BuildBoardQuery()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			using (TestConnection.TrackExecutedCommands())
			{
				_ = config.BufferSection.Factory.Load<ProcessTask>(WorkflowLoader.BuildBoardQuery(config.BufferSection, config.BufferSection.SectionConfiguration.PrimaryAxisChannels, null, null));

				var query = TestConnection.ExecutedCommands.SingleOrDefault(x => x.Contains("CurrentComponent ="));
				AssertNotNull(query);
				AssertContains("The query should include the database name, board name, and section name so that we can track down bad queries that show up in Kibana.", $@"
			-- Database: {TestConnection.CurrentDatabase}
			-- Board:    Buffer Board
			-- Section:  buffer", query);
			}
		}

		#endregion

		#region LoadTaskForWorkflows

		public void TestLoadTaskForWorkflows_ShouldUseTVPForWorkflowPKs_WithShowJobWorkflowCardsTrue()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system, "Butter");
			var section = CreateBoardSection(buffer);
			// set section "ShowJobWorkflowCards" to true
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var workflowCount = 10;
			var taskPerWorkflowCount = 10;

			var workflows = CreateWorkflows(buffer, workflowCount, taskPerWorkflowCount);
			List<ProcessTask> allTasks = new List<ProcessTask>();

			foreach (var workflow in workflows)
			{
				var tasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK));
				AssertEquals("PRE: Each of our workflows should have 10 tasks.", 10, tasks.Length);
				allTasks.AddRange(tasks);
			}

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				var newFac = new BusinessObjectFactory();
				var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);

				// this query looks at job-level workflows, not baby inner-workflows, so we give it a different array of things
				var reloadedTasks = WorkflowLoader.LoadTasksForWorkflows(sectionToDisplay, workflows.Select(w => w.ParentHeader).ToArray());
				var executedCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("SELECT FH_PK FROM dbo.ProcessHeader"));
				Assert("At least one matching command should be executed", !executedCommands.IsNullOrEmpty());

				foreach (string query in executedCommands)
				{
					AssertNotContains("WorkflowLoader.LoadTasksForWorkflows() should not use a parameter comparison", "WHERE FH_PK = ", query);
					AssertContains("WorkflowLoader.LoadTasksForWorkflows() should use TVPs", "FH_PK in (SELECT Value FROM", query);
					Assert("WorkflowLoader.LoadTasksForWorkflows() should not use a parameter list)", !Regex.IsMatch(query, @"WHERE \(P9_FH_ProcessHeader in \((@(.*?),)*?@(.*?)\)"));
				}

				foreach (var task in allTasks)
				{
					Assert("Process task should exist in our reloaded list.", reloadedTasks.Any(t => t.PK.ToGuid() == task.PK.ToGuid()));
				}
			}
		}

		public void TestLoadTaskForWorkflows_ShouldUseTVPForWorkflowPKs_WithShowJobWorkflowCardsFalse()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system, "Butter");
			var section = CreateBoardSection(buffer);
			// set section "ShowJobWorkflowCards" to false
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var workflowCount = 10;
			var taskPerWorkflowCount = 10;

			var workflows = CreateWorkflows(buffer, workflowCount, taskPerWorkflowCount);
			Factory.Save();

			List<ProcessTask> allTasks = new List<ProcessTask>();

			foreach (var workflow in workflows)
			{
				var tasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK));
				AssertEquals("PRE: Each of our workflows should have 10 tasks.", 10, tasks.Length);
				allTasks.AddRange(tasks);
			}

			using (Db.Connection.TrackExecutedCommands())
			{
				var newFac = new BusinessObjectFactory();
				var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);

				var reloadedTasks = WorkflowLoader.LoadTasksForWorkflows(sectionToDisplay, workflows);
				var executedCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("P9_FH_ProcessHeader in "));
				Assert("At least one matching command should be executed", !executedCommands.IsNullOrEmpty());

				foreach (string query in executedCommands)
				{
					AssertNotContains("WorkflowLoader.LoadTasksForWorkflows() should not use a parameter comparison", "WHERE FH_PK = ", query);
					AssertContains("WorkflowLoader.LoadTasksForWorkflows() should use TVPs", "P9_FH_ProcessHeader in (SELECT Value FROM", query);
					Assert("WorkflowLoader.LoadTasksForWorkflows() should not use a parameter list)", !Regex.IsMatch(query, @"WHERE \(P9_FH_ProcessHeader in \((@(.*?),)*?@(.*?)\)"));
				}

				foreach (var task in allTasks)
				{
					Assert("Process task should exist in our reloaded list.", reloadedTasks.Any(t => t.PK.ToGuid() == task.PK.ToGuid()));
				}
			}
		}

		#endregion

		#region LoadWorkflows

		public void TestQueryHeader_LoadWorkflows()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			using (TestConnection.TrackExecutedCommands())
			{
				WorkflowLoader.LoadWorkflows(config.BufferSection, config.BufferSection.SectionConfiguration.PrimaryAxisChannels, new ZQuery(), new ZQuery());

				var query = TestConnection.ExecutedCommands.SingleOrDefault(x => x.Contains("FH_FC_CurrentComponent ="));
				AssertNotNull(query);
				AssertContains("The query should include the database name, board name, and section name so that we can track down bad queries that show up in Kibana. SAD!", $@"
			-- Database: {TestConnection.CurrentDatabase}
			-- Board:    Buffer Board
			-- Section:  buffer", query);
			}
		}

		public void TestLoadWorkflows_ShouldUseTableValuedParametersAndRunSmoothly()
		{
			var system = CreateSystem(workflowTypes: "DUM");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			const int additionalComponentsCount = 5;
			const int globalCapabilitiesCount = 10;
			const int groupCapabilitiesCount = 10;

			var additionalComponents = new List<BMComponent>(additionalComponentsCount);
			for (int i = 0; i < additionalComponentsCount; i++)
			{
				var component = BMSTestHelper.CreateBuffer(system, name: "AdditionalComponents" + i);
				BMSTestHelper.CreateAdditionalComponent(section, component);
				additionalComponents.Add(component);
			}

			var globalCapabilities = new List<GlbCapability>(globalCapabilitiesCount);
			var groupCapabilities = new List<GlbCapability>(groupCapabilitiesCount);

			for (int i = 0; i < globalCapabilitiesCount; i++)
			{
				var capabilityForChannel = CreateCapability("C" + i, "CapabilityForChannel" + i);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capabilityForChannel.PK, overrideChannels: true);

				var globalCapability = CreateCapability("P" + i, "GlobalCapability" + i);
				globalCapabilities.Add(globalCapability);
				var groupCapability = BMSTestHelper.CreateCapability(Factory, "G" + i, "GroupCapability" + i, isGroupScope: true);
				groupCapabilities.Add(groupCapability);
			}

			var group = BMSTestHelper.CreateGroup(Factory, "GRP", "group");

			for (int i = 0; i < groupCapabilitiesCount; i++)
			{
				var staffWithGlobalCapabilities = CreateStaffInCurrentBranchDept("R" + i, "staffWithGlobalCapabilities" + i, globalCapabilities.ToArray());
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staffWithGlobalCapabilities.PK, overrideChannels: true);

				var staffWithGroupCapabilities = CreateStaffInCurrentBranchDept("S" + i, "staffWithGroupCapabilities" + i, groupCapabilities.ToArray());
				group.Staff.Add(staffWithGroupCapabilities);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staffWithGroupCapabilities.PK, overrideChannels: true);
			}

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				WorkflowLoader.LoadWorkflows(section, section.SectionConfiguration.Channels, new ZQuery(), new ZQuery());

				var query = TestConnection.ExecutedCommands.Where(sqlCommand => sqlCommand.Contains("FROM dbo.ProcessHeader")).ToArray().First();

				AssertEquals(1, Regex.Matches(query, @"\(FH_FC_CurrentComponent in \(@CW([^)]+)\)").Count);
				AssertContains("(P9_GS_NKAssignedStaffMember in (SELECT Value FROM", query);
				AssertEquals(3, Regex.Matches(query, @"\(P9_G4_RequiredCapability in \(SELECT Value FROM").Count);
				AssertEquals(2, Regex.Matches(query, @"\(SELECT GK_GG FROM dbo.GlbGroupLink WHERE \(GK_GS in").Count);
			}
		}

		public void TestLoadWorkflows_ShouldIncludeInactiveWorkflows()
		{
			var system = CreateSystem(workflowTypes: "DUM");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var staff = CreateStaffInCurrentBranchDept("AAA", "Alex Alexander");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			var processHeadersAddedToBuffer = CreateWorkflows(buffer, 5, staff: staff, numberOfTasksPerWorkflow: 1);

			foreach (var processHeader in processHeadersAddedToBuffer)
			{
				processHeader.FH_IsActive = false;
			}

			Factory.Save();

			var workflowsLoadedInSection = WorkflowLoader.LoadWorkflows(section, section.SectionConfiguration.Channels, new ZQuery(), new ZQuery());

			AssertEquals(5, workflowsLoadedInSection.Length);
			AssertContainsExactElementsInAnyOrder(workflowsLoadedInSection, processHeadersAddedToBuffer);
		}

		#endregion

		#region Capability

		public void TestLoadTasks_WhenStaffInGRPCapabilityAndNotInTheReleaseGroupOrInTheTaskGroup_ShouldNotLoadTask()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system, "Buffer");
			var section = CreateBoardSection(buffer);
			var taskGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			CreateReleaseGroup(system, releaseGroup);
			var capability = CreateCapability("CP1", "Capability1");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = CreateCapability("CP2", "Capability2");
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			releaseGroup.Staff.Add(staff1);
			var staff2 = CreateStaffInCurrentBranchDept("RE2", "resource2", capability);
			taskGroup.Staff.Add(staff2);
			var staff3 = CreateStaffInCurrentBranchDept("RE3", "resource3", capability2);
			taskGroup.Staff.Add(staff3);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff3.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability2.PK);

			var tasksToLoad = new List<ProcessTask>();

			var workflows = CreateWorkflows(buffer, numberOfWorkflows: 4, numberOfTasksPerWorkflow: 3).ToArray();

			workflows[0].FH_GG_ReleaseGroup = releaseGroup.PK;
			SetTasksCapapabilityAndGroup(workflows[0], requiredCapabilityPK: capability.PK, groupPK: ZGuid.Empty);
			tasksToLoad.AddRange(workflows[0].Tasks);

			workflows[1].FH_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			SetTasksCapapabilityAndGroup(workflows[1], requiredCapabilityPK: capability.PK, groupPK: releaseGroup.PK);
			tasksToLoad.AddRange(workflows[1].Tasks);

			SetTasksCapapabilityAndGroup(workflows[2], requiredCapabilityPK: capability2.PK, groupPK: ZGuid.Empty);
			tasksToLoad.AddRange(workflows[2].Tasks);

			//This Workflow is not in the RG and do not have any task in the Group, so should load no tasks :)
			workflows[3].FH_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			SetTasksCapapabilityAndGroup(workflows[3], requiredCapabilityPK: capability.PK, groupPK: ZGuid.Empty);
			Factory.Save();

			var newFac = new BusinessObjectFactory();
			var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);
			var loadedTasks = WorkflowLoader.LoadTasks(sectionToDisplay, section.SectionConfiguration.PrimaryAxisChannels, null, null);

			AssertContainsExactElementsInAnyOrder(tasksToLoad.Select(task => task.PK), loadedTasks.Select(task => task.PK));
		}

		public void TestLoadWorkflows_WhenStaffInGRPCapabilityAndNotInTheReleaseGroupOrInTheTaskGroup_ShouldNotLoadWorkflow()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system, "Buffer");
			var section = CreateBoardSection(buffer);
			var taskGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			CreateReleaseGroup(system, releaseGroup);
			var capability = CreateCapability("CP1", "Capability1");
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = CreateCapability("CP2", "Capability2");
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			releaseGroup.Staff.Add(staff1);
			var staff2 = CreateStaffInCurrentBranchDept("RE2", "resource2", capability);
			taskGroup.Staff.Add(staff2);
			var staff3 = CreateStaffInCurrentBranchDept("RE3", "resource3", capability2);
			taskGroup.Staff.Add(staff3);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff3.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability2.PK);

			var workflowsToLoad = new List<ProcessHeader>();

			var workflows = CreateWorkflows(buffer, numberOfWorkflows: 4, numberOfTasksPerWorkflow: 3).ToArray();

			workflows[0].FH_GG_ReleaseGroup = releaseGroup.PK;
			SetTasksCapapabilityAndGroup(workflows[0], requiredCapabilityPK: capability.PK, groupPK: ZGuid.Empty);
			workflowsToLoad.Add(workflows[0]);

			workflows[1].FH_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			SetTasksCapapabilityAndGroup(workflows[1], requiredCapabilityPK: capability.PK, groupPK: releaseGroup.PK);
			workflowsToLoad.Add(workflows[1]);

			SetTasksCapapabilityAndGroup(workflows[2], requiredCapabilityPK: capability2.PK, groupPK: ZGuid.Empty);
			workflowsToLoad.Add(workflows[2]);

			// This Workflow is not in the RG and do not have any task in the Group, so should not load:)
			workflows[3].FH_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			SetTasksCapapabilityAndGroup(workflows[3], requiredCapabilityPK: capability.PK, groupPK: ZGuid.Empty);
			Factory.Save();

			var newFac = new BusinessObjectFactory();
			var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);
			var loadedWorkflows = WorkflowLoader.LoadWorkflows(sectionToDisplay, section.SectionConfiguration.PrimaryAxisChannels, new ZQuery(), new ZQuery());

			AssertContainsExactElementsInAnyOrder(workflowsToLoad.Select(workflow => workflow.PK), loadedWorkflows.Select(workflow => workflow.PK));
		}

		#endregion

		#region Show Work In Release Group Only

		public void TestLoadTasks_WhenBoardConfigurationShowWorkInReleaseGroupOnlyIsTrue_SholdLoadTasksWithInGroup()
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

			var staff = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			boardReleaseGroup.Staff.Add(staff);

			var workflowInOtherReleaseGroup = CreateWorkflow(CreateJobHeader<OrgHeader>(), "wf1", currentComponent: buffer, releaseGroupPK: otherReleaseGroup.PK);

			var taskInBoardReleaseGroup1 = CreateTask(workflowInOtherReleaseGroup, string.Empty, 90, capability: capability, description: "In RG, but in a workflow in other RG");
			taskInBoardReleaseGroup1.P9_GG_AssignedGroup = boardReleaseGroup.PK;

			var taskInOtherReleaseGroup1 = CreateTask(workflowInOtherReleaseGroup, string.Empty, 90, capability: capability, description: "In other releaseGroup");
			taskInOtherReleaseGroup1.P9_GG_AssignedGroup = otherReleaseGroup.PK;

			var taskOnlyWithGroup = CreateTask(workflowInOtherReleaseGroup, string.Empty, 90, capability: null, description: "Only with group");
			taskOnlyWithGroup.P9_GG_AssignedGroup = boardReleaseGroup.PK;

			var workflowInBoardReleaseGroup = CreateWorkflow(CreateJobHeader<OrgHeader>(), "wf2", currentComponent: buffer, releaseGroupPK: boardReleaseGroup.PK);

			var taskWithNoGroup = CreateTask(workflowInBoardReleaseGroup, string.Empty, 90, capability: capability, description: "No group, but in a workflow in the RG");
			taskWithNoGroup.P9_GG_AssignedGroup = ZGuid.Empty;

			var taskInBoardReleaseGroup2 = CreateTask(workflowInBoardReleaseGroup, string.Empty, 90, capability: capability, description: "In RG in a workflow in the RG");
			taskInBoardReleaseGroup2.P9_GG_AssignedGroup = boardReleaseGroup.PK;

			var taskInOtherReleaseGroup2 = CreateTask(workflowInBoardReleaseGroup, string.Empty, 90, capability: capability, description: "In other RG in a workflow in the RG");
			taskInOtherReleaseGroup2.P9_GG_AssignedGroup = otherReleaseGroup.PK;

			var staffTask = CreateTask(workflowInBoardReleaseGroup, staff.GS_Code, 90, description: "Signed to staff in a workflow in the RG");
			staffTask.P9_GG_AssignedGroup = boardReleaseGroup.PK;

			var staffTaskWithCapability = CreateTask(workflowInBoardReleaseGroup, staff.GS_Code, 90, capability: capability, description: "Signed to staff with Cap in a workflow in the RG");
			staffTaskWithCapability.P9_GG_AssignedGroup = ZGuid.Empty;

			var taskInGroupChannel = CreateTask(workflowInBoardReleaseGroup, string.Empty, 90, capability: null, description: "In a group channel");
			taskInGroupChannel.P9_GG_AssignedGroup = boardReleaseGroup.PK;

			var staff1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);
			var groupChannel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Group, boardReleaseGroup.PK);

			Factory.Save();

			var newFac = new BusinessObjectFactory();
			var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);
			var tasks = WorkflowLoader.LoadTasks(sectionToDisplay, section.SectionConfiguration.PrimaryAxisChannels, null, null);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				taskInBoardReleaseGroup1.P9_Description,
				taskOnlyWithGroup.P9_Description,
				taskWithNoGroup.P9_Description,
				taskInBoardReleaseGroup2.P9_Description,
				staffTask.P9_Description,
				staffTaskWithCapability.P9_Description,
				taskInGroupChannel.P9_Description
			},
			tasks.Select(t => t.P9_Description));
		}

		public void TestLoadWorkflows_WhenBoardConfigurationShowWorkInReleaseGroupOnlyIsTrue_SholdLoadWorkflowsWithTasksWithInGroup()
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

			var staff = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			boardReleaseGroup.Staff.Add(staff);

			var workflowsToLoad = new List<ProcessHeader>();

			var workflowsInOtherReleaseGroup = CreateWorkflows(buffer, 5, 2, otherReleaseGroup);

			var workflowsInOtherReleaseGroupWithSomeTasksInTheGroup = CreateWorkflows(buffer, 5, 2, otherReleaseGroup);
			foreach (var workflow in workflowsInOtherReleaseGroupWithSomeTasksInTheGroup)
			{
				var firstTask = workflow.Tasks.First();
				firstTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				firstTask.P9_GG_AssignedGroup = boardReleaseGroup.PK;
				workflowsToLoad.Add(workflow);
			}

			var workflowsInBoardReleaseGroup = CreateWorkflows(buffer, 5, 2, boardReleaseGroup);
			foreach (var workflow in workflowsInBoardReleaseGroup)
			{
				SetTasksCapapabilityAndGroup(workflow, capability.PK, ZGuid.Empty);
				workflowsToLoad.Add(workflow);
			}

			var workflowsInBoardReleaseGroupWithNoTaskInGroup = CreateWorkflows(buffer, 5, 2, boardReleaseGroup);
			foreach (var workflow in workflowsInBoardReleaseGroupWithNoTaskInGroup)
			{
				SetTasksCapapabilityAndGroup(workflow, capability.PK, otherReleaseGroup.PK);
			}

			var staff1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			var newFac = new BusinessObjectFactory();
			var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);
			var workflows = WorkflowLoader.LoadWorkflows(sectionToDisplay, section.SectionConfiguration.PrimaryAxisChannels, new ZQuery(), new ZQuery());

			AssertContainsExactElementsInAnyOrder(workflowsToLoad.Select(w => w.PK), workflows.Select(w => w.PK));
		}

		[StressTest]
		public void TestShowWorkInReleaseGroupOnlyFilter_ShouldNotHaveTableScansOrIndexScansAndAllTaskSubqueriesShouldUseFilters()
		{
			const int workflowsPerCategory = 50;
			const int tasksPerWorkflow = 5;
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

			var staff = CreateStaffInCurrentBranchDept("RE1", "resource1", capability);
			boardReleaseGroup.Staff.Add(staff);

			var tasksToLoad = new List<ProcessTask>();

			var workflowsInOtherReleaseGroup = CreateWorkflows(buffer, workflowsPerCategory, tasksPerWorkflow, otherReleaseGroup);

			var workflowsInOtherReleaseGroupWithSomeTasksInTheGroup = CreateWorkflows(buffer, workflowsPerCategory, tasksPerWorkflow, otherReleaseGroup);
			workflowsInOtherReleaseGroupWithSomeTasksInTheGroup.ForEach(wf =>
			{
				var firstTask = wf.Tasks.First();
				firstTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				firstTask.P9_GG_AssignedGroup = boardReleaseGroup.PK;
				tasksToLoad.Add(firstTask);
			});

			var workflowsInBoardReleaseGroup = CreateWorkflows(buffer, workflowsPerCategory, tasksPerWorkflow, boardReleaseGroup);
			workflowsInBoardReleaseGroup.ForEach(wf =>
			{
				SetTasksCapapabilityAndGroup(wf, capability.PK, ZGuid.Empty);
				tasksToLoad.AddRange(wf.Tasks);
			});

			var workflowsInBoardReleaseGroupWithNoTaskInGroup = CreateWorkflows(buffer, workflowsPerCategory, tasksPerWorkflow, boardReleaseGroup);
			workflowsInBoardReleaseGroupWithNoTaskInGroup.ForEach(wf => SetTasksCapapabilityAndGroup(wf, capability.PK, otherReleaseGroup.PK));

			var staff1Channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			var expectedIndexesToScan = new[]
			{
				"NR_RX__P9_Status_P9_Type_P9_ParentTableCode",
				"NR_RX__FH_FC_CurrentComponent_FH_IsActive_FH_P0_Template_FH_PK",
				"NR_RX__P9_ParentID",
				"NR_RX__P9_FH_ProcessHeader_P9_Type_P9_Status",
				"NR_RX__P9_GS_NKAssignedStaffMember_P9_G4_RequiredCapability_P9_Status_P9_Type",
				"PK_UX__P9_PK",
				"NR_RC__P9_ParentID"
			};

			var newFac = new BusinessObjectFactory();
			var sectionToDisplay = newFac.Load<BMBoardSection>(section.PK);

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var loadedTasks = WorkflowLoader.LoadTasks(sectionToDisplay, section.SectionConfiguration.PrimaryAxisChannels, null, null);
				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("'ASN', 'OPN', 'SUS', 'WRK'"));
				var planalyzer = new QueryPlanalyzer(queryPlans.Item2.Last());

				QueryPlanalyzer.AssertNoRIDLookups(planalyzer);
				QueryPlanalyzer.AssertNoTableScans(planalyzer);

				var numberOfTypeStatusFilters = 2;

				AssertEquals("All task sub-queries should use not type and status filters", numberOfTypeStatusFilters, queryPlans.Item1.AllIndexesOf("P9_Type not in ('EXC', 'MIL', 'TRG')) and (P9_Status in ('ASN', 'OPN', 'SUS', 'WRK'))").Count());

				var indexScansExcludingParameterisedPKIndexes = planalyzer.IndexScans.Where(i => !i.IndexName.StartsWith("PK__#"));
				foreach (var indexScan in indexScansExcludingParameterisedPKIndexes)
				{
					AssertEquals($@"This index should be scanning one of the expected indexes and nothing else. For future devs, this collection should only get smaller, never larger

Scanned index: {indexScan.IndexName}", true, expectedIndexesToScan.Contains(indexScan.IndexName));
				}
			}
		}

		#endregion

		#region Implementation

		void SetTasksCapapabilityAndGroup(ProcessHeader workflow, ZGuid requiredCapabilityPK, ZGuid groupPK)
		{
			foreach (var task in workflow.Tasks)
			{
				task.P9_GS_NKAssignedStaffMember = string.Empty;
				task.P9_G4_RequiredCapability = requiredCapabilityPK;
				task.P9_GG_AssignedGroup = groupPK;
			}
		}

		#endregion
	}
}
