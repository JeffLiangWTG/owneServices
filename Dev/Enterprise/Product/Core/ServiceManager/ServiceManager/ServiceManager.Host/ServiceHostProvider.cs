using System;
using System.Net;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	internal interface IServiceHostProviderFactory
	{
		IServiceHostProvider Create(BusinessObjectFactory factory);
	}

	internal interface IServiceHostProvider
	{
		StmServiceHost LoadServiceHost(string hostName);
		StmServiceHost CreateServiceHost(string hostName);
		void InitializeController(StmServiceHost host, IHostLogger hostLogger);
	}

	class ServiceHostProviderFactory : IServiceHostProviderFactory
	{
		readonly IHostRegistrySettings hostRegistry;

		public ServiceHostProviderFactory(IHostRegistrySettings hostRegistry)
		{
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
		}

		public IServiceHostProvider Create(BusinessObjectFactory factory) => new ServiceHostProvider(factory, hostRegistry);
	}

	class ServiceHostProvider : IServiceHostProvider
	{
		readonly BusinessObjectFactory factory;
		readonly IHostRegistrySettings hostRegistry;

		public ServiceHostProvider(BusinessObjectFactory factory, IHostRegistrySettings hostRegistry)
		{
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			this.factory = factory;
		}

		public StmServiceHost LoadServiceHost(string hostName)
		{
			var host = factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostName));
			if (host != null)
			{
				host.SH_IsActive = true;
				factory.Save();
			}
			return host;
		}

		public StmServiceHost CreateServiceHost(string hostName)
		{
			var host = factory.New<StmServiceHost>();
			host.SH_HostName = hostName;
			host.SH_IsActive = true;
			factory.Save();
			return host;
		}

		public void InitializeController(StmServiceHost host, IHostLogger hostLogger)
		{
			if (hostRegistry.ShowQueryStackTraceInProcessControllerEnabled)
			{
				QueryStackTraceRecorder.Instance.Enabled = true;
			}

			if (!host.SH_ProxyAutoDetect)
			{
				WebRequest.DefaultWebProxy = host.GetWebProxy();
				hostLogger.Log(LogLevel.Debug, "Specific web proxy for registration set.");
			}

			if (host.HasChanges)
			{
				host.Factory.Save();
			}

			hostLogger.Log(LogLevel.Debug, "Service Host Controller initialization completed.");
		}
	}
}

