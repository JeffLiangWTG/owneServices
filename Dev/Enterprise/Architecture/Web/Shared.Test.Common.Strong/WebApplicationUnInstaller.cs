using System.Linq;
using Microsoft.Web.Administration;
using WTG.DevTools.TestFramework;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	public class WebApplicationUninstaller : ExecuteOrInvokeViaAppManager
	{
		public static void ExecuteOrInvoke(string siteName, string appPoolName)
		{
			new WebApplicationUninstaller { siteName = siteName, appPoolName = appPoolName }.ExecuteOrInvoke();
		}

		public override void Execute()
		{
			using (var serverManager = new ServerManager())
			{
				var appPool = serverManager.ApplicationPools[appPoolName];
				if (appPool != null && appPool.State != ObjectState.Stopped)
				{
					appPool.Stop();
				}

				var site = serverManager.Sites.SingleOrDefault(x => x.Name == siteName);
				if (site != null)
				{
					serverManager.Sites.Remove(site);
				}

				if (appPool != null)
				{
					serverManager.ApplicationPools.Remove(appPool);
				}

				serverManager.CommitChanges();
			}
		}

		protected override object[] GetState()
		{
			return new object[] { siteName, appPoolName };
		}

		protected override void ReadState(object[] state)
		{
			siteName = (string)state[0];
			appPoolName = (string)state[1];
		}

		string siteName;
		string appPoolName;
	}
}
