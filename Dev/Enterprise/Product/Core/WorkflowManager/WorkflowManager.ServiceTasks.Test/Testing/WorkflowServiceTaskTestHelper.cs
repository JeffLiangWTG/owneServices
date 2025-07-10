using System;
using System.Threading;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.ServiceManager.Tasks.LogWalker;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class WorkflowServiceTaskTestHelper : IWorkflowServiceTaskTestHelper
	{
		string IWorkflowServiceTaskTestHelper.RunFieldChangeTriggerProcessor() => RunFieldChangeTriggerProcessor();

		string IWorkflowServiceTaskTestHelper.RunLogWalker() => RunLogWalker();

		public static string RunFieldChangeTriggerProcessor()
		{
			return RunCore(() =>
			{
				var logger = new SimpleLogger();
				new WorkflowModifiedFieldChangeTriggerServiceTask() { ServiceLogger = logger }.RunTask(CancellationToken.None);
				return logger.ToString();
			});
		}

		public static string RunLogWalker()
		{
			return RunCore(() =>
			{
				var purger = LogWalkerRunner.Purge();
				var master = LogWalkerRunner.Master();
				var runner = LogWalkerRunner.Default();
				var logger = new SimpleLogger();

				purger.Process(logger, CancellationToken.None);
				master.Process(logger, CancellationToken.None);
				using (Env.Instance.TemporaryServiceTaskContext(LogWalkerServiceTask.CODE, true))
				{
					runner.Process(logger, CancellationToken.None);
				}

				return logger.ToString();
			});
		}

		static T RunCore<T>(Func<T> runAction)
		{
			var originalUserInteractiveValue = Globals.IsUserInteractive;

			Globals.IsUserInteractive = false; // Simulate how these tasks are always run in non-user interactive mode.
			try
			{
				using (Env.Instance.TemporaryServiceTaskContext("LWK", true))
				{
					using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
					{
						return runAction();
					}
				}
			}
			finally
			{
				Globals.IsUserInteractive = originalUserInteractiveValue;
			}
		}
	}
}
