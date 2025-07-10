using System;
using System.Data;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainSetValuesStrategy))]
	class FountainSetValuesStrategyTest : NumberFountainTestCase
	{
		public void TestSampleCallSameTransaction()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 5, maxValue: 10);
			Helper.InsertNumbers(TestConnection, fountainId, 1, 2, 3, 4);
			AssertEquals("Fountain should exist on DB", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);
			AssertEquals("Values", (MinValue: (long)1, NextValue: (long)5, MaxValue: (long)10, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Available Numbers Count", 4, Helper.GetAvailableNumbersCount(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, minValue: 2);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)5, MaxValue: (long)10, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Available Numbers Count", 0, Helper.GetAvailableNumbersCount(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 6);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)6, MaxValue: (long)10, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, maxValue: 20);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)6, MaxValue: (long)20, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, minValue: 3, nextValue: 7);
			AssertEquals("Values", (MinValue: (long)3, NextValue: (long)7, MaxValue: (long)20, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, minValue: 2, maxValue: 30);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)7, MaxValue: (long)30, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 10, maxValue: 25);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)10, MaxValue: (long)25, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValuesStrategy(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 100);
			AssertEquals("Values", (MinValue: (long)1, NextValue: (long)1, MaxValue: (long)100, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
		}

		public void TestLongFountainNameIsSupported()
		{
			var name = LongFountainName;
			var owner = FountainOwner;

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 5, maxValue: 10);
			Helper.InsertNumbers(TestConnection, fountainId, 1, 2, 3, 4);

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 7);
			AssertEquals(7, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestFountainSetValuesStrategyCreatesHeader_Direct()
		{
			var callInSameTransaction = true;
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 15, initMinValue: 10, initNextValue: 15, initMaxValue: 20, initCanRollover: false, callInSameTransaction: callInSameTransaction);
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;
			AssertEquals("Fountain exist?", true, fountainId > 0);
			AssertEquals("Values", (MinValue: (long)10, NextValue: (long)15, MxValue: (long)20, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
		}

		public void TestFountainSetValuesStrategyCreatesHeader_Loopback()
		{
			var callInSameTransaction = false;
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 15, initMinValue: 10, initNextValue: 15, initMaxValue: 20, initCanRollover: false, callInSameTransaction: callInSameTransaction);
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;
			AssertEquals("Fountain exist?", true, fountainId > 0);
			AssertEquals("Values", (MinValue: (long)10, NextValue: (long)15, MxValue: (long)20, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
		}

		public void TestException_FountainNotFound()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			using (var blocker = Db.NewExtraConnectionToMainDb())
			using (blocker.BeginTransactionWithManager())
			{
				blocker.ExecuteNonQuery("DELETE TOP (0) dbo.StmNums WITH (TABLOCKX);");

				var ex = AssertExceptionThrown<SqlException>(() =>
				{
					FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 11, initMinValue: 10, initNextValue: 11, initMaxValue: 199, callInSameTransaction: false);
				});
				AssertContains($"Fountain with name [{name}] and owner [{owner}] is not found and cannot be created", ex.ToString(), ignoreCase: true);
			}
		}

		public void TestException_FountainLockNotAcquired()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 1, initMinValue: 1, initNextValue: 1, initMaxValue: 100, callInSameTransaction: false);
			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			var fountainId = Helper.GetFountainId(TestConnection, name, owner).Value;

			using (var blocker = Db.NewExtraConnectionToMainDb())
			using (blocker.BeginTransactionWithManager())
			{
				var resource = Helper.GetAppLockResource(fountainId);
				blocker.ExecuteNonQuery($"EXEC sp_getapplock @Resource = '{resource}', @LockMode = N'Exclusive', @LockOwner = N'Session';");

				using (TestConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 1);
					});
					AssertContains($"[=FOUNTAIN_LOCK_NOT_ACQUIRED=]-1 (Reason: The lock request timed out (0 ms), ID: {fountainId}, Name: \"{name}\", Owner: {owner})", ex.ToString(), ignoreCase: true);
				}
			}
		}

		public void TestConcurrency()
		{
			// two connections, each calls FountainSetValuesStrategy on the same Fountain (via Loopback) 100 times - no exceptions
			var name = LongFountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			FountainSetValuesStrategy(TestConnection, name, owner, nextValue: 1, initMinValue: 1, initNextValue: 1, initMaxValue: 100, callInSameTransaction: false);
			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			void userAction()
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					for (int i = 0; i < 100; i++)
					{
						using (connection.BeginTransactionWithManager())
						{
							AssertNoExceptionThrown(() =>
							{
								FountainSetValuesStrategy(connection, name, owner, nextValue: 2, callInSameTransaction: false);
							});
						}
					}
				}
			}

			var task1 = Task.Run(userAction);
			var task2 = Task.Run(userAction);

			Task.WaitAll(task1, task2);
		}

		#region Implementation

		void FountainSetValuesStrategy(DbConnection connection, string name, Guid owner, long minValue = 0, long nextValue = 0, long maxValue = 0, long initMinValue = 0, long initNextValue = 0, long initMaxValue = 0, bool initCanRollover = false, bool callInSameTransaction = true)
		{
			using (var cmd = connection.Command(ScriptToTest.Name))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, name);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
				cmd.AddParameter("@NextValue", SqlDbType.BigInt, nextValue);
				cmd.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);
				cmd.AddParameter("@InitMinValue", SqlDbType.BigInt, initMinValue);
				cmd.AddParameter("@InitNextValue", SqlDbType.BigInt, initNextValue);
				cmd.AddParameter("@InitMaxValue", SqlDbType.BigInt, initMaxValue);
				cmd.AddParameter("@InitCanRollover", SqlDbType.Bit, initCanRollover);
				cmd.AddParameter("@CallInSameTransaction", SqlDbType.Bit, callInSameTransaction);

				cmd.ExecuteNonQuery();
			}
		}
		#endregion // Implementation
	}
}

