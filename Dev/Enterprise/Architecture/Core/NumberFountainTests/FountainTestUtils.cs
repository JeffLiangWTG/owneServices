using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.NumberFountain.Internal;
using NUnit.Framework;

namespace Enterprise.NumberFountain.Testing
{
	public static class FountainTestUtils
	{
		public static IFountainContext GetFountainContext(this IDbConnected db)
		{
			IDbConnectionInternals connection = db.Connection;
			return db.Connection.IsInTransaction ? new FountainContext(connection.InternalDbTransaction) : new FountainContext(connection.InternalDbConnection);
		}

		public static IReadOnlyList<long> AssertStrictlyOrdered(this IReadOnlyList<long> values, int expectedCount)
		{
			if (expectedCount != values.Count)
			{
				throw new AssertionFailedError(FormattableString.Invariant($"Incorrect number of values.\r\nExpected: {expectedCount}\r\nActual: {values.Count}"));
			}

			if (values.Count == 0)
			{
				return values;
			}

			long prevValue = values[0];
			foreach (long value in values.Skip(1))
			{
				if (value <= prevValue)
				{
					throw new AssertionFailedError(FormattableString.Invariant($"Array is not strictly ordered ({prevValue} before {value}).\r\nValues: {string.Join(", ", values)}"));
				}

				prevValue = value;
			}

			return values;
		}

		/// <summary>
		/// Creates the given number of threads to execute multiple tasks in parallel.
		/// Main methods of all tasks will start at the same time, after initialization for all tasks is completed.
		/// </summary>
		/// <typeparam name="TResult">Type of result returned by each task.</typeparam>
		/// <param name="threadCount">Number of threads to use.</param>
		/// <param name="createTestTask">Function that can create a task for the thread.</param>
		/// <returns>Collection of results returned by tasks.</returns>
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Results are read when all other threads are stopped. Thread.Join should make sure that all changes made by other threads are visible to the main thread.")]
		public static IReadOnlyList<TResult> TestConcurrency<TResult>(int threadCount, Func<IConcurrencyTestTask<TResult>> createTestTask)
		{
			var threads = new List<Thread>();
			long readyThreads = 0;
			var results = new ConcurrentBag<TResult>();
			var errors = new ConcurrentBag<Exception>();

			using (var startSignal = new ManualResetEvent(false))
			{
				for (int i = 0; i < threadCount; i++)
				{
					var task = createTestTask();
					var executor = new ConcurrencyTestTaskExecutor<TResult>
					(
						task: task,
						threadNum: i,
						reportReady: () => Interlocked.Increment(ref readyThreads),
						startSignal: startSignal,
						reportResult: r => results.Add(r),
						reportException: ex => errors.Add(ex)
					);

					var thread = new Thread(executor.Run);
					threads.Add(thread);

					thread.Name = nameof(TestConcurrency);
					thread.IsBackground = true;
					thread.Start();
				}

				var initTime = Stopwatch.StartNew();
				const int maxInitTime = 10000;
				while (Interlocked.Read(ref readyThreads) < threadCount)
				{
					if (initTime.ElapsedMilliseconds > maxInitTime)
					{
						throw new AssertionFailedError(FormattableString.Invariant($"Threads failed to start in {maxInitTime} ms."));
					}

					Thread.Sleep(100);
				}

				startSignal.Set();

				foreach (var thread in threads)
				{
					thread.Join();
				}
			}

			if (errors.Count > 0)
			{
				throw new AggregateException("Error in one or more threads.", errors);
			}

			if (results.Count != threadCount)
			{
				throw new AssertionFailedError(FormattableString.Invariant($"Not all threads returned a result.\r\nThread Count: {threadCount}\r\nResult Count: {results.Count}"));
			}

			return results.ToList();
		}

		class ConcurrencyTestTaskExecutor<TResult>
		{
			readonly IConcurrencyTestTask<TResult> task;
			readonly int threadNum;
			readonly Action reportReady;
			readonly ManualResetEvent startSignal;
			readonly Action<TResult> reportResult;
			readonly Action<Exception> reportException;

			public ConcurrencyTestTaskExecutor(IConcurrencyTestTask<TResult> task, int threadNum, Action reportReady, ManualResetEvent startSignal, Action<TResult> reportResult, Action<Exception> reportException)
			{
				this.task = task;
				this.threadNum = threadNum;
				this.reportReady = reportReady;
				this.startSignal = startSignal;
				this.reportResult = reportResult;
				this.reportException = reportException;
			}

			public void Run()
			{
				bool reportedReady = false;
				try
				{
					task.Init();

					reportReady();
					startSignal.WaitOne();

					var result = task.Run(threadNum);
					reportResult(result);
				}
				catch (Exception ex)
				{
					if (!reportedReady)
					{
						reportReady();
					}

					reportException(ex);
				}
				finally
				{
					try
					{
						task.Cleanup();
					}
					catch (Exception ex)
					{
						reportException(ex);
					}
				}
			}
		}
	}

