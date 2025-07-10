using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker.Testing
{
	sealed class LogBatcher : LogBatcher<object>
	{
		public LogBatcher(SetContextForLogsGroup setContext, GroupLogs groupLogs)
		{
			this.setContext = setContext;
			this.groupLogs = groupLogs;
		}

		readonly SetContextForLogsGroup setContext;
		readonly GroupLogs groupLogs;

		protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
		protected override object GetGroupLogKey(IQueuedLog log) => groupLogs?.Invoke(log) ?? ZGuid.Empty;
		protected override LogsGroupContext SetContextForLogsGroup(object groupKey, IEnumerable<IQueuedLog> queuedLogs)
		{
			return new LogsGroupContext(false, setContext?.Invoke(queuedLogs));
		}
	}
}
