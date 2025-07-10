using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.GUI.Test
{
	sealed class ArchiveRecordFormTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoCorruptedImageFormatMessageWhenFormOpens()
		{
			var imageBytes = TestFileHelper.File200KB;

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain1 = docFactory.New<ArchiveStorageMain>();
			storageMain1.SM_DB = 1;
			storageMain1.SM_OffLine = ZDateTime.Today;
			storageMain1.SM_CD1 = 1;

			var storageDoc1 = storageMain1.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto
			{
				FileName = "testHenry1",
				DocumentType = "MSC",
			});
			storageDoc1.SC_Desc = "test 1";
			storageDoc1.SC_DataType = "TIF";

			using var directory = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var volume = ArchiveVolume.CreateNew(directory.DirectoryName, 50, logger, descriptor);
			_ = Directory.CreateDirectory(volume.VolumePath);

			storageMain1.ArchiveTo(volume);
			volume.Close();

			storageDoc1.Delete();

			UnitTestUserNotification.Instance.ClearMessages();

			using var form = new ArchivedRecordForm(storageMain1);
			ArchivedRecordForm.SetLastLocationOfArchiveVolumeForTest(directory);
			form.Show();
			Application.DoEvents();
			Assert(!UnitTestUserNotification.Instance.LastMessage.Contains("An image could not be opened due to unsupported format or corrupted file"));
			CombineAssertions("Form control should be setup correctly", () =>
			{
				AssertEquals(DockStyle.None, form.previewDisplayControl.Dock);
				AssertEquals(AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right, form.previewDisplayControl.Anchor);
				AssertEquals(Point.Empty, form.previewDisplayControl.Location);
				AssertEquals(form.previewDisplayControl.Parent.ClientSize, form.previewDisplayControl.Size);
			});
			form.Close();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreviewUnsupportedDocumentFormat()
		{
			var testDocBytes = TestFileHelper.UnsupportedFormat;

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageMain1 = docFactory.New<ArchiveStorageMain>();
			storageMain1.SM_DB = 1;
			storageMain1.SM_OffLine = ZDateTime.Today;
			storageMain1.SM_CD1 = 1;

			var storageDoc1 = storageMain1.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto
			{
				FileName = "testUnsupportedFormat",
				DocumentType = "MSC",
			});
			storageDoc1.SC_Desc = "test 2";
			storageDoc1.SC_DataType = "docx";

			using var directory = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var volume = ArchiveVolume.CreateNew(directory.DirectoryName, 50, logger, descriptor);
			_ = Directory.CreateDirectory(volume.VolumePath);

			storageMain1.ArchiveTo(volume);
			volume.Close();

			UnitTestUserNotification.Instance.ClearMessages();

			using var form = new ArchivedRecordForm(storageMain1);
			ArchivedRecordForm.SetLastLocationOfArchiveVolumeForTest(directory);
			form.Show();
			Application.DoEvents();
			var previewPaneImageManager = form.previewPaneImageManager;
			var image = previewPaneImageManager.ImageFile;
			AssertType("The previewed document is the dummy document", typeof(DummyPreviewable), image);
			AssertEquals("The error message is correct", "This document format is not supported for previewing.", ((DummyPreviewable)image).ErrorMessage);
			form.Close();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandleExternalStorageException()
		{
			var testDocBytes = TestFileHelper.UnsupportedFormat;
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			{
				var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var storageMain = docFactory.New<ArchiveStorageMain>();
				storageMain.SM_DB = 1;
				storageMain.SM_OffLine = ZDateTime.Today;
				storageMain.SM_CD1 = 1;
		
				var storageDoc = storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto
				{
					FileName = "testUnsupportedFormat",
					DocumentType = "MSC",
				});
				storageDoc.SC_Desc = "test 2";
				storageDoc.SC_DataType = "docx";
				storageMain.MasterFactory.Save();
		
				using (var directory = new TempDirectory())
				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3))
				{
					var logger = new TestArchiveLogger();
					var descriptor = DummyArchiveSystem;
					var volume = ArchiveVolume.CreateNew(directory.DirectoryName, 50, logger, descriptor);
					_ = Directory.CreateDirectory(volume.VolumePath);
		
					storageMain.ArchiveTo(volume);
					volume.Close();
		
					UnitTestUserNotification.Instance.ClearMessages();

					using var form = new ArchivedRecordForm(storageMain);
					// The emptying of SC_ImageData normally runs after SaveToExternalStorage and IsMovingToExternalStorage will be set to true to avoid RetrieveFromExternalStorage
					var propertyInfo = typeof(StorageDocsWithS3Support).GetProperty("IsMovingToExternalStorage", BindingFlags.Instance | BindingFlags.Public);
					propertyInfo.SetValue(storageDoc, true);

					storageDoc.SC_ImageData = ZBlob.Empty;
					ArchivedRecordForm.SetLastLocationOfArchiveVolumeForTest(directory);
					form.Show();

					AssertEquals("Unable to access S3 storage, please contact your system administrator to check the configuration of the eDocs storage. Error message: There was an error while accessing the document.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testDbHelper = new DocManagerDBHelperTestClass();
			if (!testDbHelper.DatabaseExists(1))
			{
				_ = testDbHelper.CreateDatabase(1);
			}
		}

		DummyArchiveSystem DummyArchiveSystem
			=> new();
	}
}
