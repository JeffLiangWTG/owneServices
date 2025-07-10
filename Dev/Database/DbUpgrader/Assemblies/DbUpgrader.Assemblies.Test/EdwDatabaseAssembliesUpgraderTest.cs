using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	class EdwDatabaseAssembliesUpgraderTest : DatabaseAssembliesUpgraderRegistrationTest
	{
		protected override string[] ExpectedAssemblies { get; } =
		{
			"CargoWise.Data.SqlClr.SafeAccess",
			"CargoWise.Data.SqlClr.ExtAccess"
		};

		public void TestSafeAccessAreRegistered()
		{
			FunctionsAreRegistered(ExpectedAssemblies[0], new[]
			{
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.HashCalculator", "GetCustomizableDataKey"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "UncompressAsString"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.CssvConcatenateAgg", null),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.ConcatenateAgg", null)
			});
		}

		public void TestExtAccessAreRegistered()
		{
			FunctionsAreRegistered(ExpectedAssemblies[1], new[]
			{
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRDeleteFile"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRRenameFile"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRCopyFile")
			});
		}

		protected override DatabaseAssembliesUpgrader CreateAssembliesUpgrader(IUpgradeManager upgradeManager, DbConnection dbConnection, VersionLabel versionBeforeUpgrade)
		{
			return new EdwDatabaseAssembliesUpgrader(upgradeManager, dbConnection, dbConnection, versionBeforeUpgrade);
		}
	}
}
