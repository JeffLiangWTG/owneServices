using System;
using System.IO;
using System.Net.Http;

namespace WTG.TestHelpers;

public class AssetFetchOptions
{
	public static AssetFetchOptions Default() => new()
	{
		MaxRetryAttempts = 3,
		RetryDelayMs = 500,
		ClientFactory = () => new HttpClient { BaseAddress = new Uri("https://proget.wtg.zone/endpoints/") },
		CacheRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WiseTechGlobal", "Testing", "AssetCache")
	};

	public int MaxRetryAttempts { get; set; }
	public int RetryDelayMs { get; set; }
	public string CacheRoot { get; set; }
	public Func<HttpClient> ClientFactory { get; set; }
}
