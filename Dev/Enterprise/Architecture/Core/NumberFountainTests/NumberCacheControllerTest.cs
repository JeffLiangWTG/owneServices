#if DEBUG

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.NumberFountain.Internal;
using NUnit.Framework;

namespace Enterprise.NumberFountain.Testing
{
	class NumberCacheControllerCommonTest : TestCase
	{
		public void TestFountainNameMaxLength()
		{
			AssertEquals(NumberCacheController.FountainNameMaxLength
				, Convert.ToInt32(Db.Connection.ExecuteScalar("SELECT max_length FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.StmNums', N'U') AND name = N'SN_Name';")));
		}
	}

	[UseSnapshotProtection]
	class NumberCacheControllerTest : TestCase
	{
		static string GetName()
		{
			return string.Join(string.Empty, Enumerable.Range(1, 246).Select(i => i % 10).Select(i => i.ToString())) + DateTime.Now.ToString("HHmmssffff"); // It's a test! No need to get time from DB
		}

		public void TestGetNextsValue()
		{
			AssertGetNextsValue(GetName(), Guid.Empty);
		}

		public void TestGetNextsValueWithOwner()
		{
			AssertGetNextsValue(GetName(), Guid.NewGuid());
		}

		static void AssertGetNextsValue(string name, Guid ownerPk)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				try
				{
					var transaction = ((IDbConnectionInternals)connection).ADOTransaction;
					var fountainContext = new FountainContext(transaction);

					AssertEquals("1st Value", 1234, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 1, 1234, 9999, false, false)[0]);
					AssertEquals("2nd Value", 1235, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 1, 1234, 9999, false, false)[0]);
					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		public void TestGenerateCache()
		{
			AssertGenerateCache(GetName(), Guid.Empty);
		}

		public void TestGenerateCacheWithOwner()
		{
			AssertGenerateCache(GetName(), Guid.NewGuid());
		}

		static void AssertGenerateCache(string name, Guid ownerPk)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				try
				{
					AssertEquals("Fountain exist on DB?", false, DoesFountainExist(connection, name, ownerPk));

					var transaction = ((IDbConnectionInternals)connection).ADOTransaction;
					var fountainContext = new FountainContext(transaction);

					AssertEquals("Start", 551, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 1, 551, 9999, false, false)[0]);
					AssertEquals("Fountain exist on DB?", true, DoesFountainExist(connection, name, ownerPk));
					AssertEquals("Cache Start", 552, GetMinCacheValue(connection, name, ownerPk));
					AssertEquals("Cache End", 650, GetMaxCacheValue(connection, name, ownerPk));
					AssertEquals("Next Cache Start", 651, GetNextCacheStartValue(connection, name, ownerPk));
					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		[SnailTest]
		public void TestNoRolloverGenerateCache()
		{
			NoRolloverGenerateCache(GetName(), Guid.Empty);
		}

		[SnailTest]
		public void TestNoRolloverGenerateCacheOwner()
		{
			NoRolloverGenerateCache(GetName(), Guid.NewGuid());
		}

		static void NoRolloverGenerateCache(string name, Guid ownerPk)
		{
			const int minValue = 1;
			const int maxValue = 198;
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);
					var getNextValue = new Func<long?>(() => NumberCacheController.GetNexts(fountainContext, name, ownerPk, 1, minValue, maxValue, false, true)[0]);

					CreateFountainAndTestCache(connection, name, ownerPk, minValue, getNextValue);

					SetFountainToTheLastValueAndTestIt(connection, name, ownerPk, getNextValue, maxValue, maxValue + 1);

					// Consume remaining number in the cache
					AssertEquals("Cache Start", -1, GetMinCacheValue(connection, name, ownerPk));
					AssertEquals("Cache End", -1, GetMaxCacheValue(connection, name, ownerPk));
					AssertEquals("Next Cache Start", maxValue + 1, GetNextCacheStartValue(connection, name, ownerPk));

