using System;
using System.Windows;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

public class MainMenuVisibilityConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new MainMenuVisibilityConverter();

		var invalidNavViewModel = new NavigationMenuViewModel((NoResString)ModuleTreeLoaderConstant.Category.Jump.Name, ModuleTreeLoaderConstant.Category.Jump.Name, 1, true);
		var validNavViewModel = new NavigationMenuViewModel((NoResString)"Not Jump", "Not Jump", 2, true);

		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(invalidNavViewModel, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(validNavViewModel, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert("test", typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(1, typeof(object), "one", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(null, typeof(object), 1, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new MainMenuVisibilityConverter();
		AssertExceptionThrown(typeof(NotImplementedException), () => converter.ConvertBack(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
