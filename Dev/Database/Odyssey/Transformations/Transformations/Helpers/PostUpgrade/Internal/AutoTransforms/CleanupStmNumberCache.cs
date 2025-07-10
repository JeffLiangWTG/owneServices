using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Internal.AutoTransforms
{
	class CleanupStmNumberCache
	{
		public CleanupStmNumberCache(IUpgradeManager manager = null)
		{
			this.manager = manager;
		}

		bool CanRun
		{
			get
			{
				return
					DataUtils.ObjectExists(Db.Connection, "dbo.StmNums")
					&& DataUtils.ObjectExists(Db.Connection, "dbo.StmNumberCache")
					;
			}
		}

		public void Run()
		{
			if (CanRun)
			{
				manager?.StartTask("Cleaning up old StmNumberCache");
				StmNumberCacheCleanup();
			}
		}

		void StmNumberCacheCleanup()
		{
			var sql = @"
DECLARE
	@Nums table
	(
		ID    int NOT NULL PRIMARY KEY,
		Value bigint NOT NULL
	)

-- Behave like old version
DELETE FROM dbo.STMNUMBERCACHE WHERE SG_IsUsed = 1;

;WITH
	step_1 AS (
			SELECT
				SG_SN, SG_Value
				, _next  = ISNULL(MIN(SG_Value) OVER (PARTITION BY SG_SN ORDER BY SG_Value ROWS BETWEEN 1 FOLLOWING AND 1 FOLLOWING), 0)
			FROM
				dbo.StmNumberCache
		)
	, step_2 AS (
			SELECT d.*
				, new_value = IIF (SG_Value < IIF(_next = 0, SN_Value, _next) - 1, IIF(_next = 0, SN_Value, _next), _first)
			FROM
				step_1 AS d
				JOIN
				(
					SELECT
						SG_SN
						, _first = MIN(SG_Value)
					FROM
						dbo.StmNumberCache
					GROUP BY
						SG_SN
				) AS c ON c.SG_SN = d.SG_SN

				JOIN dbo.StmNums ON SN_ID = c.SG_SN
		)
	, step_3 AS (
			SELECT
				SG_SN
				, new_value = MAX(new_value)
			FROM
				step_2
			GROUP BY
				SG_SN
		)
UPDATE target SET
	SN_Value = source.new_value
OUTPUT
	INSERTED.SN_ID,
	INSERTED.SN_Value
	INTO
		@Nums
FROM
	step_3           AS source
	JOIN dbo.StmNums AS target ON target.SN_ID = source.SG_SN
WHERE 1=1
	AND target.SN_Value > source.new_value

OPTION (RECOMPILE, MERGE JOIN)

DELETE target
FROM
	@Nums                   AS source
	JOIN dbo.StmNumberCache AS target ON target.SG_SN = source.ID AND target.SG_Value >= source.Value
OPTION (RECOMPILE)

SELECT
	@@ROWCOUNT

";

			var performed = Db.Connection.ExecuteScalar<int>(sql);
			manager?.StartSubtask(string.Format(CultureInfo.InvariantCulture, "{0,11:N0} rows removed.", performed));
		}

		readonly IUpgradeManager manager;
	}
}
