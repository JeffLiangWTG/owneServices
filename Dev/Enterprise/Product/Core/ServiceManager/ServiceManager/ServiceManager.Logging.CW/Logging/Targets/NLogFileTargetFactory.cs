using System.IO;
using Enterprise.ServiceManager.Shared.Interfaces;
using NLog;
using NLog.Targets;
using ServiceManager.Shared.CW;

namespace ServiceManager.Logging.CW
{
	class NLogFileTargetFactory : INLogTargetFactory
	{
		public NLogFileTargetFactory(string programCode, string path, bool archiveLogFiles)
		{
			this.programCode = programCode;
			this.path = path;
			this.archiveLogFiles = archiveLogFiles;
		}

		public Target? GetOrCreateTarget()
		{
			var registry = (ILoggerRegistrySettings)SharedRegistry.Instance;
			if (!registry.FileSystemLoggingEnabled)
			{
				return null;
			}

			var fileTargetName = GetLogfileTargetName(programCode);

			var existingTarget = LogManager.Configuration.FindTargetByName<FileTarget>(fileTargetName);
			if (existingTarget != null)
			{
				return existingTarget;
			}

			var target = new FileTarget
			{
				Name = fileTargetName,
				FileName = Path.Combine(path, "${logger}_${date:format=yyyyMMdd}.txt"),
				Layout = DefaultLayoutFormatter,
				KeepFileOpen = true,
				OpenFileCacheTimeout = archiveLogFiles
					? 60
					: 5,
				OpenFileCacheSize = 3,
				ConcurrentWrites = true,
				ConcurrentWriteAttemptDelay = 5,
				ArchiveAboveSize = archiveLogFiles
					? 1024 * 1024 * 10
					: -1,
				ArchiveFileName = Path.Combine(path, "${logger}_{#}.txt"),
				ArchiveDateFormat = "yyyyMMdd.HHmmss-fff",
				ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
			};

			return target;
		}

		internal static string GetLogfileTargetName(string programCode)
		{
			return LogfileTargetNamePrefix + programCode;
		}

		const string DefaultLayoutFormatter = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff:padding=-25}${event-properties:severity:padding=-25}${event-properties:process:objectpath=pid:padding=-25}${when:when='${scopeproperty:exe}' == 'Runner':inner=PID=${event-properties:process:objectpath=pid} *FROM RUNNER*\\: ${message}:else=${message}}";
		const string LogfileTargetNamePrefix = "logfile";

		readonly string programCode;
		readonly string path;
		readonly bool archiveLogFiles;
	}
}
