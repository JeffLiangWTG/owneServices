using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	class ConvertCharToCharTest : TestCase
	{
		public void TestConvertCharToChar()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var newColumnPrefix = ColumnSynchroniser.ConvertCharToCharColumnPrefix;

				// Col_1: char(10)      NOT NULL -> nchar(10)         NULL
				// Col_2: char(10)          NULL -> varchar(100)  NOT NULL DEFAULT ('')
				// Col_3: nvarchar(max)     NULL -> char(3)           NULL -- should be ignored by upgrader
				// Col_4: nchar(10)         NULL -> nvarchar(max) NOT NULL -- should be ignored by upgrader
				// Col_5: varchar(10)       NULL -> varchar(20)   NOT NULL -- should be ignored by upgrader
				// Col_6: char(10)          NULL -> nchar(10)     SPARSE   -- non-sparse to sparse
				// Col_7: char(10)        SPARSE -> nchar(10)     SPARSE   -- sparse to sparse
				// Col_8: char(10)        SPARSE -> nchar(10)     NOT NULL -- sparse to non-sparse

				var expectedColumnDefinitions = new string[]
				{
					"TST_Col_1: char(10) NOT NULL",
					"TST_Col_2: char(10) NULL",
					"TST_Col_3: nvarchar(max) NULL",
					"TST_Col_4: nchar(10) NULL",
					"TST_Col_5: varchar(10) NULL",
					"TST_Col_6: char(10) NULL",
					"TST_Col_7: char(10) SPARSE NULL",
					"TST_Col_8: char(10) SPARSE NULL",
				};
				AssertColumnDefinitions("PRECONDITION", expectedColumnDefinitions);

				TargetTableCreateValues(1, "", null, null, null, null, null, null);
				TargetTableCreateValues(2, "", null, null, null, null, "AB", "BCDE");
				TargetTableCreateValues(3, "12345", "O'Kiev", null, null, "123", null, "ABC");
				TargetTableCreateValues(4, "12345", "O'Kiev", null, null, "12345678", "AB", null);
				TargetTableCreateValues(5, "12345", "O'Kiev", "12345", "12345", "12345678", "AB", "BCDE");

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				expectedColumnDefinitions =
				[
					"TST_Col_1: char(10) NOT NULL",
					"TST_Col_2: char(10) NULL",
					"TST_Col_3: nvarchar(max) NULL",
					"TST_Col_4: nchar(10) NULL",
					"TST_Col_5: varchar(10) NULL",
					"TST_Col_6: char(10) NULL",
					"TST_Col_7: char(10) SPARSE NULL",
					"TST_Col_8: char(10) SPARSE NULL",
					newColumnPrefix + "TST_Col_1: nchar(10) NULL",
					newColumnPrefix + "TST_Col_2: varchar(100) NOT NULL",
					newColumnPrefix + "TST_Col_6: nchar(10) SPARSE NULL",
					newColumnPrefix + "TST_Col_7: nchar(10) SPARSE NULL",
					newColumnPrefix + "TST_Col_8: nchar(10) NOT NULL",
				];
				AssertColumnDefinitions("New types", expectedColumnDefinitions);

				CombineAssertions(() =>
				{
					AssertPopulatedValues(1, "          ", "", null, null, "          ");
					AssertPopulatedValues(2, "          ", "", null, "AB        ", "BCDE      ");
					AssertPopulatedValues(3, "12345     ", "O'Kiev", "123       ", null, "ABC       ");
					AssertPopulatedValues(4, "12345     ", "O'Kiev", "12345678  ", "AB        ", "          ");
					AssertPopulatedValues(5, "12345     ", "O'Kiev", "12345678  ", "AB        ", "BCDE      ");
				});
			}
		}

		public void TestConvertCharToChar_EmptyTable()
		{
			var oldColumnName = "TST_Col_1";
			var newColumnName = ColumnSynchroniser.ConvertCharToCharColumnPrefix + oldColumnName;

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AssertEquals($"Old column {oldColumnName} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName));
				AssertEquals("Old type", "char", GetColumnTypeName(targetTable, oldColumnName));
				AssertEquals($"New column {newColumnName} exists?", false, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName));

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharToChar();

				AssertEquals($"Old column {oldColumnName} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName));
				AssertEquals("Old type", "char", GetColumnTypeName(targetTable, oldColumnName));
				AssertEquals($"New column {newColumnName} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName));
				AssertEquals("New type", "nchar", GetColumnTypeName(targetTable, newColumnName));
			}
		}

		#region Implementation

		void AssertColumnDefinitions(string message, IEnumerable<string> expectedColumnDefinitions)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Get column definitions
SELECT
	col_definition = CONCAT(c.name, ': ',
		t.name,
		'(',
			CASE
				WHEN c.max_length = -1 THEN 'max'
				WHEN t.name in ('char', 'varchar') THEN CONVERT(varchar, c.max_length)
				ELSE CONVERT(varchar, c.max_length / 2)
			END,
		')',
		CASE WHEN c.is_sparse = 1 THEN ' SPARSE' ELSE '' END,
		CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END
		)
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID('dbo.{0}', N'U')
	AND t.name in ('char', 'varchar', 'nchar', 'nvarchar')
ORDER BY
	c.name
;",
				targetTable
				);

			List<string> actualColumnDefinitions = new List<string>();
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualColumnDefinitions.Add(reader.GetString(0));
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedColumnDefinitions, actualColumnDefinitions);
		}

		void AssertPopulatedValues(int id, string tST_Col_1, string tST_Col_2, string tST_Col_6, string tST_Col_7, string tST_Col_8)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
