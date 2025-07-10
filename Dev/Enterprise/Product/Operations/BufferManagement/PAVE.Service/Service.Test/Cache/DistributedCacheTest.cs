using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.PAVE.Common.Cache;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Cache;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.ZArchitecture.Business;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	[UseSnapshotProtection]
	public class DistributedCacheTest : TestCase
	{
		[TestDate(2023, 10, 20, 1, 2, 5)]
		public void TestGetSet()
		{
			var key = "CacheKey";
			var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();

			AssertNull(distributedCache.Get<TestCacheItem>(key));

			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());
			var cacheItem = new TestCacheItem { Name = "Nome", Description = "Descrição", Value = 10 };

			distributedCache.Set(key, cacheItem, options, "E");
			var value = distributedCache.Get<TestCacheItem>(key);

			AssertEquals(cacheItem, value);

			TestDateAttribute.Date = ZDateTime.Now.AddSeconds(2).ToDateTime();
			value = distributedCache.Get<TestCacheItem>(key);

			AssertNull(value);
		}

		[TestDate(2023, 10, 20, 1, 2, 5)]
		public void TestSet_ShouldReplaceItem()
		{
			var key = "CacheKey";
			var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();

			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());
			var cacheItem1 = new TestCacheItem { Name = "Item1", Description = "Descrição1", Value = 1 };
			var cacheItem2 = new TestCacheItem { Name = "Item2", Description = "Descrição2", Value = 2 };

			distributedCache.Set(key, cacheItem1, options, "E");
			var value = distributedCache.Get<TestCacheItem>(key);

			AssertEquals(cacheItem1, value);

			distributedCache.Set(key, cacheItem2, options, "E");
			value = distributedCache.Get<TestCacheItem>(key);

			AssertEquals(cacheItem2, value);

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			value = distributedCache.Get<TestCacheItem>(key);

			AssertNull(value);
		}

		[TestDate(2023, 10, 20, 1, 2, 5)]
		public void TestSetOrGet()
		{
			var key = "CacheKey";
			var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();

			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());
			var cacheItem = new TestCacheItem { Name = "Nome", Description = "Descrição", Value = 10 };
			var value = distributedCache.GetOrSet(key, () => cacheItem, options, "E");
			var valueFromGet = distributedCache.Get<TestCacheItem>(key);

			AssertEquals(cacheItem, value);
			AssertEquals(cacheItem, valueFromGet);

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			value = distributedCache.Get<TestCacheItem>(key);

			AssertNull(value);
		}

		public void TestRemove()
		{
			var key = "CacheKey";
			var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();

			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());
			var cacheItem = new TestCacheItem { Name = "Nome", Description = "Descrição", Value = 10 };

			distributedCache.Set(key, cacheItem, options, "E");
			var value = distributedCache.Get<TestCacheItem>(key);

			AssertEquals(cacheItem, value);

			distributedCache.Remove(key);

			value = distributedCache.Get<TestCacheItem>(key);
			AssertNull(value);
		}

		public void TestCacheOperationDoNotCrash()
		{
			var key = "CacheKey";
			var cacheItem = new TestCacheItem();
			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());
			var mockDBConnectionFactory = new Mock<IDBConnectionFactory>();
			var distributedCache = new Service.Cache.DistributedCache(new SqlDistributedCache(new SystemClock(), mockDBConnectionFactory.Object));

			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(18470, 1, 1, "", "On NO!", "", 1));
			mockDBConnectionFactory.Setup(m => m.CreateOpenedConnection()).Throws(SqlExceptionBuilder.CreateSqlException(errorCollection));

			AssertNoExceptionThrown("Infra error, system is doomed, so do nothing!", () =>
			{
				distributedCache.Set(key, cacheItem, options, "E");
				AssertNull(distributedCache.Get<TestCacheItem>(key));
				AssertNull(distributedCache.GetOrSet(key, () => cacheItem, options));
				distributedCache.Remove(key);
			});

#if NETFRAMEWORK
			AssertNull("No errors should be reported", ErrorReporter.LastExceptionReported);
#else
			AssertNotNull("Some unexpected sql exception happens, it should report to investigate what is wrong!", ErrorReporter.LastExceptionReported);
