using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface IQuickSearchService
{
	Task PerformClickAsync(MenuItem item);
	Task PerformRightClickAsync(WebMouseEventArgs e, MenuItem item);
}
