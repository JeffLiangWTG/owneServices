using System;
using System.IO;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ActivityLogger
{
	public class ExternalActivityLoggerProvider : IExternalActivityLogger
	{
		#region IExternalActivityLogger Members

		public IDisposable GetInstance()
		{
			return ActivityLogger.Instance;
		}

		public bool Started
		{
			get
			{
				try
				{
					return ActivityLogger.Instance.Started;
				}
				catch (IOException ex)
				{
					if (!hasReported)
					{
						ExceptionReporter.Instance.ReportDeveloperException(exceptionReportKey, ex.Message, ex);
						hasReported = true;
					}
					return false;
				}
			}
		}

		public void Start()
		{
			try
			{
				ActivityLogger.Instance.Start();
			}
			catch (IOException ex)
			{
				if (!hasReported)
				{
					ExceptionReporter.Instance.ReportDeveloperException(exceptionReportKey, ex.Message, ex);
					hasReported = true;
				}
			}
		}

		public void Stop()
		{
			try
			{
				ActivityLogger.Instance.Stop();
			}
			catch (IOException ex)
			{
				if (!hasReported)
				{
					ExceptionReporter.Instance.ReportDeveloperException(exceptionReportKey, ex.Message, ex);
					hasReported = true;
				}
			}
		}

		[CLSCompliant(false)] // I am unsure why this is necessary. If anyone figures it out please let me know.
		public IActivityInfo DequeueActivityInfo()
		{
			return ActivityLogger.Instance.DequeueActivityInfo();
		}

#if DEBUG
		public void AddActivityInfoTestOnly(string processName, string mainWindowText)
		{
			ActivityLogger.Instance.AddActivityInfoTestOnly(processName, mainWindowText);
		}

		public bool OtherInstanceOfActivityLoggerIsRunning()
		{
			return ActivityLogger.OtherInstanceOfActivityLoggerIsRunning();
		}
#endif

		const string exceptionReportKey = "{9C8F319F-A39F-49a6-B695-CA96D20EC7AB}";
		static bool hasReported;

		#endregion
	}
}
