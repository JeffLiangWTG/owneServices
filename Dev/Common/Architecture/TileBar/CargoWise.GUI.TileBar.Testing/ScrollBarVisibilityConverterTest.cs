namespace CargoWise.GUI.TileBar.Testing
{
	using System.Windows.Controls;
	using CargoWise.Main.Navigation.ViewModels;
	using Enterprise.ZArchitecture.Core;
	using NUnit.Framework;

	public class ScrollBarVisibilityConverterTest : TestCase
	{
		public void TestConvert()
		{
			var converter = new ScrollBarVisibilityConverter();

			var showSearchTileBarViewModel = new NavigationMenuViewModel((NoResString)"Test", "Test", 3, true);
			AssertEquals("Visibility Auto", ScrollBarVisibility.Auto, converter.Convert(showSearchTileBarViewModel, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));

			var noShowSearchTileBarViewModel = new NavigationMenuViewModel((NoResString)"Test", "Test", 3, false);
			AssertEquals("Visibility Disabled", ScrollBarVisibility.Disabled, converter.Convert(noShowSearchTileBarViewModel, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		}
	}
}

