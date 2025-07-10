using System;
using Enterprise.Integration.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UserIdleWorkerLeakListener : BaseTestListener, IUserIdleWorkerLeakListener
	{
		public override void StartTest(TestCase test, DateTime startTime)
		{
			base.StartTest(test, startTime);
			queuedWorkItemCountBeforeTest = UserIdleWorker.QueuedWorkItemCount;
			UserIdleWorker.StackTraceIsEnabled = false;
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);

			Assertion.Assert(
				"There must be no new UserIdleWorker work items remaining after a test.\r\n" +
				"Make sure a Control is passed to the workItemOwner parameter of QueueWorkItem\r\n" +
				"so any stray work items are discarded when the control is disposed.",
				UserIdleWorker.QueuedWorkItemCount <= queuedWorkItemCountBeforeTest);

			UserIdleWorker.Flush();
			UserIdleWorker.StackTraceIsEnabled = true;
		}

		public override void EndAllTests(DateTime endTime)
		{
			base.EndAllTests(endTime);
			UserIdleWorker.StackTraceIsEnabled = false;
		}

		int queuedWorkItemCountBeforeTest;
	}
}
