using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace XH.XT.Monitoring.HealthCheckService.HealthChecks
{
	public abstract class HealthCheckBase : IHealthCheck
	{
		protected ILogger Logger { get; }
		bool SkipCheck { get; }

		protected HealthCheckBase(ILogger mainLogger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
		{
			var request = httpContextAccessor?.HttpContext?.Request;
			var path = request.PathBase.Value + request.Path.Value;
			var healthCheckName = path.Split("/")[1];
			this.Logger = mainLogger?.ForContext("HealthCheckName", healthCheckName);
			this.Logger.Information("Starting health check");
			SkipCheck = configuration.GetValue<bool>("HealthCheckSettings:SkipCheck");
		}

		protected abstract Task<HealthCheckResult> ExecuteHealthCheckAsync(HealthCheckContext context, CancellationToken cancellationToken);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Please use more specific exception type if you update this function.")]
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
		{
			if (SkipCheck)
			{
				return HealthCheckResult.Healthy("Health check skipped.");
			}
			try
			{
				return await ExecuteHealthCheckAsync(context, cancellationToken);
			}
			catch (Exception ex)
			{
				Logger.Warning(ex, $"Exception during health check: {ex.Message}");
				return HealthCheckResult.Unhealthy(exception: ex);
			}
		}
	}
}
