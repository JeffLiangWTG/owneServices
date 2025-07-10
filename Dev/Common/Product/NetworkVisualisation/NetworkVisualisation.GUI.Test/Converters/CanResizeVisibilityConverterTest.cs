using System.Windows;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(CanResizeVisibilityConverter))]
	class CanResizeVisibilityConverterTest : ValueConverterTestCase<CanResizeVisibilityConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(Visibility.Visible, EntityState.None);
			AssertConvertResult(Visibility.Visible, EntityState.Inactive);

			AssertConvertResult(Visibility.Hidden, EntityState.Fixed);
			AssertConvertResult(Visibility.Hidden, EntityState.Fixed | EntityState.Approved);
		}
	}
}
