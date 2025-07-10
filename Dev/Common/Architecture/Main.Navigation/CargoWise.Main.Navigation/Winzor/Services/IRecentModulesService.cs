using System;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.Pages;

namespace CargoWise.Main.Navigation;

public interface IRecentModulesService : IDisposable
{
	Task<RecentModulesData> GetRecentModulesDataAsync();
	Task PerformClickAsync(MenuItem item);
	Task PerformRightClickAsync(WebMouseEventArgs e, MenuItem item);
	Task PerformFavoriteClickAsync(MenuItem item);
}
