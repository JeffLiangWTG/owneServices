using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class SeparatorUserControl : ZUserControl
	{
		public SeparatorUserControl()
		{
			InitializeComponent();
			CentreControls();
		}

		void SeparatorUserControl_Load(object sender, EventArgs e)
		{
			CentreControls();
		}

		void CentreControls()
		{
			var txt = SeparatorText.CaptionResourceString.Caption;
			var size = SeparatorText.CreateGraphics().MeasureString(txt, SeparatorText.Font);

			SeparatorText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint((int)((Width - size.Width) / 2), (int)((Height - size.Height) / 2), false);
			SeparatorText.BringToFront();
		}

		#if !WINZOR

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			var drawLineBush = new SolidBrush(Color.Black);
			var y = (float)(Height / 2) - 1;
			var size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(Width, 2, true);
			e.Graphics.FillRectangle(drawLineBush, new RectangleF(0, y, size.Width, size.Height));
		}

		#endif

		void SeparatorUserControl_Resize(object sender, EventArgs e)
		{
			CentreControls();
		}

		public override ResourceStringData CaptionResourceString
		{
			get
			{
				return SeparatorText.CaptionResourceString;
			}
			set
			{
				SeparatorText.CaptionResourceString = value;
				CentreControls();
			}
		}
	}
}
