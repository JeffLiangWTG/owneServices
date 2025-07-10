using System;
using CargoWise.Data.Utils;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Registry.Business.eServices;
using Enterprise.Scheduler.GraphEngine;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public class XmlEDIGrEngine : GraphEngine.ServiceTasks.EDIMessageGrEngine<XmlEDIMessage, StmQueueState>
	{
		public XmlEDIGrEngine(GraphEngine.ServiceTasks.GrEngineServiceSetting setting, ISqlApplicationLockProvider lockProvider = null, LoggingInformation logger = null)
			: base(new StmQueueStateFactory(GetLogOptions, logger), setting, GetBatchSize, GetUMIBatchSize, GetCapacity, GetPreKeyBacklogSize, CreateNewPreEnqueuer, lockProvider)
		{
		}

		static int GetBatchSize() => eAdaptorRegistry.Instance.MessagesPerBatch.Value;
		static int GetUMIBatchSize() => eAdaptorRegistry.Instance.UMIMessagesPerBatch.Value;
		static int GetCapacity() => eAdaptorRegistry.Instance.MessageQueueCapacity.Value;
		static int GetPreKeyBacklogSize() => eAdaptorRegistry.Instance.UMKPreEnqueuerMaxBacklogSize.Value;
		static GraphEngine.ServiceTasks.EDIMessageGraphPreEnqueuer<EDIMessage, StmQueueState> CreateNewPreEnqueuer(GrEngineServiceSetup<StmQueueState> setup) => new GraphEngine.ServiceTasks.EDIMessageGraphPreEnqueuer<EDIMessage, StmQueueState>(setup, GetCriticalDuration, GetResetDuration);
		static TimeSpan GetCriticalDuration() => TimeSpan.FromMilliseconds(eAdaptorRegistry.Instance.UMKPreEnqueuerIncreasePerformanceLevelDurationInMillis.Value);
		static TimeSpan GetResetDuration() => TimeSpan.FromMilliseconds(eAdaptorRegistry.Instance.UMKPreEnqueuerResetPerformanceLevelDurationInMillis.Value);

		static GrEngineLogOptions GetLogOptions()
		{
			var registry = eAdaptorRegistry.Instance.ExtendedUniversalLogging.Value;
			return new GrEngineLogOptions
			{
				FirstEnqueueLoad = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIFirstMessageLoad),
				MessageAtFront = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIMessageAtFront),
				ChainStatistics = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIChainStatistics),
				OldestMessage = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIShowOldest),
				EnqueueLoads = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIAllLoads),
				AllKeygenLoads = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMKAllLoads),
				AllKeygenLocks = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMKLocksTaken),
				AllWorkerLoads = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMQAllLoads),
				AllWorkerLocks = registry.GetBoolFromCode(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMQLocksTaken),
			};
		}
		protected override string WorkerServiceTaskCode => UMIServiceTaskWorker.CODE;
		protected override string MasterServiceTaskCode => UMIServiceTask.CODE;
		protected override string KeyGenServiceTaskCode => UMIServiceTaskKeyGen.CODE;
	}
}
