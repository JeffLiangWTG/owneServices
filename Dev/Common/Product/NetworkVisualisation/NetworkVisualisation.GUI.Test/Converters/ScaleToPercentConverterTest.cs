
using NUnit.Framework;
namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(ScaleToPercentConverter))]
	class ScaleToPercentConverterTest : ValueConverterTestCase<ScaleToPercentConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(50.0, 0.5);
			AssertConvertResult(100.0, 1.0);
		}

		public void TestConvertWithString()
		{
			var oneHundredAsString = "100";

			AssertConvertBackResult(1d, oneHundredAsString);

			var ninetyAsString = "90";

			AssertConvertBackResult(0.9d, ninetyAsString);
		}
	}
}
