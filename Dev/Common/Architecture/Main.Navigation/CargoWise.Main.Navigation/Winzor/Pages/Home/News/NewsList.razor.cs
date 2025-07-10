using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NewsList.razor")]
public partial class NewsList
{
	[Parameter]
	public List<INewsItemViewModel>? Items { get; set; }

	[Parameter]
	public INewsViewModel? Model { get; set; }
	async Task ItemClickAsync(INewsItemViewModel item)
	{
		if (MainPageModelService != null)
		{
			await MainPageModelService.InvokeAsync(() =>
			{
				Model?.ExecuteShowItemCommand(item);
			});
		}
	}

	[CascadingParameter(Name = "MainPageModelService")]
	public IMainPageModelService? MainPageModelService { get; set; }

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await Task.Run(LoadThumbnails);
			StateHasChanged();
		}
	}

	void LoadThumbnails()
	{
		if (Items != null)
		{
			foreach (var item in Items)
			{
				if (item.ThumbnailData != null && item.ThumbnailData.Length > 0)
				{
					item.ThumbnailUrl = $"data:image/png;base64,{Convert.ToBase64String(item.ThumbnailData)}";
				}
			}
		}
	}
}

