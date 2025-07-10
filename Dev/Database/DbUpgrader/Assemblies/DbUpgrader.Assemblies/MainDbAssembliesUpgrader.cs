using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies
{
	public class MainDbAssembliesUpgrader : DatabaseAssembliesUpgrader
	{
		public MainDbAssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade)
			: base(manager, upgConnection, versionBeforeUpgrade, RegisteredAssemblies.MainClrObjects)
		{
		}
	}
}