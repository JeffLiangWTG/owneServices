#if DEBUG
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.Async
{
	public class SynchronousTaskSchedulerForTest : TaskScheduler
	{
		protected override void QueueTask(Task task)
		{
			TryExecuteTask(task);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return TryExecuteTask(task);
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			yield break;
		}
	}
}
#endif