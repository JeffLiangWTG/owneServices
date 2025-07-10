using CargoWise.Data;
using CargoWise.Data.SqlServer;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class TableRebuildPersisterFactory : ITableRebuildPersisterFactory
	{
		public ITableRebuildPersister Get(DbConnection connection) => new TableRebuildPersister(connection);
	}
}
