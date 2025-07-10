using System.CommandLine;
using CargoWise.Data;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host
{
	class HostCommandLineArgsParser : IServiceManagerHostOptions, IServiceNameProvider
	{
		public HostCommandLineArgsParser(params string[] args)
		{
			parser = new CommandLineParser(
				description: "Process Controller Host",
				new Argument[]
				{
					argumentServerName,
					argumentDatabaseName,
				},
				new Option[]
				{
					optionConsole,
					optionInstall,
					optionUninstall,
					optionStop,
					optionStart,
					optionQuiet,
					optionUsername,
					optionPassword,
					optionServerDirectory,
					optionHost,
					optionAutomatic,
					optionConfig,
					optionRemoveDbRecord,
				});

			parseResult = parser.Parse(args);
		}

		public bool OptionConsole => parseResult.GetValue(optionConsole);
		public bool OptionInstall => parseResult.GetValue(optionInstall);
		public bool OptionUninstall => parseResult.GetValue(optionUninstall);
		public bool OptionStop => parseResult.GetValue(optionStop);
		public bool OptionStart => parseResult.GetValue(optionStart);
		public bool OptionQuiet => parseResult.GetValue(optionQuiet);
		public string Username => parseResult.GetValue(optionUsername);
		public string Password => parseResult.GetValue(optionPassword);
		public string ServerDirectory => parseResult.GetValue(optionServerDirectory);
		public string Host => parseResult.GetValue(optionHost);
		public bool Automatic => parseResult.GetValue(optionAutomatic);
		public string Config => parseResult.GetValue(optionConfig);
		public bool RemoveDbRecord => parseResult.GetValue(optionRemoveDbRecord);
		public string ServerName => parseResult.GetValue(argumentServerName);
		public string DatabaseName => parseResult.GetValue(argumentDatabaseName);

		public string GetServiceName()
		{
			return ServiceHostProcess.GetServiceName(ServiceType.ProcessController, Db.GetMachineNameIfLocal(ServerName), DatabaseName);
		}

		readonly Option<bool> optionConsole = new Option<bool>(name: HostCommandLineOptions.OptionConsole,
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionInstall = new Option<bool>(name: HostCommandLineOptions.OptionInstall,
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionUninstall = new Option<bool>(name: HostCommandLineOptions.OptionUninstall,
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionStart = new Option<bool>(name: HostCommandLineOptions.OptionStart,
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionStop = new Option<bool>(name: HostCommandLineOptions.OptionStop,
			getDefaultValue: () => false,
			description: "");
		readonly Option<bool> optionQuiet = new Option<bool>(name: "-quiet",
			getDefaultValue: () => false,
			description: "");
		readonly Option<string> optionUsername = new Option<string>(name: "-username",
			getDefaultValue: () => "",
			description: "");
		readonly Option<string> optionPassword = new Option<string>(name: "-password",
			getDefaultValue: () => "",
			description: "");
		readonly Option<string> optionServerDirectory = new Option<string>(name: "-SDir",
			getDefaultValue: () => "",
			description: "")
			{ Arity = ArgumentArity.ZeroOrOne, };
		readonly Option<string> optionHost = new Option<string>(name: "-host",
			getDefaultValue: () => ".",
			description: "");
		readonly Option<bool> optionAutomatic = new Option<bool>(name: "-automatic",
			getDefaultValue: () => false,
			description: "");
		readonly Option<string> optionConfig = new Option<string>(name: "-config",
			getDefaultValue: () => "",
			description: "");
		readonly Option<bool> optionRemoveDbRecord = new Option<bool>(name: HostCommandLineOptions.RemoveDbRecord,
			getDefaultValue: () => false,
			description: "");
		readonly Argument<string> argumentServerName = CommandLineParser.ArgumentServerName;
		readonly Argument<string> argumentDatabaseName = CommandLineParser.ArgumentDatabaseName;

		readonly CommandLineParser parser;
		readonly CommandLineParseResult parseResult;
	}
}
