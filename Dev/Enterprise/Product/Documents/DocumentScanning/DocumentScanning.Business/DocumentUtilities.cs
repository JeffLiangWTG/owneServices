using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.DocumentScanning.Business.Res;

namespace Enterprise.DocumentScanning
{
	public class DocumentUtilities : Integration.IDocumentUtilities
	{
		public static Image GetDocumentImageFromFile(string filePath)
		{
			var stream = new MemoryStream(GetFileAsBytes(filePath));

			return Image.FromStream(stream);
		}

		/// <summary>
		/// Returns a new temp filename. Does NOT create an empty file at the location.
		/// </summary>
		public static string GetTempFilename()
		{
			return GetTempFilename((NoResString)"tif");
		}

		public static string GetTempFilename(string extension)
		{
			return Path.Combine(Env.TempPath, Guid.NewGuid() + "." + extension);
		}

		public static int[] GetInverseList(int[] aList, int totalPages)
		{
			var result = new List<int>();

			for (var i = 0; i < totalPages; ++i)
			{
				if (!aList.Contains(i))
				{
					result.Add(i);
				}
			}

			return result.ToArray();
		}

		public static string GetReorderPagesComment(int[] aPagesToMove, int aDestIndex)
		{
			var commentBuilder = new StringBuilder();

			if (aPagesToMove.Length > 0)
			{
				if (aPagesToMove.Length == 1)
				{
					commentBuilder.Append(Res.GetString("a3ce8778-edf5-4249-b440-c181b93e6768", "Page")).Append(" ");
				}
				else
				{
					commentBuilder.Append(Res.GetString("0f4fdf80-1a4c-46a6-93c6-2e1a36bcf23c", "Pages")).Append(" ");
				}

				for (var i = 0; i < aPagesToMove.Length; ++i)
				{
					if (i != 0)
					{
						commentBuilder.Append(",");
					}

					commentBuilder.Append((aPagesToMove[i] + 1).ToString()); // make page numbers 1 based.
				}

				if (aDestIndex == -1)
				{
					commentBuilder.Append(" ").Append(Res.GetString("24325449-b100-4332-9d31-af7f5032bff4", "moved before page 1"));
				}
				else
				{
					commentBuilder.Append(" ").Append(Res.GetString("6b08e81b-cbfa-457f-a676-56a8ff892983", "moved after page")).Append(" ");
					commentBuilder.Append((aDestIndex + 1).ToString());
				}
			}

			return commentBuilder.ToString();
		}

		byte[] Integration.IDocumentUtilities.GetFileAsBytes(string filePath)
		{
			return GetFileAsBytes(filePath);
		}

		public static byte[] GetFileAsBytes(string filePath)
		{
			try
			{
				// If we don't explicitly give a buffer size, use the standard way to read a file
				int bufferSize = DocManagerRegistry.Instance.FileUploadStreamBufferSize.Value;
				if (bufferSize < 1)
				{
					return File.ReadAllBytes(filePath);
				}
				else
				{
					using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize))
					using (var ms = new MemoryStream())
					{
						fs.CopyTo(ms);
						return ms.ToArray();
					}
				}
			}
			catch (IOException ex)
			{
				if (ex.Message.StartsWith((NoResString)"The file is too long."))
				{
					throw new FileAccessException(string.Format(CultureInfo.InvariantCulture, "The file size exceeds the {0}MB maximum file size allowed for eDocs.", SystemDataRegistry.Instance.eDocsMaximumFilesize.Value), filePath, ex);
				}

				throw new FileAccessException("The file is currently in use. Please close the file and try again.", filePath, ex);
			}
			catch (ArgumentException ex)
			{
				if (ex.Message.StartsWith((NoResString)"Handle does not support synchronous operations"))
				{
					throw new FileAccessException("The file is currently in use. Please close the file and try again.", filePath, ex);
				}

				throw new FileAccessException("Unable to access this file. Please try again later.", filePath, ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				throw new FileAccessException("You do not have access to use this file.", filePath, ex);
			}
			catch (OperationCanceledException ex)
			{
				throw new FileAccessException("There was an error while reading the file. Please ensure the file is accessible and try again.", filePath, ex);
			}
		}

		/// <summary>
		/// List of illegal characters is from Windows explorer
		/// </summary>
		public static ZString RemoveIllegalCharacters(ZString @string)
		{
			var returnValue = @string.Replace("\\", ZString.Empty);
			returnValue = returnValue.Replace("/", ZString.Empty);
			returnValue = returnValue.Replace(":", ZString.Empty);
			returnValue = returnValue.Replace("?", ZString.Empty);
			returnValue = returnValue.Replace("\"", ZString.Empty);
			returnValue = returnValue.Replace("<", ZString.Empty);
			returnValue = returnValue.Replace(">", ZString.Empty);
			returnValue = returnValue.Replace("|", ZString.Empty);
			returnValue = returnValue.Replace("*", ZString.Empty);

			return returnValue;
		}

		/// <summary>
		/// Takes a file and attempts to turn it into a TIF file. 
		/// Returns the converted file filename
		/// </summary>
		public static string ConvertFileToTiff(string existingFile)
		{
			var destinationFile = DocumentUtilities.GetTempFilename();
			ConvertFileToTiff(existingFile, destinationFile);

			return destinationFile;
		}

		void Integration.IDocumentUtilities.ConvertFileToTiff(string existingFile, string destinationFile)
		{
			ConvertFileToTiff(existingFile, destinationFile);
		}

