using System.Drawing;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualiserComponentBorderTest : TestCase
	{
		public void TestHasBorders()
		{
			var format = new CellFormat();
			var border = new VisualiserComponentBorder(Point.Empty, Size.Empty, format);
			AssertEquals("border.HasBorders", false, border.HasBorders);

			format.Borders.Bottom.BorderStyle = CellBorderStyle.Medium;
			AssertEquals("border.HasBorders", true, border.HasBorders);
		}
	}
}
