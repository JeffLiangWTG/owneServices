using System;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema
{
	abstract class BusinessIntelligenceSynchronisationWrapperTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestSynchronisationWithEmptyDatabase()
		{
			using (var testAdminCnx = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testAdminCnx, TestDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(testAdminCnx, TestDbName);
					AssertHasSchemas(testAdminCnx, false);
					AssertHasObjects(testAdminCnx, false);

					var synchronisationWrapper = NewSynchronisationWrapper(testAdminCnx);
					synchronisationWrapper.Run();

					AssertHasSchemas(testAdminCnx, true);
					AssertHasObjects(testAdminCnx, true);
					AssertColumnNames(testAdminCnx);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testAdminCnx, TestDbName);
				}
			}
		}

		public void TestBiDatabaseHasMainDbSchemaVersion()
		{
			AssertEquals(
				String.Format(CultureInfo.InvariantCulture, "Database [{0}] has no {1} ext pty", ActualDbName, BiConstants.MainDbSchemaVersionExtPtyName),
				SchemaVersion.Application.ToString(),
				DataUtils.LoadDbExtendedProperty(Db.Connection, BiConstants.MainDbSchemaVersionExtPtyName, ActualDbName));
		}

		void AssertColumnNames(DbConnection testConnection)
		{
			AssertNoColumnNameLongerThan60Characters(testConnection);
			AssertNoColumnNameStartsWithCargoWisePreservedColumnPrefix(testConnection);
		}

		/// <summary>
		/// The system adds prefixes and suffixes to column names when required.
		/// This test ensures there is enough room to do so without exceeding the 128-char limit for column names.
		/// </summary>
		void AssertNoColumnNameLongerThan60Characters(DbConnection testConnection)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				DECLARE @ColumnNamesOver60CharLong nvarchar(max) = '';

				SELECT
					@ColumnNamesOver60CharLong += '[' + s.name + '].[' + t.name + '].[' + c.name + '] (' + convert(varchar(3), len(c.name)) + ')' + char(10)
				FROM
					[{0}].sys.schemas s
					INNER JOIN [{0}].sys.tables t ON t.schema_id = s.schema_id
					INNER JOIN [{0}].sys.columns c ON t.object_id = c.object_id
				WHERE
					t.is_ms_shipped = 0
					AND len(c.name) > 60;

				SELECT @ColumnNamesOver60CharLong;",
				TestDbName);

			AssertEquals("Columns with name longer 60 characters", "", testConnection.ExecuteScalar(sqlText).ToString());
		}

		/// <summary>
		/// Columns starting with this prefix are never droppped.
		/// This test ensures this prefix is not used on regular columns.
		/// </summary>
		void AssertNoColumnNameStartsWithCargoWisePreservedColumnPrefix(DbConnection testConnection)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				DECLARE @ColumnNamesStartingWithCwPreservedPrefix nvarchar(max) = '';

				SELECT
					@ColumnNamesStartingWithCwPreservedPrefix += '[' + s.name + '].[' + t.name + '].[' + c.name + ']' + char(10)
				FROM
					[{0}].sys.schemas s
					INNER JOIN [{0}].sys.tables t ON t.schema_id = s.schema_id
					INNER JOIN [{0}].sys.columns c ON t.object_id = c.object_id
				WHERE
					t.is_ms_shipped = 0
					AND c.name like '{1}%';

				SELECT @ColumnNamesStartingWithCwPreservedPrefix;",
				TestDbName,
				ColumnChangeRetriever.WtgPreservedColumnPrefix
			);

			AssertEquals("Column names starting with CW preserved prefix", "", testConnection.ExecuteScalar(sqlText).ToString());
		}

		protected void AssertSchemaExists(DbConnection testConnection, string schemaName, bool expected)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture,
				"IF EXISTS(SELECT null FROM [{0}].sys.schemas WHERE name = '{1}') SELECT 1 ELSE SELECT 0",
				TestDbName,
				schemaName);

			bool schemaExist = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			AssertEquals(schemaName + " schema exists?", expected, schemaExist);
		}

		protected void AssertTableExists(DbConnection testConnection, string schemaName, string tableName, bool expected)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT null
					FROM [{0}].sys.tables t
					INNER JOIN [{0}].sys.schemas s ON s.schema_id = t.schema_id
					WHERE s.name = '{1}' AND t.name = '{2}'
				) SELECT 1 ELSE SELECT 0",
				TestDbName,
				schemaName,
				tableName);

			bool tableExist = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			AssertEquals(schemaName + "." + tableName + " table exists?", expected, tableExist);
		}

		protected abstract BusinessIntelligenceSynchronisationWrapper NewSynchronisationWrapper(AdminConnection testAdminCnx);
		protected abstract void AssertHasSchemas(DbConnection testConnection, bool expected);
		protected abstract void AssertHasObjects(DbConnection testConnection, bool expected);
		protected abstract string TestDbName { get; }
		protected abstract string ActualDbName { get; }
	}
}
