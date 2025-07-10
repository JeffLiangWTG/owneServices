namespace CargoWise.Setup;

interface IConfigurationDefaultsProvider
{
	PartialConfigurationModel GetDefaults();
}

internal class ConfigurationDefaultsProvider : IConfigurationDefaultsProvider
{
	public PartialConfigurationModel GetDefaults()
	{
		return new PartialConfigurationModel()
		{
		};
	}
}
