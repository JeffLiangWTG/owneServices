using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class RefreshManagerTest : TestCase
	{
		public void TestRefreshManager()
		{
			RefreshManager manager = new RefreshManager();
			Assert("Initial ForceRefresh", manager.ForceRefresh);
			Assert("Initial PageChanged", manager.PageChanged);
			Assert("Initial RefreshMagnifyingGlass", manager.RefreshMagnifyingGlass);

			manager.RegisterViewUpdated();
			Assert("After view updated : ForceRefresh", !manager.ForceRefresh);
			Assert("After view updated : PageChanged", manager.PageChanged);
			Assert("After view updated : RefreshMagnifyingGlass", manager.RefreshMagnifyingGlass);

			manager.PageChanged = false;
			Assert("After registered 2 : PageChanged", !manager.PageChanged);

			manager.RefreshMagnifyingGlass = false;
			Assert("After registered 2 : RefreshMagnifyingGlass", !manager.RefreshMagnifyingGlass);
		}

		public void TestRegisterViewUpdated()
		{
			RefreshManager manager = new RefreshManager();
			Assert("Initial ForceRefresh", manager.ForceRefresh);
			Assert("Initial PageChanged", manager.PageChanged);
			Assert("Initial RefreshMagnifyingGlass", manager.RefreshMagnifyingGlass);

			manager.RegisterViewUpdated();
			Assert("After view updated : ForceRefresh", !manager.ForceRefresh);
			Assert("After view updated : PageChanged", manager.PageChanged);
			Assert("After view updated : RefreshMagnifyingGlass", manager.RefreshMagnifyingGlass);
		}

		public void TestRegisterForceRefresh()
		{
			RefreshManager manager = new RefreshManager();
			Assert("Initial ForceRefresh", manager.ForceRefresh);
			Assert("Initial PageChanged", manager.PageChanged);
			Assert("Initial RefreshMagnifyingGlass", manager.RefreshMagnifyingGlass);

			manager.RegisterForceRefresh();
		}
	}
}
