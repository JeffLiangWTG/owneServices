using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Development.Common
{
	public class BiFileLogger
	{
		#region Singleton Pattern

		BiFileLogger()
		{
			RegisterEvents();
		}

		public static void Initialize()
		{
			if (instance == null)
			{
				instance = new BiFileLogger();
			}
		}

		public static BiFileLogger Instance
		{
			get { return instance ?? (instance = new BiFileLogger()); }
		}
		[ThreadSafe]
		static BiFileLogger instance;

		#endregion

		readonly object lockObj = new object();

		public static void OpenLogFile()
		{
			FileOpener.Open(Instance.LogFileName);
		}

		void RegisterEvents()
		{
#if DEBUG
			if (!Globals.IsTest)
			{
				BiLogger.Instance.OnStartTask += new BiLoggerEvent(Log);
				BiLogger.Instance.OnStartSubtask += new BiLoggerEvent(Log);
				BiLogger.Instance.OnCompleted += new BiLoggerEvent(Log);
				BiLogger.Instance.OnFailed += new BiLoggerEvent(Log);
			}
#endif
		}

		void Log(string message, DateTime time)
		{
			lock (lockObj)
			{
				if (!string.IsNullOrEmpty(message))
				{
					var line = new string[] { string.Format(CultureInfo.InvariantCulture, "{0} : {1}", time.ToLongTimeString(), message) };
					File.AppendAllLines(LogFileName, line);
				}
			}
		}

		string logFilePath;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "development tool only")]
		public string LogFilePath
		{
			get
			{
				if (logFilePath == null)
				{
					logFilePath = string.Format(CultureInfo.InvariantCulture, @"{0}\BI Automation Logs\", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

					if (!Directory.Exists(logFilePath))
					{
						Directory.CreateDirectory(logFilePath);
					}
					else
					{
						var files = Directory.GetFiles(logFilePath, "*.log");
						foreach (var file in files)
						{
							if (File.GetCreationTime(file).AddDays(14) < DateTime.Now)
							{
								File.Delete(file);
							}
						}
					}
				}
				return logFilePath;
			}
		}

		string logFileName;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "development tool only")]
		public string LogFileName
		{
			get
			{
				if (logFileName == null)
				{
					logFileName = LogFilePath + string.Format(CultureInfo.InvariantCulture, "BiAutomation_{0}.log", DateTime.Now.ToString("yyyyMMdd_Hmmss", CultureInfo.InvariantCulture));
				}
				return logFileName;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "development tool only")]
		string MainServerInstanceFileName
		{
			get
			{
				return mainServerInstanceFileName ?? (mainServerInstanceFileName = Path.Combine(BuildConstants.LocalEnterprisePath, @"Bin", "instance.txt"));
			}
		}
		string mainServerInstanceFileName;

		public void SaveMainServerInstanceFile(string instance)
		{
			Match match = instanceRegex.Match(instance);
			if (match.Success)
			{
				var matchGroup = match.Groups["instanceName"];
				if (matchGroup != null)
				{
					File.WriteAllText(MainServerInstanceFileName, matchGroup.Value);
				}
			}
			else if (File.Exists(MainServerInstanceFileName))
			{
				File.Delete(MainServerInstanceFileName);
			}
		}

		readonly Regex instanceRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"{0}\\(?<instanceName>\b\w+\b)", Environment.MachineName), RegexOptions.IgnoreCase);

		public string LoadMainServerInstanceFile()
		{
			string instance = null;

			if (File.Exists(MainServerInstanceFileName))
			{
				var instanceFromFile = File.ReadAllText(MainServerInstanceFileName);
				if (!string.IsNullOrEmpty(instanceFromFile))
				{
					instance = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", Environment.MachineName, instanceFromFile);
				}
			}
			return instance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "development tool only")]
		public string AnalysisServerInstanceFileName
		{
			get
			{
				return analysisServerInstanceFileName ?? (analysisServerInstanceFileName = Path.Combine(LogFilePath, "analysis server instance.txt"));
			}
		}
		string analysisServerInstanceFileName;

		public void SaveAnalysisServerInstanceFile(string instance)
		{
			if (!string.IsNullOrEmpty(instance))
			{
				File.WriteAllText(AnalysisServerInstanceFileName, instance);
			}
			else if (File.Exists(AnalysisServerInstanceFileName))
			{
				File.Delete(AnalysisServerInstanceFileName);
			}
		}

		public string LoadAnalysisServerInstanceFile()
		{
			string instance = null;
			if (File.Exists(AnalysisServerInstanceFileName))
			{
				instance = File.ReadAllText(AnalysisServerInstanceFileName);
			}
			return instance;
		}
	}
}
