using Microsoft.Extensions.Caching.Memory;

namespace eServices.ApplicationEvent.EnrichmentService.Services;

public class CacheService : IDisposable
{
	public CacheService(IMemoryCache cache, IConfiguration configuration, ILogger<CacheService> logger)
	{
		this.cache = ((MemoryCache)cache);
		this.logger = logger;
		cacheSizeLimit = (long)(configuration.GetValue<long>("CacheSizeLimit") * 1.05);
		cacheExpirationDefaultMinutes = configuration.GetValue<int>("CacheExpirationDefaultMinutes");
		monitor = new Timer(MonitorCallback, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
	}

	public async Task<string> GetOrAddCacheItem(string key, Func<Task<string?>> factory, int? absoluteExpirationRelativeToNowMinutes = null, CancellationToken cancellationToken = default)
	{
		string? cacheItem = null;
		while ((cacheItem = cache.Get<string>(key)) is null)
		{
			var newLock = new TaskCompletionSource();
			if (cacheLock.GetOrAdd(key, k => newLock) is var getLock && getLock == newLock)
			{
				try
				{
					cacheItem = await factory() ?? "";
					var options = new MemoryCacheEntryOptions
					{
						AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(absoluteExpirationRelativeToNowMinutes ?? cacheExpirationDefaultMinutes),
						Size = cacheItem.Length
					};
					cache.Set<string>(key, cacheItem, options);
					if (cacheItem.Length > 1024)
						logger.AddedCacheItemLarge(key, cacheItem.Length);
					else
						logger.AddedCacheItem(key, cacheItem);
					break;
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"Error adding cache item key: '{key}'", ex);
				}
				finally
				{
					getLock.SetResult();
					cacheLock.Remove(key, out _);
				}
			}
			else
			{
				await getLock.Task.WaitAsync(cancellationToken);
			}
		}
		return cacheItem!;
	}

	private void MonitorCallback(object? state)
	{
		try
		{
			if (cache.GetCurrentStatistics() is MemoryCacheStatistics stats)
			{
				var currentSize = stats.CurrentEstimatedSize.GetValueOrDefault();
				logger.CacheStatistics(stats.CurrentEntryCount, currentSize, stats.TotalHits, stats.TotalMisses);
				if (currentSize > cacheSizeLimit)
				{
					logger.CompactingCache(stats.CurrentEstimatedSize.GetValueOrDefault());
					cache.Compact(0.1);
					GC.Collect();
				}
			}
		}
		catch (Exception ex)
		{
			logger.CacheMonitorError(ex);
		}
	}

	public void Dispose()
	{
		monitor?.Dispose();
	}

	private readonly long cacheSizeLimit;
	private readonly int cacheExpirationDefaultMinutes;
	private readonly Timer monitor;
	private readonly ConcurrentDictionary<string, TaskCompletionSource> cacheLock = new();
	private readonly MemoryCache cache;
	private readonly ILogger<CacheService> logger;
}

static partial class CacheServiceLog
{
	[LoggerMessage(Level = LogLevel.Trace, Message = "Added cache item: [{Key}] = \"{Value}\"")]
	public static partial void AddedCacheItem(this ILogger logger, string key, string value);
	[LoggerMessage(Level = LogLevel.Trace, Message = "Added cache item: [{Key}] ({Size:N0} chars)")]
	public static partial void AddedCacheItemLarge(this ILogger logger, string key, long size);
	[LoggerMessage(Level = LogLevel.Information, Message = "Compacting cache. Current estimated size: {CurrentSize:N0}")]
	public static partial void CompactingCache(this ILogger logger, long currentSize);
	[LoggerMessage(Level = LogLevel.Debug, Message = "Cache Statistics - CurrentEntryCount: {CurrentEntryCount:N0}, CurrentEstimatedSize: {CurrentEstimatedSize:N0}, TotalHits: {TotalHits:N0}, TotalMisses: {TotalMisses:N0}")]
	public static partial void CacheStatistics(this ILogger logger, long currentEntryCount, long currentEstimatedSize, long totalHits, long totalMisses);
	[LoggerMessage(Level = LogLevel.Error, Message = "Cache Monitor Error")]
	public static partial void CacheMonitorError(this ILogger logger, Exception ex);
}
