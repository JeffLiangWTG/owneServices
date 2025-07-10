using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(TextAlignmentConverter))]
	class TextAlignmentConverterTest : ValueConverterTestCase<TextAlignmentConverter>
	{
		public override void TestConvert()
		{
			var converter = new TextAlignmentConverter();
			var source = NodeTextAlignment.Center;

			var result = converter.Convert(source, typeof(TextAlignment), null, null);

			AssertEquals(TextAlignment.Center, result);
		}

		public void TestConvertBack()
		{
			var converter = new TextAlignmentConverter();
			var source = TextAlignment.Center;

			var result = converter.ConvertBack(source, typeof(NodeTextAlignment), null, null);

			AssertEquals(NodeTextAlignment.Center, result);
		}
	}
}
