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
	[TestedType(typeof(TryPeekAccessToken))]
	sealed class TryPeekAccessTokenTest : DbCreateScriptTest
	{
		public void TestCanPeekTimeLimitedToken()
		{
			for (var i = 0; i < 5; i++)
			{
				AssertCanPeek("Future", "ZZZ");
			}
		}

		public void TestCannotPeekExpiredTimeLimitedToken()
		{
			AssertCannotPeek("Past", "XXX");
		}

		public void TestCannotPeekAlreadyConsumedUseLimitedToken_NegativeUsesRemaining()
		{
			AssertCannotPeek("UsedUp", "CCC");
		}

		public void TestCannotPeekAlreadyConsumedUseLimitedToken_ZeroUsesRemaining()
		{
			AssertCannotPeek("Empty", "CCC");
		}

		public void TestCanPeekUseLimitedToken()
		{
			for (var i = 0; i < 5; i++)
			{
				AssertCanPeek("HasUses", "XXX");
			}
		}

		public void TestCanPeekPermanentToken()
		{
			for (var i = 0; i < 5; i++)
			{
				AssertCanPeek("Permanent", "AAA");
			}
		}

		public void TestCanPeekTimeOrUseWhicheverComesFirstToken()
		{
			for (var i = 0; i < 5; i++)
			{
				AssertCanPeek("ComboValid", "XXX");
			}
		}

		public void TestCanPeekValidChildTokenWithValidParent()
		{
			AssertCanPeek("ValidChildWithValidParent", ":-)");
		}

		public void TestCannotPeekTimeOrUseWhicheverComesFirstTokenWhenUsesAreConsumed()
		{
			AssertCannotPeek("ComboInvalidUse", "CCC");
		}

		public void TestCannotPeekTimeOrUseWhicheverComesFirstTokenWhenTimeHasElapsed()
		{
			AssertCannotPeek("ComboInvalidTime", "CCC");
		}

		public void TestCannotPeekTokenOfADifferentType()
		{
			AssertCannotPeek("Permanent", ";_;");
		}

		public void TestCannotPeekValidChildTokenWithInvalidParent()
		{
			AssertCannotPeek("ValidChildWithInvalidParent", ":-x");
		}

		public void TestCannotPeekInvalidChildTokenWithValidParent()
		{
			AssertCannotPeek("InvalidChildWithValidParent", ":-)");
		}

		public void TestCannotPeekInvalidChildTokenWithInvalidParent()
		{
			AssertCannotPeek("InvalidChildWithInvalidParent", ":-x");
		}

		public void TestCannotPeekMaxLengthTokenWithGarbageAppended()
		{
			AssertCanPeek("123456789012345678901234567890", "AAA");
			AssertCannotPeek("123456789012345678901234567890ABCDEF", "AAA");
		}

		static void AssertCannotPeek(string token, string type)
		{
			using (var command = Db.Connection.Command("TryPeekAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TPATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();

				var result = (bool)command.GetParameterValue("@TPATResult");
				Assert(nameof(result), !result);
				AssertEquals(DBNull.Value, command.GetParameterValue("@Scope"));
				AssertEquals(DBNull.Value, command.GetParameterValue("@ParentId"));
				AssertEquals(DBNull.Value, command.GetParameterValue("@ParentTableCode"));
			}
		}

		static void AssertCanPeek(string token, string type)
		{
			using (var command = Db.Connection.Command("TryPeekAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TPATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@TPATResult");

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

