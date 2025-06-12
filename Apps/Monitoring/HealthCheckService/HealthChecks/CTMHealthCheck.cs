using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using XH.Framework.XT.REST.API.Common;
using XH.XT.Monitoring.HealthCheckService.Constant;
using XH.XT.Monitoring.HealthCheckService.XTRESTAPI;

namespace XH.XT.Monitoring.HealthCheckService.HealthChecks
{
	public class CTMHealthCheck : HealthCheckBase
	{
		readonly IHttpContextAccessor httpContextAccessor;
		readonly XTRestClient client;
		readonly string xTServerName;
		readonly IMemoryCache memoryCache;

		public CTMHealthCheck(
			ILogger logger,
			IHttpContextAccessor httpContextAccessor,
			XTRestClient client, string arg1,
			IConfiguration configuration,
			IMemoryCache memoryCache)
			: base(logger, httpContextAccessor, configuration)
		{
			this.httpContextAccessor = httpContextAccessor;
			this.client = client;
			xTServerName = arg1;
			this.memoryCache = memoryCache;
		}

		protected override async Task<HealthCheckResult> ExecuteHealthCheckAsync(HealthCheckContext context, CancellationToken cancellationToken) => await CheckCTMStatusAsync(cancellationToken);

		public virtual async Task<HealthCheckResult> CheckCTMStatusAsync(CancellationToken cancellationToken)
		{
			var xtPath = XtPath;
			if (!memoryCache.TryGetValue(xtPath, out var xtTag))
			{
				var lastSlashIndex = xtPath.LastIndexOf("/", StringComparison.Ordinal);
				string xTParentFolderPath = lastSlashIndex == -1 ? string.Empty : xtPath?.Substring(0, lastSlashIndex);

				Logger.Information($"Starting Logon Rest Api call on xTServer {xTServerName}.");
				var hasRelogonRestApi = await client.LogonRestApiAsync(xTServerName, cancellationToken);
				Logger.Information($"Has Relogon Rest Api: {hasRelogonRestApi}. Starting List Objects call on xTServer {xTServerName} and parent {xTParentFolderPath}.");
				// cancellationToken has not been passed to ListObjectsAsync knowingly because ListObjectsAsync can take more than 10 seconds to complete.
				// SCOM has timeout configured to 10 seconds.
				var xtFolders = await client.ListObjectsAsync(xTServerName, TagDictionary.XtTagDict[XtType.Folder] + xTParentFolderPath, cancellationToken: cancellationToken);

				if (xtFolders is not null && xtFolders.Any())
				{
					string xtObjectName = xtPath?.Substring(lastSlashIndex + 1);
					var xtObjectConfig = xtFolders.Where(x => x.Name == xtObjectName);
					if (xtObjectConfig is not null && xtObjectConfig.Any())
					{
						xtTag = xtObjectConfig.First().Id.Type;
						memoryCache.Set(xtPath, xtTag);
					}
				}
			}
			var unhealthyDic = new Dictionary<string, object>();

			if (xtTag is null)
			{
				unhealthyDic.Add("Path", $"The path '{xtPath}' does not exist in the server '{xTServerName}'.");
				return HealthCheckResult.Unhealthy(data: unhealthyDic);
			}

			Logger.Information($"Starting Logon Alarm Server call on xTServer {xTServerName}.");
			var hasRelogonAlarmServer = await client.LogonAlarmServerAsync(xTServerName, cancellationToken);
			Logger.Information($"Has Relogon Alarm Server: {hasRelogonAlarmServer}. Starting List Active CTM Folder call on xTServer {xTServerName} and folder {xtPath}.");
			var activeCtms = await client.ListActiveCTMFolderWithDetailsAsync(xTServerName, TagDictionary.XtTagDict[(XtType)xtTag] + xtPath, cancellationToken);

			if (activeCtms?.Length > 0)
			{
				foreach (var ctm in activeCtms)
				{
					string errorDescription;
					switch (ctm.Type)
					{
						case CTM_Type.ContractOutPortFailure_ct:
						case CTM_Type.NodeOutPortFailure_ct:
						case CTM_Type.PartyOutPortFailure_ct:
							errorDescription = "The object is on hold status";
							break;
						case CTM_Type.NodeIncomingSessions_ct:
							// Temporarily omitting " ({ctm.Details.Limit})" due to xT bug - WI00727975
							errorDescription = $"Incoming sessions has exceeded limit";
							break;
						case CTM_Type.NodeMonitorOutQueue_ct:
						case CTM_Type.ContractMonitorErrorQueue_ct:
						case CTM_Type.ContractMonitorOutQueue_ct:
							// Temporarily omitting " ({ctm.Details.Limit}) for over {ctm.Details.TimeOverLimit} seconds" due to xT bug - WI00727975
							errorDescription = $"The queue has exceeded limit";
							break;
						case CTM_Type.ContractMessageLifetimeExpired_ct:
							errorDescription = "Processing messages delay has exceeded its configured timeout";
							break;
						default:
							throw new InvalidOperationException($"Unsupported CTM type: {ctm.Type}");
					}

					unhealthyDic.Add($"{xTServerName}-{ctm.Id}", $"Object: {ctm.OwnerObjectId}, Type: {ctm.CtmName}, Description: {errorDescription}");
				}
				return HealthCheckResult.Unhealthy(data: unhealthyDic);
			}

			return HealthCheckResult.Healthy("Healthy");
		}

		internal virtual string XtPath
		{
			get
			{
				var request = httpContextAccessor.HttpContext?.Request;
				var path = $"{request.PathBase.Value}{request.Path.Value}";
				return path?
			.Replace("/wtg/status", string.Empty, StringComparison.InvariantCultureIgnoreCase)
			.Replace($"/{xTServerName}", string.Empty, StringComparison.InvariantCultureIgnoreCase)
			.Trim('/');
			}
		}
	}
}
