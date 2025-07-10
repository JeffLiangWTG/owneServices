using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;
using static Enterprise.ZArchitecture.GUI.Tools.SpellChecker;

namespace Enterprise.ZArchitecture.GUI.JSInterop;

public interface ISpellCheckerJSInterop : IJSInterop
{
	Task EnableSpellCheckAsync(ElementReference elementReference, DotNetObjectReference<SpellChecker> dotNetObjectReference);
	Task DisableSpellCheckAsync(ElementReference elementReference);
}

public class SpellCheckerJSInterop : JSInteropBase, ISpellCheckerJSInterop
{
	public SpellCheckerJSInterop(IJSRuntime jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, "/_content/Enterprise.ZArchitecture.GUI/js/spellChecker.js", fileVersionHash)
	{
	}

	public async Task EnableSpellCheckAsync(ElementReference elementReference, DotNetObjectReference<SpellChecker> dotNetObjectReference)
	{
		await InvokeJsAsync("enableSpellCheck", elementReference, dotNetObjectReference);
	}

	public async Task DisableSpellCheckAsync(ElementReference elementReference)
	{
		await InvokeJsAsync("disableSpellCheck", elementReference);
	}
}
