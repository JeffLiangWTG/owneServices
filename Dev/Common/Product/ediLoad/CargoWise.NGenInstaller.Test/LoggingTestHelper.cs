using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.NGenInstaller.Testing
{
	class LoggingTestHelper(Product product, string environment) : IDisposable
	{
		public void CleanUpLogs()
		{
			var logFile = GetLogFilename();
			if (File.Exists(logFile))
			{
				File.Delete(logFile);
			}
		}

		public IEnumerable<string> GetLogEntries()
		{
			return File
				.ReadLines(GetLogFilename())
				.ToArray();
		}

		public string GetLogFilename()
		{
			return @$"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\WiseTech Global\ApplicationLogging\{product}\{environment}-{DateTime.UtcNow:yyyyMMdd}.log";
		}

		public void Dispose()
		{
			CleanUpLogs();
		}
	}
}
