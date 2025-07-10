using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;

namespace CargoWise.Main.Navigation;

public interface INavBarToolStripService
{
	ObservableCollection<PopupMenu>? PopupMenus { get; }

	Icon GetStripIcon(PopupMenu popupMenu);

	string OptionMenuTitle { get; }
}
