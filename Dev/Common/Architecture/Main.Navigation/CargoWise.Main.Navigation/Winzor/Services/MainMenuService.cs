using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.ViewModels;

namespace CargoWise.Main.Navigation;

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8603 // Possible null reference return.
public class MainMenuService : IMainMenuService
{
	readonly IWinzorControl winzorControl;

	readonly NavigationViewModel viewModel;

	public MainMenuService(NavigationViewModel viewModel, IWinzorControl winzorControl)
	{
		this.viewModel = viewModel;
		viewModel.PropertyChanged += ViewModel_PropertyChanged;
		this.winzorControl = winzorControl;
	}

	async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		await winzorControl.InvokeAsync(winzorControl.NotifyStateChanged);
	}

	public ObservableCollection<NavigationMenuViewModel> MainMenuCategories => viewModel.MainMenuCategories;

	public NavigationMenuViewModel SelectedCategory => viewModel.SelectedCategory;

	public IMenuSection SelectedItem => SelectedCategory?.SelectedItem;

	public ObservableCollection<IMenuSection> MenuSections => SelectedCategory?.MenuSections;

	public ObservableCollection<IMenuSection> Subsections => SelectedItem?.Subsections;

	public async Task PerformClickAsync(MenuItem item)
	{
		await winzorControl.InvokeAsync(() => item.LinkAction?.Execute(null));
	}

	public async Task PerformFavoriteClickAsync(MenuItem item)
	{
		item.IsInFavorites = !item.IsInFavorites;
		await winzorControl.InvokeAsync(() => item.FavoriteAction?.Execute(null));
	}

	public void UpdateSelectedCategory(NavigationMenuViewModel category)
	{
		viewModel.SelectedCategory = category;
		if (SelectedItem is null)
		{
			viewModel.SelectedCategory.SelectedItem = MenuSections.FirstOrDefault();
		}
	}

	public void UpdateSelectedMenuSection(IMenuSection menuSection)
	{
		viewModel.SelectedCategory.SelectedItem = menuSection;
	}
}
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
