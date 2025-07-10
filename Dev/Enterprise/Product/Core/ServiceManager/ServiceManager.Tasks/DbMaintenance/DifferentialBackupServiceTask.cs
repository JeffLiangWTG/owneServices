using System.Threading;
using Enterprise.DbBackup.Engine;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.ServiceManager.Tasks.DbMaintenance.DbMaintenanceTasks.DifferentialBackupCode,
	"Differential Backup Service",
	"DBM",
	typeof(Enterprise.ServiceManager.Tasks.DbMaintenance.DifferentialBackupServiceTask),
	MinimumPeriod = "12hours",
	MaximumPeriod = "1week",
	IsReadOnlyForWiseCloudClient = true,
	DefaultScheduleRunEvery = "1day",
	CanRunInAnyBranch = true)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class DifferentialBackupServiceTask : ServiceProviderImpl
	{
		public DifferentialBackupServiceTask()
		{
			emailNotificationSender = new EmailNotificationSender();
		}

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			DbMaintenanceProxy.Instance.RunDifferentialBackup(ServiceLogger, emailNotificationSender);
		}

		readonly EmailNotificationSender emailNotificationSender;
	}
}
