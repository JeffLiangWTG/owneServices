using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	class ProGetExporter : IExporter
	{
		public ProGetExporter(IProGetAssetDirectoryRegistry proGetAssetDirectoryRegistry, IAssetDirectoryClient assetDirectoryClient)
		{
			this.proGetAssetDirectoryRegistry = proGetAssetDirectoryRegistry ?? throw new ArgumentNullException(nameof(proGetAssetDirectoryRegistry));
			this.assetDirectoryClient = assetDirectoryClient ?? throw new ArgumentNullException(nameof(assetDirectoryClient));
		}

		public IExportResult Export(IWorkingDirectory localDirectory, ILogger logger, CancellationToken cancellationToken)
		{
			var task = Task.Run(async () =>
			{
				try
				{
					var year = ZDateTime.Now.Year;
					var month = ZDateTime.Now.Month;
					var remoteFolderName = $"Escrow{year}{month:D2}";

					await ExportDirectoryAsync(localDirectory.DirectoryName, remoteFolderName, logger, cancellationToken);

					return new ExportResult(GetDirUrl(remoteFolderName));
				}
				catch (Exception e)
				{
					logger.Log(LogType.Error, e.Message);
					throw;
				}
			}, cancellationToken);

			return task.Result;
		}

		async Task ExportDirectoryAsync(string localRootPath, string remoteRootPath, ILogger logger, CancellationToken cancellationToken)
		{
			await CreateRemoteDirectoriesAsync();

			await UploadAllFilesAsync();

			async Task CreateRemoteDirectoriesAsync()
			{
				var createRemoteDirectoriesStopwatch = Stopwatch.StartNew();

				logger.Log(LogType.Information, $"> Creating remote folders...");

				var uri = GetDirUrl(remoteRootPath);
				logger.Log(LogType.Information, $"-> Delete root remote folder [{uri}]");
				await assetDirectoryClient.DeleteRemoteDirectoryAsync(remoteRootPath, cancellationToken);

				var directories = Directory.GetDirectories(localRootPath, "*", SearchOption.AllDirectories);

				logger.Log(LogType.Information, $"-> Create root remote folder [{uri}]");
				await assetDirectoryClient.CreateRemoteDirectoryAsync(remoteRootPath, cancellationToken);

				for (var i = 0; i < directories.Length; i++)
				{
					await CreateRemoteSubDirectoryAsync(i + 1, directories.Length, localRootPath.Length, directories[i], remoteRootPath);
				}

				logger.Log(LogType.Information, $"> Creating remote folders finished in {createRemoteDirectoriesStopwatch.Elapsed:G}.");
			}

			async Task UploadAllFilesAsync()
			{
				var uploadAllFilesStopwatch = Stopwatch.StartNew();

				var title = $"> Uploading files to [{GetDirUrl(remoteRootPath)}]";
				logger.Log(LogType.Information, $"{title}...");

				var files = Directory.GetFiles(localRootPath, "*", SearchOption.AllDirectories);

				for (var i = 0; i < files.Length; i++)
				{
					await UploadFileAsync(i + 1, files.Length, localRootPath.Length, files[i], remoteRootPath);
				}

				logger.Log(LogType.Information, $"{title} finished in {uploadAllFilesStopwatch.Elapsed:G}.");
			}

			async Task CreateRemoteSubDirectoryAsync(int index, int dirCount, int localRootPathLength, string directory, string remoteRootDirPath)
			{
				var relativePath = directory
					.Substring(localRootPathLength)
					.TrimStart(Path.DirectorySeparatorChar);
				var remoteDirPath = Path.Combine(remoteRootDirPath, relativePath);

				logger.Log(LogType.Information, $"-> Create remote folder [{index}/{dirCount}, {GetDirUrl(remoteDirPath)}]");
				await assetDirectoryClient.CreateRemoteDirectoryAsync(remoteDirPath, cancellationToken);
			}

			async Task UploadFileAsync(int index, int fileCount, int localRootPathLength, string file, string remoteRootDirPath)
			{
				var uploadStopwatch = Stopwatch.StartNew();
				var relativePath = file
					.Substring(localRootPathLength)
					.TrimStart(Path.DirectorySeparatorChar);
				var remoteFilePath = Path.Combine(remoteRootDirPath, relativePath);
				var subtitle = $"-> Uploading file [{index}/{fileCount}, .{Path.DirectorySeparatorChar}{relativePath}]";

				logger.Log(LogType.Information, $"{subtitle}...");
				await assetDirectoryClient.UploadFileAsync(file, remoteFilePath, cancellationToken);
				logger.Log(LogType.Information, $"{subtitle} finished in {uploadStopwatch.Elapsed:G}");
			}
		}

		string GetDirUrl(string dirPath)
		{
			return $"{proGetAssetDirectoryRegistry.AssetPathUrl.TrimEnd('/')}/{dirPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}";
		}

		record ExportResult : IExportResult
		{
			public ExportResult(string remotePath)
			{
				RemotePath = remotePath;
			}

			public string RemotePath { get; }
		}

		readonly IProGetAssetDirectoryRegistry proGetAssetDirectoryRegistry;
		readonly IAssetDirectoryClient assetDirectoryClient;
	}
}
