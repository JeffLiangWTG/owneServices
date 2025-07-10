using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.Semaphores;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Semaphores.Testing
{
	class AcquireOrUpdateSqlMutexTest : TestCase
	{
		public void TestAcquireSqlMutex_Sucess()
		{
			var testLockInfo = new TestLock();
			var dateTimeUtcNow = DateTime.UtcNow;
			var (returnValue, expiresAtTimeUtc, currentTimeUtc, _) = AcquireSqlMutex(testLockInfo);
			AssertEquals("Acquire sql mutex succeeded", 0, returnValue);
			Assert("Mutex expires at utc later than now", expiresAtTimeUtc > dateTimeUtcNow);
			AssertNotEquals(DateTime.MinValue, currentTimeUtc);
		}

		public void TestAcquireSqlMutex_Deny_ToOtherProcesses()
		{
			var testLockInfo = new TestLock();
			var dateTimeUtcNow = DateTime.UtcNow;

			var (returnValue, expiresAtTimeUtc, _, _) = AcquireSqlMutex(testLockInfo);
			AssertEquals("Acquire sql mutex succeeded", 0, returnValue);
			Assert("Mutex expires at utc later than now", expiresAtTimeUtc > dateTimeUtcNow);

			var lockInfo2 = TestLock.DeepClone(testLockInfo);
			lockInfo2.ProcessId += 1;
			var (returnValue2, expiresAtTimeUtc2, _, _) = AcquireSqlMutex(lockInfo2);

			AssertNotEquals("Second attempt to another process should NOT return 0", 0, returnValue2);
			AssertEquals("SQL error number 2601 returned as duplicate exist", 2601, returnValue2);
			Assert("Second returned expiry time reset to year 1900", expiresAtTimeUtc2 < dateTimeUtcNow);
			AssertEquals("Expires time set to year 1900", new DateTime(1900, 1, 1), expiresAtTimeUtc2);

			var lockInfo3 = TestLock.DeepClone(testLockInfo);
			lockInfo3.WorkStationName = Guid.NewGuid().ToString("N");
			var (returnValue3, expiresAtTimeUtc3, _, _) = AcquireSqlMutex(lockInfo3);

			AssertNotEquals("Second attempt to another process should NOT return 0", 0, returnValue3);
			AssertEquals("SQL error number 2601 returned as duplicate exist", 2601, returnValue3);
			Assert("Second returned expiry time reset to year 1900", expiresAtTimeUtc3 < dateTimeUtcNow);
			AssertEquals("Expires time set to year 1900", new DateTime(1900, 1, 1), expiresAtTimeUtc3);
		}

		public void TestUpdateSqlMutex_AfterExpiryTime_LoseTheLockToOtherProcess()
		{
			var testLockInfo = new TestLock();
			var dateTimeUtcNow = DateTime.UtcNow;
			var (returnValue, expiresAtTimeUtc, _, _) = AcquireSqlMutex(testLockInfo);
			AssertEquals("Acquire sql mutex succeeded", 0, returnValue);
			Assert("Mutex expires at utc later than now", expiresAtTimeUtc > dateTimeUtcNow);

			Thread.Sleep(TimeSpan.FromSeconds(testLockInfo.TimeoutInSeconds / 2));
			var (returnValue2, expiresAtTimeUtc2, _, _) = AcquireSqlMutex(testLockInfo);
			AssertEquals("Acquire sql mutex succeeded", 0, returnValue2);
			Assert("Mutex expiry time updated to later", expiresAtTimeUtc2 > expiresAtTimeUtc);

			while (DateTime.UtcNow < expiresAtTimeUtc2)
			{
				Thread.Sleep(100);
			}

			var lockInfoProcess2 = TestLock.DeepClone(testLockInfo);
			lockInfoProcess2.ProcessId += 1;
			var (returnValue3, expiresAtTimeUtc3, _, _) = AcquireSqlMutex(lockInfoProcess2);
			var (returnValue4, expiresAtTimeUtc4, _, _) = AcquireSqlMutex(testLockInfo);
			AssertEquals("Acquire sql mutex succeeded", 0, returnValue3);
			Assert("Mutex expiry time updated to later", expiresAtTimeUtc3 > DateTime.UtcNow);

			AssertNotEquals("1st process has lost the lock since failed to update before expiry", 0, returnValue4);
			AssertEquals("SQL error number 2601 returned as duplicate exist", 2601, returnValue4);
			Assert("Second returned expiry time reset to year 1900", expiresAtTimeUtc4 < DateTime.UtcNow);
			AssertEquals("Expires time set to year 1900", new DateTime(1900, 1, 1), expiresAtTimeUtc4);
		}

		public void TestAcquireSqlMutex_ReturnMessageNotSupplied()
		{
			var testLockInfo = new TestLock();
			var dateTimeUtcNow = DateTime.UtcNow;
			var (returnValue, expiresAtTimeUtc, currentTimeUtc, _) = AcquireSqlMutex(testLockInfo, addReturnMessage: false);
			AssertEquals("Acquire sql mutex succeeded", 0, returnValue);
			Assert("Mutex expires at utc later than now", expiresAtTimeUtc > dateTimeUtcNow);
			AssertNotEquals(DateTime.MinValue, currentTimeUtc);
		}

		(int ReturnValue, DateTime ExpiresAtTimeUtc, DateTime CurrentTimeUtc, string ReturnMessage) AcquireSqlMutex(TestLock testLock, bool rollbackTransaction = false, bool addReturnMessage = true)
		{
			var returnValue = -1;
			var expiresAtTimeUtc = DateTime.MinValue;
			var currentTimeUtc = DateTime.MinValue;
			var returnMessage = string.Empty;

			using (DbCommand command = Db.Connection.Command(nameof(AcquireOrUpdateSqlMutex)))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@category", SqlDbType.VarChar, testLock.Category);
				command.AddParameter("@lockInfo", SqlDbType.VarChar, testLock.LockInfo);
				command.AddParameter("@timeoutInSeconds", SqlDbType.Int, testLock.TimeoutInSeconds);
				command.AddParameter("@workStationName", SqlDbType.VarChar, testLock.WorkStationName);
				command.AddParameter("@processId", SqlDbType.Int, testLock.ProcessId);
				command.AddParameter("@userCode", SqlDbType.VarChar, testLock.UserCode);
				command.AddOutputParameter("@currentTimeUtc", SqlDbType.DateTime, 0, 0, 0, null);
				command.AddOutputParameter("@mutexExpiresAtTimeUtc", SqlDbType.DateTime, 0, 0, 0, null);
				if (addReturnMessage)
				{
					command.AddOutputParameter("@returnMessage", SqlDbType.VarChar, 256, 0, 0, string.Empty);
				}
				command.AddReturnValueParameter();

				returnValue = command.ExecuteProcedureWithReturnValue();
				expiresAtTimeUtc = (DateTime)command.GetParameterValue("@mutexExpiresAtTimeUtc");
				currentTimeUtc = (DateTime)command.GetParameterValue("@currentTimeUtc");
				if (addReturnMessage)
				{
					returnMessage = (string)command.GetParameterValue("@returnMessage");
				}
			}

			return (returnValue, expiresAtTimeUtc, currentTimeUtc, returnMessage);
		}
	}

	[Serializable]
	class TestLock
	{
		public TestLock()
		{
			Category = "TST";
			LockInfo = Guid.NewGuid().ToString("N");
			TimeoutInSeconds = 2;
			WorkStationName = System.Environment.MachineName;
			ProcessId = Process.GetCurrentProcess().Id;
			UserCode = "xxx";
		}

		public static TestLock DeepClone(TestLock obj)
		{
			return new TestLock()
			{
				Category = obj.Category,
				LockInfo = obj.LockInfo,
				TimeoutInSeconds = obj.TimeoutInSeconds,
				WorkStationName = obj.WorkStationName,
				ProcessId = obj.ProcessId,
				UserCode = obj.UserCode
			};
		}

		public string Category { get; set; }
		public string LockInfo { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int TimeoutInSeconds { get; set; }
		public string WorkStationName { get; set; }
		public int ProcessId { get; set; }
		public string UserCode { get; set; }
	}
}

