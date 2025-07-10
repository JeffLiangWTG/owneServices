using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CargoWise.Application;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.PreviewableDocument;

namespace Enterprise.DocumentScanning.Business
{
	public interface IBarcodeReader
	{
		Tuple<BaseBarcode, ICollection<string>>[] ProcessDocumentForBarcodes(IPreviewableDocument document);
	}
#if DEBUG
	public
#endif
	class BarcodeReaderImpl : IBarcodeReader
	{
		readonly DocumentFactory factory;
		readonly string jobTypeOverride;

		public BarcodeReaderImpl(DocumentFactory factory, string jobType)
		{
			this.factory = factory;
			this.jobTypeOverride = jobType;
		}

		Tuple<BaseBarcode, ICollection<string>> ProcessPageForBarcodes(IPreviewableDocument document, int pageNb)
		{
			var size = GetCanvasSize(document.GetPageSize(pageNb), document.IsVector);
			using (var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb))
			{
				using (var g = Graphics.FromImage(bitmap))
				{
					g.FillRectangle(Brushes.White, 0, 0, size.Width, size.Height);
					document.Render(g, pageNb, size);
				}

				var barcodeSorter = new BarcodeSorter(factory);
				var barcode = barcodeSorter.ProcessPageForBarcodes(bitmap, jobTypeOverride);
				var otherBarcodes = barcodeSorter.ScannedBarcodesOnPage.Cast<string>().ToArray();

				return new Tuple<BaseBarcode, ICollection<string>>(barcode, otherBarcodes);
			}
		}

		static Size GetCanvasSize(Size pageSize, bool isVector)
		{
			var prefferredSize = isVector ? Scale(pageSize, (float)Math.Sqrt(4)) : pageSize;
			return BarcodeSorter.GetBestSizeForScanner(prefferredSize);
		}

		static Size Scale(Size a, float scale)
			=> ObjectFactory.Get<IDpiScalingHelper>().NewScaledSize((int)(a.Width * scale), (int)(a.Height * scale), false);

		public Tuple<BaseBarcode, ICollection<string>>[] ProcessDocumentForBarcodes(IPreviewableDocument document)
		{
			return Enumerable.Range(0, document.NumberOfPages).Select(i => ProcessPageForBarcodes(document, i)).ToArray();
		}
	}
#if DEBUG
	public
