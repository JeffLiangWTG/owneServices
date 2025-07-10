using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Test
{
	[TestedType(typeof(TryConsumeAccessToken))]
	sealed class TryConsumeAccessTokenTest : DbCreateScriptTest
	{
		public void TestCanConsumeTimeLimitedToken()
		{
			for (var i = 0; i < 5; i++)
			{
				AssertConsumes("Future", "ZZZ");
			}
		}

		public void TestCannotConsumeExpiredTimeLimitedToken()
		{
			AssertCannotConsume("Past", "XXX");
		}

		public void TestCannotConsumeAlreadyConsumedUseLimitedToken_NegativeUsesRemaining()
		{
			AssertCannotConsume("UsedUp", "CCC");
		}

		public void TestCannotConsumeAlreadyConsumedUseLimitedToken_ZeroUsesRemaining()
		{
			AssertCannotConsume("Empty", "CCC");
		}

		public void TestCanConsumeUseLimitedToken()
		{
			for (var i = 0; i < 3; i++)
			{
				AssertConsumes("HasUses", "XXX");
			}

			// No uses left.

			for (var i = 0; i < 5; i++)
			{
				AssertCannotConsume("HasUses", "XXX");
			}
		}

		public void TestCanConsumePermanentToken()
		{
			for (var i = 0; i < 5; i++)
			{
				AssertConsumes("Permanent", "AAA");
			}
		}

		public void TestCanConsumeTimeOrUseWhicheverComesFirstToken()
		{
			for (var i = 0; i < 3; i++)
			{
				AssertConsumes("ComboValid", "XXX");
			}

			// No uses left.

			for (var i = 0; i < 5; i++)
			{
				AssertCannotConsume("ComboValid", "XXX");
			}
		}

		public void TestCanConsumeValidChildTokenWithValidParent()
		{
			AssertConsumes("ValidChildWithValidParent", ":-)");
		}

		public void TestCannotConsumeTimeOrUseWhicheverComesFirstTokenWhenUsesAreConsumed()
		{
			AssertCannotConsume("ComboInvalidUse", "CCC");
		}

		public void TestCannotConsumeTimeOrUseWhicheverComesFirstTokenWhenTimeHasElapsed()
		{
			AssertCannotConsume("ComboInvalidTime", "CCC");
		}

		public void TestCannotConsumeTokenOfADifferentType()
		{
			AssertCannotConsume("Permanent", ";_;");
		}

		public void TestCannotConsumeValidChildTokenWithInvalidParent()
		{
			AssertCannotConsume("ValidChildWithInvalidParent", ":-x");
		}

		public void TestCannotConsumeInvalidChildTokenWithValidParent()
		{
			AssertCannotConsume("InvalidChildWithValidParent", ":-)");
		}

		public void TestCannotConsumeInvalidChildTokenWithInvalidParent()
		{
			AssertCannotConsume("InvalidChildWithInvalidParent", ":-x");
		}

		public void TestCannotConsumeMaxLengthTokenWithGarbageAppended()
		{
			AssertConsumes("123456789012345678901234567890", "AAA");
			AssertCannotConsume("123456789012345678901234567890ABCDEF", "AAA");
		}

		static void AssertCannotConsume(string token, string type)
		{
			using (var command = Db.Connection.Command("TryConsumeAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TCATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();

				var result = (bool)command.GetParameterValue("@TCATResult");
				Assert(nameof(result), !result);
				AssertEquals(DBNull.Value, command.GetParameterValue("@Scope"));
				AssertEquals(DBNull.Value, command.GetParameterValue("@ParentId"));
				AssertEquals(DBNull.Value, command.GetParameterValue("@ParentTableCode"));
			}
		}

		static void AssertConsumes(string token, string type)
		{
			using (var command = Db.Connection.Command("TryConsumeAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TCATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@TCATResult");

				Assert(nameof(result), result);
				AssertEquals("ScopetyData", command.GetParameterValue("@Scope"));
				AssertEquals(Guid.Empty, command.GetParameterValue("@ParentId"));
				AssertEquals("GS", command.GetParameterValue("@ParentTableCode"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ClearTable(StmAccessTokenSchema.Constants.TableName);

			StmAccessTokenHelper.CreateStmAccessToken("Future", "ZZZ", DateTime.UtcNow.AddMonths(1), -1);
			StmAccessTokenHelper.CreateStmAccessToken("Past", "XXX", DateTime.UtcNow.AddMonths(-1), -1);
			StmAccessTokenHelper.CreateStmAccessToken("UsedUp", "CCC", null, -1);
			StmAccessTokenHelper.CreateStmAccessToken("HasUses", "XXX", null, 3);
			StmAccessTokenHelper.CreateStmAccessToken("Empty", "CCC", null, 0);
			StmAccessTokenHelper.CreateStmAccessToken("Permanent", "AAA", DateTime.UtcNow.AddMonths(1), -1, isPermanent: true);
			StmAccessTokenHelper.CreateStmAccessToken("ComboValid", "XXX", DateTime.UtcNow.AddMonths(1), 3);
			StmAccessTokenHelper.CreateStmAccessToken("ComboInvalidUse", "CCC", DateTime.UtcNow.AddDays(1), 0);
			StmAccessTokenHelper.CreateStmAccessToken("ComboInvalidTime", "CCC", DateTime.UtcNow.AddDays(-1), 1);
			StmAccessTokenHelper.CreateStmAccessToken("123456789012345678901234567890", "AAA", DateTime.UtcNow.AddMonths(1), -1);

			var validParentPK = StmAccessTokenHelper.CreateStmAccessToken("ValidParent", ":-D", DateTime.UtcNow.AddDays(1), -1, scope: "ParentScopetyData");
			StmAccessTokenHelper.CreateStmAccessToken("ValidChildWithValidParent", ":-)", null, 1, "ScopetyData", validParentPK, StmAccessTokenSchema.Constants.Prefix);
			StmAccessTokenHelper.CreateStmAccessToken("InvalidChildWithValidParent", ":-)", DateTime.UtcNow.AddDays(-1), -1, "ScopetyData", validParentPK, StmAccessTokenSchema.Constants.Prefix);

			var invalidParentPK = StmAccessTokenHelper.CreateStmAccessToken("InvalidParent", ":-D", DateTime.UtcNow.AddDays(-1), -1, scope: "ParentScopetyData");
			StmAccessTokenHelper.CreateStmAccessToken("ValidChildWithInvalidParent", ":-x", null, 1, "ScopetyData", invalidParentPK, StmAccessTokenSchema.Constants.Prefix);
			StmAccessTokenHelper.CreateStmAccessToken("InvalidChildWithInvalidParent", ":-x", DateTime.UtcNow.AddDays(-1), -1, "ScopetyData", invalidParentPK, StmAccessTokenSchema.Constants.Prefix);
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}]", tableName);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}

