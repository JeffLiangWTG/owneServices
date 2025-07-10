#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.JSInterop;
namespace Enterprise.ZArchitecture.GUI;

public partial class ZAutoCompleteTextBox
{
	readonly DotNetObjectReference<ZAutoCompleteTextBox> dotNetObjectReference;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);

		if (firstRender && AutocompleteManager != null)
		{
			await (Interop?.InitializeAsync(ElementReference, dotNetObjectReference,
				AutocompleteManager.MagicChar, selectedItems.Select(i => i.Code)) ?? Task.CompletedTask);
		}
	}

	string currentWord;

	string GetCurrentWord() => currentWord;

	void OnAfterCurrentWordUpdated(string oldWord)
	{
		if (AutocompleteManager == null)
		{
			return;
		}

		if (currentWord == null || (string.IsNullOrEmpty(currentWord) && string.IsNullOrEmpty(oldWord)))
		{
			HideDropMenu();
		}

		if (DropMenuIsOpen && currentWord != null)
		{
			dropForm.RefreshList();
		}
		else if (currentWord != null)
		{
			ShowDropMenu();
		}
	}

	[JSInvokable]
	public async Task UpdateCurrentWordAsync(string currentWord)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			var oldWord = this.currentWord;
			this.currentWord = currentWord;
			OnAfterCurrentWordUpdated(oldWord);
		});
	}

	public new virtual IAutoCompleteTextBoxJSInterop? Interop => GetJSInterop<IAutoCompleteTextBoxJSInterop>();
}
