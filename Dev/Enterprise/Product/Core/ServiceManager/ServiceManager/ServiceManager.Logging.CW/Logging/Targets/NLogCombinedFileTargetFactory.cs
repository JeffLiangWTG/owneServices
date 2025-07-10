using System.IO;
using CargoWise.Common;
using Enterprise.ServiceManager.Shared.Interfaces;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using ServiceManager.Shared.CW;

namespace ServiceManager.Logging.CW
{
	class NLogCombinedFileTargetFactory : INLogTargetFactory
	{
		public NLogCombinedFileTargetFactory(string installationCode)
		{
			this.installationCode = installationCode;
		}

		public Target? GetOrCreateTarget()
		{
			var registry = (ILoggerRegistrySettings)SharedRegistry.Instance;
			if (!registry.CombinedFileSystemLoggingEnabled)
			{
				return null;
			}

			const string targetName = "combinedFile";

			var existingTarget = LogManager.Configuration.FindTargetByName<FileTarget>(targetName);
			if (existingTarget != null)
			{
				return existingTarget;
			}

			var path = CommonProgramData.GetCargoWiseDirectory("Process Controller", string.Empty, string.Empty);
			var fileTarget = new FileTarget
			{
				Name = targetName,
				FileName = Path.Combine(path, $"{installationCode}_${{date:format=yyyyMMdd}}.log"),
				Layout = new JsonLayout
				{
					IncludeEventProperties = true,
					IncludeScopeProperties = true,
					Attributes =
					{
						new JsonAttribute("eventTime", "${date:universalTime=true:format=yyyy-MM-dd\\THH\\:mm\\:ss.fffK}"),
						new JsonAttribute("message", "${message}"),
					},
				},
				KeepFileOpen = true,
				OpenFileCacheTimeout = 5,
				OpenFileCacheSize = 1,
				ConcurrentWrites = true,
				ConcurrentWriteAttemptDelay = 15,
				MaxArchiveFiles = registry.ProcessControllerCombinedFileRetentionPeriod,
			};

			return fileTarget;
		}

		readonly string installationCode;
	}
}
