using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	class MainDbAssembliesUpgraderTest : DatabaseAssembliesUpgraderRegistrationTest
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
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "UncompressRTFAsPlainText"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "UncompressAsBytes"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "UncompressAsString"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "RegexReplace"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "CompressAsBytes"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.Uncompress", "CompressStringAsBytes"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.XmlConvert", "ConvertXmlTimeSpanToTotalSeconds"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.HashCalculator", "CalculateMD5Hash"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.CssvConcatenateAgg", null),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.ConcatenateAgg", null),
				new Tuple<string, string>("CargoWise.Data.SqlClr.SafeAccess.EdifactConvert", "GetEdifactElements")
			});
		}

		public void TestExternalAccessAreRegistered()
		{
			FunctionsAreRegistered(ExpectedAssemblies[1], new[]
			{
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.OperatingSystemInfo", "OsVersionNumber"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.OperatingSystemInfo", "OsNameAndVersion"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRDeleteFile"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRRenameFile"),
				new Tuple<string, string>("CargoWise.Data.SqlClr.ExtAccess.FileOperations", "CLRCopyFile")
			});
		}

		protected override DatabaseAssembliesUpgrader CreateAssembliesUpgrader(IUpgradeManager upgradeManager, DbConnection dbConnection, VersionLabel versionBeforeUpgrade)
		{
			return new MainDbAssembliesUpgrader(upgradeManager, dbConnection, versionBeforeUpgrade);
		}
	}
}
