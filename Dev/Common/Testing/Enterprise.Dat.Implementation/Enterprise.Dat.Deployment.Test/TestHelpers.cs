using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWiseOne.WebInfrastructure;
using Microsoft.Web.Administration;
using static System.FormattableString;

namespace Enterprise.Dat.Implementation.Testing
{
	public static class TestHelpers
	{
		public static void WinrmQuickConfigAndWaitForExit()
		{
			Process.Start("winrm", "quickconfig -quiet").WaitForExit();
		}

		public static IDisposable InstallTestSite(
			string serverName,
			string databaseName,
			out string siteName,
			out string applicationPoolName)
		{
			const string webConfig = @"
<?xml version=""1.0""?>
<configuration>
	<system.web>
		<customErrors mode=""Off"" />
		<compilation debug=""true"">
		  <assemblies>
			<remove assembly=""*""/>
		  </assemblies>
		</compilation>
	</system.web>
</configuration>
";
			var testSiteName = Invariant($"TestSite_{Guid.NewGuid():N}");
			var testAppPoolName = string.Empty;
			var cargoWiseOneWebRootDirectory = new TempDirectory();

			using (var serverManager = new ServerManager())
			{
				var site = NewWebSite("Test");
				var appPool = NewApplicationPool();
				var rootApplication = site.Applications.SingleOrDefault(app => app.Path == "/");

				site.ApplicationDefaults.ApplicationPoolName = appPool.Name;
				rootApplication.ApplicationPoolName = appPool.Name;
				siteName = site.Name;
				applicationPoolName = rootApplication.ApplicationPoolName;

				serverManager.CommitChanges();
				SaveWebDbConfiguration();

				ApplicationPool NewApplicationPool()
				{
					var applicationPool = serverManager.ApplicationPools.Add($"TestAppPool-{Guid.NewGuid()}");
					applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.ApplicationPoolIdentity;
					applicationPool.ProcessModel.LoadUserProfile = false;
					applicationPool.Enable32BitAppOnWin64 = true;
					testAppPoolName = applicationPool.Name;

					return applicationPool;
				}

				Site NewWebSite(string folderName)
				{
					var applicationPhsicalPath = Path.Combine(cargoWiseOneWebRootDirectory, "Test");
					_ = Directory.CreateDirectory(applicationPhsicalPath);
					File.WriteAllText(Path.Combine(applicationPhsicalPath, "web.config"), webConfig);

					var random = new Random();
					var newSite = serverManager.Sites.Add(testSiteName, folderName, random.Next(49152, 65535));

					return newSite;
				}

				void SaveWebDbConfiguration()
				{
					WebDbConfiguration.SaveConfiguration(
						new WebDbConfigurationInfo { ApplicationPath = WebAppPath.For(site), ServerName = serverName, DatabaseName = databaseName, });
				}
			}

			return new DisposableAction(() =>
			{
				using (var serverManager = new ServerManager())
				{
					var testSite = serverManager.Sites.FirstOrDefault(x => x.Name == testSiteName);
					if (testSite != null)
					{
						serverManager.Sites.Remove(testSite);
					}

					var appPool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == testAppPoolName);
					if (appPool != null)
					{
						serverManager.ApplicationPools.Remove(appPool);
					}

					serverManager.CommitChanges();
				}

				cargoWiseOneWebRootDirectory.Dispose();
			});
		}
	}
}
