using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OnlinePreUpgrade.Internal
{
	[UseSnapshotProtection]
	class ConvertDateTimesToDateTest : TestCase
	{
		public void TestConvertDateTimesToDate()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var newColumnPrefix = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix;

				// Col_1: smalldatetime NOT NULL   -> date NOT NULL
				// Col_2: smalldatetime     NULL   -> date     NULL
				// Col_3: datetime      NOT NULL   -> date NOT NULL
				// Col_4: datetime          NULL   -> date     NULL
				// Col_5: datetime2     NOT NULL   -> date NOT NULL
				// Col_6: datetime2         NULL   -> date     NULL
				// Col_7: smalldatetime     NULL   -> date NOT NULL DEFAULT '2024-01-01'
				// Col_8: smalldatetime     NULL   -> date SPARSE
				// Col_9: smalldatetime     SPARSE -> date SPARSE
				// Col_10: smalldatetime    SPARSE -> date NOT NULL

				var expectedColumnDefinitions = new[]
				{
					"TST_Col_1: smalldatetime NOT NULL",
					"TST_Col_2: smalldatetime NULL",
					"TST_Col_3: datetime NOT NULL",
					"TST_Col_4: datetime NULL",
					"TST_Col_5: datetime2 NOT NULL",
					"TST_Col_6: datetime2 NULL",
					"TST_Col_7: smalldatetime NULL",
					"TST_Col_8: smalldatetime NULL",
					"TST_Col_9: smalldatetime SPARSE NULL",
					"TST_Col_10: smalldatetime SPARSE NULL",
				};
				AssertColumnDefinitions("PRECONDITION", expectedColumnDefinitions);

				TargetTableCreateValues([
					"1, '2024-01-01 00:00:00', '2024-01-01 00:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-02 10:00:00', '2025-01-03 10:00:00', '2025-01-04 10:00:00'",
					"2, '2024-01-01 00:00:00', NULL, '2025-01-01 10:00:00.122', NULL, '2025-01-01 10:00:00.122', NULL, NULL, NULL, NULL, '2025-01-05 10:00:00'",
				]);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertDateTimesToDate();

				expectedColumnDefinitions =
				[
					"TST_Col_1: smalldatetime NOT NULL",
					"TST_Col_2: smalldatetime NULL",
					"TST_Col_3: datetime NOT NULL",
					"TST_Col_4: datetime NULL",
					"TST_Col_5: datetime2 NOT NULL",
					"TST_Col_6: datetime2 NULL",
					"TST_Col_7: smalldatetime NULL",
					"TST_Col_8: smalldatetime NULL",
					"TST_Col_9: smalldatetime SPARSE NULL",
					"TST_Col_10: smalldatetime SPARSE NULL",
					newColumnPrefix + "TST_Col_1: date NOT NULL",
					newColumnPrefix + "TST_Col_2: date NULL",
					newColumnPrefix + "TST_Col_3: date NOT NULL",
					newColumnPrefix + "TST_Col_4: date NULL",
					newColumnPrefix + "TST_Col_5: date NOT NULL",
					newColumnPrefix + "TST_Col_6: date NULL",
					newColumnPrefix + "TST_Col_7: date NOT NULL",
					newColumnPrefix + "TST_Col_8: date SPARSE NULL",
					newColumnPrefix + "TST_Col_9: date SPARSE NULL",
					newColumnPrefix + "TST_Col_10: date NOT NULL",
				];
				AssertColumnDefinitions("New Types", expectedColumnDefinitions);

				CombineAssertions(() =>
				{
					AssertPopulatedValues(1, "Col_1: [2024-01-01], Col_2: [2024-01-01], Col_3: [2025-01-01], Col_4: [2025-01-01], Col_5: [2025-01-01], Col_6: [2025-01-01], Col_7: [2025-01-01], Col_8: [2025-01-02], Col_9: [2025-01-03], Col_10: [2025-01-04]");
					AssertPopulatedValues(2, "Col_1: [2024-01-01], Col_2: NULL, Col_3: [2025-01-01], Col_4: NULL, Col_5: [2025-01-01], Col_6: NULL, Col_7: [2024-01-01], Col_8: NULL, Col_9: NULL, Col_10: [2025-01-05]");
				});
			}
		}

		public void TestConvertDateTimesToDate_Trigger()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				CreateAuditColumnsUpdateTrigger();

				TargetTableCreateValues([
					"1, '2024-01-01 00:00:00', '2024-01-01 00:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-02 10:00:00', '2025-01-03 10:00:00', '2025-01-04 10:00:00'",
					"2, '2024-01-01 00:00:00', NULL, '2025-01-01 10:00:00.122', NULL, '2025-01-01 10:00:00.122', NULL, NULL, NULL, NULL, NULL",
				]);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertDateTimesToDate();

				TargetTableCreateValues(["3, '2024-01-01 00:00:00', '2024-01-01 00:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-01 10:00:00', '2025-01-02 10:00:00', '2025-01-03 10:00:00', '2025-01-04 10:00:00'"]);
				AssertPopulatedValues(3, "Col_1: [2024-01-01], Col_2: [2024-01-01], Col_3: [2025-01-01], Col_4: [2025-01-01], Col_5: [2025-01-01], Col_6: [2025-01-01], Col_7: [2025-01-01], Col_8: [2025-01-02], Col_9: [2025-01-03], Col_10: [2025-01-04]");

				AssertNoExceptionThrown(() => TargetTableUpdateValue(3, "TST_Col_1", "2025-01-01 00:00:00"));
				AssertPopulatedValues(3, "Col_1: [2025-01-01], Col_2: [2024-01-01], Col_3: [2025-01-01], Col_4: [2025-01-01], Col_5: [2025-01-01], Col_6: [2025-01-01], Col_7: [2025-01-01], Col_8: [2025-01-02], Col_9: [2025-01-03], Col_10: [2025-01-04]");
			}
		}

		public void TestConvertDateTimesToDate_EmptyTable()
		{
			var oldColumnName_smalldatetime = "TST_Col_1";
			var oldColumnName_datetime = "TST_Col_3";
			var oldColumnName_datetime2 = "TST_Col_5";
			var newColumnName_smalldatetime = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix + oldColumnName_smalldatetime;
			var newColumnName_datetime = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix + oldColumnName_datetime;
			var newColumnName_datetime2 = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix + oldColumnName_datetime2;

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AssertEquals($"Old column {oldColumnName_smalldatetime} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName_smalldatetime));
				AssertEquals($"Old column {oldColumnName_datetime} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName_datetime));
				AssertEquals($"Old column {oldColumnName_datetime2} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName_datetime2));
				AssertEquals("Old type", "smalldatetime", GetColumnTypeName(targetTable, oldColumnName_smalldatetime));
				AssertEquals("Old type", "datetime", GetColumnTypeName(targetTable, oldColumnName_datetime));
				AssertEquals("Old type", "datetime2", GetColumnTypeName(targetTable, oldColumnName_datetime2));
				AssertEquals($"New column {newColumnName_smalldatetime} exists?", false, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName_smalldatetime));
				AssertEquals($"New column {newColumnName_datetime} exists?", false, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName_datetime));
				AssertEquals($"New column {newColumnName_datetime2} exists?", false, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName_datetime2));

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertDateTimesToDate();

				AssertEquals($"Old column {oldColumnName_smalldatetime} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName_smalldatetime));
				AssertEquals($"Old column {oldColumnName_datetime} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName_datetime));
				AssertEquals($"Old column {oldColumnName_datetime2} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName_datetime2));
				AssertEquals("Old type", "smalldatetime", GetColumnTypeName(targetTable, oldColumnName_smalldatetime));
				AssertEquals("Old type", "datetime", GetColumnTypeName(targetTable, oldColumnName_datetime));
				AssertEquals("Old type", "datetime2", GetColumnTypeName(targetTable, oldColumnName_datetime2));
				AssertEquals($"New column {newColumnName_smalldatetime} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName_smalldatetime));
				AssertEquals($"New column {newColumnName_datetime} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName_datetime));
				AssertEquals($"New column {newColumnName_datetime2} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName_datetime2));
				AssertEquals("New type", "date", GetColumnTypeName(targetTable, newColumnName_smalldatetime));
				AssertEquals("New type", "date", GetColumnTypeName(targetTable, newColumnName_datetime));
				AssertEquals("New type", "date", GetColumnTypeName(targetTable, newColumnName_datetime2));
			}
		}

		void CreateAuditColumnsUpdateTrigger()
		{
			var sql = @$"
CREATE TRIGGER TST_SystemLastEditTimeUtc_Update_Trigger
ON {targetTable}
AFTER UPDATE
AS
BEGIN
	IF
		(NOT EXISTS(SELECT NULL FROM inserted))
	RETURN

	IF (NOT UPDATE([TST_SystemLastEditTimeUtc]))
	BEGIN
		RAISERROR('Attempt to update without [SystemLastEditTimeUtc]', 16, 1);
		RETURN
	END
END
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void AssertColumnDefinitions(string message, IEnumerable<string> expectedColumnDefinitions)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- Get column definitions
SELECT
	col_definition = CONCAT(c.name,
		': ', t.name,
		CASE WHEN c.is_sparse = 1 THEN ' SPARSE' ELSE '' END,
		CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END
	)
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID('dbo.{0}', N'U')
	AND t.name IN ('smalldatetime', 'date', 'datetime', 'datetime2')
	AND c.name NOT LIKE '%SystemLastEdit%'
ORDER BY
	c.name
;",
				targetTable
				);

			var actualColumnDefinitions = new List<string>();
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

		void AssertPopulatedValues(int id, string values)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
SELECT
	'id: {1}'
		+ ', Col_1: ' + ISNULL('[' + FORMAT({2}TST_Col_1, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_2: ' + ISNULL('[' + FORMAT({2}TST_Col_2, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_3: ' + ISNULL('[' + FORMAT({2}TST_Col_3, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_4: ' + ISNULL('[' + FORMAT({2}TST_Col_4, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_5: ' + ISNULL('[' + FORMAT({2}TST_Col_5, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_6: ' + ISNULL('[' + FORMAT({2}TST_Col_6, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_7: ' + ISNULL('[' + FORMAT({2}TST_Col_7, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_8: ' + ISNULL('[' + FORMAT({2}TST_Col_8, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_9: ' + ISNULL('[' + FORMAT({2}TST_Col_9, 'yyyy-MM-dd') + ']', 'NULL')
		+ ', Col_10: ' + ISNULL('[' + FORMAT({2}TST_Col_10, 'yyyy-MM-dd') + ']', 'NULL')
FROM
	dbo.[{0}]
WHERE
	TST_SystemCreateUserId = {1};
",
				targetTable, // 0,
				id,          // 1
				ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix // 2
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				AssertEquals($"id: {id}, {values}", (string)cmd.ExecuteScalar());
			}
		}

		void TargetTableCreateValues(string[] valuesList)
		{
			var sql = $@"
INSERT dbo.[{targetTable}]
(
	TST_SystemLastEditTimeUtc,
	TST_SystemLastEditUser,
	TST_SystemCreateUserId,
	TST_Col_1,
	TST_Col_2,
	TST_Col_3,
	TST_Col_4,
	TST_Col_5,
	TST_Col_6,
	TST_Col_7,
	TST_Col_8,
	TST_Col_9,
	TST_Col_10
) VALUES {string.Join(", ", valuesList.Select(x => $"(GETUTCDATE(), '~BP', {x})"))};
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void TargetTableUpdateValue(int id, string columnName, string value)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, "UPDATE dbo.[{0}] SET {1} = '{2}', TST_SystemLastEditTimeUtc = GETUTCDATE(), TST_SystemLastEditUser = '~BP' WHERE TST_SystemCreateUserId = {3};",
				targetTable,
				columnName,
				value,
				id);

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
		const string templateDb = "_testPreSynchroniser_TemplateDb";
		const string targetTable = "_testPreSynchroniser_TargetTable";

		readonly UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();

		static readonly IEnumerable<string> targetPrerequisiteColumnList = new string[]
		{
			"TST_PK                    uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
			"TST_SystemCreateUserId    int              NOT NULL UNIQUE CLUSTERED",
			"TST_SystemLastEditTimeUtc smalldatetime    NULL",
			"TST_SystemLastEditUser    nvarchar(100)    NULL",
			"TST_Col_1                 smalldatetime    NOT NULL",
			"TST_Col_2                 smalldatetime    NULL",
			"TST_Col_3                 datetime         NOT NULL",
			"TST_Col_4                 datetime         NULL",
			"TST_Col_5                 datetime2        NOT NULL",
			"TST_Col_6                 datetime2        NULL",
			"TST_Col_7                 smalldatetime    NULL",
			"TST_Col_8                 smalldatetime    NULL",
			"TST_Col_9                 smalldatetime    SPARSE",
			"TST_Col_10                smalldatetime    SPARSE",
		};

		static readonly IEnumerable<string> templatePrerequisiteColumnList = new string[]
		{
			"TST_PK                    uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
			"TST_SystemCreateUserId    int              NOT NULL UNIQUE CLUSTERED",
			"TST_SystemLastEditTimeUtc smalldatetime    NULL",
			"TST_SystemLastEditUser    nvarchar(100)    NULL",
			"TST_Col_1                 date             NOT NULL",
			"TST_Col_2                 date             NULL",
			"TST_Col_3                 date             NOT NULL",
			"TST_Col_4                 date             NULL",
			"TST_Col_5                 date             NOT NULL",
			"TST_Col_6                 date             NULL",
			"TST_Col_7                 date             NOT NULL DEFAULT '2024-01-01'",
			"TST_Col_8                 date             SPARSE",
			"TST_Col_9                 date             SPARSE",
			"TST_Col_10                date             NOT NULL",
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

		void CreateTemplateDatabase()
		{
			templateDbCreator = new TablePreSynchroniserTestTemplateDbCreator(templateDb, targetTable, templatePrerequisiteColumnList);
			templateDbCreator.CreateDropExisting();
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

		void DropTemplateDatabase()
		{
			templateDbCreator.Drop();
		}
	}
}
