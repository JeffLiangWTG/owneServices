using System;
using Enterprise.ServiceManager.Business;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host;

class HostServiceTasksReloaderFactory(IServiceProvider serviceProvider) : IServiceTasksReloaderFactory
{
	public IServiceTasksReloader CreateServiceTasksReloader() =>
		(IServiceTasksReloader)serviceProvider.GetRequiredService(typeof(NativeServiceTasksReloader));

	readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
}
