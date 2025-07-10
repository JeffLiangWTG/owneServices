using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlowDeleteEntitlementRule))]
	class GlowDeleteEntitlementRuleTest : DbCreateScriptTest
	{
		public void TestRuleWithTenures()
		{
			var rule1PK = Guid.NewGuid();
			var rule2PK = Guid.NewGuid();
			var rule3PK = Guid.NewGuid();

			PrepareTestData(rule1PK, rule2PK, rule3PK);

			RunStoredProcedure(rule1PK, 0);

			AssertEquals(0, EntitlementRuleCount(rule1PK));
			AssertEquals(0, EntitlementTenureCount(rule1PK));

			AssertEquals(1, EntitlementRuleCount(rule2PK));
			AssertEquals(1, EntitlementTenureCount(rule2PK));

			AssertEquals(1, EntitlementRuleCount(rule3PK));
		}

		public void TestRuleWithoutTenures()
		{
			var rule1PK = Guid.NewGuid();
			var rule2PK = Guid.NewGuid();
			var rule3PK = Guid.NewGuid();

			PrepareTestData(rule1PK, rule2PK, rule3PK);

			RunStoredProcedure(rule3PK, 0);

			AssertEquals(0, EntitlementRuleCount(rule3PK));

			AssertEquals(1, EntitlementRuleCount(rule1PK));
			AssertEquals(4, EntitlementTenureCount(rule1PK));

			AssertEquals(1, EntitlementRuleCount(rule2PK));
			AssertEquals(1, EntitlementTenureCount(rule2PK));
		}

		public void TestVersion()
		{
			var rulePK = Guid.NewGuid();
			var sql = $@"
INSERT INTO
	dbo.HrlEntitlement (LPE_PK, LPE_Name, LPE_SystemCreateTimeUtc, LPE_SystemCreateUser, LPE_SystemLastEditTimeUtc, LPE_SystemLastEditUser)
VALUES
	('{rulePK}', 'Rule', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
";

			TestConnection.ExecuteNonQuery(sql);
			AssertEquals(1, EntitlementRuleCount(rulePK));

			RunStoredProcedure(rulePK, 1);

			AssertEquals(1, EntitlementRuleCount(rulePK));
		}

		void PrepareTestData(Guid rule1PK, Guid rule2PK, Guid rule3PK)
		{
			var sql = $@"
INSERT INTO
	dbo.HrlEntitlement (LPE_PK, LPE_Name, LPE_SystemCreateTimeUtc, LPE_SystemCreateUser, LPE_SystemLastEditTimeUtc, LPE_SystemLastEditUser)
VALUES
	('{rule1PK}', 'Rule 1', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	('{rule2PK}', 'Rule 2', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	('{rule3PK}', 'Rule 3', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

INSERT INTO
	dbo.HrlEntitlementTenure (LPT_PK, LPT_LPE_Entitlement, LPT_TenureMonths, LPT_SystemCreateTimeUtc, LPT_SystemCreateUser, LPT_SystemLastEditTimeUtc, LPT_SystemLastEditUser)
VALUES
	(NEWID(), '{rule1PK}', 12, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(NEWID(), '{rule1PK}', 24, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(NEWID(), '{rule1PK}', 36, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(NEWID(), '{rule1PK}', 999, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
	(NEWID(), '{rule2PK}', 999, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
";

			TestConnection.ExecuteNonQuery(sql);

			AssertEquals(1, EntitlementRuleCount(rule1PK));
			AssertEquals(4, EntitlementTenureCount(rule1PK));

			AssertEquals(1, EntitlementRuleCount(rule2PK));
			AssertEquals(1, EntitlementTenureCount(rule2PK));

			AssertEquals(1, EntitlementRuleCount(rule3PK));
			AssertEquals(0, EntitlementTenureCount(rule3PK));
		}

		object EntitlementRuleCount(Guid pk) => TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.HrlEntitlement WHERE LPE_PK = '{pk}'");

		object EntitlementTenureCount(Guid pk) => TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.HrlEntitlementTenure WHERE LPT_LPE_Entitlement = '{pk}'");

		static void RunStoredProcedure(Guid pk, short version)
		{
			using (var command = Db.Connection.Command("GlowDeleteEntitlementRule"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@HrlEntitlementPK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Version", SqlDbType.SmallInt, version);
				command.ExecuteScalar();
			}
		}
	}
}
