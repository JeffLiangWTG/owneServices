using System;
using System.IO;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	abstract class DirectoryAdapterTest : TestCase
	{
		protected abstract Func<ILogger, IWorkingDirectory> CreateDirectoryFunc { get; }

		protected override void SetUp()
		{
			base.SetUp();

			directoryAdapter = new DirectoryAdapter();
			loggerMock = new Mock<ILogger>();
		}

		public void TestCreatesDirectory()
		{
			// Arrange

			// Act
			using var result = CreateDirectoryFunc(loggerMock.Object);

			// Assert
			AssertEquals(true, Directory.Exists(result.DirectoryName));
		}

		public void TestDeletesDirectory()
		{
			// Arrange
			var tempDirectory = CreateDirectoryFunc(loggerMock.Object);

			// Act
			tempDirectory.Dispose();

			// Assert
			AssertEquals(false, Directory.Exists(tempDirectory.DirectoryName));
		}

		[ExpectNoExceptions]
		public void TestLogsOnErrorInDeletesDirectory()
		{
			// Arrange
			using var tempDirectory = CreateDirectoryFunc(loggerMock.Object);
			using (File.OpenWrite(Path.Combine(tempDirectory.DirectoryName, Path.GetRandomFileName())))
			{
				// Act
				tempDirectory.Dispose();
			}

			// Assert
			loggerMock.Verify(logger => logger.Log(LogType.Warning, It.IsAny<string>(), It.IsAny<IOException>()), Times.Once);
			loggerMock.VerifyNoOtherCalls();
		}

		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => CreateDirectoryFunc(null));
			AssertEquals("logger", result.ParamName);
		}

		DirectoryAdapter directoryAdapter;
		Mock<ILogger> loggerMock;

		public class CreateTempDirectoryTest : DirectoryAdapterTest
		{
			protected override Func<ILogger, IWorkingDirectory> CreateDirectoryFunc => logger => directoryAdapter.CreateTempDirectory(logger);

			public void TestUsesShortPathNameWhileNotInDotNet()
			{
				// Arrange
				using var tempDirectory = CreateDirectoryFunc(loggerMock.Object);

				// Act
				var result = tempDirectory.DirectoryName;

				// Assert
				AssertEquals(true, string.Equals(@"C:\Esc", result, StringComparison.OrdinalIgnoreCase));
			}
		}

		public class CreateOutputDirectoryTest : DirectoryAdapterTest
		{
			protected override Func<ILogger, IWorkingDirectory> CreateDirectoryFunc => logger => directoryAdapter.CreateOutputDirectory(logger);

			public void TestCreatesDirectoryInCargoWiseTemp()
			{
				// Arrange

				// Act
				using var result = CreateDirectoryFunc(loggerMock.Object);

				// Assert
				AssertStartsWith(Temp.TempPathWithoutCreating, Temp.TempPathWithoutCreating, result.DirectoryName);
				AssertNotEquals(Path.GetFullPath(Temp.TempPathWithoutCreating), Path.GetFullPath(result.DirectoryName));
			}
		}
	}
}
