using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class ConstraintDrawer : ComponentDrawer
	{
		public ConstraintDrawer(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public override void Draw(ref int x, ref int y)
		{
			var circleX = x + ((BucketDrawer.ScaledBucketEdgeDifference + BucketDrawer.ScaledBucketWidthBase / 2) - (ScaledDiameterX / 2) - ScaledCircleDistanceFromCornerX);
			using (var constraintPen = new Pen(Color.Red, 2))
			{
				var circleBounds = ControlDpiScalingHelper.NewScaledRectangle(circleX, y, ScaledDiameterX, ScaledDiameterY, false);
				DrawEllipse(constraintPen, circleBounds);
				DrawLine(constraintPen,
					circleBounds.X + ScaledCircleDistanceFromCornerX,
					circleBounds.Y + ScaledCircleDistanceFromCornerY,
					circleBounds.X + ScaledDiameterX - ScaledCircleDistanceFromCornerX,
					circleBounds.Y + ScaledDiameterY - ScaledCircleDistanceFromCornerY);
				DrawLine(constraintPen,
					circleBounds.X - ScaledCircleDistanceFromCornerX + ScaledDiameterX,
					circleBounds.Y + ScaledCircleDistanceFromCornerY,
					circleBounds.X + ScaledCircleDistanceFromCornerX,
					circleBounds.Y + ScaledDiameterY - ScaledCircleDistanceFromCornerY);
			}

			if (IsLegend)
			{
				DrawString(x, BucketDrawer.ScaledBucketWidthBase + BucketDrawer.ScaledBucketEdgeDifference, y - ScaledTextHeight, Res.GetString("a88189b8-2b7f-45f0-b258-1d12e67826b6", "Constraint"));
			}
			else
			{
				DrawString(x + (ScaledDiameterX * 2), -1, y + ((ScaledDiameterY - 2 * ScaledCircleDistanceFromCornerY) / 2), Component.FC_Name, null, false);
			}

			DrawVerticalArrow(ref x, ref y, ScaledDiameterY);

			if (IsLegend)
			{
				DrawLegendString(x, ref y, Res.GetString("91cdf2bc-5dff-47c7-adb5-86dc6d3b3662", "A constraint is a marker in a sub-schematic which shows where in the flow the Capacity Constrained Resource is being scheduled."));
			}
		}

		static int ScaledTextHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.ConstraintTextHeight);
		static int ScaledDiameterX => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.ConstraintDiameter);
		static int ScaledDiameterY => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.ConstraintDiameter);
		static int ScaledCircleDistanceFromCornerX => ScaledDiameterX / 6;
		static int ScaledCircleDistanceFromCornerY => ScaledDiameterY / 6;
	}
}
