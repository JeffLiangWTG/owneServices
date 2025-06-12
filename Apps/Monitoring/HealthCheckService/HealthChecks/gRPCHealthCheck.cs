using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using XH.Framework.GrpcClient;

namespace XH.XT.Monitoring.HealthCheckService.HealthChecks
{
	public class GrpcHealthCheck : HealthCheckBase
	{
		private readonly IXtClient xTClient;
		private readonly ILogger logger;

		public GrpcHealthCheck(ILogger logger, IHttpContextAccessor httpContextAccessor, IXtClient xTClient, IConfiguration configuration)
			: base(logger, httpContextAccessor, configuration)
		{
			this.xTClient = xTClient;
			this.logger = logger;
		}

		protected override Task<HealthCheckResult> ExecuteHealthCheckAsync(HealthCheckContext context, CancellationToken cancellationToken) => Task.FromResult(CheckConnections());

		internal virtual HealthCheckResult CheckConnections()
		{
			try
			{
				xTClient.Connect();
				xTClient.Disconnect();
			}
			catch (Grpc.Core.RpcException ex) when (ex.StatusCode != StatusCode.Unavailable)
			{
				logger.Debug(ex, $"Error happens during gRPC health check: {ex.Message} {ex.InnerException?.Message}");
			}
			catch (Exception)
			{
				throw;
			}
			return HealthCheckResult.Healthy(data: new Dictionary<string, object>
			{
				{ "gRPC", "Healthy" }
			});
		}
	}
}
