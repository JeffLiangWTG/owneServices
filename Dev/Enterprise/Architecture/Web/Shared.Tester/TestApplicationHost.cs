using System;
using System.Web;
using System.Web.Hosting;

namespace Enterprise.ZArchitecture.Web.Shared
{
	public class TestApplicationHost : MarshalByRefObject
	{
		public AppDomain GetAppDomain()
		{
			var appDomain = AppDomain.CurrentDomain;

			return appDomain;
		}

		public void ProcessRequest(string page)
		{
			HttpRuntime.ProcessRequest(new SimpleWorkerRequest(page, null, Console.Out));
		}
	}
}
