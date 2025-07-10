using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared.Interfaces;
using NLog;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace ServiceManager.Logging.CW
{
	class LoggerNLogRegistryConfiguration : ILoggerNLogConfiguration
	{
		public LoggerNLogRegistryConfiguration()
		{
			serviceTaskCodes = new Lazy<HashSet<string>>(() => Enumerable.Select(((HostedServiceAttributeProvider)ObjectFactory.Get<IClientHostedServiceAttributeProvider>()).GetHostedServices(), codeDescriptionPair => codeDescriptionPair.Code).ToHashSet());
		}

		public VerboseLoggingCollection VerboseLogging => ((ILoggerRegistrySettings)SharedRegistry.Instance).ProcessControllerVerboseLogging;

		public bool InternalNLogLoggingEnabled => ((ILoggerRegistrySettings)SharedRegistry.Instance).ProcessControllerNLogInternalLoggingEnabled;

		public void Refresh()
		{
			if (!IsTimeUpdate())
			{
				return;
			}

			if (Interlocked.Exchange(ref oneCheckAtATimeLock, 1) == 1)
			{
				return;
			}

			try
			{
				RefreshThreadSafe(VerboseLogging, serviceTaskCodes.Value);
			}
			finally
			{
				Interlocked.Exchange(ref oneCheckAtATimeLock, 0);
			}

			bool IsTimeUpdate()
			{
				if (stopwatch.IsRunning
					&& stopwatch.Elapsed <= timeout)
				{
					return false;
				}

				stopwatch.Restart();
				return true;
			}
		}

		static void RefreshThreadSafe(VerboseLoggingCollection verboseLoggingCollection, HashSet<string> serviceTaskCodes)
		{
			var values = verboseLoggingCollection
				.Cast<VerboseLoggingBusinessObject>()
				.ToDictionary(o => o.Code, o => o);
			var rules = LogManager.Configuration.LoggingRules.ToList();
			foreach (var rule in rules)
			{
				if (values
						.FirstOrDefault(valueTuple => rule.RuleName.Equals(valueTuple.Key, StringComparison.OrdinalIgnoreCase))
						.Value
						?.VerboseLogging ?? false)
				{
					rule.EnableLoggingForLevel(LogLevel.Debug);
				}
				else
				{
					if (rule.RuleName.Equals("HOST", StringComparison.Ordinal) || serviceTaskCodes.Contains(rule.RuleName))
					{
						rule.DisableLoggingForLevel(LogLevel.Debug);
					}
				}
			}

			LogManager.ReconfigExistingLoggers();
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static int oneCheckAtATimeLock;

		readonly Stopwatch stopwatch = new Stopwatch();
		readonly TimeSpan timeout = TimeSpan.FromMinutes(5);
		readonly Lazy<HashSet<string>> serviceTaskCodes;
	}
}
