using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using CargoWise.Main.Navigation;
using MenuItem = CargoWise.Main.Navigation.MenuItem;

namespace CargoWise.Main.Navigation;

public class MenuService : IMenuService
{
	protected readonly MenuSection menuSection;
	protected readonly IWinzorControl winzorControl;

	public string NoItemsLabelText { get; set; }
	public string DisplayName { get; set; }
	public ObservableCollection<MenuItem>? Items { get; set; }

	public MenuService(MenuSection section, IWinzorControl control, string noItemsLabelText)
	{
		menuSection = section;
		winzorControl = control;
		NoItemsLabelText = noItemsLabelText;
		DisplayName = menuSection.DisplayName;
		Items = menuSection.Items;

		menuSection.PropertyChanged += OnPropertyValueChanged;
		menuSection.Items.CollectionChanged += OnItemsCollectionChanged;
	}

	public void Dispose()
	{
		menuSection.PropertyChanged -= OnPropertyValueChanged;
		menuSection.Items.CollectionChanged -= OnItemsCollectionChanged;
	}

	public async Task PerformClickAsync(MenuItem item)
	{
		await winzorControl.InvokeAsync(() => item.LinkAction?.Execute(null));
	}

	public async Task PerformFavoriteClickAsync(MenuItem item)
	{
		await winzorControl.InvokeAsync(() => item.FavoriteAction?.Execute(null));
	}

	public async Task PerformRightClickAsync(WebMouseEventArgs e, MenuItem item)
	{
		winzorControl.UpdateMouseData(e);
		await winzorControl.InvokeAsync(() => item.LinkRightClickAction?.Execute(null));
	}

	void OnPropertyValueChanged(object? sender, PropertyChangedEventArgs e)
	{
		winzorControl.NotifyStateChanged();
	}

	void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		winzorControl.NotifyStateChanged();
	}
}
