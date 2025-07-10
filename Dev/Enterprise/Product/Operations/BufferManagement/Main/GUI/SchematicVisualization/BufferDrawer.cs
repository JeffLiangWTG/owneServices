using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.GUI
{
	public class BufferDrawer : ComponentDrawer
	{
		public BufferDrawer(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public override void Draw(ref int x, ref int y)
		{
			var bufferLength = GetBufferLength(Component);
			if (IsLegend)
			{
				bufferLength = BucketDrawer.ScaledBucketWidthBase + BucketDrawer.ScaledBucketEdgeDifference;
			}

			var horizontalLineStart = ControlDpiScalingHelper.NewScaledPoint(IsLegend ? x : (x + ((BucketDrawer.ScaledBucketEdgeDifference + BucketDrawer.ScaledBucketWidthBase) / 2)), y, false);
			var horizontalLineEnd = ControlDpiScalingHelper.NewScaledPoint(horizontalLineStart.X + bufferLength, horizontalLineStart.Y, false);

			using (var arrowPen = new Pen(Color.Gray, 2) { CustomEndCap = new AdjustableArrowCap(4, 4) })
			{
				DrawLine(arrowPen, horizontalLineStart, horizontalLineEnd);

				if (!IsLegend)
				{
					var verticalLineEnd = ControlDpiScalingHelper.NewScaledPoint(horizontalLineEnd.X, horizontalLineEnd.Y + GetArrowHeight(), false);
					DrawLine(arrowPen, horizontalLineEnd, verticalLineEnd);
				}
			}

			DrawString(horizontalLineStart.X, bufferLength, horizontalLineStart.Y - ScaledBufferNameLabelOffset, Res.GetString("F818AC72-64DC-49F7-89FE-BAF6070A4C2D", "{0} ({1} hrs)", IsLegend ? Res.GetString("c22bab27-6075-4e22-90a2-5b37fa39900a", "Buffer") : Component.FC_Name.ToString(), IsLegend ? string.Empty : Component.BufferTimeSpanHours.FormatWithNoMoreThanTwoDecimalPlaces()));

			y += ComponentDrawer.ScaledArrowHeight;

			if (IsLegend)
			{
				DrawLegendString(x, ref y, Res.GetString("b6299ce5-3666-46ef-bb1d-0990c33bffce", "A buffer is a time gap; items in the buffer have permission to start. The buffer absorbs variation in order to provide reliable time promises, and must be accompanied by buffer management responses to excessive buffer consumption."));
			}
		}

		public override IEnumerable<BMComponent> GetAdditionalItemsForDrawing()
		{
			return Component.ChildComponents.OrderBy(child => child.FC_DisplaySequence);
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public override void UpdatePositions(ref int x, ref int y)
		{
			x += GetBufferLength(Component);
		}

		public override Point GetConnectingPoint(int x, int y)
		{
			return ControlDpiScalingHelper.NewScaledPoint(x + (GetBufferLength(Component) / 2), y, false);
		}

		internal static int GetBufferLength(BMComponent component)
		{
			if (component != null)
			{
				var maxBuffer = component.System.Components.Where(x => x.FC_Type == BMComponentTypeList.Codes.Buffer).OrderByDescending(x => x.BufferTimeSpanHours).FirstOrDefault();
				return (int)Math.Floor((component.BufferTimeSpanHours / maxBuffer.BufferTimeSpanHours) * ScaledBufferStandardLength);
			}

			return ScaledBufferStandardLength;
		}

		public static int ScaledBufferStandardLength => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.BufferStandardLength);
		static int ScaledBufferNameLabelOffset => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.BufferNameLabelOffset);
	}
}
