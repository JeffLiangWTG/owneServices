using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public class AvailabilityGroup : IAvailabilityGroup
	{
		public AvailabilityGroup(SqlServerInfo serverInfo, IAlwaysOnDatabase alwaysOnDb)
		{
			primaryServerInfo = serverInfo;
			groupId = alwaysOnDb.GroupId;
			groupName = alwaysOnDb.GroupName;
			mainDatabaseName = alwaysOnDb.Name;
		}

		#region Load Existing

		public void Load(SqlServerInfo serverInfo)
		{
			lastErrorMessage = null;

			try
			{
				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(serverInfo);
				LoadUnsafe(sqlContext);

				isLoaded = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = ex.ToString();
				isLoaded = false;
			}
		}

		void LoadUnsafe(ISqlExecutionContext sqlContext)
		{
			replicas = GetAvailabilityReplicas(sqlContext);
			databases = GetGroupDatabases(sqlContext);
		}

		readonly SqlServerInfo primaryServerInfo;

		#endregion

		#region IAvailabilityGroup Members

		Guid IAvailabilityGroup.GroupId
		{
			get { return groupId; }
		}

		readonly Guid groupId;

		string IAvailabilityGroup.GroupName
		{
			get { return groupName; }
		}
		readonly string groupName;

		IEnumerable<IAlwaysOnReplica> IAvailabilityGroup.Replicas
		{
			get { return replicas ?? (replicas = new List<IAlwaysOnReplica>()); }
		}
		List<IAlwaysOnReplica> replicas;

		IAlwaysOnReplica IAvailabilityGroup.PrimaryReplica
		{
			get { return replicas?.FirstOrDefault(); }
		}

		List<IAlwaysOnReplica> ReplicaList
		{
			get
			{
				return (List<IAlwaysOnReplica>)((IAvailabilityGroup)this).Replicas;
			}
		}

		string IAvailabilityGroup.MainDatabaseName => mainDatabaseName ?? string.Empty;
		readonly string mainDatabaseName;

		IEnumerable<string> IAvailabilityGroup.Databases
		{
			get { return databases ?? (databases = new List<string>()); }
		}
		List<string> databases;

		List<string> DatabaseList
		{
			get
			{
				return (List<string>)((IAvailabilityGroup)this).Databases;
			}
		}

		string IValidationStatus.LastErrorMessage
		{
			get { return lastErrorMessage; }
		}
		string lastErrorMessage;

		bool IValidationStatus.IsLoaded
		{
			get { return isLoaded; }
		}
		bool isLoaded;

		bool IValidationStatus.HasErrors
		{
			get { return !string.IsNullOrWhiteSpace(lastErrorMessage); }
		}

		IAlwaysOnReplica IAvailabilityGroup.AddSecondaryReplica(ISecondaryServerInstance secondaryServer, IReplicaOptions replicaOptions)
		{
			IAlwaysOnReplica replica = null;

			var endpointUrlForNewReplica = $"TCP://{secondaryServer.ServerInfo.ServerFQDN}:{secondaryServer.AlwaysOnEndpointPort}";

			try
			{
				lastErrorMessage = null;

				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(primaryServerInfo);

				var replicaId = AddGroupSecondaryReplica(sqlContext, secondaryServer.ServerInfo.ServerAlias, endpointUrlForNewReplica, replicaOptions);

				replica = AlwaysOnReplicaFactory.New(
					roleFlag: ReplicaRole.Secondary,
					healthFlag: SyncronisationHealth.NOT_HEALTHY,
					joinStateFlag: secondaryServer.IsFailoverClusterInstance ? JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE : JoinState.JOINED_STANDALONE,
					options: replicaOptions,
					replicaId: replicaId,
					parentGroup: this,
					serverInfo: secondaryServer.ServerInfo,
					endpointAddress: endpointUrlForNewReplica);

				replica.GetOdysseyAdminLoginIfExists();
				replica.CheckOdysseyAdminIsDbOwner(replica.ParentGroup.Databases);

				ReplicaList.Add(replica);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}

			return replica;
		}

		void IAvailabilityGroup.RemoveSecondaryReplica(IAlwaysOnReplica secondaryReplica)
		{
			try
			{
				lastErrorMessage = null;

				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(primaryServerInfo);

				RemoveGroupSecondaryReplica(sqlContext, secondaryReplica.ServerInfo);

				ReplicaList.Remove(secondaryReplica);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		void IAvailabilityGroup.ChangeReplicaSettings(IAlwaysOnReplica replica, IReplicaOptions newSettings)
		{
			try
			{
				lastErrorMessage = null;

				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(primaryServerInfo);

				ChangeReplicaModeSettings(sqlContext, replica.ServerInfo.ServerAlias, replica.Options, newSettings);

				replica.ChangeSettings(newSettings);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		void IAvailabilityGroup.AddDatabases(IEnumerable<string> databasesToAdd)
		{
			try
			{
				lastErrorMessage = null;

				var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(primaryServerInfo);

				AddDatabasesToGroup(sqlContext, databasesToAdd);

				DatabaseList.AddRange(databasesToAdd);
				DatabaseList.Sort();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastErrorMessage = (ex is AlwaysOnException) ? ex.Message : ex.ToString();
			}
		}

		#endregion

		#region Data Load

		/// <summary>
		/// Get list replicas of this availability group
		/// </summary>
		List<IAlwaysOnReplica> GetAvailabilityReplicas(ISqlExecutionContext sqlContext)
		{
			var replicas = new List<IAlwaysOnReplica>();

			var getReplicaListSql = Invariant($@"
SELECT DISTINCT
	ar.replica_id,
	CASE
		WHEN arcs.join_state = 1 THEN arcn.node_name
	END AS node_name,
	ar.replica_server_name,
	ars.[role],
	ars.synchronization_health,
	ar.[availability_mode],
	ar.[failover_mode],
	ar.[secondary_role_allow_connections],
	ar.read_only_routing_url,
	ar.endpoint_url,
	arcs.join_state,
	CASE
		WHEN ars.[role] = 1 THEN 1
		WHEN ars.[role] = 2 THEN 2
		ELSE 99
	END AS sort_order
FROM
	sys.availability_replicas ar
	INNER JOIN sys.dm_hadr_availability_replica_states ars ON ars.replica_id = ar.replica_id
	INNER JOIN sys.availability_groups ag ON ag.group_id = ar.group_id
	INNER JOIN sys.dm_hadr_availability_replica_cluster_nodes arcn ON arcn.group_name = ag.[name] AND arcn.replica_server_name = ar.replica_server_name
	INNER JOIN sys.dm_hadr_availability_replica_cluster_states arcs ON arcs.replica_id = ar.replica_id AND arcs.group_id = ag.group_id
WHERE
	ar.group_id = '{groupId}'
ORDER BY
	sort_order
");

			using (var reader = sqlContext.ExecuteReader(getReplicaListSql))
			{
				while (reader.Read())
				{
					var replicaId = reader.GetGuid(0);
					var nodeName = reader[1].ToString().TrimEnd('\0');
					var replicaName = reader[2];
					var roleFlagObj = Convert.ToInt32(reader[3]);
					var healthFlagObj = Convert.ToInt32(reader[4]);
					var commitModeFlag = Convert.ToInt32(reader[5]);
					var failoverFlag = Convert.ToInt32(reader[6]);
					var secRoleConnectionFlag = Convert.ToInt32(reader[7]);
					var secReadOnlyRoutingUrlObj = reader[8];
					var secReadOnlyRoutingUrl = (secReadOnlyRoutingUrlObj == DBNull.Value || secReadOnlyRoutingUrlObj == null) ? "" : secReadOnlyRoutingUrlObj.ToString();
					var endpointUrlObj = reader[9];
					var joinStateFlag = Convert.ToInt32(reader[10]);

					var replicaOptions = ReplicaOptions.NewFromRawData(commitModeFlag, failoverFlag, secRoleConnectionFlag, secReadOnlyRoutingUrl);
					var endpointUrl = (endpointUrlObj == DBNull.Value || endpointUrlObj == null) ? "" : endpointUrlObj.ToString();
					var serverInfo = new SqlServerInfo(replicaName.ToString(), primaryServerInfo.PortNumber);
					replicas.Add(AlwaysOnReplicaFactory.New((ReplicaRole)roleFlagObj, (SyncronisationHealth)healthFlagObj, (JoinState)joinStateFlag, replicaOptions, replicaId, this, serverInfo, endpointUrl));
				}
			}

			return replicas;
		}

		internal virtual List<string> GetGroupDatabases(ISqlExecutionContext sqlContext)
		{
			var dbList = new List<string>();

			var getDatabasesSql = Invariant($@"
SELECT database_name
	FROM sys.availability_databases_cluster
	WHERE group_id = '{groupId}'
	ORDER BY database_name
");

			using (var reader = sqlContext.ExecuteReader(getDatabasesSql))
			{
				while (reader.Read())
				{
					var readerObj = reader[0];
					dbList.Add(readerObj.ToString());
				}
			}

			return dbList;
		}

		#endregion

		#region Add/Remove Secondary Replica

		Guid AddGroupSecondaryReplica(ISqlExecutionContext sqlContext, string serverFullInstance, string endPointUrl, IReplicaOptions replicaOptions)
		{
			var addSecondaryReplicaSql = Invariant($@"
ALTER AVAILABILITY GROUP [{groupName}]
	ADD REPLICA ON N'{serverFullInstance}' WITH (
		ENDPOINT_URL = N'{endPointUrl}',
		FAILOVER_MODE = {replicaOptions.Failover},
		AVAILABILITY_MODE = {replicaOptions.CommitMode},
		PRIMARY_ROLE(ALLOW_CONNECTIONS = ALL),
		SECONDARY_ROLE(ALLOW_CONNECTIONS = {replicaOptions.SecondaryAllowConnection})
	)
");

			sqlContext.ExecuteNonQuery(addSecondaryReplicaSql);

			var commandObj = sqlContext.ExecuteScalar(Invariant($@"
SELECT replica_id FROM sys.availability_replicas
WHERE group_id = '{groupId}'
AND replica_server_name = '{serverFullInstance}'
"));

			return new Guid(commandObj.ToString());
		}

		void RemoveGroupSecondaryReplica(ISqlExecutionContext sqlContext, SqlServerInfo secondaryServerInfo)
		{
			var removeSecondaryReplicaSql = $"ALTER AVAILABILITY GROUP [{groupName}] REMOVE REPLICA ON N'{secondaryServerInfo.ServerAlias}'";

			sqlContext.ExecuteNonQuery(removeSecondaryReplicaSql);
		}

		/// <summary>
		/// ALTER AVAILABILITY GROUP [group] MODIFY REPLICA ON N'replica_server_instance' WITH
		///   - (AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT } )
		///   - (FAILOVER_MODE = { AUTOMATIC | MANUAL })
		///   - (SECONDARY_ROLE (ALLOW_CONNECTIONS = { NO | READ_ONLY | ALL }))
		///   - (SECONDARY_ROLE (READ_ONLY_ROUTING_URL = 'TCP://system-address:port'))
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		void ChangeReplicaModeSettings(ISqlExecutionContext sqlContext, string secondaryServerInstance, IReplicaOptions oldSettings, IReplicaOptions newSettings)
		{
			var baseSql = $"ALTER AVAILABILITY GROUP [{groupName}] MODIFY REPLICA ON N'{secondaryServerInstance}' WITH";

			var sqlCmdBuilder = new StringBuilder();

			if (oldSettings.CommitMode != newSettings.CommitMode || oldSettings.Failover != newSettings.Failover)
			{
				var commitModeSql = $"{baseSql} (AVAILABILITY_MODE = {newSettings.CommitMode});\r\n";
				var failoverModeSql = $"{baseSql} (FAILOVER_MODE = {newSettings.Failover});\r\n";

				if (newSettings.CommitMode == AvailabilityMode.ASYNCHRONOUS_COMMIT)
				{
					if (oldSettings.Failover != newSettings.Failover)
					{
						sqlCmdBuilder.Append(failoverModeSql);
					}

					if (oldSettings.CommitMode != newSettings.CommitMode)
					{
						sqlCmdBuilder.Append(commitModeSql);
					}
				}
				else
				{
					if (oldSettings.CommitMode != newSettings.CommitMode)
					{
						sqlCmdBuilder.Append(commitModeSql);
					}

					if (oldSettings.Failover != newSettings.Failover)
					{
						sqlCmdBuilder.Append(failoverModeSql);
					}
				}
			}

			if (oldSettings.SecondaryAllowConnection != newSettings.SecondaryAllowConnection)
			{
				sqlCmdBuilder.AppendFormat("{0} (SECONDARY_ROLE(ALLOW_CONNECTIONS = {1}));\r\n", baseSql, newSettings.SecondaryAllowConnection);
			}

			if (oldSettings.SecondaryReadOnlyRoutingUrl != newSettings.SecondaryReadOnlyRoutingUrl)
			{
				sqlCmdBuilder.AppendFormat("{0} (SECONDARY_ROLE(READ_ONLY_ROUTING_URL = {1}));\r\n", baseSql, newSettings.SecondaryAllowConnection);
			}

			if (sqlCmdBuilder.Length > 0)
			{
				sqlContext.ExecuteNonQuery(sqlCmdBuilder.ToString());
			}
		}

		#endregion // Add/Remove Secondary Replica

		#region Add Database

		void AddDatabasesToGroup(ISqlExecutionContext sqlContext, IEnumerable<string> databasesToAdd)
		{
			var sqlText = string.Join("", databasesToAdd.Select(db => $"ALTER AVAILABILITY GROUP [{groupName}] ADD DATABASE [{db}];"));

			sqlContext.ExecuteNonQuery(sqlText);
		}
		#endregion // Add Database

	}
}
