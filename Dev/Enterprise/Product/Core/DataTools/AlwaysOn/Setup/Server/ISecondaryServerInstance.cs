using System;
using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public interface ISecondaryServerInstance : IDbServerInstance
	{
		IEnumerable<IDatabasePreJoinStatus> GetPreJoinDatabaseStatuses(IDictionary<string, DbFileAndTransactionLogInfo> currentGroupDatabases, Guid currentGroupId);
		void EnsureAlwaysOnEndpoint(int primaryServerEndpointPort, string sqlServiceAccount);
		void EnsureDatabaseLogins(IEnumerable<DbLoginInfo> primaryDbLogins);
		void JoinGroup(IAvailabilityGroup group);
		void JoinDatabases(IAvailabilityGroup group, IEnumerable<IDatabasePreJoinStatus> databases);
		bool EnsureOdysseyAdminLogin(IEnumerable<IDatabasePreJoinStatus> databases, bool force = false);
	}
}

