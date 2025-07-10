using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Glow
{
	[TestedType(typeof(CreateAccessTokenWhenRequired))]
	sealed class CreateAccessTokenWhenRequiredTest : DbCreateScriptTest
	{
		public void TestGeneratesAccessToken_WhenNoTokenExists_CreatesNewToken()
		{
			// SMALLDATETIME does not store seconds, it's only accurate to the nearest minute.
			// Since the test can run on the second, the minute stored and the current minute could be out by one.
			// Thus, we allow for a variation of up to two minutes for the table audit fields.
			var smallDateTimeVariation = (int)TimeSpan.FromMinutes(2).TotalSeconds;
			// There is a possibility for a datetime mismatch between DateTime.UtcNow and SYSUTCDATETIME()
			var dateTimeVariation = (int)TimeSpan.FromSeconds(30).TotalSeconds;

			var parentId = new Guid("7F0BF49E-B1E3-46D0-A730-9F74AE71968F");
			var overlap = TimeSpan.FromMinutes(10);
			var validity = TimeSpan.FromMinutes(30);
			var result = Execute("ADB", "SomeScope", parentId, "ABC", false, overlap, validity, -1, @"\o/");

			var table = new DataTable();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}] ORDER BY {1} DESC", StmAccessTokenSchema.Constants.TableName, StmAccessTokenSchema.Constants.SAT_ExpiresAt)))
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			CombineAssertions(() =>
			{
				AssertEquals("result", true, result);
				AssertEquals("Count", 1, table.Rows.Count);
				var row = table.Rows[0];
				AssertTokenCreated(row, parentId, validity, dateTimeVariation, smallDateTimeVariation);
			});
		}

		public void TestGeneratesAccessToken_WhenTokenAlreadyExpired_CreatesNewToken()
		{
			// SMALLDATETIME does not store seconds, it's only accurate to the nearest minute.
			// Since the test can run on the second, the minute stored and the current minute could be out by one.
			// Thus, we allow for a variation of up to two minutes for the table audit fields.
			var smallDateTimeVariation = (int)TimeSpan.FromMinutes(2).TotalSeconds;
			// There is a possibility for a datetime mismatch between DateTime.UtcNow and SYSUTCDATETIME()
			var dateTimeVariation = (int)TimeSpan.FromSeconds(30).TotalSeconds;

			var parentId = new Guid("7F0BF49E-B1E3-46D0-A730-9F74AE71968F");
			var overlap = TimeSpan.FromMinutes(10);
			var validity = TimeSpan.FromMinutes(30);
			StmAccessTokenHelper.CreateStmAccessToken("AboutToExpire", "ADB", DateTime.UtcNow.AddMinutes(-5), -1, "", Guid.NewGuid(), "ABC", false);
			var result = Execute("ADB", "SomeScope", parentId, "ABC", false, overlap, validity, -1, @"\o/");

			var table = new DataTable();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}] ORDER BY {1} DESC", StmAccessTokenSchema.Constants.TableName, StmAccessTokenSchema.Constants.SAT_ExpiresAt)))
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			CombineAssertions(() =>
			{
				AssertEquals("result", true, result);
				AssertEquals("Count", 2, table.Rows.Count);
				var row = table.Rows[0];
				AssertTokenCreated(row, parentId, validity, dateTimeVariation, smallDateTimeVariation);
			});
		}

		public void TestGeneratesAccessToken_WhenAboutToExpire_CreatesNewToken()
		{
			// SMALLDATETIME does not store seconds, it's only accurate to the nearest minute.
			// Since the test can run on the second, the minute stored and the current minute could be out by one.
			// Thus, we allow for a variation of up to two minutes for the table audit fields.
			var smallDateTimeVariation = (int)TimeSpan.FromMinutes(2).TotalSeconds;
			// There is a possibility for a datetime mismatch between DateTime.UtcNow and SYSUTCDATETIME()
			var dateTimeVariation = (int)TimeSpan.FromSeconds(30).TotalSeconds;

			var parentId = new Guid("7F0BF49E-B1E3-46D0-A730-9F74AE71968F");
			var overlap = TimeSpan.FromMinutes(10);
			var validity = TimeSpan.FromMinutes(30);
			StmAccessTokenHelper.CreateStmAccessToken("AboutToExpire", "ADB", DateTime.UtcNow.AddMinutes(5), -1, "", Guid.NewGuid(), "ABC", false);
			var result = Execute("ADB", "SomeScope", parentId, "ABC", false, overlap, validity, -1, @"\o/");

			var table = new DataTable();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}] ORDER BY {1} DESC", StmAccessTokenSchema.Constants.TableName, StmAccessTokenSchema.Constants.SAT_ExpiresAt)))
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			CombineAssertions(() =>
			{
				AssertEquals("result", true, result);
				AssertEquals("Count", 2, table.Rows.Count);
				var row = table.Rows[0];
				AssertTokenCreated(row, parentId, validity, dateTimeVariation, smallDateTimeVariation);
			});
		}

		public void TestGeneratesAccessToken_WhenTokenStillValid_DoesNotCreateNewToken()
		{
			var parentId = new Guid("7F0BF49E-B1E3-46D0-A730-9F74AE71968F");
			var overlap = TimeSpan.FromMinutes(10);
			var validity = TimeSpan.FromMinutes(30);
			StmAccessTokenHelper.CreateStmAccessToken("NotAboutToExpire", "ADB", DateTime.UtcNow.AddMinutes(15), -1, "SomeScope", Guid.NewGuid(), "ABC", false);
			var result = Execute("ADB", "SomeScope", parentId, "ABC", false, overlap, validity, -1, @"\o/");

			var table = new DataTable();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}] ORDER BY {1} DESC", StmAccessTokenSchema.Constants.TableName, StmAccessTokenSchema.Constants.SAT_ExpiresAt)))
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			CombineAssertions(() =>
			{
				AssertEquals("result", false, result);
				AssertEquals("Count", 1, table.Rows.Count);
				var row = table.Rows[0];
				AssertEquals("SAT_Token", "NotAboutToExpire", row[StmAccessTokenSchema.Constants.SAT_Token]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			ClearTable(StmAccessTokenSchema.Constants.TableName);
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}]", tableName);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		static bool Execute(string type, string scope, Guid parentId, string parentTableCode, bool isPermanent, TimeSpan overlap, TimeSpan validity, int useCount, string createUser)
		{
			using (var command = Db.Connection.Command("CreateAccessTokenWhenRequired"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddParameter("@Scope", SqlDbType.VarChar, scope);
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@IsPermanent", SqlDbType.Bit, isPermanent);
				command.AddParameter("@OverlapInSeconds", SqlDbType.Int, (int)overlap.TotalSeconds);
				command.AddParameter("@ValidityInSeconds", SqlDbType.Int, (int)validity.TotalSeconds);
				command.AddParameter("@UseCount", SqlDbType.Int, useCount);
				command.AddParameter("@CreateUser", SqlDbType.VarChar, createUser);
				command.AddOutputParameter("@CATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();

				return (bool)command.GetParameterValue("@CATResult");
			}
		}

		void AssertTokenCreated(DataRow row, Guid parentId, TimeSpan validity, int dateTimeVariation, int smallDateTimeVariation)
		{
			AssertEquals("Token length", 30, ((string)row[StmAccessTokenSchema.Constants.SAT_Token]).Length);
			AssertEquals("Type", "ADB", row[StmAccessTokenSchema.Constants.SAT_Type]);
			AssertEquals("Scope", "SomeScope", row[StmAccessTokenSchema.Constants.SAT_Scope]);
			AssertEquals("Parent ID", parentId, row[StmAccessTokenSchema.Constants.SAT_ParentId]);
			AssertEquals("Parent Table Code", "ABC", row[StmAccessTokenSchema.Constants.SAT_ParentTableCode]);
			AssertEquals("Is Permanent Token", false, row[StmAccessTokenSchema.Constants.SAT_IsPermanentToken]);
			AssertCloseEnough("Expires At", DateTime.UtcNow + validity, (DateTime)row[StmAccessTokenSchema.Constants.SAT_ExpiresAt], dateTimeVariation);
			AssertEquals("Remaining Use Count", -1, row[StmAccessTokenSchema.Constants.SAT_RemainingUseCount]);
			AssertCloseEnough("System Create Time UTC", DateTime.UtcNow, (DateTime)row[StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc], smallDateTimeVariation);
			AssertEquals("System Create User", @"\o/", row[StmAccessTokenSchema.Constants.SAT_SystemCreateUser]);
			AssertCloseEnough("System Last Edit Time UTC", DateTime.UtcNow, (DateTime)row[StmAccessTokenSchema.Constants.SAT_SystemLastEditTimeUtc], smallDateTimeVariation);
			AssertEquals("System Last Edit User", @"\o/", row[StmAccessTokenSchema.Constants.SAT_SystemLastEditUser]);
		}
	}
}

