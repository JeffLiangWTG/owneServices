using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class RunnerTransactionAdapterFactory : ITransactionAdapterFactory
	{
		public RunnerTransactionAdapterFactory(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public ITransactionAdapter CreateTransactionAdapter() =>
			(ITransactionAdapter)serviceProvider.GetRequiredService(typeof(NativeServiceTaskTransactionAdapter));

		readonly IServiceProvider serviceProvider;
	}
}
