using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ServiceTasks.DataTransfer.DebtorBalanceExport;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DebtorBalanceExportProcessorTask.Code,
	"Debtor's Balances Export Service",
	"ACC",
	typeof(DebtorBalanceExportProcessorTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day")
]

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.DebtorBalanceExport
{
	class DebtorBalanceExportProcessorTask : ServiceProviderImpl
	{
		public const string Code = "DBE";

		public override void RunTask(CancellationToken token)
		{
			new Accounting.DataTransfer.DebtorBalanceExport.DebtorBalanceExporter(new BusinessObjectFactory()).Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
