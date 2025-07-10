using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in MyTasks.razor")]
public partial class MyTasks : CwnComponentBase
{
	[Parameter]
	public IMyTasksViewModel? MyTasksViewModel { get; set; }

	[CascadingParameter(Name = "MainPageModelService")]
	public IMainPageModelService? MainPageModelService { get; set; }

	public string Classname => new CssBuilder()
		.AddClass("cwn-mytasks")
		.AddClass(Class)
		.Build();

	async Task RefreshMyTasksClickAsync()
	{
		if (MainPageModelService != null)
		{
			await MainPageModelService.InvokeAsync(() =>
			{
				MyTasksViewModel?.Refresh();
			});
		}
	}

	async Task TitleClickAsync()
	{
		await (MainPageModelService?.InvokeAsync(
			() => MyTasksViewModel?.OpenTaskFormCommand?.Execute(null)) ?? Task.CompletedTask);
	}

#pragma warning disable IDE0051

	async Task ClickAsync(IMyTasksItemViewModel item)
	{
		if (MainPageModelService != null)
		{
			await MainPageModelService.InvokeAsync(() =>
			{
				item.LinkAction?.Execute(null);
			});
		}
	}
#pragma warning restore IDE0051
}
