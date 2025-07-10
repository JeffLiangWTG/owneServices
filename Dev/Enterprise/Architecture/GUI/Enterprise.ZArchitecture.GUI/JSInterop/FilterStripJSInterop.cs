using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI.JSInterop
{
	public interface IFilterStripJSInterop : IJSInterop
	{
		Task<int> GetFilterStripsPanelScrollTopAsync(ElementReference element);
	}

	public sealed class FilterStripJSInterop : JSInteropBase, IFilterStripJSInterop
	{
		public FilterStripJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, "/_content/Enterprise.ZArchitecture.GUI/js/filterStrip.js", fileVersionHash)
		{
		}

		public async Task<int> GetFilterStripsPanelScrollTopAsync(ElementReference element)
		{
			return Math.Max(Convert.ToInt32(await InvokeJsAsync<int>("getFilterStripsPanelScrollTop", element)), 0);
		}
	}
}
