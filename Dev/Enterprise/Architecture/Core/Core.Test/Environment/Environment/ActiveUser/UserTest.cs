using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Core.Environment.Testing
{
	sealed class UserTest : TransactionedTestCase
	{
		public void TestGetNumberOfActiveNonSystemUsers()
		{
			var personPk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($"insert dbo.GlbPerson (PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) values ('{personPk}', 'aaa', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			AssertEquals("Number of active users excluding System Users", 0, ActiveUserQuery.GetNumberOfActiveNonSystemUsers());
			Db.Connection.ExecuteNonQuery(
				$"UPDATE dbo.GlbStaff SET GS_IsSystemAccount = 0, GS_PER = '{personPk}', GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_PK = @pk",
				cmd => cmd.AddParameterBasedOnDbColumn("@pk", EnvProxy.Instance.CurrentUser.PK, GlbStaffSchema.PK)); // Enterprise.ZArchitecture.Core project hasn't access to Business Objects
			Assert("At least current user should be found", ActiveUserQuery.GetNumberOfActiveNonSystemUsers() > 0);
		}

		#region SharedTestCode

		public void GetUserSessionsForTest(string heartbeatType, string lockInfo, Func<bool, IActiveUserSession[]> activeUserSessionsQuery)
		{
			using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
			{
				var deleteSql = @"DELETE FROM dbo.StmServiceHeartBeat WHERE SV_HeartbeatType = @HeartbeatType";
				using (var command = Db.Connection.Command(deleteSql))
				{
					command.AddParameterBasedOnDbColumn("@HeartbeatType", heartbeatType,
						StmServiceHeartBeatSchema.SV_HeartbeatType);
					command.ExecuteNonQuery();
				}

				AssertEquals(0, activeUserSessionsQuery(true).Length);

				var heartbeatPk = Guid.NewGuid();
				var insertUserSql = @"
INSERT INTO dbo.StmServiceHeartBeat (SV_PK, SV_WorkstationName, SV_ProcessID, SV_HeartbeatType, SV_ParentTableCode, SV_ParentId, SV_ExpiresAtUtc)
VALUES ( @Pk, @WorkstationName, @ProcessID, @HeartbeatType, @ParentTableCode, @ParentId, DATEADD(MINUTE, 30, SYSUTCDATETIME()) );
INSERT INTO dbo.StmServiceSemaphore (SS_PK, SS_ServiceClass, SS_LockInfo, SS_UseCount, SS_SV)
VALUES ( NEWID(), @ServiceClass, @LockInfo, @UseCount, @Pk);
";

				using (var command = Db.Connection.Command(insertUserSql))
				{
					command.AddParameter("@Pk", SqlDbType.UniqueIdentifier, heartbeatPk);
					command.AddParameter("@WorkstationName", SqlDbType.VarChar, "Test");
					command.AddParameter("@ProcessID", SqlDbType.Int, 0);
					command.AddParameter("@HeartbeatType", SqlDbType.VarChar, $"{heartbeatType}");
					command.AddParameter("@ParentTableCode", SqlDbType.VarChar, "GS");
					command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, EnvProxy.Instance.CurrentUser.PK);
					command.AddParameter("@ServiceClass", SqlDbType.VarChar, "LGN");
					command.AddParameter("@LockInfo", SqlDbType.VarChar, $"{lockInfo}");
					command.AddParameter("@UseCount", SqlDbType.Int, 0);

					command.ExecuteNonQuery();
				}

				AssertEquals(1, activeUserSessionsQuery(true).Length);
			}
		}

		#endregion

		#region TestGetEnterpriseActiveUserSessions

		public void TestGetEnterpriseActiveUserSessions()
		{
			GetUserSessionsForTest("ENT", "EnterpriseActiveLogin", ActiveUserQuery.GetEnterpriseActiveUserSessions);
		}

		#endregion

		#region TestGetEnterpriseWinzorActiveUserSessions

		public void TestGetEnterpriseWinzorActiveUserSessions()
		{
			GetUserSessionsForTest("ENT", "EnterpriseWinzorActiveLogin", ActiveUserQuery.GetEnterpriseWinzorActiveUserSessions);
		}

		#endregion

		#region TestGetGlowActiveUserSessions

		public void TestGetGlowActiveUserSessions()
		{
			GetUserSessionsForTest("GLW", "GlowLogon", ActiveUserQuery.GetGlowActiveUserSessions);
		}

		#endregion
	}
}
