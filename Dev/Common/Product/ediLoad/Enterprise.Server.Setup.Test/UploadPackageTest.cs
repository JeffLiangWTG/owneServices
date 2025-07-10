using System;
using System.IO;
using System.Reflection;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.Server.Setup
{
	public class UploadPackageTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1019:NoSqlTransactionRollbackRule")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstall()
		{
			string sourceDirectory = UpgradeManager.CreateTempDirectory();
			string targetDirectory = UpgradeManager.CreateTempDirectory();
			try
			{
				Assembly asm = Assembly.Load("CargoWise.Data");
				object connectionWrapper = asm.GetType("CargoWise.Data.Db").GetProperty("Connection").GetValue(null, Array.Empty<object>());
				connectionWrapper.GetType().GetMethod("EnsureIsOpen").Invoke(connectionWrapper, Array.Empty<object>());
				SqlConnection connection = (SqlConnection)asm.GetType("CargoWise.Data.IDbConnectionInternals").GetProperty("ADOConnection").GetValue(connectionWrapper, Array.Empty<object>());
				File.Copy(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Operations", "MasterFiles", "Business", "MasterFiles.Business", "System", "StmUpgrade", "testing.edp"), Path.Combine(sourceDirectory, "ediEnterprise.edp"));
				File.SetAttributes(Path.Combine(sourceDirectory, "ediEnterprise.edp"), FileAttributes.Normal);

				SqlTransaction transaction = connection.BeginTransaction();
				try
				{
					SetupConfiguration config = new SetupConfiguration() { StartupPathOverride = sourceDirectory };
					config.BaseTargetPath = targetDirectory;
					InstallationResultCollection resultCollection = new InstallationResultCollection();
					new UploadPackageForTest(new Installation(config), new InstallationSettings(config) { UpgradePackageFile = Path.Combine(sourceDirectory, "ediEnterprise.edp") }, connection, transaction).Install(resultCollection);
					Assert(string.Format("Installation should complete successfully\r\nThe following errors occurred:\r\n{0}", resultCollection.GetErrorMessages()), resultCollection.ErrorCount == 0);

					UpgradeManager ugm = new SqlUpgradeManager(new UpgradeSqlContext(connection, connection.DataSource, connection.Database, transaction));
					UpgradeInfo currentVersion = ugm.QueryCurrentVersion();
					AssertNotNull("Current version should correspond to the uploaded package", currentVersion);
					AssertEquals("Current version should correspond to the uploaded package", new Version(1, 2, 3, 4), currentVersion.Version);

					string tempInstallRoot = UpgradeManager.CreateTempDirectory();
					try
					{
						string tempInstallDirectory = Path.Combine(tempInstallRoot, new Version(1, 2, 3, 4).ToString());
						using (ugm.InstallUpgradePackage(currentVersion, tempInstallDirectory))
						{
							string[] expectedInstalledFiles = new string[]
							{
								"alpha.txt", "beta.txt", "CargoWise.ApplicationManager.Common.dll", ExeFileNames.CargoWiseOneExeForVersionInfo, "CargoWiseOne.Start.exe",
								"CargoWiseOne.Start.exe.config", "CurrentVersionWriter.exe", "ediLoad.exe", "ediUninstall.exe",
								"Enterprise.Upgrades.dll", "Enterprise.Upgrades.Postinstall4.0.exe", "Enterprise.Upgrades.Preinstall4.0.exe"
							};
							for (int i = 0; i < expectedInstalledFiles.Length; i++)
							{
								expectedInstalledFiles[i] = Path.Combine(tempInstallDirectory, expectedInstalledFiles[i]);
							}
							AssertContainsExactElementsInAnyOrder(expectedInstalledFiles, Directory.GetFiles(tempInstallDirectory));
						}
					}
					finally
					{
						Directory.Delete(tempInstallRoot, true);
					}
				}
				finally
				{
					transaction.Rollback();
				}
			}
			finally
			{
				Directory.Delete(sourceDirectory, true);
				Directory.Delete(targetDirectory, true);
			}
		}

		class UploadPackageForTest : UploadPackage
		{
			bool connectionOpened;

			public UploadPackageForTest(Installation installation, InstallationSettings installationSettings, SqlConnection connection, SqlTransaction transaction)
				: base(installation, installationSettings)
			{
				this.Connection = connection;
				this.Transaction = transaction;
			}

			protected override void OpenConnection()
			{
				connectionOpened = true;
			}

			protected override void DisposeConnection()
			{
				connectionOpened = false;
			}

			protected override void RunPackageDependentTasks(string tempDirectory)
			{
				if (!connectionOpened)
				{
					throw new Exception("RunPackageDependentTasks called without an open database connection");
				}
			}
		}
	}
}
