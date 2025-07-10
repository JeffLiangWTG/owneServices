using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DocumentEngine.GUI.JSInterop
{
	public static class Services
	{
		public static void AddJSInteropServices(this IServiceCollection services)
		{
			services.AddScoped<IFlexCelPreviewJSInterop, FlexCelPreviewJSInterop>();
		}
	}
}
