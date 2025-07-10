using System;
using System.Threading;
using Enterprise.DbBackup.Engine;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ScheduleTypeConstants.FullBackupCode, "Database Full Backup Service", "DBM", typeof(FullBackupServiceTask),
	MinimumPeriod = "12hours",
	MaximumPeriod = "1month",
	IsReadOnlyForWiseCloudClient = true,
	MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.Upgrade,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "0hours",
	DefaultScheduleRandomStartOffset = "1440minutes",
	CanRunInAnyBranch = true,
	ActiveByDefault = true
	)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	public class FullBackupServiceTask : ServiceProviderImpl
	{
		public FullBackupServiceTask()
		{
			emailNotificationSender = new EmailNotificationSender();
		}

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			DbMaintenanceProxy.Instance.RunFullBackup(ServiceLogger, emailNotificationSender);
		}

		readonly EmailNotificationSender emailNotificationSender;
	}
}
