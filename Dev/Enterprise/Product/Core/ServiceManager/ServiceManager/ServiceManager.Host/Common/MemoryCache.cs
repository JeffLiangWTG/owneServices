using System;
using Microsoft.Extensions.Caching.Memory;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class MemoryCache : IMemoryCache, IDisposable
	{
		public MemoryCache(ICancellationTokenProvider cancellationTokenProvider)
			: this(
				new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions()),
				cancellationTokenProvider)
		{
		}

		internal MemoryCache(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ICancellationTokenProvider cancellationTokenProvider)
		{
			this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			this.cancellationTokenProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
		}

		public void Dispose()
		{
			memoryCache.Dispose();
		}

		public void Set<T>(string key, T value, TimeSpan expiration)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			Set(memoryCache, cancellationTokenProvider, key, value, expiration);
		}

		public T Get<T>(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			return Get<T>(memoryCache, cancellationTokenProvider, key);
		}

		public T AddOrGetExisting<T>(string key, Func<T> function, TimeSpan expiration)
		{
			return AddOrGetExisting(key, function, () => expiration);
		}

		public T AddOrGetExisting<T>(string key, Func<T> function, Func<TimeSpan> expiration)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (TryGetValue(memoryCache, cancellationTokenProvider, key, out T cache))
			{
				return cache;
			}

			lock (key)
			{
				if (TryGetValue(memoryCache, cancellationTokenProvider, key, out cache))
				{
					return cache;
				}

				var value = function();
				var expirationTimeSpan = expiration();
				return Set(memoryCache, cancellationTokenProvider, key, value, expirationTimeSpan);
			}
		}

		static bool TryGetValue<T>(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ICancellationTokenProvider cancellationTokenProvider, object key, out T cache)
		{
			cancellationTokenProvider.Token.ThrowIfCancellationRequested();
			return memoryCache.TryGetValue(key, out cache);
		}

		static T Get<T>(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ICancellationTokenProvider cancellationTokenProvider, object key)
		{
			cancellationTokenProvider.Token.ThrowIfCancellationRequested();
			return memoryCache.Get<T>(key);
		}

		static T Set<T>(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ICancellationTokenProvider cancellationTokenProvider, object key, T value, TimeSpan expiration)
		{
			cancellationTokenProvider.Token.ThrowIfCancellationRequested();
			return memoryCache.Set(key, value, expiration);
		}

		readonly Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache;
		readonly ICancellationTokenProvider cancellationTokenProvider;
	}
}
