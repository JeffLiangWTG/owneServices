using System.Drawing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class PaintAreaCalculatorTest : TestCase
	{
		#region CalculateUnscaledSize

		public void TestCalculateUnscaledSize_EmptyPaintArea()
		{
			var paintArea = RectangleF.Empty;
			var scale = 1f;

			var res = PaintAreaCalculator.CalculateUnscaledSize(paintArea, scale);
			AssertEquals("unscaled size", res, SizeF.Empty);
		}

		public void TestCalculateUnscaledSize_IncorrectScale()
		{
			var paintArea = new RectangleF(0f, 0f, 100f, 20f);
			var scale = 0f;

			var res = PaintAreaCalculator.CalculateUnscaledSize(paintArea, scale);
			AssertEquals("unscaled size", res, SizeF.Empty);
		}

		public void TestCalculateUnscaledSize_DoNotApplyRounding()
		{
			var paintArea = new RectangleF(0f, 0f, 65.26857f, 21.33333f);
			var scale = 1f;

			var res = PaintAreaCalculator.CalculateUnscaledSize(paintArea, scale);
			Assert("unscaled Width", res.Width - 65.26857f <= 0.0001f);
			Assert("unscaled Height", res.Height - 21.33333f <= 0.0001f);
		}

		public void TestCalculateUnscaledSize_UnscaleDown()
		{
			var paintArea = new RectangleF(0f, 0f, 65.26857f, 21.33333f);
			var scale = 2f;

			var res = PaintAreaCalculator.CalculateUnscaledSize(paintArea, scale);
			Assert("unscaled Width", res.Width - 32.63428f <= 0.0001f);
			Assert("unscaled Height", res.Height - 10.66667f <= 0.0001f);
		}

		public void TestCalculateUnscaledSize_UnscaleUp()
		{
			var paintArea = new RectangleF(0f, 0f, 65.26857f, 21.33333f);
			var scale = 0.5f;

			var res = PaintAreaCalculator.CalculateUnscaledSize(paintArea, scale);
			Assert("unscaled Width", res.Width - 130.5371f <= 0.0001f);
			Assert("unscaled Height", res.Height - 42.66666f <= 0.0001f);
		}

		#endregion

		#region CalculateTextPaintAreaSize

		public void TestCalculateTextPaintAreaSize_EmptyFont()
		{
			var dpi = new PointF(1, 1);
			var cellSize = new SizeF(100f, 100f);
			var padding = new RectangleF(10f, 10f, 10f, 10f);
			var text = "Test String";

			var res = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, text, null, 1f, false);
			AssertEquals(res, SizeF.Empty);
		}

		public void TestCalculateTextPaintAreaSize_IncorrectFontSize()
		{
			var dpi = new PointF(1, 1);
			var cellSize = new SizeF(100f, 100f);
			var padding = new RectangleF(10f, 10f, 10f, 10f);
			var text = "Test String";
			var font = new Core.Font("Arial", 0f, FontStyle.Regular, Color.Empty, 0);

			var res = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, text, font, 1f, false);
			AssertEquals(res, SizeF.Empty);
		}

		public void TestCalculateTextPaintAreaSize_IncorrectZoom()
		{
			var dpi = new PointF(1, 1);
			var cellSize = new SizeF(100f, 100f);
			var padding = new RectangleF(10f, 10f, 10f, 10f);
			var text = "Test String";
			var font = new Core.Font("Arial", 1f, FontStyle.Regular, Color.Empty, 0);

			var res = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, text, font, 0f, false);
			AssertEquals(res, SizeF.Empty);
		}

		public void TestCalculateTextPaintAreaSize()
		{
			var dpi = new PointF(168, 168);
			var cellSize = new SizeF(60.6877937f, 76.7132339f);
			var padding = new RectangleF(0.72f, 0.72f, 0.72f, 0f);
			var font = new Core.Font("Arial", 7f, FontStyle.Regular, Color.Empty, 0);

			var res = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, "Test String", font, 1f, false);
			Assert(res.Width - 98.32669 <= 0.0001f);
			Assert(res.Height - 19.87f <= 0.0001f);

			var wrapTextRes = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, "Test String\r\nTest String", font, 1f, true);
			Assert(wrapTextRes.Width - 98.32669f <= 0.0001f);
			Assert(wrapTextRes.Height - 126.45903f <= 0.0001f);
		}

		public void TestCalculateTextPaintAreaSize_VerticalText_90()
		{
			var dpi = new PointF(168, 168);
			var cellSize = new SizeF(60.6877937f, 76.7132339f);
			var padding = new RectangleF(0.72f, 0.72f, 0.72f, 0f);
			var font = new Core.Font("Arial", 7f, FontStyle.Regular, Color.Empty, 90);

			var res = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, "Test String", font, 1f, false);
			Assert(res.Width - 19.87f <= 0.0001f);
			Assert(res.Height - 125.249435f <= 0.0001f);

			var wrapTextRes = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, "Test String\r\nTest String", font, 1f, true);
			Assert(wrapTextRes.Width - 99.53628f <= 0.0001f);
			Assert(wrapTextRes.Height - 125.24943f <= 0.0001f);
		}

		public void TestCalculateTextPaintAreaSize_VerticalText_180()
		{
			var dpi = new PointF(168, 168);
			var cellSize = new SizeF(60.6877937f, 76.7132339f);
			var padding = new RectangleF(0.72f, 0.72f, 0.72f, 0f);
			var font = new Core.Font("Arial", 7f, FontStyle.Regular, Color.Empty, 180);

			var res = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, "Test String", font, 1f, false);
			Assert(res.Width - 19.87f <= 0.0001f);
			Assert(res.Height - 125.249435f <= 0.0001f);

			var wrapTextRes = PaintAreaCalculator.CalculateTextPaintAreaSize(dpi, cellSize, padding, "Test String\r\nTest String", font, 1f, true);
			Assert(wrapTextRes.Width - 99.53628f <= 0.0001f);
			Assert(wrapTextRes.Height - 125.24943f <= 0.0001f);
		}

		#endregion
	}
}
