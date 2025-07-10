using System.IO;
using System.Linq;
using CargoWise.IO;
using Enterprise.Dat.SpecialFileHandling.Assets;
using NUnit.Framework;
using WTG.AssetDirectories.TestFramework;

namespace Enterprise.Dat.SpecialFileHandling.Testing.Assets
{
	class AssetServiceTest : TestCase
	{
		public void TestUpload()
		{
			const string AssetName = "FooBar";
			const string ApiKey = "API-KEY";
			const string Content = "SuperAwsomeContent";

			var service = new DummyService()
			{
				ApiKey = ApiKey,
			};

			using var tempDir = new TempDirectory();
			var filename = Path.Combine(tempDir.DirectoryName, "Magic.txt");
			File.WriteAllText(filename, Content);

			var assets = new AssetService(service.CreateOptions(cacheRoot: null), ApiKey);
			assets.UploadAsync(AssetName, filename).GetAwaiter().GetResult();

			var asset = service.Assets.Single().Value;
			AssertEquals(AssetName, asset.AssetName);
			AssertEquals(Content, asset.Content);
		}

		public void TestUploadExisting()
		{
			const string AssetName = "FooBar";
			const string ApiKey = "API-KEY";
			const string Content = "SuperAwsomeContent";

			var service = new DummyService()
			{
				ApiKey = ApiKey,
			};

			var existing = service.SetAsset(AssetName, Content);

			using var tempDir = new TempDirectory();
			var filename = Path.Combine(tempDir.DirectoryName, "Magic.txt");
			File.WriteAllText(filename, Content);

			var assets = new AssetService(service.CreateOptions(cacheRoot: null), ApiKey);
			// Silently ignore existing assets rather than throwing.
			assets.UploadAsync(AssetName, filename).GetAwaiter().GetResult();

			AssertSame("Should not have updated the server value.", existing, service.Assets.Single().Value);
		}
	}
}
