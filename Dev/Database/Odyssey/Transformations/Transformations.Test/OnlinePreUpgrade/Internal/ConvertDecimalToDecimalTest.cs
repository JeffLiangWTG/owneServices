using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;

[UseSnapshotProtection]
public class ConvertDecimalToDecimalTest : TestCase
{
	public void TestConvertDecimalToDecimal()
	{
		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			var newColumnPrefix = ColumnSynchroniser.ConvertDecimalToDecimalColumnPrefix;

			// Col_1: decimal(10,5)  NOT NULL -> decimal(11,6)  NOT NULL
			// Col_2: decimal(10,5)  NOT NULL -> decimal(11,5)  NOT NULL
			// Col_3: decimal(10,5)  NOT NULL -> decimal(12,6)  NOT NULL
			// Col_4: decimal(10,5)      NULL -> decimal(12,6)      NULL
			// Col_5: decimal(10,5)      NULL -> decimal(12,6)  NOT NULL DEFAULT (0.0)
			// Col_6: decimal(1,0)	 NOT NULL -> decimal(38,37) NOT NULL
			// Col_7: decimal(36,18) NOT NULL -> decimal(38,19) NOT NULL
			// Col_8: decimal(10,5)  NULL     -> decimal(15,6)  SPARSE
			// Col_9: decimal(10,5)  SPARSE   -> decimal(15,6)  SPARSE
			// Col_10: decimal(10,5) SPARSE   -> decimal(15,6)  NOT NULL

			var expectedColumnDefinitions = new[]
			{
				"TST_Col_1: decimal(10,5) NOT NULL",
				"TST_Col_2: decimal(10,5) NOT NULL",
				"TST_Col_3: decimal(10,5) NOT NULL",
				"TST_Col_4: decimal(10,5) NULL",
				"TST_Col_5: decimal(10,5) NULL",
				"TST_Col_6: decimal(1,0) NOT NULL",
				"TST_Col_7: decimal(36,18) NOT NULL",
				"TST_Col_8: decimal(10,5) NULL",
				"TST_Col_9: decimal(10,5) SPARSE NULL",
				"TST_Col_10: decimal(10,5) SPARSE NULL",
			};
			AssertColumnDefinitions("PRECONDITION", expectedColumnDefinitions);

			TargetTableCreateValues(
			[
				"1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0",
				"2, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0",
				"3, 99999.99999, 99999.99999, 99999.99999, 99999.99999, 99999.99999, 9, 999999999999999999.999999999999999999, 99999.99999, 99999.99999, 99999.99999",
				"4, 123.123, 123.123, 123.123, null, null, 1, 123456789123456789.123456789123456789, null, null, null",
				"5, 123.123, 123.123, 123.123, null, null, 1, 123456789123456789.123456789123456789, null, null, null",
			]);

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDecimalToDecimal();

			expectedColumnDefinitions = new[]
			{
				"TST_Col_1: decimal(10,5) NOT NULL",
				"TST_Col_2: decimal(10,5) NOT NULL",
				"TST_Col_3: decimal(10,5) NOT NULL",
				"TST_Col_4: decimal(10,5) NULL",
				"TST_Col_5: decimal(10,5) NULL",
				"TST_Col_6: decimal(1,0) NOT NULL",
				"TST_Col_7: decimal(36,18) NOT NULL",
				"TST_Col_8: decimal(10,5) NULL",
				"TST_Col_9: decimal(10,5) SPARSE NULL",
				"TST_Col_10: decimal(10,5) SPARSE NULL",
				newColumnPrefix + "TST_Col_1: decimal(11,6) NOT NULL",
				newColumnPrefix + "TST_Col_2: decimal(11,5) NOT NULL",
				newColumnPrefix + "TST_Col_3: decimal(12,6) NOT NULL",
				newColumnPrefix + "TST_Col_4: decimal(12,6) NULL",
				newColumnPrefix + "TST_Col_5: decimal(12,6) NOT NULL",
				newColumnPrefix + "TST_Col_6: decimal(38,37) NOT NULL",
				newColumnPrefix + "TST_Col_7: decimal(38,19) NOT NULL",
				newColumnPrefix + "TST_Col_8: decimal(15,6) SPARSE NULL",
				newColumnPrefix + "TST_Col_9: decimal(15,6) SPARSE NULL",
				newColumnPrefix + "TST_Col_10: decimal(15,6) NOT NULL",
			};
			AssertColumnDefinitions("New types", expectedColumnDefinitions);

			CombineAssertions(() =>
			{
				AssertPopulatedValues(1, $"Col_1: [0.000000], Col_2: [0.00000], Col_3: [0.000000], Col_4: [0.000000], Col_5: [0.000000], Col_6: [{0.0m:F37}], Col_7: [{0.0m:F19}], Col_8: [0.000000], Col_9: [0.000000], Col_10: [0.000000]");
				AssertPopulatedValues(2, $"Col_1: [0.000000], Col_2: [0.00000], Col_3: [0.000000], Col_4: [0.000000], Col_5: [0.000000], Col_6: [{0.0m:F37}], Col_7: [{0.0m:F19}], Col_8: [0.000000], Col_9: [0.000000], Col_10: [0.000000]");
				AssertPopulatedValues(3, $"Col_1: [99999.999990], Col_2: [99999.99999], Col_3: [99999.999990], Col_4: [99999.999990], Col_5: [99999.999990], Col_6: [{9m:F37}], Col_7: [999999999999999999.9999999999999999990], Col_8: [99999.999990], Col_9: [99999.999990], Col_10: [99999.999990]");
				AssertPopulatedValues(4, $"Col_1: [123.123000], Col_2: [123.12300], Col_3: [123.123000], Col_4: NULL, Col_5: [0.000000], Col_6: [{1m:F37}], Col_7: [123456789123456789.1234567891234567890], Col_8: NULL, Col_9: NULL, Col_10: [0.000000]");
				AssertPopulatedValues(5, $"Col_1: [123.123000], Col_2: [123.12300], Col_3: [123.123000], Col_4: NULL, Col_5: [0.000000], Col_6: [{1m:F37}], Col_7: [123456789123456789.1234567891234567890], Col_8: NULL, Col_9: NULL, Col_10: [0.000000]");
			});
		}
	}

	public void TestConvertDecimalToDecimal_EmptyTable()
	{
		var oldColumnName = "TST_Col_1";
		var newColumnName = ColumnSynchroniser.ConvertDecimalToDecimalColumnPrefix + oldColumnName;

		using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
		{
			AssertEquals($"Old column {oldColumnName} exists?", expected: true, DbObjectCreator.ColumnExists(Db.Connection, TargetTable, oldColumnName));
			AssertEquals("Old type", "decimal", GetColumnTypeName(TargetTable, oldColumnName));
			AssertEquals($"New column {newColumnName} exists?", expected: false, DbObjectCreator.ColumnExists(Db.Connection, TargetTable, newColumnName));

			var synchroniser = new TablePreSynchroniser(_manager, Db.DatabaseName, TemplateDb);
			synchroniser.ConvertDecimalToDecimal();

			AssertEquals($"Old column {oldColumnName} exists?", expected: true, DbObjectCreator.ColumnExists(Db.Connection, TargetTable, oldColumnName));
			AssertEquals("Old type", "decimal", GetColumnTypeName(TargetTable, oldColumnName));
			AssertEquals($"New column {newColumnName} exists?", expected: true, DbObjectCreator.ColumnExists(Db.Connection, TargetTable, newColumnName));
			AssertEquals("New type", "decimal", GetColumnTypeName(TargetTable, newColumnName));
		}
	}

	#region Implementation

	static void AssertColumnDefinitions(string message, IEnumerable<string> expectedColumnDefinitions)
	{
		var sql = string.Format(CultureInfo.InvariantCulture, @"-- Get column definitions
SELECT
	col_definition = CONCAT(c.name,
		': decimal(',
		c.precision,
		',',
		c.scale,
		')',
		CASE WHEN c.is_sparse = 1 THEN ' SPARSE' ELSE '' END,
		CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END
	)
FROM
	sys.columns    AS c
	JOIN sys.types AS t ON t.user_type_id = c.user_type_id
WHERE 1=1
	AND c.object_id = OBJECT_ID('dbo.{0}', N'U')
	AND t.name = 'decimal'
ORDER BY
	c.name
;",
			TargetTable
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

	static void AssertPopulatedValues(int id, string values)
	{
		var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	'id: {1}'
		+ ', Col_1: ' + ISNULL('[' + CAST({2}TST_Col_1 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_2: ' + ISNULL('[' + CAST({2}TST_Col_2 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_3: ' + ISNULL('[' + CAST({2}TST_Col_3 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_4: ' + ISNULL('[' + CAST({2}TST_Col_4 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_5: ' + ISNULL('[' + CAST({2}TST_Col_5 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_6: ' + ISNULL('[' + CAST({2}TST_Col_6 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_7: ' + ISNULL('[' + CAST({2}TST_Col_7 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_8: ' + ISNULL('[' + CAST({2}TST_Col_8 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_9: ' + ISNULL('[' + CAST({2}TST_Col_9 AS VARCHAR(39)) + ']', 'NULL')
		+ ', Col_10: ' + ISNULL('[' + CAST({2}TST_Col_10 AS VARCHAR(39)) + ']', 'NULL')
FROM
	dbo.[{0}]
WHERE
	TST_SystemCreateTimeUtc = {1};
",
			TargetTable, // 0
			id,          // 1
			ColumnSynchroniser.ConvertDecimalToDecimalColumnPrefix // 2
			);

		using var cmd = Db.Connection.Command(sql);
		AssertEquals($"id: {id}, {values}", (string)cmd.ExecuteScalar());
	}

	void TargetTableCreateValues(string[] valuesList)
	{
		var sql = string.Format(
			CultureInfo.InvariantCulture,
			"INSERT INTO dbo.[{0}] (TST_SystemCreateTimeUTC, TST_Col_1, TST_Col_2, TST_Col_3, TST_Col_4, TST_Col_5, TST_Col_6, TST_Col_7, TST_Col_8, TST_Col_9, TST_Col_10) VALUES {1};",
			TargetTable,
			string.Join(", ", valuesList.Select(v => '(' + v + ')'))
		);

		using var cmd = Db.Connection.Command(sql);
		_ = cmd.ExecuteNonQuery();
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

		return string.IsNullOrWhiteSpace(typeName) ? "" : typeName;
	}

	IAuxiliaryDbCreator _templateDbCreator;
	readonly UpgradeManagerForTestWithOutputBuffer _manager = new ();

	static readonly IEnumerable<string> TargetPrerequisiteColumnList = new[]
	{
		"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
		"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
		"TST_Col_1               decimal(10,5)    NOT NULL",
		"TST_Col_2               decimal(10,5)    NOT NULL",
		"TST_Col_3               decimal(10,5)    NOT NULL",
		"TST_Col_4               decimal(10,5)        NULL",
		"TST_Col_5               decimal(10,5)        NULL",
		"TST_Col_6               decimal(1,0)     NOT NULL",
		"TST_Col_7               decimal(36,18)   NOT NULL",
		"TST_Col_8               decimal(10,5)        NULL",
		"TST_Col_9               decimal(10,5)      SPARSE",
		"TST_Col_10              decimal(10,5)      SPARSE",
	};

	static readonly IEnumerable<string> TemplatePrerequisiteColumnList = new[]
	{
		"TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED",
		"TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED",
		"TST_Col_1               decimal(11,6)    NOT NULL",
		"TST_Col_2               decimal(11,5)    NOT NULL",
		"TST_Col_3               decimal(12,6)    NOT NULL",
		"TST_Col_4               decimal(12,6)        NULL",
		"TST_Col_5               decimal(12,6)    NOT NULL CONSTRAINT DF_TST_Col_5 DEFAULT (0.0)",
		"TST_Col_6               decimal(38,37)   NOT NULL",
		"TST_Col_7               decimal(38,19)   NOT NULL",
		"TST_Col_8               decimal(15,6)      SPARSE",
		"TST_Col_9               decimal(15,6)      SPARSE",
		"TST_Col_10              decimal(15,6)    NOT NULL",
	};

	const string TemplateDb = "_testPreSynchroniser_TemplateDb";
	const string TargetTable = "_testPreSynchroniser_TargetTable";

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
	}

	void DropTemplateDatabase()
	{
		_templateDbCreator.Drop();
	}

	#endregion // Implementation
}
