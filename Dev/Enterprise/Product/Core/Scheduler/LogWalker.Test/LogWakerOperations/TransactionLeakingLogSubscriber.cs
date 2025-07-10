using System;
using CargoWise.Data;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker.Test
{
	[Serializable]
	class TransactionLeakingLogSubscriber : MockEventSubscriber
	{
		public TransactionLeakingLogSubscriber(string name) : base(name)
		{
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			base.ProcessLogQueueItems(queuedLogs);
			Db.Connection.ExecuteScalar("BEGIN TRAN");
													   // And leak
		}
	}
}
