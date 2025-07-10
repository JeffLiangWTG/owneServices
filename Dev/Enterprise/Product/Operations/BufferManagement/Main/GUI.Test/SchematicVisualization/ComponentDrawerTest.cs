using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class ComponentDrawerTest : TestCase
	{
		public void TestBucketDrawer_LineDistances_ScalesToDpi()
		{
			DrawerTestHelper.Test_Sizes_ShouldScaleToDpi(() =>
			{
				var drawer = new BucketDrawer_ForTest(Graphics.FromImage(new Bitmap(10000, 10000)), null, true);

				int x = 0;
				int y = 0;
				drawer.Draw(ref x, ref y);

				return drawer.BucketDimensions;
			});
		}

		public void TestBufferDrawer_LineDistances_ScalesToDpi()
		{
			DrawerTestHelper.Test_Sizes_ShouldScaleToDpi(() =>
			{
				var drawer = new BufferDrawer_ForTest(Graphics.FromImage(new Bitmap(10000, 10000)), null, true);

				int x = 0;
				int y = 0;
				drawer.Draw(ref x, ref y);

				return drawer.LineDistances;
			});
		}

		public void TestConstraintDrawer_LineDistances_ScalesToDpi()
		{
			DrawerTestHelper.Test_Sizes_ShouldScaleToDpi(() =>
			{
				var drawer = new ConstraintDrawer_ForTest(Graphics.FromImage(new Bitmap(10000, 10000)), null, true);

				int x = 0;
				int y = 0;
				drawer.Draw(ref x, ref y);

				return drawer.LineDistances;
			});
		}

		public void TestConstraintDrawer_EllipseDimensions_ScalesToDpi()
		{
			DrawerTestHelper.Test_Sizes_ShouldScaleToDpi(() =>
			{
				var drawer = new ConstraintDrawer_ForTest(Graphics.FromImage(new Bitmap(10000, 10000)), null, true);

				int x = 0;
				int y = 0;
				drawer.Draw(ref x, ref y);

				return drawer.EllipseRectangleBoundDimensions;
			});
		}

		public void TestDecoupleDrawer_LineDistances_ScalesToDpi()
		{
			DrawerTestHelper.Test_Sizes_ShouldScaleToDpi(() =>
			{
				var drawer = new DecoupleDrawer_ForTest(Graphics.FromImage(new Bitmap(10000, 10000)), null, true);

				int x = 0;
				int y = 0;
				drawer.Draw(ref x, ref y);

				return drawer.LineDistances;
			});
		}
	}

	#region Implementation
	class BucketDrawer_ForTest : BucketDrawer
	{
		public BucketDrawer_ForTest(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		protected override void DrawPath(Pen pen, GraphicsPath path)
		{
			var dimensions = path.GetBounds();
			BucketDimensions.Add((int)dimensions.Width);
			BucketDimensions.Add((int)dimensions.Height);

			Graphics.DrawPath(pen, path);
		}

		public List<int> BucketDimensions { get; private set; } = new List<int>();
	}

	class BufferDrawer_ForTest : BufferDrawer
	{
		public BufferDrawer_ForTest(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		protected override void DrawLine(Pen pen, Point pt1, Point pt2)
		{
			var distance = Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
			LineDistances.Add((int)distance);

			base.DrawLine(pen, pt1, pt2);
		}

		public List<int> LineDistances { get; private set; } = new List<int>();
	}

	class ConstraintDrawer_ForTest : ConstraintDrawer
	{
		public ConstraintDrawer_ForTest(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		protected override void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
		{
			var distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
			LineDistances.Add((int)distance);

			base.DrawLine(pen, x1, y1, x2, y2);
		}

		protected override void DrawEllipse(Pen pen, Rectangle rect)
		{
			EllipseRectangleBoundDimensions.Add(rect.Width);
			EllipseRectangleBoundDimensions.Add(rect.Height);

			base.DrawEllipse(pen, rect);
		}

		public List<int> LineDistances { get; private set; } = new List<int>();
		public List<int> EllipseRectangleBoundDimensions { get; private set; } = new List<int>();
	}

	class DecoupleDrawer_ForTest : DecoupleDrawer
	{
		public DecoupleDrawer_ForTest(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		protected override void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
		{
			var distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
			LineDistances.Add((int)distance);

			base.DrawLine(pen, x1, y1, x2, y2);
		}

		public List<int> LineDistances { get; private set; } = new List<int>();
	}
	#endregion
}
