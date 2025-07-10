using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Host
{
	class LogFilesRequestHandler : RequestHandler
	{
		public LogFilesRequestHandler(string hostName, string dbServer, string dbName)
		{
			Uri = ServiceManagerHelper.GetLogFilesUri(hostName);
			this.dbServer = dbServer;
			this.dbName = dbName;
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			var relativeUri = request.Uri.PathAndQuery.Remove(0, Uri.PathAndQuery.Length + 1);

			if (string.IsNullOrEmpty(relativeUri) || relativeUri.StartsWith("..", StringComparison.OrdinalIgnoreCase))
			{
				return GetDirectoryListing("");
			}

			if (relativeUri.Length >= 1 && relativeUri.Length <= 3 &&
				Array.TrueForAll(relativeUri.ToCharArray(), char.IsLetterOrDigit)
				||
				0 ==
				string.Compare(relativeUri, ServiceManagerHelper.HostLoggerCode,
					StringComparison.OrdinalIgnoreCase))
			{
				return GetDirectoryListing(relativeUri);
			}

			var logFileName = Path.Combine(ServiceManagerHelper.GetLogFilesDirectory(dbServer, dbName), relativeUri);
			if (File.Exists(logFileName))
			{
				FileStream stream = null;
				try
				{
					stream = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					using (var sr = new StreamReader(stream))
					{
						stream = null; // https://msdn.microsoft.com/library/ms182334.aspx
						return sr.ReadToEnd().TrimEnd();
					}
				}
				finally
				{
					stream?.Dispose();
				}
			}

			return null;
		}

		string GetDirectoryListing(string taskCode)
		{
			var logDirectoryName = ServiceManagerHelper.GetLogFilesDirectory(dbServer, dbName);
			var sb = new StringBuilder();
			if (Directory.Exists(logDirectoryName))
			{
				var searchPattern = taskCode + "*" + Logger.LogFileExtension;
				var regexPattern = taskCode + ".*\\" + Logger.LogFileExtension;
				foreach (var file in Directory.GetFiles(logDirectoryName, searchPattern, SearchOption.TopDirectoryOnly))
				{
					if (Regex.IsMatch(file, regexPattern, RegexOptions.IgnoreCase))
					{
						sb.AppendLine(Path.GetFileName(file));
					}
				}
			}

			return sb.ToString();
		}

		readonly string dbName;
		readonly string dbServer;
	}
}
