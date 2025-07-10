using System.Collections.Generic;

namespace CargoWise.Data.SqlServer
{
	public interface ITableRebuildPersister
	{
		void Clear();
		IEnumerable<DbSchemaTable> GetTablesToRebuild();
		void MarkTableAsNotRequiringRebuild(DbSchemaTable table);
		bool MarkTableAsRequiringRebuild(DbSchemaTable table);
	}
}