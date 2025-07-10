using System.CommandLine;
using CargoWise.ServiceManager.Next.Shared;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

class NextLauncherCommandLineArgsParserTest
{
	[TestCase(".")]
	[TestCase("myServerName")]
	public void Constructor_ParseValidArgs(string serverName)
	{
		var nextLauncherOptions = NextLauncherOptions("-EnterpriseCode:WUT", "-ServerCode:SYD", serverName, "myDatabaseName");
		Assert.Multiple(() =>
		{
			Assert.That(nextLauncherOptions.ServerName, Is.EqualTo(serverName), nameof(nextLauncherOptions.ServerName));
			Assert.That(nextLauncherOptions.ServerName, Is.Not.EqualTo(Environment.MachineName), nameof(nextLauncherOptions.ServerName));
			Assert.That(nextLauncherOptions.DatabaseName, Is.EqualTo("myDatabaseName"), nameof(nextLauncherOptions.DatabaseName));
			var launcherHubId = nextLauncherOptions.LauncherHubEndpoint.Split('/').Last();
			Assert.That(Guid.TryParse(launcherHubId, out var launcherGuid), Is.True, nameof(nextLauncherOptions.LauncherHubEndpoint));
			Assert.That(nextLauncherOptions.LauncherHubEndpoint, Is.EqualTo($"signalR/{launcherGuid}"), nameof(nextLauncherOptions.LauncherHubEndpoint));
			Assert.That(nextLauncherOptions.EnterpriseCode, Is.EqualTo("WUT"), nameof(nextLauncherOptions.EnterpriseCode));
			Assert.That(nextLauncherOptions.ServerCode, Is.EqualTo("SYD"), nameof(nextLauncherOptions.ServerCode));
		});
	}

