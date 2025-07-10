using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class RepositoryProcessorTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			repositoryProcessor = new RepositoryProcessor();
			loggerMock = new Mock<ILogger>();
		}

		public void TestZipsToOutputDirectory()
		{
			// Arrange
			using var inputFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			using var outputFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			using var unzipFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == inputFolder.DirectoryName);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == outputFolder.DirectoryName);
			var includeFiles = CreateIncludedFiles(inputFolder.DirectoryName)
				.Select(s => (s.Replace(inputFolder.DirectoryName, string.Empty).Replace("\\src", string.Empty), File.ReadAllText(s)));
			CreateNotIncludedFiles(inputFolder.DirectoryName);

			// Act
			repositoryProcessor.PrepareAndCopy(workingDirectoryMock, outputDirectoryMock, loggerMock.Object);

			// Assert
			var zip = Directory
				.EnumerateFiles(outputDirectoryMock.DirectoryName, "*", SearchOption.AllDirectories)
				.Single();
			var result = UnzipToFolderAndEnumerate(zip, unzipFolder.DirectoryName);
			AssertContainsExactElementsInAnyOrder(includeFiles,result);

			IEnumerable<(string path, string content)> UnzipToFolderAndEnumerate(string zipPath, string destination)
			{
				ZipFile.ExtractToDirectory(zipPath, destination);
				return Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories)
					.Select(s => (s.Replace(destination, string.Empty), File.ReadAllText(s)));
			}

			static string[] CreateIncludedFiles(string path)
			{
				var files = Enumerable
					.Range(0, 10)
					.Select(i => Path.Combine(path, "src", $"{i:D5}.txt"))
					.Concat(Enumerable
						.Range(100, 10)
						.Select(i => Path.Combine(path, "src", "folder1", $"{i:D5}.txt")))
					.Concat(Enumerable
						.Range(200, 10)
						.Select(i => Path.Combine(path, "src", "folder2", $"{i:D5}.txt")))
					.Concat(Enumerable
						.Range(300, 10)
						.Select(i => Path.Combine(path, "src", "folder2", "folder3", $"{i:D5}.txt")))
					.ToArray();
				foreach (var file in files)
				{
					Directory.CreateDirectory(Path.GetDirectoryName(file));
					File.WriteAllText(file, file);
				}

				return files;
			}

			static void CreateNotIncludedFiles(string path)
			{
				var files = Enumerable
					.Range(1000, 10)
					.Select(i => Path.Combine(path, $"{i:D5}.txt"))
					.Concat(Enumerable
						.Range(2000, 10)
						.Select(i => Path.Combine(path, "source", $"{i:D5}.txt")))
					.Concat(Enumerable
						.Range(3000, 10)
						.Select(i => Path.Combine(path, "temp", $"{i:D5}.txt")));
				foreach (var file in files)
				{
					Directory.CreateDirectory(Path.GetDirectoryName(file));
					File.WriteAllText(file, file);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLogging()
		{
			// Arrange
			using var inputFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			using var outputFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			Directory.CreateDirectory(Path.Combine(inputFolder.DirectoryName, "src"));
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == inputFolder.DirectoryName);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == outputFolder.DirectoryName);

			// Act
			repositoryProcessor.PrepareAndCopy(workingDirectoryMock, outputDirectoryMock, loggerMock.Object);

			// Assert
			loggerMock
				.Verify(logger => logger.Log(
						LogType.Information,
						$"Compressing sources from [{Path.Combine(inputFolder.DirectoryName, "src")}] to [{Path.Combine(outputFolder.DirectoryName, "src.zip")}]..."),
					Times.Once());
			loggerMock
				.Verify(logger => logger.Log(
						LogType.Information,
						It.IsRegex(@"Compressing sources finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\.")),
					Times.Once());
			loggerMock.VerifyNoOtherCalls();
		}

		public void TestGitFoldersAreExcludedFromZipFile()
		{
			// Arrange
			using var inputFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			using var outputFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			using var unzipFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
			var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == inputFolder.DirectoryName);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == outputFolder.DirectoryName);
			var zipPath = Path.Combine(outputFolder.DirectoryName, "src.zip");

			CreateTestFile(inputFolder.DirectoryName, ["src", ".git"]);
			CreateTestFile(inputFolder.DirectoryName, ["src", "folder1"]);
			CreateTestFile(inputFolder.DirectoryName, ["src", "folder1", ".git"]);

			// Act
			repositoryProcessor.PrepareAndCopy(workingDirectoryMock, outputDirectoryMock, loggerMock.Object);

			// Assert
			ZipFile.ExtractToDirectory(zipPath, unzipFolder.DirectoryName);
			var extractedGitDirectories = Directory.GetDirectories(unzipFolder.DirectoryName, ".git", SearchOption.AllDirectories);
			AssertEquals($"A .git directory was found: {string.Join(", ", extractedGitDirectories)}", 0, extractedGitDirectories.Length);
		}

		static void CreateTestFile(string baseDirectory, IEnumerable<string> folderSegments)
		{
			var directoryPath = Path.Combine(folderSegments.Prepend(baseDirectory).ToArray());
			Directory.CreateDirectory(directoryPath);
			var filePath = Path.Combine(directoryPath, "file.txt");
			File.WriteAllText(filePath, ".");
		}

		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => repositoryProcessor.PrepareAndCopy(null, Mock.Of<IWorkingDirectory>(), Mock.Of<ILogger>()));
			AssertEquals("workingDirectory", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => repositoryProcessor.PrepareAndCopy(Mock.Of<IWorkingDirectory>(), null, Mock.Of<ILogger>()));
			AssertEquals("outputDirectory", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => repositoryProcessor.PrepareAndCopy(Mock.Of<IWorkingDirectory>(), Mock.Of<IWorkingDirectory>(), null));
			AssertEquals("logger", result.ParamName);
		}

		RepositoryProcessor repositoryProcessor;
		Mock<ILogger> loggerMock;
	}
}
