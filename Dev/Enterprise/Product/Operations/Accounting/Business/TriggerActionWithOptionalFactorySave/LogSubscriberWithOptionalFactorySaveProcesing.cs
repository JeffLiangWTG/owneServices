using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	[Serializable]
	public abstract class LogSubscriberWithOptionalFactorySave : LogSubscriber
	{
		protected override ExceptionHandlingResult TryHandleExceptionCore(Exception ex, IEnumerable<IQueuedLog> logs, int retryCount)
		{
			if (ex is LogSubscriberToAbortLogGroupProcessingSilentlyException)
			{
				return ExceptionHandlingResult.SaveAsSuccessfulInNewFactory;
			}

			return base.TryHandleExceptionCore(ex, logs, retryCount);
		}

		protected override void FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactoryCore(BusinessObjectFactory newFactory, Exception ex)
		{
			if (ex is LogSubscriberToAbortLogGroupProcessingSilentlyException abortException)
			{
				abortException.Emails?.ForEach(x => x.Create(newFactory));

				return;
			}

			base.FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactoryCore(newFactory, ex);
		}
	}
}
