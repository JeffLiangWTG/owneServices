using System.Net;
using eServices.eHubDataModel.eHubTransactionsCore;
using eServices.eHubPortal.Components;
using eServices.eHubPortal.Constants;
using eServices.eHubPortal.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	builder.Configuration.GetSection("KnownProxies").Get<List<string>>()?
		.ForEach(p => options.KnownProxies.Add(IPAddress.Parse(p)));
});

var dmsIdentityConnectionString = builder.Configuration.GetConnectionString("eHubTransactions")
	?? throw new InvalidOperationException("Missing connection string: eHubTransactions");
builder.Services.AddDbContextFactory<eHubTransactionsContext>(options =>
{
	options.UseSqlServer(dmsIdentityConnectionString, providerOptions => providerOptions.EnableRetryOnFailure());
	if (builder.Environment.IsDevelopment())
		options.EnableSensitiveDataLogging();
});

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
	.AddMicrosoftIdentityWebApp(options =>
	{
		builder.Configuration.Bind("AzureAd", options);
		options.TokenValidationParameters.RoleClaimType = "groups";
	});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorization(options =>
{
	if (builder.Environment.IsDevelopment())
	{
		options.AddPolicy(AccessGroups.Admin, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:Admin")));
		options.AddPolicy(AccessGroups.GlobalReadOnly, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:GlobalReadOnly")));
		options.AddPolicy(AccessGroups.CustomsReadWrite, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:CustomsReadWrite")));
		options.AddPolicy(AccessGroups.CustomsReadOnly, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:CustomsReadOnly")));
		options.AddPolicy(AccessGroups.AirReadWrite, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:AirReadWrite")));
		options.AddPolicy(AccessGroups.AirReadOnly, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:AirReadOnly")));
		options.AddPolicy(AccessGroups.OceanReadWrite, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:OceanReadWrite")));
		options.AddPolicy(AccessGroups.OceanReadOnly, policy => policy.RequireAssertion(_ => builder.Configuration.GetValue<bool>("AccessGroups:OceanReadOnly")));
	}
	else
	{
		options.AddPolicy(AccessGroups.Admin, policy => policy.RequireRole(AccessGroups.Admin));
		options.AddPolicy(AccessGroups.GlobalReadOnly, policy => policy.RequireRole(AccessGroups.GlobalReadOnly));
		options.AddPolicy(AccessGroups.CustomsReadWrite, policy => policy.RequireRole(AccessGroups.CustomsReadWrite));
		options.AddPolicy(AccessGroups.CustomsReadOnly, policy => policy.RequireRole(AccessGroups.CustomsReadOnly));
		options.AddPolicy(AccessGroups.AirReadWrite, policy => policy.RequireRole(AccessGroups.AirReadWrite));
		options.AddPolicy(AccessGroups.AirReadOnly, policy => policy.RequireRole(AccessGroups.AirReadOnly));
		options.AddPolicy(AccessGroups.OceanReadWrite, policy => policy.RequireRole(AccessGroups.OceanReadWrite));
		options.AddPolicy(AccessGroups.OceanReadOnly, policy => policy.RequireRole(AccessGroups.OceanReadOnly));
	}
	options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddScoped<IAccessGroupService, AccessGroupService>();

builder.Services.AddRazorPages()
	.AddMicrosoftIdentityUI();

builder.Logging.ClearProviders();
builder.Logging.AddGlobalSearchFileLogger();
if (builder.Environment.IsDevelopment())
{
	builder.Logging.AddSimpleConsole(config => config.IncludeScopes = true);
}

builder.Services.AddHealthChecks()
	.AddCheck("Hosting", () => HealthCheckResult.Healthy(), ["ready"])
	.AddCheck<HealthCheck>("Database");

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseAuthorization();
app.UseMiddleware<LoggingMiddleware>();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapWtgHealthChecks();

app.Run();
