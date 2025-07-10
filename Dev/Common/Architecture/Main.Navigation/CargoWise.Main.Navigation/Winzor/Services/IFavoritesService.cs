using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;
public interface IFavoritesService : IMenuService
{
	public string DragToReorderText { get; set; }

	public string SwitchViewText { get; }

	Task DropAsync(object data, int index = -1, bool insertAbove = false);
}
