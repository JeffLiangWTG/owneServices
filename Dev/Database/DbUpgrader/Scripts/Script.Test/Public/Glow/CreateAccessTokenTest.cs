using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Test
{
	[TestedType(typeof(CreateAccessToken))]
	sealed class CreateAccessTokenTest : DbCreateScriptTest
	{
		public void TestCreatesAccessToken()
		{
			// SMALLDATETIME does not store seconds, it's only accurate to the nearest minute.
			// Since the test can run on the second, the minute stored and the current minute could be out by one.
			// Thus, we allow for a variation of up to two minutes for the table audit fields.
			var smallDateTimeVariation = (int)TimeSpan.FromMinutes(2).TotalSeconds;

			var parentId = new Guid("7F0BF49E-B1E3-46D0-A730-9F74AE71968F");
			var expiresAt = new DateTime(2016, 06, 07, 12, 23, 34, DateTimeKind.Utc);
			var result = Execute("MyToken", "XXX", "SomeScope", parentId, "ABC", false, expiresAt, 10, @"\o/");
			Assert(result);

			var table = new DataTable();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}]", StmAccessTokenSchema.Constants.TableName)))
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			AssertEquals(1, table.Rows.Count);

			var row = table.Rows[0];
			AssertEquals("MyToken", row[StmAccessTokenSchema.Constants.SAT_Token]);
			AssertEquals("XXX", row[StmAccessTokenSchema.Constants.SAT_Type]);
			AssertEquals("SomeScope", row[StmAccessTokenSchema.Constants.SAT_Scope]);
			AssertEquals(parentId, row[StmAccessTokenSchema.Constants.SAT_ParentId]);
			AssertEquals("ABC", row[StmAccessTokenSchema.Constants.SAT_ParentTableCode]);
			AssertEquals(false, row[StmAccessTokenSchema.Constants.SAT_IsPermanentToken]);
			AssertEquals(expiresAt, row[StmAccessTokenSchema.Constants.SAT_ExpiresAt]);
			AssertEquals(10, row[StmAccessTokenSchema.Constants.SAT_RemainingUseCount]);
			NUnit.Framework.Assert.That((DateTime)row[StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc], Is.EqualTo(DateTime.UtcNow).Within(smallDateTimeVariation).Seconds);
			AssertEquals(@"\o/", row[StmAccessTokenSchema.Constants.SAT_SystemCreateUser]);
			NUnit.Framework.Assert.That((DateTime)row[StmAccessTokenSchema.Constants.SAT_SystemLastEditTimeUtc], Is.EqualTo(DateTime.UtcNow).Within(smallDateTimeVariation).Seconds);
			AssertEquals(@"\o/", row[StmAccessTokenSchema.Constants.SAT_SystemLastEditUser]);
		}

		public void TestReturnsErrorCodeWhenTokenIsDuplicate()
		{
			var result = Execute("MyToken", "AAA", "SomeScope", Guid.Empty, "ABC", false, null, 10, @"\o/");
			Assert(result);

			result = Execute("MyToken", "BBB", "SomeScope", Guid.Empty, "ABC", false, null, 10, @"\o/");
			Assert(!result);
		}

		public void TestThrowsSqlExceptionForOtherErrors()
		{
			AssertExceptionThrown<SqlException>(() =>
			{
				Execute("MyToken", string.Empty, "SomeScope", Guid.Empty, "ABC", false, null, 10, @"\o/");
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

		static bool Execute(string token, string type, string scope, Guid parentId, string parentTableCode, bool isPermanent, DateTime? expiresAtUtc, int useCount, string createUser)
		{
			using (var command = Db.Connection.Command("CreateAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddParameter("@Scope", SqlDbType.VarChar, scope);
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@IsPermanent", SqlDbType.Bit, isPermanent);
				command.AddParameter("@ExpiresAtUtc", SqlDbType.DateTime, (object)expiresAtUtc ?? DBNull.Value);
				command.AddParameter("@UseCount", SqlDbType.Int, useCount);
				command.AddParameter("@CreateUser", SqlDbType.VarChar, createUser);
				command.AddOutputParameter("@CATResult", SqlDbType.Bit, 0, 0, 0, DBNull.Value);

				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@CATResult");
				return result;
			}
		}
	}
}

