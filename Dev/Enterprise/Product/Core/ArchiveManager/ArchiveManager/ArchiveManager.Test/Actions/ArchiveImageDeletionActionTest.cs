using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusInbondBillAddRef = Enterprise.Customs.US.InBond.Business.CusInbondBillAddRef;

namespace Enterprise.ArchiveManager.Test.Actions
{
	sealed class ArchiveImageDeletionActionTest : TestCase
	{
		const string DummyStageName = "Dummy Stage Name";

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestExecute()
		{
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			var logger = new TestArchiveLogger();
			var systemCache = new ArchiveSystemCache();
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);

			var dummyBizo = factory.NewWithValidTestData<DummyWithDocumentSupport>();
			dummyBizo.Z0_Description = "DummyBizo";
			dummyBizo.Z0_Code = "DMY01";
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), DummyStageName, Guid.Empty, mainArchiveItem);
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();

			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = DbNumber;
			storageMain.SM_ParentFK = mainArchiveItem.PK;

			var storageDoc1 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc1.SC_SM = storageMain.PK;
			storageDoc1.SC_FileName = "mainArchiveItem.pdf";
			storageDoc1.SC_ImageData = TestFileHelper.SamplePDF;

			documentFactory.Save();
			factory.Save();

			AssertDatabaseCount("Precondition", dummyBizoCount: 1, storageMainCount: 1, storageDocsCount: 1, documentFactory, documentFactoryForDB333);

			SetupForArchiveImageDeletionAction(archiveImageDeletionAction, logger, archiveSet, configuration);
			archiveImageDeletionAction.Execute();

			AssertDatabaseCount("ArchiveSet records should still exist, but their documents should have been deleted.", dummyBizoCount: 1, storageMainCount: 0, storageDocsCount: 0, documentFactory, documentFactoryForDB333);

			CombineAssertions("Logs did not contain correct message", () =>
			{
				AssertEquals(1, logger.ListOfMessages.Count);
				AssertEquals("Information|DMY|Deleted all documents related to DMY01 - DummyBizo", logger.ListOfMessages.First());
			});
		}

		[UseSnapshotProtection]
		public void TestReproduceNoConcreteTypeException_CusInbondBillAddRef()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();
			var logger = new TestArchiveLogger();
			var cusInBondBillAddReff = factory.NewWithValidTestData<CusInbondBillAddRef>();
			var mainArchiveItem = new ArchiveItem(cusInBondBillAddReff.PKSchemaColumn, cusInBondBillAddReff.PK.ToGuid(), null, Guid.Empty, false, cusInBondBillAddReff.TablePrefix);
			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), "Dummy Stage Name", Guid.Empty, mainArchiveItem);
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			factory.Save();

			SetupForArchiveImageDeletionAction(archiveImageDeletionAction, logger, archiveSet, configuration);

			// Act
			// Assert
			AssertNoExceptionThrown(() => archiveImageDeletionAction.Execute());
		}

		[UseSnapshotProtection]
		public void TestReprooduceCusInBondContainerError()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();
			var logger = new TestArchiveLogger();
			var nctsHeader = factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			var cusInBondContainer = factory.New<Enterprise.Integration.Customs.EU.NCTS.INctsCusInBondContainer>();
			cusInBondContainer.BC_ParentID = nctsHeader.PK;
			cusInBondContainer.BC_ParentTableCode = "BH";
			var mainArchiveItem = new ArchiveItem(cusInBondContainer.PKSchemaColumn, cusInBondContainer.PK.ToGuid(), null, Guid.Empty, false, cusInBondContainer.TablePrefix);
			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), DummyStageName, Guid.Empty, mainArchiveItem);
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			factory.Save();

			SetupForArchiveImageDeletionAction(archiveImageDeletionAction, logger, archiveSet, configuration);

			// Act
			// Assert
			AssertNoExceptionThrown(() => archiveImageDeletionAction.Execute());
		}

		[UseSnapshotProtection]
		public void TestReproduceNoConcreteTypeException_CusInvPack()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestArchiveLogger();
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(factory, ArchiveManagerConstants.Codes.PDO);

			var declaration = factory.New<BaseJobDeclaration>();

			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;

			_ = declaration.Invoices.AddNew();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();

			var cusInvPack = factory.New<Enterprise.Integration.Customs.EU.NCTS.INctsPackage>();
			cusInvPack.B5_ParentID = jobComInvoiceLine.PK;
			cusInvPack.B5_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			factory.Save();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, logger, schedule));
				Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestExecute_WhenIsVerboseLogging()
		{
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			var logger = new TestArchiveLogger();
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, true, false);

			var dummyBizo = factory.NewWithValidTestData<DummyWithDocumentSupport>();
			dummyBizo.Z0_Description = "DummyBizo";
			dummyBizo.Z0_Code = "DMY01";
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), DummyStageName, Guid.Empty, mainArchiveItem);

			var archiveImageDeletionAction = new ArchiveImageDeletionAction();

			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = DbNumber;
			storageMain.SM_ParentFK = mainArchiveItem.PK;

			var storageDoc1 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc1.SC_SM = storageMain.PK;
			storageDoc1.SC_FileName = "mainArchiveItem";
			storageDoc1.SC_Desc = "Accounting";
			storageDoc1.SC_ImageData = TestFileHelper.SamplePDF;

			var storageDoc2 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc2.SC_SM = storageMain.PK;
			storageDoc2.SC_FileName = "anothermainArchiveItem";
			storageDoc2.SC_Desc = "Rating";
			storageDoc2.SC_ImageData = TestFileHelper.SamplePDF;

			documentFactory.Save();
			factory.Save();

			AssertDatabaseCount("Precondition", dummyBizoCount: 1, storageMainCount: 1, storageDocsCount: 2, documentFactory, documentFactoryForDB333);

			SetupForArchiveImageDeletionAction(archiveImageDeletionAction, logger, archiveSet, configuration);
			archiveImageDeletionAction.Execute();

			AssertDatabaseCount("ArchiveSet records should still exist, but their documents should have been deleted.", dummyBizoCount: 1, storageMainCount: 0, storageDocsCount: 0, documentFactory, documentFactoryForDB333);

			CombineAssertions("Logs did not contain the correct messages", () =>
			{
				AssertEquals(3, logger.ListOfMessages.Count);
				AssertEquals("Log 1: ", "Information|DMY|Deleted document Accounting - mainArchiveItem.tif.", logger.ListOfMessages.First());
				AssertEquals("Log 2: ", "Information|DMY|Deleted document Rating - anothermainArchiveItem.tif.", logger.ListOfMessages[1]);
				AssertEquals("Log 3: ", "Information|DMY|Deleted all documents related to DMY01 - DummyBizo", logger.ListOfMessages[2]);
			});
		}

		public void TestExecute_WithoutSetup()
		{
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();
			var expectedExceptionMessage = "Setup(..) must be called before you can call Execute() on DeleteArchiveImageAction object";

			AssertExceptionThrown(typeof(InvalidOperationException), expectedExceptionMessage, () => archiveImageDeletionAction.Execute());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestExecute_WhenArchviveSetDoesNotContainAnyDocs()
		{
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			var logger = new TestArchiveLogger();
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, true, false);

			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), DummyStageName, Guid.Empty, mainArchiveItem);
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();
			var nonArchiveItem = factory.NewWithValidTestData<DummyBusinessObject>();

			var storageMain1 = documentFactory.New<StorageMain>();
			storageMain1.SM_DB = DbNumber;
			storageMain1.SM_ParentFK = nonArchiveItem.PK;

			var storageDoc1 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc1.SC_SM = storageMain1.PK;
			storageDoc1.SC_FileName = "archiveItem.pdf";
			storageDoc1.SC_ImageData = TestFileHelper.SamplePDF;

			factory.Save();
			documentFactory.Save();

			AssertDatabaseCount("Precondition", dummyBizoCount: 2, storageMainCount: 1, storageDocsCount: 1, documentFactory, documentFactoryForDB333);

			SetupForArchiveImageDeletionAction(archiveImageDeletionAction, logger, archiveSet, configuration);
			archiveImageDeletionAction.Execute();

			AssertDatabaseCount("Documents outside the ArchiveSet should not be deleted.", dummyBizoCount: 2, storageMainCount: 1, storageDocsCount: 1, documentFactory, documentFactoryForDB333);
			storageDoc1.Delete();
			documentFactory.Save();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestExecute_WhenArchiveSetContainsMultipleArchiveItems()
		{
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			var logger = new TestArchiveLogger();
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);

			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Description = "DummyBizo";
			dummyBizo.Z0_Code = "DMY01";
			var dummyBizo1 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo1.Z0_Description = "DummyBizo";
			dummyBizo1.Z0_Code = "DMY02";
			var dummyBizo2 = factory.NewWithValidTestData<DummyBusinessObject>();
			var mainArchiveItem = new ArchiveItem(dummyBizo.PKSchemaColumn, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, dummyBizo.TablePrefix);
			var archiveItem1 = new ArchiveItem(dummyBizo1.PKSchemaColumn, dummyBizo1.PK.ToGuid(), null, Guid.Empty, false, dummyBizo1.TablePrefix);
			var archiveItem2 = new ArchiveItem(dummyBizo2.PKSchemaColumn, dummyBizo2.PK.ToGuid(), null, Guid.Empty, false, dummyBizo2.TablePrefix);

			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), DummyStageName, Guid.Empty, mainArchiveItem, dummyBizo.Z0_Code, new List<IArchiveItem>() { archiveItem1, archiveItem2 });
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();

			var storageMain1 = documentFactory.New<StorageMain>();
			storageMain1.SM_DB = DbNumber;
			storageMain1.SM_ParentFK = mainArchiveItem.PK;

			var storageMain2 = documentFactory.New<StorageMain>();
			storageMain2.SM_DB = DbNumber;
			storageMain2.SM_ParentFK = archiveItem1.PK;

			var storageDoc1 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc1.SC_SM = storageMain1.PK;
			storageDoc1.SC_FileName = "mainArchiveItem.pdf";
			storageDoc1.SC_ImageData = TestFileHelper.SamplePDF;

			var storageDoc2 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc2.SC_SM = storageMain2.PK;
			storageDoc2.SC_FileName = "archiveItem.pdf";
			storageDoc2.SC_ImageData = TestFileHelper.SamplePDF;

			documentFactory.Save();
			factory.Save();

			AssertDatabaseCount("Precondition", dummyBizoCount: 3, storageMainCount: 2, storageDocsCount: 2, documentFactory, documentFactoryForDB333);

			SetupForArchiveImageDeletionAction(archiveImageDeletionAction, logger, archiveSet, configuration);
			archiveImageDeletionAction.Execute();

			AssertDatabaseCount("ArchiveSet records should still exist, but their documents should have been deleted.", dummyBizoCount: 3, storageMainCount: 0, storageDocsCount: 0, documentFactory, documentFactoryForDB333);

			CombineAssertions("Logs did not contain the correct messages", () =>
			{
				AssertEquals(2, logger.ListOfMessages.Count);
				AssertEquals("Log 1: ", "Information|DMY|Deleted all documents related to DMY01 - DummyBizo", logger.ListOfMessages.First());
				AssertEquals("Log 2: ", "Information|DMY|Deleted all documents related to DMY02 - DummyBizo", logger.ListOfMessages.Last());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestExecute_CanDeleteDocumentsFromJobContainer()
		{
			var factory = new BusinessObjectFactory();
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			var logger = new TestArchiveLogger();
			var systemCache = new ArchiveSystemCache();
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);

			var consol = factory.NewWithValidTestData<CommonConsol>();
			_ = consol.Containers.AddNew();
			var container = consol.Containers[0];
			var jobHeader = factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = consol.PK;
			jobHeader.JH_ParentTableCode = consol.TablePrefix;
			jobHeader.JH_JobNum = consol.JK_UniqueConsignRef;

			var mainArchiveItem = new ArchiveItem(jobHeader.PKSchemaColumn, jobHeader.PK.ToGuid(), null, Guid.Empty, false, jobHeader.TablePrefix);
			var archiveItem1 = new ArchiveItem(consol.PKSchemaColumn, consol.PK.ToGuid(), null, Guid.Empty, false, consol.TablePrefix);
			var archiveItem2 = new ArchiveItem(container.PKSchemaColumn, container.PK.ToGuid(), null, Guid.Empty, false, container.TablePrefix);

			var archiveSet = new TestArchiveSet(new DummyArchiveSystem(), DummyStageName, Guid.Empty, mainArchiveItem, jobHeader.JH_JobNum, new List<IArchiveItem>() { archiveItem1, archiveItem2 });
			var archiveImageDeletionAction = new ArchiveImageDeletionAction();

			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = DbNumber;
			storageMain.SM_ParentFK = container.PK;

			var storageDoc1 = documentFactoryForDB333.New<StorageDocs>();
			storageDoc1.SC_SM = storageMain.PK;
			storageDoc1.SC_FileName = "mainArchiveItem.pdf";
			storageDoc1.SC_ImageData = TestFileHelper.SamplePDF;

			documentFactory.Save();
			factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"Table: {JobContainerSchema.Constants.TableName}.", 1, factory.GetDatabaseCount(consol.Containers[0].GetType()));
				AssertEquals($"Table: {StorageMainSchema.Constants.TableName}.", 1, documentFactory.GetDatabaseCount(typeof(StorageMain)));
				AssertEquals($"Table: {StorageDocsSchema.Constants.TableName}.", 1, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs)));
			});

			archiveImageDeletionAction.Setup(logger, archiveSet, systemCache, configuration);
			archiveImageDeletionAction.Execute();

			CombineAssertions("ArchiveSet records should still exist, but their documents should have been deleted.", () =>
			{
				AssertEquals($"Table: {JobContainerSchema.Constants.TableName}.", 1, factory.GetDatabaseCount(consol.Containers[0].GetType()));
				AssertEquals($"Table: {StorageMainSchema.Constants.TableName}.", 0, documentFactory.GetDatabaseCount(typeof(StorageMain)));
				AssertEquals($"Table: {StorageDocsSchema.Constants.TableName}.", 0, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs)));
			});

			CombineAssertions("Logs did not contain the correct message", () =>
			{
				AssertEquals(1, logger.ListOfMessages.Count);
				AssertEquals("Information|DMY|Deleted all documents related to Container", logger.ListOfMessages[0]);
			});
		}

		void AssertDatabaseCount(string message, int dummyBizoCount, int storageMainCount, int storageDocsCount, DocumentFactory documentFactory, NumberedBusinessObjectFactory documentFactoryForDB333)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals($"Table: {DummyBizoSchema.Constants.TableName}.", dummyBizoCount, documentFactory.GetDatabaseCount(typeof(DummyBusinessObject)));
				AssertEquals($"Table: {StorageMainSchema.Constants.TableName}.", storageMainCount, documentFactory.GetDatabaseCount(typeof(StorageMain)));
				AssertEquals($"Table: {StorageDocsSchema.Constants.TableName}.", storageDocsCount, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs)));
			});
		}

		void SetupForArchiveImageDeletionAction(ArchiveImageDeletionAction archiveImageDeletionAction, TestArchiveLogger logger, TestArchiveSet archiveSet, IArchiveConfiguration config)
		{
			var providerDictionary = new ArchiveableBusinessObjectProviderCache();
			providerDictionary.RegisterProvider(new DummyArchiveableBusinessObjectProvider());
			var systemCache = new ArchiveSystemCache();

			archiveImageDeletionAction.Setup(logger, archiveSet, systemCache, config);
		}

		#region DbHelper

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

		#endregion
	}
}
