using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.PreviewableDocument;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	enum BarcodeType { PlaceholderBarcode, PlaceholderWithNoInformation, NonPlaceholderBarcode, NoBarcode }

	class DummyBarcodeReader : IBarcodeReader
	{
		readonly List<Tuple<BaseBarcode, ICollection<string>>> pages = new List<Tuple<BaseBarcode, ICollection<string>>>();
		readonly DocumentFactory factory;

		public DummyBarcodeReader(DocumentFactory factory)
		{
			this.factory = factory;
		}

		public void AddPage(params string[] barcodes)
		{
			var newBarcode = barcodes.Length > 0 ? new DocTypeBarcode(factory, barcodes.First()) : null;

			pages.Add(new Tuple<BaseBarcode, ICollection<string>>(newBarcode, barcodes));
		}

		public IPreviewableDocument GetDummyDocument()
		{
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.NumberOfPages).Returns(pages.Count);

			return document.Object;
		}

		public Tuple<BaseBarcode, ICollection<string>>[] ProcessDocumentForBarcodes(IPreviewableDocument document)
		{
			return pages.ToArray();
		}
	}

	class TypedDummyBarcodeReader : DummyBarcodeReader
	{
		public const string EmptyPlaceholderString = "^|";

		public TypedDummyBarcodeReader(DocumentFactory factory, BarcodeType[] pages = null)
			: base(factory)
		{
			if (pages != null)
			{
				foreach (var page in pages)
				{
					AddPage(page);
				}
			}
		}

		public void AddPage(BarcodeType type)
		{
			if (type == BarcodeType.NoBarcode)
			{
				AddPage();
			}
			else
			{
				AddPage(GetStringForBarcodeType(type));
			}
		}

		static string GetStringForBarcodeType(BarcodeType type)
		{
			switch (type)
			{
				case BarcodeType.PlaceholderBarcode:
					return "^DDR=00003036;DDR@MEL|";
				case BarcodeType.PlaceholderWithNoInformation:
					return EmptyPlaceholderString;
				case BarcodeType.NonPlaceholderBarcode:
					return "YouHaveNoIdeaHowMuchIHateBarcodes";
				default:
					throw new NotImplementedException();
			}
		}
	}
	sealed class BarcodeFileSplitterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBarcodeReaderWithBarcodeIsSmall()
		{
			using (var document = PreviewableDocumentHelper.GetPreviewableDocument(Path.Combine(TestUtils.TestRepositoryPath, "BarcodeIsSmall.pdf")))
			{
				var result = FormatPageBarcodes(ExtractBarcodes(document));
				AssertEquals("0: P00000044, ^WPI=P00000044;WPS;|", result);
			}
		}

		public void TestEmptyPlaceholderIsActuallyThat()
		{
			Assert("PRE: For the other tests to work this one must", BarcodeHelper.IsPlaceholderBarcode(TypedDummyBarcodeReader.EmptyPlaceholderString));
		}

		string GetPageSplits(params BarcodeType[] pages)
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new TypedDummyBarcodeReader(factory, pages);
			var splitter = new BarcodeFileSplitter(reader, false, true, string.Empty, string.Empty);

			return FormatSplits(splitter.SplitFile(reader.GetDummyDocument()));
		}

		string FormatSplits(IEnumerable<FileSplitParameters> splits)
		{
			return string.Join(", ", splits.Select(split => string.Format(CultureInfo.InvariantCulture, "{0}-{1}", split.StartPageInclusive, split.EndPageExclusive)));
		}

		public void TestBarcodeReader_RasterDocumentsAreUnscaled()
		{
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.GetPageSize(0)).Returns(new Size(200, 200));
			document.Setup(m => m.NumberOfPages).Returns(1);
			document.Setup(m => m.IsVector).Returns(false);

			document.Setup(m => m.Render(It.IsAny<Graphics>(), It.IsAny<int>(), It.IsAny<Size>()))
				.Callback<Graphics, int, Size>((g, pageNo, size) =>
				{
					AssertEquals("For raster image we should use their ", new Size(200, 200), size);
				});

			ExtractBarcodes(document.Object);
			document.VerifyAll();
		}

		public void TestBarcodeReader_VectorDocumentsAreScaled()
		{
			var originalSize = new Size(100, 200);
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.GetPageSize(0)).Returns(originalSize);
			document.Setup(m => m.NumberOfPages).Returns(1);
			document.Setup(m => m.IsVector).Returns(true);

			document.Setup(m => m.Render(It.IsAny<Graphics>(), It.IsAny<int>(), It.IsAny<Size>()))
				.Callback<Graphics, int, Size>((g, pageNo, size) =>
				{
					var scaledSize = size;
					var scale = Math.Round((scaledSize.Width * scaledSize.Height) / (double)(originalSize.Width * originalSize.Height), 1);
					AssertEquals("For vector documents we should slightly increase their size to reduce the chance of aliasing/AA buggering up the barcode. A scale factor of 2 was arbitrarily chosen as it consumes a reasonable amount of memory.", 4.0, scale);
				});

			ExtractBarcodes(document.Object);
			document.VerifyAll();
		}

		public void TestBarcodeReader_VectorDocumentsHaveMaxSizeRestriction()
		{
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.GetPageSize(0)).Returns(new Size(10000, 10000));
			document.Setup(m => m.NumberOfPages).Returns(1);
			document.Setup(m => m.IsVector).Returns(true);

			document.Setup(m => m.Render(It.IsAny<Graphics>(), It.IsAny<int>(), It.IsAny<Size>()))
				.Callback<Graphics, int, Size>((g, pageNo, size) =>
				{
					AssertEquals("There is no benefit to rasterizing to a size larger than what the barcode scanner can use", new Size((int)BarcodeSorter.maxShortSide, (int)BarcodeSorter.maxShortSide), size);
				});

			ExtractBarcodes(document.Object);
			document.VerifyAll();
		}

		public void TestBarcodeReader_RasterImageSizeRestriction()
		{
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.GetPageSize(0)).Returns(new Size(10000, 10000));
			document.Setup(m => m.NumberOfPages).Returns(1);
			document.Setup(m => m.IsVector).Returns(false);

			document.Setup(m => m.Render(It.IsAny<Graphics>(), It.IsAny<int>(), It.IsAny<Size>()))
				.Callback<Graphics, int, Size>((g, pageNo, size) =>
				{
					AssertEquals("There is no benefit to drawing to a size larger than what the barcode scanner can use", new Size((int)BarcodeSorter.maxShortSide, (int)BarcodeSorter.maxShortSide), size);
				});

			ExtractBarcodes(document.Object);
			document.VerifyAll();
		}

		public void TestBarcodeReader_RatioIsMaintained()
		{
			var document = new Mock<IPreviewableDocument>();
			document.Setup(m => m.GetPageSize(0)).Returns(new Size(4000, 2000));
			document.Setup(m => m.NumberOfPages).Returns(1);
			document.Setup(m => m.IsVector).Returns(false);

			document.Setup(m => m.Render(It.IsAny<Graphics>(), It.IsAny<int>(), It.IsAny<Size>()))
				.Callback<Graphics, int, Size>((g, pageNo, size) =>
				{
					AssertEquals("There is no benefit to drawing to a size larger than what the barcode scanner can use", new Size(3000, 1500), size);
				});

			ExtractBarcodes(document.Object);
			document.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTrickyFileThatConfusesBarcodeScanner()
		{
			var expected = new[]
			{
				new [] { "^SHP=S00001410|" },
				new [] { "^DOC=ARN|" },
				Array.Empty<string>(),
			};

			using (var document = PreviewableDocumentHelper.GetPreviewableDocument(Path.Combine(TestUtils.TestRepositoryPath, "saveaspdf.pdf")))
			{
				AssertMultilineASCIIEquals("This document has caused issues for the barcode scanner before thanks to some bad aliasing. We should be able to read the barcodes though.", FormatPageBarcodes(expected), FormatPageBarcodes(ExtractBarcodes(document)));
			}
		}

		string[][] ExtractBarcodes(IPreviewableDocument document)
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new BarcodeReaderImpl(factory, string.Empty);

			return reader.ProcessDocumentForBarcodes(document).Select(b => b.Item2.ToArray()).ToArray();
		}

		string FormatPageBarcodes(string[][] barcodes)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < barcodes.Length; i++)
			{
				sb.AppendFormat(CultureInfo.CurrentCulture, "{0}: {1}", i, string.Join(", ", barcodes[i]));
			}
			return sb.ToString();
		}

		public void TestSplitFileWithSingleBarcode()
		{
			// Based on TestImportFromDirectoryWithCompanyInBarcode
			const string barcodeValue = "^DDR=00003036;DDR@MEL|";
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			var barcodeReader = new DummyBarcodeReader(factory);
			barcodeReader.AddPage(barcodeValue);
			barcodeReader.AddPage();
			barcodeReader.AddPage();

			var splitter = new BarcodeFileSplitter(barcodeReader, false, true, string.Empty, string.Empty);
			var splits = splitter.SplitFile(barcodeReader.GetDummyDocument());

			AssertEquals("There was one barcode for the three pages", 1, splits.Count());

			var split = splits.Single();
			AssertEquals("Should skip the first page because it is just a barcode", 1, split.StartPageInclusive);
			AssertEquals("Should end on the final page", 3, split.EndPageExclusive);
			AssertEquals("Should contain the properties of the barcode", "DDR", split.ReferenceType);
		}

		public void TestIsReturningTheExpectedBarcodes()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var barcodeReader = new DummyBarcodeReader(factory);
			barcodeReader.AddPage("^DDR=00003036;DDR@MEL|", "SomeRandomBarcode");
			barcodeReader.AddPage("^DDR=10101010;FFS|");
			barcodeReader.AddPage();

			var splitter = new BarcodeFileSplitter(barcodeReader, false, true, string.Empty, string.Empty);

			var split = splitter.SplitFile(barcodeReader.GetDummyDocument()).Single();
			AssertEquals("^DDR=00003036;DDR@MEL|, SomeRandomBarcode, ^DDR=10101010;FFS|", string.Join(", ", split.ScannedBarcodes));
		}

		public void TestBarcodesOfIncludedPagesAreKeptInScannedBarcodes()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var barcodeReader = new DummyBarcodeReader(factory);
			barcodeReader.AddPage("^DDR=00003036;DDR@MEL|", "SomeRandomBarcode");
			barcodeReader.AddPage();

			var splitter = new BarcodeFileSplitter(barcodeReader, false, true, string.Empty, string.Empty);
			var split = splitter.SplitFile(barcodeReader.GetDummyDocument()).Single();
			AssertEquals("^DDR=00003036;DDR@MEL|, SomeRandomBarcode", string.Join(", ", split.ScannedBarcodes));
		}

		DummyBarcodeReader GetDummyReaderMatchingMultipageTestDocument(DocumentFactory factory)
		{
			var barcodeReader = new DummyBarcodeReader(factory);

			barcodeReader.AddPage("^SHP=S00001000|");
			barcodeReader.AddPage("^DOC=CIV|");
			barcodeReader.AddPage();
			barcodeReader.AddPage("[ROHHBLSYDLAX12345678901234567890]");
			barcodeReader.AddPage();
			barcodeReader.AddPage("^DOC=PKL|");
			barcodeReader.AddPage();
			barcodeReader.AddPage();
			barcodeReader.AddPage("^DOC=MAN|");
			barcodeReader.AddPage();

			return barcodeReader;
		}

		public void TestBarcodeIncludedInDocument()
		{
			// Based on TestImportWithBarcodeAsIncludedDocument
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new DummyBarcodeReader(factory);
			reader.AddPage("^DEC=B00001000;INS;|");
			reader.AddPage();
			reader.AddPage("^DOC=AGI;|");
			reader.AddPage();
			reader.AddPage("^DEC=B00001001|");
			reader.AddPage();
			reader.AddPage("^DOC=INS|");
			reader.AddPage();

			var splitter = new BarcodeFileSplitter(reader, newDocumentForEachPage: false, isUsingCoversheet: true);
			AssertEquals("0-2, 2-4, 5-6, 7-8", FormatSplits(splitter.SplitFile(reader.GetDummyDocument())));
		}

		public void TestMultipageTiffDoc_NewDocumentForEachPage()
		{
			// Based on TestExecuteSortMultipageFileWithAutomaticSingle
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var barcodeReader = GetDummyReaderMatchingMultipageTestDocument(factory);

			var splitter = new BarcodeFileSplitter(barcodeReader, newDocumentForEachPage: true, isUsingCoversheet: true);
			AssertEquals("2-3, 3-4, 4-5, 6-7, 7-8, 9-10", FormatSplits(splitter.SplitFile(barcodeReader.GetDummyDocument())));
		}

		public void TestNoCoverSheet_NotOverridenValues()
		{
			// Based on TestImportFileWithoutUsingCoverSheetMultiPagesTif
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var barcodeReader = GetDummyReaderMatchingMultipageTestDocument(factory);

			var splitter = new BarcodeFileSplitter(barcodeReader, newDocumentForEachPage: false, isUsingCoversheet: false, jobTypeOverride: "SHP", docTypeOverride: "ARN");

			AssertEquals("0-1, 1-3, 3-5, 5-8, 8-10", FormatSplits(splitter.SplitFile(barcodeReader.GetDummyDocument())));
		}

		public void TestNoCoverSheet_OverridenValues()
		{
			// Based on TestImportFileWithoutUsingCoverSheetMultiPagesTif
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var barcodeReader = GetDummyReaderMatchingMultipageTestDocument(factory);

			var splitter = new BarcodeFileSplitter(barcodeReader, false, false, "SHP", "ARN");

			AssertEquals("0-1, 1-3, 3-5, 5-8, 8-10", FormatSplits(splitter.SplitFile(barcodeReader.GetDummyDocument())));
		}

		public void TestSplitFileWithNoBarcode()
		{
			var pages = GetPageSplits(BarcodeType.NoBarcode, BarcodeType.NoBarcode, BarcodeType.NoBarcode);

			AssertEquals("There was no barcode, so each page should have been split.", "0-1, 1-2, 2-3", pages);
		}

		public void TestSplitFileOneBarcode()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode, BarcodeType.NoBarcode);

			AssertEquals("There was no barcode, so each page should have been split.", "1-3", pages);
		}

		public void TestSplitFileMultipleBarcodeSplits()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode, BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode);

			AssertEquals("There was no barcode, so each page should have been split.", "1-2, 3-4", pages);
		}

		public void TestSplitFileMultipleMultipleSkips()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderBarcode, BarcodeType.PlaceholderBarcode, BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode);

			AssertEquals("Should skip all the barcode pages", "3-4", pages);
		}

		public void TestSplitFileWithOnlyBarcodes()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderBarcode, BarcodeType.PlaceholderBarcode, BarcodeType.PlaceholderBarcode);

			AssertEquals("There was no pages to grab", "", pages);
		}

		public void TestSplitFileWithBarcodeAtEnd()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode, BarcodeType.PlaceholderBarcode);
			AssertEquals("Page at the end should not result in a split", "1-2", pages);
		}

		public void TestSplitFileNonPlaceholderBarcode()
		{
			var pages = GetPageSplits(BarcodeType.NonPlaceholderBarcode, BarcodeType.NoBarcode);
			AssertEquals("Non placeholder barcodes are basically returnable pages", "0-1, 1-2", pages);
		}

		public void TestSplitFileNonPlaceholderBarcodeSecond()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderBarcode, BarcodeType.NonPlaceholderBarcode, BarcodeType.NoBarcode, BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode);
			AssertEquals("Non placeholder barcodes are basically returnable pages", "1-3, 4-5", pages);
		}

		public void TestSplitFileWithNonFruitfulPlaceholder_DoesntSkipAnything()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderWithNoInformation, BarcodeType.PlaceholderWithNoInformation, BarcodeType.NoBarcode, BarcodeType.PlaceholderBarcode, BarcodeType.NoBarcode, BarcodeType.NoBarcode);
			AssertEquals("Should keep all pages, but without useful information don't group the first 3 pages", "0-1, 1-2, 2-3, 4-6", pages);
		}

		public void TestSplitFileWithNonFruitfulPlaceholder_DoesntGroupFiles()
		{
			var pages = GetPageSplits(BarcodeType.PlaceholderWithNoInformation, BarcodeType.NoBarcode, BarcodeType.NoBarcode);
			AssertEquals("Should still skip the pages, but without useful information don't group other docs", "0-1, 1-2, 2-3", pages);
		}

		public void TestDocTypeBarcodeWithoutUsefulInformationWithCoverPageDoesntGoOutOfRange_SinglePage()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new DummyBarcodeReader(factory);
			reader.AddPage("^W I* , %*U[FNC4];WPS)|", "P00051053");

			var splitter = new BarcodeFileSplitter(reader, false, isUsingCoversheet: true);
			var pages = FormatSplits(splitter.SplitFile(reader.GetDummyDocument()));
			AssertEquals("When the document has only one page and the page has an unknown barcode, we should include it even if isUsingCoverSheet is true", "0-1", pages);
		}

		public void TestDocTypeBarcodeWithUsefulInformationWithCoverPageDoesntGoOutOfRange_SinglePage()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new TypedDummyBarcodeReader(factory);
			reader.AddPage(BarcodeType.PlaceholderBarcode);

			var splitter = new BarcodeFileSplitter(reader, false, isUsingCoversheet: true);
			var pages = FormatSplits(splitter.SplitFile(reader.GetDummyDocument()));
			AssertEquals("When the document has only one page and the page has a valid barcode, we should not include it if isUsingCoverSheet is true", "", pages);
		}

		public void TestDocTypeBarcodeWithoutUsefulInformationWithCoverPageDoesntGoOutOfRange_MultiPage()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new TypedDummyBarcodeReader(factory);
			reader.AddPage(BarcodeType.PlaceholderBarcode);
			reader.AddPage("NotDocTypeBarcode");
			reader.AddPage("StillNotDocTypeBarcode");
			reader.AddPage("^W I* , %*U[FNC4];WPS)|", "P00051053");

			var splitter = new BarcodeFileSplitter(reader, false, isUsingCoversheet: true);
			var pages = FormatSplits(splitter.SplitFile(reader.GetDummyDocument()));
			AssertEquals("When the document has multi pages and the last page has an unknown barcode, we should include it even if isUsingCoverSheet is true", "1-2, 2-3, 3-4", pages);
		}

		public void TestDocTypeBarcodeWithUsefulInformationWithCoverPageDoesntGoOutOfRange_MultiPage()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reader = new TypedDummyBarcodeReader(factory);
			reader.AddPage(BarcodeType.PlaceholderBarcode);
			reader.AddPage("NotDocTypeBarcode");
			reader.AddPage("StillNotDocTypeBarcode");
			reader.AddPage(BarcodeType.PlaceholderBarcode);

			var splitter = new BarcodeFileSplitter(reader, false, isUsingCoversheet: true);
			var pages = FormatSplits(splitter.SplitFile(reader.GetDummyDocument()));
			AssertEquals("When the document has multi pages and the last page has a valid barcode, we should not include it if isUsingCoverSheet is true", "1-2, 2-3", pages);
		}
	}
}
