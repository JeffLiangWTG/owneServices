using System.Threading;
using CargoWise.Types;
using Enterprise.Client.SWT;
using Enterprise.Client.SWT.ServiceTask;
using Enterprise.ClientSharedComponents.ServiceTasks;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	SWTConstants.NotYetArrivedRepServiceTaskCode,
	"Not Yet Arrived Report Deliver Process",
	"CSP",
	typeof(SWTNotYetArrivedRepServiceTask),
	MinimumPeriod = ServiceTaskOptionsInitialiser.MinimumIntervalDuration,
	DefaultScheduleRunEvery = "1day"
	)]
namespace Enterprise.Client.SWT.ServiceTask
{
	class SWTNotYetArrivedRepServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());
			if (IsEnvironmentDataValid(buffer))
			{
				buffer.Notify(new InfoNotification("'Not Yet Arrived' report started at " + ZDateTime.Now.ToShortTimeString()));
				ReportRunner.RunBatchReports(buffer, token);
				buffer.Notify(new InfoNotification("...'Not Yet Arrived' report finished"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		bool IsEnvironmentDataValid(NotificationBuffer notify)
		{
			ZStringBuilder message = new ZStringBuilder();

			var consignors = new OrgHeaderCodeListCollection(SWTDataRegistry.Instance.ConsignorsList.Value);
			if (consignors.Count == 0)
			{
				message.Append("Consignor Organisations");
			}
			if (!message.IsEmpty)
			{
				notify.Notify(new WarningNotification("Please set up the Registry items required for the 'Not Yet Arrived' Report process in Admin->System->Registry->SWT Client Extensions->Documents->Not yet Arrived Report: " + message.ToStringWithNewLineBetweenAppends()));
			}
			return message.IsEmpty;
		}

		BatchReportRunner ReportRunner
		{
			get { return reportRunner ?? (reportRunner = new BatchReportRunner()); }
		}
		BatchReportRunner reportRunner;
	}
}
