using System;
using System.Collections.Generic;
using System.Threading;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class StlRetrieverMonthly
	{
		public StlRetrieverMonthly(IUserAttendedStlRetrieverLogger logger)
			: this(logger, new BillingDataCollector(logger))
		{
		}

		public BillingDataCollector Collector { get; private set; }

		protected StlRetrieverMonthly(IUserAttendedStlRetrieverLogger logger, BillingDataCollector billingDataCollector)
		{
			this.logger = logger;
			Collector = billingDataCollector;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CWSupport Only Feature, System Service Task")]
		public IEnumerable<IStlTransaction> CollectAndSend(CancellationToken token, AusydMonthRange dateRange, string code = null)
		{
			try
			{
				Progress("Collecting and sending STL data.");
				return Collector.CollectAndSendMonthData(token, dateRange, code);
			}
			catch (BillingException ex)
			{
				if (!ex.Message.Contains("Thread was being aborted."))
				{
					Fail(ex);
				}

				return null;
			}
		}

		void Progress(string message)
		{
			if (logger != null)
			{
				logger.TaskProgress(message);
			}
		}

		void Fail(Exception ex)
		{
			if (logger == null)
			{
				throw new BillingException("No logger available to log exception: " + ex.Message, ex);
			}
			else
			{
				logger.TaskFailed(ex.Message + "\r\n" + ex.StackTrace + "\r\n");
			}
		}

		protected IUserAttendedStlRetrieverLogger logger;
	}
}
