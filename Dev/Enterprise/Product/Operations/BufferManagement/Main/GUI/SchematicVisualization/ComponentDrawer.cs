using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public abstract class ComponentDrawer
	{
		protected ComponentDrawer(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
		{
			this.Graphics = graphics;
			this.Component = component;
			this.IsNonPrimaryPath = isNonPrimaryPath;
		}

		protected bool IsNonPrimaryPath { get; private set; }
		protected BMComponent Component { get; private set; }
		public Graphics Graphics { get; private set; }

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public abstract void Draw(ref int x, ref int y);

		public virtual Point GetConnectingPoint(int x, int y)
		{
			return ControlDpiScalingHelper.NewScaledPoint(x, y, false);
		}

		protected bool IsLegend
		{
			get { return Component == null; }
		}

		public virtual IEnumerable<BMComponent> GetAdditionalItemsForDrawing()
		{
			return Enumerable.Empty<BMComponent>();
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public virtual void UpdatePositions(ref int x, ref int y)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void DrawVerticalArrow(ref int x, ref int y, int yOffset)
		{
			if (Component != null && (Component.FromMeToOthersLinks.Any() || Component.ParentComponent != null))
			{
				var newX = x + ((BucketDrawer.ScaledBucketEdgeDifference + BucketDrawer.ScaledBucketWidthBase) / 2);
				using (var arrowPen = new Pen(Color.Gray, 2) { CustomEndCap = new AdjustableArrowCap(4, 4) })
				{
					Graphics.DrawLine(arrowPen, newX, y + yOffset, newX, y + yOffset + GetArrowHeight());
				}
			}

			y += yOffset + ScaledArrowHeight;
		}

		protected int GetArrowHeight()
		{
			if (Component != null)
			{
				var subComponentsAndAdditionalFromComponents = Component.ChildComponents.Count;
				var firstLink = BufferManagementSystemDrawer.GetFirstPrimaryLink(Component);
				if (firstLink != null)
				{
					var additionalComponentsFollowingMe = 0;
					if (Component.FC_Type == BMComponentTypeList.Codes.Buffer)
					{
						additionalComponentsFollowingMe += firstLink.ComponentTo.FromOthersToMeLinks.Count(link => link.ComponentFrom.FC_Type == BMComponentTypeList.Codes.Buffer && link.ComponentFrom.BufferTimeSpanHours < Component.BufferTimeSpanHours);
					}
					else
					{
						additionalComponentsFollowingMe = firstLink.ComponentTo.FromOthersToMeLinks.Count(link => link.ComponentFrom.FC_DisplaySequence < Component.FC_DisplaySequence);
					}

					subComponentsAndAdditionalFromComponents += additionalComponentsFollowingMe;
					subComponentsAndAdditionalFromComponents = Math.Max(subComponentsAndAdditionalFromComponents, 0);
				}

				return (1 + (2 * subComponentsAndAdditionalFromComponents)) * ScaledArrowHeight;
			}
			else
			{
				return ScaledArrowHeight;
			}
		}

		protected virtual void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
		{
			Graphics.DrawLine(pen, x1, y1, x2, y2);
		}

		protected virtual void DrawLine(Pen pen, Point pt1, Point pt2)
		{
			Graphics.DrawLine(pen, pt1, pt2);
		}

		protected virtual void DrawPath(Pen pen, GraphicsPath path)
		{
			Graphics.DrawPath(pen, path);
		}

		protected virtual void DrawEllipse(Pen pen, Rectangle rect)
		{
			Graphics.DrawEllipse(pen, rect);
		}

		protected virtual void DrawString(int x, int totalWidth, int y, string text, Font overrideFont = null, bool center = true)
		{
			using (var labelFont = new Font("Arial", 10))
			using (var descriptionFont = new Font("Arial", 9))
			{
				var font = overrideFont ?? descriptionFont;
				var textSize = Graphics.MeasureString(text, labelFont);

				if (textSize.Width > totalWidth && totalWidth > -1)
				{
#if WINZOR
					Graphics.DrawString(text, font, Brushes.Black, new RectangleF(x, y, totalWidth, ScaledLegendStringHeight), null);
#else
					TextRendererHelper.DrawText(Graphics, text, font, new RectangleF(x, y, totalWidth, ScaledLegendStringHeight), Brushes.Black, renderingEngine: TextRendererType.GDIPlus);
#endif
				}
				else
				{
					if (center)
					{
						x += (int)((totalWidth - textSize.Width) / 2);
					}

#if WINZOR
					Graphics.DrawString(text, labelFont, Brushes.Black, new PointF(x, y), null);
#else
					TextRendererHelper.DrawText(Graphics, text, labelFont, new PointF(x, y), Brushes.Black, renderingEngine: TextRendererType.GDIPlus);
#endif
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void DrawLegendString(int x, ref int y, string text)
		{
			using (var legendDescriptionFont = new Font("Arial", 8))
			{
				DrawString(x, BucketDrawer.ScaledBucketWidthBase + BucketDrawer.ScaledBucketEdgeDifference, y, text, legendDescriptionFont);
				y += ScaledLegendStringPadding;
			}
		}

		public int GetStartingX(int x)
		{
			var adjustedX = x;
			if (Component.ParentComponent != null)
			{
				var ratio = (decimal)Component.FC_OffsetInMinutes / (decimal)Component.ParentComponent.FC_BufferTimespanInMinutes;
				adjustedX = (int)Math.Floor(x + (ratio * BufferDrawer.ScaledBufferStandardLength));
			}

			return adjustedX;
		}

		static int ScaledLegendStringPadding => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.ComponentDrawerLegendStringPadding);
		static int ScaledLegendStringHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.ComponentDrawerLegendStringHeight);

		protected static int ScaledExtraHeightForLastComponent => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.ComponentDrawerExtraHeightForLastComponent);
		protected static int ScaledArrowHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.ComponentDrawerArrowHeight);
	}
}
