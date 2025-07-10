using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Inedo.AssetDirectories;

namespace Enterprise.Client.EDI.Escrow
{
	class AssetDirectoryClientAdapter : IAssetDirectoryClient
	{
		public AssetDirectoryClientAdapter(IProGetAssetDirectoryRegistry proGetAssetDirectoryRegistry)
		{
			if (proGetAssetDirectoryRegistry == null)
			{
				throw new ArgumentNullException(nameof(proGetAssetDirectoryRegistry));
			}
			client = new AssetDirectoryClient(proGetAssetDirectoryRegistry.AssetPathUrl, apiKey: proGetAssetDirectoryRegistry.ApiKey);
		}

		public Task CreateRemoteDirectoryAsync(string path, CancellationToken cancellationToken)
		{
			return client.CreateDirectoryAsync(path, cancellationToken);
		}

		public async Task UploadFileAsync(string localFilePath, string remoteFilePath, CancellationToken cancellationToken)
		{
			using var sourceStream = File.OpenRead(localFilePath);
			using var destinationStream = await client.UploadMultipartFileAsync(remoteFilePath, sourceStream.Length, cancellationToken: cancellationToken);
			await sourceStream.CopyToAsync(destinationStream);
		}

		public Task DeleteRemoteDirectoryAsync(string path, CancellationToken cancellationToken)
		{
			return client.DeleteItemAsync(path, true, cancellationToken);
		}

		readonly AssetDirectoryClient client;
	}
}
