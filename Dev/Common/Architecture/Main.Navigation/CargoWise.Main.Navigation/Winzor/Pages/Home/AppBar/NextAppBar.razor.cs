using System;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;
#nullable enable
public partial class NextAppBar : CwnComponentBase
{
	[Parameter]
	public NavigationViewModel? NavigationViewModel { get; set; }

	string? searchPopoverId;
	public string SearchPopoverId => searchPopoverId ??= "search-popover-" + Guid.NewGuid();

	[Parameter]
	public Theme? Theme { get; set; }
}
