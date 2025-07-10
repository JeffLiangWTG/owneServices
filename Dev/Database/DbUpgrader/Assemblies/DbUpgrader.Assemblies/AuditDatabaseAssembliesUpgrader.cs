using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies
{
	public class AuditDatabaseAssembliesUpgrader : BiDatabaseAssembliesUpgrader
	{
		public AuditDatabaseAssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection biConnection, VersionLabel versionBeforeUpgrade)
			: base(manager, upgConnection, biConnection, versionBeforeUpgrade, RegisteredAssemblies.AuditClrObjects)
		{
		}

		protected override string DatabaseName => Db.AuditDatabaseName;
	}
}