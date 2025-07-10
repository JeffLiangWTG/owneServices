
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using CargoWise.Data.Testing;
using Enterprise.Semaphores.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common
{
	class ConcurrentSemaphoreTest : TestCase
	{
		class SemaphoreProviderWithoutRetryForDeadlocks : SemaphoreProviderForTesting
		{
			public SemaphoreProviderWithoutRetryForDeadlocks() : base(0)
			{
			}
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestDeadlockStress()
		{
			// Arrange
			using (var provider = new SemaphoreProviderWithoutRetryForDeadlocks())
			using (var startSignal = new ManualResetEvent(false))
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var semaphoreProvider = (ISemaphoreProvider)provider;
				var exceptions = new ConcurrentBag<Exception>();
				var count = 0;
				const int threadCount = 1000;
				const int loopCount = 50;

				var threads = Enumerable.Range(0, threadCount)
					.Select(i => new Thread(() =>
					{
						using (CargoWise.Data.Db.DisposableActionForDbConnection())
						{
							startSignal.WaitOne();
							for (var j = 0; j < loopCount; j++)
							{
								if (cancellationTokenSource.IsCancellationRequested)
								{
									break;
								}

								ISemaphoreHandle handle = null;
								try
								{
									var semaphore = new SemaphoreLockType($"{i % 10:D3}");
									handle = semaphoreProvider.CreateSemaphoreHandle(semaphore);
									if (!handle.Success)
									{
										if (handle.CreateException is SemaphoreTransactionDeadlockException semaphoreTransactionDeadlockException)
										{
											exceptions.Add(semaphoreTransactionDeadlockException);
											cancellationTokenSource.Cancel();
										}

										continue;
									}

									Thread.Sleep(100);
								}
								catch (Exception e)
								{
									exceptions.Add(e);
									cancellationTokenSource.Cancel();
								}
								finally
								{
									handle?.Dispose();
									Interlocked.Increment(ref count);
								}
							}
						}
					}))
					.ToList();
				threads.ForEach(thread => thread.Start());
				startSignal.Set();

				// Act
				threads.ForEach(thread => thread.Join());

				// Assert
				AssertEquals(0, exceptions.Count);
				AssertEquals(threadCount * loopCount, count);
			}
		}

		class SemaphoreLockType : ISemaphoreType
		{
			public SemaphoreLockType() : this(string.Empty)
			{
			}

			public SemaphoreLockType(string code)
			{
				LockInfo = FormattableString.Invariant($"Lock:{code}");
			}

			public string Category { get; } = "TST";
			public string LockInfo { get; }
			public int MaxConcurrentHandles { get; } = 1;
		}
	}
}
