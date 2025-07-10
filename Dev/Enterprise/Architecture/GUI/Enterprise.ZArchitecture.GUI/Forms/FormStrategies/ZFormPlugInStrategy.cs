using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IFormPlugInsProvider
	{
		PlugIns PlugIns { get; }
		ControllerID PlugInIDToSelectOnLoaded { get; }
		ZTabControl TopLevelTabControl { get; }
		Menu GetMenuForPlugIn(ControllerID controllerID);
	}

	public static class ZFormPlugInStrategy
	{
		public static void SetupPlugInsBeforeLoad(IFormPlugInsProvider plugInsProvider)
		{
			// these actions must be performed before the form is shown for performance
			if (plugInsProvider.TopLevelTabControl != null)
			{
				plugInsProvider.PlugIns.AddPlugInTabPages(plugInsProvider.TopLevelTabControl);
			}

			InsertPlugInMenuItems(plugInsProvider, plugInsProvider.PlugIns);

			plugInsProvider.PlugIns.SynchronisePlugInWithVisibleTabPage();
		}

		public static void SetupPluginsAfterLoad(IFormPlugInsProvider plugInsProvider)
		{
			if (plugInsProvider.PlugInIDToSelectOnLoaded != null)
			{
				plugInsProvider.PlugIns.SelectPlugInTabPage(plugInsProvider.PlugInIDToSelectOnLoaded);
			}
		}

		internal static void InsertPlugInMenuItems(IFormPlugInsProvider plugInsProvider, PlugIns plugIns)
		{
			if (plugIns.Instances.Length > 0)
			{
				foreach (var plugInMenuInfo in plugIns.TopLevelMenus)
				{
					var menuForPlugIn = plugInsProvider.GetMenuForPlugIn(plugInMenuInfo.ID);

					if (menuForPlugIn != null)
					{
						var addIndex = menuForPlugIn.MenuItems.Count;
						var helpMenuItem = menuForPlugIn.MenuItems[ZFormMenuStrategy.HelpMenuItemName];
						if (helpMenuItem != null)
						{
							addIndex = menuForPlugIn.MenuItems.IndexOf(helpMenuItem);
						}

						for (var i = 0; i < plugInMenuInfo.MenuItems.Length; i++)
						{
							if (!menuForPlugIn.MenuItems.Contains(plugInMenuInfo.MenuItems[i]))
							{
								menuForPlugIn.MenuItems.Add(addIndex + i, plugInMenuInfo.MenuItems[i]);
							}
						}
					}
				}
			}
		}
	}
}
