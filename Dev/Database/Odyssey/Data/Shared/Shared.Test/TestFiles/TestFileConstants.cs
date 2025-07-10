using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public static class TestFileConstants
	{
		public static string DefaultDataFileBasePath
		{
			get { return Path.Combine(BaseSourcePath, dataUpgraderBasePath); }
		}

		public static string DefaultDocumentsDataFileBasePath
		{
			get { return Path.Combine(BaseSourcePath, dataUpgraderBasePathForDocuments); }
		}

		public static string DefaultTestDataFileBasePath
		{
			get { return Path.Combine(BaseSourcePath, TestDataFileBasePath); }
		}

		public static string BaseSourcePath
		{
			get { return TestCase.BaseSourcePath; }
		}

		const string dataUpgraderBasePath = @"Database\Odyssey\Data\";
		const string dataUpgraderBasePathForDocuments = @"Enterprise\Product\Documents\ExcelTemplates\Dbupgrader.Data\";

		public const string TestDataFileBasePath = @"Database\Odyssey\Data\Shared\";
		public const string TestDataFileRelativePath = @"Shared.Test\TestFiles\TestDataFile.xml";
		public const string TestDataFileRelativeResourcePath = @"Shared.Test\TestFiles\TestDataFile.xml";
		public const string TestDataFileFullRelativePath = TestDataFileBasePath + TestDataFileRelativePath;
	}
}
