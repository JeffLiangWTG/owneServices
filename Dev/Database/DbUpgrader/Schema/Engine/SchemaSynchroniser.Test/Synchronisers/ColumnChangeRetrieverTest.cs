using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnChangeRetrieverTest : TestCase
	{
		/// <summary>
		/// Columns starting with this prefix are never droppped.
		/// This test ensures this prefix is not used on regular columns.
		/// </summary>
		public void TestNoColumnNameStartsWithCargoWisePreservedColumnPrefix()
		{
			CombineAssertions(
				"Column names starting with CW preserved prefix",
				() =>
					{
						foreach (var dbName in Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.BI))
						{
							var sqlText = $@"
								DECLARE @ColumnNamesStartingWithCwPreservedPrefix nvarchar(max) = '';

								SELECT
									@ColumnNamesStartingWithCwPreservedPrefix += '[{dbName}].[' + s.name + '].[' + t.name + '].[' + c.name + ']' + char(10)
								FROM
									[{dbName}].sys.schemas s
									INNER JOIN [{dbName}].sys.tables t ON t.schema_id = s.schema_id
									INNER JOIN [{dbName}].sys.columns c ON t.object_id = c.object_id
								WHERE
									t.is_ms_shipped = 0
									AND c.name like '{ColumnChangeRetriever.WtgPreservedColumnPrefix}%';

								SELECT @ColumnNamesStartingWithCwPreservedPrefix;
							";
							AssertEquals("", Db.Connection.ExecuteScalar(sqlText).ToString());
						}
					}
			);
		}
	}
}
