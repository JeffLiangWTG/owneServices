using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.FileFormatUtilities
{
	public static class FileSaveHelper
	{
		internal static void DeleteFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		public static void SaveBlobAsFile(byte[] blob, string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			var createFile = true;

			if (Path.GetExtension(fileName).Equals(".TIF", StringComparison.OrdinalIgnoreCase))
			{
				using (var customImageData = new CustomImageData(blob))
				{
					if (!IsImageRawFormatTiff(customImageData.Image))
					{
						createFile = false;
						SaveMultiPageTifFile(blob, fileName, customImageData.Image);
					}
				}
			}

			if (createFile)
			{
				using (var stream = File.Create(fileName, 1024))
				{
					stream.Write(blob, 0, blob.Length);
				}
			}
		}

		static bool IsImageRawFormatTiff(Image convertedImage)
		{
			return (convertedImage.RawFormat.Guid == ImageFormat.Tiff.Guid);
		}

		public static void SaveMultiPageTifFile(Stream imageData, string outputFilename, Image convertedImage = null)
		{
			SaveMultiPageTifFile(() => new CustomImageData(imageData), outputFilename, convertedImage);
		}

		public static void SaveMultiPageTifFile(byte[] imageData, string outputFilename, Image convertedImage = null)
		{
			SaveMultiPageTifFile(() => new CustomImageData(imageData), outputFilename, convertedImage);
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Debugging")]
		static void SaveMultiPageTifFile(Func<CustomImageData> createCustomImageData, string outputFilename, Image convertedImage = null)
		{
			CustomImageData customImageData = null;

			try
			{
				if (convertedImage == null)
				{
					customImageData = createCustomImageData();
					convertedImage = customImageData.Image;
				}

				SaveMultiPageTifFile(convertedImage, outputFilename);
			}
			catch (ExternalException)
			{
				try
				{
					SaveMultiPageTifFileUsingConversionToBMP(convertedImage, outputFilename);
				}
				catch (ExternalException ex)
				{
					/*
					GDI+ generic error.
					Reasons why it might be thrown:
					-No write permissions on folder or it's invalid or doesn't exist or can't be written for some other reasons...
					-If based off of an Image constructed from a Stream, destroying the Stream causes this
					-The height of an image being 65535 or greater
					-corrupt/unknown/invalid image format
					-Destination folder/disk is full
					-...
					*/
					bool canwrite = IsFolderWritable(Path.GetDirectoryName(outputFilename));

					if (canwrite)
					{
						//check if it failed because Temp folder is full
						var diskFull = IsFolderFull(Path.GetDirectoryName(outputFilename), customImageData.Length);
						if (diskFull)
						{
							throw new IOException("There is not enough space on the disk.");
						}

						var additionalInfo = string.Format(CultureInfo.InvariantCulture,
(NoResString)@"CS00163228: Cannot save a multipage tif image to a file.
Filename: {0}
Format Guid: {1}
Pixel Format: {2}
Format Flags: {3}
Image Size: {4} x {5}
Image Data Size: {6} bytes",
outputFilename, // 0
convertedImage.RawFormat.Guid, // 1
convertedImage.PixelFormat, // 2
convertedImage.Flags, // 3
convertedImage.Width, // 4
convertedImage.Height, // 5
customImageData.Length // 6
);

						if (customImageData.Length <= 50000)
						{
							string imageBytesAsString = customImageData.ToBase64();
							additionalInfo += System.Environment.NewLine + (NoResString)@"RawBytes = <data>" + imageBytesAsString + (NoResString)"</data>";
						}

						ErrorReporter.ReportOnce("SaveMultiPageTifFile_ExternalException", additionalInfo, ex);
						throw new ExternalException(additionalInfo, ex);
					}
					else
					{
						throw new IOException("Cannot write to " + Path.GetDirectoryName(outputFilename), ex);
					}
				}
			}
			finally
			{
				if (customImageData != null)
				{
					customImageData.Dispose();
				}
			}
		}

		static bool IsFolderFull(string path, long length)
		{
			long freeBytesForUser = -1;
			long bytesForUser = -1;
			long freeBytesOnDisk = -1;

			_ = TopLevelExceptionHandler.GetDiskFreeSpace(path, ref freeBytesForUser, ref bytesForUser, ref freeBytesOnDisk);

			if (freeBytesForUser < length)
			{
				return true;
			}
			return false;
		}

		static bool IsFolderWritable(string folder)
		{
			string filename = Path.Combine(folder, "testFile_" + new Guid().ToString());
			bool result;

			try
			{
				using (FileStream fstream = new FileStream(filename, FileMode.Create))
				using (TextWriter writer = new StreamWriter(fstream))
				{
					writer.WriteLine((NoResString)"sometext to check either folder is writable or not");
					result = true;
				}
			}
			catch (UnauthorizedAccessException)
			{    //No permission.
				result = false;
			}
			finally
			{
				if (File.Exists(filename))
				{
					string s;
					TempFile.TryDelete(filename, out s, false);
				}
			}
			return result;
		}

		internal static void SaveMultiPageTifFile(Image imageToSave, string newFilename)
		{
			// Specify that this save is a multi-frame save
			EncoderParameters encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
			FrameDimension dimension = new FrameDimension(imageToSave.FrameDimensionsList[0]);

			SelectPage(imageToSave, dimension, 0, newFilename);
			imageToSave.Save(newFilename, GetTiffEncoder(), encoderParams);

			// Now save the other pages
			encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
			int frameCount = imageToSave.GetFrameCount(dimension);
			for (int i = 1; i < frameCount; i++)
			{
				SelectPage(imageToSave, dimension, i, newFilename);
				imageToSave.SaveAdd(imageToSave, encoderParams);
			}

			// Flush and do the final save
			encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
			imageToSave.SaveAdd(encoderParams);
		}

		static void SelectPage(Image img, FrameDimension dimension, int pageNb, string fileName)
		{
			try
			{
				img.SelectActiveFrame(dimension, pageNb);
			}
			catch (ExternalException ex)
			{
				throw new BadImageFormatException(GetBrokenFramesMessage(fileName, pageNb, img.GetFrameCount(dimension)), ex);
			}
		}

		// WI00019049 Issue 00174935 - A generic error occurred in GDI+.
		// Dirty hack in use
		// For some tiff files Image.Save() method SaveMultiPageTifFile(...) used to fail with laconic GDI+ generic error
		// In order to avoid this we convert in memory tiff file to BMP format, and then call Image.Save()
		// to convert plain BMP to tiff.
		// Example of dodgy file can be found in test TestSaveMultiPageTifFileWithDodgyFile()
		// For really multi page tiff we need to convert to BMP every page, then add it to a newly created tiff file.
		// Alex K
		internal static void SaveMultiPageTifFileUsingConversionToBMP(Image imageToSave, string newFilename)
		{
			using (MemoryStream stream = new MemoryStream())
			using (Bitmap safeImageToSave = new Bitmap(imageToSave)) // bitmap needs to be duplicated in order to lose the images reliance on the stream which created it, this is because the stream could have been disposed and cause a GDI+ error
			{
				safeImageToSave.Save(stream, ImageFormat.Bmp);

				// Specify that this save is a multi-frame save
				EncoderParameters encoderParams = new EncoderParameters(1);
				encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
				FrameDimension dimension = new FrameDimension(safeImageToSave.FrameDimensionsList[0]);

				using (Image image = Image.FromStream(stream))
				{
					image.Save(newFilename, GetTiffEncoder(), encoderParams);

					// Now save the other pages
					encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
					int frameCount = safeImageToSave.GetFrameCount(dimension);
					for (int i = 1; i < frameCount; i++)
					{
						SelectPage(safeImageToSave, dimension, i, newFilename);

						using (MemoryStream currentPageStream = new MemoryStream())
						{
							safeImageToSave.Save(currentPageStream, ImageFormat.Bmp);
							using (Image currentPage = Image.FromStream(currentPageStream))
							{
								image.SaveAdd(currentPage, encoderParams);
							}
						}
					}

					// Flush and do the final save
					encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
					image.SaveAdd(encoderParams);
				}
			}
		}

		public static ImageCodecInfo GetTiffEncoder()
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

			foreach (ImageCodecInfo codec in encoders)
			{
				if (codec.MimeType == "image/tiff")
				{
					return codec;
				}
			}

			throw new ApplicationException("Codec for tiff not found - should be available with .NET Framework. You may need to check the .NET installation is OK. This error means that the client can't print or convert documents to .TIF files.");
		}

		static string GetBrokenFramesMessage(string fileName, int brokenFrame, int frameCount)
		{
			return string.Format((NoResString)"Multipage Tiff image '{0}' has only {1} frames accessible of total {2} frames declared. Please check and fix the image manually before processing it again.",
				fileName, brokenFrame, frameCount);
		}

		class CustomImageData : IDisposable
		{
			MemoryStream imageMemoryStream;

			public Image Image
			{
				get
				{
					return image;
				}
			}
			Image image;

			public long Length
			{
				get
				{
					return length;
				}
			}

			long length;

			public CustomImageData(byte[] imageData)
			{
				CreateImageFromByteArray(imageData);
			}

			public CustomImageData(Stream imageData)
			{
				CreateImageFromStream(imageData);
			}

			public string ToBase64()
			{
				if (imageMemoryStream == null)
				{
					imageMemoryStream = new MemoryStream();
					Image.Save(imageMemoryStream, Image.RawFormat);
				}

				return Convert.ToBase64String(imageMemoryStream.GetBuffer());
			}

			void CreateImageFromByteArray(byte[] imageData)
			{
				imageMemoryStream = new MemoryStream(imageData);
				CreateImageFromStream(imageMemoryStream);
			}

			void CreateImageFromStream(Stream imageData)
			{
				try
				{
					length = imageData.Length;
					image = Image.FromStream(imageData);
				}
				catch (Exception exception)
				{
					if (!exception.IsCriticalException()
						&& exception.IsBadImageFormatException())
					{
						throw new ImageFormatException();
					}
					throw;
				}
			}

			public void Dispose()
			{
				if (image != null)
				{
					image.Dispose();
				}
				if (imageMemoryStream != null)
				{
					imageMemoryStream.Dispose();
				}
			}
		}
	}
}
