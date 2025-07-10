using System.Globalization;
using CargoWise.Application;
using Enterprise.DbBackup.Engine;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class DbMaintenanceProxy : IDbMaintenanceProxy
	{
		public static IDbMaintenanceProxy Instance => ObjectFactory.Get<IDbMaintenanceProxy>();

		public void RunFullBackup(ILogger serviceLogger, IEmailNotificationSender emailNotificationSender)
		{
			if (string.IsNullOrEmpty(BackupDirectory))
			{
				LogInvalidBackupDirectoryError(serviceLogger);
			}
			else
			{
				serviceLogger.Log(LogType.Information, "Database Full Backup started.");
				var maintenanceController = new DbMaintenanceController(BackupDirectory, emailNotificationSender, serviceLogger);
				maintenanceController.PerformMaintenance();
			}
		}

		public void RunLogBackup(ILogger serviceLogger, IEmailNotificationSender emailNotificationSender)
		{
			if (string.IsNullOrEmpty(BackupDirectory))
			{
				LogInvalidBackupDirectoryError(serviceLogger);
			}
			else
			{
				serviceLogger.Log(LogType.Information, "Database LOG Backup started.");
				var maintenanceController = new DbMaintenanceController(BackupDirectory, emailNotificationSender, serviceLogger);
				maintenanceController.PerformLogBackup();
			}
		}

		public void RunDifferentialBackup(ILogger serviceLogger, IEmailNotificationSender emailNotificationSender)
		{
			if (string.IsNullOrEmpty(BackupDirectory))
			{
				LogInvalidBackupDirectoryError(serviceLogger);
			}
			else
			{
				serviceLogger.Log(LogType.Information, "Database Differential Backup started.");
				var maintenanceController = new DbMaintenanceController(BackupDirectory, emailNotificationSender, serviceLogger);
				maintenanceController.PerformDifferentialDbBackup();
			}
		}

		public string BackupDirectory => Env.Registry.BackupDirectoryPath.Trim();

		static void LogInvalidBackupDirectoryError(ILogger serviceLogger)
		{
			var logMessage = string.Format(CultureInfo.InvariantCulture, "Backup Directory is blank. Please fix the \'{0}\' registry item under {1}.", RawDataRegistry.Instance.BackupDirectoryPath.Caption, RawDataRegistry.Instance.BackupDirectoryPath.Location());
			serviceLogger.Log(LogType.Error, logMessage);
		}
	}
}
