using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class MRUCacheTest : TestCase
	{
		public void TestSynchronization_FillCache()
		{
			var count = 10;
			var cache = new SynchronizedMRUCache<string, string>(count);
			var threads = new Thread[count];
			for (var i = 0; i < count; ++i)
			{
				var num = i; //I think they're closuring over i which keeps changing afterwards. threads, man
				threads[i] = new Thread(() =>
				{
					cache.SetValue("key" + num, "value" + num);
				});
			}
			for (var i = 0; i < count; ++i)
			{
				threads[i].Start();
			}
			for (var i = 0; i < count; ++i)
			{
				threads[i].Join();
			}
			for (var i = 0; i < count; ++i)
			{
				string value;
				Assert(cache.TryGetValue("key" + i, out value));
				AssertEquals("value" + i, value);
			}
		}

		public void TestSynchronization_NoExceptions()
		{
			var count = 10;
			var rounds = 100;
			var cache = new SynchronizedMRUCache<string, string>(count);
			var threads = new Thread[count];
			for (var i = 0; i < count; ++i)
			{
				threads[i] = new Thread(() =>
				{
					var rng = new Random();
					for (var j = 0; j < rounds; ++j)
					{
						var rng_result = rng.Next(4);
						switch (rng_result)
						{
							case 0:
								cache.ClearCache();
								break;
							case 1:
								cache.ContainsKey("key" + rng.Next(count));
								break;
							case 2:
								cache.SetValue("key" + rng.Next(count), "value" + rng.Next(count));
								break;
							case 3:
								string value;
								cache.TryGetValue("key" + rng.Next(count), out value);
								break;
						}
					}
				});
			}
			for (var i = 0; i < count; ++i)
			{
				threads[i].Start();
			}
			for (var i = 0; i < count; ++i)
			{
				threads[i].Join();
			}

			//make sure cache is still in some kind of functioning state after being hammered
			cache.SetValue("key0", "value0");
			string value2;
			Assert(cache.TryGetValue("key0", out value2));
			AssertEquals("value0", value2);
			cache.ClearCache();
			Assert(!cache.ContainsKey("key0"));
		}

		public void TestClearCache()
		{
			var cache = new MRUCache<string, string>(10);
			cache.SetValue("key", "Value");
			cache.SetValue("key2", "Value2");
			string value;
			AssertEquals(true, cache.TryGetValue("key", out value));
			AssertEquals(true, cache.TryGetValue("key2", out value));
			cache.ClearCache();
			AssertEquals(false, cache.TryGetValue("key", out value));
			AssertEquals(false, cache.TryGetValue("key2", out value));
		}

		public void TestMaximumElements()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(10);
			AssertEquals(10, cache.MaximumElements);
		}

		public void TestTryGetValueForKnownKey()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(10);
			cache.SetValue("key", "Value");
			string value;
			AssertEquals(true, cache.TryGetValue("key", out value));
			AssertEquals("Value", value);
		}

		public void TestTryGetValueForUnknownKey()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(10);
			string value;
			AssertEquals(false, cache.TryGetValue("Hello", out value));
		}

		public void TestItemIsPushedOutOfCache()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(2);
			cache.SetValue("key", "Value");
			cache.SetValue("key2", "Value2");
			cache.SetValue("key3", "Value3");
			string value;
			AssertEquals(false, cache.TryGetValue("key", out value));
			AssertEquals(true, cache.TryGetValue("key2", out value));
			AssertEquals(true, cache.TryGetValue("key3", out value));
		}

		public void TestCacheValueIsUpdated()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(2);
			cache.SetValue("key", "Value");
			cache.SetValue("key", "Value2");
			string value;
			AssertEquals(true, cache.TryGetValue("key", out value));
			AssertEquals("Value2", value);
		}

		public void TestContainsKey()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(1);
			AssertEquals(false, cache.ContainsKey("X"));
			cache.SetValue("X", "Hello");
			AssertEquals(true, cache.ContainsKey("X"));
		}

		public void TestReusingKeySaveItFromExpiry()
		{
			MRUCache<string, string> cache = new MRUCache<string, string>(4);
			cache.SetValue("key", "Value");
			cache.SetValue("key2", "Value2");
			cache.SetValue("key3", "Value3");
			string value;
			cache.TryGetValue("key", out value);
			cache.SetValue("key4", "Value3");
			cache.SetValue("key5", "Value3");

			AssertEquals(true, cache.ContainsKey("key"));
		}

		public void TestQueueingSameItemMultipleTimes()
		{
			MRUCache<int, int> cache = new MRUCache<int, int>(3);
			cache.SetValue(1, 1);
			cache.SetValue(2, 2);
			cache.SetValue(1, 1);
			cache.SetValue(3, 3);
			cache.SetValue(1, 1);
			cache.SetValue(4, 4);

			int value;
			Assert(!cache.TryGetValue(2, out value));
			Assert(cache.TryGetValue(3, out value));
			Assert(cache.TryGetValue(4, out value));
			Assert(cache.TryGetValue(1, out value));
		}

		public void TestSetValueIsConstantTimeOperation()
		{
			long firstElapsedTicks = -1;
			var random = new Random();
			for (int size = 100; size < 100000; size *= 10)
			{
				var cache = new MRUCache<int, int>(size);
				cache.SetValue(0, 0);
				var stopwatch = Stopwatch.StartNew();
				for (int i = 0; i < 10000000; i++)
				{
					cache.SetValue(random.Next(10000000), random.Next(10000000));
				}
				stopwatch.Stop();
				if (firstElapsedTicks == -1)
				{
					firstElapsedTicks = stopwatch.ElapsedTicks;
				}
				else
				{
					Assert(string.Format("cache.SetValue() should be a constant time operation, but the time to SetValue on a cache with {0} items was {1}, more than twice the time to SetValue on a cache with 100 items, {2}", size, stopwatch.ElapsedTicks, firstElapsedTicks), stopwatch.ElapsedTicks < firstElapsedTicks * 2);
				}
			}
		}
	}
}
