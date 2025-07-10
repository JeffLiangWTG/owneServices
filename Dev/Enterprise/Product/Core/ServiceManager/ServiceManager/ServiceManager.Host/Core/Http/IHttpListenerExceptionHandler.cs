using System.Net;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface IHttpListenerExceptionHandler
	{
		bool HandleException(HttpListenerException httpListenerException, IHostLogger hostLogger);
	}
}
