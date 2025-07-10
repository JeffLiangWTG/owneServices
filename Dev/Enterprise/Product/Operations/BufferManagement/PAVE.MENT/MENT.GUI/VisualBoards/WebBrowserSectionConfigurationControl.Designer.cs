using System;
namespace Enterprise.PAVE.MENT.GUI
{
	partial class WebBrowserSectionConfigurationControl
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
            this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zLabel1 = new ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.WebBrowserSectionConfiguration);
            // 
            // zGroupBox1
            // 
            this.zGroupBox1.Controls.Add(this.zLabel1);
            this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGroupBox1.Name = "zGroupBox1";
            this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 111, true);
            this.zGroupBox1.TabIndex = 0;
            this.zGroupBox1.TabStop = false;
            this.zGroupBox1.Text = "Configuration";
            // 
            // zLabel1
            // 
            this.zLabel1.AutoSize = true;
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 64, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 25, true);
			this.zLabel1.TabIndex = 0;
            this.zLabel1.Text = "WEB sections are no longer supported.";
			// 
			// WebBrowserSectionConfigurationControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zGroupBox1);
            this.Name = "WebBrowserSectionConfigurationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 111, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zGroupBox1.ResumeLayout(false);
            this.zGroupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZLabel zLabel1;
	}
}
