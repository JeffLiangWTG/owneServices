using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageMain))]
	sealed class StorageMainTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRunPreSaveValidation_SetImageDataForAll()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var parentMain = MasterFactory.NewWithValidTestData<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parentMain.SM_ParentFK = shipment.PK;
			parentMain.SM_DB = 1;
			Factory.Save();

			var newStorageFile1 = parentMain.Files.AddNew();
			newStorageFile1.SC_FileName = "hello1";
			newStorageFile1.SC_DataType = "PDF";
			newStorageFile1.SaveToTempFile(); // sets the temp file string
			newStorageFile1.IsSC_ImageDataOutOfSync = true;

			var newStorageFile2 = parentMain.Files.AddNew();
			newStorageFile2.SC_FileName = "hello2";
			newStorageFile2.SC_DataType = "PDF";
			newStorageFile2.SaveToTempFile(); // sets the temp file string
			newStorageFile2.IsSC_ImageDataOutOfSync = false;

			try
			{
				AssertEquals("Precondition: SC_ImageData should be empty", true, newStorageFile1.SC_ImageData.IsEmpty);
				AssertEquals("Precondition: SC_ImageData should be empty", true, newStorageFile2.SC_ImageData.IsEmpty);

				var samplePdfTestFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				File.Copy(samplePdfTestFile, newStorageFile1.TempFileName, true);
				File.SetAttributes(newStorageFile1.TempFileName, FileAttributes.Normal);
				File.Copy(samplePdfTestFile, newStorageFile2.TempFileName, true);
				File.SetAttributes(newStorageFile2.TempFileName, FileAttributes.Normal);

				parentMain.ShouldSetImageDataForAll = false;
				parentMain.RunPreSaveValidation();

				AssertEquals("When flag ShouldSetImageDataForAll is false, SC_ImageData will not be set", newStorageFile1.SC_ImageData.Length, 0);
				AssertEquals("SC_ImageData will not be set when IsSC_ImageDataOutOfSync is false", true, newStorageFile2.SC_ImageData.IsEmpty);

				parentMain.ShouldSetImageDataForAll = true;
				parentMain.RunPreSaveValidation();

				AssertGreaterThan("When flag ShouldSetImageDataForAll is true, SC_ImageData will be set", newStorageFile1.SC_ImageData.Length, 0);
				AssertEquals("SC_ImageData will not be set when IsSC_ImageDataOutOfSync is false", true, newStorageFile2.SC_ImageData.IsEmpty);
				AssertEquals("Flag ShouldSetImageDataForAll will be set false", false, parentMain.ShouldSetImageDataForAll);
			}
			finally
			{
				newStorageFile1.Dispose();
				newStorageFile2.Dispose();
			}
		}

		public void TestSetImageDataForAllIfRequired()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var parentMain = MasterFactory.NewWithValidTestData<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parentMain.SM_ParentFK = shipment.PK;
			parentMain.SM_DB = 1;
			MasterFactory.Save();

			var newStorageFile1 = parentMain.Files.AddNew();
			newStorageFile1.SC_FileName = "hello1";
			newStorageFile1.SC_DataType = "PDF";
			newStorageFile1.SaveToTempFile(); // sets the temp file string
			newStorageFile1.IsSC_ImageDataOutOfSync = true;

			var newStorageFile2 = parentMain.Files.AddNew();
			newStorageFile2.SC_FileName = "hello2";
			newStorageFile2.SC_DataType = "PDF";
			newStorageFile2.SaveToTempFile(); // sets the temp file string
			newStorageFile2.IsSC_ImageDataOutOfSync = false;

			try
			{
				AssertEquals("Precondition: SC_ImageData should be empty", true, newStorageFile1.SC_ImageData.IsEmpty);
				AssertEquals("Precondition: SC_ImageData should be empty", true, newStorageFile2.SC_ImageData.IsEmpty);

				var samplePdfTestFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				File.Copy(samplePdfTestFile, newStorageFile1.TempFileName, true);
				File.SetAttributes(newStorageFile1.TempFileName, FileAttributes.Normal);
				File.Copy(samplePdfTestFile, newStorageFile2.TempFileName, true);
				File.SetAttributes(newStorageFile2.TempFileName, FileAttributes.Normal);

				parentMain.ShouldSetImageDataForAll = false;
				parentMain.SetImageDataForAllIfRequired();

				AssertEquals("When flag ShouldSetImageDataForAll is false, SC_ImageData will not be set", newStorageFile1.SC_ImageData.Length, 0);
				AssertEquals("SC_ImageData will not be set when IsSC_ImageDataOutOfSync is false", true, newStorageFile2.SC_ImageData.IsEmpty);

				parentMain.ShouldSetImageDataForAll = true;
				parentMain.SetImageDataForAllIfRequired();

				AssertGreaterThan("When flag ShouldSetImageDataForAll is true, SC_ImageData will be set", newStorageFile1.SC_ImageData.Length, 0);
				AssertEquals("SC_ImageData will not be set when IsSC_ImageDataOutOfSync is false", true, newStorageFile2.SC_ImageData.IsEmpty);
				AssertEquals("Flag ShouldSetImageDataForAll will be set false", false, parentMain.ShouldSetImageDataForAll);
			}
			finally
			{
				newStorageFile1.Dispose();
				newStorageFile2.Dispose();
			}
		}

		public void TestOnFactorySaving_WhenNewDocumentForOtherEnvIsAdded()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var parentMain = MasterFactory.NewWithValidTestData<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parentMain.SM_ParentFK = shipment.PK;
			parentMain.SM_DB = 1;
			Factory.Save();

			var newStorageDoc = parentMain.Documents.AddNew();
			newStorageDoc.SC_ImageData = SmallTifBytes;
			newStorageDoc.SC_Date = ZDateTime.UtcNow;
			newStorageDoc.SC_SM = parentMain.PK;
			newStorageDoc.SC_DocType = Core.Constants.RefDocTypes.CartageAdvice;
			newStorageDoc.SC_Desc = "ForTesting";
			newStorageDoc.SC_GC_Company = ZGuid.NewZGuid();
			newStorageDoc.SC_GB_Branch = ZGuid.NewZGuid();
			newStorageDoc.SC_GE_Department = ZGuid.NewZGuid();

			var newStorageFile = parentMain.Files.AddNew();
			newStorageFile.SC_ImageData = SmallTifBytes;
			newStorageFile.SC_Date = ZDateTime.UtcNow;
			newStorageFile.SC_SM = parentMain.PK;
			newStorageFile.SC_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			newStorageFile.SC_Desc = "ForTesting";
			newStorageFile.SC_GC_Company = ZGuid.NewZGuid();
			newStorageFile.SC_GB_Branch = ZGuid.NewZGuid();
			newStorageFile.SC_GE_Department = ZGuid.NewZGuid();

			AssertEquals("There should be no required document before saving", 0, parentMain.GetRequiredDocuments().Count);

			MasterFactory.Save();

			var requiredDocuments = parentMain.GetRequiredDocuments();

			AssertEquals("There should be 2 required document after saving", 2, requiredDocuments.Count);
			AssertNotNull(requiredDocuments.GetDocByType(Core.Constants.RefDocTypes.AgentsInstruction));
			AssertNotNull(requiredDocuments.GetDocByType(Core.Constants.RefDocTypes.CartageAdvice));
		}

		public void TestSaveMultiStorageMainWithSamePKAtSameTime()
		{
			var doc1 = ParentMain.Documents.AddNew();
			doc1.SC_ImageData = SmallTifBytes;
			doc1.SC_Date = ZDateTime.UtcNow;
			doc1.SC_SM = ParentMain.PK;
			doc1.SC_DocType = DocTypeForRequiredDocument ?? "ACV";

			var requiredDocumentsParent = ParentMain.DocumentOwner as IHaveRequiredDocuments;
			if (requiredDocumentsParent == null)
			{
				var docsAndCartageParent = (IDocsAndCartageParent)ParentMain.DocumentOwner;
				requiredDocumentsParent = docsAndCartageParent.RequiredDocumentsProvider;
			}

			var secondFactory = new DbBackendDocumentFactory(new BusinessObjectFactory(Db.NewExtraConnectionToMainDb()));
			ParentMain.OnFactorySavingForTest += (sender, e) =>
			{
				var parentMain1 = secondFactory.Load<StorageMainInAnotherDbConnectionForTest>(ParentMain.PK);
				parentMain1.SetConnection(Db.NewAdminConnection());

				var doc2 = parentMain1.Documents.AddNew();
				doc2.SC_ImageData = SmallTifBytes;
				doc2.SC_Date = ZDateTime.UtcNow;
				doc2.SC_SM = ParentMain.PK;
				doc2.SC_DocType = DocTypeForRequiredDocument ?? "ACV";

				AssertNoExceptionThrown(() => RowFactory.SaveTogether(new TransactionCoordinatorInMultipleParticipant(new ITransactionParticipant[] { secondFactory })));
				Assert(doc2.Factory.SqlLockCount == 0);
			};

			AssertEquals("Should be 0 required documents after the test was set up (because none of the starting documents have legal SC_DocType", 0, requiredDocumentsParent.RequiredDocuments.Count);
			MasterFactory.Save();

			AssertEquals("should have 1 required document added", 1, requiredDocumentsParent.RequiredDocuments.Count);
			Assert(doc1.Factory.SqlLockCount == 0);
		}

		public void TestSaveMultiStorageMainWithSameDocumentOwnerAtSameTime()
		{
			var declaration = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.FillWithValidTestData();

			var custEntryHeader = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
			custEntryHeader.FillWithValidTestData();
			custEntryHeader["CH_JE"] = declaration.PK;

			var declarationStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			declarationStorageMain.SM_Type = Core.Constants.DocManagerCodes.JobDeclaration;
			declarationStorageMain.SM_ParentFK = declaration.PK;

			var doc1 = declarationStorageMain.Documents.AddNew();
			doc1.SC_ImageData = SmallTifBytes;
			doc1.SC_Date = ZDateTime.UtcNow;
			doc1.SC_SM = declarationStorageMain.PK;
			doc1.SC_DocType = DocTypeForRequiredDocument;

			var requiredDocumentsParent = declarationStorageMain.DocumentOwner as IHaveRequiredDocuments;
			if (requiredDocumentsParent == null)
			{
				var docsAndCartageParent = (IDocsAndCartageParent)declarationStorageMain.DocumentOwner;
				requiredDocumentsParent = docsAndCartageParent.RequiredDocumentsProvider;
			}

			declarationStorageMain.OnFactorySavingForTest += (sender, e) =>
			{
				var secondFactory = new DbBackendDocumentFactory(new BusinessObjectFactory(Db.NewExtraConnectionToMainDb()));

				var custEntryStorageMain = secondFactory.NewWithValidTestData<StorageMainInAnotherDbConnectionForTest>();
				custEntryStorageMain.SM_Type = Core.Constants.DocManagerCodes.CustomsEntry;
				custEntryStorageMain.SM_ParentFK = custEntryHeader.PK;
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(custEntryStorageMain, custEntryHeader);
				custEntryStorageMain.SetConnection(Db.NewAdminConnection());

				var doc2 = custEntryStorageMain.Documents.AddNew();
				doc2.SC_ImageData = SmallTifBytes;
				doc2.SC_Date = ZDateTime.UtcNow;
				doc2.SC_SM = custEntryStorageMain.PK;
				doc2.SC_DocType = DocTypeForRequiredDocument;

				AssertNoExceptionThrown(() => RowFactory.SaveTogether(new TransactionCoordinatorInMultipleParticipant(new ITransactionParticipant[] { secondFactory })));
				Assert(doc2.Factory.SqlLockCount == 0);
			};

			AssertEquals("Should have no required document by default", 0, requiredDocumentsParent.RequiredDocuments.Count);
			MasterFactory.Save();

			AssertEquals("should have 1 required document added", 1, requiredDocumentsParent.RequiredDocuments.Count);
			Assert(doc1.Factory.SqlLockCount == 0);
		}

		public void TestNoSqlLockAcquiredWhenNoRequiredDocumentIsAboutToBeAdded()
		{
			var doc1 = ParentMain.Documents.AddNew();
			doc1.SC_ImageData = SmallTifBytes;
			doc1.SC_Date = ZDateTime.UtcNow;
			doc1.SC_SM = ParentMain.PK;
			doc1.SC_DocType = "MSC";

			ParentMain.OnFactorySavingForTest += (sender, e) =>
			{
				Assert("No sql lock acquired as no required documents added.", ParentMain.Factory.SqlLockCount == 0);
			};
			MasterFactory.Save();
		}

		#region document owner

		public void TestSM_TypeOverride()
		{
			ParentMain.SM_Type = "MEW";
			ParentMain.SM_TypeOverride = "ORG";
			AssertEquals("ORG", ParentMain.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Related Business Objects

		#region Documents

		[ExpectNoExceptions()]
		public void TestDocumentsOnDeletedObject()
		{
			ParentMain.Delete();
			AssertNotNull("Accessing documents collection should be OK even on a deleted object. (needed for cleaning up on dispose)", ParentMain.Documents);
		}

		public void TestDocuments()
		{
			AssertEquals(3, ParentMain.Documents.Count);
			AssertEquals("Document collection should be using same factory number as specified by parent", ParentMain.SM_DB, ((NumberedBusinessObjectFactory)ParentMain.Documents.Factory).DBNumber);

			AssertEquals("Precondition: ParentMain doesn't have changes", false, ParentMain.HasChanges);
			StorageDocs document = ParentMain.Documents.AddNew();
			document.SC_Desc = "this is a description";

			AssertEquals("ParentMain should have Changes (Documents collection should be RegisteredEditable)", true, ParentMain.HasChanges);
		}

		public void TestDocumentsAdd()
		{
			int previousDocCount = ParentMain.Documents.Count;
			StorageDocs newDoc = ParentMain.Documents.AddNew();
			newDoc.SC_ImageData = SmallTifBytes;

			AssertEquals("New document's factory number should be the same as the document collection factory", ((NumberedBusinessObjectFactory)ParentMain.Documents.Factory).DBNumber, ((NumberedBusinessObjectFactory)newDoc.Factory).DBNumber);
			AssertEquals("New document's factory instance should be the same as the document collection factory", ParentMain.Documents.Factory, newDoc.Factory);
			AssertEquals(ChildFactory.DBNumber, ((NumberedBusinessObjectFactory)newDoc.Factory).DBNumber);

			MasterFactory.Save();

			ParentMain.Reload();
			AssertEquals("Parent's document count has increased by 1 after adding document", previousDocCount + 1, ParentMain.Documents.Count);

			BusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			StorageDocs loadedDocument = factoryOne.Load(typeof(StorageDocs), newDoc.PK) as StorageDocs;
			AssertNotNull("New doc should load from DB1 successfully", loadedDocument);
		}

		#endregion

		#region Files

		[ExpectNoExceptions()]
		public void TestFilesOnDeletedObject()
		{
			ParentMain.Files.AddNew();
			ParentMain.Delete();
			AssertNotNull("Accessing files collection should be OK even on a deleted object. (needed for cleaning up on dispose)", ParentMain.Files);
			ParentMain.Files.DisposeAll();
		}

		public void TestFiles()
		{
			AssertEquals("Files collection should be using the correct factory as specified by parent", ParentMain.SM_DB, ((NumberedBusinessObjectFactory)ParentMain.Files.Factory).DBNumber);

			AssertEquals("Precondition: ParentMain doesn't have changes", false, ParentMain.HasChanges);
			StorageFile file = ParentMain.Files.AddNew();
			file.SC_FileName = "this is a filename";

			AssertEquals("ParentMain should have Changes (Files collection should be RegisteredEditable)", true, ParentMain.HasChanges);
		}

		public void TestIsFilesLoaded()
		{
			Assert("Files colleciton hasn't been accessed yet, shouldn't be loaded", !ParentMain.IsFilesLoaded);
			int count = ParentMain.Files.Count;
			Assert("Files collection has been accessed, so the files should be loaded", ParentMain.IsFilesLoaded);
		}

		#endregion

		#region eDocs

		public void TestEDocs()
		{
			AssertEquals("eDocs collection should show all documents regardless of published/deleted status", 3, ParentMain.eDocs.Count);

			AssertEquals("Precondition: ParentMain doesn't have changes", false, ParentMain.HasChanges);
			AssertEquals("Precondition: files count of files collection", 0, ParentMain.Files.Count);
			StorageFile file = ParentMain.Files.AddNew();
			AssertEquals("Files count of files collection", 1, ParentMain.Files.Count);
			//TODO: the collection isn't registered editable! a problem with views. It currently works through the gui, so no big drama, but ought to be fixed.
			//			AssertEquals("Parent should havechanges", true, ParentMain.HasChanges);
			//			AssertEquals("Files should register has changes", true, ParentMain.Files.HasChanges);
			AssertEquals("eDocs collection should update after file was added", 4, ParentMain.eDocs.Count);
			//			AssertEquals("eDocs should register haschanges", true, ParentMain.eDocs.HasChanges);
			//			AssertEquals("ParentMain should have changes (eDocs should be registered editable)", true, ParentMain.HasChanges);

			file.SC_IsDeleted = true;
			AssertEquals("Deleted status of file or document should not affect the eDocs collection", 4, ParentMain.eDocs.Count);

			Document1.SC_IsPublished = false;
			AssertEquals("Published status of file or document should not affect the eDocs collection", 4, ParentMain.eDocs.Count);

			Document2.SC_IsPublished = true;
			AssertEquals("Published status of file or document should not affect the eDocs collection", 4, ParentMain.eDocs.Count);
		}

		public void TestEDocsWithUnavailableDatabase()
		{
			ParentMain.SM_DB = 5; // nonexistent db
			AssertEquals("eDocs collection count", 0, ParentMain.eDocs.Count);

			ParentMain.SM_DB = 1; // valid db
			AssertNotNull("eDocs collection should be ok again if db exists", ParentMain.eDocs);
			AssertEquals("eDocs collection count", 3, ParentMain.eDocs.Count);

			ParentMain.SM_DB = 5; // nonexistent db
			AssertEquals("eDocs collection count", 0, ParentMain.eDocs.Count);
		}

		public void TestEDocsView()
		{
			StorageFile file = ParentMain.Files.AddNew();
			AssertEquals("eDocsView should show the same number of documents/files as eDocs collection if none are deleted", ParentMain.eDocs.Count, ParentMain.eDocsView.Count);
			file.SC_IsDeleted = true;
			AssertEquals("eDocsView should show one less than the number of eDocs", ParentMain.eDocs.Count - 1, ParentMain.eDocsView.Count);
			AssertEquals("File does not belong in view collection", false, ParentMain.eDocsView.Contains(file.PK));
		}

		public void TestEDocsViewWithMissingDatabase()
		{
			ParentMain.SM_DB = 999;
			AssertEquals(0, ParentMain.eDocsView.Count);
		}

		public void TestDocumentCollectionViewExcludesUnpublishedDocs()
		{
			RefDocType docType = MasterFactory.New<RefDocType>();
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = Core.Constants.DocManagerCodes.Shipment;

			ParentMain = MasterFactory.New<StorageMain>();
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			StorageDocs unpublishedDoc = (StorageDocs)ParentMain.eDocs.AddNew(typeof(StorageDocs));
			unpublishedDoc.SC_DocType = "AA";
			unpublishedDoc.SC_IsPublished = false;

			StorageDocs publishedDoc = (StorageDocs)ParentMain.eDocs.AddNew(typeof(StorageDocs));
			publishedDoc.SC_DocType = "AAA";
			publishedDoc.SC_IsPublished = true;

			StorageDocs deletedDoc = (StorageDocs)ParentMain.eDocs.AddNew(typeof(StorageDocs));
			deletedDoc.SC_DocType = "AAA";
			deletedDoc.SC_IsDeleted = true;

			AssertEquals("Document Collection View should exclude any unpublished and deleted docs from Documents", ParentMain.eDocs.Count - 2, ((IDocumentsView)ParentMain).DocumentCollectionView.Count);
		}

		public void TestPDFCollectionView()
		{
			ParentMain = MasterFactory.New<StorageMain>();
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			StorageFile doc1 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc1.SC_IsPublished = false;
			doc1.SC_DataType = "pdf";

			StorageFile doc2 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc2.SC_IsPublished = true;
			doc2.SC_DataType = "pdf";

			StorageFile doc3 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc3.SC_IsPublished = true;
			doc3.SC_DataType = "PDF";

			StorageFile doc4 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc4.SC_IsPublished = true;
			doc4.SC_DataType = "PdF";

			StorageFile doc5 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc5.SC_IsPublished = true;
			doc5.SC_DataType = "ZZZ";

			StorageFile doc6 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc6.SC_IsPublished = true;
			doc6.SC_DataType = "PDF";
			doc6.SC_IsDeleted = true;

			StorageFile doc7 = (StorageFile)ParentMain.Files.AddNew(typeof(StorageFile));
			doc7.SC_IsPublished = true;
			doc7.SC_DataType = "txt";

			AssertEquals("Document Collection View should exclude any unpublished, deleted and non-pdf docs from Documents", 3, ((IDocumentsView)ParentMain).PDFFilesCollectionView.Count);
		}

		#endregion

		#region OwnerAssemblyData

		public void TestOwnerAssemblyData()
		{
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var shipData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Shipment);
			Assert("ParentMain should have shipment owner data", shipData.Equals(ParentMain.OwnerAssemblyData));

			ParentMain.SM_Type = "ABC";
			Assert("Invalid SM_Type, should keep previous data type", shipData.Equals(ParentMain.OwnerAssemblyData));

			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			var orgData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Organisation);
			Assert("Valid SM_Type, ParentMain should now have Org owner data", orgData.Equals(ParentMain.OwnerAssemblyData));

			ParentMain.SM_Type = "";
			Assert("empty SM Type, ParentMain keeps previous value", orgData.Equals(ParentMain.OwnerAssemblyData));
		}

		#endregion

		#region RelatedParentMains

		#region OrgHeader2

		class OrgHeader2 : OrgHeader
		{
			public OrgHeader2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get
				{
					return new OrgHeaderDocumentSupporter2(this);
				}
			}
		}

		class OrgHeaderDocumentSupporter2 : OrgHeaderDocumentSupporter
		{
			public OrgHeaderDocumentSupporter2(OrgHeader orgHeader)
				: base(orgHeader)
			{
			}

			public override bool StorageDocsAreEditableIfInRelated
			{
				get
				{
					return true;
				}
			}
		}

		#endregion

		public void TestRelatedParentMains()
		{
			var shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();
			var consignee = MasterFactory.NewWithValidTestData<OrgHeader>();
			var consignor = MasterFactory.NewWithValidTestData<OrgHeader2>();
			shipment["ConsigneePK"] = consignee.PK;
			shipment["ConsignorPK"] = consignor.PK;

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parent.SM_ParentFK = shipment.PK;
			parent.SM_DB = 1;

			var consigneeParent = MasterFactory.New<StorageMain>();
			consigneeParent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consigneeParent.SM_ParentFK = consignee.PK;
			consigneeParent.SM_DB = 1;
			consigneeParent.Documents.AddNew();

			var consignorParent = MasterFactory.New<StorageMain>();
			consignorParent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consignorParent.SM_ParentFK = consignor.PK;
			consignorParent.SM_DB = 1;
			typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(consignorParent, consignor);
			consignorParent.Documents.AddNew();

			MasterFactory.Save();

			AssertEquals("StorageMain related storagemains should be the shipment parent, consignor parent and consignee parent", 3, parent.RelatedParentMains.Count);
			AssertEquals("Contains", true, parent.RelatedParentMains.Contains(consigneeParent.PK));
			AssertEquals("Contains", true, parent.RelatedParentMains.Contains(consignorParent.PK));
			Assert(!parent.IsRegisteredEditableChildObject(consigneeParent));
			Assert(parent.IsRegisteredEditableChildObject(consignorParent));
		}

		public void TestRelatedParentMainsIgnoresBizOWithNoDocuments()
		{
			var shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();
			var consignee = MasterFactory.NewWithValidTestData<OrgHeader>();
			var consignor = MasterFactory.NewWithValidTestData<OrgHeader2>();
			shipment["ConsigneePK"] = consignee.PK;
			shipment["ConsignorPK"] = consignor.PK;

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parent.SM_ParentFK = shipment.PK;
			parent.SM_DB = 1;

			var consigneeParent = MasterFactory.New<StorageMain>();
			consigneeParent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consigneeParent.SM_ParentFK = consignee.PK;
			consigneeParent.SM_DB = 1;

			var consignorParent = MasterFactory.New<StorageMain>();
			consignorParent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			consignorParent.SM_ParentFK = consignor.PK;
			consignorParent.SM_DB = 1;
			consignorParent.Documents.AddNew();

			MasterFactory.Save();

			AssertEquals("StorageMain related StorageMains should be the shipment parent and consignor parent", 2, parent.RelatedParentMains.Count);
			AssertEquals("Not Contains", false, parent.RelatedParentMains.Contains(consigneeParent.PK));
			AssertEquals("Contains", true, parent.RelatedParentMains.Contains(consignorParent.PK));
		}

		public void TestRelatedParentMainsAlwaysShowBizOWithNoDocuments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var declaration = MasterFactory.New<Enterprise.Integration.Customs.AsycudaCustoms.IJobDeclaration>();
				var entryInstruction = MasterFactory.New<Enterprise.Integration.Customs.AsycudaCustoms.ICusEntryInstruction>();
				entryInstruction.CEI_JE = declaration.PK;
				var header = MasterFactory.New<Enterprise.Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
				header.BH_ParentID = entryInstruction.PK;
				header.BH_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
				header.BH_GB = declaration.JE_GB;
				var moveHeader1 = MasterFactory.New<Enterprise.Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
				moveHeader1.BM_BH = header.PK;
				var storageMain1 = MasterFactory.New<StorageMain>();
				storageMain1.SM_Type = Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader;
				storageMain1.SM_ParentFK = moveHeader1.PK;
				storageMain1.SM_DB = 1;
				var moveHeader2 = MasterFactory.New<Enterprise.Integration.Customs.AsycudaCustoms.ICusInBondMoveHeader>();
				moveHeader2.BM_BH = header.PK;
				var storageMain2 = MasterFactory.New<StorageMain>();
				storageMain2.SM_Type = Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader;
				storageMain2.SM_ParentFK = moveHeader2.PK;
				storageMain2.SM_DB = 1;
				MasterFactory.Save();

				ParentMain.SM_Type = Core.Constants.DocManagerCodes.JobDeclaration;
				ParentMain.SM_ParentFK = declaration.PK;
				CombineAssertions(() =>
				{
					var relatedParentMains = ParentMain.RelatedParentMains.Cast<StorageMain>().Where(x => x.SM_Type == Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader).ToArray();
					AssertEquals("StorageMain related StorageMains count", 2, relatedParentMains.Length);
					if (relatedParentMains[0].SM_ParentFK == moveHeader2.PK)
					{
						AssertEquals("contains RelatedOrg(with no document) to show", moveHeader2.PK, relatedParentMains[0].SM_ParentFK);
						AssertEquals("contains RelatedOrg(with no document) to show", moveHeader1.PK, relatedParentMains[1].SM_ParentFK);
					}
					else
					{
						AssertEquals("contains RelatedOrg(with no document) to show", moveHeader1.PK, relatedParentMains[0].SM_ParentFK);
						AssertEquals("contains RelatedOrg(with no document) to show", moveHeader2.PK, relatedParentMains[1].SM_ParentFK);
					}
				});
			}
		}

		public void TestIsRelatedParentMainsLoaded()
		{
			var parent = MasterFactory.New<StorageMain>();
			CombineAssertions(() =>
			{
				Assert("before load", !parent.IsRelatedParentMainsLoaded);
				AssertNotNull("RelatedParentMains", parent.RelatedParentMains);
				Assert("after load", parent.IsRelatedParentMainsLoaded);
			});
		}

		#endregion

		#region DocManagerInfo

		public void TestDocManagerInfo()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parent.SM_ParentFK = ZGuid.NewZGuid();

			AssertNull("DocManagerInfo should be null for a nonexistent owner object", parent.DocManagerInfo);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment.FillWithValidTestData();

			parent.SM_ParentFK = shipment.PK;

			AssertNotNull("DocManagerInfo should not be null if the owner object exists", parent.DocManagerInfo);
		}

		#endregion

		#region PublishedeDocsAndFiles

		public void TestPublishedeDocsAndFiles()
		{
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			AssertEquals("None of the eDocs are published, collection count should be 0", 0, ParentMain.PublishedEDocsAndFiles.Count);

			StorageFile file = ParentMain.Files.AddNew();
			file.SC_IsPublished = true;
			AssertEquals("Now a file is published, collection count should update", 1, ParentMain.PublishedEDocsAndFiles.Count);

			Document1.SC_IsPublished = true;
			AssertEquals("Now a document is published, collection count should update", 2, ParentMain.PublishedEDocsAndFiles.Count);

			file.SC_IsPublished = false;
			AssertEquals("File is unpublished, collection should update", 1, ParentMain.PublishedEDocsAndFiles.Count);
		}

		#endregion

		#region RequiredDocuments

		public void TestGetRequiredDocuments()
		{
			StorageMain storageMain = (StorageMain)GetNewBusinessObject();
			AssertNull("GetRequiredDocuments()", storageMain.GetRequiredDocuments());

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			storageMain = (StorageMain)GetNewBusinessObject();
			storageMain.SM_ParentFK = shipment.PK;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			JobRequiredDocumentDependentCollection requiredDocuments = storageMain.GetRequiredDocuments();
			AssertNotNull("GetRequiredDocuments()", requiredDocuments);
			AssertEquals("GetRequiredDocuments()", ((IDocsAndCartageParent)storageMain.DocumentOwner).RequiredDocumentsProvider.RequiredDocuments, requiredDocuments);
		}

		#endregion

		#endregion

		#region Read Related eDocs Event

		public void TestReadRelatedDocumentsEventRaised()
		{
			Document1.SC_DocType = DocTypeWithForceUserToRead.RT_DocType;
			Document1.SC_SystemCreateUser = "XXX";
			Document2.SC_DocType = DocTypeWithForceUserToRead.RT_DocType;
			Document2.SC_SystemCreateUser = "XXX";
			Document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document3.SC_SystemCreateUser = "XXX";
			MasterFactory.Save();

			AssertReadRequiredDocumentsEvent("No log should be raised initially for the test", false);

			Document1.NotifyReadByUser();
			AssertReadRequiredDocumentsEvent("When not all force-to-read eDocs are read", false);

			Document2.NotifyReadByUser();
			AssertReadRequiredDocumentsEvent("When all force-to-read eDocs are read", true);

			Document2.NotifyReadByUser();
			AssertReadRequiredDocumentsEvent("When force-to-read eDoc is re-read, a second event is not required", false);
		}

		void AssertReadRequiredDocumentsEvent(string message, bool expectNewEventToBeRaised)
		{
			StmALog logFound = null;
			foreach (StmALog log in Org.Logs.GetAllLogs())
			{
				if (log.SL_SE_NKEvent == Events.RelatedEDocsRead.Code && !ReadRequiredEDocsEventsToIgnore.Contains(log))
				{
					if (logFound != null)
					{
						Fail("Expected only 1 event to be raised");
					}
					logFound = log;
				}
			}

			if (expectNewEventToBeRaised)
			{
				AssertNotNull(message, logFound);
				ReadRequiredEDocsEventsToIgnore.Add(logFound);
			}
			else
			{
				AssertNull(message, logFound);
			}
		}
		readonly List<StmALog> ReadRequiredEDocsEventsToIgnore = new List<StmALog>();

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (fDocTypeWithForceUserToRead == null)
				{
					fDocTypeWithForceUserToRead = MasterFactory.New<RefDocType>();
					fDocTypeWithForceUserToRead.RT_ForceUserToRead = true;
					fDocTypeWithForceUserToRead.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
					fDocTypeWithForceUserToRead.RT_DocType = "FUR";
				}
				return fDocTypeWithForceUserToRead;
			}
		}
		RefDocType fDocTypeWithForceUserToRead;

		#endregion

		#region AddFileOrDocument

		public void TestAddFileOrDocumentWhenIsArchiving()
		{
			byte[] contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_Desc = "ABC Description";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			AssertEquals("precondition: 3 children on the parent", 3, ParentMain.eDocs.Count);
			AssertEquals("Precondition: the children are all documents", 3, ParentMain.Documents.Count);

			var documentAdded = ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "document.pdf",
				DocumentType = "ABC",
				Description = "Test Description",
				IsArchiving = true,
			});
			AssertEquals("Test Description", documentAdded.SC_Desc);
		}

		public void TestAddFileOrDocument()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_Desc = "ABC Description";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_OverrideVersions = true;

			var user = Factory.New<GlbStaff>();
			user.GS_IsController = true;
			user.GS_Code = "XXX";
			Factory.Save();

			AssertEquals("precondition: 3 children on the parent", 3, ParentMain.eDocs.Count);
			AssertEquals("Precondition: the children are all documents", 3, ParentMain.Documents.Count);

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
				{
					FileName = "document.pdf",
					DocumentType = "ABC",
				});
				MasterFactory.Save();
			}

			AssertEquals("Should be 4 children on the parent now", 4, ParentMain.eDocs.Count);
			AssertEquals("Should still be 3 documents", 3, ParentMain.Documents.Count);
			AssertEquals("Should be 1 file", 1, ParentMain.Files.Count);

			var fileAdded = ParentMain.Files[0];
			AssertEquals("File added should have the correct doctype", newDocType.RT_DocType, fileAdded.SC_DocType);
			AssertEquals("File added should have the correct description", newDocType.RT_Desc, fileAdded.SC_Desc);
			AssertEquals("File added should have the correct data type (extension of filename)", "PDF", fileAdded.SC_DataType);
			AssertEquals("File added should have the correct filename", "document", fileAdded.SC_FileName);
			AssertEquals("File added should have been logged against the correct user", user.GS_Code, fileAdded.SC_AddingUser);
			AssertEquals("File added should have the right file contents", contents, fileAdded.SC_ImageData);

			StorageDocs documentAdded = null;
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				documentAdded = (StorageDocs)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
				{
					FileName = "document.tif",
					DocumentType = "ABC",
				});
				MasterFactory.Save();
			}

			AssertNotNull("The element added was a document", documentAdded);
			AssertEquals("Should be 5 children on the parent now", 5, ParentMain.eDocs.Count);
			AssertEquals("Should be 4 documents", 4, ParentMain.Documents.Count);
			AssertEquals("Should be 1 file", 1, ParentMain.Files.Count);

			AssertEquals("should have correct doctype", newDocType.RT_DocType, documentAdded.SC_DocType);
			AssertEquals("should have correct description", newDocType.RT_Desc, documentAdded.SC_Desc);
			AssertEquals("should have correct data type", "TIF", documentAdded.SC_DataType);
			AssertEquals("should have correct user", user.GS_Code, documentAdded.SC_AddingUser);
			AssertEquals("should have the right contents", contents, documentAdded.SC_ImageData);
			AssertEquals("Filename should be document", "document", documentAdded.SC_FileName);

			contents = new byte[] { 2, 1, 1, 1, 1, 1, 1, 1 };
			var iStorageMain = ParentMain as IStorageMain;
			var ieDoc = iStorageMain.AddFileOrDocument(contents, "document.pdf", "ABC");
			AssertEquals("Should be 5 children on the parent now", 5, ParentMain.eDocs.Count);
			AssertEquals("should have the right contents", contents, ieDoc.ImageData);

			newDocType.RT_OverrideVersions = false;
			ieDoc = iStorageMain.AddFileOrDocument(contents, "document.pdf", "ABC");
			AssertEquals("Should be 6 children on the parent now", 6, ParentMain.eDocs.Count);
			AssertEquals("should have the right contents", contents, ieDoc.ImageData);
		}

		public void TestAddFileOrDocumentWithBadImage()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			AssertNoExceptionThrown("Adding an image with invalid data doesn't throw exception.", () => ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "document.png",
			}));
			ParentMain.Factory.Save();
		}

		public void TestAddedFileDescriptionForWebShouldBeEqualWithFileName()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			var documentAdded = ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "document.pdf",
				DocumentType = "ABC",
			});

			Assert("DocumentAdded should be StorageFile", documentAdded is StorageFile);
			AssertEquals("Web description should be file name", documentAdded.SC_DescriptionForWeb, "document.pdf");

			documentAdded = ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "document.tif",
				DocumentType = "ABC",
			});
			Assert("DocumentAdded should be StorageDocs", documentAdded is StorageDocs);
			AssertEquals("Web description should be file name", documentAdded.SC_DescriptionForWeb, "document.tif");
		}

		public void TestRemoveIllegalCharactersInExtension()
		{
			StorageMain storageMain = Factory.New<StorageMain>();
			ZString testFileName = "tes?t.p|d*f";
			var result = storageMain.RemoveIllegalCharactersInFileName(testFileName);
			AssertEquals(result, "test.pdf");
		}

		[ExpectNoExceptions]
		public void TestAddFileOrDocumentsWithInvalidCharsInExtension()
		{
			byte[] contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.p|df",
			});
		}

		public void TestAddFileOrDocumentWithEmptyDocType()
		{
			byte[] contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			StorageFile fileAdded = (StorageFile)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "hello.pdf",
			});
			AssertNotNull("The element added should have been a file", fileAdded);
			AssertEquals("should have default doctype", "", fileAdded.SC_DocType);
			AssertEquals("Should have default desc", "", fileAdded.SC_Desc);
			AssertEquals("Should have filename", "hello", fileAdded.SC_FileName);
			AssertEquals("should have datatype", "PDF", fileAdded.SC_DataType);

			StorageDocs docAdded = (StorageDocs)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "hello.tif",
			});
			AssertNotNull("The element added should have been a doc", docAdded);
			AssertEquals("should have default doctype of blank", "", docAdded.SC_DocType);
			AssertEquals("Should have default description of blank", "", docAdded.SC_Desc);
			AssertEquals("should have datatype", "TIF", docAdded.SC_DataType);
			AssertEquals("Filename should be hello", "hello", docAdded.SC_FileName);
		}

		public void TestAddFileOrDocumentWhenFilenameIsEmpty()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_Desc = "ABC Description";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			var documentAdded = (StorageDocs)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				DocumentType = "ABC",
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a document", documentAdded);
			AssertEquals("Should be 4 children on the parent now", 4, ParentMain.eDocs.Count);
			AssertEquals("Should be 4 documents", 4, ParentMain.Documents.Count);
			AssertEquals("Should be 0 files", 0, ParentMain.Files.Count);

			AssertEquals("should have correct doctype", newDocType.RT_DocType, documentAdded.SC_DocType);
			AssertEquals("should have correct description", newDocType.RT_Desc, documentAdded.SC_Desc);
			AssertEquals("should have correct data type", "TIF", documentAdded.SC_DataType);
			AssertEquals("should have correct user", GlbStaff.CurrentUser.GS_Code, documentAdded.SC_SystemCreateUser);
			AssertEquals("should have the right contents", contents, documentAdded.SC_ImageData);
		}

		public void TestAddFileOrDocumentWithDuplicateFilename()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_Desc = "ABC Description";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			var elementAdded = (StorageFile)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.pdf",
				DocumentType = "ABC",
				FileAction = FileAction.Overwrite,
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a file", elementAdded);
			AssertEquals("Should be 4 children on the parent now", 4, ParentMain.eDocs.Count);
			AssertEquals("Shoudl be 3 documents on the parent", 3, ParentMain.Documents.Count);
			AssertEquals("Should be 1 files", 1, ParentMain.Files.Count);
			AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilename(elementAdded, newDocType, contents, "test");

			elementAdded = (StorageFile)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.pdf",
				DocumentType = "ABC",
				FileAction = FileAction.Overwrite,
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a file", elementAdded);
			AssertEquals("Should still be 4 children on the parent - should have overwritten the previous file added with the same name", 4, ParentMain.eDocs.Count);
			AssertEquals("Should be 3 documents on the parent", 3, ParentMain.Documents.Count);
			AssertEquals("Should be 1 files", 1, ParentMain.Files.Count);
			AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilename(elementAdded, newDocType, contents, "test");

			elementAdded = (StorageFile)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.pdf",
				DocumentType = "ABC",
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a file", elementAdded);
			AssertEquals("Should be 5 children on the parent - leave existing filename", 5, ParentMain.eDocs.Count);
			AssertEquals("Should be 3 documents on the parent", 3, ParentMain.Documents.Count);
			AssertEquals("Should be 2 files", 2, ParentMain.Files.Count);
			AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilename(elementAdded, newDocType, contents, "test[2]");
		}

		void AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilename(StorageFile elementAdded, RefDocType newDocType, byte[] contents, string expectedFileName)
		{
			AssertEquals("should have correct doctype", newDocType.RT_DocType, elementAdded.SC_DocType);
			AssertEquals("should have correct description", newDocType.RT_Desc, elementAdded.SC_Desc);
			AssertEquals("should have correct data type", "PDF", elementAdded.SC_DataType);
			AssertEquals("should have correct user", GlbStaff.CurrentUser.GS_Code, elementAdded.SC_SystemCreateUser);
			AssertEquals("should have the right contents", contents, elementAdded.SC_ImageData);
			AssertEquals("Should have the correct filename that was passed in", expectedFileName, elementAdded.SC_FileName);
		}

		public void TestAddFileOrDocumentWithDuplicateFilenameForTifFile()
		{
			byte[] contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };

			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_Desc = "ABC Description";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			StorageDocs elementAdded = (StorageDocs)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.tif",
				DocumentType = "ABC",
				FileAction = FileAction.Overwrite,
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a document", elementAdded);
			AssertEquals("Should be 4 children on the parent now", 4, ParentMain.eDocs.Count);
			AssertEquals("Shoudl be 4 documents on the parent", 4, ParentMain.Documents.Count);
			AssertEquals("Should be 0 file", 0, ParentMain.Files.Count);
			AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilenameForTifFile(elementAdded, newDocType, contents, "test");

			elementAdded = (StorageDocs)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.tif",
				DocumentType = "ABC",
				FileAction = FileAction.Overwrite,
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a document", elementAdded);
			AssertEquals("Should be 5 children on the parent now", 5, ParentMain.eDocs.Count);
			AssertEquals("Should be 5 documents on the parent", 5, ParentMain.Documents.Count);
			AssertEquals("Should be 0 file", 0, ParentMain.Files.Count);
			AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilenameForTifFile(elementAdded, newDocType, contents, "test[2]");

			elementAdded = (StorageDocs)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.tif",
				DocumentType = "ABC",
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a document", elementAdded);
			AssertEquals("Should be 6 children on the parent", 6, ParentMain.eDocs.Count);
			AssertEquals("Should be 6 documents on the parent", 6, ParentMain.Documents.Count);
			AssertEquals("Should be 0 file", 0, ParentMain.Files.Count);
			AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilenameForTifFile(elementAdded, newDocType, contents, "test[3]");
		}

		void AssertProperties_TestAddFilenameOrDocumentWithDuplicateFilenameForTifFile(StorageDocs elementAdded, RefDocType newDocType, byte[] contents, string expectedFileName)
		{
			AssertEquals("should have correct doctype", newDocType.RT_DocType, elementAdded.SC_DocType);
			AssertEquals("should have correct description", newDocType.RT_Desc, elementAdded.SC_Desc);
			AssertEquals("should have correct data type", "TIF", elementAdded.SC_DataType);
			AssertEquals("should have correct user", GlbStaff.CurrentUser.GS_Code, elementAdded.SC_SystemCreateUser);
			AssertEquals("should have the right contents", contents, elementAdded.SC_ImageData);
			AssertEquals("Should have the correct filename that was passed in", expectedFileName, elementAdded.SC_FileName);
		}

		public void TestAddFileOrDocument_ParentLinks()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_Desc = "ABC Description";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			var elementAdded1 = (StorageFile)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.pdf",
				DocumentType = "ABC",
				FileAction = FileAction.Overwrite,
			});
			MasterFactory.Save();

			AssertNotNull("the element added was a file", elementAdded1);
			AssertEquals("Should be 4 children on the parent now", 4, ParentMain.eDocs.Count);
			AssertEquals("Shoudl be 3 documents on the parent", 3, ParentMain.Documents.Count);
			AssertEquals("Should be 1 files", 1, ParentMain.Files.Count);

			AssertEquals("should have correct doctype", newDocType.RT_DocType, elementAdded1.SC_DocType);
			AssertEquals("should have correct description", newDocType.RT_Desc, elementAdded1.SC_Desc);
			AssertEquals("should have correct data type", "PDF", elementAdded1.SC_DataType);
			AssertEquals("should have correct user", GlbStaff.CurrentUser.GS_Code, elementAdded1.SC_SystemCreateUser);
			AssertEquals("should have the right contents", contents, elementAdded1.SC_ImageData);
			AssertEquals("Should have the correct filename that was passed in", "test", elementAdded1.SC_FileName);
			AssertEquals("Should have the correct visible company", ZGuid.Empty, elementAdded1.SC_GC_Company);
			AssertEquals("Should have the correct visible branch", ZGuid.Empty, elementAdded1.SC_GB_Branch);
			AssertEquals("Should have the correct visible department", ZGuid.Empty, elementAdded1.SC_GE_Department);

			var elementAdded2 = (StorageFile)ParentMain.AddFileOrDocument(contents, new AddFileOrDocumentDto
			{
				FileName = "test.pdf",
				DocumentType = "ABC",
				FileAction = FileAction.Overwrite,
				VisibleCompanyPK = GlbCompany.CurrentCompany.PK,
				VisibleBranchPK = GlbBranch.CurrentBranch.PK,
				VisibleDepartmentPK = GlbDepartment.CurrentDepartment.PK,
			});
			MasterFactory.Save();

			AssertEquals("Should have the correct visible company", GlbCompany.CurrentCompany.PK, elementAdded2.SC_GC_Company);
			AssertEquals("Should have the correct visible branch", GlbBranch.CurrentBranch.PK, elementAdded2.SC_GB_Branch);
			AssertEquals("Should have the correct visible department", GlbDepartment.CurrentDepartment.PK, elementAdded2.SC_GE_Department);
		}

		public void TestAddFileOrDocument_AddingDelayedUntilFileConfigured()
		{
			ParentMain.eDocs.CountChanged += (sender, e) =>
			{
				StorageDocsTestHelper.AssertFileOrDocumentConfigured((BusinessObjectCollection)sender, Core.Constants.FileFormats.PDF);
			};
			var samplePdfBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			ParentMain.AddFileOrDocument(samplePdfBytes, new AddFileOrDocumentDto
			{
				FileName = "Sample.PDF",
				DocumentType = "PDF",
			});
		}

		public void TestAddFileOrDocument_AddingDelayedUntilDocumentConfigured()
		{
			ParentMain.eDocs.CountChanged += (sender, e) =>
			{
				StorageDocsTestHelper.AssertFileOrDocumentConfigured((BusinessObjectCollection)sender, Core.Constants.FileFormats.JPG);
			};
			var compressedJpgBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.compressed.jpg");
			ParentMain.AddFileOrDocument(compressedJpgBytes, new AddFileOrDocumentDto
			{
				FileName = "compressed.jpg",
			});
		}

		public void TestAddFileOrDocument_EmptyImageContentBytes()
		{
			var storageMain = Factory.New<StorageMain>();
			var contents = Array.Empty<byte>();
			AssertExceptionThrown(
				typeof(EmptyContentEDocsException),
				"The eDoc content cannot be empty.",
				() => storageMain.AddFileOrDocument(contents, new AddFileOrDocumentDto { FileName = "test file", DocumentType = "MSC" }));
		}

		public void TestAddFileOrDocument_EmptyImageContentStream()
		{
			var contents = (SubStreamableStream)(new MemoryStream());
			var storageMain = Factory.New<StorageMain>();
			AssertExceptionThrown(
				typeof(EmptyContentEDocsException),
				"The eDoc content cannot be empty.",
				() => storageMain.AddFileOrDocument(contents, new AddFileOrDocumentDto { FileName = "test file", DocumentType = "MSC" }));
		}

		#endregion

		#region Properties

		#region DocumentOwner

		public void TestDocumentOwner()
		{
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			ParentMain.SM_ParentFK = Org.PK;
			AssertEquals("The correct business object should have been loaded", Org, ParentMain.DocumentOwner);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			ParentMain.SM_ParentFK = shipment.PK;
			AssertEquals("The shipment should have been loaded", shipment.PK, ParentMain.DocumentOwner.PK);

			AssertEquals("Because the DocumentOwner is an IDocsAndCartageParent, the Shipment.DocsAndCartage.RequiredDocuments must be an editable child object of StorageMain",
				true, ParentMain.IsRegisteredEditableChildObject(((IDocsAndCartageParent)ParentMain.DocumentOwner).RequiredDocumentsProvider.RequiredDocuments));

			ParentMain.DocumentOwner["JS_HouseBill"] = "_12345Test";
			AssertEquals("StorageMain must have changes after DocumentOwner is changed.", true, ParentMain.HasChanges);

			BusinessObject order = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IOrder)));
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Order;
			ParentMain.SM_ParentFK = order.PK;
			AssertEquals("The order should have been loaded", order.PK, ParentMain.DocumentOwner.PK);

			AssertEquals("Because the DocumentOwner is an IHaveRequiredDocuments, the Order.RequiredDocuments must be an editable child object of StorageMain",
				true, ParentMain.IsRegisteredEditableChildObject(((IHaveRequiredDocuments)ParentMain.DocumentOwner).RequiredDocuments));
		}

		public void TestDocumentOwnerCode()
		{
			AssertEquals("Code pulled from business object should be the same", Org.OH_Code, ParentMain.DocumentOwnerCode);

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			OrgHeader org2 = MasterFactory.LoadTop1<OrgHeader>(filter);

			ParentMain.SM_ParentFK = org2.PK;
			AssertEquals("Code pulled from business object should be the same", org2.OH_Code, ParentMain.DocumentOwnerCode);

			org2.Delete();
			AssertEquals("Code is empty if business object is deleted", ZString.Empty, ParentMain.DocumentOwnerCode);

			ParentMain.SM_ParentFK = ZGuid.Empty;
			AssertEquals("Code is empty", ZString.Empty, ParentMain.DocumentOwnerCode);
		}

		public void TestDocumentOwnerDescription()
		{
			ParentMain.SM_ParentFK = Guid.Empty;
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			AssertEquals("DocumentOwnerDescription just shipment if no ParentFK supplied", "Shipment", ParentMain.DocumentOwnerDescription);

			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			ParentMain.SM_ParentFK = Org.PK;
			AssertEquals("DocumentOwnerDescription has been changed, includes Code if ParentFK is supplied", "Organization ABIGAS", ParentMain.DocumentOwnerDescription);

			ParentMain.IsTopLevelParent = true;
			AssertEquals("DocumentOwnerDescription returns 'This <type>' if it is the top level parent", "This Organization", ParentMain.DocumentOwnerDescription);
		}

		#endregion

		#region SM_Type

		public void TestSM_Type()
		{
			ParentMain.SM_Type = "ORD";
			AssertEquals("SM_Type", "ORD", ParentMain.SM_Type);

			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			AssertEquals("SM_Type", Core.Constants.DocManagerCodes.Shipment, ParentMain.SM_Type);
		}

		#endregion

		#region SM_DB

		public void TestSettingSM_DBClearsDocumentsPropertyCache()
		{
			ParentMain.SM_DB = 0;
			StorageDocs doc = ParentMain.Documents.AddNew();
			long factoryInstanceId = doc.Factory._Instance;
			ParentMain.SM_DB = new DocManagerDBHelperTestClass().LastWritableDatabaseWithFreeSpace();
			doc = ParentMain.Documents.AddNew();
			Assert("Object created should be from another factory instance", factoryInstanceId != doc.Factory._Instance);
		}

		public void TestSM_DBInfo_ConcurrencyPolicy()
		{
			AssertEquals("We should prevent to merge SM_DB. See WI00232476.", ConcurrencyPolicy.Strict, ParentMain.SM_DBInfo.ConcurrencyPolicy);
		}

		#endregion

		#region SM_LastActivity

		[TestDate(2024, 01, 16, 9, 00, 00)]
		public void TestSM_LastActivity()
		{
			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			MasterFactory.Save();
			AssertEquals(ZDateTime.Empty, storageMain.SM_LastActivity);

			_ = storageMain.Documents.AddNew();
			MasterFactory.Save();
			TestDateAttribute.Date = new DateTime(2024, 01, 16, 9, 01, 00);
			var storageDoc1 = storageMain.Documents.AddNew();
			MasterFactory.Save();
			AssertEquals(TestDateAttribute.Date, storageMain.SM_LastActivity);

			TestDateAttribute.Date = new DateTime(2024, 01, 16, 9, 02, 00);
			storageDoc1.SC_Desc = "updated";
			MasterFactory.Save();
			AssertEquals(TestDateAttribute.Date, storageMain.SM_LastActivity);
		}

		#endregion

		#region ViewIncludesDeletedDocuments

		public void TestViewIncludesDeletedDocuments()
		{
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageFile file = ParentMain.Files.AddNew();

			AssertEquals("eDocsView Collection should return all documents", ParentMain.eDocsView.Count, ParentMain.eDocs.Count);
			AssertEquals("eDocsView Collection should not include deleted documents by default", false, ParentMain.ViewIncludesDeletedDocuments);
			Document1.SC_IsDeleted = true;
			Document2.SC_IsDeleted = true;
			file.SC_IsDeleted = true;

			AssertEquals("eDocs Collection should be excluding deleted documents", ParentMain.eDocs.Count - 3, ParentMain.eDocsView.Count);
			AssertEquals("First document should not be in view", false, ParentMain.eDocsView.Contains(Document1.PK));
			AssertEquals("Second document should not be in view", false, ParentMain.eDocsView.Contains(Document2.PK));
			AssertEquals("File should not be in the view", false, ParentMain.eDocsView.Contains(file.PK));

			ParentMain.ViewIncludesDeletedDocuments = true;

			AssertEquals("Documents Collection should not be excluding deleted documents", ParentMain.eDocsView.Count, ParentMain.eDocs.Count);
			AssertEquals("First document should exist in view", true, ParentMain.eDocsView.Contains(Document1.PK));
			AssertEquals("Second document should exist in view", true, ParentMain.eDocsView.Contains(Document2.PK));
			AssertEquals("File should exist in the view", true, ParentMain.eDocsView.Contains(file.PK));
		}

		#endregion

		#region ViewExcludesUnpublishedDocuments

		public void TestViewExcludesUnpublishedDocuments()
		{
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageFile file = ParentMain.Files.AddNew();

			AssertEquals("eDocsView Collection should return all documents", ParentMain.eDocs.Count, ParentMain.eDocsView.Count);
			AssertEquals("Documents Collection should not exclude unpublished documents by default", false, ParentMain.ViewExcludesUnpublishedDocuments);

			Document1.SC_IsPublished = false;
			Document2.SC_IsPublished = false;
			Document3.SC_IsPublished = true;
			file.SC_IsPublished = false;

			AssertEquals("eDocs should not be excluding unpublished documents", 4, ParentMain.eDocsView.Count);
			AssertEquals("First document should be in document view", true, ParentMain.eDocsView.Contains(Document1.PK));
			AssertEquals("Second document should be in document view", true, ParentMain.eDocsView.Contains(Document2.PK));
			AssertEquals("File should be in the document view", true, ParentMain.eDocsView.Contains(file.PK));

			ParentMain.ViewExcludesUnpublishedDocuments = true;

			AssertEquals("eDcs should exclude unpublished docs", 1, ParentMain.eDocsView.Count);
			AssertEquals("First document should not exist in document view", false, ParentMain.eDocsView.Contains(Document1.PK));
			AssertEquals("Second document should not exist in document view", false, ParentMain.eDocsView.Contains(Document2.PK));
			AssertEquals("File should not exist in the document view", false, ParentMain.eDocsView.Contains(file.PK));
		}

		#endregion

		public void TestQuickViewCard()
		{
			ParentMain.SM_ParentFK = Guid.Empty;
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			AssertEquals("QuickViewCard from DocumentOwnerDescription", "Shipment", ParentMain.QuickViewCard);
		}

		#endregion

		#region MasterFactory

		public void TestMasterFactory()
		{
			AssertEquals("MasterFactory returned is the same instance as was passed in", MasterFactory, ParentMain.MasterFactory);
		}

		#endregion

		#region TestConcurrencyFailureHandling

		[ExpectNoExceptions]
		public void TestConcurrencyFailureHandling()
		{
			ZDateTime now = ZDateTime.Now;
			OrgHeader org1 = new BusinessObjectFactory().New<OrgHeader>();
			org1.OH_Code = "~1111~";
			org1.MainAddress.OA_Code = "12345";
			OrgHeader org2 = org1.Factory.New<OrgHeader>();
			org2.OH_Code = "~2222~";
			org2.MainAddress.OA_Code = "12345";
			org1.Factory.Save();

			MasterFactory.Save();

			ParentMain.SM_ParentFK = org1.PK;

			using (new TemporaryUserContext() { StaffLoginName = User.SupportUserName, BranchPK = Env.CurrentBranch.PK, DepartmentPK = Env.CurrentDepartment.PK }.Set())
			{
				DocumentFactory newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				StorageMain parentInNewFactory = newFactory.Load<StorageMain>(ParentMain.PK);
				parentInNewFactory.SM_ParentFK = org2.PK;

				using (GetFactoryIsolater(newFactory))
				using (GetFactoryIsolater(MasterFactory))
				{
					newFactory.Save();
				}
			}

			try
			{
				MasterFactory.Save();
				Fail("should have thrown a ZSaveConcurrencyException");
			}
			catch (ZSaveConcurrencyException ex)
			{
				// should not throw exception
				ZExceptionReporting.HandleSaveException(ex);
			}

			MasterFactory.Save();
		}

		public void TestSaveEDocSuccessfullyWithConcurrencyError()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var storageMain1 = MasterFactory.NewWithValidTestData<StorageMain>();
			storageMain1.SM_ParentFK = org1.PK;
			storageMain1.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			var newFactory = new BusinessObjectFactory();
			var newMasterFactory = new DocumentFactoryProvider().GetFactory(newFactory);
			var org2 = newFactory.Load<OrgHeader>(org1.PK);
			var storageMain2 = newMasterFactory.NewWithValidTestData<StorageMain>();
			storageMain2.SM_ParentFK = org2.PK;
			storageMain2.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			using (GetFactoryIsolater(MasterFactory))
			{
				MasterFactory.Save();
			}

			storageMain2.AddFileOrDocument(Encoding.ASCII.GetBytes("Test"), new AddFileOrDocumentDto { FileName = "test.txt", DocumentType = "MSC" });
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => newMasterFactory.Save(), null, false);

			newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			storageMain1 = newFactory.Load<StorageMain>(storageMain1.PK);
			var eDoc = storageMain1.eDocs.OfType<StorageDocsBase>().FirstOrDefault(x => x.SC_FileName == "test");
			AssertNotNull(eDoc);
		}

		public void TestParentFKUniqueIndexViolation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			StorageMain storageMain1 = MasterFactory.New<StorageMain>();
			storageMain1.SM_ParentFK = org.PK;
			storageMain1.SM_DB = DBNumber;
			storageMain1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			StorageDocs doc1 = storageMain1.Documents.AddNew();
			doc1.SC_ImageData = SmallTifBytes;
			doc1.SC_Date = ZDateTime.Now;
			doc1.SC_SM = storageMain1.PK;
			doc1.SC_Desc = "1";

			ZQuery query = new ZQuery();
			query.AddToFilter(StorageMainSchema.SM_ParentFK, storageMain1.SM_ParentFK);
			query.AddToFilter(StorageMainSchema.PK, SQLComparisonOperator.NotEqual, storageMain1.PK);
			AssertNull(MasterFactory.LoadTop1<StorageMain>(query)); // Store query to query cache

			DocumentFactory anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			anotherFactory.RefreshEnabled = false;
			StorageMain storageMain2 = anotherFactory.New<StorageMain>();
			storageMain2.SM_ParentFK = org.PK;
			storageMain2.SM_DB = DBNumber;
			storageMain2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			StorageDocs doc2 = storageMain2.Documents.AddNew();
			doc2.SC_ImageData = SmallTifBytes;
			doc2.SC_Date = ZDateTime.Now;
			doc2.SC_SM = storageMain2.PK;
			doc2.SC_Desc = "2";

			using (GetFactoryIsolater(anotherFactory))
			using (GetFactoryIsolater(MasterFactory))
			{
				anotherFactory.Save();
			}
			AssertNull("Should not be loaded without row reload", MasterFactory.LoadTop1<StorageMain>(query));

			try
			{
				MasterFactory.Save();
				Fail("An exception should be thrown.");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				MasterFactory.Save();
				anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				StorageMain storageFinal = anotherFactory.Load<StorageMain>(storageMain2.eDocs[0].SC_SM);

				Assert("storageMain1 should not be saved", !storageMain1.IsInDatabase);
				AssertEquals("storageMain2 should have 2 eDocs", 2, storageFinal.eDocs.Count);
			}
		}

		public void TestParentFKUniqueIndexViolationDoesNotThrowNullReferenceExceptionWhenStorageMainGetsDeleted()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			StorageMain storageMain1 = MasterFactory.New<StorageMain>();
			storageMain1.SM_ParentFK = org.PK;
			storageMain1.SM_DB = DBNumber;
			storageMain1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			StorageDocs doc1 = storageMain1.Documents.AddNew();
			doc1.SC_ImageData = SmallTifBytes;
			doc1.SC_Date = ZDateTime.Now;
			doc1.SC_SM = storageMain1.PK;
			doc1.SC_Desc = "1";

			ZQuery query = new ZQuery();
			query.AddToFilter(StorageMainSchema.SM_ParentFK, storageMain1.SM_ParentFK);
			query.AddToFilter(StorageMainSchema.PK, SQLComparisonOperator.NotEqual, storageMain1.PK);
			AssertNull(MasterFactory.LoadTop1<StorageMain>(query)); // Store query to query cache

			DocumentFactory anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			anotherFactory.RefreshEnabled = false;
			StorageMain storageMain2 = anotherFactory.New<StorageMain>();
			storageMain2.SM_ParentFK = org.PK;
			storageMain2.SM_DB = DBNumber;
			storageMain2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			StorageDocs doc2 = storageMain2.Documents.AddNew();
			doc2.SC_ImageData = SmallTifBytes;
			doc2.SC_Date = ZDateTime.Now;
			doc2.SC_SM = storageMain2.PK;
			doc2.SC_Desc = "2";

			using (GetFactoryIsolater(anotherFactory))
			using (GetFactoryIsolater(MasterFactory))
			{
				anotherFactory.Save();
			}
			AssertNull("Should not be loaded without row reload", MasterFactory.LoadTop1<StorageMain>(query));

			try
			{
				MasterFactory.Save();
				Fail("An exception should be thrown.");
			}
			catch (ZSaveException ex)
			{
				// This is to test issue attached to WI00016034
				// looks like sometimes it gets deleted and the exception should not be thrown.
				// Though i did not manage to reproduce the situation in Enterrise when it happens
				storageMain2.Delete();
				anotherFactory.Save();

				ZExceptionReporting.HandleSaveException(ex);
				MasterFactory.Save();
				anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				StorageMain storageFinal = anotherFactory.Load<StorageMain>(storageMain1.eDocs[0].SC_SM);

				Assert("storageMain1 should be saved", storageMain1.IsInDatabase);
				AssertEquals("storageMain2 should have 1 eDocs", 1, storageFinal.eDocs.Count);

				Assert("storageMain2 should NOT be saved", !storageMain2.IsInDatabase);
			}
		}

		#endregion

		#region IStorageMain Members

		public void TestIStorageMain_Documents()
		{
			Assert("Documents", ParentMain.Documents == ((IStorageMain)ParentMain).Documents);
		}

		public void TestIStorageMain_Files()
		{
			Assert("Files", ParentMain.Files == ((IStorageMain)ParentMain).Files);
		}

		public void TestIStorageMain_eDocs()
		{
			Assert("eDocs", ParentMain.eDocs == ((IStorageMain)ParentMain).AllEDocs);
		}

		public void TestIStorageMain_EDocsView()
		{
			Assert("eDocsView", ParentMain.eDocsView == ((IStorageMain)ParentMain).EDocsView);
		}

		public void TestIStorageMain_PhysicalLocation()
		{
			Assert("PhysicalLocation", ParentMain.PhysicalLocation == ((IStorageMain)ParentMain).PhysicalLocation);
		}

		#endregion

		#region Company/Branch/Department Specific

		public void TestShowCompanyBranchDepartmentDocumentsReadOnly()
		{
			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = false;
			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = false;
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = false;
			Assert(ParentMain.ShowDocumentsForAllBranches_ReadOnly);
			Assert(ParentMain.ShowDocumentsForAllCompanies_ReadOnly);
			Assert(ParentMain.ShowDocumentsForAllDepartments_ReadOnly);

			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = true;
			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = true;
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = true;
			Assert(!ParentMain.ShowDocumentsForAllBranches_ReadOnly);
			Assert(!ParentMain.ShowDocumentsForAllCompanies_ReadOnly);
			Assert(!ParentMain.ShowDocumentsForAllDepartments_ReadOnly);
		}

		#endregion

		#region Physical Location

		public void TestPhysicalLocation()
		{
			ParentMain.SM_PhysicalLocation = "123";
			AssertEquals("PhycalLocation", "123", ParentMain.PhysicalLocation);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestCreateNullStorageMain()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertEquals(typeof(StorageMain), factory.GetNull<StorageMain>().GetType());
		}

		#region TestAddAllRequiredDocumentsGeneratesAID

		public void TestAddAllRequiredDocumentsGeneratesAID()
		{
			var docTypeA = Factory.NewWithValidTestData<RefDocType>();
			docTypeA.RT_DocType = "AAA";
			var docTypeB = Factory.NewWithValidTestData<RefDocType>();
			docTypeB.RT_DocType = "BBB";

			JobRequiredDocument required1 = Org.RequiredDocuments.AddNew();
			required1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			required1.EQ_DocType = "AAA";

			JobRequiredDocument required2 = Org.RequiredDocuments.AddNew();
			required2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			required2.EQ_DocType = "BBB";

			ParentMain.Factory.Save();
			Factory.Save();

			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "ccc.ccc",
				DocumentType = "CCC",
			}).SC_Desc = "Cccccccccccc";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertNull("No reaction on non-required document", Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));

			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "aaa.aaa",
				DocumentType = "AAA",
			}).SC_Desc = "Aaaaaaaaaaaa";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertNull("No reaction as not all required documents are yet added", Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));

			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "bbb.bbb",
				DocumentType = "BBB",
			}).SC_Desc = "Bbbbbbbbbbbb";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertNotNull("All documents are added - AID should be logged", Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));

			ZGuid lastPk = Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived).PK;
			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "bbb.bbb",
				DocumentType = "BBB",
			}).SC_Desc = "Bbbbbbbbbbbb";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertEquals("No new AID should be logged", lastPk, Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived).PK);
		}

		public void TestAddAllRequiredDocumentsWillNotGeneratesAIDWhenArchiving()
		{
			var docTypeA = Factory.NewWithValidTestData<RefDocType>();
			docTypeA.RT_DocType = "AAA";
			var docTypeB = Factory.NewWithValidTestData<RefDocType>();
			docTypeB.RT_DocType = "BBB";

			var required1 = Org.RequiredDocuments.AddNew();
			required1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			required1.EQ_DocType = "AAA";

			var required2 = Org.RequiredDocuments.AddNew();
			required2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			required2.EQ_DocType = "BBB";

			ParentMain.Factory.Save();
			Factory.Save();

			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "ccc.ccc",
				DocumentType = "CCC",
				IsArchiving = true,
			}).SC_Desc = "Cccccccccccc";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertNull("No reaction on non-required document", Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));

			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "aaa.aaa",
				DocumentType = "AAA",
				IsArchiving = true,
			}).SC_Desc = "Aaaaaaaaaaaa";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertNull("No reaction as not all required documents are yet added", Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));

			ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
			{
				FileName = "bbb.bbb",
				DocumentType = "BBB",
				IsArchiving = true,
			}).SC_Desc = "Bbbbbbbbbbbb";
			ParentMain.Factory.Save();
			Factory.Save();

			AssertNull("All documents are added - But AID should not be logged, Because we are in Archiving Mode.", Org.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestAddDocumentForDuplicatedNameInSupportedFormat()
		{
			var compressedJpgBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.compressed.jpg");
			var doc1 = ParentMain.AddFileOrDocument(compressedJpgBytes, new AddFileOrDocumentDto
			{
				FileName = "test.jpg",
				DocumentType = "CCC",
				IsArchiving = true,
			});
			AssertEquals("", "test.jpg", doc1.SC_FileNameWithExtension);

			var doc2 = ParentMain.AddFileOrDocument(compressedJpgBytes, new AddFileOrDocumentDto
			{
				FileName = "test.jpg",
				DocumentType = "CCC",
				IsArchiving = true,
			});
			AssertEquals("", "test[2].jpg", doc2.SC_FileNameWithExtension);
		}

		#endregion

		#region TestHasUnreadRelatedDocuments_ForDifferentUsers

		public void TestHasUnreadRelatedDocuments_ForDifferentUsers()
		{
			DocTypeWithForceUserToRead.Factory.Save();

			string user1Code = "US1";
			string user2Code = "US2";
			string user3Code = "US3";

			var parent = Factory.NewWithValidTestData(typeof(OrgHeader));
			Factory.Save();

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;

			string updateRERLogDate = "update dbo.stmalog set SL_PostedTimeUtc = '{0}' where SL_PK = '{1}'";
			//SC_Date in table StorageDocs is smalldatetime, while SL_PostedTimeUtc in table StmALog is datetime. So when comparing them there need at least 1 minute difference
			string startDate = "2013-05-06 06:00:00";
			string date1 = "2013-05-06 06:01:00";
			string date2 = "2013-05-06 06:02:00";
			string date3 = "2013-05-06 06:03:00";
			string date4 = "2013-05-06 06:04:00";
			string date5 = "2013-05-06 06:05:00";

			var document1 = CreateNewDocumentWithForceUserToReadDocType(docManagerInfo, DocTypeWithForceUserToRead.RT_DocType, user1Code, new ZDateTime(startDate));

			var logs = parent.GetLogs();

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials(user2Code))
			{
				AssertEquals(true, docManagerInfo.HasUnreadRelatedDocuments);
				document1.NotifyReadByUser();

				ZQuery loadLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, "RER");
				loadLogQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, "US2");
				var rerLogs = logs.Find(loadLogQuery);
				AssertEquals("There should be one RER log for US2", 1, rerLogs.Length);
				Db.Connection.ExecuteNonQuery(string.Format(updateRERLogDate, date1, rerLogs[0].PK));// cant set SL_PostedTimeUtc on BusinessObject

				AssertEquals(false, docManagerInfo.HasUnreadRelatedDocuments);
			}

			var document2 = CreateNewDocumentWithForceUserToReadDocType(docManagerInfo, DocTypeWithForceUserToRead.RT_DocType, user2Code, new ZDateTime(date2));

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials(user1Code))
			{
				document1.ResetIsReadByUserInThisSessionToFalse();
				document2.ResetIsReadByUserInThisSessionToFalse();

				AssertEquals(true, docManagerInfo.HasUnreadRelatedDocuments);
				document2.NotifyReadByUser();

				UpdateRERLogPostTimce(updateRERLogDate, date3, logs, "US1");

				AssertEquals(false, docManagerInfo.HasUnreadRelatedDocuments);
			}

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials(user3Code))
			{
				document1.ResetIsReadByUserInThisSessionToFalse();
				document2.ResetIsReadByUserInThisSessionToFalse();

				AssertEquals(true, docManagerInfo.HasUnreadRelatedDocuments);
				document1.NotifyReadByUser();
				document2.NotifyReadByUser();

				UpdateRERLogPostTimce(updateRERLogDate, date4, logs, "US3");

				AssertEquals(false, docManagerInfo.HasUnreadRelatedDocuments);
			}

			var document3 = CreateNewDocumentWithForceUserToReadDocType(docManagerInfo, DocTypeWithForceUserToRead.RT_DocType, user2Code, new ZDateTime(date5));

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials(user3Code))
			{
				AssertEquals(true, docManagerInfo.HasUnreadRelatedDocuments);
				document3.NotifyReadByUser();
				AssertEquals(false, docManagerInfo.HasUnreadRelatedDocuments);
			}
		}

		void UpdateRERLogPostTimce(string updateRERLogDate, string date, Logs logs, string userCode)
		{
			ZQuery loadLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, "RER");
			loadLogQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, userCode);
			var rerLogs = logs.Find(loadLogQuery);
			AssertEquals("There should be one RER log for" + userCode, 1, rerLogs.Length);
			Db.Connection.ExecuteNonQuery(string.Format(updateRERLogDate, date, rerLogs[0].PK));// cant set SL_PostedTimeUtc on BusinessObject
																								//also update it in factory, so that the unit test doesn't rely on stale data
																								//hacky because you can't reload StmALog or set SL_PostedTimeUtc normally...
			((INeedRow)rerLogs[0]).Row[nameof(StmALogSchema.SL_PostedTimeUtc)] = new ZDateTime(date).ToDateTime();
			rerLogs[0].HasChanges = true;
			rerLogs[0].Factory.Save(); //to refresh other factories
		}

		StorageDocsBase CreateNewDocumentWithForceUserToReadDocType(DocManagerInfo docManagerInfo, ZString docType, string userCode, ZDateTime date)
		{
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials(userCode))
			{
				StorageDocsBase result = (StorageDocsBase)docManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Test.pdf", docType);
				result.SC_Date = date;
				docManagerInfo.Save();
				return result;
			}
		}

		#endregion

		#region AddLogsForNewDocument

		public void TestAddLogsForNewDocument()
		{
			TestLogs(false);
		}

		public void TestAddLogsForNewDocumentWhenArchiving()
		{
			TestLogs(true);
		}

		void TestLogs(bool isArchiving)
		{
			var originalValue = SystemDataRegistry.Instance.DDIDocumentSource.Value;
			try
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				//Setting up SE_OverridenReferenceFormat so test passes if SE_ReferenceFormat is not set up yet.
				var fDDIEvent = ParentMain.Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "DDI"));
				if (fDDIEvent.SE_ReferenceFormat.IsEmpty)
				{
					fDDIEvent.SE_OverriddenReferenceFormat = "<[NAM, if !SRC.IsNoneOrEmpty() then \"Document Imported from <DescriptionFromCode(SRC, \"RDS\")>\" else \"\"].Join(\" \")>";
					fDDIEvent.SE_IsRefernceFormatOverridden = true;
				}
				var fDDAEvent = ParentMain.Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "DDA"));
				if (fDDAEvent.SE_ReferenceFormat.IsEmpty)
				{
					fDDAEvent.SE_OverriddenReferenceFormat = "<[NAM, if !SRC.IsNoneOrEmpty() then \"Document Imported from <DescriptionFromCode(SRC, \"RDS\")>\" else \"\"].Join(\" \")>";
					fDDAEvent.SE_IsRefernceFormatOverridden = true;
				}

				RefDocType docType = MasterFactory.NewWithValidTestData<RefDocType>();
				RefDocSource docSourceA = MasterFactory.NewWithValidTestData<RefDocSource>();
				RefDocSource docSourceB = MasterFactory.NewWithValidTestData<RefDocSource>();
				docSourceA.RDS_Code = "AAA";
				docSourceA.RDS_Desc = "Aaaaa!";
				docSourceB.RDS_Code = "BBB";
				docSourceB.RDS_Desc = "Bbbbb!";

				docType.RT_Desc = "BBB";
				docType.RT_DocType = "BBB";
				docType.RT_ReferenceType = "ALL";
				docType.RT_SE_NKDocumentReceivedEvent = Events.Arrival.Code;
				MasterFactory.Save();

				var docM = ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
				{
					FileName = "misc.msc",
					DocumentType = "MSC",
				});
				docM.SC_Desc = "meh";
				docM.SC_RDS_NKDocSource = "AAA";
				ParentMain.AddLogsForNewDocument(ParentMain.DocumentOwner, docM);

				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var docB = ParentMain.AddFileOrDocument(new byte[3], new AddFileOrDocumentDto
				{
					FileName = "bbb.bbb",
					DocumentType = "BBB",
				});
				docB.SC_Desc = "Bbbbbbbbbbbb";
				docB.SC_RDS_NKDocSource = "BBB";
				ParentMain.AddLogsForNewDocument(ParentMain.DocumentOwner, docB);

				var logsBeforeSave = Org.GetLogs().GetAllLogs().Count;

				ParentMain.IsArchiving = isArchiving;
				ParentMain.Factory.Save();

				var orgInNewFactory = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);

				if (!isArchiving)
				{
					var logA = orgInNewFactory.GetLogs().MostRecentLogByEventTime(Events.DocumentImported, string.Concat("MSC|", docM.PK.ToString(), "|NAM=MSC", "|SRC=AAA"));
					AssertNotNull(logA);
					AssertEquals("MSC Document Imported from Aaaaa!", logA.DisplayEventReference);
					AssertEquals("AAA", logA.Parameters["SRC"]);
					var logB = orgInNewFactory.GetLogs().MostRecentLogByEventTime(Events.DocumentImported, string.Concat("BBB|", docB.PK.ToString()));
					AssertNotNull(logB);
					AssertEquals("BBB|" + docB.PK.ToString(), logB.DisplayEventReference);
					var logC = orgInNewFactory.GetLogs().MostRecentLogByEventTime(Events.Arrival, docB.PK.ToString());
					AssertNotNull(logC);
				}
				else
				{
					AssertEquals("No new logs should be added when archiving", logsBeforeSave, orgInNewFactory.GetLogs().GetAllLogs().Count);
				}
			}
			finally
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		#endregion

		public void TestDeleteAllEDocsWhenDeletingStorageMain()
		{
			var consignor = MasterFactory.NewWithValidTestData<OrgHeader>();
			MasterFactory.Save();

			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = consignor.PK;
			var image = storageMain.Documents.AddNew();
			var file = storageMain.Files.AddNew();
			storageMain.Delete();
			AssertEquals(true, storageMain.IsDeleted);
			AssertEquals(true, image.IsDeleted);
			AssertEquals(true, file.IsDeleted);
		}

		public void TestDeleteAllStorageReferencesWhenDeletingArchivedStorageMain()
		{
			var archivedStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			archivedStorageMain.SM_ParentFK = Guid.NewGuid();
			archivedStorageMain.SM_Archived = DateTime.Today.AddYears(-7);
			archivedStorageMain.SM_DB = DBNumber;
			var image = archivedStorageMain.Documents.AddNew();
			var file = archivedStorageMain.Files.AddNew();

			var storageReference1 = MasterFactory.New<StorageReference>();
			storageReference1.SR_SM = archivedStorageMain.PK;

			var storageReference2 = MasterFactory.New<StorageReference>();
			storageReference2.SR_SM = archivedStorageMain.PK;

			MasterFactory.Save();

			CombineAssertions("Precondition: StorageReferences should exist.", () =>
			{
				Assert(!storageReference1.IsDeleted);
				Assert(!storageReference2.IsDeleted);
			});

			archivedStorageMain.Delete();

			CombineAssertions("All StorageReferences, StorageMains, and StorageDocs should have been deleted.", () =>
			{
				Assert(archivedStorageMain.IsDeleted);
				Assert(image.IsDeleted);
				Assert(file.IsDeleted);
				Assert(storageReference1.IsDeleted);
				Assert(storageReference2.IsDeleted);
			});
		}

		public void TestDeleteAllStorageReferencesWhenPurging()
		{
			var archivedStorageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			archivedStorageMain.SM_ParentFK = Guid.NewGuid();
			archivedStorageMain.SM_Archived = ZDateTime.Empty;
			archivedStorageMain.SM_DB = DBNumber;
			var image = archivedStorageMain.Documents.AddNew();
			var file = archivedStorageMain.Files.AddNew();

			var storageReference = MasterFactory.New<StorageReference>();
			storageReference.SR_SM = archivedStorageMain.PK;

			MasterFactory.Save();

			CombineAssertions("Precondition: StorageReference should exist.", () =>
			{
				Assert(!storageReference.IsDeleted);
			});

			archivedStorageMain.Delete();

			CombineAssertions("All StorageReferences, StorageMains, and StorageDocs should have been deleted.", () =>
			{
				Assert(archivedStorageMain.IsDeleted);
				Assert(image.IsDeleted);
				Assert(file.IsDeleted);
				Assert(storageReference.IsDeleted);
			});
		}

		public void TestRequireReload()
		{
			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			var image = storageMain.Documents.AddNew();
			var file = storageMain.Files.AddNew();

			image.SaveToTempFile();
			file.SaveToTempFile();
			AssertNotNullOrEmpty(image.TempFileName);
			AssertNotNullOrEmpty(file.TempFileName);

			storageMain.RequireReload();
			AssertNullOrEmpty(image.TempFileName);
			AssertNullOrEmpty(file.TempFileName);
		}

		public void TestShowPreviewIsLazilyInstantiated()
		{
			var storageMain = MasterFactory.New<StorageMain>();
			var showPreviewField = storageMain.GetType().GetField("fShowPreview", BindingFlags.NonPublic | BindingFlags.Instance);

			AssertNull("fShowPreview Field should initially be null", showPreviewField.GetValue(storageMain));

			_ = storageMain.ShowPreview;

			AssertNotNull("fShowPreview is no longer null", showPreviewField.GetValue(storageMain));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return MasterFactory.New(typeof(StorageMain));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return MasterFactory.NewWithValidTestData(typeof(StorageMain));
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		StorageMain ParentMain;
		StorageDocs Document1;
		StorageDocs Document2;
		StorageDocs Document3;

		DocumentFactory MasterFactory;
		NumberedBusinessObjectFactory ChildFactory;
		OrgHeader Org;
		int DBNumber;
		string DocTypeForRequiredDocument;

		protected override void SetUp()
		{
			base.SetUp();

			DBNumber = 1;
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			ChildFactory = MasterFactory.GetFactory(DBNumber);

			ParentMain = MasterFactory.NewWithValidTestData<StorageMain>();

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "ABIGAS");
			Org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			ParentMain.SM_DB = DBNumber;
			ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			ParentMain.SM_ParentFK = Org.PK;

			Document1 = ParentMain.Documents.AddNew();
			Document1.SC_ImageData = SmallTifBytes;
			Document1.SC_Date = ZDateTime.UtcNow;
			Document1.SC_SM = ParentMain.PK;
			Document2 = ParentMain.Documents.AddNew();
			Document2.SC_ImageData = SmallTifBytes;
			Document2.SC_Date = ZDateTime.UtcNow;
			Document2.SC_SM = ParentMain.PK;
			Document3 = ParentMain.Documents.AddNew();
			Document3.SC_ImageData = SmallTifBytes;
			Document3.SC_Date = ZDateTime.UtcNow;
			Document3.SC_SM = ParentMain.PK;
			MasterFactory.Save();

			DocTypeForRequiredDocument = Factory.New<JobRequiredDocument>().Lookups.DocType_List.ToList<ICodeDescription>().FirstOrDefault(a => a.Code != Core.Constants.RefDocTypes.MiscellaneousDocument && a.Code != Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument && a.Code != Core.Constants.RefDocTypes.InternallyCreatedPublicDocument).Code;
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DocManagerDBHelperTestClass dbHelper = new DocManagerDBHelperTestClass();
			if (!dbHelper.DatabaseExists(1))
			{
				dbHelper.CreateDatabase(1);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		#endregion
	}

	class StorageMainInAnotherDbConnectionForTest : StorageMain
	{
		public StorageMainInAnotherDbConnectionForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override DbConnection CurrentDbConnection
		{
			get
			{
				return con ?? base.CurrentDbConnection;
			}
		}
		DbConnection con;

		public void SetConnection(DbConnection connection)
		{
			con = connection;
		}
	}

	class TransactionCoordinatorInMultipleParticipant : TransactionCoordinator
	{
		public TransactionCoordinatorInMultipleParticipant(ITransactionParticipant[] participants) : base(participants)
		{ }

		protected override bool AllowMultipleParticipantsInTransaction
		{
			get
			{
				return true;
			}
		}
	}

	public class StorageMainNonTransactionalTest : TestCase
	{
		[UseSnapshotProtection]
		[TestDate(2024, 02, 21, 9, 00, 00)]
		public void TestSM_LastActivityInMultipleDB()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var db2Name = masterFactory.GetDatabaseName(2);
			using var conn = Db.NewAdminConnection();
			try
			{
				new DocManagerDBHelper().GetLastWritableDatabaseName();
				AdoTestUtils.CreateDbIfNotExists(conn, db2Name);

				var storageMain = masterFactory.NewWithValidTestData<StorageMain>();
				storageMain.SM_DB = 2;

				var storageDoc1 = storageMain.Documents.AddNew();
				masterFactory.Save();

				// Make SD002 read-only
				conn.AlterDbWriteableStateForDocManager(db2Name, writeable: false);

				TestDateAttribute.Date = new DateTime(2024, 02, 21, 8, 59, 00);
				storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
				{
					FileName = "NEW",
					DocumentType = "NEW",
				});
				masterFactory.Save();

				AssertEquals("Odyssey_SD001 should be last writable database.", 1, new DocManagerDBHelper().LastWritableDatabaseWithFreeSpace());
				AssertEquals("SM_DB should be changed to 1.", 1, storageMain.SM_DB);
				AssertEquals("All eDocs should be copied to Odyssey_SD001.", 2, storageMain.eDocs.Count);
				AssertEquals("SM_LastActivity should be filtered from all eDocs.", new DateTime(2024, 02, 21, 9, 00, 00), storageMain.SM_LastActivity);
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(conn, db2Name);
			}
		}

		[UseSnapshotProtection]
		public void TestEDocsWithReadOnlyDatabase()
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			string db1Name = masterFactory.GetDatabaseName(1);
			string db2Name = masterFactory.GetDatabaseName(2);
			Guid storageMainPk = Guid.NewGuid();

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				new DocManagerDBHelper().GetLastWritableDatabaseName(); // this line can generate SD001 automatically if there's no db1Name existing.
				AdoTestUtils.DropDbIfExists(auxConnection, db2Name);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(auxConnection, db2Name);
					CreateTestStorageDocs(auxConnection, db2Name, storageMainPk);

					string insertStorageMainSql = string.Format(@"
						INSERT dbo.StorageMain (SM_PK, SM_DB) VALUES ('{0}', 2);",
						storageMainPk.ToString());
					auxConnection.ExecuteNonQuery(insertStorageMainSql);
					StorageMain parentMain = masterFactory.Load<StorageMain>(storageMainPk);

					AssertEquals("(a) eDocs collection count", 2, parentMain.eDocs.Count);
					AssertEquals("(a) Storage Main DB", 2, parentMain.SM_DB);

					parentMain.SM_DB = 397; // Nonexistent DB
					AssertEquals("(b) eDocs collection count", 0, parentMain.eDocs.Count);
					AssertEquals("(b) Storage Main DB", 397, parentMain.SM_DB);

					// Make SD002 read-only
					auxConnection.AlterDbWriteableStateForDocManager(db2Name, false);

					parentMain.SM_DB = 2; // Read-only DB
					AssertEquals("(c) eDocs collection count", 2, parentMain.eDocs.Count);
					AssertEquals("(c) Storage Main DB", 2, parentMain.SM_DB);

					// Save with no eDocs changes
					masterFactory.Save();
					AssertEquals("(d) Storage Main DB", 2, parentMain.SM_DB);
					AssertEquals("(d) eDocs collection count", 2, parentMain.eDocs.Count);
					Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

					// Add new eDoc
					parentMain.eDocs[1].SC_FileName = "CHG";
					parentMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
					{
						FileName = "NEW",
						DocumentType = "NEW",
					});
					AssertEquals("(e) eDocs collection count", 3, parentMain.eDocs.Count);

					// Save after changes
					masterFactory.Save();
					AssertEquals("(f) Storage Main DB", 1, parentMain.SM_DB);
					AssertEquals("(f) eDocs collection count", 3, parentMain.eDocs.Count);
					Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

					ZQuery query = new ZQuery(StmALogSchema.SL_Parent, parentMain.PK);
					query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
					StmALog[] editLog = new BusinessObjectFactory().Load<StmALog>(query);
					AssertEquals(0, editLog.Length);

					AssertStorageDocsFields(storageMainPk);
				}
				finally
				{
					auxConnection.ExecuteNonQuery(string.Format("DELETE [{0}]..StorageDocs WHERE SC_SM = '{1}'", db1Name, storageMainPk.ToString()));
					AdoTestUtils.DropDbIfExists(auxConnection, db2Name);
				}
			}
		}

		void AssertStorageDocsFields(Guid storageMainPk)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var parentMain = masterFactory.Load<StorageMain>(storageMainPk);
			var docsInWriteableDb = parentMain.eDocs;
			AssertEquals("writeable eDocs collection count", 3, docsInWriteableDb.Count);

			parentMain.SM_DB = 2;
			var docsInReadonlyDb = parentMain.eDocs;
			AssertEquals("readonly eDocs collection count", 2, docsInReadonlyDb.Count);

			// Assert changed SC_FileName field
			AssertEquals("SC_FileName[0]", docsInReadonlyDb[0].SC_Desc, docsInWriteableDb[0].SC_FileName);
			AssertEquals("SC_FileName[1]", "DOC", docsInReadonlyDb[1].SC_FileName);
			AssertEquals("SC_FileName[1]", "CHG", docsInWriteableDb[1].SC_FileName);
			AssertEquals("SC_FileName[2]", "NEW", docsInWriteableDb[2].SC_FileName);

			for (int i = 0; i < 2; i++)
			{
				AssertEquals("Documents are the same object instance?", false, Object.ReferenceEquals(docsInReadonlyDb[i], docsInWriteableDb[i]));
				AssertEquals("PK", docsInReadonlyDb[i].PK, docsInWriteableDb[i].PK);
				AssertEquals("SC_SM", docsInReadonlyDb[i].SC_SM, docsInWriteableDb[i].SC_SM);
				AssertEquals("SC_Date", docsInReadonlyDb[i].SC_Date, docsInWriteableDb[i].SC_Date);
				AssertEquals("SC_ImageData", docsInReadonlyDb[i].SC_ImageData, docsInWriteableDb[i].SC_ImageData);
				AssertEquals("SC_DataType", docsInReadonlyDb[i].SC_DataType, docsInWriteableDb[i].SC_DataType);
				AssertEquals("SC_DocType", docsInReadonlyDb[i].SC_DocType, docsInWriteableDb[i].SC_DocType);
				AssertEquals("SC_Desc", docsInReadonlyDb[i].SC_Desc, docsInWriteableDb[i].SC_Desc);
				AssertEquals("SC_IsDeleted", docsInReadonlyDb[i].SC_IsDeleted, docsInWriteableDb[i].SC_IsDeleted);
				AssertEquals("SC_IsPublished", docsInReadonlyDb[i].SC_IsPublished, docsInWriteableDb[i].SC_IsPublished);
				AssertEquals("SC_IsSystemGenerated", docsInReadonlyDb[i].SC_IsSystemGenerated, docsInWriteableDb[i].SC_IsSystemGenerated);
				AssertEquals("SC_SaveVersions", docsInReadonlyDb[i].SC_SaveVersions, docsInWriteableDb[i].SC_SaveVersions);
				AssertEquals("SC_ParentID", docsInReadonlyDb[i].SC_ParentID, docsInWriteableDb[i].SC_ParentID);
			}
		}

		void CreateTestStorageDocs(DbConnection auxConnection, string storageDbName, Guid storageMainPk)
		{
			var deleteStorageDocTableScript = FormattableString.Invariant($@"
					IF OBJECT_ID('{storageDbName}..StorageDocs','U') IS NOT NULL
						DROP TABLE [{storageDbName}].[dbo].[StorageDocs]
");

			var createDbObjectScript = new DbUpgrader.Resource.ScriptManager().DocManagerSchemaScript;

			var insertDocSqlRaw = @"
				INSERT dbo.StorageDocs (
					SC_PK, SC_SM, SC_Date, SC_ImageData, SC_DataType, SC_DocType, SC_Desc, SC_FileName,
					SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_ParentID, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc
				)
				VALUES (
					newid(), '{0}', getutcdate(), {1}, '{2}', '{2}', '{2}', '{2}',
					'N', '{3}', 'N', '{3}', newid(), getutcdate(), getutcdate()
				);
				";

			using (((ICurrentDbControl)auxConnection).UseDatabase(storageDbName))
			{
				auxConnection.ExecuteNonQuery(deleteStorageDocTableScript);
				auxConnection.ExecuteNonQuery(createDbObjectScript);

				var insertStorageDocsSql =
					string.Format(insertDocSqlRaw, storageMainPk.ToString(), "0x0001", "TIF", "Y") +
					string.Format(insertDocSqlRaw, storageMainPk.ToString(), "0x0002", "DOC", "N");
				auxConnection.ExecuteNonQuery(insertStorageDocsSql);
			}
		}

		[UseSnapshotProtection]
		public void TestStorageMain_IsAutoLogged_ButDoesNotHaveAddAndEdtEvents()
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			string db1Name = masterFactory.GetDatabaseName(1);
			string db2Name = masterFactory.GetDatabaseName(2);
			Guid storageMainPk = Guid.NewGuid();

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				new DocManagerDBHelper()
					.GetLastWritableDatabaseName(); // this line can generate SD001 automatically if there's no db1Name existing.
				AdoTestUtils.DropDbIfExists(auxConnection, db2Name);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(auxConnection, db2Name);
					CreateTestStorageDocs(auxConnection, db2Name, storageMainPk);

					string insertStorageMainSql = string.Format(@"
						INSERT dbo.StorageMain (SM_PK, SM_DB) VALUES ('{0}', 2);",
						storageMainPk.ToString());
					auxConnection.ExecuteNonQuery(insertStorageMainSql);
					StorageMain parentMain = masterFactory.Load<StorageMain>(storageMainPk);
					AssertEquals("Pre-condition: StorageMain is auto logged", false, parentMain.IsAutoAdminBusinessObjectLoggerEnabled);
					masterFactory.Save();

					parentMain.HasChanges = false;
					masterFactory.Save();

					parentMain.eDocs[1].SC_FileName = "CHG";
					parentMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
					{
						FileName = "NEW",
						DocumentType = "NEW",
					});
					masterFactory.Save();

					parentMain.HasChanges = true;
					masterFactory.Save();

					ZQuery queryEdt = new ZQuery(StmALogSchema.SL_Table, parentMain.TableName);
					queryEdt.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
					StmALog[] editLog = new BusinessObjectFactory().Load<StmALog>(queryEdt);
					AssertEquals(0, editLog.Length);

					ZQuery queryAdd = new ZQuery(StmALogSchema.SL_Table, parentMain.TableName);
					queryAdd.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
					StmALog[] addLog = new BusinessObjectFactory().Load<StmALog>(queryAdd);
					AssertEquals(0, addLog.Length);
				}
				finally
				{
					auxConnection.ExecuteNonQuery(string.Format("DELETE [{0}]..StorageDocs WHERE SC_SM = '{1}'",
						db1Name, storageMainPk.ToString()));
					AdoTestUtils.DropDbIfExists(auxConnection, db2Name);
				}
			}
		}
	}
}
