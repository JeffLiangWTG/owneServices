namespace Enterprise.ZArchitecture.GUI
{
	partial class CustomPropertiesControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.rowLayoutPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.nothingSetupMessageLabel = new CustomFieldsNotSetupLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// rowLayoutPanel
			// 
			this.rowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rowLayoutPanel.AutoScroll = true;
			this.rowLayoutPanel.FixedRows = true;
			this.rowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rowLayoutPanel.Name = "rowLayoutPanel";
			this.rowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.rowLayoutPanel.TabIndex = 0;
			// 
			// nothingSetupMessageLabel
			// 
			this.nothingSetupMessageLabel.CaptionResourceString = null;
			this.nothingSetupMessageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nothingSetupMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.nothingSetupMessageLabel.Name = "nothingSetupMessageLabel";
			this.nothingSetupMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.nothingSetupMessageLabel.TabIndex = 1;
			this.nothingSetupMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.nothingSetupMessageLabel.Visible = false;
			// 
			// CustomPropertiesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.nothingSetupMessageLabel);
			this.Controls.Add(this.rowLayoutPanel);
			this.Name = "CustomPropertiesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.Layout.RowLayoutPanel rowLayoutPanel;
		private CustomFieldsNotSetupLabel nothingSetupMessageLabel;
	}
}
