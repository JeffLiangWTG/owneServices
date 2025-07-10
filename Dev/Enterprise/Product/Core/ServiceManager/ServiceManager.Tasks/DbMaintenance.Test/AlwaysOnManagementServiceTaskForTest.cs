using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	sealed class AlwaysOnManagementServiceTaskForTest : AlwaysOnManagementServiceTask
	{
		internal List<string> commandList = new List<string>();

		public void EnsureApplicationDbLoginRights_Exposed(AdminConnection connection)
		{
			base.EnsureApplicationDbLoginRightsForAllDatabases(connection);
		}

		public void RepairUnhealthyReplicationTest(string groupName, IEnumerable<string> databases, ILogger logger)
		{
			using (var primaryConnection = Db.NewAdminConnection())
			{
				base.RepairUnhealthyReplication(primaryConnection, groupName, databases, logger);
			}
		}

		public IEnumerable<string> GetDatabases_Exposed(DbConnection connection)
		{
			return AlwaysOnManagementServiceTask.GetDatabases(connection);
		}

		protected override string GetDatabaseStateDescription(DbConnection connection, string dbName) => stateForTesting?.Pop() ?? base.GetDatabaseStateDescription(connection, dbName);

		protected override void ExecuteSql(DbConnection connection, string sql)
		{
			commandList.Add(sql);
			if (ShouldExecuteSql?.Invoke(sql) ?? true)
			{
				base.ExecuteSql(connection, sql);
			}
		}

		protected override List<(string, string)> GetUnhealthyDatabases(DbConnection primaryConnection, string groupName, IEnumerable<string> databases) => UnhealthyDatabaseList;

		public List<(string, string)> UnhealthyDatabaseList { get; } = new List<(string, string)>();

		public Func<string, bool> ShouldExecuteSql { get; set; }

		public void SetDatabaseStateForTesting(string state)
		{
			if (stateForTesting == null)
			{
				stateForTesting = new Stack<string>();
			}
			stateForTesting.Push(state);
		}
		Stack<string> stateForTesting;
	}
}
