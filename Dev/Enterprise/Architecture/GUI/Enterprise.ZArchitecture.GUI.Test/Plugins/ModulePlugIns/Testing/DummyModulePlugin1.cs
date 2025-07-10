using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.ModulePlugIn.Testing
{
	sealed class DummyModulePlugin1 : DummyModulePlugin
	{
		protected override MenuItem GetButtonGridMenuItemToAddCore(ZModuleButtonGrid grid)
		{
			return new ZMenuItem("Button Grid Menu Item 1");
		}
	}
}
