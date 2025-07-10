#if !WINZOR
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using CargoWise.Main.Navigation;
using NUnit.Framework;

namespace CargoWise.Main.Test.Navigation.Converters;

class SnapshotsVisibilityConverterTest : TestCase
{
	public void TestConvertResult()
	{
		var converter = new SnapshotsVisibilityConverter();
		var result = converter.Convert(null, typeof(IList), null, null);
		AssertEquals(Visibility.Hidden, result);

		List<int> list = [];
		result = converter.Convert(list, typeof(IList), null, null);
		AssertEquals(Visibility.Hidden, result);

		list = [1];
		result = converter.Convert(list, typeof(IList), null, null);
		AssertEquals(Visibility.Visible, result);

		list = [1, 2, 3, 4];
		result = converter.Convert(list, typeof(IList), null, null);
		AssertEquals(Visibility.Visible, result);

		list = [1, 2, 3, 4, 5];
		result = converter.Convert(list, typeof(IList), null, null);
		AssertEquals(Visibility.Hidden, result);
	}
}
#endif
