#region Test
#if DEBUG

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.Common.Async.CausalityTracking;
using NUnit.Framework;

namespace CargoWise.Common.AsyncTaskContext.Testing
{
	[GuiTest]
	class TaskContextTests : TestCase
	{
		protected override void SetUp()
		{
			TaskContext.Current = TaskContext.CreatePumpedTaskContext();
		}

		protected override void TearDown()
		{
			TaskContext.Current.Dispose();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void TestUnitTestsHaveTaskContext()
		{
			Assert("No task Context found in unit test", TaskContext.Current != null);
		}

		public void TestDeferredActionThrowsException()
		{
			var realDispatcher = ApplicationDispatcher.Current;
			ApplicationDispatcher.Current = null;
			try
			{
				TaskContext.Current.EnqueueDeferredTask(() => throw new Exception());
				((IPumpedTaskContext)TaskContext.Current).PumpTasks();
				Assert(ErrorReporter.TotalErrorCount == 1);
				AssertEquals(typeof(Exception), ErrorReporter.LastExceptionReported.GetType());
				ErrorReporter.Clear();
			}
			finally
			{
				ApplicationDispatcher.Current = realDispatcher;
			}
		}

		public void TestTaskDoesNotExecuteIfNotPumped()
		{
			int count = 0;
			TaskContext.Current.EnqueueDeferredTask(() => ++count);
			AssertEquals("Task should not have executed", 0, count);
			((IPumpedTaskContext)TaskContext.Current).PumpTasks();
			AssertEquals("Task should have executed", 1, count);
		}

		public void TestPumpedTaskManagerIndicatesWhenPumpingRequired()
		{
			var pumpedTaskManager = ((IPumpedTaskContext)TaskContext.Current);
			AssertEquals(false, pumpedTaskManager.PumpingRequired.WaitOne(0));
			pumpedTaskManager.EnqueueDeferredTask(() => { });
			AssertEquals(true, pumpedTaskManager.PumpingRequired.WaitOne(0));
		}

		public void TestMessagePumpTaskExecution()
		{
			async Task MessagePumpTaskExecution(Action counter)
			{
				await TaskContext.Current.EnqueueTask(counter);
			}

			int count = 0;
			TaskPumper(MessagePumpTaskExecution(() => ++count));
			AssertEquals("Task did not execute", 1, count);
		}

		public void TestNewThreadHasNoTaskContext()
		{
			var mainContext = TaskContext.Current;
			var workerThread = new Thread(() =>
			{
				var context = TaskContext.Current;
				AssertNotNull(context);
				AssertNotEquals("Every thread will get a new context", context, mainContext);
			});

			workerThread.Start();
			workerThread.Join();
		}

		public void TestExceptionsCanBeCaught()
		{
			async Task ExceptionsCanBeCaught()
			{
				try
				{
					await TaskContext.Current.EnqueueTask(() => throw new InvalidCastException("Exception should be handled"));
					Assert("Exception was not caught", false);
				}
				catch (InvalidCastException)
				{
				}

				try
				{
					using (var context = TaskContext.CreateThreadTaskContext())
					{
						await context.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
					}

					Assert("Exception was not caught", false);
				}
				catch (InvalidOperationException)
				{
				}

				Assert(true);
			}

			TaskPumper(ExceptionsCanBeCaught());
		}

		public void TestPumpedExceptionsPropogate()
		{
			async Task PumpedExceptionsPropogate()
			{
				await TaskContext.Current.EnqueueTask(() => throw new InvalidCastException("Exception should be handled"));
				Assert("Exception was not caught", false);
			}

			AssertExceptionThrown<AggregateException>(() => TaskPumper(PumpedExceptionsPropogate()));
		}

		public void TestBackgroundExceptionsPropogate()
		{
			async Task Func()
			{
				using (var context = TaskContext.CreateThreadTaskContext())
				{
					await context.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
				}
			}

			AssertExceptionThrown<AggregateException>(() => TaskPumper(Func()));
		}

		public void TestDeferredExceptionCanBeHandledOnGuiThread()
		{
			RegisterDispatcher.Register();

			var failureMessage = string.Empty;
			bool keepGoing = true;
			using (var threadContext = TaskContext.CreateThreadTaskContext(() => { }, () => { }, (ex) =>
			{
				try
				{
					ex.ThrowExceptionOnMainThread();
				}
				finally
				{
					failureMessage = $"{ex}{Environment.NewLine}{new StackTrace()}";
					Thread.Sleep(TimeSpan.FromSeconds(5));
					keepGoing = false;
				}
			}))
			{
				threadContext.EnqueueDeferredTask(() => throw new InvalidOperationException("Exception should be correctly marshalled to UI thread"));
			}

			while (keepGoing)
			{
				Application.DoEvents();
			}

			Assert(failureMessage, ErrorReporter.TotalErrorCount > 0);
			ErrorReporter.Clear();
		}

		public void TestDeferredPumpedExceptionCanBeHandledOnGuiThreadConsistantly()
		{
			RegisterDispatcher.Register();

			bool keepGoing = true;
			using (var threadContext = TaskContext.CreatePumpedTaskContext((ex) =>
			{
				try
				{
					ex.ThrowExceptionOnMainThread();
				}
				finally
				{
					keepGoing = false;
				}
			}))
			{
				threadContext.EnqueueDeferredTask(() => throw new InvalidOperationException("Exception should be correctly marshalled to UI thread"));

				while (keepGoing)
				{
					threadContext.PumpTasks();
					Application.DoEvents();
				}
			}

			Assert(ErrorReporter.TotalErrorCount > 0);
			ErrorReporter.Clear();
		}

		public void TestBackgroundExceptionProducesCausalityException()
		{
			async Task Func()
			{
				using (var context = TaskContext.CreateThreadTaskContext())
				{
					await context.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
				}
			}

			try
			{
				TaskPumper(Func());
			}
			catch (AggregateException ex)
			{
				Assert(ex.InnerException is InvalidOperationException);
				Assert(CausalityTracker.TryGetDependencyChainedStack(ex.InnerException, out var discard));
			}
		}

		public void TestPumpedExceptionProducesCausalityException()
		{
			async Task Func()
			{
				await TaskContext.Current.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
			}

			try
			{
				TaskPumper(Func());
			}
			catch (AggregateException ex)
			{
				Assert(ex.InnerException is InvalidOperationException);
				Assert(CausalityTracker.TryGetDependencyChainedStack(ex.InnerException, out var discard));
			}
		}

		public void TestDeferredExecutorProducesCausalityException()
		{
			TaskContext.SetTaskContext(TaskContext.CreatePumpedTaskContext((ex) => throw ex, () => true));
			TaskContext.Current.EnqueueDeferredTask(() => throw new InvalidOperationException("Exception should be handled"));

			try
			{
				((IPumpedTaskContext)TaskContext.Current).PumpTasks();
				Application.DoEvents();
			}
			catch (InvalidOperationException ex)
			{
				Assert(CausalityTracker.TryGetDependencyChainedStack(ex, out var discard));
			}
		}

		public void TestCausalityExceptionPassedThroughContexts()
		{
			var mainContext = TaskContext.Current;

			async Task Func()
			{
				await TaskContext.Current.EnqueueTask(async () =>
				{
					using (var context = TaskContext.CreateThreadTaskContext())
					{
						await context.EnqueueTask(async () =>
						{
							await mainContext.EnqueueTask(() =>	throw new InvalidOperationException("Exception should be handled"));
						});
					}
				});
			}

			try
			{
				TaskPumper(Func());
			}
			catch (AggregateException ex)
			{
				Assert(ex.InnerException is InvalidOperationException);
				Assert(CausalityTracker.TryGetDependencyChainedStack(ex.InnerException, out var discard));
			}
		}

		public void TestExceptionChainingMultiple()
		{
			var ex = new Exception();
			AssertEquals(CausalityTracker.TryGetDependencyChainedStack(ex, out var discard), false);

			CausalityTracker.AddDependencyChainedStack(ex, "a");
			Assert(CausalityTracker.TryGetDependencyChainedStack(ex, out var text));
			AssertEquals("a", text);

			CausalityTracker.AddDependencyChainedStack(ex, "b");
			Assert(CausalityTracker.TryGetDependencyChainedStack(ex, out var text2));
			AssertEquals("ab", text2);
		}

		public void TestExceptionLookupMultiple()
		{
			var ex1 = new Exception();
			var ex2 = new Exception();
			var ex3 = new Exception();
			var ex4 = new Exception();

			CausalityTracker.AddDependencyChainedStack(ex1, "a");
			CausalityTracker.AddDependencyChainedStack(ex2, "b");
			CausalityTracker.AddDependencyChainedStack(ex3, "c");
			CausalityTracker.AddDependencyChainedStack(ex4, "d");

			ForceWeakReferenceActivity();
			GC.Collect();

			Assert(CausalityTracker.TryGetDependencyChainedStack(ex1, out var text1));
			AssertEquals("a", text1);

			Assert(CausalityTracker.TryGetDependencyChainedStack(ex2, out var text2));
			AssertEquals("b", text2);

			Assert(CausalityTracker.TryGetDependencyChainedStack(ex3, out var text3));
			AssertEquals("c", text3);

			Assert(CausalityTracker.TryGetDependencyChainedStack(ex4, out var text4));
			AssertEquals("d", text4);
		}

		void ForceWeakReferenceActivity()
		{
			var ex5 = new Exception();
			CausalityTracker.AddDependencyChainedStack(ex5, "e");
		}

		public void TestNestedExceptionsCanBeCaught()
		{
#if NET
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
#endif
			var mainContext = TaskContext.Current;

			async Task Func()
			{
				bool wasCaught = false;

				try
				{
					await TaskContext.Current.EnqueueTask(async () =>
					{
						using (var context = TaskContext.CreateThreadTaskContext())
						{
							await context.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
						}
					});
				}
				catch (InvalidOperationException)
				{
					wasCaught = true;
				}

				Assert(wasCaught);
				AssertEquals(TaskContext.Current, mainContext);

				wasCaught = false;

				try
				{
					using (var context = TaskContext.CreateThreadTaskContext())
					{
						await context.EnqueueTask(async () =>
						{
							await TaskContext.Current.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
						});
					}
				}
				catch (InvalidOperationException)
				{
					wasCaught = true;
				}

				Assert(wasCaught);
				AssertEquals(TaskContext.Current, mainContext);

				wasCaught = false;

				try
				{
					using (var context = TaskContext.CreateThreadTaskContext())
					{
						await context.EnqueueTask(async () =>
						{
							await context.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
						});
					}
				}
				catch (InvalidOperationException)
				{
					wasCaught = true;
				}

				Assert(wasCaught);
				AssertEquals(TaskContext.Current, mainContext);

				wasCaught = false;

				try
				{
					await TaskContext.Current.EnqueueTask(async () =>
					{
						await TaskContext.Current.EnqueueTask(() => throw new InvalidOperationException("Exception should be handled"));
					});
				}
				catch (InvalidOperationException)
				{
					wasCaught = true;
				}

				Assert(wasCaught);
				AssertEquals(TaskContext.Current, mainContext);
			}

			TaskPumper(Func());
		}

		public void TestExecuteRawTaskNoReturn()
		{
			async Task Func()
			{
				await TaskContext.Current.EnqueueTask(() => Assert(true));
			}

			TaskPumper(Func());
		}

		public void TestExecuteRawTaskWithReturn()
		{
			async Task Func()
			{
				AssertEquals(5, await TaskContext.Current.EnqueueTask(() => 5));
			}

			TaskPumper(Func());
		}

		public void TestExecuteAsyncTaskNoReturn()
		{
			async Task Func()
			{
				await TaskContext.Current.EnqueueTask(async () =>
				{
					await Task.Delay(0);
					Assert(true);
				});
			}

			TaskPumper(Func());
		}

		public void TestExecuteAsyncTaskWithReturn()
		{
			async Task Func()
			{
				var result = await TaskContext.Current.EnqueueTask(async () =>
				{
					await Task.Delay(0);
					return 5;
				});

				AssertEquals(5, result);
			}

			TaskPumper(Func());
		}

		public void TestBackgroundThreadAsyncContextPreservation()
		{
			using (var threadContext1 = TaskContext.CreateThreadTaskContext())
			{
				var task = threadContext1.EnqueueTask(async () =>
				{
					Assert(TaskContext.Current == threadContext1);
					await Task.Delay(5);
					Assert(TaskContext.Current == threadContext1);
				});

				while (!task.IsCompleted)
				{
					Application.DoEvents();
				}
			}
		}

		public void TestCorrectContextExecutionDifferentContext()
		{
#if NET
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
#endif
			var mainContext = TaskContext.Current;

			async Task Func()
			{
				await TaskContext.Current.EnqueueTask(async () =>
				{
					Assert(TaskContext.Current == mainContext);
					using (var threadContext1 = TaskContext.CreateThreadTaskContext())
					using (var threadContext2 = TaskContext.CreateThreadTaskContext())
					{
						await threadContext1.EnqueueTask(async () =>
						{
							Assert(TaskContext.Current == threadContext1);
							await mainContext.EnqueueTask(() => Assert(TaskContext.Current == mainContext));
							await threadContext2.EnqueueTask(() => Assert(TaskContext.Current == threadContext2));
							Assert(TaskContext.Current == threadContext1);
						});
					}

					Assert(TaskContext.Current == mainContext);
				});
			}

			TaskPumper(Func());
		}

		public void TestThreadWithoutContextCanWait()
		{
			TaskContext.SetTaskContext(null);

			var context = TaskContext.CreateThreadTaskContext();
			var signal = new ManualResetEvent(false);

			var task = context.EnqueueTask(() =>
			{
				Thread.Sleep(2000);
				signal.Set();
			});

			task.Wait();
			AssertEquals(true, signal.WaitOne(0));
		}

		void TaskPumper(Task testMethodTask)
		{
			while (!testMethodTask.IsCompleted)
			{
				((IPumpedTaskContext)TaskContext.Current).PumpTasks();
				Application.DoEvents();
			}

			// Make any unhandled exceptions visible
			testMethodTask.Wait();
		}
	}
}

#endif
#endregion
