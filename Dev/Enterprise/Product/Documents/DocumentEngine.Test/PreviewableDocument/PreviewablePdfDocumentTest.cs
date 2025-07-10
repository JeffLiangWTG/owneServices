using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.TestHelpers;

namespace Enterprise.DocumentEngine.PreviewableDocument.Testing
{
	sealed class PreviewablePdfDocumentTest : NUnit.Framework.TestCase
	{
		static byte[] PdfWithDifferentColorsForEachPage => File.ReadAllBytes(Path.Combine(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\", "PdfWithDifferentColorsForEachPage.pdf"));

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPageSize()
		{
			const float testDocumentPageSizeInInches = 3.94f;
			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, PdfWithDifferentColorsForEachPage);

				using (var pdf = new PreviewablePdfDocument(tempFile.Filename))
				{
					using (ControlDpiScalingHelper.OverrideDPI_ForTesting(96, 96))
					{
						var size = pdf.GetPageSize(0);

						AssertEquals("The value should be the Inch size of the page multiplied by the current DPI settings (Width)", testDocumentPageSizeInInches * 96, size.Width, 1);
						AssertEquals("The value should be the Inch size of the page multiplied by the current DPI settings (Height)", testDocumentPageSizeInInches * 96, size.Height, 1);
					}

					using (ControlDpiScalingHelper.OverrideDPI_ForTesting(144, 144))
					{
						var size = pdf.GetPageSize(0);

						AssertEquals("The value should be the Inch size of the page multiplied by the current DPI settings (Width)", testDocumentPageSizeInInches * 144, size.Width, 1);
						AssertEquals("The value should be the Inch size of the page multiplied by the current DPI settings (Height)", testDocumentPageSizeInInches * 144, size.Height, 1);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCorrupted()
		{
			var corrupt2ndPagePath = AssetsHelper.FetchTestAsset("Architecture/content/DocumentScanning/TestFiles/Corrupt2ndPage.pdf");
			using (var pdf = new PreviewablePdfDocument(corrupt2ndPagePath))
			{
				AssertNoExceptionThrown(() => pdf.GetPageSize(0));
				AssertExceptionThrown<CorruptedDocumentException>("Not all pages could be loaded.", () => pdf.GetPageSize(1));
			}
		}

		// After updating to pdfium, our sample "unsupported" document now works...
		// Eventually a client will give us a failing doc, when that occurs we'll already have a test :)
		//[DatCapabilityRequirement("SOURCE_CODE")]
		//public void TestInterfaceMethodsHandleEncryptedDocuments()
		//{
		//	using (var file1 = TempFile.New())
		//	using (var file2 = TempFile.New())
		//	{
		//		File.WriteAllBytes(file1.Filename, PdfWithDifferentColorsForEachPage);
		//		File.WriteAllBytes(file2.Filename, EncryptedPdfWithDifferentColorsForEachPage);
		//
		//		using (var normalPdf = new PreviewablePdfDocument(file1.Filename))
		//		using (var encryptedPdf = new PreviewablePdfDocument(file2.Filename))
		//		{
		//			AssertExceptionThrown<UnsupportedDocumentException>(() => encryptedPdf.ExtractPages(new[] { 0 }));
		//			AssertExceptionThrown<UnsupportedDocumentException>(() => encryptedPdf.Insert(1, normalPdf));
		//		}
		//	}
		//}	

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPdfIsAlwaysVector()
		{
			using (var file = TempFile.New())
			{
				File.WriteAllBytes(file.Filename, PdfWithDifferentColorsForEachPage);

				using (var pdf = new PreviewablePdfDocument(file.Filename))
				{
					Assert("A PDF is a vector file (not raster)", pdf.IsVector);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInsert()
		{
			using (var file1 = TempFile.New())
			using (var file2 = TempFile.New())
			{
				File.WriteAllBytes(file1.Filename, PdfWithDifferentColorsForEachPage);
				File.WriteAllBytes(file2.Filename, PdfWithDifferentColorsForEachPage);

				using (var pdf1 = new PreviewablePdfDocument(file1.Filename))
				using (var pdf2 = new PreviewablePdfDocument(file2.Filename))
				using (var pdf3 = pdf1.Insert(3, pdf2))
				{
					var fileColors = new[] { Color.Red, Color.Yellow, Color.Lime, Color.Aqua, Color.Blue, Color.Fuchsia };
					var expectedColors = fileColors.Take(3).Concat(fileColors).Concat(fileColors.Skip(3));

					PreviewableImageTestHelpers.AssertHasPages(pdf3, expectedColors.ToArray());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtract()
		{
			using (var file = TempFile.New())
			{
				File.WriteAllBytes(file.Filename, PdfWithDifferentColorsForEachPage);

				using (var pdf = new PreviewablePdfDocument(file.Filename))
				using (var extracted = pdf.ExtractPages(new[] { 0, 2, 4 }))
				{
					PreviewableImageTestHelpers.AssertHasPages(extracted, Color.Red, Color.Lime, Color.Blue);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractToBytes()
		{
			using (var masterfile = TempFile.New())
			using (var childfile = TempFile.New())
			using (var stream = new MemoryStream())
			{
				File.WriteAllBytes(masterfile.Filename, PdfWithDifferentColorsForEachPage);
				using (var pdf = new PreviewablePdfDocument(masterfile.Filename))
				{
					pdf.ExtractPages(stream, new[] { 0, 2, 4 });
					File.WriteAllBytes(childfile.Filename, stream.ToArray());
					using (var extracted = new PreviewablePdfDocument(childfile.Filename))
					{
						PreviewableImageTestHelpers.AssertHasPages(extracted, Color.Red, Color.Lime, Color.Blue);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractPagesWithPdfiumException()
		{
			var corrupt2ndPagePath = AssetsHelper.FetchTestAsset("Architecture/content/DocumentScanning/TestFiles/Corrupt2ndPage.pdf");
			var corrupt2ndPageBytes = File.ReadAllBytes(corrupt2ndPagePath);

			using (var file1 = TempFile.New())
			{
				File.WriteAllBytes(file1.Filename, corrupt2ndPageBytes);

				using (var corruptPdf = new PreviewablePdfDocument(file1.Filename))
				{
					AssertExceptionThrown<PdfiumException>(() => corruptPdf.ExtractPages(new[] { 1 }));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadCorruptedFile()
		{
			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "PrintProcessing", "PrintProcessing.Test", "TestHelper", "TestFiles", "CORRUPTED_File.PDF")));
				using (var pdf = new PreviewablePdfDocument(tempFile.Filename, true))
				{
					AssertExceptionThrown<CorruptedDocumentException>(() => { var number = pdf.NumberOfPages; });
				}
				AssertNullOrEmpty("Should have not problem in deleting temp file during dispose", ErrorReporter.LastKeyReported);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPage()
		{
			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, PdfWithDifferentColorsForEachPage);

				using (var pdf = new PreviewablePdfDocument(tempFile.Filename))
				{
					PreviewableImageTestHelpers.AssertHasPages(pdf, Color.Red, Color.Yellow, Color.Lime, Color.Aqua, Color.Blue, Color.Fuchsia);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRotatePage()
		{
			// The sample file has a white line up top, we can trace that to see where the rotating goes.
			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, PdfWithDifferentColorsForEachPage);

				using (var pdf = new PreviewablePdfDocument(tempFile.Filename))
				{
					pdf.RotatePage(1, true);

					pdf.RotatePage(2, false);

					pdf.RotatePage(3, true);
					pdf.RotatePage(3, true);

					CombineAssertions(() =>
					{
						AssertWhiteLineIsOnEdge(pdf, 0, CompassDirection.North);
						AssertWhiteLineIsOnEdge(pdf, 1, CompassDirection.East);
						AssertWhiteLineIsOnEdge(pdf, 2, CompassDirection.West);
						AssertWhiteLineIsOnEdge(pdf, 3, CompassDirection.South);
					});
				}
			}
		}

		[TargetFrameworks(TargetFramework.NetCore)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderBitmap()
		{
			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, PdfWithDifferentColorsForEachPage);

				var size = new Size(100, 100);
				using (var pdf = new PreviewablePdfDocument(tempFile.Filename))
				using (var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
				using (var g = Graphics.FromImage(bitmap))
				{
					AssertNoExceptionThrown(() => pdf.Render(g, 1, size));
				}
			}
		}

		void AssertWhiteLineIsOnEdge(IPreviewableDocument document, int pageNb, CompassDirection direction)
		{
			using (var img = document.GetPage(pageNb))
			{
				AssertEquals(direction, GetWhiteLineLocation(img));
			}
		}

		enum CompassDirection { North, South, East, West }

		CompassDirection GetWhiteLineLocation(Bitmap bmp)
		{
			var whiteArgb = Color.White.ToArgb();
			return Enum.GetValues(typeof(CompassDirection))
				.Cast<CompassDirection>()
				.First(direction => GetColorForDirection(bmp, direction).ToArgb() == whiteArgb);
		}

		Color GetColorForDirection(Bitmap img, CompassDirection direction)
		{
			const int offset = 5;

			int x = img.Width / 2, y = img.Height / 2;
			switch (direction)
			{
				case CompassDirection.North:
					y = offset;
					break;
				case CompassDirection.East:
					x = img.Width - offset;
					break;
				case CompassDirection.South:
					y = img.Height - offset;
					break;
				case CompassDirection.West:
					x = offset;
					break;
			}

			return img.GetPixel(x, y);
		}
	}
}