		public static void ConvertFileToTiff(string existingFile, string destinationFile)
		{
			var convertedContents = ConvertFileToTiff(GetFileAsBytes(existingFile), Path.GetFileName(existingFile));

			using (var stream = File.Create(destinationFile))
			{
				stream.Write(convertedContents, 0, convertedContents.Length);
			}
		}

		public static byte[] ConvertFileToTiff(byte[] contents, string fileNameOnly)
		{
			var convertedContents = contents;

			if (!IsTiffFile(fileNameOnly))
			{
				using (var tempFile = TempFile.NewWithExtension("TIF"))
				{
					DocumentEngine.FileFormatUtilities.FileSaveHelper.SaveMultiPageTifFile(contents, tempFile.Filename);
					convertedContents = GetFileAsBytes(tempFile.Filename);
				}
			}

			return convertedContents;
		}

		public static byte[] ConvertFileToTiff(SubStreamableStream contents, string fileNameOnly)
		{
			if (!IsTiffFile(fileNameOnly))
			{
				using (var tempFile = TempFile.NewWithExtension("TIF"))
				{
					DocumentEngine.FileFormatUtilities.FileSaveHelper.SaveMultiPageTifFile(contents, tempFile.Filename);
					return GetFileAsBytes(tempFile.Filename);
				}
			}

			return contents.ToByteArray();
		}

		public static Image GetImageFromFile(string aFilePath)
		{
			return Image.FromStream(new MemoryStream(GetFileAsBytes(aFilePath)));
		}

		public static bool IsTiffFile(string filename)
		{
			var extension = Path.GetExtension(filename);
			if (extension != null)
			{
				var extensionUpcaseWithoutDot = extension.Replace(".", "").ToUpperInvariant();
				return extensionUpcaseWithoutDot == Core.Constants.FileFormats.TIF ||
					   extensionUpcaseWithoutDot == Core.Constants.FileFormats.TIFF;
			}
			return false;
		}

		void Integration.IDocumentUtilities.AppendMultiPageImageToFile(string sourceMultiPageImageFile, string outputFilePath)
		{
			AppendMultiPageImageToFile(sourceMultiPageImageFile, outputFilePath);
		}

		/// <summary>
		/// All pages of SourceMultiPageImageFile will be appended to the existing OutputFilePath, which must exist.
		/// </summary>
		public static void AppendMultiPageImageToFile(string sourceMultiPageImageFile, string outputFilePath)
		{
			if (File.Exists(outputFilePath) && File.Exists(sourceMultiPageImageFile))
			{
				using (var reader = new ImageFileReaderNoLockUsingTempFile(outputFilePath))
				using (var tifImage = new WritableTifImage(outputFilePath))
				{
					for (var pageIndex = 0; pageIndex < reader.PageSelector.TotalPages; ++pageIndex)
					{
						reader.PageSelector.CurrentPageIndex = pageIndex;
						tifImage.AddPage(reader.PageSelector.CurrentImage);
					}

					using (var sourceMultiPageImage = new ImageFileReaderWithLock(sourceMultiPageImageFile))
					{
						for (var pageIndex = 0; pageIndex < sourceMultiPageImage.PageSelector.TotalPages; ++pageIndex)
						{
							sourceMultiPageImage.PageSelector.CurrentPageIndex = pageIndex;
							tifImage.AddPage((Bitmap)sourceMultiPageImage.PageSelector.CurrentImage);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns number of pages copied
		/// </summary>
		public static int CopySelectedPages(string sourceFile, string outputFile, int[] pageIndexesToKeep)
		{
			var numPagesCopied = 0;

			if (pageIndexesToKeep.Length > 0)
			{
				using (var tifImage = new WritableTifImage(outputFile))
				using (var sourcePageReader = new ImageFileReaderWithLock(sourceFile))
				{
					foreach (var pageIndex in pageIndexesToKeep)
					{
						sourcePageReader.PageSelector.CurrentPageIndex = pageIndex;
						tifImage.AddPage((Bitmap)sourcePageReader.PageSelector.CurrentImage);
						numPagesCopied++;
					}
				}
			}

			return numPagesCopied;
		}

		/// <summary>
		/// A single page is written out to a single file
		/// </summary>
		public static void SaveOnePageImage(string outputFile, Bitmap page)
		{
			page.Save(outputFile, ImageFormat.Tiff);
		}

		#region Testing Only
#if DEBUG

		public static bool IsCurrentPageEqual(Image first, Image second)
		{
			var originalFrameFile = DocumentUtilities.GetTempFilename();
			// this call to save saves only the ACTIVE FRAME
			first.Save(originalFrameFile, ImageFormat.Tiff);

			var newFrameFile = DocumentUtilities.GetTempFilename();
			// this call to save saves only the ACTIVE FRAME
			second.Save(newFrameFile, ImageFormat.Tiff);

			var originalImageBytes = DocumentUtilities.GetFileAsBytes(originalFrameFile);
			var newImageBytes = DocumentUtilities.GetFileAsBytes(newFrameFile);

			if (File.Exists(originalFrameFile))
			{
				File.Delete(originalFrameFile);
			}

			if (File.Exists(newFrameFile))
			{
				File.Delete(newFrameFile);
			}

			return originalImageBytes == newImageBytes;
		}

#endif
		#endregion
	}
}
