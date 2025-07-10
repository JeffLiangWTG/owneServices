using System;
using System.IO;
using NUnit.Framework;

namespace CargoWise.IO.Testing;

sealed class SftpProcessorTest : TestCase
{
	SftpProcessorForTest processorForTest;
	TempDirectory localTempDirectory;
	TempDirectory remoteTempDirectory;

	public void TestAddressWithSubFolderPath()
	{
		var address = "sftp.wisetechglobal.com/sub/folder/path";
		var processor = new SftpProcessor(address, "Test", "Test", TimeSpan.MaxValue, TimeSpan.MaxValue);
		AssertEquals("Server name should be sftp.wisetechglobal.com", "sftp.wisetechglobal.com", processor.ServerName);
		AssertEquals("Main folder path should be sub/folder/path", "sub/folder/path", processor.MainFolderPath);
	}

	public void TestAddressWithPortNumber()
	{
		var address = "sftp.wisetechglobal.com:10000";
		var processor = new SftpProcessor(address, "Test", "Test", TimeSpan.MaxValue, TimeSpan.MaxValue);
		AssertEquals("Port Number should be 10000", 10000, processor.Port);
	}

	public void TestAppendToFile()
	{
		var remoteFile = Temp.GetTempFileName(remoteTempDirectory);
		var appendedFile = Temp.GetTempFileName(localTempDirectory);

		File.WriteAllText(remoteFile, "This is uploaded file.");
		File.WriteAllText(appendedFile, "This is appended file.");

		processorForTest.AppendToFile(appendedFile, Path.GetFileName(remoteFile));
		AssertFileContent("Content should be: \"This is uploaded file.This is appended file.\"", remoteFile, "This is uploaded file.This is appended file.");
	}

	public void TestDeleteRemoteFile()
	{
		var remoteFile = Temp.GetTempFileName(remoteTempDirectory);
		File.WriteAllText(remoteFile, "Remote");

		processorForTest.DeleteRemoteFile(Path.GetFileName(remoteFile));
		AssertFileNotExist($"{remoteFile} should not be exist any more", remoteFile);
	}

	public void TestDownloadFile()
	{
		var remoteFile = Temp.GetTempFileName(remoteTempDirectory);
		var localFile = Temp.GetTempFileName(localTempDirectory);
		File.WriteAllText(remoteFile, "Remote");

		processorForTest.DownloadFile(localFile, Path.GetFileName(remoteFile));
		AssertFileContent("Downloaded file content should be \"Remote\"", localFile, "Remote");
	}

	public void TestListDirectory()
	{
		for (var i = 0; i < 10; i++)
		{
			var remoteFile = Temp.GetTempFileName(remoteTempDirectory);
			File.WriteAllText(remoteFile, $"index_{i}");
		}

		var result = processorForTest.ListDirectory(remoteTempDirectory);

		AssertEquals($"There should be 10 files in {remoteTempDirectory}", 10, result.Length);

		foreach (var fileName in result)
		{
			AssertFileExist($"{fileName} should in {remoteTempDirectory}", Path.Combine(remoteTempDirectory, fileName));
		}
	}

	public void TestRenameRemoteFile()
	{
		var remoteFile = Temp.GetTempFileName(remoteTempDirectory);
		var newRemoteFile = Temp.GetTempFileName(remoteTempDirectory, "new");

		if (File.Exists(newRemoteFile))
		{
			File.Delete(newRemoteFile);
		}

		File.WriteAllText(remoteFile, "Remote");

		processorForTest.RenameRemoteFile(Path.GetFileName(remoteFile), Path.GetFileName(newRemoteFile));
		AssertFileExist($"{newRemoteFile} should exist", newRemoteFile);
		AssertFileNotExist($"{remoteFile} should not exist", remoteFile);
	}

	public void TestUploadFile()
	{
		var localFile = Temp.GetTempFileName(localTempDirectory);
		var remoteFile = Temp.GetTempFileName(remoteTempDirectory);
		File.WriteAllText(localFile, "LocalFileForTestUpload");

		processorForTest.UploadFile(localFile, Path.GetFileName(remoteFile));
		AssertFileExist($"{remoteFile} should exist", remoteFile);
	}

	public void TestUploadFileUnique()
	{
		Directory.CreateDirectory(Path.Combine(remoteTempDirectory, "SubFolder"));
		AssertEquals(true, Directory.Exists(Path.Combine(remoteTempDirectory, "SubFolder")));

		var localFile = Temp.GetTempFileName(localTempDirectory);
		File.WriteAllText(localFile, "LocalFileForTestUploadUnique");

		processorForTest.UploadFileUnique(localFile, "SubFolder");
		AssertFileExist($"SubFolder/{Path.GetFileName(localFile)} should exist", Path.Combine(remoteTempDirectory, $"SubFolder/{Path.GetFileName(localFile)}"));
	}

	void AssertFileContent(string message, string filePath, string content)
	{
		AssertFileExist($"{filePath} should be exist", filePath);
		var contentInFile = File.ReadAllText(filePath);
		AssertEquals(message, content, contentInFile);
	}

	void AssertFileExist(string message, string filePath)
	{
		AssertEquals(message, true, File.Exists(filePath));
	}

	void AssertFileNotExist(string message, string filePath)
	{
		AssertEquals(message, false, File.Exists(filePath));
	}
	protected override void SetUp()
	{
		localTempDirectory = new TempDirectory();
		remoteTempDirectory = new TempDirectory();
		processorForTest = new SftpProcessorForTest(remoteTempDirectory.DirectoryName);
	}

	protected override void TearDown()
	{
		localTempDirectory.Dispose();
		remoteTempDirectory.Dispose();
	}
}
