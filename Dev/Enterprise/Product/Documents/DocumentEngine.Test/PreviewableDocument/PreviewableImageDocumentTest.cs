using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static CargoWise.PdfiumWrapper.Testing.ImageTestingHelpers;

namespace Enterprise.DocumentEngine.PreviewableDocument.Testing
{
	sealed class PreviewableImageDocumentTest : TestCase
	{
		public void TestCopyFiles()
		{
			using (var master = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Blue))
			using (var masterDoc = new PreviewableImageDocument(master.Filename))
			using (var child = masterDoc.ExtractPages(new[] { 1, 2 }))
			{
				AssertHasPages(child, Color.Green, Color.Blue);
			}
		}

		public void TestExtractPagesToBytes()
		{
			using (var master = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Blue))
			using (var masterDoc = new PreviewableImageDocument(master.Filename))
			using (var child = TempFile.NewWithExtension(Core.Constants.FileFormats.TIF))
			using (var stream = new MemoryStream())
			{
				masterDoc.ExtractPages(stream, new[] { 1, 2 });
				File.WriteAllBytes(child.Filename, stream.ToArray());
				using (var childDoc = new PreviewableImageDocument(child.Filename))
				{
					AssertHasPages(childDoc, Color.Green, Color.Blue);
				}
			}
		}

		public void TestNumberOfPages()
		{
			using (var singlePage = CreateImageFile(ImageFormat.Png, Color.Red))
			using (var imageManager = new PreviewableImageDocument(singlePage.Filename))
			{
				AssertEquals(1, imageManager.NumberOfPages);
			}

			using (var multiPage = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Blue))
			using (var imageManager = new PreviewableImageDocument(multiPage.Filename))
			{
				AssertEquals(3, imageManager.NumberOfPages);
			}
		}

		public void TestImageIsAlwaysRaster()
		{
			using (var singlePage = CreateImageFile(ImageFormat.Png, Color.Red))
			using (var imageManager = new PreviewableImageDocument(singlePage.Filename))
			{
				Assert("Images are raster, therefore IsVector should be false", !imageManager.IsVector);
			}

			using (var multiPage = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Blue))
			using (var imageManager = new PreviewableImageDocument(multiPage.Filename))
			{
				Assert("Images are raster, therefore IsVector should be false", !imageManager.IsVector);
			}
		}

		public void TestSavingMultipageTiff()
		{
			using (var tempfile = TempFile.New())
			{
				using (var tiff = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green))
				using (var image = new PreviewableImageDocument(tiff.Filename))
				{
					image.Save(tempfile.Filename);
				}

				using (var image = new PreviewableImageDocument(tempfile.Filename))
				{
					AssertHasPages(image, Color.Red, Color.Green);
				}
			}
		}

		public void TestPerfomABunchOfActionsWhenFileLoaded()
		{
			using (var temp = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green))
			using (var redAndGreen = new PreviewableImageDocument(temp.Filename))
			{
				redAndGreen.RotatePage(0, true);
				redAndGreen.RotatePage(1, false);

				using (var greenAndRed = redAndGreen.ExtractPages(new[] { 1, 0 }))
				using (var green = greenAndRed.ExtractPages(new[] { 0 }))
				using (var t2 = CreateImageFile(ImageFormat.Tiff, Color.Blue, Color.Black))
				using (var blueAndBlack = new PreviewableImageDocument(t2.Filename))
				using (var res3 = green.Insert(1, blueAndBlack))
				{
					blueAndBlack.RotatePage(1, false);

					AssertHasPages(res3, Color.Green, Color.Blue, Color.Black);
				}
			}
		}

		public void TestLoadedImageThrowsIOExceptionAsUnreadableDocumentException()
		{
			using (var image = new PreviewableImageDocument(Directory.GetCurrentDirectory() + "abc.de"))
			{
				AssertExceptionThrown<UnreadableDocumentException>(() => image.RotatePage(0, true));
			}
		}

		public void TestDeletePages_ExtractNoPages()
		{
			using (var onePage = CreateImageFile(ImageFormat.Png, Color.Red))
			using (var manager = new PreviewableImageDocument(onePage.Filename))
			{
				AssertExceptionThrown<ArgumentException>(() => manager.ExtractPages(Array.Empty<int>()));
			}

			using (var multiPage = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Blue))
			using (var manager = new PreviewableImageDocument(multiPage.Filename))
			{
				AssertExceptionThrown<ArgumentException>(() => manager.ExtractPages(Array.Empty<int>()));
			}
		}

		public void TestRotate_SinglePage()
		{
			using (var tempFile = TempFile.New())
			{
				using (var image = ImageOfColor(Color.Black, 2, 1))
				{
					image.SetPixel(1, 0, Color.White);
					image.Save(tempFile.Filename, ImageFormat.Png);
				}

				using (var manager = new PreviewableImageDocument(tempFile.Filename))
				{
					manager.RotatePage(0, clockwise: true);

					using (var img = manager.GetPage(0))
					{
						AssertEquals("We rotated clockwise once. The height and width should have been swapped", new Size(1, 2), img.Size);
						AssertEquals("Since we rotated, the pixel that was on the right will now be on the bottom", Color.White, AsNamedColor(img.GetPixel(0, 1)));
						AssertEquals("Since we rotated, the pixel that was on the left will now be on top", Color.Black, AsNamedColor(img.GetPixel(0, 0)));
					}

					manager.RotatePage(0, clockwise: false);

					using (var img = manager.GetPage(0))
					{
						AssertEquals("We rotated counterclockwise once. The height and width should have been swapped", new Size(2, 1), img.Size);
						AssertEquals("Since we rotated, the pixel that was on the bottom will now be on the right", Color.White, AsNamedColor(img.GetPixel(1, 0)));
						AssertEquals("Since we rotated, the pixel that was on the top will now be on left", Color.Black, AsNamedColor(img.GetPixel(0, 0)));
					}
				}
			}
		}

		public void TestRotate_MultiPage()
		{
			TempFile tempFile = null;
			try
			{
				using (var firstPage = ImageOfColor(Color.Black, 2, 1))
				{
					firstPage.SetPixel(1, 0, Color.White);

					using (var secondPage = ImageOfColor(Color.Red, 2, 1))
					{
						secondPage.SetPixel(1, 0, Color.Green);

						tempFile = CreateImageFile(ImageFormat.Tiff, firstPage, secondPage);
					}
				}

				using (var manager = new PreviewableImageDocument(tempFile.Filename))
				{
					manager.RotatePage(1, clockwise: true);

					using (var img = manager.GetPage(1))
					{
						AssertEquals("We rotated clockwise once. The height and width should have been swapped", new Size(1, 2), img.Size);
						AssertEquals("Since we rotated, the pixel that was on the right will now be on the bottom", Color.Green, AsNamedColor(img.GetPixel(0, 1)));
						AssertEquals("Since we rotated, the pixel that was on the left will now be on top", Color.Red, AsNamedColor(img.GetPixel(0, 0)));
					}

					manager.RotatePage(0, clockwise: false);

					using (var img = manager.GetPage(0))
					{
						AssertEquals("We rotated counterclockwise once. The height and width should have been swapped", new Size(1, 2), img.Size);
						AssertEquals("Since we rotated, the pixel that was on the left will now be on the bottom", Color.Black, AsNamedColor(img.GetPixel(0, 1)));
						AssertEquals("Since we rotated, the pixel that was on the right will now be on top", Color.White, AsNamedColor(img.GetPixel(0, 0)));
					}
				}
			}
			finally
			{
				tempFile?.Dispose();
			}
		}

		public void TestReorderPages()
		{
			using (var file = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Yellow, Color.Pink, Color.Green, Color.Orange))
			using (var manager = new PreviewableImageDocument(file.Filename))
			using (var child = manager.ExtractPages(new int[] { 2, 1, 0, 3, 4 }))
			{
				AssertHasPages(child, Color.Pink, Color.Yellow, Color.Red, Color.Green, Color.Orange);
			}
		}

		public void TestInsertPages_AlreadyMultipage()
		{
			using (var source = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Blue))
			using (var sourceManager = new PreviewableImageDocument(source.Filename))
			using (var destination = CreateImageFile(ImageFormat.Tiff, Color.Black, Color.White))
			using (var destManager = new PreviewableImageDocument(destination.Filename))
			using (var result = destManager.Insert(1, sourceManager))
			{
				AssertHasPages(result, Color.Black, Color.Red, Color.Blue, Color.White);
			}
		}

		public void TestInsertPages_FromSinglePage()
		{
			using (var source = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Blue))
			using (var sourceManager = new PreviewableImageDocument(source.Filename))
			using (var destination = CreateImageFile(ImageFormat.Png, Color.Black))
			using (var destManager = new PreviewableImageDocument(destination.Filename))
			using (var result = destManager.Insert(1, sourceManager))
			{
				AssertHasPages(result, Color.Black, Color.Red, Color.Blue);
			}
		}

		#region helpers

		public void TestPageGenerator()
		{
			// Want to make sure the page generator for the test works

			using (var file = CreateImageFile(ImageFormat.Png, Color.Red))
			using (var doc = new PreviewableImageDocument(file.Filename))
			{
				AssertHasPages(doc, new[] { Color.Red });
			}

			using (var file = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Black, Color.Blue))
			using (var doc = new PreviewableImageDocument(file.Filename))
			{
				AssertHasPages(doc, new[] { Color.Red, Color.Green, Color.Black, Color.Blue });
			}
		}

		void AssertHasPages(IPreviewableDocument file, params Color[] colors)
			=> PreviewableImageTestHelpers.AssertHasPages(file, colors);

		#endregion
	}
}
