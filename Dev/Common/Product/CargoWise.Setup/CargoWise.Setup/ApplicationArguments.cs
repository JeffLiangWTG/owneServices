using System.Text.Json;
using CargoWise.Setup.InstallComponents;

namespace CargoWise.Setup;

class ApplicationArguments(IConfigurationDefaultsProvider defaultsProvider)
{
	public bool HasHelpArgument(string[] args)
	{
		return args.Any(s =>
			s.Equals("--help", StringComparison.InvariantCultureIgnoreCase) ||
			s.Equals("-h", StringComparison.InvariantCultureIgnoreCase));
	}

	public ConfigurationModel ReadConfigFromArgs(string[] args)
	{
		var commandLineModel = GetCommandLineConfigurations(args);
		commandLineModel.InstallType = (args.Length >= 1 ? args[0].ToLowerInvariant() : null) switch
		{
			"install" => InstallType.Install,
			"uninstall" => InstallType.Uninstall,
			_ => throw new BadUsageException("Must specify install or uninstall")
		};

		var configFileModel = new PartialConfigurationModel();
		if (commandLineModel.ConfigFile != null)
		{
			var jsonText = File.ReadAllText(commandLineModel.ConfigFile);
			configFileModel = JsonSerializer.Deserialize<PartialConfigurationModel>(jsonText) ?? throw new BadUsageException("Invalid config file");
		}

		var defaultsModel = defaultsProvider.GetDefaults();

		var resultConfig = FromPartialModelsInPriorityOrder(commandLineModel, configFileModel, defaultsModel);
		if (commandLineModel.InstallType == InstallType.Install)
		{
			resultConfig.Components = resultConfig.Components.Append(EnvironmentComponent.ComponentName).Distinct().ToList();
		}
		return resultConfig;
	}

	internal static ConfigurationModel FromPartialModelsInPriorityOrder(params PartialConfigurationModel[] partialConfigurationModels)
	{
		var finalConfig = Activator.CreateInstance<ConfigurationModel>();
		foreach (var property in typeof(ConfigurationModel).GetProperties())
		{
			var partialModelProperty = typeof(PartialConfigurationModel).GetProperty(property.Name);
			var values = partialConfigurationModels.Select(partialModel => partialModelProperty?.GetValue(partialModel));
			var value = values.FirstOrDefault(v => v != null);
			var isRequired = Nullable.GetUnderlyingType(property.PropertyType) == null;
			if (isRequired && value == null)
			{
				throw new BadUsageException($"Required argument '{property.Name}' not provided");
			}
			property.SetValue(finalConfig, value);
		}
		return finalConfig;
	}

	static PartialConfigurationModel GetCommandLineConfigurations(string[] args)
	{
		var model = new PartialConfigurationModel();
		var allProperties = typeof(PartialConfigurationModel).GetProperties();
		foreach (var arg in args.Skip(1))
		{
			if (arg.StartsWith("--"))
			{
				var parts = arg.Substring(2).Split('=');
				if (parts.Length != 2)
				{
					throw new BadUsageException($"Invalid argument format '{arg}'");
				}

				var property = allProperties.FirstOrDefault(allProperties => allProperties.Name.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
				if (property == null)
				{
					throw new BadUsageException($"Invalid argument name '{parts[0]}'");
				}
				else if (typeof(IEnumerable<string>).IsAssignableFrom(property.PropertyType))
				{
					var list = parts[1].Split(',');
					property.SetValue(model, list);
				}
				else if (property.PropertyType == typeof(string) && parts[1].StartsWith('"') && parts[1].EndsWith('"'))
				{
					property.SetValue(model, parts[1].Substring(1, parts[1].Length - 2));
				}
				else
				{
					property.SetValue(model, Convert.ChangeType(parts[1], property.PropertyType));
				}
			}
		}
		return model;
	}
}

class BadUsageException : Exception
{
	public BadUsageException(string message) : base(message) { }
	public BadUsageException(Exception inner) : base(inner.Message, inner) { }
}
