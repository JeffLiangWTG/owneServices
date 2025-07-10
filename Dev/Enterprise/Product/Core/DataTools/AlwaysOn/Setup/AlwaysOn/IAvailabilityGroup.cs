	using System;
	using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public interface IAvailabilityGroup : IValidationStatus
	{
		Guid GroupId { get; }
		string GroupName { get; }
		string MainDatabaseName { get; }
		IEnumerable<IAlwaysOnReplica> Replicas { get; }
		IAlwaysOnReplica PrimaryReplica { get; }
		IEnumerable<string> Databases { get; }
		IAlwaysOnReplica AddSecondaryReplica(ISecondaryServerInstance secondaryServer, IReplicaOptions replicaOptions);
		void RemoveSecondaryReplica(IAlwaysOnReplica secondaryReplica);
		void ChangeReplicaSettings(IAlwaysOnReplica replica, IReplicaOptions newSettings);
		void AddDatabases(IEnumerable<string> databasesToAdd);
	}
}
