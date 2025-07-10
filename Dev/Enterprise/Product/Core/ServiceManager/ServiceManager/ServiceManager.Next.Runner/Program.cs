using System.Runtime.CompilerServices;
using CargoWise.ServiceManager.Next.Shared;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.Internals;
using Enterprise.Registry.Business;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Runner;

public static class Program
{
	[ModuleInitializer]
	public static void InitializeModule()
	{
		CargoWise.NetCoreAssemblyResolver.Setup();
	}

	public static int Main(string[] args)
	{
		using var app = ConfigureAppBuilder(args).Build();

		app.Services.InitializeWebTaskService();
		var signalRBackgroundService = app.Services.GetServices<IHostedService>().OfType<SignalRBackgroundService>().Single();
		app.Run();
		return (int)(signalRBackgroundService.ExitCode ?? RunnerExitCode.ServiceTaskUnhandledException);
	}

	public static HostApplicationBuilder ConfigureAppBuilder(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddWebTaskServices();
		builder.Services.AddHttpClient();
		builder.Services.AddScoped<ITokenGeneratorService, TokenSigning.TokenGeneratorService>();
		builder.Services.AddScoped<ITokenConfigWriterService, TokenConfigWriterServiceUsingRegistry>();
		builder.Services.AddScoped<ISystemToSystemTrustInfoService, SystemToSystemTrustInfoService>();
		builder.Services.AddSingleton<INextRunnerOptions>(_ => new NextRunnerCommandLineArgsParser(args));
		builder.Services.AddSingleton<ITokenClient, TokenGeneratorServiceClient>();
		builder.Services.AddSingleton<ITokenClient, TokenConfigWriterServiceClient>();
		builder.Services.AddHostedService<SignalRBackgroundService>();
		return builder;
	}
}
