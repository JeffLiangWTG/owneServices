using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(FontWeightConverter))]
	class FontWeightConverterTest : ValueConverterTestCase<FontWeightConverter>
	{
		public override void TestConvert()
		{
			var converter = new FontWeightConverter();
			var source = NodeFontWeight.SemiBold;

			var result = converter.Convert(source, typeof(TextAlignment), null, null);

			AssertEquals(FontWeights.SemiBold, result);
		}

		public void TestConvertBack()
		{
			var converter = new FontWeightConverter();
			var source = FontWeights.SemiBold;

			var result = converter.ConvertBack(source, typeof(NodeFontWeight), null, null);

			AssertEquals(NodeFontWeight.SemiBold, result);
		}
	}
}
