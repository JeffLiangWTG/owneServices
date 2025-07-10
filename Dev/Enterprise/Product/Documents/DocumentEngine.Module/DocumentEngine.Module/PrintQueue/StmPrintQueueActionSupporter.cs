using System;
using CargoWise.Definitions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.DocumentEngine.Module
{
	class StmPrintQueueActionSupporter : OperationalActionSupporter
	{
		public override string PluralElementNoun => Res.GetString("B688659D-5F13-4CAD-8913-EC75AF3F3EE0", "Print Queues");

		public override string SingularElementNoun => Res.GetString("0BA0A7A8-930B-4DBB-B9C7-664E3F702981", "Print Queue");

		public override Type RootType => typeof(StmPrintQueue);

		public override BusinessContext BusinessContext => BusinessContext.StmPrintQueue;

		public override SecurityCheckpoint BaseCheckpoint => Environment.Env.Security.PrintQueues;
	}
}
