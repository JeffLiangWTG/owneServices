namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	internal interface ISharedRefDbStrategy
	{
		string CurrentDatabaseName { get; }
	}
}
