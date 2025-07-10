using System;
using System.Collections.ObjectModel;
using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class CountToVisibilityConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new CountToVisibilityConverter();
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(0, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(0, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(2, typeof(object), "5", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(12, typeof(object), "13", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(1, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(1, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(5, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert("test", typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Use Default", Visibility.Visible, converter.Convert(1, typeof(object), "one", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Use Default", Visibility.Visible, converter.Convert(1, typeof(object), 1, System.Globalization.CultureInfo.CurrentCulture));

		converter.ShouldInvertResult = true;
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(0, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(2, typeof(object), "5", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(1, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(5, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertOnCollection()
	{
		var converter = new CountToVisibilityConverter();
		var collection1 = new ObservableCollection<int> { 1 };
		var collection3 = new ObservableCollection<int> { 1, 2, 3 };
		var collectionEmpty = new ObservableCollection<int>();
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(collectionEmpty, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(collectionEmpty, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(collection3, typeof(object), "5", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(collection1, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(collection1, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(collection3, typeof(object), "1", System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new CountToVisibilityConverter();
		AssertExceptionThrown(typeof(NotImplementedException), () => converter.ConvertBack(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
