using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class ProcessRunnerPoolFactory(
		IServiceProvider serviceProvider) : IProcessRunnerPoolFactory
	{
		readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		IProcessRunnerPool runnerPool;
		static readonly object lockObject = new();

		public IProcessRunnerPool GetOrCreate()
		{
			if (runnerPool is null)
			{
				lock (lockObject)
				{
					if (runnerPool is null)
					{
						runnerPool = serviceProvider.GetRequiredService<IProcessRunnerPool>();
						Created?.Invoke(this, runnerPool);
					}
				}
			}

			return runnerPool;
		}

		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<IProcessRunnerPool> Created;
	}
}