	[Test]
	public void Constructor_EnsureAllArgumentsAreMandatory()
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions("-EnterpriseCode:WUT", "-ServerCode:SYD"));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(RootCommand.ExecutableName, ErrorRequiredArgumentMissingForCommand),
			new CommandLineParseError(RootCommand.ExecutableName, ErrorRequiredArgumentMissingForCommand),
		}));
	}

	[Test]
	public void Constructor_EnsureDatabaseNameIsMandatory()
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions("ServerName", "-EnterpriseCode:WUT", "-ServerCode:SYD"));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(RootCommand.ExecutableName, ErrorRequiredArgumentMissingForCommand),
		}));
	}

	[TestCase("ServerName", " ", "DatabaseName", "-EnterpriseCode:WUT", "-ServerCode:SYD")]
	[TestCase("DatabaseName", "ServerName", " ", "-EnterpriseCode:WUT", "-ServerCode:SYD")]
	public void Constructor_EnsureArgumentValuesAreMandatory(string expectedArgument, params string[] args)
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions(args));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(expectedArgument, string.Format(ErrorArgumentValueIsNullOrWhiteSpace, expectedArgument)),
		}));
	}

	[TestCase("EnterpriseCode", ".", "Odyssey", "-EnterpriseCode:", "-ServerCode:SYD")]
	[TestCase("ServerCode", ".", "Odyssey", "-EnterpriseCode:WUT", "-ServerCode:")]
	public void Constructor_EnsureOptionValuesAreMandatory(string expectedMissingParam, params string[] args)
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions(args));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(expectedMissingParam, string.Format(ErrorRequiredArgumentMissingForOption, $"-{expectedMissingParam}")),
		}));
	}

	[TestCase("EnterpriseCode", ".", "Odyssey", "-EnterpriseCode:	", "-ServerCode:SYD")]
	[TestCase("ServerCode", ".", "Odyssey", "-EnterpriseCode:WUT", "-ServerCode:	")]
	public void Constructor_EnsureOptionValuesAreNotNullOrWhiteSpace(string expectedMissingParam, params string[] args)
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions(args));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(expectedMissingParam, string.Format(ErrorOptionValueIsNullOrWhiteSpace, expectedMissingParam)),
		}));
	}

	[TestCase("EnterpriseCode", "-ServerCode:SYD", ".", "Odyssey")]
	[TestCase("ServerCode", "-EnterpriseCode:WUT", ".", "Odyssey")]
	public void Constructor_EnsureOptionsAreMandatory(string expectedMissingParam, params string[] args)
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions(args));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(RootCommand.ExecutableName, string.Format(ErrorOptionIsRequired, $"-{expectedMissingParam}")),
		}));
	}

	[TestCase("-INVALID_OPTION:")]
	[TestCase(INextRunnerOptions.LauncherHubOption)]
	public void Constructor_InvalidOption(string invalidOption)
	{
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions("-EnterpriseCode:WUT", "-ServerCode:SYD", ".", "myDatabaseName", $"{invalidOption}123"));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError(RootCommand.ExecutableName, string.Format(ErrorUnrecognizedCommandOrArgument, $"{invalidOption}123")),
		}));
	}

	[TestCase(300)]
	[TestCase(0, "-RunnerIdleLifetime: 0")]
	[TestCase(20, "-RunnerIdleLifetime: 20")]
	[TestCase(86399, "-RunnerIdleLifetime: 86399")]
	public void Constructor_OptionalArgument_ValidOption(int expectedIdleLifetime, params string[] optionalArgs)
	{
		var mandatoryArgs = new[] { "-EnterpriseCode:WUT", "-ServerCode:SYD", ".", "myDatabaseName" };
		var nextLauncherOptions = NextLauncherOptions(optionalArgs.Concat(mandatoryArgs).ToArray());
		Assert.That(nextLauncherOptions.RunnerIdleLifetime, Is.EqualTo(TimeSpan.FromSeconds(expectedIdleLifetime)), nameof(nextLauncherOptions.RunnerIdleLifetime));
	}

	[TestCase("must be less than 86400.", "-RunnerIdleLifetime: 86400")]
	[TestCase("must be greater or equal to 0.", "-RunnerIdleLifetime: -1")]
	public void Constructor_OptionalArgument_OutOfBound(string expectedMessage, params string[] optionalArgs)
	{
		var mandatoryArgs = new[] { "-EnterpriseCode:WUT", "-ServerCode:SYD", ".", "myDatabaseName" };
		var e = Assert.Throws<CommandLineException>(() => NextLauncherOptions(optionalArgs.Concat(mandatoryArgs).ToArray()));
		Assert.That(e?.Errors, Is.EquivalentTo(new[]
		{
			new CommandLineParseError("RunnerIdleLifetime", $"RunnerIdleLifetime {expectedMessage}"),
		}));
	}

	[TestCase("-RunnerIdleLifetime: ABC")]
	[TestCase("-RunnerIdleLifetime: 0x10")]
	[TestCase("-RunnerIdleLifetime: 	")]
	public void Constructor_OptionalArgument_Invalid(params string[] optionalArgs)
	{
		var mandatoryArgs = new[] { "-EnterpriseCode:WUT", "-ServerCode:SYD", ".", "myDatabaseName" };
		var expectedErrorArgValue = optionalArgs.First().Split(':').Last();
		var e = Assert.Throws<InvalidOperationException>(() => NextLauncherOptions(optionalArgs.Concat(mandatoryArgs).ToArray()));
		Assert.That(e?.Message, Is.EqualTo($"Cannot parse argument '{expectedErrorArgValue}' for option '-RunnerIdleLifetime' as expected type 'System.Int32'."));
	}

	[Test]
	public void Constructor_HubId_UniqueAndConstant()
	{
		var args = new[] { "-EnterpriseCode:WUT", "-ServerCode:SYD", ".", "myDatabaseName" };
		var nextLauncherOptions1 = NextLauncherOptions(args);
		var nextLauncherOptions2 = NextLauncherOptions(args);
		var launcherHubEndpoint1 = nextLauncherOptions1.LauncherHubEndpoint;

		Assert.Multiple(() =>
		{
			Assert.That(nextLauncherOptions1.LauncherHubEndpoint, Is.EqualTo(launcherHubEndpoint1));
			Assert.That(nextLauncherOptions2.LauncherHubEndpoint, Is.Not.EqualTo(launcherHubEndpoint1));
		});
	}

	static readonly string ErrorRequiredArgumentMissingForCommand = $"Required argument missing for command: '{RootCommand.ExecutableName}'.";
	const string ErrorRequiredArgumentMissingForOption = "Required argument missing for option: '{0}'.";
	const string ErrorUnrecognizedCommandOrArgument = "Unrecognized command or argument '{0}'.";
	const string ErrorOptionIsRequired = "Option '{0}' is required.";
	const string ErrorArgumentValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for argument: '{0}'.";
	const string ErrorOptionValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for option: '{0}'.";

	static INextLauncherOptions NextLauncherOptions(params string[] args)
	{
		return new NextLauncherCommandLineArgsParser(args);
	}
}
