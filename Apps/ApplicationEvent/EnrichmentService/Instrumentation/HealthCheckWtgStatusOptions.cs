using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public partial class HealthCheckWtgStatusOptions : HealthCheckOptions
{
	public HealthCheckWtgStatusOptions()
	{
		ResultStatusCodes[HealthStatus.Healthy] = StatusCodes.Status200OK;
		ResultStatusCodes[HealthStatus.Degraded] = StatusCodes.Status200OK;
		ResultStatusCodes[HealthStatus.Unhealthy] = StatusCodes.Status200OK;
		ResponseWriter = WtgStatusResponseWriter;
	}

	public async Task WtgStatusResponseWriter(HttpContext context, HealthReport healthReport)
	{
		context.Response.ContentType = "text/plain; charset=utf-8";

		var sb = new StringBuilder();
		foreach (var item in healthReport.Entries)
		{
			context.RequestAborted.ThrowIfCancellationRequested();

			sb.Append(item.Value.Status switch
			{
				HealthStatus.Healthy => "INFO",
				HealthStatus.Degraded => "WARNING",
				HealthStatus.Unhealthy => "ERROR",
				_ => throw new NotImplementedException()
			}).Append($"({item.Key}): ");

			if (item.Value.Exception is Exception ex)
				sb.Append($"{ex.GetType()}: ").AppendJoin(' ', RegexWhitespace().Replace(ex.Message, " "));
			else
				sb.AppendJoin(' ', RegexWhitespace().Replace(item.Value.Description!, " "));

			sb.AppendLine();
		}

		await context.Response.WriteAsync(sb.ToString(), context.RequestAborted);
	}

	[GeneratedRegex(@"\s+")]
	private static partial Regex RegexWhitespace();
}