					// Try generating more numbers without rollover => should thrown an exception and do nothing
					AssertExceptionThrown(
						typeof(System.Data.Common.DbException),
						$"Fountain has reached its maximum value of {maxValue}. Cannot generate any more numbers.",
						() => getNextValue(),
						true);
					AssertEquals("Cache Start (no rollover)", -1, GetMinCacheValue(connection, name, ownerPk));
					AssertEquals("Cache End (no rollover)", -1, GetMaxCacheValue(connection, name, ownerPk));
					AssertEquals("Next Cache Start (no rollover)", maxValue + 1, GetNextCacheStartValue(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		public void TestNoRolloverGeneration()
		{
			NoRolloverGeneration(GetName(), Guid.Empty);
		}

		public void TestNoRolloverGenerationOwned()
		{
			NoRolloverGeneration(GetName(), Guid.NewGuid());
		}

		static void NoRolloverGeneration(string name, Guid ownerPk)
		{
			const int minValue = 1;
			const int maxValue = 20;
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateFountainAndInsertNumbers(connection, name, ownerPk, 16, minValue, maxValue, false, new[] { 1L, 2, 4, 15 });
				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);

					NumberFountainTestBase.TestArrays("Get values", new[] { 1L, 2, 4 }, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 3, minValue, maxValue, false, false));
					NumberFountainTestBase.TestArrays("Cache", new[] { 15L }, GetCacheValues(connection, name, ownerPk));
					AssertEquals("Next Cache Start", 16, GetNextCacheStartValue(connection, name, ownerPk));
					NumberFountainTestBase.TestArrays("Get values", new[] { (long)15, 16, 17, 18 }, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 4, minValue, maxValue, false, false));
					NumberFountainTestBase.TestArrays("Cache", new[] { 19L, 20 }, GetCacheValues(connection, name, ownerPk));
					AssertEquals("Next Cache Start", 21, GetNextCacheStartValue(connection, name, ownerPk));
					NumberFountainTestBase.TestArrays("Get values", new[] { 19L, 20 }, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 2, minValue, maxValue, false, false));
					AssertEquals("Next Cache Start", 21, GetNextCacheStartValue(connection, name, ownerPk));
					NumberFountainTestBase.TestArrays("Cache", Array.Empty<long>(), GetCacheValues(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		public void TestRolloverGeneration()
		{
			RolloverGeneration(GetName(), Guid.Empty);
		}

		public void TestRolloverGenerationOwned()
		{
			RolloverGeneration(GetName(), Guid.NewGuid());
		}

		static void RolloverGeneration(string name, Guid ownerPk)
		{
			const int minValue = 13;
			const int maxValue = 20000;
			const int nextCacheValue = 19501;

			var r = new Random(1);
			var cache = Enumerable.Range(1, 550).Select(i => (long)r.Next(10000, 18000)).OrderBy(i => i).Distinct().Take(500).ToArray();
			if (cache.Length != 500)
			{
				throw new IndexOutOfRangeException();
			}

			var testAmounts = new[] { 113, 34, 169, 116, 51 }; // 483

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateFountainAndInsertNumbers(connection, name, ownerPk, nextCacheValue, minValue, maxValue, true, cache);
				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);

					AssertEquals("Next Cache Start", nextCacheValue, GetNextCacheStartValue(connection, name, ownerPk));
					NumberFountainTestBase.TestArrays("Cache", cache, GetCacheValues(connection, name, ownerPk));

					// testing getting from cache
					NumberFountainTestBase.TestArrays("Get values", cache.Take(testAmounts[0]).ToArray(), NumberCacheController.GetNexts(fountainContext, name, ownerPk, testAmounts[0], minValue, maxValue, false, false));
					NumberFountainTestBase.TestArrays("Cache", cache.Skip(testAmounts[0]).ToArray(), GetCacheValues(connection, name, ownerPk));
					AssertEquals("Next Cache Start", nextCacheValue, GetNextCacheStartValue(connection, name, ownerPk));

					for (var i = 1; i < testAmounts.Length; i++)
					{
						NumberFountainTestBase.TestArrays("Get values", cache.Skip(testAmounts.Take(i).Sum()).Take(testAmounts[i]).ToArray(),
							NumberCacheController.GetNexts(fountainContext, name, ownerPk, testAmounts[i], minValue, maxValue, false, false));
						NumberFountainTestBase.TestArrays("Cache", cache.Skip(testAmounts.Take(i + 1).Sum()).ToArray(), GetCacheValues(connection, name, ownerPk));
						AssertEquals("Next Cache Start", nextCacheValue, GetNextCacheStartValue(connection, name, ownerPk));
					}

					// last part from 483
					NumberFountainTestBase.TestArrays("Cache", cache.Skip(testAmounts.Sum()).ToArray(), GetCacheValues(connection, name, ownerPk));
					AssertEquals("Next Cache Start", nextCacheValue, GetNextCacheStartValue(connection, name, ownerPk));

					// generating new cache
					var count = 34;
					NumberFountainTestBase.TestArrays("Get values", cache.Skip(testAmounts.Sum()).Concat(EnumerableExtensions.Range(nextCacheValue, count)).Select(x => x),
						NumberCacheController.GetNexts(fountainContext, name, ownerPk, 500 - testAmounts.Sum() + count, minValue, maxValue, false, false));
					cache = Enumerable.Range(0, 66).Select(i => (long)nextCacheValue + count + i).ToArray();
					NumberFountainTestBase.TestArrays("Cache", cache, GetCacheValues(connection, name, ownerPk));
					AssertEquals("Next Cache Start", nextCacheValue + count + cache.Length, GetNextCacheStartValue(connection, name, ownerPk));

					// get many numbers to almost full fountain
					count += nextCacheValue;
					NumberFountainTestBase.TestArrays("Get values", Enumerable.Range(count, 383).Select(x => (long)x),
						NumberCacheController.GetNexts(fountainContext, name, ownerPk, 383, minValue, maxValue, false, false));
					AssertEquals("Next Cache Start", minValue, GetNextCacheStartValue(connection, name, ownerPk));

					// init rollover
					count += 383;
					NumberFountainTestBase.TestArrays("Get values", Enumerable.Range(count, maxValue - count + 1).Concat(Enumerable.Range(minValue, 57)).Select(x => (long)x),
						NumberCacheController.GetNexts(fountainContext, name, ownerPk, maxValue - count + 1 + 57, minValue, maxValue, false, false));
					AssertEquals("Next Cache Start", minValue + 100, GetNextCacheStartValue(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		public void TestRolloverWithReusageFromTheEnd()
		{
			RolloverWithReusageFromTheEnd(GetName(), Guid.Empty);
		}

		public void TestRolloverWithReusageFromTheEndOwned()
		{
			RolloverWithReusageFromTheEnd(GetName(), Guid.NewGuid());
		}

		void RolloverWithReusageFromTheEnd(string name, Guid ownerPk)
		{
			const int minValue = 1;
			const int maxValue = 120;
			var cache = new[] { (long)1, 2, 4, 5, 112, 115 };

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateFountainAndInsertNumbers(connection, name, ownerPk, 116, minValue, maxValue, true, cache);

				// rollover reading of used values is available only in new transaction
				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);

					CreateSmallFountain(connection, name, ownerPk, cache, fountainContext, minValue, maxValue);

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}

				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);

					NumberFountainTestBase.TestArrays("Get values", new[] { (long)118, 119, 120 }, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 3, minValue, maxValue, false, false));
					NumberFountainTestBase.TestArrays("Cache", Array.Empty<long>(), GetCacheValues(connection, name, ownerPk));
					AssertEquals("Next Cache Start", minValue, GetNextCacheStartValue(connection, name, ownerPk));

					NumberFountainTestBase.TestArrays("Get values", new[] { (long)1, 2 }, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 2, minValue, maxValue, false, false));
					AssertEquals("Next Cache Start", 9, GetNextCacheStartValue(connection, name, ownerPk));
					NumberFountainTestBase.TestArrays("Cache", Enumerable.Range(3, 9 - 3).Select(i => (long)i), GetCacheValues(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		public void TestRolloverWithReusageBeyondTheEnd()
		{
			RolloverWithReusageBeyondTheEnd(GetName(), Guid.Empty);
		}

		public void TestRolloverWithReusageBeyondTheEndOwned()
		{
			RolloverWithReusageBeyondTheEnd(GetName(), Guid.NewGuid());
		}

		void RolloverWithReusageBeyondTheEnd(string name, Guid ownerPk)
		{
			const int minValue = 1;
			const int maxValue = 120;
			var cache = new[] { (long)1, 2, 4, 5, 112, 115 };

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				CreateFountainAndInsertNumbers(connection, name, ownerPk, 116, minValue, maxValue, true, cache);

				// rollover reading of used values is available only in new transaction
				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);

					CreateSmallFountain(connection, name, ownerPk, cache, fountainContext, minValue, maxValue);

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}

				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);

					NumberFountainTestBase.TestArrays("Get values", new[] { (long)118, 119, 120, 1, 2 }, NumberCacheController.GetNexts(fountainContext, name, ownerPk, 5, minValue, maxValue, false, false));
					AssertEquals("Next Cache Start", 9, GetNextCacheStartValue(connection, name, ownerPk));
					NumberFountainTestBase.TestArrays("Cache", Enumerable.Range(3, 9 - 3).Select(i => (long)i), GetCacheValues(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		static void CreateSmallFountain(DbConnection connection, string name, Guid ownerPk, long[] cache, IFountainContext fountainContext, int minValue, int maxValue)
		{
			NumberFountainTestBase.TestArrays("Get values", cache.Take(3).Select(x => x), NumberCacheController.GetNexts(fountainContext, name, ownerPk, 3, minValue, maxValue, false, false));
			AssertEquals("Next Cache Start", 116, GetNextCacheStartValue(connection, name, ownerPk));
			NumberFountainTestBase.TestArrays("Cache", cache.Skip(3).Select(x => x), GetCacheValues(connection, name, ownerPk));

			NumberFountainTestBase.TestArrays("Get values", cache.Skip(3).Concat(new[] { (long)116, 117 }), NumberCacheController.GetNexts(fountainContext, name, ownerPk, 5, minValue, maxValue, false, false));
			NumberFountainTestBase.TestArrays("Cache", new[] { 118L, 119, 120 }, GetCacheValues(connection, name, ownerPk));
			AssertEquals("Next Cache Start", 1, GetNextCacheStartValue(connection, name, ownerPk));
		}

		static void CreateFountainAndTestCache(DbConnection connection, string name, Guid ownerPk, long minValue, Func<long?> getNextValue)
		{
			AssertEquals("Fountain exist on DB?", false, DoesFountainExist(connection, name, ownerPk));
			AssertEquals("Create Fountain", minValue, getNextValue());
			AssertEquals("Fountain exist on DB?", true, DoesFountainExist(connection, name, ownerPk));

			// Generated cache
			AssertEquals("Cache Start", 2, GetMinCacheValue(connection, name, ownerPk));
			AssertEquals("Cache End", 100, GetMaxCacheValue(connection, name, ownerPk));
			AssertEquals("Next Cache Start", 101, GetNextCacheStartValue(connection, name, ownerPk));
		}

		static void SetFountainToTheLastValueAndTestIt(DbConnection connection, string name, Guid ownerPk, Func<long?> getNextValue, long maxValue, long nextCacheStart)
		{
			// Set fountain to its max value
			while (getNextValue() != maxValue - 1)
			{
			}
			AssertEquals("Cache Start", maxValue, GetMinCacheValue(connection, name, ownerPk));
			AssertEquals("Cache End", maxValue, GetMaxCacheValue(connection, name, ownerPk));
			AssertEquals("Next Cache Start", nextCacheStart, GetNextCacheStartValue(connection, name, ownerPk));

			AssertEquals("The last value", maxValue, getNextValue());
		}

		[SnailTest]
		public void TestRolloverGenerateCache()
		{
			RolloverGenerateCache(GetName(), Guid.Empty);
		}

		[SnailTest]
		public void TestRolloverGenerateCacheWithOwner()
		{
			RolloverGenerateCache(GetName(), Guid.NewGuid());
		}

		static void RolloverGenerateCache(string name, Guid ownerPk)
		{
			const long minValue = 1;
			const long maxValue = 198;
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);
					var getNextValue = new Func<long?>(() => NumberCacheController.GetNexts(fountainContext, name, ownerPk, 1, minValue, maxValue, true, true)[0]);

					CreateFountainAndTestCache(connection, name, ownerPk, minValue, getNextValue);

					SetFountainToTheLastValueAndTestIt(connection, name, ownerPk, getNextValue, maxValue, 1);

					// Consume remaining number in the cache
					AssertEquals("Cache Start", -1, GetMinCacheValue(connection, name, ownerPk));
					AssertEquals("Cache End", -1, GetMaxCacheValue(connection, name, ownerPk));
					AssertEquals("Next Cache Start", minValue, GetNextCacheStartValue(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}

				connection.BeginTransaction();
				try
				{
					var fountainContext = new FountainContext(((IDbConnectionInternals)connection).ADOTransaction);
					var getNextValue = new Func<long?>(() => NumberCacheController.GetNexts(fountainContext, name, ownerPk, 1, minValue, maxValue, true, false)[0]);

					// Generating more numbers => Should generate more numbers from the minimum value
					AssertEquals("The value after last one", minValue, getNextValue());
					AssertEquals("Cache Start (after rollover)", 2, GetMinCacheValue(connection, name, ownerPk));
					AssertEquals("Cache End (after rollover)", 100, GetMaxCacheValue(connection, name, ownerPk));
					AssertEquals("Next Cache Start (after rollover)", 101, GetNextCacheStartValue(connection, name, ownerPk));

					connection.CommitTransaction();
				}
				catch
				{
					connection.RollbackTransaction();
					throw;
				}
			}
		}

		public void TestSetValues()
		{
			var name = GetName();
			var owner = Guid.Empty;

			var initMinValue = 100;
			var initNextValue = 300;
			var initMaxValue = 500;
			var initCanRollover = false;
			var callInSameTransaction = true;

			var connection = Db.Connection;
			using (connection.BeginTransactionWithManager())
			{
				var fountainContext = new FountainContext(((IDbConnectionInternals)connection).InternalDbTransaction);

				AssertEquals("Fountain exists on DB?", false, DoesFountainExist(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 200,
					nextValue: 0,
					maxValue: 0,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Fountain exists on DB?", true, DoesFountainExist(connection, name, owner));

				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)300, MaxValue: (long)500), GetValues(connection, name, owner));
				AssertEquals("Available numbers", 0, GetAvailableNumbersCount(connection, name, owner));

				AssertEquals("Next number", 300, NumberCacheController.GetNexts(fountainContext, name, owner, 1, initMinValue, initMaxValue, initCanRollover, callInSameTransaction)[0]);
				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)400, MaxValue: (long)500), GetValues(connection, name, owner));
				AssertEquals("Available numbers", 99, GetAvailableNumbersCount(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 0,
					nextValue: 450,
					maxValue: 0,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)450, MaxValue: (long)500), GetValues(connection, name, owner));
				AssertEquals("Available numbers", 0, GetAvailableNumbersCount(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 0,
					nextValue: 0,
					maxValue: 700,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Values", (MinValue: (long)200, NextValue: (long)450, MaxValue: (long)700), GetValues(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 150,
					nextValue: 250,
					maxValue: 0,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Values", (MinValue: (long)150, NextValue: (long)250, MaxValue: (long)700), GetValues(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 100,
					nextValue: 0,
					maxValue: 600,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Values", (MinValue: (long)100, NextValue: (long)250, MaxValue: (long)600), GetValues(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 0,
					nextValue: 110,
					maxValue: 300,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Values", (MinValue: (long)100, NextValue: (long)110, MaxValue: (long)300), GetValues(connection, name, owner));

				NumberCacheController.SetValues(fountainContext, name, owner,
					minValue: 1,
					nextValue: 1,
					maxValue: 100,
					initMinValue, initNextValue, initMaxValue, initCanRollover, callInSameTransaction);

				AssertEquals("Values", (MinValue: (long)1, NextValue: (long)1, MaxValue: (long)100), GetValues(connection, name, owner));
			}
		}

		public void TestClosedConnection_GetMinAndMaxValues()
		{
			// Arrange
			IDbConnected connected = new BusinessObjectFactory();
			var internalDbConn = ((IDbConnectionInternals)connected.Connection).InternalDbConnection;

			// Act
			internalDbConn.Close();

			// Assert
			AssertEquals(ConnectionState.Closed, internalDbConn.State);
			AssertNoExceptionThrown(() => NumberCacheController.GetMinAndMaxValues(internalDbConn, null, "asdf", Guid.NewGuid()));
		}

		#region Implementation

		static void AddNameAndOwnerParams(DbCommand cmd, string name, Guid owner)
		{
			cmd.AddParameter("@name", SqlDbType.VarChar, 256, name);
			cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
		}

		static bool DoesFountainExist(DbConnection connection, string fountainName, Guid ownerPk)
		{
			var sqlText = "SELECT count(*) FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";
			var count = Convert.ToInt64(connection.ExecuteScalar(sqlText, cmd => AddNameAndOwnerParams(cmd, fountainName, ownerPk)));
			return count == 1;
		}

		static long GetMinCacheValue(DbConnection connection, string fountainName, Guid ownerPk)
		{
			string sqlText = "SELECT isnull(min(SG_Value),-1) FROM dbo.StmNumberCache WITH (READPAST, READCOMMITTEDLOCK) WHERE SG_SN = (SELECT SN_ID FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner) AND SG_IsUsed = 0";
			return Convert.ToInt64(connection.ExecuteScalar(sqlText, cmd => AddNameAndOwnerParams(cmd, fountainName, ownerPk)));
		}

		static long GetMaxCacheValue(DbConnection connection, string fountainName, Guid ownerPk)
		{
			var sqlText = "SELECT isnull(max(SG_Value),-1) FROM dbo.StmNumberCache WHERE SG_SN = (SELECT SN_ID FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner) AND SG_IsUsed = 0";
			return Convert.ToInt64(connection.ExecuteScalar(sqlText, cmd => AddNameAndOwnerParams(cmd, fountainName, ownerPk)));
		}

		static long GetNextCacheStartValue(DbConnection connection, string fountainName, Guid ownerPk)
		{
			var sqlText = "SELECT SN_Value FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner";
			return Convert.ToInt64(connection.ExecuteScalar(sqlText, cmd => AddNameAndOwnerParams(cmd, fountainName, ownerPk)));
		}

		static long[] GetCacheValues(DbConnection connection, string fountainName, Guid ownerPk)
		{
			var sqlText = "SELECT SG_Value FROM dbo.StmNumberCache WHERE SG_SN IN " +
										"(SELECT SN_Id FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner) AND SG_IsUsed = 0";
			var values = new List<long>();
			using (var cmd = connection.Command(sqlText))
			{
				AddNameAndOwnerParams(cmd, fountainName, ownerPk);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						values.Add(Convert.ToInt64(reader[0]));
					}
				}
			}
			
			return values.ToArray();
		}

		static int GetAvailableNumbersCount(DbConnection connection, string name, Guid owner)
		{
			return Convert.ToInt32(connection.ExecuteScalar(@"
SELECT
	cnt = COUNT(*)
FROM
	dbo.StmNums
	JOIN dbo.StmNumberCache WITH (READPAST, READCOMMITTEDLOCK) ON SG_SN = SN_Id
WHERE 1=1
	AND SN_Name = @name
	AND SN_Owner = @owner
	AND SG_IsUsed = 0

", cmd => AddNameAndOwnerParams(cmd, name, owner)
				));
		}

		static (long Min, long Next, long Max) GetValues(DbConnection connection, string name, Guid owner)
		{
			var minValue = 0L;
			var nextValue = 0L;
			var maxValue = 0L;

			connection.ExecuteReader("SELECT SN_MinimumValue, SN_Value, SN_MaximumValue FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner"
				, cmd => AddNameAndOwnerParams(cmd, name, owner)
				, (reader) =>
				{
					minValue = (long)reader["SN_MinimumValue"];
					nextValue = (long)reader["SN_Value"];
					maxValue = (long)reader["SN_MaximumValue"];
				});

			return (minValue, nextValue, maxValue);
		}

		static void CreateFountainAndInsertNumbers(DbConnection conn, string fountainName, Guid owner, int nextCacheValue, long minValue, long maxValue, bool isRollover, IReadOnlyList<long> cacheInts)
		{
			conn.BeginTransaction();

			var sqlText =
				$"INSERT dbo.StmNums (SN_Name, SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover) VALUES (@Name, @Owner, {nextCacheValue}, {minValue}, {maxValue}, @Rollover); " +
				"DECLARE @FountainId int = SCOPE_IDENTITY();" +
				"INSERT dbo.StmNumberCache (SG_SN, SG_Value) VALUES " +
				string.Join(",", cacheInts.Select(i => $"(@FountainId, {i})")) + ";";

			using (var command = conn.Command(sqlText))
			{
				command.AddParameter("@Name", SqlDbType.VarChar, fountainName);
				command.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				command.AddParameter("@Rollover", SqlDbType.Bit, isRollover);
				command.ExecuteNonQuery();
			}
			conn.CommitTransaction();

			NumberFountainTestBase.TestArrays("Test created cache", cacheInts, GetCacheValues(conn, fountainName, owner));
		}

		#endregion // Implementation
	}
}

#endif
