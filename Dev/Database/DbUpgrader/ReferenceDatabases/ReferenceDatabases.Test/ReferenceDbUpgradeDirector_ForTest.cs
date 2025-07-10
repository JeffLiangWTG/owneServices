using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class ReferenceDbUpgradeDirector_ForTest : ReferenceDbUpgradeDirector
	{
		public ReferenceDbUpgradeDirector_ForTest(IUpgradeContext upgradeContext, DbConnection connection, IUpgradeTaskWorkflowLogger logger) : base(upgradeContext, connection, logger) { }

		protected override IEnumerable<string> GetServerList() => new List<string>() { Db.ServerName, "UnavailableServer_ForTest" };

		protected override void CreateSingleRefDatabase(string server, string databaseName)
		{
			if (server == "UnavailableServer_ForTest")
			{
				throw new Exception($"Cannot connect to server {server}");
			}
			else
			{
				base.CreateSingleRefDatabase(server, databaseName);
			}
		}
	}
}
