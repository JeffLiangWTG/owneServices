using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.eHubPortal.Services;

public static class HealthCheckBuilderExtensions
{
	public static IEndpointRouteBuilder MapWtgHealthChecks(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapHealthChecks("/wtg/ready", new HealthCheckOptions
		{
			ResultStatusCodes =
			{
				[HealthStatus.Healthy] = StatusCodes.Status200OK,
				[HealthStatus.Degraded] = StatusCodes.Status200OK,
				[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
			},
			Predicate = r => r.Tags.Contains("ready")
		}).AllowAnonymous();

		endpoints.MapHealthChecks("/wtg/status", new HealthCheckOptions
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
					}).Append($"({item.Key})");

					var details = new List<string>();

					if (item.Value.Description != null)
					{
						details.Add(item.Value.Description);
					}

					if (item.Value.Data != null)
					{
						foreach (var data in item.Value.Data)
						{
							details.Add($"{data.Key}:{data.Value}");
						}
					}

					if (item.Value.Exception != null)
					{
						details.Add($"{item.Value.Exception.GetType()}: {item.Value.Exception.Message}");
					}

					if (details.Count > 0)
					{
						sb.Append(": ").Append(string.Join("|", details).ReplaceLineEndings());
					}

					sb.AppendLine();
				}

				return context.Response.WriteAsync(sb.ToString());
			}
		}).AllowAnonymous();

		return endpoints;
	}
}
