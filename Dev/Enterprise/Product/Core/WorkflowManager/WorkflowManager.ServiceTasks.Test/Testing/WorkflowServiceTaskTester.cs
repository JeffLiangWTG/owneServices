using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	public class WorkflowServiceTaskTester : IProcessorTest
	{
		public WorkflowServiceTaskTester() : this(new WorkflowModifiedFieldChangeTriggerProcessor())
		{
		}

		internal WorkflowServiceTaskTester(WorkflowModifiedFieldChangeTriggerProcessor wmfTask)
		{
			Logs = new NotificationLoggerWomboComboForTest();
			WmfTask = wmfTask;
		}

		public NotificationLoggerWomboComboForTest Logs { get; }
		internal WorkflowModifiedFieldChangeTriggerProcessor WmfTask { get; }

		public void Process(INotifications notifications, CancellationToken token)
		{
			using (Logs.WithLogger(notifications))
			{
				RunChain(token);
			}
		}

		public void RunChain()
		{
			RunChain(CancellationToken.None);
		}

		public void RunChain(CancellationToken token)
		{
			WmfTask.Process(Logs, token);
			LogWalkerRunner.Purge().Process(new LogWalkerCategoryLogger(Logs), token);
			LogWalkerRunner.Master().Process(new LogWalkerCategoryLogger(Logs), token);
			LogWalkerRunner.Default().Process(new LogWalkerCategoryLogger(Logs), token);
		}
	}
}
