using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceBusinessObjectBindingsSubProvider
	{
		IEnumerable<IHostedServiceBusinessObjectBinding> BusinessObjectBindings { get; }
	}
}
