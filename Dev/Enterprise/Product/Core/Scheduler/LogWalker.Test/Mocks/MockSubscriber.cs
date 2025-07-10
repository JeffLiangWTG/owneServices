using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Test
{
	public delegate void ProcessLogs(IQueuedLog[] queuedLogs);

	[Serializable]
	public class MockSubscriber : LogSubscriber
	{
		public MockSubscriber(ProcessLogs logsDelegate = null, string[] eventTypes = null, string[] tableNames = null, string name = null, ILogBatcher batcher = null,
			Func<Exception, IEnumerable<IQueuedLog>, int, ExceptionHandlingResult> tryHandleException = null,
			Action<BusinessObjectFactory, Exception> finalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory = null)
		{
			this.logsDelegate = logsDelegate ?? (_ => { });
			this.eventTypes = eventTypes ?? Events.Customizables.Select(c => c.Code).ToArray();
			this.tableNames = tableNames ?? new string[] { DummyBizoSchema.Constants.TableName };
			this.batcher = batcher;

			this.nameOverride = name;

			TryHandleException = tryHandleException;
			FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory = finalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory;
		}

		readonly ILogBatcher batcher;
		readonly ProcessLogs logsDelegate;
		readonly string[] eventTypes;
		readonly string[] tableNames;
		readonly string nameOverride;

		new Func<Exception, IEnumerable<IQueuedLog>, int, ExceptionHandlingResult> TryHandleException { get; }
		new Action<BusinessObjectFactory, Exception> FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory { get; }

		protected override ILogBatcher GetLogBatcher() => batcher ?? base.GetLogBatcher();
		public override string[] EventTypes => eventTypes;
		public override string[] TableNames => tableNames;
		public override string Name => nameOverride ?? "MockSubscriber";
		public override bool HasDynamicProperties => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs) => logsDelegate(queuedLogs);

		protected override ExceptionHandlingResult TryHandleExceptionCore(Exception ex, IEnumerable<IQueuedLog> logs, int retryCount)
		{
			if (TryHandleException != null)
			{
				return TryHandleException(ex, logs, retryCount);
			}

			return base.TryHandleExceptionCore(ex, logs, retryCount);
		}

		protected override void FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactoryCore(BusinessObjectFactory newFactory, Exception ex)
		{
			if (FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory != null)
			{
				FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory(newFactory, ex);
			}
			else
			{
				base.FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactoryCore(newFactory, ex);
			}
		}
	}
}
