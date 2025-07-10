namespace CargoWise.Data
{
	public interface ISqlAlwaysOnAutomation
	{
		void BackupNewDatabase(string newDbName);
		void AddDatabaseToAlwaysOnGroup(string newDbName);
	}
}