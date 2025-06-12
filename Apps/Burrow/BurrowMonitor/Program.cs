using System.Text;
using eServices.BurrowMonitor;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var healthChecks = builder.Services.AddHealthChecks();
var monitors = builder.Configuration.GetSection("Burrow:Monitors").Get<Dictionary<string, string>>();
foreach (var monitor in monitors!)
{
	healthChecks.AddTypeActivatedCheck<BurrowHealthCheck>(monitor.Key, monitor.Value);
}

builder.Services.AddHttpClient("Burrow", (services, httpClient) =>
{
	httpClient.BaseAddress = new Uri(services.GetRequiredService<IConfiguration>()["Burrow:Address"]!);
});

var app = builder.Build();

app.UseHealthChecks("/wtg/status", new HealthCheckOptions
{
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status200OK
	},
	ResponseWriter = (HttpContext context, HealthReport healthReport) =>
	{
		context.Response.ContentType = "text/plain; charset=utf-8";

		var sb = new StringBuilder();
		foreach (var item in healthReport.Entries)
		{
			sb.Append(item.Value.Status switch
			{
				HealthStatus.Healthy => "INFO",
				HealthStatus.Degraded => "WARNING",
				HealthStatus.Unhealthy => "ERROR",
				_ => throw new NotImplementedException()
			}).Append($"({item.Key}): ");

			if (item.Value.Exception != null)
			{
				sb.Append($"{item.Value.Exception.GetType()}: ");
				sb.Append($"{item.Value.Exception.Message.Replace(Environment.NewLine, "")}");
			}
			else
			{
				sb.Append($"Count={item.Value.Data["lag"]}, ");
				sb.Append($"Age={(int)DateTimeOffset.UtcNow.Subtract(
					(DateTimeOffset)item.Value.Data["timestamp"]).TotalMinutes}, ");
				sb.Append($"Report={item.Value.Data["url"]}");
			}
			sb.AppendLine();
		}

		return context.Response.WriteAsync(sb.ToString());
	}
});

app.Run();
