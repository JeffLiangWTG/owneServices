using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class BoardSectionDataSourceTest : BMSTestCaseWithFactory
	{
		#region Resource and Staff

		public void TestGetTasksForChannel_Resource()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DE";

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Hi";

			var bucket = system.Components.AddNew();
			bucket.FC_Name = "TheBucket";
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;
			section.SectionConfiguration.OverrideChannels = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "DE", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "DE", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "DE", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var capability1 = Factory.New<GlbCapability>();
			capability1.G4_Code = "C1";
			staff.Capabilities.Add(capability1);
			var capability2 = Factory.New<GlbCapability>();
			capability2.G4_Code = "C2";
			var workflow4 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 4", ZDateTime.Now.AddHours(0));
			BMSTestHelper.CreateTasks(org, workflow4, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", capability1.PK), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", capability2.PK), Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 2, "E", capability1.PK));

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(4, tasks.Length);
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 3");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 4 Task 4");
			}
		}

		public void TestLoadGroup_FactoryDoesntLoadUnusedStaffAndTasks()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			group.Staff.Add(staff2);

			Factory.Save();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Sys";

			var buffer = BMSTestHelper.CreateBuffer(system, "SoBuff");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Group, group.PK);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Nonjon";
			var header1 = ProcessJobHeader.GetForParent(org1, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header1, buffer, "Workflow 1", ZDateTime.BrettsBirthday);
			BMSTestHelper.CreateTasks(org1, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Swagyolo";
			var header2 = ProcessJobHeader.GetForParent(org2, Factory);

			var workflow2 = BMSTestHelper.CreateProcessHeader(header2, buffer, "Workflow 2", ZDateTime.BrettsBirthday);
			BMSTestHelper.CreateTasks(org2, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff3.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			Factory.Save();

			var otherFactory = Factory.CreateNewFactory();
			var loadedSection = otherFactory.Load<BMBoardSection>(section.PK);
			var loadedSystem = otherFactory.Load<BMSystem>(system.PK);

			var factoryStatistic = new BusinessObjectFactoryStatistic(otherFactory);
			var sectionInOtherFactory = otherFactory.Load<BMBoardSection>(section.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(sectionInOtherFactory);

				AssertEquals(0, factoryStatistic.FactoryInternals.AllBusinessObjects.OfType<GlbStaff>().Count());
				AssertEquals(0, factoryStatistic.FactoryInternals.AllBusinessObjects.OfType<ProcessTask>().Count());

				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(2, factoryStatistic.FactoryInternals.AllBusinessObjects.OfType<GlbStaff>().Count());
				AssertEquals(2, factoryStatistic.FactoryInternals.AllBusinessObjects.OfType<ProcessTask>().Count());
			}
		}

		#endregion

		#region GetIncompleteTasks

		public void TestGetIncompleteTask_ShouldUseSimpleQuery_WhenEnabled()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff.PK, displaySequence: 1);

			var section = config.BufferSection;
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflowInComponent = BMSTestHelper.CreateWorkflow(jobHeader, "WF", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflowInComponent, staffCode: staff.GS_Code);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				dataSource.GetIncompleteTasks_ForTest();
				AssertEquals(0, TestConnection.ExecutedCommands.Count(command => command.Contains("-- Simple Board Query")));
			}

			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var provider = new ExperimentalSettingsProvider(section.Board.PK, Factory);
			provider.ExperimentalSettings.Add(new ExperimentalSetting { Key = ExperimentalSettingsProvider.SimpleBoardQueryExperimentalSettingsKey, Value = true.ToString() });
			provider.SaveSettings();

			using (TestConnection.TrackExecutedCommands())
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				dataSource.GetIncompleteTasks_ForTest();
				AssertEquals(1, TestConnection.ExecutedCommands.Count(command => command.Contains("-- Simple Board Query")));
			}
		}

		public void TestGetIncompleteTasks()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";

			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Bucket";
			var board = system.Boards.AddNew();
			board.MB_Name = "Board";
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);

				Factory.Save();

				using (TestConnection.TrackExecutedCommands())
				{
					var isBoardNameAndSectionNameIncludedInCommandText = false;

					var tasks = dataSource.GetIncompleteTasks_ForTest().ToArray();
					strategy.AwaitAll(taskToIgnore: null);
					var partialQueryFormatted = string.Format(CultureInfo.InvariantCulture,
						TestConnection.CurrentDatabase,
						WorkflowLoader.BoardSectionNameSQLAdditionalInfo,
						section.Board.MB_Name,
						section.SectionName);

					foreach (string query in TestConnection.ExecutedCommands)
					{
						if (query.Contains(partialQueryFormatted))
						{
							isBoardNameAndSectionNameIncludedInCommandText = true;
							break;
						}
					}

					Assert("We include additional information (Board Name and Section Name) to the section tasks query to help us to identify slow query.",
						isBoardNameAndSectionNameIncludedInCommandText);

					AssertEquals(6, tasks.Length);
					AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 3");
					AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
					AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 3");
					AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 4");
					AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 3");
					AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 4");
				}
			}
		}

		#endregion

		#region ReleaseGroup

		public void TestGetIncompleteTasks_UseReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G1";
			var releaseGroup1 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "G2";
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup2.FSG_GG_Group = group2.PK;

			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Bucket";
			var board = system.Boards.AddNew();
			board.MB_Name = "Board";
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			workflow1.FH_GG_ReleaseGroup = group1.PK;
			workflow2.FH_GG_ReleaseGroup = group2.PK;
			section.SectionConfiguration.ReleaseGroupPK = group1.PK;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasks_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals("Only tasks from workflows in specified release group should be returned", 2, tasks.Length);
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 3");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
			}
		}

		public void TestReleaseScheduler_ShouldShowAllInReleaseGroup()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var staffInReleaseGroup = Factory.NewWithValidTestData<GlbStaff>();
			config.ReleaseGroup.Staff.Add(staffInReleaseGroup);

			var staffWithCapability = Factory.NewWithValidTestData<GlbStaff>();
			var capabilityInReleaseGroup = Factory.NewWithValidTestData<GlbCapability>();
			staffWithCapability.Capabilities.Add(capabilityInReleaseGroup);
			config.ReleaseGroup.Staff.Add(staffWithCapability);

			var section = VisualBoardsTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 1", config.Bucket, ZDateTime.Now.AddHours(-1), config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow1, string.Empty);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var workflows = dataSource.GetIncompleteWorkflows_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);
				AssertContainsExactElementsInAnyOrder("The task should not be returned, as it isn't assigned to any resource in the release group", Array.Empty<ProcessHeader>(), workflows);

				var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 2", config.Bucket, ZDateTime.Now.AddHours(-1), config.ReleaseGroup.PK);
				var task2 = BMSTestHelper.CreateTask(workflow2, capability: capabilityInReleaseGroup);
				Factory.Save();
				workflows = dataSource.GetIncompleteWorkflows_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);
				AssertContainsExactElementsInAnyOrder("The task should be returned, as it contains a capability containing a staff member within the release group", new[] { workflow2 }, workflows);

				var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 3", config.Bucket, ZDateTime.Now.AddHours(-1), config.ReleaseGroup.PK);
				var task3 = BMSTestHelper.CreateTask(workflow3, staffInReleaseGroup.GS_Code);
				Factory.Save();
				workflows = dataSource.GetIncompleteWorkflows_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);
				AssertContainsExactElementsInAnyOrder("The task should be returned, as it contains a staff member within the release group", new[] { workflow2, workflow3 }, workflows);

				var workflow4 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 4", config.Bucket, ZDateTime.Now.AddHours(-1));
				workflow4.FH_GG_ReleaseGroup = Factory.New<GlbGroup>().PK;
				BMSTestHelper.CreateTask(workflow4, Factory.New<GlbStaff>().GS_Code);
				var workflow5 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 5", config.Bucket, ZDateTime.Now.AddHours(-1));
				BMSTestHelper.CreateTask(workflow5, string.Empty);
				Factory.Save();
				workflows = dataSource.GetIncompleteWorkflows_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);
				AssertContainsExactElementsInAnyOrder("Should only return tasks in release group, with resource in release group, or with capability containing a resource in release group", new[] { workflow2, workflow3 }, workflows);
			}
		}

		public void TestGetIncompleteTasks_NotUsingReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G1";
			var releaseGroup1 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "G2";
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup2.FSG_GG_Group = group2.PK;

			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Bucket";
			var board = system.Boards.AddNew();
			board.MB_Name = "Board";
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, bucket, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			workflow1.FH_GG_ReleaseGroup = group1.PK;
			workflow2.FH_GG_ReleaseGroup = group2.PK;
			section.SectionConfiguration.ReleaseGroupPK = group1.PK;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);

				var tasks = dataSource.GetIncompleteTasks_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals("Only tasks from workflows in specified release group should be returned", 6, tasks.Length);
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 3");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 3");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 3");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 4");
			}
		}

		#endregion

		#region GetIncompleteTasksForCurrentChannels

		public void TestGetIncompleteTasks_ForCurrentChannels_Performance()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");

				var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
				var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

				var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1", releaseGroupPK: config.ReleaseGroup.PK);
				var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, description: "Task 1");

				var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow 2", releaseGroupPK: config.ReleaseGroup.PK);
				var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, description: "Task 2");

				var section = config.BucketSection;
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

				var strategy = new TaskTrackingAsyncBaseStrategy();
				using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
				using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
				{
					var dataSource = GetDataSource(loadedSection);
					var tasks = dataSource.GetIncompleteTasksForCurrentChannels();
					strategy.AwaitAll(taskToIgnore: null);

					var expectedTasks = new[] { task1, task2 };
					AssertContainsExactElementsInAnyOrder(
						"Task1 should be found",
						expectedTasks.Select(t => t.P9_Description),
						tasks.Select(t => t.P9_Description));

					CombineAssertions("SQL Performance", () =>
					{
						AssertEquals("tasks count", 2, tasks.AllTasks.Count);

						var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.Where(t => t.Item1.Contains("FROM dbo.ProcessHeaderLink"));

						foreach (var queryPlan in queryPlans)
						{
							var queryPlanType = queryPlan.Item1.Contains("FP_FH_HeaderTo =")
								? "HeaderTo"
								: queryPlan.Item1.Contains("FP_FH_HeaderFrom =")
									? "HeaderFrom"
									: "??";

							AssertEquals("Should have 2 ProcessHeaderLink query i.e. HeaderTo and HeaderFrom query", 2, queryPlans.Count());
							AssertTestGetIncompleteTasks_ForCurrentChannels_Performance(queryPlanType, queryPlan);
						}
					});
				}
			}
		}

		void AssertTestGetIncompleteTasks_ForCurrentChannels_Performance(string queryPlanType, Tuple<string, List<string>> queryPlan)
		{
			var planAnalyzer = new QueryPlanalyzer(queryPlan.Item2.Single());

			AssertEquals($"{queryPlanType}: should not have index scan", 0, planAnalyzer.IndexScans.Count(indexScan => string.Equals(indexScan.TableName, "ProcessHeaderLink", StringComparison.OrdinalIgnoreCase)));
			AssertEquals($"{queryPlanType}: should not have table scan", 0, planAnalyzer.TableScans.Count(indexScan => string.Equals(indexScan.TableName, "ProcessHeaderLink", StringComparison.OrdinalIgnoreCase)));
			Assert($"{queryPlanType}: should contain FORCESEEK", queryPlan.Item1.Contains("ProcessHeaderLink WITH (FORCESEEK)"));

			switch (queryPlanType)
			{
				case "HeaderTo":
					Assert($"{queryPlanType}: should contain TVP for HeaderTo PKs", queryPlan.Item1.Contains("(FP_FH_HeaderTo in (SELECT Value FROM"));
					break;
				case "HeaderFrom":
					Assert($"{queryPlanType}: should contain TVP for HeaderFrom PKs", queryPlan.Item1.Contains("(FP_FH_HeaderFrom in (SELECT Value FROM"));
					break;
				default:
					Assert("Unknown query", false);
					break;
			}
		}

		public void TestGetIncompleteTasks_ForCurrentChannels_IgnoreMilestonesExecAndTriggers_JobLevelWorkflowCard()
		{
			AssertGetIncompleteTasks_ForCurrentChannels_IgnoreMilestonesExecAndTriggers(CardTypeList.Codes.JobLevelWorkflow);
		}

		public void TestGetIncompleteTasks_ForCurrentChannels_IgnoreMilestonesExecAndTriggers_WorkflowCard()
		{
			AssertGetIncompleteTasks_ForCurrentChannels_IgnoreMilestonesExecAndTriggers(CardTypeList.Codes.Workflow);
		}

		void AssertGetIncompleteTasks_ForCurrentChannels_IgnoreMilestonesExecAndTriggers(string cardType)
		{
			var system = CreateSystem("ORG");

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.CardType = cardType;

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, "workflow1", section.Component, releaseDateTime: ZDateTime.Today, staffCode: staff.GS_Code, lowEstMinutes: 60, description: "task1");
			var task1 = workflow1.Tasks.First();

			var exceptionTask = workflow1.Parent.WorkflowItems.Exceptions.AddNew();
			exceptionTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			exceptionTask.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			exceptionTask.P9_Description = "task1 exception";

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, "workflow2", section.Component, releaseDateTime: ZDateTime.Today, staffCode: staff.GS_Code, lowEstMinutes: 60, description: "task2");
			var task2 = workflow2.Tasks.First();

			var milestoneTask = workflow2.Parent.WorkflowItems.Milestones.AddNew();
			milestoneTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			milestoneTask.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			milestoneTask.P9_Description = "task2 milestone";

			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, "workflow3", section.Component, releaseDateTime: ZDateTime.Today, staffCode: staff.GS_Code, lowEstMinutes: 60, description: "task3");
			var task3 = workflow3.Tasks.First();

			var triggerTask = workflow3.Parent.WorkflowItems.Milestones.AddNew();
			triggerTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			triggerTask.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			triggerTask.P9_Description = "task3 trigger";

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				var expectedTasks = new[] { task1, task2, task3 };
				AssertContainsExactElementsInAnyOrder("Tasks of type Exceptions, Milestones and Triggers should not be found", expectedTasks.Select(t => t.P9_Description), tasks.Select(t => t.P9_Description));
			}
		}

		public void TestGetIncompleteTasks_ForCurrentChannels_IgnoreMilestonesExecAndTriggers()
		{
			var system = CreateSystem("ORG");

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var buffer = CreateBuffer(system);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			var jobWorkflow = CreateJobHeader<OrgHeader>(false);
			var org = jobWorkflow.Parent;

			var workflow1 = BMSTestHelper.CreateProcessHeader(jobWorkflow, buffer, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty))
				.ForEach(t => t.P9_Type = Constants.Workflow.ExceptionType);

			var workflow2 = BMSTestHelper.CreateProcessHeader(jobWorkflow, buffer, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty))
				.ForEach(t => t.P9_Type = Constants.Workflow.MilestoneType);

			var workflow3 = BMSTestHelper.CreateProcessHeader(jobWorkflow, buffer, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff2.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty))
				.ForEach(t => t.P9_Type = Constants.Workflow.WorkflowTriggerType);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals("All of the tasks are Exceptions, Milestones and Triggers, which means that none will be found.", 0, tasks.Length);
			}
		}

		public void TestGetIncompleteTasks_ForCurrentChannels()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Sys";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var buffer = BMSTestHelper.CreateBuffer(system, "SoBuff");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.OverrideChannels = true;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff2.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(3, tasks.Length);

				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 4");
			}
		}

		public void TestGetIncompleteTasks_WithStaffAndReleaseGroups()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Sys";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			group.Staff.Add(staff2);
			group.Staff.Add(staff3);

			var buffer = BMSTestHelper.CreateBuffer(system, "SoBuff");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Swagyolo";
			var header = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 1", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow1, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff1.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow2 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 2", ZDateTime.Now.AddHours(-2));
			BMSTestHelper.CreateTasks(org, workflow2, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff2.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			var workflow3 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 3", ZDateTime.Now.AddHours(-1));
			BMSTestHelper.CreateTasks(org, workflow3, Tuple.Create(ProcessTaskStatusCodeList.Codes.Assigned, 4, staff3.GS_Code.ToString(), ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Open, 3, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Closed, 2, "", ZGuid.Empty), Tuple.Create(ProcessTaskStatusCodeList.Codes.Cancelled, 1, "", ZGuid.Empty));

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Group, group.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(3, tasks.Length);

				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 1 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 2 Task 4");
				AssertCollectionContains(tasks, t => t.P9_NotesAsString == "Workflow 3 Task 4");
			}
		}

		public void TestGetIncompleteTasks_ReleaseSchedulerSection_ShouldGetReleaseGatePlusReleasedWork()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var bucket2 = CreateBucket(config.System, "bucket2");

			LinkComponents(bucket2, config.Buffer, isReleaseGate: false);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", bucket2);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", config.Buffer);

			var task1 = CreateTask(workflow1, config.CCR.GS_Code, 60);
			var task2 = CreateTask(workflow2, config.CCR.GS_Code, 60);
			var task3 = CreateTask(workflow3, config.CCR.GS_Code, 60);

			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			AssertNoErrors(section);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasks_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertContainsExactElementsInAnyOrder(new[] { task1, task3 }, tasks);
			}
		}

		public void TestLoadTicketsWithSuppliedMaxRows()
		{
			const int maxToLoad = 100;
			const int workflowsToCreate = 150;
			const int tasksPerWorkflow = 5;

			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowsToCreate * tasksPerWorkflow);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			var jobHeader = CreateJobHeader<OrgHeader>();
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(config.Buffer, numberOfWorkflows: workflowsToCreate, numberOfTasksPerWorkflow: tasksPerWorkflow, releaseGroup: config.ReleaseGroup, staff: config.CCR);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);

				var currentChannelsTaskMap = dataSource.GetIncompleteTasksForCurrentChannels();
				var unchannelledTaskMap = dataSource.GetIncompleteTasksForUnchannelled();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals("The board should load all the workflows, as a large maximum is set in the registry.", workflowsToCreate, currentChannelsTaskMap.AllWorkflows.Count);
				AssertEquals("The board should load all the tasks, as a large maximum is set in the registry.", workflowsToCreate * tasksPerWorkflow, currentChannelsTaskMap.AllTasks.Count);
				AssertEquals("The board should load all the workflows, as a large maximum is set in the registry.", workflowsToCreate, unchannelledTaskMap.AllWorkflows.Count);
				AssertEquals("The board should load all the tasks, as a large maximum is set in the registry.", workflowsToCreate * tasksPerWorkflow, unchannelledTaskMap.AllTasks.Count);

				BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, maxToLoad);

				section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
				Factory.Save();
				currentChannelsTaskMap = dataSource.GetIncompleteTasksForCurrentChannels();
				unchannelledTaskMap = dataSource.GetIncompleteTasksForUnchannelled();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals("The board should not load more workflows than the maximum in the registry.", maxToLoad + 1, currentChannelsTaskMap.AllWorkflows.Count);
				AssertEquals("The board should not load more workflows than the maximum in the registry.", maxToLoad + 1, unchannelledTaskMap.AllWorkflows.Count);

				section.SectionConfiguration.CardType = CardTypeList.Codes.Task;
				Factory.Save();
				currentChannelsTaskMap = dataSource.GetIncompleteTasksForCurrentChannels();
				unchannelledTaskMap = dataSource.GetIncompleteTasksForUnchannelled();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals("The board should not load more tasks than the maximum in the registry.", maxToLoad + 1, currentChannelsTaskMap.AllTasks.Count);
				AssertEquals("The board should not load more tasks than the maximum in the registry.", maxToLoad + 1, unchannelledTaskMap.AllTasks.Count);
			}
		}

		public void TestLoadJobCardsWithSuppliedMaxRows_ShouldLoadMaximumPlusOne()
		{
			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var resource = CreateStaffInCurrentBranchDept("STF", "Test Staff");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			for (var i = 0; i < 150; i++)
			{
				var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				var task = CreateTask(workflow, resource.GS_Code, 60);
			}

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(section);
				var workflows = dataSource.GetIncompleteWorkflows_ForTest();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(101, workflows.Count);
			}
		}

		public void TestLoadJobCardsWithoutCellsPerSubSection_ShouldReturnEmptyArrayOfProcessHeaders()
		{
			var resource = CreateStaffInCurrentBranchDept("STF", "Test Staff");
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			section.SectionConfiguration.CellsPerSubsection = 0;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(section);
				var workflows = dataSource.GetIncompleteWorkflows_ForTest();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(0, workflows.Count);
			}
		}

		public void TestTaskChannelMapWithUnchannelledTasks()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system, "bucket");
			var buffer = CreateBuffer(system);
			var group = Factory.NewWithValidTestData<GlbGroup>();

			LinkComponents(bucket, buffer, isReleaseGate: true);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflows = BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket, numberOfWorkflows: 10, numberOfTasksPerWorkflow: 5, releaseGroup: group);
			workflows[0].Tasks.First().GetProcessHeader().FH_FC_CurrentComponent = ZGuid.Empty;

			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);
				AssertNoExceptionThrown(delegate
				{
					dataSource.GetIncompleteTasksForUnchannelled();
					strategy.AwaitAll(taskToIgnore: null);
				});
			}
		}

		#endregion

		#region Query Optimisation

		public void TestGetTasksForTagQuery()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "FLP");
			var tagMag1 = BMSTestHelper.CreateTagMagnitude(tagDef, "FLG");
			var tagMag2 = BMSTestHelper.CreateTagMagnitude(tagDef, "FLA");
			var tagMag3 = BMSTestHelper.CreateTagMagnitude(tagDef, "FLB");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader5 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader6 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "YEP", currentComponent: config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "YEP", currentComponent: config.Buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "YEP", currentComponent: config.Buffer);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader4, "YEP", currentComponent: config.Buffer);
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader5, "YEP", currentComponent: config.Buffer);
			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader6, "YEP", currentComponent: config.Buffer);

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow3);
			var task4 = BMSTestHelper.CreateTask(workflow4);
			var task5 = BMSTestHelper.CreateTask(workflow5);
			var task6 = BMSTestHelper.CreateTask(workflow6);

			jobHeader1.AddTag(tagMag1, false);
			workflow2.AddTag(tagMag1, false);
			task3.AddTag(tagMag1, false);

			jobHeader4.AddTag(tagMag1, false);
			workflow4.AddTag(tagMag1, false);
			task4.AddTag(tagMag1, false);

			jobHeader5.AddTag(tagMag2, false);
			workflow5.AddTag(tagMag2, false);
			task5.AddTag(tagMag2, false);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, tagMag1.PK, overrideChannels: true);
			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(config.BufferSection);
				var tasks = dataSource.GetIncompleteTasks_ForTest().Select(t => t.PK).ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(4, tasks.Length);
				AssertCollectionContains(task1.PK, tasks);
				AssertCollectionContains(task2.PK, tasks);
				AssertCollectionContains(task3.PK, tasks);
				AssertCollectionContains(task4.PK, tasks);
				AssertCollectionNotContains("Task 5 has different magnitude", task5.PK, tasks);
				AssertCollectionNotContains("Task 6 doesnt have any magnitude", task6.PK, tasks);
			}

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, tagMag2.PK, overrideChannels: true);
			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(config.BufferSection);
				var tasks = dataSource.GetIncompleteTasks_ForTest().Select(t => t.PK).ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(5, tasks.Length);
				AssertCollectionContains(task1.PK, tasks);
				AssertCollectionContains(task2.PK, tasks);
				AssertCollectionContains(task3.PK, tasks);
				AssertCollectionContains(task4.PK, tasks);
				AssertCollectionContains(task5.PK, tasks);
				AssertCollectionNotContains("Task 6 doesnt have any magnitude", task6.PK, tasks);
			}

			jobHeader6.AddTag(tagMag3, false);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, tagMag3.PK, overrideChannels: true);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(config.BufferSection);
				var tasks = dataSource.GetIncompleteTasks_ForTest().Select(t => t.PK).ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(6, tasks.Length);
				AssertCollectionContains(task1.PK, tasks);
				AssertCollectionContains(task2.PK, tasks);
				AssertCollectionContains(task3.PK, tasks);
				AssertCollectionContains(task4.PK, tasks);
				AssertCollectionContains(task6.PK, tasks);
			}
		}

		public void TestGetIncompleteTasksForCurrentChannels_TagChannels_ShouldHitDbSoftly()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);
			var workflow2 = CreateWorkflow(jobHeader1, "workflow2", config.Buffer);
			var workflow3 = CreateWorkflow(jobHeader2, "workflow3", config.Buffer);

			var task1_1 = CreateTask(workflow1, string.Empty, 60);
			var task1_2 = CreateTask(workflow1, string.Empty, 60);
			var task2_1 = CreateTask(workflow2, string.Empty, 60);
			var task2_2 = CreateTask(workflow2, string.Empty, 60);
			var task3 = CreateTask(workflow3, string.Empty, 60);

			jobHeader1.AddTag(config.DerpyHoovesTag);
			jobHeader2.AddTag(config.DerpyHoovesTag);
			workflow1.AddTag(config.PrincessCelestiaTag);
			task2_1.AddTag(config.PlatinumTag);

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, config.DerpyHoovesTag.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, config.PrincessCelestiaTag.PK);
			var channel3 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Tag, config.PlatinumTag.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var tagChannel1 = viewModel.CreateChannelForTest(config.DerpyHoovesTag);
			var tagChannel2 = viewModel.CreateChannelForTest(config.PrincessCelestiaTag);
			var tagChannel3 = viewModel.CreateChannelForTest(config.PlatinumTag);

			AssertEquals(true, tagChannel1.IsInChannel(task1_1, false));
			AssertEquals(true, tagChannel1.IsInChannel(task1_2, false));
			AssertEquals(true, tagChannel1.IsInChannel(task2_1, false));
			AssertEquals(true, tagChannel1.IsInChannel(task2_2, false));
			AssertEquals(true, tagChannel1.IsInChannel(task3, false));

			AssertEquals(true, tagChannel2.IsInChannel(task1_1, false));
			AssertEquals(true, tagChannel2.IsInChannel(task1_2, false));
			AssertEquals(false, tagChannel2.IsInChannel(task2_1, false));
			AssertEquals(false, tagChannel2.IsInChannel(task2_2, false));
			AssertEquals(false, tagChannel2.IsInChannel(task3, false));

			AssertEquals(false, tagChannel3.IsInChannel(task1_1, false));
			AssertEquals(false, tagChannel3.IsInChannel(task1_2, false));
			AssertEquals(true, tagChannel3.IsInChannel(task2_1, false));
			AssertEquals(false, tagChannel3.IsInChannel(task2_2, false));
			AssertEquals(false, tagChannel3.IsInChannel(task3, false));

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);
			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);

				var tagLnkStartingHits = newFactory.GetTableHitCount(TagLinkSchema.Constants.TableName);
				var tagMagStartingHits = newFactory.GetTableHitCount(TagMagnitudeSchema.Constants.TableName);
				var tagDefStartingHits = newFactory.GetTableHitCount(TagDefinitionSchema.Constants.TableName);
				var processHeaderStartingHits = newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName);
				var processTasksStartingHits = newFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName);

				var tasks = GetIncompleteTasks(dataSource, forCurrentChannels: true);
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(tagLnkStartingHits + 1, newFactory.GetTableHitCount(TagLinkSchema.Constants.TableName));
				AssertEquals("Magnitudes should already be loaded", tagMagStartingHits, newFactory.GetTableHitCount(TagMagnitudeSchema.Constants.TableName));
				AssertEquals("Tag groups should be loaded in one hit (plus UberFactory hit)", tagDefStartingHits + 1, newFactory.GetTableHitCount(TagDefinitionSchema.Constants.TableName));
				AssertEquals("Should load all workflows in one hit, and all job headers in one more", processHeaderStartingHits + 2, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
				AssertEquals(processTasksStartingHits + 2, newFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));

				AssertCollectionContains(tasks, t => t.PK == task1_1.PK);
				AssertCollectionContains(tasks, t => t.PK == task1_2.PK);
				AssertCollectionContains(tasks, t => t.PK == task2_1.PK);
				AssertCollectionContains(tasks, t => t.PK == task2_2.PK);
				AssertCollectionContains(tasks, t => t.PK == task3.PK);
				AssertEquals("All tasks are relevant in this context", 5, tasks.Length);

				channel1 = loadedSection.SectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Single(c => c.MSC_ParentID == config.DerpyHoovesTag.PK);
				channel1.Delete();
				loadedSection.Factory.Save();

				tasks = GetIncompleteTasks(loadedSection, forCurrentChannels: true);
				strategy.AwaitAll(taskToIgnore: null);
				AssertCollectionContains(tasks, t => t.PK == task1_1.PK);
				AssertCollectionContains(tasks, t => t.PK == task1_2.PK);
				AssertCollectionContains(tasks, t => t.PK == task2_1.PK);
				AssertEquals("Only tasks for remaining channels are relevant", 3, tasks.Length);

				channel2 = loadedSection.SectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Single(c => c.MSC_ParentID == config.PrincessCelestiaTag.PK);
				channel2.Delete();
				loadedSection.Factory.Save();

				tasks = GetIncompleteTasks(loadedSection, forCurrentChannels: true);
				strategy.AwaitAll(taskToIgnore: null);
				AssertEquals("Only task for remaining channel is relevant", 1, tasks.Length);
				AssertSamePK(task2_1, tasks[0]);
			}
		}

		public void TestGetIncompleteWorkflowsAndTasksForExcessiveCallsToProcessHeader_WithoutParentHeader()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);

			var previousWorkflow = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);
			CreateTask(previousWorkflow, string.Empty, 60);
			for (int i = 0; i < 5; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader1, "workflow1_" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow, previousWorkflow);
				previousWorkflow = newWorkflow;
			}

			previousWorkflow = CreateWorkflow(jobHeader1, "workflow2", config.Buffer);
			CreateTask(previousWorkflow, string.Empty, 60);
			for (int i = 0; i < 5; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader1, "workflow2_" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(previousWorkflow, newWorkflow);
				previousWorkflow = newWorkflow;
			}

			var previousWorkflow1 = CreateWorkflow(jobHeader1, "workflow3", config.Buffer);
			var previousWorkflow2 = previousWorkflow1;
			CreateTask(previousWorkflow1, string.Empty, 60);
			for (int i = 0; i < 5; i++)
			{
				var newWorkflow1 = CreateWorkflow(jobHeader1, "workflow3_1_" + i.ToString());
				CreateTask(newWorkflow1, string.Empty, 60);
				CreateParentChildRelationship(previousWorkflow1, newWorkflow1);
				previousWorkflow1 = newWorkflow1;
				var newWorkflow2 = CreateWorkflow(jobHeader1, "workflow3_2_" + i.ToString());
				CreateTask(newWorkflow2, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow2, previousWorkflow2);
				previousWorkflow2 = newWorkflow2;
			}

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection);

			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);

				dataSource.GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(3, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
				AssertEquals(12, newFactory.GetTableHitCount(ProcessHeaderLinkSchema.Constants.TableName));
			}
		}

		public void TestGetIncompleteWorkflowsAndTasksForExcessiveCallsToProcessHeader_WithParentHeader()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var previousWorkflow = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);
			CreateTask(previousWorkflow, string.Empty, 60);
			for (int i = 0; i < 7; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader1, "workflow1_" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow, previousWorkflow);
				previousWorkflow = newWorkflow;
			}
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = CreateWorkflow(jobHeader2, "workflow1");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow2 = CreateWorkflow(jobHeader3, "workflow2");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow3 = CreateWorkflow(jobHeader4, "workflow3");
			var jobHeader5 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow4 = CreateWorkflow(jobHeader5, "workflow4");

			CreateTask(workflow1, string.Empty, 60);

			CreateParentChildRelationship(jobHeader1, workflow1);
			CreateParentChildRelationship(jobHeader2, workflow2);
			CreateParentChildRelationship(jobHeader3, workflow3);
			CreateParentChildRelationship(jobHeader4, workflow4);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection);

			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);

				dataSource.GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(10, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
				AssertEquals(30, newFactory.GetTableHitCount(ProcessHeaderLinkSchema.Constants.TableName));
			}
		}

		public void CreateParentChildRelationship(IWorkflow w1, IWorkflow w2)
		{
			var link = Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = w1.PK;
			link.FP_FH_HeaderTo = w2.PK;
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
		}

		public void TestGetIncompleteWorkflowsAndTasks_SimulateArchitectureBoardStructure()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var previousWorkflow = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);
			CreateTask(previousWorkflow, string.Empty, 60);
			for (int i = 0; i < 14; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader1, "workflow1_" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow, previousWorkflow);
				previousWorkflow = newWorkflow;
			}
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = CreateWorkflow(jobHeader2, "workflow1");

			CreateTask(workflow1, string.Empty, 60);

			CreateParentChildRelationship(jobHeader1, workflow1);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection);

			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);

				dataSource.GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(4, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
				AssertEquals(32, newFactory.GetTableHitCount(ProcessHeaderLinkSchema.Constants.TableName));
			}
		}

		public void FAT_TestGetIncompleteWorkflowsAndTasksForExcessiveCallsToProcessHeader()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);
			CreateTask(workflow1, string.Empty, 60);

			var previousWorkflow = workflow1;
			for (int i = 0; i < 25; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader1, "workflow" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow, previousWorkflow);
				previousWorkflow = newWorkflow;
			}

			previousWorkflow = workflow1;
			for (int i = 0; i < 25; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader1, "workflow" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(previousWorkflow, newWorkflow);
				previousWorkflow = newWorkflow;
			}

			previousWorkflow = jobHeader1;
			for (int i = 0; i < 5; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader2, "workflow" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(previousWorkflow, newWorkflow);
				previousWorkflow = newWorkflow;
			}

			previousWorkflow = jobHeader1;
			for (int i = 0; i < 5; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader2, "workflow" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow, previousWorkflow);
				previousWorkflow = newWorkflow;
			}

			previousWorkflow = jobHeader2;
			for (int i = 0; i < 2; i++)
			{
				var newWorkflow = CreateWorkflow(jobHeader3, "workflow" + i.ToString());
				CreateTask(newWorkflow, string.Empty, 60);
				CreateParentChildRelationship(newWorkflow, previousWorkflow);
				previousWorkflow = newWorkflow;
			}

			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(loadedSection);

				dataSource.GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(6, newFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName));
			}
		}

		#endregion

		#region Job-level Workflows

		public void TestGetIncompleteTasks_ForJobLevelWorkflowSection_WhenFilteringByReleaseGroup_ReleaseGroupFilterShouldApplyToWorkflowsWithinJob()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BucketSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			var otherGroup = Factory.NewWithValidTestData<GlbGroup>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2", releaseGroupPK: otherGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2", releaseGroupPK: otherGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3", releaseGroupPK: ZGuid.Empty);

			BMSTestHelper.CreateTask(workflow1_1);
			BMSTestHelper.CreateTask(workflow1_2);
			BMSTestHelper.CreateTask(workflow2);
			BMSTestHelper.CreateTask(workflow3);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(section);
				var workflows = dataSource.GetIncompleteWorkflows_ForTest();
				strategy.AwaitAll(taskToIgnore: null);
				AssertContainsExactElementsInAnyOrder("Only the workflow in the board section's release group should be returned", new[] { workflow1_1 }, workflows);
			}

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(section);
				var workflows = dataSource.GetIncompleteWorkflows_ForTest();
				strategy.AwaitAll(taskToIgnore: null);
				AssertContainsExactElementsInAnyOrder("Only the job-level workflow whose release group is the same as the board section's release group should be returned", new[] { jobHeader1 }, workflows);
			}
		}

		#endregion

		#region Additional Components

		public void TestAdditionalActiveComponents_WhenInactive_ShouldNotIncludeInBoardSection()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var additionalBuffer = BMSTestHelper.CreateBuffer(config.System, "Second Breakfast");

			var section = config.BufferSection;
			BMSTestHelper.CreateAdditionalComponent(section, additionalBuffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflowWithinActiveComponent = BMSTestHelper.CreateWorkflow(jobHeader, "Should be shown on board section", config.Buffer);
			var workflowWithinInactiveComponent = BMSTestHelper.CreateWorkflow(jobHeader, "Should NOT be shown on board section", additionalBuffer);

			var task1 = BMSTestHelper.CreateTask(workflowWithinActiveComponent, resource.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflowWithinInactiveComponent, resource.GS_Code);

			Factory.Save();

			additionalBuffer.FC_IsActive = false;
			Factory.Save();

			AssertEquals("Section should not consider inactive component", "buffer", section.SectionName);
			AssertContainsExactElementsInAnyOrder(new[] { config.Buffer }, section.AllComponents);
			AssertContainsExactElementsInAnyOrder("Should only return tasks within active components", new[] { task1 }, GetIncompleteTasks(section));
		}

		public void TestAdditionalActiveComponents_ShouldIgnoreInactiveRelationships()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var primaryBuffer = BMSTestHelper.CreateBuffer(system, name: "Primary Buffer");
			var section = BMSTestHelper.CreateBoardSection(primaryBuffer);

			var relationship = BMSTestHelper.CreateComponentRelationship(Factory);
			var buffer1 = BMSTestHelper.CreateBuffer(system, name: "Duplicate Buffer");
			var buffer2 = BMSTestHelper.CreateBuffer(system, name: "Related Buffer");
			BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, buffer1);
			BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, buffer2);
			BMSTestHelper.CreateAdditionalComponent(section, relationship);
			BMSTestHelper.CreateAdditionalComponent(section, buffer1);

			AssertContainsExactElementsInAnyOrder("Precondition: Should flatten additional components, removing relationships and duplicates.", new[] { buffer1, buffer2 }, section.AdditionalActiveComponents);

			relationship.FC_IsActive = false;

			AssertContainsExactElementsInAnyOrder("Inactive relationships should be ignored.", buffer1, section.AdditionalActiveComponents);
		}

		public void TestAdditionalActiveComponents_ShouldExcludePrimaryComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var primaryBuffer = BMSTestHelper.CreateBuffer(system, name: "Primary buffer");
			var section = BMSTestHelper.CreateBoardSection(primaryBuffer);

			var relationship = BMSTestHelper.CreateComponentRelationship(Factory);
			var relatedBuffer = BMSTestHelper.CreateBuffer(system, name: "Related Buffer");
			BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, primaryBuffer);
			BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, relatedBuffer);
			BMSTestHelper.CreateAdditionalComponent(section, relationship);

			AssertContainsExactElementsInAnyOrder("Property should flatten additional components, removing relationships and the primary buffer.", new[] { relatedBuffer }, section.AdditionalActiveComponents);
		}

		#endregion

		#region Performance

		public void TestBoardSectionQuery_ForVariousChannels_ShouldNotIncludePredicatesThatWillSlowTheQuery()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var variousStaff = Enumerable.Range(0, 5).Select(i => BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)).ToArray();
			var capability = BMSTestHelper.CreateCapability(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			BMSTestHelper.SetOverriddenSectionName(section, "Barnabandle");

			variousStaff.ForEach(s => BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, s.PK));
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability.PK);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Tag, config.DerpyHoovesTag.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Tag, config.PrincessCelestiaTag.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Tag, config.PrincessLunaTag.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(section);

				using (TestConnection.TrackExecutedCommands())
				{
					var tasks = dataSource.GetIncompleteTasksForCurrentChannels();
					strategy.AwaitAll(taskToIgnore: null);
					var executedCommand = TestConnection.ExecutedCommands.Single(s => s.Contains("Barnabandle"));

					AssertNotContains("We haven't optimised for loading parent table codes, of any kind.", "TGL_ParentTableCode = ", executedCommand, ignoreCase: true);
					AssertNotContains("We haven't optimised for loading parent table codes, of any kind.", "P9_ParentTableCode = ", executedCommand, ignoreCase: true);
				}
			}
		}

		public void TestGetIncompleteTasksForCurrentChannels_ShouldNotCreateActiveBusinessObjectCollections()
		{
			const int numWorkflows = 100;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			for (var i = 0; i < numWorkflows; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Bird Person", releaseGroupPK: config.ReleaseGroup.PK);
				var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code);
			}

			var section = config.BucketSection;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(numWorkflows, tasks.AllTasks.Count);
			}

			var retainedIndexCount = ActiveBusinessObjectCollectionIndexFinderForTest.GetNumberOfRetainedIndexes(ProcessTasksSchema.Constants.TableName, Factory);
			AssertEquals("Should not create an ActiveBusinessObjectCollection for each workflow when loading tasks. Each time we create one and load bizos, we must re-evaluate existing ABOC indexes for the new bizos. This is really expensive.", 0, retainedIndexCount);
		}

		public void TestTagChannel_TaskTickets_ShouldHitTagLinkTableLessThanOncePerTicket()
		{
			AssertTagLinkHitsForTagChannel(CardTypeList.Codes.Task, 1);
		}

		public void TestTagChannel_WorkflowTickets_ShouldHitTagLinkTableLessThanOncePerTicket()
		{
			AssertTagLinkHitsForTagChannel(CardTypeList.Codes.Workflow, 1);
		}

		public void TestTagChannel_JobTickets_ShouldHitTagLinkTableLessThanOncePerTicket()
		{
			AssertTagLinkHitsForTagChannel(CardTypeList.Codes.JobLevelWorkflow, 1);
		}

		void AssertTagLinkHitsForTagChannel(string cardType, int expectedHits)
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var def = BMSTestHelper.CreateTagDefinition(Factory, "ALL", "All the times");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "WHA", "What does these things means?");
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, 10, 3, config.ReleaseGroup, GlbStaff.CurrentUser);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "TAG", mag.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", GlbStaff.CurrentUser.PK);
			config.BufferSection.SectionConfiguration.CardType = cardType;

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(config.BufferSection);
				Factory.ResetDatabaseLoadCount();

				var map = dataSource.GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);
				var actualHits = Factory.GetTableHitCount(TagLinkSchema.Constants.TableName);
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "TagLink should not have been hit once per ticket, and yet... Ticket Type: {0}, Tickets: {1}, TagLink hits: {2}", cardType, expectedHits, actualHits), expectedHits, actualHits);
				AssertEquals(30, map.AllTasks.Count);
			}
		}

		public void TestBoardSectionNameSQLAdditionalInfo_ShouldContainQueryTypeInComments()
		{
			Assert(WorkflowLoader.BoardSectionNameSQLAdditionalInfo.Contains("-- Type: Board Load"));
		}

		#endregion

		#region TaskChannelMap

		public void TestMapForJobSection_WithWorkflowsSpecifedNotInSectionComponents_ShouldNotReport()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", GlbStaff.CurrentUser.PK);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Unrelorkflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "task1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			viewModel.BeforeCardAllocation_ForTest += delegate
				(object sender, EventArgs eventArgs)
			{
				workflow.FH_FC_CurrentComponent = config.Bucket.PK;
				Factory.Save();
			};

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var map = GetDataSource(config.BufferSection).GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);
				CardAllocationMap.UseBizoCardContents.Value = false;

				AssertNoExceptionThrown(
					() => CardAllocationMap.NewAllocationMap(config.BufferSection, viewModel, map));

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		#region Implementation

		static ProcessTask[] GetIncompleteTasks(BMBoardSection section, bool forCurrentChannels = false)
		{
			var dataSource = GetDataSource(section);
			return GetIncompleteTasks(dataSource, forCurrentChannels);
		}

		static ProcessTask[] GetIncompleteTasks(BoardSectionDataSource dataSource, bool forCurrentChannels = false)
		{
			var tasks = forCurrentChannels ? dataSource.GetIncompleteTasksForCurrentChannels() : dataSource.GetIncompleteTasks_ForTest();
			return tasks.ToArray();
		}

		static BoardSectionDataSource GetDataSource(BMBoardSection section)
		{
			var viewModel = BMSTestHelper.CreateViewModel(section);
			return new BoardSectionDataSource(section, viewModel);
		}

		#endregion
	}
}
