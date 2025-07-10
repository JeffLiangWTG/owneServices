using System.IO;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZipCompressionTest : TestCase
	{
		public void TestZipAndUnzip()
		{
			string sourceFolder = Path.Combine(EnvProxy.Instance.TempPath, "ZipperTest");
			if (!Directory.Exists(sourceFolder))
			{
				Directory.CreateDirectory(sourceFolder);
			}

			string testFile1 = Path.Combine(sourceFolder, "File1.txt");
			string testFile2 = Path.Combine(sourceFolder, "File2.txt");
			string testFile3 = Path.Combine(sourceFolder, "File3.txt");

			string targetFolder = Path.Combine(EnvProxy.Instance.TempPath, "ZipperTestUnzip");

			string testUnzippedFile1 = Path.Combine(targetFolder, "File1.txt");
			string testUnzippedFile2 = Path.Combine(targetFolder, "File2.txt");
			string testUnzippedFile3 = Path.Combine(targetFolder, "File3.txt");

			string targetFile = Path.Combine(targetFolder, "ZippedFile.zip");

			try
			{
				CreateTextFile(testFile1, "TestFile1");
				CreateTextFile(testFile2, "TestFile2");
				CreateTextFile(testFile3, "TestFile3");

				Assert("TestFile1 created", File.Exists(testFile1));
				Assert("TestFile2 created", File.Exists(testFile2));
				Assert("TestFile3 created", File.Exists(testFile3));

				if (File.Exists(targetFile))
				{
					File.Delete(targetFile);
				}

				AssertEquals("Zipping Error: " + ZipCompression.LastError, true, ZipCompression.Zip(sourceFolder, targetFile));
				Assert(targetFile + " should be created", File.Exists(targetFile));
				AssertEquals("LastError", "", ZipCompression.LastError);

				ZipCompression.Unzip(targetFile, targetFolder);
				AssertEquals("LastError", "", ZipCompression.LastError);
				string[] filesInTargetFolder = Directory.GetFiles(targetFolder);
				AssertEquals("File count", 4, filesInTargetFolder.Length);

				Assert(testUnzippedFile1 + " should exist", File.Exists(testUnzippedFile1));
				Assert(testUnzippedFile2 + " should exist", File.Exists(testUnzippedFile2));
				Assert(testUnzippedFile3 + " should exist", File.Exists(testUnzippedFile3));

				using (StreamReader reader = File.OpenText(testUnzippedFile1))
				{
					AssertEquals(testUnzippedFile1 + " should contain", "TestFile1", reader.ReadLine());
				}

				using (StreamReader reader = File.OpenText(testUnzippedFile2))
				{
					AssertEquals(testUnzippedFile2 + " should contain", "TestFile2", reader.ReadLine());
				}

				using (StreamReader reader = File.OpenText(testUnzippedFile3))
				{
					AssertEquals(testUnzippedFile3 + " should contain", "TestFile3", reader.ReadLine());
				}
			}
			finally
			{
				ZipCompression.ClearLastErrorForTest();
				if (Directory.Exists(sourceFolder))
				{
					Directory.Delete(sourceFolder, true);
				}

				if (Directory.Exists(targetFolder))
				{
					Directory.Delete(targetFolder, true);
				}
			}
		}

		void CreateTextFile(string fileName, string content)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			using (StreamWriter writer = File.CreateText(fileName))
			{
				writer.WriteLine(content);
				writer.Close();
			}
		}
	}
}
