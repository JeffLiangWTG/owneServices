using CargoWise.Main.Navigation.JsInterop;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Main.Navigation;

public static class Services
{
	public static IServiceCollection AddCargoWiseMainNavigation(this IServiceCollection services) => services
		.AddScoped<IQuickSearchJsInterop, QuickSearchJsInterop>()
		.AddScoped<IFavoritesJsInterop, FavoritesJsInterop>()
		.AddScoped<INavBarToolStripMenuInterop, NavBarToolStripMenuJSInterop>();
}
