using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Enterprise.ZArchitecture.ActivityLogging;

namespace Enterprise.ActivityLogger
{
	public class ActivityInfo : IActivityInfo
	{
		ActivityInfo()
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		internal ActivityInfo(int processId)
		{
			ProcessId = processId;
			var mainWindowHandle = IntPtr.Zero;

			try
			{
				var process = Process.GetProcessById(ProcessId);
				ProcessName = process.ProcessName;
				mainWindowHandle = process.MainWindowHandle;
			}
			catch (ArgumentException) { }
			catch (InvalidOperationException) { }
			catch (Win32Exception) { }

			if (mainWindowHandle != IntPtr.Zero)
			{
				try
				{
					MainWindowText = SafeNativeMethods.GetWindowText(mainWindowHandle);
				}
				catch (Win32Exception)
				{
					MainWindowText = string.Format(CultureInfo.CurrentCulture, "(Unknown window title (PID:{0}))", processId);
				}
			}

			StartTimeUtc = DateTime.UtcNow;
		}

		public void End()
		{
			EndTimeUtc = DateTime.UtcNow;
		}

		#region Properties

		public int ProcessId { get; }

		public string ProcessName { get; set; }

		public string MainWindowText { get; set; }

		public DateTime StartTimeUtc { get; }

		public DateTime EndTimeUtc { get; set; }

		public TimeSpan ActiveTime => EndTimeUtc - StartTimeUtc - InactiveTime;

		public TimeSpan InactiveTime { get; internal set; }

		public int ControlChanges { get; internal set; }

		public int MouseClicks { get; internal set; }

		public int KeyStrokes { get; internal set; }

		public bool IsValid => MainWindowText != null;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		public override string ToString()
		{
			if (IsValid)
			{
				return string.Format(CultureInfo.InvariantCulture, "ProcessId = {0}\nMainWindowText = {1}\nStartTimeUtc = {2}\nEndTimeUtc = {3}\nActiveTime = {4}\nInactiveTime = {5}\nControlChanges = {6}\nMouseClicks = {7}\nKeyStrokes = {8}",
					ProcessId,
					MainWindowText,
					StartTimeUtc,
					EndTimeUtc,
					ActiveTime,
					InactiveTime,
					ControlChanges,
					MouseClicks,
					KeyStrokes);
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, "ProcessId = {0}\nInvalid!", ProcessId);
			}
		}

		#region Test methods

		internal static ActivityInfo CreateActivityInfoTestOnly(string processName, string mainWindowText)
		{
			var activityInfo = new ActivityInfo();
			activityInfo.ProcessName = processName;
			activityInfo.MainWindowText = mainWindowText;

			return activityInfo;
		}

		#endregion
	}
}
