using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsDependentCollectionBase))]
	sealed class StorageDocsDependentCollectionBaseTest : BusinessObjectCollectionTestCase
	{
		public void TestSetImageDataForAll()
		{
			StorageFile file1 = Parent.Files.AddNew();
			file1.SC_FileName = "hello1";
			file1.SC_DataType = "PDF";
			file1.SaveToTempFile(); // sets the temp file string
			file1.IsSC_ImageDataOutOfSync = true;

			StorageFile file2 = Parent.Files.AddNew();
			file2.SC_FileName = "hello2";
			file2.SC_DataType = "PDF";
			file2.SaveToTempFile(); // sets the temp file string
			file2.IsSC_ImageDataOutOfSync = true;

			StorageDocs doc1 = Parent.Documents.AddNew();
			doc1.SC_FileName = "hello3";
			doc1.SC_DataType = "TIF";
			doc1.SaveToTempFile(); // sets the temp file string

			StorageDocs doc2 = Parent.Documents.AddNew();
			doc2.SC_FileName = "hello4";
			doc2.SC_DataType = "JPG";
			doc2.SaveToTempFile(); // sets the temp file string
			doc2.WasOpenInExternalEditor = true;
			doc2.IsSC_ImageDataOutOfSync = false;

			StorageDocs doc3 = Parent.Documents.AddNew();
			doc3.SC_FileName = "hello5";
			doc3.SC_DataType = "JPG";
			doc3.SaveToTempFile(); // sets the temp file string
			doc3.WasOpenInExternalEditor = true;
			doc3.IsSC_ImageDataOutOfSync = true;

			try
			{
				AssertEquals("Precondition: SC_ImageData should be empty", true, file1.SC_ImageData.IsEmpty);
				AssertEquals("Precondition: SC_ImageData should be empty", true, file2.SC_ImageData.IsEmpty);
				AssertEquals("Precondition: SC_ImageData should be empty", true, doc1.SC_ImageData.IsEmpty);
				AssertEquals("Precondition: SC_ImageData should be empty", true, doc2.SC_ImageData.IsEmpty);

				string file1TempFile = file1.TempFileName;
				string file2TempFile = file2.TempFileName;

				string doc1TempFile = doc1.TempFileName;
				string doc2TempFile = doc2.TempFileName;
				string doc3TempFile = doc3.TempFileName;

				var samplePdfTestFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				File.Copy(samplePdfTestFile, file1TempFile, true);
				File.Copy(samplePdfTestFile, file2TempFile, true);
				File.SetAttributes(file1TempFile, FileAttributes.Normal);
				File.SetAttributes(file2TempFile, FileAttributes.Normal);

				var smallTifTestFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				File.Copy(smallTifTestFile, doc1TempFile, true);
				File.Copy(smallTifTestFile, doc2TempFile, true);
				File.Copy(smallTifTestFile, doc3TempFile, true);
				File.SetAttributes(doc1TempFile, FileAttributes.Normal);
				File.SetAttributes(doc2TempFile, FileAttributes.Normal);
				File.SetAttributes(doc3TempFile, FileAttributes.Normal);

				Parent.eDocs.SetImageDataForAllIfRequired();

				Assert("After SetImageDataForAll called, SC_ImageData should be populated", file1.SC_ImageData.Length > 0);
				Assert("After SetImageDataForAll is called, the temp file path should be empty", file1.TempFileName.IsEmpty);
				Assert("After SetImageDataForAll is called, the temp file should not longer exist", !File.Exists(file1TempFile));

				Assert("After SetImageDataForAll called, SC_ImageData should be populated", file2.SC_ImageData.Length > 0);
				Assert("After SetImageDataForAll is called, the temp file path should be empty", file2.TempFileName.IsEmpty);
				Assert("After SetImageDataForAll is called, the temp file should not longer exist", !File.Exists(file2TempFile));

				Assert("After SetImageDataForAll called, SC_ImageData should not be populated as doc has WasOpenInExternalEditor is false and has no changes", doc1.SC_ImageData.Length == 0);
				Assert("After SetImageDataForAll is called, the temp file path should not be empty - SC_ImageData wasn't set", !doc1.TempFileName.IsEmpty);
				Assert("After SetImageDataForAll is called, the temp file should exist - SC_ImageData wasn't set", File.Exists(doc1TempFile));

				Assert("After SetImageDataForAll called, SC_ImageData should not be populated as doc has no changes", doc2.SC_ImageData.Length == 0);
				Assert("After SetImageDataForAll is called, the temp file path should not be empty - SC_ImageData wasn't set", !doc2.TempFileName.IsEmpty);
				Assert("After SetImageDataForAll is called, the temp file should exist - SC_ImageData wasn't set", File.Exists(doc2TempFile));

				Assert("After SetImageDataForAll called, SC_ImageData should be populated", doc3.SC_ImageData.Length > 0);
				Assert("After SetImageDataForAll is called, the temp file path should be empty", doc3.TempFileName.IsEmpty);
				Assert("After SetImageDataForAll is called, the temp file should not longer exist", !File.Exists(doc3TempFile));
			}
			finally
			{
				file1.Dispose();
				file2.Dispose();
				doc1.Dispose();
				doc2.Dispose();
				doc3.Dispose();
			}
		}

		public void TestDisposeAll()
		{
			StorageDocs file1 = Parent.Documents.AddNew();
			file1.SC_FileName = "Test";
			file1.SC_DataType = "TIF";
			file1.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
			file1.SaveToTempFile();

			StorageFile file2 = Parent.Files.AddNew();
			file2.SC_FileName = "Sample";
			file2.SC_DataType = "PDF";
			file2.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			file2.SaveToTempFile();

			string file1TempFile = file1.TempFileName;
			string file2TempFile = file2.TempFileName;

			AssertEquals("Files should exist on filesystem", true, File.Exists(file1.TempFileName));
			AssertEquals("Files should exist on filesystem", true, File.Exists(file2.TempFileName));

			file1.Dispose();
			file2.Dispose();

			AssertEquals("Files should have been cleaned up on dispose", false, File.Exists(file1TempFile));
			AssertEquals("Files should have been cleaned up on dispose", false, File.Exists(file2TempFile));
			AssertEquals("Temp file name should be blank", true, file1.TempFileName.IsEmpty);
			AssertEquals("Temp file name should be blank", true, file2.TempFileName.IsEmpty);
		}

		public void TestSetDefaultsForNewChild()
		{
			ZGuid guid = ZGuid.NewZGuid();
			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_ParentFK = guid;
			StorageDocs newElement = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));

			AssertNotNull("A new document should have been added", newElement);
			AssertEquals("form category should be set on the doc", Core.Constants.DocManagerCodes.Organisation, newElement.ParentMain.SM_Type);
			AssertEquals("Owner Pk should be set on the doc", guid, newElement.ParentMain.SM_ParentFK);
		}

		public override void TestAddNew()
		{
			Assert("not required here", true);
		}

		#region IStorageDocsBaseCollection Members

		public void TestGetMostRecentEDoc()
		{
			StorageDocs systemGeneratedEDoc = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, false, false);
			StorageDocs nonSystemGeneratedEDoc = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, false, false);
			StorageDocs earlierEDoc = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 9, 1), new ZBlob(), false, false, false);
			AssertEquals("GetMostRecentEDoc()", systemGeneratedEDoc, ((IStorageDocsBaseCollection)Parent.eDocs).GetMostRecentEDoc("ABC"));
		}

		public void TestGetMostRecentEDoc_NotDeleted()
		{
			StorageDocs deletedFirst = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			StorageDocs deletedSecond = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, true, false);
			StorageDocs deletedThird = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 9, 1), new ZBlob(), false, true, false);

			AssertEquals("GetMostRecentEDoc will not return any documents that are deleted.", null, ((IStorageDocsBaseCollection)Parent.eDocs).GetMostRecentEDoc("ABC"));

			StorageDocs notDeletedSecond = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, false, false);
			StorageDocs notDeletedThird = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 9, 1), new ZBlob(), false, false, false);

			AssertEquals("GetMostRecentEDoc will return the latest date of all docs not deleted", notDeletedSecond, ((IStorageDocsBaseCollection)Parent.eDocs).GetMostRecentEDoc("ABC"));
		}

		public void TestGetFromUniqueKey()
		{
			StorageDocs eDoc1 = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, false, false);
			StorageDocs eDoc2 = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, false, false);
			AssertEquals(eDoc2.PK, ((IStorageDocsBaseCollection)Parent.eDocs).GetFromUniqueKey(eDoc2.PK.ToGuid()).UniqueKey);
			AssertEquals(eDoc1.PK, ((IStorageDocsBaseCollection)Parent.eDocs).GetFromUniqueKey(eDoc1.PK.ToGuid()).UniqueKey);
		}

		public void TestAdd_IStorageDocsBaseCollection()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;

			collection.Add(document);
			AssertEquals("There should be 1 document added to the collection", 1, collection.Count);
		}

		public void TestRemove_IStorageDocsBaseCollection()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;

			collection.Add(document);
			AssertEquals("There should be 1 document added to the collection", 1, collection.Count);

			collection.Remove(document);
			AssertEquals("The document should be removed", 0, collection.Count);
		}

		public void TestContains()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;
			collection.Add(document);
			AssertEquals("Collection now contains the document", true, collection.Contains(document));
		}

		public void TestIndexer()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;

			collection.Add(document);
			AssertNotNull(collection[0]);
		}

		public void TestContainsDocType()
		{
			var eDoc = Parent.Files.AddNew();
			eDoc.SC_DocType = "ABC";
			AssertEquals(true, ((IStorageDocsBaseCollection)Parent.Files).ContainsDocType("ABC"));

			eDoc.SC_IsDeleted = true;
			AssertEquals(false, ((IStorageDocsBaseCollection)Parent.Files).ContainsDocType("ABC"));
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		StorageMain Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = MasterFactory.New<StorageMain>();
				}
				return fParent;
			}
		}
		StorageMain fParent;

		StorageDocs CreateNewDocument(StorageMain parent, string docType, ZDateTime lastEdited, ZBlob imageData, bool isSystemGenerated, bool isDeleted, bool isPublished)
		{
			StorageDocs document = parent.Documents.AddNew();
			document.SC_DocType = docType;
			document.SC_Date = lastEdited;
			document.SC_ImageData = imageData;
			document.SC_IsSystemGenerated = isSystemGenerated;
			document.SC_IsDeleted = isDeleted;
			document.SC_IsPublished = isPublished;
			return document;
		}

		DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Parent.eDocs;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return StorageDocs.New_DEBUG(MasterFactory);
		}

		#endregion
	}
}
