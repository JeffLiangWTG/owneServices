using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed class FieldFindBoxButton : ZButton.Bare
	{
		public FieldFindBoxButton()
		{
			BackColor = SystemColors.ButtonFace;
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return ""; }
			set { }
		}

		#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			TextRendererHelper.DrawText(e.Graphics, "...", Font, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2), SystemBrushes.ControlText);
		}

		#endif
	}
}
