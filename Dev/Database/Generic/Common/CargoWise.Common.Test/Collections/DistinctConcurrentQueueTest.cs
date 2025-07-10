using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	public class DistinctConcurrentQueueTest : TestCase
	{
		public void TestMultiThreadedSafety()
		{
			var distinctQueue = new DistinctConcurrentQueue<int>();
			var publishers = new List<Thread>();
			var subscribers = new List<Thread>();
			var exceptions = new ConcurrentBag<Exception>();
			for (int i = 0; i < 20; ++i)
			{
				var rnd = new Random(i);
				publishers.Add(new Thread(() =>
				{
					try
					{
						for (int j = 0; j < 1000000; ++j)
						{
							distinctQueue.TryEnqueue(rnd.Next(10));
						}
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}));
			}

			for (int i = 0; i < 20; ++i)
			{
				subscribers.Add(new Thread(() =>
				{
					try
					{
						for (int j = 0; j < 1000000; ++j)
						{
							distinctQueue.TryDequeue(out _);
						}
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}));
			}

			foreach (var subscriber in subscribers)
			{
				subscriber.Start();
			}

			foreach (var publisher in publishers)
			{
				publisher.Start();
			}

			foreach (var subscriber in subscribers)
			{
				subscriber.Join();
			}

			foreach (var publisher in publishers)
			{
				publisher.Join();
			}

			Assert(exceptions.IsEmpty);
		}

		public void TestQueueEnqueueABunch()
		{
			var queue = new DistinctConcurrentQueue<int>();
			for (var i = 0; i < 10000; i++)
			{
				Assert(queue.TryEnqueue(i));
				Assert(queue.TryDequeue(out var _));
			}

			AssertEquals(false, queue.TryDequeue(out var _));
		}

		public void TestLotsOfDequeue()
		{
			var queue = new DistinctConcurrentQueue<int>();
			for (var i = 0; i < 10000; i++)
			{
				Assert(queue.TryEnqueue(i));
			}

			var counter = 0;
			while (queue.TryDequeue(out var _))
			{
				counter++;
			}

			AssertEquals(10000, counter);
		}

		public void TestEnqueueDuplicates()
		{
			var queue = new DistinctConcurrentQueue<int>();
			AssertEquals(true, queue.TryEnqueue(1));
			AssertEquals(false, queue.TryEnqueue(1));
			AssertEquals(1, queue.Count);
			AssertEquals(true, queue.TryDequeue(out var r));
			AssertEquals(1, r);
			AssertEquals(false, queue.TryDequeue(out var _));
		}

		public void TestEnumerate()
		{
			var queue = new DistinctConcurrentQueue<int>();
			AssertEquals(true, queue.TryEnqueue(1));
			AssertEquals(true, queue.TryEnqueue(2));
			var count = 0;
			foreach (var item in queue)
			{
				count++;
			}

			AssertEquals(2, count);
		}
	}
}