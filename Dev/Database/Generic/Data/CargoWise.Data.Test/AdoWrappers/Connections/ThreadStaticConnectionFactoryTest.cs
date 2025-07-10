using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ThreadStaticConnectionFactoryTest : TestCase
	{
		static ThreadStaticConnectionFactory<DbConnection> MakeFactoryForTest()
		{
			return new ThreadStaticConnectionFactory<DbConnection>(new ConnectionFactoryAction<DbConnection>(() => Db.NewExtraConnectionToMainDb()));
		}

		public void TestUsingBeginThreadScopeFromAsyncTasksAfterAwait()
		{
			var factory = MakeFactoryForTest();

			// Arrange
			var task = Task.Run(() =>
			{
				using (factory.BeginThreadScope())
				{
					// Act
					{
						RunAsync().Wait();
						factory.ProvideConnection().ExecuteScalar(@"SELECT @@servername");

						async Task RunAsync()
						{
							await Task.Yield();
							using (factory.BeginThreadScope())
							{
								// Just because I have some legacy code here
							}
						}
					}
				}
			});
			task.Wait();

			// Assert
			AssertNull($"We should not receive [{ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction}]", ErrorReporter.LastExceptionReported);
		}

		public void TestUsingBeginThreadScopeFromAsyncTasksAfterAwaitInSynchronousCallOfAsyncTask()
		{
			var factory = MakeFactoryForTest();

			// Arrange
			var task = Task.Run(() =>
			{
				using (factory.BeginThreadScope())
				{
					// Act
					{
						RunAsync().Wait();
						factory.ProvideConnection().ExecuteScalar(@"SELECT @@servername");

						async Task RunAsync()
						{
							await Task.Yield();
							PseudoAsync().GetAwaiter().GetResult();
						}

						async Task PseudoAsync()
						{
							using (factory.BeginThreadScope())
							{
								await Task.Yield();
							}
						}
					}
				}
			});
			task.Wait();

			// Assert
			AssertNull($"We should not receive [{ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction}]", ErrorReporter.LastExceptionReported);
		}

		public void TestThread()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			var thread1 = new Thread(() =>
			{
				using (factory.BeginThreadScope())
				{
					PokeDbConnectionForFactory(factory);
				}
			});

			var thread2 = new Thread(() =>
			{
				using (factory.BeginThreadScope())
				{
					PokeDbConnectionForFactory(factory);
				}
			});

			thread1.Start();
			thread2.Start();

			thread1.Join();
			thread2.Join();

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestMultipleTasksOnTheSameThread()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;
			var connections = new ConcurrentStack<DbConnection>();
			var tasks = new List<Task>();

			using (factory.BeginThreadScope())
			{
				connections.Push(factory.ProvideConnection()); // this is the paremt thread's connection

				for (var i = 0; i < 5; i++)
				{
					tasks.Add(Task.Run(() =>
					{
						using (factory.BeginThreadScope())
						{
							connections.Push(factory.ProvideConnection());
						}
					}));
				}
				Task.WaitAll(tasks.ToArray());

				for (var i = 0; i < 6; i++)
				{
					connections.TryPop(out var connection);
					AssertCollectionNotContains(connection, connections); // compare that no connection is a duplicate.
				}
			}

			AssertEquals(0, connections.ToArray().Length);
			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestParallelForEach()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			void GetConnectionWrappedInDbDotDisposableAction()
			{
				using (factory.BeginThreadScope())
				{
					PokeDbConnectionForFactory(factory);
				}
			}

			Parallel.ForEach(Enumerable.Range(0, 5), _ => GetConnectionWrappedInDbDotDisposableAction());

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		#region Thread As Main Context

		public void TestThread_Thread()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			var thread1 = new Thread(() =>
			{
				using (factory.BeginThreadScope())
				{
					PokeDbConnectionForFactory(factory);

					var thread2 = new Thread(() =>
					{
						using (factory.BeginThreadScope())
						{
							PokeDbConnectionForFactory(factory);
						}
					});

					thread2.Start();
					thread2.Join();
				}
			});

			thread1.Start();
			thread1.Join();

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestThread_Task()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			using (var resetEvent = new AutoResetEvent(false))
			{
				var thread = new Thread(() =>
				{
					using (factory.BeginThreadScope())
					{
						PokeDbConnectionForFactory(factory);

						Task.Run(() =>
						{
							using (factory.BeginThreadScope())
							{
								PokeDbConnectionForFactory(factory);
							}

							resetEvent.Set();
						});

						PokeDbConnectionForFactory(factory);
					}
				});
				thread.Start();

				thread.Join(TimeSpan.FromSeconds(2));
				resetEvent.WaitOne();
			}

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestThread_TaskAwait()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			using (var resetEvent = new AutoResetEvent(false))
			{
				var thread = new Thread(() =>
				{
					using (factory.BeginThreadScope())
					{
						PokeDbConnectionForFactory(factory);

						Task.Run(async () =>
						{
							using (factory.BeginThreadScope())
							{
								PokeDbConnectionForFactory(factory);

								await Task.Run(() =>
								{
									using (factory.BeginThreadScope())
									{
										PokeDbConnectionForFactory(factory);
									}
								});

								PokeDbConnectionForFactory(factory);
							}

							resetEvent.Set();
						});

						PokeDbConnectionForFactory(factory);
					}
				});
				thread.Start();

				thread.Join(TimeSpan.FromSeconds(2));
				resetEvent.WaitOne();
			}

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		#endregion // Thread As Main Context

		#region Task As Main Context

		public void TestTask_Thread()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			var task = Task.Run(() =>
			{
				using (factory.BeginThreadScope())
				{
					PokeDbConnectionForFactory(factory);

					var thread1 = new Thread(() =>
					{
						using (factory.BeginThreadScope())
						{
							PokeDbConnectionForFactory(factory);
						}
					});

					var thread2 = new Thread(() =>
					{
						using (factory.BeginThreadScope())
						{
							PokeDbConnectionForFactory(factory);
						}
					});

					thread1.Start();
					thread2.Start();

					thread1.Join();
					thread2.Join();
				}
			});
			task.Wait();

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestTask_Task()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			using (var resetEvent1 = new AutoResetEvent(false))
			using (var resetEvent2 = new AutoResetEvent(false))
			using (var resetEvent3 = new AutoResetEvent(false))
			{
				Task.Run(() =>
				{
					using (factory.BeginThreadScope())
					{
						PokeDbConnectionForFactory(factory);

						Task.Run(() =>
						{
							using (factory.BeginThreadScope())
							{
								PokeDbConnectionForFactory(factory);

								Task.Run(() =>
								{
									using (factory.BeginThreadScope())
									{
										PokeDbConnectionForFactory(factory);
									}

									resetEvent3.Set();
								});

								PokeDbConnectionForFactory(factory);
							}

							resetEvent2.Set();
						});

						PokeDbConnectionForFactory(factory);
					}

					resetEvent1.Set();
				});

				resetEvent1.WaitOne();
				resetEvent2.WaitOne();
				resetEvent3.WaitOne();
			}

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestTask_TaskAwaitNested()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			using (var resetEvent = new AutoResetEvent(false))
			{
				Task.Run(async () =>
				{
					using (factory.BeginThreadScope())
					{
						PokeDbConnectionForFactory(factory);

						await Task.Run(async () =>
						{
							using (factory.BeginThreadScope())
							{
								PokeDbConnectionForFactory(factory);

								await Task.Run(() =>
								{
									using (factory.BeginThreadScope())
									{
										PokeDbConnectionForFactory(factory);
									}
								});

								PokeDbConnectionForFactory(factory);
							}
						});

						PokeDbConnectionForFactory(factory);

						await Task.Run(async () =>
						{
							using (factory.BeginThreadScope())
							{
								PokeDbConnectionForFactory(factory);

								await Task.Run(() =>
								{
									using (factory.BeginThreadScope())
									{
										PokeDbConnectionForFactory(factory);
									}
								});

								PokeDbConnectionForFactory(factory);
							}
						});

						PokeDbConnectionForFactory(factory);
					}

					resetEvent.Set();
				});

				resetEvent.WaitOne();
			}

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		public void TestTask_ContinueWith()
		{
			var factory = MakeFactoryForTest();

			var initialCount = factory.ConnectionCount;

			IDisposable disposableAction = null;
			var task = Task.Run(() =>
			{
				disposableAction = factory.BeginThreadScope();
				PokeDbConnectionForFactory(factory);
			}).ContinueWith(_ =>
			{
				using (factory.BeginThreadScope())
				{
					PokeDbConnectionForFactory(factory);
				}

				disposableAction.Dispose();
			});
			task.Wait();

			AssertEquals(initialCount, factory.ConnectionCount);
		}

		#endregion // Task As Main Context

		#region Implementation

		void PokeDbConnection(DbConnection connection)
		{
			connection.ExecuteReader("SELECT spid = @@SPID", (reader) => { });
		}

		void PokeDbConnectionForFactory(ThreadStaticConnectionFactory<DbConnection> factory)
		{
			PokeDbConnection(factory.ProvideConnection());
		}

		#endregion // Implementation

		public class IsDbConnectionDisposerMissingTest : TestCase
		{
			public void TestFromTask()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				bool disposerIsMissing = true;

				Task
					.Run(() =>
					{
						using (factory.BeginThreadScope())
						{
							// Act
							disposerIsMissing = factory.IsDbConnectionDisposerMissing();
						}
					})
					.Wait();

				// Assert
				AssertEquals("Disposer should not be missing", false, disposerIsMissing);
			}

			public void TestIsMissingFromTask()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				bool disposerIsMissing = false;

				Task
					.Run(() =>
					{
						// Act
						using (factory.BeginThreadScope())
						{ }
						disposerIsMissing = factory.IsDbConnectionDisposerMissing();
					})
					.Wait();

				// Assert
				AssertEquals("Disposer should be missing", true, disposerIsMissing);
			}

			public void TestFromThread()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				bool disposerIsMissing = true;

				var thread = new Thread(() =>
				{
					using (factory.BeginThreadScope())
					{
						disposerIsMissing = factory.IsDbConnectionDisposerMissing();
					}
				});

				// Act
				thread.Start();
				thread.Join();

				// Assert
				AssertEquals("Disposer should not be missing", false, disposerIsMissing);
			}

			public void TestIsMissingFromThread()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				bool disposerIsMissing = false;

				var thread = new Thread(() =>
				{
					using (factory.BeginThreadScope())
					{ }
					disposerIsMissing = factory.IsDbConnectionDisposerMissing();
				});

				// Act
				thread.Start();
				thread.Join();

				// Assert
				AssertEquals("Disposer should be missing", true, disposerIsMissing);
			}

			public void TestNestedTasks()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				bool disposerIsMissingOuter = true;
				bool disposerIsMissingInner = true;

				// Act
				Task
					.Run(() =>
					{
						using (factory.BeginThreadScope())
						{
							Task
								.Run(() =>
								{
									using (factory.BeginThreadScope())
									{
										disposerIsMissingInner = factory.IsDbConnectionDisposerMissing();
									}
								})
								.Wait();

							disposerIsMissingOuter = factory.IsDbConnectionDisposerMissing();
						}
					})
					.Wait();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Outer Disposer not be missing", false, disposerIsMissingOuter);
					AssertEquals("Inner Disposer not be missing", false, disposerIsMissingInner);
				});
			}

			public void TestDoesNotCauseBlocking()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				using var threadBlocker1 = new ManualResetEvent(false);
				using var threadBlocker2 = new ManualResetEvent(false);

				var firstThread = new Thread(() =>
				{
					using (factory.BeginThreadScope())
					{
						Assert("First thread is unblocked", threadBlocker1.WaitOne(TimeSpan.FromSeconds(15)));
						var disposerIsMissing = factory.IsDbConnectionDisposerMissing();
						threadBlocker2.Set();
					}
				});

				var secondThread = new Thread(() =>
				{
					using (factory.BeginThreadScope())
					{
						var disposerIsMissing = factory.IsDbConnectionDisposerMissing();
						threadBlocker1.Set();
						Assert("Second thread is unblocked", threadBlocker2.WaitOne(TimeSpan.FromSeconds(15)));
					}
				});

				var threads = new[] { firstThread, secondThread };
				threads.ForEach(t => t.Start());
				threads.ForEach(t => t.Join());

				// Assert
				Assert("All threads have exited with no blocks", threads.All(t => !t.IsAlive));
			}

			public void TestExcessiveTasks()
			{
				var factory = MakeFactoryForTest();

				// Arrange
				ConcurrentHashSet<Task> tasks = new ConcurrentHashSet<Task>();

				for (int i = 0; i < 10; i++)
				{
					var task = new Task(() =>
					{
						using (factory.BeginThreadScope())
						{
							// Act
							var disposerIsMissing = factory.IsDbConnectionDisposerMissing();
						}
					});

					task.Start();
					tasks.TryAdd(task);
				}

				// Assert
				Assert("All tasks complete", Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(15)));
			}
		}
	}
}
