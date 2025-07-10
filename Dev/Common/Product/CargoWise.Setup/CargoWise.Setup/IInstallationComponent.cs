namespace CargoWise.Setup;

public interface IInstallationComponent
{
	Task Install(ConfigurationModel config, CancellationToken token);
	Task Remove(ConfigurationModel config, CancellationToken token);
	string Name { get; }
}
