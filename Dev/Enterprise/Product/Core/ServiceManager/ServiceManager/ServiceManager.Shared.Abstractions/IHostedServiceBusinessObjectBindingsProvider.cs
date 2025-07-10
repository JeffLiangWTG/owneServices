using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface IHostedServiceBusinessObjectBindingsProvider
	{
		IEnumerable<IHostedServiceBusinessObjectBinding> BusinessObjectBindings { get; }
	}
}
