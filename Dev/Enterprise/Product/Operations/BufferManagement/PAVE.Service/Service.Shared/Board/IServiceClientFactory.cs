using System;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IServiceClientFactory<T>
	{
		T GetNewClient(Uri serviceUrl, TimeSpan sendTimeout, string serverAndConnectionName);
	}
}
