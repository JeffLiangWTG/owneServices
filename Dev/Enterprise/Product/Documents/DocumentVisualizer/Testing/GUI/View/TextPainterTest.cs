using System;
using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class TextPainterTest : TestCase
	{
		#region PaintArea

		const float marginOfError = 0.01f;

		public void TestPaintAreaForTextWithNoPadding()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				RectangleF.Empty,
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 0);
			text.Wrap = true;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(0, 0, 96f, 19.2f);
			var actualPaintArea = painter.PaintArea;

			Assert("paint area",
				Math.Abs(expectedPaintArea.Left - actualPaintArea.Left) < marginOfError
				&& Math.Abs(expectedPaintArea.Right - actualPaintArea.Right) < marginOfError
				&& Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError
				&& Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
		}

		public void TestPaintAreaForVerticalTextWithNoPadding_90()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				RectangleF.Empty,
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 90);
			text.Wrap = true;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(0, 0, 96f, 19.2f);
			var actualPaintArea = painter.PaintArea;

			Assert("paint area",
				Math.Abs(expectedPaintArea.Left - actualPaintArea.Left) < marginOfError
				&& Math.Abs(expectedPaintArea.Right - actualPaintArea.Right) < marginOfError
				&& Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError
				&& Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
		}

		public void TestPaintAreaForVerticalTextWithNoPadding_180()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				RectangleF.Empty,
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 180);
			text.Wrap = true;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(0, 0, 96f, 19.2f);
			var actualPaintArea = painter.PaintArea;

			Assert("paint area",
				Math.Abs(expectedPaintArea.Left - actualPaintArea.Left) < marginOfError
				&& Math.Abs(expectedPaintArea.Right - actualPaintArea.Right) < marginOfError
				&& Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError
				&& Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
		}

		public void TestPaintAreaForTextWithPadding()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 0);
			text.Wrap = true;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(2.88f, 2.88f, 87.36f, 10.56f);
			var actualPaintArea = painter.PaintArea;

			Assert("paint area",
				Math.Abs(expectedPaintArea.Left - actualPaintArea.Left) < marginOfError
				&& Math.Abs(expectedPaintArea.Right - actualPaintArea.Right) < marginOfError
				&& Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError
				&& Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
		}

		public void TestPaintAreaForVerticalTextWithPadding_90()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 90);
			text.Wrap = true;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(2.88f, 2.88f, 87.36f, 10.56f);
			var actualPaintArea = painter.PaintArea;

			Assert("paint area",
				Math.Abs(expectedPaintArea.Left - actualPaintArea.Left) < marginOfError
				&& Math.Abs(expectedPaintArea.Right - actualPaintArea.Right) < marginOfError
				&& Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError
				&& Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
		}

		public void TestPaintAreaForVerticalTextWithPadding_180()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 180);
			text.Wrap = true;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(2.88f, 2.88f, 87.36f, 10.56f);
			var actualPaintArea = painter.PaintArea;

			Assert("paint area",
				Math.Abs(expectedPaintArea.Left - actualPaintArea.Left) < marginOfError
				&& Math.Abs(expectedPaintArea.Right - actualPaintArea.Right) < marginOfError
				&& Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError
				&& Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
		}

		public void TestPaintAreaForVerticallyCenteredText()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 100),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 0);
			text.VAlignment = Core.Alignment.Center;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(2.88f, 36.62f, 87.36f, 19.87f);
			var actualPaintArea = painter.PaintArea;

			CombineAssertions(() =>
			{
				Assert(string.Format("expected paint area X: {0}, actual paint area X: {1}", expectedPaintArea.X, actualPaintArea.X), Math.Abs(expectedPaintArea.X - actualPaintArea.X) < marginOfError);
				Assert(string.Format("expected paint area Y: {0}, actual paint area Y: {1}", expectedPaintArea.Y, actualPaintArea.Y), Math.Abs(expectedPaintArea.Y - actualPaintArea.Y) < marginOfError);
				Assert(string.Format("expected paint area Width: {0}, actual paint area Width: {1}", expectedPaintArea.Width, actualPaintArea.Width), Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError);
				Assert(string.Format("expected paint area Height: {0}, actual paint area Height: {1}", expectedPaintArea.Height, actualPaintArea.Height), Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
			});
		}

		public void TestPaintAreaForVerticallyCenteredVerticalText_90()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 100),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 90);
			text.HAlignment = Core.Alignment.Center;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(36.62f, 2.88f, 19.87f, 87.36f);
			var actualPaintArea = painter.PaintArea;

			CombineAssertions(() =>
			{
				Assert(string.Format("expected paint area X: {0}, actual paint area X: {1}", expectedPaintArea.X, actualPaintArea.X), Math.Abs(expectedPaintArea.X - actualPaintArea.X) < marginOfError);
				Assert(string.Format("expected paint area Y: {0}, actual paint area Y: {1}", expectedPaintArea.Y, actualPaintArea.Y), Math.Abs(expectedPaintArea.Y - actualPaintArea.Y) < marginOfError);
				Assert(string.Format("expected paint area Width: {0}, actual paint area Width: {1}", expectedPaintArea.Width, actualPaintArea.Width), Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError);
				Assert(string.Format("expected paint area Height: {0}, actual paint area Height: {1}", expectedPaintArea.Height, actualPaintArea.Height), Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
			});
		}

		public void TestPaintAreaForVerticallyCenteredVerticalText_180()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 100),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 180);
			text.HAlignment = Core.Alignment.Center;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(36.62f, 2.88f, 19.87f, 87.36f);
			var actualPaintArea = painter.PaintArea;

			CombineAssertions(() =>
			{
				Assert(string.Format("expected paint area X: {0}, actual paint area X: {1}", expectedPaintArea.X, actualPaintArea.X), Math.Abs(expectedPaintArea.X - actualPaintArea.X) < marginOfError);
				Assert(string.Format("expected paint area Y: {0}, actual paint area Y: {1}", expectedPaintArea.Y, actualPaintArea.Y), Math.Abs(expectedPaintArea.Y - actualPaintArea.Y) < marginOfError);
				Assert(string.Format("expected paint area Width: {0}, actual paint area Width: {1}", expectedPaintArea.Width, actualPaintArea.Width), Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError);
				Assert(string.Format("expected paint area Height: {0}, actual paint area Height: {1}", expectedPaintArea.Height, actualPaintArea.Height), Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
			});
		}

		public void TestPaintAreaForVerticallyBottomAlignedText()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 100),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 0);
			text.VAlignment = Core.Alignment.Bottom;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(2.88f, 70.36f, 87.36f, 19.87f);
			var actualPaintArea = painter.PaintArea;

			CombineAssertions(() =>
			{
				Assert(string.Format("expected paint area X: {0}, actual paint area X: {1}", expectedPaintArea.X, actualPaintArea.X), Math.Abs(expectedPaintArea.X - actualPaintArea.X) < marginOfError);
				Assert(string.Format("expected paint area Y: {0}, actual paint area Y: {1}", expectedPaintArea.Y, actualPaintArea.Y), Math.Abs(expectedPaintArea.Y - actualPaintArea.Y) < marginOfError);
				Assert(string.Format("expected paint area Width: {0}, actual paint area Width: {1}", expectedPaintArea.Width, actualPaintArea.Width), Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError);
				Assert(string.Format("expected paint area Height: {0}, actual paint area Height: {1}", expectedPaintArea.Height, actualPaintArea.Height), Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
			});
		}

		public void TestPaintAreaForVerticallyRightAlignedVerticalText_90()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 100),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 90);
			text.HAlignment = Core.Alignment.Right;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(70.36f, 2.88f, 19.87f, 87.36f);
			var actualPaintArea = painter.PaintArea;

			CombineAssertions(() =>
			{
				Assert(string.Format("expected paint area X: {0}, actual paint area X: {1}", expectedPaintArea.X, actualPaintArea.X), Math.Abs(expectedPaintArea.X - actualPaintArea.X) < marginOfError);
				Assert(string.Format("expected paint area Y: {0}, actual paint area Y: {1}", expectedPaintArea.Y, actualPaintArea.Y), Math.Abs(expectedPaintArea.Y - actualPaintArea.Y) < marginOfError);
				Assert(string.Format("expected paint area Width: {0}, actual paint area Width: {1}", expectedPaintArea.Width, actualPaintArea.Width), Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError);
				Assert(string.Format("expected paint area Height: {0}, actual paint area Height: {1}", expectedPaintArea.Height, actualPaintArea.Height), Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
			});
		}

		public void TestPaintAreaForVerticallyRightAlignedVerticalText_180()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 100),
				new RectangleF(3, 3, 3, 3),
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 90);
			text.HAlignment = Core.Alignment.Right;

			var painter = new TextPainter(text);

			var expectedPaintArea = new RectangleF(70.36f, 2.88f, 19.87f, 87.36f);
			var actualPaintArea = painter.PaintArea;

			CombineAssertions(() =>
			{
				Assert(string.Format("expected paint area X: {0}, actual paint area X: {1}", expectedPaintArea.X, actualPaintArea.X), Math.Abs(expectedPaintArea.X - actualPaintArea.X) < marginOfError);
				Assert(string.Format("expected paint area Y: {0}, actual paint area Y: {1}", expectedPaintArea.Y, actualPaintArea.Y), Math.Abs(expectedPaintArea.Y - actualPaintArea.Y) < marginOfError);
				Assert(string.Format("expected paint area Width: {0}, actual paint area Width: {1}", expectedPaintArea.Width, actualPaintArea.Width), Math.Abs(expectedPaintArea.Width - actualPaintArea.Width) < marginOfError);
				Assert(string.Format("expected paint area Height: {0}, actual paint area Height: {1}", expectedPaintArea.Height, actualPaintArea.Height), Math.Abs(expectedPaintArea.Height - actualPaintArea.Height) < marginOfError);
			});
		}

		#endregion

		#region UpdateStringFormatForVerticalText

		public void TestUpdateStringFormatForVerticalText_90()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				RectangleF.Empty,
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 90);

			text.HAlignment = Alignment.Center;
			text.VAlignment = Alignment.Center;

			var painter = new TextPainter(text);
			var stringFormat = new StringFormat();

			painter.UpdateStringFormatForVerticalText(stringFormat);

			CombineAssertions(() =>
			{
				AssertEquals(StringAlignment.Center, stringFormat.Alignment);
				AssertEquals(StringAlignment.Center, stringFormat.LineAlignment);
			});

			text.HAlignment = Alignment.Left;
			text.VAlignment = Alignment.Top;

			painter.UpdateStringFormatForVerticalText(stringFormat);

			CombineAssertions(() =>
			{
				AssertEquals(StringAlignment.Far, stringFormat.Alignment);
				AssertEquals(StringAlignment.Near, stringFormat.LineAlignment);
			});

			text.HAlignment = Alignment.Right;
			text.VAlignment = Alignment.Bottom;

			painter.UpdateStringFormatForVerticalText(stringFormat);

			CombineAssertions(() =>
			{
				AssertEquals(StringAlignment.Near, stringFormat.Alignment);
				AssertEquals(StringAlignment.Far, stringFormat.LineAlignment);
			});
		}

		public void TestUpdateStringFormatForVerticalText_180()
		{
			var text = new Text(new PointF(0, 0),
				new SizeF(100, 20),
				RectangleF.Empty,
				"xxx");

			text.Font = new Core.Font("Arial", 12f, FontStyle.Regular, Color.Black, 180);

			text.HAlignment = Alignment.Center;
			text.VAlignment = Alignment.Center;

			var painter = new TextPainter(text);
			var stringFormat = new StringFormat();

			painter.UpdateStringFormatForVerticalText(stringFormat);

			CombineAssertions(() =>
			{
				AssertEquals(StringAlignment.Center, stringFormat.Alignment);
				AssertEquals(StringAlignment.Center, stringFormat.LineAlignment);
			});

			text.HAlignment = Alignment.Left;
			text.VAlignment = Alignment.Top;

			painter.UpdateStringFormatForVerticalText(stringFormat);

			CombineAssertions(() =>
			{
				AssertEquals(StringAlignment.Near, stringFormat.Alignment);
				AssertEquals(StringAlignment.Far, stringFormat.LineAlignment);
			});

			text.HAlignment = Alignment.Right;
			text.VAlignment = Alignment.Bottom;

			painter.UpdateStringFormatForVerticalText(stringFormat);

			CombineAssertions(() =>
			{
				AssertEquals(StringAlignment.Far, stringFormat.Alignment);
				AssertEquals(StringAlignment.Near, stringFormat.LineAlignment);
			});
		}

		#endregion
	}
}
