using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public interface IAlwaysOnHelper
	{
		List<string> GetAvailabilityGroups(AdminConnection connection);
		bool IsServerAvailabilityGroupListener(AdminConnection connection);
		bool IsPrimaryReplicaOnAvailabilityGroup(AdminConnection connection, string groupName);
		string GetPrimaryReplicaOnAvailabilityGroup(AdminConnection connection, string groupName);
		bool IsDbPartOfAlwaysOn(DbConnection connection, string dbName);
		List<string> GetReplicaServersWithDatabase(AdminConnection connection, string dbName, string groupName);
		List<string> GetSecondaryReplicaNamesList(AdminConnection connection, string dbName);
		List<string> GetSecondaryReplicaNamesListForAvailabilityGroup(AdminConnection connection, string groupName);
		string GetAvailabilityGroupName(AdminConnection connection, string dbName);
		bool RemoveDatabaseFromAvailabilityGroup(AdminConnection connection, string dbName, string groupName);
		bool AddDatabaseToAvailabilityGroup(AdminConnection connection, string dbName, string groupName);
		bool JoinSecondaryDatabaseToAvailabilityGroup(AdminConnection connection, string dbName, string groupName);
	}

	public class AlwaysOnHelper : IAlwaysOnHelper
	{
		public List<string> GetAvailabilityGroups(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			const string sqlScript = @"
				SELECT 
					ag.name AS AvailabilityGroupName
				FROM 
					sys.availability_groups AS ag
				ORDER BY
					ag.name ASC;";

			var availabilityGroups = new List<string>();
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteReader(
					sqlScript,
					record => availabilityGroups.Add(record[0].ToString()));
			}

			return availabilityGroups;
		}

		public bool IsServerAvailabilityGroupListener(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			const string sqlScript = @"
				SELECT 
					ag.name AS AvailabilityGroup
				FROM 
					sys.availability_groups ag
				JOIN 
					sys.availability_group_listeners agl ON ag.group_id = agl.group_id
				JOIN 
					sys.availability_group_listener_ip_addresses aglip ON agl.listener_id = aglip.listener_id
				WHERE 
					aglip.ip_address = CONNECTIONPROPERTY('local_net_address')";

			using (DbCommand dbCommand = connection.Command(sqlScript))
			{
				return dbCommand.ExecuteScalar() != null;
			}
		}

		public bool IsPrimaryReplicaOnAvailabilityGroup(AdminConnection connection, string groupName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			const string sqlScript = @"
				SELECT 
					1
				FROM 
					sys.dm_hadr_availability_replica_states AS ReplicaStates
				JOIN 
					sys.availability_replicas AS REPS ON ReplicaStates.replica_id = REPS.replica_id
				JOIN 
					sys.availability_groups AS ag ON REPS.group_id = ag.group_id
				WHERE 
					ag.name = @groupName AND ReplicaStates.is_local = 1 AND ReplicaStates.role = 1";

			using (DbCommand dbCommand = connection.Command(sqlScript))
			{
				dbCommand.AddParameter("@groupName", SqlDbType.NVarChar, groupName);

				return dbCommand.ExecuteScalar() != null;
			}
		}

		public string GetPrimaryReplicaOnAvailabilityGroup(AdminConnection connection, string groupName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			const string sqlScript = @"
				SELECT
					Reps.replica_server_name
				FROM
					sys.availability_groups AS AG
					JOIN sys.availability_replicas AS Reps ON AG.group_id = Reps.group_id
					JOIN sys.dm_hadr_availability_replica_states AS ReplicaStates 
						ON Reps.replica_id = ReplicaStates.replica_id
				WHERE
					AG.name = @groupName
					AND ReplicaStates.role = 1";

			using (DbCommand dbCommand = connection.Command(sqlScript))
			{
				dbCommand.AddParameter("@groupName", SqlDbType.NVarChar, groupName);
				var serverName = Convert.ToString(dbCommand.ExecuteScalar());
				return AddDomainToServerName(serverName, connection.ServerDomain);
			}
		}

		public bool IsDbPartOfAlwaysOn(DbConnection connection, string dbName)
		{
			return AlwaysOn.IsDbPartOfAlwaysOn(connection, dbName);
		}

		public List<string> GetReplicaServersWithDatabase(AdminConnection connection, string dbName, string groupName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			var sqlScript = @"
				SELECT
					Reps.replica_server_name
				FROM
					sys.availability_replicas Reps
					JOIN sys.availability_groups AS AG ON Reps.group_id = AG.group_id
				WHERE
					AG.name = @groupName;
			";

			var replicaServers = new List<string>();

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				var servers = new List<string>();
				connection.ExecuteReader(
					sqlScript,
					cmd =>
					{
						cmd.AddParameter("@groupName", SqlDbType.NVarChar, groupName);
					},
					record => servers.Add(record[0]?.ToString())
				);

				servers = AddDomainToServerNames(servers, connection.ServerDomain);

				foreach (var serverName in servers)
				{
					sqlScript = @"
						SELECT name FROM sys.databases WHERE name = @dbName;
					";

					using (Db.DisableSchemaVersionCheck())
					using (var dbConnection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
					{
						if (dbConnection.DatabaseExists(dbName))
						{
							replicaServers.Add(serverName);
						}
					}
				}
			}

			return replicaServers;
		}

		public List<string> GetSecondaryReplicaNamesList(AdminConnection connection, string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(connection, nameof(connection));

			const string sqlScript = @"
				SELECT
					Reps.replica_server_name
				FROM
					sys.dm_hadr_database_replica_states AS DB_Rep_Sts
					JOIN sys.availability_replicas      AS Reps       ON Reps.replica_id = DB_Rep_Sts.replica_id
				WHERE
					DB_Rep_Sts.database_id = DB_ID(@dbName)
					AND is_primary_replica != 1
					AND DB_Rep_Sts.synchronization_health = 2";

			var names = new List<string>();
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteReader(
					sqlScript,
					cmd => cmd.AddParameter("@dbName", SqlDbType.NVarChar, dbName),
					record => names.Add(record[0].ToString()));
			}

			return AddDomainToServerNames(names, connection.ServerDomain);
		}

		public List<string> GetSecondaryReplicaNamesListForAvailabilityGroup(AdminConnection connection, string groupName)
		{
			Argument.NotNullOrEmpty(groupName, nameof(groupName));
			Argument.NotNull(connection, nameof(connection));

			const string sqlScript = @"
				SELECT
					Reps.replica_server_name
				FROM
					sys.availability_groups AS AG
					JOIN sys.availability_replicas AS Reps ON AG.group_id = Reps.group_id
					JOIN sys.dm_hadr_availability_replica_states AS ReplicaStates 
						ON Reps.replica_id = ReplicaStates.replica_id
				WHERE
					AG.name = @groupName
					AND ReplicaStates.role = 2";

			var names = new List<string>();
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteReader(
					sqlScript,
					cmd => cmd.AddParameter("@groupName", SqlDbType.NVarChar, groupName),
					record => names.Add(record[0].ToString()));
			}

			return AddDomainToServerNames(names, connection.ServerDomain);
		}

		internal List<string> AddDomainToServerNames(List<string> replicaServerNames, string domain)
		{
			if (string.IsNullOrEmpty(domain))
			{
				return replicaServerNames;
			}

			var updatedServerNames = replicaServerNames
				.Select(serverName => AddDomainToServerName(serverName, domain))
				.ToList();

			return updatedServerNames;
		}

		string AddDomainToServerName(string serverName, string domain)
		{
			if (string.IsNullOrEmpty(domain))
			{
				return serverName;
			}

			// If the server has an instance name
			if (serverName.Contains("\\"))
			{
				var parts = serverName.Split('\\');
				return $"{parts[0]}.{domain}\\{parts[1]}";
			}
			else
			{
				return $"{serverName}.{domain}";
			}
		}

		public string GetAvailabilityGroupName(AdminConnection connection, string dbName)
		{
			return AvailabilityGroupInfo.GetGroupInfo(connection, dbName).GroupName;
		}

		public bool RemoveDatabaseFromAvailabilityGroup(AdminConnection connection, string dbName, string groupName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			const string sqlScript = @"ALTER AVAILABILITY GROUP [{0}] REMOVE DATABASE [{1}]";

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery(string.Format(sqlScript, groupName, dbName));
				return !IsDbPartOfAlwaysOn(connection, dbName);
			}
		}

		public bool AddDatabaseToAvailabilityGroup(AdminConnection connection, string dbName, string groupName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			const string sqlScript = @"ALTER AVAILABILITY GROUP [{0}] ADD DATABASE [{1}]";

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery(string.Format(sqlScript, groupName, dbName));
				return IsDbPartOfAlwaysOn(connection, dbName);
			}
		}

		public bool JoinSecondaryDatabaseToAvailabilityGroup(AdminConnection connection, string dbName, string groupName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			const string sqlScript = @"ALTER DATABASE [{0}] SET HADR AVAILABILITY GROUP = [{1}]";

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			using (connection.TemporarySetDeadlockPriority(DeadlockPriority.Max))
			{
				connection.ExecuteNonQuery(string.Format(sqlScript, dbName, groupName));
				return IsDbPartOfAlwaysOn(connection, dbName);
			}
		}
	}
}
