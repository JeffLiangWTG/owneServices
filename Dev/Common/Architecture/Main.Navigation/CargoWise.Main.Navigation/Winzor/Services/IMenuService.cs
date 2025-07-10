using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface IMenuService
{
	string DisplayName { get; set; }
	string NoItemsLabelText { get; set; }
	ObservableCollection<MenuItem>? Items { get; set; }
	Task PerformClickAsync(MenuItem item);
	Task PerformRightClickAsync(WebMouseEventArgs e, MenuItem item);
	Task PerformFavoriteClickAsync(MenuItem item);
}
