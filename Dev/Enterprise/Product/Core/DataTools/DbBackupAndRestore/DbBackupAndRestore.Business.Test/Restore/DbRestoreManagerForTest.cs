using System;
using System.Collections.Generic;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Restore
{
	public class DbRestoreManagerForTest : IDbRestoreManager
	{
		public DatabaseStatus DbStatus { get; set; } = DatabaseStatus.Online;
		public bool IsPrimaryReplica { get; set; } = true;

		public readonly List<string> InfoMessages = new List<string>();

		public void RefreshExtendedProperties(string dbServer)
		{
			// do nothing
		}

		public void LogMessage(string msg)
		{
			InfoMessages.Add(msg);
		}

		public DbFileInfoCollection GetBackupDbFileInfoCollection(string dbServer, string backupFilePath, string auditServer, string auditBackupFilePath, string dwServer, string edwBackupFilePath)
		{
			return new DbFileInfoCollection();
		}

		public DbFileInfoCollection ApplyExtendedProperties(DbFileInfoCollection dbFiles, out bool applyForAllFiles)
		{
			applyForAllFiles = false;
			return dbFiles;
		}

		public void RestoreDatabases(DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
		{
			throw new NotImplementedException();
		}

		public string GetAuditServer(string serverName, string targetDbName)
		{
			return serverName;
		}

		public string GetDataWarehouseServer(string serverName, string targetDbName)
		{
			return serverName;
		}

		public List<string> GetAvailabilityGroupsList(string dbServer)
		{
			return new List<string> { };
		}

		public string GetAvailabilityGroupName(string dbServer, string dbName)
		{
			return "testGroup";
		}

		public string GetPrimaryReplicaOnAvailabilityGroup(string dbServer, string groupName)
		{
			return IsPrimaryReplica ? dbServer : string.Empty;
		}

		public bool IsPrimaryReplicaOnAvailabilityGroup(string dbServer, string groupName)
		{
			return IsPrimaryReplica;
		}

		public DatabaseStatus GetDatabaseStatus(string serverName, string targetDbName)
		{
			return DbStatus;
		}

		public bool IsServerAvailabilityGroupListener(string dbServer)
		{
			return false;
		}

		public bool IsDbPartOfAlwaysOn(string dbServer, string dbName)
		{
			return true;
		}
	}
}
