using System;
using System.IO;
using System.Text;
using CargoWise.IO;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZipExtractorTest : TestCase
	{
		public void TestExtractPasswordZipStream()
		{
			using (var zipStream = PasswordEDIZipStream)
			using (var outputStream = new MemoryStream())
			{
				AssertExceptionThrown<ZipException>(() => new ZipExtractor().ExtractZipStream(zipStream, outputStream, "EDI-E00000003.xml"));
				new ZipExtractor(PasswordForTest).ExtractZipStream(zipStream, outputStream, "EDI-E00000003.xml");
				AssertEquals("<EDI_Exception_Report></EDI_Exception_Report>", Encoding.UTF8.GetString(outputStream.ToArray()));
			}
		}

		public void TestExtractZipStream()
		{
			using (var zipStream = EDIZipStream)
			using (var outputStream = new MemoryStream())
			{
				new ZipExtractor().ExtractZipStream(zipStream, outputStream, "EDI-E00000003.xml");
				AssertEquals("<EDI_Exception_Report></EDI_Exception_Report>", Encoding.UTF8.GetString(outputStream.ToArray()));
			}
		}

		public void TestContainsFile()
		{
			using (var zipStream = EDIZipStream)
			{
				AssertEquals(true, new ZipExtractor().ContainsFile(zipStream, "EDI-E00000003.xml"));
				AssertEquals(false, new ZipExtractor().ContainsFile(zipStream, "EDI-E00000003.doc"));
				AssertEquals(false, new ZipExtractor().ContainsFile(zipStream, "EDI-E00000004.xml"));
			}
		}

		public void TestContainsFilePassword()
		{
			using (var zipStream = PasswordEDIZipStream)
			{
				AssertEquals(true, new ZipExtractor(PasswordForTest).ContainsFile(zipStream, "EDI-E00000003.xml"));
				AssertEquals(false, new ZipExtractor(PasswordForTest).ContainsFile(zipStream, "EDI-E00000003.doc"));
				AssertEquals(false, new ZipExtractor(PasswordForTest).ContainsFile(zipStream, "EDI-E00000004.xml"));
			}
		}

		public void TestGetFileNames()
		{
			using (var zipStream = EDIZipStream)
			{
				var fileNames = new ZipExtractor().GetFileNames(zipStream);
				AssertEquals(1, fileNames.Length);
				AssertEquals("EDI-E00000003.xml", fileNames[0]);
			}
		}

		public void TestGetZipFileInfos()
		{
			using (var zipStream = EDIZipStream)
			{
				var fileInfos = new ZipExtractor().GetZipFileInfos(zipStream);
				AssertEquals(1, fileInfos.Length);
				AssertZipFileInfo(new ZipExtractor.ZipFileInfo { FileName = "EDI-E00000003.xml", CompressedSize = 28, Size = 45, Crc = 3707616352, HasCrc = true, IsFile = true, IsDirectory = false }, fileInfos[0]);
			}
		}

		static void AssertZipFileInfo(ZipExtractor.ZipFileInfo expectedInfo, ZipExtractor.ZipFileInfo actualInfo)
		{
			AssertEquals(expectedInfo.FileName, actualInfo.FileName);
			AssertEquals(expectedInfo.CompressedSize, actualInfo.CompressedSize);
			AssertEquals(expectedInfo.Size, actualInfo.Size);
			AssertEquals(expectedInfo.Crc, actualInfo.Crc);
			AssertEquals(expectedInfo.HasCrc, actualInfo.HasCrc);
			AssertEquals(expectedInfo.IsFile, actualInfo.IsFile);
			AssertEquals(expectedInfo.IsDirectory, actualInfo.IsDirectory);
		}

		public void TestGetFileNamesPassword()
		{
			using (var zipStream = PasswordEDIZipStream)
			{
				var fileNames = new ZipExtractor(PasswordForTest).GetFileNames(zipStream);
				AssertEquals(1, fileNames.Length);
				AssertEquals("EDI-E00000003.xml", fileNames[0]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		Stream EDIZipStream => resourceRetriever.Value.GetStream("Enterprise.ZArchitecture.Core.Test.ErrorReporting.Testing.EDI-E00000003.zip");

		Stream PasswordEDIZipStream => resourceRetriever.Value.GetStream("Enterprise.ZArchitecture.Core.Test.ErrorReporting.Testing.PasswordEDI-E00000003.zip");

		internal const string PasswordForTest = "isApassWord";
	}
}
