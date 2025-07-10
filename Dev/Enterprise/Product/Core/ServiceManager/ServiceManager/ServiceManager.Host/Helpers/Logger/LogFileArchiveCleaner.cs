using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Host
{
	public interface ILogFileArchiveCleaner
	{
		void PerformCleaning();
	}

	public class LogFileArchiveCleaner : ILogFileArchiveCleaner
	{
		readonly IHostLogger logger;

		public LogFileArchiveCleaner(IHostLogger logger)
		{
			this.logger = logger;
		}

		IExtendedLifetimeServiceTasksLookup extendedLifetimeLookup;
		IExtendedLifetimeServiceTasksLookup ExtendedLifetimeLookup
		{
			get
			{
				if (extendedLifetimeLookup == null)
				{
					extendedLifetimeLookup = new ExtendedLifetimeServiceTasksLookup();
				}

				return extendedLifetimeLookup;
			}
		}

		string GetLogFilesDirectory()
		{
			return ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
		}

		public void PerformCleaning()
		{
			var numDeleted = 0;
			var logsByCodeAndDate = GetLogFiles(GetLogFilesDirectory());
			foreach (var entry in logsByCodeAndDate)
			{
				var numOfLogsToRemove = entry.Value.Count - ExtendedLifetimeLookup.GetNumberOfLogFilesToPreserve(entry.Key);
				foreach (var fileNames in entry.Value.Values)
				{
					if ((numOfLogsToRemove--) > 0)
					{
						foreach (var fileName in fileNames)
						{
							try
							{
								File.Delete(fileName);
								numDeleted++;
							}
							catch (Exception ex) when (ex is ArgumentException || ex is DirectoryNotFoundException || ex is IOException || ex is NotSupportedException || ex is PathTooLongException || ex is UnauthorizedAccessException)
							{
								logger.Log(LogLevel.Error, string.Format(CultureInfo.InvariantCulture, "Unable to clean up old log file: {0}", fileName), ex);
							}
						}
					}
					else
					{
						break;
					}
				}
			}
		}

		Dictionary<string, SortedDictionary<DateTime, List<string>>> GetLogFiles(string logDirectoryName)
		{
			var result = new Dictionary<string, SortedDictionary<DateTime, List<string>>>();
			if (Directory.Exists(logDirectoryName))
			{
				var rx = new Regex(@"(\w{1,4})_(\d{8}).*\" + Logger.LogFileExtension + "$", RegexOptions.IgnoreCase);
				foreach (var file in Directory.GetFiles(logDirectoryName))
				{
					var m = rx.Match(file);
					if (m.Success)
					{
						DateTime date;
						if (DateTime.TryParseExact(m.Groups[2].Value, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
						{
							var taskCode = m.Groups[1].Value;
							SortedDictionary<DateTime, List<string>> taskLogsByDate;
							if (!result.TryGetValue(taskCode, out taskLogsByDate))
							{
								taskLogsByDate = new SortedDictionary<DateTime, List<string>>();
								result.Add(taskCode, taskLogsByDate);
							}

							if (taskLogsByDate.ContainsKey(date))
							{
								taskLogsByDate[date].Add(file);
							}
							else
							{
								taskLogsByDate.Add(date, new List<string>() { file });
							}
						}
					}
				}
			}

			return result;
		}
	}
}
