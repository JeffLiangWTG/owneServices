using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WTG.TestHelpers;

public static class AssetsHelper
{
	static readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new();

	static SemaphoreSlim GetLock(string key) => locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

	public static string FetchTestAsset(string assetPath) => FetchTestAssetAsync(assetPath, AssetFetchOptions.Default(), CancellationToken.None).GetAwaiter().GetResult();

	public static Task<string> FetchTestAssetAsync(string assetPath) => FetchTestAssetAsync(assetPath, AssetFetchOptions.Default(), CancellationToken.None);

	public static Task<string> FetchTestAssetAsync(string assetPath, CancellationToken cancellationToken) => FetchTestAssetAsync(assetPath, AssetFetchOptions.Default(), cancellationToken);

	public static async Task<string> FetchTestAssetAsync(string assetPath, AssetFetchOptions options, CancellationToken cancellationToken)
	{
		if (options == null)
		{
			throw new ArgumentNullException(nameof(options));
		}

		if (options.ClientFactory == null)
		{
			throw new ArgumentException("ClientFactory cannot be null", nameof(options));
		}

		if (options.CacheRoot == null)
		{
			throw new ArgumentException("CacheRoot cannot be null", nameof(options));
		}

		using var httpClient = options.ClientFactory.Invoke();

		var key = ToSha256(assetPath);
		var sem = GetLock(key);
		await sem.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			var cacheRoot = options.CacheRoot;
			Directory.CreateDirectory(cacheRoot);
			var cachePath = Path.Combine(cacheRoot, key);
			var metaPath = cachePath + ".meta";
			var meta = TryGetMeta(metaPath);

			using var response = await SendWithRetryAsync(httpClient, assetPath, meta, options.MaxRetryAttempts, options.RetryDelayMs, cancellationToken).ConfigureAwait(false);
			if (response.StatusCode == HttpStatusCode.NotModified && File.Exists(cachePath))
			{
				return cachePath;
			}

			response.EnsureSuccessStatusCode();

			if(File.Exists(cachePath))
			{
				File.SetAttributes(cachePath, File.GetAttributes(cachePath) & ~FileAttributes.ReadOnly);
			}
			using (var fs = File.Create(cachePath))
			{
				await response.Content.CopyToAsync(fs).ConfigureAwait(false);
			}
			File.SetAttributes(cachePath, File.GetAttributes(cachePath) | FileAttributes.ReadOnly);

			WriteMeta(response, metaPath);
			return cachePath;
		}
		finally
		{
			sem.Release();
		}
	}

	static async Task<HttpResponseMessage> SendWithRetryAsync(HttpClient http, string assetPath, CacheMeta meta, int maxRetryAttempts, int retryDelayMs, CancellationToken ct)
	{
		var attempt = 0;
		Exception lastException = null;

		for (; attempt <= maxRetryAttempts; attempt++)
		{
			try
			{
				using var request = BuildRequest(assetPath, meta);
				return await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
			}
			catch (HttpRequestException ex)
			{
				lastException = ex;
				if (attempt == maxRetryAttempts)
				{
					break;
				}

				await Task.Delay(retryDelayMs, ct).ConfigureAwait(false);
			}
		}

		throw new IOException($"Unable to reach {http.BaseAddress}{assetPath}. Possible reasons: offline, DNS failure, or not connected to the VPN.", lastException);
	}

	static HttpRequestMessage BuildRequest(string assetPath, CacheMeta meta)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, assetPath);
		if (meta != null)
		{
			var eTagUsed = false;

			if (!string.IsNullOrEmpty(meta.ETag))
			{
				if (request.Headers.TryAddWithoutValidation("If-None-Match", meta.ETag))
				{
					eTagUsed = true;
				}
			}
			if (!eTagUsed && meta.LastModified is not null)
			{
				request.Headers.IfModifiedSince = meta.LastModified;
			}
		}

		return request;
	}

	static string ToSha256(string input)
	{
		using var sha = SHA256.Create();
		var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}

	static void WriteMeta(HttpResponseMessage response, string metaPath)
	{
		var newMeta = new CacheMeta
		{
			ETag = response.Headers.ETag?.Tag ?? (response.Headers.TryGetValues("ETag", out var v)
					   ? v.FirstOrDefault()
					   : null),
			LastModified = response.Content.Headers.LastModified
		};

		File.WriteAllText(metaPath, JsonSerializer.Serialize(newMeta));
	}

	static CacheMeta TryGetMeta(string metaPath)
	{
		if (File.Exists(metaPath))
		{
			try
			{
				return JsonSerializer.Deserialize<CacheMeta>(File.ReadAllText(metaPath));
			}
			catch
			{
				return null;
			}
		}

		return null;
	}

	record CacheMeta
	{
		public string ETag { get; set; }
		public DateTimeOffset? LastModified { get; set; }
	}
}
