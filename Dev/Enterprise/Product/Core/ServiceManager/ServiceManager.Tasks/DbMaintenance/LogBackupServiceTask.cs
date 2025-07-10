using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using Enterprise.DbBackup.Engine;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(DbMaintenanceTasks.LogBackupCode, "Database Log Backup Service", "DBM", typeof(LogBackupServiceTask),
	MinimumPeriod = "15minutes",
	MaximumPeriod = "1day",
	IsReadOnlyForWiseCloudClient = true,
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0hours",
	DefaultScheduleRandomStartOffset = "14minutes",
	CanRunInAnyBranch = true,
	ActiveByDefault = true
	)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	public class LogBackupServiceTask : ServiceProviderImpl
	{
		public LogBackupServiceTask() : this(new EmailNotificationSender())
		{
		}

		internal LogBackupServiceTask(IEmailNotificationSender emailNotificationSender)
		{
			this.emailNotificationSender = emailNotificationSender ?? throw new ArgumentNullException(nameof(emailNotificationSender));
		}

		public override void RunTask(CancellationToken token)
		{
			EnsureTaskRunLongerThanOneSecond();

			LogProcess("Start");
			DbMaintenanceProxy.Instance.RunLogBackup(ServiceLogger, emailNotificationSender);
			LogProcess("End");
		}

		void EnsureTaskRunLongerThanOneSecond()
		{
			// LBK runs less than one second which makes it possible for 2 runners runs within the same second
			// and leads to identical timestamp on the backup file as the timestamp is accurate to second. This ensures backup file has a unique timestamp in its name.
			Thread.Sleep(TimeSpan.FromSeconds(1));
		}

		readonly IEmailNotificationSender emailNotificationSender;

		void LogProcess(string message)
		{
			try
			{
				ServiceLogger.Information(message + " process " + Process.GetCurrentProcess().Id);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Information(ex.Message + ": " + message);
			}
		}
	}
}
