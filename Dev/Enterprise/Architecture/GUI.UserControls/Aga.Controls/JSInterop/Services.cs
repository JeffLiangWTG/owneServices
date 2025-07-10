using Microsoft.Extensions.DependencyInjection;

namespace Aga.Controls.JSInterop
{
	public static class Services
	{
		public static void AddJSInteropServices(this IServiceCollection services)
		{
			services.AddScoped<ITreeViewAdvJSInterop, TreeViewAdvJSInterop>();
		}
	}
}
