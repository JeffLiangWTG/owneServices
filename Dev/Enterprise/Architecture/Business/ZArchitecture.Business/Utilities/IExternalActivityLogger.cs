using System;

namespace Enterprise.ZArchitecture.ActivityLogging
{
	public interface IExternalActivityLogger
	{
		IDisposable GetInstance();
		bool Started { get; }
		void Start();
		void Stop();
		IActivityInfo DequeueActivityInfo();

#if DEBUG
		void AddActivityInfoTestOnly(string processName, string mainWindowText);
		bool OtherInstanceOfActivityLoggerIsRunning();
#endif

	}
}
