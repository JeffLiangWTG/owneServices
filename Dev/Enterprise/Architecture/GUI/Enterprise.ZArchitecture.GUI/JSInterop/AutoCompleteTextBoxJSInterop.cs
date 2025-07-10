using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI.JSInterop
{
	public interface IAutoCompleteTextBoxJSInterop : IJSInterop
	{
		Task InitializeAsync(ElementReference elementReference, DotNetObjectReference<ZAutoCompleteTextBox> dotNetObjectReference, char magicChar, IEnumerable<string> selectedTags);
		Task InsertTagAndUpdateTagsAsync(ElementReference elementReference, string tag, IEnumerable<string> selectedTags);
	}

	public sealed class AutoCompleteTextBoxJSInterop : JSInteropBase, IAutoCompleteTextBoxJSInterop
	{
		public AutoCompleteTextBoxJSInterop(IJSRuntime jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, "/_content/Enterprise.ZArchitecture.GUI/js/autoCompleteTextBox.js", fileVersionHash)
		{
		}

		public async Task InitializeAsync(ElementReference elementReference, DotNetObjectReference<ZAutoCompleteTextBox> dotNetObjectReference, char magicChar, IEnumerable<string> selectedTags)
		{
			await InvokeJsAsync("initialize", elementReference, dotNetObjectReference, magicChar, selectedTags);
		}

		public async Task InsertTagAndUpdateTagsAsync(ElementReference elementReference, string tag, IEnumerable<string> selectedTags)
		{
			await InvokeJsAsync("insertTagAndUpdateTags", elementReference, tag, selectedTags);
		}
	}
}
