namespace CargoWise.Setup.Test.Helpers;

internal static class PartialConfigurationModelExtensions
{
	public static ConfigurationModel WithDefaults(this PartialConfigurationModel model)
	{
		var defaultsForTest = new PartialConfigurationModel
		{
			InstallType = InstallType.Install,
			Components = [],
		};
		return ApplicationArguments.FromPartialModelsInPriorityOrder(model, new ConfigurationDefaultsProvider().GetDefaults(), defaultsForTest);
	}
}
