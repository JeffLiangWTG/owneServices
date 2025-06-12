using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Nest;
using Serilog;
using XH.XT.Monitoring.HealthCheckService.ElasticSearch;
using DateMath = Nest.DateMath;

namespace XH.XT.Monitoring.HealthCheckService.HealthChecks
{
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public class TrackingMessageHealthCheck : HealthCheckBase
	{
		private HealthCheckSettings healthCheckSettings;
		private IElasticClient elasticClient;

		public TrackingMessageHealthCheck(ILogger logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IOptionsMonitor<HealthCheckSettings> settingsOptions, IElasticClient elasticClient)
			: base(logger, httpContextAccessor, configuration)
		{
			healthCheckSettings = settingsOptions?.CurrentValue;
			this.elasticClient = elasticClient;
		}
		protected override Task<HealthCheckResult> ExecuteHealthCheckAsync(HealthCheckContext context, CancellationToken cancellationToken)
		{
			return Task.FromResult(TrackingMessagesHealthCheck(out var result, out var description)
				? HealthCheckResult.Healthy(data: result)
				: HealthCheckResult.Unhealthy(description, data: result));
		}
		internal virtual bool TrackingMessagesHealthCheck(out Dictionary<string, object> result, out string description)
		{
			var healthyResult = new Dictionary<string, object>();
			var unhealthyResult = new Dictionary<string, object>();
			foreach (var trackingMessage in healthCheckSettings.TrackingMessages)
			{
				var responseResult = elasticClient.Search<ExtendedElasticArchiveMessage>(search =>
					search.Index(trackingMessage.ElasticIndex)
						.Query(q => q.Bool(b =>
						{
							var mustQueries = new List<Func<QueryContainerDescriptor<ExtendedElasticArchiveMessage>, QueryContainer>>()
								{
									must => must.Match(m => m.Field(f => f.CustomPublic.SourceParty).Query(trackingMessage.SourceParty)),
									must => must.Match(m => m.Field(f => f.CustomPublic.DestinationParty).Query(trackingMessage.DestinationParty)),
									must => must.Bool(processResultBool => processResultBool.Should(
										should => should.Match(m => m.Field(f => f.ProcessResult).Query(ProcessResult.Successful)),
										should => should.Match(m => m.Field(f => f.ProcessResult).Query(ProcessResult.SuccessfulAfterRetry))))
								};
							if (trackingMessage.Contract != null)
							{
								mustQueries.Add(must => must.Match(m => m.Field(f => f.Contract).Query(trackingMessage.Contract)));
							}
							mustQueries.Add(must => must.DateRange(drd => drd.Field(f => f.Timestamp).GreaterThanOrEquals(DateMath.FromString($"now-{trackingMessage.ErrorDelayMins}m")).LessThanOrEquals(DateMath.Now)));
							return b.Must(mustQueries);
						})));

				if (responseResult.Documents.Count == 0)
				{
					unhealthyResult.Add(trackingMessage.Description, $"Processing backlog delay exceeded {trackingMessage.ErrorDelayMins} minutes");
				}
				else
				{
					healthyResult.Add(trackingMessage.Description, "Healthy");
				}
			}

			var isHealthy = unhealthyResult.Count == 0;
			result = isHealthy ? healthyResult : unhealthyResult;
			description = !isHealthy && healthyResult.Count > 0 ? $"Healthy interface(s): {string.Join(", ", healthyResult.Keys)}" : null;

			return isHealthy;
		}
	}
}
