using System.IO;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZipCreatorTest : TestCase
	{
		ZipCreator creator;

		public void TestCreateZipFile()
		{
			string originFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.txt");
			string destinationFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");
			try
			{
				File.WriteAllText(originFileName, new string('x', 10000));

				Creator.CreateZipFile(originFileName, destinationFileName);

				AssertEquals(true, File.Exists(destinationFileName));

				int fileSizeOrigin = File.ReadAllBytes(originFileName).Length;
				int fileSizeDestination = File.ReadAllBytes(destinationFileName).Length;
				Assert(fileSizeOrigin > fileSizeDestination);
				Assert(fileSizeDestination > 0);
			}
			finally
			{
				File.Delete(originFileName);
				File.Delete(destinationFileName);
			}
		}

		public void TestZipFileStructure()
		{
			using (MemoryStream inputStream = new MemoryStream(new byte[] { 1, 2, 3 }))
			using (MemoryStream outputStream = new MemoryStream())
			{
				Creator.ZipStream("zip.txt", inputStream, outputStream);
				outputStream.Position = 18;

				byte[] compressedSizeHeaderPart = new byte[4];
				outputStream.Read(compressedSizeHeaderPart, 0, 4);
				AssertEquals("compressedSizeHeaderPart", new byte[] { 5, 0, 0, 0 }, compressedSizeHeaderPart);

				byte[] uncompressedSizeHeaderPath = new byte[4];
				outputStream.Read(uncompressedSizeHeaderPath, 0, 4);
				AssertEquals("uncompressedSizeHeaderPath", new byte[] { 3, 0, 0, 0 }, uncompressedSizeHeaderPath);
			}
		}

		[ExpectNoExceptions]
		public void TestZipStream()
		{
			using (MemoryStream outputStream = new MemoryStream())
			using (MemoryStream inputStream = new MemoryStream())
			{
				inputStream.Write((new byte[4] { 1, 2, 3, 4 }), 0, 4);
				inputStream.Position = inputStream.Length - 1;
				AssertNotEquals(0, inputStream.Position);
				Creator.ZipStream("test", inputStream, outputStream);
			}
		}

		public void TestZipStreamWithMultipleZipEntries()
		{
			using (MemoryStream outputStream = new MemoryStream())
			using (MemoryStream inputStream1 = new MemoryStream())
			using (MemoryStream inputStream2 = new MemoryStream())
			{
				inputStream1.Write((new byte[4] { 1, 2, 3, 4 }), 0, 4);
				inputStream1.Position = inputStream1.Length - 1;
				AssertNotEquals(0, inputStream1.Position);

				inputStream2.Write((new byte[4] { 4, 3, 2, 1 }), 0, 4);
				inputStream2.Position = inputStream2.Length - 1;
				AssertNotEquals(0, inputStream2.Position);

				Creator.ZipStream(new ZipStream[] { new ZipStream("filename1.txt", inputStream1),
					new ZipStream("filename2.txt", inputStream2) }, outputStream);

				ZipExtractor extractor = new ZipExtractor();
				AssertEquals("OutputStream should contain filename1.txt", true, extractor.ContainsFile(outputStream, "filename1.txt"));
				AssertEquals("OutputStream should contain filename2.txt", true, extractor.ContainsFile(outputStream, "filename2.txt"));
			}
		}

		public void TestZippingAndExtractingMultiplePasswordEntries()
		{
			string testFileName = EnvProxy.Instance.GetTempFileName();
			try
			{
				byte[] input1data = new byte[4] { 1, 2, 3, 4 };
				byte[] input2data = new byte[4] { 4, 3, 2, 1 };
				using (FileStream fileStream = File.OpenWrite(testFileName))
				using (MemoryStream inputStream1 = new MemoryStream())
				using (MemoryStream inputStream2 = new MemoryStream())
				{
					inputStream1.Write(input1data, 0, 4);
					inputStream1.Position = inputStream1.Length - 1;
					AssertNotEquals(0, inputStream1.Position);

					inputStream2.Write(input2data, 0, 4);
					inputStream2.Position = inputStream2.Length - 1;
					AssertNotEquals(0, inputStream2.Position);

					Creator.Password = ZipExtractorTest.PasswordForTest;
					Creator.ZipStream(new ZipStream[] { new ZipStream("filename1.txt", inputStream1),
					new ZipStream("filename2.txt", inputStream2) }, fileStream);
				}

				using (FileStream zipStream = File.OpenRead(testFileName))
				{
					using (MemoryStream outputStream = new MemoryStream())
					{
						ZipExtractor extractor = new ZipExtractor(ZipExtractorTest.PasswordForTest);
						extractor.ExtractZipStream(zipStream, outputStream, "filename1.txt");
						AssertEquals(input1data, outputStream.ToArray());

						extractor.ExtractZipStream(zipStream, outputStream, "filename2.txt");
						AssertEquals(input2data, outputStream.ToArray());
					}
				}
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		public void TestZipStreamWithUnicodeFileNames()
		{
			using (var outputStream = new MemoryStream())
			using (var inputStream1 = new MemoryStream())
			using (var inputStream2 = new MemoryStream())
			{
				inputStream1.Write((new byte[4] { 1, 2, 3, 4 }), 0, 4);
				inputStream1.Position = inputStream1.Length - 1;
				AssertNotEquals(0, inputStream1.Position);

				inputStream2.Write((new byte[4] { 4, 3, 2, 1 }), 0, 4);
				inputStream2.Position = inputStream2.Length - 1;
				AssertNotEquals(0, inputStream2.Position);

				var fileName1 = "filename1.txt";
				var fileName2 = "\u4E2D\u6587.txt";

				Creator.ZipStream(new ZipStream[] { new ZipStream(fileName1, inputStream1),
					new ZipStream(fileName2, inputStream2) }, outputStream);

				var extractor = new ZipExtractor();
				AssertEquals(true, extractor.ContainsFile(outputStream, fileName1));
				AssertEquals(true, extractor.ContainsFile(outputStream, fileName2));
			}
		}

		ZipCreator Creator
		{
			get { return creator ?? (creator = new ZipCreator()); }
		}
	}
}
