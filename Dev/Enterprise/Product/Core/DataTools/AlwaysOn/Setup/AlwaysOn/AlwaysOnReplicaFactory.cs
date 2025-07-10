using System;

namespace Enterprise.AlwaysOn.Setup
{
	public static class AlwaysOnReplicaFactory
	{
		public static IAlwaysOnReplica New(ReplicaRole roleFlag, SyncronisationHealth healthFlag, JoinState joinStateFlag, IReplicaOptions options, Guid replicaId, IAvailabilityGroup parentGroup, SqlServerInfo serverInfo, string endpointAddress)
		{
			var result = new AlwaysOnReplica(roleFlag, healthFlag, joinStateFlag, options, replicaId, parentGroup, serverInfo, endpointAddress);
			return result;
		}
	}
}
