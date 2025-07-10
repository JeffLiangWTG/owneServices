using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.Semaphores;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Semaphores.Testing
{
	[TestedType(typeof(SemaphoreKeepSessionAlive))]
	class SemaphoreKeepSessionAliveTest : DbCreateScriptTest
	{
		public void TestKeepSessionAlive()
		{
			Guid heartbeatPk = Guid.NewGuid();

			ExecuteAndAssert(heartbeatPk, pulseInSeconds: 30, assertionPrefix: "[No test hearetbeat session]", expectedRefreshed: false);
			InsertHeartbeat(heartbeatPk, 60);
			ExecuteAndAssert(heartbeatPk, pulseInSeconds: 30, assertionPrefix: "[New session]", expectedRefreshed: true);
		}

		public void TestExpiredSession()
		{
			Guid heartbeatPk = Guid.NewGuid();
			InsertHeartbeat(heartbeatPk, -60);
			ExecuteAndAssert(heartbeatPk, pulseInSeconds: 30, assertionPrefix: "[Session logged off]", expectedRefreshed: false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void ExecuteAndAssert(Guid heartbeatPk, int pulseInSeconds, string assertionPrefix, bool expectedRefreshed)
		{
			DateTime baseUtc = (DateTime)TestConnection.ExecuteScalar("SELECT GETUTCDATE()");
			DateTime previousSessionExpiryDateTime = GetSessionExpiryDateTime(heartbeatPk);

			DateTime executionUtc;
			bool sessionRefreshed = ExecuteSemaphoreKeepSessionAlive(heartbeatPk, pulseInSeconds, out executionUtc);

			AssertEquals(assertionPrefix + " => Session Refreshed?", expectedRefreshed, sessionRefreshed);
			AssertEquals(assertionPrefix + " => Execution UTC [" + SqlFormatInfo.ToSqlDateTimeString(executionUtc) + "] >= Base UTC [" + SqlFormatInfo.ToSqlDateTimeString(baseUtc) + "]?", true, executionUtc >= baseUtc);
			AssertEquals(assertionPrefix + " => Session expiry datetime", (expectedRefreshed ? executionUtc.AddSeconds(pulseInSeconds) : previousSessionExpiryDateTime), GetSessionExpiryDateTime(heartbeatPk));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		bool ExecuteSemaphoreKeepSessionAlive(Guid heartbeatPk, int pulseInSeconds, out DateTime executionUtc)
		{
			bool sessionRefreshed = false;

			using (var command = TestConnection.Command(ScriptToTest.Name))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@isNewConnection", SqlDbType.Bit, false);
				command.AddParameter("@heartbeatPk", SqlDbType.UniqueIdentifier, heartbeatPk);
				command.AddParameter("@pulseInSeconds", SqlDbType.Int, pulseInSeconds);
				command.AddOutputParameter("@currentUTC", SqlDbType.DateTime, 0, 0, 0, DBNull.Value);

				int rowsAffected = command.ExecuteProcedureWithReturnValue();
				sessionRefreshed = (rowsAffected > 0);
				executionUtc = (DateTime)command.GetParameterValue("@currentUTC");
			}

			return sessionRefreshed;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void InsertHeartbeat(Guid heartbeatPk, int secondsOffset)
		{
			string sqlText = string.Format(@"
				INSERT dbo.StmServiceHeartBeat (SV_PK, SV_ParentTableCode, SV_ParentId, SV_ExpiresAtUtc)
				VALUES('{0}', 'GS', NEWID(), DateAdd(second, " + secondsOffset.ToString() + ", getutcdate()))",
				heartbeatPk);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		DateTime GetSessionExpiryDateTime(Guid heartbeatPk)
		{
			string sqlText = string.Format(@"
				SELECT SV_ExpiresAtUtc
				FROM dbo.StmServiceHeartBeat
				WHERE SV_PK = '{0}'",
				heartbeatPk);
			object objectResult = TestConnection.ExecuteScalar(sqlText);
			return (objectResult == null || objectResult == DBNull.Value) ? DateTime.MinValue : (DateTime)TestConnection.ExecuteScalar(sqlText);
		}
	}
}

