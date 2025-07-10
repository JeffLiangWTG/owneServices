using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class StatisticsBugsTest : TestCase
	{
		[DatCapabilityRequirement("SQL2019")]
		public void TestStats_Sql2019()
		{
			PrepareData(new SqlServerVersionNumber("15.00.0000.0"));

			var expected = new List<string>(70);
			expected.Add("--------------------------------------------------------------------"); //  0
			expected.Add("|   Auto-stats settings   | Trace flags |        | PK    | auto    |"); //  1
			expected.Add("|-------------------------|-------------| Action | index | stats   |"); //  2
			expected.Add("| Create | Update | Async | CE  |  4199 |        | hint  | created |"); //  3
			expected.Add("|--------|--------|-------|-----|-------|--------|-------|---------|"); //  4
			expected.Add("|    OFF |    OFF |   OFF | old |   OFF | Update |    no |      no |"); //  5
			expected.Add("|    OFF |    OFF |   OFF | old |   OFF | Update |   yes |      no |"); //  6
			expected.Add("|    OFF |    OFF |   OFF | old |   OFF | Delete |    no |      no |"); //  7
			expected.Add("|    OFF |    OFF |   OFF | old |   OFF | Delete |   yes |      no |"); //  8
			expected.Add("|    OFF |    OFF |   OFF | old |    ON | Update |    no |      no |"); //  9
			expected.Add("|    OFF |    OFF |   OFF | old |    ON | Update |   yes |      no |"); // 10
			expected.Add("|    OFF |    OFF |   OFF | old |    ON | Delete |    no |      no |"); // 11
			expected.Add("|    OFF |    OFF |   OFF | old |    ON | Delete |   yes |      no |"); // 12
			expected.Add("|    OFF |    OFF |   OFF | new |   OFF | Update |    no |      no |"); // 13
			expected.Add("|    OFF |    OFF |   OFF | new |   OFF | Update |   yes |      no |"); // 14
			expected.Add("|    OFF |    OFF |   OFF | new |   OFF | Delete |    no |      no |"); // 15
			expected.Add("|    OFF |    OFF |   OFF | new |   OFF | Delete |   yes |      no |"); // 16
			expected.Add("|    OFF |    OFF |   OFF | new |    ON | Update |    no |      no |"); // 17
			expected.Add("|    OFF |    OFF |   OFF | new |    ON | Update |   yes |      no |"); // 18
			expected.Add("|    OFF |    OFF |   OFF | new |    ON | Delete |    no |      no |"); // 19
			expected.Add("|    OFF |    OFF |   OFF | new |    ON | Delete |   yes |      no |"); // 20
			expected.Add("|    OFF |     ON |   OFF | old |   OFF | Update |    no |      no |"); // 21
			expected.Add("|    OFF |     ON |   OFF | old |   OFF | Update |   yes |      no |"); // 22
			expected.Add("|    OFF |     ON |   OFF | old |   OFF | Delete |    no |      no |"); // 23
			expected.Add("|    OFF |     ON |   OFF | old |   OFF | Delete |   yes |      no |"); // 24
			expected.Add("|    OFF |     ON |   OFF | old |    ON | Update |    no |      no |"); // 25
			expected.Add("|    OFF |     ON |   OFF | old |    ON | Update |   yes |      no |"); // 26
			expected.Add("|    OFF |     ON |   OFF | old |    ON | Delete |    no |      no |"); // 27
			expected.Add("|    OFF |     ON |   OFF | old |    ON | Delete |   yes |      no |"); // 28
			expected.Add("|    OFF |     ON |   OFF | new |   OFF | Update |    no |      no |"); // 29
			expected.Add("|    OFF |     ON |   OFF | new |   OFF | Update |   yes |      no |"); // 30 
			expected.Add("|    OFF |     ON |   OFF | new |   OFF | Delete |    no |      no |"); // 31
			expected.Add("|    OFF |     ON |   OFF | new |   OFF | Delete |   yes |      no |"); // 32
			expected.Add("|    OFF |     ON |   OFF | new |    ON | Update |    no |      no |"); // 33
			expected.Add("|    OFF |     ON |   OFF | new |    ON | Update |   yes |      no |"); // 34
			expected.Add("|    OFF |     ON |   OFF | new |    ON | Delete |    no |      no |"); // 35
			expected.Add("|    OFF |     ON |   OFF | new |    ON | Delete |   yes |      no |"); // 36
			expected.Add("|     ON |    OFF |   OFF | old |   OFF | Update |    no |     yes |"); // 37
			expected.Add("|     ON |    OFF |   OFF | old |   OFF | Update |   yes |     yes |"); // 38
			expected.Add("|     ON |    OFF |   OFF | old |   OFF | Delete |    no |     yes |"); // 39
			expected.Add("|     ON |    OFF |   OFF | old |   OFF | Delete |   yes |     yes |"); // 40
			expected.Add("|     ON |    OFF |   OFF | old |    ON | Update |    no |     yes |"); // 41
			expected.Add("|     ON |    OFF |   OFF | old |    ON | Update |   yes |     yes |"); // 42
			expected.Add("|     ON |    OFF |   OFF | old |    ON | Delete |    no |     yes |"); // 43
			expected.Add("|     ON |    OFF |   OFF | old |    ON | Delete |   yes |     yes |"); // 44
			expected.Add("|     ON |    OFF |   OFF | new |   OFF | Update |    no |     yes |"); // 45
			expected.Add("|     ON |    OFF |   OFF | new |   OFF | Update |   yes |      no |"); // 46
			expected.Add("|     ON |    OFF |   OFF | new |   OFF | Delete |    no |     yes |"); // 47
			expected.Add("|     ON |    OFF |   OFF | new |   OFF | Delete |   yes |      no |"); // 48
			expected.Add("|     ON |    OFF |   OFF | new |    ON | Update |    no |     yes |"); // 49
			expected.Add("|     ON |    OFF |   OFF | new |    ON | Update |   yes |      no |"); // 50
			expected.Add("|     ON |    OFF |   OFF | new |    ON | Delete |    no |     yes |"); // 51
			expected.Add("|     ON |    OFF |   OFF | new |    ON | Delete |   yes |      no |"); // 52
			expected.Add("|     ON |     ON |   OFF | old |   OFF | Update |    no |     yes |"); // 53
			expected.Add("|     ON |     ON |   OFF | old |   OFF | Update |   yes |     yes |"); // 54
			expected.Add("|     ON |     ON |   OFF | old |   OFF | Delete |    no |     yes |"); // 55
			expected.Add("|     ON |     ON |   OFF | old |   OFF | Delete |   yes |     yes |"); // 56
			expected.Add("|     ON |     ON |   OFF | old |    ON | Update |    no |     yes |"); // 57
			expected.Add("|     ON |     ON |   OFF | old |    ON | Update |   yes |     yes |"); // 58
			expected.Add("|     ON |     ON |   OFF | old |    ON | Delete |    no |     yes |"); // 59
			expected.Add("|     ON |     ON |   OFF | old |    ON | Delete |   yes |     yes |"); // 60
			expected.Add("|     ON |     ON |   OFF | new |   OFF | Update |    no |     yes |"); // 61
			expected.Add("|     ON |     ON |   OFF | new |   OFF | Update |   yes |      no |"); // 62
			expected.Add("|     ON |     ON |   OFF | new |   OFF | Delete |    no |     yes |"); // 63
			expected.Add("|     ON |     ON |   OFF | new |   OFF | Delete |   yes |      no |"); // 64
			expected.Add("|     ON |     ON |   OFF | new |    ON | Update |    no |     yes |"); // 65
			expected.Add("|     ON |     ON |   OFF | new |    ON | Update |   yes |      no |"); // 66
			expected.Add("|     ON |     ON |   OFF | new |    ON | Delete |    no |     yes |"); // 67
			expected.Add("|     ON |     ON |   OFF | new |    ON | Delete |   yes |      no |"); // 68
			expected.Add("--------------------------------------------------------------------"); // 69

			var actual = new List<string>(70);
			actual.Add("--------------------------------------------------------------------");
			actual.Add("|   Auto-stats settings   | Trace flags |        | PK    | auto    |");
			actual.Add("|-------------------------|-------------| Action | index | stats   |");
			actual.Add("| Create | Update | Async | CE  |  4199 |        | hint  | created |");
			actual.Add("|--------|--------|-------|-----|-------|--------|-------|---------|");
			RunActionsWithPermutation(actual);
			actual.Add("--------------------------------------------------------------------");

			AssertContainsExactElementsInAnyOrder(TestConnection.ServerFullVersionText, expected, actual);
		}

		void RunActionsWithPermutation(List<string> actual)
		{
			var sql = @"-- Generate permutations
SELECT
	stmt = ''
		+ '|    '  + AutoCreate.val + ' '
		+ '|    '  + AutoUpdate.val + ' '
		+ '|   '   + AutoAsync.val  + ' '
		+ '| '     + CE.val         + ' '
		+ '|   '   + TF4199.val     + ' '
		+ '| '     + UserAction.val + ' '
		+ '|   '   + Hint.val       + ' '
		+ '|     ' --+ CASE WHEN AutoCreate.val = 'OFF' THEN ' no' ELSE 'yes' END + ' '
		--+ '|'

	, [Create]   = LTRIM(AutoCreate.val)
	, [Update]   = LTRIM(AutoUpdate.val)
	, [Async]    = LTRIM(AutoAsync.val)
	, [CE]       = LTRIM(CE.val)
	, [4199]     = LTRIM(TF4199.val)
	, [Action]   = LTRIM(UserAction.val)
	, [Hint]     = LTRIM(Hint.val)
	--, [Expected] = CASE WHEN AutoCreate.val = 'OFF' THEN 'no' ELSE 'yes' END
FROM
	(VALUES (NULL)) AS Fake(val)
	CROSS JOIN (VALUES ('OFF')   , (' ON')   ) AS AutoCreate(val)
	CROSS JOIN (VALUES ('OFF')   , (' ON')   ) AS AutoUpdate(val)
	CROSS JOIN (VALUES ('OFF')               ) AS AutoAsync(val)
	CROSS JOIN (VALUES ('old')   , ('new')   ) AS CE(val)
	CROSS JOIN (VALUES ('OFF')   , (' ON')   ) AS TF4199(val)
	CROSS JOIN (VALUES ('Update'), ('Delete')) AS UserAction(val)
	CROSS JOIN (VALUES (' no')   , ('yes')   ) AS Hint(val)
ORDER BY
	[Create],
	[Update],
	[Async],
	[CE] DESC,
	[4199],
	[Action] DESC,
	[Hint]
	--, [Expected] DESC
";

			var stats = StatisticsSwitch.Instance.GetStatisticsSettings(TestDatabase);
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var actualTestCase = (string)reader["stmt"];

					var statsRequired = DbAutoStatsSettings.New((string)reader["Create"] + "-" + (string)reader["Update"] + "-" + (string)reader["Async"]);
					if (!stats.Current.Equals(statsRequired))
					{
						StatisticsSwitch.Instance.SetDbStatisticsSettings(TestConnection, TestDatabase, autoCreateON: statsRequired.AutoCreate, autoUpdateON: statsRequired.AutoUpdate, autoUpdateAsyncON: statsRequired.AutoUpdateAsync);
					}

					var newCE = "new".Equals((string)reader["CE"], StringComparison.OrdinalIgnoreCase);
					var applyHotFixes = "ON".Equals((string)reader["4199"], StringComparison.OrdinalIgnoreCase);
					var userAction = (string)reader["Action"];
					var withHint = "yes".Equals((string)reader["Hint"], StringComparison.OrdinalIgnoreCase);

					var actualResult = " no |";
					if (userAction.Equals("Update", StringComparison.OrdinalIgnoreCase))
					{
						if (Update(newCE, applyHotFixes, withHint))
						{
							actualResult = "yes |";
						}
					}
					else if (userAction.Equals("Delete", StringComparison.OrdinalIgnoreCase))
					{
						if (Delete(newCE, applyHotFixes, withHint))
						{
							actualResult = "yes |";
						}
					}

					actual.Add(actualTestCase + actualResult);
				}
			}
		}

		#region Implementation

		const string TestDatabase = "DbForStats76DFCAD72DEA4AD59692B499C1CB43A0";
		const string TestTable_Update = "Table_Update";
		const string TestTable_Update_Hint = "Table_Update_Hint";
		const string TestTable_Delete = "Table_Delete";
		const string TestTable_Delete_Hint = "Table_Delete_Hint";

		AdminConnection TestConnection;

		void PrepareData(SqlServerVersionNumber sqlServerProduct)
		{
			AdoTestUtils.CreateDbDropExisting(TestDatabase, Db.DatabaseName);
			TestConnection = Db.NewAdminConnection(TestDatabase);

			// setup compatibility level
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
			{
				TestConnection.ExecuteNonQuery($"ALTER DATABASE {TestDatabase.QuoteName()} SET COMPATIBILITY_LEVEL = {sqlServerProduct.CompatibilityLevel};");
			}

			// create tables
			CreateTable(TestTable_Update);
			CreateTable(TestTable_Update_Hint);
			CreateTable(TestTable_Delete);
			CreateTable(TestTable_Delete_Hint);
		}

		void CreateTable(string tableName)
		{
			var sql = $@"
if (OBJECT_ID(N'dbo.{tableName.QuoteEscapedName('\'')}', N'U') is NOT NULL) DROP TABLE dbo.{tableName.QuoteName()}
CREATE TABLE dbo.{tableName.QuoteName()}
(
	TT_PK uniqueidentifier NOT NULL,
	TT_1  int              NOT NULL DEFAULT 0,
	TT_2  int              NOT NULL DEFAULT 0,
	TT_3  int              NOT NULL DEFAULT 0,
	TT_4  int                  NULL,
)

INSERT dbo.{tableName.QuoteName()} (TT_PK, TT_1, TT_2, TT_3, TT_4) VALUES
-- (TT_PK                                 , TT_1, TT_2, TT_3, TT_4)
	('00000001-0000-0000-0000-000000000000',    1,    1,    1,    1),
	('00000002-0000-0000-0000-000000000000',    2,    2,    2,    2),
	('00000003-0000-0000-0000-000000000000',    3,    3,    3, NULL),
	('00000004-0000-0000-0000-000000000000',    4,    4,    4,    3)

ALTER TABLE dbo.{tableName.QuoteName()} ADD CONSTRAINT [PK_{tableName.QuoteEscapedName()}] PRIMARY KEY NONCLUSTERED (TT_PK)
CREATE CLUSTERED INDEX [RC_{tableName.QuoteEscapedName()}_1_2] ON dbo.{tableName.QuoteName()} (TT_1, TT_2)
CREATE NONCLUSTERED INDEX [RX_F_{tableName.QuoteEscapedName()}_4] ON dbo.{tableName.QuoteName()} (TT_4) WHERE TT_4 is NOT NULL

";

			TestConnection.ExecuteNonQuery(sql);
		}

		bool Update(bool newCE, bool applyHotFixes, bool withHint)
		{
			var tableName = (withHint) ? TestTable_Update_Hint : TestTable_Update;

			DropAutoStats(tableName);

			var options = (newCE ? "QUERYTRACEON 2312" : "QUERYTRACEON 9481") + (applyHotFixes ? ", QUERYTRACEON 4199" : "");
			var indexHint = (withHint) ? $"WITH (INDEX({"PK_" + tableName}))" : "";

			var sql = $@"
UPDATE dbo.{tableName.QuoteName()} SET
	TT_3 = 4
FROM
	dbo.{tableName.QuoteName()} {indexHint}
WHERE
	TT_PK = '00000003-0000-0000-0000-000000000000'
	AND TT_1 = 3
	AND TT_2 = 3
	AND TT_3 = 3
	AND TT_4 is NULL
OPTION ({options})

";
			TestConnection.ExecuteNonQuery(sql);

			return AutoStatsExists(tableName);
		}

		bool Delete(bool newCE, bool applyHotFixes, bool withHint)
		{
			var tableName = (withHint) ? TestTable_Delete_Hint : TestTable_Delete;

			DropAutoStats(tableName);

			var options = (newCE ? "QUERYTRACEON 2312" : "QUERYTRACEON 9481") + (applyHotFixes ? ", QUERYTRACEON 4199" : "");
			var indexHint = (withHint) ? $"WITH (INDEX({"PK_" + tableName}))" : "";

			var sql = $@"
DELETE dbo.{tableName.QuoteName()}
FROM
	dbo.{tableName.QuoteName()} {indexHint}
WHERE
	TT_PK = '00000003-0000-0000-0000-000000000000'
	AND TT_1 = 3
	AND TT_2 = 3
	AND TT_3 = 3
	AND TT_4 is NULL
OPTION ({options})

";
			TestConnection.ExecuteNonQuery(sql);

			return AutoStatsExists(tableName);
		}

		void DropAutoStats(string tableName)
		{
			var sql = $@"-- ClearAutoStats
DECLARE
	@stmt nvarchar(4000)

SELECT
	@stmt = ISNULL(@stmt + CHAR(13) + CHAR(10), N'')
		+ N'DROP STATISTICS [dbo].' + QUOTENAME(o.name) + N'.' + QUOTENAME(s.name) + N';'
FROM
	sys.tables     AS o
	JOIN sys.stats AS s ON s.object_id = o.object_id
WHERE
	o.schema_id = SCHEMA_ID(N'dbo')
	AND o.name = @tableName
	AND s.auto_created = 1

if (@stmt is NOT NULL) EXEC (@stmt)

";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.ExecuteNonQuery();
			}
		}

		bool AutoStatsExists(string tableName)
		{
			var sql = @"
SELECT
	CONVERT(bit,
		CASE
			WHEN EXISTS
				(
					SELECT NULL
					FROM
						sys.stats AS s
					WHERE 1=1
						AND s.object_id = OBJECT_ID(@tableName)
						AND s.auto_created = 1
				)
				THEN 1
			ELSE 0
		END
		)
";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, "[dbo]." + tableName.QuoteName());

				return (bool)cmd.ExecuteScalar();
			}
		}

		protected override void TearDown()
		{
			TestConnection?.Dispose();
			AdoTestUtils.DropDbIfExists(TestDatabase);

			DbCommitTracker.Ignore("-- Generate permutations");

			base.TearDown();
		}

		#endregion // Implementation
	}
}
