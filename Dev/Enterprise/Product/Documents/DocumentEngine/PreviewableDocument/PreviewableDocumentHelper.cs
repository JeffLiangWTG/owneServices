using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.PreviewableDocument
{
	public static class PreviewableDocumentHelper
	{
		public static IEnumerable<string> SupportedFileFormats => PreviewablePdfDocument.SupportedFileFormats.Concat(PreviewableImageDocument.SupportedFileFormats);

		public static bool IsSupported(string extension)
		{
			return SupportedFileFormats.Contains(extension.Trim('.').ToUpperInvariant());
		}

		public static IPreviewableDocument GetPreviewableDocument(string file) => GetPreviewableDocument(file, false);

		public static IPreviewableDocument GetPreviewableDocument(string fileExtension, byte[] fileContents)
		{
			var filename = Temp.GetTempFileNameWithExtension(fileExtension);
			File.WriteAllBytes(filename, fileContents);
			try
			{
				return GetPreviewableDocument(filename, true);
			}
			catch
			{
				TempFile.TryDeleteHandleAllExceptions(filename, out _);
				throw;
			}
		}

		static IPreviewableDocument GetPreviewableDocument(string file, bool deleteOnDispose)
		{
			return IsPdf(file) ?
					new PreviewablePdfDocument(file, deleteOnDispose) :
					new PreviewableImageDocument(file, deleteOnDispose);
		}

		public static bool IsPdf(string filename)
		{
			if (filename.EndsWith(PreviewablePdfDocument.PdfFileExtension, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			try
			{
				using (var stream = File.OpenRead(filename))
				{
					return IsPdf(stream);
				}
			}
			catch (IOException ex) { throw new UnreadableDocumentException(ex);  }
		}

		public static bool IsPdf(Stream file)
		{
			var header = new byte[4];
			return file.Read(header, 0, 4) == 4 &&
				IsPdf(header);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File header literals are not translatable")]
		public static bool IsPdf(byte[] fileContents)
		{
			// http://stackoverflow.com/questions/6186980/determine-if-a-byte-is-a-pdf-file
			const string pdfFileHeaderLiteral = "%PDF";

			return fileContents.Length >= 4 && Encoding.UTF8.GetString(fileContents, 0, 4) == pdfFileHeaderLiteral;
		}

		public static IPreviewableDocument Clone(this IPreviewableDocument document) =>
			document.ExtractPages(Enumerable.Range(0, document.NumberOfPages).ToArray());

		public static string GetUnsupportedPasteMessage(IPreviewableDocument file)
		{
			return file is PreviewablePdfDocument ?
				Res.GetString("BC813BFA-C3F4-4AFA-8265-40A614836D41", "The following files could not be pasted because they are not in a recognized PDF file format.") :
				Res.GetString("23F5DA10-7364-450B-BC1B-55668CA4642C", "The following files could not be pasted because they are not in a recognized image file format.");
		}

		public static bool IsAllowedToInsert(IPreviewableDocument originDocument, string file, out IPreviewableDocument newDocument)
		{
			newDocument = null;
			if (originDocument is PreviewablePdfDocument && IsPdf(file))
			{
				newDocument = new PreviewablePdfDocument(file, false);
			}
			else if (originDocument is PreviewableImageDocument && IsImage(file))
			{
				newDocument = new PreviewableImageDocument(file, false);
			}

			return newDocument != null;
		}

		static bool IsImage(string file)
		{
			try
			{
				using (var image = Image.FromFile(file))
				{ }
				return true;
			}
			catch { }

			return false;
		}
	}
}
