using System;

namespace Enterprise.ZArchitecture.ActivityLogging
{
	public interface IActivityInfo
	{
		void End();
		int ProcessId { get; }
		string ProcessName { get; }
		string MainWindowText { get; }
		DateTime StartTimeUtc { get; }
		DateTime EndTimeUtc { get; }
		TimeSpan ActiveTime { get; }
		TimeSpan InactiveTime { get; }
		int ControlChanges { get; }
		int MouseClicks { get; }
		int KeyStrokes { get; }
		bool IsValid { get; }
	}
}
