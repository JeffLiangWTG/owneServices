using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WTG.TestHelpers.Test
{
	public class AssetsHelperTest : NUnit.Framework.TestCase
	{
		public void TestFetchTestAssetAsync() 
		{
			const string assetPath = "/fake/asset.txt";
			const string fileText = "hello world";

			var tempCache = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				Directory.CreateDirectory(tempCache);
				var stubHandler = new StubHandler(fileText, failFirstTime: false);
				var options = new AssetFetchOptions
				{
					MaxRetryAttempts = 1,
					RetryDelayMs = 0,
					CacheRoot = tempCache,
					ClientFactory = () => new HttpClient(stubHandler) { BaseAddress = new Uri("http://unit.test") }
				};

				var cachedPath1 = AssetsHelper.FetchTestAssetAsync(assetPath, options, CancellationToken.None).GetAwaiter().GetResult();
				Assert("File does not exist", File.Exists(cachedPath1));
				var cachedPath2 = AssetsHelper.FetchTestAssetAsync(assetPath, options, CancellationToken.None).GetAwaiter().GetResult();
				Assert("Paths are different", cachedPath1.Equals(cachedPath2));
				Assert("More than one download", stubHandler.newFileCalls.Equals(1));
			}
			finally
			{
				ForceDeleteDirectory(tempCache);
			}
		}

		public void TestFetchTestAssetAsyncDoesRetry()
		{
			const string assetPath = "/fake/asset.txt";
			const string fileText = "hello world";

			var tempCache = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				Directory.CreateDirectory(tempCache);
				var stubHandler = new StubHandler(fileText, failFirstTime: true);
				var options = new AssetFetchOptions
				{
					MaxRetryAttempts = 1,
					RetryDelayMs = 0,
					CacheRoot = tempCache,
					ClientFactory = () => new HttpClient(stubHandler) { BaseAddress = new Uri("http://unit.test") }
				};

				var cachedPath = AssetsHelper.FetchTestAssetAsync(assetPath, options, CancellationToken.None).GetAwaiter().GetResult();
				Assert("File does not exist", File.Exists(cachedPath));
				Assert("Didn't download twice", stubHandler.newFileCalls.Equals(2));
			}
			finally
			{
				ForceDeleteDirectory(tempCache);
			}
		}

		public void TestFetchTestAssetAsyncRecoversMetadata()
		{
			const string assetPath = "/fake/asset.txt";
			const string fileText = "hello world";

			var tempCache = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				Directory.CreateDirectory(tempCache);
				var stubHandler = new StubHandler(fileText, failFirstTime: false);
				var options = new AssetFetchOptions
				{
					MaxRetryAttempts = 1,
					RetryDelayMs = 0,
					CacheRoot = tempCache,
					ClientFactory = () => new HttpClient(stubHandler) { BaseAddress = new Uri("http://unit.test") }
				};

				var cachedPath1 = AssetsHelper.FetchTestAssetAsync(assetPath, options, CancellationToken.None).GetAwaiter().GetResult();
				File.WriteAllText($"{cachedPath1}.meta", "Corrupted metadata");
				var cachedPath2 = AssetsHelper.FetchTestAssetAsync(assetPath, options, CancellationToken.None).GetAwaiter().GetResult();
				Assert("File does not exist", File.Exists(cachedPath2));
				Assert("Didn't download twice", stubHandler.newFileCalls.Equals(2));
			}
			finally
			{
				ForceDeleteDirectory(tempCache);
			}
		}

		public void TestFetchTestAssetAsyncFileIsReadOnly()
		{
			const string assetPath = "/fake/asset.txt";
			const string fileText = "hello world";

			var tempCache = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				Directory.CreateDirectory(tempCache);
				var stubHandler = new StubHandler(fileText, failFirstTime: false);
				var options = new AssetFetchOptions
				{
					MaxRetryAttempts = 1,
					RetryDelayMs = 0,
					CacheRoot = tempCache,
					ClientFactory = () => new HttpClient(stubHandler) { BaseAddress = new Uri("http://unit.test") }
				};

				var cachedPath1 = AssetsHelper.FetchTestAssetAsync(assetPath, options, CancellationToken.None).GetAwaiter().GetResult();
				AssertExceptionThrown<UnauthorizedAccessException>(() => File.WriteAllText(cachedPath1, "I'm breaking this file"));
			}
			finally
			{
				ForceDeleteDirectory(tempCache);
			}
		}

		void ForceDeleteDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				return;
			}

			foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
			{
				var attrs = File.GetAttributes(file);
				if ((attrs & FileAttributes.ReadOnly) != 0)
				{
					File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
}
			}

			Directory.Delete(directory, recursive: true);
		}

		sealed class StubHandler(string text, bool failFirstTime, string etag = "\"abc123\"") : DelegatingHandler
		{
			public int newFileCalls;
			readonly byte[] payload = System.Text.Encoding.UTF8.GetBytes(text);

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				HttpResponseMessage resp;
				var requestEtag = request.Headers.IfNoneMatch.FirstOrDefault()?.Tag;
				if (requestEtag != etag)
				{
					newFileCalls++;
					if (failFirstTime && newFileCalls == 1)
					{
						throw new HttpRequestException("Simulated failure on first request");
					}

					resp = new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new ByteArrayContent(payload)
					};
					resp.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue(etag);
				}
				else
				{
					resp = new HttpResponseMessage(HttpStatusCode.NotModified);
				}

				return Task.FromResult(resp);
			}
		}
	}
}
