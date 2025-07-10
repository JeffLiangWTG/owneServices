using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class DocManagerDataObjectTest : TestCaseWithDocumentFactory
	{
		public void TestConstructor()
		{
			StorageDocs document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			DocManagerDataObject dataObject = new DocManagerDataObject(null, new StorageDocs[] { document });
			AssertEquals("data should be reigstered as filedrop", true, dataObject.GetDataPresent(DataFormats.FileDrop));
			AssertEquals("data should be registered as type of serialized edoc", true, dataObject.GetDataPresent(DocManagerDataObject.DataFormatType));
		}

		public void TestDataFormatType()
		{
			AssertEquals("data format type", typeof(SerializableEDocCollection).FullName, DocManagerDataObject.DataFormatType);
		}

		public void TestGetDataAsFiledrop()
		{
			var contents1 = SmallTifBytes;
			var contents2 = SamplePdfBytes;

			StorageDocs document1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document1.SC_ImageData = contents1;
			StorageDocs document2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document2.SC_ImageData = contents2;

			DocManagerDataObject dataObject = new DocManagerDataObject(null, new StorageDocs[] { document1, document2 });
			string[] fileDropContents = (string[])dataObject.GetData(DataFormats.FileDrop);
			AssertEquals("file drop contents has the same number of docs on the data object", 2, fileDropContents.Length);
			AssertEquals("Files should exist", true, File.Exists(fileDropContents[0]));
			AssertEquals("Files should exist", true, File.Exists(fileDropContents[1]));
			AssertEquals("file drop contents should be in the right order", contents1, DocumentUtilities.GetFileAsBytes(fileDropContents[0]));
			AssertEquals("file drop contents should be in the right order", contents2, DocumentUtilities.GetFileAsBytes(fileDropContents[1]));

			File.Delete(fileDropContents[0]);
			File.Delete(fileDropContents[1]);
		}

		public void TestGetDataAsSerializedDocs()
		{
			var contents1 = SmallTifBytes;
			var contents2 = SamplePdfBytes;

			StorageDocs document1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document1.SC_ImageData = contents1;
			StorageDocs document2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document2.SC_ImageData = contents2;

			DocManagerDataObject dataObject = new DocManagerDataObject(null, new StorageDocs[] { document1, document2 });
			SerializableEDocCollection retrievedCollection = (SerializableEDocCollection)dataObject.GetData(DocManagerDataObject.DataFormatType);
			AssertEquals("Collection on the way back includes the same number of docs", 2, retrievedCollection.Count);
			AssertEquals("file contents should be in the right order", contents1, DocumentUtilities.GetFileAsBytes(retrievedCollection[0].FileNameWithExtension));
			AssertEquals("file contents should be in the right order", contents2, DocumentUtilities.GetFileAsBytes(retrievedCollection[1].FileNameWithExtension));
		}

		public void TestSerializedDocsNotLoadingBlob()
		{
			var testDocPath1 = SmallTifPath;
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var masterFactory = documentFactory.GetFactory(1);

			using (StorageDocs document1 = StorageDocs.NewWithParent_DEBUG(masterFactory))
			{
				document1.SC_ImageData = DocumentUtilities.GetFileAsBytes(testDocPath1);
				document1.SaveToTempFile();
				File.Copy(testDocPath1, document1.TempFileName, true);
				File.SetAttributes(document1.TempFileName, FileAttributes.Normal);
				document1.SetImageData();
				documentFactory.Save();

				var newFactory1 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).GetFactory(1);
				var file1 = newFactory1.Load<StorageFile>(document1.PK);

				AssertEquals(true, file1.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
				SerializableEDocCollection collection = SerializableEDocCollection.New(new BusinessObject[] { file1 });
				AssertEquals(true, file1.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));

				DocManagerDataObject dataObject = new DocManagerDataObject(null, new BusinessObject[] { file1 });
				SerializableEDocCollection retrievedCollection = (SerializableEDocCollection)dataObject.GetData(DocManagerDataObject.DataFormatType);
				AssertEquals("Collection on the way back includes the same number of docs", collection.Count, retrievedCollection.Count);
				AssertEquals("file contents should be in the right order", document1.SC_ImageData, DocumentUtilities.GetFileAsBytes(retrievedCollection[0].FileNameWithExtension));
				file1.Delete();
			}
		}

		public void TestDispose()
		{
			var contents1 = SmallTifBytes;
			var contents2 = SamplePdfBytes;

			StorageDocs document1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document1.SC_ImageData = contents1;
			StorageDocs document2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document2.SC_ImageData = contents2;

			DocManagerDataObject dataObject = new DocManagerDataObject(null, new StorageDocs[] { document1, document2 });
			string[] fileDropContents = (string[])dataObject.GetData(DataFormats.FileDrop);
			AssertEquals("file drop contents has the same number of docs on the data object", 2, fileDropContents.Length);
			AssertEquals("Files should exist", true, File.Exists(fileDropContents[0]));
			AssertEquals("Files should exist", true, File.Exists(fileDropContents[1]));

			SerializableEDocCollection collection = (SerializableEDocCollection)dataObject.GetData(DocManagerDataObject.DataFormatType);
			AssertEquals("Files is still in disk", true, File.Exists(fileDropContents[0]));
			AssertEquals("Files is still in disk", true, File.Exists(fileDropContents[1]));
		}

		public void TestDataOnClipboardAfterDocumentIsDisposed()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SetSC_ImageDataSource(new FileStreamSource(SmallTifPath));
			document.SaveToTempFile();
			var dataObject = new DocManagerDataObject(null, new StorageDocs[] { document });
			document.Dispose();
			var fileDropContents = (string[])dataObject.GetData(DataFormats.FileDrop);
			AssertFileSameAsBytes(fileDropContents[0], document.SC_ImageData);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteTempFiles();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly));

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

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

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
	}
}
