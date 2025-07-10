using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in RecentModules.razor")]
public partial class RecentModules : CwnComponentBase
{
	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Called in OnInitializedAsync")]
	RecentModulesData Data = new();

	[CascadingParameter]
	public IRecentModulesService? Service { get; set; }

	protected string Classname => new CssBuilder()
		.AddClass(Class)
		.Build();

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		if (Service is not null)
		{
			Data = await Service.GetRecentModulesDataAsync();
		}
	}

	void OnClickAsync(MenuItem item) => Service?.PerformClickAsync(item);

	async Task OnRightClickAsync(MouseEventArgs e, MenuItem item) => await (Service is not null ? Service.PerformRightClickAsync(e, item) : Task.CompletedTask);

	async Task OnFavoriteClickAsync(MenuItem item) => await (Service is not null ? Service.PerformFavoriteClickAsync(item) : Task.CompletedTask);
}
