using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "ZEnterpriseGlobal is here to replace EnterpriseHttpApplication")]
	public partial class ZEnterpriseGlobal : ZEnterpriseGlobalBase
	{
		protected virtual void Application_Start(object sender, EventArgs e)
		{
			InitializeServerAndDatabaseNames();
			Initialiser.InitialiseWeb(EnableErrorReport, WebExceptionReporter, WebEnvProvider);
			AssemblyLoader.Instance = new WebAssemblyLoader();
			InitializeWebUpgradeBootstrapper();
		}

		protected virtual void Session_Start(object sender, EventArgs e)
		{
			var siteUser = GetNewSiteUser();
			if (siteUser != null && HttpContext.Current?.Session != null)
			{
				HttpContext.Current.Session.Add(SiteUserSessionKey, siteUser);
			}
		}

		protected virtual void Application_BeginRequest(object sender, EventArgs e)
		{
			if (Db.IsDatabaseUpgraded)
			{
#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
				var currentTicks = DateTime.UtcNow.Ticks;
				var lastReopenTicksSnapshot = Interlocked.Read(ref lastReopenTicks);

				if (new TimeSpan(currentTicks - lastReopenTicksSnapshot) > CloseAndReopenConnectionInterval.Value
					&& (Interlocked.CompareExchange(ref lastReopenTicks, currentTicks, lastReopenTicksSnapshot) == lastReopenTicksSnapshot))
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
							((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection();
						}
					}
					catch (DatabaseUpgradeExceptionCaughtException) { }
				}
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
			}

			if (Db.IsDatabaseUpgraded)
			{
				throw new DatabaseUpgradedException();
			}
		}

		protected virtual void Application_EndRequest(object sender, EventArgs e)
		{
		}

		protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected virtual void Application_PreSendRequestHeaders(object sender, EventArgs e)
		{
		}

		protected virtual void Application_Error(object sender, EventArgs e)
		{
			if (!(sender is HttpApplication httpApplication))
			{
				return;
			}

			try
			{
				httpApplication.Response.ClearHeaders();
				httpApplication.Response.ClearContent();
			}
			catch (HttpException)
			{
				return;
			}

			var server = httpApplication.Server;
			var exception = server.GetLastError();

			TopLevelWebExceptionHandler.HandleUnhandledException(exception);
		}

		protected virtual void Session_End(object sender, EventArgs e)
		{
			HttpContext.Current?.Session?.Remove(SiteUserSessionKey);
		}

		protected virtual void Application_End(object sender, EventArgs e)
		{
			webUpgradeBootstrapper?.Stop(true);

			if (lazyEnvProvider.IsValueCreated)
			{
				lazyEnvProvider.Value.Dispose();
			}

			AssemblyLoader.Instance = null;
		}

		[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "ZEnterpriseGlobal is here to replace EnterpriseHttpApplication")]
		[SuppressMessage("CargoWiseOne", "CW1018A:HttpApplicationDbAppSettingsRule", Justification = "ZEnterpriseGlobal is here to replace EnterpriseHttpApplication")]
		protected internal virtual void InitializeServerAndDatabaseNames()
		{
			var webDbConfig = WebDbConfiguration.GetCurrentConfiguration();

			if (string.IsNullOrEmpty(webDbConfig.ServerName) || string.IsNullOrEmpty(webDbConfig.DatabaseName))
			{
				Db.InitializeDatabaseDetails(
					ConfigurationManager.AppSettings["ServerName"],
					ConfigurationManager.AppSettings["DatabaseName"],
					CargoWise.DataProtection.ApplicationType.Web);
			}
			else
			{
				Db.InitializeDatabaseDetails(
					webDbConfig.ServerName,
					webDbConfig.DatabaseName,
					CargoWise.DataProtection.ApplicationType.Web);
			}
		}

		protected virtual void InitializeWebUpgradeBootstrapper()
		{
			webUpgradeBootstrapper = new WebUpgradeBootstrapper(Db.ServerName);
		}

		WebUpgradeBootstrapper webUpgradeBootstrapper;

		protected virtual bool EnableErrorReport => true;
		protected virtual BaseExceptionReporter WebExceptionReporter => lazyBaseWebExceptionReporter.Value;
		protected virtual EnvProvider WebEnvProvider => lazyEnvProvider.Value;

		readonly Lazy<BaseWebExceptionReporter> lazyBaseWebExceptionReporter =
			new Lazy<BaseWebExceptionReporter>(() => new BaseWebExceptionReporter(TopLevelWebExceptionHandler.LazyInstance.Value));
		readonly Lazy<EnvProvider> lazyEnvProvider = new Lazy<EnvProvider>(() => new WebEnvProvider());

		[ThreadSafe]
		static long lastReopenTicks = 0L;

		static readonly Overridable<TimeSpan> CloseAndReopenConnectionInterval = new Overridable<TimeSpan>(TimeSpan.FromSeconds(1));

#if DEBUG

		public static Overridable<TimeSpan> CloseAndReopenConnectionIntervalForTest => CloseAndReopenConnectionInterval;

#endif

	}
}

#if DEBUG

#region Partial Class

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public partial class ZEnterpriseGlobal
	{
		public IDisposable StartApplicationDisposable()
		{
			Application_Start(this, EventArgs.Empty);

			return new DisposableAction(() =>
			{
				Application_End(this, EventArgs.Empty);
			});
		}

		public IDisposable StartSessionDisposable()
		{
			Session_Start(this, EventArgs.Empty);

			return new DisposableAction(() => Session_End(this, EventArgs.Empty));
		}

		public bool EnableErrorReport_Exposed => EnableErrorReport;
		public BaseExceptionReporter WebExceptionReporter_Exposed => WebExceptionReporter;
		public EnvProvider WebEnvProvider_Exposed => WebEnvProvider;
		public TopLevelExceptionHandler TopLevelExceptionHandler_Exposed => TopLevelWebExceptionHandler.LazyInstance.Value;
		public void Application_BeginRequest_Exposed(object sender, EventArgs e) => Application_BeginRequest(sender, e);
		public void Application_PreSendRequestHeaders_Exposed(object sender, EventArgs e) => Application_PreSendRequestHeaders(sender, e);
	}
}

#endregion

#endif