#endif
			errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(1, 1, 1, "", "On NO!", "", 1));
			mockDBConnectionFactory.Reset();
			mockDBConnectionFactory.Setup(m => m.CreateOpenedConnection()).Throws(SqlExceptionBuilder.CreateSqlException(errorCollection));

			AssertNoExceptionThrown("Sql error will report, but cache should not crash!", () =>
			{
				distributedCache.Set(key, cacheItem, options, "E");
				AssertNull(distributedCache.Get<TestCacheItem>(key));
				AssertNull(distributedCache.GetOrSet(key, () => cacheItem, options));
				distributedCache.Remove(key);
			});

			AssertEquals("Some inexpected sql exception happens, it should report to investigate what is wrong!", "On NO!", ErrorReporter.LastExceptionReported.Message);

			mockDBConnectionFactory.Reset();
			mockDBConnectionFactory.Setup(m => m.CreateOpenedConnection()).Throws(new System.Exception("Where is my cache?😄"));

			AssertNoExceptionThrown("Error, will report, but cache should not crash!", () =>
			{
				distributedCache.Set(key, cacheItem, options, "E");
				AssertNull(distributedCache.Get<TestCacheItem>(key));
				AssertNull(distributedCache.GetOrSet(key, () => cacheItem, options));
				distributedCache.Remove(key);
			});

#if NETFRAMEWORK
			AssertEquals("Some unexpected exception happens, it should report to investigate what is wrong!", "Where is my cache?😄", ErrorReporter.LastExceptionReported.Message);
#else
			AssertEquals("Some unexpected sql exception happens, it should report to investigate what is wrong!", "On NO!", ErrorReporter.LastExceptionReported.Message);
#endif

			ErrorReporter.Clear();
		}

		[TestDate(2023, 10, 20, 1, 2, 5)]
		[DeveloperOnlyTest]
		public void TestThreadSafe()
		{
			var cacheItem = new TestCacheItem();
			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());

			Parallel.For(0, 100, (index) =>
			{
				var key = "CacheKey" + index;
				using (Db.DisposableActionForDbConnection())
				{
					var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();

					for (var i = 0; i < 100; i++)
					{
						distributedCache.Set(key, cacheItem, options, "E");
						distributedCache.Get<TestCacheItem>(key);
						distributedCache.GetOrSet(key, () => cacheItem, options, "E");
						distributedCache.Remove(key);
					}
				}
			});

#if NETFRAMEWORK
			AssertNull("No errors should be reported", ErrorReporter.LastExceptionReported);
#else
			AssertNotNull("Some unexpected sql exception happens, it should report to investigate what is wrong!", ErrorReporter.LastExceptionReported);
#endif
		}

		[TestDate(2024, 8, 28, 1, 2, 5)]
		public void TestSet_GetsCurrentUserCodeWhenNoCodeIsProvided()
		{
			var mockSqlDistributedCache = new Mock<SqlDistributedCache>(new SystemClock(), new DBConnectionFactory());
			mockSqlDistributedCache.As<ISqlDistributedCache>();

			var cacheItem = new TestCacheItem();
			var options = new DistributedCacheOptions(absoluteExpiration: ZDateTimeOffset.UtcNow.AddSeconds(1).ToDateTimeOffset());

			var distributedCache = new Service.Cache.DistributedCache(mockSqlDistributedCache.Object);
			distributedCache.Set("test", cacheItem, options);

			AssertNoExceptionThrown(() => mockSqlDistributedCache.As<ISqlDistributedCache>().Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), StaticCurrentFetcher.Instance.CurrentUserCode), Times.AtLeastOnce()));
		}

		class TestCacheItem
		{
			public string Name { get; set; }
			public string Description { get; set; }

			public int Value { get; set; }

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				var toCompare = obj as TestCacheItem;

				if (toCompare == null)
				{
					return false;
				}

				return Name.Equals(toCompare.Name) && Description.Equals(toCompare.Description) && Value.Equals(toCompare.Value);
			}

			public override int GetHashCode() => Value.GetHashCode();
		}
	}
}
