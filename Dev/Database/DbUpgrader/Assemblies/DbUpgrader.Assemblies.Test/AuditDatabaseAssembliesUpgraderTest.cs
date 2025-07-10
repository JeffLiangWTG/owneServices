using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	class AuditDatabaseAssembliesUpgraderTest : DatabaseAssembliesUpgraderRegistrationTest
	{
		protected override string[] ExpectedAssemblies { get; } =
		{
			"CargoWise.Data.SqlClr.ExtAccess"
		};

		public void TestExtAccessAreRegistered()
		{
			FunctionsAreRegistered(ExpectedAssemblies[0], new[]
			{
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRDeleteFile"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRRenameFile"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRCopyFile")
			});
		}

		protected override DatabaseAssembliesUpgrader CreateAssembliesUpgrader(IUpgradeManager upgradeManager, DbConnection dbConnection, VersionLabel versionBeforeUpgrade)
		{
			return new AuditDatabaseAssembliesUpgrader(upgradeManager, dbConnection, dbConnection, versionBeforeUpgrade);
		}
	}
}