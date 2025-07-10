namespace CargoWise.Data.SqlServer
{
	public interface ITableRebuildPersisterFactory
	{
		ITableRebuildPersister Get(DbConnection connection);
	}
}