SELECT
	'id: {1}'
		+ ', Col_1: ' + ISNULL('[' + {2}TST_Col_1 + ']', 'NULL')
		+ ', Col_2: ' + ISNULL('[' + {2}TST_Col_2 + ']', 'NULL')
		+ ', Col_6: ' + ISNULL('[' + {2}TST_Col_6 + ']', 'NULL')
		+ ', Col_7: ' + ISNULL('[' + {2}TST_Col_7 + ']', 'NULL')
		+ ', Col_8: ' + ISNULL('[' + {2}TST_Col_8 + ']', 'NULL')
FROM
	dbo.[{0}]
WHERE
	TST_SystemCreateTimeUtc = {1};
",
				targetTable, // 0
				id,          // 1
				ColumnSynchroniser.ConvertCharToCharColumnPrefix // 2
				);

			var expected = String.Format(
				"id: {0}, Col_1: {1}, Col_2: {2}, Col_6: {3}, Col_7: {4}, Col_8: {5}",
				id,
				(tST_Col_1 == null) ? "NULL" : String.Format("[{0}]", tST_Col_1),
				(tST_Col_2 == null) ? "NULL" : String.Format("[{0}]", tST_Col_2),
				(tST_Col_6 == null) ? "NULL" : String.Format("[{0}]", tST_Col_6),
				(tST_Col_7 == null) ? "NULL" : String.Format("[{0}]", tST_Col_7),
				(tST_Col_8 == null) ? "NULL" : String.Format("[{0}]", tST_Col_8));

			using (var cmd = Db.Connection.Command(sql))
			{
				AssertEquals(expected, (string)cmd.ExecuteScalar());
			}
		}

		void TargetTableCreateValues(int id, string tST_Col_1, string tST_Col_2, string tST_Col_3, string tST_Col_4, string tST_Col_6, string tST_Col_7, string tST_Col_8)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT dbo.[{0}] (TST_SystemCreateTimeUtc, TST_Col_1, TST_Col_2, TST_Col_3, TST_Col_4, TST_Col_6, TST_Col_7, TST_Col_8) VALUES ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});
",
				targetTable,                                                                         // 0
				id,                                                                                  // 1
				(tST_Col_1 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_1)), // 2
				(tST_Col_2 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_2)), // 3
				(tST_Col_3 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_3)), // 4
				(tST_Col_4 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_4)), // 5
				(tST_Col_6 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_6)), // 6
				(tST_Col_7 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_7)), // 7
				(tST_Col_8 == null) ? "NULL" : String.Format("'{0}'", DataUtils.EscapeSingleQuotes(tST_Col_8))  // 8
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		static string GetColumnTypeName(string tableName, string columnName)
		{
			var typeName = (string)Db.Connection.ExecuteScalar($@"
SELECT
	t.name
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID({tableName.QuoteName('\'')})
	AND c.name = {columnName.QuoteName('\'')}
"
				);

			return (string.IsNullOrWhiteSpace(typeName)) ? "" : typeName;
		}

		IAuxiliaryDbCreator templateDbCreator;
		readonly UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();

		// Col_1: char(10)      NOT NULL -> nchar(10)         NULL
		// Col_2: char(10)          NULL -> varchar(100)  NOT NULL DEFAULT ('')
		// Col_3: nvarchar(max)     NULL -> char(3)           NULL
		// Col_4: nchar(10)         NULL -> nvarchar(max) NOT NULL
		// Col_5: varchar(10)       NULL -> varchar(20)   NOT NULL -- should be skipped by upgrader
		readonly IEnumerable<string> targetPrerequisiteColumnList = new string[]
		{
			"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
			"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
			"TST_Col_1               char(10)         NOT NULL",
			"TST_Col_2               char(10)             NULL",
			"TST_Col_3               nvarchar(max)        NULL",
			"TST_Col_4               nchar(10)            NULL",
			"TST_Col_5               varchar(10)          NULL",
			"TST_Col_6               char(10)             NULL",
			"TST_Col_7               char(10)           SPARSE",
			"TST_Col_8               char(10)           SPARSE",
		};

		readonly IEnumerable<string> templatePrerequisiteColumnList = new string[]
		{
			"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
			"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
			"TST_Col_1               nchar(10)            NULL",
			"TST_Col_2               varchar(100)     NOT NULL CONSTRAINT DF_TST_Col_2 DEFAULT ('')",
			"TST_Col_3               char(3)              NULL",
			"TST_Col_4               nvarchar(max)    NOT NULL",
			"TST_Col_5               varchar(20)      NOT NULL",
			"TST_Col_6               nchar(10)        SPARSE",
			"TST_Col_7               nchar(10)        SPARSE",
			"TST_Col_8               nchar(10)        NOT NULL",
		};

		const string templateDb = "_testPreSynchroniser_TemplateDb";
		const string targetTable = "_testPreSynchroniser_TargetTable";

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
		}

		void DropTemplateDatabase()
		{
			templateDbCreator.Drop();
		}

		#endregion // Implementation
	}
}
