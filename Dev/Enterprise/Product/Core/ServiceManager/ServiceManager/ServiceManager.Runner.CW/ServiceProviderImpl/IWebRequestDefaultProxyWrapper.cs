using System.Net;
using Enterprise.ServiceManager.Business;

namespace Enterprise.ServiceManager.Runner
{
	public interface IWebRequestDefaultProxyWrapper
	{
		IWebProxy? DefaultWebProxy { get; }
		void SetDefaultProxyToSystemProxy();
		void SetDefaultProxyToHostProxy(StmServiceHost host);
	}
}
