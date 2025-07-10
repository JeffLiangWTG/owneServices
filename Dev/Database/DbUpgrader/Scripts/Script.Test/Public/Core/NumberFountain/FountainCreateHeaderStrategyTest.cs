using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainCreateHeaderStrategy))]
	class FountainCreateHeaderStrategyTest : NumberFountainTestCase
	{
		public void TestSampleCall_SameTransaction()
		{
			var callInSameTransaction = true;
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var fountainId = FountainCreateHeaderStrategy(TestConnection, name, owner, minValue: 34, nextValue: 52, maxValue: 9_999, canRollover: false, callInSameTransaction);
			AssertEquals("Fountain id", fountainId, Helper.GetFountainId(TestConnection, name, owner));
			AssertEquals("Values", (MinValue: (long)34, NextValue: (long)52, MaxValue: (long)9_999, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var sameFountainId = FountainCreateHeaderStrategy(TestConnection, name, owner, minValue: 1_098, nextValue: 1_111, maxValue: 2_000, canRollover: true, callInSameTransaction);
			AssertEquals("Fountain id", fountainId, sameFountainId);
			AssertEquals("Values", (MinValue: (long)34, NextValue: (long)52, MaxValue: (long)9_999, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);
		}

		public void TestSampleCall_NotSameTransaction()
		{
			var callInSameTransaction = false;
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var fountainId = FountainCreateHeaderStrategy(TestConnection, name, owner, minValue: 34, nextValue: 52, maxValue: 9_999, canRollover: false, callInSameTransaction);
			AssertEquals("Fountain id", fountainId, Helper.GetFountainId(TestConnection, name, owner));
			AssertEquals("Values", (MinValue: (long)34, NextValue: (long)52, MaxValue: (long)9_999, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var sameFountainId = FountainCreateHeaderStrategy(TestConnection, name, owner, minValue: 1_098, nextValue: 1_111, maxValue: 2_000, canRollover: true, callInSameTransaction);
			AssertEquals("Fountain id", fountainId, sameFountainId);
			AssertEquals("Values", (MinValue: (long)34, NextValue: (long)52, MaxValue: (long)9_999, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);
		}

		public void TestFountainNotFoundException()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var defaultTimeout = TestConnection.DefaultCommandTimeOutInSeconds;
			try
			{
				TestConnection.DefaultCommandTimeOutInSeconds = 15;

				using (var blocker = Db.NewExtraConnectionToMainDb())
				using (blocker.BeginTransactionWithManager())
				{
					blocker.ExecuteNonQuery("DELETE TOP (0) dbo.StmNums WITH (TABLOCKX);");

					var ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainCreateHeaderStrategy(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 100, canRollover: false, callInSameTransaction: false);
					});
					AssertContains($"Fountain with name [{name}] and owner [{owner}] is not found and cannot be created", ex.ToString(), ignoreCase: true);

					ex = AssertExceptionThrown<SqlException>(() =>
					{
						FountainCreateHeaderStrategy(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 100, canRollover: false, callInSameTransaction: true);
					});
					AssertContains("Execution Timeout Expired.", ex.ToString(), ignoreCase: true);
				}
			}
			finally
			{
				TestConnection.DefaultCommandTimeOutInSeconds = defaultTimeout;
			}
		}

		#region Implementation

		int FountainCreateHeaderStrategy(DbConnection connection, string fountainName, Guid owner, long minValue, long nextValue, long maxValue, bool canRollover, bool callInSameTransaction)
		{
			using (var cmd = connection.Command(ScriptToTest.Name))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, fountainName);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
				cmd.AddParameter("@NextValue", SqlDbType.BigInt, nextValue);
				cmd.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);
				cmd.AddParameter("@CanRollover", SqlDbType.Bit, canRollover);
				cmd.AddParameter("@CallInSameTransaction", SqlDbType.Bit, callInSameTransaction);

				return Convert.ToInt32(cmd.ExecuteProcedureWithReturnValue());
			}
		}
		#endregion // Implementation
	}
}

