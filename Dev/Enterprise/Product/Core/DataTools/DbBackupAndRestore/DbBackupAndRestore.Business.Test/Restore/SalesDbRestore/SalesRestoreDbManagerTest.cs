using System.IO;
using CargoWise.IO;
using Enterprise.DataTools.DbBackupAndRestore.Testing.Restore;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SalesRestoreDbManagerTest : TestCase
	{
		public void TestSearchBackupFile()
		{
			var testBackupFileName1 = "CW1Bkp_T-20090205-163825_S-TEST-TST-1_D-SalesDBTest.bak";
			var testBackupFileName2 = "CW1Bkp_T-20090205-163825_S-TEST-TST-2_D-SalesDBTest.bak";
			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath2 = TestDataHelpers.CopyAndNameTestFileResource(tempDir.DirectoryName, "testProdCopy.bak", testBackupFileName2);

				var restoreManager = new SalesRestoreDbManagerForTesting();
				AssertEquals("Backup exists", testLocalBackupFilePath2, restoreManager.SearchBackupFile_Exposed(tempDir.DirectoryName));

				var testLocalBackupFilePath1 = Path.Combine(tempDir.DirectoryName, testBackupFileName1);
				File.Copy(testLocalBackupFilePath2, testLocalBackupFilePath1);
				AssertEquals("When there are two backup files in the folder, should return the lastest by file creation time", testLocalBackupFilePath1, restoreManager.SearchBackupFile_Exposed(tempDir.DirectoryName));
			}
		}
	}
}
