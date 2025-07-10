using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DbBackup.Engine;
using Enterprise.DbHealth.Check;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ScheduleTypeConstants))]
[assembly: HostedService(ScheduleTypeConstants.DbHealthCheckCode, "Database Health Check Service", "DBM", typeof(DbHealthCheckServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1month",
	IsReadOnlyForWiseCloudClient = true,
	DefaultScheduleRunEvery = "30days",
	DefaultScheduleStartAtLocal = "0seconds",
	CanRunInAnyBranch = true,
	ActiveByDefault = true
	)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class DbHealthCheckServiceTask : ServiceProviderImpl
	{
		public DbHealthCheckServiceTask()
		{
			emailNotificationSender = new EmailNotificationSender();
		}

		public override void RunTask(CancellationToken token)
		{
			RunDbHealthCheck(token);
		}

		void RunDbHealthCheck(CancellationToken token)
		{
			var dbChecker = new DbCheckRunner();
			var healthWarnings = dbChecker.PerformMainChecks(Db.ServerName, Db.DatabaseName, ServiceLogger);
			EmailWarningListIfChangedAndSaveLastWarningList(healthWarnings, token);

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Database Health Check found {0} warning(s) - Server: {1}, Main DB: {2}", healthWarnings.Count, healthWarnings.DbServerAndInstance, healthWarnings.MainDbName));
		}

		#region Email Warning List

		protected void EmailWarningListIfChangedAndSaveLastWarningList(DbHealthWarningList healthWarnings, CancellationToken token)
		{
			var registryWarningColection = GetRegistryWarningCollectionFromWarningList(healthWarnings, token);

			if (healthWarnings.Count > 0 && SetAcknowledgementFlagsAndCheckIfShouldSendWarningNotification(registryWarningColection))
			{
				string emailSubject = Res.GetString("afa8fcc9-68db-4de4-8b55-08f3d5b12b4e", "Database Health Check ({0})", healthWarnings.GetFormattedLicenceInfo(", "));
				string warningMessage = healthWarnings.ToHtmlMessage();
				SendWarningEmail(emailSubject, warningMessage);
			}

			SaveLastWarningListInfo(registryWarningColection);
		}

		protected virtual void SendWarningEmail(string subject, string body)
		{
			emailNotificationSender.SendNotificationToDatabaseAdministrator(subject, body, ServiceLogger);
		}

		bool IsSameWarning(DbHealthWarningRegistryElement newWarning, DbHealthWarningRegistryElement lastWarning)
		{
			var result = newWarning.WarningType.EqualsIgnoringCase(lastWarning.WarningType) && newWarning.Source.EqualsIgnoringCase(lastWarning.Source);

			if (newWarning.WarningType.EqualsIgnoringCase(DatabaseWarning.RestoreWarning))
			{
				result &= newWarning.Description.EqualsIgnoringCase(lastWarning.Description);
			}

			return result;
		}

		bool SetAcknowledgementFlagsAndCheckIfShouldSendWarningNotification(DbHealthWarningRegistryCollection newRegistryWarningColection)
		{
			int acknowledgedWarnings = 0;
			var lastRegistryWarningColection = LoadLastWarningCollection();

			foreach (DbHealthWarningRegistryElement newWarning in newRegistryWarningColection)
			{
				foreach (DbHealthWarningRegistryElement lastWarning in lastRegistryWarningColection)
				{
					if (IsSameWarning(newWarning, lastWarning))
					{
						if (newWarning.IsAcknowledgeable && lastWarning.IsAcknowledged)
						{
							acknowledgedWarnings++;
							newWarning.SetAcknowledgementInfo(lastWarning.AcknowledgedBy, lastWarning.AcknowledgedDate);
						}

						break;
					}
				}
			}

			// If not all new warnings have been acnowledged or it's the first day of the month => send another notification
			bool result = (acknowledgedWarnings != newRegistryWarningColection.Count || ZDateTime.UtcNow.Day == 1);
			return result;
		}

		DbHealthWarningRegistryCollection GetRegistryWarningCollectionFromWarningList(DbHealthWarningList warningList, CancellationToken token)
		{
			var result = new DbHealthWarningRegistryCollection();

			foreach (DbHealthWarning warning in warningList)
			{
				token.ThrowIfCancellationRequested();
				ZBool isAckowledgeable = (
					warning.WarningType == DatabaseWarning.FileLocationWarning
					|| warning.WarningType == ServerWarning.SqlVersionWarning
					|| warning.WarningType == DiskWarning.DiskSpaceWarning
					|| warning.WarningType == DatabaseWarning.RestoreWarning
				);

				var registryWarning = new DbHealthWarningRegistryElement(warning.Source, warning.WarningType, warning.Description, isAckowledgeable);
				result.Add(registryWarning);
			}

			return result;
		}

		void SaveLastWarningListInfo(DbHealthWarningRegistryCollection registryWarningColection)
		{
			SystemDataRegistry.Instance.LastDbHealthCheckWarningList.SetValue(registryWarningColection);
		}

		DbHealthWarningRegistryCollection LoadLastWarningCollection()
		{
			return SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
		}

		#endregion

		readonly EmailNotificationSender emailNotificationSender;
	}
}
