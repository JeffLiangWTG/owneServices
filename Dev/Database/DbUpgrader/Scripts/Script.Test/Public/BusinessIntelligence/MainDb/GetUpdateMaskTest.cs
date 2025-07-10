using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.MainDb.Testing
{
	[TestedType(typeof(GetUpdateMask))]
	class GetUpdateMaskTransactionedTest : DbCreateScriptTest
	{
		public void TestHashValue()
		{
			var expectedHash = "76C73521602E2CED61BCC42A3E8042B6392AFD9016B75584671CFEA233F443CE";

			var script = ScriptToTest.Text.Trim();
			var actualHash = GetHashString(script);

			var message = "Script has been modified. Please update $/Shared/CargoWise.BusinessIntelligence/src/CargoWise.Bi.Shared.Testing/AuditHelperTest.cs to reflect the changes and match the hash string.";

			AssertEquals(message, expectedHash, actualHash);
		}
	}

	class GetUpdateMaskTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetUpdateMask()
		{
			using (var conn = Db.NewAdminConnection())
			{
				EnableCdc(conn);

				var updateMask = GetUpdateMask(conn, "dbo", "GlbStaff", new[] { "" });
				AssertEquals("PK columns selected", 0, updateMask.UpdateMaskCombined());

				var pkUpdateMask = GetUpdateMask(conn, "dbo", "GlbStaff", new[] { "GS_PK" });
				AssertGreaterThan("PK columns selected", pkUpdateMask.UpdateMaskCombined(), 0);

				var multiColumnUpdateMask = GetUpdateMask(conn, "dbo", "GlbStaff", new[] { "GS_PK", "GS_Code" });
				AssertGreaterThan("PK columns selected", multiColumnUpdateMask.UpdateMaskCombined(), pkUpdateMask.UpdateMaskCombined());
			}
		}

		void EnableCdc(DbConnection connection)
		{
			if (IsDatabaseCdcEnabled(connection))
			{
				DisableCdcOnDatabase(connection);
			}
			EnableCdcOnDatabase(connection);
			EnableCdcOnTable(connection, Db.SqlDbOwnerSchema, "GlbStaff");
		}

		bool IsDatabaseCdcEnabled(DbConnection connection)
		{
			var sqlText = $"SELECT is_cdc_enabled FROM sys.databases WHERE name = '{Db.DatabaseName}'";
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText));
		}

		void DisableCdcOnDatabase(DbConnection connection)
		{
			using (var cmd = connection.Command("sys.sp_cdc_disable_db"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}
		}

		void EnableCdcOnDatabase(DbConnection connection)
		{
			using (var cmd = connection.Command("sys.sp_cdc_enable_db"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}
		}

		void EnableCdcOnTable(DbConnection connection, string schema, string table)
		{
			using (var cmd = connection.Command("sys.sp_cdc_enable_table"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@source_schema", SqlDbType.NVarChar, schema);
				cmd.AddParameter("@source_name", SqlDbType.NVarChar, table);
				cmd.AddParameter("@capture_instance", SqlDbType.NVarChar, $"{schema}_{table}");
				cmd.AddParameter("@role_name", SqlDbType.NVarChar, DBNull.Value);

				cmd.ExecuteNonQuery();
			}
		}

		UpdateMask GetUpdateMask(DbConnection conn, string schemaName, string tableName, IEnumerable<string> columnList)
		{
			using (var cmd = conn.Command("SELECT UpdateMask1, UpdateMask2, UpdateMask3, UpdateMask4 FROM dbo.GetUpdateMask (@schema, @table, @search_column_list)"))
			{
				cmd.AddParameter("@schema", SqlDbType.NVarChar, schemaName);
				cmd.AddParameter("@table", SqlDbType.NVarChar, tableName);
				cmd.AddTableValuedParameter("@search_column_list", "dbo.TVP_varchar_250", columnList);

				using (var reader = cmd.ExecuteReader())
				{
					long updateMask1 = 0;
					long updateMask2 = 0;
					long updateMask3 = 0;
					long updateMask4 = 0;

					if (reader.Read())
					{
						updateMask1 = Convert.ToInt64(reader["UpdateMask1"]);
						updateMask2 = Convert.ToInt64(reader["UpdateMask2"]);
						updateMask3 = Convert.ToInt64(reader["UpdateMask3"]);
						updateMask4 = Convert.ToInt64(reader["UpdateMask4"]);
					}

					return new UpdateMask(updateMask1, updateMask2, updateMask3, updateMask4);
				}
			}
		}

		class UpdateMask
		{
			public UpdateMask(long updateMask1, long updateMask2, long updateMask3, long updateMask4)
			{
				UpdateMask1 = updateMask1;
				UpdateMask2 = updateMask2;
				UpdateMask3 = updateMask3;
				UpdateMask4 = updateMask4;
			}

			public long UpdateMaskCombined()
			{
				return UpdateMask1 + UpdateMask2 + UpdateMask3 + UpdateMask4;
			}

			public long UpdateMask1 { get; }
			public long UpdateMask2 { get; }
			public long UpdateMask3 { get; }
			public long UpdateMask4 { get; }
		}
	}
}

