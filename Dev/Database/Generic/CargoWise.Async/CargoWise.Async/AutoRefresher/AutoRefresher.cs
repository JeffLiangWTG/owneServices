using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;

namespace CargoWise.Async
{
	public sealed class AutoRefresher : Disposable, INotifyPropertyChanged, IAutoRefresher
	{
		public AutoRefresher(AutoRefreshAction update, TimeSpan refreshDelay, Func<bool> shouldRefresh = null)
		{
			Argument.NotNull(update, nameof(update));

			this.cancellationSource = new CancellationTokenSource();
			this.waitEvent = new AutoResetEvent(false);
			this.update = update;
			this.RefreshDelay = refreshDelay;
			this.shouldRefresh = shouldRefresh ?? (() => true);
		}

		public void Start(bool refreshImmediately = true)
		{
			if (refreshImmediately)
			{
				timeUntilRefresh = 0;
			}

			this.currentTask = Pulsar();
		}

		public bool IsPaused { get; set; }

		public TimeSpan RefreshDelay
		{
			get
			{
				return refreshDelay;
			}
			set
			{
				refreshDelay = value;
				TimeUntilRefresh = value;
			}
		}

		public TimeSpan TimeUntilRefresh
		{
			get
			{
				return TimeSpan.FromSeconds(timeUntilRefresh);
			}
			set
			{
				timeUntilRefresh = (int)value.TotalSeconds;
				OnPropertyChanged("TimeUntilRefresh");
			}
		}

		public void Refresh()
		{
			var oldState = Interlocked.CompareExchange(ref state, StateRestarting, StateReady);

			if (oldState == StateReady)
			{
				try
				{
					if (currentTask == null || currentTask.IsFaulted || currentTask.IsCompleted)
					{
						currentTask = Pulsar();
					}
					else
					{
						isManualRefresh = true;
						waitEvent.Set();
					}
				}
				finally
				{
					oldState = Interlocked.CompareExchange(ref state, StateReady, StateRestarting);
					TimeUntilRefresh = refreshDelay;
				}
			}
			else if (oldState == StateDisposed)
			{
				throw new ObjectDisposedException("Refresher");
			}
		}

		bool isManualRefresh;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions already reported on the continuation")]
		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				var oldState = Interlocked.Exchange(ref state, StateDisposed);

				if (oldState != StateDisposed)
				{
					cancellationSource.Cancel();
					waitEvent.Set();

					try
					{
						if (oldState == StateReady && currentTask != null)
						{
							try
							{
								currentTask.GetAwaiter().GetResult();
							}
							catch
							{ } // Exceptions already reported on the continuation
						}
					}
					finally
					{
						waitEvent.Dispose();
						cancellationSource.Dispose();
					}
				}
			}
		}

		Task Pulsar()
		{
			var result = PulsarCore(cancellationSource.Token);
			result.ContinueWith(t => MainThreadRunner.RunOnMainThread(() => ErrorReporter.ReportOnce("AutoRefresh", t.Exception)), cancellationSource.Token, TaskContinuationOptions.OnlyOnFaulted & TaskContinuationOptions.DenyChildAttach, TaskScheduler.Current);
			return result;
		}

		async Task PulsarCore(CancellationToken cancel)
		{
			while (!cancel.IsCancellationRequested)
			{
				await Pulse(cancel).ConfigureAwait(false);
			}
		}

		async Task Pulse(CancellationToken cancel)
		{
			if (refreshDelay < TimeSpan.FromSeconds(1))
			{
				await update(cancel).ConfigureAwait(false);
				waitEvent.Reset();
				await waitEvent.WaitOneAsync(refreshDelay, cancel).ConfigureAwait(false);
			}
			else
			{
				if (!IsPaused || isManualRefresh)
				{
					DecrementTimeUntilRefresh();

					if ((timeUntilRefresh <= 0 || isManualRefresh) && shouldRefresh())
					{
						await update(cancel).ConfigureAwait(false);
						if (isManualRefresh)
						{
							isManualRefresh = false;
						}
						else
						{
							TimeUntilRefresh = refreshDelay;
						}
					}
				}
				waitEvent.Reset();
				await waitEvent.WaitOneAsync(TimeSpan.FromSeconds(1), cancel).ConfigureAwait(false);
			}
		}

		void DecrementTimeUntilRefresh()
		{
			timeUntilRefresh = Math.Max(timeUntilRefresh - 1, 0);
			OnPropertyChanged("TimeUntilRefresh");
		}

		readonly CancellationTokenSource cancellationSource;
		readonly AutoResetEvent waitEvent;
		readonly AutoRefreshAction update;
		readonly Func<bool> shouldRefresh;

		TimeSpan refreshDelay;
		Task currentTask;
		int state;
		int timeUntilRefresh;

		/// <summary>
		/// The object is not disposed and cleanup has not been suspended.
		/// </summary>
		const int StateReady = 0;

		/// <summary>
		/// Dispose has been requested.
		/// </summary>
		const int StateDisposed = 1;

		/// <summary>
		/// The reset method has temporarialy suspended dispose clean-up.
		/// </summary>
		const int StateRestarting = 2;

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));

			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion

		#region For Test
#if DEBUG

		public bool WaitForNextPulse_ForTest()
		{
			return WaitForNextPulse_ForTest(TimeSpan.FromSeconds(5));
		}

		public bool WaitForNextPulse_ForTest(TimeSpan timeout)
		{
			return waitEvent.WaitOne(timeout);
		}

#endif
		#endregion
	}
}
