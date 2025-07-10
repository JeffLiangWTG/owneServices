using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainSetValues))]
	class FountainSetValuesTest : NumberFountainTestCase
	{
		public void TestSampleCall()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 5, maxValue: 10);
			Helper.InsertNumbers(TestConnection, fountainId, 1, 2, 3, 4);
			AssertEquals("Fountain Id", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);
			AssertEquals("Values", (MinValue: (long)1, NextValue: (long)5, MaxValue: (long)10, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Available Numbers Count", 4, Helper.GetAvailableNumbersCount(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, minValue: 2);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)5, MaxValue: (long)10, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Available Numbers Count", 0, Helper.GetAvailableNumbersCount(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, nextValue: 6);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)6, MaxValue: (long)10, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, maxValue: 20);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)6, MaxValue: (long)20, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, minValue: 3, nextValue: 7);
			AssertEquals("Values", (MinValue: (long)3, NextValue: (long)7, MaxValue: (long)20, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, minValue: 2, maxValue: 30);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)7, MaxValue: (long)30, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, nextValue: 10, maxValue: 25);
			AssertEquals("Values", (MinValue: (long)2, NextValue: (long)10, MaxValue: (long)25, CanRollover: false), Helper.GetValues(TestConnection, fountainId));

			FountainSetValues(TestConnection, name, owner, fountainId, minValue: 1, nextValue: 1, maxValue: 100);
			AssertEquals("Values", (MinValue: (long)1, NextValue: (long)1, MaxValue: (long)100, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
		}

		public void TestLongFountainNameIsSupported()
		{
			var name = LongFountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 3, maxValue: 10);
			AssertEquals(3, Helper.GetValues(TestConnection, fountainId).NextValue);

			FountainSetValues(TestConnection, name, owner, fountainId, nextValue: 7);
			AssertEquals(7, Helper.GetValues(TestConnection, fountainId).NextValue);
		}

		public void TestException_FountainId()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId: -1);
			});
			AssertContains("Fountain Id must be greater than 0 (-1)", ex.ToString(), ignoreCase: true);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId: 0);
			});
			AssertContains("Fountain Id must be greater than 0 (0)", ex.ToString(), ignoreCase: true);
		}

		public void TestException_EmptyValues()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId: 1, minValue: 0, nextValue: 0, maxValue: 0);
			});
			AssertContains("At least one new value must be specified", ex.ToString(), ignoreCase: true);
		}

		[UseSnapshotProtection]
		public void TestException_FountainIsBlocked()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = 0;
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				fountainId = Helper.CreateFountain(connection, name, owner, minValue: 10, nextValue: 10, maxValue: 199);
			}

			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			using (var blocker = Db.NewExtraConnectionToMainDb())
			using (blocker.BeginTransactionWithManager())
			{
				blocker.ExecuteNonQuery($"UPDATE dbo.StmNums SET SN_Value = SN_Value WHERE SN_Name = '{name}' AND SN_Owner = '{owner}';");

				var ex = AssertExceptionThrown<SqlException>(() =>
				{
					FountainSetValues(TestConnection, name, owner, fountainId, nextValue: 1);
				});
				AssertContains($"Fountain is blocked at the moment (ID: {fountainId}, Name: \"{name}\", Owner: {owner})", ex.ToString(), ignoreCase: true);
			}
		}

		public void TestException_Outbounds()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 10, nextValue: 10, maxValue: 199, canRollover: false);
			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 1, maxValue: 100);
			});
			AssertContains("Proposed fountain values do not follow the rules: ", ex.ToString(), ignoreCase: true);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 110, maxValue: 100);
			});
			AssertContains("Proposed fountain values do not follow the rules: ", ex.ToString(), ignoreCase: true);

			AssertNoExceptionThrown(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 101, maxValue: 100);
			});
		}

		public void TestException_OutboundsRollover()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 10, nextValue: 10, maxValue: 199, canRollover: true);
			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 1, maxValue: 100);
			});
			AssertContains("Proposed fountain values do not follow the rules: ", ex.ToString(), ignoreCase: true);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 110, maxValue: 100);
			});
			AssertContains("Proposed fountain values do not follow the rules: ", ex.ToString(), ignoreCase: true);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 101, maxValue: 100);
			});
			AssertContains("Proposed fountain values do not follow the rules: ", ex.ToString(), ignoreCase: true);

			AssertNoExceptionThrown(() =>
			{
				FountainSetValues(TestConnection, name, owner, fountainId, minValue: 10, nextValue: 100, maxValue: 100);
			});
		}

		#region Implementation

		void FountainSetValues(DbConnection connection, string fountainName, Guid owner, int fountainId, long minValue = 0, long nextValue = 0, long maxValue = 0)
		{
			using (var cmd = connection.Command(ScriptToTest.Name))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, fountainName);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@FountainId", SqlDbType.Int, fountainId);
				cmd.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
				cmd.AddParameter("@NextValue", SqlDbType.BigInt, nextValue);
				cmd.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);

				cmd.ExecuteNonQuery();
			}
		}
		#endregion // Implementation
	}
}

