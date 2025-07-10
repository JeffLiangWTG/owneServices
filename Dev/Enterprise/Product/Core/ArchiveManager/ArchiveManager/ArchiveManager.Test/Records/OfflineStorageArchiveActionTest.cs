using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Engine;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	class OfflineStorageArchiveActionTest : TestCase
	{
		const string DummyStageName = "Dummy Stage Name";

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		[TestDate(2023, 12, 12, 12, 0, 0)]
		public void TestOfflineStorageArchiveActionSetsStorageMainOfflineDate()
		{
			using var tempDir = new TempDirectory();
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			var logger = new TestArchiveLogger();
			var systemCache = new ArchiveSystemCache();
			var manager = new ArchiveVolumeManager(tempDir.DirectoryName, 2M);

			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = DbNumber;
			storageMain.SM_ParentFK = Guid.NewGuid();

			var storageDoc1 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc1.SC_SM = storageMain.PK;
			storageDoc1.SC_FileName = "mainArchiveItem.pdf";
			storageDoc1.SC_ImageData = TestFileHelper.SamplePDF;

			var mainArchiveItem = new ArchiveItem(storageMain.PKSchemaColumn, storageMain.PK.ToGuid(), null, Guid.Empty, false, storageMain.TablePrefix);
			var archiveSet = new TestArchiveSet(new DummyArchiveSystem("OFL"), DummyStageName, Guid.Empty, mainArchiveItem);
			var offlineStorageArchiveAction = new OfflineStorageArchiveAction(archiveSet);

			documentFactory.Save();
			factory.Save();

			AssertEquals("There should be 1 eDoc before executing", 1, storageMain.eDocs.Count);
			AssertEquals("Offline date should be empty before executing", ZDateTime.Empty, storageMain.SM_OffLine);

			offlineStorageArchiveAction.Execute();
			var storageMainAfterExecuting = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).Load<StorageMain>(storageMain.PK);
			AssertEquals("There should be 0 eDocs after executing", 0, storageMainAfterExecuting.eDocs.Count);
			AssertEquals("Offline date should be filled in after executing", ZDateTime.Now, storageMainAfterExecuting.SM_OffLine);
		}

		const int DbNumber = 333;
		readonly DocManagerDBHelperTestClass DbHelper = new();

		protected override void SetUp()
		{
			base.SetUp();
			DropDatabase();
			_ = DbHelper.CreateDatabase(DbNumber);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DropDatabase();
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			DropDatabase();
		}

		void DropDatabase()
		{
			if (DbHelper.DatabaseExists(DbNumber))
			{
				var dbName = DbHelper.GetDatabaseName(DbNumber);
				DbHelper.DropDatabase(dbName);
			}
		}
	}
}
