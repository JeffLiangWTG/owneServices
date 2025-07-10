namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	using System;
	using CargoWise.Data;

	interface IRefDbPreparationStrategy
	{
		string RefDbName { get; }
		int GetVersionFromDatabase();
		void PrepareAndUpgradeDatabase(Action<DbConnection> performUpgradeTasksCallback);
	}
}
