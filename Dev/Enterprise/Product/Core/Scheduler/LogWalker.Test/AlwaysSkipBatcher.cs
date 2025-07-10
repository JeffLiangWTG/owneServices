using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker.Test
{
	sealed class AlwaysSkipBatcher : LogBatcher<ZGuid>
	{
		protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
		protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.SJ_ParentID;
		protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs) => new LogsGroupContext(true);
	}
}
