using System;
using System.Drawing;

using CargoWise.Windows.UI;
// reflected from System.Windows.Forms.dll
namespace Enterprise.ZArchitecture.GUI
{
	internal enum TriangleDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	internal static class Triangle
	{
		static Point[] BuildTrianglePoints(TriangleDirection dir, Rectangle bounds)
		{
			var pointArray1 = new Point[3];
			var num1 = (int)(bounds.Width * 0.8);
			if ((num1 % 2) == 1)
			{
				num1++;
			}
			var num2 = (int)Math.Ceiling((double)((num1 / 2) * 2.5));
			var num3 = (int)(bounds.Height * 0.8);
			if ((num3 % 2) == 0)
			{
				num3++;
			}
			var num4 = (int)Math.Ceiling((double)((num3 / 2) * 2.5));
			switch (dir)
			{
				case TriangleDirection.Up:
					{
						pointArray1[0] = ControlDpiScalingHelper.NewScaledPoint(0, num2, false);
						pointArray1[1] = ControlDpiScalingHelper.NewScaledPoint(num1, num2, false);
						pointArray1[2] = ControlDpiScalingHelper.NewScaledPoint(num1 / 2, 0, false);
						break;
					}
				case TriangleDirection.Down:
					{
						pointArray1[0] = ControlDpiScalingHelper.NewScaledPoint(0, 0);
						pointArray1[1] = ControlDpiScalingHelper.NewScaledPoint(num1, 0, false);
						pointArray1[2] = ControlDpiScalingHelper.NewScaledPoint(num1 / 2, num2, false);
						break;
					}
				case TriangleDirection.Left:
					{
						pointArray1[0] = ControlDpiScalingHelper.NewScaledPoint(num3, 0, false);
						pointArray1[1] = ControlDpiScalingHelper.NewScaledPoint(num3, num4, false);
						pointArray1[2] = ControlDpiScalingHelper.NewScaledPoint(0, num4 / 2, false);
						break;
					}
				case TriangleDirection.Right:
					{
						pointArray1[0] = ControlDpiScalingHelper.NewScaledPoint(0, 0);
						pointArray1[1] = ControlDpiScalingHelper.NewScaledPoint(0, num4, false);
						pointArray1[2] = ControlDpiScalingHelper.NewScaledPoint(num3, num4 / 2, false);
						break;
					}
			}
			switch (dir)
			{
				case TriangleDirection.Up:
				case TriangleDirection.Down:
					{
						Triangle.OffsetPoints(pointArray1, bounds.X + ((bounds.Width - num2) / 2), bounds.Y + ((bounds.Height - num1) / 2));
						return pointArray1;
					}
				case TriangleDirection.Left:
				case TriangleDirection.Right:
					{
						Triangle.OffsetPoints(pointArray1, bounds.X + ((bounds.Width - num3) / 2), bounds.Y + ((bounds.Height - num4) / 2));
						return pointArray1;
					}
			}
			return pointArray1;
		}

		static void OffsetPoints(Point[] points, int xOffset, int yOffset)
		{
			for (var num1 = 0; num1 < points.Length; num1++)
			{
				ControlDpiScalingHelper.SetX(ref points[num1], points[num1].X + xOffset, false);
				ControlDpiScalingHelper.SetY(ref points[num1], points[num1].Y + yOffset, false);
			}
		}

		public static void Paint(Graphics g, Rectangle bounds, TriangleDirection dir, Brush backBr, Pen backPen1, Pen backPen2, Pen backPen3, bool opaque)
		{
			var pointArray1 = Triangle.BuildTrianglePoints(dir, bounds);
			if (opaque)
			{
				g.FillPolygon(backBr, pointArray1);
			}
			g.DrawLine(backPen1, pointArray1[0], pointArray1[1]);
			g.DrawLine(backPen2, pointArray1[1], pointArray1[2]);
			g.DrawLine(backPen3, pointArray1[2], pointArray1[0]);
		}
	}
}
