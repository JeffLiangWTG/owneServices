using System.Runtime.CompilerServices;
using CargoWise.ServiceManager.Next.Shared;
using Enterprise.ServiceManager.Shared;
using Microsoft.AspNetCore.Server.HttpSys;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Launcher;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Used as type argument for WebApplicationFactory")]
public class Program
{
	[ModuleInitializer]
	public static void InitializeModule()
	{
		NetCoreAssemblyResolver.Setup();
	}

	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddProblemDetails();
		builder.Services.AddSingleton<INextLauncherOptions>(_ => new NextLauncherCommandLineArgsParser(args));
		builder.Services.AddControllers();
		builder.Services.AddSignalR();
		builder.Services.AddWebTaskServices();
		builder.Services.AddSingleton<IProcessFactory, ProcessWrapperFactory>();
		builder.Services.AddSingleton<INextProcessRunnerPool, NextProcessRunnerPool>();
		builder.Services.AddSingleton<INextProcessRunnerFactory, NextProcessRunnerFactory>();
		builder.Services.AddSingleton<ICommandSender, SignalRCommandSender>();
		builder.Services.AddSingleton<IAccessTokenService, AccessTokenService>();
		builder.Services.AddHostedService<TokenCreationService>();
		builder.Services.AddOptions<HttpSysOptions>().Configure((HttpSysOptions options, INextLauncherOptions nextLauncherOptions) =>
		{
			options.AllowSynchronousIO = false;
			options.Authentication.Schemes = AuthenticationSchemes.None;
			options.Authentication.AllowAnonymous = true;
			options.MaxConnections = null;
			options.MaxRequestBodySize = 30_000_000;
			options.UrlPrefixes.Add(ServiceManagerHelper.GetListenerStrongBinding(nextLauncherOptions.ServiceType, nextLauncherOptions.EnterpriseCode, nextLauncherOptions.ServerCode));
		});
		builder.WebHost.UseHttpSys();
		builder.Host.UseWindowsService();

		var app = builder.Build();
		app.UseExceptionHandler();
		app.MapControllers();
		var nextLauncherOptions = app.Services.GetRequiredService<INextLauncherOptions>();
		app.Services.InitializeWebTaskService();

		var nextHubEndpoint = nextLauncherOptions.LauncherHubEndpoint;
		app.Logger.LogInformation("Next SignalR Hub endpoint: {NextHubEndpoint}", nextHubEndpoint);
		app.MapHub<NextHub>(nextHubEndpoint);

		app.Run();
	}
}
