using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Preconditions.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	sealed class SqlServerMemoryCheckerTest : TestCase
	{
		public void TestCheckPreconditionMet_LowMinimumDefaultMax()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				EnableAdvancedOptions(connection);
				GetCurrentMemoryValues(connection, out var currentMinMemoryMB, out var currentMaxMemoryMB);
				try
				{
					SetMemoryValues(connection, MinimumMemorySettingMB / 2, 2147483647);
					var expectedMaxMemoryMB = SqlServerMemoryChecker.MaximumValueForMaxMemoryMB;
					var sqlServerMemoryChecker = new SqlServerMemoryChecker();
					sqlServerMemoryChecker.CheckPreconditionMet();
					GetCurrentMemoryValues(connection, out var updatedMinMemoryMB, out var updatedMaxMemoryMB);
					CombineAssertions(() =>
					{
						AssertEquals("Min Memory", updatedMinMemoryMB, 1024);
						AssertEquals("Max Memory", updatedMaxMemoryMB, expectedMaxMemoryMB);
					});
				}
				finally
				{
					SetMemoryValues(connection, currentMinMemoryMB, currentMaxMemoryMB);
				}
			}
		}

		public void TestCheckPreconditionMet_LowMaximum()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				EnableAdvancedOptions(connection);
				GetCurrentMemoryValues(connection, out var currentMinMemoryMB, out var currentMaxMemoryMB);
				try
				{
					SetMemoryValues(connection, MinimumMemorySettingMB, 2048);
					var expectedMaxMemoryMB = SqlServerMemoryChecker.MaximumValueForMaxMemoryMB;
					var sqlServerMemoryChecker = new SqlServerMemoryChecker();
					sqlServerMemoryChecker.CheckPreconditionMet();
					GetCurrentMemoryValues(connection, out var updatedMinMemoryMB, out var updatedMaxMemoryMB);
					AssertEquals("Max Memory", updatedMaxMemoryMB, expectedMaxMemoryMB);
				}
				finally
				{
					SetMemoryValues(connection, currentMinMemoryMB, currentMaxMemoryMB);
				}
			}
		}

		public void TestCheckPreconditionMet_HighMaximum()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				EnableAdvancedOptions(connection);
				GetCurrentMemoryValues(connection, out var currentMinMemoryMB, out var currentMaxMemoryMB);
				try
				{
					var maxMemoryMB = SqlServerMemoryChecker.MaximumValueForMaxMemoryMB + 512;
					SetMemoryValues(connection, MinimumMemorySettingMB, maxMemoryMB);
					var sqlServerMemoryChecker = new SqlServerMemoryChecker();
					sqlServerMemoryChecker.CheckPreconditionMet();
					GetCurrentMemoryValues(connection, out var updatedMinMemoryMB, out var updatedMaxMemoryMB);
					AssertEquals("Max Memory", updatedMaxMemoryMB, maxMemoryMB);
				}
				finally
				{
					SetMemoryValues(connection, currentMinMemoryMB, currentMaxMemoryMB);
				}
			}
		}

		public void TestErrorMessage()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				EnableAdvancedOptions(connection);
				GetCurrentMemoryValues(connection, out var currentMinMemoryMB, out var currentMaxMemoryMB);
				try
				{
					var sqlServerMemoryChecker = new SqlServerMemoryChecker();
					sqlServerMemoryChecker.MinimumMemorySettingMB = 999999999999999999;
					sqlServerMemoryChecker.CheckPreconditionMet();
#if NETFRAMEWORK
					AssertStartsWith("Settings and Exception", $"SQL Server as attempting to set 999999999999999999MB and {SqlServerMemoryChecker.MaximumValueForMaxMemoryMB}MB as memory settings. Exception: System.Data.SqlClient.SqlException (0x80131904): Error converting data type numeric to int.", ((IPrecondition)sqlServerMemoryChecker).ErrorMessage);
#else
					AssertStartsWith("Settings and Exception", $"SQL Server as attempting to set 999999999999999999MB and {SqlServerMemoryChecker.MaximumValueForMaxMemoryMB}MB as memory settings. Exception: Microsoft.Data.SqlClient.SqlException (0x80131904): Error converting data type numeric to int.", ((IPrecondition)sqlServerMemoryChecker).ErrorMessage);
#endif
				}
				finally
				{
					SetMemoryValues(connection, currentMinMemoryMB, currentMaxMemoryMB);
				}
			}
		}

		void EnableAdvancedOptions(SqlConnection connection)
		{
			using (var cmd = new SqlCommand("sp_configure 'show advanced options', 1", connection))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void GetCurrentMemoryValues(SqlConnection connection, out int configuredMinMemoryMB, out int configuredMaxMemoryMB)
		{
			using (var cmd = new SqlCommand("exec sp_configure N'min server memory (MB)'", connection))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				configuredMinMemoryMB = (int)reader["run_value"];
			}
			using (var cmd = new SqlCommand("exec sp_configure N'max server memory (MB)'", connection))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				configuredMaxMemoryMB = (int)reader["run_value"];
			}
		}

		void SetMemoryValues(SqlConnection connection, int minMemoryMB, int maxMemoryMB)
		{
			using (var cmd = new SqlCommand("exec sp_configure N'min server memory (MB)', " + minMemoryMB, connection))
			{
				cmd.ExecuteNonQuery();
			}

			using (var cmd = new SqlCommand("exec sp_configure N'max server memory (MB)', " + maxMemoryMB, connection))
			{
				cmd.ExecuteNonQuery();
			}
		}

		const int MinimumMemorySettingMB = 1024;
	}
}
