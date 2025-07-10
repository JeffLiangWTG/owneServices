using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Engine;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(OfflineStorage))]
	[UseSnapshotProtection]
	sealed class OfflineStorageTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCalculateStorageCapacity()
		{
			var imageBytes = TestFileHelper.File1MB;

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMain1 = docFactory.New<ArchiveStorageMain>();
			storageMain1.SM_DB = DbNumber;

			var dto = new AddFileOrDocumentDto { DocumentType = "MSC" };
			dto.FileName = "testHenry";
			_ = storageMain1.AddFileOrDocument(imageBytes, dto);
			dto.FileName = "testHenry2";
			_ = storageMain1.AddFileOrDocument(imageBytes, dto);
			dto.FileName = "testHenry3";
			_ = storageMain1.AddFileOrDocument(imageBytes, dto);
			dto.FileName = "testHenry4";
			_ = storageMain1.AddFileOrDocument(imageBytes, dto);
			storageMain1.SM_Archived = new ZDateTime(2009, 1, 14);
			storageMain1.SM_ParentFK = ZGuid.NewZGuid();

			docFactory.Save();

			var storage = new OfflineStorage()
			{
				ArchiveDateTo = new ZDateTime(2009, 1, 15)
			};
			storage.CalculateRequiredStorageCapacity();

			AssertEquals("StorageCapacityRequiredInMB", 4, storage.StorageCapacityRequiredInMB);
			Assert("StorageMainRecordsForArchiving", storage.StorageMainRecordsForArchiving >= 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestArchiveRunOutOfSpaceAfterOneVolume()
		{
			var imageBytes = TestFileHelper.File1MB;

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);

			var storageMain1 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 12));
			var storageMain2 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 13));
			var storageMain3 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 14));

			docFactory.Save();

			var startArchivingTime = ZDateTime.UtcNow.AddMinutes(-1);

			using var tempDir = new TempDirectory();
			var storage = new OfflineStorage()
			{
				ArchiveDateTo = new ZDateTime(2009, 1, 14),
				OfflineLocation = tempDir.DirectoryName
			};
			storage.FreeSpaceInMB = 3;

			using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
			{
				Env.NumberFountains.ArchiveVolumeNo.SetNext(Factory, 1);
				transactionManager.CommitTransaction();
			}

			storage.Archive();

			var volume1 = Path.Combine(storage.OfflineLocation, "Vol-00000001.zip");
			Assert("volume1 exists", File.Exists(volume1));

			AssertEquals("no other volumes created", 2, Directory.GetFiles(storage.OfflineLocation).Length);

			var nextVolumeNo = Env.NumberFountains.ArchiveVolumeNo.PeekPreliminary(Factory);
			AssertEquals("next volume no", 3, nextVolumeNo);

			var docFactoryReloaded = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMain1Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain1.PK);
			var storageMain2Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain2.PK);
			var storageMain3Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain3.PK);

			Assert("storageMain1 SM_OffLine", storageMain1Reloaded.SM_OffLine >= startArchivingTime);
			Assert("storageMain2 SM_OffLine", storageMain2Reloaded.SM_OffLine >= startArchivingTime);
			Assert("storageMain3 SM_OffLine", storageMain3Reloaded.SM_OffLine >= startArchivingTime);

			AssertEquals("storageMain1 eDocs.Count", 0, storageMain1Reloaded.eDocs.Count);
			AssertEquals("storageMain2 eDocs.Count", 0, storageMain2Reloaded.eDocs.Count);
			AssertEquals("storageMain3 eDocs.Count", 0, storageMain3Reloaded.eDocs.Count);

			AssertEquals("storageMain1 volume", 1, storageMain1Reloaded.SM_CD1);
			AssertEquals("storageMain2 volume", 1, storageMain2Reloaded.SM_CD1);
			AssertEquals("storageMain3 volume", 2, storageMain3Reloaded.SM_CD1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestArchiveStopThenArchiveAgain()
		{
			var imageBytes = TestFileHelper.File1MB;

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);

			var storageMain1 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 12));
			var storageMain2 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 13));
			var storageMain3 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 14));

			var storageMain4 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 15));
			var storageMain5 = NewStorageMain(docFactory, imageBytes, new ZDateTime(2009, 1, 16));

			docFactory.Save();

			var startArchivingTime = ZDateTime.UtcNow.AddMinutes(-1);

			using var tempDir = new TempDirectory();
			var storage = new OfflineStorage()
			{
				ArchiveDateTo = new ZDateTime(2009, 1, 14),
				OfflineLocation = tempDir.DirectoryName
			};
			storage.FreeSpaceInMB = 100;

			using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
			{
				Env.NumberFountains.ArchiveVolumeNo.SetNext(Factory, 1);
				transactionManager.CommitTransaction();
			}

			storage.Archive();

			var volumeOutputDirectories = Directory.GetDirectories(storage.OfflineLocation);
			AssertEquals("output directories", 0, volumeOutputDirectories.Length);

			var volumeZipFiles = Directory.GetFiles(storage.OfflineLocation);

			var volume1 = Path.Combine(storage.OfflineLocation, "Vol-00000001.zip");
			var expectedVolumeFiles = new string[] { volume1 };

			AssertArrayEqualsByElements("volume files", expectedVolumeFiles, volumeZipFiles);

			AssertVolumeContent(volume1, storageMain1, storageMain2, storageMain3);

			var nextVolumeNo = Env.NumberFountains.ArchiveVolumeNo.PeekPreliminary(Factory);
			AssertEquals("next volume no", 2, nextVolumeNo);

			var docFactoryReloaded = new DocumentFactoryProvider().GetFactory(Factory);
			var storageMain1Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain1.PK);
			var storageMain2Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain2.PK);
			var storageMain3Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain3.PK);
			var storageMain4Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain4.PK);
			var storageMain5Reloaded = docFactoryReloaded.Load<ArchiveStorageMain>(storageMain5.PK);

			Assert("storageMain1 SM_OffLine", storageMain1Reloaded.SM_OffLine >= startArchivingTime);
			Assert("storageMain2 SM_OffLine", storageMain2Reloaded.SM_OffLine >= startArchivingTime);
			Assert("storageMain3 SM_OffLine", storageMain3Reloaded.SM_OffLine >= startArchivingTime);

			AssertEquals("storageMain4 SM_OffLine", ZDateTime.Empty, storageMain4Reloaded.SM_OffLine);
			AssertEquals("storageMain5 SM_OffLine", ZDateTime.Empty, storageMain5Reloaded.SM_OffLine);

			AssertEquals("storageMain1 eDocs.Count", 0, storageMain1Reloaded.eDocs.Count);
			AssertEquals("storageMain2 eDocs.Count", 0, storageMain2Reloaded.eDocs.Count);
			AssertEquals("storageMain3 eDocs.Count", 0, storageMain3Reloaded.eDocs.Count);
			AssertEquals("storageMain4 eDocs.Count", 2, storageMain4Reloaded.eDocs.Count);
			AssertEquals("storageMain5 eDocs.Count", 2, storageMain5Reloaded.eDocs.Count);

			AssertEquals("storageMain1 volume", 1, storageMain1Reloaded.SM_CD1);
			AssertEquals("storageMain2 volume", 1, storageMain2Reloaded.SM_CD1);
			AssertEquals("storageMain3 volume", 1, storageMain3Reloaded.SM_CD1);
			AssertEquals("storageMain4 volume", 0, storageMain4Reloaded.SM_CD1);
			AssertEquals("storageMain5 volume", 0, storageMain5Reloaded.SM_CD1);

			storage.ArchiveDateTo = new ZDateTime(2009, 1, 16);
			storage.Archive();

			var volume3 = Path.Combine(storage.OfflineLocation, "Vol-00000002.zip");
			expectedVolumeFiles = new string[] { volume1, volume3 };
			volumeZipFiles = Directory.GetFiles(storage.OfflineLocation);
			Array.Sort(volumeZipFiles);
			AssertArrayEqualsByElements("volume paths", expectedVolumeFiles, volumeZipFiles);

			AssertVolumeContent(volume3, storageMain4, storageMain5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOfflineArchive_DoesNotAffectOPSTempTables()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.OPS);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, false);
			var archiveStage = archiveSystem.GetArchiveStages(config).FirstOrDefault();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var logger = new TestArchiveLogger();
			archiveStage.BeginRun(config, schedule, logger);

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			_ = NewStorageMain(docFactory, TestFileHelper.File1MB, new ZDateTime(2009, 1, 12));
			docFactory.Save();

			using (var tempDir = new TempDirectory())
			{
				var storage = new OfflineStorage()
				{
					ArchiveDateTo = new ZDateTime(2009, 1, 14),
					OfflineLocation = tempDir.DirectoryName
				};
				storage.FreeSpaceInMB = 100;
				storage.Archive();
			}

			AssertNoExceptionThrown(() => archiveStage.GetNextArchiveSet(null, schedule, archiveStage));
		}

		void SaveDocumentToS3(StorageDocsBase storageDoc, byte[] dataForTest)
		{
			var awsPersister = (AWSPersisterForTest)ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3);
			awsPersister.VersionIdForTest = "randomVersionID";
			Assert("SaveToExternalStorage with encryption", storageDoc.SaveToExternalStorage());
			storageDoc.SC_ImageData = ZBlob.Empty;
			AssertNoExceptionThrown(() => storageDoc.ParentMain.Factory.Save());

			Assert("IsMovingToExternalStorage", storageDoc.IsMovingToExternalStorage);
			AssertEquals("SC_UncompressedSize", dataForTest.Length, storageDoc.SC_UncompressedSize);
			AssertEquals("SC_VersionID", "randomVersionID", storageDoc.SC_VersionID);
		}

		[TestDate(2022, 02, 20)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOfflineArchive_WhenS3IsEnabled()
		{
			var dataForTest = TestFileHelper.File1MB;
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageDoc = StorageDocs.NewWithParentWithoutFK_DEBUG(masterFactory.GetFactory(DbNumber));
			storageDoc.SC_ImageData = dataForTest;
			storageDoc.ParentMain.SM_DB = DbNumber;
			storageDoc.ParentMain.SM_Archived = ZDateTime.Today.AddDays(-1);
			masterFactory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.local"))
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=dude;Secret=top"))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(supportUpload: true, supportDownload: true))
			{
				SaveDocumentToS3(storageDoc, dataForTest);

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("EDocsStorageProvider should be set to 'S3'.", Core.Constants.EDocsStorageProviders.Code.S3, SystemDataRegistry.Instance.EDocsStorageProvider.Value);
					AssertEquals("StorageMain should have 1 document.", 1, storageDoc.ParentMain.eDocs.Count);

					AssertEquals($"'{StorageMainSchema.Constants.SM_Archived}' should contain the archived date.", ZDateTime.Today.AddDays(-1), storageDoc.ParentMain.SM_Archived);
					AssertEquals($"'{StorageMainSchema.Constants.SM_CD1}' should be the default.", 0, storageDoc.ParentMain.SM_CD1);
					Assert($"'{StorageMainSchema.Constants.SM_OffLine}' should be empty.", storageDoc.ParentMain.SM_OffLine.IsEmpty);
				});

				using var tempDirectory = new TempDirectory();
				var offlineStorage = new OfflineStorage()
				{
					ArchiveDateTo = ZDateTime.Today,
					OfflineLocation = tempDirectory.DirectoryName,
				};

				CombineAssertions(() =>
				{
					AssertEquals($"There should be 1 record to Offline Archive.", 1, offlineStorage.StorageMainRecordsForArchiving);
					AssertEquals($"There should be 1MB of data to Offline Archive.", 1, offlineStorage.StorageCapacityRequiredInMB);
				});

				offlineStorage.Archive();
				var volume = Path.Combine(offlineStorage.OfflineLocation, "Vol-00000001.zip");
				storageDoc.ParentMain.Reload();

				CombineAssertions("Offline Archive should work correctly with S3.", () =>
				{
					Assert("Volume was created.", File.Exists(volume));
					AssertEquals("No other volumes were created.", 1, Directory.GetFiles(offlineStorage.OfflineLocation).Length);

					Assert("When S3 is Enabled, for each document deleted, a record should be added to StorageDocsToDelete table.", new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.SCD_StorageDocIdentifier, storageDoc.PK)));
					AssertEquals("StorageMain should have no eDocs.", 0, storageDoc.ParentMain.eDocs.Count);

					AssertEquals($"'{StorageMainSchema.Constants.SM_CD1}' should have been updated to 1.", 1, storageDoc.ParentMain.SM_CD1);
					Assert($"'{StorageMainSchema.Constants.SM_OffLine}' should contain a date.", !storageDoc.ParentMain.SM_OffLine.IsEmpty);
				});
			}
		}

		ArchiveStorageMain NewStorageMain(DocumentFactory docFactory, byte[] imageBytes, ZDateTime archivedDate)
		{
			var storageMain = docFactory.New<ArchiveStorageMain>();
			storageMain.SM_DB = DbNumber;
			var dto = new AddFileOrDocumentDto { DocumentType = "MSC" };
			dto.FileName = "testHenry";
			_ = storageMain.AddFileOrDocument(imageBytes, dto);
			dto.FileName = "testHenry2";
			_ = storageMain.AddFileOrDocument(imageBytes, dto);
			storageMain.SM_Archived = archivedDate;
			storageMain.SM_ParentFK = ZGuid.NewZGuid();

			return storageMain;
		}

		void AssertVolumeContent(string volumeFile, params StorageMain[] expectedStorageMains)
		{
			var storageMainPKs = new List<Guid>();

			var volume = ArchiveVolume.LoadVolume(volumeFile);
			foreach (var storageMainPK in volume.GetAllStorageMainPKs())
			{
				storageMainPKs.Add(storageMainPK);
			}

			foreach (var main in expectedStorageMains)
			{
				_ = storageMainPKs.Remove(main.PK.ToGuid());
			}

			Assert("Volume content does not match", storageMainPKs.Count == 0);
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			if (!dbHelper.DatabaseExists(DbNumber))
			{
				_ = dbHelper.CreateDatabase(DbNumber);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			if (dbHelper.DatabaseExists(DbNumber))
			{
				var dbName = dbHelper.GetDatabaseName(DbNumber);
				dbHelper.DropDatabase(dbName);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new OfflineStorage();

		const int DbNumber = 333;
		readonly DocManagerDBHelperTestClass dbHelper = new();
	}
}
