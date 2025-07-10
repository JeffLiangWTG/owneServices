using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;

namespace CargoWise.Main.Navigation;

public class QuickSearchService : IQuickSearchService
{
	readonly IWinzorControl winzorControl;

	public QuickSearchService(IWinzorControl winzorControl)
	{
		this.winzorControl = winzorControl;
	}

	public async Task PerformClickAsync(MenuItem item)
	{
		await winzorControl.InvokeAsync(() => item.LinkAction.Execute(null));
	}

	public async Task PerformRightClickAsync(WebMouseEventArgs e, MenuItem item)
	{
		if(item is null) { return; }
		winzorControl.UpdateMouseData(e);
		await winzorControl.InvokeAsync(() => item.LinkRightClickAction?.Execute(null));
	}
}
