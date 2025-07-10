using System;
using System.Threading.Tasks;
using WTG.AssetDirectories;

namespace Enterprise.Dat.SpecialFileHandling.Assets
{
	public sealed class AssetService : IAssetService
	{
		public static AssetService Create()
		{
			return new AssetService(AssetsClientOptions.Default, ApiKeys.ResourceStringsApiKey);
		}

		public AssetService(AssetsClientOptions fetchOptions, string apiKey)
		{
			if (fetchOptions == null)
			{
				throw new ArgumentNullException(nameof(fetchOptions));
			}

			if (apiKey == null)
			{
				throw new ArgumentNullException(nameof(apiKey));
			}

			this.client = AssetsClientFactory.GetAssetsClient(fetchOptions);
			this.apiKey = apiKey;
		}

		public async Task UploadAsync(string assetName, string fileName)
		{
			if (!await client.AssetExistsAsync(assetName).ConfigureAwait(false))
			{
				await client.UploadAssetAsync(assetName, fileName, apiKey: apiKey).ConfigureAwait(false);
			}
		}

		readonly string apiKey;
		readonly AssetsClient client;
	}
}
