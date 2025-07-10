using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.DbBackup.Engine;
using Enterprise.DbHealth.Check;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(DbMaintenanceTasks.DbConsistencyCheckCode, "Database Consistency Check Service", "DBM", typeof(DbConsistencyCheckServiceTask),
	IsMandatory = false,
	MinimumPeriod = "1day",
	IsReadOnlyForWiseCloudClient = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "5hours",
	DefaultScheduleRandomStartOffset = "720minutes",
	CanRunInAnyBranch = true,
	ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class DbConsistencyCheckServiceTask : ServiceProviderImpl
	{
		public DbConsistencyCheckServiceTask()
		{
			emailNotificationSender = new EmailNotificationSender();
		}

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			try
			{
				RunDbHealthCheck();
			}
			catch (SqlLockLostException ex)
			{
				ServiceLogger.Information(ex.Message);
			}
		}

		void RunDbHealthCheck()
		{
			var recoveryModel = DbRecoveryModelManager.GetActual(Db.Connection, Db.DatabaseName);

			if (recoveryModel == DbRecoveryModel.Full)
			{
				var dbChecker = new DbCheckRunner();
				var healthWarnings = dbChecker.CheckDatabaseConsistency(Db.ServerName, Db.DatabaseName, ServiceLogger);
				if (healthWarnings.Count > 0)
				{
					string emailSubject = Res.GetString("16b1ac2c-e40c-4c57-912a-188fbc734d06", "Database Consistency Check ({0})", healthWarnings.GetFormattedLicenceInfo(", "));
					string warningMessage = healthWarnings.ToHtmlMessage();
					SendWarningEmail(emailSubject, warningMessage);
					foreach (var warning in healthWarnings)
					{
						ServiceLogger.Log(LogType.Warning, warning.Description);
					}
				}
			}
			else
			{
				var info = "Skip database consistency check, because the recovery model is " + recoveryModel;

				if (!DataUtils.IsWiseTechGlobalDatabaseServer(Db.Connection))
				{
					info += System.Environment.NewLine + "To enable database consistency check, go to Maintain > System > Registry > System > Database > Recovery Model of Databases, change the recovery model from SIMPLE to FULL";
				}

				ServiceLogger.Log(LogType.Information, info);
			}
		}

		protected virtual void SendWarningEmail(string subject, string body)
		{
			emailNotificationSender.SendNotificationToDatabaseAdministrator(subject, body, ServiceLogger);
		}

		readonly EmailNotificationSender emailNotificationSender;
	}
}
