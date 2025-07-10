using System;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemoteDesktopServices.Server.TrackingInfo
{
	[ThreadSafe]
	public class TrackingInfoLogger
	{
		public static readonly TrackingInfoLogger Instance = new TrackingInfoLogger();

		TrackingInfoLogger()
		{
		}

		public event EventHandler<string> OnNewLog;

		// to get rid of unnecessary string allocation, we only generate message when there is any subscriber
		public void NewLog(Func<string> messageFunc)
		{
			OnNewLog?.Invoke(this, messageFunc());
		}

		public event EventHandler OnShowAllDemand;
		public void ShowAll()
		{
			OnShowAllDemand?.Invoke(this, EventArgs.Empty);
		}

		public bool HasListener => OnNewLog != null;
	}
}
