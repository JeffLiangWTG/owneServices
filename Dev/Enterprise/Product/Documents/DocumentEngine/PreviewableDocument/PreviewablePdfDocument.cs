using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.PreviewableDocument
{
	class PreviewablePdfDocument : Disposable, IPreviewableDocument
	{
		public const string PdfFileExtension = "PDF";
		public static IEnumerable<string> SupportedFileFormats => new[] { PdfFileExtension };

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public string PreferredExtension => PdfFileExtension.ToLowerInvariant();

		public bool IsDocumentCorrupted { get; }

		readonly string pdfFilePath;
		readonly bool deleteOnDispose;

		public PreviewablePdfDocument(string pdfFilePath)
			: this(pdfFilePath, false)
		{
		}

		internal PreviewablePdfDocument(string pdfFile, bool deleteOnDispose)
		{
			this.pdfFilePath = Argument.NotNull(pdfFile, nameof(pdfFile));
			this.deleteOnDispose = deleteOnDispose;
		}

		PdfDocument loadedFile;
		PdfDocument LoadedFile => loadedFile ?? (loadedFile = LoadFile(pdfFilePath));

		static PdfDocument LoadFile(string pdfFilePath)
		{
			try
			{
				return PdfDocument.LoadFile(pdfFilePath);
			}
			catch (PdfiumException ex)
			{
				throw new CorruptedDocumentException("Could not load the PDF", ex);
			}
		}

		public bool IsVector => true;

		public int NumberOfPages => LoadedFile.PageCount;

		readonly Dictionary<int, RenderPageRotation> currentRotations = new Dictionary<int, RenderPageRotation>();
		RenderPageRotation GetPageRotation(int pageNb)
			=> currentRotations.TryGetValue(pageNb, out var currentRotation) ? currentRotation : RenderPageRotation.CW0;

		void CheckPageNumber(int pageNb)
		{
			if (pageNb < 0 || pageNb >= NumberOfPages)
			{
				throw new ArgumentException("Page number is out of range");
			}
		}

		public void RotatePage(int pageNb, bool clockwise)
		{
			CheckPageNumber(pageNb);

			var currentPage = GetPageRotation(pageNb);
			var howManyClockwiseRotations = clockwise ? 1 : 3;
			currentRotations[pageNb] = (RenderPageRotation)((int)(currentPage + howManyClockwiseRotations) % 4);
		}

		public Size GetPageSize(int pageNb)
		{
			// Pdfium's size returns the value in printer points, which is 1/72"
			const float pdfiumScaleValue = 72.0f;

			CheckPageNumber(pageNb);

			var relevantPage = SizeF.Empty;

			try
			{
				relevantPage = LoadedFile.GetPageSizeInPoints(pageNb);
			}
			catch (ArgumentException ex)
			{
				throw new CorruptedDocumentException(ex.Message, ex);
			}

			return ControlDpiScalingHelper.NewScaledSize(
				(int)(ControlDpiScalingHelper.DpiX * relevantPage.Width / pdfiumScaleValue),
				(int)(ControlDpiScalingHelper.DpiY * relevantPage.Height / pdfiumScaleValue),
				false
			);
		}

		public void Render(Graphics g, int pageNb, Size size)
		{
			CheckPageNumber(pageNb);

			LoadedFile.RenderPage(g, pageNb, size.IsEmpty ? GetPageSize(pageNb) : size, GetPageRotation(pageNb));
		}

		public IPreviewableDocument Insert(int insertIndex, IPreviewableDocument file)
		{
			if (insertIndex < 0 || insertIndex > NumberOfPages)
			{
				throw new ArgumentException("The insertion index must be a valid index within the pdf");
			}

			var pdfToInsert = file as PreviewablePdfDocument ?? throw new ArgumentException("The file to insert must be a pdf");

			try
			{
				string tempFile;
				using (var newDocument = new PdfDocument())
				{
					newDocument.InsertPages(LoadedFile, 0, null);
					newDocument.InsertPages(pdfToInsert.LoadedFile, insertIndex, null);
					tempFile = Temp.GetTempFileNameWithExtension(PreferredExtension);

					newDocument.Save(tempFile);
				}

				return new PreviewablePdfDocument(tempFile, true);
			}
			catch (PdfiumException ex) when (IsOperationNotSupportedDueToEncryption(ex))
			{
				throw new UnsupportedDocumentException(ResString.GetMultilingualString("e3cf0c40-9ecf-489a-bd2e-5cb13e9e6923", "it is encrypted"), string.Empty, ex);
			}
		}

		public IPreviewableDocument ExtractPages(int[] pagesToExtract)
		{
			var newFile = Temp.GetTempFileNameWithExtension(PreferredExtension);
			try
			{
				using (var stream = File.Create(newFile))
				{
					ExtractPages(stream, pagesToExtract);
				}

				return new PreviewablePdfDocument(newFile, true);
			}
			catch
			{
				TempFile.TryDeleteHandleAllExceptions(newFile);
				throw;
			}
		}

		public void ExtractPages(Stream stream, int[] pagesToExtract)
		{
			if (pagesToExtract.Length == 0 || pagesToExtract.Min() < 0 || pagesToExtract.Max() >= NumberOfPages)
			{
				throw new ArgumentException("All pages for extraction must be valid page numbers");
			}

			try
			{
				using (var outputDocument = new PdfDocument())
				{
					outputDocument.InsertPages(LoadedFile, 0, pagesToExtract);
					outputDocument.Save(stream);
				}
			}
			catch (PdfiumException ex) when (IsOperationNotSupportedDueToEncryption(ex))
			{
				throw new UnsupportedDocumentException(ResString.GetMultilingualString("e3cf0c40-9ecf-489a-bd2e-5cb13e9e6923", "it is encrypted"), string.Empty, ex);
			}
		}

		static bool IsOperationNotSupportedDueToEncryption(PdfiumException ex) => ex.ErrorType == PdfiumError.UnsupportedEncryption;

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				loadedFile?.Dispose();
				if (deleteOnDispose && !TempFile.TryDeleteHandleAllExceptions(pdfFilePath, out var message))
				{
					ErrorReporter.ReportOnce("FailedToDeleteTempFile", message);
				}
			}
		}

		public void Save(string path)
			=> LoadedFile.Save(path);
	}
}
