using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	class VisualiserComponentBorderToGraphicsConverter : IVisualiserDrawer
	{
		public VisualiserComponentBorderToGraphicsConverter(Control parentControl)
		{
			this.parentControl = parentControl;
		}

		readonly Control parentControl;
		KeyValuePair<Point, Point>[] borderPoints;

		public void Draw(IEnumerable<VisualiserComponent> components)
		{
			borderPoints = GetBorderPointPairsFromComponents(components);
			parentControl.Paint += new PaintEventHandler(ParentControl_Paint);
		}

		KeyValuePair<Point, Point>[] GetBorderPointPairsFromComponents(IEnumerable<VisualiserComponent> components)
		{
			var linesPoints = new Stack<KeyValuePair<Point, Point>>();
			var borders = GetOptimisedBordersFromComponents(components);
			foreach (var border in borders)
			{
				linesPoints.Push(new KeyValuePair<Point, Point>(border.Point1, border.Point2));
			}

			return linesPoints.ToArray();
		}

		Border[] GetOptimisedBordersFromComponents(IEnumerable<VisualiserComponent> components)
		{
			var horizontalBorders = new List<HorizontalBorder>();
			var verticalBorders = new List<VerticalBorder>();

			foreach (var component in components)
			{
				var componentWithCellFormat = component as VisualiserComponentBorder;
				if (componentWithCellFormat != null)
				{
					var width = component.Size.Width;
					var height = component.Size.Height;
					var topLeft = component.Location;
					var topRight = component.Location;
					topRight.Offset(width, 0);
					var bottomLeft = component.Location;
					bottomLeft.Offset(0, height);

					var cellFormatBorders = componentWithCellFormat.CellFormat.Borders;
					if (cellFormatBorders.Bottom.BorderStyle != CellBorderStyle.None)
					{
						horizontalBorders.Add(new HorizontalBorder(bottomLeft, width));
					}

					if (cellFormatBorders.Left.BorderStyle != CellBorderStyle.None)
					{
						verticalBorders.Add(new VerticalBorder(topLeft, height));
					}

					if (cellFormatBorders.Right.BorderStyle != CellBorderStyle.None)
					{
						verticalBorders.Add(new VerticalBorder(topRight, height));
					}

					if (cellFormatBorders.Top.BorderStyle != CellBorderStyle.None)
					{
						horizontalBorders.Add(new HorizontalBorder(topLeft, width));
					}
				}
			}

			MergeBorders(horizontalBorders, 1);
			MergeBorders(verticalBorders, 1);
			ExtendBorders(verticalBorders, horizontalBorders, 1);

			MergeBorders(horizontalBorders, 2);
			MergeBorders(verticalBorders, 2);
			ExtendBorders(verticalBorders, horizontalBorders, 2);

			RemoveRedundantBorders(verticalBorders, horizontalBorders);

			var borders = new List<Border>();
			borders.AddRange(horizontalBorders.ToArray());
			borders.AddRange(verticalBorders.ToArray());

			return borders.ToArray();
		}

		void MergeBorders<T>(List<T> borders, int padding) where T : Border
		{
			for (var checkBorderIndex = 0; checkBorderIndex < borders.Count; checkBorderIndex++)
			{
				var checkingBorder = borders[checkBorderIndex];
				for (var mergeToBorderIndex = checkBorderIndex + 1; mergeToBorderIndex < borders.Count; mergeToBorderIndex++)
				{
					var mergeToBorder = borders[mergeToBorderIndex];
					if (IsMergeable(checkingBorder, mergeToBorder, padding))
					{
						checkingBorder.Length = Math.Max(checkingBorder.ParallelCoord2, mergeToBorder.ParallelCoord2) - Math.Min(checkingBorder.ParallelCoord1, mergeToBorder.ParallelCoord1);
						checkingBorder.ParallelCoord1 = Math.Min(checkingBorder.ParallelCoord1, mergeToBorder.ParallelCoord1);
						checkingBorder.PerpendicularCoord = (checkingBorder.PerpendicularCoord + mergeToBorder.PerpendicularCoord) / 2;
						borders.Remove(mergeToBorder);
						mergeToBorderIndex--;
					}
				}
			}
		}

		static bool IsMergeable<T>(T border1, T border2, int padding) where T : Border
		{
			return (Math.Abs(border1.PerpendicularCoord - border2.PerpendicularCoord) < padding
				&& (Math.Max(border1.ParallelCoord1, border2.ParallelCoord1) - padding <= Math.Min(border1.ParallelCoord2, border2.ParallelCoord2)));
		}

		void ExtendBorders<T, U>(List<T> bordersToExtend, List<U> edgeBorders, int padding)
			where T : Border
			where U : Border
		{
			bordersToExtend.Sort((b1, b2) => (b1.ParallelCoord1 == b2.ParallelCoord1) ? (b1.PerpendicularCoord.CompareTo(b2.PerpendicularCoord)) : b1.ParallelCoord1.CompareTo(b2.ParallelCoord1));
			edgeBorders.Sort((b1, b2) => (b1.PerpendicularCoord == b2.PerpendicularCoord) ? (b1.ParallelCoord1.CompareTo(b2.ParallelCoord1)) : b1.PerpendicularCoord.CompareTo(b2.PerpendicularCoord));

			var borderToExtendIndex = 0;
			var potentialBordersToExtend = new List<T>();

			for (var edgeBorderIndex = 0; edgeBorderIndex < edgeBorders.Count; edgeBorderIndex++)
			{
				var edgeBorder = edgeBorders[edgeBorderIndex];
				while (borderToExtendIndex < bordersToExtend.Count && bordersToExtend[borderToExtendIndex].ParallelCoord1 < edgeBorder.PerpendicularCoord)
				{
					var borderToExtend = bordersToExtend[borderToExtendIndex];
					bool borderIsExtended = false;
					for (var potentialBorderToExtendIndex = 0; potentialBorderToExtendIndex < potentialBordersToExtend.Count; potentialBorderToExtendIndex++)
					{
						var potentialBorderToExtend = potentialBordersToExtend[potentialBorderToExtendIndex];
						if (IsExtendable(potentialBorderToExtend, borderToExtend, padding))
						{
							potentialBorderToExtend.Length = Math.Max(borderToExtend.ParallelCoord2, potentialBorderToExtend.ParallelCoord2) - Math.Min(borderToExtend.ParallelCoord1, potentialBorderToExtend.ParallelCoord1);
							potentialBordersToExtend.Remove(potentialBorderToExtend);
							borderToExtendIndex--;

							bordersToExtend.Remove(borderToExtend);
							borderToExtendIndex--;

							borderIsExtended = true;
							break;
						}
					}

					if (!borderIsExtended)
					{
						potentialBordersToExtend.Add(borderToExtend);
					}
					borderToExtendIndex++;
				}

				potentialBordersToExtend.RemoveAll(x => IsAbove(x, edgeBorder, padding * 2));
			}
		}

		static bool IsAbove<T, U>(T borderToCheck, U borderToBeAbove, int padding)
			where T : Border
			where U : Border
		{
			return (borderToCheck.ParallelCoord2 <= borderToBeAbove.PerpendicularCoord + padding)
				&& IsWeaklyIncreasing(borderToBeAbove.ParallelCoord1 - padding, borderToCheck.PerpendicularCoord, borderToBeAbove.ParallelCoord2 + padding);
		}

		static bool IsExtendable<T>(T borderToExtend, T borderToMeet, int padding)
			where T : Border
		{
			return (Math.Abs(borderToExtend.PerpendicularCoord - borderToMeet.PerpendicularCoord) < padding)
				&& (borderToExtend.ParallelCoord2 < borderToMeet.ParallelCoord1);
		}

		void RemoveRedundantBorders<T, U>(List<T> bordersToCheck, List<U> perpendicularBorders)
			where T : Border
			where U : Border
		{
			for (var bordersToCheckIndex = 0; bordersToCheckIndex < bordersToCheck.Count; bordersToCheckIndex++)
			{
				var borderToCheck = bordersToCheck[bordersToCheckIndex];
				if (IsARedundantBorder(borderToCheck, perpendicularBorders))
				{
					bordersToCheck.RemoveAt(bordersToCheckIndex);
					bordersToCheckIndex--;
				}
			}
		}

		static bool IsARedundantBorder<T, U>(T borderToCheck, List<U> perpendicularBorders)
			where T : Border
			where U : Border
		{
			var padding = 10;
			foreach (var border in perpendicularBorders)
			{
				if (IsWeaklyIncreasing(borderToCheck.ParallelCoord1 - padding, border.PerpendicularCoord, borderToCheck.ParallelCoord2 + padding)
					&& IsWeaklyIncreasing(border.ParallelCoord1 - padding, borderToCheck.PerpendicularCoord, border.ParallelCoord2 + padding))
				{
					return false;
				}
			}
			return true;
		}

		static bool IsWeaklyIncreasing(params int[] integers)
		{
			for (var index = 1; index < integers.Length; index++)
			{
				if (integers[index] < integers[index - 1])
				{
					return false;
				}
			}
			return true;
		}

		void ParentControl_Paint(object sender, PaintEventArgs e)
		{
			var offset = sender is ScrollableControl parentControl ?
				ControlDpiScalingHelper.NewScaledSize(parentControl.AutoScrollPosition.X, parentControl.AutoScrollPosition.Y) :
				ControlDpiScalingHelper.NewScaledSize(0, 0);

			foreach (var border in borderPoints)
			{
				e.Graphics.DrawLine(Pens.Black, border.Key + offset, border.Value + offset);
			}
		}

		#region Border Classes

		abstract class Border
		{
			public abstract Point Point1 { get; }
			public abstract Point Point2 { get; }

			public abstract int PerpendicularCoord { get; set; }
			public abstract int ParallelCoord1 { get; set; }
			public abstract int ParallelCoord2 { get; }

			public abstract int Length { get; set; }
		}

		class VerticalBorder : Border
		{
			Point Top;
			int Height;

			public VerticalBorder(Point top, int height)
			{
				this.Top = top;
				this.Height = height;
			}

			public override Point Point1
			{
				get { return Top; }
			}

			public override Point Point2
			{
				get { return Top + ControlDpiScalingHelper.NewScaledSize(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height)); }
			}

			public override int PerpendicularCoord
			{
				get { return Top.X; }
				set { ControlDpiScalingHelper.SetX(ref Top, value, false); }
			}

			public override int ParallelCoord1
			{
				get { return Top.Y; }
				set { ControlDpiScalingHelper.SetY(ref Top, value, false); }
			}

			public override int ParallelCoord2
			{
				get { return Top.Y + Height; }
			}

			public override int Length
			{
				get { return Height; }
				set { Height = value; }
			}
		}

		class HorizontalBorder : Border
		{
			Point Left;
			int Width;

			public HorizontalBorder(Point left, int width)
			{
				this.Left = left;
				this.Width = width;
			}

			public override Point Point1
			{
				get { return Left; }
			}

			public override Point Point2
			{
				get { return Left + ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(Width), 0); }
			}

			public override int PerpendicularCoord
			{
				get { return Left.Y; }
				set { ControlDpiScalingHelper.SetY(ref Left, value, false); }
			}

			public override int ParallelCoord1
			{
				get { return Left.X; }
				set { ControlDpiScalingHelper.SetX(ref Left, value, false); }
			}

			public override int ParallelCoord2
			{
				get { return Left.X + Width; }
			}

			public override int Length
			{
				get { return Width; }
				set { Width = value; }
			}
		}

		#endregion
	}
}
