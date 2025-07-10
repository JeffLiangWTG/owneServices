using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.ActivityLogging
{
	public class ZFormActivityLogger : IDisposable
	{
		public static ZFormActivityLogger Instance
		{
			get
			{
				if (instance_ == null)
				{
					lock (instanceLock)
					{
						if (instance_ == null)
						{
							instance_ = new ZFormActivityLogger();
							System.Threading.Thread.MemoryBarrier();
						}
					}
				}

				return instance_;
			}
		}
		[SuppressThreadStaticFieldMessage]
#if DEBUG
		internal
#endif
		static ZFormActivityLogger instance_;
		[SuppressThreadStaticFieldMessage]
#if DEBUG
		internal
#endif
		static object instanceLock = new object();

		protected ZFormActivityLogger()
		{
		}

		#region Enable

		public void EnableActivityLogger()
		{
			if (!isEnabled && !EnvProxy.Instance.IsWeb)
			{
				SetSaveTimerInterval();
				SaveTimer.Tick += new EventHandler(saveTimer_Tick);
				SaveTimer.Enabled = true;

				externalProcessTimer = new ProxyWindowsTimer();
				externalProcessTimer.Interval = (int)new TimeSpan(0, 0, MaxExternalProcessPollTimerSeconds).TotalMilliseconds;
				externalProcessTimer.Tick += new EventHandler(externalProcessTimer_Tick);
				externalProcessTimer.Enabled = true;

				isEnabled = true;
			}
		}

		public void DisableActivityLogger()
		{
			if (isEnabled && !EnvProxy.Instance.IsWeb)
			{
				ExternalActivityLogger.Stop();
				SaveTimer.Enabled = false;
				saveTimer.Dispose();
				saveTimer = null;
				externalProcessTimer.Enabled = false;
				isEnabled = false;
			}
		}

		public bool IsEnabled
		{
			get { return isEnabled; }
		}

		bool isEnabled;

		#endregion

		#region External Process Tracking

		void externalProcessTimer_Tick(object sender, EventArgs e)
		{
			externalProcessTimer.Enabled = false;
			try
			{
				if (Db.DatabaseUpgradedExceptionHasBeenThrownInConnection)
				{
					return;
				}

				if (EnvProxy.Instance.Registry.UserEventTrackingExternal)
				{
					if (!ExternalActivityLogger.Started)
					{
						ExternalActivityLogger.Start();
					}
				}
				else
				{
					if (ExternalActivityLogger.Started)
					{
						ExternalActivityLogger.Stop();
					}
				}

				IActivityInfo activityInfo;
				while ((activityInfo = ExternalActivityLogger.DequeueActivityInfo()) != null && ShouldAddActivityLogForExternalProcess(activityInfo))
				{
					CreateExternalProcessLog(activityInfo);
				}
			}
			finally
			{
				externalProcessTimer.Enabled = true;
			}
		}

		void CreateExternalProcessLog(IActivityInfo activityInfo)
		{
			var stats = new FormUserStatistics();
			stats.IsExternalProcess = true;
			stats.NotifyFormShownUtc(activityInfo.MainWindowText, activityInfo.ProcessName, activityInfo.StartTimeUtc);
			stats.NotifyFormClosedUtc(activityInfo.EndTimeUtc, activityInfo.InactiveTime, activityInfo.ControlChanges, activityInfo.MouseClicks, activityInfo.KeyStrokes);
			StatLogs.Add(stats);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Process name prefix")]
		bool ShouldAddActivityLogForExternalProcess(IActivityInfo activityInfo)
		{
			// rdpinit is a software component of Microsoft Windows, specifically part of the Remote Desktop Protocol (RDP) system.
			if (activityInfo.ProcessName != null && activityInfo.ProcessName.StartsWith("rdpinit", StringComparison.OrdinalIgnoreCase) &&
				activityInfo.MainWindowText != null && activityInfo.MainWindowText.StartsWith("RemoteApp Marker Window", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}

#if DEBUG
		internal
#endif
		IWindowsTimer externalProcessTimer;

		public IExternalActivityLogger ExternalActivityLogger
		{
			get
			{
				if (externalActivityLogger == null)
				{
					externalActivityLogger = ObjectFactory.Get<IExternalActivityLogger>();
				}
				return externalActivityLogger;
			}
		}

		IExternalActivityLogger externalActivityLogger;

		#endregion

		#region Saving

		public readonly List<FormUserStatistics> StatLogs = new List<FormUserStatistics>();

		public void SavePendingLogs(bool forceNotClosedFormsToBeWritten)
		{
			StatLogs.RemoveAll(log => log == null);
			if (StatLogs.Count > 0)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("SavePendingLogs"))
				{
					var logsToRemoveAfterSuccessfulSave = new List<FormUserStatistics>();
					var factory = new BusinessObjectFactory();

					foreach (var log in StatLogs.ToArray())
					{
						if (LogIsNotEmpty(log) && (log.IsClosed || forceNotClosedFormsToBeWritten))
						{
							if (!log.IsClosed)
							{
								log.NotifyFormClosed(Guid.Empty, string.Empty);
							}

							var activityLog = factory.New<StmActivityLog>();
							activityLog.PopulateFromStatistics(log);
							logsToRemoveAfterSuccessfulSave.Add(log);
						}
						else if (!LogIsNotEmpty(log))
						{
							StatLogs.Remove(log);
						}
					}
					try
					{
						SaveWrittenLogs(factory);
						foreach (var log in logsToRemoveAfterSuccessfulSave)
						{
							StatLogs.Remove(log);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("ZFormActivityLogger_Save", "Unable to save Activity Logs", ex);
					}
				}
			}
		}

		bool LogIsNotEmpty(FormUserStatistics log)
		{
			if ((log.FormCaption.IsNullOrEmpty() && log.ModuleName.IsNullOrEmpty() && log.BusinessObjectTableCode.IsNullOrEmpty()) || log.ShownDateTimeUtc == DateTime.MinValue)
			{
				return false;
			}

			return true;
		}

		protected virtual void SaveWrittenLogs(BusinessObjectFactory factory)
		{
#if DEBUG
			if (!Globals.IsTest || ZFormActivityLogger.AllowActivityLogSavesInTests)
#endif
			{
				factory.Save();
			}
		}

#if DEBUG
		public static bool AllowActivityLogSavesInTests;
#endif

		protected virtual void RemoveLog(FormUserStatistics log)
		{
			StatLogs.Remove(log);
		}
#if DEBUG
		internal
#endif
		void SetSaveTimerInterval()
		{
			SaveTimer.Interval = DataRegistry.Instance.InternalApplicationActivityTrackingInterval * 1000;
		}

#if DEBUG
		internal
#endif
		IWindowsTimer SaveTimer => saveTimer ?? (saveTimer = new ProxyWindowsTimer());
#if DEBUG
		internal
#endif
		IWindowsTimer saveTimer;

#if DEBUG
		internal
#else
		protected
#endif
		virtual int MaxExternalProcessPollTimerSeconds
		{
			get { return 30; } // 30 seconds
		}

		void saveTimer_Tick(object sender, EventArgs e)
		{
			SaveTimer.Enabled = false;
			try
			{
				SavePendingLogs(false);
				SetSaveTimerInterval();
			}
			finally
			{
				SaveTimer.Enabled = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				saveTimer.Dispose();
				externalProcessTimer.Dispose();
			}
		}
#endregion
	}
}
