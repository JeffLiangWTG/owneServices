using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;

[UseSnapshotProtection]
public class ConvertDecimalToDecimalConditionsTest : TestCase
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
					"Col_1 decimal(1,0) NOT NULL DEFAULT 1.",
					"Col_2 AS 1.",
					"Col_3 AS 1."
			});

			AlterTable(TemplateDb, new[]
			{
					"Col_1 AS 1.0",
					"Col_2 decimal(2,1) NOT NULL DEFAULT 1.0",
					"Col_3 AS CONVERT(decimal(2,1), 1.0)"
			});

			var expectedTargetColumns = new[]
			{
					"Col_1 decimal(1,0)",
					"Col_2 decimal(1,0)",
					"Col_3 decimal(1,0)"
			};
			AssertColumnDefinitions("PRECONDITION targetDb.columns", _targetDb, expectedTargetColumns);

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDecimalToDecimal();

			AssertColumnDefinitions("targetDb.columns", _targetDb, expectedTargetColumns);
		}
	}

	public void TestIgnoresNonDecimal()
	{
		// Col_1: decimal  -> datetime
		// Col_2: datetime -> decimal
		// Col_3: date     -> datetime
		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			AlterTable(_targetDb, new[]
			{
					"Col_1 decimal(1,0) NOT NULL DEFAULT 1.",
					"Col_2 datetime",
					"Col_3 date",
				});

			AlterTable(TemplateDb, new[]
			{
					"Col_1 datetime",
					"Col_2 decimal(1,0) NOT NULL DEFAULT 1.",
					"Col_3 datetime",
				});

			var expectedTargetColumns = new[]
			{
					"Col_1 decimal(1,0)",
					"Col_2 datetime",
					"Col_3 date",
			};
			AssertColumnDefinitions("PRECONDITION targetDb.columns", _targetDb, expectedTargetColumns);

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDecimalToDecimal();

			AssertColumnDefinitions("targetDb.columns", _targetDb, expectedTargetColumns);
		}
	}

	public void TestIgnoresDecreasingPrecisionAndOrLeadingDigits()
	{
		// Col_1: decimal(10,5) -> decimal(9,4)
		// Col_2: decimal(10,5) -> decimal(10,4)
		// Col_3: decimal(10,5) -> decimal(9,5)
		// Col_4: decimal(10,5) -> decimal(10,6)
		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			var targetColumns = new[]
			{
				"Col_1 decimal(10,5)",
				"Col_2 decimal(10,5)",
				"Col_3 decimal(10,5)",
				"Col_4 decimal(10,5)"
			};

			AlterTable(_targetDb, targetColumns);
			AssertColumnDefinitions("PRECONDITION targetDb.columns", _targetDb, targetColumns);

			AlterTable(TemplateDb, new[]
			{
				"Col_1 decimal(9,4)",
				"Col_2 decimal(10,4)",
				"Col_3 decimal(9,5)",
				"Col_4 decimal(10,6)"
			});

			var expectedTargetColumns = targetColumns;

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDecimalToDecimal();

			AssertColumnDefinitions("targetDb.columns", _targetDb, expectedTargetColumns);
		}
	}

	public void TestIgnoresMetadataOnlyOperations()
	{
		// Col_1: decimal(10,5) NOT NULL -> decimal(10,5) NULL -- ignored
		// Changes such as decimal(1) -> decimal(9) should be metadata-only, but aren't
		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			var targetColumns = new[]
			{
				"Col_1 decimal(10,5) NOT NULL"
			};

			var expectedTargetColumns = new[]
			{
				"Col_1 decimal(10,5)"
			};

			AlterTable(_targetDb, targetColumns);
			AssertColumnDefinitions("PRECONDITION targetDb.columns", _targetDb, expectedTargetColumns);

			AlterTable(TemplateDb, new []
			{
				"Col_1 decimal(10,5) NULL"
			});

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDecimalToDecimal();

			AssertColumnDefinitions("targetDb.columns", _targetDb, expectedTargetColumns);
		}
	}

	#region Implementation

	const string TemplateDb = "_testPreSynchroniser_TemplateDb";
	readonly string _targetDb = Db.DatabaseName;
	const string TargetTable = "_testPreSynchroniser_TargetTable";

	IAuxiliaryDbCreator _templateDbCreator;
	readonly UpgradeManagerForTestWithOutputBuffer _manager = new ();

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

	void AssertColumnDefinitions(string message, string dbName, IEnumerable<string> expectedColumnDefinitions)
	{
		const string sql = $@"-- Get column definitions
SELECT
	col_definition =
		CASE
			WHEN t.name in ('decimal', 'numeric') THEN CONCAT(c.name, ' decimal(', c.precision, ',', c.scale, ')')
			ELSE CONCAT(c.name, ' ', t.name)
		END
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID('dbo.{TargetTable}', N'U')
	AND c.name NOT in (N'TST_PK', N'TST_SystemCreateTimeUtc')
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

	static readonly IEnumerable<string> TargetPrerequisiteColumnList = new[]
	{
		"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
		"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
	};

	static readonly IEnumerable<string> TemplatePrerequisiteColumnList = new[]
	{
		"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
		"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
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

	void CreateTemplateDatabase()
	{
		_templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(TemplateDb, TargetTable, TemplatePrerequisiteColumnList);
		_templateDbCreator.CreateDropExisting();
		Db.Connection.AlterDbWriteableState(TemplateDb, writeable: true);
	}

	void DropTemplateDatabase()
	{
		_templateDbCreator.Drop();
	}

	#endregion // Implementation
}
