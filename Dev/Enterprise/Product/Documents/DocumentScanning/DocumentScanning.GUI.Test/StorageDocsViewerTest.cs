using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class StorageDocsViewerTest : TestCaseWithDocumentFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestViewFile_ExceptionIsHandledInNewThread_WhenFileWasDeleted()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			StorageFile document;

			using (var form = new ZFormForPlugInTest(org))
			{
				var testFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\");
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				document = topLevelParentMain.Files.AddNew();
				document.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(testFilePath, "Sample.pdf"));
				document.SC_DataType = "TXT";
				topLevelParentMain.Factory.Save();
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (var reloadedForm = new ZFormForPlugInTest(new BusinessObjectFactory().Load<OrgHeader>(org.PK)))
			{
				reloadedForm.Show();
				reloadedForm.ExposeAllTabPages();
				var isHandled = true;

				AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
				{
					isHandled = false;
				};

				var plugIn = (eDocsPlugIn)reloadedForm.PlugIns.Instances[0];
				var topLevelParentMain = (StorageMain)plugIn.BusinessEntity;
				document = topLevelParentMain.Files.FindByPK(document.PK) as StorageFile;

				AssertNotNull(document);

				using (var storageDocImageViewer = new StorageDocViewerForTesting())
				{
					storageDocImageViewer.Initialise(reloadedForm, plugIn);

					var doc1DatabaseName = new DocManagerDBHelper().GetDatabaseName(1);
					var query = $"DELETE {doc1DatabaseName}..{AutoStorageDocs.Schema.TableName} WHERE {AutoStorageDocs.Schema.PK} = @pk";

					using (var cmd = TestConnection.Command(query))
					{
						cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, document.PK.ToGuid());

						cmd.ExecuteNonQuery();
					}

					storageDocImageViewer.View(document, false);
					storageDocImageViewer.ThreadForViewingFile.Join(5000);

					AssertEquals(true, isHandled);
					AssertEquals("Whilst you were working another user has deleted some information you are attempting to see. Please close this form and retry your action.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestViewEmptyStorageDocsUnderDbStorageShouldTriggerWarning()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DB"))
			using (var form = new ZFormForPlugInTest(Factory.LoadTop1<OrgHeader>(new ZQuery())))
			{
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				using (var storageDocImageViewer = new StorageDocViewerForTesting())
				{
					storageDocImageViewer.Initialise(form, plugIn);

					var document = topLevelParentMain.Documents.AddNew();
					document.SC_ImageData = ZBlob.Empty;
					topLevelParentMain.Factory.Save();

					Application.DoEvents();

					storageDocImageViewer.View(document, false);

					AssertEquals(@"This eDoc has a file size of 0B (bytes) and cannot be opened. eDocs Storage is currently set to SQL Server DocManager database.

Please contact your System Administrator to confirm if the file was previously stored using another eDocs Storage configuration option, such as S3 compatible storage. To verify this, review the Change Log in the Registry for System > DocManager > eDocs Storage.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBadImage()
		{
			string testFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\");

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;
				using (var storageDocImageViewer = new StorageDocViewerForTesting())
				{
					storageDocImageViewer.Initialise(form, plugIn);

					StorageDocs document = topLevelParentMain.Documents.AddNew();
					document.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(testFilePath, "UnsupportedFormat.tif"));
					topLevelParentMain.Factory.Save();

					Application.DoEvents();

					storageDocImageViewer.OpenImageForTesting(document, false);

					if (storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count == 0)
					{
						AssertEquals("0 image view form should be open as it has unsupported format", 0, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("This image cannot be opened because file appears to be damaged or corrupted."));
					}
					else
					{
						AssertEquals("0 image view form should be open as it has unsupported format", 1, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestViewImage_WhereExternalStorageExceptionShouldBeHandled()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=testkeyid;Secret=testsecret"))
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				using (var storageDocImageViewer = new StorageDocViewerForTesting())
				{
					storageDocImageViewer.Initialise(form, plugIn);

					var document = topLevelParentMain.Documents.AddNew();
					document.SC_ImageData = ZBlob.Empty;
					topLevelParentMain.Factory.Save();

					Application.DoEvents();

					AssertNoExceptionThrown("S3 exception should be handled", () => storageDocImageViewer.View(document, false));
					AssertEquals("0 image view form should be open as an exception is thrown when retrieving data from S3", 0, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
					AssertEquals("Unable to access S3 storage, please contact your system administrator to check the configuration of the eDocs storage. Error message: There was an error while accessing the document.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

					using (SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						AssertNoExceptionThrown("S3 exception should be handled", () => storageDocImageViewer.View(document, false));
						AssertEquals("0 image view form should be open as an exception is thrown when retrieving data from S3", 0, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
						AssertEquals("Unable to access S3 storage, please contact your system administrator to check the configuration of the eDocs storage. Error message: There was an error while accessing the document.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCorruptedImage()
		{
			string testFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\");

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;
				using (var storageDocImageViewer = new StorageDocViewerForTesting())
				{
					storageDocImageViewer.Initialise(form, plugIn);

					StorageDocs document = topLevelParentMain.Documents.AddNew();
					document.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(testFilePath, "TestDamagedImage.tif"));
					topLevelParentMain.Factory.Save();

					Application.DoEvents();

					storageDocImageViewer.OpenImageForTesting(document, false);

					if (storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count == 0)
					{
						AssertEquals("0 image view form should be open as it has unsupported format", 0, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("An image could not be opened due to unsupported format or corrupted file."));
					}
					else
					{
						AssertEquals("0 image view form should be open as it has unsupported format", 1, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCloseImageFormOnDocumentDeleting()
		{
			string testFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.GUI\OCR\test-tifs\";

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;
				using (var storageDocImageViewer = new StorageDocViewerForTesting())
				{
					storageDocImageViewer.Initialise(form, plugIn);

					StorageDocs document = topLevelParentMain.Documents.AddNew();
					document.SC_ImageData = DocumentUtilities.GetFileAsBytes(testFilePath + "help.tif");
					topLevelParentMain.Factory.Save();

					Application.DoEvents();

					storageDocImageViewer.OpenImageForTesting(document, false);
					AssertEquals("1 image view form should be open", 1, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);

					storageDocImageViewer.NotifyDocumentDeleted(document.PK);
					AssertEquals("Previously open form should be closed", 0, storageDocImageViewer.GetGraphicDisplayChildFormsForTesting().Count);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRotateImage_CannotDeleteUsingFile()
		{
			string orgFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif";
			string testFilePath = DocumentUtilities.ConvertFileToTiff(orgFilePath);
			AssertEquals(true, File.Exists(testFilePath));

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var topLevelParentMain = (StorageMain)plugIn.BusinessEntity;
				var document = topLevelParentMain.Documents.AddNew();
				var displayForm = new GraphicalDisplayForm(false);
				var manager = new ImageManager(testFilePath, document, displayForm);
				AssertNoExceptionThrown(() => displayForm.GraphicalDisplayControlForTesting.RotateSelectedPages(true));

				manager.Dispose();
				displayForm.Close();
			}

			AssertEquals(false, File.Exists(testFilePath));
		}

		public void TestUpdatingPlugin()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			using (var plugin = new eDocsPlugInForTesting(org1))
			using (var eDocsControl = new eDocsUserControl(plugin))
			using (var secondPlugin = new eDocsPlugInForTesting(org2))
			{
				var storageDocsViewer = eDocsControl.StorageDocsGrid.StorageDocImageViewer;

				AssertEquals("Pre-condition", plugin, storageDocsViewer.plugIn);

				eDocsControl.PlugIn = secondPlugin;

				AssertEquals("Plugin should have updated", secondPlugin, storageDocsViewer.plugIn);
			}
		}

		#region Implementation

		sealed class StorageDocViewerForTesting : StorageDocsViewer
		{
			public Dictionary<ZGuid, ImageManager> GetGraphicDisplayChildFormsForTesting()
			{
				return GraphicDisplayChildForms;
			}

			public void OpenImageForTesting(StorageDocsBase targetImage, bool readOnly)
			{
				base.OpenImage(targetImage, readOnly);
			}

			public Thread ThreadForViewingFile => threadForViewingFile;
		}

		#endregion
	}
}
