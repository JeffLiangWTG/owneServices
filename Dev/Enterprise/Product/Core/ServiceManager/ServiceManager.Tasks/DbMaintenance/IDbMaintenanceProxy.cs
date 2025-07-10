using Enterprise.DbBackup.Engine;
using Enterprise.Integration;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	interface IDbMaintenanceProxy
	{
		void RunFullBackup(ILogger serviceLogger, IEmailNotificationSender emailNotificationSender);

		void RunLogBackup(ILogger serviceLogger, IEmailNotificationSender emailNotificationSender);

		void RunDifferentialBackup(ILogger serviceLogger, IEmailNotificationSender emailNotificationSender);

		string BackupDirectory { get; }
	}
}
