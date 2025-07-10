using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.LogWalker
{
	[Immutable]
	public class DefaultLogBatcher : ILogBatcher
	{
		public static DefaultLogBatcher Instance { get; } = new DefaultLogBatcher();

		DefaultLogBatcher()
		{
		}

		LogsGroupContext ILogBatcher.SetContextForLogsGroup(object groupKey, IEnumerable<IQueuedLog> queuedLogs) => new LogsGroupContext(false);
		void ILogBatcher.AddGroupingFetchHints(IEnumerable<IQueuedLog> enumerable) { }
		object ILogBatcher.GetGroupLogKey(IQueuedLog log) => ZGuid.Empty;
	}
}
