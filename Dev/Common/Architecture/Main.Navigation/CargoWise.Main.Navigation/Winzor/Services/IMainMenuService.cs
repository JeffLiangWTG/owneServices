using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.ViewModels;

namespace CargoWise.Main.Navigation;
public interface IMainMenuService
{
	ObservableCollection<NavigationMenuViewModel> MainMenuCategories { get; }

	NavigationMenuViewModel SelectedCategory { get; }

	IMenuSection SelectedItem { get; }

	ObservableCollection<IMenuSection> MenuSections { get; }

	ObservableCollection<IMenuSection> Subsections { get; }

	void UpdateSelectedCategory(NavigationMenuViewModel category);

	void UpdateSelectedMenuSection(IMenuSection menuSection);

	Task PerformClickAsync(MenuItem item);

	Task PerformFavoriteClickAsync(MenuItem item);
}
