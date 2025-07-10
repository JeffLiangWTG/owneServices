using System.Collections.Generic;
using System.IO;
using System.Threading;
using Enterprise.ZArchitecture.Environment;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZipFileCoreTest : TestCase
	{
		public void TestConstructorAndDispose()
		{
			string originFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.txt");
			string destinationFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");
			try
			{
				File.WriteAllText(originFileName, new string('x', 10000));

				new ZipCreator().CreateZipFile(originFileName, destinationFileName);

				byte[] data = File.ReadAllBytes(destinationFileName);
				ZipFileCore zip;
				long length;
				using (ZipFile tmpZip = new ZipFile(destinationFileName, null))
				{
					length = tmpZip.Count;
				}
				int destinationFileSize = File.ReadAllBytes(destinationFileName).Length;
				using (zip = new ZipFileCore(data))
				{
					AssertEquals(length, zip.Count);
					AssertNotEquals(string.Empty, zip.TempFileName);
					Assert(File.Exists(zip.TempFileName));

					int fileSizeTempZip = File.ReadAllBytes(zip.TempFileName).Length;
					Assert(destinationFileSize == fileSizeTempZip);
				}
				AssertEquals(string.Empty, zip.TempFileName);
				Assert(!File.Exists(zip.TempFileName));

				using (zip = new ZipFileCore(new FileStream(destinationFileName, FileMode.Open)))
				{
					AssertEquals(length, zip.Count);
					AssertNotEquals(string.Empty, zip.TempFileName);
					Assert(File.Exists(zip.TempFileName));

					int fileSizeTempZip = File.ReadAllBytes(zip.TempFileName).Length;
					Assert(destinationFileSize == fileSizeTempZip);
				}
				AssertEquals(string.Empty, zip.TempFileName);
				Assert(!File.Exists(zip.TempFileName));

				using (zip = new ZipFileCore(new MemoryStream(data)))
				{
					AssertEquals(length, zip.Count);
					AssertNotEquals(string.Empty, zip.TempFileName);
					Assert(File.Exists(zip.TempFileName));

					int fileSizeTempZip = File.ReadAllBytes(zip.TempFileName).Length;
					Assert(destinationFileSize == fileSizeTempZip);
				}
				AssertEquals(string.Empty, zip.TempFileName);
				Assert(!File.Exists(zip.TempFileName));

				using (zip = new ZipFileCore(destinationFileName))
				{
					AssertEquals(length, zip.Count);
					AssertNotEquals("", zip.TempFileName);
					Assert(File.Exists(zip.TempFileName));
					AssertEquals(destinationFileName, zip.TempFileName);
				}
				AssertNotEquals(string.Empty, zip.TempFileName);
				Assert(File.Exists(zip.TempFileName));
			}
			finally
			{
				string mes = "";
				TempFile.TryDelete(originFileName, out mes);
				TempFile.TryDelete(destinationFileName, out mes);
			}
		}

		public void TestRemoveEntries()
		{
			string file1 = Path.Combine(EnvProxy.Instance.TempPath, "file1.txt");
			string file2 = Path.Combine(EnvProxy.Instance.TempPath, "file2.txt");
			string file3 = Path.Combine(EnvProxy.Instance.TempPath, "file3.txt");
			string file4 = Path.Combine(EnvProxy.Instance.TempPath, "file4.txt");
			string destinationFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");
			try
			{
				File.WriteAllText(file1, new string('x', 10000));
				File.WriteAllText(file2, new string('x', 10000));
				File.WriteAllText(file3, new string('x', 10000));
				File.WriteAllText(file4, new string('x', 10000));

				new ZipCreator().CreateZipFile(file1, destinationFileName);
				using (ZipFileCore zip = new ZipFileCore(destinationFileName))
				{
					AssertEquals(1, zip.Count);
					zip.BeginUpdate();
					zip.Add(file2);
					zip.Add(file3);
					zip.Add(file4);
					zip.CommitUpdate();

					AssertEquals(4, zip.Count);

					zip.BeginUpdate();
					List<ZipEntry> list = new List<ZipEntry>();
					list.Add(zip[1]);
					list.Add(zip[2]);
					list.Add(zip[3]);
					zip.RemoveEntries(list.ToArray());
					zip.CommitUpdate();
					AssertEquals(1, zip.Count);
				}
			}
			finally
			{
				string mes = "";
				TempFile.TryDelete(file1, out mes);
				TempFile.TryDelete(file2, out mes);
				TempFile.TryDelete(file3, out mes);
				TempFile.TryDelete(file4, out mes);
				TempFile.TryDelete(destinationFileName, out mes);
			}
		}

		public void TestExtractEntry()
		{
			string file1 = Path.Combine(EnvProxy.Instance.TempPath, "file1.txt");
			string file2 = Path.Combine(EnvProxy.Instance.TempPath, "file2.txt");
			string newPath = EnvProxy.Instance.TempPath + "1\\";
			string file3 = Path.Combine(newPath, "file2.txt");
			string destinationFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");
			try
			{
				if (!Directory.Exists(newPath))
				{
					Directory.CreateDirectory(newPath);
				}
				File.WriteAllText(file1, new string('x', 10000));
				File.WriteAllText(file2, new string('x', 5000));

				new ZipCreator().CreateZipFile(file1, destinationFileName);
				using (ZipFileCore zip = new ZipFileCore(destinationFileName))
				{
					AssertEquals(1, zip.Count);
					zip.BeginUpdate();
					zip.Add(file2);

					zip.CommitUpdate();
					AssertEquals(2, zip.Count);
					zip.ExtractEntry(zip[1].Name, newPath);

					int file2Size = File.ReadAllBytes(file2).Length;
					int file3Size = File.ReadAllBytes(file3).Length;
					Assert(file2Size == file3Size);
				}
			}
			finally
			{
				string mes = "";
				TempFile.TryDelete(file1, out mes);
				TempFile.TryDelete(file2, out mes);
				TempFile.TryDelete(destinationFileName, out mes);
				TempFile.TryDelete(file3, out mes);
				int count = 0;
				while (count++ < 10)
				{
					try
					{
						Directory.Delete(newPath);
					}
					catch
					{
						Thread.Sleep(1000);
					}
				}
			}
		}

		public void TestSaveToFile()
		{
			string file1 = Path.Combine(EnvProxy.Instance.TempPath, "file1.txt");
			string destinationFileName = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");
			string file2 = Path.Combine(EnvProxy.Instance.TempPath, "file2.zip");
			try
			{
				File.WriteAllText(file1, new string('x', 10000));

				new ZipCreator().CreateZipFile(file1, destinationFileName);
				using (ZipFileCore zip = new ZipFileCore(destinationFileName))
				{
					Assert(!File.Exists(file2));
					zip.SaveToFile(file2);
					zip.SaveToFile(file2);  // Tests overwrite works
					Assert(File.Exists(file2));
					int file1Size = File.ReadAllBytes(destinationFileName).Length;
					int file2Size = File.ReadAllBytes(file2).Length;
					Assert(file1Size == file2Size);
				}
			}
			finally
			{
				string mes = "";
				TempFile.TryDelete(file1, out mes);
				TempFile.TryDelete(file2, out mes);
				TempFile.TryDelete(destinationFileName, out mes);
			}
		}
	}
}
