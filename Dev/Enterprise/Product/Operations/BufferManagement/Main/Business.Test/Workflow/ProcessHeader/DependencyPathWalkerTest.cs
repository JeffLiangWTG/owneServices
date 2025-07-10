using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using QuikGraph.Algorithms;

namespace Enterprise.BufferManagement.Business.Test
{
	class DependencyPathWalkerTest : BMSTestCaseWithFactory
	{
		public void TestConstructorForJobHeader_ShouldNotExplode()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			AssertNoExceptionThrown(() => CreateWalker(jobHeader));
		}

		public void TestIsOnCriticalPathForJobHeader_ShouldNotExplode()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			var walker = CreateWalker(jobHeader);

			AssertEquals(false, walker.IsOnCriticalPath(jobHeader));
		}

		public void TestChainWithPostreqInAnotherJobWithNoParent()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Nope");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Nope nope");

			var jobHeader2 = Factory.NewWithValidTestData<ProcessJobHeader>();

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(jobHeader2);

			var walker = CreateWalker(jobHeader1);

			Assert(walker.IsOnCriticalPath(workflow1));
			Assert(walker.IsOnCriticalPath(workflow2));
		}

		public void TestLoopedDependency_ShouldNotStackOverflow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow 1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow 2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Workflow 3";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_1 = workflow2.GetOrCreateDependencyLink(workflow1);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			Factory.Save();

			AssertNoExceptionThrown(() => CreateWalker(jobHeader));
		}

		public void TestIsOnCriticalPath()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			workflow4.FH_CompletionStatement = "workflow4";
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			workflow5.FH_CompletionStatement = "workflow5";

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow2.MakePrerequisiteOf(workflow4);
			workflow3.MakePrerequisiteOf(workflow5);
			workflow4.MakePrerequisiteOf(workflow5);

			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			var task2 = workflow2.Parent.WorkflowItems.AddNew();
			task2.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			var task3 = workflow3.Parent.WorkflowItems.AddNew();
			task3.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task3.P9_FH_ProcessHeader = workflow3.PK;

			var task4 = workflow4.Parent.WorkflowItems.AddNew();
			task4.P9_EstDuration = new ZInt(120).GetDateTimeFromMinutes(); // Workflow 4 is the longest, therefore critical path is 1 -> 2 -> 4 -> 5
			task4.P9_FH_ProcessHeader = workflow4.PK;

			var task5 = workflow5.Parent.WorkflowItems.AddNew();
			task5.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task5.P9_FH_ProcessHeader = workflow5.PK;

			Factory.Save();

			var walker = CreateWalker(jobHeader);

			AssertEquals(true, walker.IsOnCriticalPath(workflow1));
			AssertEquals(true, walker.IsOnCriticalPath(workflow2));
			AssertEquals(false, walker.IsOnCriticalPath(workflow3));
			AssertEquals(true, walker.IsOnCriticalPath(workflow4));
			AssertEquals(true, walker.IsOnCriticalPath(workflow5));

			AssertEquals(true, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
			AssertEquals(false, ((IProposedNetworkEntity)workflow3).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow4).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow5).IsOnCriticalPath);
		}

		public void TestIsOnCriticalPath_ClosedTask()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			workflow4.FH_CompletionStatement = "workflow4";
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			workflow5.FH_CompletionStatement = "workflow5";

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow2.MakePrerequisiteOf(workflow4);
			workflow3.MakePrerequisiteOf(workflow5);
			workflow4.MakePrerequisiteOf(workflow5);

			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			var task2 = workflow2.Parent.WorkflowItems.AddNew();
			task2.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			var task3 = workflow3.Parent.WorkflowItems.AddNew();
			task3.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task3.P9_FH_ProcessHeader = workflow3.PK;

			var task4 = workflow4.Parent.WorkflowItems.AddNew();
			task4.P9_EstDuration = new ZInt(120).GetDateTimeFromMinutes(); // Workflow 4 is the longest, therefore critical path is 1 -> 2 -> 4 -> 5
			task4.P9_FH_ProcessHeader = workflow4.PK;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task5 = workflow5.Parent.WorkflowItems.AddNew();
			task5.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task5.P9_FH_ProcessHeader = workflow5.PK;

			Factory.Save();

			var walker = CreateWalker(jobHeader);

			AssertEquals(true, walker.IsOnCriticalPath(workflow1));
			AssertEquals(true, walker.IsOnCriticalPath(workflow2));
			AssertEquals(false, walker.IsOnCriticalPath(workflow3));
			AssertEquals(true, walker.IsOnCriticalPath(workflow4));
			AssertEquals(true, walker.IsOnCriticalPath(workflow5));

			AssertEquals(true, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
			AssertEquals(false, ((IProposedNetworkEntity)workflow3).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow4).IsOnCriticalPath);
			AssertEquals(true, ((IProposedNetworkEntity)workflow5).IsOnCriticalPath);
		}

		#region Implementation

		static DependencyPathWalker CreateWalker(ProcessJobHeader jobHeader, bool includeClosedTaskHours = true)
		{
			// This is primarily here to support the above tests, which may be testing required functionality of DependencyPathWalker.

			var network = ProcessHeaderGraphProvider.CreateGraph(jobHeader, jobHeader.ProcessHeaders, includeClosedTaskHours);
			if (network.Graph.IsDirectedAcyclicGraph())
			{
				var endpointSchedules = network.Graph.Vertices.Where(v => v.Entity != jobHeader && network.Graph.IsOutEdgesEmpty(v)).ToArray();

				return new DependencyPathWalker(endpointSchedules, includeClosedTaskHours);
			}

			return new DependencyPathWalker(System.Array.Empty<ScheduleNode>(), includeClosedTaskHours);
		}

		#endregion
	}
}
