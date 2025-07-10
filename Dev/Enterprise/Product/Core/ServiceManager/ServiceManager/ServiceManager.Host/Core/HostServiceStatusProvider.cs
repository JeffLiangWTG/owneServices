using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host;

sealed class HostServiceStatusProvider : IHostServiceStatusProvider
{
	readonly IHostLogger logger;
	readonly IServiceHostsCache serviceHostsCache;

	public HostServiceStatusProvider(IHostLogger logger, IServiceHostsCache serviceHostsCache)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.serviceHostsCache = serviceHostsCache ?? throw new ArgumentNullException(nameof(serviceHostsCache));
	}

	public bool IsReady()
	{
		var extraServiceTypes = ServiceManagerHelper.GetExtraServiceTypes();
		if (extraServiceTypes.Length == 0)
		{
			logger.Log(LogLevel.Debug, "No extra services to check, returning IsReady = true.");
			return true;
		}

		var hostName = ServiceManagerHelper.GetHostName();
		if (!GetServiceHostClient(hostName, out var serviceHostClient))
		{
			return false;
		}

		foreach (var serviceType in extraServiceTypes)
		{
			try
			{
				logger.Log(LogLevel.Debug, $"sending request for {serviceType}");
				serviceHostClient.CheckSecurityServiceReady();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				logger.Log(LogLevel.Information, $"Unable to get response for {serviceType} on host {hostName}, returning IsReady = false", e);
				return false;
			}
		}

		logger.Log(LogLevel.Debug, $"Successful response from all extra services [{string.Join(",", extraServiceTypes)}], returning IsReady = true.");
		return true;
	}

	bool GetServiceHostClient(string hostName, out IServiceHostClient serviceHostClient)
	{
		try
		{
			serviceHostClient = serviceHostsCache
				.ConfiguredServiceHosts
				.Single(s => s.HostName == hostName);
			return true;
		}
		catch (Exception e) when (!e.IsCriticalException())
		{
			logger.Log(LogLevel.Error, $"Unable to get service host client for {hostName}", e);
			serviceHostClient = null;
			return false;
		}
	}
}
