using System.Collections.Generic;
using CargoWise.PAVE.Common.Cache;
using CargoWise.Types;
using Moq;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ReleaseGateFailureLogService_ForTest : ReleaseGateFailureLogService
	{
		public ReleaseGateFailureLogService_ForTest()
		{
			distributedCacheMock = new Mock<IDistributedCache>();

			distributedCacheMock.Setup(m => m.Set(It.IsAny<string>(), It.IsAny<ReleaseGateFailureCacheItem>(), It.IsAny<DistributedCacheOptions>(), It.IsAny<string>()))
				.Callback((string key, ReleaseGateFailureCacheItem value, DistributedCacheOptions options, string userCode) =>
				{
					cache[key] = (value, options);
				});

			distributedCacheMock.Setup(m => m.Get<ReleaseGateFailureCacheItem>(It.IsAny<string>()))
				.Returns((string key) =>
				{
					if (!cache.TryGetValue(key, out var cacheItem))
					{
						return null;
					}

					if (cacheItem.options.AbsoluteExpiration < ZDateTimeOffset.UtcNow)
					{
						return null;
					}

					return cacheItem.value;
				});
		}

		readonly Dictionary<string, (ReleaseGateFailureCacheItem value, DistributedCacheOptions options)> cache = new Dictionary<string, (ReleaseGateFailureCacheItem value, DistributedCacheOptions options)>();
		readonly Mock<IDistributedCache> distributedCacheMock;

		protected override IDistributedCache DistributedCache => distributedCacheMock.Object;
	}
}
