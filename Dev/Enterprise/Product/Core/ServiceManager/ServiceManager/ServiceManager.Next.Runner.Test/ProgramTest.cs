using CargoWise.Data;
using CargoWise.ServiceManager.Next.Shared;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.Internals;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using TokenGeneratorService = CargoWise.ServiceManager.Next.Runner.TokenSigning.TokenGeneratorService;

namespace CargoWise.ServiceManager.Next.Runner.Test;

class ProgramTest : TransactionedTestCase
{
	public void TestMain_Exit_WhenParametersNotSet()
	{
		var e = AssertExceptionThrown<CommandLineException>(() => Program.Main(Array.Empty<string>()));
		AssertEquals("Option '-LauncherHub' is required.", e.Errors.ElementAt(0).Message);
		AssertContains("Required argument missing for command:", e.Errors.ElementAt(1).Message);
		AssertContains("Required argument missing for command:", e.Errors.ElementAt(2).Message);
	}

	public void TestMain_Exit_WhenParametersSetAndHubUriNotValid()
	{
		var e = AssertExceptionThrown<UriFormatException>(() => Program.Main(new[] { "-LauncherHub:123", Db.ServerName, Db.DatabaseName }));
		AssertEquals("Invalid URI: The format of the URI could not be determined.", e?.Message);
	}

	public void TestMain_Exit_WhenParametersSetAndHubNotRunning()
	{
		var exitCode = Program.Main(ValidArgs);
		AssertEquals((int)RunnerExitCode.ServiceTaskUnhandledException, exitCode);
	}

	public void TestMain_LogToFileSystem()
	{
		var logDir = ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
		var logFileName = Path.Combine(logDir, $"WEB_{DateTime.UtcNow:yyyyMMdd}.txt");
		if (File.Exists(logFileName))
		{
			File.Delete(logFileName);
		}

		_ = Program.Main(ValidArgs);

		Assert("Log file does not exists", File.Exists(logFileName));
		using var stream = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var sr = new StreamReader(stream);
		var content = sr.ReadToEnd();
		AssertContains("Log file does not contains expected error", "Exception caught while running SignalRBackgroundService, closing the application", content);
	}

	public void TestConfigureAppBuilder_AddServices()
	{
		var builder = Program.ConfigureAppBuilder(ValidArgs);
		var services = builder.Services;
		CombineAssertions(() =>
		{
			using var serviceProvider = services.BuildServiceProvider();
			AssertType<TokenGeneratorService>(serviceProvider.GetService<ITokenGeneratorService>());
			AssertType<TokenConfigWriterServiceUsingRegistry>(serviceProvider.GetService<ITokenConfigWriterService>());
			AssertType<NextRunnerCommandLineArgsParser>(serviceProvider.GetService<INextRunnerOptions>());
			AssertType<SignalRBackgroundService>(serviceProvider.GetService<IHostedService>());
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(TokenGeneratorServiceClient),
					typeof(TokenConfigWriterServiceClient),
				},
				serviceProvider.GetServices<ITokenClient>().Select(s => s.GetType()));
		});
	}

	string[] ValidArgs =>
	[
		"-LauncherHub:http://localhost:1234/signalR/123",
		Db.ServerName,
		Db.DatabaseName
	];
}
