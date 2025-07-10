using System;
using System.IO;
using System.Linq;
using Microsoft.Web.Administration;

namespace CargoWiseOne.WebInfrastructure.Integration.Strong.Test.Helpers
{
	class SiteUninstallerViaAppManager : WTG.DevTools.TestFramework.ExecuteOrInvokeViaAppManager
	{
		public static void ExecuteOrInvoke(string applicationPhysicalPath)
		{
			new SiteUninstallerViaAppManager
			{
				applicationPhysicalPath = applicationPhysicalPath,
			}.ExecuteOrInvoke();
		}

		public override void Execute()
		{
			try
			{
				using var serverManager = new ServerManager();
				var applicationsToDelete =
					(from site in serverManager.Sites
						from application in site.Applications
						from virtualDirectory in application.VirtualDirectories
						where virtualDirectory.PhysicalPath.StartsWith(applicationPhysicalPath, StringComparison.OrdinalIgnoreCase)
						select (site, application)).ToList();

				foreach (var (site, application) in applicationsToDelete)
				{
					ApplicationRemover.DeleteApplicationAndRelatedItems(serverManager, site, application);
					WebDbConfiguration.DeleteAllConfigurations(WebAppPath.For(site));
					serverManager.Sites.Remove(site);
				}

				serverManager.CommitChanges();
			}
			finally
			{
				try
				{
					if (Directory.Exists(applicationPhysicalPath))
					{
						Directory.Delete(applicationPhysicalPath, true);
					}
				}
				catch
				{
					// ignored
				}
			}
		}

		protected override object[] GetState()
		{
			return new object[] { applicationPhysicalPath };
		}

		protected override void ReadState(object[] state)
		{
			applicationPhysicalPath = (string)state[0];
		}

		string applicationPhysicalPath;
	}
}
