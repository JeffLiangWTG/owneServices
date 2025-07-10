using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			InitializeCustomsAreasControlsLayout();
		}

		void InitializeCustomsAreasControlsLayout()
		{
			if (!DesignMode)
			{
				var rightPanel = new ZArchitecture.GUI.ZPanel();

				rightPanel.SuspendLayout();
				RightTabControl.SuspendLayout();
				CustomsAreasGroupBox.SuspendLayout();

				rightPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;

				ControlDpiScalingHelper.SetHeight(rightPanel, RightTabControl.Height + CustomsAreasGroupBox.Height, false);
				ControlDpiScalingHelper.SetWidth(rightPanel, RightTabControl.Width, false);

				rightPanel.Location = RightTabControl.Location;

				Controls.Remove(RightTabControl);
				Controls.Remove(CustomsAreasGroupBox);

				CustomsAreasGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
				RightTabControl.Dock = System.Windows.Forms.DockStyle.Fill;

				rightPanel.Controls.Add(RightTabControl);
				rightPanel.Controls.Add(CustomsAreasGroupBox);
				Controls.Add(rightPanel);

				CustomsAreasGroupBox.ResumeLayout(true);
				CustomsAreasGroupBox.PerformLayout();
				RightTabControl.ResumeLayout(true);
				RightTabControl.PerformLayout();
				rightPanel.ResumeLayout(true);
				rightPanel.PerformLayout();
			}
		}
	}
}
