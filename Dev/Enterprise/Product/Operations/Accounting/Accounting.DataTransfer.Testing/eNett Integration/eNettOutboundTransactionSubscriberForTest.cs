using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	[Serializable]
	public class eNettOutboundTransactionSubscriberForTest : eNettOutboundTransactionSubscriber
	{
		public void ProcessLogQueueItemsExposed(IQueuedLog[] queuedLogs)
		{
			ProcessLogQueueItems(queuedLogs);
		}

		public ExceptionHandlingResult TryHandleExceptionCoreExposed(Exception ex, IEnumerable<IQueuedLog> logs, int retryCount)
		{
			return TryHandleExceptionCore(ex, logs, retryCount);
		}

		protected override void Process(IQueuedLog queuedLog, IEnumerable<GlbCompany> companiesWithComPayConfiguration)
		{
			if (companiesWithComPayConfiguration.Any(company => ((string)company?.GC_Name) == "ThrowCriticalException"))
			{
				throw new OutOfMemoryException();
			}
			else
			{
				throw new NullReferenceException();
			}
		}
	}
}
