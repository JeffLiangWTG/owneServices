using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Script;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	public sealed class AlwaysRequiredScriptUpgradeManager : BaseUpgradeManager
	{
		public override bool IsHosted => false;

		public override ValidationResponse Run()
		{
			using (var upgConnection = Db.NewAdminConnection())
			using (var auditConnection = GetAuditConnection(upgConnection))
			using (var dataWarehouseConnection = GetDataWarehouseConnection(upgConnection))
			{
				var dbInfo = string.Format(CultureInfo.InvariantCulture, "Server: {0} - Database: {1}{2}{3}",
					upgConnection.ServerNameReportedByDatabase, Db.DatabaseName,
					auditConnection != null ?
						string.Format(CultureInfo.InvariantCulture, " - Audit Server: {0}", auditConnection.ServerNameReportedByDatabase)
						: "",
					dataWarehouseConnection != null ?
						string.Format(CultureInfo.InvariantCulture, " - Data Warehouse Server: {0}", dataWarehouseConnection.ServerNameReportedByDatabase)
						: ""
					);

				((IUpgradeManager)this).ShowInfoMessage(dbInfo);

				var scriptUpgrader = new ScriptUpgrader(this, upgConnection, auditConnection, dataWarehouseConnection, new VersionLabel(0, 0));
				((IUpgradeManager)this).ActivateTaskProgress(scriptUpgrader.EstimatedNumberOfTasks);

				scriptUpgrader.RunUpgrade();
			}

			var result = new ValidationResponse();
			result.Successful = true;
			return result;
		}

		AdminConnection GetAuditConnection(DbConnection mainDbConnection)
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);
			return !string.IsNullOrEmpty(auditServer) ?
				Db.NewAdminConnection(auditServer, Db.SqlMasterDb) :
				null;
		}

		AdminConnection GetDataWarehouseConnection(DbConnection mainDbConnection)
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(mainDbConnection);
			return !string.IsNullOrEmpty(dwServer) ?
				Db.NewAdminConnection(dwServer, Db.SqlMasterDb) :
				null;
		}

		public override VersionLabel SchemaVersionBeforeUpgrade => new VersionLabel(0, 0);

		public override VersionLabel TransformationVersionBeforeUpgrade => new VersionLabel(0, 0);
	}
}
