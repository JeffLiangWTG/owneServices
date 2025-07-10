using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IFilterGridMenuItemProvider
	{
		IEnumerable<MenuItem> GetMenuItems(ZFilterGridModule module);
	}

	public interface IFilterGridTopLevelMenuItemProvider : IFilterGridMenuItemProvider
	{
		bool TryGetButtonDetail(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip);
	}
}
