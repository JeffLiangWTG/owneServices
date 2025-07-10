using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	public class FadeLabel : ZLabel
	{
		public FadeLabel(CellContent cell, float gradientAngle)
		{
			this.cell = cell;
			this.gradientAngle = gradientAngle;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to pass BufferManagement.GUI.Test.TaskCardControlTest.TestGetBitmapRender_RendersChildControlsInCorrectOrder")]
		protected readonly CellContent cell;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to pass BufferManagement.GUI.Test.TaskCardControlTest.TestGetBitmapRender_RendersChildControlsInCorrectOrder")]
		readonly float gradientAngle;

#if !WINZOR

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			var value = cell?.BackgroundFadeColor;
			if (value != null)
			{
				this.PaintGradientBackground(BackColor, value.Value, gradientAngle, e.Graphics);
			}
			else
			{
				base.OnPaintBackground(e);
			}
		}

#endif
	}
}
