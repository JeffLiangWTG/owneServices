using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public abstract class LogBatcher<TKey> : ILogBatcher
	{
		protected LogBatcher()
		{
		}

		protected abstract void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable);
		protected abstract TKey GetGroupLogKey(IQueuedLog log);
		protected abstract LogsGroupContext SetContextForLogsGroup(TKey groupKey, IEnumerable<IQueuedLog> queuedLogs);

		void ILogBatcher.AddGroupingFetchHints(IEnumerable<IQueuedLog> enumerable) => AddGroupingFetchHints(enumerable);
		object ILogBatcher.GetGroupLogKey(IQueuedLog log) => GetGroupLogKey(log);
		LogsGroupContext ILogBatcher.SetContextForLogsGroup(object groupKey, IEnumerable<IQueuedLog> queuedLogs) => SetContextForLogsGroup((TKey)groupKey, queuedLogs);
	}
}
