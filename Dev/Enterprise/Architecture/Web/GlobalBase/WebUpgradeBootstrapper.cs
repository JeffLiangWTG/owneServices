using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWiseOne.WebInfrastructure;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public partial class WebUpgradeBootstrapper : IRegisteredObject
	{
		public WebUpgradeBootstrapper(string serverName)
		{
			this.siteInformation = SiteInformation.Create(serverName: Db.ServerName, databaseName: Db.DatabaseName);
			WebUpgradeManager.Log($"WebUpgradeBootstrapper ctor {siteInformation.ServerName} {siteInformation.DatabaseName}.", EventLogEntryType.Information, siteInformation);
			if (HostingEnvironment.IsHosted)
			{
				HostingEnvironment.RegisterObject(this);
			}

			HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
			{
				CreateAndStartWebUpgradeManagerAsync(serverName, cancellationToken);
			});
		}

		Task CreateAndStartWebUpgradeManagerAsync(string serverName, CancellationToken cancellationToken)
		{
			try
			{
				WebUpgradeManager.Log($"WebUpgradeBootstrapper CreateAndStartWebUpgradeManagerAsync {siteInformation.ServerName} {siteInformation.DatabaseName}.", EventLogEntryType.Information, siteInformation);

				StartWebUpgradeManager(serverName);

				if (!cancellationToken.IsCancellationRequested)
				{
					WebUpgradeManager.Log($"WebUpgradeBootstrapper starting WebUpgradeManager {siteInformation.ServerName} {siteInformation.DatabaseName}.", EventLogEntryType.Information, siteInformation);
				}
				else
				{
					WebUpgradeManager.Log($"WebUpgradeBootstrapper CreateAndStartWebUpgradeManagerAsync has been cancelled {siteInformation.ServerName} {siteInformation.DatabaseName}.", EventLogEntryType.Information, siteInformation);
				}
			}
			catch (Exception ex)
			{
				WebUpgradeManager.Log($"WebUpgradeBootstrapper CreateAndStartWebUpgradeManagerAsync Error on: {siteInformation.ServerName} {siteInformation.DatabaseName}, exception: {ex}", EventLogEntryType.Error, siteInformation);
			}
			finally
			{
				NotifyWebUpgradeManagerStarted_ForTest();
			}

			return Task.CompletedTask;
		}

		void StartWebUpgradeManager(string serverName)
		{
			sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () =>
			{
				var sqlDataProviderFactory = new CargoWise.Data.Providers.Common.SqlDataProviderFactory();

				return (SqlConnection)sqlDataProviderFactory.OpenNewDbConnection<RestrictedWriterLoginCredentials>(
					Db.ServerName,
					Db.DatabaseName,
					applicationName: DbConnection.GetSuffixedApplicationName("WebUpgradeManager"),
					connectTimeout: DbEnv.Instance.ConnectionTimeout,
					connectionPooling: DbEnv.Instance.ConnectionPooling.IsPooling,
					loadBalanceTimeout: DbEnv.Instance.ConnectionPooling.LoadBalanceTimeout,
					minPoolSize: DbEnv.Instance.ConnectionPooling.MinPoolSize,
					maxPoolSize: DbEnv.Instance.ConnectionPooling.MaxPoolSize);
			});

			webUpgradeManager = new WebUpgradeManager(sqlContext);
		}

		public void Stop(bool immediate)
		{
			webUpgradeManager?.Dispose();
			sqlContext?.Dispose();

			if (HostingEnvironment.IsHosted)
			{
				HostingEnvironment.UnregisterObject(this);
			}

			WebUpgradeManager.Log($"WebUpgradeBootstrapper disposed {siteInformation.ServerName}   {siteInformation.DatabaseName}.", EventLogEntryType.Information, siteInformation);
		}

		WebUpgradeSqlContext sqlContext;
		WebUpgradeManager webUpgradeManager;
		readonly SiteInformation siteInformation;

		partial void NotifyWebUpgradeManagerStarted_ForTest();
	}
}

#if DEBUG
namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public partial class WebUpgradeBootstrapper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]

		partial void NotifyWebUpgradeManagerStarted_ForTest()
		{
			webUpgradeManagerStartedAction_ForTest.Value?.Invoke(webUpgradeManager);
		}

		public static IDisposable OverrideWebUpgradeManager_ForTest(Func<string, SqlConnection, WebUpgradeManager> managerToUse)
		{
			GetWebUpgradeManagerToUse_ForTest.Value = managerToUse;
			return new DisposableAction(() => webUpgradeManagerStartedAction_ForTest.ResetValue());
		}
		static readonly Overridable<Func<string, SqlConnection, WebUpgradeManager>> GetWebUpgradeManagerToUse_ForTest = new Overridable<Func<string, SqlConnection, WebUpgradeManager>>();

		public static IDisposable DisposableWebUpgradeManagerStartedAction_ForTest(Action<WebUpgradeManager> action)
		{
			webUpgradeManagerStartedAction_ForTest.Value = action;
			return new DisposableAction(() => webUpgradeManagerStartedAction_ForTest.ResetValue());
		}

		static readonly Overridable<Action<WebUpgradeManager>> webUpgradeManagerStartedAction_ForTest = new Overridable<Action<WebUpgradeManager>>();
	}
}
#endif
