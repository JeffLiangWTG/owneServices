using WTG.ErrorReporting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddRollingFileLogger();
builder.Logging.AddSimpleConsole(config => config.SingleLine = true);

var cacheSizeLimit = builder.Configuration.GetValue<long>("CacheSizeLimit", 2000000000);

builder.Services.AddWindowsService();
builder.Services.AddMemoryCache(options =>
{
	options.SizeLimit = cacheSizeLimit;
	options.TrackStatistics = true;
});
builder.Services.AddSingleton<CacheService>();

var connectionString = builder.Configuration.GetConnectionString("eHubTransactions")
	?? throw new InvalidOperationException("Missing connection string: eHubTransactions");

var maxRetries = builder.Configuration.GetValue<int>("DbConnectionMaxRetries", 30);

builder.Services.AddDbContext<eHubTransactionsContext>(options => options
	.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure(maxRetries))
	.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));

EnricherFactoryBuilder.AddEnricherFactory(builder);
KafkaBuilder.AddKafkaFlow(builder);

var metricsOptionsConfig = builder.Configuration.GetSection("Metrics");
builder.Services.Configure<MetricsOptions>(metricsOptionsConfig);
var metricsOptions = new MetricsOptions();
metricsOptionsConfig.Bind(metricsOptions);
var otel = builder.Services.AddOpenTelemetry();
otel.ConfigureResource(resource => resource
	.AddService(builder.Environment.ApplicationName));
otel.WithMetrics(metrics =>
{
	metrics.AddMeter(builder.Environment.ApplicationName);
	metrics.AddPrometheusExporter();
	if (metricsOptions.ExportToConsole)
	{
		metrics.AddConsoleExporter();
	}
});
if (metricsOptions.ExportToConsole)
{
	otel.WithTracing(tracing =>
	{
		tracing.AddSource(KafkaFlowInstrumentation.ActivitySourceName);
		tracing.AddSource(builder.Environment.ApplicationName);
		tracing.AddConsoleExporter();
	});
}

builder.Services.AddSingleton<FaultRetryHandler>();
builder.Services.AddSingleton<MessageWorkflowHandler>();

builder.Services.AddTransient<IErrorReportingClient>(serviceProvider =>
	Uri.TryCreate(serviceProvider.GetRequiredService<IConfiguration>()["IssueManagerUri"], UriKind.Absolute, out var issueManagerUri)
		? new ErrorReportingClient(issueManagerUri) : null!);
builder.Services.AddSingleton<IErrorReporting, ErrorReporting>();
builder.Services.AddSingleton<IMetrics, Metrics>();
builder.Services.AddSingleton<HealthCheckMetrics>();
builder.Services.AddHostedService<HealthCheckMetricsListener>();

var healthChecks = builder.Services.AddHealthChecks();
healthChecks.AddCheck<HealthCheckCountTotals>($"{nameof(MessageEnrichmentHandler)}_Count_Totals");
foreach (var app in builder.Configuration.GetSection("Enrichers")?.Get<Dictionary<string, object?>>() ?? [])
	healthChecks.AddTypeActivatedCheck<HealthCheckCountApp>($"{nameof(MessageEnrichmentHandler)}_Count_{app.Key}", app.Key);
healthChecks.AddCheck<HealthCheckLag>($"{nameof(MessageEnrichmentHandler)}_Lag");
healthChecks.AddCheck<HealthCheckFaults>($"{nameof(MessageEnrichmentHandler)}_Faults");

var host = builder.Build();

host.Services.GetRequiredService<IErrorReporting>().RegisterHandlers();

host.UseKafkaFlowDashboard();
await host.Services.CreateKafkaBus().StartAsync();

host.UseHealthChecks("/wtg/status", new HealthCheckWtgStatusOptions());
host.MapPrometheusScrapingEndpoint();

await host.RunAsync();
