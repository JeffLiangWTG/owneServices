using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

[assembly: CLSCompliant(true)]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "This is the dumbest CA rule I've ever seen. Just read the docs.", Scope = "namespace", Target = "Enterprise.ActivityLogger")]
namespace Enterprise.ActivityLogger
{
	[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	public class ActivityLogger : IDisposable
	{
		#region Constructor

		public static ActivityLogger Instance => instance ?? (instance = new ActivityLogger());

		#endregion

		#region Start / Stop

		public bool Started => hookerThread != null;

		// returns true if the thread is started, regardless of whether we actually started it
		// now or it was already started
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		public bool Start()
		{
			if (disposed)
			{
				TestLogAppendLine("Start: already disposed");
				return false;
			}

			if (!Started && Hooker.Attach())
			{
				if (Hooker.Instance is null)
				{
					throw new InvalidOperationException("Hooker was null before HookerThread was entered");
				}

				hookerThread = new Thread(HookerThread);
				hookerThread.Start();
			}

			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		public void Stop()
		{
			if (disposed)
			{
				TestLogAppendLine("Stop: already disposed");
				return;
			}

			if (Started)
			{
				stopHookTask = true;
				Thread.MemoryBarrier();
				hookerThread.Join();
				hookerThread = null;
			}
		}

		#endregion

		#region Hooker thread

		void HookerThread()
		{
			if (Hooker.Instance is null)
			{
				throw new InvalidOperationException("Hooker was null after HookerThread was entered");
			}

			// this will dispose the Hooker instance on loop exit, effectively Detaching the Hooker
			using (var hooker = Hooker.Instance)
			{
				var enterpriseProcessId = Process.GetCurrentProcess().Id;
				ActivityInfo currentProcessInfo = null;
				var inactive = false;
				var inactivityStarted = DateTime.Now; // Doesn't use ZArchitecture

				while (!stopHookTask)
				{
					var hookerEvent = Hooker.Instance.WaitForEvent(1000);
					if (hookerEvent.EventType == SyshookInterop.HookerEventType.TimeOut || hookerEvent.EventType == SyshookInterop.HookerEventType.Unknown)
					{
						if (!inactive)
						{
							inactive = true;
							inactivityStarted = DateTime.Now; // Doesn't use ZArchitecture
						}
					}
					else
					{
						if (hookerEvent.EventType == SyshookInterop.HookerEventType.Cbt && (hookerEvent.NCode != (Int32)SyshookInterop.HCBT.SetFocus || currentProcessInfo == null || currentProcessInfo.ProcessId != hookerEvent.ProcessId))
						{
							continue;
						}

						if (inactive)
						{
							inactive = false;
							if (currentProcessInfo != null)
							{
								var timeSpan = DateTime.Now - inactivityStarted; // Doesn't use ZArchitecture
								if (timeSpan.TotalMinutes > 1)
								{
									currentProcessInfo.InactiveTime += timeSpan;
								}
							}
						}

						if (hookerEvent.EventType == SyshookInterop.HookerEventType.Mouse && hookerEvent.WParam == (IntPtr)SyshookInterop.WM_MouseMessage.MouseMove)
						{
							continue;
						}

						ActivityInfo previousProcessInfo = null;
						if (currentProcessInfo == null || currentProcessInfo.ProcessId != hookerEvent.ProcessId)
						{
							previousProcessInfo = currentProcessInfo;
							currentProcessInfo = new ActivityInfo(hookerEvent.ProcessId);
						}

						if (previousProcessInfo != null)
						{
							previousProcessInfo.End();
							if (previousProcessInfo.IsValid && previousProcessInfo.ProcessId != enterpriseProcessId)
							{
								EnqueueActivityInfo(previousProcessInfo);
							}
						}

						if (currentProcessInfo.IsValid)
						{
							AddEventToProcessInfo(hookerEvent, ref currentProcessInfo);
						}
					}
				}

				currentProcessInfo?.End();
			}
		}

		static void AddEventToProcessInfo(HookerEvent e, ref ActivityInfo currentProcessInfo)
		{
			switch (e.EventType)
			{
				case SyshookInterop.HookerEventType.Mouse:
					if (e.WParam == (IntPtr)SyshookInterop.WM_MouseMessage.LeftButtonDown || e.WParam == (IntPtr)SyshookInterop.WM_MouseMessage.RightButtonDown)
					{
						currentProcessInfo.MouseClicks++;
					}
					break;
				case SyshookInterop.HookerEventType.Keyboard:
					currentProcessInfo.KeyStrokes++;
					break;
				case SyshookInterop.HookerEventType.Cbt:
					if (e.NCode == (Int32)SyshookInterop.HCBT.SetFocus)
					{
						currentProcessInfo.ControlChanges++;
					}
					break;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "The design has ensured there are no thread issues with this. Additionally, alternatives to this such as Overridable do not work.")]
		static ActivityLogger instance;
		Thread hookerThread;
		volatile bool stopHookTask;

		#endregion

		#region ActivityInfoQueue

		void EnqueueActivityInfo(ActivityInfo activityInfo)
		{
			activityInfoQueue.Enqueue(activityInfo);
		}

		public ActivityInfo DequeueActivityInfo()
		{
			return activityInfoQueue.TryDequeue(out ActivityInfo result) ? result : null;
		}

		readonly ConcurrentQueue<ActivityInfo> activityInfoQueue = new ConcurrentQueue<ActivityInfo>();

		#region Test methods
#if DEBUG

		public void AddActivityInfoTestOnly(string processName, string mainWindowText)
		{
			EnqueueActivityInfo(ActivityInfo.CreateActivityInfoTestOnly(processName, mainWindowText));
		}

#endif
		#endregion

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ActivityLogger()
		{
			Dispose(false);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		void Dispose(bool disposing)
		{
			Stop();

			if (!disposed && disposing)
			{
				disposed = true;
			}
		}

		bool disposed;

		#endregion

		#region Test Methods

		[Conditional("DEBUG")]
		public void TestLogAppendLine(string value)
		{
#if DEBUG
			if (log == null)
			{
				log = new StringBuilder();
			}

			log.AppendLine(value);
#endif
		}

		[Conditional("DEBUG")]
		[SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Test code")]
		public void TestLogAppendListOfProcesses()
		{
			try
			{
				var processes = Process.GetProcesses();
				foreach (var process in processes)
				{
					if (process.Id == Process.GetCurrentProcess().Id)
					{
						TestLogAppendLine("current => " + process.ProcessName);
					}
					else
					{
						TestLogAppendLine(process.ProcessName);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				TestLogAppendLine(ex.Message);
			}
		}

#if DEBUG
		public void ClearTestLog()
		{
			log = null;
		}

		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public string GetTestLog() => log?.ToString() ?? "";

		StringBuilder log;
#endif

#if DEBUG
		public static bool OtherInstanceOfActivityLoggerIsRunning()
		{
			string syshookPath = null;
			try
			{
				syshookPath = SafeNativeMethods.GetSyshookPath();
			}
			catch (Exception e) when (e is Win32Exception)
			{
				// Don't show to user, just report back to us for investigation
				ExceptionReporter.Instance.ReportDeveloperException("GetSyshookPath", e);
			}

			if (syshookPath != null)
			{
				var sysHookDir = Path.GetDirectoryName(syshookPath);
				var enterpriseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				return string.Compare(sysHookDir, enterpriseDir, StringComparison.OrdinalIgnoreCase) != 0;
			}

			return false;
		}
#endif

		#endregion
	}
}
