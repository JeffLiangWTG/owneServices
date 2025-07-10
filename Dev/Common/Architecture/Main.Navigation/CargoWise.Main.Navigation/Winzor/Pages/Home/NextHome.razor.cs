using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Windows.UI;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

public partial class NextHome : CwnComponentBase
{
	[CascadingParameter(Name = "MainPageModelService")]
	public IMainPageModelService? MainPageModelService { get; set; }

	[Parameter]
	public Theme? Theme { get; set; }

	protected string Classname => new CssBuilder()
		.AddClass("cwn-home")
		.AddClass(Class)
		.Build();

	public string RightContainerClassName => new CssBuilder()
		.AddClass("cwn-home__right-container", IsRightContentVisible)
		.Build();
	[Parameter]
	public bool IsRightContentVisible { get; set; } = true;

	[Parameter]
	public string? ToggleRightContentVisibilityButtonToolTip { get; set; }

	[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NextHome.razor")]

	Task ToggleView()
	{
		IsRightContentVisible = !IsRightContentVisible;
		if (MainPageModelService != null && MainPageModelService.NavigationViewModel != null)
		{
			MainPageModelService.NavigationViewModel.IsRightContentVisible = IsRightContentVisible;
			ToggleRightContentVisibilityButtonToolTip = MainPageModelService.NavigationViewModel.ToggleRightContentVisibilityButtonTooltip;
		}
		return Task.CompletedTask;
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		IsRightContentVisible = MainPageModelService?.NavigationViewModel?.IsRightContentVisible ?? true;
		ToggleRightContentVisibilityButtonToolTip = MainPageModelService?.NavigationViewModel?.ToggleRightContentVisibilityButtonTooltip;
	}
}
