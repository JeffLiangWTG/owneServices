using System.Collections.Generic;

namespace Enterprise.LogWalker
{
	class MasterOperationsManager : OperationsManager
	{
		protected override IEnumerable<ILogWalkerOperation> GetOrderedOperations()
		{
			yield return GetQueue();
		}

		QueueLogs queuedLogs;
		protected virtual QueueLogs GetQueue() => queuedLogs ?? (queuedLogs = new QueueLogs());
	}
}
