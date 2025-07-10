using System;

namespace ServiceManager.Host.Abstractions
{
	public interface IResourceThrottler : IDisposable
	{
		ResourceThrottlerResult WaitForResource();
	}
}
