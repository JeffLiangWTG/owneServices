using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(ArrowAppearanceToDashesConverter))]
	class ArrowAppearanceToDashesConverterTest : ValueConverterTestCase<ArrowAppearanceToDashesConverter>
	{
		public override void TestConvert()
		{
			AssertConvertResult(DashStyles.Solid.Dashes, "Blaaah");
			AssertConvertResult(DashStyles.Solid.Dashes, null);

			AssertConvertResult(DashStyles.Solid.Dashes, ArrowAppearance.Normal);
			AssertConvertResult(o => new[] { 5.0, 2.0 }.SequenceEqual((IEnumerable<double>)o), ArrowAppearance.Dashed);
			AssertConvertResult(DashStyles.DashDot.Dashes, ArrowAppearance.Dotted);
		}
	}
}
