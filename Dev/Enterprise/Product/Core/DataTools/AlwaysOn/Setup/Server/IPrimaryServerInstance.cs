using System;
using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public interface IPrimaryServerInstance : IDbServerInstance
	{
		string SqlServiceAccount { get; }
		IEnumerable<IAlwaysOnDatabase> EligibleTopLevelDatabases { get; }
		IEnumerable<DatabaseAlwaysOnStatus> GetAlwaysOnDatabaseSet(string mainDbName, bool alterDbSettingToMeetRequiremtns);
		Guid CreateAvailabilityGroup(string mainDbName, string newGroupName, int endpointPort);
		void RemoveAvailabilityGroup(string groupName);
		void SuspendReplication(string dbName);
		void ResumeReplication(string dbName);
	}
}
