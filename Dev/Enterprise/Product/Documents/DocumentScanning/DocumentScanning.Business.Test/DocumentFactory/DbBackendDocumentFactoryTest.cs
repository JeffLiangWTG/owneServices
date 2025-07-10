using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Testing;
using Enterprise.PrintProcessing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class DocumentFactoryForParentFactoryTest : NumberedBusinessObjectFactoryForParentFactoryTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSC_FileNameAutoTruncatedWhenAllocating()
		{
			var documentFactory = new DbBackendDocumentFactory(Factory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var longFileName = "9666767,9669368,194850045,194850046,194850047,194850048,194850049,194849921,4502954512,4502959021,4502976577,4502976829,4502980492,4502971036,33682648,33682649,33682650,33682651,33682652,33678439,207134298,207167866,207265469,207280675,207292837,207232684,456789";
			var file = documentFactory.CreateAndAllocateDocument(shipment.PK, "SHP", PrintProcessingConstants.Small, "MSC", "Test Document", false, longFileName, "") as StorageDocsBase;
			AssertEquals("9666767,9669368,194850045,194850046,194850047,194850048,194850049,194849921,4502954512,4502959021,4502976577,4502976829,4502980492,4502971036,33682648,33682649,33682650,33682651,33682652,33678439,207134298,207167866,207265469,207280675,207292837,207232684,", file.SC_FileName);

			var normalFileName = "Normal Name";
			file = documentFactory.CreateAndAllocateDocument(shipment.PK, "SHP", PrintProcessingConstants.Small, "MSC", "Test Document", false, normalFileName, "") as StorageDocsBase;
			AssertEquals("Normal Name", file.SC_FileName);
		}

		protected override NumberedBusinessObjectFactory GetFactory(BusinessObjectFactory factory)
		{
			return new DocumentFactoryProvider().GetFactory(factory);
		}

		public void TestConstructorDoesntLetWrongFactoryTypeThtough()
		{
			{
				var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var result = new DocumentFactoryProvider().GetFactory(factory);
				AssertNotEquals(factory, result.FactoryForEverythingExceptEDocs);
			}
			{
				var factory = new DocumentFactoryProvider().GetFactory(new NumberedBusinessObjectFactory(0, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory())));
				var result = new DocumentFactoryProvider().GetFactory(factory);
				AssertNotEquals(factory, result.FactoryForEverythingExceptEDocs);
			}
			{
				var factory = new BusinessObjectFactory();
				var result = new DocumentFactoryProvider().GetFactory(factory);
				AssertEquals(factory, result.FactoryForEverythingExceptEDocs);
			}
		}

		public void TestFactoryForSavingEverythingExceptEDocs()
		{
			var fac = new BusinessObjectFactory();
			var docFac = new DocumentFactoryProvider().GetFactory(fac);
			AssertEquals(fac, docFac.FactoryForEverythingExceptEDocs);

			docFac = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertNotNull(docFac.FactoryForEverythingExceptEDocs);
			AssertEquals(typeof(BusinessObjectFactory), docFac.FactoryForEverythingExceptEDocs.GetType());
		}

		public override void TestOnlyAllowedObjectsAreInFactory_LoadArray()
		{
			base.TestOnlyAllowedObjectsAreInFactory_LoadArray();
			AssertOnlyAllowedObjectsAreInFactory_LoadArray<StorageMain>(false, CreateRandomStorageMain, StorageMainSchema.PK);
		}

		public override void TestOnlyAllowedObjectsAreInFactory_LoadFromUniqueKey()
		{
			base.TestOnlyAllowedObjectsAreInFactory_LoadFromUniqueKey();
			AssertOnlyAllowedObjectsAreInFactory_LoadFromUniqueKey<StorageMain>(false, CreateRandomStorageMain, StorageMainSchema.SM_ParentFK);
		}

		public override void TestOnlyAllowedObjectsAreInFactory_LoadOne()
		{
			base.TestOnlyAllowedObjectsAreInFactory_LoadOne();
			AssertOnlyAllowedObjectsAreInFactory_LoadOne<StorageMain>(false, CreateRandomStorageMain, StorageMainSchema.PK, "SM");
		}

		public override void TestOnlyAllowedObjectsAreInFactory_LoadTopOne()
		{
			base.TestOnlyAllowedObjectsAreInFactory_LoadTopOne();
			AssertOnlyAllowedObjectsAreInFactory_LoadTopOne<StorageMain>(false, CreateRandomStorageMain, StorageMainSchema.PK);
		}

		public override void TestOnlyAllowedObjectsAreInFactory_New()
		{
			base.TestOnlyAllowedObjectsAreInFactory_New();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageMain)).Factory);
		}

		public override void TestOnlyAllowedObjectsAreInFactory_NewTemplate()
		{
			base.TestOnlyAllowedObjectsAreInFactory_NewTemplate();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			AssertEquals(docFactory, docFactory.New<StorageMain>().Factory);
		}

		public override void TestOnlyAllowedObjectsAreInFactory_NewWithPrimaryKey()
		{
			base.TestOnlyAllowedObjectsAreInFactory_NewWithPrimaryKey();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			AssertEquals(docFactory, docFactory.NewWithPrimaryKey<StorageMain>(Guid.NewGuid()).Factory);
		}

		public override void TestOnlyAllowedObjectsAreInFactory_NewWithTypeAndPK()
		{
			base.TestOnlyAllowedObjectsAreInFactory_NewWithTypeAndPK();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			AssertEquals(docFactory, docFactory.New(typeof(StorageMain), Guid.NewGuid()).Factory);
		}

		public override void TestOnlyAllowedObjectsAreInFactoryAddToCollection()
		{
			base.TestOnlyAllowedObjectsAreInFactoryAddToCollection();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NumberedBusinessObjectFactory docFactory = GetFactory(factory);
			AssertEquals(docFactory, new StorageMainCollection(docFactory).AddNew().Factory);
		}
	}

	sealed class DocumentFactoryTest : TestCaseWithDocumentFactory
	{
		public void TestCreateAndAllocateDocumentNotAllowEmptyFile()
		{
			var log = new List<LogEventArgs>();
			MasterFactory.Log += args => { log.Add(args); };
			using (var tempDir = new TempDirectory())
			{
				var filePath = Temp.GetTempFileName(tempDir);
				var file = MasterFactory.CreateAndAllocateDocument(Guid.NewGuid(), "SHP", filePath, "MSC", "Jerry Test Document", false);

				AssertNull("Should not allow allocate empty file", file);
				AssertEquals("Cannot allocate document 'Jerry Test Document' of type 'MSC': Document is empty.", log[0].Message);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateAndAllocateDocumentWithExistingFileIsEmpty()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var file = MasterFactory.CreateAndAllocateDocument(shipment.PK, "SHP", PrintProcessingConstants.Small, "MSC", "Test Document", false) as StorageDocsBase;
			file.SC_ImageData = Array.Empty<byte>();
			file.SC_UncompressedSize = 0;
			MasterFactory.Save();

			var externalPersisterProvider = new Mock<IExternalPersisterProvider>();
			var externalPersister = new Mock<IExternalPersister>();
			externalPersister.Setup(p => p.RetrieveStream(It.IsAny<ZGuid>()))
							.Throws(new ExternalStorageObjectNotFoundException(file.PK.ToString(), "Document doesn't exist in external storage.", Core.Constants.EDocsStorageProviders.Code.S3, null));
			externalPersisterProvider.Setup(p => p.GetExternalPersister(It.IsAny<string>())).Returns(externalPersister.Object);
			using (ObjectFactory.Substitute(externalPersisterProvider.Object))
			{
				ZBlob expectedImageData = DocumentUtilities.GetFileAsBytes(PrintProcessingConstants.Small);
				AssertEquals("Small file is not empty", false, expectedImageData.IsEmpty);

				StorageDocsBase newFile = null;
				AssertNoExceptionThrown(() => newFile = MasterFactory.CreateAndAllocateDocument(shipment.PK, "SHP", PrintProcessingConstants.Small, "MSC", "Test Document", false) as StorageDocsBase);
				AssertEquals("The new file should already exist", file.PK, newFile.PK);
				AssertEquals("The new file SC_ImageData should be set successfully", expectedImageData, newFile.SC_ImageData);
			}
		}

		public void TestDeleteOrphanWithTheSamePK()
		{
			if (!DbHelper.DatabaseExists(2))
			{
				DbHelper.CreateDatabase(2);
			}

			// Create a document in DB 2
			var document = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory.GetFactory(2));
			document.SC_Desc = "Not orphan";
			MasterFactory.Save();

			// Create an orphan document in DB 1 with the same PK to the non-orphan document
			var insertDuplicatedStorageDocsSql = $@"
INSERT [{MasterFactory.GetFactory(1).DBName}]..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData, SC_Desc)
VALUES ('{document.PK}', '{document.ParentMain.PK}', '{document.SC_DocType}',  sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), 0x010203, 'Orphan');";
			Db.Connection.ExecuteNonQuery(insertDuplicatedStorageDocsSql);

			var orphanDoc = MasterFactory.GetFactory(1).Load<StorageDocsBase>(document.PK);

			// verify orphan and non-orphan before delete
			AssertEquals("orphan doc and non-orphan doc have the same PK", document.PK, orphanDoc.PK);
			AssertNotEquals("orphan doc and non-orphan doc are not the same doc", document.SC_Desc, orphanDoc.SC_Desc);

			AssertEquals("Orpha Doc should be in DB 1", 1, ((NumberedBusinessObjectFactory)orphanDoc.Factory).DBNumber);
			AssertNotEquals("Orphan Doc's parentMain is not pointing to it", orphanDoc.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)orphanDoc.Factory).DBNumber);

			AssertEquals("Non-orphan doc should be in DB 2", 2, ((NumberedBusinessObjectFactory)document.Factory).DBNumber);
			AssertEquals("Non-orphan doc's parentMain is pointing to it", document.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)document.Factory).DBNumber);

			// delete the orphan in DB 1 (and turn off RefreshEnabled so it won't broadcast the deletion to other factories)
			orphanDoc.Factory.RefreshEnabled = false;
			orphanDoc.Delete();
			MasterFactory.Save();

			// Verify with loaded docs and existing master factory. With RefreshEnabled off, the delete only happens to the intended child factory of DB 1
			AssertEquals("Orphan should be deleted", true, orphanDoc.IsDeleted);
			AssertEquals("Non-orphan should not be deleted", false, document.IsDeleted);
			document = MasterFactory.GetFactory(2).Load<StorageDocs>(document.PK);
			AssertEquals("Non-orphan (reloaded) should not be deleted", false, document.IsDeleted);

			// Verify again with a new master factory to ensure the non orphan is not deleted other db, and the orphan is indeed deleted in the intended DB 1
			var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertNull("Orphan can't be reload from DB 1", newMasterFactory.GetFactory(1).Load<StorageDocsBase>(document.PK));
			var reloadDoc = newMasterFactory.GetFactory(2).Load<StorageDocsBase>(document.PK);
			AssertNotNull("Non-orphan doc can be reloaded from DB 1", reloadDoc);
			AssertEquals("Non-orphan doc is not deleted", false, reloadDoc.IsDeleted);
		}

		public void TestSaveFactoriesExceptFactoryForEverythingExceptEDocs()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			factory.Save();

			var docManager = staff.DocManagerInfo;
			docManager.UseBusinessEntityFactoryAsInternal = true;

			staff.GS_Code = "BBB";
			staff.DocManagerInfo.AddFileOrDocument(new byte[3] { 1, 2, 3 }, "text1.txt", "COR");
			staff.DocManagerInfo.MasterFactory.Save();
			var newFactory1 = new BusinessObjectFactory();
			var staffReload1 = newFactory1.Load<GlbStaff>(staff.PK);
			AssertEquals("There should be 1 edoc.", 1, staffReload1.DocManagerInfo.AllEDocs.Count);
			AssertEquals("staff code should be BBB.", "BBB", staffReload1.GS_Code);

			staff.GS_Code = "CCC";
			staff.DocManagerInfo.AddFileOrDocument(new byte[3] { 1, 2, 3 }, "text2.txt", "COR");
			staff.DocManagerInfo.MasterFactory.SaveFactoriesExceptFactoryForEverythingExceptEDocs();
			var newFactory2 = new BusinessObjectFactory();
			var staffReload2 = newFactory2.Load<GlbStaff>(staff.PK);
			AssertEquals("There should be 2 edocs.", 2, staffReload2.DocManagerInfo.AllEDocs.Count);
			AssertEquals("staff code should be BBB as we don't save the bizo factory.", "BBB", staffReload2.GS_Code);
		}

		public void TestChildFactories()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(factory);
			AssertEquals(1, docFactory.ChildFactories.Count);
			AssertEquals(factory, docFactory.ChildFactories[0]);
		}

		public void TestGetFactory()
		{
			NumberedBusinessObjectFactory one = MasterFactory.GetFactory(1);
			AssertEquals(one.DBNumber, 1);
		}

		public void TestGetDatabaseName()
		{
			AssertEquals(Db.DatabaseName + "_SD001", MasterFactory.GetDatabaseName(1));
			AssertEquals(Db.DatabaseName + "_SD999", MasterFactory.GetDatabaseName(999));
		}

		public void TestLastWriteableDatabaseWithFreeSpace()
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			int dbInUse = masterFactory.LastWriteableDatabaseWithFreeSpace();
			AssertEquals("DB in use should be number 1", 1, dbInUse);
		}

		public void TestSavingMultipleFactories()
		{
			SetupParent();
			StorageDocs doc = Parent.Documents.AddNew();
			doc.SC_ImageData = SmallTifBytes;
			NumberedBusinessObjectFactory numFactory = doc.Factory as NumberedBusinessObjectFactory;
			AssertEquals("New document created does not have the correct factory as specified by the parent", Parent.SM_DB, numFactory.DBNumber);

			doc.SC_Date = ZDateTime.Now;

			MasterFactory.Save();
			Parent.Reload();
			AssertEquals("After save and reload, the parent has not found its child doc", 1, Parent.Documents.Count);
		}

		public void TestCreateAndAllocateDocumentWithSqlError()
		{
			StorageMainCollection oldParentMainCollection = new StorageMainCollection(MasterFactory);
			oldParentMainCollection.Load();

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			OrgHeader org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			RefDocType newDocType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			newDocType.RT_DocType = "CRE";
			newDocType.RT_Desc = "Credit Application";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			MasterFactory.Save();

			StorageMain storageMain = MasterFactory.New<StorageMain>();
			storageMain.SM_DB = 999;
			storageMain.SM_ParentFK = org.PK;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			MasterFactory.Save();

			AssertExceptionThrown(typeof(EDocsOffLineException), delegate
			{
				((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(org.PK, ((IDocManagerSupport)org).DocManagerInfo.DocManagerCode, SmallThreePagesTif, "CRE", "EDI (Sydney) - Credit Application");
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateAndAllocateDocumentWithSameFileNameShouldUseUniqueFileName()
		{
			var documentFactory = new DbBackendDocumentFactory(Factory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var fileName = Path.GetFileNameWithoutExtension(PrintProcessingConstants.Small);
			var file = documentFactory.CreateAndAllocateDocument(shipment.PK, "SHP", PrintProcessingConstants.Small, "MSC", "Test Document", false, fileName, "") as StorageDocsBase;
			file.SC_DataType = "XLS";
			AssertEquals("Small", file.SC_FileName);
			documentFactory.Save();

			file = documentFactory.CreateAndAllocateDocument(shipment.PK, "SHP", PrintProcessingConstants.Small, "MSC", "Test Document", false, fileName, "") as StorageDocsBase;
			AssertEquals("Small[2]", file.SC_FileName);
		}

		public void TestCreateAndAllocateDocument()
		{
			StorageMainCollection oldParentMainCollection = new StorageMainCollection(MasterFactory);
			oldParentMainCollection.Load();

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			OrgHeader org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			RefDocType newDocType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			newDocType.RT_DocType = "CRE";
			newDocType.RT_Desc = "Credit Application";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_SE_NKDocumentReceivedEvent = "DCF";
			MasterFactory.Save();

			Assert("Precondition: DDA - DocumentAllocated event should NOT be logged against Org yet", org.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocated.Code)).Length == 0);

			var printJobPKAsString = ZGuid.NewZGuid().ToString();
			StorageDocs returnedDoc = (StorageDocs)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(org.PK, ((IDocManagerSupport)org).DocManagerInfo.DocManagerCode, SmallThreePagesTif, "CRE", "EDI (Sydney) - Credit Application", false, "", "", "", printJobPKAsString);
			AssertNotNull("Document should have successfully been created", returnedDoc);
			Assert("Document is marked as system generated", returnedDoc.SC_IsSystemGenerated);
			AssertEquals("Doc Type", "CRE", returnedDoc.SC_DocType);
			AssertEquals("Description shouldn't get set to the CRE default because it's sysgenerated- instead it should default to what was passed into to the method", "EDI (Sydney) - Credit Application", returnedDoc.SC_Desc);

			StorageMainCollection newParentMainCollection = new StorageMainCollection(MasterFactory);
			newParentMainCollection.Load();
			AssertEquals("Only one more storagemain record has been created", oldParentMainCollection.Count + 1, newParentMainCollection.Count);

			ZQuery filter2 = new ZQuery(StorageMainSchema.SM_ParentFK, org.PK);
			StorageMain docParentMain = MasterFactory.LoadTop1(typeof(StorageMain), filter2) as StorageMain;

			AssertEquals("Document should have successfully been assigned", docParentMain, returnedDoc.ParentMain);
			AssertEquals("Document is saved in the correct factory", docParentMain.SM_DB, ((NumberedBusinessObjectFactory)returnedDoc.Factory).DBNumber);
			AssertEquals("ParentMain has just one document", 1, docParentMain.Documents.Count);
			TimeSpan diff = ZDateTime.UtcNow - docParentMain.Documents[0].SC_Date;
			Assert(diff.TotalSeconds < 20);

			newParentMainCollection = new StorageMainCollection(MasterFactory);
			newParentMainCollection.Load();
			AssertEquals("Only one more storagemain record has been created", oldParentMainCollection.Count + 1, newParentMainCollection.Count);

			AssertEquals("The event from the doc type should not be logged", 0, org.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DCF")).Length);
			AssertNotNull("DDA - DocumentAllocated event should be logged against Org", org.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocated.Code))[0]);

			AssertStartsWith("Reference should contain document type", "CRE|", org.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocated.Code))[0].SL_Reference);

			var documentAllocatedEvent = org.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocated.Code)).Single();
			var guid = StmALog.GetGuid(documentAllocatedEvent.SL_Reference);

			AssertNotEquals("Reference should contain GUID", ZGuid.Empty, guid);
			AssertContains(returnedDoc.PK.ToString(), documentAllocatedEvent.SL_Reference);
			AssertContains(printJobPKAsString, documentAllocatedEvent.SL_Reference);
		}

		[ExpectNoExceptions]
		public void TestCreateAndAllocateDocumentWithDifferentParent()
		{
			StorageMainCollection oldParentMainCollection = new StorageMainCollection(MasterFactory);
			oldParentMainCollection.Load();

			OrgHeader orgHeader = MasterFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));

			RefDocType docType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			docType.RT_DocType = "CRE";
			docType.RT_Desc = "Credit Application";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			MasterFactory.Save();

			Assert("Precondition: DDA - DocumentAllocated event should NOT be logged against Org yet", orgHeader.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocated.Code)).Length == 0);

			StorageDocs returnedDoc = (StorageDocs)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(orgHeader.PK, ((IDocManagerSupport)orgHeader).DocManagerInfo.DocManagerCode, SmallThreePagesTif, "CRE", "EDI (Sydney) - Credit Application");
			AssertNotNull("Document should have successfully been created", returnedDoc);

			returnedDoc = (StorageDocs)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(orgHeader.PK, "SHP", SmallThreePagesTif, "CRE", "EDI (Sydney) - Credit Application", true, "file");
			AssertNotNull("Document should have successfully been created", returnedDoc);
			AssertEquals("file", returnedDoc.SC_FileName);
			AssertEquals("file.tif", returnedDoc.SC_FileNameWithExtension);
		}

		public void TestCreateAndAllocateDocumentSetsPublishedFlag()
		{
			var documentFactory = (IDocumentFactory)MasterFactory;
			OrgHeader organization = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var docManagerSupport = (IDocManagerSupport)organization;

			ZQuery allReferenceTypeQuery = new ZQuery(RefDocTypeSchema.RT_ReferenceType, "ALL");
			RefDocType nonPublishedDocType = Factory.LoadTop1<RefDocType>(new ZQuery(allReferenceTypeQuery, new ZQuery(RefDocTypeSchema.RT_IsPublished, false)));
			RefDocType publishedDocType = Factory.LoadTop1<RefDocType>(new ZQuery(allReferenceTypeQuery, new ZQuery(RefDocTypeSchema.RT_IsPublished, true)));

			AssertEquals("SC_IsPublished", false, ((StorageDocs)documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SmallThreePagesTif, "", "")).SC_IsPublished);
			AssertEquals("SC_IsPublished", false, ((StorageDocs)documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SmallThreePagesTif, nonPublishedDocType.RT_DocType, "")).SC_IsPublished);
			AssertEquals("SC_IsPublished", true, ((StorageDocs)documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SmallThreePagesTif, publishedDocType.RT_DocType, "")).SC_IsPublished);
		}

		public void TestCreateAndAllocateDocumentUnpublishOlderVersionDocuments()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var documentFactory = (IDocumentFactory)MasterFactory;
			var organization = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var docManagerSupport = (IDocManagerSupport)organization;

			var allReferenceTypeQuery = new ZQuery(RefDocTypeSchema.RT_ReferenceType, "ALL");
			var publishedDocType = Factory.LoadTop1<RefDocType>(new ZQuery(allReferenceTypeQuery, new ZQuery(RefDocTypeSchema.RT_IsPublished, true)));

			// Test FinalizeSupersedeOlderVersionDocs for Documents
			var publishedDoc = (StorageDocs)documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SmallThreePagesTif, publishedDocType.RT_DocType, "", fileName: "TestDoc");
			publishedDoc.SC_SaveVersions = true;
			AssertEquals("SC_IsPublished", true, publishedDoc.SC_IsPublished);

			documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SmallThreePagesTif, publishedDocType.RT_DocType, "", fileName: "TestDoc");
			AssertEquals("SC_IsPublished", false, publishedDoc.SC_IsPublished);

			// Test FinalizeSupersedeOlderVersionDocs for Files
			var publishedFile = (StorageDocs)documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SamplePdfPath, publishedDocType.RT_DocType, "", fileName: "TestDoc");
			publishedFile.SC_SaveVersions = true;
			AssertEquals("SC_IsPublished", true, publishedFile.SC_IsPublished);

			documentFactory.CreateAndAllocateDocument(organization.PK, docManagerSupport.DocManagerInfo.DocManagerCode, SamplePdfPath, publishedDocType.RT_DocType, "", fileName: "TestDoc");
			AssertEquals("SC_IsPublished", false, publishedFile.SC_IsPublished);
		}

		public void TestCreateAndAllocateDocumentUsingDocTypeNotLoggingEDocs()
		{
			ZGuid parentGuid = ZGuid.NewZGuid();

			RefDocType docType = MasterFactory.New<RefDocType>();
			docType.RT_LogSystemCreatedDocsToEDocs = false;
			docType.RT_DocType = "ZZZ";
			docType.RT_Desc = "This is a test doctype";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;

			StorageDocs returnedDoc = (StorageDocs)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(parentGuid, Core.Constants.DocManagerCodes.Shipment, SmallTifPath, "ZZZ", "This is a test doctype");
			AssertNull("DocType does not log system created docs, ReturnedDoc should be null", returnedDoc);

			returnedDoc = (StorageDocs)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(parentGuid, Core.Constants.DocManagerCodes.Shipment, SmallTifPath, "ZZZ", "This is a test doctype", true, "AAA");
			AssertNotNull("Force to allocate docs, ReturnedDoc should not be null", returnedDoc);
			AssertEquals("AAA", returnedDoc.SC_FileName);
			AssertEquals("AAA.tif", returnedDoc.SC_FileNameWithExtension);

			docType.RT_LogSystemCreatedDocsToEDocs = true;
			returnedDoc = (StorageDocs)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(parentGuid, Core.Constants.DocManagerCodes.Shipment, SmallTifPath, "ZZZ", "This is a test doctype");
			AssertNotNull("DocType now logs system created docs, ReturnedDoc should not be null", returnedDoc);
		}

		public void TestCreateAndAllocatePdfDocumentSetsFileNameAndDataType()
		{
			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "CRE";
			newDocType.RT_Desc = "Credit Application";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			MasterFactory.Save();

			var description = "EDI (Sydney) - Credit Application";
			var returnedDoc = (StorageDocsBase)((IDocumentFactory)MasterFactory).CreateAndAllocateDocument(org.PK, ((IDocManagerSupport)org).DocManagerInfo.DocManagerCode, SamplePdfPath, "CRE", description);

			AssertNotNull("Should have successfully been created", returnedDoc);
			AssertEquals("Description should be set", description, returnedDoc.SC_Desc);
			AssertEquals("File name should be the description", description, returnedDoc.SC_FileName);
			AssertEquals("Data type should be set", Core.Constants.FileFormats.PDF, returnedDoc.SC_DataType);
			AssertEquals("EDocFormat should be the data type", Core.Constants.FileFormats.PDF, returnedDoc.EDocFormat);
		}

		public void TestFileNameMinusPrefix()
		{
			var documentFactory = new DbBackendDocumentFactory(Factory);
			AssertEquals("abc.tif", documentFactory.GetFileNameMinusPrefix("[XXX YYY ZZZ] abc.tif"));
			AssertEquals("abc.tif", documentFactory.GetFileNameMinusPrefix("[XXX YYY ZZZ]abc.tif"));
			AssertEquals("[XXX YYY ZZZ].tif", documentFactory.GetFileNameMinusPrefix("[XXX YYY ZZZ].tif"));
			AssertEquals("abc.tif", documentFactory.GetFileNameMinusPrefix("abc.tif"));
			AssertEquals("abc", documentFactory.GetFileNameMinusPrefix("[XXX YYY ZZZ] abc"));
			AssertEquals("z.z", documentFactory.GetFileNameMinusPrefix("[XXX YYY ZZZ]z.z"));
			AssertEquals("[XXX YYY ZZZ].xlsx", documentFactory.GetFileNameMinusPrefix("[XXX YYY ZZZ].xlsx"));
			AssertEquals("abc[2].xlsm", documentFactory.GetFileNameMinusPrefix("abc[2].xlsm"));
			AssertEquals("abc[2]xyz.docx", documentFactory.GetFileNameMinusPrefix("abc[2]xyz.docx"));
			AssertEquals("2]xyz.docx", documentFactory.GetFileNameMinusPrefix("2]xyz.docx"));
			AssertEquals("[xyz.docx", documentFactory.GetFileNameMinusPrefix("[xyz.docx"));
			AssertEquals("[xyz].abc.doc", documentFactory.GetFileNameMinusPrefix("[xyz].abc.doc"));
			AssertEquals("z.", documentFactory.GetFileNameMinusPrefix("[xyz]z."));
			AssertEquals("[xyz]", documentFactory.GetFileNameMinusPrefix("[xyz]"));
			AssertEquals("[xyz.]docx", documentFactory.GetFileNameMinusPrefix("[xyz.]docx"));
			AssertEquals("z.]docx", documentFactory.GetFileNameMinusPrefix("[xy]z.]docx"));
			AssertEquals("[xyz.docx]", documentFactory.GetFileNameMinusPrefix("[xyz.docx]"));
			AssertEquals("z   .doc", documentFactory.GetFileNameMinusPrefix("[xyz]z   .doc"));
			AssertEquals("z   .doc", documentFactory.GetFileNameMinusPrefix("[xyz]   z   .doc"));
			AssertEquals("z.doc", documentFactory.GetFileNameMinusPrefix("[xyz]   z.doc"));
			AssertEquals("[xyz]   .   doc", documentFactory.GetFileNameMinusPrefix("[xyz]   .   doc"));
			AssertEquals("[xyz].abc   .doc", documentFactory.GetFileNameMinusPrefix("[xyz].abc   .doc"));
			AssertEquals("[xyz]   .abc   .doc", documentFactory.GetFileNameMinusPrefix("[xyz]   .abc   .doc"));
		}

		public void TestImportDocument()
		{
			SetupShipmentObjects();

			GlbStaff staffMember = MasterFactory.New(typeof(GlbStaff)) as GlbStaff;
			staffMember.GS_Code = "_AA";
			staffMember.GS_LoginName = "AStaff";

			MasterFactory.Save();
			string finalDocType;
			MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", ZString.Empty, ZGuid.Empty, ZString.Empty, "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
			MasterFactory.Save();
			BusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("", finalDocType);
			AssertEquals("There should be 2 more docs than before - two docs in the file don't have enough info to be automatically allocated", 2, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("There should be 2 more docs than before in DB1 - two docs in the file have enough info to be automatically allocated", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportDocument_RestrictedByRegistry()
		{
			var collection = new CodeSelectionCollection(SystemDataRegistry.Instance.DocumentTypesRestrictedListProvider);
			collection.AddNew().Code = "CIV";

			using (SystemDataRegistry.Instance.DocumentTypesRestrictedForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				SetupShipmentObjects();

				GlbStaff staffMember = MasterFactory.New(typeof(GlbStaff)) as GlbStaff;
				staffMember.GS_Code = "_AA";
				staffMember.GS_LoginName = "AStaff";

				MasterFactory.Save();
				string finalDocType;
				MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", ZString.Empty, ZGuid.Empty, ZString.Empty, "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
				MasterFactory.Save();
				BusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("CIV", finalDocType);
				AssertEquals("There should be 2 more docs than before - two docs in the file don't have enough info to be automatically allocated", 2, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
				AssertEquals("There should be 1 more docs than before in DB1 - HBL allocated, but CIV denied", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			}
		}

		public void TestImportDocumentAddsLogs()
		{
			var originalValue = SystemDataRegistry.Instance.DDIDocumentSource.Value;
			try
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				SetupShipmentObjects();

				GlbStaff staff = MasterFactory.New<GlbStaff>();
				staff.GS_Code = "_AA";
				staff.GS_LoginName = "AStaff";

				RefDocType dorDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "PMR"));
				dorDocType.RT_SE_NKDocumentReceivedEvent = Events.DeliveryOrderReceivedCode;
				dorDocType.RT_LogMacro = "eDoc '<@data.SC_Desc>' imported";

				MasterFactory.Save();

				const string fileName = "test.tif";
				string finalDocType;
				MasterFactory.Import(SmallTifBytes, fileName, "SHP", ShipmentS00001000.PK, dorDocType.RT_DocType, "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
				MasterFactory.Save();

				BusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("One document should have been allocated", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

				StorageDocs doc = factoryOne.LoadTop1<StorageDocs>(new ZQuery());

				StmALog[] ddiLogs = MasterFactory.Load<StmALog>(new ZQuery(new ZQuery(StmALogSchema.SL_Parent, ShipmentS00001000.PK), new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentImportedCode)));
				AssertEquals(1, ddiLogs.Length);
				AssertEquals(string.Concat(dorDocType.RT_DocType, "|", doc.PK.ToString()), ddiLogs[0].SL_Reference);

				StmALog[] dorLogs = MasterFactory.Load<StmALog>(new ZQuery(new ZQuery(StmALogSchema.SL_Parent, ShipmentS00001000.PK), new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeliveryOrderReceivedCode)));
				AssertEquals(1, dorLogs.Length);
				AssertEquals(string.Format("{0}|eDoc '{1}' imported", doc.PK, doc.SC_Desc), dorLogs[0].SL_Reference);
			}
			finally
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestImportDocumentPDFWithBarCode_BadAllocationStatusLogged()
		{
			SetupShipmentObjects();

			GlbStaff staff = MasterFactory.New<GlbStaff>();
			staff.GS_Code = "_AA";
			staff.GS_LoginName = "AStaff";

			RefDocType docType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			docType.RT_DocType = "CSD";
			docType.RT_Desc = "CSD Description";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;

			MasterFactory.Save();

			const string fileName = "PDF Document with Barcode - S00001000.PDF";
			string finalDocType;

			MasterFactory.Import(PdfDocumentWithBarcodeBytes, fileName, string.Empty, ZGuid.Empty, string.Empty, "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
			MasterFactory.Save();

			var childFactory = MasterFactory.GetFactory(1);
			var mainQuery = new ZDBOnlyQuery(typeof(StorageDocs));
			mainQuery.AddToFilter(StorageDocsSchema.SC_DocType, "CSD");
			var subQuery = new ZDBOnlySubQuery(typeof(StorageMain), StorageMainSchema.PK);
			subQuery.AddToFilter(JoinCondition.And, StorageMainSchema.SM_ParentFK, SQLComparisonOperator.Equal, ShipmentS00001000.PK);
			mainQuery.AddSubQuery(StorageDocsSchema.SC_SM, subQuery, JoinCondition.And);

			StorageDocsBase[] documentsInDb1 = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), mainQuery);

			AssertEquals("Only one storage document", 1, documentsInDb1.Length);
			AssertEquals("Data Type should be PDF", Core.Constants.FileFormats.PDF, documentsInDb1[0].SC_DataType);
		}

		public void TestImportDocumentPDFWithBarCode_CorrectFileExtension()
		{
			SetupShipmentObjects();

			var staff = MasterFactory.New<GlbStaff>();
			staff.GS_Code = "_AA";
			staff.GS_LoginName = "AStaff";

			var docType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			docType.RT_DocType = "CSD";
			docType.RT_Desc = "CSD Description";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;

			MasterFactory.Save();

			var fileName = ".PDF";
			string finalDocType;

			var log = new List<LogEventArgs>();
			MasterFactory.Log += args => { log.Add(args); };
			MasterFactory.Import(PdfDocumentWithBarcodeBytes, fileName, string.Empty, ZGuid.Empty, string.Empty, "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
			MasterFactory.Save();

			// Allocation should fail because the file name is not valid, so it creates an unallocated document with error in allocation status.
			var unallocatedDoc = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery(StorageDocsSchema.SC_Desc, docType.RT_Desc));
			AssertNotNull(unallocatedDoc);

			var errorStatus = MasterFactory.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, unallocatedDoc.PK));
			AssertNotNull(errorStatus);
			AssertEquals("Validation errors prevent allocation", errorStatus.ST_NoteText);
		}

		public void TestImportDocumentTIFWithBarCode_CorrectFileExtension()
		{
			SetupShipmentObjects();

			GlbStaff staff = MasterFactory.New<GlbStaff>();
			staff.GS_Code = "_AA";
			staff.GS_LoginName = "AStaff";

			RefDocType docType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			docType.RT_DocType = "CSD";
			docType.RT_Desc = "CSD Description";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;

			MasterFactory.Save();

			var contents = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.TIF Document with Barcode - S00001000.TIF");
			const string fileName = "TIF Document with Barcode - S00001000.TIF";
			string finalDocType;

			MasterFactory.Import(contents, fileName, string.Empty, ZGuid.Empty, string.Empty, "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
			MasterFactory.Save();

			var childFactory = MasterFactory.GetFactory(1);
			var mainQuery = new ZDBOnlyQuery(typeof(StorageDocs));
			mainQuery.AddToFilter(StorageDocsSchema.SC_DocType, "CSD");
			var subQuery = new ZDBOnlySubQuery(typeof(StorageMain), StorageMainSchema.PK);
			subQuery.AddToFilter(JoinCondition.And, StorageMainSchema.SM_ParentFK, SQLComparisonOperator.Equal, ShipmentS00001000.PK);
			mainQuery.AddSubQuery(StorageDocsSchema.SC_SM, subQuery, JoinCondition.And);

			StorageDocsBase[] documentsInDb1 = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), mainQuery);

			AssertEquals("Only one storage document", 1, documentsInDb1.Length);
			AssertEquals("Data Type should be TIF", Core.Constants.FileFormats.TIF, documentsInDb1[0].SC_DataType);
		}

		public void TestAllocate()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			OrgHeader org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			// Allocate to an organisation
			StorageDocs newDocument = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_Date = ZDateTime.Now;
			newDocument.SC_ImageData = SmallTifBytes;
			StorageMain org1ParentMain = newDocument.ParentMain;

			Assert("Document is not allocated", newDocument.IsUnallocated);

			var allocatedDoc = MasterFactory.Allocate(newDocument, org.PK);

			StorageMainCollection intermediateMainCollection = new StorageMainCollection(MasterFactory);
			intermediateMainCollection.Load();
			Assert("Document is allocated", allocatedDoc.IsAllocated);
			AssertEquals("Allocating to BizO that has no storagemain, should have used existing StorageMain, no more StorageMains should have been added", 1, intermediateMainCollection.Count);
			AssertEquals("Same ParentMain should be used for the storagemain record", org1ParentMain, allocatedDoc.ParentMain);
			AssertEquals("Document should be saved in the correct factory", allocatedDoc.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)allocatedDoc.Factory).DBNumber);
			AssertEquals("Document parent for Org1 has only one document", 1, allocatedDoc.ParentMain.Documents.Count);

			StorageDocs testDocHasBeenDeleted = MasterFactory.Load(typeof(StorageDocs), newDocument.PK) as StorageDocs;
			AssertNull("Checking doc that was used to allocate has been deleted", testDocHasBeenDeleted);

			// allocate to a different organisation
			ZQuery filter2 = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			OrgHeader org2 = MasterFactory.LoadTop1(typeof(OrgHeader), filter2) as OrgHeader;
			StorageMain beforeAllocation = allocatedDoc.ParentMain;

			Assert("Document is allocated", allocatedDoc.IsAllocated);
			AssertEquals(1, beforeAllocation.Documents.Count);

			var allocatedDoc2 = MasterFactory.Allocate(allocatedDoc, org2.PK);

			StorageMainCollection intermediateMainCollection2 = new StorageMainCollection(MasterFactory);
			intermediateMainCollection2.Load();
			Assert("Document is allocated", allocatedDoc2.IsAllocated);
			AssertEquals("Allocating to BizO that has no StorageMain, only one more storageMainRecord should have been added", intermediateMainCollection.Count + 1, intermediateMainCollection2.Count);
			Assert("Different ParentMain should be used for StorageMain record", beforeAllocation != allocatedDoc2.ParentMain);
			AssertEquals("Document should be saved in correct factory", allocatedDoc2.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)allocatedDoc2.Factory).DBNumber);
			AssertEquals("New document parent for Org2 has only one document", 1, allocatedDoc2.ParentMain.Documents.Count);

			BusinessObjectFactory childFactory = MasterFactory.GetFactory(beforeAllocation.SM_DB);
			testDocHasBeenDeleted = childFactory.Load(typeof(StorageDocs), allocatedDoc.PK) as StorageDocs;
			AssertNull("Checking doc that was used to allocate has been deleted", testDocHasBeenDeleted);

			// Allocate back to org
			StorageMain beforeAllocation2 = allocatedDoc2.ParentMain;

			Assert("Document is allocated", allocatedDoc2.IsAllocated);

			var allocatedDoc3 = MasterFactory.Allocate(allocatedDoc2, org.PK);
			StorageMainCollection intermediateMainCollection3 = new StorageMainCollection(MasterFactory);
			intermediateMainCollection3.Load();
			Assert("Document is allocated", allocatedDoc3.IsAllocated);
			AssertEquals("Allocating to BizO that has a StorageMain already, no storage mains should have been added", intermediateMainCollection2.Count, intermediateMainCollection3.Count);
			Assert("Different ParentMain should be used for StorageMain record", beforeAllocation2 != allocatedDoc3.ParentMain);
			AssertEquals("Document should be saved in correct factory", allocatedDoc3.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)allocatedDoc3.Factory).DBNumber);
			AssertEquals("1 document now allocated to Org1's parentMain", 1, allocatedDoc3.ParentMain.Documents.Count);

			childFactory = MasterFactory.GetFactory(beforeAllocation2.SM_DB);
			testDocHasBeenDeleted = childFactory.Load(typeof(StorageDocs), allocatedDoc2.PK) as StorageDocs;
			AssertNull("Checking doc that was used to allocate has been deleted", testDocHasBeenDeleted);

			MasterFactory.Save();

			StorageMain afterSave = MasterFactory.Load(typeof(StorageMain), beforeAllocation2.PK) as StorageMain;
			AssertEquals("Org2's storagemain should have no docs after loading again", 0, afterSave.Documents.Count);
			afterSave = MasterFactory.Load(typeof(StorageMain), org1ParentMain.PK) as StorageMain;
			AssertEquals("Org1's storagemain should have only 1 doc after loading again", 1, afterSave.Documents.Count);
		}

		public void TestAllocateWithOutOfMemoryExceptionIsHandled()
		{
			var filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			var org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			// Allocate to an organisation
			var newDocument = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_Date = ZDateTime.Now;
			newDocument.SC_ImageData = SmallTifBytes;

			Assert("Document is not allocated", newDocument.IsUnallocated);

			try
			{
				newDocument.ForceThrowOutOfMemoryException = true;
				NUnit.Framework.Assert.That(delegate
						{
							if (org != null)
							{
								MasterFactory.Allocate(newDocument, org.PK);
							}
						}, CustomConstraints.InnermostExceptionThrown(typeof(OutOfMemoryException), "An error has occured because this program is running low on memory."), "Shoud handle the OOM exception");
				Assert(MasterFactory.IsNewlyAllocatedDocDeleted);
			}
			finally
			{
				newDocument.ForceThrowOutOfMemoryException = false;
			}
		}

		public void TestAllocateWithExternalStorageException()
		{
			// Allocate to an organisation
			var newDocument = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_Date = ZDateTime.Now;
			newDocument.SC_ImageData = ZBlob.Empty;
			MasterFactory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			{
				Assert("Document is not allocated", newDocument.IsUnallocated);
				var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
				AssertExceptionThrown<ExternalStorageException>("ExternalStorageException should be thrown and not swallowed", () => MasterFactory.Allocate(newDocument, org.PK));
				Assert("Newly allocated doc should be deleted if ExternalStorageException thrown", MasterFactory.IsNewlyAllocatedDocDeleted);
			}
		}

		public void TestAllocateLeavesExistingDocumentIfItIsCorrectlyAllocated()
		{
			ZQuery orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader org = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			StorageMain parent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			parent.SM_ParentFK = org.PK;
			parent.SM_DB = 1;

			StorageDocs newDocument = parent.Documents.AddNew();
			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_Date = ZDateTime.Now;
			newDocument.SC_ImageData = SmallTifBytes;

			var allocatedDoc = MasterFactory.Allocate(newDocument, org.PK);

			AssertEquals("The document returned should be the same one already attached to the Parent", newDocument.PK, allocatedDoc.PK);
		}

		public void TestAllocate_S3Enabled()
		{
			var originalEDoc = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			originalEDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.InterchangeAttachments;
			originalEDoc.SC_Date = ZDateTime.Now;
			originalEDoc.SC_ImageData = ZBlob.Empty; // it has been uploaded to S3
			MasterFactory.Save();

			Assert("Document is not allocated", originalEDoc.IsUnallocated);

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.local"))
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=dude;Secret=top"))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(false, true))
			{
				var allocatedEDoc = MasterFactory.Allocate(originalEDoc, Guid.NewGuid());
				AssertNotNull("Allocated successfully ", allocatedEDoc);
				AssertEquals("Allocated content is not empty", false, allocatedEDoc.SC_ImageData.IsEmpty);
				AssertEquals("Allocated content should same as the binary retrieve from S3", AWSPersisterForTest.ImageDataForTest, allocatedEDoc.SC_ImageData);
			}
		}

		public void TestAllocate_S3Enabled_ErrorInRetrieve()
		{
			var originalEDoc = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			originalEDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.InterchangeAttachments;
			originalEDoc.SC_Date = ZDateTime.Now;
			originalEDoc.SC_ImageData = ZBlob.Empty; // it is already uploaded to S3
			MasterFactory.Save();

			Assert("Document is not allocated", originalEDoc.IsUnallocated);

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			{
				StorageDocsBase allocatedEDoc = null;
				AssertExceptionThrown<ExternalStorageException>(() => allocatedEDoc = MasterFactory.Allocate(originalEDoc, Guid.NewGuid()));

				AssertNull("If retrieve throwing error, it should not be allocated", allocatedEDoc);
				AssertNotNull("If retrieve throwing error, the original should remain", originalEDoc);
			}
		}

		public void TestUnallocateSingleDocument()
		{
			var manager = new AllocateDocumentsManager(MasterFactory);
			var doc1 = manager.UnallocatedDocuments.AddNew();
			var doc2 = manager.UnallocatedDocuments.AddNew();

			var orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			var org1 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "R");
			var org2 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			doc1.SC_DataType = "ORG";
			doc1.SC_DocType = "QUO";
			doc1.SC_ParentID = org1.PK;

			doc2.SC_DataType = "ORG";
			doc2.SC_DocType = "QUO";
			doc2.SC_ParentID = org2.PK;

			manager.AllocateDocuments();

			var docToRemove = manager.AllocatedDocuments[0];
			var unallocatedDoc = MasterFactory.Unallocate(docToRemove);

			AssertEquals("Parent's document count should be one less after unallocate", 1, manager.AllocatedDocuments.Count);

			BusinessObjectFactory childFactory = MasterFactory.GetFactory(manager.AllocatedDocuments[0].ParentMain.SM_DB);
			var deletedDoc = childFactory.Load(typeof(StorageDocs), docToRemove.PK) as StorageDocs;
			AssertEquals("Doc should have been deleted from DB, Load will return null", null, deletedDoc);

			var loadedUnallocatedDoc = MasterFactory.Load<StorageDocsUnallocated>(unallocatedDoc.PK);
			MasterFactory.Save();
			AssertNotNull("Doc should be in main factory now", loadedUnallocatedDoc);
			AssertEquals("Only one more storagemain record should have been added for the unallocated document", 2, MasterFactory.GetDatabaseCount(typeof(StorageMain)));
		}

		public void TestAllocateToDB()
		{
			SetupParent();
			Parent.SM_DB = 0;
			MasterFactory.AllocateToDB(Parent);
			Assert("Parent db was 0, should have been allocated", !Parent.SM_DB.IsEmpty);

			int previousDB = Parent.SM_DB;
			MasterFactory.AllocateToDB(Parent);
			AssertEquals("Parent DB was not 0, shouldn't have been changed", previousDB, Parent.SM_DB);
			Assert(!Parent.SM_DB.IsEmpty);
		}

		public void TestGetFactoryNumberInUseForSingleObject()
		{
			SetupParent();
			StorageDocs doc1 = Parent.Documents.AddNew();
			AssertEquals("Document should be using the factory number specified by parent main", Parent.SM_DB, MasterFactory.GetFactoryNumberInUse(doc1));
		}

		public void TestGetFactoryNumberInUseForCollection()
		{
			SetupParent();
			AssertEquals("Document should be using the factory number specified by parent main", Parent.SM_DB, MasterFactory.GetFactoryNumberInUse(Parent.Documents));
		}

		public void TestSave()
		{
			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);

			StorageDocs factoryOneDocument = StorageDocs.NewWithParentWithoutFK_DEBUG(factoryOne);
			factoryOneDocument.ParentMain.SM_Type = "UNA";
			factoryOneDocument.ParentMain.SM_DB = 1;

			RefDocType documentTypeInMaster = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			documentTypeInMaster.RT_ReferenceType = Core.Constants.DocManagerCodes.Shipment;
			documentTypeInMaster.RT_DocType = "ZZZ";
			documentTypeInMaster.RT_Desc = "LKJLSDFJDLSFJSDF";

			Assert("both objects are not saved in their respective databases yet", !factoryOneDocument.IsInDatabase && !documentTypeInMaster.IsInDatabase);

			MasterFactory.Save();

			Assert("both objects were saved in their respective factories", factoryOneDocument.IsInDatabase && documentTypeInMaster.IsInDatabase);
		}

		public void TestGetDocumentToAllocateWithNoExistingDocumentOrMain()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			var document = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");

			AssertNotNull("A document is supposed to be returned", document);
			AssertEquals("Document's parent reference to SM_ParentFK should be BLANK - other functionality depends on this", ZGuid.Empty, document.ParentMain.SM_ParentFK);
			Assert("Document should be system generated", document.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithNoExistingDocumentButExistingMain()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));
			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			var document = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", document);
			AssertEquals("The document should come from the same factory as the parent specifies", factoryOne, document.Factory);
			Assert("Document should be system generated", document.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMain()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello";
			document.SC_IsSystemGenerated = true;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Overwriting existing doc, doc should have the correct parent", retrievedDocument.ParentMain.PK, main.PK);
			AssertEquals("Document should come from Factory 1", MasterFactory.GetFactory(1), retrievedDocument.Factory);
			AssertEquals("Same document should have been retrieved", document.PK, retrievedDocument.PK);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentWhenDescExceedsMaximumLength()
		{
			var orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			var orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			var main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			var documentDesc = new ZString('A', StorageDocsSchema.SC_Desc.MaxLength + 1);
			var document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = documentDesc.Left(StorageDocsSchema.SC_Desc.MaxLength);
			document.SC_IsSystemGenerated = true;

			var fileDesc = new ZString('B', StorageDocsSchema.SC_Desc.MaxLength + 1);
			var file = main.Files.AddNew();
			file.SC_DocType = Core.Constants.FileFormats.PDF;
			file.SC_Desc = fileDesc.Left(StorageDocsSchema.SC_Desc.MaxLength);
			file.SC_IsSystemGenerated = true;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, documentDesc);
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Same document should have been retrieved", document.PK, retrievedDocument.PK);

			var retrievedFile = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.FileFormats.PDF, fileDesc);
			AssertNotNull("A file is supposed to be returned", retrievedFile);
			AssertEquals("Same file should have been retrieved", file.PK, retrievedFile.PK);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainButNotSystemGenerated()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello";
			document.SC_IsSystemGenerated = false;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			Assert("Not overwriting existing doc because it is not system generated. The returned doc should be different from the existing one", document.PK != retrievedDocument.PK);
			AssertEquals("Should come from the correct DB that the parent specifies", MasterFactory.GetFactory(main.SM_DB), retrievedDocument.Factory);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatches()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_Desc = "Hello";
			document2.SC_IsSystemGenerated = true;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Overwriting an existing doc, should have the correct parent", retrievedDocument.ParentMain.PK, main.PK);
			AssertEquals("Doc should be in database 1", MasterFactory.GetFactory(1), retrievedDocument.Factory);
			AssertEquals("Document returned should be the one with the latest date", document2.PK, retrievedDocument.PK);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatchesOneSysGenerated()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_Desc = "Hello";
			document2.SC_IsSystemGenerated = false;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Overwriting an existing doc, should have the correct parent", retrievedDocument.ParentMain.PK, main.PK);
			AssertEquals("Doc should be in database 1", MasterFactory.GetFactory(1), retrievedDocument.Factory);

			AssertEquals("Document returned will be the one with the earlier date because its sysgenerated", document.PK, retrievedDocument.PK);

			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatchesMSCTypeDifferentDescription()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello World";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_Desc = "Hello World";
			document2.SC_IsSystemGenerated = false;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			Assert("Document returned will be neither of the existing ones, because their document descriptions don't match and their doctype is MSC", document.PK != retrievedDocument.PK);
			Assert("Document returned will be neither of the existing ones, because their document descriptions don't match and their doctype is MSC", document2.PK != retrievedDocument.PK);
			AssertEquals("Should be from the database specified by the parent", MasterFactory.GetFactory(main.SM_DB), retrievedDocument.Factory);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatchesMSCTypeOneMatchingDescription()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello World";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_Desc = "Hello";
			document.SC_Date = new ZDateTime(2004, 04, 04);
			document2.SC_IsSystemGenerated = true;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Should match the second document", document2.PK, retrievedDocument.PK);
			AssertEquals("Parent should match", document2.ParentMain.PK, retrievedDocument.ParentMain.PK);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatchesDifferentDocType()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "LOP";
			newDocType.RT_Desc = "LOP DocType";
			newDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = "LOP";
			document.SC_Desc = "Non matching description will not match";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = "LOP";
			document2.SC_IsSystemGenerated = false;

			StorageDocs document3 = main.Documents.AddNew();
			document3.SC_DocType = "LOP";
			document3.SC_Desc = "Hello";
			document3.SC_Date = new ZDateTime(2004, 04, 04);
			document3.SC_IsSystemGenerated = true;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, "LOP", "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Document returned will be the earlier sysgenerated document because it matches the correct description", document3.PK, retrievedDocument.PK);
			AssertEquals("the Description used is the one passed in", "Hello", document3.SC_Desc);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatchesSaveVersioned()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "Hello";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;
			document.SC_SaveVersions = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_Desc = "Hello";
			document2.SC_IsSystemGenerated = false;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Document should be from the parent's specified directory", MasterFactory.GetFactory(main.SM_DB), retrievedDocument.Factory);
			Assert("Completely new document returned because first doc SaveVersions flag is set and second doc is not sysgenerated", document.PK != retrievedDocument.PK);
			Assert("Completely new document returned because first doc SaveVersions flag is set and second doc is not sysgenerated", document2.PK != retrievedDocument.PK);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWithExistingDocumentAndExistingMainAndMultipleMatchesSavedVersionsOnDocType()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "LOP";
			newDocType.RT_Desc = "LOP Document";
			newDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;
			newDocType.RT_SaveVersions = true;

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = "LOP";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;

			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_DocType = "LOP";
			document2.SC_IsSystemGenerated = true;

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument, "Hello");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Completely new document returned, should come from db specified by parent", MasterFactory.GetFactory(main.SM_DB), retrievedDocument.Factory);
			Assert("Completely new document returned because DocType SaveVersions flag is set", document.PK != retrievedDocument.PK);
			Assert("Completely new document returned because DocType SaveVersions flag is set", document2.PK != retrievedDocument.PK);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
		}

		public void TestGetDocumentToAllocateWith_MatchByLanguage()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "LOP";
			newDocType.RT_Desc = "LOP DocType";
			newDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			ZQuery mainQuery = new ZQuery(StorageMainSchema.SM_ParentFK, orgToAllocateTo.PK);
			AssertEquals("Precondition: No storage main for this org", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain), mainQuery));

			StorageMain main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			main.SM_ParentFK = orgToAllocateTo.PK;
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = "LOP";
			document.SC_Desc = "Hello";
			document.SC_Date = new ZDateTime(2004, 05, 04);
			document.SC_IsSystemGenerated = true;
			document.SC_Language = "EN-US";

			var retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, "LOP", "Hello", "");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertNotEquals("No language? No override.", document.PK, retrievedDocument.PK);

			retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, "LOP", "Hello", "ZH-CN");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertNotEquals("Wrong language? No override.", document.PK, retrievedDocument.PK);

			retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, "LOP", "Hello", "EN-US");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("Document returned will be the earlier sysgenerated document because it matches the correct description", document.PK, retrievedDocument.PK);
			AssertEquals("the Description used is the one passed in", "Hello", document.SC_Desc);
			Assert("Document should be system generated", retrievedDocument.SC_IsSystemGenerated);
			AssertEquals("Language is correct", "EN-US", retrievedDocument.SC_Language);

			document.SC_Language = "";

			retrievedDocument = MasterFactory.GetDocumentToAllocate(orgToAllocateTo.PK, Core.Constants.DocManagerCodes.Organisation, "LOP", "Hello", "EN-US");
			AssertNotNull("A document is supposed to be returned", retrievedDocument);
			AssertEquals("If previous document has no language, any new language will override it (as a grandfather mechanism).", document.PK, retrievedDocument.PK);
			AssertEquals("Language is correct", "EN-US", retrievedDocument.SC_Language);
		}

		public void TestGetNewStorageMainAndCleanupOldReferencesWithNoExistingMain()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			StorageDocs newDoc = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			newDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDoc.SC_Desc = "Hello";

			StorageMainCollection mainsPrevious = new StorageMainCollection(MasterFactory);
			mainsPrevious.Load();

			StorageMain newMain = MasterFactory.GetNewStorageMainAndCleanupOldReferences(newDoc, orgToAllocateTo.PK);
			AssertEquals("Should have the existing main already on the document", newDoc.ParentMain, newMain);

			StorageMainCollection mainsAfter = new StorageMainCollection(MasterFactory);
			mainsAfter.Load();

			AssertEquals("No extra mains should have been created", mainsPrevious.Count, mainsAfter.Count);
		}

		public void TestGetNewStorageMainAndCleanupOldReferencesWithExistingMainFromUnallocated()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			StorageMain existingMain = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			existingMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			existingMain.SM_ParentFK = orgToAllocateTo.PK;
			existingMain.SM_DB = 1;

			StorageMainCollection mainsPrevious = new StorageMainCollection(MasterFactory);
			mainsPrevious.Load();

			StorageDocs newDoc = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			newDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDoc.SC_Desc = "Hello";
			StorageMain previousParent = newDoc.ParentMain;

			StorageMain newMain = MasterFactory.GetNewStorageMainAndCleanupOldReferences(newDoc, orgToAllocateTo.PK);
			AssertEquals("Should have retrieved the correct storagemain", newMain, existingMain);
			Assert("Previous parent should be deleted", previousParent.IsDeleted);

			StorageMainCollection mainsAfter = new StorageMainCollection(MasterFactory);
			mainsAfter.Load();
			AssertEquals("Same number of mains should exist afterwards", mainsPrevious.Count, mainsAfter.Count);
		}

		public void TestGetNewStorageMainAndCleanupOldReferencesWithExistingMainFromAllocated()
		{
			Assert("Precondition: DB1 exists", DbHelper.DatabaseExists(1));

			ZQuery orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			OrgHeader orgToAllocateTo = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "R");
			OrgHeader orgToAllocateFrom = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			StorageMain mainAllocateTo = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			mainAllocateTo.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			mainAllocateTo.SM_ParentFK = orgToAllocateTo.PK;
			mainAllocateTo.SM_DB = 1;

			StorageMain mainAllocateFrom = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			mainAllocateFrom.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			mainAllocateFrom.SM_ParentFK = orgToAllocateFrom.PK;
			mainAllocateFrom.SM_DB = 1;

			StorageDocs newDoc = mainAllocateFrom.Documents.AddNew();
			newDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDoc.SC_Desc = "Hello";
			StorageMain previousParent = newDoc.ParentMain;

			StorageMainCollection mainsPrevious = new StorageMainCollection(MasterFactory);
			mainsPrevious.Load();

			StorageMain newMain = MasterFactory.GetNewStorageMainAndCleanupOldReferences(newDoc, orgToAllocateTo.PK);
			AssertEquals("Should have retrieved the correct storagemain", newMain, mainAllocateTo);
			Assert("Previous parent should still exist, not be deleted", !previousParent.IsDeleted);
			AssertEquals("Previous parent should have no documents", 0, previousParent.Documents.Count);

			StorageMainCollection mainsAfter = new StorageMainCollection(MasterFactory);
			mainsAfter.Load();
			AssertEquals("Same number of mains should exist afterwards", mainsPrevious.Count, mainsAfter.Count);
		}

		public void TestLogDocumentType()
		{
			RefDocType testDocType = MasterFactory.New<RefDocType>();
			testDocType.RT_LogSystemCreatedDocsToEDocs = false;
			testDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;
			testDocType.RT_DocType = "ZZZ";

			Assert("Can log documents that belong to all ref types", MasterFactory.LogDocumentType(Core.Constants.DocManagerCodes.Organisation, Core.Constants.RefDocTypes.MiscellaneousDocument));
			Assert("Can't log a doctype with LogSystemCreatedDocsToEDocs false", !MasterFactory.LogDocumentType(Core.Constants.DocManagerCodes.Organisation, "ZZZ"));
		}

		public void TestUpdatePublishedFlagForAllEDocs()
		{
			StorageMain parentDB0 = MasterFactory.New<StorageMain>();
			parentDB0.SM_DB = 1;
			parentDB0.SM_ParentFK = ZGuid.NewZGuid();
			parentDB0.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageDocs doc1DB0 = parentDB0.Documents.AddNew();
			doc1DB0.SC_DocType = "AAA";
			doc1DB0.SC_IsPublished = false;
			doc1DB0.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageDocs doc2DB0 = parentDB0.Documents.AddNew();
			doc2DB0.SC_DocType = "AAA";
			doc2DB0.SC_IsPublished = true;
			doc2DB0.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageDocs doc3DB0 = parentDB0.Documents.AddNew();
			doc3DB0.SC_DocType = "BBB";
			doc3DB0.SC_IsPublished = false;
			doc3DB0.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageDocs doc4DB0 = parentDB0.Documents.AddNew();
			doc4DB0.SC_DocType = "BBB";
			doc4DB0.SC_IsPublished = true;
			doc4DB0.SC_ImageData = new byte[] { 1, 2, 3 };

			StorageFile file1DB0 = parentDB0.Files.AddNew();
			file1DB0.SC_DocType = "AAA";
			file1DB0.SC_IsPublished = false;
			file1DB0.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageFile file2DB0 = parentDB0.Files.AddNew();
			file2DB0.SC_DocType = "AAA";
			file2DB0.SC_IsPublished = true;
			file2DB0.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageFile file3DB0 = parentDB0.Files.AddNew();
			file3DB0.SC_DocType = "BBB";
			file3DB0.SC_IsPublished = false;
			file3DB0.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageFile file4DB0 = parentDB0.Files.AddNew();
			file4DB0.SC_DocType = "BBB";
			file4DB0.SC_IsPublished = true;
			file4DB0.SC_ImageData = new byte[] { 1, 2, 3 };

			StorageMain parentDB1 = MasterFactory.New<StorageMain>();
			parentDB1.SM_DB = 1;
			parentDB1.SM_ParentFK = ZGuid.NewZGuid();
			parentDB1.SM_Type = "BKG";
			StorageDocs doc1DB1 = parentDB1.Documents.AddNew();
			doc1DB1.SC_DocType = "AAA";
			doc1DB1.SC_IsPublished = false;
			doc1DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageDocs doc2DB1 = parentDB1.Documents.AddNew();
			doc2DB1.SC_DocType = "AAA";
			doc2DB1.SC_IsPublished = true;
			doc2DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageDocs doc3DB1 = parentDB1.Documents.AddNew();
			doc3DB1.SC_DocType = "BBB";
			doc3DB1.SC_IsPublished = false;
			doc3DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageDocs doc4DB1 = parentDB1.Documents.AddNew();
			doc4DB1.SC_DocType = "BBB";
			doc4DB1.SC_IsPublished = true;
			doc4DB1.SC_ImageData = new byte[] { 1, 2, 3 };

			StorageFile file1DB1 = parentDB1.Files.AddNew();
			file1DB1.SC_DocType = "AAA";
			file1DB1.SC_IsPublished = false;
			file1DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageFile file2DB1 = parentDB1.Files.AddNew();
			file2DB1.SC_DocType = "AAA";
			file2DB1.SC_IsPublished = true;
			file2DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageFile file3DB1 = parentDB1.Files.AddNew();
			file3DB1.SC_DocType = "BBB";
			file3DB1.SC_IsPublished = false;
			file3DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			StorageFile file4DB1 = parentDB1.Files.AddNew();
			file4DB1.SC_DocType = "BBB";
			file4DB1.SC_IsPublished = true;
			file4DB1.SC_ImageData = new byte[] { 1, 2, 3 };

			MasterFactory.Save();
			var instance = MasterFactory as IDocumentFactory;
			instance.UpdatePublishedFlagForAllEDocs("SCL", "AAA", true);

			doc1DB0.Reload();
			doc2DB0.Reload();
			doc3DB0.Reload();
			doc4DB0.Reload();
			doc1DB1.Reload();
			doc2DB1.Reload();
			doc3DB1.Reload();
			doc4DB1.Reload();

			file1DB0.Reload();
			file2DB0.Reload();
			file3DB0.Reload();
			file4DB0.Reload();
			file1DB1.Reload();
			file2DB1.Reload();
			file3DB1.Reload();
			file4DB1.Reload();

			AssertEquals("published flag should update", true, doc1DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, doc2DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, doc3DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, doc4DB0.SC_IsPublished);
			AssertEquals("published flag should update", true, file1DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, file2DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, file3DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, file4DB0.SC_IsPublished);

			AssertEquals("published flag should update - other db 1", true, doc1DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, doc2DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, doc3DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, doc4DB1.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", true, file1DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, file2DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, file3DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, file4DB1.SC_IsPublished);

			instance.UpdatePublishedFlagForAllEDocs("SCL", "AAA", false);
			doc1DB0.Reload();
			doc2DB0.Reload();
			doc3DB0.Reload();
			doc4DB0.Reload();
			doc1DB1.Reload();
			doc2DB1.Reload();
			doc3DB1.Reload();
			doc4DB1.Reload();
			file1DB0.Reload();
			file2DB0.Reload();
			file3DB0.Reload();
			file4DB0.Reload();
			file1DB1.Reload();
			file2DB1.Reload();
			file3DB1.Reload();
			file4DB1.Reload();
			AssertEquals("published flag should update", false, doc1DB0.SC_IsPublished);
			AssertEquals("published flag should update", false, doc2DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, doc3DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, doc4DB0.SC_IsPublished);
			AssertEquals("published flag should update", false, file1DB0.SC_IsPublished);
			AssertEquals("published flag should update", false, file2DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, file3DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, file4DB0.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", false, doc1DB1.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", false, doc2DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, doc3DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, doc4DB1.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", false, file1DB1.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", false, file2DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, file3DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, file4DB1.SC_IsPublished);

			parentDB1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			MasterFactory.Save();
			instance.UpdatePublishedFlagForAllEDocs("CSR", "BBB", true);
			doc1DB0.Reload();
			doc2DB0.Reload();
			doc3DB0.Reload();
			doc4DB0.Reload();
			doc1DB1.Reload();
			doc2DB1.Reload();
			doc3DB1.Reload();
			doc4DB1.Reload();
			file1DB0.Reload();
			file2DB0.Reload();
			file3DB0.Reload();
			file4DB0.Reload();
			file1DB1.Reload();
			file2DB1.Reload();
			file3DB1.Reload();
			file4DB1.Reload();
			AssertEquals("published flag should stay", false, doc1DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, doc2DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, doc3DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, doc4DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, file1DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, file2DB0.SC_IsPublished);
			AssertEquals("published flag should stay", false, file3DB0.SC_IsPublished);
			AssertEquals("published flag should stay", true, file4DB0.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, doc1DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, doc2DB1.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", true, doc3DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, doc4DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, file1DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", false, file2DB1.SC_IsPublished);
			AssertEquals("published flag should update - other db 1", true, file3DB1.SC_IsPublished);
			AssertEquals("published flag should stay - other db 1", true, file4DB1.SC_IsPublished);
		}

		public void TestUpdatePublishedFlagForDocTypeWithALLReferenceType()
		{
			var parent1 = MasterFactory.New<StorageMain>();
			parent1.SM_DB = 1;
			parent1.SM_ParentFK = ZGuid.NewZGuid();
			parent1.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			var doc1 = parent1.Documents.AddNew();
			doc1.SC_DocType = "PRV";
			doc1.SC_IsPublished = false;
			doc1.SC_ImageData = new byte[] { 1, 2, 3 };

			var parent2 = MasterFactory.New<StorageMain>();
			parent2.SM_DB = 1;
			parent2.SM_ParentFK = ZGuid.NewZGuid();
			parent2.SM_Type = Core.Constants.DocManagerCodes.AgencyBillContainers;

			var doc2 = parent2.Documents.AddNew();
			doc2.SC_DocType = "PRV";
			doc2.SC_IsPublished = false;
			doc2.SC_ImageData = new byte[] { 1, 2, 3 };

			MasterFactory.Save();
			var instance = MasterFactory as IDocumentFactory;
			instance.UpdatePublishedFlagForAllEDocs("ALL", "PRV", true);

			doc1.Reload();
			doc2.Reload();

			Action<StorageDocs> assertIsPublished = d => AssertEquals("Should update IsPublished flag regardless of ReferenceType", true, d.SC_IsPublished);

			assertIsPublished(doc1);
			assertIsPublished(doc2);
		}

		public void TestUpdatePublishedFlagForEDocsQuery()
		{
			var factory = new DocumentFactoryBarcodeReading(Factory);
			var query = factory.GetWhereQueryForUpdatingEDocs("AAA", "ALL");

			Assert("update published flag on all edoc when reference type is ALL should not contain select query into StorageMain", !query.LiteralTextADO.Contains(@"and (SC_SM IN (SELECT SM_PK FROM "));

			query = factory.GetWhereQueryForUpdatingEDocs("AAA", "SCL");
			Assert("update published flag on all edoc when reference type isn't ALL should contain select query into StorageMain", query.LiteralTextADO.Contains(@"and (SC_SM IN (SELECT SM_PK FROM "));
		}

		public void TestGetQueryForUpdateingJobRequiredDocument()
		{
			var factory = new DocumentFactoryBarcodeReading(Factory);
			var query = factory.GetQueryForUpdatingJobRequiredDocument("AAA", "ALL");
			Assert("Query should not contain condition of EQ_DocCategory if reference type is ALL.", !query.LiteralTextADO.Contains("and EQ_DocCategory = 'ALL'"));

			query = factory.GetQueryForUpdatingJobRequiredDocument("AAA", "SCL");
			Assert("Query should contain condition of EQ_DocCategory if reference type is ALL.", query.LiteralTextADO.Contains("and EQ_DocCategory = 'SCL'"));
		}

		public void TestUpdateDocTypeForAllEDocs()
		{
			var documents = SetupStorageDocs();
			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();

			var instance = MasterFactory as IDocumentFactory;

			refDocType.Reload();
			refDocType.RT_DocType = "DTU";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated because storage1.SM_Type 'BAC' is not in Reference Type 'CSR'.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Nothing updated.", documents.doc2Storage1.SC_DocType == "DT2" && documents.doc2Storage1.SC_Desc == "DT2-CSR");

			documents.doc1Storage2.Reload();
			Assert("Document type should be updated because storage2.SM_Type 'ORG' is in Reference Type 'CSR'.", documents.doc1Storage2.SC_DocType == "DTU" && documents.doc1Storage2.SC_Desc == "DT1-CSR");

			documents.doc2Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage2.SC_DocType == "DT2" && documents.doc2Storage2.SC_Desc == "DT2-CSR");

			refDocType.RT_ReferenceType = "ALL";
			refDocType.RT_DocType = "DT2";
			refDocType.RT_Desc = "DT2-CSR";

			MasterFactory.Save();
			refDocType.Reload();
			refDocType.RT_DocType = "DT3";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Document type should be updated should be updated because referenceType is ALL.", documents.doc2Storage1.SC_DocType == "DT3" && documents.doc2Storage1.SC_Desc == "DT2-CSR");

			documents.doc1Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage2.SC_DocType == "DTU" && documents.doc1Storage2.SC_Desc == "DT1-CSR");

			documents.doc2Storage2.Reload();
			Assert("Document type should be updated because referenceType is ALL.", documents.doc2Storage2.SC_DocType == "DT3" && documents.doc2Storage2.SC_Desc == "DT2-CSR");
		}

		public void TestUpdateDocumentDescriptionForAllEDocs()
		{
			var documents = SetupStorageDocs();
			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();

			var instance = MasterFactory as IDocumentFactory;

			refDocType.Reload();
			refDocType.RT_Desc = "DTU-CSR";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated because storage1.SM_Type 'BAC' is not in Reference Type 'CSR'.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage1.SC_DocType == "DT2" && documents.doc2Storage1.SC_Desc == "DT2-CSR");

			documents.doc1Storage2.Reload();
			Assert("Description should be updated because storage2.SM_Type 'ORG' is in Reference Type 'CSR'.", documents.doc1Storage2.SC_DocType == "DT1" && documents.doc1Storage2.SC_Desc == "DTU-CSR");

			documents.doc2Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage2.SC_DocType == "DT2" && documents.doc2Storage2.SC_Desc == "DT2-CSR");

			refDocType.RT_ReferenceType = "ALL";
			refDocType.RT_DocType = "DT2";
			refDocType.RT_Desc = "DT2-CSR";

			MasterFactory.Save();
			refDocType.Reload();
			refDocType.RT_Desc = "DT3-CSR";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Description should be updated should be updated because referenceType is ALL.", documents.doc2Storage1.SC_DocType == "DT2" && documents.doc2Storage1.SC_Desc == "DT3-CSR");

			documents.doc1Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage2.SC_DocType == "DT1" && documents.doc1Storage2.SC_Desc == "DTU-CSR");

			documents.doc2Storage2.Reload();
			Assert("Description should be updated because referenceType is ALL.", documents.doc2Storage2.SC_DocType == "DT2" && documents.doc2Storage2.SC_Desc == "DT3-CSR");
		}

		public void TestUpdateDocTypeAndDescriptionForAllEDocs()
		{
			var documents = SetupStorageDocs();

			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();

			var instance = MasterFactory as IDocumentFactory;

			MasterFactory.Save();
			refDocType.Reload();
			refDocType.RT_DocType = "DTU";
			refDocType.RT_Desc = "DTU-CSR";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated because storage1.SM_Type 'BAC' is not in Reference Type 'CSR'.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage1.SC_DocType == "DT2" && documents.doc2Storage1.SC_Desc == "DT2-CSR");

			documents.doc1Storage2.Reload();
			Assert("Both document type and description should be updated because storage2.SM_Type 'ORG' is in Reference Type 'CSR'.", documents.doc1Storage2.SC_DocType == "DTU" && documents.doc1Storage2.SC_Desc == "DTU-CSR");

			documents.doc2Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage2.SC_DocType == "DT2" && documents.doc2Storage2.SC_Desc == "DT2-CSR");

			MasterFactory.Save();
			refDocType.Reload();
			refDocType.RT_ReferenceType = "SCL";
			refDocType.RT_DocType = "DT5";
			refDocType.RT_Desc = "DT5-SCL";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage1.SC_DocType == "DT2" && documents.doc2Storage1.SC_Desc == "DT2-CSR");

			documents.doc1Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage2.SC_DocType == "DTU" && documents.doc1Storage2.SC_Desc == "DTU-CSR");

			documents.doc2Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc2Storage2.SC_DocType == "DT2" && documents.doc2Storage2.SC_Desc == "DT2-CSR");

			refDocType.RT_ReferenceType = "ALL";
			refDocType.RT_DocType = "DT2";
			refDocType.RT_Desc = "DT2-CSR";

			MasterFactory.Save();
			refDocType.Reload();
			refDocType.RT_DocType = "DT3";
			refDocType.RT_Desc = "DT3-CSR";

			instance.UpdateDocTypeForAllEDocs(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.doc1Storage1.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage1.SC_DocType == "DT1" && documents.doc1Storage1.SC_Desc == "DT1-CSR");

			documents.doc2Storage1.Reload();
			Assert("Both document type and description should be updated because referenceType is ALL.", documents.doc2Storage1.SC_DocType == "DT3" && documents.doc2Storage1.SC_Desc == "DT3-CSR");

			documents.doc1Storage2.Reload();
			Assert("Nothing should be updated.", documents.doc1Storage2.SC_DocType == "DTU" && documents.doc1Storage2.SC_Desc == "DTU-CSR");

			documents.doc2Storage2.Reload();
			Assert("Both document type and description should be updated because referenceType is ALL.", documents.doc2Storage2.SC_DocType == "DT3" && documents.doc2Storage2.SC_Desc == "DT3-CSR");
		}

		(StorageDocsBase doc1Storage1, StorageDocsBase doc2Storage1, StorageDocsBase doc1Storage2, StorageDocsBase doc2Storage2) SetupStorageDocs()
		{
			var storage1 = MasterFactory.New<StorageMain>();
			storage1.SM_DB = 1;
			storage1.SM_ParentFK = ZGuid.NewZGuid();
			storage1.SM_Type = DocManagerCodes.BankAccount;

			var doc1Storage1 = storage1.Documents.AddNew();
			doc1Storage1.SC_DocType = "DT1";
			doc1Storage1.SC_Desc = "DT1-CSR";
			doc1Storage1.SC_ImageData = new byte[] { 1, 2, 3 };

			var doc2Storage1 = storage1.Documents.AddNew();
			doc2Storage1.SC_DocType = "DT2";
			doc2Storage1.SC_Desc = "DT2-CSR";
			doc2Storage1.SC_ImageData = new byte[] { 1, 2, 3 };

			var storage2 = MasterFactory.New<StorageMain>();
			storage2.SM_DB = 1;
			storage2.SM_ParentFK = ZGuid.NewZGuid();
			storage2.SM_Type = DocManagerCodes.Organisation;

			var doc1Storage2 = storage2.Documents.AddNew();
			doc1Storage2.SC_DocType = "DT1";
			doc1Storage2.SC_Desc = "DT1-CSR";
			doc1Storage2.SC_ImageData = new byte[] { 1, 2, 3 };

			var doc2Storage2 = storage2.Files.AddNew();
			doc2Storage2.SC_DocType = "DT2";
			doc2Storage2.SC_Desc = "DT2-CSR";
			doc2Storage2.SC_ImageData = new byte[] { 1, 2, 3 };

			return (doc1Storage1, doc2Storage1, doc1Storage2, doc2Storage2);
		}

		public void TestUpdateDocTypeForJobRequiredDocument()
		{
			var documents = SetupJobRequiredDocument();

			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();
			var instance = MasterFactory as IDocumentFactory;

			refDocType.Reload();
			refDocType.RT_DocType = "DT2";

			instance.UpdateDocTypeForJobRequiredDocument(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);
			documents.jobRequiredDocument1.Reload();
			Assert("Document type should be updated.", documents.jobRequiredDocument1.EQ_DocType == "DT2" && documents.jobRequiredDocument1.EQ_DocDescription == "DT1-CSR");

			documents.jobRequiredDocument2.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument2.EQ_DocType == "DT1" && documents.jobRequiredDocument2.EQ_DocDescription == "DT1-ALL");

			documents.jobRequiredDocument3.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument3.EQ_DocType == "DT1" && documents.jobRequiredDocument3.EQ_DocDescription == "DT1-SCL");
		}

		public void TestUpdateDocumentDescriptionForJobRequiredDocument()
		{
			var documents = SetupJobRequiredDocument();

			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();
			var instance = MasterFactory as IDocumentFactory;

			refDocType.Reload();
			refDocType.RT_Desc = "DT2-CSR";

			instance.UpdateDocTypeForJobRequiredDocument(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.jobRequiredDocument1.Reload();
			Assert("Description should be updated.", documents.jobRequiredDocument1.EQ_DocType == "DT1" && documents.jobRequiredDocument1.EQ_DocDescription == "DT2-CSR");

			documents.jobRequiredDocument2.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument2.EQ_DocType == "DT1" && documents.jobRequiredDocument2.EQ_DocDescription == "DT1-ALL");

			documents.jobRequiredDocument3.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument3.EQ_DocType == "DT1" && documents.jobRequiredDocument3.EQ_DocDescription == "DT1-SCL");
		}

		public void TestUpdateDocumentDescriptionTruncatedForJobRequiredDocument()
		{
			var documents = SetupJobRequiredDocument();

			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();
			var instance = MasterFactory as IDocumentFactory;

			refDocType.Reload();
			refDocType.RT_Desc = "12345678901234567890123456789012345678901234567890";

			instance.UpdateDocTypeForJobRequiredDocument(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.jobRequiredDocument1.Reload();
			AssertEquals("Doc Type should not be updated.", "DT1", documents.jobRequiredDocument1.EQ_DocType);
			AssertEquals("Description should be updated.", "12345678901234567890123456789012345", documents.jobRequiredDocument1.EQ_DocDescriptionMultilingual);

			documents.jobRequiredDocument2.Reload();
			AssertEquals("Nothing should be updated.", "DT1", documents.jobRequiredDocument2.EQ_DocType);
			AssertEquals("Nothing should be updated.", "DT1-ALL", documents.jobRequiredDocument2.EQ_DocDescriptionMultilingual);

			documents.jobRequiredDocument3.Reload();
			AssertEquals("Nothing should be updated.", "DT1", documents.jobRequiredDocument3.EQ_DocType);
			AssertEquals("Nothing should be updated.", "DT1-SCL", documents.jobRequiredDocument3.EQ_DocDescriptionMultilingual);
		}

		public void TestUpdateDocTypeAndDescriptionForJobRequiredDocument()
		{
			var documents = SetupJobRequiredDocument();

			var refDocType = MasterFactory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = "CSR";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-CSR";

			MasterFactory.Save();
			var instance = MasterFactory as IDocumentFactory;

			refDocType.Reload();
			refDocType.RT_DocType = "DT3";
			refDocType.RT_Desc = "DT3-CSR";

			instance.UpdateDocTypeForJobRequiredDocument(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.jobRequiredDocument1.Reload();
			Assert("Both document type and description should be updated.", documents.jobRequiredDocument1.EQ_DocType == "DT3" && documents.jobRequiredDocument1.EQ_DocDescription == "DT3-CSR");

			documents.jobRequiredDocument2.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument2.EQ_DocType == "DT1" && documents.jobRequiredDocument2.EQ_DocDescription == "DT1-ALL");

			documents.jobRequiredDocument3.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument3.EQ_DocType == "DT1" && documents.jobRequiredDocument3.EQ_DocDescription == "DT1-SCL");

			refDocType.RT_ReferenceType = "ALL";
			refDocType.RT_DocType = "DT1";
			refDocType.RT_Desc = "DT1-SCL";

			MasterFactory.Save();
			refDocType.Reload();
			refDocType.RT_DocType = "DT2";
			refDocType.RT_Desc = "DT2-ALL";

			instance.UpdateDocTypeForJobRequiredDocument(refDocType.RT_ReferenceTypeInfo, refDocType.RT_DocTypeInfo, refDocType.RT_DescInfo);

			documents.jobRequiredDocument1.Reload();
			Assert("Nothing should be updated.", documents.jobRequiredDocument1.EQ_DocType == "DT3" && documents.jobRequiredDocument1.EQ_DocDescription == "DT3-CSR");

			documents.jobRequiredDocument2.Reload();
			Assert("Both document type and description should be updated regardless of reference type.", documents.jobRequiredDocument2.EQ_DocType == "DT2" && documents.jobRequiredDocument2.EQ_DocDescription == "DT2-ALL");

			documents.jobRequiredDocument3.Reload();
			Assert("Both document type and description should be updated regardless of reference type.", documents.jobRequiredDocument3.EQ_DocType == "DT2" && documents.jobRequiredDocument3.EQ_DocDescription == "DT2-ALL");
		}

		(JobRequiredDocument jobRequiredDocument1, JobRequiredDocument jobRequiredDocument2, JobRequiredDocument jobRequiredDocument3) SetupJobRequiredDocument()
		{
			var orgHeader = MasterFactory.NewWithValidTestData<OrgHeader>();

			var jobRequiredDocument1 = MasterFactory.NewWithValidTestData<JobRequiredDocument>();
			jobRequiredDocument1.EQ_ParentID = orgHeader.PK;
			jobRequiredDocument1.EQ_ParentTableCode = orgHeader.TablePrefix;
			jobRequiredDocument1.ParentType = typeof(OrgHeader);
			jobRequiredDocument1.EQ_DocCategory = "CSR";
			jobRequiredDocument1.EQ_DocType = "DT1";
			jobRequiredDocument1.EQ_DocDescription = "DT1-CSR";
			jobRequiredDocument1.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

			var jobRequiredDocument2 = MasterFactory.NewWithValidTestData<JobRequiredDocument>();
			jobRequiredDocument2.EQ_ParentID = orgHeader.PK;
			jobRequiredDocument2.EQ_ParentTableCode = orgHeader.TablePrefix;
			jobRequiredDocument2.ParentType = typeof(OrgHeader);
			jobRequiredDocument2.EQ_DocCategory = "ALL";
			jobRequiredDocument2.EQ_DocType = "DT1";
			jobRequiredDocument2.EQ_DocDescription = "DT1-ALL";
			jobRequiredDocument2.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

			var jobRequiredDocument3 = MasterFactory.NewWithValidTestData<JobRequiredDocument>();
			jobRequiredDocument3.EQ_ParentID = orgHeader.PK;
			jobRequiredDocument3.EQ_ParentTableCode = orgHeader.TablePrefix;
			jobRequiredDocument3.ParentType = typeof(OrgHeader);
			jobRequiredDocument3.EQ_DocCategory = "SCL";
			jobRequiredDocument3.EQ_DocType = "DT1";
			jobRequiredDocument3.EQ_DocDescription = "DT1-SCL";
			jobRequiredDocument3.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

			return (jobRequiredDocument1, jobRequiredDocument2, jobRequiredDocument3);
		}

		public void TestCreateDocument()
		{
			StorageMain main = MasterFactory.New<StorageMain>();
			AssertEquals("Main's document collection count should be 0", 0, main.Documents.Count);
			var instance = MasterFactory as IDocumentFactoryForTest;
			instance.CreateDocument(main.PK);
			AssertEquals("Main's document collection count should be 1", 1, main.Documents.Count);

			BusinessObject doc = instance.CreateDocument(ZGuid.NewZGuid());
			AssertNotNull("Doc shouldn't be null even if invalid parent pk passed in", doc);
		}

		public void TestGetFactoryOnInterface()
		{
			var instance = MasterFactory as IDocumentFactory;
			NumberedBusinessObjectFactory one = MasterFactory.GetFactory(1);
			AssertEquals("Same factory should be returned", one, instance.GetFactory(1));
		}

		public void TestGetStorageMainForPK()
		{
			ZGuid testPK = ZGuid.NewZGuid();
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_ParentFK = testPK;
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Correct object retrieved", main, MasterFactory.GetStorageMainForPK(testPK));
			AssertEquals("GetStorageMain did not return the same as GetStorageMainForPK", MasterFactory.GetStorageMainForPK(testPK), MasterFactory.GetStorageMainForPK(testPK));
			AssertNull("No object returned", MasterFactory.GetStorageMainForPK(ZGuid.NewZGuid()));
		}

		public void TestGetDocumentAndFilesCountForStorageMain()
		{
			SetupParent();
			StorageDocs document = Parent.Documents.AddNew();
			document.SC_IsPublished = true;
			StorageDocs document2 = Parent.Documents.AddNew();
			document2.SC_IsPublished = true;
			StorageDocs document3 = Parent.Documents.AddNew();
			document3.SC_IsPublished = true;
			StorageFile file1 = Parent.Files.AddNew();
			file1.SC_IsPublished = true;
			MasterFactory.Save();

			AssertEquals("Parent's eDocs count should be 4", 4, Parent.eDocs.Count);
			StorageMain_NoAccessForDocuments loadedParent = MasterFactory.Load<StorageMain_NoAccessForDocuments>(Parent.PK);

			int count = 0;
			try
			{
				count = MasterFactory.GetCountOfPublishedDocumentsAndFilesForStorageMain(loadedParent);
			}
			catch (Exception e)
			{
				Fail("GetDocumentCountForStorageMain shouldn't access the Documents collection on ParentMain. Message (" + e.Message + ")");
			}

			AssertEquals("Count should have returned 4", 4, count);

			document3.SC_IsDeleted = true;
			MasterFactory.Save();
			loadedParent = MasterFactory.Load<StorageMain_NoAccessForDocuments>(Parent.PK);
			count = 0;

			try
			{
				count = MasterFactory.GetCountOfPublishedDocumentsAndFilesForStorageMain(loadedParent);
			}
			catch (Exception e)
			{
				Fail("GetDocumentCountForStorageMain shouldn't access the Documents collection on ParentMain. Message (" + e.Message + ")");
			}

			AssertEquals("Document count should exclude the deleted document", 3, count);

			document2.SC_IsPublished = false;
			MasterFactory.Save();
			loadedParent = MasterFactory.Load<StorageMain_NoAccessForDocuments>(Parent.PK);
			count = 0;

			try
			{
				count = MasterFactory.GetCountOfPublishedDocumentsAndFilesForStorageMain(loadedParent);
			}
			catch (Exception e)
			{
				Fail("GetDocumentCountForStorageMain shouldn't access the Documents collection on ParentMain. Message (" + e.Message + ")");
			}

			AssertEquals("Document count should exclude the unpublished document", 2, count);
		}

		[ExpectNoExceptions]
		public void TestGetDocumentCountWithNullParent()
		{
			int count = MasterFactory.GetCountOfPublishedDocumentsAndFilesForStorageMain(null);
			AssertEquals("If null parent passsed in the count should be 0", 0, count);
		}

		[ExpectException(typeof(EDocsOffLineException))]
		public void TestAddFileOrDocument_WithSqlError()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			StorageMain parent = MasterFactory.GetStorageMainForPK(shipment.PK);
			AssertNull("Precondition: parent shouldn't exist", parent);

			var documentFactory = MasterFactory as IDocumentFactory;

			documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, SamplePdfBytes, "Sample.pdf", "CIV", "DEF", true);

			parent = MasterFactory.GetStorageMainForPK(shipment.PK);
			parent.Factory.Save();

			DocumentFactory masterFactory2 = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain parentInNewFactory = masterFactory2.GetStorageMainForPK(shipment.PK);
			parentInNewFactory.SM_DB = 999;
			masterFactory2.Save();

			((IDocumentFactory)masterFactory2).AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, SamplePdfBytes, "Sample.pdf", "CIV", "DEF", true);
		}

		public void TestAddFileOrDocument_WithPDFFile()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			StorageMain parent = MasterFactory.GetStorageMainForPK(shipment.PK);
			AssertNull("Precondition: parent shouldn't exist", parent);

			var documentFactory = MasterFactory as IDocumentFactory;

			BusinessObject bizOAdded = documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, SamplePdfBytes, "Sample.pdf", "CIV", "DEF", true);
			Assert(bizOAdded is StorageFile);
			AssertNotNull("Parent should be created when document is created", ((StorageFile)bizOAdded).ParentMain);
			parent = ((StorageFile)bizOAdded).ParentMain;
			AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
			AssertEquals("File name should be the same as what was added", "Sample.pdf", parent.eDocs[0].SC_FileNameWithExtension);
			AssertEquals("File should be the same pk", bizOAdded.PK, parent.eDocs[0].PK);
			AssertEquals("DocType should be set", "CIV", parent.eDocs[0].SC_DocType);

			BusinessObject bizOAdded2 = documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, TestXlsBytes, "Test.xls", "CIV", "DEF", true);
			Assert(bizOAdded2 is StorageFile);
			AssertEquals("eDocs count should be 2", 2, parent.eDocs.Count);
			AssertEquals("File name should be same as what ws added", "Test.xls", bizOAdded2[StorageFile.Schema.SC_FileNameWithExtensionSchemaName]);
			AssertEquals("File should be the same pk", bizOAdded2.PK, parent.eDocs[1].PK);
			AssertEquals("DocType should be set", "CIV", parent.eDocs[1].SC_DocType);

			BusinessObject bizOAdded3 = documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, SamplePdfBytes, "Sample.pdf", "NOT", "DEF", true);
			Assert(bizOAdded3 is StorageFile);
			AssertEquals("eDocs count should be 2 - the original Sample.pdf should have been overwritten", 2, parent.eDocs.Count);
			AssertEquals("Filename should be the same", "Sample.pdf", bizOAdded3[StorageFile.Schema.SC_FileNameWithExtensionSchemaName]);
			AssertEquals("Original file added and the new file added should be the same pk", bizOAdded.PK, bizOAdded3.PK);
			AssertEquals("DocType should be set", "NOT", parent.eDocs[0].SC_DocType);

			BusinessObject bizOAdded4 = documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, SamplePdfBytes, "Sample.pdf", "CIV", "DEF", false);
			Assert(bizOAdded4 is StorageFile);
			AssertEquals("eDocs count should be 3 - the file Sample.pdf has not been overwritten", 3, parent.eDocs.Count);
			AssertEquals("Filename should be different", "Sample[2].pdf", bizOAdded4[StorageFile.Schema.SC_FileNameWithExtensionSchemaName]);
			AssertEquals("DocType should be set", "CIV", parent.eDocs[2].SC_DocType);
		}

		public void TestAddFileOrDocument_WithImageFile()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			StorageMain parent = MasterFactory.GetStorageMainForPK(shipment.PK);
			AssertNull("Precondition: parent shouldn't exist", parent);

			var documentFactory = MasterFactory as IDocumentFactory;
			var smallGifBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");

			StorageDocs storageDoc1 = (StorageDocs)documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment,
				smallGifBytes, "small.gif", Core.Constants.RefDocTypes.CommercialInvoice, "DEF", false);
			AssertNotNull("Parent should be created when document is created", storageDoc1.ParentMain);
			parent = storageDoc1.ParentMain;

			AssertEquals("eDocs count should be 1 - a new bizO should be added to the collection", 1, parent.eDocs.Count);
			AssertEquals("Filename should be set", "small", parent.eDocs[0].SC_FileName);
			AssertEquals("DocType should be set", "CIV", parent.eDocs[0].SC_DocType);

			StorageDocs storageDoc2 = (StorageDocs)documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, smallGifBytes, "small.gif", "CIV", "DEF", false);
			documentFactory.Save();

			AssertEquals("eDocs count should be 2 - a new bizO should be added to the collection", 2, parent.eDocs.Count);
			AssertEquals("Filename should be set", "small[2]", parent.eDocs[1].SC_FileName);
			AssertEquals("DocType should be set", "CIV", parent.eDocs[1].SC_DocType);

			StorageDocs storageDoc3 = (StorageDocs)documentFactory.AddFileOrDocument(shipment.PK, Core.Constants.DocManagerCodes.Shipment, smallGifBytes, "small.gif", "CIV", "DEF", true);
			documentFactory.Save();

			AssertEquals("eDocs count should be 3 - a new bizO should be added to the collection regardless of the value of the filename override argument", 3, parent.eDocs.Count);
			AssertEquals("Filename should be set", "small[3]", parent.eDocs[2].SC_FileName);
			AssertEquals("DocType should be set", "CIV", parent.eDocs[2].SC_DocType);
		}

		#region Importing with full allocation info

		public void TestImportWithTIFFileAndAllocatingInfo()
		{
			ImportWithFullAllocationInfo(SmallTifPath);
		}

		public void TestImportWithGIFFileAndAllocatingInfo()
		{
			ImportWithFullAllocationInfo(SmallGifPath);
		}

		public void TestImportWithXLSFileAndAllocatingInfo()
		{
			ImportWithFullAllocationInfo(TestXlsPath);
		}

		public void TestImportWithPDFFileAndAllocatingInfo()
		{
			ImportWithFullAllocationInfo(SamplePdfPath);
		}

		public void TestImportWithTIFFileAndAllocatingInfo_Restricted()
		{
			ImportWithFullAllocationInfo(SmallTifPath, true);
		}

		public void TestImportWithGIFFileAndAllocatingInfo_Restricted()
		{
			ImportWithFullAllocationInfo(SmallGifPath, true);
		}

		public void TestImportWithXLSFileAndAllocatingInfo_Restricted()
		{
			ImportWithFullAllocationInfo(TestXlsPath, true);
		}

		public void TestImportWithPDFFileAndAllocatingInfo_Restricted()
		{
			ImportWithFullAllocationInfo(SamplePdfPath, true);
		}

		void ImportWithFullAllocationInfo(string fileToImport, bool restrictImport = false)
		{
			var collection = new CodeSelectionCollection(SystemDataRegistry.Instance.DocumentTypesRestrictedListProvider);
			if (restrictImport)
			{
				collection.AddNew().Code = "CIV";
			}

			using (SystemDataRegistry.Instance.DocumentTypesRestrictedForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				SetupShipmentObjects();
				SetupStaffMember();

				NumberedBusinessObjectFactory childFactory = MasterFactory.GetFactory(1);
				byte[] contents = DocumentUtilities.GetFileAsBytes(fileToImport);

				bool imported = MasterFactory.Import(contents, Path.GetFileName(fileToImport), DocManagerCodes.Shipment, ShipmentS00001000.PK, "CIV", "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true);
				using (SwitchToTemporaryUser(StaffMember))
				{
					MasterFactory.Save();
				}

				if (restrictImport)
				{
					AssertEquals("MasterFactory Import should return false", false, imported);
					StorageDocsBase[] documents = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), new ZQuery());
					AssertEquals("Should be 0 docs in the 1st db (allocated doc)", 0, documents.Length);
				}
				else
				{
					AssertEquals("MasterFactory Import should return true", true, imported);
					AssertEquals("should be no docs in the main db - they should have been allocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

					StorageDocsBase[] documents = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), new ZQuery());
					AssertEquals("Should be 1 doc in the 1st db (allocated doc)", 1, documents.Length);

					AssertDocumentProperties(documents[0], Path.GetFileNameWithoutExtension(fileToImport), "CIV", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK, StaffMember.GS_Code, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
				}
			}
		}

		#endregion

		#region

		public void TestImporttWithTIFFileAndDeniedDocType()
		{
			ImportWithDeniedDocType(SmallTifPath);
		}

		public void TestImportWithGIFFileAndDeniedDocType()
		{
			ImportWithDeniedDocType(SmallGifPath);
		}

		public void TestImportWithXLSFileAndDeniedDocType()
		{ 
			ImportWithDeniedDocType(TestXlsPath);
		}

		public void TestImportWithPDFFileAndDeniedDocType()
		{
			ImportWithDeniedDocType(SamplePdfPath);
		}

		void ImportWithDeniedDocType(string fileToImport)
		{
			Env.Security.GetDocumentTypeUploadCheckPoint("CIV").IsAllowed = false;
			SetupShipmentObjects();

			var childFactory = MasterFactory.GetFactory(1);
			var contents = DocumentUtilities.GetFileAsBytes(fileToImport);

			var imported = MasterFactory.Import(contents, Path.GetFileName(fileToImport), DocManagerCodes.Shipment, ShipmentS00001000.PK, "CIV", "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true);

			AssertEquals("MasterFactory Import should return true", true, imported);
			AssertEquals("should be no docs in the main db - they should have been allocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			var documents = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), new ZQuery());
			AssertEquals("Should be 1 doc in the 1st db (allocated doc)", 1, documents.Length);

			var document = documents[0];
			AssertDocumentProperties(document, Path.GetFileNameWithoutExtension(fileToImport), "CIV", DocManagerCodes.Shipment, ShipmentS00001000.PK, "", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);

			AssertEquals("expected 'Commercial Invoice' for Doc Type", "Commercial Invoice", document.SC_Desc);
		}

		#endregion

		#region Importing with partial allocation info

		public void TestImportWithTIFFileAndPartialAllocatingInfo()
		{
			ImportWithPartialAllocatingInfo(SmallTifPath);
		}

		public void TestImportWithGIFFileAndPartialAllocatingInfo()
		{
			ImportWithPartialAllocatingInfo(SmallGifPath);
		}

		void ImportWithPartialAllocatingInfo(string fileToImport)
		{
			SetupShipmentObjects();
			SetupStaffMember();
			NumberedBusinessObjectFactory childFactory = MasterFactory.GetFactory(1);

			byte[] contents = DocumentUtilities.GetFileAsBytes(fileToImport);

			bool imported = MasterFactory.Import(contents, Path.GetFileName(fileToImport), DocManagerCodes.Shipment, ZGuid.Empty, "CIV", "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			StorageDocsBase[] documents = (StorageDocsBase[])MasterFactory.Load(typeof(StorageDocsUnallocated), new ZQuery());
			AssertEquals("should be 1 doc in the main db - not enough info to allocate the doc", 1, documents.Length);
			AssertEquals("Should be no docs in the first db, not enough info to allocate any docs", 0, childFactory.GetDatabaseCount(typeof(StorageDocs)));

			AssertDocumentProperties(documents[0], Path.GetFileNameWithoutExtension(fileToImport), "CIV", Core.Constants.DocManagerCodes.Shipment, ZGuid.Empty, StaffMember.GS_Code, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		public void TestImportWithXLSFileAndPartialAllocatingInfo()
		{
			SetupStaffMember();
			bool imported = MasterFactory.Import(TestXlsBytes, "Test.xls", DocManagerCodes.Shipment, ZGuid.Empty, "CIV", "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true);
			AssertEquals("Import not successful because you can't import non image file without a valid REF and PK guid allocation info", false, imported);

			AssertEquals("Should be no extra docs created", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no extra mains created", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));
		}

		#endregion

		#region Import with no document type

		public void TestImportWithTIFFileAndNoDocType()
		{
			ImportWithNoDocType(SmallTifPath);
		}

		public void TestImportWithGIFFileAndNoDocType()
		{
			ImportWithNoDocType(SmallGifPath);
		}

		public void TestImportWithXLSFileAndNoDocType()
		{
			ImportWithNoDocType(TestXlsPath);
		}

		public void TestImportWithPDFFileAndNoDocType()
		{
			ImportWithNoDocType(SamplePdfPath);
		}

		void ImportWithNoDocType(string fileToImport)
		{
			SetupShipmentObjects();
			SetupStaffMember();
			NumberedBusinessObjectFactory childFactory = MasterFactory.GetFactory(1);

			byte[] contents = DocumentUtilities.GetFileAsBytes(fileToImport);

			bool imported = MasterFactory.Import(contents, Path.GetFileName(fileToImport), DocManagerCodes.Shipment, ShipmentS00001000.PK, ZString.Empty, "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			StorageDocsBase[] documents = (StorageDocsBase[])MasterFactory.Load(typeof(StorageDocs), new ZQuery());
			AssertEquals("should be no doc in the main db - can allocate based on ref info and dfault doctype", 0, documents.Length);
			documents = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), new ZQuery());
			AssertEquals("Should be 1 doc in the first db", 1, documents.Length);

			AssertDocumentProperties(documents[0], Path.GetFileNameWithoutExtension(fileToImport), "", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK, StaffMember.GS_Code, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		#endregion

		#region Import with no allocating info

		public void TestImportWithTIFFileAndNoAllocatingInfo()
		{
			ImportWithNoAllocatingInfo(SmallTifPath);
		}

		public void TestImportWithGIFFileAndNoAllocatingInfo()
		{
			ImportWithNoAllocatingInfo(SmallGifPath);
		}

		public void TestImportWithPDFFileAndNoAllocatingInfo()
		{
			ImportWithNoAllocatingInfo(SamplePdfPath);
		}

		public void TestImportWithXLSFileAndNoAllocatingInfo()
		{
			int previousCount = MasterFactory.GetDatabaseCount(typeof(StorageMain));
			SetupStaffMember();
			bool imported = MasterFactory.Import(TestXlsBytes, "Test.xls", string.Empty, ZGuid.Empty, string.Empty, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true);
			AssertEquals("Import not successful because you can't import non image file without allocation info", false, imported);

			AssertEquals("Should be no extra docs created", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no extra mains created", previousCount, MasterFactory.GetDatabaseCount(typeof(StorageMain)));
		}

		void ImportWithNoAllocatingInfo(string fileToImport)
		{
			SetupStaffMember();
			NumberedBusinessObjectFactory childFactory = MasterFactory.GetFactory(1);

			byte[] contents = DocumentUtilities.GetFileAsBytes(fileToImport);

			bool imported = MasterFactory.Import(contents, Path.GetFileName(fileToImport), string.Empty, ZGuid.Empty, string.Empty, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			var documents = (StorageDocsBase[])MasterFactory.Load<StorageDocsUnallocated>(new ZQuery());
			AssertEquals("should be 1 doc in the main db - not enough info to allocate the doc", 1, documents.Length);
			AssertEquals("Should be no docs in the first db, not enough info to allocate any docs", 0, childFactory.GetDatabaseCount(typeof(StorageDocs)));

			AssertDocumentProperties(documents[0], Path.GetFileNameWithoutExtension(fileToImport), string.Empty, "UNA", ZGuid.Empty, StaffMember.GS_Code);
		}

		#endregion

		public void TestImportWithInvalidAllocatingInfo()
		{
			SetupStaffMember();
			string finalDocType;
			bool imported = MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", "AAA", ZGuid.Empty, "CIV", "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true, out finalDocType);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			StorageDocsUnallocated document = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery());
			AssertDocumentProperties(document, "MultipageTestDocument", "CIV", "AAA", ZGuid.Empty, StaffMember.GS_Code, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		public void TestImportWithTIFFileWithBarcodesAndNoAllocatingInfo()
		{
			SetupShipmentObjects();
			SetupStaffMember();
			NumberedBusinessObjectFactory childFactory = MasterFactory.GetFactory(1);

			string finalDocType;
			bool imported = MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", string.Empty, ZGuid.Empty, string.Empty, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", true, out finalDocType);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			ZQuery loadQuery = new ZQuery();
			loadQuery.OrderBy = StorageDocsSchema.SC_DocType.Name;

			// load by doctype alphabetically
			StorageDocsBase[] documentsInDb0 = (StorageDocsBase[])MasterFactory.Load(typeof(StorageDocsUnallocated), loadQuery);
			AssertEquals("should be 2 docs in the main db - not enough info to allocate the doc", 2, documentsInDb0.Length);

			// load by doctype alphabetically
			StorageDocsBase[] documentsInDb1 = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), loadQuery);
			AssertEquals("Should be 2 docs in the first db, based on barcoding info enough info to allocate any docs", 2, documentsInDb1.Length);

			AssertDocumentProperties(documentsInDb0[0], "MultipageTestDocument", "MAN", "UNA", ZGuid.Empty, StaffMember.GS_Code);
			AssertDocumentProperties(documentsInDb0[1], "MultipageTestDocument", "PKL", "UNA", ZGuid.Empty, StaffMember.GS_Code);
			AssertDocumentProperties(documentsInDb1[0], "MultipageTestDocument", "CIV", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK, StaffMember.GS_Code);
			AssertDocumentProperties(documentsInDb1[1], "MultipageTestDocument", "HBL", Core.Constants.DocManagerCodes.Shipment, ShipmentWithHouseBill.PK, StaffMember.GS_Code);

			var anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertEquals("barcode saved to database", "^DOC=MAN|", anotherFactory.Load<StorageDocsUnallocated>(documentsInDb0[0].PK).ScannedBarcodeValue);
			AssertEquals("barcode saved to database", "^DOC=PKL|", anotherFactory.Load<StorageDocsUnallocated>(documentsInDb0[1].PK).ScannedBarcodeValue);
		}

		public void TestImportWithTIFFileWithBarcodesAndAllocatingInfo()
		{
			SetupShipmentObjects();
			SetupStaffMember();
			NumberedBusinessObjectFactory childFactory = MasterFactory.GetFactory(1);

			string finalDocType;
			bool imported = MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", DocManagerCodes.Shipment, ShipmentS00001000.PK, "HBL", "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true, out finalDocType);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			StorageDocsBase[] documentsInDb0 = (StorageDocsBase[])MasterFactory.Load(typeof(StorageDocs), new ZQuery());
			AssertEquals("should be 0 docs in the main db - all had info to allocate properly", 0, documentsInDb0.Length);

			StorageDocsBase[] documentsInDb1 = (StorageDocsBase[])childFactory.Load(typeof(StorageDocs), new ZQuery());
			AssertEquals("Should be 1 doc in the first db, based on allocating info allocate any docs", 1, documentsInDb1.Length);

			AssertDocumentProperties(documentsInDb1[0], "MultipageTestDocument", "HBL", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK, StaffMember.GS_Code, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		public void TestBarcodeIsSavedAfertImporting()
		{
			SetupShipmentObjects();
			SetupStaffMember();

			var fileToImport = TwoBarcodesOnSamePageTifPath;
			var contents = DocumentUtilities.GetFileAsBytes(fileToImport);
			string finalDocType;
			bool imported = MasterFactory.Import(contents, Path.GetFileName(fileToImport), "AAA", ZGuid.Empty, string.Empty, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, fileToImport, true, out finalDocType);
			using (SwitchToTemporaryUser(StaffMember))
			{
				MasterFactory.Save();
			}

			AssertEquals("MasterFactory Import should return true", true, imported);

			var loadQuery = new ZQuery();
			loadQuery.OrderBy = StorageDocsSchema.SC_DocType.Name;

			var documentsInDb0 = (StorageDocsBase[])MasterFactory.Load(typeof(StorageDocsUnallocated), loadQuery);
			AssertEquals("should be 1 docs in the main db - not enough info to allocate the doc", 1, documentsInDb0.Length);

			var anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertEquals("^DMC=CN00000023;MFD;| , ^DMC=CN00000004;MFD;|", anotherFactory.Load<StorageDocsUnallocated>(documentsInDb0[0].PK).ScannedBarcodeValue);
		}

		public void TestJoinValidBarcodesOnAdd()
		{
			var fileToImport = TwoBarcodesOnSamePageTifPath;
			var contents = DocumentUtilities.GetFileAsBytes(fileToImport);

			var factory = new DocumentFactoryBarcodeReading(Factory);
			var importer = new FileImporterDocumentFactoryTest(factory, true);

			AssertNoExceptionThrown("Should not throw exception on allocate documents", () => { factory.ImportByUserInformation(importer, contents, Path.GetFileName(fileToImport), "AAA", ZGuid.Empty, string.Empty, "DEF", fileToImport, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty); });

			Assert("Should have barcodes that do not exceed the limit of ScannedBarcodeValue field", factory.Load<StorageDocsUnallocated>(new ZQuery()).FirstOrDefault().ScannedBarcodeValue == "^TRN=S00028854/I;CAD;| , ^TRN=S00028854/I;CAD;| , ^TRN=S00028854/I;CAD;|");
		}

		public void TestUnpublishOlderVersionDocumentsAfterImport()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupStaffMember();
			var allReferenceTypeQuery = new ZQuery(RefDocTypeSchema.RT_ReferenceType, "ALL");
			var publishedDocType = MasterFactory.LoadTop1<RefDocType>(new ZQuery(allReferenceTypeQuery, new ZQuery(RefDocTypeSchema.RT_IsPublished, true)));
			var refPK = ZGuid.NewZGuid();

			// Test documents
			var imported = MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", StaffMember.GS_Code, refPK, publishedDocType.RT_DocType, "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true, out _);
			MasterFactory.Save();
			AssertEquals("MasterFactory Import should return true", true, imported);

			BusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			var document = factoryOne.LoadTop1<StorageDocsBase>(new ZQuery());
			Assert("Document is published", document.SC_IsPublished);

			imported = MasterFactory.Import(MultipageTestDocumentTifBytes, "MultipageTestDocument.tif", StaffMember.GS_Code, refPK, publishedDocType.RT_DocType, "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true, out _);
			MasterFactory.Save();
			AssertEquals("MasterFactory Import should return true", true, imported);

			var loadQuery = new ZQuery();
			loadQuery.OrderBy = StorageDocsSchema.SC_FileName.Name;
			var documents = factoryOne.Load<StorageDocsBase>(loadQuery);
			AssertEquals("Document is unpublished by the second doc", false, documents[0].SC_IsPublished);
			AssertEquals("The second document should be published", true, documents[1].SC_IsPublished);

			// Test files
			imported = MasterFactory.Import(SamplePdfBytes, "Sample.PDF", StaffMember.GS_Code, refPK, publishedDocType.RT_DocType, "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true, out _);
			MasterFactory.Save();
			AssertEquals("MasterFactory Import should return true", true, imported);

			var filter = new ZQuery(StorageDocsSchema.SC_DataType, SQLComparisonOperator.Equal, "PDF");
			var file = factoryOne.LoadTop1<StorageDocsBase>(filter);
			Assert("File is published", file.SC_IsPublished);

			imported = MasterFactory.Import(SamplePdfBytes, "Sample.PDF", StaffMember.GS_Code, refPK, publishedDocType.RT_DocType, "DEF", GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "", true, out _);
			MasterFactory.Save();
			AssertEquals("MasterFactory Import should return true", true, imported);

			filter.OrderBy = StorageDocsSchema.SC_FileName.Name;
			documents = factoryOne.Load<StorageDocsBase>(filter);
			AssertEquals("File is unpublished by the second doc", false, documents[0].SC_IsPublished);
			AssertEquals("The second file should be published", true, documents[1].SC_IsPublished);
		}

		[UseSnapshotProtection]
		public void TestFindEDocsFromAllSDDatabasesByPK()
		{
			if (!DbHelper.DatabaseExists(2))
			{
				DbHelper.CreateDatabase(2);
			}

			MasterFactory.UseExtraConnectionForEDocsFactories = true;

			var parentDB1 = MasterFactory.New<StorageMain>();
			parentDB1.SM_DB = 1;
			parentDB1.SM_ParentFK = ZGuid.NewZGuid();
			parentDB1.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var doc1DB1 = parentDB1.Documents.AddNew();
			doc1DB1.SC_DocType = "AAA";
			doc1DB1.SC_Desc = "AAA Doc 11";
			doc1DB1.SC_IsPublished = false;
			doc1DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			var doc2DB1 = parentDB1.Documents.AddNew();
			doc2DB1.SC_DocType = "AAA";
			doc2DB1.SC_Desc = "AAA Doc 12";
			doc2DB1.SC_IsPublished = true;
			doc2DB1.SC_ImageData = new byte[] { 1, 2, 3 };

			var parentDB2 = MasterFactory.New<StorageMain>();
			parentDB2.SM_DB = 2;
			parentDB2.SM_ParentFK = ZGuid.NewZGuid();
			parentDB2.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var doc1DB2 = parentDB2.Documents.AddNew();
			doc1DB2.SC_DocType = "AAA";
			doc1DB2.SC_Desc = "AAA Doc 21";
			doc1DB2.SC_IsPublished = false;
			doc1DB2.SC_ImageData = new byte[] { 1, 2, 3 };
			var doc2DB2 = parentDB2.Documents.AddNew();
			doc2DB2.SC_DocType = "AAA";
			doc2DB2.SC_Desc = "AAA Doc 22";
			doc2DB2.SC_IsPublished = true;
			doc2DB2.SC_ImageData = new byte[] { 1, 2, 3 };

			MasterFactory.Save();

			var instance = MasterFactory as IDocumentFactory;
			AssertEquals("AAA Doc 11", instance.FindEDocsFromAllSDDatabasesByPK(doc1DB1.PK).Description);
			AssertEquals("AAA Doc 12", instance.FindEDocsFromAllSDDatabasesByPK(doc2DB1.PK).Description);
			AssertEquals("AAA Doc 21", instance.FindEDocsFromAllSDDatabasesByPK(doc1DB2.PK).Description);
			AssertEquals("AAA Doc 22", instance.FindEDocsFromAllSDDatabasesByPK(doc2DB2.PK).Description);

			DbHelper.DropDatabase(DbHelper.GetDatabaseName(2));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			originalDDIDocumentSourceValue = SystemDataRegistry.Instance.DDIDocumentSource.Value;
			SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			DbHelper = new DocManagerDBHelperTestClass();
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DbHelper.GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));
			MasterFactory = new DocumentFactoryProviderForTest().GetFactory(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			MasterFactory.Dispose();
			SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalDDIDocumentSourceValue);
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			if (DbHelper.DatabaseExists(2))
			{
				DbHelper.DropDatabase(DbHelper.GetDatabaseName(2));
			}
			base.OnAfterBaseTestCaseRunBare();
		}

		bool originalDDIDocumentSourceValue;

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DbHelper = new DocManagerDBHelperTestClass();
			if (!DbHelper.DatabaseExists(1))
			{
				DbHelper.CreateDatabase(1);
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string SmallTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallTifPath))
				{
					smallTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
				}
				return smallTifPath;
			}
		}
		string smallTifPath;

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		string SmallThreePagesTif
		{
			get
			{
				if (string.IsNullOrEmpty(smallThreePagesTif))
				{
					smallThreePagesTif = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small_3pages.tif");
				}
				return smallThreePagesTif;
			}
		}
		string smallThreePagesTif;

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		string SamplePdfPath
		{
			get
			{
				if (string.IsNullOrEmpty(samplePdf))
				{
					samplePdf = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				}
				return samplePdf;
			}
		}
		string samplePdf;

		byte[] MultipageTestDocumentTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTestDocument.tif");

		byte[] TestXlsBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");

		string TestXlsPath
		{
			get
			{
				if (string.IsNullOrEmpty(testXlsPath))
				{
					testXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");
				}
				return testXlsPath;
			}
		}
		string testXlsPath;

		string SmallGifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallGifPath))
				{
					smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
				}
				return smallGifPath;
			}
		}
		string smallGifPath;

		string TwoBarcodesOnSamePageTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(twoBarcodesOnSamePageTifPath))
				{
					twoBarcodesOnSamePageTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.TwoBarcodesOnSamePage.TIF");
				}
				return twoBarcodesOnSamePageTifPath;
			}
		}
		string twoBarcodesOnSamePageTifPath;

		byte[] PdfDocumentWithBarcodeBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.PDF Document with Barcode - S00001000.PDF");

		void SetupParent()
		{
			Parent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_DB = 1;
		}

		void SetupStaffMember()
		{
			StaffMember = MasterFactory.New<GlbStaff>();
			StaffMember.GS_Code = "AA";
			StaffMember.GS_LoginName = "AA";
			StaffMember.GS_FullName = "Test Person";
			MasterFactory.Save();
		}

		void SetupShipmentObjects()
		{
			ZQuery query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000");
			ShipmentS00001000 = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query);

			if (ShipmentS00001000 == null)
			{
				ShipmentS00001000 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
				ShipmentS00001000[JobShipmentSchema.JS_UniqueConsignRef] = "S00001000";
			}

			query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001");
			ShipmentWithHouseBill = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query);

			if (ShipmentWithHouseBill == null)
			{
				ShipmentWithHouseBill = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
				ShipmentWithHouseBill[JobShipmentSchema.JS_UniqueConsignRef] = "S00001001";
			}

			ShipmentWithHouseBill[JobShipmentSchema.JS_HouseBill] = "12345678901234567890";
			ShipmentWithHouseBill[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			ShipmentWithHouseBill[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			MasterFactory.Save();
		}

		IDisposable SwitchToTemporaryUser(GlbStaff member)
		{
			return Env.SetTemporaryUserContext(member.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
		}

		GlbStaff StaffMember;
		new DbBackendDocumentFactoryForTest MasterFactory;
		StorageMain Parent;
		BusinessObject ShipmentS00001000;
		BusinessObject ShipmentWithHouseBill;
		DocManagerDBHelperTestClass DbHelper;

		#endregion
	}

	public class DocumentFactoryNonTransactionalTest : TestCase
	{
		public void TestJpegCodec()
		{
			AssertNotNull(TiffToJpegConverter.JpegCodec.Codec);
			AssertEquals("image/jpeg", TiffToJpegConverter.JpegCodec.Codec.MimeType);
		}

		[UseSnapshotProtection]
		public void TestGetDbWriteableState()
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			string db1Name = masterFactory.GetDatabaseName(1);

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(auxConnection, db1Name);
					AssertEquals("DB001 writeable state", DbWriteableState.Writeable, masterFactory.GetDbWriteableState(1));
					AssertEquals("DB999 should not exist", DbWriteableState.NonExistent, masterFactory.GetDbWriteableState(999));

					// Make DB1 read-only
					auxConnection.AlterDbWriteableStateForDocManager(db1Name, false);
					AssertEquals("DB001 writeable state", DbWriteableState.ReadOnly, masterFactory.GetDbWriteableState(1));
				}
				finally
				{
					// Make DB1 writeable
					auxConnection.AlterDbWriteableStateForDocManager(db1Name, true);
				}
			}
		}
	}

	public class DocumentFactoryBarcodeReading : DbBackendDocumentFactory
	{
		public DocumentFactoryBarcodeReading(BusinessObjectFactory factoryForEverythingExceptEDocs) : base(factoryForEverythingExceptEDocs)
		{
		}

		public new bool ImportByUserInformation(FileImporter importer, byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, string fileFullPath, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK)
		{
			return base.ImportByUserInformation(importer, contents, filenameOnly, userSuppliedRefType, userSuppliedRefPK, userSuppliedDocType, userSuppliedDocSource, fileFullPath, visibleCompanyPK, visibleBranchPK, visibleDepartmentPK);
		}
	}

	public class FileImporterDocumentFactoryTest : FileImporter
	{
		public FileImporterDocumentFactoryTest(DocumentFactory factory, bool isForImport) : base(factory, isForImport)
		{
		}

		public override List<string> ReadBarcodesFromFile(string filePath)
		{
			return new List<string>()
			{
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|",
				"^TRN=S00028854/I;CAD;|"
			};
		}
	}

	#region Help Classes

	public class StorageMain_NoAccessForDocuments : StorageMain
	{
		public StorageMain_NoAccessForDocuments(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new StorageDocsCollectionViewBase Documents
		{
			get { throw new Exception("DocumentsCollection on Parent accessed"); }
		}
	}

	#endregion
}
