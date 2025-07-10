using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in RecentModules.razor")]
public partial class SearchResultSection
{
	[CascadingParameter]
	IQuickSearchService? Service { get; set; }

	[Parameter]
	public string Title { get; set; } = string.Empty;

	[Parameter]
	public SectionType SectionType { get; set; }

	[Parameter]
	public IEnumerable<MenuItem>? Items { get; set; }

	[Parameter]
	public EventCallback<MenuItem> OnMenuItemClick { get; set; }

	Icon GetStartIcon()
	{
		switch (SectionType)
		{
			case SectionType.Favorite:
				return Icon.StarFilled;
			case SectionType.RecentItem:
				return Icon.History;
			default:
				return Icon.Vector;
		}
	}

	async Task OnClickAsync(MenuItem item)
	{
		await OnMenuItemClick.InvokeAsync(item);
		await (Service?.PerformClickAsync(item) ?? Task.CompletedTask);
	}

	async Task OnRightClickAsync(MouseEventArgs e, MenuItem item) => await (Service?.PerformRightClickAsync(e, item) ?? Task.CompletedTask);
}
