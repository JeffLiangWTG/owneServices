using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ZArchitecture.GUI.JSInterop
{
	public static class Services
	{
		public static void AddJSInteropServices(this IServiceCollection services)
		{
			services.AddScoped<IDropFormJSInterop, DropFormJSInterop>();
			services.AddScoped<IFilterStripJSInterop, FilterStripJSInterop>();
			services.AddScoped<IAutoCompleteTextBoxJSInterop, AutoCompleteTextBoxJSInterop>();
			services.AddScoped<IWebBrowserJSInterop, WebBrowserJSInterop>();
			services.AddScoped<ISpellCheckerJSInterop, SpellCheckerJSInterop>();
		}
	}
}
