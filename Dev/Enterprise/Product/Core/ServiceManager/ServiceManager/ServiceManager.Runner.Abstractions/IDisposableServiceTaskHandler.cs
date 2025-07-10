using System;

namespace ServiceManager.Runner.Abstractions
{
	public interface IDisposableServiceTaskHandler : IServiceTaskHandler, IDisposable
	{
	}
}

