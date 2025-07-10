using System.Threading;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.ServiceTasks.DataTransfer;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AccountingDataCSVExportProcessTask.Code,
	"Accounting csv-file export",
	"ACC",
	typeof(AccountingDataCSVExportProcessTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day"
	)]

namespace Enterprise.Accounting.ServiceTasks.DataTransfer
{
	public class AccountingDataCSVExportProcessTask : ServiceProviderImpl
	{
		public const string Code = "ACE";

		//  The automatic ar/ap trans export batch process will move to service task eventually
		public override void RunTask(CancellationToken token)
		{
			GLTransactionExportProcessor.New().Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
