using System;
using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public enum ReplicaRole
	{
		Resolving = 0,
		Primary = 1,
		Secondary = 2,
	}

	public enum SyncronisationHealth
	{
		NOT_HEALTHY = 0,
		PARTIALLY_HEALTHY = 1,
		HEALTHY = 2,
	}

	public enum JoinState
	{
		NOT_JOINED = 0,
		JOINED_STANDALONE = 1,
		JOINED_FAILOVER_CLUSTER_INSTANCE = 2,
	}

	/// <summary>
	/// SELECT * FROM sys.dm_hadr_database_replica_cluster_states WHERE replica_id = '...';
	/// SELECT * FROM sys.dm_hadr_availability_replica_cluster_nodes WHERE replica_id = '...';
	/// SELECT * FROM sys.dm_hadr_database_replica_states WHERE replica_id = GUID'...';
	/// SELECT * FROM sys.dm_hadr_availability_replica_cluster_states WHERE replica_id = '...';
	/// </summary>
	public interface IAlwaysOnReplica : IValidationStatus
	{
		Guid ReplicaId { get; }
		IAvailabilityGroup ParentGroup { get; }

		SqlServerInfo ServerInfo { get; }
		string ServerAddressFromEndpointUrl { get; }

		ReplicaRole Role { get; }
		SyncronisationHealth Health { get; }
		JoinState JoinState { get; }
		IReplicaOptions Options { get; }

		IDictionary<string, DbFileAndTransactionLogInfo> GetGroupDatabases();
		IEnumerable<DbLoginInfo> GetGroupDatabaseLogins();
		IEnumerable<DbLoginInfo> GetGroupDatabaseLoginsWithoutErrorHandling();
		void CopyDatabaseLoginsFromPrimaryReplica();
		void PerformPlannedManualFailover();
		void ChangeSettings(IReplicaOptions newSettings);
		void SuspendReplication(string dbName);
		void ResumeReplication(string dbName);
		void RefreshHealthState();

		DbLoginInfo GetOdysseyAdminLoginIfExists();
		DbLoginInfo GetOdysseyAdminLoginIfExistsWithoutErrorHandling();
		void CheckOdysseyAdminIsDbOwner(IEnumerable<string> groupDatabases);
		DbLoginInfo OdysseyAdminLogin { get; }
		bool IsOdysseyAdminLoginSidDifferentFromPrimary { get; }
		bool IsOdysseyAdminDbOwner { get; }
		bool RequiresManualFailOver { get; }
		bool IsPrimary { get; }
		bool IsAvailableForManualFailOver { get; }

		event EventHandler HealthStatusChanged;
	}
}
