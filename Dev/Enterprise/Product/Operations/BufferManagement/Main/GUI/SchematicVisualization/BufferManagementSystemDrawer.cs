using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public class BufferManagementSystemDrawer
	{
		public Image GetSchematicImage(BMSystem system, bool includeNonPrimaryPath, int maxWidth = 2000, int maxHeight = 2000)
		{
			int initialOffset = SchematicVisualizationConstantsUnscaled.BufferManagementSystemDrawerInitialOffset;
			var x = ControlDpiScalingHelper.ScaleToCurrentDpiX(initialOffset);
			var y = ControlDpiScalingHelper.ScaleToCurrentDpiY(initialOffset);

			return GetImage(graphics =>
			{
				var drawnLinks = new List<BMComponentLink>();

				var startComponent = system.Components.FirstOrDefault(child => child.FC_IsActive && !child.GetFromOthersToMeLinksWithinSystem(system).Any());
				DrawComponentAndLinks(graphics, drawnLinks, ref x, ref y, startComponent, false);

				if (includeNonPrimaryPath)
				{
					var remainingLinks = system.Components
						.OrderBy(component => component.FC_DisplaySequence)
						.SelectMany(component => component.FromMeToOthersLinks.OrderBy(link => link.ComponentTo.FC_DisplaySequence))
						.Except(drawnLinks)
						.Where(link => link.ComponentFrom.FC_IsActive && link.ComponentTo.FC_IsActive && link.ComponentToSystemPK == system.PK);

					while (remainingLinks.Any())
					{
						foreach (var link in remainingLinks)
						{
							drawnLinks.Add(link);
							if (!componentPositions.ContainsKey(link.ComponentTo) && componentPositions.ContainsKey(link.ComponentFrom))
							{
								x = componentPositions[link.ComponentFrom].X;
								y = componentPositions[link.ComponentFrom].Y + 150;
								DrawComponentAndLinks(graphics, drawnLinks, ref x, ref y, link.ComponentTo, true);
							}

							if (link.ComponentFrom.FC_Type != BMComponentTypeList.Codes.Buffer && link.ComponentTo.FC_Type != BMComponentTypeList.Codes.Buffer)
							{
								DrawArrowBetween(graphics, link.ComponentFrom, link.ComponentTo);
							}
						}
					}
				}
				DrawnLinks = drawnLinks;
			},
			maxWidth, maxHeight, 300);
		}

#if DEBUG
		public
#endif
		List<BMComponentLink> DrawnLinks
		{ get; set; }

		int maxX;
		int maxY;

		public Image GetLegendImage(int maxWidth = 500, int maxHeight = 1250)
		{
			var x = 0;
			var y = 0;

			var scaledMaxWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(maxWidth);
			var scaledMaxHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(maxHeight);

			return GetImage(graphics =>
			{
				new BucketDrawer(graphics, null, false).Draw(ref x, ref y);
				new BufferDrawer(graphics, null, false).Draw(ref x, ref y);
				new ConstraintDrawer(graphics, null, false).Draw(ref x, ref y);
				new DecoupleDrawer(graphics, null, false).Draw(ref x, ref y);

				maxX = x;
				maxY = y;
			},
			scaledMaxWidth, scaledMaxHeight);
		}

		#region Implementation

		Image GetImage(Action<Graphics> generateImageAction, int maxWidth, int maxHeight, int unscaledWidthPadding = SchematicVisualizationConstantsUnscaled.BucketWidthBase + SchematicVisualizationConstantsUnscaled.BucketEdgeDifference)
		{
			Argument.NotNull(generateImageAction, "generateImageAction");

			var scaledPadding = ControlDpiScalingHelper.ScaleToCurrentDpiX(unscaledWidthPadding);

			using (var imageBuffer = new Bitmap(maxWidth, maxHeight, PixelFormat.Format16bppRgb555))
			using (var graphics = Graphics.FromImage(imageBuffer))
			{
				graphics.Clear(Color.White);
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

				generateImageAction(graphics);

				return ((maxX + scaledPadding) <= maxWidth && maxY <= maxHeight)
								? imageBuffer.Clone(ControlDpiScalingHelper.NewScaledRectangle(0, 0, maxX + scaledPadding, maxY, false), imageBuffer.PixelFormat)
								: null;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		void DrawComponentAndLinks(Graphics graphics, List<BMComponentLink> drawnLinks, ref int x, ref int y, BMComponent startComponent, bool isNonPrimaryPath)
		{
			var componentToDraw = startComponent;
			while (componentToDraw != null)
			{
				if (!componentPositions.ContainsKey(componentToDraw))
				{
					DrawItem(graphics, ref x, ref y, componentToDraw, false, isNonPrimaryPath);
					var firstLink = GetFirstPrimaryLink(componentToDraw);
					if (firstLink != null)
					{
						if (firstLink.ComponentToSystemPK == startComponent.FC_FS_System)
						{
							drawnLinks.Add(firstLink);
							componentToDraw = firstLink.ComponentTo;
						}
					}
					else
					{
						componentToDraw = null;
					}
				}
				else
				{
					componentToDraw = null;
				}
			}
		}

		readonly Dictionary<BMComponent, Point> componentPositions = new Dictionary<BMComponent, Point>();

		internal static BMComponentLink GetFirstPrimaryLink(BMComponent component)
		{
			var subsequentLinks = component.FromMeToOthersLinks.OrderByDescending(link => link.ComponentTo.BufferTimeSpanHours).ThenBy(link => link.ComponentTo.FC_DisplaySequence).Where(link => link.ComponentTo.FC_IsActive);
			BMComponentLink firstLink;
			if (subsequentLinks.Count() > 1)
			{
				firstLink = subsequentLinks.FirstOrDefault(link => link.ComponentTo.FC_DisplaySequence > component.FC_DisplaySequence);
			}
			else
			{
				firstLink = subsequentLinks.FirstOrDefault();
			}

			return firstLink;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		void DrawItem(Graphics graphics, ref int x, ref int y, BMComponent component, bool isSubItem, bool isNonPrimaryPath)
		{
			var drawer = (ComponentDrawer)Activator.CreateInstance(DrawerTypes[component.FC_Type], graphics, component, isNonPrimaryPath);
			var adjustedX = drawer.GetStartingX(x);
			drawer.Draw(ref adjustedX, ref y);
			foreach (var subItem in drawer.GetAdditionalItemsForDrawing())
			{
				DrawItem(graphics, ref adjustedX, ref y, subItem, true, isNonPrimaryPath);
			}

			if (!isSubItem)
			{
				drawer.UpdatePositions(ref x, ref y);
			}

			componentPositions.Add(component, drawer.GetConnectingPoint(adjustedX, y));

			maxX = Math.Max(x, maxX);
			maxY = Math.Max(y, maxY);
		}

		void DrawArrowBetween(Graphics graphics, BMComponent from, BMComponent to)
		{
			Point fromPoint, toPoint;
			if (componentPositions.TryGetValue(from, out fromPoint) && componentPositions.TryGetValue(to, out toPoint))
			{
				Point adjustedFromPoint;
				Point adjustedToPoint;
				var scaledFromPointX = fromPoint.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				var scaledFromPointY = fromPoint.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
				var scaledToPointX = toPoint.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				var scaledToPointY = toPoint.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(50);

				if (from.FC_Type == BMComponentTypeList.Codes.Buffer)
				{
					adjustedFromPoint = ControlDpiScalingHelper.NewScaledPoint(scaledFromPointX - (BufferDrawer.GetBufferLength(from) / 2), scaledFromPointY, false);
				}
				else
				{
					adjustedFromPoint = ControlDpiScalingHelper.NewScaledPoint(scaledFromPointX, scaledFromPointY, false);
				}

				if (to.FC_Type == BMComponentTypeList.Codes.Buffer)
				{
					adjustedToPoint = ControlDpiScalingHelper.NewScaledPoint(scaledToPointX - (BufferDrawer.GetBufferLength(to) / 2), scaledToPointY, false);
				}
				else
				{
					adjustedToPoint = ControlDpiScalingHelper.NewScaledPoint(scaledToPointX, scaledToPointY, false);
				}

				graphics.DrawLine(DashedArrowPen, adjustedFromPoint, adjustedToPoint);
			}
		}

		Dictionary<string, Type> DrawerTypes
		{
			get
			{
				if (drawerTypes == null)
				{
					drawerTypes = new Dictionary<string, Type>();
					drawerTypes.Add(BMComponentTypeList.Codes.Bucket, typeof(BucketDrawer));
					drawerTypes.Add(BMComponentTypeList.Codes.Buffer, typeof(BufferDrawer));
					drawerTypes.Add(BMComponentTypeList.Codes.Constraint, typeof(ConstraintDrawer));
					drawerTypes.Add(BMComponentTypeList.Codes.Decouple, typeof(DecoupleDrawer));
				}

				return drawerTypes;
			}
		}

		Dictionary<string, Type> drawerTypes;

		static Pen DashedArrowPen
		{
			get
			{
				if (dashedArrowPen == null)
				{
					dashedArrowPen = new Pen(Color.Orange, 2);
					dashedArrowPen.CustomEndCap = new AdjustableArrowCap(4, 4);
					dashedArrowPen.DashCap = DashCap.Round;
					dashedArrowPen.DashStyle = DashStyle.Dash;
				}
				return dashedArrowPen;
			}
		}

		[ThreadStatic]
		static Pen dashedArrowPen;

		#endregion
	}
}
