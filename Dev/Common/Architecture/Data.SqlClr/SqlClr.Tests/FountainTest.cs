using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.Data.SqlClr.Testing
{
	class FountainTest : TestCase
	{
		public void TestDuplicateCallToCreateHeader()
		{
			var fountainId = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 1, nextValue: 1, maxValue: 100);
			AssertEquals("Fountain should exist", true, fountainId > 0);

			var fountainId2 = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 10, nextValue: 10, maxValue: 1000);
			AssertEquals("Fountain should match", fountainId, fountainId2);
		}

		public void TestFountainCreateHeaderAtLoopback()
		{
			var fountainId = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 7, nextValue: 9, maxValue: 99);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 7, expectedNextValue: 9, expectedMaxValue: 99);
		}

		public void TestFountainCreateHeaderAtLoopback_Blocked()
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery("DELETE TOP (0) dbo.StmNums WITH (TABLOCKX)");

				var fountainId = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 1, nextValue: 1, maxValue: 100);
				AssertEquals("Fountain Id", -1, fountainId);
			}
		}

		public void TestFountainGenerateNumbersAtLoopback()
		{
			var fountainId = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 50, nextValue: 50, maxValue: 100);

			FountainGenerateNumbersAtLoopback(fountainId, amount: 50);
			AssertCacheNumbers(fountainId, expectedAmount: 51);
		}

		public void TestFountainSetValuesAtLoopback()
		{
			var fountainId = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 50, nextValue: 50, maxValue: 100);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 50, expectedNextValue: 50, expectedMaxValue: 100);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, minValue: 1);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 1, expectedNextValue: 50, expectedMaxValue: 100);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, nextValue: 1);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 1, expectedNextValue: 1, expectedMaxValue: 100);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, maxValue: 50);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 1, expectedNextValue: 1, expectedMaxValue: 50);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, minValue: 10, nextValue: 20);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 10, expectedNextValue: 20, expectedMaxValue: 50);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, minValue: 11, maxValue: 200);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 11, expectedNextValue: 20, expectedMaxValue: 200);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, nextValue: 30, maxValue: 150);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 11, expectedNextValue: 30, expectedMaxValue: 150);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, minValue: 1, nextValue: 1, maxValue: 10);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 1, expectedNextValue: 1, expectedMaxValue: 10);
		}

		public void TestFunctionCall()
		{
			var fountainId = FountainCreateHeaderAtLoopback(FountainName, fountainOwner, minValue: 50, nextValue: 50, maxValue: 100);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 50, expectedNextValue: 50, expectedMaxValue: 100);
			AssertCacheNumbers(fountainId, expectedAmount: 0);

			FountainGenerateNumbersAtLoopback(fountainId, amount: 50);
			AssertCacheNumbers(fountainId, expectedAmount: 51);

			FountainSetValuesAtLoopback(FountainName, fountainOwner, fountainId, nextValue: 75);
			AssertValues(FountainName, fountainOwner, expectedId: fountainId, expectedMinValue: 50, expectedNextValue: 75, expectedMaxValue: 100);
		}

		#region Implementation

		static int FountainCreateHeaderAtLoopback(string name, Guid owner, long minValue, long nextValue, long maxValue, bool canRollover = false)
		{
			using (var cmd = Db.Connection.Command("FountainCreateHeaderAtLoopback"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@name", SqlDbType.VarChar, name);
				cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@minValue", SqlDbType.BigInt, minValue);
				cmd.AddParameter("@nextValue", SqlDbType.BigInt, nextValue);
				cmd.AddParameter("@maxValue", SqlDbType.BigInt, maxValue);
				cmd.AddParameter("@canRollover", SqlDbType.Bit, canRollover);

				return Convert.ToInt32(cmd.ExecuteProcedureWithReturnValue());
			}
		}

		static void FountainGenerateNumbersAtLoopback(int fountainId, int amount)
		{
			using (var cmd = Db.Connection.Command("FountainGenerateNumbersAtLoopback"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FountainId", SqlDbType.Int, fountainId);
				cmd.AddParameter("@AmountRequested", SqlDbType.Int, amount);

				cmd.ExecuteNonQuery();
			}
		}

		static void FountainSetValuesAtLoopback(string name, Guid owner, int fountainId, long minValue = 0, long nextValue = 0, long maxValue = 0)
		{
			using (var cmd = Db.Connection.Command("FountainSetValuesAtLoopback"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@name", SqlDbType.VarChar, name);
				cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
				cmd.AddParameter("@fountainId", SqlDbType.Int, fountainId);
				cmd.AddParameter("@minValue", SqlDbType.BigInt, minValue);
				cmd.AddParameter("@nextValue", SqlDbType.BigInt, nextValue);
				cmd.AddParameter("@maxValue", SqlDbType.BigInt, maxValue);

				cmd.ExecuteNonQuery();
			}
		}

		static void AssertCacheNumbers(int fountainId, int expectedAmount)
		{
			using (var command = Db.Connection.Command("SELECT COUNT(*) FROM dbo.StmNumberCache WHERE SG_SN = @fountainId"))
			{
				command.AddParameter("@fountainId", SqlDbType.Int, fountainId);
				AssertEquals("Cache size", expectedAmount, command.ExecuteScalar());
			}
		}

		static void AssertValues(string name, Guid owner, int expectedId = 0, long expectedMinValue = 0, long expectedNextValue = 0, long expectedMaxValue = 0)
		{
			var totalCount = 0;
			var fountainId = 0;
			var minValue = 0L;
			var nextValue = 0L;
			var maxValue = 0L;

			Db.Connection.ExecuteReader(
				"SELECT SN_Id, SN_MinimumValue, SN_Value, SN_MaximumValue FROM dbo.StmNums WHERE SN_Name = @Name AND SN_Owner = @Owner"
				, (cmd) =>
				{
					cmd.AddParameter("@Name", SqlDbType.VarChar, name);
					cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				}
				, (reader) =>
				{
					totalCount += 1;
					fountainId = Convert.ToInt32(reader["SN_Id"]);
					minValue = Convert.ToInt64(reader["SN_MinimumValue"]);
					nextValue = Convert.ToInt64(reader["SN_Value"]);
					maxValue = Convert.ToInt64(reader["SN_MaximumValue"]);
				});

			CombineAssertions(() =>
			{
				AssertEquals("Fountains count", 1, totalCount);

				if (expectedId > 0)
				{
					AssertEquals("fountainId", expectedId, fountainId);
				}

				if (expectedMinValue > 0)
				{
					AssertEquals("minValue", expectedMinValue, minValue);
				}

				if (expectedNextValue > 0)
				{
					AssertEquals("nextValue", expectedNextValue, nextValue);
				}

				if (expectedMaxValue > 0)
				{
					AssertEquals("maxValue", expectedMaxValue, maxValue);
				}
			});
		}

		const string FountainName = "FountainName";
		readonly Guid fountainOwner = Guid.Parse("EB1A3D0A-089B-4D23-0000-7AB523707D5D");

		protected override void TearDown()
		{
			Db.Connection.ExecuteNonQuery($"/* CLEANUP FOUNTAIN */ DELETE dbo.StmNums WHERE SN_Name = '{FountainName}' AND SN_Owner = '{fountainOwner}';");
			DbCommitTracker.Ignore("/* CLEANUP FOUNTAIN */");

			base.TearDown();
		}

		#endregion // Implementation
	}
}
