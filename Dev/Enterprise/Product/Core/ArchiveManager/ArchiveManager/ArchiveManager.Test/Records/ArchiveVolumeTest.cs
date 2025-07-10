using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ArchiveManager.Business.Records;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	sealed class ArchiveVolumeTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVolume()
		{
			using var tempDir = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var manager = new ArchiveVolumeManager(tempDir.DirectoryName, 2M);
			AssertNull("CurrentVolume", manager.CurrentVolume);

			manager.CreateNewVolume(logger, descriptor);
			AssertNotNull("CurrentVolume", manager.CurrentVolume);

			var volume = manager.CurrentVolume;
			AssertEquals("ExceededCapacity", expected: false, volume.ExceededCapacity);

			var sourceFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\1MB.dat";

			MemoryStream imageStream;
			using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
			{
				imageStream = new MemoryStream((int)stream.Length);
				CopyStream(stream, imageStream);
			}

			volume.Add("testFile1.tif", imageStream);
			volume.Add("testFile2.tif", imageStream);
			AssertEquals("ExceededCapacity", expected: false, volume.ExceededCapacity);

			volume.Add("testFile3.tif", imageStream);
			AssertEquals("ExceededCapacity", expected: true, volume.ExceededCapacity);

			var previousVolumeNo = volume.VolumeNo;
			var previousVolume = volume;
			manager.CreateNewVolume(logger, descriptor);
			volume = manager.CurrentVolume;
			volume.Add("testFile4", imageStream);

			AssertEquals("new volumeNo", previousVolumeNo + 1, manager.CurrentVolume.VolumeNo);
			AssertEquals("previous volume VolumeState", VolumeState.Closed, previousVolume.VolumeState);
			AssertEquals("previous volume VolumeZipPath", previousVolume.VolumePath + ".zip", previousVolume.VolumeZipPath);
			Assert("zip file exists", File.Exists(previousVolume.VolumeZipPath));
			AssertEquals("previous volume is not encrypted", expected: false, previousVolume.IsVolumeCrypted());
			previousVolume.Extract("testFile1.tif", tempDir.DirectoryName);
			Assert("testFile1 file exists", File.Exists(Path.Combine(tempDir.DirectoryName, "testFile1.tif")));
			Assert("temp files deleted", !Directory.Exists(previousVolume.VolumePath));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddCanHandleInvalidFileNameChars()
		{
			using var tempDir = new TempDirectory();
			const string fileNameWithInvalidChar = "Company Name (Location) Limited - EMAIL COPY - HAWB No: TST0123456";
			var invalidFileNameChars = Path.GetInvalidFileNameChars();
			Assert("Precondition: Filename contains an invalid character.", fileNameWithInvalidChar.IndexOfAny(invalidFileNameChars) >= 0);

			var logger = new TestArchiveLogger();
			var manager = new ArchiveVolumeManager(tempDir.DirectoryName, 2M);
			var sourceFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\1MB.dat";

			AssertNull("CurrentVolume", manager.CurrentVolume);
			manager.CreateNewVolume(logger, DummyArchiveSystem);
			AssertNotNull("CurrentVolume", manager.CurrentVolume);
			var volume = manager.CurrentVolume;
			Assert("ExceededCapacity", !volume.ExceededCapacity);

			MemoryStream imageStream;
			using (FileStream stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
			{
				imageStream = new MemoryStream((int)stream.Length);
				CopyStream(stream, imageStream);
			}

			const string validFileName = "Company Name (Location) Limited - EMAIL COPY - HAWB No_ TST0123456";

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Filename with invalid chars should be handled.", () => volume.Add(fileNameWithInvalidChar, imageStream));
				volume.Close();
				volume.Extract(validFileName, tempDir.DirectoryName);
				Assert("Filename with invalid chars should be handled correctly and added to the archive volume.", File.Exists(Path.Combine(tempDir.DirectoryName, validFileName)));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddVolumeWithLongFileNames()
		{
			var message = "These files are way too long even by themselves";
			var reallyLongFileNames = new List<string>();

			for (var i = 0; i < 5; i++)
			{
				var reallylongFiLeName = new string('a', 2000) + i.ToString() + ".dat";
				reallyLongFileNames.Add(reallylongFiLeName);
			}

			AssertAddFilesToVolumeThrowsNoException(reallyLongFileNames, message);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddVolumeWithLongFullyQualifiedUniqueFileNames()
		{
			var message = "These files are not too long, but once appended to the file path the path is too long";
			var reallyLongFileNames = new List<string>();

			for (var i = 0; i < 5; i++)
			{
				var reallylongFiLeName = new string('a', 200) + i.ToString() + ".dat";
				reallyLongFileNames.Add(reallylongFiLeName);
			}

			AssertAddFilesToVolumeThrowsNoException(reallyLongFileNames, message);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddVolumeWithLongFullyQualifiedDuplicateFileNames()
		{
			var message = "These files are not too long, but once appended to the file path the path is too long";
			var reallyLongFileNames = new List<string>();

			for (var i = 0; i < 5; i++)
			{
				var reallylongFiLeName = new string('a', 200) + ".dat";
				reallyLongFileNames.Add(reallylongFiLeName);
			}

			AssertAddFilesToVolumeThrowsNoException(reallyLongFileNames, message);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddVolumeWithFileNameThatIsTooLongOnceDuplicateTagIsAdded()
		{
			var message = "These files are just barely short enough, but become too long once the numbers at the end (in case of duplicates) are added.";
			var tempDir = new TempDirectory();
			var reallyLongFileNames = new List<string>();

			for (var i = 0; i < 5; i++)
			{
				var reallyLongFileName = new string('a', 259 - ".dat".Length - tempDir.DirectoryName.Length) + ".dat";
				reallyLongFileNames.Add(reallyLongFileName);
			}

			AssertAddFilesToVolumeThrowsNoException(reallyLongFileNames, message, tempDir);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddVolumeWithDuplicateFileNames()
		{
			var message = "These files all have the same name";
			var duplicateFileNames = new List<string>();

			for (var i = 0; i < 5; i++)
			{
				var duplicateFileName = new string('a', 2) + ".dat";
				duplicateFileNames.Add(duplicateFileName);
			}

			AssertAddFilesToVolumeThrowsNoException(duplicateFileNames, message, fileNamesAreTooLong: false);
		}

		void AssertAddFilesToVolumeThrowsNoException(List<string> fileNames, string message, TempDirectory offlineDirectory = null, bool fileNamesAreTooLong = true)
		{
			using var tempDir = offlineDirectory ?? new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			var manager = new ArchiveVolumeManager(tempDir.DirectoryName, 2M);
			var sourceFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\1MB.dat";

			AssertNull("CurrentVolume", manager.CurrentVolume);
			manager.CreateNewVolume(logger, descriptor);
			AssertNotNull("CurrentVolume", manager.CurrentVolume);
			var volume = manager.CurrentVolume;
			AssertEquals("ExceededCapacity", expected: false, volume.ExceededCapacity);

			MemoryStream imageStream;

			using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
			{
				imageStream = new MemoryStream((int)stream.Length);
				CopyStream(stream, imageStream);
			}

			foreach (var file in fileNames)
			{
				AssertNoExceptionThrown(message, () => volume.Add(file, imageStream));
			}

			var numberOfFiles = fileNames.Count;
			var filesNamesInVolume = Directory.GetFiles(volume.VolumePath);
			AssertEquals("Volume should contain all files", numberOfFiles, filesNamesInVolume.Length);
			Assert("All files should retain their extensions", filesNamesInVolume.Where(f => f.Substring(f.Length - 4) != ".dat").IsNullOrEmpty());

			if (fileNamesAreTooLong)
			{
				AssertEquals("We should have logs saying that long file names were truncated", numberOfFiles,
					logger.ListOfMessages.Where(m => m.Contains(".dat was truncated")).Count());
			}
		}

		public void TestExtractVolumeWithPassword()
		{
			using var tempDir = new TempDirectory();
			var volumeWithPassword = new FastZip();
			volumeWithPassword.Password = "CoG0W1sE";
			_ = Directory.CreateDirectory(tempDir + @"\Vol-password");
			var path = tempDir + @"\Vol-password";
			using (var outputFile = new StreamWriter(Path.Combine(path, "testFile.txt")))
			{
				outputFile.WriteLine("test");
			}

			volumeWithPassword.CreateZip(Path.Combine(tempDir.DirectoryName, "Vol-00000001.zip"), tempDir + @"\Vol-password", recurse: true, string.Empty);

			var volume = ArchiveVolume.LoadVolume(Path.Combine(tempDir.DirectoryName, "Vol-00000001.zip"));
			AssertEquals("Volume is crypted", expected: true, volume.IsVolumeCrypted());
			volume.Extract("testFile.txt", tempDir.DirectoryName);
			Assert("testFile.txt file exists", File.Exists(Path.Combine(tempDir.DirectoryName, "testFile.txt")));
			Assert("temp files deleted", !Directory.Exists(volume.VolumePath));
		}

		public void TestExtractVolumeWithWrongZipFile()
		{
			using var tempDir = new TempDirectory();
			var volumeWithPassword = new FastZip();
			_ = Directory.CreateDirectory(tempDir + @"\Vol-test");
			var path = tempDir + @"\Vol-test";
			using (var outputFile = new StreamWriter(Path.Combine(path, "testFile2.txt")))
			{
				outputFile.WriteLine("test");
			}

			volumeWithPassword.CreateZip(Path.Combine(tempDir.DirectoryName, "Vol-00000001.zip"), tempDir + @"\Vol-test", recurse: true, string.Empty);

			var volume = ArchiveVolume.LoadVolume(Path.Combine(tempDir.DirectoryName, "Vol-00000001.zip"));
			_ = AssertExceptionThrown<InvalidOperationException>("Unable to find the file in this volume.", () => volume.Extract("testFile.txt", tempDir.DirectoryName));
		}

		public void TestVolumeIfVolumePathOrZipFileExists()
		{
			using var tempDir = new TempDirectory();
			var logger = new TestArchiveLogger();
			var descriptor = DummyArchiveSystem;
			_ = Directory.CreateDirectory(tempDir + @"\Vol-00000001");
			_ = Directory.CreateDirectory(tempDir + @"\Vol-00000002");
			_ = Directory.CreateDirectory(tempDir + @"\Vol-00000003");
			var manager = new ArchiveVolumeManager(tempDir.DirectoryName, 2M);
			AssertNull("CurrentVolume", manager.CurrentVolume);
			manager.CreateNewVolume(logger, descriptor);
			Assert("CurrentVolume", manager.CurrentVolume.VolumeNo == 4);
			AssertContains("The Log Should Contain", "The system will create a new volume.", logger.ListOfMessages[0]);
		}

		static void CopyStream(Stream source, Stream target)
		{
			const int bufSize = 0x1000;
			var buf = new byte[bufSize];
			int bytesRead;
			while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
			{
				target.Write(buf, 0, bytesRead);
			}
		}

		DummyArchiveSystem DummyArchiveSystem
			=> new();
	}
}
