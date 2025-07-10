using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;

[UseSnapshotProtection]
public class ConvertDateTimesToDateConditionsTest : TestCase
{
	public void TestIgnoresComputed()
	{
		// Col_1: non-computed -> computed
		// Col_2: computed -> non-computed
		// col_3: computed -> computed
		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			AlterTable(_targetDb, new[]
			{
				"Col_1 smalldatetime NOT NULL DEFAULT '2024-01-01 00:00:00'",
				"Col_2 AS CONVERT(smalldatetime, GETDATE())",
				"Col_3 AS CONVERT(smalldatetime, GETDATE())"
			});

			AlterTable(TemplateDb, new[]
			{
				"Col_1 AS GETDATE()",
				"Col_2 date NOT NULL DEFAULT '2024-01-01'",
				"Col_3 AS DATEADD(month, 2, '2017/08/25')"
			});

			var expectedTargetColumns = new[]
			{
				"Col_1 smalldatetime",
				"Col_2 smalldatetime",
				"Col_3 smalldatetime"
			};

			AssertColumnDefinitions("PRECONDITION targetDb.Columns", _targetDb, expectedTargetColumns);

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDateTimesToDate();

			AssertColumnDefinitions("targetDb.columns", _targetDb, expectedTargetColumns);
		}
	}

	public void TestIgnoresNonDateTimesAndDate()
	{
		// Col_1: smalldatetime -> datetimeoffset
		// Col_2: datetimeoffset -> date
		// Col_3: datetimeoffset -> datetime
		// Col_4: datetime -> datetimeoffset
		// Col_4: datetime2 -> datetimeoffset

		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			AlterTable(_targetDb, new[]
			{
				"Col_1 smalldatetime NULL",
				"Col_2 datetimeoffset NULL",
				"Col_3 datetimeoffset NULL",
				"Col_4 datetime NULL",
				"Col_5 datetime2 NULL"
			});

			AlterTable(TemplateDb, new[]
			{
				"Col_1 datetimeoffset NULL",
				"Col_2 date NULL",
				"Col_3 datetime NULL",
				"Col_4 datetimeoffset NULL",
				"Col_5 datetimeoffset NULL"
			});

			var expectedTargetColumns = new[]
			{
				"Col_1 smalldatetime",
				"Col_2 datetimeoffset",
				"Col_3 datetimeoffset",
				"Col_4 datetime",
				"Col_5 datetime2"
			};
			AssertColumnDefinitions("PRECONDITION targetDb.columns", _targetDb, expectedTargetColumns);

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDateTimesToDate();

			AssertColumnDefinitions("targetDb.Columns", _targetDb, expectedTargetColumns);
		}
	}

	#region Implementation

	const string TemplateDb = "_testPreSynchroniser_TemplateDb";
	const string TargetTable = "_testPreSynchroniser_TargetTable";
	readonly string _targetDb = Db.DatabaseName;
	readonly UpgradeManagerForTestWithOutputBuffer _manager = new();

	IAuxiliaryDbCreator _templateDbCreator;

	static readonly IEnumerable<string> TargetPrerequisiteColumnList = new[]
	{
		"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
		"TST_SystemCreateUserId  int              NOT NULL UNIQUE CLUSTERED",
	};

	static readonly IEnumerable<string> TemplatePrerequisiteColumnList = new[]
	{
		"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
		"TST_SystemCreateUserId  int              NOT NULL UNIQUE CLUSTERED",
	};

	protected override void SetUp()
	{
		base.SetUp();

		CreateTemplateDatabase();
		TablePreSynchroniser.CreatePreAddDb_ForTest();

		CreateTestTable();
	}

	protected override void TearDown()
	{
		TablePreSynchroniser.DropPreAddDb_ForTest();
		DropTemplateDatabase();

		base.TearDown();
	}

	void AssertColumnDefinitions(string message, string dbName, IEnumerable<string> expectedColumnDefinitions)
	{
		const string sql = $@"-- Get column definitions
SELECT
	col_definition = CONCAT(c.name, ' ', t.name)
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID('dbo.{TargetTable}', N'U')
	AND c.name NOT in (N'TST_PK', N'TST_SystemCreateUserId')
ORDER BY
	c.name

";

		var actualColumnDefinitions = new List<string>();
		using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
		{
			Db.Connection.ExecuteReader(sql, reader => actualColumnDefinitions.Add((string)reader["col_definition"]));
		}

		AssertContainsExactElementsInAnyOrder(message, expectedColumnDefinitions, actualColumnDefinitions);
	}

	void AlterTable(string dbName, string[] columnDefinitions)
	{
		using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
		{
			foreach (var column in columnDefinitions)
			{
				_ = Db.Connection.ExecuteNonQuery($"ALTER TABLE [{TargetTable}] ADD {column}");
			}
		}
	}

	void CreateTemplateDatabase()
	{
		_templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(TemplateDb, TargetTable, TemplatePrerequisiteColumnList);
		_templateDbCreator.CreateDropExisting();
		Db.Connection.AlterDbWriteableState(TemplateDb, writeable: true);
	}

	void CreateTestTable()
	{
		var sql = string.Format(CultureInfo.InvariantCulture, @"
if (OBJECT_ID(N'[dbo].[{0}]', N'U') is NOT NULL) DROP TABLE [dbo].[{0}];
CREATE TABLE [dbo].[{0}]
(
	{1}
);
",
			TargetTable,
			string.Join(",\r\n\t", TargetPrerequisiteColumnList)
			);

		_ = Db.Connection.ExecuteNonQuery(sql);
	}

	void DropTemplateDatabase()
	{
		_templateDbCreator.Drop();
	}

	#endregion
}