#endif
	class BarcodeFileSplitter : IFileSplitter
	{
		readonly bool isUsingCoversheet;
		readonly bool newDocumentForEachPage;
		readonly string jobTypeOverride;
		readonly string docTypeOverride;
		readonly IBarcodeReader barcodeReader;

		public BarcodeFileSplitter(DocumentFactory factory, bool newDocumentForEachPage, bool isUsingCoversheet, string jobTypeOverride = "", string docTypeOverride = "")
			: this(new BarcodeReaderImpl(factory, jobTypeOverride), newDocumentForEachPage, isUsingCoversheet, jobTypeOverride, docTypeOverride)
		{ }

#if DEBUG
		public
#else
		internal
#endif
		BarcodeFileSplitter(IBarcodeReader reader, bool newDocumentForEachPage, bool isUsingCoversheet, string jobTypeOverride = "", string docTypeOverride = "")
		{
			this.barcodeReader = reader;
			this.newDocumentForEachPage = newDocumentForEachPage;
			this.isUsingCoversheet = isUsingCoversheet;
			this.jobTypeOverride = jobTypeOverride;
			this.docTypeOverride = docTypeOverride;
		}

		static bool ShouldBeAddedToDocument(BaseBarcode barcode, bool isUsingCoverSheet)
			=> barcode == null || !BarcodeHelper.IsPlaceholderBarcode(barcode.FullBarcodeText) || !isUsingCoverSheet || IsBarcodeWithoutData(barcode);

		static bool IsBarcodeWithoutData(BaseBarcode barcode)
			=> string.IsNullOrEmpty(barcode.DocManagerCode) && string.IsNullOrEmpty(barcode.RefCode) && string.IsNullOrEmpty(barcode.DocType);

		bool nextBarcodeWillDefinitlyBeNewDocument = true;
		int FindFirstPageToKeep(Tuple<BaseBarcode, ICollection<string>>[] pages, int startPage, ref string refType, ref string refCode, ref string docType, ref string companyCode)
		{
			// Scans the barcode page, extracting the lovely, lovely data.
			// Returns when we've extracted the relevant data and want to start adding pages
			int i;
			for (i = startPage; i < pages.Length; i++)
			{
				var barcode = pages[i].Item1;
				if (barcode != null)
				{
					if (nextBarcodeWillDefinitlyBeNewDocument)
					{
						refType = refCode = docType = companyCode = string.Empty;
					}

					if (BarcodeHelper.IsRefTypeEncoded(barcode.FullBarcodeText))
					{
						refType = barcode.DocManagerCode;
						refCode = barcode.RefCode;
					}

					if (BarcodeHelper.IsDocTypeEncoded(barcode.FullBarcodeText))
					{
						docType = barcode.DocType;
					}

					if (BarcodeHelper.IsCompanySpecificBarcode(barcode.FullBarcodeText))
					{
						companyCode = barcode.CompanyCode;
					}

					nextBarcodeWillDefinitlyBeNewDocument = !BarcodeHelper.IsDocTypeBarcode(barcode.FullBarcodeText);
				}

				if (ShouldBeAddedToDocument(barcode, isUsingCoversheet))
				{
					break;
				}
			}

			if (!isUsingCoversheet)
			{
				if (!string.IsNullOrEmpty(jobTypeOverride))
				{ refType = jobTypeOverride; }
				if (!string.IsNullOrEmpty(docTypeOverride))
				{ docType = docTypeOverride; }
			}

			return i;
		}

		int FindNextNewDocumentBarcode(Tuple<BaseBarcode, ICollection<string>>[] pages, int startPage)
		{
			for (var i = startPage; i < pages.Length; i++)
			{
				if (pages[i].Item1 != null)
				{
					return i;
				}
			}

			return pages.Length;
		}

		public IEnumerable<FileSplitParameters> SplitFile(IPreviewableDocument document)
		{
			var results = new List<FileSplitParameters>();
			var pageBarcodes = barcodeReader.ProcessDocumentForBarcodes(document);

			string refType = string.Empty, refCode = string.Empty, docType = string.Empty, companyCode = string.Empty;
			for (var pageNb = 0; pageNb < document.NumberOfPages; /* Spooky lack of incrementor!!! */)
			{
				var firstPageToKeep = FindFirstPageToKeep(pageBarcodes, pageNb, ref refType, ref refCode, ref docType, ref companyCode);
				var endOfSplit = firstPageToKeep + 1;

				var barcodeWasADud = string.IsNullOrEmpty(refType) && string.IsNullOrEmpty(docType) && string.IsNullOrEmpty(refCode);
				if (!newDocumentForEachPage && !barcodeWasADud)
				{
					endOfSplit = FindNextNewDocumentBarcode(pageBarcodes, firstPageToKeep + 1);
				}

				var isEmptySplit = firstPageToKeep == endOfSplit || endOfSplit > document.NumberOfPages;
				if (!isEmptySplit)
				{
					var barcodePagesOfSplit = pageBarcodes.Skip(pageNb).Take(endOfSplit - pageNb);
					var splitBarcodes = barcodePagesOfSplit.SelectMany(s => s.Item2).ToList();
					var firstBarcode = barcodePagesOfSplit.FirstOrDefault(s => s.Item1 != null)?.Item1;
					var barcodeWasEmpty = new[] { refCode, refType, docType }.All(string.IsNullOrEmpty);

					results.Add(new FileSplitParameters(refType, refCode, docType, companyCode, firstPageToKeep, endOfSplit, firstBarcode, splitBarcodes, !barcodeWasEmpty));
				}

				pageNb = endOfSplit;
			}

			return results;
		}
	}
}
