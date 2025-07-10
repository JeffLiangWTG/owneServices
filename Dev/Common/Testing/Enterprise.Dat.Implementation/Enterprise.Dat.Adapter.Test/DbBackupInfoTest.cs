using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class DbBackupInfoTest : TestCase
	{
		public void TestDataAndLogLogicalPaths()
		{
			using (var sqlCon = LocalDBConnection.GetConnection())
			{
				sqlCon.Open();

				string backupFilePath = Path.Combine(DbRestorer.DatDatabaseBackups, CurrentDatDbBackupPrefix.GetValue() + ".bak");
				var backupInfo = new DbBackupInfo();
				backupInfo.LoadBackupInfo(sqlCon, backupFilePath);
				AssertBackupContainsFile(backupInfo, "Odyssey_Data", "_Data.mdf");
				AssertBackupContainsFile(backupInfo, "Odyssey_Data02", "_Data02.ndf");
				AssertBackupContainsFile(backupInfo, "Odyssey_Log", "_Log.ldf");
			}
		}

		void AssertBackupContainsFile(DbBackupInfo backupInfo, string expectedLogicalName, string expectedPhysicalFileSuffix)
		{
			Assert(expectedLogicalName + " file found", backupInfo.DatabaseFiles.Contains(new DbBackupInfo.LogicalNameAndPhysicalFileSuffix(expectedLogicalName, expectedPhysicalFileSuffix)));
		}
	}
}
