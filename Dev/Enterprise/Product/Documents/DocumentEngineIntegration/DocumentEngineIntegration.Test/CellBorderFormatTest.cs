using System.Drawing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineIntegration.Testing
{
	sealed class CellBorderFormatTest : TestCase
	{
		public void TestSetColor()
		{
			var borderFormat = new CellBorderFormat();
			borderFormat.Top.BorderStyle = CellBorderStyle.Thin;

			borderFormat.SetColor(Color.Red);
			AssertEquals("borderFormat.Top.BorderColor", Color.Red, borderFormat.Top.BorderColor);
		}

		public void TestGetCommonColor()
		{
			var borderFormat = new CellBorderFormat();
			borderFormat.Top.BorderStyle = CellBorderStyle.Thin;

			borderFormat.SetColor(Color.Red);
			AssertEquals("borderFormat.GetCommonColor()", Color.Red, borderFormat.GetCommonColor());
		}

		public void TestIsUsedColorsAllTheSame()
		{
			var borderFormat = new CellBorderFormat();
			borderFormat.Top.BorderStyle = CellBorderStyle.Thin;
			borderFormat.Bottom.BorderStyle = CellBorderStyle.Thin;

			borderFormat.SetColor(Color.Red);
			AssertEquals("borderFormat.IsUsedColorsAllTheSame", true, borderFormat.IsUsedColorsAllTheSame);

			borderFormat.Bottom.BorderColor = Color.Green;
			AssertEquals("borderFormat.IsUsedColorsAllTheSame", false, borderFormat.IsUsedColorsAllTheSame);
		}
	}
}
