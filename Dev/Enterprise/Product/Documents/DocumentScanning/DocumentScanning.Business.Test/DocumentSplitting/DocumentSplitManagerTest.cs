using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.PdfiumWrapper;
using CargoWise.Types;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.TestHelpers;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DocumentSplitManager))]
	sealed class DocumentSplitManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateAll()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory.GetFactory(1));
			doc.SC_FileName = "Test";
			doc.SC_ImageData = new ZBlob(TifFileHavingSixPages);
			doc.SC_DataType = "TIF";
			doc.SC_DocType = "ACV";

			var splitManager = new DocumentSplitManager(doc);
			splitManager.DocumentToSplit.SC_FileName = "Test";
			splitManager.MaxSizeInMb = 1;

			var splitInfo = splitManager.DocumentSplitConfigCollection.AddNew();
			splitInfo.DocumentName = "Test";
			splitInfo.StartPage = 1;
			splitInfo.EndPage = 2;
			splitInfo.AppendPageNumber = false;

			splitManager.ValidateAll();
			AssertEquals("DocumentSplitManager should have no error", expected: false, splitManager.HasErrors);

			splitInfo.DocumentType = "123";
			splitManager.ValidateAll();

			AssertEquals("DocumentSplitManager should have errors", expected: true, splitManager.HasErrors);
			AssertEquals("Errors are from child DocumentSplitConfigCollection", expected: false, splitManager.HasErrorsNotIncludingChildren);

			splitInfo.DocumentType = "ACV";
			splitManager.MaxSizeInKb = 2147483648m;

			AssertEquals("DocumentSplitManager should have errors", expected: true, splitManager.HasErrors);
			AssertEquals("Errors are not from child, but from MaxSizeInKb", expected: true, splitManager.HasErrorsNotIncludingChildren);
		}

		public void TestMaxSizeInKBAndMaxSizeInMB_ValuesShouldNotBeNegative()
		{
			var splitManager = (DocumentSplitManager)GetNewBusinessObject();
			splitManager.MaxSizeInKb = 1024m;
			AssertEquals("MaxSizeInMb should be set automatically", 1m, splitManager.MaxSizeInMb);

			splitManager.MaxSizeInMb = 2m;
			AssertEquals("MaxSizeInKb should be set automatically", 2048m, splitManager.MaxSizeInKb);

			splitManager.MaxSizeInKb = -1m;
			AssertHasError(splitManager.MaxSizeInKbInfo, "Max size should be equal or larger than zero.");
			AssertHasError(splitManager.MaxSizeInMbInfo, "Max size should be equal or larger than zero.");
		}

		public void TestMaxSizeInKBAndMaxSizeInMB_ValuesShouldNotExceedMaxValueOfInteger()
		{
			var splitManager = (DocumentSplitManager)GetNewBusinessObject();
			splitManager.MaxSizeInKb = 1024m;
			AssertEquals("MaxSizeInMb should be set automatically", 1m, splitManager.MaxSizeInMb);

			splitManager.MaxSizeInMb = 2m;
			AssertEquals("MaxSizeInKb should be set automatically", 2048m, splitManager.MaxSizeInKb);

			splitManager.MaxSizeInKb = 2147483648m;
			AssertHasError(splitManager.MaxSizeInKbInfo, "Max size should be equal or less than 2147483647.");

			splitManager.MaxSizeInMb = 2147483648m;
			AssertHasError(splitManager.MaxSizeInKbInfo, "Max size should be equal or less than 2147483647.");
			AssertHasError(splitManager.MaxSizeInMbInfo, "Max size should be equal or less than 2147483647.");
		}

		public void TestSplitLoggingForPDFTypeAndOrgHeaderOwner()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var org = masterFactory.NewWithValidTestData<OrgHeader>(); //storage main requires parent. If you want another one - reassign.

			var sourceDocument = masterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			sourceDocument.ParentMain.SM_ParentFK = org.PK;
			sourceDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			sourceDocument.ParentMain.SM_DB = 1;
			sourceDocument.SC_DataType = Core.Constants.FileFormats.PDF;
			sourceDocument.SC_Desc = "document description";
			var splitManager = new DocumentSplitManager(sourceDocument);

			var doc = (StorageDocsBase)splitManager.CollectionToAddTo.AddNew(sourceDocument.GetType());

			doc.SC_FileName = "TestFileName";
			doc.SC_DocType = "MSC";
			doc.SC_IsPublished = false;
			doc.SC_DataType = sourceDocument.SC_DataType;
			doc.SC_Desc = sourceDocument.SC_Desc;

			splitManager.LogSplit(doc);

			var ownerLogItem = org.Logs.MostRecentLogByEventTime(Events.DocumentImported);
			AssertNotNull("Owner Log", ownerLogItem);
			AssertEquals(ownerLogItem.SL_Reference, string.Concat(doc.SC_DocType, "|", doc.PK.ToString()));
		}

		public void TestSplitLoggingForTIFTypeAndOrgHeaderOwner()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var org = masterFactory.NewWithValidTestData<OrgHeader>(); //storage main requires parent. If you want another one - reassign.

			var sourceDocument = masterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			sourceDocument.ParentMain.SM_ParentFK = org.PK;
			sourceDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			sourceDocument.ParentMain.SM_DB = 1;
			sourceDocument.SC_DataType = Core.Constants.FileFormats.TIF;
			sourceDocument.SC_Desc = "document description";
			var splitManager = new DocumentSplitManager(sourceDocument);

			var doc = (StorageDocsBase)splitManager.CollectionToAddTo.AddNew(sourceDocument.GetType());

			doc.SC_FileName = "TestFileName";
			doc.SC_DocType = "MSC";
			doc.SC_IsPublished = false;
			doc.SC_DataType = sourceDocument.SC_DataType;
			doc.SC_Desc = sourceDocument.SC_Desc;

			splitManager.LogSplit(doc);

			var ownerLogItem = org.Logs.MostRecentLogByEventTime(Events.DocumentImported);
			AssertNotNull("Owner Log", ownerLogItem);
			AssertEquals(ownerLogItem.SL_Reference, string.Concat(doc.SC_DocType, "|", doc.PK.ToString()));
		}

		public void TestSplitLoggingForPDFTypeAndSupplierPartOwner()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var part = masterFactory.NewWithValidTestData<OrgSupplierPart>(); //storage main requires parent. If you want another one - reassign.

			var sourceDocument = masterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			sourceDocument.ParentMain.SM_ParentFK = part.PK;
			sourceDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Product;
			sourceDocument.ParentMain.SM_DB = 1;
			sourceDocument.SC_DataType = Core.Constants.FileFormats.PDF;
			sourceDocument.SC_Desc = "document description";
			var splitManager = new DocumentSplitManager(sourceDocument);

			var doc = (StorageDocsBase)splitManager.CollectionToAddTo.AddNew(sourceDocument.GetType());

			doc.SC_FileName = "TestFileName";
			doc.SC_DocType = "MSC";
			doc.SC_IsPublished = false;
			doc.SC_DataType = sourceDocument.SC_DataType;
			doc.SC_Desc = sourceDocument.SC_Desc;

			splitManager.LogSplit(doc);

			var ownerLogItem = part.Logs.MostRecentLogByEventTime(Events.DocumentImported);
			AssertNotNull("Owner Log", ownerLogItem);
			AssertEquals(ownerLogItem.SL_Reference, string.Concat(doc.SC_DocType, "|", doc.PK.ToString()));
		}

		public void TestSplitLoggingForPDFTypeAndNotAllocated()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var unallocatedDocument = masterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			unallocatedDocument.SC_DataType = Core.Constants.FileFormats.PDF;
			unallocatedDocument.SC_Desc = "document description";

			var docList = new StorageDocsCollection(masterFactory);
			var splitManager = new DocumentSplitManager(unallocatedDocument, docList);

			var doc = (StorageDocsBase)splitManager.CollectionToAddTo.AddNew(unallocatedDocument.GetType());
			doc.SC_FileName = "TestFileName";
			doc.SC_DocType = "MSC";
			doc.SC_IsPublished = false;
			doc.SC_DataType = unallocatedDocument.SC_DataType;
			doc.SC_Desc = unallocatedDocument.SC_Desc;

			AssertNull("Parent Main is null", doc.ParentMain);

			AssertNoExceptionThrown(() => { splitManager.LogSplit(doc); });
		}

		[ExpectNoExceptions]
		public void TestParentFileNameIsDuplicated()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_FileName = "Test";
			var splitManager = new DocumentSplitManager(doc, doc.ParentMain.eDocs);

			var doc1 = (StorageDocs)doc.ParentMain.eDocs.AddNew(typeof(StorageDocs));
			doc1.SC_FileName = "Test";

			var usedDocumentNames = splitManager.GetUsedDocumentNamesInEDocs();
			AssertEquals("Should contain one element", 1, usedDocumentNames.Count);
			Assert("Document name", usedDocumentNames.Contains("Test"));
		}

		public void TestSplitDocumentName()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory.GetFactory(1));
			doc.SC_FileName = "Test";
			doc.SC_ImageData = new ZBlob(TifFileHavingSixPages);
			doc.SC_DataType = "TIF";
			var splitManager = new DocumentSplitManager(doc);

			splitManager.DocumentToSplit.SC_FileName = "Test";
			splitManager.DocumentSplitConfigCollection.SuspendValidation();
			var splitInfo = splitManager.DocumentSplitConfigCollection.AddNew();
			splitInfo.DocumentName = "Test";
			splitInfo.DocumentType = "AIN";
			splitInfo.StartPage = 1;
			splitInfo.EndPage = 2;
			splitInfo.AppendPageNumber = false;

			var splitInfo1 = splitManager.DocumentSplitConfigCollection.AddNew();
			splitInfo1.DocumentName = "Test1";
			splitInfo1.DocumentType = "AIN";
			splitInfo1.StartPage = 1;
			splitInfo1.EndPage = 2;
			splitInfo1.AppendPageNumber = true;

			var splitInfo2 = splitManager.DocumentSplitConfigCollection.AddNew();
			splitInfo2.DocumentName = "Test2";
			splitInfo2.DocumentType = "AIN";
			splitInfo2.StartPage = 2;
			splitInfo2.EndPage = 2;
			splitInfo2.AppendPageNumber = true;

			var splitInfo3 = splitManager.DocumentSplitConfigCollection.AddNew();
			splitInfo3.DocumentName = "Test2";
			splitInfo3.DocumentType = "AIN";
			splitInfo3.StartPage = 2;
			splitInfo3.EndPage = 2;
			splitInfo3.AppendPageNumber = true;

			var splitInfo4 = splitManager.DocumentSplitConfigCollection.AddNew();
			splitInfo4.DocumentName = "Test2";
			splitInfo4.DocumentType = "AIN";
			splitInfo4.StartPage = 3;
			splitInfo4.EndPage = 3;
			splitInfo4.AppendPageNumber = true;

			splitManager.MaxSizeInKb = 0;
			splitManager.SplitDocument();
			AssertEquals("Document name should be unique and don't contains page info after split", "Test[1]", splitManager.DocumentSplitResultCollection[0].DocumentName);
			AssertEquals("Document name should be unique and contains page info after split", "Test1 (Page 1 - 2)", splitManager.DocumentSplitResultCollection[1].DocumentName);
			AssertEquals("Document name should be unique and contains page info after split", "Test2 (Page 1)", splitManager.DocumentSplitResultCollection[2].DocumentName);
			AssertEquals("Document name should be unique and contains page info after split", "Test2 (Page 2)", splitManager.DocumentSplitResultCollection[3].DocumentName);
			AssertEquals("Document name should be unique and contains page info after split", "Test2 (Page 3)", splitManager.DocumentSplitResultCollection[4].DocumentName);
		}

		public void TestSplitDocumentByPageRange()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory.GetFactory(1));
			doc.SC_FileName = "Test";
			doc.SC_ImageData = new ZBlob(TifFileHavingSixPages);
			doc.SC_DataType = Core.Constants.FileFormats.TIF;
			var splitManager = new DocumentSplitManager(doc);

			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			splitInfoCollection.SuspendValidation();
			var splitInfo = splitInfoCollection.AddNew();
			splitInfo.DocumentName = "Test";
			splitInfo.DocumentType = "AIN";
			splitInfo.StartPage = 1;
			splitInfo.EndPage = 6;

			var splitInfo1 = splitInfoCollection.AddNew();
			splitInfo1.DocumentName = "Test1";
			splitInfo1.DocumentType = "AIN";
			splitInfo1.StartPage = 1;
			splitInfo1.EndPage = 3;

			var splitInfo2 = splitInfoCollection.AddNew();
			splitInfo2.DocumentName = "Test2";
			splitInfo2.DocumentType = "AIN";
			splitInfo2.StartPage = 4;
			splitInfo2.EndPage = 6;

			splitManager.MaxSizeInKb = 0;
			splitManager.SplitDocument();
			AssertEquals("Document should be splited into several documents", 3, splitManager.DocumentSplitResultCollection.Count);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 6, splitManager.DocumentSplitResultCollection[0].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 3, splitManager.DocumentSplitResultCollection[1].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 3, splitManager.DocumentSplitResultCollection[2].DocumentData);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSplitDocumentWithPdfiumException()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory.GetFactory(1));
			var corrupt2ndPagePath = AssetsHelper.FetchTestAsset("Architecture/content/DocumentScanning/TestFiles/Corrupt2ndPage.pdf");
			var corrupt2ndPageBytes = File.ReadAllBytes(corrupt2ndPagePath);
			doc.SC_FileName = "Test";
			doc.SC_ImageData = new ZBlob(corrupt2ndPageBytes);
			doc.SC_DataType = "PDF";
			var splitManager = new DocumentSplitManager(doc);
			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			splitInfoCollection.SuspendValidation();
			var splitInfo = splitInfoCollection.AddNew();
			splitInfo.DocumentName = "Test";
			splitInfo.DocumentType = "AIN";
			splitInfo.StartPage = 1;
			splitInfo.EndPage = 2;
			AssertExceptionThrown<PdfiumException>("PDF Ruturn Can not load page error", "Cannot Import Pages", () => splitManager.SplitDocument());
		}

		public void TestSplitDocumentByPageRangeAndMaxSize()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory.GetFactory(1));
			doc.SC_FileName = "Test";
			doc.SC_ImageData = new ZBlob(TifFileHavingSixPages);
			doc.SC_DataType = Core.Constants.FileFormats.TIF;
			var splitManager = new DocumentSplitManager(doc);

			var splitInfoCollection = splitManager.DocumentSplitConfigCollection;
			splitInfoCollection.SuspendValidation();
			var splitInfo = splitInfoCollection.AddNew();
			splitInfo.DocumentName = "Test";
			splitInfo.DocumentType = "AIN";
			splitInfo.StartPage = 1;
			splitInfo.EndPage = 6;
			splitManager.MaxSizeInKb = 50;

			splitManager.SplitDocument();
			AssertEquals("Document should be splited into several documents", 4, splitManager.DocumentSplitResultCollection.Count);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[0].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[1].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[2].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 3, splitManager.DocumentSplitResultCollection[3].DocumentData);

			splitManager.MaxSizeInKb = 0;
			splitManager.MaxSizeInMb = 0.01;
			splitManager.SplitDocument();
			AssertEquals("Document should be splited into several documents", 6, splitManager.DocumentSplitResultCollection.Count);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[0].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[1].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[2].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[3].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[4].DocumentData);
			AssertContainsCorrectPageNumber(Core.Constants.FileFormats.TIF, 1, splitManager.DocumentSplitResultCollection[5].DocumentData);
		}

		void AssertContainsCorrectPageNumber(string fileType, int expectedPageNumber, ZBlob documentData)
		{
			using (var previewableDocument = PreviewableDocumentHelper.GetPreviewableDocument(fileType, documentData))
			{
				AssertEquals("Document should contain correct page number", expectedPageNumber, previewableDocument.NumberOfPages);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_ImageData = new ZBlob(TifFileHavingSixPages);
			return new DocumentSplitManager(doc);
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

		byte[] TifFileHavingSixPages => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.TifFileHavingSixPages.tif");

		DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		#endregion

	}
}
