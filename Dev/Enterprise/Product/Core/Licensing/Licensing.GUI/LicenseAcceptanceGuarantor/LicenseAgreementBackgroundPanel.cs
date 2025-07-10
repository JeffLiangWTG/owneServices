using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licensing.GUI
{
	public partial class LicenseAgreementBackgroundPanel : ZUserControl
	{
		public LicenseAgreementBackgroundPanel(Control control)
		{
			InitializeComponent();
			this.mainTableLayoutPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.formTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();

			if (control.Dock == DockStyle.Fill)
			{
				MainPanel.AutoSize = false;
				MainPanel.Dock = DockStyle.Fill;
				control.Dock = DockStyle.Fill;
			}
			else
			{
				control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			}

			formTableLayoutPanel.Controls.Add(control, 0, 0);

			this.ResumeLayout(false);
			this.formTableLayoutPanel.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.ResumeLayout(false);
		}

		public ZPanel MainPanel => mainPanel;
	}
}
