using System;
using System.Drawing;

using FlexCel.Pdf;

using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public class WatermarkForTesting : Watermark
	{
		public WatermarkForTesting(WatermarkHorizontalAlign hAlign, WatermarkVerticalAlign vAlign, float hOffset, float vOffset)
			: base(hAlign, vAlign, hOffset, vOffset, 0)
		{
		}

		public PointF MockGetAlign(SizeF textSize, SizeF pageSize)
		{
			return base.GetAlign(textSize, pageSize);
		}

		public override void Draw(PdfWriter pdf, SizeF pageSize)
		{
			throw new NotImplementedException();
		}

		public override void Draw(Graphics gr, SizeF pageSize)
		{
			throw new NotImplementedException();
		}
	}

	public class WatermarkTest : TestCase
	{
		void AlignTest(float hOffset, float vOffset, WatermarkHorizontalAlign hAlign, WatermarkVerticalAlign vAlign, float expectedX, float expectedY)
		{
			var pageSize = new SizeF(100, 200);
			var textSize = new SizeF(20, 50);
			var watermark = new WatermarkForTesting(hAlign, vAlign, hOffset, vOffset);
			var result = watermark.GetAlign(textSize, pageSize);
			AssertEquals(expectedX, result.X);
			AssertEquals(expectedY, result.Y);
		}

		public void TestGetAlignZeroOffset()
		{
			AlignTest(0, 0, WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Top, 0, 0);
			AlignTest(0, 0, WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Middle, 0, 75);
			AlignTest(0, 0, WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Bottom, 0, 150);

			AlignTest(0, 0, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Top, 40, 0);
			AlignTest(0, 0, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 40, 75);
			AlignTest(0, 0, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Bottom, 40, 150);

			AlignTest(0, 0, WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Top, 80, 0);
			AlignTest(0, 0, WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Middle, 80, 75);
			AlignTest(0, 0, WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Bottom, 80, 150);
		}

		public void TestGetAlignWithOffset()
		{
			AlignTest(17, 22, WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Top, 17, 22);
			AlignTest(44, -23, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 40 + 44, 75 - 23);
			AlignTest(5, 8, WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Bottom, 80 - 5, 150 - 8);

			AlignTest(17.3f, 22.2f, WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Top, 17.3f, 22.2f);
		}
	}
}
