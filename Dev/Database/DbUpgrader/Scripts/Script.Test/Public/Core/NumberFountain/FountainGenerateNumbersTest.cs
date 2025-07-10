using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainGenerateNumbers))]
	class FountainGenerateNumbersTest : NumberFountainTestCase
	{
		public void TestNoRollover()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 199);
			AssertEquals("Fountain should exist on DB", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 40);
			AssertEquals("Available Numbers", 100, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 101, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 140);
			AssertEquals("Available Numbers", 199, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 200, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestNoRolloverGenerateOverflow()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 102, canRollover: false);
			AssertEquals("Fountain Id", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 1);
			AssertEquals("Available Numbers", 100, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next Value", 101, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 101);
			AssertEquals("Available Numbers", 102, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next Value", 103, Helper.GetValues(TestConnection, fountainId).NextValue);

			AssertNoExceptionThrown(() => Helper.GenerateNumbers_Direct(TestConnection, fountainId, 1));

			Helper.UseNumbers(TestConnection, fountainId, 0, 200);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				Helper.GenerateNumbers_Direct(TestConnection, fountainId, 1);
			});
			AssertContains("Fountain has reached its maximum value of 102. Cannot generate any more numbers.", ex.ToString(), ignoreCase: true);
		}

		public void TestCacheForMillion()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 1_500_000);
			AssertEquals("Fountain Id", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 2_000_000);
			AssertEquals("Available Numbers", 1_000_000, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next Value", 1_000_001, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 500_000);
			AssertEquals("Available Numbers", 1_500_000, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next Value", 1_500_001, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestUpdateTheWholeCache()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 1_500_000);
			AssertEquals("Fountain should exist on DB", fountainId, Helper.GetFountainId(TestConnection, name, owner));

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 1_000);
			Helper.UseNumbers(TestConnection, fountainId, 0, 2_000);
			AssertEquals("Available Numbers", 0, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next Value", 1_001, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 1);
			AssertEquals("Available Numbers", 1_000, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next Value", 2_001, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestRollover()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 200, canRollover: true);
			AssertEquals("Fountain should exist on DB", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 80);
			AssertEquals("Available Numbers", 100, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("NextValue", 101, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.UseNumbers(TestConnection, fountainId, 0, 500);
			AssertEquals("Available Numbers", 0, Helper.GetAvailableNumbersCount(TestConnection, fountainId));

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 40);
			AssertEquals("Available Numbers", 100, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("NextValue", 1, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 60);
			AssertEquals("Available Numbers", 200, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("NextValue", 101, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.UseNumbers(TestConnection, fountainId, 0, 500);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 40);
			AssertEquals("Available Numbers", 100, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("NextValue", 1, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.UseNumbers(TestConnection, fountainId, 0, 500);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 200);
			AssertEquals("Available Numbers", 200, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("NextValue", 1, Helper.GetValues(TestConnection, fountainId).NextValue);

			Helper.UseNumbers(TestConnection, fountainId, 0, 500);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 180);
			AssertEquals("Available Numbers", 200, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("NextValue", 1, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestRolloverGenerateMoreThanAvailable()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 100, canRollover: true);
			AssertEquals("Fountain should exist on DB", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);

			Helper.GenerateNumbers_Direct(TestConnection, fountainId, 100);
			AssertEquals("Cache size", 100, Helper.GetAvailableNumbersCount(TestConnection, fountainId));
			AssertEquals("Next value", 1, Helper.GetValues(TestConnection, fountainId).NextValue);

			AssertNoExceptionThrown(() => Helper.GenerateNumbers_Direct(TestConnection, fountainId, 101));
		}

		public void TestFountainNotFound_Direct()
		{
			var fountainId = Convert.ToInt32(Db.Connection.ExecuteScalar("SELECT FountainID = ISNULL((SELECT MAX(SN_ID) FROM dbo.StmNums) + 1, 0)"));
			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				Helper.GenerateNumbers_Direct(Db.Connection, fountainId, 1);
			});
			AssertContains($"Fountain is not found (ID: {fountainId})", ex.Message, ignoreCase: true);
		}
		public void TestFountainNotFound_Loopback()
		{
			var fountainId = Convert.ToInt32(Db.Connection.ExecuteScalar("SELECT FountainID = ISNULL((SELECT MAX(SN_ID) FROM dbo.StmNums) + 1, 0)"));
			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				Helper.GenerateNumbers_Loopback(Db.Connection, fountainId, 1);
			});
			AssertContains($"Fountain is not found (ID: {fountainId})", ex.Message, ignoreCase: true);
		}
	}

	[UseSnapshotProtection]
	class FountainGenerateNumbersExceptionTest : TestCase
	{
		public void TestFountainIsBlocked_Direct()
		{
			var name = "BlockedFountain";
			var owner = Guid.Empty;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(Db.Connection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(Db.Connection, name, owner, minValue: 1, nextValue: 1, maxValue: 999_999_999_999);

			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery($"UPDATE dbo.StmNums SET SN_CanRollover = SN_CanRollover WHERE SN_Name = '{name}' AND SN_Owner = '{owner}';");

				using (var newConnection = Db.NewExtraConnectionToMainDb())
				using (newConnection.BeginTransactionWithManager())
				{
					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						Helper.GenerateNumbers_Direct(newConnection, fountainId, 1);
					});
					AssertContains($"Fountain is blocked at the moment (ID: {fountainId}, Name: \"{name}\", Owner: {owner})", ex.Message, ignoreCase: true);

					AssertEquals("Still in open transaction", true, newConnection.IsInTransaction);
				}

				AssertEquals("Still in open transaction", true, Db.Connection.IsInTransaction);
			}
		}

		public void TestFountainIsBlocked_Loopback()
		{
			var name = "BlockedFountain";
			var owner = Guid.Empty;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(Db.Connection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(Db.Connection, name, owner, minValue: 1, nextValue: 1, maxValue: 999_999_999_999, canRollover: false);

			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery($"UPDATE dbo.StmNums SET SN_CanRollover = SN_CanRollover WHERE SN_Name = '{name}' AND SN_Owner = '{owner}';");

				var ex = AssertExceptionThrown<SqlException>(() =>
				{
					Helper.GenerateNumbers_Loopback(Db.Connection, fountainId, 1);
				});
				AssertContains($"Fountain is blocked at the moment (ID: {fountainId}, Name: \"{name}\", Owner: {owner})", ex.Message, ignoreCase: true);

				AssertEquals("Still in open transaction", true, Db.Connection.IsInTransaction);
			}
		}
	}
}

