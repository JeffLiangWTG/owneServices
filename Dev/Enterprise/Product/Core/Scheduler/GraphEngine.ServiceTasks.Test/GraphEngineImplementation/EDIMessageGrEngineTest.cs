using System;
using CargoWise.Data.Utils;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;

namespace Enterprise.GraphEngine.ServiceTasks.Testing
{
	public class EDIMessageGrEngineTestClass : EDIMessageGrEngine<EDIMessage, StmQueueState>
	{
		public static EDIMessageGrEngineTestClass NewForKegGen() => new EDIMessageGrEngineTestClass(GrEngineServiceSetting.KeyGen, 10, 10, 6, 10, null, null);
		public static EDIMessageGrEngineTestClass NewForFlipper() => new EDIMessageGrEngineTestClass(GrEngineServiceSetting.Flipper, 10, 10, 6, 10, null, null);
		public static EDIMessageGrEngineTestClass NewForWorker() => new EDIMessageGrEngineTestClass(GrEngineServiceSetting.Worker, 10, 10, 6, 10, null, null);

		public EDIMessageGrEngineTestClass(GrEngineServiceSetting setting, int getBatchSize, int flipperBatchSize, int getCapacity, int getPreKeyBacklogSize, GrEngineLogOptions getLogOptions, ISqlApplicationLockProvider lockProvider)
			: base(new StmQueueStateFactory(() => getLogOptions), setting, getBatchSize: () => getBatchSize, getFlipperBatchSize: () => flipperBatchSize, getCapacity: () => getCapacity, getPreKeyBacklogSize: () => getPreKeyBacklogSize,
				(setup) => new EDIMessageGraphPreEnqueuer<EDIMessage, StmQueueState>(setup, () => TimeSpan.FromMinutes(1), () => TimeSpan.FromMinutes(1)), lockProvider)
		{
		}

		public const string WorkerServiceTaskCodeForTesting = "T!W";
		protected override string WorkerServiceTaskCode => WorkerServiceTaskCodeForTesting;
		public const string MasterServiceTaskCodeForTesting = "T!M";
		protected override string MasterServiceTaskCode => MasterServiceTaskCodeForTesting;
		public const string KeyGenServiceTaskCodeForTesting = "T!K";
		protected override string KeyGenServiceTaskCode => KeyGenServiceTaskCodeForTesting;
	}
}
