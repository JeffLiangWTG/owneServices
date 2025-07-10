using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.Pages;

namespace CargoWise.Main.Navigation;

class RecentModulesService : IRecentModulesService
{
	readonly MenuSection recentModulesSection;
	readonly IWinzorControl winzorControl;
	readonly string noItemsMessage;

	public RecentModulesService(MenuSection recentModulesSection, IWinzorControl winzorControl, string noItemsMessage)
	{
		this.recentModulesSection = recentModulesSection;
		this.winzorControl = winzorControl;

#pragma warning disable CS8622
		recentModulesSection.PropertyChanged += OnPropertyValueChanged;
		recentModulesSection.Items.CollectionChanged += OnItemsCollectionChanged;
#pragma warning restore CS8622
		this.noItemsMessage = noItemsMessage;
	}

	public void Dispose()
	{
#pragma warning disable CS8622
		recentModulesSection.PropertyChanged -= OnPropertyValueChanged;
		recentModulesSection.Items.CollectionChanged -= OnItemsCollectionChanged;
#pragma warning restore CS8622
	}

	public async Task<RecentModulesData> GetRecentModulesDataAsync()
	{
		RecentModulesData data = new();
		await winzorControl.InvokeAsync(() =>
		{
			data = new RecentModulesData
			{
				NoItemsMessage = noItemsMessage,
				DisplayName = recentModulesSection.DisplayName,
				Items = recentModulesSection.Items,
			};
		});

		return data;
	}

	public async Task PerformClickAsync(MenuItem item)
	{
		await winzorControl.InvokeAsync(() => item.LinkAction.Execute(null));
	}

	public async Task PerformRightClickAsync(WebMouseEventArgs e, MenuItem item)
	{
		winzorControl.UpdateMouseData(e);
		await winzorControl.InvokeAsync(() => item.LinkRightClickAction.Execute(null));
	}

	public async Task PerformFavoriteClickAsync(MenuItem item)
	{
		await winzorControl.InvokeAsync(() => item.FavoriteAction.Execute(null));
	}

	void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
	{
		winzorControl.NotifyStateChanged();
	}

	void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		winzorControl.NotifyStateChanged();
	}
}
