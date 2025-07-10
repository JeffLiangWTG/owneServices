using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BoardSectionDataSourceTest : BMSTestCaseWithFactory
	{
		#region ReleaseGroup

		public void TestReleaseScheduler_ShouldOnlyShowTasksForThatBuffer()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var bucket2 = CreateBucket(config.System, "anotherBucket");
			var buffer2 = CreateBuffer(config.System, "anotherBuffer");

			FilterStripsTestHelper.AddStartsWithFilter(config.ComponentLink.FilterRule, "Completion Statement", "Bam");

			var link2 = LinkComponents(bucket2, buffer2, isReleaseGate: true);
			FilterStripsTestHelper.AddStartsWithFilter(link2.FilterRule, "Completion Statement", "Pow");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bam", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Not Bam", config.Bucket);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Pow", bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Not Pow", bucket2);

			var task1 = CreateTask(workflow1, config.CCR.GS_Code, 60);
			var task2 = CreateTask(workflow2, config.CCR.GS_Code, 60);
			var task3 = CreateTask(workflow3, config.CCR.GS_Code, 60);
			var task4 = CreateTask(workflow4, config.CCR.GS_Code, 60);

			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			AssertNoErrors(section);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteWorkflows_ForTest().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, tasks);
			}
		}

		#endregion

		#region GetIncompleteTasksForCurrentChannels

		public void TestGetIncompleteTasks_ForCurrentChannels_NoResultFilterForSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			FilterStripsTestHelper.AddFilterStrips(section.SectionConfiguration.WorkflowFilter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "1=2",
				});

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var job = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateProcessHeaderAndTask(job, buffer, 5, completionStatement: "1");
			var workflow2 = BMSTestHelper.CreateProcessHeaderAndTask(job, buffer, 5, completionStatement: "2");
			var workflow3 = BMSTestHelper.CreateProcessHeaderAndTask(job, buffer, 5, completionStatement: "3");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertEquals(0, tasks.Length);
			}
		}

		public void TestGetIncompleteTasks_ForCurrentChannels_TaskAndWorkflowFilters()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			FilterStripsTestHelper.AddStartsWithFilter(section.WorkflowFilter, "Completion Statement", "Meowbert");
			FilterStripsTestHelper.AddStartsWithFilter(section.TaskFilter, "Description", "Recyclopse");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ILoveBron";
			var job = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(job, "Meowbert", buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, staffCode: staff1.GS_Code, description: "Recyclopse in Meowbert");
			var task2 = BMSTestHelper.CreateTask(workflow1, staffCode: staff1.GS_Code, description: "Poluticorn in Meowbert");

			var workflow2 = BMSTestHelper.CreateWorkflow(job, "Bombastic", buffer);
			var task3 = BMSTestHelper.CreateTask(workflow2, staffCode: staff1.GS_Code, description: "Recyclopse in Bombastic");
			var task4 = BMSTestHelper.CreateTask(workflow2, staffCode: staff1.GS_Code, description: "Poluticorn in Bombastic");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var dataSource = BMSTestHelper.GetDataSource(section);
				var tasks = dataSource.GetIncompleteTasksForCurrentChannels().ToArray();
				strategy.AwaitAll(taskToIgnore: null);

				AssertContainsExactElementsInAnyOrder("Both workflow and task filters should have been applied, and yet...", new[] { "Recyclopse in Meowbert" }, tasks.Select(x => x.P9_Description.ToString()));
			}
		}

		#endregion

		#region ReleaseScheduler Section

		public void TestGetIncompleteTasks_ReleaseSchedulerSection_ShouldConsiderFilterRulesForReleaseGateComponentsOnly()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var otherBucket1 = BMSTestHelper.CreateBucket(config.System, "Stucket");
			var otherBucket2 = BMSTestHelper.CreateBucket(config.System, "Scrucket");
			var otherLink1 = BMSTestHelper.LinkComponents(otherBucket1, config.Buffer, isReleaseGate: true);
			var otherLink2 = BMSTestHelper.LinkComponents(otherBucket2, config.Buffer, isReleaseGate: false); // Not marked as 'release gate' so no workflows in this bucket should be included.

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1", config.Bucket);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2", config.Bucket);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_1", otherBucket1);
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_2", otherBucket1);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow3_1 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3_1", otherBucket2);
			var workflow3_2 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3_2", otherBucket2);

			FilterStripsTestHelper.AddCustomSQLFilterStrip(config.ComponentLink.FilterRule, $"FH_PK = '{workflow1_1.PK}'");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(otherLink1.FilterRule, $"FH_PK = '{workflow2_1.PK}'");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(otherLink2.FilterRule, $"FH_PK = '{workflow3_1.PK}'");

			var releaseScheduler = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			var task1_1 = BMSTestHelper.CreateTask(workflow1_1, config.CCR.GS_Code);
			var task1_2 = BMSTestHelper.CreateTask(workflow1_2, config.CCR.GS_Code);
			var task2_1 = BMSTestHelper.CreateTask(workflow2_1, config.CCR.GS_Code);
			var task2_2 = BMSTestHelper.CreateTask(workflow2_2, config.CCR.GS_Code);
			var task3_1 = BMSTestHelper.CreateTask(workflow3_1, config.CCR.GS_Code);
			var task3_2 = BMSTestHelper.CreateTask(workflow3_2, config.CCR.GS_Code);

			Factory.Save();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				var map = GetDataSource(releaseScheduler).GetIncompleteTasksForCurrentChannels();
				strategy.AwaitAll(taskToIgnore: null);
				var matchingTasks = new[] { task1_1, task2_1 };

				AssertContainsExactElementsInAnyOrder("The only matching tasks should be the ones whose workflows are in components which have links into the buffer that are marked as 'release gate' links, and match that link's filter rules.", matchingTasks, map);
			}
		}

		#endregion

		#region Implementation

		static BoardSectionDataSource GetDataSource(BMBoardSection section)
		{
			var viewModel = BMSTestHelper.CreateViewModel(section);
			return new BoardSectionDataSource(section, viewModel);
		}

		#endregion
	}
}
