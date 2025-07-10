using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CargoWise.Main.Navigation.WPF;

public class SearchResultTemplateSelector : DataTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item is IEnumerable<SearchResultSection>)
		{
			return SearchDetailsTemplate;
		}
		else
		{
			var section = item as SearchResultSection;
			if (section != null)
			{
				switch (section.SectionType)
				{
					case SectionType.Favorite:
						return FavoriteItemResultsTemplate;

					case SectionType.RecentItem:
						return RecentItemResultsTemplate;

					case SectionType.RecentModule:
					case SectionType.Subcategory:
					case SectionType.Module:
					case SectionType.GlobalSearch:
						break;
				}
			}
		}
		return SearchItemResultsTemplate;
	}

	public DataTemplate SearchDetailsTemplate { get; set; }
	public DataTemplate SearchItemResultsTemplate { get; set; }
	public DataTemplate FavoriteItemResultsTemplate { get; set; }
	public DataTemplate RecentItemResultsTemplate { get; set; }
}

