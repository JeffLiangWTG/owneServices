using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			TestNameOfAGridElement((ZModuleButtonGrid)control, notifications);
		}
		internal static void TestNameOfAGridElement(ZModuleButtonGrid grid, INotifications notifications)
		{
			if (grid.NameOfAGridElement.IsEmpty())
			{
				notifications.AddError(ControlDescription.GetControlPath(grid) + " has an element grid without name.");
			}
		}
	}
}
