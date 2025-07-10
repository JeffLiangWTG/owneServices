using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.ProductRegistration.Service;

public class ServiceProviderDependencyResolver : IDependencyResolver
{
	readonly IServiceProvider serviceProvider;

	public ServiceProviderDependencyResolver(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public IDependencyScope BeginScope()
	{
		return new ServiceProviderDependencyScope(serviceProvider.CreateScope());
	}

	public object GetService(Type serviceType)
	{
		return serviceProvider.GetService(serviceType);
	}

	public IEnumerable<object> GetServices(Type serviceType)
	{
		return serviceProvider.GetServices(serviceType);
	}

	public void Dispose()
	{
	}

	class ServiceProviderDependencyScope : IDependencyScope
	{
		readonly IServiceScope scope;

		public ServiceProviderDependencyScope(IServiceScope scope)
		{
			this.scope = scope;
		}

		public object GetService(Type serviceType)
		{
			return scope.ServiceProvider.GetService(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return scope.ServiceProvider.GetServices(serviceType);
		}

		public void Dispose()
		{
			scope.Dispose();
		}
	}
}
