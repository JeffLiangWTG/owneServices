using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	class AssembliesUpgraderTest : BaseUpgraderTestCase
	{
		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			var versionBeforeUpgrade = new VersionLabel(0, 0);
			return new AssembliesUpgrader(dummyUpgradeManager, Db.Connection, versionBeforeUpgrade, new DatabaseAssembliesUpgraderStub(dummyUpgradeManager, Db.Connection, versionBeforeUpgrade));
		}

		class DatabaseAssembliesUpgraderStub : DatabaseAssembliesUpgrader
		{
			public DatabaseAssembliesUpgraderStub(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade, params SqlAssemblyClrObjectInfo[] registeredClrObjects)
				: base(manager, upgConnection, versionBeforeUpgrade, registeredClrObjects)
			{
			}
		}
	}
}