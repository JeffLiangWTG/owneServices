using System.Windows.Media;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(IsOnCriticalPathBorderColorConverter))]
	class IsOnCriticalPathBorderColorTest : ValueConverterTestCase<IsOnCriticalPathBorderColorConverter>
	{
		public override void TestConvert()
		{
			AssertBrushConvertResult(Colors.Red, true);
			AssertBrushConvertResult(Colors.Black, false);
		}
	}
}
