using System;
using System.ComponentModel;

namespace CargoWise.Async
{
	public interface IAutoRefresher : IDisposable
	{
		bool IsPaused { get; set; }
		bool IsDisposed { get; }
		event PropertyChangedEventHandler PropertyChanged;
		void Refresh();
		TimeSpan RefreshDelay { get; set; }
		void Start(bool refreshImmediately = true);
		TimeSpan TimeUntilRefresh { get; set; }
	}
}
