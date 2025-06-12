using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace XH.XT.Monitoring.HealthCheckService.HealthChecks
{
	public class ServiceHealthCheck : HealthCheckBase
	{
		public ServiceHealthCheck(ILogger logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
			: base(logger, httpContextAccessor, configuration)
		{
		}

		protected override Task<HealthCheckResult> ExecuteHealthCheckAsync(HealthCheckContext context, CancellationToken cancellationToken)
		{
			return Task.FromResult(HealthCheckResult.Healthy(data: new Dictionary<string, object>
			{
				{ "xTHealthCheckService", "Service is alive" }
			}));
		}
	}
}
