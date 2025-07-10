using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using CargoWise.PdfiumWrapper.Testing;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FileFormatUtilities.Testing
{
	sealed class FileSaveHelperTest : TransactionedTestCase
	{
		public void TestSaveImageWhenLastPageIsActive()
		{
			using (var tempOut = TempFile.NewWithExtension(".tiff"))
			{
				using (var file = ImageTestingHelpers.CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Green, Color.Blue))
				using (var image = Image.FromFile(file.Filename))
				{
					image.SelectActiveFrame(FrameDimension.Page, 2);

					FileSaveHelper.SaveMultiPageTifFile(image, tempOut.Filename);
				}

				using (var image = Image.FromFile(tempOut.Filename))
				{
					AssertArrayEqualsByElements("We should have our three original pages in the correct order, regardless of the active frame", new[] { Color.Red, Color.Green, Color.Blue }, ImageTestingHelpers.GetPageColors(image));
				}
			}
		}

		public void TestSaveImageWithClosedStream()
		{
			Bitmap imageTest;
			using (Stream stream = new MemoryStream(new byte[] { 71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 247, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 33, 249, 4, 1, 0, 0, 255, 0, 44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 8, 4, 0, 255, 5, 4, 0, 59 }))
			{
				var testImage = new Bitmap(stream);
				imageTest = testImage;
			}

			using (var file = TempFile.NewWithExtension("TIF"))
			{
				AssertNoExceptionThrown("No exception was thrown", () => FileSaveHelper.SaveMultiPageTifFileUsingConversionToBMP(imageTest, file.Filename));
			}
		}

		public void TestIsFolderWritableTestCorrectFolder()
		{
			var imageByte = new byte[] { 71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 247, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 33, 249, 4, 1, 0, 0, 255, 0, 44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 8, 4, 0, 255, 5, 4, 0, 59 };

			using (var file = TempFile.NewWithExtension("TIF"))
			{
				try
				{
					FileSaveHelper.SaveMultiPageTifFile(imageByte, Path.GetDirectoryName(file.Filename) + @"t\" + Path.GetFileName(file.Filename));
				}
				catch (Exception e)
				{
					AssertEquals(e.Message, string.Format(@"Could not find a part of the path '{0}'.", Path.GetDirectoryName(file.Filename) + @"t\" + "testFile_" + new Guid().ToString()));
				}
			}
		}

		public void TestInvalidFileContentThrowsRightException()
		{
			AssertExceptionThrown(typeof(ImageFormatException), delegate
			{
				FileSaveHelper.SaveMultiPageTifFile(Encoding.ASCII.GetBytes("I Like Bing Lee"), "Fred.TIF");
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBrokenMultipageTifFileThrowsRightException()
		{
			using (var tempFile = TempFile.New())
			{
				AssertExceptionThrown(
					typeof(BadImageFormatException),
					string.Format("Multipage Tiff image '{0}' has only 7 frames accessible of total 14 frames declared. Please check and fix the image manually before processing it again.", tempFile.Filename),
					() => FileSaveHelper.SaveMultiPageTifFile(
						File.ReadAllBytes(Path.Combine(PrintProcessingConstants.TestWorkingDirectory, PrintProcessingConstants.TestMissingFrameTifFileName)),
						tempFile.Filename));
			}
		}

		public void TestGetEncoderInfo()
		{
			AssertNotNull("image/tiff codec", FileSaveHelper.GetTiffEncoder());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveMultiPageTifFile()
		{
			var fileBlob = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestFaxTifFileName);
			var tempFile = Env.TempPath + Guid.NewGuid() + ".TIF";
			FileSaveHelper.SaveMultiPageTifFile(fileBlob, tempFile);

			using (var convertedFile = Image.FromFile(tempFile))
			{
				AssertEquals(ImageFormat.Tiff, convertedFile.RawFormat);
			}
			File.Delete(tempFile);

			tempFile = Env.TempPath + Guid.NewGuid() + ".TIF";
			try
			{
				FileSaveHelper.SaveMultiPageTifFile(fileBlob, tempFile);
			}
			catch (Exception e)
			{
				Assert("Exception thrown second time run with the same fileblob, exception: " + e.Message, false);
			}

			using (var convertedFile = Image.FromFile(tempFile))
			{
				AssertEquals(ImageFormat.Tiff, convertedFile.RawFormat);
			}
			File.Delete(tempFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveBlobAsFile()
		{
			// TIF image with TIFF RawFormat
			var fileBlobBeforeTiff = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.Test8bppTifFileName);
			using (MemoryStream imageStream = new MemoryStream(fileBlobBeforeTiff))
			{
				AssertEquals("Image RawFormat should be Tiff", true, Image.FromStream(imageStream).RawFormat.Guid == ImageFormat.Tiff.Guid);
			}

			var tempFileTiff = Env.TempPath + Guid.NewGuid() + ".TIF";
			FileSaveHelper.SaveBlobAsFile(fileBlobBeforeTiff, tempFileTiff);
			using (var resultingImage = Image.FromFile(tempFileTiff))
			{
				AssertEquals("Saving TIF file, not all pages saved", 6, resultingImage.GetFrameCount(FrameDimension.Page));
			}
			AssertEquals("The images should have the same size", fileBlobBeforeTiff.Length, File.ReadAllBytes(tempFileTiff).Length);
			File.Delete(tempFileTiff);

			// TIF image without TIFF RawFormat
			var fileBlobBefore = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestTiffRawFormatFileName);
			using (MemoryStream imageStream = new MemoryStream(fileBlobBefore))
			{
				AssertEquals("Image RawFormat should be Tiff", false, Image.FromStream(imageStream).RawFormat.Guid == ImageFormat.Tiff.Guid);
			}

			var tempFile = Env.TempPath + Guid.NewGuid() + ".TIF";
			FileSaveHelper.SaveBlobAsFile(fileBlobBefore, tempFile);
			using (var resultingImage = Image.FromFile(tempFile))
			{
				AssertEquals("Saving TIF file, not all pages saved", 1, resultingImage.GetFrameCount(FrameDimension.Page));
			}
			AssertNotEquals("The images shouldn't have the same size", fileBlobBefore.Length, File.ReadAllBytes(tempFile).Length);
			File.Delete(tempFile);

			// PDF file
			var pDFTempFile = Env.TempPath + Guid.NewGuid() + ".PDF";
			var fileBlobOriginal = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName);
			FileSaveHelper.SaveBlobAsFile(fileBlobOriginal, pDFTempFile);
			var fileBlobAfterSave = File.ReadAllBytes(pDFTempFile);

			AssertEquals("Blob contents should be the same", fileBlobAfterSave, fileBlobOriginal);
			File.Delete(pDFTempFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveMultiPageTifFileWithDodgyFile()
		{
			var filenameOfDodgyFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestBadImageGDIExternalExceptionFileName;
			var fileBlob = File.ReadAllBytes(filenameOfDodgyFile);
			var tempFile = Env.TempPath + Guid.NewGuid() + ".TIF";

			FileSaveHelper.SaveMultiPageTifFile(fileBlob, tempFile);

			using (var convertedFile = Image.FromFile(tempFile))
			{
				AssertEquals(System.Drawing.Imaging.ImageFormat.Tiff, convertedFile.RawFormat);
				AssertEquals("File shold contain 4 pages", 4, convertedFile.GetFrameCount(new FrameDimension(convertedFile.FrameDimensionsList[0])));
			}

			File.Delete(tempFile);
		}
	}
}
