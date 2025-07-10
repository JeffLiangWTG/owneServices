using System;
using System.Web;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	/// <summary>
	/// Environment for Web projects
	/// </summary>
	public class WebEnvironment : BaseEnvironment, IWebEnvironment
	{
		public WebEnvironment(IUserContextManager contextManager = null)
			: base(contextManager ?? new MultiThreadUserContextManager())
		{
		}

		public override void ExitApplication()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain.Unload(AppDomain.CurrentDomain);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		public override IUserLoginController LoginController
		{
			get { return null; }
		}

		public override string ApplicationStartupPath
		{
			get { return HttpRuntime.AppDomainAppPath; }
		}

		public override bool IsWeb
		{
			get { return true; }
		}

		public IContactBase WebUser => (HttpContext.Current?.ApplicationInstance as ZEnterpriseGlobalBase)?.SiteUser?.LoggedInUser;

		protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider => new WebDummySemaphoreProvider();
	}
}
