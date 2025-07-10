using System;
using System.Globalization;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client
{
	public class LogFileManager
	{
		public static LogFileManager Instance
		{
			get
			{
				if (instance == null)
				{
					lock (locker)
					{
						instance = instance ?? new LogFileManager();
					}
				}
				return instance;
			}

			set
			{
				lock (locker)
				{
					instance = value;
				}
			}
		}
		static readonly object locker = new object();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Static fields in this class do not need to be thread-static")]
		static LogFileManager instance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "onShowInformation")]
		public void CleanOldLogFiles(int daysToKeep, Action<string> onShowInformation)
		{
			if (!OutputDirectory.Exists)
			{
				return;
			}

			if (inCleanOldLogFiles)
			{
				return;
			}
			inCleanOldLogFiles = true;

			try
			{
				var oldestDateToKeep = DateTime.Now.AddDays(-daysToKeep);

				var filesToDelete = OutputDirectory.GetFiles("*", SearchOption.AllDirectories);

				var deletedFilesCount = 0;

				try
				{
					foreach (var fileInfo in filesToDelete)
					{
						try
						{
							if (fileInfo.LastWriteTime.Date < oldestDateToKeep.Date)
							{
								fileInfo.Delete();
								deletedFilesCount++;
							}
						}
						catch (IOException) // Process for individual files
						{
							// File is open or locked in other way - skip
						}
					}
				}
				catch (SecurityException ex) // Process once for whole directory
				{
					onShowInformation?.Invoke("Was not able to clean old log files due to restricted permission: " + ex.Message);
				}

				if (deletedFilesCount > 0)
				{
					onShowInformation?.Invoke($"Cleaned {deletedFilesCount} log files older than {daysToKeep} days.");
				}

				if (deletedFilesCount == 0)
				{
					onShowInformation?.Invoke($"No log files older than {daysToKeep} days.");
				}
			}
			finally
			{
				inCleanOldLogFiles = false;
			}
		}

		#region LogSetting

		public void SaveLogConfiguration(WebClientLogConfiguration logConfiguration)
		{
			var key = ConnectionRegistryManager.Instance.FindKey(LogSettings);
			key.SetValue(ShouldClearOldLog, logConfiguration.ShouldClearOldLog, RegistryValueKind.DWord);
			key.SetValue(DayToKeepOldLogFile, logConfiguration.DayToKeepOldLogFile, RegistryValueKind.String);
		}

		const string DayToKeepOldLogFile = "DayToKeepOldLogFile";
		const string ShouldClearOldLog = "ShouldClearOldLog";
		const int DefaultDayToKeepOldLogFile = 14;

		public WebClientLogConfiguration GetLogConfiguration()
		{
			using (var key = ConnectionRegistryManager.Instance.FindKey(LogSettings))
			{
				return new WebClientLogConfiguration(ConnectionRegistryManager.Instance.GetIntRegistryItem(DayToKeepOldLogFile, DefaultDayToKeepOldLogFile, key),
					Convert.ToBoolean(key.GetValue(ShouldClearOldLog, true), CultureInfo.InvariantCulture));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogSettings")]
		protected virtual string LogSettings => "SOFTWARE\\" + Constants.RegistryManager.CargoWiseKeyName + "\\" + Constants.RegistryManager.WebPrintLogSettings;

		#endregion

		protected virtual DirectoryInfo OutputDirectory => LogWriter.GetOutputDirectory();

		bool inCleanOldLogFiles;
	}
}
