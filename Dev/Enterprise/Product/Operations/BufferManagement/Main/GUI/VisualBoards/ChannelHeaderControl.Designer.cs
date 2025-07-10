namespace Enterprise.BufferManagement.GUI
{
	partial class ChannelHeaderControl
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
			this.components = new System.ComponentModel.Container();
			this.FadePanel = new Enterprise.ZArchitecture.GUI.ZFadePanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaff);
			// 
			// FadePanel
			// 
			this.FadePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FadePanel.FadeEndColor = System.Drawing.Color.Empty;
			this.FadePanel.FadeStartColor = System.Drawing.Color.Empty;
			this.FadePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FadePanel.Name = "FadePanel";
			this.FadePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 41, true);
			this.FadePanel.TabIndex = 1;
			// 
			// ChannelHeaderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.FadePanel);
			this.Name = "ChannelHeaderControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 41, true);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ChannelHeaderControl_MouseClick);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZFadePanel FadePanel;
	}
}
