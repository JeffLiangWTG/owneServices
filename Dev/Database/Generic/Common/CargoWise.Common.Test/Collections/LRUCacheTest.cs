using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using CargoWise.Common.MemoryManagement;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class LRUCacheTest : TestCase
	{
		public void TestReclaim()
		{
			StrongReferenceCount = 10;
			object[] objects = new[] { "LeastRecentlyUsedItem", "Original", "Modified" };
			Cache.Add(1, objects[0]);
			Cache.Add(2, objects[1]);
			Cache.Add(3, objects[2]);
			AssertEquals(0, Cache.WeakCacheCount);
			AssertEquals(3, Cache.StrongCacheCount);
			AssertEquals(FlushResult.Partial, Cache.ReclaimMemory(FlushAction.Partial));
			AssertEquals(1, Cache.WeakCacheCount);
			AssertEquals(2, Cache.StrongCacheCount);
			AssertEquals(FlushResult.Partial, Cache.ReclaimMemory(FlushAction.Partial));
			AssertEquals(0, Cache.WeakCacheCount);
			AssertEquals(2, Cache.StrongCacheCount);
			AssertEquals(FlushResult.Partial, Cache.ReclaimMemory(FlushAction.Partial));
			AssertEquals(1, Cache.WeakCacheCount);
			AssertEquals(1, Cache.StrongCacheCount);
			AssertEquals(FlushResult.Partial, Cache.ReclaimMemory(FlushAction.Partial));
			AssertEquals(0, Cache.WeakCacheCount);
			AssertEquals(1, Cache.StrongCacheCount);
			AssertEquals(FlushResult.Partial, Cache.ReclaimMemory(FlushAction.Partial));
			AssertEquals(1, Cache.WeakCacheCount);
			AssertEquals(0, Cache.StrongCacheCount);
			AssertEquals(FlushResult.Exhausted, Cache.ReclaimMemory(FlushAction.Partial));
			AssertEquals(0, Cache.StrongCacheCount);
			AssertEquals(0, Cache.WeakCacheCount);
		}

		public void TestAddingDuplicateKeyDifferentValue()
		{
			StrongReferenceCount = 10;
			Cache.Add(2, "LeastRecentlyUsedItem");
			Cache.Add(1, "Original");
			Cache.Add(1, "Modified");
			AssertEquals("LeastRecentlyUsedItem", Cache[2]);
			AssertEquals("Modified", Cache[1]);
		}

		public void TestGetValue()
		{
			var valueRef1 = AddUnreferencedObject(1);
			var valueRef2 = AddUnreferencedObject(2);
			var valueRef3 = AddUnreferencedObject(3);
			object value1WasWeakNowMadeStrong = Cache[1];
			GC.Collect();
			AssertEquals("Value1 strongly referenced", true, valueRef1.IsAlive);
			AssertEquals("Value2 weak referenced", false, valueRef2.IsAlive);
			AssertEquals("Value3 strongly referenced", true, valueRef3.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference AddUnreferencedObject(int i)
		{
			var obj = new object();
			Cache.Add(i, obj);
			return new WeakReference(obj);
		}

		[MemoryTestRetryCount(2)]
		[ExpectNoExceptions]
		public void TestThreadSafety()
		{
			StrongReferenceCount = 50;
			for (int i = 0; i < 100; i++)
			{
				DoOperationOnThread();
			}

			foreach (Thread thead in activeThreads)
			{
				thead.Join();
			}

			if (lastExceptionOnThread != null)
			{
				throw new Exception(lastExceptionOnThreadStack, lastExceptionOnThread);
			}
		}

		#region Implementation
		int currentKey;
		readonly List<Thread> activeThreads = new List<Thread>();
		string lastExceptionOnThreadStack;
		Exception lastExceptionOnThread;
		int StrongReferenceCount = 2;
		LRUCache<long, object> Cache
		{
			get
			{
				return cache ?? (cache = new LRUCache<long, object>(StrongReferenceCount));
			}
		}

		LRUCache<long, object> cache;
		void DoOperationOnThread()
		{
			Thread thread = new Thread(new ThreadStart(delegate
			{
				try
				{
					for (int i = 0; i < 100; i++)
					{
						Interlocked.Increment(ref currentKey);
						int keyAndValue = currentKey;
						Cache.Add(keyAndValue, keyAndValue);
					}

					for (int i = 0; i <= currentKey; i++)
					{
						object value = Cache[i];
						if (value != null && i != (int)value)
						{
							throw new InvalidOperationException("Value should be correct or null");
						}
					}
				}
				catch (Exception ex)
				{
					if (lastExceptionOnThread == null)
					{
						lastExceptionOnThreadStack = ex.ToString();
						lastExceptionOnThread = ex;
					}
				}
			}));
			thread.Start();
		}
		#endregion
	}
}