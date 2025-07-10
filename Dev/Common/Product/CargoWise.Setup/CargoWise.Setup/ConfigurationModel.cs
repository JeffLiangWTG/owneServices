namespace CargoWise.Setup;

public class ConfigurationModel
{
	public InstallType InstallType { get; set; }
	public required IReadOnlyCollection<string> Components { get; set; }
}

class PartialConfigurationModel
{
	public InstallType? InstallType { get; set; }
	public IReadOnlyCollection<string>? Components { get; set; }
	public string? ConfigFile { get; set; }
}

public enum InstallType
{
	Install,
	Uninstall,
}
