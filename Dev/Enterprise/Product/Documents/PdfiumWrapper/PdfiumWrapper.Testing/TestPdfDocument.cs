using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace CargoWise.PdfiumWrapper.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class PdfiumTest : TestCase
	{
		static string TestRepositoryPath => Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs");
		static byte[] PdfWithDifferentColorsForEachPage => File.ReadAllBytes(Path.Combine(TestRepositoryPath, "PdfWithDifferentColorsForEachPage.pdf"));
		static byte[] PdfWithFullscreenBlackButton => File.ReadAllBytes(Path.Combine(TestRepositoryPath, "PdfWithFullscreenBlackButton.pdf"));

		IDisposable LoadPdfWithDifferentColorsForEachPage(out PdfDocument document)
		{
			var tempFile = TempFile.New();
			File.WriteAllBytes(tempFile.Filename, PdfWithDifferentColorsForEachPage);

			document = PdfDocument.LoadFile(tempFile.Filename);
			return new DisposableList(2) { document, tempFile };
		}

		IDisposable LoadPdfWithFullscreenBlackButton(out PdfDocument document)
		{
			var tempFile = TempFile.New();
			File.WriteAllBytes(tempFile.Filename, PdfWithFullscreenBlackButton);

			document = PdfDocument.LoadFile(tempFile.Filename);
			return new DisposableList(2) { document, tempFile };
		}

		public void TestFPDFText_GetSignatureCount()
		{
			using (var document = PdfDocument.LoadFile(Path.Combine(TestRepositoryPath, "FileWithSignature.pdf")))
			{
				AssertEquals(1, document.SignatureCount);
			}
		}

		public void TestCorrectExceptionsAreProvided()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<DirectoryNotFoundException>(() =>
				{
					using (PdfDocument.LoadFile("Non\\existant\\path.pdf"))
					{ }
				});

				AssertPdfiumException("When we try to load something that isn't a pdf, we should get the corrupted exception", Path.Combine(TestRepositoryPath, "MultipageTestDocument.Tif"), PdfiumError.FileCorrupt);
				// I need sample documents for the rest...
			});
		}

		public void TestNoDefineErrorException()
		{
			var exception = AssertExceptionThrown<PdfiumException>(() =>
			{
				throw new PdfiumException((PdfiumError)int.MaxValue);
			});

			CombineAssertions(() =>
			{
				AssertContains("Error Code Not Define", "Error Code Not Define", exception.Message);
			});
		}

		public void TestFileHandleIsReleasedIfFailToLoad()
		{
			var tempFile = TempFile.NewWithExtension("tif");
			File.WriteAllBytes(tempFile.Filename, File.ReadAllBytes(Path.Combine(TestRepositoryPath, "MultipageTestDocument.Tif")));
			AssertPdfiumException("When we try to load something that isn't a pdf, we should get the corrupted exception", tempFile.Filename, PdfiumError.FileCorrupt);
			Assert("File should able to be deleted if fail to load", TempFile.TryDeleteHandleAllExceptions(tempFile.Filename));
			tempFile.Dispose();
		}

		public void TestEnumerableLivesBeyondDocument()
		{
			try
			{
				using (var document = PdfDocument.LoadFile(Path.Combine(TestRepositoryPath, "eat_glass.pdf")))
				{
					foreach (var result in document.Search(" ", flags: Native.PdfSearchFlags.NONE))
					{
						document.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is NullReferenceException ||
					ex is AccessViolationException ||
					ex is ObjectDisposedException)
				{
					Assert(true);
				}
				else
				{
					throw;
				}
			}
		}

		class MemoryStreamWithDisposedFlag : MemoryStream
		{
			public bool IsDisposed { get; private set; }

			public MemoryStreamWithDisposedFlag(byte[] data)
				: base(data) { }

			protected override void Dispose(bool disposing)
			{
				IsDisposed |= disposing;

				base.Dispose(disposing);
			}
		}

		public void TestLoadingFromStream()
		{
			using (var stream = new MemoryStreamWithDisposedFlag(PdfWithDifferentColorsForEachPage))
			{
				using (var pdf = new PdfDocument(stream))
				{
					AssertHasPages("Should loaded the document correctly", pdf, Color.Red, Color.Yellow, Color.Lime, Color.Aqua, Color.Blue, Color.Fuchsia);
				}

				Assert("The stream should be disposed with the document", stream.IsDisposed);
			}
		}

		void AssertPdfiumException(string message, string pathToLoad, PdfiumError expectedError)
		{
			PdfDocument document = null;
			try
			{
				document = PdfDocument.LoadFile(pathToLoad);
				Fail("Expected PdfiumException but none were thrown");
			}
			catch (PdfiumException ex)
			{
				AssertEquals(message, expectedError, ex.ErrorType);
			}
			catch (Exception ex)
			{
				Fail(message + "\r\nExpected PdfiumException got " + ex.GetType().ToString());
			}
			finally
			{
				document?.Dispose();
			}
		}

		public void TestHandlesAreDisposedCorrectly()
		{
			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, PdfWithDifferentColorsForEachPage);

				using (var doc = PdfDocument.LoadFile(tempFile.Filename))
				{
					AssertEquals("PRE: Just asserting something to ensure we're opening the document", 6, doc.PageCount);
				}

				using (var doc = PdfDocument.LoadFile(tempFile.Filename))
				{
					AssertEquals("PRE: Just asserting something to ensure we're opening the document", 6, doc.PageCount);
				}
			}
		}

		public void TestGetPageCount()
		{
			using (LoadPdfWithDifferentColorsForEachPage(out var pdfDoc))
			{
				AssertEquals(6, pdfDoc.PageCount);
			}
		}

		public void TestGetPageSizeInPoints()
		{
			using (LoadPdfWithDifferentColorsForEachPage(out var pdfDoc))
			{
				var size = pdfDoc.GetPageSizeInPoints(0);
				CombineAssertions(() =>
				{
					AssertEquals("Should return the pdfs first page's WIDTH", 283, size.Width, 1);
					AssertEquals("Should return the pdfs first page's HEIGHT", 283, size.Height, 1);
				});
			}
		}

		public void TestRenderPage()
		{
			using (LoadPdfWithDifferentColorsForEachPage(out var pdfDoc))
			using (var bmp = new Bitmap(400, 400))
			using (var g = Graphics.FromImage(bmp))
			{
				pdfDoc.RenderPage(g, 0, bmp.Size);

				var centerPixel = bmp.GetPixel(200, 200);
				AssertEquals(Color.Red, ImageTestingHelpers.AsNamedColor(centerPixel));
			}
		}

		public void TestRenderPage_WithWidgetAnnotations()
		{
			using (LoadPdfWithFullscreenBlackButton(out var pdfDoc))
			using (var bmp = new Bitmap(400, 400))
			using (var g = Graphics.FromImage(bmp))
			{
				pdfDoc.RenderPage(g, 0, bmp.Size);

				var centerPixel = bmp.GetPixel(200, 200);
				AssertEquals(Color.Black, ImageTestingHelpers.AsNamedColor(centerPixel));
			}
		}

		public void TestImportPage()
		{
			using (LoadPdfWithDifferentColorsForEachPage(out var sourceDoc))
			using (var destDoc = new PdfDocument())
			{
				destDoc.InsertPages(sourceDoc, 0, 1, 2, 4);

				AssertEquals("We inserted three pages, so the pagecount should be 3", 3, destDoc.PageCount);
				AssertArrayEqualsByElements("Range '1-2,4' should have grabbed the second, third and fifth pages", new[] { Color.Yellow, Color.Lime, Color.Blue }, GetPageColors(destDoc));
			}
		}

		public void TestSave_SimpleCopy()
		{
			var originalPath = Path.Combine(TestRepositoryPath, "Sample.pdf");
			using (var original = PdfDocument.LoadFile(originalPath))
			using (var copyPath = TempFile.NewWithExtension("pdf"))
			{
				AssertNoExceptionThrown("We should be able to save a copy of an existing doc. Check your save flags (PdfSaveFlags.Incremental will cause this to fail)", () => original.Save(copyPath.Filename));

				Assert("The file should have been created", File.Exists(copyPath.Filename));
			}
		}

		public void TestRenderPageWithBitmap()
		{
			using (var document = PdfDocument.LoadFile(Path.Combine(TestRepositoryPath, "ABCDEF012345.pdf")))
			{
				var size = document.GetPageSizeInPoints(0).ToSize();
				using (var renderedBitmap = new Bitmap(size.Width, size.Height))
				{
					using (var g = Graphics.FromImage(renderedBitmap))
					{
						document.RenderPage(g, 0, size);
					}

					using (var referenceImage = (Bitmap)Image.FromFile(Path.Combine(TestRepositoryPath, "ABCDEF012345.bmp")))
					{
						var htmlMessage = new StringBuilder("<p>");
						htmlMessage.AppendLine("Images should appear identical: <br />")
							.AppendLine("<table>")
							.AppendLine("<tr><th>Expected</th><th>Actual</th></tr>")
							.AppendLine($"<tr><td>{AsEmbeddedImageTag(referenceImage)}</td><td>{AsEmbeddedImageTag(renderedBitmap)}</td></tr>")
							.AppendLine("</table>")
							.AppendLine("</p>");

						HtmlAssert(htmlMessage.ToString(), ImagesAppearIdentical(renderedBitmap, referenceImage));
					}
				}
			}
		}

		string AsEmbeddedImageTag(Bitmap bmp)
		{
			using (var ms = new MemoryStream())
			{
				bmp.Save(ms, ImageFormat.Png); // Compress it a bit

				return $"<img src=\"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}\" />";
			}
		}

		bool ImagesAppearIdentical(Bitmap a, Bitmap b)
		{
			if (a.Size != b.Size)
			{
				return false;
			}

			for (var x = 0; x < a.Width; x++)
			{
				for (var y = 0; y < a.Height; y++)
				{
					if (a.GetPixel(x, y) != b.GetPixel(x, y))
					{
						return false;
					}
				}
			}

			return true;
		}

		public void TestSave()
		{
			using (var tempFile = TempFile.New())
			{
				using (LoadPdfWithDifferentColorsForEachPage(out var sourceDoc))
				using (var doc = new PdfDocument())
				{
					doc.InsertPages(sourceDoc, 0, 1, 2);

					using (var outstream = new FileStream(tempFile.Filename, FileMode.OpenOrCreate))
					{
						doc.Save(outstream);
					}
				}

				using (var doc = PdfDocument.LoadFile(tempFile.Filename))
				{
					AssertEquals("Should have saved with all the pages we inserted", 2, doc.PageCount);
					AssertArrayEqualsByElements(new[] { Color.Yellow, Color.Lime }, GetPageColors(doc));
				}
			}
		}

		public void TestSave_WriteExceptionsAreHandled()
		{
			var mockStream = new Mock<Stream>();
			mockStream.Setup(s => s.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
				.Throws(new Exception("Boom"));

			using (LoadPdfWithDifferentColorsForEachPage(out var doc))
			{
				AssertExceptionThrown<PdfiumException>("We need to handle any exceptions in the unmanaged delegate, otherwise we'll take the whole CW1 down.", () => doc.Save(mockStream.Object));
			}
		}

		public void TestGCDoesntPrematurelyCollectTheLoadDelegate()
		{
			using (var memoryStream = new MemoryStream(PdfWithDifferentColorsForEachPage))
			using (var document = new PdfDocument(memoryStream))
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				AssertNoExceptionThrown("The delegate should not have been GC'd", () => GetPageColors(document));
			}
		}

		public void TestGetAllText()
		{
			using (var document = PdfDocument.LoadFile(Path.Combine(TestRepositoryPath, "eat_glass.pdf")))
			{
				const string expectedContent = "I can eat glass and it doesn't hurt me.我能吞下玻璃而不伤身体。私はガラスを食べられます。それは私を傷つけません。";
				var pdfContent = document.GetAllText().Replace("\u0000", "");
				AssertEquals("Should get the expected content.", expectedContent, pdfContent);
			}
		}

		public void TestSearch()
		{
			using (var document = PdfDocument.LoadFile(Path.Combine(TestRepositoryPath, "eat_glass.pdf")))
			{
				int counter1 = 0;
				foreach (var found in document.Search(" "))
				{ counter1 += 1; }

				int counter2 = 0;
				foreach (var found in document.Search("。", 0, 0))
				{ counter2 += 1; }

				int counter3 = 0;
				foreach (var found in document.Search("。", 2, 0))
				{ counter3 += 1; }

				int counter4 = 0;
				foreach (var found in document.Search("I can eat glass and it doesn't hurt me"))
				{ counter4 += 1; }

				int counter5 = 0;
				foreach (var found in document.Search("I CAN eat glass and it doesn't hurt me", flags: Native.PdfSearchFlags.MATCHCASE))
				{ counter5 += 1; }

				int counter6 = 0;
				foreach (var found in document.Search("I CAN eat glass and it doesn't hurt me", flags: Native.PdfSearchFlags.NONE))
				{ counter6 += 1; }

				CombineAssertions(() =>
				{
					AssertEquals("Should find all 8 white spaces.", 8, counter1);
					AssertEquals("Should find all 3 '。'.", 3, counter2);
					AssertEquals("Should find 2 '。' on page 3.", 2, counter3);
					AssertEquals("Should find the text.", 1, counter4);
					AssertEquals("Should NOT find the text.", 0, counter5);
					AssertEquals("Should find the text.", 1, counter6);
				});
			}
		}

		public void TestSearchResult()
		{
			using (var document = PdfDocument.LoadFile(Path.Combine(TestRepositoryPath, "eat_glass.pdf")))
			{
				var result = new PdfDocument.SearchResult(0, 0, false);
				foreach (PdfDocument.SearchResult found in document.Search("能吞下玻璃而不伤身体。", 0, 0))
				{
					result = found;
				}

				CombineAssertions(() =>
				{
					AssertEquals("Should find the text on page 2.", true, result.Found);
					AssertEquals("Text should be on page 2.", 1, result.Page);
					AssertEquals("Text should start on character 1.", 1, result.Character);
				});
			}
		}

		void AssertHasPages(string message, PdfDocument document, params Color[] expectedPageColors)
			=> AssertArrayEqualsByElements(message, expectedPageColors, GetPageColors(document));

		Color[] GetPageColors(PdfDocument document)
		{
			using (var bmp = new Bitmap(10, 10))
			using (var g = Graphics.FromImage(bmp))
			{
				var colors = new Color[document.PageCount];
				for (var i = 0; i < colors.Length; i++)
				{
					document.RenderPage(g, i, bmp.Size);
					g.Flush();

					colors[i] = ImageTestingHelpers.AsNamedColor(bmp.GetPixel(5, 5));
				}

				return colors;
			}
		}
	}
}
