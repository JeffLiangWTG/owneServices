using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.PreviewableDocument
{
	class PreviewableImageDocument : Disposable, IPreviewableDocument
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File types are not translatable")]
		const string TiffExtension = "tiff";

		readonly string filename;
		readonly bool deleteOnDispose;

		public PreviewableImageDocument(string filename)
			: this(filename, false)
		{
		}

		internal PreviewableImageDocument(string filename, bool deleteOnDispose)
		{
			this.filename = Argument.NotNullOrEmpty(filename, nameof(filename));
			this.deleteOnDispose = deleteOnDispose;
		}

		public static IEnumerable<string> SupportedFileFormats => new[] { "BMP", "GIF", "JPG", "JPEG", "PNG", "TIF", "TIFF" };

		public bool IsVector => false;

		Image loadedImage;
		Image LoadedImage
		{
			get
			{
				if (loadedImage == null)
				{
					try
					{
						loadedImage = Image.FromFile(filename);
					}
					catch (IOException ex) { throw new UnreadableDocumentException(ex); }
					catch (Exception ex) when (ex is OutOfMemoryException || IsCorruptedDocumentException(ex))
					{
						throw new CorruptedDocumentException(null, ex);
					}
				}
				return loadedImage;
			}
		}

		public bool IsDocumentCorrupted { get; private set; }

		public int NumberOfPages
			=> LoadedImage.FrameDimensionsList.Contains(FrameDimension.Page.Guid) ?
				LoadedImage.GetFrameCount(FrameDimension.Page) :
				1;

		public string PreferredExtension => Path.GetExtension(filename).TrimStart('.');

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

			string tempFile = null;
			var fileReader = new ImageFileReaderWithLock(filename);
			try
			{
				fileReader.PageSelector.CurrentPageIndex = pageNb;
				if (fileReader.PageSelector.CurrentImage == null)
				{
					return;
				}

				tempFile = RotateDocument.RotateActivePage(clockwise, fileReader.PageSelector);
				fileReader.Dispose();
				fileReader = null;

				loadedImage?.Dispose();
				loadedImage = null;

				File.Copy(tempFile, filename, true);
			}
			finally
			{
				fileReader?.Dispose();
				TempFile.TryDeleteHandleAllExceptions(tempFile);
			}
		}

		public void Save(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));

			if (NumberOfPages == 1)
			{
				LoadedImage.Save(path);
			}
			else
			{
				FileSaveHelper.SaveMultiPageTifFile((byte[])null, path, loadedImage);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				loadedImage?.Dispose();

				if (deleteOnDispose)
				{
					if (!TempFile.TryDeleteHandleAllExceptions(filename, out var message))
					{
						ErrorReporter.ReportOnce("FailedToDeleteTempFile", message);
					}
				}
			}
		}

		bool IsCorruptedDocumentException(Exception ex)
		{
			return ex is ExternalException && ex.Message.Contains("GDI+");
		}

		public Size GetPageSize(int pageNb)
		{
			CheckPageNumber(pageNb);

			try
			{
				LoadedImage.SelectActiveFrame(FrameDimension.Page, pageNb);
			}
			catch (Exception ex) when (IsCorruptedDocumentException(ex))
			{
				IsDocumentCorrupted = true;
				throw new CorruptedDocumentException(null, ex);
			}

			return LoadedImage.Size;
		}

		public void Render(Graphics g, int pageNb, Size size)
		{
			Argument.NotNull(g, nameof(g));
			CheckPageNumber(pageNb);

			LoadedImage.SelectActiveFrame(FrameDimension.Page, pageNb);
			g.DrawImage(LoadedImage, 0, 0, size.Width, size.Height);
		}

		public IPreviewableDocument Insert(int insertIndex, IPreviewableDocument file)
		{
			if (insertIndex < 0 || insertIndex > NumberOfPages)
			{
				throw new ArgumentException("Insertion index is out of range");
			}

			var imageFile = file as PreviewableImageDocument ?? throw new ArgumentException("The file must be a PreviewableImageDocument");

			var newFile = Temp.GetTempFileNameWithExtension(TiffExtension);

			Save(newFile);
			DocumentImagePageWriterUtils.InsertMultiPageImageToFile(imageFile.filename, newFile, insertIndex - 1);

			return new PreviewableImageDocument(newFile, true);
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

				return new PreviewableImageDocument(newFile, true);
			}
			catch
			{
				TempFile.TryDeleteHandleAllExceptions(newFile);
				throw;
			}
		}

		public void ExtractPages(Stream outputStream, int[] pagesToExtract)
		{
			if (pagesToExtract.Length == 0 || pagesToExtract.Min() < 0 || pagesToExtract.Max() >= NumberOfPages)
			{
				throw new ArgumentException("All pages for extraction must be valid page numbers");
			}

			using (var imageSelector = new StandardImagePageSelector(LoadedImage))
			{
				var outputImage = CreateInitialImage(imageSelector, pagesToExtract.First(), outputStream);
				foreach (var pageIndex in pagesToExtract.Skip(1))
				{
					imageSelector.CurrentPageIndex = pageIndex;
					outputImage.SaveAdd(imageSelector.CurrentImage, GetEncoderParameters(EncoderValue.FrameDimensionPage));
				}
			}
		}

		static Image CreateInitialImage(StandardImagePageSelector pageSelector, int firstPageIndex, Stream outputStream)
		{
			using (var stream = new MemoryStream())
			{
				pageSelector.CurrentPageIndex = firstPageIndex;
				pageSelector.CurrentImage.Save(stream, ImageFormat.Tiff);

				var outputImage = Image.FromStream(stream);
				outputImage.Save(outputStream, FileSaveHelper.GetTiffEncoder(), GetEncoderParameters(EncoderValue.MultiFrame));

				return outputImage;
			}
		}

		static EncoderParameters GetEncoderParameters(EncoderValue value)
		{
			var @params = new EncoderParameters();
			@params.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)value);
			return @params;
		}
	}
}
