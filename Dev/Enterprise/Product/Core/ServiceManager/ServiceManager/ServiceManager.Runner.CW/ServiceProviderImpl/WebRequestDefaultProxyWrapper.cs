using System.Net;
using Enterprise.ServiceManager.Business;

namespace Enterprise.ServiceManager.Runner
{
	internal class WebRequestDefaultProxyWrapper : IWebRequestDefaultProxyWrapper
	{
		public void SetDefaultProxyToSystemProxy()
		{
			DefaultWebProxy = WebRequest.GetSystemWebProxy();
		}

		public void SetDefaultProxyToHostProxy(StmServiceHost host)
		{
			DefaultWebProxy = host.GetWebProxy();
		}

		public IWebProxy? DefaultWebProxy
		{
			get { return WebRequest.DefaultWebProxy; }
			private set { WebRequest.DefaultWebProxy = value; }
		}
	}
}
