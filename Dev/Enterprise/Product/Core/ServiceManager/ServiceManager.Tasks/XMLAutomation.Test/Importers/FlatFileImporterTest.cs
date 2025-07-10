using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class FlatFileImporterTest : TestCaseWithFactory
	{
		public void TestImportFlatFile()
		{
			string tempDirectory = Env.TempPath;
			FlatFileImporterForTesting importer = new FlatFileImporterForTesting();
			importer.ImportFlatFile(new FileInfo(tempDirectory), new NotificationBuffer());
			AssertEquals("Import should have been run", true, importer.ImportHasBeenRun);
		}

		class FlatFileImporterForTesting : FlatFileImporter
		{
			protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
			{
				ImportHasBeenRun = true;
			}

			public bool ImportHasBeenRun;
		}
	}
}
