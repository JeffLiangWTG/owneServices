using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;
using DragEventArgs = Microsoft.AspNetCore.Components.Web.DragEventArgs;

namespace CargoWise.Main.Navigation.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in Favorites.razor")]
public partial class Favorites : CwnComponentBase
{
	[CascadingParameter(Name = "FavoritesService")]
	public IFavoritesService? Service { get; set; }

	bool IsAnyItems => Service?.Items?.Any() ?? false;

	[Inject]
	IFavoritesJsInterop? JS { get; set; }

	bool isCompressView;

	Task ToggleView()
	{
		isCompressView = !isCompressView;
		return Task.CompletedTask;
	}

	MenuItem? draggedItem;
	void OnDragStart(WebDragEventArgs e, MenuItem item)
	{
		draggedItem = item;
	}

	// Triggered when an item is dropped
	async Task OnDropAsync(DragEventArgs e, MenuItem targetItem)
	{
		if (draggedItem != targetItem && draggedItem != null && Service?.Items != null)
		{
			// Reorder the list
			var draggedIndex = Service.Items.IndexOf(draggedItem);
			var targetIndex = Service.Items.IndexOf(targetItem);

			await Service.DropAsync(draggedItem, targetIndex, targetIndex <= draggedIndex);
			draggedItem = null;
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && JS != null)
		{
			await JS.SetupTooltip();
		}
	}
}
