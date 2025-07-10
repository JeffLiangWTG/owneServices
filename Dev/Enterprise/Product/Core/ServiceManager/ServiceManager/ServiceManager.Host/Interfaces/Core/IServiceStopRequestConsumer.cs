using System;

namespace Enterprise.ServiceManager.Host
{
	public interface IServiceStopRequestConsumer
	{
		bool WaitForServiceStopRequest(TimeSpan timeout);
	}
}