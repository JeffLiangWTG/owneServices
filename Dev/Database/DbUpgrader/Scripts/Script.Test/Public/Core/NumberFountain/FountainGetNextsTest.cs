using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainGetNexts))]
	class FountainGetNextsTest : NumberFountainTestCase
	{
		public void TestSampleCall()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 8, maxValue: 10);
			Helper.InsertNumbers(TestConnection, fountainId, 1, 2, 3, 4, 5, 6, 7);

			AssertNumbers("Next numbers", new long[] { 1 }, FountainGetNexts(TestConnection, name, owner, amount: 1));
			AssertNumbers("Next numbers", new long[] { 2, 3, 4 }, FountainGetNexts(TestConnection, name, owner, amount: 3));
			AssertEquals("Available Numbers", 3, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
		}

		public void TestJustGeneration()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = 0;
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (connection.BeginTransactionWithManager())
				{
					AssertNumbers("Next numbers"
						, new long[] { 1 }
						, FountainGetNexts(connection, name, owner, amount: 1, minValue: 1, maxValue: 9_999, canRollover: false, callInSameTransaction: false));
					fountainId = Helper.GetFountainId(connection, name, owner).Value;
					AssertEquals("Available Numbers", 99, Helper.GetAvailableNumbersCount(connection, fountainId));

					AssertNumbers("Next numbers"
						, Enumerable.Range(2, 10).Select(x => (long)x).ToArray()
						, FountainGetNexts(connection, name, owner, amount: 10, callInSameTransaction: false));
					AssertEquals("Available Numbers", 89, Helper.GetAvailableNumbersCount(connection, fountainId));

					AssertNumbers("Next numbers"
						, Enumerable.Range(12, 500).Select(x => (long)x).ToArray()
						, FountainGetNexts(connection, name, owner, amount: 500, callInSameTransaction: false));
					AssertEquals("Available Numbers", 89, Helper.GetAvailableNumbersCount(connection, fountainId));
				}

				using (connection.BeginTransactionWithManager())
				{
					fountainId = Helper.GetFountainId(connection, name, owner).Value;
					Assert("Fountain should exists", fountainId > 0);
					AssertEquals("Available Numbers", 600, Helper.GetAvailableNumbersCount(connection, fountainId));

					AssertNumbers("Next numbers"
						, new long[] { 1 }
						, FountainGetNexts(connection, name, owner, amount: 1));
					AssertEquals("Available Numbers", 599, Helper.GetAvailableNumbersCount(connection, fountainId));

					AssertNumbers("Next numbers"
						, Enumerable.Range(2, 500).Select(x => (long)x).ToArray()
						, FountainGetNexts(connection, name, owner, amount: 500));
					AssertEquals("Available Numbers", 99, Helper.GetAvailableNumbersCount(connection, fountainId));
				}
			}
		}

		public void TestWrongCreation()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: -1, minValue: 1, maxValue: 199);
			});
			AssertContains("Amount of required numbers should be greater than 0.", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: 0, minValue: 1, maxValue: 199, false);
			});
			AssertContains("Amount of required numbers should be greater than 0.", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: -10, maxValue: 199);
			});
			AssertContains("Minimum fountain value should be greater than 0.", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 0, maxValue: 199);
			});
			AssertContains("Minimum fountain value should be greater than 0.", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 100, maxValue: 99);
			});
			AssertContains("Maximum fountain value should be greater than Minimum fountain value.", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 100, maxValue: -1);
			});
			AssertContains("Maximum fountain value should be greater than Minimum fountain value.", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			AssertNumbers("Create fountain", new long[] { 100 }, FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 100, maxValue: 100));
		}

		public void TestNoRolloverGeneration()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			AssertNumbers("Create fountain"
				, new long[] { 1 }
				, FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 1, maxValue: 199, canRollover: false, callInSameTransaction: true));
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;
			AssertEquals("Fountain should exist on DB", true, fountainId > 0);
			AssertEquals("Available Numbers", 99, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 101, Helper.GetValues(TestConnection, fountainId).NextValue);

			AssertNumbers("Next numbers"
				, Enumerable.Range(2, 90).Select(x => (long)x).ToArray()
				, FountainGetNexts(TestConnection, name, owner, amount: 90));
			AssertEquals("Available Numbers", 9, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 101, Helper.GetValues(TestConnection, fountainId).NextValue);

			AssertNumbers("Next numbers"
				, Enumerable.Range(92, 50).Select(x => (long)x).ToArray()
				, FountainGetNexts(TestConnection, name, owner, amount: 50));
			AssertEquals("Available Numbers", 58, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 200, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestNoRolloverOverflow()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			AssertNumbers("Create fountain"
				, Enumerable.Range(1, 199).Select(x => (long)x).ToArray()
				, FountainGetNexts(TestConnection, name, owner, amount: 199, minValue: 1, maxValue: 199, canRollover: false));
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;
			AssertEquals("Fountain should exist on DB", true, fountainId > 0);
			AssertEquals("Available Numbers", 0, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 200, Helper.GetValues(TestConnection, fountainId).NextValue);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainGetNexts(TestConnection, name, owner, amount: 1);
			});
			AssertContains("Fountain has reached its maximum value of 199. Cannot generate any more numbers.", ex.ToString(), ignoreCase: true);
		}

		public void TestNoRolloverCreationOverflow()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = 0;
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (connection.BeginTransactionWithManager())
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainGetNexts(connection, name, owner, amount: 200, minValue: 1, maxValue: 199, canRollover: false, callInSameTransaction: false);
					});
					AssertContains("Fountain has reached its maximum value of 199. Cannot generate any more numbers.", ex.ToString(), ignoreCase: true);
				}

				fountainId = Helper.GetFountainId(connection, name, owner).Value;
				AssertEquals("Fountain should exist on DB", true, fountainId > 0);
				AssertEquals("Available Numbers", 199, Helper.GetAvailableNumbersCount(connection, fountainId));

				using (connection.BeginTransactionWithManager())
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainGetNexts(connection, name, owner, amount: 200);
					});
					AssertContains("Fountain has reached its maximum value of 199. Cannot generate any more numbers.", ex.ToString(), ignoreCase: true);
				}

				AssertEquals("Available Numbers", 199, Helper.GetAvailableNumbersCount(connection, fountainId));
			}

			AssertEquals("Available Numbers", 199, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertNumbers("Next numbers"
				, Enumerable.Range(1, 199).Select(x => (long)x).ToArray()
				, FountainGetNexts(TestConnection, name, owner, amount: 199));
			AssertEquals("Available Numbers", 0, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 200, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestLongFountainNameIsSupported()
		{
			var name = LongFountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainGetNexts(TestConnection, name, owner, 10, 34, 9999, false);
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;
			AssertEquals("Fountain name", name, Helper.GetFountainName(TestConnection, fountainId));
		}

		public void TestCreateHeaderBlocked_Direct()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			using (var blockingConnection = Db.NewExtraConnectionToMainDb())
			using (blockingConnection.BeginTransactionWithManager())
			{
				blockingConnection.ExecuteNonQuery("DELETE TOP (0) dbo.StmNums WITH (TABLOCKX)");

				using (var blockedConnection = Db.NewExtraConnectionToMainDb())
				using (blockedConnection.BeginTransactionWithManager())
				{
					blockedConnection.DefaultCommandTimeOutInSeconds = 3;

					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainGetNexts(blockedConnection, name, owner, amount: 10, minValue: 34, maxValue: 100, callInSameTransaction: true);
					});
					AssertContains("Execution Timeout Expired.", ex.ToString(), ignoreCase: true);
				}
			}
		}

		public void TestCreateHeaderBlocked_Loopback()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			using (var blockingConnection = Db.NewExtraConnectionToMainDb())
			using (blockingConnection.BeginTransactionWithManager())
			{
				blockingConnection.ExecuteNonQuery("DELETE TOP (0) dbo.StmNums WITH (TABLOCKX)");

				var ex = AssertExceptionThrown<SqlException>(() =>
				{
					FountainGetNexts(TestConnection, name, owner, amount: 10, minValue: 34, maxValue: 100, callInSameTransaction: false);
				});
				AssertContains($"Fountain with name [{name}] and owner [{owner}] is not found and cannot be created", ex.ToString(), ignoreCase: true);
			}
		}

		public void TestFirstRequestorGetsHisNumbersFirst()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 1, maxValue: 1000, callInSameTransaction: false);

			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;

			Task requestor1;
			Task requestor2;

			long[] resultNumbers1 = null;
			long[] resultNumbers2 = null;

			using (var blocker = Db.NewExtraConnectionToMainDb())
			using (blocker.BeginTransactionWithManager())
			using (var reset1 = new AutoResetEvent(false))
			using (var reset2 = new AutoResetEvent(false))
			{
				Helper.GetAppLock(blocker, fountainId);

				requestor1 = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection = Db.NewExtraConnectionToMainDb())
					using (var manager = connection.BeginTransactionWithManager())
					{
						reset1.Set();
						resultNumbers1 = FountainGetNexts(connection, name, owner, amount: 100, callInSameTransaction: false);
						manager.CommitTransaction();
					}
				});

				reset1.WaitOne();

				requestor2 = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection = Db.NewExtraConnectionToMainDb())
					using (var manager = connection.BeginTransactionWithManager())
					{
						reset2.Set();
						resultNumbers2 = FountainGetNexts(connection, name, owner, amount: 2, callInSameTransaction: false);
						manager.CommitTransaction();
					}
				});

				reset2.WaitOne();
			}

			Task.WaitAll(requestor1, requestor2);

			CombineAssertions(() =>
			{
				AssertNumbers("Next numbers 1", Enumerable.Range(2, 100).Select(x => (long)x).ToArray(), resultNumbers1);
				AssertNumbers("Next numbers 2", Enumerable.Range(102, 2).Select(x => (long)x).ToArray(), resultNumbers2);
			});
		}

		public void TestException_FountainLockNotAcquired()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 100, maxValue: 100);
			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;

			using (var blocker = Db.NewExtraConnectionToMainDb())
			using (blocker.BeginTransactionWithManager())
			{
				Helper.GetAppLock(blocker, fountainId);

				using (TestConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainGetNexts(TestConnection, name, owner, amount: 1, minValue: 100, maxValue: 100);
					});
					AssertContains($"[=FOUNTAIN_LOCK_NOT_ACQUIRED=]-1 (Reason: The lock request timed out (0 ms), ID: {fountainId}, Name: \"{name}\", Owner: {owner})", ex.ToString(), ignoreCase: true);
				}
			}
		}

		#region Implementation

		long[] FountainGetNexts(DbConnection connection, string name, Guid owner, int amount, long minValue = 1, long maxValue = 1, bool canRollover = false, bool callInSameTransaction = true)
		{
			var numbers = new List<long>();

			connection.ExecuteReader(ScriptToTest.Name
				, (cmd) =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Name", SqlDbType.VarChar, name);
					cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
					cmd.AddParameter("@Amount", SqlDbType.Int, amount);
					cmd.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
					cmd.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);
					cmd.AddParameter("@CanRollover", SqlDbType.Bit, canRollover);
					cmd.AddParameter("@CallInSameTransaction", SqlDbType.Bit, callInSameTransaction);
				}
				, (reader) =>
				{
					numbers.Add(Convert.ToInt64(reader[0]));
				});

			return numbers.ToArray();
		}
		#endregion // Implementation
	}
}
