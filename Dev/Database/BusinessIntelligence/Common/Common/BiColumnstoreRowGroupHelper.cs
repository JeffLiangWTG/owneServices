using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common.Integration;
using CargoWise.Data;

namespace CargoWise.Bi.Common
{
	public class BiColumnstoreRowGroupHelper : IBiColumnstoreRowGroupHelper
	{
		public DataTable GetColumnstoreRowGroupPhysicalState(AdminConnection auditConnection)
		{
			var query = String.Format(CultureInfo.InvariantCulture, @"
						SELECT
						CSRowGroups.state_desc RowGroupState,
						t.name TableName
						FROM [{0}].sys.indexes AS i
						JOIN [{0}].sys.tables t ON t.object_id = i.object_id
						JOIN [{0}].sys.dm_db_column_store_row_group_physical_stats AS CSRowGroups
							ON i.object_id = CSRowGroups.object_id AND i.index_id = CSRowGroups.index_id
						Where State IN(0, 2)
						ORDER BY t.name",
					Db.AuditDatabaseName);

			using (var cmd = auditConnection.Command(query))
			{
				var queryResult = DataUtils.GetDataTableFromCommand(cmd);
				return queryResult;
			}
		}
	}
}
