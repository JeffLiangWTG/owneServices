namespace Enterprise.Customs.DE.GUI
{
	partial class TemporaryStorageMutexTabPageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CoveringLabel
			// 
			this.CoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CoveringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CoveringLabel.IsFontBold = true;
			this.CoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CoveringLabel.Name = "CoveringLabel";
			this.CoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.CoveringLabel.TabIndex = 0;
			this.CoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CoveringLabel.Visible = false;
			// 
			// TemporaryStorageMutexTabPageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CoveringLabel);
			this.Name = "TemporaryStorageMutexTabPageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.ZLabel CoveringLabel;
	}
}
