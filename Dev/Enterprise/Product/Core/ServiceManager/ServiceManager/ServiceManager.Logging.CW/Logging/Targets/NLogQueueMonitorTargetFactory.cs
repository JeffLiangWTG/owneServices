using System;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;

namespace ServiceManager.Logging.CW
{
	class NLogQueueMonitorTargetFactory : INLogTargetFactory
	{
		readonly IHostRegistrySettings hostRegistry;

		public NLogQueueMonitorTargetFactory(IHostRegistrySettings hostRegistry)
		{
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
		}

		public Target GetOrCreateTarget()
		{
			LogManager.Configuration ??= new LoggingConfiguration();

			var target = LogManager.Configuration.FindTargetByName<FileTarget>(TargetName);
			if (target != null)
			{
				return target;
			}

			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var path = CommonProgramData.GetCargoWiseDirectory("Process Controller", "Queue Monitoring", string.Empty);

			target = new FileTarget
			{
				Name = TargetName,
				FileName = Path.Combine(path, $"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}_${{date:format=yyyyMMdd}}.log"),
				Layout = new JsonLayout
				{
					IncludeEventProperties = true,
					Attributes =
					{
						new JsonAttribute("eventTime", "${date:universalTime=true:format=yyyy-MM-dd\\THH\\:mm\\:ss.fffK}"),
						new JsonAttribute("wisecloud.installation.code", $"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}"),
					},
				},
				KeepFileOpen = true,
				OpenFileCacheTimeout = 5,
				OpenFileCacheSize = 1,
				ConcurrentWrites = true,
				ConcurrentWriteAttemptDelay = 15,
				MaxArchiveFiles = hostRegistry.ProcessControllerQueueMonitoringRetentionPeriodInDays,
			};

			return target;
		}

		public const string TargetName = nameof(IQueueMonitorLogger);
	}
}
