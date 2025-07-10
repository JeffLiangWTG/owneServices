using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	class ConvertCharToCharConditionsTest : TestCase
	{
		public void TestComputedColumns()
		{
			// Computed columns are ignored
			// Col_1: char(2)            -> varchar(2) AS 'xx'
			// Col_2: varchar(2) AS 'xx' -> char(2)
			// Col_3: varchar(2) AS 'xx' -> char(2) AS CONVERT(char(2), 'yy')

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AlterTable(targetDb, new[]
				{
					"Col_1 char(2) NOT NULL DEFAULT ''",
					"Col_2 AS 'xx'",
					"Col_3 AS 'xx'",
				});

				AlterTable(templateDb, new[]
				{
					"Col_1 AS 'xx'",
					"Col_2 char(2) NOT NULL DEFAULT ''",
					"Col_3 AS CONVERT(char(2), 'yy')",
				});

				var expectedTargetColumns = new string[]
				{
					"Col_1 char(2)",
					"Col_2 varchar(2)",
					"Col_3 varchar(2)",
				};
				AssertColumnDefinitions("PRECONDITION targetDb.columns", targetDb, expectedTargetColumns);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				AssertColumnDefinitions("targetDb.columns", targetDb, expectedTargetColumns);
			}
		}

		public void TestStringColumns()
		{
			// Non-string columns are ignored
			// Col_1: char(2)  -> datetime
			// Col_2: datetime -> char(200)
			// Col_3: date     -> datetime

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AlterTable(targetDb, new[]
				{
					"Col_1 char(2) NOT NULL DEFAULT ''",
					"Col_2 datetime",
					"Col_3 date",
				});

				AlterTable(templateDb, new[]
				{
					"Col_1 datetime",
					"Col_2 char(200) NOT NULL DEFAULT ''",
					"Col_3 datetime",
				});

				var expectedTargetColumns = new string[]
				{
					"Col_1 char(2)",
					"Col_2 datetime",
					"Col_3 date",
				};
				AssertColumnDefinitions("PRECONDITION targetDb.columns", targetDb, expectedTargetColumns);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				AssertColumnDefinitions("targetDb.columns", targetDb, expectedTargetColumns);
			}
		}

		public void TestDecreasingSize()
		{
			// Any decreasing size is ignored
			// Col_1:  char(20)      -> char(10)
			// Col_2:  char(20)      -> varchar(10)
			// Col_3:  char(20)      -> nchar(10)
			// Col_4:  char(20)      -> nvarchar(10)
			// Col_5:  varchar(20)   -> char(10)
			// Col_6:  varchar(20)   -> varchar(10)
			// Col_7:  varchar(20)   -> nchar(10)
			// Col_8:  varchar(20)   -> nvarchar(10)
			// Col_9:  nchar(20)     -> char(10)
			// Col_10: nchar(20)     -> varchar(10)
			// Col_11: nchar(20)     -> nchar(10)
			// Col_12: nchar(20)     -> nvarchar(10)
			// Col_13: nvarchar(20)  -> char(10)
			// Col_14: nvarchar(20)  -> varchar(10)
			// Col_15: nvarchar(20)  -> nchar(10)
			// Col_16: nvarchar(20)  -> nvarchar(10)
			// Col_17: varchar(max)  -> char(10)
			// Col_18: varchar(max)  -> varchar(10)
			// Col_19: varchar(max)  -> nchar(10)
			// Col_20: varchar(max)  -> nvarchar(10)
			// Col_21: nvarchar(max) -> char(10)
			// Col_22: nvarchar(max) -> varchar(10)
			// Col_23: nvarchar(max) -> nchar(10)
			// Col_24: nvarchar(max) -> nvarchar(10)

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AlterTable(targetDb, new[]
				{
					"Col_1  char(20)      ",
					"Col_2  char(20)      ",
					"Col_3  char(20)      ",
					"Col_4  char(20)      ",

					"Col_5  varchar(20)   ",
					"Col_6  varchar(20)   ",
					"Col_7  varchar(20)   ",
					"Col_8  varchar(20)   ",

					"Col_9  nchar(20)     ",
					"Col_10 nchar(20)     ",
					"Col_11 nchar(20)     ",
					"Col_12 nchar(20)     ",

					"Col_13 nvarchar(20)  ",
					"Col_14 nvarchar(20)  ",
					"Col_15 nvarchar(20)  ",
					"Col_16 nvarchar(20)  ",

					"Col_17 varchar(max)  ",
					"Col_18 varchar(max)  ",
					"Col_19 varchar(max)  ",
					"Col_20 varchar(max)  ",

					"Col_21 nvarchar(max) ",
					"Col_22 nvarchar(max) ",
					"Col_23 nvarchar(max) ",
					"Col_24 nvarchar(max) ",
				});

				AlterTable(templateDb, new[]
				{
					"Col_1  char(10)",
					"Col_2  varchar(10)",
					"Col_3  nchar(10)",
					"Col_4  nvarchar(10)",

					"Col_5  char(10)",
					"Col_6  varchar(10)",
					"Col_7  nchar(10)",
					"Col_8  nvarchar(10)",

					"Col_9  char(10)",
					"Col_10 varchar(10)",
					"Col_11 nchar(10)",
					"Col_12 nvarchar(10)",

					"Col_13 char(10)",
					"Col_14 varchar(10)",
					"Col_15 nchar(10)",
					"Col_16 nvarchar(10)",

					"Col_17 char(10)",
					"Col_18 varchar(10)",
					"Col_19 nchar(10)",
					"Col_20 nvarchar(10)",

					"Col_21 char(10)",
					"Col_22 varchar(10)",
					"Col_23 nchar(10)",
					"Col_24 nvarchar(10)",
				});

				var expectedTargetColumns = new string[]
				{
					"Col_1 char(20)",
					"Col_2 char(20)",
					"Col_3 char(20)",
					"Col_4 char(20)",

					"Col_5 varchar(20)",
					"Col_6 varchar(20)",
					"Col_7 varchar(20)",
					"Col_8 varchar(20)",

					"Col_9 nchar(20)",
					"Col_10 nchar(20)",
					"Col_11 nchar(20)",
					"Col_12 nchar(20)",

					"Col_13 nvarchar(20)",
					"Col_14 nvarchar(20)",
					"Col_15 nvarchar(20)",
					"Col_16 nvarchar(20)",

					"Col_17 varchar(max)",
					"Col_18 varchar(max)",
					"Col_19 varchar(max)",
					"Col_20 varchar(max)",

					"Col_21 nvarchar(max)",
					"Col_22 nvarchar(max)",
					"Col_23 nvarchar(max)",
					"Col_24 nvarchar(max)",
				};
				AssertColumnDefinitions("PRECONDITION targetDb.columns", targetDb, expectedTargetColumns);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				AssertColumnDefinitions("targetDb.columns", targetDb, expectedTargetColumns);
			}
		}

		public void TestMetadataOnlyOperations()
		{
			// Metadata-only operations are ignored
			// Col_1:  char(10)     -> char(10)     -- ignored
			// Col_2:  varchar(10)  -> varchar(10)  -- ignored
			// Col_3:  nchar(10)    -> nchar(10)    -- ignored
			// Col_4:  nvarchar(10) -> nvarchar(10) -- ignored
			// Col_5:  char(10)     -> char(20)     -- converted
			// Col_6:  varchar(10)  -> varchar(20)  -- not indexed - ignored
			// Col_7:  nchar(10)    -> nchar(20)    -- converted
			// Col_8:  nvarchar(10) -> nvarchar(20) -- not indexed - ignored
			// Col_9:  varchar(10)  -> varchar(20)  -- indexed, key     - converted
			// Col_10: nvarchar(10) -> nvarchar(20) -- indexed, key     - converted
			// Col_11: varchar(10)  -> varchar(20)  -- indexed, include - converted
			// Col_12: nvarchar(10) -> nvarchar(20) -- indexed, include - converted
			// Col_13: varchar(10)  -> varchar(20)  -- indexed, where   - converted
			// Col_14: nvarchar(10) -> nvarchar(20) -- indexed, where   - converted

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AlterTable(targetDb, new[]
				{
					"Col_1  char(10)",
					"Col_2  varchar(10)",
					"Col_3  nchar(10)",
					"Col_4  nvarchar(10)",
					"Col_5  char(10)",
					"Col_6  varchar(10)",
					"Col_7  nchar(10)",
					"Col_8  nvarchar(10)",
					"Col_9  varchar(10)",
					"Col_10 nvarchar(10)",
					"Col_11 varchar(10)",
					"Col_12 nvarchar(10)",
					"Col_13 varchar(10)",
					"Col_14 nvarchar(10)",
				});

				AlterTable(templateDb, new[]
				{
					"Col_1  char(10)",
					"Col_2  varchar(10)",
					"Col_3  nchar(10)",
					"Col_4  nvarchar(10)",
					"Col_5  char(20)",
					"Col_6  varchar(20)",
					"Col_7  nchar(20)",
					"Col_8  nvarchar(20)",
					"Col_9  varchar(20)",
					"Col_10 nvarchar(20)",
					"Col_11 varchar(20)",
					"Col_12 nvarchar(20)",
					"Col_13 varchar(20)",
					"Col_14 nvarchar(20)",
				});

				using (((ICurrentDbControl)Db.Connection).UseDatabase(templateDb))
				{
					Db.Connection.ExecuteNonQuery($@"
CREATE INDEX _ind_9 ON [{targetTable}] (TST_PK, Col_9);
CREATE INDEX _ind_10 ON [{targetTable}] (TST_PK, Col_10);
CREATE INDEX _ind_11 ON [{targetTable}] (TST_PK) INCLUDE (Col_11);
CREATE INDEX _ind_12 ON [{targetTable}] (TST_PK) INCLUDE (Col_12);
CREATE INDEX _ind_13 ON [{targetTable}] (TST_PK) WHERE (Col_13 = '');
CREATE INDEX _ind_14 ON [{targetTable}] (TST_PK) WHERE (Col_14 = '');

");
				}

				var expectedTargetColumns = new string[]
				{
					"Col_1 char(10)",
					"Col_2 varchar(10)",
					"Col_3 nchar(10)",
					"Col_4 nvarchar(10)",
					"Col_5 char(10)",
					"Col_6 varchar(10)",
					"Col_7 nchar(10)",
					"Col_8 nvarchar(10)",
					"Col_9 varchar(10)",
					"Col_10 nvarchar(10)",
					"Col_11 varchar(10)",
					"Col_12 nvarchar(10)",
					"Col_13 varchar(10)",
					"Col_14 nvarchar(10)",
				};
				AssertColumnDefinitions("PRECONDITION targetDb.columns", targetDb, expectedTargetColumns);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				expectedTargetColumns = new string[]
				{
					"Col_1 char(10)",
					"Col_2 varchar(10)",
					"Col_3 nchar(10)",
					"Col_4 nvarchar(10)",
					"Col_5 char(10)",
					"Col_6 varchar(10)",
					"Col_7 nchar(10)",
					"Col_8 nvarchar(10)",
					"Col_9 varchar(10)",
					"Col_10 nvarchar(10)",
					"Col_11 varchar(10)",
					"Col_12 nvarchar(10)",
					"Col_13 varchar(10)",
					"Col_14 nvarchar(10)",
					newColumnPrefix + "Col_5 char(20)",
					newColumnPrefix + "Col_7 nchar(20)",
					newColumnPrefix + "Col_9 varchar(20)",
					newColumnPrefix + "Col_10 nvarchar(20)",
					newColumnPrefix + "Col_11 varchar(20)",
					newColumnPrefix + "Col_12 nvarchar(20)",
					newColumnPrefix + "Col_13 varchar(20)",
					newColumnPrefix + "Col_14 nvarchar(20)",
				};
				AssertColumnDefinitions("targetDb.columns", targetDb, expectedTargetColumns);
			}
		}

		public void TestOfflineOperations()
		{
			// NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation

			// Offline-only operations are ignored
			// Col_1_char_2_varchar      char(10)     NOT NULL ->  varchar(max) NOT NULL -- ignored
			// Col_2_nchar_2_varchar     nchar(10)    NOT NULL ->  varchar(max) NOT NULL -- ignored
			// Col_3_varchar_2_varchar   varchar(10)  NOT NULL ->  varchar(max) NOT NULL -- ignored
			// Col_4_nvarchar_2_varchar  nvarchar(10) NOT NULL ->  varchar(max) NOT NULL -- ignored
			// Col_5_char_2_nvarchar     char(10)     NOT NULL -> nvarchar(max) NOT NULL -- ignored
			// Col_6_nchar_2_nvarchar    nchar(10)    NOT NULL -> nvarchar(max) NOT NULL -- ignored
			// Col_7_varchar_2_nvarchar  varchar(10)  NOT NULL -> nvarchar(max) NOT NULL -- ignored
			// Col_8_nvarchar_2_nvarchar nvarchar(10) NOT NULL -> nvarchar(max) NOT NULL -- ignored

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AlterTable(targetDb, new[]
				{
					"Col_1_char_2_varchar      char(10)     NOT NULL DEFAULT ''",
					"Col_2_nchar_2_varchar     nchar(10)    NOT NULL DEFAULT ''",
					"Col_3_varchar_2_varchar   varchar(10)  NOT NULL DEFAULT ''",
					"Col_4_nvarchar_2_varchar  nvarchar(10) NOT NULL DEFAULT ''",
					"Col_5_char_2_nvarchar     char(10)     NOT NULL DEFAULT ''",
					"Col_6_nchar_2_nvarchar    nchar(10)    NOT NULL DEFAULT ''",
					"Col_7_varchar_2_nvarchar  varchar(10)  NOT NULL DEFAULT ''",
					"Col_8_nvarchar_2_nvarchar nvarchar(10) NOT NULL DEFAULT ''",
				});

				AlterTable(templateDb, new[]
				{
					"Col_1_char_2_varchar       varchar(max) NOT NULL DEFAULT ''",
					"Col_2_nchar_2_varchar      varchar(max) NOT NULL DEFAULT ''",
					"Col_3_varchar_2_varchar    varchar(max) NOT NULL DEFAULT ''",
					"Col_4_nvarchar_2_varchar   varchar(max) NOT NULL DEFAULT ''",
					"Col_5_char_2_nvarchar     nvarchar(max) NOT NULL DEFAULT ''",
					"Col_6_nchar_2_nvarchar    nvarchar(max) NOT NULL DEFAULT ''",
					"Col_7_varchar_2_nvarchar  nvarchar(max) NOT NULL DEFAULT ''",
					"Col_8_nvarchar_2_nvarchar nvarchar(max) NOT NULL DEFAULT ''",
				});

				var expectedTargetColumns = new string[]
				{
					"Col_1_char_2_varchar char(10)",
					"Col_2_nchar_2_varchar nchar(10)",
					"Col_3_varchar_2_varchar varchar(10)",
					"Col_4_nvarchar_2_varchar nvarchar(10)",
					"Col_5_char_2_nvarchar char(10)",
					"Col_6_nchar_2_nvarchar nchar(10)",
					"Col_7_varchar_2_nvarchar varchar(10)",
					"Col_8_nvarchar_2_nvarchar nvarchar(10)",
				};
				AssertColumnDefinitions("PRECONDITION targetDb.columns", targetDb, expectedTargetColumns);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				AssertColumnDefinitions("targetDb.columns", targetDb, expectedTargetColumns);
			}
		}

		#region Implementation

		readonly string newColumnPrefix = ColumnSynchroniser.ConvertCharToCharColumnPrefix;
		const string templateDb = "_testPreSynchroniser_TemplateDb";
		readonly string targetDb = Db.DatabaseName;
		const string targetTable = "_testPreSynchroniser_TargetTable";

		IAuxiliaryDbCreator templateDbCreator;
		readonly UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();

		void AlterTable(string dbName, string[] columnDefinitions)
		{
			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				foreach (var column in columnDefinitions)
				{
					Db.Connection.ExecuteNonQuery($"ALTER TABLE [{targetTable}] ADD {column}");
				}
			}
		}

		void AssertColumnDefinitions(string message, string dbName, IEnumerable<string> expectedColumnDefinitions)
		{
			var sql = FormattableString.Invariant($@"-- Get column definitions
SELECT
	col_definition =
		CASE
			WHEN c.max_length = -1 THEN CONCAT(c.name, ' ', t.name, '(max)')
			WHEN t.name in (N'nchar', N'nvarchar') THEN CONCAT(c.name, ' ', t.name, '(', c.max_length / 2, ')')
			WHEN t.name in (N'char', N'varchar') THEN CONCAT(c.name, ' ', t.name, '(', c.max_length, ')')
			ELSE CONCAT(c.name, ' ', t.name)
		END
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID('dbo.{targetTable}', N'U')
	AND c.name NOT in (N'TST_PK', N'TST_SystemCreateTimeUtc')
ORDER BY
	c.name

"
				);

			var actualColumnDefinitions = new List<string>();
			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				Db.Connection.ExecuteReader(sql
					, (reader) =>
					{
						actualColumnDefinitions.Add((string)reader["col_definition"]);
					});
			}

			AssertContainsExactElementsInAnyOrder(message, expectedColumnDefinitions, actualColumnDefinitions);
		}

		readonly IEnumerable<string> targetPrerequisiteColumnList = new string[]
			{
				"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
				"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
			};

		readonly IEnumerable<string> templatePrerequisiteColumnList = new string[]
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
			var sql = String.Format(CultureInfo.InvariantCulture, @"
if (OBJECT_ID(N'[dbo].[{0}]', N'U') is NOT NULL) DROP TABLE [dbo].[{0}];
CREATE TABLE [dbo].[{0}]
(
	{1}
);
",
				targetTable,
				String.Join(",\r\n\t", targetPrerequisiteColumnList)
				);

			Db.Connection.ExecuteNonQuery(sql);
		}

		void CreateTemplateDatabase()
		{
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(templateDb, targetTable, templatePrerequisiteColumnList);
			templateDbCreator.CreateDropExisting();
			Db.Connection.AlterDbWriteableState(templateDb, writeable: true);
		}

		void DropTemplateDatabase()
		{
			templateDbCreator.Drop();
		}

		#endregion // Implementation
	}
}
