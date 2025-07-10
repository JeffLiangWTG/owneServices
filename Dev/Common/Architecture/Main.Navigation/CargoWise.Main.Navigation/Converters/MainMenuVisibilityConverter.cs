using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Navigation.Converters;

public class MainMenuVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is NavigationMenuViewModel viewModel
			&& viewModel.Name != ModuleTreeLoaderConstant.Category.Jump.Name)
		{
			return Visibility.Visible;
		}

		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
