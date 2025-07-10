using System;
using System.Web;
using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Web.TestWebApplication.Common
{
	public class WebEnvironment : BaseEnvironment, IWebEnvironment, IHeartBeatRemoteLogoff
	{
		public WebEnvironment()
			: base(new MultiThreadUserContextManager())
		{
			AssemblyLoader.Instance = new WebAssemblyLoader();
			Logger = new DetailedLoggerForTest();
		}

		public override string ApplicationStartupPath => HttpRuntime.AppDomainAppPath;

		public override IUserLoginController LoginController => null;

		public IContactBase WebUser => (HttpContext.Current?.ApplicationInstance as ZGlobalBase)?.SiteUser?.LoggedInUser;

		protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider => new WebSemaphoreProvider(this);

		public ILogger Logger { get; }

		public override void ExitApplication()
		{
			Logger.Log(LogType.Debug, $"{nameof(ExitApplication)}");

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain.Unload(AppDomain.CurrentDomain);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		public void OnRemoteLogoff()
		{
			Logger.Log(LogType.Debug, $"{nameof(OnRemoteLogoff)}");
		}

		public void OnRemoteUpgradeLogoff(DateTime upgradeDateTimeUtc, Func<bool> updateExists)
		{
			Logger.Log(LogType.Debug, $"{nameof(OnRemoteUpgradeLogoff)}, upgradeDateTimeUtc: {upgradeDateTimeUtc}");
		}

		protected override void Dispose(bool isDisposing)
		{
			AssemblyLoader.Instance = null;

			base.Dispose(isDisposing);
		}
	}
}
