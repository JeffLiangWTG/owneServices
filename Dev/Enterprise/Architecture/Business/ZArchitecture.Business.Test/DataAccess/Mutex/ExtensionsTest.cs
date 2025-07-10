using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Data.Mutex.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestGetMutexLockByInfo()
		{
			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment))
			{
				AssertEquals("mutex Locked OK?", true, mutex.Lock());
				Assert("Mutex lock by", mutex.GetMutexLockByInfo().StartsWith("CargoWise Support ("));
			}
		}

		public void TestGetUserWithLock()
		{
			LockInfo info1 = null;
			var nullInfo = info1.GetUserWithLock();
			AssertEquals("Unknown", nullInfo);

			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DUMMY"))
			{
				AssertEquals(true, mutex.Lock());
				var mutexLockInfo = mutex.GetLockInfo();
				var user = mutexLockInfo.GetUserWithLock();
				AssertEquals("CargoWise Support", user);
			}

			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DUMMY"))
			{
				AssertEquals(true, mutex.Lock());

				var lockInfo = MutexIDs.DataProcessing.Name;
				var emptyGuid = Guid.Empty;

				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
							SET SV_ParentId = '{emptyGuid}',
								SV_SystemLastEditTimeUtc = GetUtcDate(),
								SV_SystemLastEditUser = 'USR'
							FROM dbo.StmServiceSemaphore
							INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
							WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.ExecuteNonQuery(sql);

				var mutexLockInfo = mutex.GetLockInfo();
				var nullUser = mutexLockInfo.GetUserWithLock();
				AssertEquals("Unknown", nullUser);
			}
		}

		public void TestGetMutexLockByInfo_NullUser()
		{
			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DUMMY"))
			using (ZGlobalMutex mutex2 = new ZGlobalMutex(MutexIDs.DataProcessing, "DUMMY"))
			{
				AssertEquals(true, mutex2.Lock());

				var lockInfo = MutexIDs.DataProcessing.Name;
				var emptyGuid = Guid.Empty;

				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
							SET SV_ParentId = '{emptyGuid}',
								SV_SystemLastEditTimeUtc = GetUtcDate(),
								SV_SystemLastEditUser = 'USR'
							FROM dbo.StmServiceSemaphore
							INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
							WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.ExecuteNonQuery(sql);

				AssertEquals(false, mutex.Lock());
				AssertNoExceptionThrown(() => mutex.GetMutexLockByInfo());
			}
		}
	}
}
