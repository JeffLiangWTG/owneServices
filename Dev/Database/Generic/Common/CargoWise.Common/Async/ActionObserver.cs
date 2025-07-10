using System;
using System.Threading;
namespace CargoWise.Common.Async
{
#nullable enable
	static class ActionObserver //Observes a task and stops it when the backlog gets too full
	{
		/// <summary>
		/// Runs an action on the main thread while an observer delegate runs on a separate thread and periodically returns if the main action should be canceled or not.
		/// </summary>
		/// <param name="runnerAction">The action to run on the main thread.</param>
		/// <param name="canCancelOnObserve">Observer delegate to be run on a separate thread. The return value refers to whether or not the main action should be canceled.</param>
		/// <param name="observerInterval">How frequently the canCancelOnObserve delegate should be run.</param>
		/// <param name="runnerCTS"></param>
		/// <returns>Whether or not the task has been canceled due to the observer. Will return false if task completed successfully or was canceled due to an exception.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static bool RunObservableTask(Action runnerAction, Func<bool> canCancelOnObserve, TimeSpan observerInterval, CancellationTokenSource runnerCTS)
		{
			if (runnerAction == null)
			{
				throw new ArgumentNullException(nameof(runnerAction));
			}

			if (canCancelOnObserve == null)
			{
				throw new ArgumentNullException(nameof(canCancelOnObserve));
			}

			if (observerInterval <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(observerInterval));
			}

			if (runnerCTS == null)
			{
				throw new ArgumentNullException(nameof(runnerCTS));
			}

			var canceledDueToObserver = false;

			using var observerCTS = CancellationTokenSource.CreateLinkedTokenSource(runnerCTS.Token);
			var observerThread = new Thread(() =>
			{
				try
				{
					while (!observerCTS.Token.IsCancellationRequested)
					{
						observerCTS.Token.WaitHandle.WaitOne(observerInterval);

						if (!observerCTS.Token.IsCancellationRequested && canCancelOnObserve())
						{
							canceledDueToObserver = true;
							runnerCTS.Cancel();
						}
					}
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce($"{ex.GetType()} occurred within the canCancelObserver delegate.", ex);
					throw;
				}
			});

			observerThread.Name = "Action Observer";

			observerThread.Start();

			try
			{
				runnerAction();
			}
			catch (OperationCanceledException)
			{
				return canceledDueToObserver;
			}
			finally
			{
				observerCTS?.Cancel();
				observerThread?.Join();
			}

			return canceledDueToObserver;
		}
	}
#nullable disable
}
