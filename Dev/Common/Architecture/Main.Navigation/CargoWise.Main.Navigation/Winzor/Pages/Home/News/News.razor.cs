using System.Collections.Generic;
using System.Linq;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

public partial class News : CwnComponentBase
{
	[Parameter]
	public INewsViewModel? NewsViewModel { get; set; }

	public string Classname => new CssBuilder()
		.AddClass("cwn-news")
		.AddClass(Class)
		.Build();

	public List<INewsItemViewModel> LoadSection(INewsSectionViewModel section)
	{
		if (NewsViewModel == null)
		{
			return new List<INewsItemViewModel>();
		}
		if (section.SectionName == NewsViewModel.NameAll)
		{
			return NewsViewModel.FilteredNewsItems.ToList();
		}
		else
		{
			return NewsViewModel.FilteredNewsItems.Where(t => t.SectionName == section.SectionName).ToList();
		}
	}
}
