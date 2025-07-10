using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[RequiresLargeLogFile]
	[UseSnapshotProtection]
	class ConvertCharFlagToBitTest : TestCase
	{
		public void TestConvertCharFlagsToBit_Value()
		{
			// Arrange
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var yES = "'Y'";
				var nO = "'N'";
				var eMPTY = "''";

				var t2_PK = new Guid("00000000-2222-0000-0000-000000000000");

				TargetTableCreateValues(1, yES, nO, null, null, null, null, null);
				TargetTableCreateValues(2, yES, yES, yES, yES, yES, yES, yES);
				TargetTableCreateValues(3, nO, nO, nO, nO, nO, nO, nO);
				TargetTableCreateValues(4, eMPTY, eMPTY, eMPTY, eMPTY, eMPTY, eMPTY, eMPTY);

				Table2CreateValues(t2_PK);
				AssertView("PRECONDITION", t2_PK);

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);

				SqlEventTracker.Instance.Clear(); // Make sure we don't have any events from previous tests

				// Act
				synchroniser.ConvertCharFlagsToBit();

				// Assert
				// The events stored in SqlEventTracker.Instance.SqlEventDescription can be overridden by the following assertions, so we need to check them first.
				AssertMaxdopOptionIsUsedInPopulatingTargetTable();

				AssertNoExceptionThrown(() => AssertView("PRECONDITION", t2_PK));

				AssertPopulatedValues(1, 1, 0, 0, null, null, null, 0);
				AssertPopulatedValues(2, 1, 1, 1, 1, 1, 1, 1);
				AssertPopulatedValues(3, 0, 0, 0, 0, 0, 0, 0);
				AssertPopulatedValues(4, 0, null, 0, null, null, null, 0);
			}
		}

		public void TestConvertCharFlagsToBit_Type()
		{
			// Arrange
			string[] expectedColumnDefinitions =
			[
				"[TST_Flag_1] varchar(1) NOT NULL",
				"[TST_Flag_2] char(1) NOT NULL",
				"[TST_Flag_3] varchar(1) NULL",
				"[TST_Flag_4] char(1) NULL",
				"[TST_Flag_5] char(1) NULL",
				"[TST_Flag_6] char(1) SPARSE NULL",
				"[TST_Flag_7] char(1) SPARSE NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_1] bit NOT NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_2] bit NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_3] bit NOT NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_4] bit NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_5] bit SPARSE NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_6] bit SPARSE NULL",
				$"[{ColumnSynchroniser.ConvertCharToBitColumnPrefix}TST_Flag_7] bit NOT NULL",
			];

			using var useAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
			var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);

			// Act
			synchroniser.ConvertCharFlagsToBit();

			// Assert
			var columns = ColumnChangeMetadataHelper.GetTableColumnMetadata(Db.Connection,"dbo", targetTable)
				.Where(x => x.ColumnName.Contains("TST_Flag_"))
				.Select(x => x.ColumnDeclaration)
				.ToArray();
			AssertContainsExactElementsInAnyOrder(expectedColumnDefinitions, columns);
		}

		public void TestConvertCharFlagsToBit_EmptyTable()
		{
			var oldColumnName = "TST_Flag_1";
			var newColumnName = ColumnSynchroniser.ConvertCharToBitColumnPrefix + oldColumnName;

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				AssertEquals($"Old column {oldColumnName} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName));
				AssertEquals("Old type", "varchar", GetColumnTypeName(targetTable, oldColumnName));
				AssertEquals($"New column {newColumnName} exists?", false, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName));

				var synchroniser = new TablePreSynchroniser(manager, Db.DatabaseName, templateDb);
				synchroniser.ConvertCharFlagsToBit();

				AssertEquals($"Old column {oldColumnName} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, oldColumnName));
				AssertEquals("Old type", "varchar", GetColumnTypeName(targetTable, oldColumnName));
				AssertEquals($"New column {newColumnName} exists?", true, DbObjectCreator.ColumnExists(Db.Connection, targetTable, newColumnName));
				AssertEquals("New type", "bit", GetColumnTypeName(targetTable, newColumnName));
			}
		}

		#region Implementation

		void AssertMaxdopOptionIsUsedInPopulatingTargetTable()
		{
			var sqlEvents = SqlEventTracker.Instance.SqlEventDescription;
			var populatingTargetTableQueries = sqlEvents.Split(new string[] { "<Command>" }, StringSplitOptions.RemoveEmptyEntries).Where(st => st.StartsWith("-- Populating target table", StringComparison.Ordinal)).ToArray();
			Assert("We should have at least one of these statements in the last 20 SQL events", populatingTargetTableQueries.Any());
			Assert("All of these should contain OPTION (MAXDOP 1)", populatingTargetTableQueries.All(st => st.Contains("\r\nOPTION (MAXDOP 1)\r\n")));
		}

		void AssertPopulatedValues(int id, int? flag_1, int? flag_2, int? flag_3, int? flag_4, int? flag_5, int? flag_6, int? flag_7)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
SELECT
	'id: {1}'
		+ ', Flag_1: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_1), 'NULL')
		+ ', Flag_2: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_2), 'NULL')
		+ ', Flag_3: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_3), 'NULL')
		+ ', Flag_4: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_4), 'NULL')
		+ ', Flag_5: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_5), 'NULL')
		+ ', Flag_6: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_6), 'NULL')
		+ ', Flag_7: ' + ISNULL(CONVERT(varchar, {2}TST_Flag_7), 'NULL')
