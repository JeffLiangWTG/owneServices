using NLog;
using NLog.Config;
using NLog.Targets;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

class LoggerConfigurationChecker : IEnvironmentChecker
{
	public void Initialize(IRunCommandInfo runCommandInfo)
	{
		initialLoggerConfigurationSnapshot = new LoggerConfigurationSnapshot();
	}

	public void CheckOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler)
	{
		initialLoggerConfigurationSnapshot!.VerifyCurrentConfiguration(serviceTaskHandler.HostedServiceAttribute);
	}

	public void CheckOnServiceTaskException(IServiceTaskHandler serviceTaskHandler) { }

	class LoggerConfigurationSnapshot : ILoggerConfigurationSnapshot
	{
		public LoggerConfigurationSnapshot()
		{
			originalRules = LogManager.Configuration.LoggingRules.ToArray();
			originalTargets = LogManager.Configuration.AllTargets.ToArray();
		}

		public void VerifyCurrentConfiguration(IHostedServiceAttribute hostedServiceAttribute)
		{
			var currentConfiguration = LogManager.Configuration;
			var currentRules = currentConfiguration.LoggingRules.ToArray();
			var currentTargets = currentConfiguration.AllTargets.ToArray();

			var addedRules = currentRules.Except(originalRules).ToList();
			var removedRules = originalRules.Except(currentRules).ToList();
			var addedTargets = currentTargets.Except(originalTargets).ToList();
			var removedTargets = originalTargets.Except(currentTargets).ToList();

			var violationsList = new List<string>();
			if (addedRules.Count > 0)
			{
				violationsList.Add($"Logging rules added: [{string.Join(", ", addedRules.Select(r => r.RuleName).ToArray())}]");
			}
			if (removedRules.Count > 0)
			{
				violationsList.Add($"Logging rules removed: [{string.Join(", ", removedRules.Select(r => r.RuleName).ToArray())}]");
			}
			if (addedTargets.Count > 0)
			{
				violationsList.Add($"Logging targets added: [{string.Join(", ", addedTargets.Select(t => t.Name).ToArray())}]");
			}
			if (removedTargets.Count > 0)
			{
				violationsList.Add($"Logging targets removed: [{string.Join(", ", removedTargets.Select(t => t.Name).ToArray())}]");
			}

			var violations = string.Join(System.Environment.NewLine, violationsList);
			if (violationsList.Count != 0)
			{
				throw new LoggerConfigurationCorruptedException(hostedServiceAttribute, violations);
			}
		}

		readonly IEnumerable<LoggingRule> originalRules;
		readonly IEnumerable<Target> originalTargets;
	}

	ILoggerConfigurationSnapshot? initialLoggerConfigurationSnapshot;
}
