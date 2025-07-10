using System;
using System.IO;
using CargoWise.Common;
using CargoWise.DataProtection;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Server.Setup
{
	class UploadPackage : InstallationItem
	{
		public UploadPackage(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.installationSettings = installationSettings;
		}

		readonly InstallationSettings installationSettings;

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			string tempDirectory = UpgradeManager.CreateTempDirectory();
			try
			{
				ChangeCurrentTaskDescription("Extracting client installation package");
				EdpFile.Unpack(installationSettings.UpgradePackageFile, tempDirectory, ExtractOnProgress);

				var (version, versionDate) = UpgradeManager.GetVersionOfPackage(installationSettings.UpgradePackageFile);

				ChangeCurrentTaskDescription("Copying server files");

				ChangeCurrentTaskDescription("Uploading client installation package");
				OpenConnection();

				if (Transaction != null)
				{
					UpgradeManager upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(Connection, Connection.DataSource, Connection.Database, Transaction));
					upgradeManager.UploadUpgradePackage(installationSettings.UpgradePackageFile, version, versionDate, "CUR", "First installed version", null);
				}
				else
				{
					using (var updateTransaction = Connection.BeginTransaction())
					{
						UpgradeManager upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(Connection, Connection.DataSource, Connection.Database, updateTransaction));
						upgradeManager.UploadUpgradePackage(installationSettings.UpgradePackageFile, version, versionDate, "CUR", "First installed version", null);
						updateTransaction.Commit();
					}
				}

				RunPackageDependentTasks(tempDirectory);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return InstallationResult.Error(e.Message);
			}
			finally
			{
				try
				{
					Directory.Delete(tempDirectory, true);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}

				try
				{
					DisposeConnection();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
			}

			return InstallationResult.OK();
		}

		protected virtual void RunPackageDependentTasks(string tempDirectory)
		{
			ChangeCurrentTaskDescription("Initializing registry");
			RegistryInitialiser.SetRegistryItems(installationSettings.DbName, Connection, installationSettings.SelectedDatabase.InstanceName, Path.Combine(tempDirectory, @"Distribution\Application"), null);
		}

		bool ExtractOnProgress(string status, int percentComplete)
		{
			ChangeCurrentTaskDescription("Extracting client installation package");
			return true;
		}

		protected virtual void OpenConnection()
		{
			var connectionProvider = Application.ServiceProvider.GetRequiredService<ISqlConnectionProvider>();
			var pdsFacatory = Application.ServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();
			var pds = pdsFacatory.CreateSystemService(installationSettings.ServerName, installationSettings.DbName);
			Connection = connectionProvider.OpenNewSqlConnection<RestrictedWriterLoginCredentials>(pds, (b) =>
			{
				b.DataSource = installationSettings.ServerName;
				b.InitialCatalog = installationSettings.DbName;
			});
			Transaction = null;
		}

		protected virtual void DisposeConnection()
		{
			Connection?.Dispose();
		}

		protected SqlConnection Connection;

		protected SqlTransaction Transaction;
	}
}
