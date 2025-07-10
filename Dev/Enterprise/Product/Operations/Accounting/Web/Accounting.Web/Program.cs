#if NET
using System.Runtime.CompilerServices;
using CargoWise;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NetCore.WebInfrastructure;
using Serilog;

namespace Enterprise.Accounting.Web;

class Program
{
	[ModuleInitializer]
	public static void InitializeModule()
	{
		NetCoreAssemblyResolver.Setup();
	}

	public static void Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
					.Enrich.FromLogContext()
					.WriteTo.Console()
					.CreateLogger();

		Log.Information($"Starting up");

		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddServiceModelServices();
		builder.WebHost.UseIISIntegration();
		builder.Services.AddServiceModelMetadata();
		builder.Services.AddHttpLogging(o => { });
		builder.Services.AddSerilog(Log.Logger);
		builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
		builder.Services.AddSingleton<IWebConfigurationProvider, WebConfigurationProvider>();

		var app = builder.Build();
		app.UseFileServer();
		app.UseSerilogRequestLogging();

		Initialiser.InitialiseServiceManager(new BaseExceptionReporter(), usePooledConnection: true);
		DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceRunner;
		var envIisPhyPath = System.Environment.GetEnvironmentVariable("ASPNETCORE_IIS_PHYSICAL_PATH");

		Log.Information($"envIisPhyPath: {envIisPhyPath}");

		WebDbConfigurationInfo config = null;

		var configurationProvider = app.Services.GetRequiredService<IWebConfigurationProvider>();
		var dbConfigFromRegistry = configurationProvider.TryGetWebDbConfigFromIISRegistry(out config);
		Log.Information($"dbConfigFromRegistry : {dbConfigFromRegistry}");

		var serverName = dbConfigFromRegistry ? config.ServerName : builder.Configuration["ServerName"];
		var databaseName = dbConfigFromRegistry ? config.DatabaseName : builder.Configuration["DatabaseName"];

		Log.Information($"serverName : {serverName}");
		Log.Information($"databaseName : {databaseName}");

		InitializeDatabase(serverName, databaseName);

		var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
		serviceMetadataBehavior.HttpGetEnabled = true;

		((IApplicationBuilder)app).UseServiceModel(serviceBuilder =>
		{
			AddSecureServiceToBuilder<AccountingTransactionExportService, IAccountingTransactionExportService>(ref serviceBuilder);
			AddSecureServiceToBuilder<CreditLimitService, ICreditLimitService>(ref serviceBuilder);
			AddSecureServiceToBuilder<UpdateInvoicePaymentDetailsService, IUpdateInvoicePaymentDetailsService>(ref serviceBuilder);
		});

		app.Run();
	}

	static void InitializeDatabase(string serverName, string databaseName)
	{
		// if already set for unit tests, don't change it
		if (Db.DatabaseNameIsInitialized && Db.ServerNameIsInitialized)
		{
			return;
		}

		Db.InitializeDatabaseDetails(
			Db.GetMachineNameIfLocal(serverName),
			databaseName,
			CargoWise.DataProtection.ApplicationType.Web);

		using (Db.DisposableActionForDbConnection())
		{
			Db.Connection.EnsureIsOpen();
		}
	}

	public static void AddSecureServiceToBuilder<Implementation, Contract>(ref IServiceBuilder serviceBuilder)
		where Implementation : class, Contract
	{
		serviceBuilder.AddService<Implementation>(serviceOptions =>
		{
			serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
		});

		serviceBuilder.AddServiceEndpoint<Implementation, Contract>(
		new BasicHttpBinding(),
		$"Accounting/{typeof(Implementation).Name}.asmx",
		endpoint =>
			endpoint.EndpointBehaviors.Add(new SecurityHeaderEndpointBehaviour()));
	}
}
#endif
