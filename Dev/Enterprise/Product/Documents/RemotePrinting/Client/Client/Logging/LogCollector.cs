using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.IO;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public class LogCollector
	{
		public LogCollector(string logFilesPath)
		{
			this.logFilesPath = logFilesPath;
		}

		readonly string logFilesPath;

		public byte[] GetCompressedLogData(LogTypes logTypes, DateTime startDate, DateTime endDate)
		{
			byte[] bytes;

			var zipFilePath = GetCompressedLogFile(logTypes, startDate, endDate);
			try
			{
				bytes = File.ReadAllBytes(zipFilePath);
			}
			finally
			{
				DeleteTempLogFiles(zipFilePath, 3, false);
			}

			return bytes;
		}

		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "WebPrint Client has no access to ZDateTime")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
		public string GetCompressedLogFile(LogTypes logTypes, DateTime startDate, DateTime endDate)
		{
			var srcDirectory = new DirectoryInfo(logFilesPath);
			var fileName = Guid.NewGuid().ToString("N") + ".tmp" + ".zip";
			var folderPath = Temp.TempPath;
			var zipPath = Path.Combine(folderPath, fileName);

			using (var zipFile = ZipFile.Open(zipPath, ZipArchiveMode.Create))
			{
				foreach (var log in Enum.GetValues(typeof(LogTypes)))
				{
					if ((LogTypes)log != LogTypes.None && ((LogTypes)log & logTypes) == (LogTypes)log)
					{
						if ((LogTypes)log == LogTypes.WindowsEvent)
						{
							var windowsEventLogFileName = "WindowsEventLogs.txt";
							var tempFileName = Temp.GetTempFileName(folderPath);
							try
							{
								var webPrintWindowsEvents = GetWebPrintWindowsEvents(startDate, endDate);
								if (webPrintWindowsEvents.Count != 0)
								{
									GenerateTxtFromWindowsEvents(webPrintWindowsEvents, folderPath, tempFileName);
									ZipFileExtensions.CreateEntryFromFile(zipFile, Path.Combine(folderPath, tempFileName), windowsEventLogFileName);
								}
							}
							catch (Exception ex)
							{
								var errorMessage = new StringBuilder();
								errorMessage
									.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
									.AppendLine(" Error Type :  ").AppendLine(ex.GetType().ToString())
									.AppendLine(" Error Message: ").AppendLine(ex.Message);

								File.AppendAllText(Path.Combine(folderPath, tempFileName), errorMessage.ToString());
								ZipFileExtensions.CreateEntryFromFile(zipFile, Path.Combine(folderPath, tempFileName), windowsEventLogFileName);
							}
							finally
							{
								DeleteTempLogFiles(Path.Combine(folderPath, tempFileName), 3, false);
							}
						}
						else
						{
							var pattern = log + "_*.txt";

							AddDirectoryToZip(zipFile, srcDirectory, pattern, startDate, endDate);
						}
					}
				}
			}

			return zipPath;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
#if DEBUG
		protected virtual
#endif
		List<EventRecord> GetWebPrintWindowsEvents(DateTime startDate, DateTime endDate)
		{
			string dateFormat = "yyyy-MM-ddTHH:mm:ssZ";
			string startDateString = startDate.ToUniversalTime().ToString(dateFormat);
			string endDateString = endDate.ToUniversalTime().ToString(dateFormat);
			string source = "Application";
			string query = "*[System[TimeCreated[(@SystemTime <= '" + endDateString + "') and (@SystemTime >= '" + startDateString + "')]]]";
			EventLogQuery eventsQuery = new EventLogQuery(source, PathType.LogName, query);
			EventLogReader logReader = new EventLogReader(eventsQuery);
			List<EventRecord> webPrintWindowsEvents = new List<EventRecord>();
			for (EventRecord eventDetail = logReader.ReadEvent(); eventDetail != null; eventDetail = logReader.ReadEvent())
			{
				if ((eventDetail.ProviderName.Equals(".NET Runtime", StringComparison.OrdinalIgnoreCase) ||
					eventDetail.ProviderName.IndexOf("Remote Printing", StringComparison.OrdinalIgnoreCase) >= 0 ||
					eventDetail.ProviderName.IndexOf("RemotePrinting", StringComparison.OrdinalIgnoreCase) >= 0 ||
					eventDetail.ProviderName.IndexOf("WebPrint", StringComparison.OrdinalIgnoreCase) >= 0))
				{
					webPrintWindowsEvents.Add(eventDetail);
				}
			}

			return webPrintWindowsEvents;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
		public void GenerateTxtFromWindowsEvents(List<EventRecord> windowsEvents, string destinationFilePath, string destinationFileName)
		{
			var records = new List<string>();
			foreach (EventRecord eventRecord in windowsEvents)
			{
				var message = new StringBuilder();
				message
					.Append(eventRecord.TimeCreated?.ToString("yyyy-MM-dd HH:mm:ss"))
					.Append(" - ").Append(eventRecord.ProviderName)
					.Append(": ").Append(eventRecord.FormatDescription())
					.AppendLine();

				records.Add(message.ToString());
			}
			File.WriteAllLines(Path.Combine(destinationFilePath, destinationFileName), records);
		}

		public static void AddDirectoryToZip(ZipArchive archive, DirectoryInfo sourceDir, string pattern, DateTime startDate, DateTime endDate, string entryName = "")
		{
			var files = sourceDir.GetFiles(pattern).Where(file => file.LastWriteTime >= startDate && file.LastWriteTime < endDate).ToList();

			foreach (var file in files)
			{
				AddFileToZip(archive, file, entryName);
			}
			foreach (var dir in Directory.GetDirectories(sourceDir.FullName))
			{
				var dirInfo = new DirectoryInfo(dir);
				AddDirectoryToZip(archive, dirInfo, pattern, startDate, endDate, dirInfo.Name);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule", Justification = "WebPrint Client has no access to ZDateTime")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
		public static void AddFileToZip(ZipArchive archive, FileInfo file, string entryName = "")
		{
			const int MaxRetries = 3;

			for (var attempt = 0; attempt < MaxRetries; attempt++)
			{
				try
				{
					archive.CreateEntryFromFile(file.FullName, Path.Combine(entryName, file.Name), CompressionLevel.Fastest);
					break;
				}
				catch (Exception ex)
				{
					if (attempt == (MaxRetries - 1))
					{
						var errorLogFile = "Error-" + file.Name + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
						var errorMessage = new StringBuilder();
						errorMessage
							.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
							.AppendLine(" Error Type :  ").AppendLine(ex.GetType().ToString())
							.AppendLine(" Error Message: ").AppendLine(ex.Message);
						archive.CreateEntryFromFile(file.FullName, errorLogFile, CompressionLevel.Fastest);
					}
					else
					{
						Thread.Sleep(10);
					}
				}
			}
		}

		public void DeleteTempLogFiles(string path, int retries, bool isFolder)
		{
			try
			{
				if (isFolder)
				{
					if (Directory.Exists(path))
					{
						Directory.Delete(path, true);
					}
				}
				else
				{
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
			}
			catch (IOException)
			{
				retries--;
				if (retries > 0)
				{
					Thread.Sleep(100);
					DeleteTempLogFiles(path, retries, isFolder);
				}
			}
		}
	}
}
