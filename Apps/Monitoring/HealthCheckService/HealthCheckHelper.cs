using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XH.XT.Monitoring.HealthCheckService
{
	public static class HealthCheckHelper
	{
		public static Task WriteResponse(HttpContext context, HealthReport healthReport)
		{
			ArgumentNullException.ThrowIfNull(context, nameof(context));
			context.Response.ContentType = "text/plain; charset=utf-8";

			var sb = new StringBuilder();
			sb.AppendLine(CultureInfo.InvariantCulture, $"INFO(Health Check Url): {context.Request.GetDisplayUrl()}");
			sb.AppendLine(CultureInfo.InvariantCulture, $"INFO(Health Check Server HostName): {Environment.MachineName}");

			ArgumentNullException.ThrowIfNull(healthReport, nameof(healthReport));
			foreach (var entry in healthReport.Entries)
			{
				var report = entry.Value;

				if (report.Exception != null)
				{
					sb.AppendLine(CultureInfo.InvariantCulture, $"ERROR({context.Request.PathBase.Value.Trim('/')}): {report.Exception.GetType()}: {report.Exception.Message.Replace(Environment.NewLine, string.Empty, StringComparison.Ordinal)}");
				}
				else
				{
					var status = GetStatus(report.Status);
					foreach (var reportData in report.Data)
					{
						sb.AppendLine(CultureInfo.InvariantCulture, $"{status}({reportData.Key}): {reportData.Value}");
					}

					if (!string.IsNullOrEmpty(report.Description))
					{
						sb.AppendLine(CultureInfo.InvariantCulture, $"INFO({entry.Key.Replace("HealthCheck", string.Empty, StringComparison.Ordinal)}): {report.Description}");
					}
				}
			}

			return context.Response.WriteAsync(sb.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
		}

		static string GetStatus(HealthStatus healthStatus)
		{
			return healthStatus switch
			{
				HealthStatus.Healthy => "INFO",
				HealthStatus.Degraded => "WARNING",
				HealthStatus.Unhealthy => "ERROR",
				_ => throw new NotImplementedException(),
			};
		}
	}
}
