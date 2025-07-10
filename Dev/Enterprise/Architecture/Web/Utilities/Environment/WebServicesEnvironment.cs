using System;
using System.Web;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	public class WebServicesEnvironment : BaseEnvironment, IWebEnvironment
	{
		public WebServicesEnvironment()
			: base(new MultiThreadUserContextManager())
		{
		}

		public override bool IsWebService => true;

		public override string ApplicationStartupPath => HttpRuntime.AppDomainAppPath;

		public override IUserLoginController LoginController => null;

		public IContactBase WebUser => (HttpContext.Current?.ApplicationInstance as ZEnterpriseGlobalBase)?.SiteUser?.LoggedInUser;

		protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider => new WebServicesSemaphoreProvider();

		public override void ExitApplication()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain.Unload(AppDomain.CurrentDomain);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}
	}
}
