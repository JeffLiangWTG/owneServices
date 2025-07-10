using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public interface ILogBatcher
	{
		object GetGroupLogKey(IQueuedLog log);
		void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumerable);
		LogsGroupContext SetContextForLogsGroup(object groupKey, IEnumerable<IQueuedLog> queuedLogs);
	}
}
