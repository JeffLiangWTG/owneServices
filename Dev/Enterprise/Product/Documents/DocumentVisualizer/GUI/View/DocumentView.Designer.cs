namespace Enterprise.DocumentVisualizer.GUI
{
	partial class DocumentView
	{
		internal PagesLayoutPanel pagesLayoutPanel;

		private void InitializeComponent()
		{
			this.pagesLayoutPanel = new Enterprise.DocumentVisualizer.GUI.PagesLayoutPanel();
			this.watermarkLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// pagesLayoutPanel
			//
			this.pagesLayoutPanel.BackColor = System.Drawing.Color.Gray;
			this.pagesLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pagesLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pagesLayoutPanel.Name = "pagesLayoutPanel";
			this.pagesLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 370, true);
			this.pagesLayoutPanel.TabIndex = 0;
			//
			// watermarkLabel
			//
			this.watermarkLabel.BackColor = System.Drawing.Color.Gray;
			this.watermarkLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("cffa7975-d124-4df6-9138-25d50f525ef0", "There is no document to show");
			this.watermarkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.watermarkLabel.ForeColor = System.Drawing.SystemColors.HighlightText;
			this.watermarkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 36f);
			this.watermarkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.watermarkLabel.Name = "watermarkLabel";
			this.watermarkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 370, true);
			this.watermarkLabel.TabIndex = 15;
			this.watermarkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.watermarkLabel.Visible = false;
			//
			// DocumentView
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.watermarkLabel);
			this.Controls.Add(this.pagesLayoutPanel);
			this.Name = "DocumentView";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 370, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZLabel watermarkLabel;
	}
}
