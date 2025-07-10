using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NavBarMenuItem.razor")]
[SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Used in NavBarMenuItem.razor")]
public partial class NavBarMenuItem : CwnComponentBase
{
	/// <summary>
	/// Item, type of PopupMenu
	/// </summary>
	[Parameter]
	public PopupMenu? Item { get; set; }

	readonly Guid ItemId = Guid.NewGuid();

	string GetStripTitle(PopupMenu popupMenu)
	{
		return popupMenu?.Title != null ? popupMenu.Title.Replace("&", "") : string.Empty;
	}

	async Task ItemClickAsync(PopupMenu item)
	{
		if (MainPageInvokeService != null)
		{
			await MainPageInvokeService.InvokeAsync(() =>
			{
				item.Command?.Execute(null);
			});
		}
	}

	/// <summary>
	/// MainPageInvokeService
	/// </summary>
	[CascadingParameter(Name = "MainPageInvokeService")]
	public IMainPageInvokeService? MainPageInvokeService { get; set; }

	bool IsChangeBranch
	{
		get
		{
			if (Item?.Title == ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartmentClose",
					"Change Company/&Branch/Dept. (Close Forms)")
				|| Item?.Title == ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartmentSaveAndClose",
					"Change Company/&Branch/Dept. (Save/Close Forms)")
				|| Item?.Title == ResString.GetMultilingualString("MenuItem.Main.ChangeBranchDepartment",
					"Change Company/&Branch/Dept. (Manually Close Forms)"))
			{
				return true;
			}

			return false;
		}
	}

	List<string> GetShortcutKeys(string gestureText)
	{
		if (string.IsNullOrEmpty(gestureText))
		{
			return new List<string>();
		}

		return gestureText.Split("+").ToList();
	}
}


