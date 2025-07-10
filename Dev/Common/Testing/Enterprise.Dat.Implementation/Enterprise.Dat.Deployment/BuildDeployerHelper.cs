using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using CargoWise.Data;

namespace Enterprise.Dat.Implementation
{
	class BuildDeployerHelper
	{
		public static string GetSqlServerDns(DbConnection connection)
		{
			// get ip
			var ipAddress = connection.ExecuteScalar<string>("SELECT CONNECTIONPROPERTY('local_net_address')");

			if (string.IsNullOrEmpty(ipAddress))
			{
				throw new InvalidOperationException($"Connection should use TCP protocol (SPID: {connection.SPID}). Please also check that the TCP protocol is enabled on server '{connection.ServerName}'");
			}

			// get dns name
			return  Dns.GetHostEntry(IPAddress.Parse(ipAddress)).HostName;
		}

		public static int GetSqlServerHadrPort(DbConnection connection)
		{
			return connection.ExecuteScalar<int>("SELECT port FROM sys.tcp_endpoints WHERE TYPE_DESC = 'DATABASE_MIRRORING'");
		}

		public static string GetServerName(DbConnection connection)
		{
			return connection.ExecuteScalar<string>("SELECT @@SERVERNAME");
		}

		public static void DropBackupUsingDumpDevice(DbConnection connection, string logicalName, string physicalName)
		{
			// LLZ: this method to drop backups was chosen based on
			// https://learn.microsoft.com/en-us/sql/database-engine/database-mirroring/the-database-mirroring-endpoint-sql-server?view=sql-server-ver16
			// this is a documented method, and in addition, it allows dropping all backups for the database disregarding the location

			connection.ExecuteNonQuery(
				$@"
IF EXISTS (SELECT null FROM sys.backup_devices WHERE type_desc = 'DISK' AND name = @logicalname)
	EXEC sys.sp_dropdevice
		@logicalname,
		'DELFILE'
EXEC sys.sp_addumpdevice
	'DISK',
	@logicalname,
	@physicalname
EXEC sys.sp_dropdevice
	@logicalname,
	'DELFILE'
",
				cmd =>
				{
					cmd.AddParameter("logicalname", SqlDbType.NVarChar, 128, logicalName);
					cmd.AddParameter("physicalname", SqlDbType.NVarChar, 260, physicalName);
				});
		}

		#region backup files handling

		public struct BackupFileInfo
		{
			public BackupFileInfo(string fileName, string dbName)
			{
				FileName = fileName;
				DatabaseName = dbName;
			}

			public string FileName { get; }

			public string DatabaseName { get; }
		}
		public static IEnumerable<BackupFileInfo> GetBackupsToDrop(DbConnection connection, string mainDbName, string singleRefDbName, string availabilityGroupName)
		{
			var backupsToDrop = new List<BackupFileInfo>();
			var sharedAvailabilityGroupRefDbPrefix = RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix;

			var getRelatedBackupsSql = $@"
SELECT DISTINCT bs.database_name, bmf.physical_device_name
FROM msdb.dbo.backupmediafamily bmf
	JOIN  msdb.dbo.backupset bs ON bs.media_set_id = bmf.media_set_id
WHERE 1=2
	OR bs.database_name = @dbName
	OR bs.database_name LIKE CONCAT(@dbName, '[_]%')
";

			if (!string.IsNullOrEmpty(availabilityGroupName))
			{
				getRelatedBackupsSql = $@"
{getRelatedBackupsSql}
	OR bs.database_name LIKE CONCAT(@sharedAvailabilityGroupRefDbPrefix, '-%')
";
			}

			if (!string.IsNullOrEmpty(singleRefDbName))
			{
				getRelatedBackupsSql = $@"
{getRelatedBackupsSql}
OR (1 = 1
	AND bs.database_name = @singleRefDbName
	AND bs.database_name <> 'CW-RefDatabase'
)";
			}

			connection.ExecuteReader(
				getRelatedBackupsSql,
				cmd =>
				{
					cmd.AddParameter("dbName", SqlDbType.VarChar, DataUtils.ReplaceSqlLikeWildcard(mainDbName));

					if (!string.IsNullOrWhiteSpace(availabilityGroupName))
					{
						cmd.AddParameter("sharedAvailabilityGroupRefDbPrefix", SqlDbType.VarChar, DataUtils.ReplaceSqlLikeWildcard(sharedAvailabilityGroupRefDbPrefix + availabilityGroupName));
					}

					if (!string.IsNullOrEmpty(singleRefDbName))
					{
						cmd.AddParameter("singleRefDbName", SqlDbType.VarChar, singleRefDbName);
					}
				},
				record =>
				{
					backupsToDrop.Add(new BackupFileInfo((string)record["physical_device_name"], (string)record["database_name"]));
				});

			return backupsToDrop;
		}

		public static void DropBackupHistoryForDatabase(DbConnection connection, string database)
		{
			connection.ExecuteNonQuery(
				@"EXEC msdb.dbo.sp_delete_database_backuphistory @dbName",
				cmd => cmd.AddParameter("dbName", SqlDbType.NVarChar, 128, database));
		}

		#endregion backup files handling

	}
}
