using System.CommandLine;
using CargoWise.ServiceManager.Next.Shared;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Runner.Test;

class NextRunnerCommandLineArgsParserTest
{
	[TestCase(".")]
	[TestCase("myServerName")]
	public void Constructor_ParseValidArgs(string serverName)
	{
		var hubUri = new Uri($"http://localhost:1234/signalR/{Guid.NewGuid()}");
		var nextRunnerOptions = NextRunnerOptions($"-LauncherHub:{hubUri}", serverName, "myDatabaseName");
		Assert.Multiple(() =>
		{
			Assert.That(nextRunnerOptions.ServerName, Is.EqualTo(serverName));
			Assert.That(nextRunnerOptions.ServerName, Is.Not.EqualTo(Environment.MachineName));
			Assert.That(nextRunnerOptions.DatabaseName, Is.EqualTo("myDatabaseName"));
			Assert.That(nextRunnerOptions.LauncherHub, Is.EqualTo(hubUri));
		});
	}

	[Test]
	public void Constructor_EnsureAllArgumentsAreMandatory()
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions("-LauncherHub:http://localhost:1234/signalR/123"));
		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(ErrorRequiredArgumentMissingForCommand));
		Assert.That(e?.Errors.ElementAt(1).Message, Is.EqualTo(ErrorRequiredArgumentMissingForCommand));
	}

	[Test]
	public void Constructor_EnsureDatabaseNameIsMandatory()
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions("ServerName", "-LauncherHub:http://localhost:1234/signalR/123"));
		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(ErrorRequiredArgumentMissingForCommand));
	}

	[TestCase("ServerName", " ", "DatabaseName", "-LauncherHub:http://localhost:1234/signalR/123")]
	[TestCase("DatabaseName", "ServerName", " ", "-LauncherHub:http://localhost:1234/signalR/123")]
	public void Constructor_EnsureArgumentValuesAreMandatory(string expectedArgument, params string[] args)
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions(args));
		Assert.That(e?.Errors.ElementAt(0).Name, Is.EqualTo(expectedArgument));
		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(string.Format(ErrorArgumentValueIsNullOrWhiteSpace, expectedArgument)));
	}

	[Test]
	public void Constructor_EnsureOptionValuesAreMandatory()
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions(".", "Odyssey", "-LauncherHub:"));

		Assert.That(e?.Errors.ElementAt(0).Name, Is.EqualTo("LauncherHub"));
		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(string.Format(ErrorRequiredArgumentMissingForOption, $"-LauncherHub")));
	}

	[Test]
	public void Constructor_EnsureOptionValuesAreNotNullOrWhiteSpace()
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions(".", "Odyssey", "-LauncherHub:  	  "));

		Assert.That(e?.Errors.ElementAt(0).Name, Is.EqualTo("LauncherHub"));
		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(string.Format(ErrorOptionValueIsNullOrWhiteSpace, "LauncherHub")));
	}

	[Test]
	public void Constructor_EnsureOptionsAreMandatory()
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions(".", "Odyssey"));
		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(string.Format(ErrorOptionIsRequired, $"-LauncherHub")));
	}

	[Test]
	public void Constructor_InvalidOption()
	{
		var e = Assert.Throws<CommandLineException>(() => NextRunnerOptions(".", "myDatabaseName", "-INVALID_OPTION:123", "-LauncherHub:http//localhost:1234/signalR/123"));

		Assert.That(e?.Errors.ElementAt(0).Message, Is.EqualTo(string.Format(ErrorUnrecognizedCommandOrArgument, "-INVALID_OPTION:123")));
	}

	[TestCase("signalR/123")]
	[TestCase("/signalR/123")]
	public void Constructor_CheckAbsoluteUrl(string relativeUri)
	{
		var hubUri = new Uri("http://localhost:12345/signalR/123");
		var baseUri = new Uri("http://localhost:12345");

		var e = Assert.Throws<UriFormatException>(() => NextRunnerOptions($"-LauncherHub:{relativeUri}", ".", "myDatabaseName"));

		var nextRunnerOptions = NextRunnerOptions($"-LauncherHub:{new Uri(baseUri, relativeUri)}", ".", "myDatabaseName");
		Assert.Multiple(() =>
		{
			Assert.That(e?.Message, Is.EqualTo("Invalid URI: The format of the URI could not be determined."));
			Assert.That(nextRunnerOptions.ServerName, Is.EqualTo("."));
			Assert.That(nextRunnerOptions.DatabaseName, Is.EqualTo("myDatabaseName"));
			Assert.That(nextRunnerOptions.LauncherHub, Is.EqualTo(hubUri));
		});
	}

	static readonly string ErrorRequiredArgumentMissingForCommand = $"Required argument missing for command: '{RootCommand.ExecutableName}'.";
	const string ErrorRequiredArgumentMissingForOption = "Required argument missing for option: '{0}'.";
	const string ErrorUnrecognizedCommandOrArgument = "Unrecognized command or argument '{0}'.";
	const string ErrorOptionIsRequired = "Option '{0}' is required.";
	const string ErrorArgumentValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for argument: '{0}'.";
	const string ErrorOptionValueIsNullOrWhiteSpace = "Argument value cannot be null or empty for option: '{0}'.";

	static INextRunnerOptions NextRunnerOptions(params string[] args)
	{
		return new NextRunnerCommandLineArgsParser(args);
	}
}
