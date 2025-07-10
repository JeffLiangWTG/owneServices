using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class BucketDrawer : ComponentDrawer
	{
		public BucketDrawer(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public override void Draw(ref int x, ref int y)
		{
			using (var bucketPen = new Pen(Color.Blue, 2))
			{
				DrawPath(bucketPen, new GraphicsPath(new[]
				{
					ControlDpiScalingHelper.NewScaledPoint(x, y, false),
					ControlDpiScalingHelper.NewScaledPoint(x + ScaledBucketEdgeDifference, y + ScaledBucketHeight, false),
					ControlDpiScalingHelper.NewScaledPoint(x + ScaledBucketWidthBase, y + ScaledBucketHeight, false),
					ControlDpiScalingHelper.NewScaledPoint(x + ScaledBucketWidthBase + ScaledBucketEdgeDifference, y, false)
				},
				new[]
				{
					(byte)PathPointType.Line,
					(byte)PathPointType.Line,
					(byte)PathPointType.Line,
					(byte)PathPointType.Line
				}));
			}

			DrawString(x, ScaledBucketWidthBase + ScaledBucketEdgeDifference, y + ScaledBucketHeight / 2, IsLegend ? Res.GetString("9063f689-69c2-47c7-bc5c-e8e53438b605", "Bucket") : Component.FC_Name.ToString());

			var isLastComponent = Component != null && !Component.FromMeToOthersLinks.Any(link => link.ComponentTo.FC_DisplaySequence > Component.FC_DisplaySequence);
			if (!isLastComponent && !IsNonPrimaryPath)
			{
				DrawVerticalArrow(ref x, ref y, ScaledBucketHeight);
			}
			else
			{
				y += ScaledBucketHeight + ScaledArrowHeight;
			}

			if (IsLegend)
			{
				y += ScaledLegendPadding;
				DrawLegendString(x, ref y, Res.GetString("7146a12d-063d-45cc-89c7-290cf44945d1", "A bucket contains items which do not have a 'release time' yet. That is, permission to start has not been granted."));
			}
		}

		static int ScaledLegendPadding => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.BucketLegendPadding);
		static int ScaledBucketHeight => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.BucketHeight);
		static internal int ScaledBucketWidthBase => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.BucketWidthBase);
		static internal int ScaledBucketEdgeDifference => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.BucketEdgeDifference);
	}
}
