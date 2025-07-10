using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DocumentScanning.GUI.JSInterop
{
	public static class Services
	{
		public static void AddJSInteropServices(this IServiceCollection services)
		{
			services.AddScoped<IMagnifyFormJSInterop, MagnifyFormJSInterop>();
		}
	}
}
