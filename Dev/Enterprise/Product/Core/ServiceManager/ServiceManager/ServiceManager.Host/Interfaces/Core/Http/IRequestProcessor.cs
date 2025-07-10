using System.Net;
using System.Threading;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IRequestProcessor
	{
		void ProcessRequest(HttpListenerContext context, CancellationToken cancellationToken);
	}
}
