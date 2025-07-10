
using NUnit.Framework;
namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(TooltipTextConverter))]
	class TooltipTextConverterTest : ValueConverterTestCase<TooltipTextConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(null, null);
			AssertConvertResult(null, string.Empty);
			AssertConvertResult("Something", "Something");
		}
	}
}
