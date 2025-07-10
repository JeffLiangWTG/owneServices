using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class DecoupleDrawer : ComponentDrawer
	{
		public DecoupleDrawer(Graphics graphics, BMComponent component, bool isNonPrimaryPath)
			: base(graphics, component, isNonPrimaryPath)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public override void Draw(ref int x, ref int y)
		{
			using (var decouplePen = new Pen(Color.Green, 2))
			{
				DrawLine(decouplePen, x, y, x + ScaledDecoupleWidth, y);
				DrawLine(decouplePen, x, y + ScaledDecoupleGap, x + ScaledDecoupleWidth, y + ScaledDecoupleGap);
			}

			DrawString(x, ScaledDecoupleWidth, y + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), IsLegend ? Res.GetString("68766d05-5414-4217-84be-2e86b8b1c28d", "Decouple") : Component.FC_Name.ToString());

			DrawVerticalArrow(ref x, ref y, ScaledDecoupleGap);

			if (IsLegend)
			{
				DrawLegendString(x, ref y, Res.GetString("fee013a6-4bc1-4e34-9a67-1acc64f60389", "A decouple is a marker in a sub-schematic which shows where inventory is deliberately added in order to decouple lead time. This allows preceding sub-components to start with a negative offset."));
			}
		}

		static int ScaledDecoupleWidth => ControlDpiScalingHelper.ScaleToCurrentDpiX(SchematicVisualizationConstantsUnscaled.DecoupleWidth);
		static int ScaledDecoupleGap => ControlDpiScalingHelper.ScaleToCurrentDpiY(SchematicVisualizationConstantsUnscaled.DecoupleGap);
	}
}
