using System;
using System.Net.Http;
using System.Threading;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ESC",
	"Escrow Export Source code",
	"CSP",
	typeof(EscrowExportSourceCodeServiceTask),
	MinimumPeriod = "1week",
	MaximumPeriod = "1year",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "3months",
	DefaultScheduleDayOfMonth = 1,
	DefaultScheduleStartAtUtc = "0seconds",
	ActiveByDefault = false
	)]

namespace Enterprise.Client.EDI.Escrow
{
	class EscrowExportSourceCodeServiceTask : ServiceProviderImpl
	{
		public EscrowExportSourceCodeServiceTask()
			: this(
				new DirectoryAdapter(),
				new RepositoryRetriever(
					new DataRegistryProvider(),
					new GitAdapter(),
					new GitHubInstallationTokenFactory(new DataRegistryProvider(), new GitHubClientFactory()),
					new DataRegistryProvider()),
				new RepositoryProcessor(),
				new BinaryProcessor(new WTG.Foundation.Http.HttpClientFactory(() => new HttpClientHandler())),
				new ProGetExporter(new DataRegistryProvider(), new AssetDirectoryClientAdapter(new DataRegistryProvider())),
				new IncidentCreator(new DataRegistryProvider()),
				new GitDownloader(new DataRegistryProvider()))
		{
		}

		internal EscrowExportSourceCodeServiceTask(IDirectoryAdapter directoryAdapter, IRepositoryRetriever repositoryRetriever, IRepositoryProcessor repositoryProcessor, IBinaryProcessor binaryProcessor, IExporter exporter, IIncidentCreator incidentCreator, IGitDownloader gitDownloader)
		{
			this.directoryAdapter = directoryAdapter ?? throw new ArgumentNullException(nameof(directoryAdapter));
			this.repositoryRetriever = repositoryRetriever ?? throw new ArgumentNullException(nameof(repositoryRetriever));
			this.repositoryProcessor = repositoryProcessor ?? throw new ArgumentNullException(nameof(repositoryProcessor));
			this.binaryProcessor = binaryProcessor ?? throw new ArgumentNullException(nameof(binaryProcessor));
			this.exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
			this.incidentCreator = incidentCreator ?? throw new ArgumentNullException(nameof(incidentCreator));
			this.gitDownloader = gitDownloader ?? throw new ArgumentNullException(nameof(gitDownloader));
		}

		public override void RunTask(CancellationToken cancellationToken)
		{
			ServiceLogger.Log(LogType.Information, "Export starting");
			using var tempDirectory = directoryAdapter.CreateTempDirectory(ServiceLogger);
			using var outputDirectory = directoryAdapter.CreateOutputDirectory(ServiceLogger);
			using var gitDirectory = directoryAdapter.CreateGitDirectory(ServiceLogger);

			ServiceLogger.Log(LogType.Information, "Downloading portable git zip");
			var archiveFileName = gitDownloader.Download(gitDirectory, cancellationToken);

			ServiceLogger.Log(LogType.Information, "Unzipping portable git");
			gitDownloader.UnzipPortableGit(archiveFileName, gitDirectory);

			ServiceLogger.Log(LogType.Information, "Downloading source codes");
			var downloadedRepositories = repositoryRetriever.DownloadRepositories(tempDirectory, ServiceLogger, gitDirectory);

			ServiceLogger.Log(LogType.Information, "Preparing source codes");
			repositoryProcessor.PrepareAndCopy(tempDirectory, outputDirectory, ServiceLogger);

			ServiceLogger.Log(LogType.Information, "Preparing binaries");
			binaryProcessor.FindAndCopy(outputDirectory, downloadedRepositories, ServiceLogger);

			ServiceLogger.Log(LogType.Information, "Exporting results");
			var exportResult = exporter.Export(outputDirectory, ServiceLogger, cancellationToken);

			ServiceLogger.Log(LogType.Information, "Creating incident");
			incidentCreator.Create(exportResult, ServiceLogger);

			ServiceLogger.Log(LogType.Information, "Export finished");
		}

		readonly IBinaryProcessor binaryProcessor;
		readonly IExporter exporter;
		readonly IDirectoryAdapter directoryAdapter;
		readonly IRepositoryProcessor repositoryProcessor;
		readonly IRepositoryRetriever repositoryRetriever;
		readonly IIncidentCreator incidentCreator;
		readonly IGitDownloader gitDownloader;
	}
}
