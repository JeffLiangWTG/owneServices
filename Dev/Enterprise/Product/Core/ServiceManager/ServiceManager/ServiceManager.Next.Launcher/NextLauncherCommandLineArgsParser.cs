using System.CommandLine;
using CargoWise.ServiceManager.Next.Shared;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace CargoWise.ServiceManager.Next.Launcher;

public class NextLauncherCommandLineArgsParser : INextLauncherOptions
{
	public NextLauncherCommandLineArgsParser(params string[] args)
	{
		optionEnterpriseCode.AddValidator(CommandLineParser.StringNotNullOrWhiteSpaceValidator);
		optionServerCode.AddValidator(CommandLineParser.StringNotNullOrWhiteSpaceValidator);
		optionRunnerIdleLifetime.AddValidator(result =>
		{
			var valueForOption = result.GetValueForOption(optionRunnerIdleLifetime);
			if (valueForOption < 0)
			{
				result.ErrorMessage = $"{result.Option.Name} must be greater or equal to 0.";
			}
			else if (valueForOption >= 60 * 60 * 24)
			{
				result.ErrorMessage = $"{result.Option.Name} must be less than 86400.";
			}
		});

		parser = new CommandLineParser(
			description: "Process Controller Next Launcher",
			new Argument[]
			{
				argumentServerName,
				argumentDatabaseName,
			},
			new Option[]
			{
				optionEnterpriseCode,
				optionServerCode,
				optionRunnerIdleLifetime,
			});

		parseResult = parser.Parse(args);

		LauncherHubEndpoint = $"signalR/{Guid.NewGuid()}";
		ServiceType = ServiceType.LauncherSecurity;
	}

	public string LauncherHubEndpoint { get; }
	public string ServerName => parseResult.GetValue(argumentServerName!) ?? throw new ArgumentNullException(nameof(ServerName));
	public string DatabaseName => parseResult.GetValue(argumentDatabaseName!) ?? throw new ArgumentNullException(nameof(DatabaseName));
	public string EnterpriseCode => parseResult.GetValue(optionEnterpriseCode!) ?? throw new ArgumentNullException(nameof(EnterpriseCode));
	public ServiceType ServiceType { get; }
	public string ServerCode => parseResult.GetValue(optionServerCode!) ?? throw new ArgumentNullException(nameof(ServerCode));
	public TimeSpan RunnerIdleLifetime => TimeSpan.FromSeconds(parseResult.GetValue(optionRunnerIdleLifetime));

	readonly Option<string> optionEnterpriseCode =
		new Option<string>(
			name: $"-{nameof(EnterpriseCode)}",
			description: string.Empty)
		{
			IsRequired = true,
		};
	readonly Option<string> optionServerCode =
		new Option<string>(
			name: $"-{nameof(ServerCode)}",
			description: string.Empty)
		{
			IsRequired = true,
		};
	readonly Option<int> optionRunnerIdleLifetime =
		new Option<int>(
			name: $"-{nameof(RunnerIdleLifetime)}",
			description: "Time that runner will remain idle (in seconds)",
			getDefaultValue: () => 300)
		{
			IsRequired = false,
		};
	readonly Argument<string> argumentServerName = CommandLineParser.ArgumentServerName;
	readonly Argument<string> argumentDatabaseName = CommandLineParser.ArgumentDatabaseName;

	readonly CommandLineParser parser;
	readonly CommandLineParseResult parseResult;
}
