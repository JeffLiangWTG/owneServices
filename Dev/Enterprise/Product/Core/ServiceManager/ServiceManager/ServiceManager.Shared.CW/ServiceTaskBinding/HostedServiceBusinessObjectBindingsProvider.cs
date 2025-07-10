using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Shared.CW
{
	public sealed class HostedServiceBusinessObjectBindingsProvider : IHostedServiceBusinessObjectBindingsProvider
	{
		public static IHostedServiceBusinessObjectBindingsProvider Instance => ObjectFactory.Get<IHostedServiceBusinessObjectBindingsProvider>();

		public IEnumerable<IHostedServiceBusinessObjectBinding> BusinessObjectBindings => serviceBindingsLazy.Value;

		readonly Lazy<IEnumerable<IHostedServiceBusinessObjectBinding>> serviceBindingsLazy = new Lazy<IEnumerable<IHostedServiceBusinessObjectBinding>>(
			GetHostedBusinessObjectBindings, LazyThreadSafetyMode.ExecutionAndPublication);

		static IEnumerable<IHostedServiceBusinessObjectBinding> GetHostedBusinessObjectBindings()
		{
			return ObjectFactory.Get<IEnumerable>("HostedServiceBusinessObjectBindingsSubProviders")
				.Cast<IHostedServiceBusinessObjectBindingsSubProvider>()
				.SelectMany(h => h.BusinessObjectBindings)
				.ToArray();
		}
	}
}
