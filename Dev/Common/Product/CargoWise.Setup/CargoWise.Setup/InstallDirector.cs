using Microsoft.Extensions.Logging;

namespace CargoWise.Setup;

public interface IInstallDirector
{
	Task Run(ConfigurationModel config, CancellationToken token);
}

class InstallDirector(IEnumerable<IInstallationComponent> components, ILogger<IInstallDirector> logger) : IInstallDirector
{
	public async Task Run(ConfigurationModel config, CancellationToken token)
	{
		logger.LogInformation($"Starting {config.InstallType.ToString()}");
		if (config.InstallType == InstallType.Install)
		{
			await Install(config, token);
		}
		else
		{
			await Uninstall(config, token);
		}
		logger.LogInformation($"Finished {config.InstallType.ToString()}");
	}

	Task Install(ConfigurationModel config, CancellationToken token) =>
		Execute(components, (c, t) => c.Install(config, t), config.Components, token);

	Task Uninstall(ConfigurationModel config, CancellationToken token) =>
		Execute(components.Reverse(), (c, t) => c.Remove(config, t), config.Components, token);

	async Task Execute(IEnumerable<IInstallationComponent> allComponents, Func<IInstallationComponent, CancellationToken, Task> executor, IEnumerable<string> selectedComponentNames, CancellationToken token)
	{
		foreach (var component in allComponents)
		{
			if(token.IsCancellationRequested)
			{
				logger.LogInformation("Cancellation requested");
				break;
			}

			if (selectedComponentNames.Contains(component.Name, StringComparer.InvariantCultureIgnoreCase))
			{
				logger.LogInformation($"Starting component: {component.Name}");
				await executor(component, token);
				logger.LogInformation($"Finished component: {component.Name}");
			}
		}
	}
}
