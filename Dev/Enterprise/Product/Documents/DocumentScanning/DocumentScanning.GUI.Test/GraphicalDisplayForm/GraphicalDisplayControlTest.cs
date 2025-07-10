using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Thumbnails;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class GraphicalDisplayControlTest : Business.Test.TestCaseWithDocumentFactory
	{
		[RequiresSTA]
		public void TestRotateForFullPageView() => ShowFormAndRotate(false);

		public void TestRotateForThumbnail() => ShowFormAndRotate(true);

		(TempFile, StorageDocs, IPreviewableDocument) CreateDocument(string sourcePath)
		{
			var file = TempFile.NewFromFile(sourcePath);
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_ImageData = DocumentUtilities.GetFileAsBytes(file.Filename);
			return (file, document, PreviewableDocumentHelper.GetPreviewableDocument(file.Filename));
		}

		void ShowFormAndRotate(bool thumbnailView)
		{
			var multipageTestDocumentBytes = MultipageTestDocumentBytes;
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_ImageData = multipageTestDocumentBytes;

			using (var form = new GraphicalDisplayForm(false))
			{
				form.GraphicalDisplayControlForTesting.ThumbnailView = thumbnailView;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Minimum = 1;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Maximum = 5;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Value = 1; // go to first page (1 based index)
				form.ShowFile(PreviewableDocumentHelper.GetPreviewableDocument("TIF", multipageTestDocumentBytes), document, 1);
				form.Show();
				var rotationEventFiredCount = 0;
				form.GraphicalDisplayControlForTesting.Rotation += delegate
				{ rotationEventFiredCount++; };
				form.GraphicalDisplayControlForTesting.RotateSelectedPages(true);
				AssertEquals("PageNumericUpDownValue should have stayed the same", (decimal)1, form.GraphicalDisplayControlForTesting.PageNumericUpDown.Value);
				AssertEquals("Rotation event should have been fired", 1, rotationEventFiredCount);
			}
		}

		[GuiTest]
		public void TestMultipleShowFileCallsDontIncreaseEventHandlers()
		{
			using (var disposables = new DisposableList(10))
			using (var form = new GraphicalDisplayForm(false))
			{
				form.Show();

				Application.DoEvents();
				var multipageTestDocumentTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTestDocument.tif");
				for (var i = 0; i < 10; i++)
				{
					var (file, storageDoc, doc) = CreateDocument(multipageTestDocumentTifPath);
					disposables.Add(file, doc);

					form.GraphicalDisplayControlForTesting.ShowFile(doc, storageDoc);
					Application.DoEvents();
				}

				var picturebox = form.GraphicalDisplayControlForTesting.DocumentPreviewPictureBox;
				FireMouseEvent(picturebox, LeftClickCenter(picturebox));

				var openMagnifyForms = ZApplication.GetOpenForms().OfType<MagnifyForm>().ToList();
				disposables.AddRange(openMagnifyForms);

				AssertEquals(1, openMagnifyForms.Count);
			}
		}

		[GuiTest]
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowCorruptedImage()
		{
			var dummyFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\CorruptedImage.tif");

			using (var disposables = new DisposableList(0))
			using (var form = new GraphicalDisplayForm(false))
			{
				var modifiedRegistry = Env.Registry.DMThumbnailSettings;
				modifiedRegistry.ThumbNailViewActive = true;

				Env.Registry.DMThumbnailSettings = modifiedRegistry;
				var (file, storageDoc, doc) = CreateDocument(dummyFile);
				disposables.Add(file, doc);

				form.GraphicalDisplayControlForTesting.ShowFile(doc, storageDoc, 1);
				form.Show();
				Application.DoEvents();

				AssertEquals("The document is corrupted, system preview could not be created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
			}
		}

		void FireMouseEvent(Control c, MouseEventArgs args)
		{
			var method = c.GetType().GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(c, new object[] { args });
		}

		MouseEventArgs LeftClickCenter(Control c)
			=> new MouseEventArgs(MouseButtons.Left, 1, c.Width / 2, c.Height / 2, 0);

		public void TestThumbnailsClearedWhenImageProviderIsNull()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var multipageTestDocumentBytes = MultipageTestDocumentBytes;
			using (var previewableDocument = PreviewableDocumentHelper.GetPreviewableDocument("TIF", multipageTestDocumentBytes))
			{
				var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
				document.SC_ImageData = multipageTestDocumentBytes;

				using (var form = new GraphicalDisplayForm(false))
				{
					form.GraphicalDisplayControlForTesting.ThumbnailView = true;
					AssertNull("Pre-Condition", form.GraphicalDisplayControlForTesting.imageProvider);
					AssertEquals("Pre-Condition", 0, form.GraphicalDisplayControlForTesting.ThumbNailDrawer.ThumbnailList.Count);
					form.GraphicalDisplayControlForTesting.ShowFile(previewableDocument, document, 1);
					form.Show();
					AssertNotNull("imageProvider should not be null", form.GraphicalDisplayControlForTesting.imageProvider);

					// Simulate deleting the file
					form.GraphicalDisplayControlForTesting.imageProvider.Dispose();
					form.GraphicalDisplayControlForTesting.imageProvider = null;
					form.GraphicalDisplayControlForTesting.UpdatePreviewImage(true);

					AssertEquals("Thumbnails should have been cleared", 0, form.GraphicalDisplayControlForTesting.ThumbNailDrawer.ThumbnailList.Count);
				}
			}
		}

		public void TestMessageShownToUserWithOutOfMemoryException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ErrorReporter.Clear();
			var testTifBytes = TestTifBytes;
			var document1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document1.SC_ImageData = testTifBytes;

			using (var form = new GraphicalDisplayFormThrowsOutOfMemoryException(false))
			using (var document = PreviewableDocumentHelper.GetPreviewableDocument("TIF", testTifBytes))
			{
				form.GraphicalDisplayControlForTesting.ThumbnailView = true;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Minimum = 1;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Maximum = 5;
				form.ShowFile(document, document1, 1);
				form.Show();

				Assert("User message should have been shown in Thumbnail View", UnitTestUserNotification.Instance.LastMessage.Contains("There were not enough resources to show this preview."));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ErrorReporter.Clear();

				form.GraphicalDisplayControlForTesting.ThumbnailView = false;
				form.GraphicalDisplayControlForTesting.ToolBar_ButtonClick(form.GraphicalDisplayControlForTesting.ToolBar, new ToolBarButtonClickEventArgs(form.GraphicalDisplayControlForTesting.ToolBar.Buttons[3]));
				Assert("User message should have been shown in Single Page View", UnitTestUserNotification.Instance.LastMessage.Contains("There were not enough resources to show this preview."));
			}

			ErrorReporter.Clear();
		}

		[RequiresSTA]
		public void TestFullScreenWithoutImageProvider()
		{
			using (var form = new GraphicalDisplayForm(false))
			{
				form.Show();
				AssertNoExceptionThrown("FullScreenView call should check null for imageProvider field.",
					() => form.GraphicalDisplayControlForTesting.ToolBar_ButtonClick(
						form.GraphicalDisplayControlForTesting.ToolBar,
						new ToolBarButtonClickEventArgs(form.GraphicalDisplayControlForTesting.ToolBar.Buttons[4])));
			}
		}

		public void TestRotateWithExternalException()
		{
			var multipageTestDocumentBytes = MultipageTestDocumentBytes;
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_ImageData = multipageTestDocumentBytes;

			using (var form = new GraphicalDisplayForm(false))
			{
				form.GraphicalDisplayControlForTesting.ThumbnailView = true;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Minimum = 1;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Maximum = 5;
				form.GraphicalDisplayControlForTesting.PageNumericUpDown.Value = 1; // go to first page (1 based index)
				form.ShowFile(PreviewableDocumentHelper.GetPreviewableDocument("TIF", multipageTestDocumentBytes), document, 1);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ErrorReporter.Clear();

				form.GraphicalDisplayControlForTesting.Rotation += GraphicalDisplayFormThrowsExternalException_Rotation;
				AssertNoExceptionThrown(() => form.GraphicalDisplayControlForTesting.RotateSelectedPages(true));
			}
			ErrorReporter.Clear();
		}

		void GraphicalDisplayFormThrowsExternalException_Rotation(object sender, RotateEventArgs e) => throw new ExternalException("A generic error occurred in GDI+.");

		[ExpectNoExceptions()]
		public void TestShowFile_MagFormNotBeNull()
		{
			var testTifBytes = TestTifBytes;
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_ImageData = testTifBytes;

			using (var form = new GraphicalDisplayForm(false))
			{
				form.GraphicalDisplayControlForTesting.ThumbnailView = true;
				form.ShowFile(PreviewableDocumentHelper.GetPreviewableDocument("TIF", testTifBytes), document, 1);
				form.Show();

				var sender = form.GraphicalDisplayControlForTesting.DocumentPreviewPictureBox;
				var mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0);
				form.GraphicalDisplayControlForTesting.MagnifyManager.PictureBox_MouseDown(sender, mouseArgs);

				AssertNotNull(form.GraphicalDisplayControlForTesting.MagnifyManager.MagForm);

				form.ShowFile(PreviewableDocumentHelper.GetPreviewableDocument("TIF", testTifBytes), document, 1);

				AssertNotNull(form.GraphicalDisplayControlForTesting.MagnifyManager.MagForm);
			}
		}

		public void TestClearImage()
		{
			var multipageTestDocumentBytes = MultipageTestDocumentBytes;
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_ImageData = multipageTestDocumentBytes;

			using (var form = new GraphicalDisplayForm(false))
			{
				form.GraphicalDisplayControlForTesting.ShowFile(PreviewableDocumentHelper.GetPreviewableDocument("TIF", multipageTestDocumentBytes), document, 1);
				form.Show();
				AssertEquals(10, form.GraphicalDisplayControlForTesting.PageSelector.TotalPages);

				form.GraphicalDisplayControlForTesting.Close();
				AssertNull(form.GraphicalDisplayControlForTesting.PageSelector);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageIsResizedWithPanel()
		{
			using (var tmpFile = TempFile.NewFromFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\200kb.tif")))
			{
				var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
				doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(tmpFile.Filename);

				using (var form = new GraphicalDisplayForm(false))
				using (var document = PreviewableDocumentHelper.GetPreviewableDocument("TIF", File.ReadAllBytes(tmpFile.Filename)))
				{
					form.GraphicalDisplayControlForTesting.ShowFile(document, doc, 1);
					form.Show();
					var x1 = form.GraphicalDisplayControlForTesting.ThumbNailDrawer.SizeManager.TotalBoxSizeForOneThumbnail.Width;
					var y1 = form.GraphicalDisplayControlForTesting.ThumbNailDrawer.SizeManager.TotalBoxSizeForOneThumbnail.Height;
					form.Size = new Size(form.Size.Width / 10, form.Size.Height / 10);
					form.GraphicalDisplayControlForTesting.Size = new Size(form.GraphicalDisplayControlForTesting.Size.Width / 10, form.GraphicalDisplayControlForTesting.Size.Height / 10);
					form.GraphicalDisplayControlForTesting.ThumbnailPanel.Size = new Size(form.GraphicalDisplayControlForTesting.ThumbnailPanel.Size.Width / 10, form.GraphicalDisplayControlForTesting.ThumbnailPanel.Size.Height / 10);
					var x2 = form.GraphicalDisplayControlForTesting.ThumbNailDrawer.SizeManager.TotalBoxSizeForOneThumbnail.Width;
					var y2 = form.GraphicalDisplayControlForTesting.ThumbNailDrawer.SizeManager.TotalBoxSizeForOneThumbnail.Height;
					AssertNotEquals(x1, x2);
					AssertNotEquals(y1, y2);
				}
			}
		}

		[ExpectNoExceptions()]
		[RequiresSTA]
		public void TestResizingDoesNotCrash()
		{
			using (var form = new GraphicalDisplayFormExposeOnSizeChanged(false))
			{
				form.GraphicalDisplayControlForTesting.UpdateThumbnailView();
				form.GraphicalDisplayControlForTesting.UpdateSingleImageView();
				form.GraphicalDisplayControlForTesting.Size = new Size(284, 385);
				((GraphicalDisplayControlExposeOnSizeChanged)form.GraphicalDisplayControlForTesting).OnSizeChanged(new EventArgs());
				form.GraphicalDisplayControlForTesting.ThumbnailPanel_SizeChanged(new object(), new EventArgs());
				form.GraphicalDisplayControlForTesting.UpdatePreviewImage(true);
				form.GraphicalDisplayControlForTesting.UpdatePreviewImage(false);
			}
		}

		public void TestDisableRotate()
		{
			using (var form = new GraphicalDisplayForm(false))
			{
				var control = form.GraphicalDisplayControlForTesting;
				var rotateButtons = new[] { control.ToolBar.Buttons["RotateRight"], control.ToolBar.Buttons["RotateLeft"] };
				Assert("By default the rotate buttons should be enabled", control.AllowRotate);

				control.AllowRotate = false;
				Assert("Should honor setting the value", !control.AllowRotate);
				Assert("The rotate buttons should be disabled", rotateButtons.All(b => !b.Enabled));

				control.AllowRotate = true;
				Assert("Should honor setting the value", control.AllowRotate);
				Assert("The rotate buttons should be enabled", rotateButtons.All(b => b.Enabled));
			}
		}

		[RequiresSTA]
		public void TestDBHitsForSaveThumbnailVisibleSetting()
		{
			using (var form = new GraphicalDisplayForm(false))
			{
				var setting = Env.Registry.DMThumbnailSettings;
				setting.ThumbNailViewActive = true;
				Env.Registry.DMThumbnailSettings = setting;

				var commandCount = Db.Connection.ExecutedCommandCount;
				form.GraphicalDisplayControlForTesting.SetThumbnailVisible(setting.ThumbNailViewActive);
				var newCommandCount = Db.Connection.ExecutedCommandCount;
				AssertEquals("Should not execute update DMThumbnailSettings command", commandCount, newCommandCount);

				commandCount = Db.Connection.ExecutedCommandCount;
				form.GraphicalDisplayControlForTesting.SetThumbnailVisible(!setting.ThumbNailViewActive);
				newCommandCount = Db.Connection.ExecutedCommandCount;
				AssertEquals("Should just execute update DMThumbnailSettings command one time", commandCount + 1, newCommandCount);

				commandCount = Db.Connection.ExecutedCommandCount;
				form.GraphicalDisplayControlForTesting.SetThumbnailVisible(!setting.ThumbNailViewActive);
				newCommandCount = Db.Connection.ExecutedCommandCount;
				AssertEquals("Should not execute update DMThumbnailSettings command", commandCount, newCommandCount);
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

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly));

		byte[] MultipageTestDocumentBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTestDocument.tif");

		byte[] TestTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");

		class GraphicalDisplayFormThrowsOutOfMemoryException : GraphicalDisplayForm
		{
			public GraphicalDisplayFormThrowsOutOfMemoryException(bool readOnly)
				: base(readOnly)
			{
			}

			protected override GraphicalDisplayControl GetNewGraphicalDisplayControl(bool readOnly) => new GraphicalDisplayControlThrowsOutOfMemoryException(readOnly);
		}

		class GraphicalDisplayControlThrowsOutOfMemoryException : GraphicalDisplayControl
		{
			public GraphicalDisplayControlThrowsOutOfMemoryException(bool readOnly)
				: base(readOnly)
			{
			}

			protected override SinglePagePreview GetNewSinglePagePreview(PictureBox previewPictureBox, Panel imagePanel) => new SinglePagePreviewThrowsOutOfMemoryException(previewPictureBox, imagePanel);

			protected override ThumbNailDrawer GetNewThumbNailDrawer(Panel thumbPanel) => new ThumbNailDrawerThrowsOutOfMemoryException(thumbPanel);
		}

		class GraphicalDisplayFormExposeOnSizeChanged : GraphicalDisplayForm
		{
			public GraphicalDisplayFormExposeOnSizeChanged(bool readOnly)
				: base(readOnly)
			{
			}

			protected override GraphicalDisplayControl GetNewGraphicalDisplayControl(bool readOnly) => new GraphicalDisplayControlExposeOnSizeChanged(readOnly);
		}

		class GraphicalDisplayControlExposeOnSizeChanged : GraphicalDisplayControl
		{
			public GraphicalDisplayControlExposeOnSizeChanged(bool readOnly)
				: base(readOnly)
			{
			}

			public new void OnSizeChanged(EventArgs e) => base.OnSizeChanged(e);
		}

		class SinglePagePreviewThrowsOutOfMemoryException : SinglePagePreview
		{
			public SinglePagePreviewThrowsOutOfMemoryException(PictureBox previewPictureBox, Panel parentPanel)
				: base(previewPictureBox, parentPanel)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
			protected override Bitmap GetThumbnailFromImage(Image imageToThumbnail, Size boundingSize) => throw new OutOfMemoryException();
		}

		class ThumbNailDrawerThrowsOutOfMemoryException : ThumbNailDrawer
		{
			public ThumbNailDrawerThrowsOutOfMemoryException(Panel parentPanel)
				: base(parentPanel)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
			protected override Bitmap GetThumbnailFromImage(Image image, Size boundingSize) => throw new OutOfMemoryException();
		}
	}
}
