using System.Windows.Forms;

namespace Enterprise.Startup
{
	partial class StartupUserControl
	{
		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.StartupLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// StartupLayoutPanel
			//
			this.StartupLayoutPanel.SuspendLayout();
			this.StartupLayoutPanel.ColumnCount = 3;
			this.StartupLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.StartupLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.StartupLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.StartupLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StartupLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StartupLayoutPanel.Name = "StartupLayoutPanel";
			this.StartupLayoutPanel.RowCount = 3;
			this.StartupLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(47)));
			this.StartupLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.StartupLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.StartupLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 585, true);
			this.StartupLayoutPanel.TabIndex = 2;
			this.StartupLayoutPanel.ResumeLayout();
			//
			// StartupUserControl
			//
			this.Controls.Add(this.StartupLayoutPanel);
			this.Name = "StartupUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 585, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		CargoWise.Windows.UI.KTableLayoutPanel StartupLayoutPanel;

		#endregion
	}
}