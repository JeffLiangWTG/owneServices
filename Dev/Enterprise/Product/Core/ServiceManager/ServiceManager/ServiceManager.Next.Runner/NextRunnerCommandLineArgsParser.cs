using System.CommandLine;
using CargoWise.ServiceManager.Next.Shared;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Runner;

public class NextRunnerCommandLineArgsParser : INextRunnerOptions
{
	public NextRunnerCommandLineArgsParser(params string[] args)
	{
		optionLauncherHub.AddValidator(CommandLineParser.StringNotNullOrWhiteSpaceValidator);

		parser = new CommandLineParser(
			description: "Process Controller Next Runner",
			new Argument[]
			{
				argumentServerName,
				argumentDatabaseName,
			},
			new Option[]
			{
				optionLauncherHub,
			});

		parseResult = parser.Parse(args);

		LauncherHub = new Uri(LauncherHubId, UriKind.Absolute);
	}

	public Uri LauncherHub { get; }
	public string ServerName => parseResult.GetValue(argumentServerName!) ?? throw new ArgumentNullException(nameof(ServerName));
	public string DatabaseName => parseResult.GetValue(argumentDatabaseName!) ?? throw new ArgumentNullException(nameof(DatabaseName));
	internal string LauncherHubId => parseResult.GetValue(optionLauncherHub!) ?? throw new ArgumentNullException(nameof(LauncherHubId));

	readonly Option<string> optionLauncherHub =
		new Option<string>(
			name: $"-{nameof(LauncherHub)}",
			description: string.Empty)
		{
			IsRequired = true,
		};
	readonly Argument<string> argumentServerName = CommandLineParser.ArgumentServerName;
	readonly Argument<string> argumentDatabaseName = CommandLineParser.ArgumentDatabaseName;

	readonly CommandLineParser parser;
	readonly CommandLineParseResult parseResult;
}