FROM
	dbo.[{0}]
WHERE
	TST_SystemCreateTimeUtc = {1};
",
				targetTable, // 0
				id,          // 1
				ColumnSynchroniser.ConvertCharToBitColumnPrefix // 2
				);

			var expected = String.Format("id: {0}, Flag_1: {1}, Flag_2: {2}, Flag_3: {3}, Flag_4: {4}, Flag_5: {5}, Flag_6: {6}, Flag_7: {7}",
				id,
				(!flag_1.HasValue) ? "NULL" : flag_1.Value.ToString(),
				(!flag_2.HasValue) ? "NULL" : flag_2.Value.ToString(),
				(!flag_3.HasValue) ? "NULL" : flag_3.Value.ToString(),
				(!flag_4.HasValue) ? "NULL" : flag_4.Value.ToString(),
				(!flag_5.HasValue) ? "NULL" : flag_5.Value.ToString(),
				(!flag_6.HasValue) ? "NULL" : flag_6.Value.ToString(),
				(!flag_7.HasValue) ? "NULL" : flag_7.Value.ToString()
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				AssertEquals(expected, (string)cmd.ExecuteScalar());
			}
		}

		void AssertView(string message, Guid expectedTable2PK)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"SELECT TOP(1) T2_PK FROM dbo.[{0}];", testView);
			using (var cmd = Db.Connection.Command(sql))
			{
				AssertEquals(message, expectedTable2PK, (Guid)cmd.ExecuteScalar());
			}
		}

		void TargetTableCreateValues(int id, string tST_Flag_1, string tST_Flag_2, string tST_Flag_3, string tST_Flag_4, string tST_Flag_5, string tST_Flag_6, string tST_Flag_7)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT dbo.[{0}] (TST_SystemCreateTimeUtc, TST_Flag_1, TST_Flag_2, TST_Flag_3, TST_Flag_4, TST_Flag_5, TST_Flag_6, TST_Flag_7) VALUES ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});
",
				targetTable,          // 0
				id,                   // 1
				tST_Flag_1 ?? "NULL", // 2
				tST_Flag_2 ?? "NULL", // 3
				tST_Flag_3 ?? "NULL", // 4
				tST_Flag_4 ?? "NULL", // 5
				tST_Flag_5 ?? "NULL", // 6
				tST_Flag_6 ?? "NULL", // 7
				tST_Flag_7 ?? "NULL"  // 8
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void Table2CreateValues(Guid t2_PK)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"INSERT dbo.[{0}] VALUES ('{1}');", table2, t2_PK.ToString());
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

		readonly IEnumerable<string> targetPrerequisiteColumnList = new string[]
		{
			"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
			"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
			"TST_Flag_1              varchar(1)       NOT NULL",
			"TST_Flag_2              char(1)          NOT NULL",
			"TST_Flag_3              varchar(1)           NULL",
			"TST_Flag_4              char(1)              NULL",
			"TST_Flag_5              char(1)              NULL",
			"TST_Flag_6              char(1)            SPARSE",
			"TST_Flag_7              char(1)            SPARSE",
		};

		readonly IEnumerable<string> templatePrerequisiteColumnList = new string[]
		{
			"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
			"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
			"TST_Flag_1              bit              NOT NULL CONSTRAINT DF_TST_Flag_1 DEFAULT (0)",
			"TST_Flag_2              bit                  NULL",
			"TST_Flag_3              bit              NOT NULL",
			"TST_Flag_4              bit                  NULL",
			"TST_Flag_5              bit                SPARSE",
			"TST_Flag_6              bit                SPARSE",
			"TST_Flag_7              bit              NOT NULL",
		};

		readonly IEnumerable<string> table2_Columns = new string[]
		{
			"T2_PK uniqueidentifier NOT NULL",
		};

		const string templateDb = "_testPreSynchroniser_TemplateDb";
		const string targetTable = "_testPreSynchroniser_TargetTable";
		const string table2 = "_testPreSynchroniser_Table2";
		const string testView = "_testPreSynchroniser_View";

		protected override void SetUp()
		{
			base.SetUp();

			TablePreSynchroniser.CreatePreAddDb_ForTest();
			CreateTemplateDatabase();
			CreateTestTable(targetTable, targetPrerequisiteColumnList);
			CreateTestTable(table2, table2_Columns);
			CreateTestView(testView, targetTable, table2);
		}

		protected override void TearDown()
		{
			DropTemplateDatabase();
			TablePreSynchroniser.DropPreAddDb_ForTest();

			base.TearDown();
		}

		void CreateTestTable(string tableName, IEnumerable<string> columns)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
if (OBJECT_ID(N'[dbo].[{0}]', N'U') is NOT NULL) DROP TABLE [dbo].[{0}];
CREATE TABLE [dbo].[{0}]
(
	{1}
);
",
				tableName,
				String.Join(",\r\n\t", columns)
				);

			Db.Connection.ExecuteNonQuery(sql);
		}

		void CreateTestView(string viewName, string tableName1, string tableName2)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
CREATE VIEW [dbo].[{0}]
AS
	SELECT *
	FROM
		[dbo].[{1}] AS t1
		CROSS JOIN [dbo].[{2}] AS t2
;
"
				, viewName
				, tableName1
				, tableName2
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

		#endregion
	}
}
