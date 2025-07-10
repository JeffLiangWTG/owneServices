using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using CargoWiseNext.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in HeadNav.razor")]
public partial class HeadNav
{
	[CascadingParameter]
	public IMainMenuService? Service { get; set; }

	[CascadingParameter]
	public KeyboardService? KeyboardService { get; set; }

	[Inject]
	public IPopoverService? PopoverService { get; set; }

	protected ElementReference ElementReference { get; set; }

	[Parameter]
	public string? SearchPopoverId { get; set; }

	[Parameter]
	public NavigationViewModel? NavigationViewModel { get; set; }

	protected override void OnInitialized()
	{
		base.OnInitialized();
		if (KeyboardService is not null)
		{
			KeyboardService.OnKeyDown += HandleKeyDown;
		}
	}

	public void Dispose()
	{
		if (KeyboardService is not null)
		{
			KeyboardService.OnKeyDown -= HandleKeyDown;
		}
	}

	bool isMouseDownOnButton;

#if DEBUG
	public
#endif
	bool isMainNavVisible { get; set; }

	void HandleFocusOut()
	{
		// Don't close MainNav when we click HeadNav!
		if (isMouseDownOnButton)
		{
			isMouseDownOnButton = false;
			return;
		}

		isMainNavVisible = false;
	}

	void HandleCategoryButtonMouseDown(NavigationMenuViewModel category)
	{
		// Don't close MainNav when we click HeadNav!
		if (isMainNavVisible)
		{
			isMouseDownOnButton = true;
		}

		UpdateMainNav(category);
	}

	void UpdateMainNav(NavigationMenuViewModel category)
	{
		Service?.UpdateSelectedCategory(category);
		isMainNavVisible = true;
		PopoverService?.HideAsync(SearchPopoverId);
	}

	string GetClassName(NavigationMenuViewModel category)
	{
		var classname = "cwn-button--tooltip";

		if (Service is null)
		{
			return classname;
		}

		classname += (category == Service.SelectedCategory && isMainNavVisible) ? " cwn-button--selected" : string.Empty;

		return classname;
	}

	internal void HandleKeyDown(object? sender, WebKeyboardEventArgs e)
	{
		var wasCtrlShortcutPerformed = HandleCtrlShortcut(sender, e);
		if (!wasCtrlShortcutPerformed)
		{
			HandleEscapeKeyPress(sender, e);
		}
	}

	bool HandleCtrlShortcut(object? sender, WebKeyboardEventArgs e)
	{
		if (Service != null && e.CtrlKey)
		{
			var key = e.Key;
			foreach (var category in Service.MainMenuCategories)
			{
				if (category.ShortcutIndex.ToString() == key)
				{
					UpdateMainNav(category);
					return true;
				}
			}
		}

		return false;
	}

#if DEBUG
	public
#endif
	void HandleEscapeKeyPress(object? sender, KeyboardEventArgs e)
	{
		if (!isMainNavVisible || NavigationViewModel == null)
		{
			return;
		}

		if (e.Key == "\u001b")
		{
			isMainNavVisible = false;
			NavigationViewModel.HideAllMenus();
		}
	}

	string GetButtonShortcut(NavigationMenuViewModel category) => $"Ctrl+{category.ShortcutIndex}";

	void SetMainNavVisible()
	{
		isMainNavVisible = true;
	}
}
