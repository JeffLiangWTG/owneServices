using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.Data.SqlServer
{
	public static partial class AlwaysOn
	{
		public static bool IsDbPartOfAlwaysOn(DbConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

#if DEBUG
			if (IsDbPartOfAlwaysOn_ForTest.Value.HasValue)
			{
				return IsDbPartOfAlwaysOn_ForTest.Value.Value;
			}
			else if (AlwaysOnDatabases_ForTest.Value != null)
			{
				return AlwaysOnDatabases_ForTest.Value.Contains(dbName, StringComparer.OrdinalIgnoreCase);
			}
#endif

			const string sql = "SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL FROM sys.databases WHERE name = @dbName AND group_database_id IS NOT NULL) THEN 1 ELSE 0 END)"; // this is a sql query
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				var result = cmd.ExecuteScalar();
				return result != null && (bool)result;
			}
		}

		public static bool IsDbOnPrimaryReplica(DbConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var adminConnection = connection as AdminConnection;

			if (adminConnection == null)
			{
				using (var newAdminConnection = Db.NewAdminConnection(connection.ServerName, Db.SqlMasterDb))
				{
					return IsPrimaryReplicaDb(newAdminConnection, dbName);
				}
			}
			else
			{
				return IsPrimaryReplicaDb(adminConnection, dbName);
			}
		}

		static bool IsPrimaryReplicaDb(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			using (var cmd = connection.Command("select sys.fn_hadr_is_primary_replica(@dbName)"))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				var result = cmd.ExecuteScalar();
				return result != DBNull.Value && (bool)result;
			}
		}

		public static bool RemoveDatabaseFromAlwaysOnSetup(string dbName)
		{
			using var conn = Db.NewAdminConnection(Db.SqlMasterDb);
			return RemoveDatabaseFromAlwaysOnSetup(conn, dbName);
		}

		public static bool RemoveDatabaseFromAlwaysOnSetup(AdminConnection conn, string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
#if DEBUG
			var testActionResult = RemoveDatabaseFromAlwaysOnSetup_ForTest(dbName);
			if (testActionResult != null)
			{
				return (bool)testActionResult;
			}
#endif

			const string sqlScript = @"
Declare @ReplicaID uniqueidentifier = (Select replica_id from sys.databases Where name = @dbName);
Declare @GroupID uniqueidentifier = (Select group_id From sys.availability_replicas Where replica_id = @ReplicaID);
Declare @GroupName varchar(max) = (Select name From sys.availability_groups Where group_id = @GroupID);
If @GroupName IS NOT NULL
Begin
   Declare @SQL varchar(max) = 'Alter Availability Group [' + @GroupName + '] Remove Database [{0}]';
   EXEC (@SQL);
End";

			var finalSql = string.Format(sqlScript, dbName);
			using (var cmd = conn.Command(finalSql))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				cmd.ExecuteNonQuery();
			}
			return !IsDbPartOfAlwaysOn(conn, dbName);
		}

		public static List<string> GetAlwaysOnSecondaryReplicaNamesList(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				return GetAlwaysOnSecondaryReplicaNamesListOnMasterDb(connection, dbName);
			}
		}

		public static List<string> GetAlwaysOnSecondaryReplicaNamesList(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				return GetAlwaysOnSecondaryReplicaNamesListOnMasterDb(connection, dbName);
			}
		}

		static List<string> GetAlwaysOnSecondaryReplicaNamesListOnMasterDb(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var serverFullName = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(connection.ServerNameReportedByDatabase);
			return GetAlwaysOnReplicaInfosOnMasterDb(connection, dbName)
				.Select(info => info.ReplicaServerName)
				.Where(name => string.Compare(name, serverFullName, StringComparison.OrdinalIgnoreCase) != 0)
				.ToList();
		}

		public static List<AlwaysOnReplicaInfo> GetAlwaysOnReplicaInfos(AdminConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				return GetAlwaysOnReplicaInfosOnMasterDb(connection, dbName);
			}
		}

		static List<AlwaysOnReplicaInfo> GetAlwaysOnReplicaInfosOnMasterDb(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			RunUserAction_ForTest(connection);

#if DEBUG
			if (ReplicaNames_ForTest.Value != null)
			{
				return ReplicaNames_ForTest.Value;
			}
#endif

			var sqlText = FormattableString.Invariant($@"
SELECT
	Reps.replica_server_name,
	Reps.availability_mode
FROM
	sys.dm_hadr_database_replica_states AS DB_Rep_Sts
	JOIN sys.availability_replicas      AS Reps       ON Reps.replica_id = DB_Rep_Sts.replica_id
WHERE
	DB_Rep_Sts.database_id = DB_ID(@dbName)
");

			var result = new List<AlwaysOnReplicaInfo>();
			connection.ExecuteReader(
				sqlText,
				cmd => cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName),
				reader => result.Add(new AlwaysOnReplicaInfo
				{
					ReplicaServerName = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(Convert.ToString(reader["replica_server_name"], CultureInfo.InvariantCulture)),
					AvailabilityMode = Convert.ToInt32(reader["availability_mode"])
				}));

			return result;
		}

		#region Implementation

		static partial void RunUserAction_ForTest(AdminConnection connection);

		#endregion // Implementation
	}
}

#region Test
#if DEBUG

#region Partial class

namespace CargoWise.Data.SqlServer
{
	using System.Diagnostics.CodeAnalysis;

	public static partial class AlwaysOn
	{
		public static Overridable<Func<string, bool>> OverridableRemoveDatabaseFromAlwaysOnSetupForTest = new Overridable<Func<string, bool>>(null);

		static bool? RemoveDatabaseFromAlwaysOnSetup_ForTest(string dbName)
		{
			return OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value?.Invoke(dbName);
		}

		[SuppressMessage("Microsoft.Design", "CA1006: DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static LazyOverridable<bool?> IsDbPartOfAlwaysOn_ForTest = new LazyOverridable<bool?>(() => null);

		[SuppressMessage("Microsoft.Design", "CA1006: DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static LazyOverridable<List<string>> AlwaysOnDatabases_ForTest = new LazyOverridable<List<string>>(() => null);

		[SuppressMessage("Microsoft.Design", "CA1006: DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static LazyOverridable<List<AlwaysOnReplicaInfo>> ReplicaNames_ForTest = new LazyOverridable<List<AlwaysOnReplicaInfo>>(() => null);

		public static LazyOverridable<Action<AdminConnection>> UserAction_ForTest = new LazyOverridable<Action<AdminConnection>>(() => null);

		static partial void RunUserAction_ForTest(AdminConnection connection)
		{
			UserAction_ForTest.Value?.Invoke(connection);
		}
	}
}

#endregion // Partial class

#endif
#endregion
