using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Enterprise.Dat.Implementation.Preconditions
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	sealed class SqlServerMemoryChecker : IPrecondition
	{
		public SqlServerMemoryChecker()
		{
		}

		public struct MemoryStatus
		{
			public int Length;
			public int MemoryLoad;
			public ulong TotalPhysical;
			public ulong AvailablePhysical;
			public ulong TotalPageFile;
			public ulong AvailablePageFile;
			public ulong TotalVirtual;
			public ulong AvailableVirtual;
			public ulong AvailableExtendedVirtual;
		}

		public bool CheckPreconditionMet()
		{
			try
			{
				using (var connection = LocalDBConnection.GetConnection())
				{
					connection.Open();
					UpdateMaximumMemorySetting(connection);
					UpdateMinimumMemorySetting(connection);
				}
				return true;
			}
			catch (SqlException ex)
			{
				errorMessage = $"SQL Server as attempting to set {MinimumMemorySettingMB}MB and {MaximumValueForMaxMemoryMB}MB as memory settings. Exception: {ex}";
				return false;
			}
		}

		[DllImport("kernel32.dll")]
		public static extern void GlobalMemoryStatusEx(out MemoryStatus stat);

		static void UpdateMaximumMemorySetting(System.Data.Common.DbConnection connection)
		{
			var maximumSettingForMaxMemoryMB = MaximumValueForMaxMemoryMB;
			int currentMaxMemoryMB;
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "sp_configure 'show advanced options', 1";
				cmd.ExecuteNonQuery();
			}

			SqlServerTools.Reconfigure(connection);

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "exec sp_configure N'max server memory (MB)'";
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					currentMaxMemoryMB = (int)reader["run_value"];
				}
			}

			if (maximumSettingForMaxMemoryMB > currentMaxMemoryMB || currentMaxMemoryMB == 2147483647)
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "exec sp_configure N'max server memory (MB)', " + maximumSettingForMaxMemoryMB;
					cmd.ExecuteNonQuery();
				}
				SqlServerTools.Reconfigure(connection);
			}
		}

		void UpdateMinimumMemorySetting(System.Data.Common.DbConnection connection)
		{
			var minimumMemorySettingMB = MinimumMemorySettingMB;
			int currentMinimumMemoryMB;

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "exec sp_configure N'min server memory (MB)'";
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					currentMinimumMemoryMB = (int)reader["run_value"];
				}
			}

			if (minimumMemorySettingMB > currentMinimumMemoryMB)
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "exec sp_configure N'min server memory (MB)', " + minimumMemorySettingMB;
					cmd.ExecuteNonQuery();
				}

				SqlServerTools.Reconfigure(connection);
			}
		}

		internal static int MaximumValueForMaxMemoryMB
		{
			get
			{
				var stat = new MemoryStatus();
				stat.Length = 64;
				GlobalMemoryStatusEx(out stat);
				var ramBytes = (long)stat.TotalPhysical;
				return Math.Max(3072, Convert.ToInt32(ramBytes / 1048576L / 4L));
			}
		}

		internal long MinimumMemorySettingMB = 1024;

		string errorMessage;
		string IPrecondition.ErrorMessage => errorMessage;
	}
}
