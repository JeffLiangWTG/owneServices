using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;

namespace CargoWise.Main.Navigation;

public class NavBarToolStripService : INavBarToolStripService
{
	readonly NavigationViewModel NavigationViewModel;

	public ObservableCollection<PopupMenu>? PopupMenus => NavigationViewModel?.PopupMenus;

	public NavBarToolStripService(NavigationViewModel navigationViewModel)
	{
		NavigationViewModel = navigationViewModel;
	}

	public Icon GetStripIcon(PopupMenu popupMenu)
	{
		var title = popupMenu?.Title;
		if (title is null)
		{
			return Icon.Module;
		}
		if (title == NavigationViewModel.OptionMenuTitle)
		{
			return Icon.Settings;
		}
		else if (title == NavigationViewModel.HelpMenuTitle)
		{
			return Icon.Help;
		}
		else if (title == NavigationViewModel.TestingMenuTitle)
		{
			// now we don't have a testing menu in winzor due to NUnit Test Runner
			return Icon.Module;
		}
		return Icon.Module;
	}

	public string OptionMenuTitle => NavigationViewModel.OptionMenuTitle;
}
