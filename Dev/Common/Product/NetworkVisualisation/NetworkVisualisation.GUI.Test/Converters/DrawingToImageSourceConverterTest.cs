using System.Globalization;
using System.Windows.Media;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(DrawingToImageSourceConverter))]
	public class DrawingToImageSourceConverterTest : ValueConverterTestCase<DrawingToImageSourceConverter>
	{
		public override void TestConvert()
		{
			var drawing = new DrawingGroup();
			var converter = new DrawingToImageSourceConverter();
			var result = converter.Convert(drawing, typeof(ImageSource), null, CultureInfo.CurrentCulture);
			var imageSource = result as DrawingImage;
			AssertNotNull(imageSource);
			AssertEquals(drawing, imageSource.Drawing);
		}
	}
}
