using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Common
{
	class MemoryCacheTest
	{
		[Test]
		public void TestReturnsNullOnNoValue()
		{
			// Arrange

			// Act
			var result = memoryCache!.Get<string>("key");

			// Assert
			Assert.That(result, Is.EqualTo(default(string)));
		}

		[Test]
		public void TestReturnsValue()
		{
			Test("key", "value");
			Test("key", 123);
			Test("key", 123.456);
			Test("key1", "value");
			Test("key1", 123);
			Test("key1", 123.456);

			void Test<T>(string key, T value)
			{
				// Arrange
				memoryCache!.Set(key, value, TimeSpan.FromHours(1));

				// Act
				var result = memoryCache.Get<T>(key);

				// Assert
				Assert.That(result, Is.EqualTo(value));
			}
		}

		[Test]
		public void TestReturnsDefaultOnExpire()
		{
			Test("key", new MyStruct { I = 16, S = "M" });
			Test("key", 123);
			Test("key", 123.456);
			Test("key1", new MyStruct { I = 16, S = "M" });
			Test("key1", 123);
			Test("key1", 123.456);

			void Test<T>(string key, T value) where T : struct
			{
				// Arrange
				memoryCache!.Set(key, value, TimeSpan.FromSeconds(1));
				Thread.Sleep(TimeSpan.FromSeconds(1.5));

				// Act
				var result = memoryCache.Get<T>(key);

				// Assert
				Assert.That(result, Is.EqualTo(default(T)));
			}
		}

		[Test]
		public void TestReturnsNullOnExpire()
		{
			Test("key", "value");
			Test("key", (int?)123);
			Test("key", (double?)123.456);
			Test("key1", "value");
			Test("key1", (int?)123);
			Test("key1", (double?)123.456);

			void Test<T>(string key, T value)
			{
				// Arrange
				memoryCache!.Set(key, value, TimeSpan.FromSeconds(1));
				Thread.Sleep(TimeSpan.FromSeconds(1.5));

				// Act
				var result = memoryCache.Get<T>(key);

				// Assert
				Assert.That(result, Is.EqualTo(default(T)));
			}
		}

		[Test]
		public void TestReturnOverwrittenValue()
		{
			Test("key", "value", "newValue");
			Test("key", 123, 456);
			Test("key", 123.456, 456.123);
			Test("key1", "value", "newValue");
			Test("key1", 123, 456);
			Test("key1", 123.456, 456.123);

			void Test<T>(string key, T value, T newValue)
			{
				// Arrange
				memoryCache!.Set(key, value, TimeSpan.FromHours(1));
				memoryCache.Set(key, newValue, TimeSpan.FromHours(1));

				// Act
				var result = memoryCache.Get<T>(key);

				// Assert
				Assert.That(result, Is.EqualTo(newValue));
			}
		}

		[Test]
		public void TestOperationCancelledExceptionThrownWhenAccessingCacheDuringShutdown_AddOrGetExisting() => AssertOperationCancelledExceptionThrownPriorToCacheAccessDuringShutdown((m) => m.AddOrGetExisting("something", () => "test", TimeSpan.FromSeconds(5)));
		[Test]
		public void TestOperationCancelledExceptionThrownWhenAccessingCacheDuringShutdown_AddOrGetExistingTimeoutFunction() => AssertOperationCancelledExceptionThrownPriorToCacheAccessDuringShutdown((m) => m.AddOrGetExisting("something", () => "test", () => TimeSpan.FromSeconds(5)));
		[Test]
		public void TestOperationCancelledExceptionThrownWhenAccessingCacheDuringShutdown_Get() => AssertOperationCancelledExceptionThrownPriorToCacheAccessDuringShutdown((m) => m.Get<object>("something"));
		[Test]
		public void TestOperationCancelledExceptionThrownWhenAccessingCacheDuringShutdown_Set() => AssertOperationCancelledExceptionThrownPriorToCacheAccessDuringShutdown((m) => m.Set("something", () => "test", TimeSpan.FromSeconds(5)));

		void AssertOperationCancelledExceptionThrownPriorToCacheAccessDuringShutdown(Action<MemoryCache> action)
		{
			// Arrange
			var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
			memoryCacheMock
				.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
				.Throws(new ObjectDisposedException("objectName"));
			using var memoryCacheWithDisposedInternal = new MemoryCache(memoryCacheMock.Object, cancellationTokenProviderMock);
			cancellationTokenSource!.Cancel();

			// Act
			// Assert
			Assert.Throws<OperationCanceledException>(() => action(memoryCacheWithDisposedInternal));
		}

		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = memoryCache!.Get<object>(null));
				Assert.That(result?.ParamName, Is.EqualTo("key"));

				result = Assert.Throws<ArgumentNullException>(() => memoryCache!.Set(null, string.Empty, TimeSpan.Zero));
				Assert.That(result?.ParamName, Is.EqualTo("key"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new MemoryCache(null, cancellationTokenProviderMock));
				Assert.That(result?.ParamName, Is.EqualTo("memoryCache"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new MemoryCache(Mock.Of<Microsoft.Extensions.Caching.Memory.IMemoryCache>(), null));
				Assert.That(result?.ParamName, Is.EqualTo("cancellationTokenProvider"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new MemoryCache(null));
				Assert.That(result?.ParamName, Is.EqualTo("cancellationTokenProvider"));

				Assert.DoesNotThrow(() => memoryCache!.Set(string.Empty, (object?)null, TimeSpan.FromSeconds(1)));
				Assert.DoesNotThrow(() => memoryCache!.Set(string.Empty, string.Empty, TimeSpan.FromSeconds(1)));
			});
		}

		class AddOrGetExistingTest
		{
			[Test]
			public void TestCacheValuePresentBeforeLock()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(true);

				// Act
				var result = hostMemoryCache.AddOrGetExisting<object>("key", () => "test", TimeSpan.FromMinutes(1));

				// Assert
				Assert.That(cachedObject, Is.EqualTo(result));
				memoryCacheMock.Verify(m => m.CreateEntry(It.Is<string>(ce => ce == "key")), Times.Never);
				cacheEntryMock.VerifySet(ce => ce.Value = cachedObject, Times.Never);
				memoryCacheMock.Verify(m => m.TryGetValue(It.Is<string>(ce => ce == "key"), out cachedObject), Times.Once);
			}

			[Test]
			public void TestCacheValuePresentAfterLock()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.SetupSequence(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(false)
					.Returns(true);

				// Act
				var result = hostMemoryCache.AddOrGetExisting<object>("key", () => "test", TimeSpan.FromMinutes(1));

				// Assert
				Assert.That(cachedObject, Is.EqualTo(result));
				memoryCacheMock.Verify(m => m.CreateEntry(It.Is<string>(ce => ce == "key")), Times.Never);
				cacheEntryMock.VerifySet(ce => ce.Value = cachedObject, Times.Never);
				memoryCacheMock.Verify(m => m.TryGetValue(It.Is<string>(ce => ce == "key"), out cachedObject), Times.Exactly(2));
			}

			[Test]
			public void TestCacheValuePresentBeforeLock_FunctionNotCalled()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(true);

				// Act
				var functionCalled = false;
				var nonCachedObject = new object();
				var result = hostMemoryCache.AddOrGetExisting(
					"key",
					() =>
					{
						functionCalled = true;
						return nonCachedObject;
					},
					TimeSpan.FromMinutes(1));

				// Assert
				Assert.That(cachedObject, Is.EqualTo(result));
				Assert.That(functionCalled, Is.EqualTo(false));
				memoryCacheMock.Verify(m => m.CreateEntry(It.Is<string>(ce => ce == "key")), Times.Never);
				cacheEntryMock.VerifySet(ce => ce.Value = cachedObject, Times.Never);
				memoryCacheMock.Verify(m => m.TryGetValue(It.Is<string>(ce => ce == "key"), out cachedObject), Times.Once);
			}

			[Test]
			public void TestCacheValuePresentAfterLock_FunctionNotCalled()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.SetupSequence(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(false)
					.Returns(true);

				// Act
				var functionCalled = false;
				var nonCachedObject = new object();
				var result = hostMemoryCache.AddOrGetExisting(
					"key",
					() =>
					{
						functionCalled = true;
						return nonCachedObject;
					},
					TimeSpan.FromMinutes(1));

				// Assert
				Assert.That(cachedObject, Is.EqualTo(result));
				Assert.That(functionCalled, Is.EqualTo(false));
				memoryCacheMock.Verify(m => m.CreateEntry(It.Is<string>(ce => ce == "key")), Times.Never);
				cacheEntryMock.VerifySet(ce => ce.Value = cachedObject, Times.Never);
				memoryCacheMock.Verify(m => m.TryGetValue(It.Is<string>(ce => ce == "key"), out cachedObject), Times.Exactly(2));
			}

			[Test]
			public void TestCacheValueNotPresent_LogFunctionIsCalled()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.SetupSequence(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(false)
					.Returns(false);

				// Act
				var functionCalled = false;
				var nonCachedObject = new object();
				var result = hostMemoryCache.AddOrGetExisting(
					"key",
					() =>
					{
						functionCalled = true;
						return nonCachedObject;
					},
					TimeSpan.FromMinutes(1));

				// Assert
				Assert.That(functionCalled, Is.EqualTo(true));
				memoryCacheMock.Verify(m => m.CreateEntry(It.Is<string>(ce => ce == "key")), Times.Once);
				cacheEntryMock.VerifySet(ce => ce.Value = nonCachedObject, Times.Once);
				memoryCacheMock.Verify(m => m.TryGetValue(It.Is<string>(ce => ce == "key"), out cachedObject), Times.Exactly(2));
			}

			[Test]
			public void TestCacheValueNotPresent_TimeSpanFunctionIsCalled()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.SetupSequence(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(false)
					.Returns(false);

				// Act
				var timeSpan = TimeSpan.FromMinutes(30);
				var result = hostMemoryCache.AddOrGetExisting(
					"key",
					() =>
					{
						timeSpan = TimeSpan.FromMinutes(1);
						return new object();
					},
					() => timeSpan);

				// Assert
				Assert.That(timeSpan, Is.EqualTo(TimeSpan.FromMinutes(1)));
			}

			[Test]
			public void TestCacheValueNotPresent_ValueReturnedFromNewEntryOfCache()
			{
				// Arrange
				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);
				var cachedObject = new object();
				_ = memoryCacheMock
					.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns(cacheEntryMock.Object);
				_ = memoryCacheMock
					.SetupSequence(m => m.TryGetValue(It.IsAny<object>(), out cachedObject))
					.Returns(false)
					.Returns(false);

				// Act
				var nonCachedObject = new object();
				var result = hostMemoryCache.AddOrGetExisting("key", () => nonCachedObject, TimeSpan.FromMinutes(1));

				// Assert
				Assert.That(nonCachedObject, Is.EqualTo(result));
				memoryCacheMock.Verify(m => m.CreateEntry(It.Is<string>(ce => ce == "key")), Times.Once);
				cacheEntryMock.VerifySet(ce => ce.Value = nonCachedObject, Times.Once);
				memoryCacheMock.Verify(m => m.TryGetValue(It.Is<string>(ce => ce == "key"), out cachedObject), Times.Exactly(2));
			}

			[Test]
			public void TestOnlyOneSubmits()
			{
				// Arrange
				var key = "testKey";
				var value = "testValue";
				var expiration = TimeSpan.FromMinutes(30);

				var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
				var cacheEntryMock = new Mock<ICacheEntry>();
				using var hostMemoryCache = new MemoryCache(memoryCacheMock.Object, cancellationTokenProvider);

				bool valueSet = false;

				memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
					.Returns(() => valueSet);

				memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
					.Returns<object>(m =>
					{
						valueSet = true;
						return cacheEntryMock.Object;
					});

				// Act
				Parallel.For(0, 10, i =>
				{
					hostMemoryCache.AddOrGetExisting(key, () => value, expiration);
				});

				// Assert
				memoryCacheMock.Verify(m => m.CreateEntry(key), Times.Once);
			}

			[SetUp]
			public void SetUp()
			{
				cancellationTokenProvider = Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None);
			}

			ICancellationTokenProvider? cancellationTokenProvider;
		}

		[SetUp]
		public void SetUp()
		{
			cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenProviderMock = Mock.Of<ICancellationTokenProvider>(p => p.Token == cancellationTokenSource.Token);
			memoryCache = new MemoryCache(cancellationTokenProviderMock);
		}

		MemoryCache? memoryCache;
		ICancellationTokenProvider? cancellationTokenProviderMock;
		CancellationTokenSource? cancellationTokenSource;

		struct MyStruct
		{
			public int I { get; set; }
			public string S { get; set; }
		}
	}
}
