using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NavBarToolStrip.razor")]
public partial class NavBarToolStrip : CwnComponentBase, IDisposable
{
	[CascadingParameter]
	public INavBarToolStripService? Service { get; set; }

	[Inject]
	public INavBarToolStripMenuInterop? MenuJsInterop { get; set; }

	string GetStripTitle(PopupMenu popupMenu)
	{
		return popupMenu?.Title != null ? popupMenu.Title.Replace("&", "") : string.Empty;
	}

	Icon? GetStripIcon(PopupMenu popupMenu)
	{
		return Service?.GetStripIcon(popupMenu);
	}

	EventHandler? _popupMenusChangedHandler;
	PopupMenu? _optionMenu;

	async Task ViewModel_OnPopupMenusChanged(object? sender, EventArgs e)
	{
		await InvokeAsync(StateHasChanged);
	}
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if(!firstRender)
		{
			return;
		}

		if (Service != null && Service.PopupMenus != null)
		{
			_optionMenu = Service.PopupMenus.FirstOrDefault(x => x.Title == Service.OptionMenuTitle);
			if (_optionMenu != null)
			{
				_popupMenusChangedHandler = async (sender, e) => await ViewModel_OnPopupMenusChanged(sender, e);
				_optionMenu.PopupMenuEvent.OnSubMenusChanged += _popupMenusChangedHandler;
			}
		}

		if (MenuJsInterop != null)
		{
			await MenuJsInterop.Setup();
		}
	}

	public void Dispose()
	{
		if (_popupMenusChangedHandler != null && _optionMenu != null)
		{
			_optionMenu.PopupMenuEvent.OnSubMenusChanged -= _popupMenusChangedHandler;
		}
	}
}
