using System.IO;
using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	public class FileSystemAccessControlUtilsTest : TestCase
	{
		string unwritableDir;
		string writableDir;

		public void TestIsDirectoryWritable()
		{
			AssertEquals(true, FileSystemAccessControlUtils.IsDirectoryWritable(writableDir, out var _));
			AssertEquals(false, FileSystemAccessControlUtils.IsDirectoryWritable(unwritableDir, out var ex));
			AssertEquals("Exception for Unit Test", ex.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			unwritableDir = Path.Combine(Enterprise.Environment.Env.TempPath, "unwritableDir");
			writableDir = Path.Combine(Enterprise.Environment.Env.TempPath, "writableDir");
			FileSystemAccessControlUtilsTestHelper.CreateFolderWithBlockedPermissionsForTest(unwritableDir);
			Directory.CreateDirectory(writableDir);
		}

		protected override void TearDown()
		{
			Directory.Delete(unwritableDir);
			Directory.Delete(writableDir);
			base.TearDown();
		}
	}
}
