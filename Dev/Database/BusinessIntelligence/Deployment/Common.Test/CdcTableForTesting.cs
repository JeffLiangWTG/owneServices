using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	public class CdcTableForTesting : CdcTable
	{
		public CdcTableForTesting(DbConnection connection, string schemaName, string tableName)
			: base(schemaName, tableName)
		{
			this.testConnection = connection;
		}

		public CdcTableForTesting(string schemaName, string tableName)
			: base(schemaName, tableName)
		{
		}

		readonly DbConnection testConnection;

		protected override string FileGroupName => TestingState.IsRunningTests ? null : base.FileGroupName;

		public override IEnumerable<string> GetEligibleCdcColumns(bool includeAuditColumns, bool includeEdwColumns)
		{
			var result = base.GetEligibleCdcColumns(includeAuditColumns, includeEdwColumns);

			if (testConnection is null)
			{
				return result;
			}

			if (result != null && !result.Any())
			{
				string sqlText = string.Format(@"
					SELECT col.name
					FROM
						sys.tables tab
						INNER JOIN sys.schemas sch ON sch.schema_id = tab.schema_id
						INNER JOIN sys.columns col ON col.object_id = tab.object_id
					WHERE
						sch.name = '{0}'
						AND tab.name = '{1}'
						AND (
							col.system_type_id not in (34, 35, 98, 99, 189, 240, 241)
							AND col.max_length > 0
							AND col.max_length <= 8000
						)",
					schemaName,
					tableName);

				result = DataUtils.GetListOfValuesFromQuery(testConnection, sqlText);
			}

			return result;
		}
	}
}
