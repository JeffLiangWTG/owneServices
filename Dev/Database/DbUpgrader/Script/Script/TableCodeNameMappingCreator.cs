using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script
{
	public class TableCodeNameMappingCreator
	{
		#region Constructor

		public TableCodeNameMappingCreator(IUpgradeManager manager, DbConnection upgConnection)
		{
			this.manager = manager;
			this.upgConnection = upgConnection;
		}

		#endregion

		readonly IUpgradeManager manager;
		protected readonly DbConnection upgConnection;
		protected readonly string dbBeingUpgraded;

		#region SuppressResourceStringsCheckRegion

		public void Run()
		{
			manager.ShowInfoMessage("Creating TableCodeNameMapping function");

			upgConnection.ExecuteNonQuery("if (OBJECT_ID('dbo.TableCodeNameMapping') is NOT NULL) DROP FUNCTION [dbo].[TableCodeNameMapping]");

			var itemsSQL = @"
SELECT
	stmt = CONCAT(''
		, CHAR(9), CHAR(9), CHAR(9)
		, '(', QUOTENAME(LEFT(col.name, CHARINDEX('_', col.name) - 1), ''''), ', ', QUOTENAME(tab.name, ''''), ')'
		, IIF(LEAD(col.name) OVER (ORDER BY col.name) is NULL, '', ',')
		)
FROM
	sys.columns     AS col
	JOIN sys.tables AS tab ON tab.object_id = col.object_id
WHERE 1 = 1
	AND tab.is_ms_shipped = 0
	AND SCHEMA_NAME(schema_id) NOT IN ('sys', 'cdc')
	AND tab.name NOT LIKE 'Client%'
	AND tab.name NOT LIKE 'RptDt%'
	AND (col.name LIKE '__[_]PK' OR col.name LIKE '___[_]PK')
ORDER BY
	col.name
";

			var items = new List<string>();
			upgConnection.ExecuteReader(itemsSQL, reader => items.Add((string)reader["stmt"]));

			var createSql = $@"--DROP FUNCTION [dbo].[TableCodeNameMapping]
CREATE FUNCTION [dbo].[TableCodeNameMapping]
(
	@key    varchar(35),
	@isCode bit
)

RETURNS TABLE AS RETURN

WITH
	Map AS (SELECT * FROM (VALUES
{string.Join(System.Environment.NewLine, items)}
		) AS t(Code, Name))
SELECT
	Code, Name
	, Result = IIF(@isCode = 1, Name, Code)
FROM
	Map
WHERE 1 = 2
	OR @isCode = 1 AND Code = @key
	OR @isCode = 0 AND Name = @key
";

			upgConnection.ExecuteNonQuery(createSql);
		}

		#endregion
	}
}
