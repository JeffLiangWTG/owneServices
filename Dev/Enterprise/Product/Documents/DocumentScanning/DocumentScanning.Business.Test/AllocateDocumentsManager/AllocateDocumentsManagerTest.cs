using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.DocumentScanning.Business.Test	
{
	public class AllocateDocumentsManager_Test : TestCaseWithFactory
	{
		public void TestAllocateDocumentDoesnotBlowupWhenDocumentCannotBeAllocatedCorrectly()
		{
			var dBHelper = new DocManagerDBHelperTestClass();
			if (!dBHelper.DatabaseExists(1))
			{
				dBHelper.CreateDatabase(1);
			}
			DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageDocsUnallocated obj = docFactory.New<StorageDocsUnallocated>();
			AllocateDocumentsManager docManager = new AllocateDocumentsManager(docFactory);
			var objectShouldBeNull = docManager.AllocateDocument(obj, ZGuid.Empty);
			AssertNull("If the itempk is empty then we should simply return null and not blow up the collection", objectShouldBeNull);
		}
	}

	public class AllocateDocumentsManagerExposed : AllocateDocumentsManager
	{
		public AllocateDocumentsManagerExposed(DocumentFactory factory)
			: base(factory)
		{
		}

		public void AllocateDocumentExposed(StorageDocsBase document, ZGuid itemFK)
		{
			var doc = MasterFactory.New<StorageDocs>();
			doc.CopyPersistentValuesFrom(document);
			doc.SC_DataType = Core.Constants.FileFormats.TIF;
			var parent = MasterFactory.CreateParentFor(doc);
			parent.SM_ParentFK = document.SC_ParentID;
			base.AllocateDocument(doc, document, itemFK);
		}

		public StorageDocs AllocateDocumentExposedForArchiving(StorageDocsBase document, ZGuid itemFK)
		{
			var doc = MasterFactory.New<StorageDocs>();
			doc.CopyPersistentValuesFrom(document);
			doc.SC_DataType = Core.Constants.FileFormats.TIF;
			var parent = MasterFactory.CreateParentFor(doc);
			parent.SM_ParentFK = document.SC_ParentID;
			parent.IsArchiving = true;
			parent.SM_ParentFK = itemFK;

			base.AllocateDocument(doc, document, itemFK);

			return doc;
		}

		public void UnallocateDocumentExposed(StorageDocsBase document)
		{
			base.UnallocateDocument(document);
		}
	}

	public class AllocateDocumentsManagerBarcodeReading : AllocateDocumentsManagerExposed
	{
		public AllocateDocumentsManagerBarcodeReading(DocumentFactory factory) : base(factory)
		{
		}

		public override FileImporter FileImporterForImport
		{
			get { return new FileImporterDocumentManagerTest(MasterFactory, false); }
		}
	}

	public class FileImporterDocumentManagerTest : FileImporter
	{
		public FileImporterDocumentManagerTest(DocumentFactory factory, bool isForImport) : base(factory, isForImport)
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

	[TestedType(typeof(AllocateDocumentsManager))]
	sealed class AllocateDocumentsManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocateDocumentsManager(new DocumentFactoryProvider().GetFactory(Factory));
		}

		public void TestDeleteDocumentShouldHaveDDPLog()
		{
			Doc1FirstSet.SC_FileName = "tif file";
			DocManager.DeleteDocument(Doc1FirstSet.PK);
			Assert("should be deleted", Doc1FirstSet.IsDeleted);
			var deletedLog = Doc1FirstSet.ParentMain.GetLogs().MostRecentLogByEventTime(Events.DocumentDeletedPermanently);
			AssertNotNull(deletedLog);

			var expectedReference = $"eDoc 'Eagle Datamation International-BN - AUBNE-MSC-tif file.tif' Document Deleted Permanently|{Doc1FirstSet.PK}";
			AssertEquals(expectedReference, deletedLog.SL_Reference);
		}

		public void TestIsJpegFile()
		{
			AssertEquals("jpg", true, AllocateDocumentsManager.IsJpegFile("xxx.jpg"));
			AssertEquals("jpeg", true, AllocateDocumentsManager.IsJpegFile("xxx.jpeg"));
		}

		public void TestIsGifFile()
		{
			AssertEquals("gif", true, AllocateDocumentsManager.IsGifFile("xxx.gif"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAsImage()
		{
			var imageBytes = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, Path.Combine(TestDocsHelper.TestDocsPath, @"TestDamagedImage.tif")));
			using (var memoryStream = new MemoryStream(imageBytes))
			{
				AssertNull("Should return null for corrupted image file", AllocateDocumentsManager.AsImage(memoryStream));
			}
		}

		public void TestAllocatedDocuments()
		{
			AssertEquals("Precondition: Should be 6 allocated documents", 6, DocManager.AllocatedDocuments.Count);
			DocManager.AllocateDocumentExposed(Doc1UnallocatedSet1, Org1.PK);
			AssertEquals("After allocating a document, the allocated document count should increase by 1", 7, DocManager.AllocatedDocuments.Count);
		}

		public void TestAllocatedDocumentsViewForHasChanges()
		{
			MasterFactory.Save();
			AssertEquals("Precondition: Should be 6 allocated documents", 6, DocManager.AllocatedDocumentsView.Count);
			DocManager.AllocatedDocumentsView.AddNew();
			AssertEquals("After allocating a document, the allocated document count should increase by 1", 7, DocManager.AllocatedDocumentsView.Count);
			DocManager.AllocatedDocuments.AddNew();
		}

		public void TestUnallocatedDocuments()
		{
			AssertEquals("Precondition: should be 4 unallocated documents", 4, DocManager.UnallocatedDocuments.Count);
			DocManager.AllocateDocumentExposed(Doc1UnallocatedSet1, Org1.PK);
			AssertEquals("After allocating, the unallocated document count should decrease by 1", 3, DocManager.UnallocatedDocuments.Count);
		}

		public void TestAllocateDocumentsWhenArchiving()
		{
			var doc = DocManager.AllocateDocumentExposedForArchiving(Doc1UnallocatedSet1, Org1.PK);
			AssertEquals("Owner should output no logs while archiving", 0, doc.ParentMain.GetLogs().GetAllLogs().Count);
		}

		public void TestUnallocateDocuments()
		{
			BusinessObject[] list = { Doc1FirstSet };

			AssertEquals("Precondition: there are 3 docs for the StorageMain in DB1", 3, ParentFirstSet.Documents.Count);
			AssertEquals("Precondition: there are 4 unallocated docs", 4, DocManager.UnallocatedDocuments.Count);
			DocManager.UnallocateDocuments(list);
			AssertEquals("After unallocating 1 doc, the document count on that StorageMain should decrease by 1", 2, ParentFirstSet.Documents.Count);
			AssertEquals("Now the unallocated document collection count should increase by 1", 5, DocManager.UnallocatedDocuments.Count);
		}

		public void TestAllocateDocuments()
		{
			Doc1UnallocatedSet1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1UnallocatedSet1.SC_Desc = "Misc";
			Doc1UnallocatedSet1.SC_ParentID = Org1.PK;

			Doc1UnallocatedSet2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1UnallocatedSet2.SC_Desc = "Misc";
			Doc1UnallocatedSet2.SC_ParentID = Org1.PK;

			AssertEquals("Precondition: there are 3 docs for the storagemain in DB1", 3, ParentFirstSet.Documents.Count);
			DocManager.AllocateDocuments();

			AssertEquals("After allocating 2 documents the document count for the storagemain should increase by 2", 5, ParentFirstSet.Documents.Count);
			AssertEquals("The unallocated count in the collection should decrease by 2", 2, DocManager.UnallocatedDocuments.Count);
			//AssertEquals("The parent count for unallocated docs should always be 0", 0, DocManager.DocumentParents.Count);
		}

		public void TestLogs()
		{
			var originalValue = SystemDataRegistry.Instance.DDIDocumentSource.Value;
			try
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				RefDocType docType = MasterFactory.NewWithValidTestData<RefDocType>();

				docType.RT_Desc = "XXX";
				docType.RT_DocType = "XXX";
				docType.RT_ReferenceType = "ALL";
				docType.RT_SE_NKDocumentReceivedEvent = Events.Arrival.Code;

				MasterFactory.Save();

				Doc1UnallocatedSet1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				Doc1UnallocatedSet1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				Doc1UnallocatedSet1.SC_Desc = "Misc";
				Doc1UnallocatedSet1.SC_ParentID = Org1.PK;

				Doc1UnallocatedSet2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				Doc1UnallocatedSet2.SC_DocType_List.Add(docType);
				Doc1UnallocatedSet2.SC_DocType = docType.RT_DocType;
				Doc1UnallocatedSet2.SC_Desc = "Misc 2";
				Doc1UnallocatedSet2.SC_ParentID = Org1.PK;

				AssertEquals("Precondition: there are 3 docs for the storagemain in DB1", 3, ParentFirstSet.Documents.Count);
				DocManager.AllocateDocuments();

				AssertEquals("After allocating 2 documents the document count for the storagemain should increase by 2", 5, ParentFirstSet.Documents.Count);
				AssertEquals("The unallocated count in the collection should decrease by 2", 2, DocManager.UnallocatedDocuments.Count);

				AssertNotNull((ParentFirstSet.DocumentOwner as EnterpriseBusinessObject).Logs.MostRecentLogByEventTime(Events.DocumentAllocated, string.Concat("XXX|", ParentFirstSet.Documents[3].PK.ToString())));
				AssertNotNull((ParentFirstSet.DocumentOwner as EnterpriseBusinessObject).Logs.MostRecentLogByEventTime(Events.DocumentAllocated, string.Concat(Core.Constants.RefDocTypes.MiscellaneousDocument, "|", ParentFirstSet.Documents[4].PK.ToString())));
				AssertNotNull((ParentFirstSet.DocumentOwner as EnterpriseBusinessObject).Logs.MostRecentLogByEventTime(Events.Arrival, "eDoc 'Misc 2' allocated"));
			}
			finally
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestAllocateDocumentsWithInvalidParentFK()
		{
			StorageDocsUnallocated testDoc = MasterFactory.New<StorageDocsUnallocated>();
			testDoc.SC_ParentID = ZGuid.Empty;
			testDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			testDoc.SC_Desc = "Test document";
			testDoc.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			DocManager.UnallocatedDocuments.Add(testDoc);

			int originalAllocatedDocs = DocManager.AllocatedDocuments.Count;
			int originalUnallocatedDocs = DocManager.UnallocatedDocuments.Count;

			DocManager.AllocateDocuments([testDoc]);

			AssertEquals("The doc should not be allocated if the ItemFK is empty.", originalAllocatedDocs, DocManager.AllocatedDocuments.Count);
			AssertEquals("The doc should not be removed from the uallocated collection if it is not allocated.", originalUnallocatedDocs, DocManager.UnallocatedDocuments.Count);
		}

		public void TestNoErrorsExistAfterAllocating()
		{
			Doc1UnallocatedSet1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1UnallocatedSet1.SC_Desc = "Misc";
			Doc1UnallocatedSet1.SC_ParentID = Org1.PK;

			Doc1UnallocatedSet2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1UnallocatedSet2.SC_Desc = "Misc";
			Doc1UnallocatedSet2.SC_ParentID = Org1.PK;

			DocManager.AllocateDocuments();

			foreach (var document in DocManager.UnallocatedDocuments)
			{
				Assert("Document should not have errors", !document.HasErrors);
			}

			foreach (var document in DocManager.AllocatedDocuments)
			{
				Assert("Document should not have errors", !document.HasErrors);
			}
		}

		public void TestDeleteDocumentsQuietly()
		{
			BusinessObject[] list = { Doc1FirstSet };
			AssertEquals("Precondition: 6 docs in Allocated collection", 6, DocManager.AllocatedDocuments.Count);
			DocManager.DeleteDocumentsQuietly(list);
			AssertEquals("After deleting the docs, the allocated collection count should remain unchanged", 6, DocManager.AllocatedDocuments.Count);
			AssertEquals("The allocated view count should decrease by 1", 5, DocManager.AllocatedDocumentsView.Count);

			BusinessObject[] newList = { Doc1UnallocatedSet1 };
			AssertEquals("Precondition: 4 unallocated docs in collection", 4, DocManager.UnallocatedDocuments.Count);
			DocManager.DeleteDocumentsQuietly(newList);
			AssertEquals("The unallocated collection count should remain unchanged", 4, DocManager.UnallocatedDocuments.Count);
			AssertEquals("The unallocated view should decrease by 1", 3, DocManager.UnallocatedDocumentsView.Count);
		}

		[ExpectNoExceptions]
		public void TestDeleteDocumentsQuietlyDoesNotCauseProblemsOnSave()
		{
			Factory.Save();
			BusinessObject[] list = { Doc1UnallocatedSet1 };
			DocManager.DeleteDocumentsQuietly(list);
			Factory.Save();
		}

		public void TestDeleteDocumentsPermanently()
		{
			Doc1UnallocatedSet1.SC_FileName = "deletedFile";
			Doc1UnallocatedSet1.SC_DataType = "txt";
			BusinessObject[] newList = { Doc1UnallocatedSet1 };
			AssertEquals("Precondition: 4 unallocated docs in collection", 4, DocManager.UnallocatedDocuments.Count);
			DocManager.DeleteDocumentsPermanently(newList);
			AssertEquals("The unallocated collection count should decrease by 1", 3, DocManager.UnallocatedDocuments.Count);
			AssertEquals("The unallocated view should also decrease by 1", 3, DocManager.UnallocatedDocumentsView.Count);

			var deletedLog = Doc1UnallocatedSet1.GetLogs().Find(log => log.SL_SE_NKEvent == "DDP");
			AssertEquals(1, deletedLog.Count());

			var expectedReference = $"eDoc 'Eagle Datamation International-BN - AUBNE--deletedFile.txt' Document Deleted Permanently|{Doc1UnallocatedSet1.PK}";
			AssertEquals(expectedReference, deletedLog.First().SL_Reference);

			Assert(Doc1UnallocatedSet1.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestDeleteDocumentsPermanentlyDoesNotCauseProblemsOnSave()
		{
			Factory.Save();
			BusinessObject[] list = { Doc1UnallocatedSet1 };
			DocManager.DeleteDocumentsPermanently(list);
			Factory.Save();
		}

		public void TestAllocateDocument()
		{
			AssertEquals("Precondition: 4 unallocated documents", 4, DocManager.UnallocatedDocuments.Count);
			AssertEquals("Precondition: 6 allocated documents in collection", 6, DocManager.AllocatedDocuments.Count);
			DocManager.AllocateDocumentExposed(Doc1UnallocatedSet1, Org1.PK);
			//AssertEquals("After allocating one the parent count should still be 0", 0, DocManager.DocumentParents.Count);
			AssertEquals("After allocating one the unallocated document count should decrease by 1", 3, DocManager.UnallocatedDocuments.Count);
			AssertEquals("After allocating one the allocated document count should increase by 1", 7, DocManager.AllocatedDocuments.Count);
		}

		public void TestUnallocateDocument()
		{
			AssertEquals("Precondition: 4 unallocated documents", 4, DocManager.UnallocatedDocuments.Count);
			AssertEquals("Precondition: 6 documents in allocated collection", 6, DocManager.AllocatedDocuments.Count);
			DocManager.UnallocateDocumentExposed(Doc1FirstSet);
			AssertEquals("After unallocating one the unallocated collection count should increase by 1", 5, DocManager.UnallocatedDocuments.Count);
			AssertEquals("After unallocating one the allocated count should decrease by 1", 5, DocManager.AllocatedDocuments.Count);
			//AssertEquals("After unallocating one the ParentCount should still be 0", 0, DocManager.DocumentParents.Count);
		}

		public void TestAddFromFilesystem()
		{
			AssertEquals("Precondition: 4 unallocated documents", 4, DocManager.UnallocatedDocuments.Count);

			var testFileDropXls = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");
			var testFileDropGIF = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
			var testFileDropPDF = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			var invalidFilename = Path.Combine(Env.TempPath, TestDocsHelper.TestDocsPath, @"abc.abc");
			var files = new[] { TestTifPath, testFileDropXls, testFileDropGIF, testFileDropPDF, invalidFilename };
			var rejectedFiles = ((IDragDropSupport)DocManager).Add(files);

			MasterFactory.Save();

			AssertEquals("Three more unallocated docs after filedrop - two image files and one pdf only", 7, DocManager.UnallocatedDocuments.Count);
			AssertEquals("Rejected file count is 2 (the abc and xls files are of unsupported extension type)", 2, rejectedFiles.Length);
			AssertCollectionContains("Invalid file should be rejected", invalidFilename, rejectedFiles);
			AssertCollectionContains("xls file should be rejected", testFileDropXls, rejectedFiles);
		}

		public void TestReadBarcodesWhenDraggingAndDropping()
		{
			AssertEquals("Precondition: 4 unallocated documents", 4, DocManager.UnallocatedDocuments.Count);
			var twoBarcodesOnSamePageTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.TwoBarcodesOnSamePage.TIF", "TwoBarcodesOnSamePage.TIF");
			string[] files = { twoBarcodesOnSamePageTifPath };
			((IDragDropSupport)DocManager).Add(files);
			MasterFactory.Save();
			AssertEquals("one more unallocated docs after filedrop", 5, DocManager.UnallocatedDocuments.Count);
			AssertEquals("ScannedBarcodeValue", "^DMC=CN00000023;MFD;| , ^DMC=CN00000004;MFD;|", DocManager.UnallocatedDocuments[4].ScannedBarcodeValue);

			var documentPK = DocManager.UnallocatedDocuments[4].PK;
			var barcodePK = DocManager.UnallocatedDocuments[4].StorageDocsBarcode.PK;
			var masterFactory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reloadedDocument = masterFactory2.Load<StorageDocsUnallocated>(documentPK);
			AssertEquals("TwoBarcodesOnSamePage", reloadedDocument.SC_FileName);
			AssertEquals("ScannedBarcodeValue is saved to database", "^DMC=CN00000023;MFD;| , ^DMC=CN00000004;MFD;|", reloadedDocument.ScannedBarcodeValue);

			reloadedDocument.Delete();
			masterFactory2.Save();
			AssertEquals(null, masterFactory2.Load<StorageDocsBarcode>(barcodePK));
		}

		public void TestAddFromClipboard()
		{
			var tIFContents = TestTifBytes;
			var xLSContents = TestXlsBytes;
			var gIFContents = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");

			Doc1FirstSet.SC_ImageData = tIFContents;
			Doc1FirstSet.SC_FileName = "tif file";
			Doc2FirstSet.SC_ImageData = xLSContents;
			Doc2FirstSet.SC_FileName = "xls file";
			Doc3FirstSet.SC_ImageData = gIFContents;
			Doc3FirstSet.SC_FileName = "gif file";

			var documents = new BusinessObject[] { Doc1FirstSet, Doc2FirstSet, Doc3FirstSet };
			var dataFromClipboard = SerializableEDocCollection.New(documents);

			var initialUnalloctedDocCount = DocManager.UnallocatedDocuments.Count;

			((IDragDropSupport)DocManager).Add(dataFromClipboard);

			AssertEquals("Should have created 3 more documents on top of the 4 existing unallocated docs, one for each file", 7, DocManager.UnallocatedDocuments.Count);

			var firstDocumentAdded = DocManager.UnallocatedDocuments[initialUnalloctedDocCount]; // 0-based index
			var secondDocumentAdded = DocManager.UnallocatedDocuments[initialUnalloctedDocCount + 1];
			var thirdDocumentAdded = DocManager.UnallocatedDocuments[initialUnalloctedDocCount + 2];

			AssertNotEquals("First Document should not have the TIF contents", tIFContents, firstDocumentAdded.SC_ImageData);
			AssertEquals("tif file", firstDocumentAdded.SC_FileName);
			AssertEquals("Second Document should have the XLS contents", xLSContents, secondDocumentAdded.SC_ImageData);
			AssertEquals("xls file", secondDocumentAdded.SC_FileName);
			AssertEquals("Third Document should have the GIF contents", gIFContents, thirdDocumentAdded.SC_ImageData);
			AssertEquals("gif file", thirdDocumentAdded.SC_FileName);
		}

		public void TestAddFromClipboardWithPreviewableFiles()
		{
			var pdfDocument = StorageFile.NewWithParent_DEBUG(MasterFactory);
			pdfDocument.SC_DataType = "PDF";
			pdfDocument.SC_ImageData = SamplePdfBytes;

			var documents = new BusinessObject[] { pdfDocument };
			var dataFromClipboard = SerializableEDocCollection.New(documents);
			var dragDropSupport = (IDragDropSupport)DocManager;

			DocManager.NotifyNonPreviewableFilesNotAdded += DocManager_NotifyNonPreviewableFilesNotAdded;
			AssertEquals("Precondition: notify event hasn't been called yet", 0, NotifyNotifyNonPreviewableFilesNotAddedCallCount);

			dragDropSupport.Add(dataFromClipboard);
			AssertEquals("Notification shouldn't have fired - now we can drag pdfs", 0, NotifyNotifyNonPreviewableFilesNotAddedCallCount);

			var tifDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			tifDocument.SC_DataType = "TIF";
			tifDocument.SC_ImageData = TestTifBytes;

			documents = new BusinessObject[] { tifDocument };
			dataFromClipboard = SerializableEDocCollection.New(documents);

			dragDropSupport.Add(dataFromClipboard);
			AssertEquals("Notification shouldn't have fired - the datatype is a TIF and ok to drop", 0, NotifyNotifyNonPreviewableFilesNotAddedCallCount);

			var xlsDocument = StorageFile.NewWithParent_DEBUG(MasterFactory);
			xlsDocument.SC_DataType = "XLS";
			xlsDocument.SC_ImageData = TestXlsBytes;

			documents = new BusinessObject[] { xlsDocument };
			dataFromClipboard = SerializableEDocCollection.New(documents);

			dragDropSupport.Add(dataFromClipboard);
			AssertEquals("Notification should have fired - the datatype is a XLS and cannot ok to drop", 1, NotifyNotifyNonPreviewableFilesNotAddedCallCount);
		}

		void DocManager_NotifyNonPreviewableFilesNotAdded(object sender, EventArgs e)
		{
			NotifyNotifyNonPreviewableFilesNotAddedCallCount++;
		}

		int NotifyNotifyNonPreviewableFilesNotAddedCallCount;

		public void AsssertCreateNewDocument(byte[] imageData, StorageDocsBase storageDocBase)
		{
			storageDocBase.SC_ImageData = imageData;
			storageDocBase.SC_Date = new ZDateTime(2005, 12, 01);

			var eDoc = new SerializableEDoc(storageDocBase);
			DocManager.CreateNewDocument(eDoc);
			AssertEquals("Should have created 1 more document on top of the 4 existing unallocated docs", 5, DocManager.UnallocatedDocuments.Count);

			var diff = ZDateTime.UtcNow - DocManager.UnallocatedDocuments[4].SC_Date;
			Assert("Date should be updated to now", diff.TotalSeconds < 10);

			AssertEquals(storageDocBase.ParentMain.SM_ParentFK, DocManager.UnallocatedDocuments[4].SC_ParentID);
			AssertEquals(storageDocBase.SM_Type, DocManager.UnallocatedDocuments[4].SM_Type);
		}

		public void TestCreateNewDocument_TIF()
		{
			var storageDoc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			storageDoc.SC_DataType = "TIF";

			AsssertCreateNewDocument(TestTifBytes, storageDoc);
		}

		public void TestCreateNewDocument_PDF()
		{
			var storageFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			storageFile.SC_DataType = "PDF";

			AsssertCreateNewDocument(SamplePdfBytes, storageFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewDocumentAfterScanning()
		{
			using (TempFile tempFile = TempFile.NewWithExtension("tif"))
			{
				var sourceFilePath = Path.Combine(BaseSourcePath, Path.Combine(TestDocsHelper.TestDocsPath, @"Small.tif"));
				File.Copy(sourceFilePath, tempFile.Filename, true);
				File.SetAttributes(tempFile.Filename, FileAttributes.Normal);

				AssertEquals(0, DocManager.UnallocatedDocuments.Where(x => x.SC_FileName == "Small").Count());

				var documentResult = new DocumentResult(MasterFactory, tempFile.Filename);
				documentResult.SourceFileName = sourceFilePath;

				ScanningFinishedEventArgs args = new ScanningFinishedEventArgs();
				args.AddFileDetail(documentResult);

				DocManager.AddNewDocumentAfterScanning(args, true);

				AssertEquals("Should be one more unallocated file", 5, DocManager.UnallocatedDocuments.Count);
				AssertEquals("New scanned file has file name set", 1, DocManager.UnallocatedDocuments.Where(x => x.SC_FileName == "Small").Count());
				AssertEquals(1, DocManager.UnallocatedDocuments.Where(x => x.AllocationStatus == "No suitable barcode detected").Count());
				AssertEquals("File should be deleted afterwards", false, File.Exists(tempFile.Filename));
				AssertPostScanningState();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewDocumentAfterScanningWithAutoAllocatableDoc()
		{
			using (var tempFile = TempFile.NewWithExtension("tif"))
			{
				File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"AutoAllocatableDoc.tif"), tempFile.Filename, true);
				File.SetAttributes(tempFile.Filename, FileAttributes.Normal);

				var testDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "TST"));

				if (testDocType == null)
				{
					testDocType = Factory.New<RefDocType>();
					testDocType.RT_DocType = "TST";
					testDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
					testDocType.RT_Desc = "Test";
					Factory.Save();
				}

				var shipment = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

				if (shipment == null)
				{
					shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>());
					shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001000";
				}

				shipment[JobShipmentSchema.JS_TransportMode] = Core.Constants.TransportModes.Sea;
				shipment[JobShipmentSchema.JS_PackingMode] = "FCL";
				Factory.Save();

				var documentResult = new DocTypeBarcode(MasterFactory, "^SHP=S00001000;TST|");
				documentResult.FilePath = tempFile.Filename;
				documentResult.SourceFileName = "Filename";

				var args = new ScanningFinishedEventArgs();
				args.AddFileDetail(documentResult);

				DocManager.AddNewDocumentAfterScanning(args, true);

				AssertEquals("Should be four allocated docs", 4, DocManager.UnallocatedDocuments.Count);
				AssertEquals("File should be deleted afterwards", false, File.Exists(tempFile.Filename));
				AssertPostScanningState();
			}
		}

		public void TestAddNewDocumentAfterScanningWithAutoAllocatableDoc_PDFFile()
		{
			var pdfDocumentWithBarcodePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.PDF Document with Barcode - S00001000.PDF");
			File.SetAttributes(pdfDocumentWithBarcodePath, FileAttributes.Normal);

			var testDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "TST"));

			if (testDocType == null)
			{
				testDocType = Factory.New<RefDocType>();
				testDocType.RT_DocType = "TST";
				testDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
				testDocType.RT_Desc = "Test";
				Factory.Save();
			}

			var shipment = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

			if (shipment == null)
			{
				shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>());
				shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001000";
			}

			shipment[JobShipmentSchema.JS_TransportMode] = Core.Constants.TransportModes.Sea;
			shipment[JobShipmentSchema.JS_PackingMode] = "FCL";
			Factory.Save();

			var documentResult = new DocTypeBarcode(MasterFactory, "^SHP=S00001000;TST|");
			documentResult.FilePath = pdfDocumentWithBarcodePath;
			documentResult.SourceFileName = "Filename";

			var args = new ScanningFinishedEventArgs();
			args.AddFileDetail(documentResult);

			DocManager.AddNewDocumentAfterScanning(args, true);
			var storageDocs = DocManager.AllocatedDocuments.Last() as StorageDocs;

			AssertEquals("Should be four allocated docs", 4, DocManager.UnallocatedDocuments.Count);
			Assert("File should be deleted afterwards", !File.Exists(pdfDocumentWithBarcodePath));
			AssertEquals(Core.Constants.FileFormats.PDF, storageDocs.SC_DataType);
			AssertPostScanningState();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewDocumentAfterScanningWithMSCDoc_ShouldBeAutoAllocatable()
		{
			using (var tempFile = TempFile.NewWithExtension("tif"))
			{
				File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"AutoAllocatableDoc.tif"), tempFile.Filename, true);
				File.SetAttributes(tempFile.Filename, FileAttributes.Normal);

				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>());
				Factory.Save();

				var barcode = new BaseBarcode(MasterFactory);
				barcode.FilePath = tempFile.Filename;
				barcode.DocManagerCode = Core.Constants.DocManagerCodes.Shipment;
				barcode.RefCode = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
				barcode.DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				barcode.SourceFileName = "Filename";

				var args = new ScanningFinishedEventArgs();
				args.AddFileDetail(barcode);

				DocManager.AddNewDocumentAfterScanning(args, true);
				AssertEquals("DocManager.UnallocatedDocuments.Count", 4, DocManager.UnallocatedDocuments.Count);
				AssertEquals("File should be deleted afterwards.", false, File.Exists(tempFile.Filename));
				AssertPostScanningState();

				File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"AutoAllocatableDoc.tif"), tempFile.Filename, true);
				File.SetAttributes(tempFile.Filename, FileAttributes.Normal);
				DocManager.AddNewDocumentAfterScanning(args, false);
				var unallocatedDocument = DocManager.UnallocatedDocuments.Last() as StorageDocsUnallocated;

				AssertEquals("Should get parent id from barcode ref pk even not auto allocate.", unallocatedDocument.SC_ParentID, barcode.RefPK);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewDocumentAfterScanningWithOutOfMemoryExceptionIsHandled()
		{
			using (var tempFile = TempFile.NewWithExtension("tif"))
			{
				File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"AutoAllocatableDoc.tif"), tempFile.Filename, true);
				File.SetAttributes(tempFile.Filename, FileAttributes.Normal);

				var testDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "TST"));

				if (testDocType == null)
				{
					testDocType = Factory.New<RefDocType>();
					testDocType.RT_DocType = "TST";
					testDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
					testDocType.RT_Desc = "Test";
					Factory.Save();
				}

				var shipment = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

				if (shipment == null)
				{
					shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>());
					shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001000";
				}

				shipment[JobShipmentSchema.JS_TransportMode] = Core.Constants.TransportModes.Sea;
				shipment[JobShipmentSchema.JS_PackingMode] = "FCL";
				Factory.Save();

				var documentResult = new DocTypeBarcode(MasterFactory, "^SHP=S00001000;TST|");
				documentResult.FilePath = tempFile.Filename;
				documentResult.SourceFileName = "Filename";

				var args = new ScanningFinishedEventArgs();
				args.AddFileDetail(documentResult);

				try
				{
					var originalAllocatedDocsCount = DocManager.AllocatedDocuments.Count;
					DocManager.ForceThrowOutOfMemoryException = true;
					NUnit.Framework.Assert.That(delegate
					{
						DocManager.AddNewDocumentAfterScanning(args, true);
					}, CustomConstraints.InnermostExceptionThrown(typeof(OutOfMemoryException), "An error has occured because this program is running low on memory."), "Shoud handle the OOM exception");
					AssertEquals($"Some eDocs were not allocated because this program is running low on memory. Please restart {Core.Constants.ProductName} and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("If the document was not created with a parent, he cannot be allocated", originalAllocatedDocsCount, DocManager.AllocatedDocuments.Count); // If the document was allocated, a exception will be thrown when you try to save
					Assert(DocManager.IsDocumentWithParentDeleted);
				}
				finally
				{
					DocManager.ForceThrowOutOfMemoryException = false;
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		public void TestSqlStreamReaderRowNotFoundExceptionIsHandled()
		{
			AssertEquals(6, DocManager.AllocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);

			SetupDocumentForAllocate(Doc1UnallocatedSet1, true);
			SetupDocumentForAllocate(Doc1UnallocatedSet2, false);
			SetupDocumentForAllocate(Doc1UnallocatedSet3, true);
			SetupDocumentForAllocate(Doc1UnallocatedSet4, false);

			AssertNoExceptionThrown(() => { DocManager.AllocateDocuments(); });
			AssertEquals("Two more documents has been allocated.", 8, DocManager.AllocatedDocuments.Count);
			AssertEquals("Two documents which throws exception should have been removed.", 0, DocManager.UnallocatedDocuments.Count);
			AssertEquals($@"The following document(s) have been allocated or deleted by another user before this allocation operation was done, the allocation for these file(s) was canceled.
Parent Type: ORG Doc Type: MSC Scan Date: {now}
Parent Type: ORG Doc Type: MSC Scan Date: {now}", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNoExceptionThrown(() => { Doc1UnallocatedSet1.Factory.Save(); });
		}

		void SetupDocumentForAllocate(StorageDocsBase doc, bool forceThrowSqlStreamReaderRowNotFoundException)
		{
			doc.SC_ParentID = Org1.PK;
			doc.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			doc.SC_Desc = "AAA";
			doc.SC_Date = now;
			doc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			doc.ForceThrowSqlStreamReaderRowNotFoundException = forceThrowSqlStreamReaderRowNotFoundException;
		}
		readonly ZDateTime now = ZDateTime.Now;

		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		public void TestAllocatingDocumentsAlreadyProcessed()
		{
			//I don't think this can matter but just in case
			DocManager.EndDateForFilter = ZDateTime.Empty;
			DocManager.StartDateForFilter = ZDateTime.Empty;

			AssertEquals(6, DocManager.AllocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);

			SetupDocumentForAllocate(Doc1UnallocatedSet1, false);
			SetupDocumentForAllocate(Doc1UnallocatedSet2, false);
			SetupDocumentForAllocate(Doc1UnallocatedSet3, false);
			SetupDocumentForAllocate(Doc1UnallocatedSet4, false);

			MasterFactory.Save();
			MasterFactory.RefreshEnabled = false;
			((IBusinessObjectFactoryInternals)MasterFactory).DisableQueryCacheReset = true;
			DocManager.UnallocatedDocuments.Load();

			var anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			anotherFactory.RefreshEnabled = false;
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;
			anotherFactory.Load<StorageDocs>(Doc1UnallocatedSet1.PK).Delete();
			anotherFactory.Load<StorageDocs>(Doc1UnallocatedSet2.PK).Delete();
			anotherFactory.Save();

			//simulate doing AllocateDocuments so if a specific step fails we can introspect it - for amnesty failure
			AssertEquals("Still at six allocated", 6, DocManager.AllocatedDocuments.Count);
			AssertEquals("Still at four unallocated", 4, DocManager.UnallocatedDocuments.Count);
			AssertContainsExactElementsInAnyOrder("right PKs are here", new ZGuid[] { Doc1UnallocatedSet1.PK, Doc1UnallocatedSet2.PK, Doc1UnallocatedSet3.PK, Doc1UnallocatedSet4.PK }, DocManager.UnallocatedDocuments.Select(x => x.PK));
			AssertEquals("they're not deleted and are in the database", true, DocManager.UnallocatedDocuments.All(x => x.IsInDatabase && !x.IsDeleted));
			AssertEquals("Empty", 0, DocManager.CurrentUnallocatedDocuments.Count);

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			newFactory.NameForDebugging = "FactoryForRemoveAlreadyProcessedDocuments()";
			var latestDocumentsInDatabase = new StorageDocsUnallocatedCollection(newFactory);
			latestDocumentsInDatabase.Load();
			AssertEquals("latestDocumentsInDatabase contains 2", 2, latestDocumentsInDatabase.Count);
			AssertContainsExactElementsInAnyOrder("right PKs are here", new[] { Doc1UnallocatedSet3.PK, Doc1UnallocatedSet4.PK }, latestDocumentsInDatabase.Select(x => x.PK));
			AssertEquals("they're not deleted and are in the database", true, latestDocumentsInDatabase.All(x => x.IsInDatabase && !x.IsDeleted));

			var processedDocuments = DocManager.UnallocatedDocuments
				.Cast<StorageDocsUnallocated>()
				.Where(document => document.IsInDatabase && latestDocumentsInDatabase.FindByPK(document.PK) == null)
				.ToList();
			AssertEquals("Still at two unallocated", 2, processedDocuments.Count);
			AssertContainsExactElementsInAnyOrder("right PKs are here", new ZGuid[] { Doc1UnallocatedSet1.PK, Doc1UnallocatedSet2.PK }, processedDocuments.Select(x => x.PK));

			//back to your regularly scheduled Unit Test
			AssertNoExceptionThrown(() => { DocManager.AllocateDocuments(); });
			AssertEquals("Two more documents has been allocated.", 8, DocManager.AllocatedDocuments.Count);
			AssertEquals("Two documents which throws exception should have been removed.", 0, DocManager.UnallocatedDocuments.Count);
			AssertEquals($@"The following document(s) have been allocated or deleted by another user before this allocation operation was done, the allocation for these file(s) was canceled.
Parent Type: ORG Doc Type: MSC Scan Date: {now}
Parent Type: ORG Doc Type: MSC Scan Date: {now}", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNoExceptionThrown(() => { Doc1UnallocatedSet1.Factory.Save(); });
		}

		public void TestFilter()
		{
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocumentsView.Count);

			DocManager.StartDateForFilter = ZDateTime.Now.AddMinutes(-5);
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);
			DocManager.EndDateForFilter = ZDateTime.Now.AddMinutes(5);
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);
			DocManager.EndDateForFilter = ZDateTime.Now.AddMinutes(-4);
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(0, DocManager.UnallocatedDocumentsView.Count);
			DocManager.StartDateForFilter = ZDateTime.Empty;
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(3, DocManager.UnallocatedDocumentsView.Count);
			DocManager.StartDateForFilter = ZDateTime.Now.AddDays(-5);
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(3, DocManager.UnallocatedDocumentsView.Count);

			DocManager.ClearUnallocatedDocumentsFilterValues();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocumentsView.Count);

			DocManager.AddingUserForFilter = "E";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(2, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AddingUserForFilter = "~BP";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(2, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AddingUserForFilter = "B";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(0, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AddingUserForFilter = "";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocumentsView.Count);

			DocManager.AllocationNotesForFilter = "a";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationNotesForFilter = "c";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(2, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationNotesForFilter = "cde";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationNotesForFilter = "1";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(0, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationNotesForFilter = "";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocumentsView.Count);

			DocManager.AllocationStatusForFilter = "1";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationStatusForFilter = "3";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(2, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationStatusForFilter = "567";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationStatusForFilter = "e";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(0, DocManager.UnallocatedDocumentsView.Count);
			DocManager.AllocationStatusForFilter = "";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocumentsView.Count);

			DocManager.StartDateForFilter = ZDateTime.Now.AddDays(-1).AddMinutes(-5);
			DocManager.EndDateForFilter = ZDateTime.Now.AddDays(-1).AddMinutes(5);
			DocManager.AddingUserForFilter = "~BP";
			DocManager.AllocationNotesForFilter = "c";
			DocManager.AllocationStatusForFilter = "5";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(1, DocManager.UnallocatedDocumentsView.Count);

			DocManager.ClearUnallocatedDocumentsFilterValues();
			AssertEquals(4, DocManager.UnallocatedDocuments.Count);
			AssertEquals(4, DocManager.UnallocatedDocumentsView.Count);
		}

		public void TestInvalidParentIDInFilteredOutDocument()
		{
			DocManager.UnallocatedDocuments[0].SC_ParentID = ZGuid.Invalid;
			DocManager.UnallocatedDocuments[0].RunPreSaveValidation();
			AssertHasError(DocManager.UnallocatedDocuments[0].SC_ParentIDInfo, "Enter a valid Unique ID.");

			DocManager.AllocationStatusForFilter = "e";
			DocManager.LoadUnallocatedDocumentsWithFiltering();
			DocManager.RunPreSaveValidation();
			AssertEquals(1, DocManager.NotificationsIncludingChildren.Count(x => x.Message == "Error - SC_ParentID: Enter a valid Unique ID."));

			DocManager.ClearUnallocatedDocumentsFilterValues();
			DocManager.RunPreSaveValidation();
			AssertEquals(1, DocManager.NotificationsIncludingChildren.Count(x => x.Message == "Error - SC_ParentID: Enter a valid Unique ID."));
		}

		public void TestJoinValidBarcodesOnAdd()
		{
			var docManager = new AllocateDocumentsManagerBarcodeReading(new DocumentFactoryProvider().GetFactory(Factory));

			AssertNoExceptionThrown("Should not throw exception on allocate documents", () => { ((IDragDropSupport)docManager).Add(new string[] { TestTifPath }); });

			Assert("Should have barcodes that do not exceed the limit of ScannedBarcodeValue field", ((StorageDocsUnallocated)docManager.UnallocatedDocuments.LastOrDefault()).ScannedBarcodeValue == "^TRN=S00028854/I;CAD;| , ^TRN=S00028854/I;CAD;| , ^TRN=S00028854/I;CAD;|");
		}

		[RequiresSoftware(RequiredSoftware.CanRunGUITests)]
		public void TestOutOfMemoryExceptionIsHandled()
		{
			DocumentFactory newDocFactory1 = new DocumentFactoryProvider().GetFactory(Factory);
			StorageDocsUnallocated tempDoc = newDocFactory1.New<StorageDocsUnallocated>();
			tempDoc.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			tempDoc.SC_Date = ZDateTime.Now;
			tempDoc.SC_ImageData = SmallTifBytes;
			tempDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			tempDoc.SC_Desc = "Misc";
			tempDoc.SC_ParentID = Org1.PK;
			tempDoc.ForceThrowOutOfMemoryException = true;

			AllocateDocumentsManager manager1 = new AllocateDocumentsManager(newDocFactory1);
			manager1.UnallocatedDocuments.RemoveAll();
			manager1.AllocatedDocuments.RemoveAll();
			manager1.UnallocatedDocuments.Add(tempDoc);

			AssertNoExceptionThrown("Should not throw exception on allocate documents", () => { manager1.AllocateDocuments(); });
			AssertEquals($"Some eDocs were not allocated because this program is running low on memory. Please restart {Core.Constants.ProductName} and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("If the document was not created with a parent, he cannot be allocated", 0, manager1.AllocatedDocuments.Count); // If the document was allocated, a exception will be thrown when you try to save
		}

		public void TestPasteFromClipboardWithStorageDocsUnallocated()
		{
			var document = MasterFactory.New<StorageDocsUnallocated>();
			document.SC_DataType = "PDF";
			document.SC_ImageData = SamplePdfBytes;

			var dataFromClipboard = SerializableEDocCollection.New(new BusinessObject[] { document });
			var dragDropSupport = (IDragDropSupport)DocManager;
			AssertNoExceptionThrown(() => dragDropSupport.Add(dataFromClipboard));
		}

		public void TestShowUnallocatedDocumentsForAllCompaniesReadOnly()
		{
			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = false;
			Assert(DocManager.ShowUnallocatedDocumentsForAllCompanies_ReadOnly);

			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = true;
			Assert(!DocManager.ShowUnallocatedDocumentsForAllCompanies_ReadOnly);
		}

		public void TestShowUnallocatedDocumentsForAllBranchesReadOnly()
		{
			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = false;
			Assert(DocManager.ShowUnallocatedDocumentsForAllBranches_ReadOnly);
			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = true;
			Assert(!DocManager.ShowUnallocatedDocumentsForAllBranches_ReadOnly);
		}

		public void TestShowUnallocatedDocumentsForAllDepartmentsReadOnly()
		{
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = false;
			Assert(DocManager.ShowUnallocatedDocumentsForAllDepartments_ReadOnly);
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = true;
			Assert(!DocManager.ShowUnallocatedDocumentsForAllDepartments_ReadOnly);
		}

		public void TestShowAllocatedDocumentsForAllCompaniesReadOnly()
		{
			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = false;
			Assert(DocManager.ShowAllocatedDocumentsForAllCompanies_ReadOnly);

			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = true;
			Assert(!DocManager.ShowAllocatedDocumentsForAllCompanies_ReadOnly);
		}

		public void TestShowAllocatedDocumentsForAllBranchesReadOnly()
		{
			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = false;
			Assert(DocManager.ShowAllocatedDocumentsForAllBranches_ReadOnly);
			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = true;
			Assert(!DocManager.ShowAllocatedDocumentsForAllBranches_ReadOnly);
		}

		public void TestShowAllocatedDocumentsForAllDepartmentsReadOnly()
		{
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = false;
			Assert(DocManager.ShowAllocatedDocumentsForAllDepartments_ReadOnly);
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = true;
			Assert(!DocManager.ShowAllocatedDocumentsForAllDepartments_ReadOnly);
		}

		#region Implementation

		void AssertPostScanningState()
		{
			DocManagerDBHelper dbHelper = new DocManagerDBHelper();
			int smallestDB = dbHelper.LastWritableDatabaseWithFreeSpace();

			Assert("The smallest db should be > 0.", smallestDB > 0);

			foreach (StorageDocs doc in DocManager.AllocatedDocuments)
			{
				AssertNotNull(string.Format("Parent main must not be null for an allocated doc, but it was for the document {0}.", doc.PK), doc.ParentMain);
				AssertEquals(string.Format("Each allocated document should be assigned to a database. The document {0} was not assigned to a DB.", doc.PK), smallestDB, doc.ParentMain.SM_DB);
			}
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DocManagerDBHelperTestClass dBHelper = new DocManagerDBHelperTestClass();
			if (!dBHelper.DatabaseExists(1))
			{
				dBHelper.CreateDatabase(1);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable((new DocManagerDBHelper()).GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			Org1 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;
			filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			Org2 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			SetUpStorageMainObjects();

			DocManager = new AllocateDocumentsManagerExposed(MasterFactory);

			SetUpDocumentObjects();
			SetUpUnallocatedDocuments();

			DocManager.AllocateDocuments();

			Doc1FirstSet = ParentFirstSet.Documents[0];
			Doc2FirstSet = ParentFirstSet.Documents[1];
			Doc3FirstSet = ParentFirstSet.Documents[2];
		}

		void SetUpStorageMainObjects()
		{
			ParentFirstSet = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			ParentFirstSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			MasterFactory.AllocateToDB(ParentFirstSet);
			ParentFirstSet.SM_ParentFK = Org1.PK;

			ParentSecondSet = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			ParentSecondSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			MasterFactory.AllocateToDB(ParentSecondSet);
			ParentSecondSet.SM_ParentFK = Org2.PK;
		}

		void SetUpDocumentObjects()
		{
			Doc1FirstSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc1FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1FirstSetUnallocated.SC_ParentID = Org1.PK;
			Doc1FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1FirstSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1FirstSetUnallocated.SC_Desc = "AAA";
			Doc1FirstSetUnallocated.SC_ImageData = SmallTifBytes;

			Doc2FirstSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc2FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2FirstSetUnallocated.SC_ParentID = Org1.PK;
			Doc2FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2FirstSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc2FirstSetUnallocated.SC_Desc = "BBB";
			Doc2FirstSetUnallocated.SC_ImageData = SmallTifBytes;

			Doc3FirstSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc3FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3FirstSetUnallocated.SC_ParentID = Org1.PK;
			Doc3FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3FirstSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc3FirstSetUnallocated.SC_Desc = "CCC";
			Doc3FirstSetUnallocated.SC_ImageData = SmallTifBytes;

			Doc1SecondSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc1SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1SecondSetUnallocated.SC_ParentID = Org2.PK;
			Doc1SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1SecondSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1SecondSetUnallocated.SC_Desc = "DDD";
			Doc1SecondSetUnallocated.SC_ImageData = SmallTifBytes;

			Doc2SecondSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc2SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2SecondSetUnallocated.SC_ParentID = Org2.PK;
			Doc2SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2SecondSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc2SecondSetUnallocated.SC_Desc = "EEE";
			Doc2SecondSetUnallocated.SC_ImageData = SmallTifBytes;

			Doc3SecondSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc3SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3SecondSetUnallocated.SC_ParentID = Org2.PK;
			Doc3SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3SecondSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc3SecondSetUnallocated.SC_Desc = "FFF";
			Doc3SecondSetUnallocated.SC_ImageData = SmallTifBytes;
		}

		void SetUpUnallocatedDocuments()
		{
			Doc1UnallocatedSet1 = DocManager.UnallocatedDocuments.AddNew();
			Doc1UnallocatedSet1.SC_ImageData = SmallTifBytes;
			Doc1UnallocatedSet1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet1.SC_Date = ZDateTime.Now;
			Doc1UnallocatedSet1.SC_SystemCreateUser = "E";
			Doc1UnallocatedSet1.AllocationNotes = "abc";
			Doc1UnallocatedSet1.AllocationStatus = "123";

			Doc1UnallocatedSet2 = DocManager.UnallocatedDocuments.AddNew();
			Doc1UnallocatedSet2.SC_ImageData = SmallTifBytes;
			Doc1UnallocatedSet2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet2.SC_Date = ZDateTime.Now.AddDays(-1);
			Doc1UnallocatedSet2.SC_SystemCreateUser = "~BP";
			Doc1UnallocatedSet2.AllocationNotes = "cde";
			Doc1UnallocatedSet2.AllocationStatus = "345";

			Doc1UnallocatedSet3 = DocManager.UnallocatedDocuments.AddNew();
			Doc1UnallocatedSet3.SC_ImageData = SmallTifBytes;
			Doc1UnallocatedSet3.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet3.SC_Date = ZDateTime.Now.AddDays(-2);
			Doc1UnallocatedSet3.SC_SystemCreateUser = "E";
			Doc1UnallocatedSet3.AllocationNotes = "efg";
			Doc1UnallocatedSet3.AllocationStatus = "567";

			Doc1UnallocatedSet4 = DocManager.UnallocatedDocuments.AddNew();
			Doc1UnallocatedSet4.SC_ImageData = SmallTifBytes;
			Doc1UnallocatedSet4.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1UnallocatedSet4.SC_Date = ZDateTime.Now.AddDays(-4);
			Doc1UnallocatedSet4.SC_SystemCreateUser = "~BP";

			DocManager.MasterFactory.Save();
		}

		AllocateDocumentsManagerExposed DocManager;

		DocumentFactory MasterFactory;
		OrgHeader Org1;
		OrgHeader Org2;
		StorageMain ParentFirstSet;
		StorageDocsUnallocated Doc1FirstSetUnallocated;
		StorageDocsUnallocated Doc2FirstSetUnallocated;
		StorageDocsUnallocated Doc3FirstSetUnallocated;
		StorageMain ParentSecondSet;
		StorageDocsUnallocated Doc1SecondSetUnallocated;
		StorageDocsUnallocated Doc2SecondSetUnallocated;
		StorageDocsUnallocated Doc3SecondSetUnallocated;

		StorageDocsBase Doc1FirstSet;
		StorageDocsBase Doc2FirstSet;
		StorageDocsBase Doc3FirstSet;

		StorageDocsUnallocated Doc1UnallocatedSet1;
		StorageDocsUnallocated Doc1UnallocatedSet2;
		StorageDocsUnallocated Doc1UnallocatedSet3;
		StorageDocsUnallocated Doc1UnallocatedSet4;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] TestTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");

		string TestTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(testTifPath))
				{
					testTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");
				}
				return testTifPath;
			}
		}
		string testTifPath;

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		byte[] TestXlsBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");

		protected override void TearDown()
		{
			base.TearDown();
			foreach (var file in SerializableEDoc.fileNames)
			{
				if (File.Exists(file))
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception) // file in use, etc. don't bother trying to handle.
					{
					}
				}
			}
			SerializableEDoc.fileNames.Clear();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		#endregion
	}

	sealed class TestAllocateDocumentBugs : TestCaseWithFactory
	{
		public void TestAllocateTwoDocumentsToSameJob()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testImage = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
				ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
				OrgHeader org1 = Factory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

				// Allocate something to this org first to create an existing storagemain record for this org.
				DocumentFactory newDocFactory1 = new DocumentFactoryProvider().GetFactory(Factory);
				StorageDocsUnallocated tempDoc = newDocFactory1.New<StorageDocsUnallocated>();
				tempDoc.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				tempDoc.SC_Date = ZDateTime.Now;
				tempDoc.SC_ImageData = testImage;
				tempDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				tempDoc.SC_Desc = "Misc";
				tempDoc.SC_ParentID = org1.PK;

				AllocateDocumentsManager manager1 = new AllocateDocumentsManager(newDocFactory1);
				manager1.UnallocatedDocuments.Add(tempDoc);
				manager1.AllocateDocuments();
				newDocFactory1.Save();
				ZGuid savedStorageMainPK = manager1.AllocatedDocuments[0].ParentMain.PK;

				// Now start allocating
				DocumentFactory newDocFactory = new DocumentFactoryProvider().GetFactory(Factory);
				AllocateDocumentsManager manager = new AllocateDocumentsManager(newDocFactory);

				StorageDocsUnallocated doc1 = manager.UnallocatedDocuments.AddNew();
				doc1.SC_ImageData = testImage;
				doc1.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc1.SC_Desc = "Misc";
				doc1.SC_ParentID = org1.PK;

				StorageDocsUnallocated doc2 = manager.UnallocatedDocuments.AddNew();
				doc2.SC_ImageData = testImage;
				doc2.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				doc2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				doc2.SC_Desc = "Misc";
				doc2.SC_ParentID = org1.PK;

				AssertEquals("UnallocatedDocuments.Count", 2, manager.UnallocatedDocuments.Count);
				manager.AllocateDocuments();
				AssertEquals("AllocatedDocuments.Count", 2, manager.AllocatedDocuments.Count);

				AssertEquals("Allocated 1st Doc's ParentMain.IsDeleted", false, manager.AllocatedDocuments[0].ParentMain.IsDeleted);
				AssertEquals("Allocated 2nd Doc's ParentMain.IsDeleted", false, manager.AllocatedDocuments[1].ParentMain.IsDeleted);

				AssertEquals("1st doc ParentFK", org1.PK, manager.AllocatedDocuments[0].ParentMain.SM_ParentFK);
				AssertEquals("2nd doc ParentFK", org1.PK, manager.AllocatedDocuments[1].ParentMain.SM_ParentFK);

				AssertEquals("1st Doc StorageMainPK", manager.AllocatedDocuments[0].ParentMain.PK, savedStorageMainPK);
				AssertEquals("2nd Doc StorageMainPK", manager.AllocatedDocuments[1].ParentMain.PK, savedStorageMainPK);

				AssertEquals("1st Doc StorageMainPK", manager.AllocatedDocuments[0].ParentMain.SM_Type, Core.Constants.DocManagerCodes.Organisation);
				AssertEquals("2nd Doc StorageMainPK", manager.AllocatedDocuments[1].ParentMain.SM_Type, Core.Constants.DocManagerCodes.Organisation);
			}
		}

		public void TestDontLoadDeletedDocuments()
		{
			var pk1 = ZGuid.NewZGuid();
			AddUnallocatedStorageDocs(pk1.ToGuid(), true);
			var pk2 = ZGuid.NewZGuid();
			AddUnallocatedStorageDocs(pk2.ToGuid(), false);

			var testDocs = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory { RefreshEnabled = false }).Load<StorageDocs>(new ZQuery { OrderBy = StorageDocsSchema.Constants.SC_IsDeleted });
			AssertEquals(2, testDocs.Length);
			AssertEquals(pk2, testDocs[0].PK);
			AssertEquals(pk1, testDocs[1].PK);

			var manager = new AllocateDocumentsManager(new DocumentFactoryProvider().GetFactory(Factory));

			AssertEquals("Should contain deleted document in unallocated collection", 2, manager.UnallocatedDocuments.Count);
			AssertEquals("Should contain deleted document", pk1, manager.UnallocatedDocuments[0].PK);

			AssertEquals("Should not contain deleted document in view until All Deleted checkbox is checked", 1, manager.UnallocatedDocumentsView.Count);
			AssertEquals("Should only contain undeleted document in view", pk2, manager.UnallocatedDocumentsView[0].PK);
		}

		void AddUnallocatedStorageDocs(Guid pk, bool isDeleted)
		{
			using (var command = TestConnection.Command(
				string.Format(
					"insert into dbo.StorageDocs(SC_PK, SC_DataType, SC_Date, SC_ImageData, SC_SM, SC_IsDeleted, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) values('{0}', 'ORG', getdate(), 0x01, '00000000-0000-0000-0000-000000000000', {1}, getdate(), getdate())",
					pk, isDeleted ? "'Y'" : "'N'")))
			{
				command.ExecuteNonQuery();
			}
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DocManagerDBHelperTestClass dBHelper = new DocManagerDBHelperTestClass();
			if (!dBHelper.DatabaseExists(1))
			{
				dBHelper.CreateDatabase(1);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable((new DocManagerDBHelper()).GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));
		}
	}
}
