using System;
using System.ComponentModel;
using System.Threading;
using CargoWise.Async;

namespace Enterprise.VisualBoards.Business.Test
{
	/// <summary>
	/// AutoRefresher that can be triggerred manually i.e. synchronous (For Testing)
	/// </summary>
	public class TriggerableAutoRefresher : IAutoRefresher
	{
		public TriggerableAutoRefresher(AutoRefreshAction updateAction, TimeSpan delay, Func<bool> shouldRefresh = null)
		{
			update = updateAction;
			RefreshDelay = delay;
			this.shouldRefresh = shouldRefresh ?? (() => true);
		}

		public void Start(bool refreshImmediately = true)
		{
			if (refreshImmediately)
			{
				timeUntilRefresh = 0;
				Refresh();
			}
		}

		/// <summary>
		/// Refresh right away, without waiting for delay
		/// </summary>
		public void Refresh()
		{
			if (shouldRefresh())
			{
				var token = new CancellationToken();
				update(token);

				TimeUntilRefresh = refreshDelay;
			}
		}

		/// <summary>
		/// Decrement delay and refresh when it reaches 0
		/// </summary>
		public void Pulse(int totalPulse)
		{
			for (int i = 0; i < totalPulse; i++)
			{
				RefreshWaitDelay();
			}
		}

		#region Implementation

		void RefreshWaitDelay()
		{
			if (timeUntilRefresh > 0)
			{
				DecrementTimeUntilRefresh();
			}

			if ((RefreshDelay < TimeSpan.FromSeconds(1)) || (timeUntilRefresh <= 0))
			{
				Refresh();
			}
		}

		void DecrementTimeUntilRefresh()
		{
			timeUntilRefresh = Math.Max(timeUntilRefresh - 1, 0);
			OnPropertyChanged("TimeUntilRefresh");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

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

		TimeSpan refreshDelay;

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

		int timeUntilRefresh;

		public bool IsPaused { get; set; }

		public void Dispose()
		{
			IsDisposed = true;
		}

		public bool IsDisposed { get; private set; }

		readonly AutoRefreshAction update;
		readonly Func<bool> shouldRefresh;

		#endregion
	}
}
