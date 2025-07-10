
using NUnit.Framework;
namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(IsOnCriticalPathBorderThicknessConverter))]
	class IsOnCriticalPathBorderThicknessTest : ValueConverterTestCase<IsOnCriticalPathBorderThicknessConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(2.0, true);
			AssertConvertResult(1.3, false);
		}
	}
}
