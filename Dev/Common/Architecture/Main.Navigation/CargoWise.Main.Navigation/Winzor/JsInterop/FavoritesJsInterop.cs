using System.Threading.Tasks;
using WinzorFramework.JSInterop;

namespace CargoWise.Main.Navigation;

public interface IFavoritesJsInterop
{
	Task SetupTooltip();
}

public class FavoritesJsInterop : JSInteropBase, IFavoritesJsInterop
{
	public FavoritesJsInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWise.Main.Navigation/js/favoriteTooltip.js", fileVersionHash)
	{
	}

	public async Task SetupTooltip()
	{
		await InvokeJsAsync("setupTooltip");
	}
}

