using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.Escrow
{
	[TestedType(typeof(EscrowExportSourceCodeServiceTask))]
	class EscrowExportSourceCodeServiceTaskTest : ServiceTaskTestCase<EscrowExportSourceCodeServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceAttributes()
		{
			var serviceAttribute = GetHostedServiceAttributes().Single();
			AssertEquals(false, serviceAttribute.ActiveByDefault);
			AssertEquals(true, serviceAttribute.CanRunInAnyBranch);
			AssertEquals("CSP", serviceAttribute.Category);
			AssertEquals("ESC", serviceAttribute.Code);
			AssertEquals("Escrow Export Source code", serviceAttribute.Description);
			AssertEquals("3months", serviceAttribute.DefaultScheduleRunEvery);
			AssertEquals(1, serviceAttribute.DefaultScheduleDayOfMonth);
			AssertEquals("0seconds", serviceAttribute.DefaultScheduleStartAtUtc);
			AssertEquals("1year", serviceAttribute.MaximumPeriod);
			AssertEquals("1week", serviceAttribute.MinimumPeriod);
		}

		public void TestOrderOfEvents()
		{
			// Arrange
			var callSequence = new List<string>();
			var tempDirectoryMock = new Mock<ITempDirectory>();
			var outputDirectoryMock = new Mock<ITempDirectory>();
			var gitDirectoryMock = new Mock<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateTempDirectory(It.IsAny<ILogger>()))
				.Returns(tempDirectoryMock.Object)
				.Callback(() => callSequence.Add($"{nameof(IDirectoryAdapter)}.{nameof(IDirectoryAdapter.CreateTempDirectory)}"));
			directoryAdapterMock
				.Setup(directory => directory.CreateOutputDirectory(It.IsAny<ILogger>()))
				.Returns(outputDirectoryMock.Object)
				.Callback(() => callSequence.Add($"{nameof(IDirectoryAdapter)}.{nameof(IDirectoryAdapter.CreateOutputDirectory)}"));
			directoryAdapterMock
				.Setup(directory => directory.CreateGitDirectory(It.IsAny<ILogger>()))
				.Returns(gitDirectoryMock.Object)
				.Callback(() => callSequence.Add($"{nameof(IDirectoryAdapter)}.{nameof(IDirectoryAdapter.CreateGitDirectory)}"));
			gitDownloaderMock
				.Setup(downloader => downloader.Download(It.IsAny<IWorkingDirectory>(), It.IsAny<CancellationToken>()))
				.Callback(() => callSequence.Add($"{nameof(IGitDownloader)}.{nameof(IGitDownloader.Download)}"));
			gitDownloaderMock
				.Setup(downloader => downloader.UnzipPortableGit(It.IsAny<string>(), It.IsAny<IWorkingDirectory>()))
				.Callback(() => callSequence.Add($"{nameof(IGitDownloader)}.{nameof(IGitDownloader.UnzipPortableGit)}"));
			repositoryRetrieverMock
				.Setup(retriever => retriever.DownloadRepositories(It.IsAny<IWorkingDirectory>(), It.IsAny<ILogger>(), It.IsAny<IWorkingDirectory>()))
				.Callback(() => callSequence.Add($"{nameof(IRepositoryRetriever)}.{nameof(IRepositoryRetriever.DownloadRepositories)}"));
			repositoryProcessorMock
				.Setup(processor => processor.PrepareAndCopy(It.IsAny<IWorkingDirectory>(), It.IsAny<IWorkingDirectory>(), It.IsAny<ILogger>()))
				.Callback(() => callSequence.Add($"{nameof(IRepositoryProcessor)}.{nameof(IRepositoryProcessor.PrepareAndCopy)}"));
			binaryProcessorMock
				.Setup(processor => processor.FindAndCopy(It.IsAny<IWorkingDirectory>(), It.IsAny<IReadOnlyCollection<IRepository>>(), It.IsAny<ILogger>()))
				.Callback(() => callSequence.Add($"{nameof(IBinaryProcessor)}.{nameof(IBinaryProcessor.FindAndCopy)}"));
			exporterMock
				.Setup(exporter => exporter.Export(It.IsAny<IWorkingDirectory>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
				.Callback(() => callSequence.Add($"{nameof(IExporter)}.{nameof(IExporter.Export)}"));
			incidentCreatorMock
				.Setup(creator => creator.Create(It.IsAny<IExportResult>(), It.IsAny<ILogger>()))
				.Callback(() => callSequence.Add($"{nameof(IIncidentCreator)}.{nameof(IIncidentCreator.Create)}"));
			outputDirectoryMock
				.Setup(workingDirectory => workingDirectory.Dispose())
				.Callback(() => callSequence.Add($"{nameof(IWorkingDirectory)}.{nameof(IWorkingDirectory.Dispose)}"));
			tempDirectoryMock
				.Setup(tempDirectory => tempDirectory.Dispose())
				.Callback(() => callSequence.Add($"{nameof(ITempDirectory)}.{nameof(ITempDirectory.Dispose)}"));

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					"IDirectoryAdapter.CreateTempDirectory",
					"IDirectoryAdapter.CreateOutputDirectory",
					"IDirectoryAdapter.CreateGitDirectory",
					"IGitDownloader.Download",
					"IGitDownloader.UnzipPortableGit",
					"IRepositoryRetriever.DownloadRepositories",
					"IRepositoryProcessor.PrepareAndCopy",
					"IBinaryProcessor.FindAndCopy",
					"IExporter.Export",
					"IIncidentCreator.Create",
					"IWorkingDirectory.Dispose",
					"ITempDirectory.Dispose",
				},
				callSequence);
		}

		public void TestLoggingOnSuccessIsSufficient()
		{
			// Arrange
			var logs = new List<string>();
			loggerMock
				.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback<LogType, string>((logType, message) => logs.Add($"{logType}|{message}"));

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					"Information|Export starting",
					"Information|Downloading portable git zip",
					"Information|Unzipping portable git",
					"Information|Downloading source codes",
					"Information|Preparing source codes",
					"Information|Preparing binaries",
					"Information|Exporting results",
					"Information|Creating incident",
					"Information|Export finished",
				},
				logs);
		}

		[ExpectNoExceptions]
		public void TestDirectoryAdapterGetsCorrectLogger()
		{
			// Arrange

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			directoryAdapterMock.Verify(adapter => adapter.CreateTempDirectory(serviceTask.ServiceLogger), Times.Once);
			directoryAdapterMock.Verify(adapter => adapter.CreateOutputDirectory(serviceTask.ServiceLogger), Times.Once);
			directoryAdapterMock.Verify(adapter => adapter.CreateGitDirectory(serviceTask.ServiceLogger), Times.Once);
			directoryAdapterMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestRepositoryRetrieverGetsCorrectDirectories()
		{
			// Arrange
			var tempDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateTempDirectory(It.IsAny<ILogger>()))
				.Returns(tempDirectoryMock);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateOutputDirectory(It.IsAny<ILogger>()))
				.Returns(outputDirectoryMock);
			var gitDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateGitDirectory(It.IsAny<ILogger>()))
				.Returns(gitDirectoryMock);

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			repositoryRetrieverMock.Verify(
				retriever => retriever.DownloadRepositories(tempDirectoryMock, serviceTask.ServiceLogger, gitDirectoryMock),
				Times.Once);
			repositoryRetrieverMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestRepositoryProcessorGetsCorrectParameters()
		{
			// Arrange
			var tempDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateTempDirectory(It.IsAny<ILogger>()))
				.Returns(tempDirectoryMock);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateOutputDirectory(It.IsAny<ILogger>()))
				.Returns(outputDirectoryMock);
			var gitDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateGitDirectory(It.IsAny<ILogger>()))
				.Returns(gitDirectoryMock);

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			repositoryProcessorMock.Verify(
				processor => processor.PrepareAndCopy(tempDirectoryMock, outputDirectoryMock, loggerMock.Object),
				Times.Once);
			repositoryProcessorMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestBinaryProcessorGetsCorrectParameters()
		{
			// Arrange
			var tempDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateTempDirectory(It.IsAny<ILogger>()))
				.Returns(tempDirectoryMock);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateOutputDirectory(It.IsAny<ILogger>()))
				.Returns(outputDirectoryMock);
			var gitDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateGitDirectory(It.IsAny<ILogger>()))
				.Returns(gitDirectoryMock);
			var downloadedRepositoriesMock = Mock.Of<IReadOnlyCollection<IRepository>>();
			repositoryRetrieverMock
				.Setup(retriever => retriever.DownloadRepositories(It.IsAny<IWorkingDirectory>(), It.IsAny<ILogger>(), It.IsAny<IWorkingDirectory>()))
				.Returns(downloadedRepositoriesMock);

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			binaryProcessorMock.Verify(
				processor => processor.FindAndCopy(outputDirectoryMock, downloadedRepositoriesMock, loggerMock.Object),
				Times.Once);
			binaryProcessorMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestExporterGetsCorrectParameters()
		{
			// Arrange
			var tempDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateTempDirectory(It.IsAny<ILogger>()))
				.Returns(tempDirectoryMock);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateOutputDirectory(It.IsAny<ILogger>()))
				.Returns(outputDirectoryMock);
			var gitDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateGitDirectory(It.IsAny<ILogger>()))
				.Returns(gitDirectoryMock);

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			exporterMock.Verify(
				exporter => exporter.Export(outputDirectoryMock, loggerMock.Object, It.IsAny<CancellationToken>()),
				Times.Once);
			exporterMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestIncidentCreatorGetsCorrectParameters()
		{
			// Arrange
			var tempDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateTempDirectory(It.IsAny<ILogger>()))
				.Returns(tempDirectoryMock);
			var outputDirectoryMock = Mock.Of<IWorkingDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateOutputDirectory(It.IsAny<ILogger>()))
				.Returns(outputDirectoryMock);
			var gitDirectoryMock = Mock.Of<ITempDirectory>();
			directoryAdapterMock
				.Setup(directory => directory.CreateGitDirectory(It.IsAny<ILogger>()))
				.Returns(gitDirectoryMock);
			var exportResult = Mock.Of<IExportResult>();
			exporterMock
				.Setup(exporter => exporter.Export(It.IsAny<IWorkingDirectory>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
				.Returns(exportResult);

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			incidentCreatorMock.Verify(
				creator => creator.Create(exportResult, loggerMock.Object),
				Times.Once);
			incidentCreatorMock.VerifyNoOtherCalls();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			directoryAdapterMock = new Mock<IDirectoryAdapter>();
			repositoryRetrieverMock = new Mock<IRepositoryRetriever>();
			repositoryProcessorMock = new Mock<IRepositoryProcessor>();
			binaryProcessorMock = new Mock<IBinaryProcessor>();
			exporterMock = new Mock<IExporter>();
			incidentCreatorMock = new Mock<IIncidentCreator>();
			gitDownloaderMock = new Mock<IGitDownloader>();
			serviceTask = new EscrowExportSourceCodeServiceTask(
				directoryAdapterMock.Object,
				repositoryRetrieverMock.Object,
				repositoryProcessorMock.Object,
				binaryProcessorMock.Object,
				exporterMock.Object,
				incidentCreatorMock.Object,
				gitDownloaderMock.Object);
			loggerMock = new Mock<ILogger>();
			serviceTask.ServiceLogger = loggerMock.Object;
		}

		Mock<IBinaryProcessor> binaryProcessorMock;
		Mock<IDirectoryAdapter> directoryAdapterMock;
		Mock<IExporter> exporterMock;
		Mock<ILogger> loggerMock;
		Mock<IRepositoryProcessor> repositoryProcessorMock;
		Mock<IRepositoryRetriever> repositoryRetrieverMock;
		EscrowExportSourceCodeServiceTask serviceTask;
		Mock<IIncidentCreator> incidentCreatorMock;
		Mock<IGitDownloader> gitDownloaderMock;
	}
}
