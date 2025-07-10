using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class SerializableEDocCollectionTest : TestCaseWithDocumentFactory
	{
		public void TestConstructor()
		{
			StorageDocs doc1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			StorageDocs doc2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			BusinessObject[] bizOs = new BusinessObject[] { doc1, doc2 };

			SerializableEDocCollection collection = SerializableEDocCollection.New(bizOs);
			AssertEquals("Collection should have two elements", 2, collection.Count);
			AssertEquals("Collection should contain SerializableEDocs populated with the BizOs passed in", doc1.PK, ((SerializableEDoc)collection.ExposedItems[0]).PK);
			AssertEquals("Collection should contain SerializableEDocs populated with the BizOs passed in", doc2.PK, ((SerializableEDoc)collection.ExposedItems[1]).PK);
		}

		public void TestToBusinessObjectsWithParent()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			var doc1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			doc1.SC_Desc = "This is a test";
			doc1.SC_ImageData =  new byte[] { 1, 2, 3 };

			var doc2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			doc2.SC_Desc = "This is another test";
			doc2.SC_ImageData = new byte[] { 1, 2, 3 };

			var bizOs = new BusinessObject[] { doc1, doc2 };

			var collection = SerializableEDocCollection.New(bizOs);
			AssertEquals("Collection should have two elements", 2, collection.Count);

			var differentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var anotherParent = differentFactory.New<StorageMain>();
			var restoredDocs = collection.ToBusinessObjects(anotherParent);
			AssertEquals("RestoredDocs count should be 2", 2, restoredDocs.Length);
			AssertEquals("Docs should have the parent passed into the ToBusinessObjects method", anotherParent, restoredDocs[0].ParentMain);
			AssertEquals("Docs should have the parent passed into the ToBusinessObjects method", anotherParent, restoredDocs[1].ParentMain);
		}

		public void TestAdd()
		{
			SerializableEDocCollection collection = new SerializableEDocCollection();
			AssertEquals("Count before add", 0, collection.ExposedItems.Count);
			collection.Add(new SerializableEDoc(StorageDocs.New_DEBUG(MasterFactory)));
			AssertEquals("Count after add", 1, collection.ExposedItems.Count);
		}

		public void TestRemove()
		{
			SerializableEDocCollection collection = new SerializableEDocCollection();
			SerializableEDoc doc = new SerializableEDoc(StorageDocs.New_DEBUG(MasterFactory));
			collection.Add(doc);
			AssertEquals("Count before removal", 1, collection.ExposedItems.Count);
			collection.RemoveForTest(doc);
			AssertEquals("Count after removal", 0, collection.ExposedItems.Count);
		}

		public void TestCount()
		{
			SerializableEDocCollection collection = new SerializableEDocCollection();
			AssertEquals("Count before add", 0, collection.Count);
			collection.Add(new SerializableEDoc(StorageDocs.New_DEBUG(MasterFactory)));
			AssertEquals("Count after add", 1, collection.Count);
			collection.Add(new SerializableEDoc(StorageDocs.New_DEBUG(MasterFactory)));
			AssertEquals("Count after second add", 2, collection.Count);
		}

		public void TestNewFromBusinessObjectArray()
		{
			StorageDocs doc = StorageDocs.New_DEBUG(MasterFactory);
			StorageFile file = StorageFile.New_DEBUG(MasterFactory);

			BusinessObject[] bizOs = new BusinessObject[] { doc, file };
			SerializableEDocCollection collection = SerializableEDocCollection.New(bizOs);
			AssertEquals("Should have the same number of elements as the BizO array", bizOs.Length, collection.Count);
			AssertEquals("Order of adding", new SerializableEDoc(doc).PK, collection[0].PK);
			AssertEquals("Order of adding", new SerializableEDoc(file).PK, collection[1].PK);
		}

		public void TestGetContentsAsFiles()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var doc1ImageData = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
				var doc2ImageData = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				StorageDocs doc1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
				doc1.SC_ImageData = doc1ImageData;
				StorageDocs doc2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
				doc2.SC_ImageData = doc2ImageData;

				string[] filenames = null;

				try
				{
					SerializableEDocCollection collection = SerializableEDocCollection.New(new BusinessObject[] { doc1, doc2 });
					filenames = collection.GetContentsAsFiles();

					AssertEquals("Should have the same number of filenames generated as number of things in the collection", filenames.Length, collection.Count);
					AssertEquals("Contents of the files should be in the right order", doc1ImageData, DocumentUtilities.GetFileAsBytes(filenames[0]));
					AssertEquals("Contents of the files should be in the right order", doc2ImageData, DocumentUtilities.GetFileAsBytes(filenames[1]));
				}
				finally
				{
					SerializableEDocCollection.DisposeFiles(filenames);
				}
			}
		}

		public void TestContainsPK()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			SerializableEDocCollection collection = new SerializableEDocCollection();
			AssertEquals("should not contain pk", false, collection.ContainsPK(doc.PK));

			SerializableEDoc eDoc = new SerializableEDoc(doc);
			collection.Add(eDoc);
			AssertEquals("should contain pk", true, collection.ContainsPK(doc.PK));
		}

		public void TestContainsSystemGeneratedDocuments()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_IsSystemGenerated = true;

			SerializableEDocCollection collection = new SerializableEDocCollection();
			AssertEquals("should not contain system generated documents", false, collection.ContainsSystemGeneratedDocuments);

			SerializableEDoc eDoc = new SerializableEDoc(doc);
			collection.Add(eDoc);
			AssertEquals("should contain system generated documents", true, collection.ContainsSystemGeneratedDocuments);

			collection.RemoveForTest(eDoc);

			doc.SC_IsSystemGenerated = false;
			eDoc = new SerializableEDoc(doc);
			collection.Add(eDoc);
			AssertEquals("should not contain system generated documents", false, collection.ContainsSystemGeneratedDocuments);
		}

		public void TestContainsTifOnlyFiles()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			SerializableEDocCollection collection = new SerializableEDocCollection();
			SerializableEDoc eDoc = new SerializableEDoc(doc);
			collection.Add(eDoc);
			AssertEquals("Should contain only tif files", true, collection.ContainsOnlyImageFiles);

			collection.RemoveForTest(eDoc);

			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			file.SC_DataType = "PDF";
			eDoc = new SerializableEDoc(file);
			collection.Add(eDoc);

			AssertEquals("Should return false for contains only tif files - there is a pdf in the collection", false, collection.ContainsOnlyImageFiles);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteTempFiles();
		}
	}
}
