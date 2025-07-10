using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	public sealed class ServiceProviderImplProxy : BasicServiceProvider, IServiceTaskConfigurationUserProvider
	{
		public ServiceProviderImplProxy(ServiceProviderImpl serviceProviderImpl, IServiceTaskLoader taskLoader, ILoggerFactory loggerFactory)
			: base(
				factory: null,
				proxyWrapper: new WebRequestDefaultProxyWrapper(),
				loggerFactory: new ProxyLoggerFactory(serviceProviderImpl, loggerFactory),
				taskLoader: taskLoader)
		{
			_serviceProviderImpl = serviceProviderImpl;
		}

		protected override void RunTask(CancellationToken cancellationToken)
		{
			IDisposable? disposable = null;
			if (_serviceProviderImpl.GetType().GetCustomAttribute(typeof(NeedsDataRefreshAttribute)) == null)
			{
				disposable = new DataRefreshManager.DisableRefreshForServiceTask();
			}

			using (disposable)
			{
				_serviceProviderImpl.ServiceCode = HostedServiceAttribute!.Code;
				_serviceProviderImpl.RunTask(cancellationToken);
			}
		}

		#region IServiceTaskConfigurationUserProvider Members

		IServiceTaskConfigurationUser? IServiceTaskConfigurationUserProvider.GetServiceTaskConfigurationUser()
		{
			return _serviceProviderImpl as IServiceTaskConfigurationUser;
		}

		#endregion

		readonly ServiceProviderImpl _serviceProviderImpl;

		class ProxyLoggerFactory : ILoggerFactory
		{
			readonly ServiceProviderImpl serviceProviderImpl;
			readonly ILoggerFactory loggerFactory;

			public ProxyLoggerFactory(ServiceProviderImpl serviceProviderImpl, ILoggerFactory loggerFactory)
			{
				this.serviceProviderImpl = serviceProviderImpl;
				this.loggerFactory = loggerFactory;
			}

			public ILogger NewServiceTaskLogger(string dbServer, string dbName, string programCode, string? suffix = null)
			{
				if (serviceProviderImpl.ServiceLogger == null)
				{
					var logger = loggerFactory.NewServiceTaskLogger(dbServer, dbName, programCode);
					serviceProviderImpl.ServiceLogger = logger;
					return logger;
				}
				else
				{
					return serviceProviderImpl.ServiceLogger;
				}
			}

			public ILogger NewScheduledUpgradeLogger(string dbServer, string dbName)
			{
				return loggerFactory.NewScheduledUpgradeLogger(dbServer, dbName);
			}
		}
	}
}
