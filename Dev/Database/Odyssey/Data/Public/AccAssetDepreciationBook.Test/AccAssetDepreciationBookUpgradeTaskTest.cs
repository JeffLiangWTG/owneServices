using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class AccAssetDepreciationBookUpgradeTaskTest : TransactionedTestCase
	{
		void AssertDefaultBook()
		{
			AssertFromDB("DEF", "Default Asset Depreciation Book", true, true);
		}

		public void TestEmptyDatabase_DefaultBook()
		{
			DataHelpers.ClearTable(TableName);
			new AccAssetDepreciationBookUpgradeTask().Run();
			AssertDefaultBook();
		}

		public void TestDatabaseAlreadyHasBook()
		{
			AssertUpdate(true, "Default Asset Depreciation Book");
		}

		void AssertInsert(bool active, bool withCompany)
		{
			var code = withCompany ? "DEF" : "STD";
			var desc = "Test ADB Desc";

			DataHelpers.ClearTable(TableName);
			var company = withCompany ? new GlbCompany("Co1", "AU").InsertAndReturnObject(TestConnection) : null;
			var bookTest = InsertBookData(code, desc, false, active, company);

			new AccAssetDepreciationBookUpgradeTask().Run();

			if (withCompany)
			{
				AssertFromDB(code, desc, false, active, company, bookTest);
			}
			AssertDefaultBook();
		}

		#region TestInsert

		public void TestInsert_OtherNullCompanyBook_Active()
		{
			AssertInsert(true, false);
		}

		public void TestInsert_OtherNullCompanyBook_NotActive()
		{
			AssertInsert(false, false);
		}

		public void TestInsert_OtherCompanyBook_Active()
		{
			AssertInsert(true, true);
		}

		public void TestInsert_OtherCompanyBook_NotActive()
		{
			AssertInsert(false, true);
		}

		#endregion

		void AssertUpdate(bool active, string desc)
		{
			DataHelpers.ClearTable(TableName);
			InsertBookData("DEF", desc, true, active, null, DefaultGuid);
			new AccAssetDepreciationBookUpgradeTask().Run();
			AssertFromDB("DEF", desc, true, active);
		}

		#region TestUpdate

		public void TestUpdate_Active()
		{
			AssertUpdate(true, "Potato for dinner Everyone");
		}

		public void TestUpdate_NotActive()
		{
			AssertUpdate(false, "No changes");
		}

		#endregion

		#region Implementation

		void AssertFromDB(string code, string desc, bool system, bool active, GlbCompany company = null, Guid? pk = null)
		{
			var query = $"SELECT * FROM dbo.{TableName} WHERE ADB_Code = @Code";
			if (pk.HasValue)
			{
				query += " AND ADB_PK = @PK";
			}
			if (company == null)
			{
				query += " AND ADB_GC_Company IS NULL";
			}
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@Code", SqlDbType.VarChar, code);
				if (pk.HasValue)
				{
					command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk.Value);
				}
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals(desc, (string)reader["ADB_Description"]);
						AssertEquals(system, (bool)reader["ADB_IsSystem"]);
						AssertEquals(active, (bool)reader["ADB_IsActive"]);
						if (company != null)
						{
							AssertEquals(company.PK, (Guid)reader["ADB_GC_Company"]);
						}
					});
				}
			}
		}

		public static Guid InsertBookData(string code, string desc, bool system, bool active, GlbCompany company = null, Guid? pk = null)
		{
			var key = pk ?? Guid.NewGuid();
			var query = @$"INSERT dbo.{TableName} (ADB_PK, ADB_Code, ADB_Description, ADB_IsSystem, ADB_IsActive, {(company != null ? "ADB_GC_Company, " : "")}
ADB_SystemCreateTimeUtc, ADB_SystemCreateUser, ADB_SystemLastEditTimeUtc, ADB_SystemLastEditUser)
VALUES (@PK, @Code, @Desc, @IsSystem, @IsActive, {(company != null ? "@Company, " : "")}
@ADB_SystemCreateTimeUtc, @ADB_SystemCreateUser, @ADB_SystemLastEditTimeUtc, @ADB_SystemLastEditUser)";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, key);
				command.AddParameter("@Code", SqlDbType.VarChar, code);
				command.AddParameter("@Desc", SqlDbType.NVarChar, desc);
				command.AddParameter("@IsSystem", SqlDbType.Bit, system);
				command.AddParameter("@IsActive", SqlDbType.Bit, active);
				if (company != null)
				{
					command.AddParameter("@Company", SqlDbType.UniqueIdentifier, company.PK);
				}
				var now = DateTime.Now;
				command.AddParameter("@ADB_SystemCreateTimeUtc", SqlDbType.SmallDateTime, now);
				command.AddParameter("@ADB_SystemCreateUser", SqlDbType.VarChar, "~UK");
				command.AddParameter("@ADB_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, now);
				command.AddParameter("@ADB_SystemLastEditUser", SqlDbType.VarChar, "~UK");
				command.ExecuteNonQuery();
			}
			return key;
		}

		public const string TableName = "AccAssetDepreciationBook";

		static readonly Guid DefaultGuid = new("60F4517D-34D3-4F1B-AB53-ECC07D871AAA");

		#endregion
	}
}
