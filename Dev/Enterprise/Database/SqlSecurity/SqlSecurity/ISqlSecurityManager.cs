using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;

namespace Enterprise.SqlSecurity
{
	public interface ISqlSecurityManager
	{
		void Propagate(AdminConnection connection, IEnumerable<string> replicas, CancellationToken token);
		void BuildDatabaseSecurity(AdminConnection connection, string databaseName, bool trialRun);
		void BuildServerSecurity(AdminConnection connection, bool trialRun);
		void BuildSecurityForAllDatabases(AdminConnection connection, CancellationToken cancellationToken);
		void BuildSecurity(AdminConnection connection, CancellationToken cancellationToken, bool trialRun = false);
	}
}
