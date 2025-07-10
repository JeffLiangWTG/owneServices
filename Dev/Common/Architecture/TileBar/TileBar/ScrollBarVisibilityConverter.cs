using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using CargoWise.Main.Navigation.ViewModels;

namespace CargoWise.GUI.TileBar
{
	public class ScrollBarVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value is NavigationMenuViewModel tileBarViewModel && tileBarViewModel.ShowSearch) ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}


