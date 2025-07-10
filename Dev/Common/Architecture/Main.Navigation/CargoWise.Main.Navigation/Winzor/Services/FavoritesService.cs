using System.Threading.Tasks;
using CargoWise.Main.Navigation;
using CargoWiseNext.Blazor.Components;

namespace CargoWise.Main.Navigation;

public class FavoritesService : MenuService, IFavoritesService
{
	public string DragToReorderText { get; set; }

	public string SwitchViewText => menuSection.SwitchViewText;

	public FavoritesService(MenuSection section, IWinzorControl control, string noItemsLabelText, string dragToReorderText) : base(section, control, noItemsLabelText)
	{
		DragToReorderText = dragToReorderText;
	}

	public async Task DropAsync(object data, int index = -1, bool insertAbove = false)
	{
		await winzorControl.InvokeAsync(() => menuSection.Drop(data, index, insertAbove));
	}
}
