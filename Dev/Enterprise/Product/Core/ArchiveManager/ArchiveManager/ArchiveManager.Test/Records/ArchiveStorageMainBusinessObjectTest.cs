using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(ArchiveStorageMain))]
	sealed class ArchiveStorageMainBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObjectFactory NewFactory()
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		public override void TestCloneAuditProperties()
		{
			// not required.
			Assert(true);
		}

		public override void TestCloneAuditContextProperties()
		{
			// not required.
			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestArchiveThenRestore()
		{
			var imageBytes = TestFileHelper.File1MB;

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain1 = docFactory.New<ArchiveStorageMain>();
			storageMain1.SM_DB = 1;

			var storageDoc1 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "testHenry1", DocumentType = "MSC" });
			storageDoc1.SC_Desc = "test 1";
			storageDoc1.SC_DataType = "TIF";

			var storageDoc2 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "testHenry2", DocumentType = "AGI" });
			storageDoc2.SC_Desc = "test 2";
			storageDoc2.SC_DataType = "PDF";

			var storageDoc3 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "testHenry3", DocumentType = "ARV" });
			storageDoc3.SC_Desc = "test 3";
			storageDoc3.SC_DataType = "PDF";

			docFactory.Save();

			var docFactory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			docFactory2.RefreshEnabled = false;
			var storageMain1Copy = docFactory.Load<ArchiveStorageMain>(storageMain1.PK);
			AssertEquals("edocs count", 3, storageMain1Copy.eDocs.Count);

			using var directory = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var volume = ArchiveVolume.CreateNew(directory.DirectoryName, 50, logger, descriptor);
			_ = Directory.CreateDirectory(volume.VolumePath);

			storageMain1.ArchiveTo(volume);
			volume.Close();

			AssertEquals("file name ", expected: true, volume.HasEntry(storageDoc1.GetFileNameOnlyWithExtension()));
			AssertEquals("file name ", expected: true, volume.HasEntry(storageDoc2.GetFileNameOnlyWithExtension()));
			AssertEquals("file name ", expected: true, volume.HasEntry(storageDoc3.GetFileNameOnlyWithExtension()));

			storageDoc1.Delete();
			storageDoc2.Delete();
			storageDoc3.Delete();
			docFactory.Save();

			var storageMain1Reloaded = docFactory.Load<ArchiveStorageMain>(storageMain1.PK);
			AssertEquals("edocs count", 0, storageMain1Reloaded.eDocs.Count);

			storageMain1Reloaded.RestoreFrom(volume);
			AssertEquals("edocs count", 3, storageMain1Reloaded.eDocs.Count);

			foreach (var loadedDoc in storageMain1Reloaded.eDocs.Cast<StorageDocsBase>())
			{
				StorageDocsBase expectedDoc = null;
				foreach (var expectedCopy in storageMain1Copy.eDocs.Cast<StorageDocsBase>())
				{
					if (loadedDoc.SC_FileName == expectedCopy.SC_FileName)
					{
						expectedDoc = expectedCopy;
						break;
					}
				}

				AssertNotNull("expectedDoc", expectedDoc);
				AssertEquals("FileName", expectedDoc.SC_FileName, loadedDoc.SC_FileName);
				AssertEquals("DataType", expectedDoc.SC_DataType, loadedDoc.SC_DataType);
				AssertEquals("Date", expectedDoc.SC_Date, loadedDoc.SC_Date);
				AssertEquals("Desc", expectedDoc.SC_Desc, loadedDoc.SC_Desc);
				AssertEquals("DocType", expectedDoc.SC_DocType, loadedDoc.SC_DocType);
				Assert("image1 equal", Utilities.IsByteArrayEqual(expectedDoc.SC_ImageData, storageMain1Reloaded.eDocs[0].SC_ImageData));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestArchiveThenRestoreWithDuplicateFileNames()
		{
			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			var storageMain1 = docFactory.New<ArchiveStorageMain>();
			storageMain1.SM_DB = 1;

			var file1Blob = TestFileHelper.File1MB;
			var storageDocOnStorageMain1 = storageMain1.AddFileOrDocument(file1Blob, new AddFileOrDocumentDto { FileName = "test", DocumentType = "MSC" });
			storageDocOnStorageMain1.SC_Desc = "test 1";
			storageDocOnStorageMain1.SC_DataType = Core.Constants.FileFormats.TIF;

			var storageMain2 = docFactory.New<ArchiveStorageMain>();
			storageMain2.SM_DB = 1;
			storageMain2.SM_ParentFK = ZGuid.NewZGuid();

			var file2Blob = TestFileHelper.File200KB;
			var storageDocOnStorageMain2 = storageMain2.AddFileOrDocument(file2Blob, new AddFileOrDocumentDto { FileName = "test", DocumentType = "MSC" });
			storageDocOnStorageMain2.SC_Desc = "test 2";
			storageDocOnStorageMain2.SC_DataType = Core.Constants.FileFormats.TIF;

			using var directory = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var volume = ArchiveVolume.CreateNew(directory.DirectoryName, 50, logger, descriptor);
			_ = Directory.CreateDirectory(volume.VolumePath);

			storageMain1.ArchiveTo(volume);
			storageMain2.ArchiveTo(volume);
			volume.Close();

			CombineAssertions("Precondition: Files have been archived offline", () =>
			{
				Assert("File without duplicate tag should exist", volume.HasEntry("test.tif"));
				Assert("File with duplicate tag should exist", volume.HasEntry("test(1).tif"));
			});

			storageDocOnStorageMain1.Delete();
			storageDocOnStorageMain2.Delete();
			docFactory.Save();

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain1Reloaded = newFactory.Load<ArchiveStorageMain>(storageMain1.PK);
			var storageMain2Reloaded = newFactory.Load<ArchiveStorageMain>(storageMain2.PK);

			CombineAssertions("Precondition: There are no files before restore", () =>
			{
				AssertEquals("StorageMain1", 0, storageMain1Reloaded.eDocs.Count);
				AssertEquals("StorageMain2", 0, storageMain2Reloaded.eDocs.Count);
			});

			storageMain1Reloaded.RestoreFrom(volume);
			storageMain2Reloaded.RestoreFrom(volume);

			var storageDoc1Restored = storageMain1Reloaded.eDocs.Cast<StorageDocsBase>().FirstOrDefault();
			var storageDoc2Restored = storageMain2Reloaded.eDocs.Cast<StorageDocsBase>().FirstOrDefault();

			CombineAssertions("File names are correct", () =>
			{
				AssertEquals("First file name should not have been changed", "test", storageDoc1Restored.SC_FileName);
				AssertEquals("Second file name should have been changed as there is a duplicate", "test(1)", storageDoc2Restored.SC_FileName);
			});

			CombineAssertions("Contents are correct", () =>
			{
				Assert("File should be the 1MB file", Utilities.IsByteArrayEqual(file1Blob, storageDoc1Restored.SC_ImageData));
				Assert("File 2 is compressed, but shouldn't be equal to the first", !Utilities.IsByteArrayEqual(file1Blob, storageDoc2Restored.SC_ImageData));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReStoreFromBLBDocuments()
		{
			var imageBytes = TestFileHelper.File1MB;

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain1 = docFactory.New<ArchiveStorageMain>();
			storageMain1.SM_DB = 1;

			var storageDoc1 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "testHenry1", DocumentType = "MSC" });
			storageDoc1.SC_Desc = "test 1";
			storageDoc1.SC_DataType = "TIF";

			var storageDoc2 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "testHenry2", DocumentType = "AGI" });
			storageDoc2.SC_Desc = "test 2";
			storageDoc2.SC_DataType = "PDF";

			var storageDoc3 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "testHenry3", DocumentType = "ARV" });
			storageDoc3.SC_Desc = "test 3";
			storageDoc3.SC_DataType = "PDF";

			docFactory.Save();

			var docFactory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			docFactory2.RefreshEnabled = false;
			var storageMain1Copy = docFactory.Load<ArchiveStorageMain>(storageMain1.PK);
			AssertEquals("edocs count", 3, storageMain1Copy.eDocs.Count);

			using var directory = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var volume = ArchiveVolume.CreateNew(directory.DirectoryName, 50, logger, descriptor);
			_ = Directory.CreateDirectory(volume.VolumePath);

			ArchiveToVolumeWithBLBFileForTest(volume, storageMain1);
			volume.Close();

			AssertEquals("file name ", expected: true, volume.HasEntry(storageMain1.DocBlobFileName(storageDoc1)));
			AssertEquals("file name ", expected: true, volume.HasEntry(storageMain1.DocBlobFileName(storageDoc2)));
			AssertEquals("file name ", expected: true, volume.HasEntry(storageMain1.DocBlobFileName(storageDoc3)));

			storageDoc1.Delete();
			storageDoc2.Delete();
			storageDoc3.Delete();
			docFactory.Save();

			var storageMain1Reloaded = docFactory.Load<ArchiveStorageMain>(storageMain1.PK);
			AssertEquals("edocs count", 0, storageMain1Reloaded.eDocs.Count);

			storageMain1Reloaded.RestoreFrom(volume);
			AssertEquals("edocs count", 3, storageMain1Reloaded.eDocs.Count);

			foreach (var loadedDoc in storageMain1Reloaded.eDocs.Cast<StorageDocsBase>())
			{
				StorageDocsBase expectedDoc = null;
				foreach (var expectedCopy in storageMain1Copy.eDocs.Cast<StorageDocsBase>())
				{
					if (loadedDoc.SC_FileName == expectedCopy.SC_FileName)
					{
						expectedDoc = expectedCopy;
						break;
					}
				}

				AssertNotNull("expectedDoc", expectedDoc);
				AssertEquals("FileName", expectedDoc.SC_FileName, loadedDoc.SC_FileName);
				AssertEquals("DataType", expectedDoc.SC_DataType, loadedDoc.SC_DataType);
				AssertEquals("Date", expectedDoc.SC_Date, loadedDoc.SC_Date);
				AssertEquals("Desc", expectedDoc.SC_Desc, loadedDoc.SC_Desc);
				AssertEquals("DocType", expectedDoc.SC_DocType, loadedDoc.SC_DocType);
				Assert("image1 equal", Utilities.IsByteArrayEqual(expectedDoc.SC_ImageData, storageMain1Reloaded.eDocs[0].SC_ImageData));
			}
		}

		public void TestHumanReadableName()
		{
			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain1 = docFactory.New<ArchiveStorageMain>();

			AssertEquals(storageMain1.MainReference, storageMain1.HumanReadableName);
		}

		public void ArchiveToVolumeWithBLBFileForTest(ArchiveVolume volume, ArchiveStorageMain storageMain)
		{
			var setting = new XmlWriterSettings();
			setting.Encoding = Encoding.Unicode;
			setting.Indent = true;
			var dateFormatString = "yyyy-MM-dd HH:mm:ss";
			using var metaFileStream = new MemoryStream((storageMain.eDocs.Count * 164) + 90);
			using var writer = XmlWriter.Create(metaFileStream, setting);
			writer.WriteStartDocument(true);
			writer.WriteStartElement("StorageDocs");

			foreach (var doc in storageMain.eDocs.Cast<StorageDocsBase>())
			{
				volume.Add(storageMain.DocBlobFileName(doc), doc.SaveToStream, doc);
				writer.WriteStartElement("StorageDoc");
				writer.WriteAttributeString(AutoStorageDocs.Schema.PK, doc.PK.ToString());
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_DocType, doc.SC_DocType);
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_FileName, doc.SC_FileName);
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_DataType, doc.SC_DataType);
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_Date, doc.SC_Date.ToString(dateFormatString));
				writer.WriteAttributeString(AutoStorageDocs.Schema.SC_Desc, doc.SC_DescMultilingual.GetUnresolvedString());
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			volume.Add(storageMain.MetaFileName, metaFileStream);
			writer.Close();
		}

		DummyArchiveSystem DummyArchiveSystem
			=> new();
	}
}