	public class FountainTestUtilsTest : TestCase
	{
		public void TestAssertOrdered()
		{
			FountainTestUtils.AssertStrictlyOrdered(Array.Empty<long>(), 0);
			FountainTestUtils.AssertStrictlyOrdered(new long[] { 1 }, 1);
			FountainTestUtils.AssertStrictlyOrdered(new long[] { 1, 2 }, 2);
			FountainTestUtils.AssertStrictlyOrdered(new long[] { 10, 20, 30 }, 3);

			AssertEquals(
				"Incorrect number of values.\r\nExpected: 2\r\nActual: 1",
				AssertExceptionThrown<AssertionFailedError>(() => FountainTestUtils.AssertStrictlyOrdered(new long[] { 1 }, 2)).Message
			);
			AssertEquals(
				"Array is not strictly ordered (1 before 1).\r\nValues: 1, 1",
				AssertExceptionThrown<AssertionFailedError>(() => FountainTestUtils.AssertStrictlyOrdered(new long[] { 1, 1 }, 2)).Message
			);
			AssertEquals(
				"Array is not strictly ordered (2 before 1).\r\nValues: 2, 1",
				AssertExceptionThrown<AssertionFailedError>(() => FountainTestUtils.AssertStrictlyOrdered(new long[] { 2, 1 }, 2)).Message
			);
			AssertEquals(
				"Array is not strictly ordered (2 before 2).\r\nValues: 1, 2, 2, 3",
				AssertExceptionThrown<AssertionFailedError>(() => FountainTestUtils.AssertStrictlyOrdered(new long[] { 1, 2, 2, 3 }, 4)).Message
			);
		}

		public void TestTestConcurrency()
		{
			int counter = 0;
			int cleanupCalls = 0;
			var results = FountainTestUtils.TestConcurrency(5, () => new GenericTestTask<int>(
				() => { },
				() => Interlocked.Increment(ref counter),
				() => Interlocked.Increment(ref cleanupCalls)
			));

			AssertContainsExactElementsInAnyOrder(new int[] { 1, 2, 3, 4, 5 }, results);
			AssertEquals(5, cleanupCalls);
		}

		public void TestTestConcurrency_RunAfterInit()
		{
			object monitor = new object();
			var actions = new List<string>();
			int delay = 0;

			var results = FountainTestUtils.TestConcurrency(5, () => new GenericTestTask<int>(
				() =>
				{
					Thread.Sleep(Interlocked.Increment(ref delay) * 50);
					lock (monitor)
					{
						actions.Add("init");
					}
				},
				() =>
				{
					lock (monitor)
					{
						actions.Add("run");
					}

					return 0;
				},
				() => { }
			));

			AssertEquals(
				"calls to 'run' should only be made when all calls to 'init' are finished",
				string.Join(", ", new string[] { "init", "init", "init", "init", "init", "run", "run", "run", "run", "run" }),
				string.Join(", ", actions)
			);
		}

		public void TestTestConcurrency_ExceptionInInit()
		{
			int cleanupCalls = 0;
			var aggregateException = AssertExceptionThrown<AggregateException>(() => FountainTestUtils.TestConcurrency(5, () => new GenericTestTask<int>(
				() => throw new Exception("Test exception."),
				() => 0,
				() => Interlocked.Increment(ref cleanupCalls)
			)));
			AssertEquals(5, aggregateException.InnerExceptions.Count);
			AssertEquals(true, aggregateException.InnerExceptions.All(e => e.Message == "Test exception.")); // Exception is thrown right in this method with this specific message.
			AssertEquals(5, cleanupCalls);
		}

		public void TestTestConcurrency_ExceptionInRun()
		{
			int cleanupCalls = 0;
			var aggregateException = AssertExceptionThrown<AggregateException>(() => FountainTestUtils.TestConcurrency(5, () => new GenericTestTask<int>(
				() => { },
				() => throw new Exception("Test exception."),
				() => Interlocked.Increment(ref cleanupCalls)
			)));
			AssertEquals(5, aggregateException.InnerExceptions.Count);
			AssertEquals(true, aggregateException.InnerExceptions.All(e => e.Message == "Test exception.")); // Exception is thrown right in this method with this specific message.
			AssertEquals(5, cleanupCalls);
		}

		public void TestTestConcurrency_ExceptionInCleanup()
		{
			int cleanupCalls = 0;
			var aggregateException = AssertExceptionThrown<AggregateException>(() => FountainTestUtils.TestConcurrency(5, () => new GenericTestTask<int>(
				() => { },
				() => 0,
				() =>
				{
					Interlocked.Increment(ref cleanupCalls);
					throw new Exception("Test exception.");
				}
			)));
			AssertEquals(5, aggregateException.InnerExceptions.Count);
			AssertEquals(true, aggregateException.InnerExceptions.All(e => e.Message == "Test exception.")); // Exception is thrown right in this method with this specific message.
			AssertEquals(5, cleanupCalls);
		}

		public void TestTestConcurrency_ExceptionInRunAndCleanup()
		{
			int cleanupCalls = 0;
			var aggregateException = AssertExceptionThrown<AggregateException>(() => FountainTestUtils.TestConcurrency(5, () => new GenericTestTask<int>(
				() => { },
				() => throw new Exception("Test exception."),
				() =>
				{
					Interlocked.Increment(ref cleanupCalls);
					throw new Exception("Test exception.");
				}
			)));
			AssertEquals(10, aggregateException.InnerExceptions.Count);
			AssertEquals(true, aggregateException.InnerExceptions.All(e => e.Message == "Test exception.")); // Exception is thrown right in this method with this specific message.
			AssertEquals(5, cleanupCalls);
		}

		class GenericTestTask<TResult> : IConcurrencyTestTask<TResult>
		{
			readonly Action init;
			readonly Func<TResult> run;
			readonly Action cleanup;

			public GenericTestTask(Action init, Func<TResult> run, Action cleanup)
			{
				this.init = init;
				this.run = run;
				this.cleanup = cleanup;
			}

			public void Init()
			{
				init();
			}

			public TResult Run(int threadNum)
			{
				return run();
			}

			public void Cleanup()
			{
				cleanup();
			}
		}
	}
}
