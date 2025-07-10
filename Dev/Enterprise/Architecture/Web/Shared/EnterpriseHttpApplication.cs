#if NETFRAMEWORK
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;

namespace Enterprise.ZArchitecture.Web.Shared
{
	public class EnterpriseHttpApplication : HttpApplication
	{
		protected virtual void Application_Start(Object sender, EventArgs e)
		{
			InitializeDbConnection();
			try
			{
				CreateAndStartWebUpgradeManager();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				WebUpgradeManager.Log($"Web upgrade exception during Application_Start: " + ex.ToString(), EventLogEntryType.Error, lazySiteInfo?.Value);
				initializeUpgradeManagerTimer = new Timer(InitializeUpgradeManager, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
			}
		}

		void InitializeUpgradeManager(object state)
		{
			lock (initializeUpgradeManagerLock)
			{
				if (initializeUpgradeManagerTimer != null)
				{
					try
					{
						CreateAndStartWebUpgradeManager();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						WebUpgradeManager.Log($"Exception during InitializeUpgradeManager: " + ex.ToString(), EventLogEntryType.Error, lazySiteInfo?.Value);
						return;
					}
					initializeUpgradeManagerTimer.Dispose();
					initializeUpgradeManagerTimer = null;
				}
			}
		}

		void CreateAndStartWebUpgradeManager()
		{
			webDbUpgrader = CreateWebUpgradeManager();
		}

		readonly object initializeUpgradeManagerLock = new object();
		Timer initializeUpgradeManagerTimer;
		protected virtual void Application_End(Object sender, EventArgs e)
		{
			if (webDbUpgrader != null)
			{
				webDbUpgrader.Dispose();
				webDbUpgrader = null;
			}
			sqlContext?.Dispose();
		}

		void InitializeDbConnection()
		{
			var config = WebDbConfiguration.GetCurrentConfiguration();
			if (!string.IsNullOrEmpty(config.ServerName) && !string.IsNullOrEmpty(config.DatabaseName))
			{
				Db.InitializeDatabaseDetails(config.ServerName, config.DatabaseName, CargoWise.DataProtection.ApplicationType.Web);
			}
			else
			{
				Db.InitializeDatabaseDetails(
					ConfigurationManager.AppSettings["ServerName"],
					ConfigurationManager.AppSettings["DatabaseName"],
					CargoWise.DataProtection.ApplicationType.Web);
			}
		}

		protected virtual WebUpgradeManager CreateWebUpgradeManager()
		{
			sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => GetSqlConnection());
			return new WebUpgradeManager(sqlContext);
		}

		protected virtual System.Data.Common.DbConnection GetSqlConnection()
		{
			using (Db.DisableSchemaVersionCheck())
			{
				return ((IDbConnectionInternals)Db.NewExtraConnectionToMainDb()).ADOConnection;
			}
		}

		WebUpgradeSqlContext sqlContext;
		WebUpgradeManager webDbUpgrader;
		readonly Lazy<SiteInformation> lazySiteInfo = new Lazy<SiteInformation>(()
			=> SiteInformation.Create(serverName: Db.ServerName, databaseName: Db.DatabaseName));
	}
}
#endif