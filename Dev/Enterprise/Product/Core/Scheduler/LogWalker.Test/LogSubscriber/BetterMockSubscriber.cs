using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker.Testing
{
	public delegate void ProcessLog(IQueuedLog log);

	public delegate IDisposable SetContextForLogsGroup(IEnumerable<IQueuedLog> queuedLogs);

	public delegate object GroupLogs(IQueuedLog queuedLog);

	[Serializable]
	sealed class BetterMockSubscriber : MockSubscriber
	{
		public BetterMockSubscriber(ProcessLog processLog = null, SetContextForLogsGroup setContext = null, GroupLogs groupLogs = null, string[] tableNamesOverride = null, string[] eventTypesOverride = null, string nameOverride = null)
		{
			this.processLog = processLog;
			this.batcher = new LogBatcher(setContext, groupLogs);
			this.tableNamesOverride = tableNamesOverride ?? Array.Empty<string>();
			this.eventTypesOverride = eventTypesOverride ?? Array.Empty<string>();
			this.nameOverride = nameOverride;
		}

		readonly string[] tableNamesOverride, eventTypesOverride;
		readonly ProcessLog processLog;
		readonly string nameOverride;
		[NonSerialized]
		readonly LogBatcher batcher;

		public override string Name
		{
			get { return nameOverride ?? base.Name; }
		}

		public override string[] TableNames
		{
			get { return tableNamesOverride; }
		}

		public override string[] EventTypes
		{
			get { return eventTypesOverride; }
		}

		protected override ILogBatcher GetLogBatcher() => batcher;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			base.ProcessLogQueueItems(queuedLogs);

			if (processLog != null)
			{
				foreach (var item in queuedLogs)
				{
					processLog(item);
				}
			}
		}
	}
}
