using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class ExpiringDictionaryTest : TransactionedTestCase
	{
		public void TestExpiringDictionaryImplementsCorrectly()
		{
			var instance = new ExpiringConcurrentDictionary<object, object>(TimeSpan.FromSeconds(5));
			var key = new object();
			var expectedValue = new object();
			var actualValue = new object();

			Assert(!instance.Contains(key));

			instance.Add(key, expectedValue);

			Assert(instance.Contains(key));

			instance.TryGetValue(key, out actualValue);
			AssertEquals(expectedValue, actualValue);

			instance.Remove(key);

			Assert(!instance.Contains(key));
		}

		public void TestClear()
		{
			var instance = new ExpiringConcurrentDictionary<object, object>(TimeSpan.FromSeconds(5));
			var key = new object();
			var value = new object();

			instance.Add(key, value);

			Assert(instance.Contains(key));

			instance.Clear();

			Assert(!instance.Contains(key));
		}

		public void TestIndexer()
		{
			var instance = new ExpiringConcurrentDictionary<object, object>(TimeSpan.FromSeconds(5));
			var key = new object();
			var expectedValue = new object();
			var actualValue = new object();

			instance.Add(key, expectedValue);

			instance.TryGetValue(key, out actualValue);
			AssertEquals(expectedValue, actualValue);
		}

		public void TestDictionaryExpiresViaContains()
		{
			var refresh_ms = 200;
			var instance = new ExpiringConcurrentDictionary<object, object>(TimeSpan.FromMilliseconds(refresh_ms));
			var key = new object();
			var value = new object();

			instance.Add(key, value);
			AssertEquals(true, instance.Contains(key));
			AssertEquals(true, instance.Contains(key));

			Thread.Sleep(refresh_ms);
			AssertEquals(false, instance.Contains(key));

			instance.Add(key, value);
			AssertEquals(true, instance.Contains(key));
			AssertEquals(true, instance.Contains(key));
		}

		public void TestDictionaryExpiresViaTryGetValue()
		{
			var refresh_ms = 200;
			var instance = new ExpiringConcurrentDictionary<object, object>(TimeSpan.FromMilliseconds(refresh_ms));
			var key = new object();
			var value = new object();
			object actualValue;

			instance.Add(key, value);
			AssertEquals(true, instance.TryGetValue(key, out actualValue));
			AssertEquals(true, instance.TryGetValue(key, out actualValue));

			Thread.Sleep(refresh_ms);
			AssertEquals(false, instance.TryGetValue(key, out actualValue));

			instance.Add(key, value);
			AssertEquals(true, instance.TryGetValue(key, out actualValue));
			AssertEquals(true, instance.TryGetValue(key, out actualValue));
		}

		public void TestNoExceptionsOnMultiThreadedAccess()
		{
			var refresh_ms = 100_000; // Do not use expire clear functionality in this unit test
			var cache = new ExpiringConcurrentDictionary<string, string>(TimeSpan.FromMilliseconds(refresh_ms));
			var key = "ABC";

			Parallel.For(0, 100, i =>
			{
				var value = i.ToString();

				if (!cache.Contains(key))
				{
					Thread.Sleep(1);
					AssertNoExceptionThrown("Should not fail if item was already added in different thread.", () => { cache.Add(key, value); });
				}

				Assert("Should contain item with this key", cache.Contains(key));
			});
		}
	}
}
