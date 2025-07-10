using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	sealed class DummyPluginWithGetNewTopMenuInPlugInIsExistingMenu : ZAlwaysLoadPlugIn
	{
		public DummyPluginWithGetNewTopMenuInPlugInIsExistingMenu(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			var topLevelMenu = Form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
			topLevelMenu.MenuItems.Add(new ZMenuItem(Name));
			return topLevelMenu;
		}

		public override string Name => "DummyPluginWithGetNewTopMenuInPlugInIsExistingMenu";

		protected internal override ZBool HasUserControl => false;

		protected override LicenceCheckpoint LicenceCheckPoint => (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow;
	}
}
