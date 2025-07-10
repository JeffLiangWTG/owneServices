using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_DatabaseSetInfo))]
	class ep_DatabaseSetInfoTest : DbCreateScriptTest
	{
		public void TestDatabaseSetInfo()
		{
			var expectedDbList = Db.Connection.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository);

			var dbSetInfoTable = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC " + ScriptToTest.Name);
			var enumerableDbSetInfo = dbSetInfoTable.AsEnumerable();

			AssertEquals("Non CW1 database retrieved?", false, enumerableDbSetInfo.Where(r => r["DbName"].ToString() == nonRelevantDb).Any());
			AssertEquals("System database retrieved?", false, enumerableDbSetInfo.Where(r => r["DbName"].ToString() == Db.SqlMasterDb).Any());

			string missingDbs = string.Join("\r\n\t", expectedDbList.Where(db => !enumerableDbSetInfo.Where(r => r["DbName"].ToString() == db).Any()));
			Assert(string.Format("The following expected databases were not returnd by {0}\r\n\t{1}", ScriptToTest.Name, missingDbs), missingDbs.Length == 0);

			string unexpectedDbs = string.Join("\r\n\t", enumerableDbSetInfo.Select(r => r["DbName"].ToString()).Where(db => !expectedDbList.Contains(db)));
			Assert(string.Format("The following unexpected databases were returnd by {0}\r\n\t{1}", ScriptToTest.Name, unexpectedDbs), unexpectedDbs.Length == 0);

			foreach (var dbInfoRow in enumerableDbSetInfo)
			{
				string dbName = dbInfoRow["DbName"].ToString();
				Guid dbGuid = new Guid(dbInfoRow["DbGuid"].ToString());
				int dbUsedSizeMb = Convert.ToInt32(dbInfoRow["UsedSizeMb"], CultureInfo.InvariantCulture);
				int diskSizeMb = Convert.ToInt32(dbInfoRow["DiskSizeMb"], CultureInfo.InvariantCulture);
				AssertDatabaseInfo(dbName, dbGuid, dbUsedSizeMb, diskSizeMb);
			}
		}

		void AssertDatabaseInfo(string dbName, Guid dbGuid, int dbUsedSizeMb, int diskSizeMb)
		{
			AssertEquals("DB broker guid", GetDatabaseBrokerGuid(dbName), dbGuid);
			AssertEquals("DB used size", GetDatabaseUsedSize(dbName), dbUsedSizeMb);
			AssertEquals("DB allocated size", GetDatabaseAllocatedSize(dbName), diskSizeMb);
		}

		int GetDatabaseUsedSize(string dbName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT convert(int, ceiling(sum(used_pages)/128.)) FROM [{0}].sys.allocation_units", dbName);
			int usedSize = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			return usedSize;
		}

		int GetDatabaseAllocatedSize(string dbName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT convert(int, ceiling(sum(size)/128.)) FROM [{0}].sys.database_files WHERE [type] = 0", dbName);
			int allocatedSize = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			return allocatedSize;
		}

		Guid GetDatabaseBrokerGuid(string dbName)
		{
			using (var cmd = TestConnection.Command("SELECT service_broker_guid FROM sys.databases WHERE name = @DbName"))
			{
				cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, dbName);
				return new Guid(cmd.ExecuteScalar().ToString());
			}
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var testAdminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(testAdminConnection, nonRelevantDb);
			}
		}

		protected override void FinalTearDown()
		{
			using (var testAdminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(testAdminConnection, nonRelevantDb);
			}

			base.FinalTearDown();
		}

		readonly string nonRelevantDb = Db.DatabaseName + "_DatabaseSetInfoTestNonCw1Db";
	}
}

