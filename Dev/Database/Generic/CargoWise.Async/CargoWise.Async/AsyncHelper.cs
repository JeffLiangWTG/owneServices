using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.Async
{
	public static class AsyncHelper
	{
#if DEBUG
		/// <summary>
		/// Waits for all active tasks to complete, to assist with test cleanup.
		/// </summary>
		/// <remarks>
		///<para>
		/// This is only suitable for use in test code where a test knows all the
		/// tasks that are running in its environment and knows that it is safe to wait for them.
		///
		/// Production code must not call this because it cannot know what background tasks may be running in the same process.
		/// Instead, production code should await, Task.WaitAll, etc. the Task objects it knows about.</para>
		/// <para>
		/// Example of when to use this: A test calls a third-party library that runs a fire-and-forget background housekeeping task.
		/// The test wants to wait for that task to finish before the test completes, so any exceptions can be caught
		/// and so the test does not leak global state which could affect subsequent tests. (The TaskTestListener
		/// would fail the test otherwise.)</para>
		/// </remarks>
		/// <param name="expectedExceptionHandler">A predicate for AggregateException.Handle. Returns true if the exception should be ignored, false if it should be rethrown.</param>
		/// <param name="timeout">Timeout period to wait for tasks, default is 30 seconds</param>
		public static void WaitAllActiveTasksForTest(Func<Exception, bool> expectedExceptionHandler = null)
		{
			WaitAllActiveTasksForTest(TimeSpan.FromSeconds(30), expectedExceptionHandler);
		}

		public static void WaitAllActiveTasksForTest(TimeSpan timeout, Func<Exception, bool> expectedExceptionHandler = null)
		{
			// This relies on TaskTestListener having already enabled async debugging, otherwise active tasks are not tracked.
			if (currentActiveTasksField == null)
			{
				var taskType = typeof(Task);
				currentActiveTasksField = taskType.GetField("s_currentActiveTasks", BindingFlags.NonPublic | BindingFlags.Static);
			}

			var stopwatch = Stopwatch.StartNew();
			Task[] activeTasks;
			int retryCount = 0;

			do
			{
				while (true)
				{
					try
					{
						var activeTasksDictionary = (Dictionary<int, Task>)currentActiveTasksField.GetValue(null);
						activeTasks = activeTasksDictionary?.Values.Where(task => task != null).ToArray() ?? Array.Empty<Task>();
						break;
					}
					catch (InvalidOperationException)
					{
						if (retryCount++ == 100)
						{
							throw;
						}
						Thread.Sleep(TimeSpan.FromMilliseconds(50));
					}
				}

				if (activeTasks.Length > 0)
				{
					try
					{
						if (!Task.WaitAll(activeTasks, 100) && stopwatch.Elapsed > timeout)
						{
							throw new TimeoutException($"Active tasks did not complete within {timeout}");
						}
					}
					catch (AggregateException ex)
					{
						ex.Flatten().Handle(innerEx => innerEx is OperationCanceledException || (expectedExceptionHandler != null && expectedExceptionHandler(innerEx)));
					}
				}
			}
			while (activeTasks.Length > 0);
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "FieldInfo always same so only needs to be retrieved once per run.")]
		static FieldInfo currentActiveTasksField;
#endif

		public static Task RunTask(Action action, string description)
		{
			var task = Task.Run(action);

#if DEBUG
			TaskRegistryForTest.RegisterTask(task, description);
#endif
			return task;
		}

		public static Task<TResult> RunTask<TResult>(Func<TResult> action, CancellationToken cancellationToken, string description)
		{
			var task = Task.Run(action, cancellationToken);

#if DEBUG
			TaskRegistryForTest.RegisterTask(task, description);
#endif
			return task;
		}
	}
}
