using System.Collections.Generic;
using Enterprise.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(DsbJobBatchCloseServiceTask))]
	public class DsbJobBatchCloseServiceTaskTest : ServiceTaskTestCase<DsbJobBatchCloseServiceTask>
	{
		public void TestJobClosureTaskShouldUseMultipleInstances()
		{
			AssertEquals("Not Allows Multiple Instances", false, GetHostedServiceAttributes()[0].AllowsMultipleInstances);
		}

		public void TestInitialiseAndRunTaskSchedule()
		{
			var task = new DsbJobBatchCloseServiceTask();
			AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(task));

			using (Env.Instance.TemporaryServiceTaskContext(DsbJobBatchCloseServiceTask.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
