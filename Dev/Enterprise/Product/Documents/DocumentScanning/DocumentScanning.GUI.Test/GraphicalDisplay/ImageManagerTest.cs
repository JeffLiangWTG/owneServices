using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentEngine.PreviewableDocument.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.TestHelpers;
using static CargoWise.PdfiumWrapper.Testing.ImageTestingHelpers;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ImageManagerTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPagePasteWithDifferentFiles_PastePDFToImage()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			using (var imageFile = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green))
			{
				using (var doc = factory.NewWithValidTestData<StorageDocs>())
				{
					doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(imageFile.Filename);
					using (var form = new GraphicalDisplayForm(false))
					using (var manager = new ImageManager(doc.SaveToTempFile(), doc, form))
					{
						var testFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test Corrupted.pdf");
						var dataObject = new DataObject();
						dataObject.SetData(DataFormats.FileDrop, new string[] { testFile });
						SafeClipboard.SetDataObject(dataObject);

						var expectedMessage = $@"The following files could not be pasted because they are not in a recognized image file format.
{testFile}
";
						AssertNoExceptionThrown(() => manager.PagePaste(null, new PagePasteEventArgs(1)));
						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Local file should be exists after paste", true, File.Exists(testFile));
					}
				}
			}
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPagePasteWithDifferentFiles_PasteImageToPDF()
		{
			var testPDFFile = Path.Combine(TempForTest.TempPath, "Jerry Sample.PDF");
			File.Copy(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF"), testPDFFile, true);
			var testImageFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Image50dpi.tif");
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			using (var doc = factory.NewWithValidTestData<StorageDocs>())
			{
				doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(testPDFFile);
				using (var form = new GraphicalDisplayForm(false))
				using (var manager = new ImageManager(testPDFFile, doc, form))
				{
					var dataObject = new DataObject();
					dataObject.SetData(DataFormats.FileDrop, new string[] { testImageFile });
					SafeClipboard.SetDataObject(dataObject);

					var expectedMessage = $@"The following files could not be pasted because they are not in a recognized PDF file format.
{testImageFile}
";
					AssertNoExceptionThrown(() => manager.PagePaste(null, new PagePasteEventArgs(1)));
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPagePasteWithDifferentFiles_PasteIncorrectPDFToPDF()
		{
			var testPDFFile1 = Path.Combine(TempForTest.TempPath, "Jerry Sample.PDF");
			var testPDFFile2 = AssetsHelper.FetchTestAsset("Architecture/content/DocumentScanning/TestFiles/Corrupt2ndPage.pdf");
			File.Copy(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF"), testPDFFile1, true);
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			using (var doc = factory.NewWithValidTestData<StorageDocs>())
			{
				doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(testPDFFile1);
				using (var form = new GraphicalDisplayForm(false))
				using (var manager = new ImageManager(testPDFFile1, doc, form))
				{
					var dataObject = new DataObject();
					dataObject.SetData(DataFormats.FileDrop, new string[] { testPDFFile2 });
					SafeClipboard.SetDataObject(dataObject);

					var expectedMessage = $@"The following files could not be pasted because they are not in a recognized PDF file format.
{testPDFFile2}
The error message is Cannot Import Pages

";
					AssertNoExceptionThrown(() => manager.PagePaste(null, new PagePasteEventArgs(1)));
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSubscribeEventHandlerShouldAfterReloadImage()
		{
			var testFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test Corrupted.pdf");
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			using (var doc = factory.NewWithValidTestData<StorageDocs>())
			using (var form = new GraphicalDisplayForm(false))
			{
				AssertExceptionThrown<CorruptedDocumentException>(() => new ImageManager(testFile, doc, form));

				var testNewFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
				var dataObject = new DataObject();
				dataObject.SetData(DataFormats.FileDrop, new string[] { testNewFile });
				SafeClipboard.SetDataObject(dataObject);

				AssertNoExceptionThrown(form.PagePasteForTesting);
			}
		}

		[DeveloperOnlyTest]
		public void TestPagePaste()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			using (var imageFile = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green))
			{
				using (var doc = factory.NewWithValidTestData<StorageDocs>())
				{
					doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(imageFile.Filename);
					using (var form = new GraphicalDisplayForm(false))
					using (var manager = new ImageManager(doc.SaveToTempFile(), doc, form))
					{
						AssertEquals(2, manager.ImageFile.NumberOfPages);

						using (var imageFileToBePasted = CreateImageFile(ImageFormat.Tiff, Color.Blue, Color.Brown))
						{
							var docToBePasted = factory.NewWithValidTestData<StorageDocs>();
							docToBePasted.SC_ImageData = DocumentUtilities.GetFileAsBytes(imageFileToBePasted.Filename);
							var serialisableEdoc = new SerializableEDoc(docToBePasted, File.ReadAllBytes(imageFileToBePasted.Filename));
							SafeClipboard.SetDataObject(new DocManagerDataObject(serialisableEdoc));

							manager.PagePaste(null, new PagePasteEventArgs(1));

							AssertEquals(4, manager.ImageFile.NumberOfPages);
							AssertHasPages(manager.ImageFile, Color.Red, Color.Blue, Color.Brown, Color.Green);
						}
					}
				}
			}
		}

		// Tests PageCut, but that creates a temporary clipboard file for the copied content, we we
		// also call Paste to remove that from the clipboard.
		// The test just reverses the two pages from Red-Green to Green-Red via Cut-and-Paste
		[DeveloperOnlyTest]
		public void TestPageCutAndPaste()
		{
			using (var imageFile = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green))
			{
				var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				using (var doc = factory.NewWithValidTestData<StorageDocs>())
				{
					doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(imageFile.Filename);
					var docFileName = doc.SaveToTempFile();

					using (var form = new GraphicalDisplayForm(false))
					using (var manager = new ImageManager(docFileName, doc, form))
					{
						AssertEquals(2, manager.ImageFile.NumberOfPages);
						AssertHasPages(manager.ImageFile, Color.Red, Color.Green);
						manager.PageCut(null, new PageCutEventArgs(new int[] { 0 }, true));
						manager.PagePaste(null, new PagePasteEventArgs(1));
						AssertEquals(2, manager.ImageFile.NumberOfPages);
						AssertHasPages(manager.ImageFile, Color.Green, Color.Red);
					}
				}
			}
		}

		void AssertHasPages(IPreviewableDocument file, params Color[] colors)
			=> PreviewableImageTestHelpers.AssertHasPages(file, colors);
	}
}
