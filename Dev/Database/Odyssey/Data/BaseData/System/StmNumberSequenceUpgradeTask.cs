using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data.BaseData.System
{
	public class StmNumberSequenceUpgradeTask : IUpgradeTask
	{
		#region IUpgradeTask Members

		public bool IsRequired
		{
			get { return RequiredPopulating(); }
		}

		public void Run()
		{
			var sql = @"
TRUNCATE TABLE dbo.StmNumberSequence;
INSERT dbo.StmNumberSequence (SNS_Number) VALUES (0);

WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L0 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT dbo.StmNumberSequence WITH(TABLOCKX) (SNS_Number)
	SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL))
	FROM L3;

ALTER TABLE dbo.StmNumberSequence REBUILD;
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		public string TaskNameWhenUpgrading
		{
			get { return "Populating dbo.StmNumberSequence"; }
		}

		#endregion // IUpgradeTask Members

		readonly int MinValue;
		const int MaxValue = 1000000;
		const int RowCount = 1000001;

		bool RequiredPopulating()
		{
			var sql = @"
SELECT
	[MinValue] = ISNULL(MIN(SNS_Number), 0),
	[MaxValue] = ISNULL(MAX(SNS_Number), 0),
	[RowCount] = COUNT(*)
FROM
	dbo.StmNumberSequence
";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					return 1 == 2
						|| MinValue != (int)reader["MinValue"]
						|| MaxValue != (int)reader["MaxValue"]
						|| RowCount != (int)reader["RowCount"];
				}
			}

			return true;
		}
	}
}
