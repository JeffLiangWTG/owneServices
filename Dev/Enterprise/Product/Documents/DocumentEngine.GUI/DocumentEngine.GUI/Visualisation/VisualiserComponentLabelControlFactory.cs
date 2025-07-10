using System.Drawing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	class VisualiserComponentLabelControlFactory : VisualiserComponentControlFactory<VisualiserComponentLabel, ZLabel>
	{
		protected override ZLabel CreateCore(VisualiserComponentLabel component)
		{
			ZLabel result = new ZLabel();
			result.Text = component.Caption;
			result.Location = component.Location;
			result.Size = component.Size;
			result.BackColor = Color.Transparent;
			result.Font = component.CellFormat.GetFont();
			switch (component.CellFormat.HTextAlign)
			{
				case HorizontalTextAlignment.Left:
					result.TextAlign = ContentAlignment.MiddleLeft;
					break;

				case HorizontalTextAlignment.Right:
					result.TextAlign = ContentAlignment.MiddleRight;
					break;

				case HorizontalTextAlignment.Centre:
					result.TextAlign = ContentAlignment.MiddleCenter;
					break;
			}

			result.ForeColor = component.CellFormat.TextColor;
			if (component.CellFormat.FillPattern != FillPatternStyle.None)
			{
				result.BackColor = component.CellFormat.BackgroundColor;
			}
			return result;
		}
	}
}
