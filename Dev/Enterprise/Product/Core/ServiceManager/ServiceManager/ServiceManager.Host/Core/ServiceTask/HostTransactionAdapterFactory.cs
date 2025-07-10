using System;
using Enterprise.ServiceManager.Business;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host;

class HostTransactionAdapterFactory(IServiceProvider serviceProvider) : ITransactionAdapterFactory
{
	public ITransactionAdapter CreateTransactionAdapter() =>
		(ITransactionAdapter)serviceProvider.GetRequiredService(typeof(NativeServiceTaskTransactionAdapter));

	readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
}
