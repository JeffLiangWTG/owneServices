using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Windows.UI.JSInterop;

public static class Services
{
	public static void AddJSInteropServices(this IServiceCollection services)
	{
		services.AddScoped<IControlInformationOverlayJSInterop, ControlInformationOverlayJSInterop>();
		services.AddScoped<IControlHighlightJSInterop, ControlHighlightJSInterop>();
	}
}
