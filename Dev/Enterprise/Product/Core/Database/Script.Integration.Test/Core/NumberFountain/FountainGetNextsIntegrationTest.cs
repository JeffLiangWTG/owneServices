using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	class FountainGetNextsIntegrationTest : TransactionedTestCase
	{
		const string FountainName = "TestFountainForScripts";
		readonly Guid FountainOwner = Guid.Empty;

		public void TestSampleCallSafe_Direct()
		{
			var callInSameTransaction = true;
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			AssertNumbers("Next numbers"
				, new long[] { 1, 2 }
				, FountainGetNexts(TestConnection, name, owner, amount: 2, minValue: 1, maxValue: 10, canRollover: false, callInSameTransaction: callInSameTransaction));
		}

		public void TestSampleCallSafe_Loopback()
		{
			var callInSameTransaction = false;
			var name = FountainName;
			var owner = FountainOwner;

			AssertNumbers("Next numbers"
				, new long[] { 1, 2 }
				, FountainGetNexts(TestConnection, name, owner, amount: 2, minValue: 1, maxValue: 10, canRollover: false, callInSameTransaction: callInSameTransaction));
		}

		#region Implementation

		protected override void SetUp()
		{
			FountainsCleanup();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			FountainsCleanup();
		}

		protected void AssertNumbers(string message, IReadOnlyList<long> expected, IReadOnlyList<long> result)
		{
			AssertEquals(message, expected.Count, result.Count);
			for (var i = 0; i < expected.Count; i++)
			{
				AssertEquals(message, expected[i], result[i]);
			}
		}

		void FountainsCleanup()
		{
			Helper.DeleteFountain(TestConnection, FountainName, FountainOwner);
			DbCommitTracker.Ignore("/* FOUNTAIN CLEANUP */");
		}

		long[] FountainGetNexts(DbConnection connection, string name, Guid owner, int amount, long minValue = 1, long maxValue = 1, bool canRollover = false, bool callInSameTransaction = true)
		{
			var numbers = new List<long>();

			connection.ExecuteReader("FountainGetNexts"
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
