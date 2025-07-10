using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class AuditDatabaseSynchronisationWrapperSpecificTest : TestCase
	{
		public void TestEnsureCharToCharChangeIsRenamed()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testConnection, TestDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(testConnection, TestDbName);

					using (((ICurrentDbControl)testConnection).UseDatabase(TestDbName))
					{
						var lsn = new byte[] { 0x1 };
						var tranEndTime = DateTime.UtcNow;
						AddBiAdminTables(testConnection, TestDbName);
						InsertLsnTimeMapping(testConnection, TestDbName, lsn, tranEndTime);
						AddTestColumns1(testConnection);

						CombineAssertions(() =>
						{
							// Assert columns before
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]RenameMe01", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]RenameMe02", expected: true);
						});

						new AuditDatabaseSynchronisationWrapperForTesting(TestDbName, testConnection).Run();

						CombineAssertions(() =>
						{
							// Assert columns afterwards
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]RenameMe01", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]RenameMe02", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "CW!!D-[2-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]-[0-2][0-9][0-5][0-9]![0-9]!GS[_]RenameMe01", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "CW!!D-[2-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]-[0-2][0-9][0-5][0-9]![0-9]!GS[_]RenameMe02", expected: true);
						});
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
				}
			}
		}

		/// <summary>
		/// 1 column in a biadmin schema table
		/// 1 column starting with __$
		/// 1 column starting with CW!!
		/// 2 standard columns
		/// </summary>
		void AddTestColumns1(AdminConnection testConnection)
		{
			string createTestTablesSql = String.Format(CultureInfo.InvariantCulture, @"
				CREATE TABLE [{1}].GlbStaff           ([GS_RenameMe01] varchar(10), [GS_RenameMe02] varchar(10));",
				BiConstants.BiAdminSchemaName,
				Db.SqlDbOwnerSchema,
				AuditDatabaseSynchronisationWrapper.CdcSystemColumnPrefix,
				ColumnChangeRetriever.WtgPreservedColumnPrefix);

			using ((testConnection as ICurrentDbControl).UseDatabase(TestDbName))
			{
				testConnection.ExecuteNonQuery(createTestTablesSql);
			}
		}

		public void TestOldColumnsRenamedAndPreserved()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testConnection, TestDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(testConnection, TestDbName);

					using (((ICurrentDbControl)testConnection).UseDatabase(TestDbName))
					{
						var lsn = new byte[] { 0x1 };
						var tranEndTime = DateTime.UtcNow;
						AddBiAdminTables(testConnection, TestDbName);
						InsertLsnTimeMapping(testConnection, TestDbName, lsn, tranEndTime);
						AddTestColumns2(testConnection);

						CombineAssertions(() =>
						{
							// Assert columns before
							AssertColumnExists(testConnection, BiConstants.BiAdminSchemaName, "TableConfiguration", "ColumnDeleteMe01", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbCompany", "[_][_]$ColumnDeleteMe02", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "JobShipment", "CW!!ColumnAlreadyDeleted", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]ColumnPreserveMe01", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]Code", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]City", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "OrgHeader", "OH[_]ColumnPreserveMe02", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "OrgHeader", "OH[_]DeleteMeAsIDoNotAllowNull", expected: true);
						});

						new AuditDatabaseSynchronisationWrapperForTesting(TestDbName, testConnection).Run();

						CombineAssertions(() =>
						{
							// Assert columns afterwards
							AssertColumnExists(testConnection, BiConstants.BiAdminSchemaName, "TableConfiguration", "ColumnDeleteMe01", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbCompany", "[_][_]$ColumnDeleteMe02", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "JobShipment", "CW!!ColumnAlreadyDeleted", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]Code", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]City", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]ColumnPreserveMe01", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "CW!!D-[2-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]-[0-2][0-9][0-5][0-9]![0-9]!GS[_]ColumnPreserveMe01", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "OrgHeader", "OH[_]ColumnPreserveMe02", expected: false);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "OrgHeader", "CW!!D-[2-9][0-9][0-9][0-9][0-1][0-9][0-3][0-9]-[0-2][0-9][0-5][0-9]![0-9]!OH[_]ColumnPreserveMe02", expected: true);
							AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "OrgHeader", "OH[_]DeleteMeAsIDoNotAllowNull", expected: false);
						});

						// Assert preserved columns: count and timestamp format
						AssertHistoricalColumnTimeStampsAreValid(testConnection, expectedPreservedColumnCount: 4);

						CombineAssertions(() =>
						{
							// Assert schema mapping summary
							AssertSchemaMapping(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_ColumnPreserveMe01");
							AssertSchemaMapping(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_City");
							AssertSchemaMapping(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_Code");
							AssertSchemaMapping(testConnection, Db.SqlDbOwnerSchema, "OrgHeader", "OH_ColumnPreserveMe02");
						});
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
				}
			}
		}

		public void TestSchemaMappingSummaryHasNoDuplicates()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testConnection, TestDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(testConnection, TestDbName);

					using (((ICurrentDbControl)testConnection).UseDatabase(TestDbName))
					{
						var lsn = new byte[] { 0x1 };
						var tranEndTime = DateTime.UtcNow;
						AddBiAdminTables(testConnection, TestDbName);
						InsertLsnTimeMapping(testConnection, TestDbName, lsn, tranEndTime);
						AddTestColumns2(testConnection);

						// Assert columns before
						AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]Code", expected: true);

						new AuditDatabaseSynchronisationWrapperForTesting(TestDbName, testConnection).Run();

						AssertColumnExists(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS[_]Code", expected: true);

						// Alter column again to replicate duplicate mapping
						testConnection.ExecuteNonQuery("ALTER TABLE dbo.GlbStaff ALTER COLUMN GS_Code varchar(128)");

						new AuditDatabaseSynchronisationWrapperForTesting(TestDbName, testConnection).Run();

						// Assert schema mapping summary
						AssertSchemaMapping(testConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_Code");
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
				}
			}
		}

		void AddBiAdminTables(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				connection.ExecuteNonQuery("CREATE SCHEMA [biadmin]");

				var sqlText = @"
CREATE TABLE [biadmin].[SchemaMappingSummary]
(
	[MaxLsn] binary(10) NOT NULL,
	[MaxLsnTimeUTC] datetime NOT NULL,
	[EffectiveSchemaVersion] varchar(128) NOT NULL,
	[TableName] varchar(128) NOT NULL,
	[RenamedColumn] varchar(128) NOT NULL,
	[MappedColumn] varchar(128) NOT NULL
);

CREATE TABLE [biadmin].[LsnTimeMapping]
(
	StartLsn BINARY(10) NOT NULL,
	TranEndTimeUtc DATETIME NOT NULL
);";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void InsertLsnTimeMapping(DbConnection connection, string auditDbName, byte[] lsn, DateTime tranEndtime)
		{
			var sqlText = @"INSERT INTO[biadmin].[LsnTimeMapping] (StartLsn, TranEndTimeUtc) VALUES(@Lsn, @TranEndTime)";
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@Lsn", SqlDbType.Binary, lsn);
				cmd.AddParameter("@TranEndTime", SqlDbType.DateTime, tranEndtime);
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// 1 column in a biadmin schema table
		/// 1 column starting with __$
		/// 1 column starting with CW!!
		/// 2 standard columns
		/// </summary>
		void AddTestColumns2(AdminConnection testConnection)
		{
			string createTestTablesSql = String.Format(CultureInfo.InvariantCulture, @"
				CREATE TABLE [{0}].TableConfiguration ([ColumnDeleteMe01] CHAR(1));
				CREATE TABLE [{1}].GlbCompany         ([{2}ColumnDeleteMe02] CHAR(1));
				CREATE TABLE [{1}].JobShipment        ([{3}ColumnAlreadyDeleted] DATETIME);
				CREATE TABLE [{1}].GlbStaff           ([GS_ColumnPreserveMe01] CHAR(1) CHECK ([GS_ColumnPreserveMe01] <> ''), [GS_Code] varchar(128), [GS_City] varchar(128));
				CREATE TABLE [{1}].OrgHeader          ([OH_ColumnPreserveMe02] BIT DEFAULT 1, [OH_DeleteMeAsIDoNotAllowNull] INT NOT NULL);",
				BiConstants.BiAdminSchemaName,
				Db.SqlDbOwnerSchema,
				AuditDatabaseSynchronisationWrapper.CdcSystemColumnPrefix,
				ColumnChangeRetriever.WtgPreservedColumnPrefix);

			using ((testConnection as ICurrentDbControl).UseDatabase(TestDbName))
			{
				testConnection.ExecuteNonQuery(createTestTablesSql);
			}
		}

		void AssertColumnExists(DbConnection testConnection, string schemaName, string tableName, string columnLikePattern, bool expected)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT 1
					FROM [{0}].sys.schemas s
					INNER JOIN [{0}].sys.tables t ON t.schema_id = s.schema_id
					INNER JOIN [{0}].sys.columns c ON c.object_id = t.object_id
					WHERE s.name = '{1}'
					AND t.name = '{2}'
					AND c.name like '{3}'
				) SELECT 1 ELSE SELECT 0",
				TestDbName,
				schemaName,
				tableName,
				columnLikePattern);

			bool columnExist = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			AssertEquals(schemaName + "." + tableName + "." + columnLikePattern + " column exists", expected, columnExist);
		}

		void AssertHistoricalColumnTimeStampsAreValid(DbConnection testConnection, int expectedPreservedColumnCount)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture,
				"SELECT name FROM [{0}].sys.columns WHERE name like '{1}{2}%'",
				/*0*/TestDbName,
				/*1*/ColumnChangeRetriever.WtgPreservedColumnPrefix,
				/*2*/AuditDatabaseSynchronisationWrapper.RenamedColumnDeletedFlag);

			var historicalColumns = DataUtils.GetListOfValuesFromQuery(testConnection, sqlText).ToArray();

			AssertEquals("Historical column count", expectedPreservedColumnCount, historicalColumns.Length);

			CombineAssertions("Found historical columns with invalid time-stamps", () =>
			{
				DateTime dummyDateTimeForParsing;
				Array.ForEach<string>(
					historicalColumns,
					(c) => Assert(
						c,
						DateTime.TryParseExact(
							c.Substring(
								ColumnChangeRetriever.WtgPreservedColumnPrefix.Length + AuditDatabaseSynchronisationWrapper.RenamedColumnDeletedFlag.Length,
								AuditDatabaseSynchronisationWrapper.RenamedColumnTimeStampFormat.Length),
							AuditDatabaseSynchronisationWrapper.RenamedColumnTimeStampFormat,
							CultureInfo.InvariantCulture,
							DateTimeStyles.None,
							out dummyDateTimeForParsing))
				);
			});
		}

		void AssertSchemaMapping(AdminConnection testConnection, string schemaName, string tableName, string columnName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT 1
					FROM [{0}].[{1}].[SchemaMappingSummary]
					WHERE
						TableName = '{2}'
						AND MappedColumn = '{3}'
				) SELECT 1 ELSE SELECT 0",
				TestDbName,
				BiConstants.BiAdminSchemaName,
				$"{schemaName}.{tableName}",
				columnName);

			bool mappingExists = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			Assert(schemaName + "." + tableName + "." + columnName + " mapping should exist", mappingExists);
		}

		/// <summary>
		/// Audit columns which are renamed when removed from the latest must be nullable.
		/// Otherwise the Audit ETL to fail when inserting new rows with no values for the renamed columns.
		/// </summary>
		public void TestRenamableAuditColumnsAreNullable()
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				DECLARE @RenamableColumnsThatDoNotAllowNull nvarchar(max) = '';

				SELECT
					@RenamableColumnsThatDoNotAllowNull += '[' + s.name + '].[' + t.name + '].[' + c.name + ']' + char(10)
				FROM
					[{0}].sys.schemas s
					INNER JOIN [{0}].sys.tables t ON t.schema_id = s.schema_id
					INNER JOIN [{0}].sys.columns c ON t.object_id = c.object_id
				WHERE
					t.is_ms_shipped = 0
					AND s.name <> '{1}'
					AND c.name not like '{2}%'
					AND c.is_nullable = 0;

				SELECT @RenamableColumnsThatDoNotAllowNull;",
				/*0*/Db.AuditDatabaseName,
				/*1*/BiConstants.BiAdminSchemaName,
				/*2*/DataUtils.ReplaceSqlLikeWildcard(AuditDatabaseSynchronisationWrapper.CdcSystemColumnPrefix)
			);

			AssertEquals("Renamable audit columns which do not allow null values", "", Db.Connection.ExecuteScalar(sqlText).ToString());
		}

		const string TestDbName = "Enterprise.DbUpgrader.Schema.Launch.Testing.AuditDatabaseSynchronisationWrapperSpecificTest.Db";
	}
}
