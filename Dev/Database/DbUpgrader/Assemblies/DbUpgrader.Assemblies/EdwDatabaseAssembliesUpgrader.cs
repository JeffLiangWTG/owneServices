using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies
{
	public class EdwDatabaseAssembliesUpgrader : BiDatabaseAssembliesUpgrader
	{
		public EdwDatabaseAssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection biConnection, VersionLabel versionBeforeUpgrade)
			: base(manager, upgConnection, biConnection, versionBeforeUpgrade, RegisteredAssemblies.EdwClrObjects)
		{
		}

		protected override string DatabaseName => Db.EdwDatabaseName;
	}
}