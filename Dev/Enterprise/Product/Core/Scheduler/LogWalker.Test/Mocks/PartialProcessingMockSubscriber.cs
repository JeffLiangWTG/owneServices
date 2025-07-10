using System;
using System.Linq;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Test
{
	public delegate LogSubscriberResult ProcessLogsBatch(IQueuedLog[] queuedLogs);

	[Serializable]
	public class PartialProcessingMockSubscriber : LogSubscriber
	{
		public PartialProcessingMockSubscriber(ProcessLogsBatch logsDelegate = null, string[] eventTypes = null, string[] tableNames = null, string name = null, ILogBatcher batcher = null)
		{
			this.logsDelegate = logsDelegate ?? (l => new LogSubscriberResult(l, Array.Empty<IQueuedLog>()));
			this.eventTypes = eventTypes ?? Events.Customizables.Select(c => c.Code).ToArray();
			this.tableNames = tableNames ?? new string[] { DummyBizoSchema.Constants.TableName };
			this.batcher = batcher;

			this.nameOverride = name;
		}

		readonly ILogBatcher batcher;
		readonly ProcessLogsBatch logsDelegate;
		readonly string[] eventTypes;
		readonly string[] tableNames;
		readonly string nameOverride;

		protected override ILogBatcher GetLogBatcher() => batcher ?? base.GetLogBatcher();
		public override string[] EventTypes => eventTypes;
		public override string[] TableNames => tableNames;
		public override string Name => nameOverride ?? "MockSubscriber";
		public override bool HasDynamicProperties => true;

		protected override LogSubscriberResult ProcessBatch(IQueuedLog[] queuedLogs) => logsDelegate(queuedLogs);
	}
}

