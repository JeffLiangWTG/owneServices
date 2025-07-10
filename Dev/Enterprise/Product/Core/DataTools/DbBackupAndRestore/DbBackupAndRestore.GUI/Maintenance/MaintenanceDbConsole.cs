using System.Diagnostics.CodeAnalysis;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	public class MaintenanceDbConsole : DbConsole
	{
		public MaintenanceDbConsole()
		{
			Initialise();
		}

		void Initialise()
		{
			maintenanceManager = new DbMaintenanceManager();
			maintenanceManager.OnTaskStarted += new InformationEvent(DbTools_OnTaskStarted);
			maintenanceManager.OnSubtaskStarted += new ProgressEvent(DbTools_OnSubtaskStarted);
			maintenanceManager.OnTaskCompleted += new InformationEvent(DbTools_OnTaskCompleted);
			maintenanceManager.OnTaskFailed += new InformationEvent(DbTools_OnTaskFailed);
			maintenanceManager.OnShowInfoMessage += new InformationEvent(DbTools_OnShowInfoMessage);
			maintenanceManager.OnConfirmationPrompt = DbTools_OnConfirmationPrompt;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Top level handling. Catches exception and displays an error to user instead of letting it become an unhandled exception.")]
		protected override void OnDatabaseTaskStart()
		{
			maintenanceManager.DropDatabases(Defaults.Instance.ServerName, Defaults.Instance.DatabaseName, Defaults.Instance.IncludeOperationalDbs, Defaults.Instance.IncludeReferenceDbs, Defaults.Instance.IncludeBiDbs);
		}

		DbMaintenanceManager maintenanceManager;
	}
}
