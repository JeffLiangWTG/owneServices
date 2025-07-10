using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;
public partial class RecentItems : CwnComponentBase
{
	[CascadingParameter(Name = "RecentItemsService")]
	public IMenuService? Service { get; set; }

	protected string Classname => new CssBuilder()
		.AddClass("cwn-recent-items")
		.AddClass(Class)
		.Build();
}
