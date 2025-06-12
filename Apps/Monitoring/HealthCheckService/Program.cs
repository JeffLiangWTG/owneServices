using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using XH.Framework.Logging;
using XH.XT.Monitoring.HealthCheckService;

Log.Logger = LoggerFactory.GetLogger();

var builder = WebApplication.CreateBuilder(args);
#pragma warning disable CA1416
#pragma warning disable ASP0013

builder.Host.ConfigureAppConfiguration((context, configurationBuilder) =>
{
	var env = context.HostingEnvironment;
	configurationBuilder.AddJsonFile("appsettings.json", false, true);
	configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
	configurationBuilder.AddJsonFile("HealthCheckSettings.json", false, true);
}).ConfigureServices(Startup.ConfigureServices);

var app = builder.Build();
Startup.Configure(app);
app.Run();

#pragma warning restore ASP0013
#pragma warning restore CA1416

public partial class Program
{
}
