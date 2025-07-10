#if DEBUG

using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	public class AutoRegenTemplate : MainDbTemplateWithExtraDevelopmentObjects
	{
		public AutoRegenTemplate(IUpgradeManager manager, string templateDbName)
			: base(manager, templateDbName)
		{
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			using (var repairConnection = Db.NewAdminConnection(dbName))
			{
				DataUtils.AlterDbAuthorisation(repairConnection, dbName);
				DataUtils.EnsureClrEnabledAndTrustworthyOn(repairConnection, dbName);

				((IDbLoginRepair)repairConnection).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => { });
			}

			base.SetupDatabaseAfterCreation(conn);
		}

		protected override void SetDatabaseReadOnly(DbConnection conn)
		{
		}
	}
}

#endif
