
using NUnit.Framework;
namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(BooleanInverterConverter))]
	class BooleanInverterConverterTest : ValueConverterTestCase<BooleanInverterConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(true, false);
			AssertConvertResult(false, true);
		}
	}
}
