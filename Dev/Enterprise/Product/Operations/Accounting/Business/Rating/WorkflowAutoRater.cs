using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business
{
	public class WorkflowAutoRater : IProcessor
	{
		public WorkflowAutoRater(IBusiness ratingObject, bool autoRateRevenue, bool autoRateCosts, bool excludeConsolLevelCharges)
		{
			this.ratingObject = ratingObject;
			this.autoRateRevenue = autoRateRevenue;
			this.autoRateCosts = autoRateCosts;
			this.excludeConsolLevelCharges = excludeConsolLevelCharges;
		}

		readonly IBusiness ratingObject;
		readonly bool autoRateRevenue;
		readonly bool autoRateCosts;
		readonly bool excludeConsolLevelCharges;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			LoadJobWithParent(ratingObject);

			var starter = new AutoRatingStarter(ratingObject, new NotificationsInteractor(notifications));
			starter.ExecuteAutorating(new AutoRateOptions
			(
				autoRateCost: autoRateCosts,
				autoRateRevenue: autoRateRevenue,
				excludeConsolLevelChargesOnCosting: excludeConsolLevelCharges,
				triggerSource: AutoRateTriggerSource.Workflow
			));
		}

		static void LoadJobWithParent(IBusiness business)
		{
			var jobHeaderParent = business as IJobHeaderParent;
			var jobLoader = new Job.Loader(jobHeaderParent);
			jobLoader.Load(true);
		}

		class NotificationsInteractor : ILoggerWithSession
		{
			public NotificationsInteractor(INotifications notifications)
			{
				this.notifications = notifications;
			}

			readonly INotifications notifications;

			public IDisposable StartRatingSession()
			{
				return null;
			}

			public void Log(LogType type, string message)
			{
				Log(type, message, null);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				switch (type)
				{
					case LogType.Error:
						notifications.AddError(message);
						break;
					case LogType.Warning:
						notifications.AddWarning(message);
						break;
					case LogType.Information:
						notifications.Add(ErrorType.Info, message);
						break;
				}
			}

			public void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell)
			{
			}
		}
	}
}

