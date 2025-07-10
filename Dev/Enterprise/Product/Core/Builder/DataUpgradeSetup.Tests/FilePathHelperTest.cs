using System.IO;
using CargoWise.BuildTools.Testing;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	class FilePathHelperTest
	{
		public static string TestDataFilePath
		{
			get { return Path.Combine(MockSourceControl.MockWorkspacePath, @"DataUpgradeSetup\TestDataFile.xml"); }
		}

		public static string CompressedTestDataFilePath
		{
			get { return Path.Combine(MockSourceControl.MockWorkspacePath, @"DataUpgradeSetup\TestDataFile.xml.gz"); }
		}
	}
}
