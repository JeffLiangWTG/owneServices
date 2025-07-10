using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainCreateHeader))]
	class FountainCreateHeaderTest : NumberFountainTestCase
	{
		public void TestSampleCall()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 34, nextValue: 52, maxValue: 100, canRollover: false);
			AssertEquals("Fountain id", fountainId, Helper.GetFountainId(TestConnection, name, owner));
			AssertEquals("Values", (MinValue: (long)34, NextValue: (long)52, MaxValue: (long)100, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var sameFountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 1_098, nextValue: 1_111, maxValue: 2_000, canRollover: true);
			AssertEquals("Fountain id", fountainId, sameFountainId);
			AssertEquals("Values", (MinValue: (long)34, NextValue: (long)52, MaxValue: (long)100, CanRollover: false), Helper.GetValues(TestConnection, fountainId));
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);
		}

		public void TestLongFountainNameIsSupported()
		{
			var name = LongFountainName;
			var owner = FountainOwner;

			var fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 34, nextValue: 52, maxValue: 100);
			AssertEquals(name, Helper.GetFountainName(TestConnection, fountainId));
		}

		public void TestWrongValues()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			var fountainId = 0;

			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 0, nextValue: 1, maxValue: 100, canRollover: false);
			});
			AssertContains("Proposed fountain values do not follow the rules: 0 < MinValue <= NextValue <= MaxValue(+1 for non-rollover)", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("FountainId", 0, fountainId);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 10, nextValue: 1, maxValue: 100, canRollover: false);
			});
			AssertContains("Proposed fountain values do not follow the rules: 0 < MinValue <= NextValue <= MaxValue(+1 for non-rollover)", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("FountainId", 0, fountainId);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 10, nextValue: 110, maxValue: 100, canRollover: false);
			});
			AssertContains("Proposed fountain values do not follow the rules: 0 < MinValue <= NextValue <= MaxValue(+1 for non-rollover)", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("FountainId", 0, fountainId);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 10, nextValue: 101, maxValue: 100, canRollover: true);
			});
			AssertContains("Proposed fountain values do not follow the rules: 0 < MinValue <= NextValue <= MaxValue(+1 for non-rollover)", ex.ToString(), ignoreCase: true);
			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("FountainId", 0, fountainId);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);

			AssertNoExceptionThrown(() =>
			{
				fountainId = FountainCreateHeader(TestConnection, name, owner, minValue: 10, nextValue: 101, maxValue: 100, canRollover: false);
			});
			AssertEquals("Fountain exists?", true, Helper.GetFountainId(TestConnection, name, owner).HasValue);
			AssertEquals("FountainId", true, fountainId > 0);
			AssertEquals("Is In Transaction?", true, TestConnection.IsInTransaction);
		}

		#region Implementation

		int FountainCreateHeader(DbConnection connection, string name, Guid owner, long minValue, long nextValue, long maxValue, bool canRollover = false)
		{
			using (var cmd = connection.Command(ScriptToTest.Name))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, name);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
				cmd.AddParameter("@NextValue", SqlDbType.BigInt, nextValue);
				cmd.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);
				cmd.AddParameter("@CanRollover", SqlDbType.Bit, canRollover);

				return Convert.ToInt32(cmd.ExecuteProcedureWithReturnValue());
			}
		}
		#endregion // Implementation
	}
}

