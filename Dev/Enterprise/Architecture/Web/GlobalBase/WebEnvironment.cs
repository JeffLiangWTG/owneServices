using System;
using System.Web;
using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class WebEnvironment : BaseEnvironment, IWebEnvironment, IHeartBeatRemoteLogoff
	{
		public WebEnvironment(IUserContextManager contextManager = null)
			: base(contextManager ?? new MultiThreadUserContextManager())
		{
			lazyWebSemaphoreProvider =
				new Lazy<WebSemaphoreProvider>(() => new WebSemaphoreProvider(this));
			InitializeWebEnvironment();
		}

		protected void InitializeWebEnvironment()
		{
			AssemblyLoader.Instance = new WebAssemblyLoader();
		}

		public override string ApplicationStartupPath => HttpRuntime.AppDomainAppPath;

		public override IUserLoginController LoginController => null;

		public virtual IContactBase WebUser => (HttpContext.Current?.Session?[ZEnterpriseGlobalBase.SiteUserSessionKey] as WebUser)?.LoggedInUser;

		protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider => lazyWebSemaphoreProvider.Value;

		public override void ExitApplication()
		{
		}

		public virtual void OnRemoteLogoff()
		{
		}

		public virtual void OnRemoteUpgradeLogoff(DateTime upgradeDateTimeUtc, Func<bool> updateExists)
		{
		}

		protected override void Dispose(bool isDisposing)
		{
			AssemblyLoader.Instance = null;

			base.Dispose(isDisposing);
		}

		public override bool IsWeb => true;

		readonly Lazy<WebSemaphoreProvider> lazyWebSemaphoreProvider;
	}
}
