using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation.Pages;

public partial class MainNav
{
	const int MaxListItemCount = 22;

	[CascadingParameter]
	public IMainMenuService? Service { get; set; }

	[Parameter]
	public string? PopoverID { get; set; }

	[Parameter]
	public bool IsVisible { get; set; }

	[Parameter]
	public EventCallback OnBlur { get; set; }

	[Parameter]
	public EventCallback OnVisibleCallback { get; set; }

	protected ElementReference ElementReference { get; set; }

	bool isMouseDownOnButton;

	protected async Task OnBlurHandlerAsync()
	{
		if (!isMouseDownOnButton && OnBlur.HasDelegate)
		{
			await OnBlur.InvokeAsync();
		}

		isMouseDownOnButton = false;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		await ElementReference.FocusAsync();
	}

	protected string Classname => new CssBuilder()
		.AddClass("cwn-mainnav")
		.AddClass("cwn-mainnav--visible", IsVisible)
		.Build();

	protected string Stylename => new StyleBuilder()
		.Build() ?? string.Empty;

	void OnKeyPress(KeyboardEventArgs obj)
	{
		var section = Service?.MenuSections.FirstOrDefault(s => s.Letter.Equals(obj.Key, StringComparison.OrdinalIgnoreCase));
		if (section != null)
		{
			Service?.UpdateSelectedMenuSection(section);
		}
	}

	async Task AccessKeyFocusCallback(IMenuSection section)
	{
		Service?.UpdateSelectedMenuSection(section);
		if (OnVisibleCallback.HasDelegate)
		{
			await OnVisibleCallback.InvokeAsync();
		}
	}

	List<List<Object>>? GetRenderItems()// an Ul can contain multiple sections, each section can contain multiple items
	{
		if (Service?.Subsections is not null)
		{
			List<object>? ul = null;
			var result = new List<List<object>>();
			foreach (var menuSection in Service.Subsections)
			{
				var subSection = (MenuSection)menuSection;
				if (ul == null ||
					ul.Count + 1 + subSection.Items.Count > MaxListItemCount)// The rest of the Ul can not fit in the section, create a new one
				{
					ul = new List<object>();
					result.Add(ul);
				}

				ul?.Add(subSection);
				foreach (var menuItem in subSection.Items)
				{
					if(ul?.Count < MaxListItemCount )
					{
						ul.Add(menuItem);
					}
					else // ul is full
					{
						ul = [new MenuItem(string.Empty, null), menuItem ];
						result.Add(ul);
					}
				}
			}

			return result;
		}

		return null;
	}
}
