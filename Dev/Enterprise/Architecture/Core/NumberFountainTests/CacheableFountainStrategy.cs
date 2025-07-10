#if DEBUG

namespace Enterprise.NumberFountain.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using CargoWise.Data;
	using CargoWise.Data.SqlServer;
	using CargoWise.Data.Testing;
	using Enterprise.NumberFountain.Internal;
	using NUnit.Framework;

	class MainConnectionCacheableFountainStrategyTest : CacheableFountainStrategyTest
	{
		protected override DbConnection TestConnection()
		{
			return connection;
		}

		readonly DbConnection connection = Db.Connection;

		protected override void TearDown()
		{
			base.TearDown();
			connection.Dispose();
		}
	}

	class AnotherConnectionCacheableFountainStrategyTest : CacheableFountainStrategyTest
	{
		protected override DbConnection TestConnection()
		{
			return _connection.Value;
		}

		readonly Lazy<DbConnection> _connection = new Lazy<DbConnection>(Db.NewExtraConnectionToMainDb);

		protected override void TearDown()
		{
			base.TearDown();
			_connection.Value.Dispose();
		}
	}

	[UseSnapshotProtection]
	abstract class CacheableFountainStrategyTest : TestCase
	{
		readonly Guid someGuid = Guid.Parse("C116AF7B-61C2-4026-89DA-4DF849B57967");

		internal static CacheableFountainStrategyForTestBase CreateCacheableFountainStrategyForTest(string name, IDbTransaction transaction, int minValue = 1, int maxValue = 100, Guid? ownerPk = null)
		{
			return ownerPk == null
				? new CacheableFountainStrategyForTransactionalTest(name, transaction, minValue, maxValue)
				: new CacheableFountainStrategyForTransactionalTest(name, transaction, minValue, maxValue, ownerPk.Value);
		}

		protected abstract DbConnection TestConnection();

		IDbTransaction Transaction()
		{
			return ((IDbConnectionInternals)TestConnection()).ADOTransaction;
		}

		internal static string GetName()
		{
			return string.Join(string.Empty, Enumerable.Range(1, 246).Select(i => i % 10).Select(i => i.ToString())) + DateTime.Now.ToString("HHmmssffff"); // It's a test! No need to get time from DB
		}

		#region GetNext

		public void TestGetNext()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), 903, 9999);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				AssertEquals("New Key in Fountain", 903, testStrategy.GetNext());
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("Get increments value", 904, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGetNextWithOwner()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), 1001, 9999, someGuid);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				AssertEquals("New Key in Fountain", 1001, testStrategy.GetNext());
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("Get increments value", 1002, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGetNextThrowsExceptionIfNegativeValueReturned()
		{
			const int minValue = 2;
			const int maxValue = 3;

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, maxValue);

				CreateFountainWithFirstValue(testStrategy, minValue);

				var sqlText = @"
				UPDATE stc
					SET SG_Value = -stc.SG_Value,
						SG_IsUsed = 0
					FROM dbo.StmNumberCache stc
					INNER JOIN dbo.StmNums sn ON stc.SG_SN = sn.SN_ID
					WHERE sn.SN_Name = @name";

				TestConnection().ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@name", SqlDbType.VarChar, 256, testStrategy.TestKey));

				AssertEquals("Peek preliminary returns value as is", -maxValue, testStrategy.PeekPreliminary());

				AssertExceptionThrown(
					typeof(NumberFountainInvalidFountainException),
					string.Format(CultureInfo.InvariantCulture,
						"Invalid fountain. Fountain contains (1) negative value(s). All values must be greater than zero. [Fountain ({0})]",
						testStrategy.TestKey),
					() => testStrategy.GetNext()
				);
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestSetNextValue()
		{
			var name = GetName();
			SetNextValue(name, Guid.Empty);
		}

		public void TestSetNextValueWithOwner()
		{
			var name = GetName();
			var ownerPk = someGuid;
			SetNextValue(name, ownerPk);
		}

		void SetNextValue(string name, Guid ownerPk)
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());
				AssertEquals("Should be MinValue", 1, testStrategy.PeekPreliminary());
				AssertEquals("Should be MinValue", 1, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());

				testStrategy.SetNext(1250);
				AssertEquals("Same as SetNextValue", 1250, testStrategy.PeekPreliminary());
				AssertEquals("Take the Value Set", 1250, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());

				testStrategy.SetNext(2250);
				AssertEquals("Same as SetNextValue", 2250, testStrategy.PeekPreliminary());
				AssertEquals("Take the Value Set", 2250, testStrategy.GetNext());
				// !!!!
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());

				AssertEquals("Same as SetNextValue", 2251, testStrategy.PeekPreliminary());
				AssertEquals("Take the Value Set", 2251, testStrategy.GetNext());
				AssertEquals("Incremented", 2252, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch (Exception)
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestPeekPreliminary()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), 701, 9999);
				Assert("Fountain should NOT exist on DB (1)", !testStrategy.FountainExistsOnDb());
				AssertEquals("Should be MinValue", 701, testStrategy.PeekPreliminary());
				Assert("Fountain should NOT exist on DB (2)", !testStrategy.FountainExistsOnDb());

				AssertEquals("New Key in Fountain", 701, testStrategy.GetNext());
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("Value after GetNext", 702, testStrategy.PeekPreliminary());
				AssertEquals("Peek shouldn't increment value", 702, testStrategy.PeekPreliminary());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestPeekPreliminaryClosedConnection()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CacheableFountainStrategyForTestBase testStrategy;
				connection.BeginTransaction();
				try
				{
					testStrategy = CreateCacheableFountainStrategyForTest(GetName(), ((IDbConnectionInternals)connection).ADOTransaction, 701, 9999);
					testStrategy.GetNext();
					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}

				connection.CloseConnection();
				AssertEquals("Should return minimum value", 701, testStrategy.PeekPreliminary());
			}
		}

		[SnailTest]
		public void TestPeekPreliminaryWithEmptyCache()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction());

				AssertEquals("Fountain should NOT exist on DB", false, testStrategy.FountainExistsOnDb());
				AssertEquals("1st Key in Fountain", FountainUtils.MinNumber, testStrategy.PeekPreliminary());

				// GetNext "CacheSize" times from the fountain.
				// 1st GetNext will generate a cache with "CacheSize" numbers and consume the 1st number.
				// Others will consume the rest of the cache. Cache should be empty after that.
				for (var i = testStrategy.TestMinValue; i <= testStrategy.TestMaxValue; i++)
				{
					testStrategy.GetNext();
				}

				AssertEquals("Fountain should exist on DB", true, testStrategy.FountainExistsOnDb());
				AssertEquals("Cache should be empty", 0, testStrategy.FountainCurrentCacheSize());
				AssertEquals("Next Key in Fountain", testStrategy.TestMaxValue + 1, testStrategy.PeekPreliminary());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGenerateCache()
		{
			const int minValue = 501;
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, 9999);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				CreateFountainWithFirstValue(testStrategy, minValue);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("GetNext", minValue + 1, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGenerateCacheWithOwner()
		{
			const int minValue = 551;
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, 9999, someGuid);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());
				AssertEquals("GetNext", minValue, testStrategy.GetNext());
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("GetNext", minValue + 1, testStrategy.GetNext());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		[SnailTest]
		public void TestNoRolloverFountain_Direct()
		{
			AssertNoRolloverFountain(callInSameTransaction: true);
		}

		[SnailTest]
		public void TestNoRolloverFountain_CLR()
		{
			AssertNoRolloverFountain(callInSameTransaction: false);
		}

		void AssertNoRolloverFountain(bool callInSameTransaction)
		{
			const int minValue = 551;
			const int maxValue = 999;

			using (TestConnection().BeginTransactionWithManager())
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, maxValue);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				CreateFountainWithFirstValue(testStrategy, minValue, callInSameTransaction);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("GetNext", minValue + 1, testStrategy.GetNext(callInSameTransaction));
				while (testStrategy.GetNext(callInSameTransaction) != maxValue)
				{
				}
				AssertEquals("PeekPreliminary", maxValue + 1, testStrategy.PeekPreliminary());
				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					$"Fountain has reached its maximum value of 999. Cannot generate any more numbers. [Fountain ({testStrategy.TestKey})]",
					delegate { testStrategy.GetNext(callInSameTransaction); },
					true);
			}
		}

		public void TestGetNextException_MaximumValueReached_Direct()
		{
			AssertGetNextException_MaximumValueReached(callInSameTransaction: true);
		}

		public void TestGetNextException_MaximumValueReached_CLR()
		{
			AssertGetNextException_MaximumValueReached(callInSameTransaction: false);
		}

		void AssertGetNextException_MaximumValueReached(bool callInSameTransaction)
		{
			var name = GetName();
			var initMaxValue = 100;

			using (TestConnection().BeginTransactionWithManager())
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), minValue: 1, maxValue: initMaxValue);
				AssertEquals("Fountain exists?", false, testStrategy.FountainExistsOnDb());

				NumberFountainTestBase.TestArrays("New Key in Fountain"
					, Enumerable.Range(1, 100).Select(x => (long)x)
					, testStrategy.GetNexts(amount: 100, callInSameTransaction: callInSameTransaction));
				AssertEquals("Fountain exists?", true, testStrategy.FountainExistsOnDb());

				var dbMaxValue = initMaxValue;
				var ex = AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() =>
				{
					testStrategy.GetNext(callInSameTransaction);
				});
				AssertContains($"Fountain has reached its maximum value of {initMaxValue}. Cannot generate any more numbers. [Fountain ({name})]", ex.Message, ignoreCase: true);

				dbMaxValue = initMaxValue - 50;
				testStrategy.SetValues(minValue: 0, nextValue: dbMaxValue + 1, maxValue: dbMaxValue, callInSameTransaction);
				ex = AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() =>
				{
					testStrategy.GetNext(callInSameTransaction);
				});
				AssertContains($"Fountain has reached its maximum value of {dbMaxValue} (Init value: {initMaxValue}). Cannot generate any more numbers. [Fountain ({name})]", ex.Message, ignoreCase: true);

				dbMaxValue = initMaxValue + 100;
				testStrategy.SetValues(minValue: 0, nextValue: dbMaxValue + 1, maxValue: dbMaxValue, callInSameTransaction);
				ex = AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() =>
				{
					testStrategy.GetNext(callInSameTransaction);
				});
				AssertContains($"Fountain has reached its maximum value of {dbMaxValue} (Init value: {initMaxValue}). Cannot generate any more numbers. [Fountain ({name})]", ex.Message, ignoreCase: true);
			}
		}

		static void CreateFountainWithFirstValue(CacheableFountainStrategyForTestBase testStrategy, int minValue, bool callInSameTransaction = true)
		{
			AssertEquals("CreateFountainAndGenerateCache returns value", minValue, testStrategy.CreateFountainAndGenerateCache(callInSameTransaction));
		}

		#endregion // GetNext

		#region GetNexts

		public void TestGetNexts()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), 903, 9999);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				NumberFountainTestBase.TestArrays("New Key in Fountain", new[] { (long)903 }, testStrategy.GetNexts(1));
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				NumberFountainTestBase.TestArrays("Get increments values", Enumerable.Range(904, 10).Select(x => (long)x), testStrategy.GetNexts(10));
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGetNextsWithOwner()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), 1001, 9999, someGuid);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				NumberFountainTestBase.TestArrays("New Key in Fountain", Enumerable.Range(1001, 500).Select(x => (long)x), testStrategy.GetNexts(500));
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				NumberFountainTestBase.TestArrays("Get increments value", Enumerable.Range(1501, 10).Select(x => (long)x), testStrategy.GetNexts(10));
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGetNextsThrowsExceptionIfNegativeValueReturned()
		{
			const int minValue = 2;
			const int maxValue = 3;

			TestConnection().BeginTransaction();
			CacheableFountainStrategyForTestBase testStrategy;
			var name = GetName();
			try
			{
				testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), minValue, maxValue);

				CreateFountainWithFirstValue(testStrategy, minValue);

				var sqlText = @"
				UPDATE stc
					SET SG_Value = -stc.SG_Value,
						SG_IsUsed = 0
					FROM dbo.StmNumberCache stc
					INNER JOIN dbo.StmNums sn ON stc.SG_SN = sn.SN_ID
					WHERE sn.SN_Name = @name";

				TestConnection().ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@name", SqlDbType.VarChar, 256, testStrategy.TestKey));

				//because all cached used, so it will get SN_Value from dbo.StmNums
				AssertEquals("Peek preliminary returns value as is", -maxValue, testStrategy.PeekPreliminary());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), minValue, maxValue);
				AssertExceptionThrown(
					typeof(NumberFountainInvalidFountainException),
					"Invalid fountain. Fountain contains (1) negative value(s). All values must be greater than zero.",
					() => testStrategy.GetNexts(1),
					true
					);
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestSetNextValues()
		{
			var name = GetName();
			SetNextValues(name, Guid.Empty);
		}

		public void TestSetNextValuesWithOwner()
		{
			var name = GetName();
			SetNextValues(name, someGuid);
		}

		void SetNextValues(string name, Guid ownerPk)
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());
				AssertEquals("Should be MinValue", 1, testStrategy.PeekPreliminary());
				NumberFountainTestBase.TestArrays("Should be MinValue", Enumerable.Range(1, 5).Select(x => (long)x), testStrategy.GetNexts(5));
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());

				testStrategy.SetNext(1250);
				AssertEquals("Same as SetNextValue", 1250, testStrategy.PeekPreliminary());
				NumberFountainTestBase.TestArrays("Take the Value Set", Enumerable.Range(1250, 150).Select(x => (long)x), testStrategy.GetNexts(150));
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());

				testStrategy.SetNext(2250);
				AssertEquals("Same as SetNextValue", 2250, testStrategy.PeekPreliminary());
				// !!!!
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}

			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), 1, 9999, ownerPk);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());

				AssertEquals("Same as SetNextValue", 2250, testStrategy.PeekPreliminary());
				NumberFountainTestBase.TestArrays("Take the Value Set", Enumerable.Range(2250, 350).Select(x => (long)x), testStrategy.GetNexts(350));
				NumberFountainTestBase.TestArrays("Incremented", Enumerable.Range(2600, 73).Select(x => (long)x), testStrategy.GetNexts(73));
				TestConnection().CommitTransaction();
			}
			catch (Exception)
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestPeeksPreliminary()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), 701, 9999);
				Assert("Fountain should NOT exist on DB (1)", !testStrategy.FountainExistsOnDb());
				AssertEquals("Should be MinValue", 701, testStrategy.PeekPreliminary());
				Assert("Fountain should NOT exist on DB (2)", !testStrategy.FountainExistsOnDb());

				NumberFountainTestBase.TestArrays("New Key in Fountain", new[] { (long)701 }, testStrategy.GetNexts(1));
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("Value after GetNext", 702, testStrategy.PeekPreliminary());
				AssertEquals("Peek shouldn't increment value", 702, testStrategy.PeekPreliminary());

				NumberFountainTestBase.TestArrays("New Key in Fountain", Enumerable.Range(702, 123).Select(x => (long)x), testStrategy.GetNexts(123));
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				AssertEquals("Value after GetNext", 825, testStrategy.PeekPreliminary());
				AssertEquals("Peek shouldn't increment value", 825, testStrategy.PeekPreliminary());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestPeeksPreliminaryClosedConnection()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CacheableFountainStrategyForTestBase testStrategy;
				connection.BeginTransaction();
				try
				{
					testStrategy = CreateCacheableFountainStrategyForTest(GetName(), ((IDbConnectionInternals)connection).ADOTransaction, 701, 9999);
					testStrategy.GetNexts(10);
					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}

				connection.CloseConnection();
				AssertEquals("Should return minimum value", 701, testStrategy.PeekPreliminary());
			}
		}

		public void TestPeeksPreliminaryWithEmptyCache()
		{
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction());

				AssertEquals("Fountain should NOT exist on DB", false, testStrategy.FountainExistsOnDb());
				AssertEquals("1st Key in Fountain", FountainUtils.MinNumber, testStrategy.PeekPreliminary());

				// GetNexts "CacheSize" times from the fountain.
				// 1st GetNext will generate a cache with "CacheSize" numbers and consume the 1st number.
				// Others will consume the rest of the cache. Cache should be empty after that.
				testStrategy.GetNexts((int)(testStrategy.TestMaxValue - testStrategy.TestMinValue + 1));

				AssertEquals("Fountain should exist on DB", true, testStrategy.FountainExistsOnDb());
				AssertEquals("Cache should be empty", 0, testStrategy.FountainCurrentCacheSize());
				AssertEquals("Next Key in Fountain", testStrategy.TestMaxValue + 1, testStrategy.PeekPreliminary());
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGenerateCaches()
		{
			const int minValue = 501;
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, 9999);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				CreateFountainWithFirstValue(testStrategy, minValue);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				NumberFountainTestBase.TestArrays("GetNext", new[] { (long)minValue + 1 }, testStrategy.GetNexts(1));
				NumberFountainTestBase.TestArrays("GetNext", Enumerable.Range(minValue + 2, 150).Select(x => (long)x), testStrategy.GetNexts(150));
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		public void TestGenerateCachesWithOwner()
		{
			const int minValue = 551;
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, 9999, someGuid);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				CreateFountainWithFirstValue(testStrategy, minValue);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				NumberFountainTestBase.TestArrays("GetNexts", new[] { (long)minValue + 1 }, testStrategy.GetNexts(1));
				NumberFountainTestBase.TestArrays("GetNexts", Enumerable.Range(minValue + 2, 150).Select(x => (long)x), testStrategy.GetNexts(150));
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		[SnailTest]
		public void TestNoRolloversFountain_Direct()
		{
			AssertNoRolloversFountain(callInSameTransaction: true);
		}

		[SnailTest]
		public void TestNoRolloversFountain_CLR()
		{
			AssertNoRolloversFountain(callInSameTransaction: false);
		}

		void AssertNoRolloversFountain(bool callInSameTransaction)
		{
			const int minValue = 551;
			const int maxValue = 999;
			TestConnection().BeginTransaction();
			try
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(GetName(), Transaction(), minValue, maxValue);
				Assert("Fountain should NOT exist on DB", !testStrategy.FountainExistsOnDb());

				CreateFountainWithFirstValue(testStrategy, minValue, callInSameTransaction);
				Assert("Fountain should exist on DB", testStrategy.FountainExistsOnDb());
				NumberFountainTestBase.TestArrays("GetNext", new[] { (long)minValue + 1 }, testStrategy.GetNexts(1, callInSameTransaction));
				testStrategy.GetNexts(maxValue - minValue - 1, callInSameTransaction);
				AssertEquals("PeekPreliminary", maxValue + 1, testStrategy.PeekPreliminary());
				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					$"Fountain has reached its maximum value of 999. Cannot generate any more numbers. [Fountain ({testStrategy.TestKey})]",
					delegate
					{ testStrategy.GetNexts(1, callInSameTransaction); },
					true);
				TestConnection().CommitTransaction();
			}
			catch
			{
				TestConnection().RollbackTransaction();
				throw;
			}
		}

		#endregion // GetNexts

		#region SetValues

		public void TestSetNextCreatesNewFountain()
		{
			var name = GetName();
			var owner = Guid.Empty;

			using (var manager = TestConnection().BeginTransactionWithManager())
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), minValue: 1001, maxValue: 9999, owner);
				AssertEquals("Fountain exists?", false, testStrategy.FountainExistsOnDb());

				testStrategy.SetNext(1001);
				AssertEquals("Fountain exists?", true, testStrategy.FountainExistsOnDb());
			}
		}

		public void TestSetValues()
		{
			var name = GetName();
			var owner = Guid.Empty;

			using (var manager = TestConnection().BeginTransactionWithManager())
			{
				var testStrategy = CreateCacheableFountainStrategyForTest(name, Transaction(), minValue: 100, maxValue: 500, owner);

				AssertEquals("Fountain exists?", false, testStrategy.FountainExistsOnDb());

				testStrategy.SetValues(minValue: 200, nextValue: 0, maxValue: 0);
				AssertEquals("Fountain exists?", true, testStrategy.FountainExistsOnDb());
				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)200, MaxValue: (long)500), testStrategy.GetValues());

				testStrategy.SetValues(minValue: 0, nextValue: 300, maxValue: 0);
				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)300, MaxValue: (long)500), testStrategy.GetValues());

				testStrategy.SetValues(minValue: 0, nextValue: 0, maxValue: 600);
				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)300, MaxValue: (long)600), testStrategy.GetValues());

				testStrategy.SetValues(minValue: 100, nextValue: 150, maxValue: 0);
				AssertEquals("Values", (MinValue: (long)100, NextValue: (long)150, MaxValue: (long)600), testStrategy.GetValues());

				testStrategy.SetValues(minValue: 10, nextValue: 0, maxValue: 700);
				AssertEquals("Values", (MinValue: (long)10, NextValue: (long)150, MaxValue: (long)700), testStrategy.GetValues());

				testStrategy.SetValues(minValue: 0, nextValue: 650, maxValue: 800);
				AssertEquals("Values", (MinValue: (long)10, NextValue: (long)650, MaxValue: (long)800), testStrategy.GetValues());

				testStrategy.SetValues(minValue: 1, nextValue: 1, maxValue: 100);
				AssertEquals("Values", (MinValue: (long)1, NextValue: (long)1, MaxValue: (long)100), testStrategy.GetValues());
			}
		}

		#endregion // SetValues

		class CacheableFountainStrategyForTransactionalTest : CacheableFountainStrategyForTest
		{
			CacheableFountainStrategyForTransactionalTest(string name, IDbTransaction transaction)
				: base(transaction)
			{
				TestKey = name;
			}

			public CacheableFountainStrategyForTransactionalTest(string name, IDbTransaction transaction, int minValue, int maxValue)
				: this(name, transaction)
			{
				TestMinValue = minValue;
				TestMaxValue = maxValue;
			}

			public CacheableFountainStrategyForTransactionalTest(string name, IDbTransaction transaction, int minValue, int maxValue, Guid ownerPk)
				: this(name, transaction, minValue, maxValue)
			{
				TestOwnerPk = ownerPk;
			}

			public override string TestKey { get; }
		}
	}

	class CacheableFountainStrategyCallStackTest : TestCase
	{
		public void TestCallStack_GetNexts_Direct()
		{
			var callInSameTransaction = true;

			CacheableFountainStrategyForTestBase strategy = null;
			using (Db.Connection.BeginTransactionWithManager())
			{
				strategy = CacheableFountainStrategyTest.CreateCacheableFountainStrategyForTest(CacheableFountainStrategyTest.GetName(), ((IDbConnectionInternals)Db.Connection).InternalDbTransaction, minValue: 1, maxValue: 1);
			}

			var ex = AssertExceptionThrown<Exception>(() =>
			{
				strategy.GetNexts(amount: 10, callInSameTransaction);
			});

			var message = ex.ToString();
			CombineAssertions(() =>
			{
				AssertContains("CacheableFountainStrategy.GetNexts", ".CacheableFountainStrategy.GetNexts(", message, ignoreCase: true);
				AssertContains("CacheableFountainStrategy.GetNextsInSameTransaction", ".CacheableFountainStrategy.GetNextsInSameTransaction(", message, ignoreCase: true);
				AssertContains("NumberCacheController.GetNexts", ".NumberCacheController.GetNexts(", message, ignoreCase: true);
			});
		}

		public void TestCallStack_GetNexts_CLR()
		{
			var callInSameTransaction = false;

			CacheableFountainStrategyForTestBase strategy = null;
			using (Db.Connection.BeginTransactionWithManager())
			{
				strategy = CacheableFountainStrategyTest.CreateCacheableFountainStrategyForTest(CacheableFountainStrategyTest.GetName(), ((IDbConnectionInternals)Db.Connection).InternalDbTransaction, minValue: 1, maxValue: 1);
			}

			var ex = AssertExceptionThrown<Exception>(() =>
			{
				strategy.GetNexts(amount: 10, callInSameTransaction);
			});

			var message = ex.ToString();
			CombineAssertions(() =>
			{
				AssertContains("CacheableFountainStrategy.GetNexts", ".CacheableFountainStrategy.GetNexts(", message, ignoreCase: true);
				AssertContains("CacheableFountainStrategy.GetNextsInSeparateTransaction", ".CacheableFountainStrategy.GetNextsInSeparateTransaction(", message, ignoreCase: true);
				AssertContains("NumberCacheController.GetNexts", ".NumberCacheController.GetNexts(", message, ignoreCase: true);
			});
		}

		public void TestCallStack_SetNext_Direct()
		{
			var callInSameTransaction = true;

			CacheableFountainStrategyForTestBase strategy = null;
			using (Db.Connection.BeginTransactionWithManager())
			{
				strategy = CacheableFountainStrategyTest.CreateCacheableFountainStrategyForTest(CacheableFountainStrategyTest.GetName(), ((IDbConnectionInternals)Db.Connection).InternalDbTransaction, minValue: 1, maxValue: 1);
			}

			var ex = AssertExceptionThrown<Exception>(() =>
			{
				strategy.SetNext(nextValue: 10, callInSameTransaction);
			});

			var message = ex.ToString();
			CombineAssertions(() =>
			{
				AssertContains("CacheableFountainStrategy.SetValues", ".CacheableFountainStrategy.SetValues(", message, ignoreCase: true);
				AssertContains("CacheableFountainStrategy.SetValuesInSameTransaction", ".CacheableFountainStrategy.SetValuesInSameTransaction(", message, ignoreCase: true);
				AssertContains("NumberCacheController.SetValues", ".NumberCacheController.SetValues(", message, ignoreCase: true);
			});
		}

		public void TestCallStack_SetNext_CLR()
		{
			var callInSameTransaction = false;

			CacheableFountainStrategyForTestBase strategy = null;
			using (Db.Connection.BeginTransactionWithManager())
			{
				strategy = CacheableFountainStrategyTest.CreateCacheableFountainStrategyForTest(CacheableFountainStrategyTest.GetName(), ((IDbConnectionInternals)Db.Connection).InternalDbTransaction, minValue: 1, maxValue: 1);
			}

			var ex = AssertExceptionThrown<Exception>(() =>
			{
				strategy.SetNext(nextValue: 10, callInSameTransaction);
			});

			var message = ex.ToString();
			CombineAssertions(() =>
			{
				AssertContains("CacheableFountainStrategy.SetValues", ".CacheableFountainStrategy.SetValues(", message, ignoreCase: true);
				AssertContains("CacheableFountainStrategy.SetValuesInSeparateTransaction", ".CacheableFountainStrategy.SetValuesInSeparateTransaction(", message, ignoreCase: true);
				AssertContains("NumberCacheController.SetValues", ".NumberCacheController.SetValues(", message, ignoreCase: true);
			});
		}
	}

	[UseSnapshotProtection]
	class CacheableFountainStrategyNonTransactionedTest : TestCase
	{
		public object Thead { get; private set; }

		public void TestGhostRecordsAreAbsent()
		{
			var name = GetName();

			ClearTestFountainData(name);

			SetupTestFountain(4, name);

			long Ghost() => (long)Db.Connection.ExecuteScalar("select sum(Ghost_record_Count + Version_Ghost_Record_Count) from sys.dm_db_index_physical_stats(db_id(), object_id('StmNumberCache'), null, null, 'DETAILED')");

			var initialCount = Ghost();
			try
			{
				Db.Connection.BeginTransaction();

				var testStrategyOnMainConnection = new CacheableStrategyForNonTransactionalTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

				// lots of numbers, will pump fountain multiple times
				for (int i = 1; i <= 10000; i++)
				{
					AssertEquals("GetNext on MainConnection", i, testStrategyOnMainConnection.GetNext());
				}

				Assert((long)Db.Connection.ExecuteScalar("select sum(Ghost_record_Count + Version_Ghost_Record_Count) from sys.dm_db_index_physical_stats(db_id(), object_id('StmNumberCache'), null, null, 'DETAILED')") <= initialCount);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[UseSnapshotProtection]
		public void TestNumberFountainPerformanceSingleThreadMultiTransaction100Thousand()
		{
			var name = GetName();

			ClearTestFountainData(name);

			SetupTestFountain(4, name);

			for (int tranLoop = 0; tranLoop < 100; tranLoop++)
			{
				try
				{
					Db.Connection.BeginTransaction();

					var testStrategyOnMainConnection = new CacheableStrategyForNonTransactionalTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

					// lots of numbers, will pump fountain multiple times
					const int qtyPerTransaction = 1000;
					for (int i = 1; i <= qtyPerTransaction; i++)
					{
						AssertEquals("GetNext on MainConnection", tranLoop * qtyPerTransaction + i, testStrategyOnMainConnection.GetNext());
					}
				}
				finally
				{
					Db.Connection.CommitTransaction();
				}
			}
		}

		[SnailTest]
		[UseSnapshotProtection]
		public void TestNumberFountainPerformanceMultiThread()
		{
			const int threadCount = 60; // concurrent generators (Cpu saturation / more numbers in fountain)
			const int fountainCount = 3; // more equals less contention (faster) and less numbers per fountain
			const int qtyPerThread = 50; // numbers to create per thread (more = longer transaction + more numbers required in fountain)

			var outOfOrder = false;

			var allNumbers = new List<string>(threadCount * qtyPerThread);
			Exception caughtException = null;

			var threads = new List<Thread>();
			for (int threadLoop = 0; threadLoop < threadCount; threadLoop++)
			{
				if (caughtException != null)
				{
					break;
				}

				var thread = new Thread(mythreadLoop =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						try
						{
							var numbers = new List<long>(qtyPerThread);
							var fountainName = "Thread:" + ((int)mythreadLoop % fountainCount).ToString();

							Db.Connection.RunInTransaction(() =>
							{
								CacheableFountainStrategy.GetNext(GetFountainContext(Db.Connection), fountainName, Guid.Empty, false, 1, 10000000, false);

								// lots of numbers, will pump fountain multiple times
								for (int i = 1; i <= qtyPerThread; i++)
								{
									numbers.Add(CacheableFountainStrategy.GetNexts(GetFountainContext(Db.Connection), fountainName, Guid.Empty, 1, false, 1, 10000000, false).First());
								}
							});

							if (!IsIncreasingMontonically(numbers))
							{
								//uncomment to see out of order issues
								//File.WriteAllText("c:\\temp\\fail.txt", string.Join(Environment.NewLine, numbers.Select(x => x.ToString())));
								outOfOrder = true;
							}

							lock (allNumbers)
							{
								allNumbers.AddRange(numbers.Select(x => fountainName + ":" + x.ToString()));
							}
						}
						catch (Exception ex)
						{
							caughtException = ex;
						}
					}
				});
				threads.Add(thread);
				thread.Start(threadLoop);
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			if (caughtException != null)
			{
				Fail(caughtException.ToString());
			}

			AssertEquals(threadCount * qtyPerThread, allNumbers.Count);
			AssertEquals(threadCount * qtyPerThread, allNumbers.Distinct().Count());
			AssertEquals("Out of Order", false, outOfOrder);
		}

		static FountainContext GetFountainContext(DbConnection connection)
		{
			return new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);
		}

		static bool IsIncreasingMontonically(List<long> list)
		{
			return !list.SkipWhile((x, i) => i == 0 || list[i - 1] <= x).Any();
		}

		public void TestApplicationLockInGetNexts()
		{
			// Arrange
			const string keyName = "FountainName";
			var ownerPk = Guid.Empty;

			using (var originalConnection = Db.NewExtraConnectionToMainDb())
			using (originalConnection.BeginTransactionWithManager())
			{
				var context = new FountainContext(((IDbConnectionInternals)originalConnection).ADOTransaction);
				CacheableFountainStrategy.GetNext(context, keyName, ownerPk, false, 1, 10000000, false);

				using (var blockingConnection = Db.NewExtraConnectionToMainDb())
				using (blockingConnection.BeginTransactionWithManager())
				using (var command = blockingConnection.Command($"SELECT * FROM dbo.StmNums WITH (UPDLOCK) WHERE SN_Name='{keyName}'"))
				{
					command.ExecuteNonQuery();

					AssertExceptionThrown<System.Data.Common.DbException>("Header is locked, but we want to generate cache", () => CacheableFountainStrategy.GetNexts(context, keyName, ownerPk, 1000, false, 1, 10000000, false));
				}

				AssertNoExceptionThrown("Header is NOT locked now, but we still want to generate cache", () => CacheableFountainStrategy.GetNexts(context, keyName, ownerPk, 1000, false, 1, 10000000, false));

				// Act
				using (var blockedConnection = Db.NewExtraConnectionToMainDb())
				using (blockedConnection.BeginTransactionWithManager())
				{
					var blockedContext = new FountainContext(((IDbConnectionInternals)blockedConnection).ADOTransaction);

					// Assert
					AssertNoExceptionThrown("Should work without any exceptions", () => CacheableFountainStrategy.GetNexts(blockedContext, keyName, ownerPk, 1000, false, 1, 10000000, false));
				}
			}
		}

		public void TestGetNextWithConcurrency()
		{
			var name = GetName();
			ClearTestFountainData(name);
			SetupTestFountain(4, name);

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					Db.Connection.BeginTransaction();
					anotherConnection.BeginTransaction();

					var testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
					var testStrategyOnAnotherConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)anotherConnection).ADOTransaction);

					Assert("Fountain should exist on DB", testStrategyOnMainConnection.FountainExistsOnDb());
					Assert("Fountain should exist on DB", testStrategyOnAnotherConnection.FountainExistsOnDb());

					// Gets value 1 using main connection
					AssertEquals("GetNext on MainConnection", 1, testStrategyOnMainConnection.GetNext());

					// Gets value 2 using another connection
					AssertEquals("PeekPreliminary before GetNext on AnotherConnection", 2, testStrategyOnMainConnection.PeekPreliminary());
					AssertEquals("GetNext on AnotherConnection", 2, testStrategyOnAnotherConnection.GetNext());
					AssertEquals("PeekPreliminary after GetNext on AnotherConnection", 3, testStrategyOnMainConnection.PeekPreliminary());

					// Gets value 3 using main connection
					Assert("Fountain should exist on DB", testStrategyOnMainConnection.FountainExistsOnDb());
					AssertEquals("GetNext on MainConnection", 3, testStrategyOnMainConnection.GetNext());
					AssertEquals("GetNext on MainConnection", 4, testStrategyOnMainConnection.GetNext());

					// This get next will call GenerateCache to create values 5 - 104
					AssertEquals("GetNext on MainConnection", 5, testStrategyOnMainConnection.GetNext());

					// Check value on StmNums after generate cache - should be PreviousValue + Freed size 3
					AssertEquals("Next cache generation start value", 8, testStrategyOnMainConnection.CurrentCacheGenerationStartValue());
					AssertEquals("Next cache generation start value", 8, testStrategyOnAnotherConnection.CurrentCacheGenerationStartValue());
				}
				finally
				{
					if (anotherConnection.IsInTransaction)
					{
						anotherConnection.RollbackTransaction();
					}

					if (Db.Connection.IsInTransaction)
					{
						Db.Connection.RollbackTransaction();
					}
				}
			}
		}

		[SnailTest]
		public void TestGetNextWithConcurrency_BlockAndCommit()
		{
			var name = GetName();
			ClearTestFountainData(name);
			var assertGetNextUsingAnotherConnection = new Action<int>(expectedNextValue => AssertGetNextWithAnotherConnection(expectedNextValue, name));

			try
			{
				Db.Connection.BeginTransaction();
				var testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

				// Get next in main connection
				AssertEquals("GetNext on another connection", 1, testStrategyOnMainConnection.GetNext());
				AssertEquals("Current Cache Size", 99, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Get next in another thread
				var asyncResult = Task.Run(() => assertGetNextUsingAnotherConnection(2));
				Thread.Sleep(500);

				// Commit main connection transaction => release lock
				Db.Connection.CommitTransaction();
				if (!asyncResult.Wait(2000))
				{
					throw new TimeoutException();
				}

				// (cache size after commit = 0, so other thread will add more numbers to the cache)
				// Note: in CLR GetNext there is no additional buffer creation
				AssertEquals("Current Cache Size", 98, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Delete all numbers from the cache
				Db.Connection.BeginTransaction();
				testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
				var sqlText = "DELETE dbo.StmNumberCache WHERE SG_SN = (SELECT SN_ID FROM dbo.StmNums WHERE SN_Name = @name)";
				Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@name", SqlDbType.VarChar, 256, testStrategyOnMainConnection.TestKey));
				AssertEquals("Current Cache Size", 0, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Get next using another connection (no need for another thread as it shouldn't be blocked on DB)
				assertGetNextUsingAnotherConnection(101);
				AssertEquals("Current Cache Size", 99, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Rollback main connection transaction (DELETE CACHE) and Get next using another connection
				Db.Connection.RollbackTransaction();
				AssertEquals("Current Cache Size", 197, testStrategyOnMainConnection.FountainCurrentCacheSize());
				assertGetNextUsingAnotherConnection(3);
				AssertEquals("Current Cache Size", 196, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Set next in main connection
				Db.Connection.BeginTransaction();
				testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
				testStrategyOnMainConnection.SetNext(333);
				AssertEquals("Current Cache Size", 0, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Get next in another thread
				var e = AssertExceptionThrown<AggregateException>(() =>
				{
					asyncResult = Task.Run(() => assertGetNextUsingAnotherConnection(333));
					asyncResult.Wait();
				});
				AssertContains("Fountain is blocked at the moment", e.ToString(), ignoreCase: true);
				AssertEquals("Still in open transaction", true, Db.Connection.IsInTransaction);

				// Commit main connection transaction => release lock
				Db.Connection.CommitTransaction();

				// cache size after commit = 0
				AssertEquals("Current Cache Size", 0, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Get next in main connection
				Db.Connection.BeginTransaction();
				testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
				AssertEquals("GetNext on another connection", 333, testStrategyOnMainConnection.GetNext());
				AssertEquals("Current Cache Size", 199, testStrategyOnMainConnection.FountainCurrentCacheSize());

				// Check value on StmNums
				AssertEquals("Next cache generation start value", 533, testStrategyOnMainConnection.CurrentCacheGenerationStartValue());
			}
			finally
			{
				if (Db.Connection.IsInTransaction)
				{
					try { Db.Connection.RollbackTransaction(); }
					catch
					{
						// ignored
					}
				}
			}
		}

		[SnailTest]
		public void TestGetNextWithConcurrency_BlockAndRollback()
		{
			var name = GetName();
			ClearTestFountainData(name);
			var assertGetNextUsingAnotherConnection = new Action<int>(expectedNextValue => AssertGetNextWithAnotherConnection(expectedNextValue, name));

			try
			{
				Db.Connection.BeginTransaction();
				try
				{
					var testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

					// Get next in main connection
					AssertEquals("GetNext in main connection", 1, testStrategyOnMainConnection.GetNext());
					AssertEquals("Current Cache Size", 99, testStrategyOnMainConnection.FountainCurrentCacheSize());

					// Get next in another thread
					var asyncResult = Task.Run(() => assertGetNextUsingAnotherConnection(1));
					if (asyncResult.Wait(500))
					{
						throw new InvalidOperationException("Delay expected");
					}
					Db.Connection.RollbackTransaction();
					if (!asyncResult.Wait(2000))
					{
						throw new TimeoutException();
					}
				}
				catch
				{
					Db.Connection.RollbackTransaction();
					throw;
				}

				Db.Connection.BeginTransaction();
				try
				{
					var testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

					// fountain stays after rollback, only used values are reseted. So other thread will take only one value 2
					AssertEquals("Current Cache Size", 99, testStrategyOnMainConnection.FountainCurrentCacheSize());

					// Get next in main connection
					AssertEquals("GetNext in main connection", 2, testStrategyOnMainConnection.GetNext());
					AssertEquals("Current Cache Size", 98, testStrategyOnMainConnection.FountainCurrentCacheSize());
					AssertEquals("GetNext in main connection", 3, testStrategyOnMainConnection.GetNext());
					AssertEquals("Current Cache Size", 97, testStrategyOnMainConnection.FountainCurrentCacheSize());

					Db.Connection.RollbackTransaction();

					// cache size after rollback stays 0 and start value 742
					AssertEquals("Current Cache Size", 99, testStrategyOnMainConnection.FountainCurrentCacheSize());
				}
				catch
				{
					Db.Connection.RollbackTransaction();
					throw;
				}

				// Set next in main connection
				Db.Connection.BeginTransaction();
				try
				{
					var testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
					testStrategyOnMainConnection.SetNext(742);
					AssertEquals("Current Cache Size", 0, testStrategyOnMainConnection.FountainCurrentCacheSize());
					AssertEquals("GetNext in main connection", 742, testStrategyOnMainConnection.GetNext());

					Db.Connection.CommitTransaction();
				}
				catch
				{
					Db.Connection.RollbackTransaction();
					throw;
				}

				// Get next in main connection
				Db.Connection.BeginTransaction();
				try
				{
					var testStrategyOnMainConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
					AssertEquals("GetNext in main connection", 743, testStrategyOnMainConnection.GetNext());
					AssertEquals("Current Cache Size", 98, testStrategyOnMainConnection.FountainCurrentCacheSize());

					// Check value on StmNums
					AssertEquals("Next cache generation start value", 842, testStrategyOnMainConnection.CurrentCacheGenerationStartValue());
				}
				catch
				{
					Db.Connection.RollbackTransaction();
					throw;
				}
			}
			finally
			{
				if (Db.Connection.IsInTransaction)
				{
					try
					{ Db.Connection.RollbackTransaction(); }
					catch
					{
						// ignored
					}
				}
			}
		}

		static void AssertGetNextWithAnotherConnection(int expectedNextValue, string name)
		{
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				anotherConnection.BeginTransaction();
				var testStrategyOnAnotherConnection = CreateCacheableFountainStrategyForTest(name, ((IDbConnectionInternals)anotherConnection).ADOTransaction);
				AssertEquals("GetNext on another connection", expectedNextValue, testStrategyOnAnotherConnection.GetNext());
				anotherConnection.CommitTransaction();
			}
		}

		[UseSnapshotProtection]
		public void TestReleaseAppLock_GetNexts()
		{
			using (Db.Connection.TemporarySetLockTimeout(DbConnection.LockTimeout.SqlDefault))
			{
				var name = GetName();
				ClearTestFountainData(name);
				SetupTestFountain(4, name);

				// generate sql exec plan to avoid blocking by Sch-M lock later
				using (Db.Connection.BeginTransactionWithManager())
				{
					var strategy = new CacheableStrategyForNonTransactionalTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
					strategy.GetNexts(amount: 1, callInSameTransaction: true);
				}

				using (Db.Connection.BeginTransactionWithManager())
				{
					var strategy = new CacheableStrategyForNonTransactionalTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

					using (var blocker = Db.NewExtraConnectionToMainDb())
					using (blocker.BeginTransactionWithManager())
					{
						// put Sch-M lock
						DataUtils.AddTableExtendedProperty(blocker, new DbSchemaTable(Db.DatabaseName, "dbo", "StmNumberCache"), "Test property", "1");

						var ex = AssertExceptionThrown<System.Data.Common.DbException>(() =>
						{
							strategy.GetNexts(amount: 1, callInSameTransaction: true);
						});
						AssertContains("Timeout Expired", ex.Message, ignoreCase: true);

						var resource = $"NumberFountain_{strategy.GetFountainId(name)}";
						using (var cmd = Db.Connection.Command("SELECT APPLOCK_MODE('public', @Resource, 'Session')"))
						{
							cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, resource);

#if NETFRAMEWORK
							AssertEquals("NoLock", cmd.ExecuteScalar());
#elif Net
							AssertEquals("Exclusive", cmd.ExecuteScalar());
#endif
						}
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestReleaseAppLock_SetNext()
		{
			using (Db.Connection.TemporarySetLockTimeout(DbConnection.LockTimeout.SqlDefault))
			{
				var name = GetName();
				ClearTestFountainData(name);
				SetupTestFountain(4, name);

				// generate sql exec plan to avoid blocking by Sch-M lock later
				using (Db.Connection.BeginTransactionWithManager())
				{
					var strategy = new CacheableStrategyForNonTransactionalTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
					strategy.SetNext(nextValue: 1, callInSameTransaction: true);
				}

				using (Db.Connection.BeginTransactionWithManager())
				{
					var strategy = new CacheableStrategyForNonTransactionalTest(name, ((IDbConnectionInternals)Db.Connection).ADOTransaction);

					using (var blocker = Db.NewExtraConnectionToMainDb())
					using (blocker.BeginTransactionWithManager())
					{
						// put Sch-M lock
						DataUtils.AddTableExtendedProperty(blocker, new DbSchemaTable(Db.DatabaseName, "dbo", "StmNumberCache"), "Test property", "1");

						var ex = AssertExceptionThrown<System.Data.Common.DbException>(() =>
						{
							strategy.SetNext(nextValue: 1, callInSameTransaction: true);
						});
						AssertContains("Timeout Expired", ex.Message, ignoreCase: true);

						var resource = $"NumberFountain_{strategy.GetFountainId(name)}";
						using (var cmd = Db.Connection.Command("SELECT APPLOCK_MODE('public', @Resource, 'Session');"))
						{
							cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, resource);

#if NETFRAMEWORK
							AssertEquals("NoLock", cmd.ExecuteScalar());
#elif Net
							AssertEquals("Exclusive", cmd.ExecuteScalar());
#endif
						}
					}
				}
			}
		}

		static void SetupTestFountain(int initialCacheSize, string name)
		{
			const string sqlText = "INSERT dbo.StmNums (SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue) VALUES (@Name, @Value, @MinValue, @MaxValue); SELECT SCOPE_IDENTITY();";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@Name", SqlDbType.VarChar, name);
			cmd.AddParameter("@Value", SqlDbType.BigInt, initialCacheSize + 1);
			cmd.AddParameter("@MinValue", SqlDbType.BigInt, FountainUtils.MinNumber);
			cmd.AddParameter("@MaxValue", SqlDbType.BigInt, FountainUtils.MaxNumber);
			var testFountainId = Convert.ToInt32(cmd.ExecuteScalar());

			cmd.CommandText = "INSERT dbo.StmNumberCache (SG_SN, SG_Value) VALUES (@FountainId, @Value)";
			cmd.RemoveParameterIfExists("@Name");
			cmd.AddParameter("@FountainId", SqlDbType.Int, testFountainId);
			for (var i = 1; i <= initialCacheSize; i++)
			{
				cmd.SetParameterValue("@Value", i);
				cmd.ExecuteNonQuery();
			}
		}

		static void ClearTestFountainData(string name)
		{
			var sqlText = "DELETE dbo.StmNums WHERE SN_Name = @Name";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@Name", SqlDbType.VarChar, name);
			cmd.ExecuteNonQuery();
		}

		static CacheableFountainStrategyForTestBase CreateCacheableFountainStrategyForTest(string name, IDbTransaction transaction)
		{
			return new CacheableStrategyForNonTransactionalTest(name, transaction);
		}

		static string GetName()
		{
			return DateTime.Now.ToString("HHmmssffff"); // It's a test! No need to get time from DB
		}

		class CacheableStrategyForNonTransactionalTest : CacheableFountainStrategyForTest
		{
			public CacheableStrategyForNonTransactionalTest(string name, IDbTransaction transaction)
				: base(transaction)
			{
				TestKey = name;
			}

			public override string TestKey { get; }
		}
	}

	abstract class CacheableFountainStrategyForTestBase
	{
		protected CacheableFountainStrategyForTestBase(IDbTransaction transaction)
		{
			Connection = transaction.Connection;
			Transaction = transaction;
		}

		protected readonly IDbConnection Connection;
		protected readonly IDbTransaction Transaction;

		public Guid TestOwnerPk = Guid.Empty;
		public bool TestRollOver = FountainUtils.NoRollOver;
		public long TestMinValue = 1;
		public long TestMaxValue = 9999;

		public abstract string TestKey { get; }

		public abstract long GetNext(bool callInSameTransaction = true);

		public abstract long[] GetNexts(int amount, bool callInSameTransaction = true);

		public int GetFountainId(string name)
		{
			return GetFountainId(name, owner: Guid.Empty);
		}

		public int GetFountainId(string name, Guid owner)
		{
			var cmd = Connection.CreateCommand(); // We're not in Kansas any more Toto (we're outside of Enterprise)
			cmd.Transaction = Transaction;
			cmd.CommandText = "SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";

			AddTestKeyAndOwnerParams(cmd, name, owner);

			return Convert.ToInt32(cmd.ExecuteScalar());
		}

		public void SetNext(long nextValue, bool callInSameTransaction = true)
		{
			CacheableFountainStrategy.SetValues(new FountainContext(Transaction), TestKey, TestOwnerPk,
				minValue: 0,
				nextValue: nextValue,
				maxValue: 0,
				initMinValue: TestMinValue,
				initNextValue: nextValue,
				initMaxValue: TestMaxValue,
				initCanRollover: TestRollOver,
				callInSameTransaction);
		}

		public void SetValues(long minValue, long nextValue, long maxValue, bool callInSameTransaction = true)
		{
			CacheableFountainStrategy.SetValues(new FountainContext(Transaction), TestKey, TestOwnerPk, minValue, nextValue, maxValue,
				initMinValue: TestMinValue,
				initNextValue: (nextValue > 0) ? nextValue : (minValue > 0) ? minValue : TestMinValue,
				initMaxValue: TestMaxValue,
				initCanRollover: TestRollOver,
				callInSameTransaction);
		}

		public (long MinValue, long NextValue, long MaxValue) GetValues()
		{
			var minValue = 0L;
			var nextValue = 0L;
			var maxValue = 0L;

			var sql = "SELECT SN_MinimumValue, SN_Value, SN_MaximumValue FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";
			using (var cmd = Connection.CreateCommand()) // We're not in Kansas any more Toto (we're outside of Enterprise)
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = sql;

				AddTestKeyAndOwnerParams(cmd);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						minValue = (long)reader["SN_MinimumValue"];
						nextValue = (long)reader["SN_Value"];
						maxValue = (long)reader["SN_MaximumValue"];
					}
				}
			}

			return (minValue, nextValue, maxValue);
		}

		public long PeekPreliminary()
		{
			return CacheableFountainStrategy.PeekPreliminary(Connection, Transaction, TestKey, TestOwnerPk, TestMinValue);
		}

		public abstract long CreateFountainAndGenerateCache(bool callInSameTransaction = true);

		public bool FountainExistsOnDb()
		{
			var sqlText = "SELECT count(*) FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";
			var command = Connection.CreateCommand(); // We're not in Kansas any more Toto (we're outside of Enterprise)
			command.Transaction = Transaction;
			command.CommandText = sqlText;

			AddTestKeyAndOwnerParams(command);

			var count = Convert.ToInt32(command.ExecuteScalar());
			return (count == 1);
		}

		void AddTestKeyAndOwnerParams(IDbCommand command)
		{
			AddTestKeyAndOwnerParams(command, TestKey, TestOwnerPk);
		}

		void AddTestKeyAndOwnerParams(IDbCommand command, string key, Guid owner)
		{
			if (command is DbCommand dbCommand)
			{
				dbCommand.AddParameter("@name", SqlDbType.VarChar, 256, key);
				dbCommand.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
			}
			else
			{
				var nameParam = new SqlParameter("@name", SqlDbType.VarChar, 256);
				nameParam.Value = key;

				var ownerParam = new SqlParameter("@owner", SqlDbType.UniqueIdentifier);
				ownerParam.Value = owner;

				command.Parameters.Add(nameParam);
				command.Parameters.Add(ownerParam);
			}
		}

		public long CurrentCacheGenerationStartValue()
		{
			var sqlText = $"SELECT SN_Value FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";
			var objResult = Db.Connection.ExecuteScalar(
				sqlText,
				cmd =>
				{
					cmd.AddParameter("@name", SqlDbType.VarChar, 256, TestKey);
					cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, TestOwnerPk);
				});
			var result = (objResult == null || objResult == DBNull.Value) ? -1 : Convert.ToInt64(objResult);
			return result;
		}

		public int FountainCurrentCacheSize()
		{
			var sqlText = @"
				SELECT count(*) 
				FROM dbo.StmNumberCache with (READPAST, READCOMMITTEDLOCK)
				WHERE
					SG_IsUsed = 0
					AND SG_SN = (SELECT SN_ID FROM dbo.StmNums
				               WHERE SN_Name = @name AND SN_Owner = @owner)";
			var command = Connection.CreateCommand(); // We're not in Kansas any more Toto (we're outside of Enterprise)
			command.Transaction = Transaction;
			command.CommandText = sqlText;
			AddTestKeyAndOwnerParams(command);
			int count = Convert.ToInt32(command.ExecuteScalar());
			return count;
		}
	}

	abstract class CacheableFountainStrategyForTest : CacheableFountainStrategyForTestBase
	{
		protected CacheableFountainStrategyForTest(IDbTransaction transaction)
			: base(transaction)
		{
		}

		public override long GetNext(bool callInSameTransaction = true)
		{
			return CacheableFountainStrategy.GetNext(new FountainContext(Transaction), TestKey, TestOwnerPk, TestRollOver, TestMinValue, TestMaxValue, callInSameTransaction);
		}

		public override long[] GetNexts(int amount, bool callInSameTransaction = true)
		{
			return CacheableFountainStrategy.GetNexts(new FountainContext(Transaction), TestKey, TestOwnerPk, amount, TestRollOver, TestMinValue, TestMaxValue, callInSameTransaction);
		}

		public override long CreateFountainAndGenerateCache(bool callInSameTransaction = true)
		{
			return CacheableFountainStrategy.GetNext(new FountainContext(Transaction), TestKey, TestOwnerPk, TestRollOver, TestMinValue, TestMaxValue, callInSameTransaction);
		}
	}
}

#endif
