using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ServiceManager.Shared;

namespace Enterprise.Client.EDI.Mail.Business
{
	public static class ReportProcessorHelper
	{
		public static void SaveReport(ILogger serviceLogger, string folder, string enterpriseCode, string serverCode, string xmlData)
		{
			string fileName = enterpriseCode + '-' + serverCode + ".xml";
			SaveReport(serviceLogger, folder, fileName, xmlData);
		}

		public static void SaveReport(ILogger serviceLogger, string folder, string fileName, string text)
		{
			try
			{
				string path = GetReportDirectoryPath(folder);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				File.WriteAllText(Path.Combine(path, fileName), text);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				serviceLogger?.Log(LogType.Error, "Couldn't save file " + fileName, ex);
				ErrorReporter.ReportOnce("Couldn't save file", ex.Message, ex);
			}
		}

		public static string GetReportDirectoryPath(string folder)
		{
#if DEBUG
			if (reportDirectoryForTesting != null)
			{
				return Path.Combine(reportDirectoryForTesting, folder);
			}
#endif
			return Path.Combine(ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName), folder);
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static string reportDirectoryForTesting = Env.TempPath;

		public static void ClearReportsFromTesting(string folder)
		{
			string path = GetReportDirectoryPath(folder);
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}
#endif
	}
}


