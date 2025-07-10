using System;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public abstract class TestCaseWithMockMainDbAndTemplateDb : SchemaSyncTestCase
	{
		public TestCaseWithMockMainDbAndTemplateDb()
		{
			this.mockMainDbCreator = GetMockMainDbCreator();
			this.mockTemplateDbCreator = GetMockTemplateDbCreator();
		}

		void CreateMockDbs()
		{
			mockMainDbCreator.CreateDropExisting();
			mockTemplateDbCreator.CreateDropExisting();
		}

		void DropMockDbs()
		{
			mockMainDbCreator.Drop();
			mockTemplateDbCreator.Drop();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateMockDbs();
		}

		protected override void TearDown()
		{
			DropMockDbs();
			DbCommitTracker.Ignore(mockMainDb);
			base.TearDown();
		}

		protected virtual IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new MainUpgradeDbCreatorForTesting(mockMainDb);
		}

		protected virtual IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new MainTemplateDbCreatorForTesting(mockTemplateDb);
		}

		protected void RunActionOnMockMainDb(Action actionToRun)
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				actionToRun();
			}
		}

		protected void AssertColumnExistInMockMainDb(string tableName, string columnName, string expectedType, string isNullable, string length)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}].INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{1}'
				AND COLUMN_NAME = '{2}'
				AND DATA_TYPE = '{3}'
				AND IS_NULLABLE = '{4}'",
				mockMainDb, tableName, columnName, expectedType, isNullable);

			if (length != null && expectedType == "DateTimeOffset")
			{
				sqlText += " AND DATETIME_PRECISION = " + length;
			}
			else if (length != null && expectedType == "decimal")
			{
				var precisionAndScale = length.Split(',');
				var precision = int.Parse(precisionAndScale[0]);
				var scale = int.Parse(precisionAndScale[1]);
				sqlText += $" AND NUMERIC_PRECISION = {precision} AND NUMERIC_SCALE = {scale} ";
			}
			else if (length != null)
			{
				sqlText += " AND CHARACTER_MAXIMUM_LENGTH = " + length;
			}

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			string assertMessage = String.Format(
				"Column Exists? => {0}.{1} / Type: {2} / IsNullable: {3}{4}",
				tableName, columnName, expectedType, isNullable, (length == null) ? "" : " / Length: " + length);
			AssertEquals(assertMessage, true, qtyRows == 1);
		}

		protected void AssertColumnDoesNotExistInMockMainDb(string tableName, string columnName)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}].INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{1}'
				AND COLUMN_NAME = '{2}'",
				mockMainDb, tableName, columnName);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Column Exists? => {0}.{1}", tableName, columnName);
			AssertEquals(assertMessage, false, qtyRows != 0);
		}

		protected void AssertIdentityMockMainDbIdentityColumnSeedAndIncrement(string tableName, string columnName, int expectedSeed, int expectedIncrement)
		{
			string sqlText = String.Format(@"
				SELECT
					col.seed_value,
					col.increment_value
				FROM
					[{0}].sys.tables tab
					INNER JOIN [{0}].sys.identity_columns col ON col.object_id = tab.object_id
				WHERE
					tab.name = '{1}'
					AND col.name = '{2}'",
				mockMainDb, tableName, columnName);

			using (var reader = TestConnection.Command(sqlText).ExecuteReader())
			{
				if (reader.Read())
				{
					int actualSeed = Convert.ToInt32(reader[0]);
					AssertEquals(String.Format("[{0}].[{1}] identity seed:", tableName, columnName), expectedSeed, actualSeed);
					int actualIncrement = Convert.ToInt32(reader[1]);
					AssertEquals(String.Format("[{0}].[{1}] identity increment:", tableName, columnName), expectedIncrement, actualIncrement);
				}
				else
				{
					Fail(String.Format("[{0}].[{1}] is not an identity column or does not exist.", tableName, columnName));
				}
			}
		}

		protected void AssertClassificationExistInMockMainDb(string tableName, string columnName, string label = null, string labelId = null, string information = null, string informationId = null, string rank = null)
		{
			var sqlBuilder = new StringBuilder();
			var sql = $@"
FROM
	{mockMainDb.QuoteName()}.sys.sensitivity_classifications AS cf
	LEFT JOIN {mockMainDb.QuoteName()}.sys.objects           AS obj ON cf.major_id = obj.object_id
	LEFT JOIN {mockMainDb.QuoteName()}.sys.columns           AS col ON cf.major_id = col.object_id AND cf.minor_id = col.column_id
WHERE 1=1
	AND obj.type = 'U'
	AND obj.name = N'{tableName}'
	AND col.name = N'{columnName}'
";
			sqlBuilder.Append(sql);

			if (label != null)
			{
				sqlBuilder.Append($"AND CONVERT(NVARCHAR(128), cf.label) = N'{label}'");
			}
			if (labelId != null)
			{
				sqlBuilder.Append($"AND CONVERT(NVARCHAR(128), cf.label_id) = N'{labelId}'");
			}
			if (information != null)
			{
				sqlBuilder.Append($"AND CONVERT(NVARCHAR(128), cf.information_type) = N'{information}'");
			}
			if (informationId != null)
			{
				sqlBuilder.Append($"AND CONVERT(NVARCHAR(128), cf.information_type_id) = N'{informationId}'");
			}
			if (rank != null)
			{
				sqlBuilder.Append($"AND CONVERT(NVARCHAR(128), cf.rank_desc) = N'{rank}'");
			}

			AssertEquals($"Classification with label [{label}], label_id [{labelId}], information_type [{information}], information_type_id [{informationId}], rank [{rank}] does not exist on [{tableName}].[{columnName}]", true, TestConnection.Exists(sqlBuilder.ToString()));
		}

		protected void AssertClassificationDoesNotExistInMockMainDb(string tableName, string columnName)
		{
			var sql = $@"
FROM
	{mockMainDb.QuoteName()}.sys.sensitivity_classifications AS cf
	LEFT JOIN {mockMainDb.QuoteName()}.sys.objects           AS obj ON cf.major_id = obj.object_id
	LEFT JOIN {mockMainDb.QuoteName()}.sys.columns           AS col ON cf.major_id = col.object_id AND cf.minor_id = col.column_id
WHERE 1=1
	AND obj.type = 'U'
	AND obj.name = N'{tableName}'
	AND col.name = N'{columnName}'
";

			AssertEquals($"Classification exists on [{tableName}].[{columnName}]", false, TestConnection.Exists(sql));
		}

		protected void AssertComputedColumnDefinition(string database, string schema, string table, string column, string expected)
		{
			var fullTableName = schema + "." + table;
			AssertComputedColumnDefinition(database, fullTableName, column, expected);
		}

		protected void AssertComputedColumnDefinition(string database, string fullTableName, string column, string expected)
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(database))
			{
				var actual = TestConnection.ExecuteScalar<string>($@"
SELECT
	definition = ISNULL(
		(
			SELECT
				CONCAT(N''
					, Col.name
					, N' ', Typ.name
					, N' ', IIF(Col.is_nullable = 1, N'NULL', N'NOT NULL')
					, N' ', IIF(Col.is_persisted = 1, N'PERSISTED', N'NOT PERSISTED')
					, N' AS ', Col.definition
					)
			FROM
				sys.computed_columns AS Col
				JOIN sys.types       AS Typ ON Typ.user_type_id = Col.user_type_id
			WHERE 1=1
				AND Col.object_id = OBJECT_ID(@fullTableName, 'U')
				AND Col.name = @column
		)
		, N'')


				"
				, (cmd) =>
				{
						cmd.AddParameter("@fullTableName", SqlDbType.NVarChar, 257, fullTableName);
						cmd.AddParameter("@column", SqlDbType.NVarChar, 128, column);
				});

				AssertEquals($"Definition for the column [{database}].{fullTableName}.{column}:", expected, actual);
			}
		}

		protected const string mockMainDb = UpgUtils.UpgraderPrefix + "TestCaseWithMockMainDbAndTemplateDb-MainDb";
		protected const string mockTemplateDb = UpgUtils.UpgraderPrefix + "TestCaseWithMockMainDbAndTemplateDb-TemplateDb";

		readonly IAuxiliaryDbCreator mockMainDbCreator;
		readonly IAuxiliaryDbCreator mockTemplateDbCreator;
	}
}
