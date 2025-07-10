using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Glow
{
	[TestedType(typeof(BulkSessionsUpdate))]
	sealed class BulkSessionsUpdateTest : DbCreateScriptTest
	{
		const string SemaphoreTypePendingUserAction = "ACT";
		const string SemaphoreTypeOther = "OTH";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int TestStaleDurationInSeconds = 600;

		public void TestActiveHeartBeatIsUpdated()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var currentExpiration = now.AddMinutes(5);

			InsertHeartBeat(heartbeatPk, currentExpiration);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertExpiration(heartbeatPk, newExpiration);
		}

		public void TestExpiredHeartBeatIsLeftUnchanged()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var expiredExpiration = now.AddMinutes(-5);

			InsertHeartBeat(heartbeatPk, expiredExpiration);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertExpiration(heartbeatPk, expiredExpiration);
		}

		public void TestStaleSemaphoreRelatedToSessionIsDeleted()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var currentExpiration = now.AddMinutes(5);

			InsertHeartBeat(heartbeatPk, currentExpiration);

			var semaphorePk = Guid.NewGuid();
			var additionalTimeBuffer = 20;
			var staleAcquiredUtc = DateTime.UtcNow.AddSeconds(-1 * TestStaleDurationInSeconds - additionalTimeBuffer);
			InsertSemaphore(semaphorePk, SemaphoreTypePendingUserAction, staleAcquiredUtc, heartbeatPk);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertSemaphoreExists(semaphorePk, false);
		}

		public void TestValidSemaphoreRelatedToSessionIsNotDeleted()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var currentExpiration = now.AddMinutes(5);

			InsertHeartBeat(heartbeatPk, currentExpiration);

			var semaphorePk = Guid.NewGuid();
			var ageSemaphore = 20;
			var staleAcquiredUtc = DateTime.UtcNow.AddSeconds(-1 * ageSemaphore);
			InsertSemaphore(semaphorePk, SemaphoreTypePendingUserAction, staleAcquiredUtc, heartbeatPk);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertSemaphoreExists(semaphorePk, true);
		}

		public void TestStaleNonPendingActionSemaphireRelatedToSessionIsNotDeleted()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var currentExpiration = now.AddMinutes(5);

			InsertHeartBeat(heartbeatPk, currentExpiration);

			var semaphorePk = Guid.NewGuid();
			var additionalTimeBuffer = 20;
			var staleAcquiredUtc = DateTime.UtcNow.AddSeconds(-1 * TestStaleDurationInSeconds - additionalTimeBuffer);
			InsertSemaphore(semaphorePk, SemaphoreTypeOther, staleAcquiredUtc, heartbeatPk);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertSemaphoreExists(semaphorePk, true);
		}

		public void TestStaleNonPendingActionSemaphireUnrelatedToSessionIsNotDeleted()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var currentExpiration = now.AddMinutes(5);

			InsertHeartBeat(heartbeatPk, currentExpiration);

			var unrelatedheartbeatPk = Guid.NewGuid();
			InsertHeartBeat(unrelatedheartbeatPk, currentExpiration);

			var semaphorePk = Guid.NewGuid();
			var additionalTimeBuffer = 20;
			var staleAcquiredUtc = DateTime.UtcNow.AddSeconds(-1 * TestStaleDurationInSeconds - additionalTimeBuffer);
			InsertSemaphore(semaphorePk, SemaphoreTypeOther, staleAcquiredUtc, unrelatedheartbeatPk);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertSemaphoreExists(semaphorePk, true);
		}

		public void TestStalePendingActionSemaphireUnrelatedToSessionIsNotDeleted()
		{
			var heartbeatPk = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var currentExpiration = now.AddMinutes(5);

			InsertHeartBeat(heartbeatPk, currentExpiration);

			var unrelatedheartbeatPk = Guid.NewGuid();
			InsertHeartBeat(unrelatedheartbeatPk, currentExpiration);

			var semaphorePk = Guid.NewGuid();
			var additionalTimeBuffer = 20;
			var staleAcquiredUtc = DateTime.UtcNow.AddSeconds(-1 * TestStaleDurationInSeconds - additionalTimeBuffer);
			InsertSemaphore(semaphorePk, SemaphoreTypePendingUserAction, staleAcquiredUtc, unrelatedheartbeatPk);

			var dt = GetNewUserSessionExpiryTable();
			var newExpiration = now.AddMinutes(10);
			dt.Rows.Add(heartbeatPk, newExpiration);
			BulkSessionsUpdate(dt);

			AssertSemaphoreExists(semaphorePk, true);
		}

		DataTable GetNewUserSessionExpiryTable()
		{
			var userSessionExpiry = new DataTable();
			userSessionExpiry.Locale = CultureInfo.InvariantCulture;
			// SuppressStringValidation SQL literal required for TVP
			userSessionExpiry.Columns.Add("HeartbeatPK", typeof(Guid));
			userSessionExpiry.Columns.Add("Expiry", typeof(DateTime));

			return userSessionExpiry;
		}

		void InsertHeartBeat(Guid heartbeatPk, DateTime expiration)
		{
			using (var command = LocalConnection.Command(@"
			INSERT INTO [dbo].[StmServiceHeartBeat] ([SV_PK] ,[SV_HeartbeatType] ,[SV_ExpiresAtUtc] ,[SV_WorkstationName] ,[SV_ProcessID] ,[SV_ParentId] ,[SV_ParentTableCode] ,[SV_CreateTimeUtc])
			VALUES ( @PK ,'GLW' ,@ExpiresAtUtc ,'WorkstationName' ,7 ,NEWID() ,'GS' ,GETUTCDATE())"))
			{
				command.CommandTimeout = 10;
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, heartbeatPk);
				command.AddParameter("@ExpiresAtUtc", SqlDbType.DateTime, expiration);
				command.ExecuteNonQuery();
			}
		}

		void AssertExpiration(Guid heartbeatPk, DateTime expiration)
		{
			using (var command = LocalConnection.Command(@"SELECT SV_ExpiresAtUtc FROM [dbo].[StmServiceHeartBeat] WHERE SV_PK = @PK"))
			{
				command.CommandTimeout = 10;
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, heartbeatPk);
				var setExpiration = (DateTime)command.ExecuteScalar();
				AssertDateTimeWithinOneSecond("Checking that the saved expiration matches expected expiration", expiration, setExpiration);
			}
		}

		void BulkSessionsUpdate(DataTable dt, int staleDuration = TestStaleDurationInSeconds)
		{
			using (var command = LocalConnection.Command("BulkSessionsUpdate"))
			{
				command.CommandTimeout = 10;
				command.CommandType = CommandType.StoredProcedure;
				command.AddTableValuedParameter("@TVP", "dbo.TVP_UserSessionExpiry", dt);
				command.AddParameter("@durationInSecondsToBeStale", SqlDbType.Int, staleDuration);
				command.ExecuteNonQuery();
			}
		}

		void InsertSemaphore(Guid pk, string serviceClass, DateTime acquiredTimeUtc, Guid heartbeatPk)
		{
			using (var command = LocalConnection.Command(@"
			INSERT INTO [dbo].[StmServiceSemaphore] ([SS_PK] ,[SS_ServiceClass] ,[SS_LockInfo] ,[SS_UseCount] ,[SS_AcquiredTimeUtc] ,[SS_SV])
			VALUES (@PK ,@ServiceClass ,'LockInfo' ,0 ,@AcquiredTimeUtc ,@heartbeatPk)"))
			{
				command.CommandTimeout = 10;
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ServiceClass", SqlDbType.Char, serviceClass);
				command.AddParameter("@AcquiredTimeUtc", SqlDbType.DateTime, acquiredTimeUtc);
				command.AddParameter("@heartbeatPk", SqlDbType.UniqueIdentifier, heartbeatPk);
				command.ExecuteNonQuery();
			}
		}

		void AssertSemaphoreExists(Guid pk, bool exists)
		{
			using (var command = LocalConnection.Command(@"SELECT count(*) FROM [dbo].[StmServiceSemaphore] WHERE SS_PK = @PK"))
			{
				command.CommandTimeout = 10;
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				var count = (int)command.ExecuteScalar();
				AssertEquals(count > 0, exists);
			}
		}

		DbConnection LocalConnection => Db.Connection;
	}
}
