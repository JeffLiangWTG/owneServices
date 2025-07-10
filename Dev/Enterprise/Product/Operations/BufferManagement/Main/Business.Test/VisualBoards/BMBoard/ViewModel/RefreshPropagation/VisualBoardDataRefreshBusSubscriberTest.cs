using System.Collections.Generic;
using CargoWise.Pipes.Test;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class VisualBoardDataRefreshBusSubscriberTest : BMSTestCaseWithFactory
	{
		public void TestCreateNewTask_WithoutDispatcher()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			AssertEquals(0, RefreshHandler.Refreshes.Count);
		}

		public void TestCreateNewTask()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			Dispatcher.DispatchAll();
			AssertWorkflowOperationRefresh(new[] { task.PK }, System.Array.Empty<ZGuid>());
		}

		public void TestCreateNewWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Jimminy Jillikers");
			Factory.Save();

			Dispatcher.DispatchAll();
			AssertWorkflowOperationRefresh(System.Array.Empty<ZGuid>(), new[] { workflow.PK, jobHeader.PK });
		}

		public void TestCreateNewTaskAndWorkflow()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Moonoo");
			var task = CreateTask(workflow, "", 40);

			Factory.Save();

			Dispatcher.DispatchAll();
			AssertWorkflowOperationRefresh(new[] { task.PK }, new[] { workflow.PK, jobHeader.PK });
		}

		public void TestSuspend()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Moonoo");
			var task = CreateTask(workflow, "", 40);

			using (Subscriber.Suspend())
			{
				Factory.Save();
			}

			Dispatcher.DispatchAll();
			AssertEquals(0, RefreshHandler.Refreshes.Count);
		}

		[ExpectNoExceptions]
		public void TestMultiDispose()
		{
			Subscriber.Dispose();
			Subscriber.Dispose();
		}

		#region Implementation

		public void AssertWorkflowOperationRefresh(ZGuid[] taskPks, ZGuid[] workflowPks)
		{
			Assert("Expect refresh to have been triggered", RefreshHandler.Refreshes.Count > 0);

			var result = RefreshHandler.Refreshes.Dequeue();
			AssertType<WorkflowUpdatedOperation>(result);

			var op = (WorkflowUpdatedOperation)result;

			AssertContainsExactElementsInAnyOrder("Expect tasks to be in update.", taskPks, op.TaskPKs);
			AssertContainsExactElementsInAnyOrder("Expect workflows to be in update.", workflowPks, op.WorkflowPKs);
		}

		MockRefreshOperationHandler RefreshHandler { get; set; }
		VisualBoardDataRefreshBusSubscriber Subscriber { get; set; }
		MockDispatcher Dispatcher { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			RefreshHandler = new MockRefreshOperationHandler();
			Dispatcher = new MockDispatcher();

			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(board, Dispatcher);

			Subscriber = new VisualBoardDataRefreshBusSubscriber(RefreshHandler, new TestFactoryProvider(Factory, viewModel), Dispatcher);
		}

		protected override void TearDown()
		{
			try
			{
				AssertEquals("Expect no refreshes left unchecked by test assertions.", 0, RefreshHandler.Refreshes.Count);
			}
			finally
			{
				Subscriber.Dispose();
				base.TearDown();
			}
		}

		class MockRefreshOperationHandler : IBoardRefreshable
		{
			public Queue<IBoardRefreshContext> Refreshes { get; } = new Queue<IBoardRefreshContext>();

			#region IBoardRefreshable Implementation

			IEnumerable<IBoardRefreshable> IBoardRefreshable.Children
			{
				get { yield break; }
			}

			IBoardRefreshable IBoardRefreshable.Parent
			{
				get { return null; }
			}

			void IBoardRefreshable.AddAggregator(IBoardRefreshContext context, HashSet<IBoardRefreshOperationAggregator> aggregators)
			{
			}

			void IBoardRefreshable.PerformRefreshAction(IBoardRefreshContext refreshContext)
			{
				Refreshes.Enqueue(refreshContext);
			}

			string IBoardRefreshable.Name => "JimminyJillickers!";

			void IBoardRefreshable.BeforeDataRefresh()
			{
			}

			#endregion
		}

		#endregion
	}
}
