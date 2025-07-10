using System.Windows;
using System.Windows.Controls;

namespace CargoWise.Main.Navigation.WPF;
public class MenuTemplateSelector : DataTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (item is MenuDetails)
		{
			return DetailsTemplate;
		}
		else
		{
			var section = item as MenuSection;
			if (section != null)
			{
				switch (section.Type)
				{
					case SectionType.Favorite:
						return FavoritesTemplate;

					case SectionType.RecentModule:
						return RecentTemplate;

					case SectionType.RecentItem:
						return RecentItemsTemplate;

					case SectionType.Subcategory:
					case SectionType.Module:
						break;
				}
			}
		}

		return SectionTemplate;
	}

	public DataTemplate SectionTemplate { get; set; }
	public DataTemplate DetailsTemplate { get; set; }
	public DataTemplate FavoritesTemplate { get; set; }
	public DataTemplate RecentTemplate { get; set; }
	public DataTemplate RecentItemsTemplate { get; set; }
}
