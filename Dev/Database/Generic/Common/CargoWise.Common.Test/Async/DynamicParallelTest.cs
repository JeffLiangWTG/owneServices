using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.Common.Async.Testing
{
	internal class DynamicParallelTest : TestCase
	{
		public void TestInvalidMaxThreads()
		{
			var collection = new[] { 1, 2, 3, 4 };

			AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
			{
				DynamicParallel<int>.Invoke(collection, (_) => { }, TimeSpan.FromSeconds(1), 0, CancellationToken.None);
			});
		}

		public void TestInvalidDelay()
		{
			var collection = new[] { 1, 2, 3, 4 };

			AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
			{
				DynamicParallel<int>.Invoke(collection, (_) => { }, TimeSpan.Zero, 0, CancellationToken.None);
			});
		}

		public void TestInvalidCollection()
		{
			AssertExceptionThrown<ArgumentException>(() =>
			{
				DynamicParallel<int>.Invoke(new List<int>(), (_) => { }, TimeSpan.FromSeconds(1), 2, CancellationToken.None);
			});
		}

		public void TestProcessesItems_Linear_Int()
		{
			var collection = new [] { 1, 2, 3, 4 };
			AssertContainsExactElementsInAnyOrder(collection, ProcessItems(collection, TimeSpan.FromSeconds(1), () => { } ));
		}

		public void TestProcessesItems_Linear_Int_Big()
		{
			var collection = new List<int>();
			for (int i = 0; i < 1000; i++)
			{
				collection.Add(i);
			}
			AssertContainsExactElementsInAnyOrder(collection, ProcessItems(collection, TimeSpan.FromSeconds(1), () => { } ));
		}

		public void TestProcessItems_Random_StartOnMainThread()
		{
			//Arrange
			var collection = new List<int>();
			var rand = new Random();
			for (int i = 0; i < 500; i++)
			{
				collection.Add(rand.Next(200));
			}

			//Act
			var processed = ProcessItems(collection, TimeSpan.FromSeconds(1), () => { });

			//Assert
			AssertContainsExactElementsInAnyOrder(collection, processed);
		}

		public void TestProcessesItems_Linear_String()
		{
			var collection = new[] { "hello", "world" };
			AssertContainsExactElementsInAnyOrder(collection, ProcessItems(collection, TimeSpan.FromSeconds(1), () => { }));
		}

		public void TestProcesses_LessThanMaxThreads_Int_LargeNotMainThread() => AssertProcesses_LessThanMaxThreads_Int_Large(false);
		public void TestProcesses_LessThanMaxThreads_Int_LargeMainThread() => AssertProcesses_LessThanMaxThreads_Int_Large(true);
		void AssertProcesses_LessThanMaxThreads_Int_Large(bool startOnMainThread)
		{
			var collection = new List<int>();
			for (int i = 0; i < 100; i++)
			{
				collection.Add(i);
			}
			AssertDoesNotExceedMaxThreads(collection, 2, 1, 10, startOnMainThread);
		}

		public void TestProcesses_LessThanMaxThreads_Int_SmallMainThread() => AssertProcesses_LessThanMaxThreads_Int_Small(true);
		public void TestProcesses_LessThanMaxThreads_Int_SmallNotMainThread() => AssertProcesses_LessThanMaxThreads_Int_Small(false);
		void AssertProcesses_LessThanMaxThreads_Int_Small(bool startOnMainThread)
		{
			var collection = new List<int>
			{
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9,
				10,
			};

			AssertDoesNotExceedMaxThreads(collection, 2, 1, 4, startOnMainThread);
		}

		public void TestProcesses_LessThanMaxThreads_StringMainThread() => AssertProcesses_LessThanMaxThreads_String(true);
		public void TestProcesses_LessThanMaxThreads_StringNotMainThread() => AssertProcesses_LessThanMaxThreads_String(false);
		void AssertProcesses_LessThanMaxThreads_String(bool startOnMainThread)
		{
			var collection = new[] { "hello", "world", "tim" };
			AssertDoesNotExceedMaxThreads(collection, 1, 1, 1, startOnMainThread);
		}

		public void AssertDoesNotExceedMaxThreads<T>(IEnumerable<T> collection, int threadDelay, int threadInterval, int maxThreads, bool startOnMainThread)
		{
			//Arrange
			ConcurrentDictionary<int, byte> registeredTasks = new ();
			var action = () =>
			{
				registeredTasks.TryAdd(Task.CurrentId ?? Environment.CurrentManagedThreadId, new byte());

				Task.Delay(TimeSpan.FromSeconds(threadDelay)).Wait();
			};

			//Act
			var processedItems = ProcessItems(collection, TimeSpan.FromSeconds(threadInterval), addAction: action, maxThreadCount: maxThreads);

			//Assert
			CombineAssertions(() =>
			{
				Assert($"Should create less tasks than or equal to max threads\ncreated: {registeredTasks.Keys.Count}\nmax: {maxThreads}", registeredTasks.Keys.Count <= maxThreads);
				AssertContainsExactElementsInAnyOrder("Should process all items in collection", collection, processedItems);
			});
		}

		public void TestCancelsExecutingTasks_Int()
		{
			var collection = new List<int>();
			for (int i = 0; i < 100; i++)
			{
				collection.Add(i);
			}
			AssertCancelsExecutingTask(collection);
		}

		public void TestCancelsExecutingTasks_String()
		{
			var collection = new[] { "hello", "world", "tim", "bob", "jeremy" };
			AssertCancelsExecutingTask(collection);
		}

		void AssertCancelsExecutingTask<T>(IEnumerable<T> collection)
		{
			//Arrange
			using var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(3000);

			ConcurrentDictionary<int, byte> registeredThreads = new ();
			var action = (T a) =>
			{
				registeredThreads.TryAdd(Environment.CurrentManagedThreadId, new byte());
				Thread.Sleep(TimeSpan.FromSeconds(1));
			};

			var taskType = typeof(Task);
			var currentActiveTasksField = taskType.GetField("s_currentActiveTasks", BindingFlags.NonPublic | BindingFlags.Static);

			//Act
			AssertExceptionThrown<OperationCanceledException>(() => { DynamicParallel<T>.Invoke(collection, action, TimeSpan.FromSeconds(1), collection.Count(), tokenSource.Token); });

			//Assert : check tasks are cleaned up
			var activeTasksDictionary = (Dictionary<int, Task>)currentActiveTasksField.GetValue(null);
			var activeTasks = activeTasksDictionary?.Keys.ToHashSet() ?? new HashSet<int>();
			Assert("No created tasks should be running after cancelled", !registeredThreads.Keys.ToHashSet().IsSubsetOf(activeTasks));
		}

		public void Test_Processing_Catches_One_Int()
		{
			var collection = new List<int> { 1, 2, 3, 4 };
			AssertThreadCatchesExceptions(
				collection,
				new (Predicate<int>, Exception)[]
				{
					(i => i == 3, new InvalidOperationException())
				},
				Array.Empty<(Type, int)>(), CancellationToken.None);
		}

		public void Test_Processing_Catches_Many_Int()
		{
			var collection = new List<int>();
			for (int i = 0; i < 30; i++)
			{
				collection.Add(i);
			}
			AssertThreadCatchesExceptions(
				collection,
				new (Predicate<int>, Exception)[]
				{
					(x => x == 3, new InvalidOperationException()),
					(x => x == 6, new InvalidOperationException()),
					(x => x == 7, new ArgumentException()),
				},
				Array.Empty<(Type, int)>(),
				CancellationToken.None);
		}

		public void Test_Processing_Catches_Many_String()
		{
			var collection = new List<string>
			{
				"Hello",
				"World",
				"!!",
				"C",
				"OST",
				"Australia"
			};

			AssertThreadCatchesExceptions(
				collection,
				new (Predicate<string>, Exception)[]
				{
					(i => i == "OST", new InvalidOperationException()),
					(i => i == "!!", new ApplicationException()),
				},
				Array.Empty<(Type, int)>(),
				CancellationToken.None,
				(a) => Thread.Sleep(3000));
		}

		public void Test_Handles_CancelAndException()
		{
			using var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(1000);

			var collection = new List<int> { 1, 2 };

			AssertThreadCatchesExceptions(
				collection,
				new (Predicate<int>, Exception)[]
				{
					(i => i == 2, new InvalidOperationException())
				},
				Array.Empty<(Type, int)>(),
				CancellationToken.None);
		}

		public void Test_CancelsSubtask_ContainsException_SameToken()
		{
			using var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(3000);

			var collection = new List<int> { 1, 2, 3 };
			AssertThreadCatchesExceptions(
				collection,
				Array.Empty<(Predicate<int> predicate, Exception exception)>(),
				new[]
				{
					(typeof(OperationCanceledException), 4)
				},
				tokenSource.Token,
				(_) =>
				{
					tokenSource.Token.WaitHandle.WaitOne();
					tokenSource.Token.ThrowIfCancellationRequested();
				});
		}

		public void Test_CancelsSubtask_ContainsException_SameToken_WithExtraException()
		{
			using var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(3000);

			var collection = new List<int> { 1, 2, 3 };
			AssertThreadCatchesExceptions(
				collection,
				new (Predicate<int> predicate, Exception exception)[]
				{
					(x => x == 2, new ApplicationException())
				},
				new[]
				{
					(typeof(OperationCanceledException), 3)
				},
				tokenSource.Token,
				(_) =>
				{
					tokenSource.Token.WaitHandle.WaitOne();
					tokenSource.Token.ThrowIfCancellationRequested();
				});
		}

		public void Test_CancelsSubtask_ContainsException_DifferentToken()
		{
			using var tokenSource = new CancellationTokenSource();
			tokenSource.CancelAfter(3000);

			var collection = new List<int> { 1, 2, 3 };
			AssertThreadCatchesExceptions(
				collection,
				Array.Empty<(Predicate<int> predicate, Exception exception)>(),
				new[]
				{
					(typeof(OperationCanceledException), 3)
				},
				CancellationToken.None,
				(_) =>
				{
					tokenSource.Token.WaitHandle.WaitOne();
					tokenSource.Token.ThrowIfCancellationRequested();
				});
		}

		void AssertThreadCatchesExceptions<T>(IEnumerable<T> collection, (Predicate<T> predicate, Exception exception)[] exceptionConditionals, (Type type, int count)[] addExceptions, CancellationToken cancellationToken, Action<T> addAction = null)
		{
			//Arrange
			ConcurrentDictionary<int, byte> registeredThreads = new ();
			var action = (T a) =>
			{
				registeredThreads.TryAdd(Environment.CurrentManagedThreadId, new byte());
				foreach (var condition in exceptionConditionals)
				{
					if (condition.predicate(a))
					{
						throw condition.exception;
					}
				}

				if (addAction != null)
				{
					addAction.Invoke(a);
				}
				else
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
				}
			};

			var taskType = typeof(Task);
			var currentActiveTasksField = taskType.GetField("s_currentActiveTasks", BindingFlags.NonPublic | BindingFlags.Static);

			try
			{
				//Act
				DynamicParallel<T>.Invoke(collection, action, TimeSpan.FromSeconds(1), collection.Count(), cancellationToken);

				//Assert
				Assert("Should have thrown aggregate exception", false);
			}
			catch (AggregateException ex)
			{
				CombineAssertions(() =>
				{
					foreach (var exception in exceptionConditionals.Select(x => x.exception))
					{
						Assert($"Aggregate exception should contain expected {exception}", ex.InnerExceptions.Contains(exception));
					}

					var exceptionCountGroups = ex.InnerExceptions.Select(x => x.GetType()).GroupBy(x => x)
						.ToDictionary(x => x.Key, x => x.Count());
					foreach (var addException in addExceptions)
					{
						Assert($"Aggregate exception should contain type {addException.type}", exceptionCountGroups.Keys.Contains(addException.type));
						AssertEquals($"Should contain exception {addException.type} with count {addException.count}\nWas{exceptionCountGroups[addException.type]}", addException.count, exceptionCountGroups[addException.type]);
					}

					//Assert threads cleaned up
					var activeTasksDictionary = (Dictionary<int, Task>)currentActiveTasksField.GetValue(null);
					var activeTasks = activeTasksDictionary?.Keys.ToHashSet() ?? new HashSet<int>();
					Assert("No created tasks should be running after cancelled", !registeredThreads.Keys.ToHashSet().IsSubsetOf(activeTasks));
				});
			}
		}

		IEnumerable<T> ProcessItems<T>(IEnumerable<T> inputCollection, TimeSpan interval, Action addAction, int maxThreadCount = 1)
		{
			var result = new ConcurrentBag<T>();

			void ProcessingAction(T a)
			{
				addAction.Invoke();
				result.Add(a);
			}

			DynamicParallel<T>.Invoke(inputCollection, ProcessingAction, interval, maxThreadCount, CancellationToken.None);
			return result;
		}

		public void Test_StartsOnMainThread()
		{
			//Arrange
			var collection = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			ConcurrentQueue<int> registeredThreads = new ();

			var action = (int a) =>
			{
				registeredThreads.Enqueue(Environment.CurrentManagedThreadId);

				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
			};

			//Act
			DynamicParallel<int>.Invoke(collection, action, TimeSpan.FromSeconds(1), 10, CancellationToken.None);

			//Assert
			Assert(registeredThreads.TryDequeue(out var firstThread));
			AssertEquals("Should start on main thread", Environment.CurrentManagedThreadId,firstThread);
		}

		public void Test_ExpectedThreads_Deployed()
		{
			//Arrange
			const int maxThreadCount = 10;
			const int testDelay = 5;
			const int testInterval = 1;
			var seenThreads = new ConcurrentDictionary<int, byte>();
			var collection = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			var processedCount = 0;

			int mainThreadId = Thread.CurrentThread.ManagedThreadId;
			void ProcessingAction(int a)
			{
				int tid = Task.CurrentId ?? mainThreadId;

				seenThreads.TryAdd(tid, 0);

				var delaySec = a == 1 ? testDelay : testInterval;
				Task.Delay(TimeSpan.FromSeconds(delaySec)).Wait();

				Interlocked.Increment(ref processedCount);
			}

			//Act
			DynamicParallel<int>.Invoke(collection, ProcessingAction, TimeSpan.FromSeconds(testInterval), maxThreadCount, CancellationToken.None);

			//Assert
			CombineAssertions(() =>
			{
				Assert("Should have less tasks than maxThreadCount", seenThreads.Count < maxThreadCount);
				Assert($"Should have deployed at least Delay:{testDelay}-1 tasks, Actual: {seenThreads.Count}", seenThreads.Count >= testDelay - 1);
				Assert("Should have processed each item in the collection", processedCount == collection.Count);
			});
		}
	}
}
