using System;
using Enterprise.ServiceManager.Business;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host;

class HostServiceTaskLoaderFactory(IServiceProvider serviceProvider) : IServiceTaskLoaderFactory
{
	public IServiceTaskLoader CreateServiceTaskLoader() =>
		(IServiceTaskLoader)serviceProvider.GetRequiredService(typeof(NativeServiceTaskLoader));

	readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
}
