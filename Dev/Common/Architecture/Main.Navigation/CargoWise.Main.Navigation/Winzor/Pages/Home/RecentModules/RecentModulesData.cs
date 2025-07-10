using System.Collections.ObjectModel;
using CargoWise.Main.Navigation;

namespace CargoWise.Main.Navigation.Pages;

public class RecentModulesData
{
	public string NoItemsMessage { get; set; } = string.Empty;
	public string DisplayName { get; internal set; } = string.Empty;
	public ObservableCollection<MenuItem> Items { get; internal set; } = [];
}
