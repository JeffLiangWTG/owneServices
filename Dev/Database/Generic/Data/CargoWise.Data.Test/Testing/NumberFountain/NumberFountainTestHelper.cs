using System;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public static class NumberFountainTestHelper
	{
		public static int FountainNameMaxLength => 256;

		#region AppLockMode

		public enum AppLockMode
		{
			NoLock,
			IntentShared,
			Shared,
			Update,
			IntentExclusive,
			Exclusive,
			SharedIntentExclusive,
			UpdateIntentExclusive,
		}

		#endregion // AppLockMode

		public static int CreateFountain(DbConnection connection, string name, Guid owner, long minValue, long nextValue, long maxValue, bool canRollover = false)
		{
			const string sql = @"
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_MinimumValue, SN_Value, SN_MaximumValue, SN_CanRollover) VALUES
	(@Name, @Owner, @MinValue, @NextValue, @MaxValue, @CanRollover);
SELECT CONVERT(int, SCOPE_IDENTITY());
";

			return connection.ExecuteScalar<int>(sql,
				(cmd) =>
				{
					cmd.AddParameter("@Name", SqlDbType.VarChar, FountainNameMaxLength, name);
					cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
					cmd.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
					cmd.AddParameter("@NextValue", SqlDbType.BigInt, nextValue);
					cmd.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);
					cmd.AddParameter("@CanRollover", SqlDbType.Bit, canRollover);
				});
		}

		public static int? GetFountainId(DbConnection connection, string name, Guid owner)
		{
			return (int?)connection.ExecuteScalar(
				$"SELECT SN_ID FROM dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner;"
				, (cmd) =>
				{
					cmd.AddParameter("@name", SqlDbType.VarChar, FountainNameMaxLength, name);
					cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
				});
		}

		public static string GetFountainName(DbConnection connection, int fountainId)
		{
			return connection.ExecuteScalar<string>(
				$"SELECT SN_Name FROM dbo.StmNums WHERE SN_Id = @fountainId"
				, (cmd) =>
				{
					cmd.AddParameter("@fountainId", SqlDbType.Int, fountainId);
				});
		}

		public static (long MinValue, long NextValue, long MaxValue, bool CanRollover) GetValues(DbConnection connection, int fountainId)
		{
			var minValue = 0L;
			var nextValue = 0L;
			var maxValue = 0L;
			var canRollover = false;

			connection.ExecuteReader(
				$"SELECT SN_MinimumValue, SN_Value, SN_MaximumValue, SN_CanRollover FROM dbo.StmNums WHERE SN_Id = @fountainId"
				, (cmd) =>
				{
					cmd.AddParameter("@fountainId", SqlDbType.Int, fountainId);
				}
				, (reader) =>
				{
					minValue = (long)reader["SN_MinimumValue"];
					nextValue = (long)reader["SN_Value"];
					maxValue = (long)reader["SN_MaximumValue"];
					canRollover = (bool)reader["SN_CanRollover"];
				});

			return (MinValue: minValue, NextValue: nextValue, MaxValue: maxValue, CanRollover: canRollover);
		}

		public static void GenerateNumbers_Direct(DbConnection connection, int fountainId, int amount)
		{
			connection.ExecuteNonQuery("FountainGenerateNumbers"
				, (cmd) =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@FountainID", SqlDbType.Int, fountainId);
					cmd.AddParameter("@AmountRequested", SqlDbType.Int, amount);
				});
		}

		public static void GenerateNumbers_Loopback(DbConnection connection, int fountainId, int amount)
		{
			connection.ExecuteNonQuery("FountainGenerateNumbersAtLoopback"
				, (cmd) =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@fountainID", SqlDbType.Int, fountainId);
					cmd.AddParameter("@AmountRequested", SqlDbType.Int, amount);
				});
		}

		public static void InsertNumbers(DbConnection connection, int fountainId, params long[] numbers)
		{
			var sql = "INSERT dbo.StmNumberCache (SG_SN, SG_Value) VALUES "
				+ string.Join(",", numbers.Select(n => $"({fountainId}, {n})"));

			connection.ExecuteNonQuery(sql);
		}

		public static void UseNumbers(DbConnection connection, int fountainId, long startValue, int count)
		{
			var sql = @"
UPDATE dbo.StmNumberCache SET
	SG_IsUsed = 1
WHERE 1=1
	AND SG_SN = @fountainId
	AND SG_IsUsed = 0
	AND SG_Value BETWEEN @startValue AND @endValue
";
			connection.ExecuteNonQuery(sql
				, (cmd) =>
				{
					cmd.AddParameter("@fountainId", SqlDbType.Int, fountainId);
					cmd.AddParameter("@startValue", SqlDbType.BigInt, startValue);
					cmd.AddParameter("@endValue", SqlDbType.BigInt, startValue + count - 1);
				});
		}

		public static int GetAvailableNumbersCount(DbConnection connection, int fountainId)
		{
			return connection.ExecuteScalar<int>(
				$"SELECT COUNT(*) FROM dbo.StmNumberCache WHERE SG_SN = @fountainId AND SG_IsUsed = 0"
				, (cmd) =>
				{
					cmd.AddParameter("@fountainId", SqlDbType.Int, fountainId);
				});
		}

		public static void DeleteFountain(DbConnection connection, string name, Guid owner)
		{
			connection.ExecuteNonQuery(
				"/* FOUNTAIN CLEANUP */ DELETE dbo.StmNums WHERE SN_Name = @name AND SN_Owner = @owner"
				, (cmd) =>
				{
					cmd.AddParameter("@name", SqlDbType.VarChar, FountainNameMaxLength, name);
					cmd.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
				});
		}

		public static string GetAppLockResource(int fountainId)
		{
			return $"NumberFountain_{fountainId}";
		}

		public static string GetAppLock(DbConnection connection, int fountainId) => GetAppLock(connection, name: "", owner: null, fountainId: fountainId);
		public static string GetAppLock(DbConnection connection, string name, Guid? owner, int fountainId)
		{
			using (var cmd = connection.Command("FountainGetAppLock"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, FountainNameMaxLength, name);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner ?? Guid.Empty);
				cmd.AddParameter("@FountainId", SqlDbType.Int, fountainId);
				cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, null);
				cmd.GetParameter("@Resource").Direction = ParameterDirection.Output;

				cmd.ExecuteNonQuery();

				var resource = (string)cmd.GetParameterValue("@Resource");
				Assertion.AssertEquals(GetAppLockResource(fountainId), resource);

				return resource;
			}
		}

		public static void ReleaseAppLock(DbConnection connection, string resource) => ReleaseAppLock(connection, fountainId: 0, resource: resource);
		public static void ReleaseAppLock(DbConnection connection, int fountainId) => ReleaseAppLock(connection, fountainId: fountainId, resource: "");
		public static void ReleaseAppLock(DbConnection connection, string name, Guid owner) => ReleaseAppLock(connection, name, owner, fountainId: 0);
		static void ReleaseAppLock(DbConnection connection, string name = "", Guid? owner = null, int fountainId = 0, string resource = "")
		{
			connection.ExecuteNonQuery("FountainReleaseAppLock"
				, (cmd) =>
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Name", SqlDbType.VarChar, FountainNameMaxLength, name);
					cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner ?? Guid.Empty);
					cmd.AddParameter("@FountainId", SqlDbType.Int, fountainId);
					cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, resource);
				});
		}

		public static void AssertAppLockMode(DbConnection connection, int fountainId, AppLockMode expected) => AssertAppLockMode(connection, resource: GetAppLockResource(fountainId), expected: expected);
		public static void AssertAppLockMode(DbConnection connection, string resource, AppLockMode expected)
		{
			using (var cmd = connection.Command("SELECT APPLOCK_MODE('public', @Resource, 'Session');"))
			{
				cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, resource);

				Assertion.AssertEquals(expected.ToString(), cmd.ExecuteScalar());
			}
		}

		public static void AssertAppLockAvailable(DbConnection connection, int fountainId, bool expected) => AssertAppLockAvailable(connection, resource: GetAppLockResource(fountainId), expected: expected);
		public static void AssertAppLockAvailable(DbConnection connection, string resource, bool expected)
		{
			using (var cmd = connection.Command("SELECT CONVERT(bit, APPLOCK_TEST('public', @resource, 'Exclusive', 'Session'));"))
			{
				cmd.AddParameter("@Resource", SqlDbType.NVarChar, 255, resource);

				Assertion.AssertEquals(expected, cmd.ExecuteScalar());
			}
		}
	}
}
