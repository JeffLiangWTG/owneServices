using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Data.Mutex.Testing
{
	sealed class ZGlobalMutexTest : TestCase
	{
		public void TestConstructorWithRecordIdentifier()
		{
			ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1234");
			AssertEquals("Mutex ID", MutexIDs.NewDummyAttachedToShipment, mutex.MutexID);
			AssertEquals("Record ID", "1234", mutex.RecordIdentifier);
		}

		public void TestConstructorWithoutRecordIdentifier()
		{
			ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment);
			AssertEquals("Mutex ID", MutexIDs.NewDummyAttachedToShipment, mutex.MutexID);
			Assert("Record ID", mutex.RecordIdentifier.IsEmpty);
		}

		public void TestLockIsExclusive()
		{
			using (ZGlobalMutex mutex1 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment))
			using (ZGlobalMutex mutex2 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment))
			{
				AssertEquals("Mutex1 Locked OK?", true, mutex1.Lock());
				Assert("Should not be able to lock Mutex2 as Mutex1 has the lock", !mutex2.Lock());

				AssertEquals("Mutex1 has lock?", true, mutex1.HasLock);
				AssertEquals("Mutex2 has lock?", false, mutex2.HasLock);
			}
		}

		public void TestLockIsExclusiveForRecordID()
		{
			using (ZGlobalMutex mutex1 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1"))
			using (ZGlobalMutex mutex2 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1"))
			using (ZGlobalMutex mutex3 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "2"))
			{
				Assert("Locked OK", mutex1.Lock());
				Assert("Can't lock as Mutex1 has lock", !mutex2.Lock());
				Assert("Can lock as is on another record id", mutex3.Lock());

				AssertEquals("Mutex1 is locked?", true, mutex1.IsLocked);
				AssertEquals("Mutex2 is locked?", true, mutex2.IsLocked);
				AssertEquals("Mutex3 is locked?", true, mutex3.IsLocked);

				mutex1.Unlock();

				AssertEquals("Mutex1 is locked?", false, mutex1.IsLocked);
				AssertEquals("Mutex2 is locked?", false, mutex2.IsLocked);
				AssertEquals("Mutex3 is locked?", true, mutex3.IsLocked);

				Assert("Mutex2 Should be able to lock as mutex1 has released it", mutex2.Lock());
			}
		}

		[ExpectException(typeof(MutexNotLockedException))]
		public void TestUnlockThrowsExceptionIfNoLock()
		{
			ZGlobalMutex mutex1 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1");
			mutex1.Unlock();
		}

		public void TestGetLockInfo()
		{
			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1"))
			{
				AssertNull("No locks yet", mutex.GetLockInfo());

				Assert("Locked OK", mutex.Lock());

				LockInfo mutex1Info = mutex.GetLockInfo();

				ZDateTime utcNowish = EnvProxy.Instance.Time.CurrentUtcDateTime.AddMinutes(1);
				AssertJustAfter("Lock start time", utcNowish, mutex1Info.LockStartTime);
				AssertEquals("MutexID", MutexIDs.NewDummyAttachedToShipment, mutex1Info.MutexID);
				AssertEquals("RecordID", "1", mutex1Info.RecordID);
				AssertEquals("User with lock", StaticCurrentFetcher.Instance.CurrentUser.PK, mutex1Info.UserWithLock.PK);
			}
		}

		[UseSnapshotProtection]
		public void TestReleaseLocks()
		{
			using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1"))
			{
				AssertNull("No locks yet", mutex.GetLockInfo());
				Assert("Locked OK", mutex.Lock());

				LockInfo mutex1Info = mutex.GetLockInfo();
				using (ZGlobalMutex mutex2 = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1"))
				{
					Assert("Locked NOT OK", !mutex2.Lock());

					mutex2.ReleaseLocks(mutex1Info.UserWithLock.PK.ToGuid());
					Assert("Locked OK", mutex2.Lock());
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionDoesNotCauseDeadlock()
		{
			var connection1 = Db.Connection;

			using (var connection2 = Db.NewExtraConnectionToMainDb())
			using (var mutex = new ZGlobalMutex(MutexIDs.NewDummyAttachedToShipment, "1"))
			{
				connection1.Command("CREATE TABLE TestTransaction (One varchar(3))").ExecuteNonQuery();

				try
				{
					connection1.Command("INSERT INTO TestTransaction VALUES('ABC')").ExecuteNonQuery();
					using (var delayedTransactionManager = connection1.DelayedTransactionWithManager(new Moq.Mock<ITransactionLockManager>().Object))
					{
						// This lock is taken before the underlying transaction is started, but it needs to be on it's own connection or else
						// the transaction it creates will be left open, leading to a deadlock when we try an release.
						Assert("Locked OK", mutex.Lock());

						using (var internalTransactionManager = connection1.BeginTransactionWithManager())
						{
							connection1.Command("INSERT INTO TestTransaction VALUES('123')").ExecuteNonQuery();
							connection1.Command("INSERT INTO TestTransaction VALUES('DEF')").ExecuteNonQuery();
							internalTransactionManager.CommitTransaction();
						}

						int result = Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar());
						AssertEquals("Only 1 row actually commited until outerscope", 1, result);

						mutex.Unlock();
					}

					AssertEquals("Transaction should have been rolled back", 1, Convert.ToInt32(connection2.Command("SELECT COUNT(*) FROM TestTransaction").ExecuteScalar()));
				}
				finally
				{
					connection1.Command("IF (OBJECT_ID('tempdb..TestTransaction') IS NOT NULL) DROP TABLE TestTransaction").ExecuteNonQuery();
					DbCommitTracker.Ignore("TestTransaction");
					connection1.RollbackTransaction();
				}
			}
		}

		void AssertJustAfter(string dateUsageMessage, ZDateTime correctDate, ZDateTime shouldBeJustAfterCorrectDate)
		{
			long difference = shouldBeJustAfterCorrectDate.Ticks - correctDate.Ticks;
			const string DateFormat = "dd-MMM-yyyy HH:mm:ss:fff";

			Assert(dateUsageMessage + ": " + shouldBeJustAfterCorrectDate.ToString(DateFormat) +
				" is is not within 100 milliseconds of " + correctDate.ToString(DateFormat), difference < 1000000);
		}
	}
}
