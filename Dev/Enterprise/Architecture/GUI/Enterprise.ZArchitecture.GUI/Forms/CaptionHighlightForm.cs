using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	class CaptionHighlightForm : Form // Low level form for masking
	{
		public static void Show(Form form)
		{
			if (instance == null)
			{
				instance = new CaptionHighlightForm();
			}
			instance.Run(form);
		}

		static CaptionHighlightForm instance;

#if DEBUG
		public
#endif
		CaptionHighlightForm()
		{
			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.Icon = null;
			this.BackColor = Color.LightSeaGreen;
			this.Opacity = 0.50;
			this.TopMost = true;

			this.MouseMove += new MouseEventHandler(CaptionHighlightForm_MouseMove);
			this.MouseLeave += new EventHandler(CaptionHighlightForm_MouseLeave);
			this.Click += new EventHandler(CaptionHighlightForm_Click);
		}

		void CaptionHighlightForm_Click(object sender, EventArgs e)
		{
			TranslationFeedbackManager.OpenFeedbackForm(parent);
			Close();
		}

		void Run(Form parent)
		{
			this.parent = parent;
			this.Show();
			this.BringToFront();
			this.Bounds = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(parent.Bounds.Location.X, parent.Bounds.Location.Y, parent.Bounds.Width, ControlDpiScalingHelper.ScaleToCurrentDpiY(30), false);
		}

		void CaptionHighlightForm_MouseMove(object sender, MouseEventArgs e)
		{
			if (!TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				Close();
			}
		}

		void CaptionHighlightForm_MouseLeave(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			parent = null;
			instance = null;
		}

		Form parent;
	}
}
