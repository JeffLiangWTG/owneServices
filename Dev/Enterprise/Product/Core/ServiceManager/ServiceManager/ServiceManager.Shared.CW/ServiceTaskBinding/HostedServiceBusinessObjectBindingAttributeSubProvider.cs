using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace ServiceManager.Shared.CW
{
	sealed class HostedServiceBusinessObjectBindingAttributeSubProvider : IHostedServiceBusinessObjectBindingsSubProvider
	{
		IEnumerable<IHostedServiceBusinessObjectBinding> IHostedServiceBusinessObjectBindingsSubProvider.BusinessObjectBindings => hostedServiceBusinessObjectBindingAttributes.Value;

		readonly Lazy<IEnumerable<IHostedServiceBusinessObjectBinding>> hostedServiceBusinessObjectBindingAttributes = new Lazy<IEnumerable<IHostedServiceBusinessObjectBinding>>(
			() => AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().ToArray(),
			LazyThreadSafetyMode.ExecutionAndPublication);
	}
}
