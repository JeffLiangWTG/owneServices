using System.IO;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

#if !WINZOR
// Added to Enterprise.Winzor.Architecture.Test/RemoteZipCompressionTest for Winzor test.

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class RemoteZipCompressionTest : TestCase
	{
		public void TestZipFiles()
		{
			using (var tempDir = new TempDirectory())
			{
				string file1 = Path.Combine(tempDir, "file1.txt");
				string file2 = Path.Combine(tempDir, "file2.txt");
				string file3 = Path.Combine(tempDir, "file3.txt");
				string file4 = Path.Combine(tempDir, "file4.txt");
				var unmappedPath = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");

				File.WriteAllText(file1, new string('x', 10000));
				File.WriteAllText(file2, new string('x', 10000));
				File.WriteAllText(file3, new string('x', 10000));
				File.WriteAllText(file4, new string('x', 10000));
				AssertEquals("Zip process runs sucessfully", true, RemoteZipCompression.Zip(tempDir, unmappedPath));
				AssertEquals("file.zip should be exported", true, File.Exists(unmappedPath));

				TempFile.TryDelete(unmappedPath, out _);
			}
		}

		public void TestZipFilesWhenExist()
		{
			using (var tempDir = new TempDirectory())
			using (var tempDir2 = new TempDirectory())
			{
				string file1 = Path.Combine(tempDir, "file1.txt");
				string file2 = Path.Combine(tempDir, "file2.txt");
				string file3 = Path.Combine(tempDir, "file3.txt");
				string file4 = Path.Combine(tempDir, "file4.txt");

				File.WriteAllText(file1, new string('x', 10000));
				File.WriteAllText(file2, new string('x', 10000));
				File.WriteAllText(file3, new string('x', 10000));
				File.WriteAllText(file4, new string('x', 10000));

				string file5 = Path.Combine(tempDir2, "file5.txt");
				File.WriteAllText(file5, new string('x', 10000));

				var unmappedPath = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");

				AssertEquals("Zip process runs sucessfully", true, RemoteZipCompression.Zip(tempDir, unmappedPath));
				AssertNoExceptionThrown("Not throw exception when file already exist", () => RemoteZipCompression.Zip(tempDir2, unmappedPath));
				AssertEquals("file.zip should be exported", true, File.Exists(unmappedPath));

				TempFile.TryDelete(unmappedPath, out _);
			}
		}

		public void TestZipInvalidPath()
		{
			using (var tempDir = new TempDirectory())
			{
				string file1 = Path.Combine(tempDir, "file1.txt");
				string file2 = Path.Combine(tempDir, "file2.txt");
				string file3 = Path.Combine(tempDir, "file3.txt");
				string file4 = Path.Combine(tempDir, "file4.txt");
				var unmappedPath = @"~!@#$%^&*()_+-={}[]/|?\><,.:;'//file.zip";

				File.WriteAllText(file1, new string('x', 10000));
				File.WriteAllText(file2, new string('x', 10000));
				File.WriteAllText(file3, new string('x', 10000));
				File.WriteAllText(file4, new string('x', 10000));
				AssertEquals("Zip process should fail for invalid path", false, RemoteZipCompression.Zip(tempDir, unmappedPath));
			}
		}
	}
}
#endif
