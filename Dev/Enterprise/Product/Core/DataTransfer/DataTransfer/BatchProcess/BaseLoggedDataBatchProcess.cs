using System.Collections;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.BatchProcessor
{
	public abstract class BaseLoggedDataBatchProcess : BatchProcess
	{
		public BaseLoggedDataBatchProcess(LoggingInformation logger)
		{
			this.Logger = logger;
		}

		public override string HumanReadableName
		{
			get { return Res.GetString("335ec7d0-738f-4cf3-9058-6744aa421fbb", "Log Walker"); }
		}

		#region Execute

		protected override void Execute(CancellationToken token)
		{
			if (Listeners != null)
			{
				ZDateTime currentHighWaterMark = HighWaterMark;
				ZDateTime endDateTimeToLagBy = ZDateTime.UtcNow.AddMinutes(MinutesToLagBy * -1);
				ZDateTime endDateTimeInFuture = currentHighWaterMark.AddHours(NumberOfHoursInFuture);
				ZDateTime endDateTime = (endDateTimeToLagBy > endDateTimeInFuture) ? endDateTimeInFuture : endDateTimeToLagBy;

				#region Log Collection if In test mode
#if DEBUG
				LogsLastExecute = null;
				ArrayList logsInRun = new ArrayList();
#endif
				#endregion

				if (currentHighWaterMark < endDateTime)
				{
					FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(FactoryProvider, CreateFilter(endDateTime), typeof(StmALog));
					int count = 0;
					foreach (StmALog log in reader)
					{
						token.ThrowIfCancellationRequested();
						#region Add Logs to Collection
#if DEBUG
						logsInRun.Add(log);
#endif
						#endregion

						foreach (LogBatchListener listener in Listeners)
						{
							token.ThrowIfCancellationRequested();
							listener.Match(log, Notifications);
						}

						if ((++count % NumberOfLogToUpdateHighWaterMark) == 0)
						{
							HighWaterMark = log.SL_PostedTimeUtc;
						}
					}

					HighWaterMark = endDateTime;
				}

				#region InTestMode Populate LogsLastExecute
#if DEBUG
				LogsLastExecute = (StmALog[])logsInRun.ToArray(typeof(StmALog));
#endif
				#endregion
			}
		}

		#endregion

		#region Implementation

		protected virtual int NumberOfHoursInFuture
		{
			get { return 2; }
		}

		protected virtual int NumberOfLogToUpdateHighWaterMark
		{
			get { return 400; }
		}

		protected virtual int MinutesToLagBy
		{
			get { return 10; }
		}

		protected ZQuery CreateFilter(ZDateTime startProcessingTime)
		{
			ZQuery result = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, HighWaterMark); //select logs between last hwm
			result.AddToFilter(JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, startProcessingTime); //and Now.
			result.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name;
			result.IsNoLock = false;
			return result;
		}

		protected abstract ZDateTime HighWaterMark
		{
			get;
			set;
		}

		protected abstract ILogBatchListenerProxy[] Listeners
		{
			get;
		}

		protected BusinessObjectFactory Factory
		{
			get { return FactoryProvider.Current; }
		}

		#region FactoryProvider

		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (fFactoryProvider == null)
				{
					BusinessObjectFactory initialFactory = new BusinessObjectFactory();
					initialFactory.RefreshEnabled = false;
					fFactoryProvider = new BusinessObjectFactoryProvider(initialFactory);
				}
				return fFactoryProvider;
			}
		}

		BusinessObjectFactoryProvider fFactoryProvider;

		#endregion

		#region Notify

		protected BatchProcessorNotificationBufferBridge Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new BatchProcessorNotificationBufferBridge(Logger);
				}
				return fNotifications;
			}
		}

		BatchProcessorNotificationBufferBridge fNotifications;

		#endregion

		#endregion

		#region TestMode Helpers

#if DEBUG
		public StmALog[] LogsLastExecute;
#endif

		#endregion
	}
}
