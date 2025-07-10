using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.IO;
using Inedo.AssetDirectories;

namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	class GitDownloader : IGitDownloader
	{
		public GitDownloader(IProGetAssetDirectoryPathUrlRegistry proGetAssetDirectoryPathUrlRegistry)
		{
			this.proGetAssetDirectoryPathUrlRegistry = proGetAssetDirectoryPathUrlRegistry ?? throw new ArgumentNullException(nameof(proGetAssetDirectoryPathUrlRegistry));
		}

		public string Download(IWorkingDirectory gitDirectory, CancellationToken cancellationToken)
		{
			var task = Task.Run(async () =>
				{
					var client = new AssetDirectoryClient(proGetAssetDirectoryPathUrlRegistry.AssetPathUrl);
					var tempFileName = Temp.GetTempFileName(gitDirectory.DirectoryName);

					try
					{
						_ = await client.GetItemMetadataAsync(PortableGitPath, cancellationToken);
					}
					catch (Exception ex) when (ex is AssetDirectoryException || ex is JsonException)
					{
						throw new PortableGitNotFoundException(client.EndpointUrl, PortableGitPath, ex);
					}

					using (var destination = File.Create(tempFileName))
					using (var stream = await client.DownloadFileAsync(PortableGitPath, cancellationToken))
					{
						const int defaultBufferSizeFromStreamImplementation = 81920;
						await stream.CopyToAsync(destination, defaultBufferSizeFromStreamImplementation, cancellationToken);
					}

					return tempFileName;
				},
				cancellationToken);
			task.Wait(cancellationToken);
			return task.Result;
		}

		public void UnzipPortableGit(string archiveFileName, IWorkingDirectory gitDirectory)
		{
			try
			{
				ZipFile.ExtractToDirectory(archiveFileName, gitDirectory.DirectoryName);
			}
			catch (Exception ex)
			{
				throw new PortableGitExtractionException(archiveFileName, ex);
			}

			// sleep after unzipping to avoid git not available error
			Thread.Sleep(TimeSpan.FromSeconds(3));
		}

		const string PortableGitPath = "PortableGit.zip";
		readonly IProGetAssetDirectoryPathUrlRegistry proGetAssetDirectoryPathUrlRegistry;
	}
}
